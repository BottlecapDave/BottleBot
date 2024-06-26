﻿namespace BottleBot.Commands;

public interface ICommandService
{
    /// <summary>
    /// The collection of registered commands
    /// </summary>
    IEnumerable<ICommand> Commands { get; }
}
