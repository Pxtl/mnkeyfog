namespace MnkeyFog.Text;

/// <summary>
/// Text-interface-oriented extension methods on <see cref="BoardView"/>.
/// </summary>
public static class BoardViewExtensions {
    /// <summary>
    /// The board's <see cref="ModelName"/> — its <c>CommandName</c> (what a user
    /// types to select the board / the board component of space command-names)
    /// and its <c>DisplayName</c> (what is shown). Callers pick the field they
    /// need.
    /// </summary>
    public static ModelName GetBoardName(this BoardView boardView, ModelNamer namer)
    => namer.GetBoardModelName(boardView.BoardIndex);

    public static string GetSpaceName(this BoardView boardView, ModelNamer namer, GameView gameView, sbyte col, sbyte row)
    => gameView.GetSpaceName(boardView.GetBoardName(namer).CommandName, col, row, namer);
}