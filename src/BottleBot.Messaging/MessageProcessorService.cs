namespace BottleBot.Messaging;

public class MessageProcessorService : IMessageProcessorService
{
    List<IMessageProcessor> _registeredProcessors = new();

    public IEnumerable<IMessageProcessor> Processors => _registeredProcessors;

    public void Register(IMessageProcessor processor)
    {
        _registeredProcessors.Add(processor);
    }
}
