namespace MnkeyFog.Model.Views;

public record BoardView
: GameObjectView {
    public BoardView(Board board, int? playerIndex, sbyte boardIndex, bool isGameOver)
    : base(playerIndex) {
        BoardIndex = boardIndex;

        IsDone = board.IsDone;
        // When the game is over, every mark is visible to every player (no fog
        // of war) — the same omniscient visibility as a null/spectator index.
        int? markVisibilityPlayerIndex = isGameOver ? null : playerIndex;
        SpaceDTOs = new SpaceViewDTO[board.ColumnCount, board.RowCount];
        for (sbyte row = 0; row < board.RowCount; row += 1) {
            for (sbyte col = 0; col < board.ColumnCount; col += 1) {
                var space = board.Spaces[col, row];
                var mark = space.IsKnownToPlayerIndex(markVisibilityPlayerIndex)
                    ? space.MarkIndex
                    : null;
                SpaceDTOs[col, row] = new SpaceViewDTO(mark);
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
    /// The player's view of the spaces on the board, one per cell.  Each entry
    /// holds just the mark; the full <see cref="SpaceView"/> for a cell is
    /// reconstructed from its position by <see cref="GetSpaceView(sbyte, sbyte)"/>
    /// and <see cref="AsSpaceViewEnumerable"/>.  The marks match those of the
    /// underlying Board, but their contents may not (fog of war).
    /// </summary>
    public SpaceViewDTO[,] SpaceDTOs { get; init; }

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
    /// Number of columns on the board.
    /// </summary>
    [JsonIgnore()]
    public sbyte ColumnCount
    => SpaceDTOs.GetLength(0).AsSByte;

    /// <summary>
    /// Number of rows on the board.
    /// </summary>
    [JsonIgnore()]
    public sbyte RowCount
    => SpaceDTOs.GetLength(1).AsSByte;

    /// <summary>
    /// Get how many spaces are on the board.
    /// </summary>
    [JsonIgnore()]
    public int SpaceCount
    => SpaceDTOs.GetLength(0) * SpaceDTOs.GetLength(1);
    #endregion

    public SpaceView GetSpaceView(sbyte col, sbyte row)
    => CreateSpaceView(col, row);

    private SpaceView CreateSpaceView(sbyte col, sbyte row)
    => new SpaceView(SpaceDTOs[col, row].MarkIndex, PlayerIndex, col, row, BoardIndex);

    /// <summary>
    /// Enumerate across all of the spaces on the board.
    /// </summary>
    public IEnumerable<SpaceView> AsSpaceViewEnumerable() {
        for (sbyte col = 0; col < ColumnCount; col += 1) {
            for (sbyte row = 0; row < RowCount; row += 1) {
                yield return CreateSpaceView(col, row);
            }
        }
    }

    /// <summary>
    /// True if the given position is a valid space on this board.
    /// </summary>
    public bool IsSpaceInsideOfBoard((sbyte Col, sbyte Row) pos)
    => Board.IsSpaceInsideOfBoard(pos, (ColumnCount, RowCount));
}
