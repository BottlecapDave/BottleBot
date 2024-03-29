using BottleBot.Commands;

namespace BottleBot.Messaging.Processors;

public class CommandMessageProcessor : IMessageProcessor
{
    private readonly ICommandService _commandService;

    public CommandMessageProcessor(ICommandService commandService)
    {
        _commandService = commandService;
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
        if (message.StartsWith('!'))
        {
            int commandEndIndex = message.IndexOf(' ');
            string commandKey = message.Substring(1, commandEndIndex >= 0 ? commandEndIndex - 1 : message.Length - 1);

            ICommand? command = _commandService.Commands.FirstOrDefault(c => c.Key == commandKey);
            if (command != null)
            {
                await command.ExecuteAsync(new(
                    user,
                    channel,
                    commandEndIndex >= 0 ? message.Substring(commandEndIndex).Trim() : string.Empty
                ));

                return true;
            }
        }

        return false;
    }
}
