namespace BottleBot.MessageLogging;

public class LocalMessageLogger : IMessageLogger
{
    private readonly LocalMessageLoggerConfig _config;

    public LocalMessageLogger(LocalMessageLoggerConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Save a new message
    /// </summary>
    /// <param name="message">The message to save</param>
    public async Task SaveAsync(SaveMessage message)
    {
        string logFileName = Path.Join(_config.RootDir, message.Channel, $"{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
        string logFileDirectory = Directory.GetParent(logFileName).FullName;
        if (Directory.Exists(logFileDirectory) == false)
        {
            Directory.CreateDirectory(logFileDirectory);
        }

        string now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        await File.AppendAllLinesAsync(logFileName, new string[] { $"[{now}] ({message.User}): {message.Content}" });
    }
}
