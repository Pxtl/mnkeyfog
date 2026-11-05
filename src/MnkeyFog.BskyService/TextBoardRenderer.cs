using System.Text;
using MnkeyFog.Text;

namespace MnkeyFog.BskyService;

/// <summary>
/// Renders a game board as compact text for Bluesky, with a turn/score header,
/// a scores footer, and a character budget for posts (DMs get a larger budget).
///
/// The shared pipe-drawing lives in BoardRenderer (MnkeyFog.Text); this class
/// only adds the Bsky-specific header/footer and character budget, and sets the
/// BoardRenderer.CellNameResolver so that empty cells display their
/// space *name* (so players can type the name of the space to move) instead of
/// the fog-aware command name the CommandLine renderer uses.
/// </summary>
public static class TextBoardRenderer {
    private static readonly BoardRenderer _renderer = new();

    static TextBoardRenderer() {
        BoardRenderer.CellNameResolver = BskyCellName;
    }

    /// <summary>
    /// Render the board from the given view, with turn/score info, budgeted to
    /// <paramref name="maxChars"/> characters.
    /// </summary>
    public static string Render(GameView gameView, int maxChars = 300) {
        var sb = new StringBuilder(256);

        // Header: turn (or winners when the game is over).
        if (gameView.IsGameOver) {
            var winners = gameView.ScoreCard.Highest.AsPlayerInfos(gameView.PlayersState).ToList();
            sb.Append(winners.Count == 0
                ? "Game over. Draw."
                : $"Game over. Winner: {string.Join(", ", winners.Select(w => w.Mark))}");
        } else {
            var turnMarks = string.Join(
                ",",
                gameView.PlayersState.PlayersAvailableForTurn.Select(p => p.Mark)
            );
            sb.Append($"Turn: {turnMarks}");
        }

        sb.AppendLine();

        // Board(s) — shared pipe-drawing from MnkeyFog.Text.
        sb.Append(_renderer.DrawBoards(gameView, int.MaxValue));

        // Footer: scores.
        var scoreSummary = RenderScores(gameView);
        if (scoreSummary.Length > 0) {
            sb.AppendLine();
            sb.Append(scoreSummary);
        }

        var result = sb.ToString();
        return result.Length <= maxChars ? result : result[..maxChars];
    }

    /// <summary>
    /// Bsky's cell-name rule, overriding the CommandLine fog-aware default: show
    /// the space name on empty cells so players can type the name to move, and
    /// show the occupying mark (already fog-filtered for filled cells) on
    /// occupied cells.
    /// </summary>
    private static string BskyCellName(GameView gameView, sbyte boardIndex, sbyte col, sbyte row) {
        var board = gameView.GetBoardViewByIndex(boardIndex);
        var space = board.GetSpaceView(col, row);
        return space.MarkIndex.HasValue
            ? gameView.PlayersState.GetMark(space.MarkIndex)
            : board.GetSpaceName(gameView, col, row);
    }

    private static string RenderScores(GameView gameView) {
        var parts = gameView.ScoreCard.PlayerScores
            .Select(ps => $"{gameView.PlayersState.GetMark(ps.PlayerIndex)}:{ps.Score}");
        var joined = string.Join(" ", parts);
        return joined.Length == 0 ? "" : $"Scores: {joined}";
    }
}
