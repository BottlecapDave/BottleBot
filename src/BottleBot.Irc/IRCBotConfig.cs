namespace BottleBot.Irc;

public record IRCBotConfig(string Server, int Port, string User, string Nick, string Channel, int MaxRetries = 3);
