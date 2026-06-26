using System.ComponentModel.DataAnnotations;
using OneOf;
using OneOf.Types;

namespace KriegspielTicTacToe.Model;

[ModelSerializable]
public abstract record GameAction() {
    public abstract IPlayActionResult Attempt(GameState gameState, Player actionPlayer);
    public abstract void DoAction(GameState gameState, Player actionPlayer);
    public abstract bool IsActionCollision(PlayerAction otherAction, Player actionPlayer);
    public abstract void DoActionCollision(GameState gameState, Player actionPlayer, IReadOnlyList<PlayerAction> collisions);
    public PlayerAction GetPlayerAction(Player player) => new PlayerAction(this, player);
}
