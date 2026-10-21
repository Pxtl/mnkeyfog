using System.Text;

namespace MnkeyFog.Text;

/// <summary>
/// Draws the full board based on the given gamestate, from the perspective of
/// the given player.
/// </summary>
public class BoardRenderer {
    #region constructors
    public BoardRenderer()
    : this(BoardRendererConfiguration.FullPipes) {
    }

    public BoardRenderer(BoardRendererConfiguration configuration) {
        Configuration = configuration;
    }
    #endregion

    public BoardRendererConfiguration Configuration { get; }

    public string DrawBoards(
        GameView gameView,
        int maxRenderWidth = int.MaxValue
    ) {
        bool doShowBoardNameForAllBoards = gameView.BoardsCount > 1;
        var maxRowCount = gameView.Boards.Max(b => b.RowCount);
        var sb = new StringBuilder();

        sbyte nextDrawnBoardIndex = 0;
        var boardRenderWidth = GetBoardRenderWidth(gameView.GetBoardViewByIndex(nextDrawnBoardIndex));

        while (nextDrawnBoardIndex < gameView.BoardsCount) {
            sbyte drawBoardIndex = nextDrawnBoardIndex;

            var doShowBoardName = doShowBoardNameForAllBoards;
            if (Configuration.HasTopBorder) {
                DrawBorderRow(
                    gameView,
                    drawBoardIndex,
                    Configuration.TopLeftCorner,
                    Configuration.TopIntersection,
                    Configuration.TopRightCorner,
                    Configuration.CellRowSeparator,
                    doShowBoardName,
                    maxRenderWidth,
                    sb
                );
                doShowBoardName = false;
            }

            for (sbyte row = 0; row < maxRowCount; row += 1) {
                if (row > 0) {
                    if (Configuration.HasRowSeparators) {
                        DrawBorderRow(
                            gameView,
                            drawBoardIndex,
                            Configuration.LeftIntersection,
                            Configuration.MiddleIntersection,
                            Configuration.RightIntersection,
                            Configuration.CellRowSeparator,
                            false,
                            maxRenderWidth,
                            sb
                        );
                    }
                }
                nextDrawnBoardIndex = DrawBoardSpacesRow(
                    gameView,
                    drawBoardIndex,
                    row,
                    boardRenderWidth,
                    doShowBoardName,
                    maxRenderWidth,
                    sb
                );
                doShowBoardName = false;
            }
            if (Configuration.HasBottomBorder) {
                DrawBorderRow(
                    gameView,
                    drawBoardIndex,
                    Configuration.BottomLeftCorner,
                    Configuration.BottomIntersection,
                    Configuration.BottomRightCorner,
                    Configuration.CellRowSeparator,
                    false,
                    maxRenderWidth,
                    sb
                );
            }
        }

        sb.AppendLine();
        return sb.ToString();
    }

    public int GetBoardRenderWidth(BoardView board)
        => board.ColumnCount * Configuration.CellWidth
        + (Configuration.HasColumnSeparators ? board.ColumnCount - 1 : 0)
        + (Configuration.HasLeftBorder ? 1 : 0)
        + (Configuration.HasRightBorder ? 1 : 0)
        + 2; //indent

    /// <summary>
    /// Helper function to draw a border row of the board.
    /// Wraps to newline when maxWidth is exceeded.
    /// </summary>
    private sbyte DrawBorderRow(
        GameView gameView,
        sbyte startBoardIndex,
        string cellStartString,
        string cellIntersectionString,
        string cellEndString,
        string cellSpanString,
        bool doShowBoardName,
        int maxRenderWidth,
        StringBuilder sb
    ) {
        var boardIndex = startBoardIndex;
        for (; boardIndex < gameView.BoardsCount; boardIndex += 1) {
            var board = gameView.GetBoardViewByIndex(boardIndex);
            var cursorX = GetCursorX(sb);

            //wrap check - break if cursor would exceed maxWidth
            if (cursorX > 0 && (cursorX + GetBoardRenderWidth(board) > maxRenderWidth)) {
                break;
            }

            DrawBoardLeader(doShowBoardName, sb, boardIndex, board);

            sb.Append($"{cellStartString}{cellSpanString}");

            for (sbyte col = 0; col < board.ColumnCount - 1; col += 1) {
                sb.Append($"{cellIntersectionString}{cellSpanString}");
            }
            sb.Append(cellEndString);
        }
        sb.AppendLine();
        return boardIndex;
    }

    /// <summary>
    /// Draw a row of board spaces with window wrapping.
    /// Wraps to newline when maxWidth is exceeded.
    /// </summary>
    private sbyte DrawBoardSpacesRow(
        GameView gameView,
        sbyte startBoardIndex,
        sbyte rowIndex,
        int boardRenderWidth,
        bool doShowBoardName,
        int maxRenderWidth,
        StringBuilder sb
    ) {
        var boardIndex = startBoardIndex;
        for (; boardIndex < gameView.BoardsCount; boardIndex += 1) {
            var board = gameView.GetBoardViewByIndex(boardIndex);
            var cursorX = GetCursorX(sb);

            //wrap check - break if cursor would exceed maxWidth
            if (cursorX > 0 && (cursorX + boardRenderWidth > maxRenderWidth)) {
                break;
            }

            DrawBoardLeader(doShowBoardName, sb, boardIndex, board);

            if (Configuration.HasLeftBorder) {
                sb.Append(Configuration.CellColumnSeparator);
            }
            for (sbyte col = 0; col < board.ColumnCount; col += 1) {
                if (col > 0 && Configuration.HasColumnSeparators) {
                    sb.Append(Configuration.CellColumnSeparator);
                }
                var body = CommandNameTool.SpaceCommandName(gameView, boardIndex, col, rowIndex);
                DrawSpaceBody(body, Configuration.CellWidth, sb);
            }
            if (Configuration.HasRightBorder) {
                sb.Append(Configuration.CellColumnSeparator);
            }
        }
        sb.AppendLine();
        return boardIndex;
    }

    private void DrawBoardLeader(bool doShowBoardName, StringBuilder sb, sbyte boardIndex, BoardView board) {
        sb.Append(doShowBoardName
            ? (board.IsDone
                ? Configuration.DoneBoardLeader
                : Configuration.BoardNameMappingToBoardLeader(CommandNameTool.BoardNameFromIndex(boardIndex))
            ) : Configuration.EmptyBoardLeader
        );
    }

    /// <summary>
    /// Helper function to draw the body-spaces of the board.
    /// </summary>
    private static void DrawSpaceBody(string body, int cellWidth, StringBuilder sb) {
        body = body.PadLeft(cellWidth / 2 + 1);
        body = body.PadRight(cellWidth);
        sb.Append(body);
    }

    /// <summary>
    /// Get the cursor position (number of characters since last line break).
    /// </summary>
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
