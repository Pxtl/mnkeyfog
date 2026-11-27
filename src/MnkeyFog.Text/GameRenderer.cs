using System.Text;

namespace MnkeyFog.Text;

/// <summary>
/// Renders a whole game as compact text for chat platforms (e.g. Bluesky): the
/// game name, an event message (whose turn it is, or the result when the game
/// is over), the board(s), the active players (with their scores), and the
/// resigned players. The output is budgeted to a character limit for posts (DMs
/// get a larger budget).
///
/// The shared board pipe-drawing lives in <see cref="BoardRenderer"/>; this
/// class adds the surrounding game context. Empty board cells show their space
/// command name so players can type the name to move.
/// </summary>
public class GameRenderer {
    /// <summary>
    /// Compact board width. Bluesky renders emoji in a fixed-width layout of
    /// ~14-16 characters per line, so the board's pipe-drawing is budgeted to
    /// that width.
    /// </summary>
    private const int MaxRenderWidth = 15;

    private readonly BoardRenderer _renderer;

    public GameRenderer(BoardRenderer renderer) {
        _renderer = renderer;
    }

    /// <summary>
    /// Render the game from the given view, budgeted to
    /// <paramref name="maxChars"/> characters.
    /// </summary>
    public string Render(GameView gameView, int maxChars = 300) {
        var sb = new StringBuilder(256);

        // 1. Game name.
        if (gameView.GameTemplate.CommandName is { Length: > 0 } gameName) {
            sb.Append(gameName);
            sb.AppendLine();
        }

        // 2. Event message: whose turn it is (or the result when the game is over).
        sb.Append(RenderEventMessage(gameView));
        sb.AppendLine();

        // 3. Board(s) — shared pipe-drawing. Show space command names on empty
        // cells so players can type the name to move.
        sb.Append(_renderer.DrawBoards(gameView, doShowSpaceCommandName: true, maxRenderWidth: MaxRenderWidth));

        // 4. Active players (with scores).
        var activePlayers = RenderActivePlayers(gameView);
        if (activePlayers.Length > 0) {
            sb.AppendLine();
            sb.Append(activePlayers);
        }

        // 5. Resigned players.
        var resignedPlayers = RenderResignedPlayers(gameView);
        if (resignedPlayers.Length > 0) {
            sb.AppendLine();
            sb.Append(resignedPlayers);
        }

        var result = sb.ToString();
        return result.Length <= maxChars ? result : result[..maxChars];
    }

    private string RenderEventMessage(GameView gameView) {
        var players = gameView.PlayersState;
        if (gameView.IsGameOver) {
            var winners = gameView.ScoreCard.Highest.PlayerScores
                .Select(ps => _renderer.ModelNamer.GetPlayerDisplayName(ps.PlayerIndex, players));
            var joined = string.Join(", ", winners);
            return joined.Length == 0
                ? "Game over. Draw."
                : $"Game over. Winner: {joined}";
        } else {
            var turnNames = string.Join(
                ",",
                players.PlayersAvailableForTurn.Select(p => _renderer.ModelNamer.GetPlayerDisplayName(p.Index, players))
            );
            return $"Turn: {turnNames}";
        }
    }

    private string RenderActivePlayers(GameView gameView) {
        var players = gameView.PlayersState;
        var scoreCard = gameView.ScoreCard;
        var parts = players.ActivePlayers
            .Select(p => $"{_renderer.ModelNamer.GetPlayerDisplayName(p.Index, players)} : {GetScore(scoreCard, p.Index)}");
        var joined = string.Join(" ", parts);
        return joined.Length == 0 ? "" : $"Players: {joined}";
    }

    private string RenderResignedPlayers(GameView gameView) {
        var players = gameView.PlayersState;
        var parts = players.PlayersIndexed
            .Where(p => players.IsResignedPlayer(p.Index))
            .Select(p => _renderer.ModelNamer.GetPlayerDisplayName(p.Index, players));
        var joined = string.Join(" ", parts);
        return joined.Length == 0 ? "" : $"Resigned: {joined}";
    }

    private static int GetScore(ScoreCard scoreCard, int playerIndex)
        => scoreCard.PlayerScores
            .Where(ps => ps.PlayerIndex == playerIndex)
            .Select(ps => ps.Score)
            .DefaultIfEmpty(0)
            .Sum();
}
