namespace MnkeyFog.Model;

/// <summary>
/// Carries info about a player.  Do not serialize.
/// </summary>
public record Player(PlayerInfo Info, int Index) {
    public Player(string commandName, int index)
    : this(new PlayerInfo(commandName), index) { }

    [JsonIgnore]
    public string CommandName => Info.CommandName;
}
