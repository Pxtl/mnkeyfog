using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.Views;
using PxtlCa.Collections;

namespace KriegspielTicTacToe.Model.PlayerAIs;

public static class AIGameRunner {
	public const int MaxPlayerAIAttemptCount = 100;

	/// <summary>
	/// Variant of RunGame that outputs the gamestate for examination.
	/// </summary>
	public static ScoreCard RunAIGame(GameTemplate gameTemplate, OrderedDictionary<Player, IPlayerAI> aiPlayers, out GameState gameState) {
		gameState = new GameState(aiPlayers.Keys.ToArray(), gameTemplate, true);
		while(!gameState.IsGameOver) {
			var playerAttemptCounts = new AutoConstructingDictionary<Player, int>(); //defaults all keys to zero.
			while(!gameState.PlayManager.IsRoundOver && !gameState.IsGameOver) {
				var player = gameState.PlayManager.PlayersAvailableForTurn.First();
				var gameView = new GameView(gameState, player);
				
				var playerAttemptsCount = playerAttemptCounts[player];
				if (playerAttemptsCount > MaxPlayerAIAttemptCount) {
					// resign if the player AI can't figure out a legal move.
					gameView.ResignPlayer();
				} else {
					var ai = aiPlayers[player];
					ai.Attempt(gameView, gameView.GetAvailableActions());
					playerAttemptCounts[player] = playerAttemptsCount + 1;
				}
			}
			gameState.PlayManager.EndRound(out _);
		}
		return gameState.ScoreCard;
	}

	public static ScoreCard RunAIGame(GameTemplate gameTemplate, OrderedDictionary<Player, IPlayerAI> aiPlayers) {
		return RunAIGame(gameTemplate, aiPlayers, out _);
	}
}
