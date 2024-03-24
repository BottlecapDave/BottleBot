using BottleBot;
using BottleBot.Commands;
using BottleBot.Commands.SlackNotify;
using BottleBot.Irc;
using BottleBot.MessageLogging;
using Microsoft.Extensions.Logging;

LogLevel logLevel = Enum.Parse<LogLevel>(Environment.GetEnvironmentVariable("LOG_LEVEL"), true);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(logLevel)
        .AddConsole();
});

string server = Environment.GetEnvironmentVariable("IRC_SERVER");
int port = int.Parse(Environment.GetEnvironmentVariable("IRC_PORT"));
string user = Environment.GetEnvironmentVariable("IRC_USER");
string nick = Environment.GetEnvironmentVariable("IRC_NICK");
string channel = Environment.GetEnvironmentVariable("IRC_CHANNEL");
string logRootDirectory = Environment.GetEnvironmentVariable("IRC_LOG_DIRECTORY");
string slackWebhookUrl = Environment.GetEnvironmentVariable("SLACK_NOTIFY_WEBHOOK_URL");

CommandService commandService = new();

LocalMessageLogger messageLogger = new(new(logRootDirectory));

ILogger<IRCbot> ircLogger = loggerFactory.CreateLogger<IRCbot>();
IRCBotConfig config = new(server, port, user, nick, channel);
IRCbot bot = new(commandService, messageLogger, config, ircLogger);

commandService.Register(new HelpCommand(bot, commandService));

SlackNotifyCommandConfig notifyCommandConfig = new(slackWebhookUrl);
commandService.Register(new SlackNotifyCommand(bot, notifyCommandConfig));

bot.StartAsync().Wait();
