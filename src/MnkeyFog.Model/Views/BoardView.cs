namespace MnkeyFog.Model.Views;

public record BoardView
: GameObjectView {
    public BoardView(Board board, int? playerIndex, sbyte boardIndex)
    : base(playerIndex) {
        BoardIndex = boardIndex;

        IsDone = board.IsDone;
        Spaces = new SpaceView[board.ColumnCount, board.RowCount];
        for (sbyte row = 0; row < board.RowCount; row += 1) {
            for (sbyte col = 0; col < board.ColumnCount; col += 1) {
                Spaces[col, row] = new SpaceView(board.Spaces[col, row], PlayerIndex, col, row, BoardIndex);
            }
        }
        ScoreCard = board.Ruleset.Score(board, playerIndex);
    }
    #region data properties

    /// <summary>
    /// The index for the corresponding Board within the main GameState
    /// boardarray.
    /// </summary>
    public sbyte BoardIndex { get; init; }

    /// <summary>
    /// The player's view of the spaces on the board.  The Spaces will match the
    /// Spaces of the underlying Board, but their contents may not.
    /// </summary>
    public SpaceView[,] Spaces { get; init; }

    /// <summary>
    /// The player's view of the score for the board.  May or may not reflect
    /// the true nature of the score.
    /// </summary>
    public ScoreCard ScoreCard { get; init; }
    #endregion

    #region copied calculated properties

    /// <summary>
    /// Whether or not the underlying board is Done.  If it is Done, then it
    /// accepts no actions.
    /// </summary>
    public bool IsDone { get; init; }
    #endregion

    #region helper properties
    /// <summary>
    /// The name of the board as used in the UI.  Typically it's BoardIndex+1
    /// converted to string.
    /// </summary>
    [JsonIgnore()]
    public string BoardName
    => CommandNameTool.BoardNameFromIndex(BoardIndex);

    /// <summary>
    /// Number of columns on the board.
    /// </summary>
    [JsonIgnore()]
    public sbyte ColumnCount
    => Spaces.GetLength(0).AsSByte;

    /// <summary>
    /// Number of rows on the board.
    /// </summary>
    [JsonIgnore()]
    public sbyte RowCount
    => Spaces.GetLength(1).AsSByte;

    /// <summary>
    /// Get how many spaces are on the board.
    /// </summary>
    [JsonIgnore()]
    public int SpaceCount
    => Spaces.GetLength(0) * Spaces.GetLength(1);
    #endregion

    public string GetSpaceName(GameView gameView, sbyte col, sbyte row)
    => gameView.GetSpaceName(BoardName, col, row);

    public SpaceView GetSpaceView(sbyte col, sbyte row)
    => Spaces[col, row];

    /// <summary>
    /// Enumerate across all of the spaces on the board.
    /// </summary>
    public IEnumerable<SpaceView> AsSpaceViewEnumerable() {
        for (sbyte col = 0; col < ColumnCount; col += 1) {
            for (sbyte row = 0; row < RowCount; row += 1) {
                yield return Spaces[col, row];
            }
        }
    }

    /// <summary>
    /// True if the given position is a valid space on this board.
    /// </summary>
    public bool IsSpaceInsideOfBoard((sbyte Col, sbyte Row) pos)
    => Board.IsSpaceInsideOfBoard(pos, (ColumnCount, RowCount));
}
