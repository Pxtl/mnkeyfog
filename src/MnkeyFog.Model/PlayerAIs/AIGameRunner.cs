using MnkeyFog.Model.Template;
using MnkeyFog.Model.Views;
using PxtlCa.Collections;

namespace MnkeyFog.Model.PlayerAIs;

public static class AIGameRunner {
    public const int MaxPlayerAIAttemptCount = 100;

    /// <summary>
    /// Variant of RunGame that outputs the gamestate for examination.
    /// </summary>
    public static ScoreCard RunAIGame(GameTemplate gameTemplate, OrderedDictionary<PlayerInfo, IPlayerAI> aiPlayers, out GameState gameState, bool isRandomPlayerOrder = true) {
        var playersIndexed = aiPlayers.Keys.ToPlayersIndexed();
        gameState = new GameState(aiPlayers.Keys.ToArray(), gameTemplate, isRandomPlayerOrder);
        while (!gameState.IsGameOver) {
            var playerAttemptCounts = new AutoConstructingDictionary<int, int>(); //defaults all keys to zero.
            while (!gameState.PlayersState.IsRoundOver && !gameState.IsGameOver) {
                var player = gameState.PlayersState.PlayersAvailableForTurn.First();
                var gameView = new GameView(gameState, player.Index);

                var playerAttemptsCount = playerAttemptCounts[player.Index];
                if (playerAttemptsCount > MaxPlayerAIAttemptCount) {
                    // resign if the player AI can't figure out a legal move.
                    gameView.ResignPlayer();
                } else {
                    var ai = aiPlayers[player.Info];
                    ai.Attempt(gameView);
                    playerAttemptCounts[player.Index] = playerAttemptsCount + 1;
                }
            }
            gameState.EndRound(out _);
        }
        var playersState = gameState.PlayersState;
        // since the player order has been shuffled, the indices won't match, so
        // we have to translate them back to the original index order.
        var translatedPlayerScores = gameState.ScoreCard.PlayerScores.Select(ps => new PlayerIndexScore(
            playersIndexed.Single(pi => pi.Info == playersState.GetPlayer(ps.PlayerIndex).Info).Index,
            ps.Score
        ));
        return new ScoreCard(translatedPlayerScores);
    }

    public static ScoreCard RunAIGame(GameTemplate gameTemplate, OrderedDictionary<PlayerInfo, IPlayerAI> aiPlayers, bool isRandomPlayerOrder = true) {
        return RunAIGame(gameTemplate, aiPlayers, out _, isRandomPlayerOrder);
    }
}
