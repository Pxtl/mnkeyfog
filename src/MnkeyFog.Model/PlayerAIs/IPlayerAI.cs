using MnkeyFog.Model.Template;
using MnkeyFog.Model.Views;

namespace MnkeyFog.Model.PlayerAIs;

public interface IPlayerAI {
	void Attempt(GameView gameView, IEnumerable<GameActionFactory> actionFactories);
	public string Description { get; }
}
