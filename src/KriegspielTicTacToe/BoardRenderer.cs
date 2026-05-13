namespace KriegspielTicTacToe;

using KriegspielTicTacToe.Model;
using System.Collections.Generic;

/// <summary>
/// Draws the full board based on the given gamestate, from the perspective of
/// the given player.
/// </summary>
public static class BoardRenderer {
    /// <summary>
    /// Draws the full board based on the given gamestate, from the perspective of
    /// the given player.
    /// </summary>
    public static void DrawBoards(TicTacToeState state, char? player, int? activeBoardIndex) {
        bool doShowBoardCode = state.Boards.Count > 1;
        var maxRowCount = state.Boards.Max(b => b.RowCount);

        var nextDrawnBoardIndex = 0; //handle too many boards to fit horizontally.
        while (nextDrawnBoardIndex < state.Boards.Count) {
            //top row border
            DrawBorderRow(state, nextDrawnBoardIndex, "┌", "┬", "┐", "───", doShowBoardCode);
            
            for(var row = 0; row < maxRowCount; row+=1) {
                if(row > 0) {
                    //internal border
                    DrawBorderRow(state, nextDrawnBoardIndex, "├", "┼", "┤", "───", showBoardCode: false);
                }
                DrawBoardSpacesRow(state, nextDrawnBoardIndex, player, "|", activeBoardIndex, row);
            }
            nextDrawnBoardIndex = DrawBorderRow(state, nextDrawnBoardIndex, "└", "┴", "┘", "───", showBoardCode: false);
            Console.WriteLine();
        }
        if(state.ResignedPlayersSet.Count > 0) {
            foreach(var resignedPlayer in state.ResignedPlayersSet) {
                Console.Out.WriteLine($" - player '{resignedPlayer}' is resigned.");   
            }
        }
    }

    public static int GetBoardDrawWidth(Board board) 
        => board.ColumnCount * 4 + 3;

    /// <summary>
    /// Helper function to draw a border row of the board.
    /// </summary>
    /// <returns>
    /// Returns the index of the next board to draw if it needs another row of
    /// boards.
    /// </returns>
    private static int DrawBorderRow(
        TicTacToeState state,
        int startBoardIndex,
        string startBarString, 
        string midBarString, 
        string endBarString, 
        string spanString, 
        bool showBoardCode)
    {
        for (var boardIndex = startBoardIndex; boardIndex < state.Boards.Count; boardIndex+=1) {
            var board = state.Boards[boardIndex];
            (var cursorLeft, var cusorTop) = Console.GetCursorPosition ();
            if(cursorLeft > 0 && (cursorLeft + GetBoardDrawWidth(board) > Console.WindowWidth)) {
                Console.Out.WriteLine();
                return boardIndex;
            }

            Console.Out.Write(showBoardCode
                ? (board.IsDone
                    ? " ✓" //board is done so just show a checkmark.
                    : $" {boardIndex + 1}" //key-index to choose it
                ): "  " //blank space
            );

            Console.Out.Write($"{startBarString}{spanString}");
            
            for(var col = 0; col < board.ColumnCount-1; col+=1) {
                Console.Out.Write($"{midBarString}{spanString}");
            }
            Console.Out.Write(endBarString);
        }
        Console.Out.WriteLine();
        return state.Boards.Count;
    }

    
    /// <summary>
    /// Draw a row of board spaces.
    /// </summary>
    /// <returns>
    /// Returns the index of the next board to draw if it needs another row of
    /// boards.
    /// </returns>
    private static int DrawBoardSpacesRow(TicTacToeState state, int startBoardIndex, char? player, string borderBarString, int? activeBoardIndex, int rowIndex) {
        for (int boardIndex = startBoardIndex; boardIndex < state.Boards.Count; boardIndex+=1) {
            var board = state.Boards[boardIndex];
            (var cursorLeft, var cusorTop) = Console.GetCursorPosition ();
            if(cursorLeft > 0 && (cursorLeft + GetBoardDrawWidth(board) > Console.WindowWidth)) {
                Console.Out.WriteLine();
                return boardIndex;
            }

            Console.Out.Write("  ");

            for(var col = 0; col < board.ColumnCount; col+=1) {
                DrawSpaceBody(GetSpaceString(state, player, boardIndex, activeBoardIndex, col, rowIndex), borderBarString);
            }
            Console.Out.Write(borderBarString);
        }
        Console.Out.WriteLine();
        return state.Boards.Count();
    }

    /// <summary>
    /// Helper function to draw the body-spaces of the board.
    /// </summary>
    private static void DrawSpaceBody(string body, string borderBarString)
    {
        body = body.PadLeft(2);
        body = body.PadRight(3);
        Console.Out.Write($"{borderBarString}{body}");
    }

    /// <summary>
    /// For the given space, for the given player, get the textual
    /// representation of the space.  If the game is over, all players see
    /// everything, all marks on the board.  If not, they will only see the ones
    /// they have created or discovered.  If the player is the
    /// current-turn-player, then the space index codes will be displayed.
    /// </summary>
    private static string GetSpaceString(TicTacToeState state, char? player, int boardIndex, int? activeBoardIndex, int col, int row) {
        player = state.IsGameOver //show for all players if the game is over.
            ? (char?)null
            : player;

        var board = state.Boards[boardIndex];
        var foundSpace = board.Spaces[col,row].ToString(player);
        
        if (
            string.IsNullOrWhiteSpace(foundSpace) 
            && !board.IsDone 
            && player == state.CurrentTurnPlayer
            && activeBoardIndex == boardIndex
        ) {
            return board.GetSpaceIndexCode(col, row)
                .ToString(new string('0', board.SpaceIndexCodeLength)); //zero-pad
        } else {
            return foundSpace;
        }
    }
}
