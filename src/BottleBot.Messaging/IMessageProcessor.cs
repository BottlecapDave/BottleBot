namespace BottleBot.Messaging;

public interface IMessageProcessor
{
    /// <summary>
    /// Process a given message for a given channel sent by a given user.
    /// </summary>
    /// <param name="user">The user who sent the message</param>
    /// <param name="channel">The channel that the message was sent on</param>
    /// <param name="message">The message that was sent</param>
    /// <returns>True if the message was processed; False otherwise</returns>
    Task<bool> ProcessAsync(string user, string channel, string message);
}
