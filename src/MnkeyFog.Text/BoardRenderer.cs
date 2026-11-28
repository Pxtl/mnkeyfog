using System.Text;

namespace MnkeyFog.Text;

/// <summary>
/// Draws the full board based on the given gamestate, from the perspective of
/// the given player.
/// </summary>
public class BoardRenderer {
    private enum RowKind {
        Space,
        RowColumnHeader,
        RowColumnData
    }

    #region constructors
    public BoardRenderer()
    : this(new ModelNamer(), BoardRendererConfiguration.FullPipes) {
    }

    public BoardRenderer(BoardRendererConfiguration configuration)
    : this(new ModelNamer(), configuration) {
    }

    public BoardRenderer(ModelNamer modelNamer)
    : this(modelNamer, BoardRendererConfiguration.FullPipes) {
    }

    public BoardRenderer(ModelNamer modelNamer, BoardRendererConfiguration configuration) {
        ModelNamer = modelNamer;
        Configuration = configuration;
    }
    #endregion

    public ModelNamer ModelNamer { get; }

    public BoardRendererConfiguration Configuration { get; }

    public string DrawBoards(GameView gameView, bool doShowSpaceCommandName, int maxRenderWidth = int.MaxValue) {
        bool isRowColumn = Configuration.ShowRowColumnLabels;
        bool doShowBoardNameForAllBoards = gameView.BoardsCount > 1;
        var maxRowCount = gameView.Boards.Max(b => b.RowCount);
        var sb = new StringBuilder();
        sbyte nextDrawnBoardIndex = 0;
        var extraColumns = isRowColumn ? 1 : 0;
        var hasLeader = !isRowColumn;
        var boardRenderWidth = GetBoardRenderWidth(gameView.GetBoardViewByIndex(0).ColumnCount + extraColumns, hasLeader);

        while (nextDrawnBoardIndex < gameView.BoardsCount) {
            sbyte drawBoardIndex = nextDrawnBoardIndex;
            var doShowBoardName = doShowBoardNameForAllBoards;

            if (Configuration.HasTopBorder) {
                DrawBorderRow(
                    gameView, drawBoardIndex,
                    Configuration.TopLeftCorner.ToString(),
                    Configuration.TopIntersection.ToString(),
                    Configuration.TopRightCorner.ToString(),
                    Configuration.CellRowSeparator,
                    doShowBoardName, maxRenderWidth, sb,
                    extraColumns, hasLeader
                );
                if (hasLeader) {
                    doShowBoardName = false;
                }
            }

            if (isRowColumn) {
                nextDrawnBoardIndex = DrawBoardRow(
                    gameView, drawBoardIndex, 0, boardRenderWidth, doShowBoardName, maxRenderWidth, sb,
                    RowKind.RowColumnHeader, doShowSpaceCommandName
                );
                doShowBoardName = false;
            }

            for (sbyte row = 0; row < maxRowCount; row += 1) {
                if ((row > 0 || isRowColumn) && Configuration.HasRowSeparators) {
                    DrawBorderRow(
                        gameView, drawBoardIndex,
                        Configuration.LeftIntersection.ToString(),
                        Configuration.MiddleIntersection.ToString(),
                        Configuration.RightIntersection.ToString(),
                        Configuration.CellRowSeparator,
                        false, maxRenderWidth, sb,
                        extraColumns, hasLeader
                    );
                }
                var rowKind = isRowColumn ? RowKind.RowColumnData : RowKind.Space;
                var showSpaceCommandName = isRowColumn ? false : doShowSpaceCommandName;
                nextDrawnBoardIndex = DrawBoardRow(
                    gameView, drawBoardIndex, row, boardRenderWidth, doShowBoardName, maxRenderWidth, sb,
                    rowKind, showSpaceCommandName
                );
                doShowBoardName = false;
            }

            if (Configuration.HasBottomBorder) {
                DrawBorderRow(
                    gameView, drawBoardIndex,
                    Configuration.BottomLeftCorner.ToString(),
                    Configuration.BottomIntersection.ToString(),
                    Configuration.BottomRightCorner.ToString(),
                    Configuration.CellRowSeparator,
                    false, maxRenderWidth, sb,
                    extraColumns, hasLeader
                );
            }
        }

        sb.AppendLine();
        return sb.ToString();
    }

    public int GetBoardRenderWidth(BoardView board) => GetBoardRenderWidth(board.ColumnCount, hasLeader: true);

    private int GetBoardRenderWidth(int cellCount, bool hasLeader) {
        return cellCount * Configuration.CellWidth
            + (Configuration.HasColumnSeparators ? cellCount - 1 : 0)
            + (Configuration.HasLeftBorder ? 1 : 0)
            + (Configuration.HasRightBorder ? 1 : 0)
            + (hasLeader ? 2 : 0);
    }

