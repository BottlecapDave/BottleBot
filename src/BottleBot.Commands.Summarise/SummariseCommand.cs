using BottleBot.Messaging;
using BottleBot.Messaging.Logging;

namespace BottleBot.Commands.Summarise;

public class SummariseCommand : ICommand
{
    private SemaphoreSlim _lock = new(1);

    private readonly IMessagingService _messagingService;

    private readonly IMessageLogger _messageLogger;

    private readonly SummariseCommandConfig _config;
    public SummariseCommand(IMessagingService messagingService, IMessageLogger messageLogger, SummariseCommandConfig config)
    {
        _messagingService = messagingService;
        _messageLogger = messageLogger;
        _config = config;
    }

    /// <summary>
    /// The key to call to execute the command
    /// </summary>
    public string Key { get { return "summarise"; } }

    /// <summary>
    /// The description of what the command does
    /// </summary>
    public string Description { get { return "Summarises the events for a given day."; } }

    /// <summary>
    /// The description of how the command can be used
    /// </summary>
    public string Help { get { return $"To summarise a day, type !{Key} followed by the date in YYYY-MM-DD format (e.g. !{Key} {DateTime.Now.ToString("yyyy-MM-dd")} to summarise today)"; } }

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="message">The context the command was executed within</param>
    public async Task ExecuteAsync(CommandContext context)
    {
        if (await _lock.WaitAsync(1000) == false)
        {
            await _messagingService.SendAsync(new(context.Channel, $"Busy summarising a previous request. Please try again later"));
            return;
        }

        try
        {
            if (DateOnly.TryParse(context.Content.Trim(), out DateOnly targetDate) == false)
            {
                await _messagingService.SendAsync(new(context.Channel, $"Invalid date provided. Must be in the format YYYY-MM-DD (e.g. {DateTime.Now.ToString("yyyy-MM-dd")} for today)"));
                return;
            }

            string[] messages = await _messageLogger.GetAsync(targetDate, context.Channel);
            if (messages.Length < 1)
            {
                await _messagingService.SendAsync(new(context.Channel, $"No messages to summarise"));
                return;
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.Arguments = String.Format(_config.Prompt, string.Join('\n', messages));
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            process.StartInfo = startInfo;
            process.Start();

            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            await _messagingService.SendAsync(new(context.Channel, process.StandardOutput.ReadToEnd()));
        }
        catch (System.Exception ex)
        {
            await _messagingService.SendAsync(new(context.Channel, $"Something went wrong: {ex.Message}"));
        }
        finally
        {
            _lock.Release();
        }
    }
}
