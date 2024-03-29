namespace BottleBot.Messaging.Logging;

public interface IMessageLogger
{
    /// <summary>
    /// Save a new message.
    /// </summary>
    /// <param name="message">The message to save</param>
    Task SaveAsync(SaveMessage message);

    /// <summary>
    /// Retrieve the contents for the provided timestamp and channel.
    /// </summary>
    /// <param name="date">The date the messages should be retrieved for.</param>
    /// <param name="channel">The channel the messages should be retrieved for.</param>
    /// <returns>The messages that were raised for the specified date</returns>
    Task<string[]> GetAsync(DateOnly date, string channel);
}
