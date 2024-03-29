using BottleBot.Commands;
using BottleBot.Commands.SlackNotify;
using BottleBot.Irc;
using BottleBot.Messaging;
using BottleBot.Messaging.Logging;
using BottleBot.Messaging.Processors;
using Microsoft.Extensions.Logging;
using Serilog;

string server = Environment.GetEnvironmentVariable("IRC_SERVER");
int port = int.Parse(Environment.GetEnvironmentVariable("IRC_PORT"));
string user = Environment.GetEnvironmentVariable("IRC_USER");
string nick = Environment.GetEnvironmentVariable("IRC_NICK");
string channel = Environment.GetEnvironmentVariable("IRC_CHANNEL");
string logRootDirectory = Environment.GetEnvironmentVariable("IRC_LOG_DIRECTORY");
string slackWebhookUrl = Environment.GetEnvironmentVariable("SLACK_NOTIFY_WEBHOOK_URL");
string[] keywords = Environment.GetEnvironmentVariable("MESSAGE_KEYWORDS").Split(',');

LogLevel logLevel = Enum.Parse<LogLevel>(Environment.GetEnvironmentVariable("LOG_LEVEL"), true);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    var logger = new LoggerConfiguration()
        .WriteTo.File(Path.Join(logRootDirectory, "main", "log.txt"), rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.SetMinimumLevel(logLevel)
        .AddConsole()
        .AddSerilog(logger);
});

CommandService commandService = new();
MessageProcessorService messageProcessorService = new();

ILogger<IRCbot> ircLogger = loggerFactory.CreateLogger<IRCbot>();
IRCBotConfig config = new(server, port, user, nick, channel);
IRCbot bot = new(messageProcessorService, config, ircLogger);

commandService.Register(new HelpCommand(bot, commandService));

SlackNotifyCommand notifyCommand = new(bot, new(slackWebhookUrl));
commandService.Register(notifyCommand);

// SummariseCommandConfig summariseCommandConfig = new("ls {0}");
// commandService.Register(new SummariseCommand(bot, messageLogger, summariseCommandConfig));

messageProcessorService.Register(new CommandMessageProcessor(commandService));

LocalMessageLogger messageLogger = new(new(logRootDirectory));
messageProcessorService.Register(new LogMessageProcessor(messageLogger));

messageProcessorService.Register(new KeywordMessageProcessor(new(keywords), notifyCommand));

bot.StartAsync().Wait();
