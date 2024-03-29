namespace BottleBot.Messaging;

public interface IMessagingService
{
    /// <summary>
    /// Send a message
    /// </summary>
    /// <param name="message">The message to be sent</param>
    Task SendAsync(SendMessageContent mesage);
}
