using KriegspielTicTacToe.Model.Template;
using OneOf;
using OneOf.Types;

namespace KriegspielTicTacToe.Model.Views;

/// <summary>
/// Player-based wrapper for GameState that restricts the API only to things
/// that a player can do and see.
/// </summary>
public record GameView
: GameObjectView<GameState> {
    #region Constructors
    public GameView(GameState gameState, Player? player)
    : base(gameState, player) { }
    #endregion

    #region Calculated Members
    public bool CanTakeTurn => Value.PlayManager.CanTakeTurn(Player);
    public IEnumerable<BoardView> Boards { get {
        for(sbyte i = 0; i < BoardsCount; i+=1) {
            yield return new BoardView(Value.Boards[i], Player, i);
        }
    } }
    

    [JsonIgnore()]
    public bool IsGameOver => Value.IsGameOver;

    [JsonIgnore()]
    public IEnumerable<Player> Winners => Value.Winners;
    
    #endregion

    #region Player Actions
    public IEnumerable<GameActionFactory> GetAvailableActions() {
        if (Player == null) {
            throw new InvalidOperationException($"{nameof(Player)} is null.");
        } else {
            return Value.GameTemplate.GetAvailableActions(Value, Player);
        }
    }

    public Resigned ResignPlayer() {
        if (Player == null) {
            throw new InvalidOperationException($"{nameof(Player)} is null.");
        } else {
            Value.PlayManager.ResignPlayer(Player);
            return new Resigned(Player);
        }
    }

    public IPlayActionResult Attempt(GameAction playAction) {
        if (Player == null) {
            throw new InvalidOperationException($"{nameof(Player)} is null.");
        }
        return playAction.GetPlayerAction(Player).Attempt(Value);
    }

    public OneOf<Result<BoardView>, InvalidCommand, BoardIsDone> AttemptBoard(string boardName)
        => CommandNameTool.GetBoardIndexByName(boardName, Value.Boards.Count).Match(
            notFound => new InvalidCommand(boardName),
            indexResult => AttemptBoard(indexResult.Value)
        );
    
    public OneOf<Result<BoardView>, InvalidCommand, BoardIsDone> AttemptBoard(sbyte boardIndex)
        => (boardIndex >= 0 && boardIndex < BoardsCount)
            ? (
                Value.Boards[boardIndex].IsDone
                    ? OneOf<Result<BoardView>, InvalidCommand, BoardIsDone>.FromT2(new BoardIsDone())
                    : new Result<BoardView>(GetBoardViewByIndex(boardIndex))
                )
            : new InvalidCommand(CommandNameTool.BoardNameFromIndex(boardIndex));
    
    //TODO: Row and Column attempt functions.
    #endregion

    #region private helpers
    public IEnumerable<string> BoardNames { get {
        for(var i = 1; i <= Value.Boards.Count; i += 1) {
            yield return i.ToString();
        }
    }}
    #endregion

    #region board management
    [JsonIgnore()]
    public sbyte BoardsCount => (sbyte)Value.Boards.Count;

    public BoardView GetBoardViewByIndex(sbyte boardIndex)
    => new BoardView(Value.Boards[boardIndex], Player, boardIndex);

    public BoardView GetBoardViewByName(string boardName) {
        if (BoardsCount == 1) {
            return Boards.Single();
        }
        var boardIndex = CommandNameTool.GetBoardIndexByName(boardName, BoardsCount).Match(
            notFound => throw new ArgumentException($"That is not a valid board: '{boardName}", nameof(boardName)),
            result => result.Value
        );
        return GetBoardViewByIndex(boardIndex);
    }
    #endregion

    #region space names

    [JsonIgnore()]
    public IEnumerable<string> SpaceNames
    => Boards.SelectMany(b => b
        .AsSpaceViewEnumerable()
        .Select(s => GetSpaceName(b.BoardName, s.Col, s.Row))
    ); //zero-pad.
        
    /// <summary>
    /// For the given space on the board, generate the space's name. Only used
    /// on small (3x3 or less) boards.
    /// </summary>
    private int GetSpaceNameAsInt(BoardView board, sbyte col, sbyte row) {
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

    private bool IsSpaceNamingNumpadLayout(BoardView board) => board.SpaceCount < 10;

    /// <summary>
    /// For the given space on the given board, generate the space's name.
    /// </summary>
    public string GetSpaceName(BoardView board, sbyte col, sbyte row)
    => (BoardsCount > 1 ? board.BoardName : "") //board name component
        + 
        (IsSpaceNamingNumpadLayout(board) //space name component
            ? GetSpaceNameAsInt(board, col, row).ToString()
            : (
                // letter component.  Can be only length 1 because max board size is 26.
                ((char)('A' + col)).ToString()
                // number component, zero-padded to SpaceNameLength without the letter component.
                + (board.RowCount - row).ToString(new string('0', SpaceNameLength(board) - 1)) 
            )
        );

    /// <summary>
    /// For the given space on the given board, generate the space's name.
    /// </summary>
    public string GetSpaceName(sbyte boardIndex, sbyte col, sbyte row) {
        var board = GetBoardViewByIndex(boardIndex);
        return GetSpaceName(board, col, row);
    }

    /// <summary>
    /// For the given space on the given board, generate the space's name.
    /// </summary>
    public string GetSpaceName(string boardName, sbyte col, sbyte row) {
        var board = GetBoardViewByName(boardName);
        return GetSpaceName(board, col, row);
    }

    public bool TryGetCoordinatesFromSpaceName(string spaceName, out sbyte boardIndex, out sbyte resultCol, out sbyte resultRow) {
        if (TryGetCoordinatesFromSpaceName(spaceName, out string boardName, out resultCol, out resultRow)) {
            if(CommandNameTool.TryGetBoardIndexByName(boardName, BoardsCount, out boardIndex)) {
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
    public bool TryGetCoordinatesFromSpaceName(string spaceName, out string boardName, out sbyte resultCol, out sbyte resultRow) {
        boardName = "1";
        if(BoardsCount > 1) {
            boardName = spaceName.Substring(0, 1);
        }
        var board = GetBoardViewByName(boardName);

        //brute-force search
        //todo: smarter algo
        for (sbyte col = 0; col < board.ColumnCount; col += 1) {
            for (sbyte row = 0; row < board.RowCount; row += 1) {
                if (GetSpaceName(boardName, col, row).Equals(spaceName, StringComparison.OrdinalIgnoreCase)) {
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
    /// Get how many chars the users will have to type in to type in a
    /// space-name.
    /// </summary>
    public int SpaceNameLength(BoardView board)
    => IsSpaceNamingNumpadLayout(board)
        ? 1
        : (int)Math.Log10(board.RowCount) + 2; //(int)Math.Log10(RowCount) is number of digits - 1.  Add 2, 1 for digits, 1 for letter.

    #endregion
}

