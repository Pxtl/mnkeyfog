namespace MnkeyFog.Model.Views;

public record BoardView
: GameObjectView {
    public BoardView(Board board, Player? player, sbyte boardIndex)
    : base(player) {
        BoardIndex = boardIndex;

        BoardName = CommandNameTool.BoardNameFromIndex(BoardIndex);
	    IsDone = board.IsDone;
        Spaces = new SpaceView[board.ColumnCount, board.RowCount];
        for(sbyte row = 0; row < board.RowCount; row += 1) {
            for(sbyte col = 0; col < board.ColumnCount; col += 1) {
                Spaces[col, row] =  new SpaceView(board.Spaces[col, row], Player, col, row);
            }
        }
    }
    #region data properties
    public sbyte BoardIndex { get; init; }
    public SpaceView[,] Spaces {get; init; }
    #endregion

    #region copied calculated properties
    public string BoardName { get; init; }
	public bool IsDone { get; init; }
    #endregion

    #region helper properties
    [JsonIgnore()]
    public sbyte ColumnCount
    => (sbyte)Spaces.GetLength(0);

    [JsonIgnore()]
    public sbyte RowCount
    => (sbyte)Spaces.GetLength(1);

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

    public IEnumerable<SpaceView> AsSpaceViewEnumerable() {
        for (sbyte col = 0; col < ColumnCount; col += 1) {
            for (sbyte row = 0; row < RowCount; row += 1) {
                yield return Spaces[col, row];
            }
        }
    }

    public bool IsSpaceInsideOfBoard((sbyte Col, sbyte Row) pos)
    => Board.IsSpaceInsideOfBoard(pos, (ColumnCount, RowCount));
}
