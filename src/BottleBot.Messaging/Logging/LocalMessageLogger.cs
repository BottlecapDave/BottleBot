namespace BottleBot.Messaging.Logging;

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
        string logFileName = GetFilePath(DateOnly.FromDateTime(DateTime.Now), message.Channel);
        string now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        await File.AppendAllLinesAsync(logFileName, new string[] { $"[{now}] ({message.User}): {message.Content}" });
    }

    /// <summary>
    /// Retrieve the contents for the provided timestamp and channel.
    /// </summary>
    /// <param name="date">The date the messages should be retrieved for.</param>
    /// <param name="channel">The channel the messages should be retrieved for.</param>
    /// <returns>The messages that were raised for the specified date</returns>
    public Task<string[]> GetAsync(DateOnly date, string channel)
    {
        string logFileName = GetFilePath(date, channel);
        return File.ReadAllLinesAsync(logFileName);
    }

    /// <summary>
    /// Get the path to the file for the specified date/channel.
    /// </summary>
    /// <param name="date">The date to get the file path for</param>
    /// <param name="channel">The channel to get  the file path for</param>
    /// <returns>The file path for the message file for the specified date/channel.</returns>
    private string GetFilePath(DateOnly date, string channel)
    {
        string logFileName = Path.Join(_config.RootDir, channel, $"{date.ToString("yyyy-MM-dd")}.txt");
        string logFileDirectory = Directory.GetParent(logFileName).FullName;
        if (Directory.Exists(logFileDirectory) == false)
        {
            Directory.CreateDirectory(logFileDirectory);
        }

        return logFileName;
    }
}
