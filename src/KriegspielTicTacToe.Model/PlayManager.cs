namespace KriegspielTicTacToe.Model;

using OneOf;
using OneOf.Types;

/// <summary>
/// Base class containing shared play management logic for retirement and turn tracking.
/// </summary>
public abstract class PlayManager
{
    #region members
    public IReadOnlyList<Player> Players {get;init;} = new List<Player>();

    public int RoundIndex {get;set;}

    public HashSet<Player> ResignedPlayersSet {get; init;} = new HashSet<Player>();
    
    public HashSet<Player> PlayedPlayersSet {get; init;} = new HashSet<Player>();
    #endregion

    #region methods
    /// <summary>
    /// Advance to the next player's turn.  Must notify if current player is
    /// resigning because that effects how the turn counter is incremented.
    /// </summary>
    /// <remarks>
    /// The logic here is tricky. If the current player resigns, then their slot
    /// is removed from the index and we skip incrementing.  But that means
    /// there are 2 "index 0" turns, so we can't use "index 0" as new round in
    /// that case.
    /// </remarks>
    public void EndTurn(Player currentPlayer, out bool hasStateChanged) {
        MarkPlayerPlayed(currentPlayer);
        EndedTurn(out hasStateChanged);
    }

    public void MarkPlayerPlayed(Player player) {
        if (PlayedPlayersSet.Contains(player)) {
            throw new InvalidOperationException($"Player {player} has already played");
        }
        PlayedPlayersSet.Add(player);
    }

    public void EndRound(out bool hasStateChanged) {
        RoundIndex += 1;
        PlayedPlayersSet.Clear();
        EndedRound(out hasStateChanged);
    }
    
    /// <summary>
    /// Test if the given player has resigned.
    /// </summary>
    public bool IsResignedPlayer(Player player)
        => ResignedPlayersSet.Contains(player);

    /// <summary>
    /// Mark the given player as resigned. If it is the current player's turn,
    /// do *not* call NextTurn.
    /// </summary>
    public void ResignPlayer(Player player) {
        ResignedPlayersSet.Add(player);
    }

    /// <summary>
    /// True if the given player is able to take a turn.
    /// </summary>
    public bool CanTakeTurn(Player? player)
        => player != null && PlayersAvailableForTurn.Contains(player);

    protected abstract void EndedTurn(out bool hasStateChanged);
    protected abstract void EndedRound(out bool hasStateChanged);
    #endregion

    #region helper properties
    [JsonIgnore()]
    public int NumberOfActivePlayers
        => ActivePlayers.Count();
    
    /// <summary>
    /// Get all of the current active players.  Order is consistent.
    /// </summary>
    [JsonIgnore()]
    public IEnumerable<Player> ActivePlayers
        => Players.Except(ResignedPlayersSet);

    [JsonIgnore()]
    public abstract IEnumerable<Player> PlayersAvailableForTurn {get;}

    [JsonIgnore()]
    public bool IsRoundOver
        => PlayersAvailableForTurn.Count() == 0;

    /// <summary>
    /// Abstract GameStateText property - implemented by subclasses.
    /// </summary>
    [JsonIgnore()]
    public abstract string GameStateText {get;}
    /// <summary>
    /// The action buffer.  Is set by parent's Init, but Init is
    /// post-constructor so must be nullable.
    /// </summary>
    [JsonIgnore()]
    public PlayActionBuffer? PlayActionBuffer { get; internal set; }
    #endregion
}
