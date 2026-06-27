namespace KriegspielTicTacToe.Model.Views;

public record BoardView
: GameObjectView<Board> {
    public BoardView(Board board, Player? player, sbyte boardIndex)
    : base(board, player) {
        BoardIndex = boardIndex;
    }
    #region data properties
    public sbyte BoardIndex { get; init; }
    public string BoardName => CommandNameTool.BoardNameFromIndex(BoardIndex);
    #endregion

    #region calculated properties
	public bool IsDone => Value.IsDone;
    public sbyte RowCount => Value.RowCount;
    public sbyte ColumnCount => Value.ColumnCount;
	public int SpaceCount => Value.SpaceCount;
	#endregion

	public string GetSpaceName(GameView gameView, sbyte col, sbyte row)
    => gameView.GetSpaceName(BoardName, col, row);

	public SpaceView GetSpaceView(sbyte col, sbyte row)
    => new SpaceView(Value.Spaces[col, row], Player, col, row);

    public IEnumerable<SpaceView> AsSpaceViewEnumerable()
    => Value.AsSpaceViewEnumerable();

    public bool IsSpaceInsideOfBoard((sbyte Col, sbyte Row) pos, (sbyte Col, sbyte Row) boardSize)
    => Value.IsSpaceInsideOfBoard(pos, boardSize);
}
