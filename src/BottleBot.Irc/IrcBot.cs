using BottleBot.Messaging;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace BottleBot.Irc;

public class IRCbot : IMessagingService
{
    private readonly IRCBotConfig _config;

    private readonly ICommandService _commandService;

    private readonly ILogger<IRCbot> _logger;

    private StreamWriter? _writer;

    public IRCbot(ICommandService commandService, IRCBotConfig config, ILogger<IRCbot> logger)
    {
        _commandService = commandService;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Send a message
    /// </summary>
    /// <param name="message">The message to be sent</param>
    public async Task SendAsync(SendMessageContent message)
    {
        if (_writer != null)
        {
            await _writer.WriteLineAsync($"PRIVMSG {message.Channel} :{message.Content}");
            await _writer.FlushAsync();
        }
    }

    /// <summary>
    /// Send a message
    /// </summary>
    /// <param name="message">The message to be sent</param>
    public async Task SendAsync(string message)
    {
        if (_writer != null)
        {
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
        }
    }

    public async Task StartAsync()
    {
        var retry = false;
        var retryCount = 0;
        do
        {
            try
            {
                using (var irc = new TcpClient(_config.Server, _config.Port))
                using (var stream = irc.GetStream())
                using (var reader = new StreamReader(stream))
                {
                    if (_writer != null)
                    {
                        await _writer.DisposeAsync();
                    }

                    _writer = new StreamWriter(stream);

                    await SendAsync($"NICK {_config.Nick}");
                    await SendAsync(_config.User);

                    while (true)
                    {
                        string? inputLine;
                        while ((inputLine = reader.ReadLine()) != null)
                        {
                            _logger.LogDebug($"MSG --> {inputLine}");

                            // split the lines sent from the server by spaces (seems to be the easiest way to parse them)
                            string[] splitInput = inputLine.Split(new Char[] { ' ' });

                            if (splitInput[0] == "PING")
                            {
                                string PongReply = splitInput[1];
                                await SendAsync($"PONG {PongReply}");
                                continue;
                            }

                            int commandStartIndex = inputLine.IndexOf(':');
                            if (commandStartIndex < 0)
                            {
                                continue;
                            }

                            // Go beyond the :
                            commandStartIndex += 1;

                            int commandEndIndex = inputLine.Substring(commandStartIndex + 1).IndexOf(':');
                            if (commandEndIndex < 0)
                            {
                                continue;
                            }

                            commandEndIndex += 1;

                            string command = inputLine.Substring(commandStartIndex, commandEndIndex - commandStartIndex);
                            string content = inputLine.Substring(commandStartIndex + commandEndIndex + 1);

                            splitInput = command.Split(new Char[] { ' ' });

                            switch (splitInput[1])
                            {
                                case "001":
                                    await JoinChannelAsync();
                                    break;
                                case "PRIVMSG":
                                    await ProcessPrivateMessageAsync(splitInput, content);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                Console.WriteLine(e.ToString());
                Thread.Sleep(5000);
                retry = ++retryCount <= _config.MaxRetries;
            }
        } while (retry);
    }

    private async Task JoinChannelAsync()
    {
        await SendAsync($"JOIN {_config.Channel}");
    }

    private async Task ProcessPrivateMessageAsync(string[] commandParts, string content)
    {
        string user = commandParts[0].Split('!')[0];
        string channel = commandParts[2];

        if (content.StartsWith('!'))
        {
            int commandEndIndex = content.IndexOf(' ');
            string commandKey = content.Substring(1, commandEndIndex >= 0 ? commandEndIndex - 1 : content.Length - 1);

            ICommand? command = _commandService.Commands.FirstOrDefault(c => c.Key == commandKey);
            if (command != null)
            {
                await command.ExecuteAsync(new(
                    user,
                    channel,
                    commandEndIndex >= 0 ? content.Substring(commandEndIndex).Trim() : string.Empty
                ));
            }

            return;
        }

        string logFileName = Path.Join(_config.LogRootDir, channel, $"{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
        string logFileDirectory = Directory.GetParent(logFileName).FullName;
        if (Directory.Exists(logFileDirectory) == false)
        {
            Directory.CreateDirectory(logFileDirectory);
        }

        string now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        await File.AppendAllLinesAsync(logFileName, new string[] { $"[{now}] ({user}): {content}" });
    }
}
