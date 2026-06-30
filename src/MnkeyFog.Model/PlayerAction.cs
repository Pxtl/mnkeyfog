namespace MnkeyFog.Model;

[ModelSerializable]
public sealed record PlayerAction(GameAction GameAction, Player Player) {
    public IPlayActionResult Attempt(GameState gameState) => GameAction.Attempt(gameState, Player);
	public bool IsActionCollision(PlayerAction otherAction) => GameAction.IsActionCollision(otherAction, Player);
    public void DoAction(GameState gameState) => GameAction.DoAction(gameState, Player);
    public void DoActionCollision(GameState gameState, IReadOnlyList<PlayerAction> collisions) 
    => GameAction.DoActionCollision(gameState, Player, collisions);
}
