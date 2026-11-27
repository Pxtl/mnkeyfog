namespace MnkeyFog.Model;

/// <summary>
/// A board's name in the text interface. <see cref="CommandName"/> is what a
/// user types to select the board (and the board component of space
/// command-names); <see cref="DisplayName"/> is what is shown on the board
/// itself.
/// </summary>
/// <param name="CommandName">The command-name a user types to select the board.</param>
/// <param name="DisplayName">The name shown on the board.</param>
public readonly record struct ModelName(string CommandName, string DisplayName);
