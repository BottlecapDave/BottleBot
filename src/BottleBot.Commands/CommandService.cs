namespace BottleBot.Commands;

public class CommandService : ICommandService
{
    Dictionary<string, ICommand> _registeredCommands = new Dictionary<string, ICommand>();

    public IEnumerable<ICommand> Commands => _registeredCommands.Values;

    public void Register(ICommand command)
    {
        _registeredCommands.Add(command.Key, command);
    }
}
