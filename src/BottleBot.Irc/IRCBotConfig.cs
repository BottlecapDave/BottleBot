namespace BottleBot.Irc;

public record IRCBotConfig(string Server, int Port, string User, string Nick, string Channel, string LogRootDir, int MaxRetries = 3);
