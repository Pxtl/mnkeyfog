namespace MnkeyFog.Model;

using System.ComponentModel;
using OneOf;
using OneOf.Types;

/// <summary>
/// PlayManager for turn-based mode - each player's move is immediately revealed.
/// </summary>
[ModelSerializable]
[ImmutableObject(true)] //not read by anything just useful metadata.
public class RoundRobinPlayManager
: PlayManager {
    public static RoundRobinPlayManager Instance { get; } = new RoundRobinPlayManager();

    public override string GameStateText(PlayersState playerState)
        => playerState.PlayersAvailableForTurn.Count() > 0
        ? $"Round-robin play. Current player is {playerState.PlayersAvailableForTurn.First().Info.CommandName}."
        : "Round over.";

    public override void EndedRound(GameState gameState, out bool hasStateChanged) {
        hasStateChanged = false;
    }

    public override void EndedTurn(GameState gameState, out bool hasStateChanged) {
        gameState.ExecutePendingActions();
        hasStateChanged = true;
    }

    public override IEnumerable<Player> PlayersAvailableForTurn(PlayersState playerState)
        => playerState.ActivePlayers.Where(p => !playerState.PlayedPlayerIndicesSet.Contains(p.Index)).Take(1);
}
