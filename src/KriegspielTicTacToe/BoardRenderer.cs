using System.Text;

namespace KriegspielTicTacToe;

/// <summary>
/// Draws the full board based on the given gamestate, from the perspective of
/// the given player.
/// </summary>
public static class BoardRenderer {
    public static string DrawBoards(
        GameView gameView,
        int maxRenderWidth = int.MaxValue
    ) {
        bool doShowBoardCode = gameView.BoardsCount > 1;
        var maxRowCount = gameView.Boards.Max(b => b.RowCount);
        var sb = new StringBuilder();

        sbyte nextDrawnBoardIndex = 0;
        var boardRenderWidth = GetBoardRenderWidth(gameView.GetBoardViewByIndex(nextDrawnBoardIndex));
       
        while (nextDrawnBoardIndex < gameView.BoardsCount) {
            DrawBorderRow(gameView, nextDrawnBoardIndex, "┌", "┬", "┐", "───", doShowBoardCode, maxRenderWidth, sb);
            
            for(sbyte row = 0; row < maxRowCount; row += 1) {
                if(row > 0) {
                    DrawBorderRow(gameView, nextDrawnBoardIndex, "├", "┼", "┤", "───", false, maxRenderWidth, sb);
                }
                DrawBoardSpacesRow(gameView, nextDrawnBoardIndex, "│", row, boardRenderWidth, maxRenderWidth, sb);
            }
            nextDrawnBoardIndex = DrawBorderRow(gameView, nextDrawnBoardIndex, "└", "┴", "┘", "───", false, maxRenderWidth, sb);
        }

        sb.AppendLine();
        return sb.ToString();
    }

    public static int GetBoardRenderWidth(BoardView board)
        => board.ColumnCount * 4 + 3; // 4 chars per-space, plus 2 for indent, 

    /// <summary>
    /// Helper function to draw a border row of the board.
    /// Wraps to newline when maxWidth is exceeded.
    /// </summary>
    private static sbyte DrawBorderRow(
        GameView gameView,
        sbyte startBoardIndex,
        string startBarString, 
        string midBarString, 
        string endBarString, 
        string spanString, 
        bool showBoardName,
        int maxRenderWidth,
        StringBuilder sb
    ) {
        var boardIndex = startBoardIndex;
        for (; boardIndex < gameView.BoardsCount; boardIndex += 1) {
            var board = gameView.GetBoardViewByIndex(boardIndex);
            var cursorX = sb.GetCursorX();
            
            //wrap check - break if cursor would exceed maxWidth
            if(cursorX > 0 && (cursorX + GetBoardRenderWidth(board) > maxRenderWidth)) {
                break;
            }

            sb.Append(showBoardName
                ? (board.IsDone
                    ? " ✓" //board is done so just show a checkmark.
                    : CommandNameTool.BoardNameFromIndex(boardIndex).PadLeft(2) //key-index to choose it
                ) : "  " //blank space
            );

            sb.Append($"{startBarString}{spanString}");
            
            for(sbyte col = 0; col < board.ColumnCount - 1; col += 1) {
                sb.Append($"{midBarString}{spanString}");
            }
            sb.Append(endBarString);
        }
        sb.AppendLine();
        return boardIndex;
    }

    /// <summary>
    /// Draw a row of board spaces with window wrapping.
    /// Wraps to newline when maxWidth is exceeded.
    /// </summary>
    private static sbyte DrawBoardSpacesRow(
        GameView gameView, 
        sbyte startBoardIndex,
        string borderBarString,
        sbyte rowIndex,
        int boardRenderWidth,
        int maxRenderWidth,
        StringBuilder sb
    ) {
        var boardIndex = startBoardIndex;
        for (; boardIndex < gameView.BoardsCount; boardIndex += 1)
        {
            var board = gameView.GetBoardViewByIndex(boardIndex);
            var cursorX = sb.GetCursorX();
            
            //wrap check - break if cursor would exceed maxWidth
            if(cursorX > 0 && (cursorX + boardRenderWidth > maxRenderWidth)) {
                break;
            }

            //indentation before start of board, provides space for board number.
            sb.Append("  ");

            for(sbyte col = 0; col < board.ColumnCount; col += 1) {
                var body = CommandNameTool.SpaceCommandName(gameView, boardIndex, col, rowIndex);
                DrawSpaceBody(body, borderBarString, sb);
            }
            sb.Append(borderBarString);
        }
        sb.AppendLine();
        return boardIndex;
    }

    /// <summary>
    /// Helper function to draw the body-spaces of the board.
    /// </summary>
    private static void DrawSpaceBody(string body, string borderBarString, StringBuilder sb) {
        body = body.PadLeft(2);
        body = body.PadRight(3);
        sb.Append($"{borderBarString}{body}");
    }

    public static int GetCursorX(this StringBuilder sb) {
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
