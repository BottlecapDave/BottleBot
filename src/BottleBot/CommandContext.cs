namespace BottleBot;

/// <summary>
/// The context of the command that has been executed
/// </summary>
/// <param name="User">The user who requested the command</param>
/// <param name="Channel">The channel that the command was executed within</param>
/// <param name="Content">The configuration of the command</param>
public record CommandContext(string User, string Channel, string Content);
