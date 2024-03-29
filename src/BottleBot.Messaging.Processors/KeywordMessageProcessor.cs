using BottleBot.Commands;

namespace BottleBot.Messaging.Processors;

/// <summary>
/// Processes a message looking for keywords, and if present, executes a given command
/// </summary>
public class KeywordMessageProcessor : IMessageProcessor
{
    private readonly KeywordMessageProcessorConfig _config;
    private readonly ICommand _targetCommand;

    public KeywordMessageProcessor(KeywordMessageProcessorConfig config, ICommand targetCommand)
    {
        _config = config;
        _targetCommand = targetCommand;
    }

    /// <summary>
    /// Process a given message for a given channel sent by a given user.
    /// </summary>
    /// <param name="user">The user who sent the message</param>
    /// <param name="channel">The channel that the message was sent on</param>
    /// <param name="message">The message that was sent</param>
    /// <returns>True if the message was processed; False otherwise</returns>
    public async Task<bool> ProcessAsync(string user, string channel, string message)
    {
        if (_config.Keywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            await _targetCommand.ExecuteAsync(new(user, channel, message));
            return true;
        }

        return false;
    }
}
