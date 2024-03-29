using BottleBot.Messaging;
using BottleBot.Messaging.Processors;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace BottleBot.Irc;

public class IRCbot : IMessagingService
{
    private readonly IRCBotConfig _config;

    private readonly IMessageProcessorService _messageProcessorService;

    private readonly ILogger<IRCbot> _logger;

    private StreamWriter? _writer;

    public IRCbot(IMessageProcessorService messageProcessorService, IRCBotConfig config, ILogger<IRCbot> logger)
    {
        _messageProcessorService = messageProcessorService;
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
        var retry = true;
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
                            try
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

                                int commandEndIndex = inputLine.Substring(commandStartIndex + 1).IndexOf(" :");
                                if (commandEndIndex < 0)
                                {
                                    continue;
                                }

                                commandEndIndex += 1;

                                string command = inputLine.Substring(commandStartIndex, commandEndIndex - commandStartIndex + 1);
                                string content = inputLine.Substring(commandStartIndex + commandEndIndex + 2);

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
                            } catch (Exception ex)
                            {
                                _logger.LogError("Recoverable error - {Error}: {StackTrace}", ex.Message, ex.StackTrace);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                _logger.LogError("Error requires reconnection - {Error}: {StackTrace}", ex.Message, ex.StackTrace);
                Thread.Sleep(5000);
                retryCount++;
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

        foreach (IMessageProcessor processor in _messageProcessorService.Processors)
        {
            await processor.ProcessAsync(user, channel, content);
        }
    }
}
