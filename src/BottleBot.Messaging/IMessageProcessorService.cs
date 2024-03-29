namespace BottleBot.Messaging;

public interface IMessageProcessorService
{
    /// <summary>
    /// The collection of registered message processors
    /// </summary>
    IEnumerable<IMessageProcessor> Processors { get; }
}
