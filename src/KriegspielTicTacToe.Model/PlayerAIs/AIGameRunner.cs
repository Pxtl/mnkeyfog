using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.Views;

namespace KriegspielTicTacToe.Model.PlayerAIs;

public static class AIGameRunner {
	public static ScoreCard RunAIGame(GameTemplate gameTemplate, OrderedDictionary<Player, IPlayerAI> aiPlayers) {
		var gameState = new GameState(aiPlayers.Keys.ToArray(), gameTemplate, true);
		while(!gameState.IsGameOver) {
			while(!gameState.PlayManager.IsRoundOver && !gameState.IsGameOver) {
				var player = gameState.PlayManager.PlayersAvailableForTurn.First();
				var ai = aiPlayers[player];
				var gameView = new GameView(gameState, player);
				ai.Attempt(gameView, gameView.GetAvailableActions());
			}
			gameState.PlayManager.EndRound(out _);
		}
		return gameState.ScoreCard;
	}
}
