using System.Text;
using MnkeyFog.Text;

namespace MnkeyFog.Bsky;

/// <summary>
/// Renders a game board as compact text for Bluesky, with a turn/score header,
/// a scores footer, and a character budget for posts (DMs get a larger budget).
///
/// The shared pipe-drawing lives in BoardRenderer (MnkeyFog.Text); this class
/// only adds the Bsky-specific header/footer and character budget.  It renders
/// with doShowSpaceCommandName set, so empty cells display their space *name*
/// (so players can type the name of the space to move).
/// </summary>
public class TextBoardRenderer {
    /// <summary>
    /// BSky renders emoji in a fixed-width layout of ~14-16 characters per line,
    /// so the board's pipe-drawing is budgeted to that width.
    /// </summary>
    private const int MaxRenderWidth = 15;

    private readonly BoardRenderer _renderer;

    public TextBoardRenderer(ModelNamer namer) {
        _renderer = new BoardRenderer(namer);
    }

    /// <summary>
    /// Render the board from the given view, with turn/score info, budgeted to
    /// <paramref name="maxChars"/> characters.
    /// </summary>
    public string Render(GameView gameView, int maxChars = 300) {
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

        // Board(s) — shared pipe-drawing from MnkeyFog.Text.  Show space
        // command names on empty cells so players can type the name to move.
        sb.Append(_renderer.DrawBoards(gameView, doShowSpaceCommandName: true, maxRenderWidth: MaxRenderWidth));

        // Footer: scores.
        var scoreSummary = RenderScores(gameView);
        if (scoreSummary.Length > 0) {
            sb.AppendLine();
            sb.Append(scoreSummary);
        }

        var result = sb.ToString();
        return result.Length <= maxChars ? result : result[..maxChars];
    }

    private string RenderScores(GameView gameView) {
        var parts = gameView.ScoreCard.PlayerScores
            .Select(ps => $"{gameView.PlayersState.GetMark(ps.PlayerIndex)}:{ps.Score}");
        var joined = string.Join(" ", parts);
        return joined.Length == 0 ? "" : $"Scores: {joined}";
    }
}
