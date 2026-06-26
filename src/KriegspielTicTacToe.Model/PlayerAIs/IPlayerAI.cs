using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.Views;

namespace KriegspielTicTacToe.Model.PlayerAIs;

public interface IPlayerAI {
	void Attempt(GameView gameView, IEnumerable<GameActionFactory> actionFactories);
	public string Description { get; }
}
