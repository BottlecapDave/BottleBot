namespace BottleBot.MessageLogging;

/// <summary>
/// The config for the LocalMessageLogger
/// </summary>
/// <param name="RootDir">The root directory for the local message logger</param>
public record LocalMessageLoggerConfig(string RootDir);
