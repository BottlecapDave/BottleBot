namespace BottleBot.Messaging;

/// <summary>
/// The details of the message to send
/// </summary>
/// <param name="Channel">The channel to send the message to</param>
/// <param name="Content">The content of the message to send</param>
public record SendMessageContent(string Channel, string Content);