namespace MnkeyFog.Text;

using OneOf;
using OneOf.Types;

/// <summary>
/// Extension methods on <see cref="GameView"/>.
/// </summary>
public static class GameViewExtensions {
    /// <summary>
    /// Attempts to select a board by name.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the selected board if successful;
    /// an <see cref="InvalidCommand"/> if the name is invalid; or a
    /// <see cref="BoardIsDone"/> if the board is finished.
    /// </returns>
    public static OneOf<Result<BoardView>, InvalidCommand, BoardIsDone> AttemptBoard(this GameView gameView, string boardName)
    => BoardNameTool.GetBoardIndexByName(boardName, gameView.Boards.Count).Match(
        notFound => new InvalidCommand(boardName),
        indexResult => gameView.AttemptBoard(indexResult.Value)
    );

    /// <summary>
    /// Attempts to select a board by index.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the selected board if successful;
    /// an <see cref="InvalidCommand"/> if the index is out of range; or a
    /// <see cref="BoardIsDone"/> if the board is finished.
    /// </returns>
    public static OneOf<Result<BoardView>, InvalidCommand, BoardIsDone> AttemptBoard(this GameView gameView, sbyte boardIndex)
    => (boardIndex >= 0 && boardIndex < gameView.BoardsCount)
        ? (
            gameView.Boards[boardIndex].IsDone
                ? OneOf<Result<BoardView>, InvalidCommand, BoardIsDone>.FromT2(new BoardIsDone())
                : new Result<BoardView>(gameView.GetBoardViewByIndex(boardIndex))
            )
        : new InvalidCommand(BoardNameTool.BoardNameFromIndex(boardIndex));
}
