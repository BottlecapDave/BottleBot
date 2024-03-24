namespace BottleBot;

public interface ICommand
{
    /// <summary>
    /// The key to call to execute the command
    /// </summary>
    string Key { get; }

    /// <summary>
    /// The description of what the command does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The help text for the command
    /// </summary>
    string Help { get; }

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="message">The context the command was executed within</param>
    Task ExecuteAsync(CommandContext context);
}
