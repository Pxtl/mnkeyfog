namespace MnkeyFog.Text;

using OneOf;
using OneOf.Types;

/// <summary>
/// Text-interface-oriented extension methods on <see cref="GameView"/>.
/// </summary>
public static class GameViewExtensions {
    #region Board Names
    public static IEnumerable<string> GetAllBoardNames(this GameView gameView) {
        for (var i = 1; i <= gameView.Boards.Count; i += 1) {
            yield return i.ToString();
        }
    }

    /// <summary>
    /// Attempts to select a board by name.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the selected board if successful;
    /// an <see cref="InvalidCommand"/> if the name is invalid; or a
    /// <see cref="BoardIsDone"/> if the board is finished.
    /// </returns>
    public static OneOf<Result<BoardView>, InvalidCommand, BoardIsDone> AttemptBoard(this GameView gameView, string boardName, ModelNamer namer)
    => namer.GetBoardIndexByName(boardName, gameView.Boards.Count).Match(
        notFound => new InvalidCommand(boardName),
        indexResult => gameView.AttemptBoard(indexResult.Value, namer)
    );

    public static BoardView GetBoardViewByName(this GameView gameView, string boardName, ModelNamer namer) {
        if (gameView.BoardsCount == 1) {
            return gameView.Boards.Single();
        }
        var boardIndex = namer.GetBoardIndexByName(boardName, gameView.BoardsCount).Match(
            notFound => throw new ArgumentException($"That is not a valid board: '{boardName}", nameof(boardName)),
            result => result.Value
        );
        return gameView.GetBoardViewByIndex(boardIndex);
    }

    /// <summary>
    /// Attempts to select a board by index.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the selected board if successful;
    /// an <see cref="InvalidCommand"/> if the index is out of range; or a
    /// <see cref="BoardIsDone"/> if the board is finished.
    /// </returns>
    public static OneOf<Result<BoardView>, InvalidCommand, BoardIsDone> AttemptBoard(this GameView gameView, sbyte boardIndex, ModelNamer namer)
    => (boardIndex >= 0 && boardIndex < gameView.BoardsCount)
        ? (
            gameView.Boards[boardIndex].IsDone
                ? OneOf<Result<BoardView>, InvalidCommand, BoardIsDone>.FromT2(new BoardIsDone())
                : new Result<BoardView>(gameView.GetBoardViewByIndex(boardIndex))
            )
        : new InvalidCommand(namer.BoardNameFromIndex(boardIndex));

    #endregion

    #region Space Names
    public static IEnumerable<string> GetSpaceNames(this GameView gameView, ModelNamer namer)
    => gameView.Boards.SelectMany(b => b
        .AsSpaceViewEnumerable()
        .Select(s => gameView.GetSpaceName(b.GetBoardName(namer), s.Col, s.Row, namer))
    ); //zero-pad.

    public static bool TryGetCoordinatesFromSpaceName(this GameView gameView, string spaceName, ModelNamer namer, out sbyte boardIndex, out sbyte resultCol, out sbyte resultRow) {
        if (gameView.TryGetCoordinatesFromSpaceName(spaceName, namer, out string boardName, out resultCol, out resultRow)) {
            if (namer.TryGetBoardIndexByName(boardName, gameView.BoardsCount, out boardIndex)) {
                return true;
            } else {
                return false;
            }
        } else {
            boardIndex = -1;
            return false;
        }
    }

    /// <summary>
    /// For the given space index code, find the coordinates.  Uses a "Try"
    /// signature so that it shall return false if the given spaceindex is not
    /// on the board at all.
    /// </summary>
    public static bool TryGetCoordinatesFromSpaceName(this GameView gameView, string spaceName, ModelNamer namer, out string boardName, out sbyte resultCol, out sbyte resultRow) {
        boardName = "1";
        if (gameView.BoardsCount > 1) {
            boardName = spaceName.Substring(0, 1);
        }
        var board = gameView.GetBoardViewByName(boardName, namer);

        //brute-force search
        //todo: smarter algo
        for (sbyte col = 0; col < board.ColumnCount; col += 1) {
            for (sbyte row = 0; row < board.RowCount; row += 1) {
                if (gameView.GetSpaceName(boardName, col, row, namer).Equals(spaceName, StringComparison.OrdinalIgnoreCase)) {
                    resultCol = col;
                    resultRow = row;
                    return true;
                }
            }
        }
        // if not found
        resultCol = resultRow = -1;
        return false;
    }

    /// <summary>
    /// For the given space on the board, generate the space's name. Only used
    /// on small (3x3 or less) boards.
    /// </summary>
    private static int GetSpaceNameAsInt(BoardView board, sbyte col, sbyte row) {
        //up to basic 3x3. Supports larger but this function does not get
        //called for those.

        //7 8 9
        //4 5 6
        //1 2 3
        return board.RowCount * (board.ColumnCount - 1) //top-left
            + col
            - row * board.ColumnCount
            + 1; //1-based
    }

    private static bool IsSpaceNamingNumpadLayout(BoardView board) => board.SpaceCount < 10;

    /// <summary>
    /// For the given space on the given board, generate the space's name.
    /// </summary>
    public static string GetSpaceName(this GameView gameView, BoardView board, sbyte col, sbyte row, ModelNamer namer)
    => (gameView.BoardsCount > 1 ? board.GetBoardName(namer) : "") //board name component
        +
        (IsSpaceNamingNumpadLayout(board) //space name component
            ? GetSpaceNameAsInt(board, col, row).ToString()
            : (
                // letter component.  Can be only length 1 because max board size is 26.
                ((char)('A' + col)).ToString()
                // number component, zero-padded to SpaceNameLength without the letter component.
                + (board.RowCount - row).ToString(new string('0', gameView.SpaceNameLength(board) - 1))
            )
        );

    /// <summary>
    /// For the given space on the given board, generate the space's name.
    /// </summary>
    public static string GetSpaceName(this GameView gameView, sbyte boardIndex, sbyte col, sbyte row, ModelNamer namer) {
        var board = gameView.GetBoardViewByIndex(boardIndex);
        return gameView.GetSpaceName(board, col, row, namer);
    }

    /// <summary>
    /// For the given space on the given board, generate the space's name.
    /// </summary>
    public static string GetSpaceName(this GameView gameView, string boardName, sbyte col, sbyte row, ModelNamer namer) {
        var board = gameView.GetBoardViewByName(boardName, namer);
        return gameView.GetSpaceName(board, col, row, namer);
    }

    /// <summary>
    /// Get how many chars the users will have to type in to type in a
    /// space-name.
    /// </summary>
    public static int SpaceNameLength(this GameView gameView, BoardView board)
    => IsSpaceNamingNumpadLayout(board)
        ? 1
        : (int)Math.Log10(board.RowCount) + 2; //(int)Math.Log10(RowCount) is number of digits - 1.  Add 2, 1 for digits, 1 for letter.

    #endregion
}
