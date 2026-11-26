namespace MnkeyFog.Text;

/// <summary>
/// Text-interface-oriented extension methods on <see cref="BoardView"/>.
/// </summary>
public static class BoardViewExtensions {
    /// <summary>
    /// The board's <c>CommandName</c> — what a user types to select the board and
    /// the board component of space command-names.
    /// </summary>
    public static string GetBoardName(this BoardView boardView, ModelNamer namer) {
        return namer.GetBoardModelName(boardView.BoardIndex).CommandName;
    }

    public static string GetSpaceName(this BoardView boardView, ModelNamer namer, GameView gameView, sbyte col, sbyte row)
    => gameView.GetSpaceName(boardView.GetBoardName(namer), col, row, namer);
}