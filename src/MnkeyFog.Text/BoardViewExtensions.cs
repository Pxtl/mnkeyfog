namespace MnkeyFog.Text;

/// <summary>
/// Text-interface-oriented extension methods on <see cref="BoardView"/>.
/// </summary>
public static class BoardViewExtensions {
    /// <summary>
    /// The name of the board as used in the UI.  Typically it's BoardIndex+1
    /// converted to string.
    /// </summary>
    public static string GetBoardName(this BoardView boardView, ModelNamer namer) {
        return namer.BoardNameFromIndex(boardView.BoardIndex);
    }

    public static string GetSpaceName(this BoardView boardView, ModelNamer namer, GameView gameView, sbyte col, sbyte row)
    => gameView.GetSpaceName(boardView.GetBoardName(namer), col, row, namer);
}