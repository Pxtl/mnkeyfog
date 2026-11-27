using System.ComponentModel;

namespace MnkeyFog.Model;

/// <summary>
/// Represents a player in the game. Stores their <see cref="CommandName"/> - the
/// human-typeable name the player joins with (a CLI mark char, or a BSky handle).
/// The display Mark is a separate concern, computed by the ModelNamer.
/// </summary>
[ModelSerializable]
public sealed record PlayerInfo {
    public PlayerInfo(string commandName) {
        if (commandName == null) {
            throw new ArgumentNullException("CommandName must be provided", nameof(commandName));
        } else if (string.IsNullOrWhiteSpace(commandName)) {
            throw new ArgumentException("CommandName must not be empty or whitespace.", nameof(commandName));
        } else if (char.IsControl(commandName[0])) {
            throw new ArgumentException("CommandName must not be a control char.", nameof(commandName));
        }
        CommandName = commandName;
    }

    /// <summary>
    /// The human-typeable name the player joins with. Settable so the BSky join
    /// path can assign the player's handle after the slot is claimed.
    /// </summary>
    public string CommandName { get; set; }

    public override string ToString()
        => CommandName;

    /// <summary>
    /// Create a PlayerInfo from a char.
    /// </summary>
    public static PlayerInfo FromChar(char value) => new PlayerInfo(value.ToString());

    /// <summary>
    /// Create a PlayerInfo from a 1-character nullable string.  Returns null if the
    /// parameter is not a 1-character string.
    /// </summary>
    public static PlayerInfo? FromString(string? value) {
        if (value == null) {
            return null;
        } else if (value.Length != 1) {
            return null;
        }
        return new PlayerInfo(value);
    }

    /// <summary>
    /// Implicit conversion from char string to PlayerInfo.
    /// </summary>
    public static implicit operator PlayerInfo(string value) => new PlayerInfo(value);

    /// <summary>
    /// Implicit conversion from char to PlayerInfo.
    /// </summary>
    public static implicit operator PlayerInfo(char value) => new PlayerInfo(value.ToString());

    /// <summary>
    /// Implicit conversion from PlayerInfo to string.
    /// </summary>
    public static implicit operator string(PlayerInfo player) => player.CommandName;
}
