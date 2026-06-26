using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.MNKGame;

namespace KriegspielTicTacToe.Model;
public static partial class GameTemplates {
    public static GameTemplate SynchroWeinersmith {get;} = new MNKTemplate(
        "synchro-weinersmith",
        "Kriegspiel Tic-Tac-Toe, but with more players and synchronous play.",
        [2,3,4], //legal player-counts
        [
            new(3, 3, new MNKBoardRuleset()),
            new(3, 3, new MNKBoardRuleset()),
            new(3, 3, new MNKBoardRuleset())
        ],
        isKriegspiel: true,
        isSynchronousMode: true
    );
}