    private sbyte DrawBorderRow(
        GameView gameView,
        sbyte startBoardIndex,
        string cellStartString,
        string cellIntersectionString,
        string cellEndString,
        string cellSpanString,
        bool doShowBoardName,
        int maxRenderWidth,
        StringBuilder sb,
        int extraColumns,
        bool hasLeader
    ) {
        var boardIndex = startBoardIndex;
        for (; boardIndex < gameView.BoardsCount; boardIndex += 1) {
            var board = gameView.GetBoardViewByIndex(boardIndex);
            var cursorX = GetCursorX(sb);
            var cellCount = board.ColumnCount + extraColumns;
            var boardWidth = GetBoardRenderWidth(cellCount, hasLeader);
            if (cursorX > 0 && (cursorX + boardWidth > maxRenderWidth)) {
                break;
            }
            if (hasLeader) {
                DrawBoardLeader(doShowBoardName, sb, boardIndex, board);
            }
            sb.Append($"{cellStartString}{cellSpanString}");
            for (int col = 0; col < cellCount - 1; col += 1) {
                sb.Append($"{cellIntersectionString}{cellSpanString}");
            }
            sb.Append(cellEndString);
        }
        sb.AppendLine();
        return boardIndex;
    }

    private sbyte DrawBoardRow(
        GameView gameView,
        sbyte startBoardIndex,
        sbyte rowIndex,
        int boardRenderWidth,
        bool doShowBoardName,
        int maxRenderWidth,
        StringBuilder sb,
        RowKind rowKind,
        bool doShowSpaceCommandName
    ) {
        var boardIndex = startBoardIndex;
        var isRowColumn = rowKind != RowKind.Space;
        var extraColumns = isRowColumn ? 1 : 0;
        for (; boardIndex < gameView.BoardsCount; boardIndex += 1) {
            var board = gameView.GetBoardViewByIndex(boardIndex);
            var cursorX = GetCursorX(sb);
            var cellCount = board.ColumnCount + extraColumns;
            if (cursorX > 0 && (cursorX + boardRenderWidth > maxRenderWidth)) {
                break;
            }
            if (!isRowColumn) {
                DrawBoardLeader(doShowBoardName, sb, boardIndex, board);
            }
            if (Configuration.HasLeftBorder) {
                sb.Append(Configuration.CellColumnSeparator);
            }
            for (int cell = 0; cell < cellCount; cell += 1) {
                if (cell > 0 && Configuration.HasColumnSeparators) {
                    sb.Append(Configuration.CellColumnSeparator);
                }
                var body = GetCellBody(gameView, boardIndex, (sbyte)cell, rowIndex, rowKind, doShowSpaceCommandName, doShowBoardName);
                DrawSpaceBody(body, Configuration.CellWidth, sb);
            }
            if (Configuration.HasRightBorder) {
                sb.Append(Configuration.CellColumnSeparator);
            }
        }
        sb.AppendLine();
        return boardIndex;
    }

    private string GetCellBody(
        GameView gameView,
        sbyte boardIndex,
        sbyte cellIndex,
        sbyte rowIndex,
        RowKind rowKind,
        bool doShowSpaceCommandName,
        bool doShowBoardName
    ) {
        var board = gameView.GetBoardViewByIndex(boardIndex);
        return rowKind switch {
            RowKind.Space => ModelNamer.GetSpaceBody(gameView, boardIndex, cellIndex, rowIndex, doShowSpaceCommandName),
            RowKind.RowColumnHeader => cellIndex == 0
                ? (doShowBoardName ? ModelNamer.GetBoardModelName(boardIndex).DisplayName : string.Empty)
                : ModelNamer.GetColumnModelName((sbyte)(cellIndex - 1), board.ColumnCount).DisplayName,
            RowKind.RowColumnData => cellIndex == 0
                ? ModelNamer.GetRowModelName(rowIndex, board.RowCount).DisplayName
                : ModelNamer.GetSpaceBody(gameView, boardIndex, (sbyte)(cellIndex - 1), rowIndex, doShowSpaceCommandName: false),
            _ => throw new ArgumentOutOfRangeException(nameof(rowKind))
        };
    }

    private void DrawBoardLeader(bool doShowBoardName, StringBuilder sb, sbyte boardIndex, BoardView board) {
        sb.Append(doShowBoardName
            ? (board.IsDone
                ? Configuration.DoneBoardLeader
                : Configuration.BoardNameMappingToBoardLeader(ModelNamer.GetBoardModelName(boardIndex).DisplayName))
            : Configuration.EmptyBoardLeader
        );
    }

    private static void DrawSpaceBody(string body, int cellWidth, StringBuilder sb) {
        body = body.PadLeft(cellWidth / 2 + 1);
        body = body.PadRight(cellWidth);
        sb.Append(body);
    }

    public static int GetCursorX(StringBuilder sb) {
        int charsSinceLineBreak = 0;

        for (int i = sb.Length - 1; i >= 0; i--) {
            if (sb[i] == '\n') {
                break;
            }
            charsSinceLineBreak++;
        }
        return charsSinceLineBreak;
    }
}
