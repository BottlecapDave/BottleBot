namespace BottleBot.MessageLogging;

public interface IMessageLogger
{
    /// <summary>
    /// Save a new message
    /// </summary>
    /// <param name="message">The message to save</param>
    Task SaveAsync(SaveMessage message);
}
