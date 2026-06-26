namespace KriegspielTicTacToe.Model;

using OneOf;
using OneOf.Types;

/// <summary>
/// PlayManager for turn-based mode - each player's move is immediately revealed.
/// </summary>
[ModelSerializable]
public class RoundRobinPlayManager
: PlayManager {
    #region constructors
    public RoundRobinPlayManager(IReadOnlyList<Player> players) : base(players) { }
    #endregion

    #region properties
    [JsonIgnore()]
    public override string GameStateText
        => PlayersAvailableForTurn.Count() > 0 
        ? $"Round-robin play. Current player is {PlayersAvailableForTurn.First().Mark}."
        : "Round over.";

    protected override void EndedRound(out bool hasStateChanged) {
        hasStateChanged = false;
    }

    protected override void EndedTurn(out bool hasStateChanged) {
        ActionQueue!.ExecutePendingActions();
        hasStateChanged = true;
    }

    [JsonIgnore()]
    public override IEnumerable<Player> PlayersAvailableForTurn
        => ActivePlayers.Except(PlayedPlayersSet).Take(1);
    #endregion
}

[ModelSerializable]
public record RoundRobinPlayManagerFactory
: Template.PlayManagerFactory {
    public static RoundRobinPlayManagerFactory Instance {get;} = new RoundRobinPlayManagerFactory();
	public override PlayManager Create(IReadOnlyList<Player> players) 
		=> new RoundRobinPlayManager(players);
}
