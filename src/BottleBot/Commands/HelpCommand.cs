using BottleBot.Messaging;
using System.Text;

namespace BottleBot.Commands;

public class HelpCommand : ICommand
{
    private readonly IMessagingService _messagingService;

    private readonly ICommandService _commandService;

    public HelpCommand(IMessagingService messagingService, ICommandService commandService)
    {
        _messagingService = messagingService;
        _commandService = commandService;
    }

    /// <summary>
    /// The key to call to execute the command
    /// </summary>
    public string Key { get { return "help"; } }

    /// <summary>
    /// The description of what the command does
    /// </summary>
    public string Description { get { return "returns all available commands."; } }

    /// <summary>
    /// The description of what the command does
    /// </summary>
    public string Help { get { return $"To get help on a specific command, type !{Key} followed by the command (e.g. !{Key} {_commandService.Commands.Last().Key})"; } }

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="message">The context the command was executed within</param>
    public async Task ExecuteAsync(CommandContext context)
    {
        if (string.IsNullOrEmpty(context.Content))
        {
            await _messagingService.SendAsync(new(context.Channel, string.Join(';', _commandService.Commands.Select(c => $"{c.Key} - {c.Description}"))));

            await _messagingService.SendAsync(new(context.Channel, $"{Help}\n"));
        }
        else
        {
            string targetKey = context.Content.Trim();
            ICommand? target = _commandService.Commands.FirstOrDefault(c => c.Key == targetKey);
            if (target == null)
            {
                await _messagingService.SendAsync(new(context.Channel, $"Failed to find command {targetKey}"));
            }
            else
            {
                await _messagingService.SendAsync(new(context.Channel, $"{target.Help}\n"));
            }

        }
    }
}
