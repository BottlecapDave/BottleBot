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

    private SemaphoreSlim _lastPingLock = new(1, 1);

    private DateTimeOffset? _lastPing;

    private DateTimeOffset? LastPing
    {
        get
        {
            _lastPingLock.Wait();
            try
            {
                return _lastPing;
            }
            finally
            {
                _lastPingLock.Release();
            }
        }
        set
        {
            _lastPingLock.Wait();
            try
            {
                _lastPing = value;
            }
            finally
            {
                _lastPingLock.Release();
            }
        }
    }

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
            _logger.LogDebug($"RESPONSE --> {message}");
        }
    }

    public async Task StartAsync()
    {
        bool retry = true;
        do
        {
            bool isConnected = true;
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {

                void checkPing()
                {
                    while (LastPing == null || LastPing >= DateTime.UtcNow.AddMinutes(-5))
                    {
                        
                        _logger.LogDebug($"Ping received in timely manner. Checking soon.");
                        Thread.Sleep(60000);
                    }

                    _logger.LogDebug($"Application hasn't received a ping since {LastPing}, so restarting");
                    LastPing = null;
                    isConnected = false;
                    cts.Cancel();
                }

                Thread checkPingThread = new Thread(checkPing);
                checkPingThread.Start();

                try
                {
                    using (var irc = new TcpClient(_config.Server, _config.Port))
                    {
                        using (var stream = irc.GetStream())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                if (_writer != null)
                                {
                                    await _writer.DisposeAsync();
                                }

                                _writer = new StreamWriter(stream);

                                await SendAsync($"NICK {_config.Nick}");
                                await SendAsync(_config.User);

                                while (isConnected && irc.Connected)
                                {
                                    string? inputLine = await reader.ReadLineAsync(cts.Token);
                                    isConnected = await ProcessMessageAsync(inputLine);
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
                }
            }
        }
        while (retry);
    }

    private async Task<bool> ProcessMessageAsync(string? inputLine)
    {
        if (inputLine == null)
        {
            return true;
        }

        _logger.LogDebug($"MSG --> {inputLine}");

        // split the lines sent from the server by spaces (seems to be the easiest way to parse them)
        string[] splitInput = inputLine.Split(new Char[] { ' ' });

        if (splitInput[0] == "PING")
        {
            LastPing = DateTimeOffset.Now;
            string PongReply = splitInput[1];
            await SendAsync($"PONG {PongReply}");
            return true;
        }

        if (inputLine.Contains(":Closing Link:"))
        {
            _logger.LogInformation("Disconnected from host. Trying to reconnect");
            return false;
        }

        int commandStartIndex = inputLine.IndexOf(':');
        if (commandStartIndex < 0)
        {
            return true;
        }

        // Go beyond the :
        commandStartIndex += 1;

        int commandEndIndex = inputLine.Substring(commandStartIndex + 1).IndexOf(" :");
        if (commandEndIndex < 0)
        {
            return true;
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

        return true;
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
