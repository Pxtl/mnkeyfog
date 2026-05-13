namespace KriegspielTicTacToe.Model;

/// <summary>
/// class to represent a space on the board.
/// </summary>
public record Space {
    /// <summary>
    /// The current state of the space - NULL means available.
    /// </summary>
    public char? MarkChar {get;set;}
    private HashSet<char> _knownToPlayersSet {get;set;} = new HashSet<char>();
    public IReadOnlySet<char> KnownToPlayersSet => _knownToPlayersSet;
    
    /// <summary>
    /// Test if this space is known to the given player.
    /// </summary>
    public bool IsKnownToPlayer(char player) 
        => KnownToPlayersSet.Contains(player);
    
    /// <summary>
    /// Mark this space as known to the given player.
    /// </summary>
    public void MakeKnownToPlayer(char player) {
        _knownToPlayersSet.Add(player);
    }

    /// <summary>
    /// Get the display value of this space for the given player.  Show always
    /// if the player is null.
    /// </summary>
    public string ToString(char? player)
        => (!player.HasValue || IsKnownToPlayer(player.Value))
            ? (MarkChar.ToString() ?? " ")
            : " ";
}
