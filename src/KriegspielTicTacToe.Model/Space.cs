namespace KriegspielTicTacToe.Model;

/// <summary>
/// class to represent a space on the board.
/// </summary>
[ModelSerializable]
public sealed record Space {
    /// <summary>
    /// The current state of the space - null means available.
    /// '█' means it's an impasse (two players contested this space in same round).
    /// </summary>
    public string? Mark {get;set;}
    
    private HashSet<Player> _knownToPlayersSet {get;set;} = [];
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.None, TypeNameHandling = TypeNameHandling.None)] //non-polymorphic
    public IReadOnlySet<Player> KnownToPlayersSet => _knownToPlayersSet;
    
    /// <summary>
    /// Test if this space is known to the given player.
    /// </summary>
    public bool IsKnownToPlayer(Player? player) 
        => (player == null) || KnownToPlayersSet.Contains(player);
    
    /// <summary>
    /// Mark this space as known to the given player.
    /// </summary>
    public void MakeKnownToPlayer(Player player) {
        _knownToPlayersSet.Add(player);
    }

    /// <summary>
    /// Get the display value of this space for the given player.
    /// Show always if the player is null.
    /// </summary>
    public string ToString(Player? player)
        => (player == null || IsKnownToPlayer(player))
            ? (Mark ?? " ")
            : " ";
}
