namespace KriegspielTicTacToe.Model;

using OneOf;
using OneOf.Types;

/// <summary>
/// PlayManager for turn-based mode - each player's move is immediately revealed.
/// </summary>
public class RoundRobinPlayManager : PlayManager
{
    #region constructors
    public RoundRobinPlayManager(IReadOnlyList<Player> players) {
        Players = players;
    }
    #endregion

    #region properties
    [JsonIgnore()]
    public override string GameStateText
        => PlayersAvailableForTurn.Count() > 0 
        ? $"Player {PlayersAvailableForTurn.First().Value} turn."
        : "Round over.";

    protected override void EndedRound(out bool hasStateChanged) {
        hasStateChanged = false;
    }

    protected override void EndedTurn(out bool hasStateChanged) {
        PlayActionBuffer!.ExecutePendingActions();
        hasStateChanged = true;
    }

    [JsonIgnore()]
    public override IEnumerable<Player> PlayersAvailableForTurn
        => ActivePlayers.Except(PlayedPlayersSet).Take(1);
    #endregion
}
