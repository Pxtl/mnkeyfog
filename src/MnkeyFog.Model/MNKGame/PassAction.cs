using System.ComponentModel;

namespace MnkeyFog.Model.MNKGame;

/// <summary>
/// A no-op move: the player passes their turn without placing a marker or
/// affecting the board. Offered by casual MNK templates so a player can skip
/// a turn.
/// </summary>
[ModelSerializable]
[ImmutableObject(true)]
public record PassAction
: GameAction {
    public override IPlayActionResult Attempt(GameState gameState, int actionPlayerIndex) {
        if (!gameState.PlayersState.CanTakeTurn(actionPlayerIndex)) {
            return new CannotTakeTurn(actionPlayerIndex);
        }
        gameState.ActionQueue.Add(GetPlayerAction(actionPlayerIndex));
        gameState.EndTurn(actionPlayerIndex, out var hasStateChanged);
        return new Enqueued(hasStateChanged);
    }

    public override void DoAction(GameState gameState, int actionPlayer) {
        // No-op: passing changes nothing on the board.
    }

    public override bool IsActionCollision(PlayerAction otherAction, int actionPlayer)
        => false;

    public override void DoActionCollision(GameState gameState, int actionPlayer, IReadOnlyList<PlayerAction> collisions) {
        // No-op: passing never collides with another action.
    }
}
