using BottleBot.Messaging.Logging;

namespace BottleBot.Messaging.Processors;

/// <summary>
/// Message processor which saves the inbound messages
/// </summary>
public class LogMessageProcessor : IMessageProcessor
{
    private readonly IMessageLogger _messageLogger;

    public LogMessageProcessor(IMessageLogger messageLogger)
    {
        _messageLogger = messageLogger;
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
        await _messageLogger.SaveAsync(new(DateTime.Now, user, channel, message));

        return true;
    }
}
