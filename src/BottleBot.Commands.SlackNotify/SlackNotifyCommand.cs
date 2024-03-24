using BottleBot.Messaging;
using System.Net.Http.Json;

namespace BottleBot.Commands.SlackNotify;

public class SlackNotifyCommand : ICommand
{
    private const string DefaultMessage = "Request your attention";

    private readonly IMessagingService _messagingService;

    private readonly SlackNotifyCommandConfig _config;


    public SlackNotifyCommand(IMessagingService messagingService, SlackNotifyCommandConfig config)
    {
        _messagingService = messagingService;
        _config = config;
    }

    /// <summary>
    /// The key to call to execute the command
    /// </summary>
    public string Key { get { return "notify"; } }

    /// <summary>
    /// The description of what the command does
    /// </summary>
    public string Description { get { return "Notifies BottlecapDave that his attention is requested."; } }

    /// <summary>
    /// The description of what the command does
    /// </summary>
    public string Help { get { return $"To notify BottlecapDave, type !{Key} followed by an optional message (e.g. !{Key} come quick)"; } }

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="message">The context the command was executed within</param>
    public async Task ExecuteAsync(CommandContext context)
    {
        using (HttpClient client = new())
        {
            var msg = new { text = $"From {context.User}@{context.Channel}: {(string.IsNullOrEmpty(context.Content) == false ? context.Content : DefaultMessage)}" };
            await client.PostAsJsonAsync(_config.SlackWebhookUrl, msg);
            await _messagingService.SendAsync(new(context.Channel, "Notification has been sent"));

        }
    }
}
