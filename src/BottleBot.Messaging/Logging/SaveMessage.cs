namespace BottleBot.Messaging.Logging;

/// <summary>
/// The content of the message to save
/// </summary>
/// <param name="Timestamp">The timestamp of the message</param>
/// <param name="User">The user who sent the message</param>
/// <param name="Channel">The channel the message was sent from</param>
/// <param name="Content">The content of the message</param>
public record SaveMessage(DateTimeOffset Timestamp, string User, string Channel, string Content);
