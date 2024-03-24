namespace BottleBot.Messaging;

/// <summary>
/// Represents a sent message
/// </summary>
/// <param name="Timestamp">The datetime when the message was sent</param>
/// <param name="User">The user who sent the message</param>
/// <param name="Channel">The channel to send the message to</param>
/// <param name="Content">The contents of the message</param>
public record Message(DateTimeOffset Timestamp, string User, string Channel, string Content);
