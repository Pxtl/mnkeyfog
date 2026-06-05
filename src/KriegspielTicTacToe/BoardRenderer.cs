namespace KriegspielTicTacToe;

using KriegspielTicTacToe.Model;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Draws the full board based on the given gamestate, from the perspective of
/// the given player.
/// </summary>
public static class BoardRenderer {
    public static string DrawBoards(
        TicTacToeState state,
        Player player,
        int? activeBoardIndex,
        int maxWidth = int.MaxValue
    ) {
        bool doShowBoardCode = state.Boards.Count > 1;
        var maxRowCount = state.Boards.Max(b => b.RowCount);
        var sb = new StringBuilder();

        var nextDrawnBoardIndex = 0;
        var boardWidth = GetBoardWidth(state.Boards[nextDrawnBoardIndex]);
       
        while (nextDrawnBoardIndex < state.Boards.Count) {
            DrawBorderRow(state, nextDrawnBoardIndex, "┌", "┬", "┐", "───", doShowBoardCode, maxWidth, sb);
            
            for(var row = 0; row < maxRowCount; row += 1) {
                if(row > 0) {
                    DrawBorderRow(state, nextDrawnBoardIndex, "├", "┼", "┤", "───", false, maxWidth, sb);
                }
                DrawBoardSpacesRow(state, nextDrawnBoardIndex, player, "│", activeBoardIndex, row, boardWidth, maxWidth, sb);
            }
            nextDrawnBoardIndex = DrawBorderRow(state, nextDrawnBoardIndex, "└", "┴", "┘", "───", false, maxWidth, sb);
        }

        sb.AppendLine();
        return sb.ToString();
    }

    public static int GetBoardWidth(Board board)
        => board.ColumnCount * 4 + 3;

    /// <summary>
    /// Helper function to draw a border row of the board.
    /// Wraps to newline when maxWidth is exceeded.
    /// </summary>
    private static int DrawBorderRow(
        TicTacToeState state,
        int startBoardIndex,
        string startBarString, 
        string midBarString, 
        string endBarString, 
        string spanString, 
        bool showBoardCode,
        int maxWidth,
        StringBuilder sb
    ) {
        var boardIndex = startBoardIndex;
        for (; boardIndex < state.Boards.Count; boardIndex += 1) {
            var board = state.Boards[boardIndex];
            var cursorX = sb.GetCursorX();
            
            //wrap check - break if cursor would exceed maxWidth
            if(cursorX > 0 && (cursorX + GetBoardWidth(board) > maxWidth)) {
                break;
            }

            sb.Append(showBoardCode
                ? (board.IsDone
                    ? " ✓" //board is done so just show a checkmark.
                    : $" {boardIndex + 1}" //key-index to choose it
                ) : "  " //blank space
            );

            sb.Append($"{startBarString}{spanString}");
            
            for(var col = 0; col < board.ColumnCount - 1; col += 1) {
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
    private static int DrawBoardSpacesRow(
        TicTacToeState state, 
        int startBoardIndex,
        Player player,
        string borderBarString,
        int? activeBoardIndex,
        int rowIndex,
        int boardWidth,
        int maxWidth,
        StringBuilder sb
    ) {
        for (int boardIndex = startBoardIndex; boardIndex < state.Boards.Count; boardIndex += 1)
        {
            var board = state.Boards[boardIndex];
            var cursorX = sb.GetCursorX();
            
            //wrap check - break if cursor would exceed maxWidth
            if(cursorX > 0 && (cursorX + boardWidth > maxWidth)) {
                break;
            }

            sb.Append("  ");

            for(var col = 0; col < board.ColumnCount; col += 1) {
                var body = ModelToCommandNameUtility.GetSpaceCommandName(state, player, boardIndex, activeBoardIndex, col, rowIndex);
                DrawSpaceBody(body, borderBarString, sb);
            }
            sb.Append(borderBarString);
        }
        sb.AppendLine();
        return state.Boards.Count;
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
