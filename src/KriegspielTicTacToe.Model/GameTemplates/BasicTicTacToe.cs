using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.MNKGame;

namespace KriegspielTicTacToe.Model;

public static partial class GameTemplates {
    public static GameTemplate TicTacToe {get;} = new MNKTemplate(
        "basic-tictactoe",
        "Basic simple tic-tac-toe.",
        [2], //playercount.
        [
            new BoardBuilder(3, 3, new MNKBoardRuleset(IsBoardDoneWhenScored: true))
        ],
        
        isKriegspiel: false,
        isSynchronousMode: false
    );
}