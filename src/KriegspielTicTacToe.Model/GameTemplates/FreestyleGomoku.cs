using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.MNKGame;

namespace KriegspielTicTacToe.Model;
public static partial class GameTemplates {
    public static GameTemplate FreestyleGomoku {get;} = new MNKTemplate(
        "freestyle-gomoku",
        "Standard freestyle gomoku. Any line of 5 on a 15x15 board.",
        [2], //playercount.
        [
            new BoardBuilder(15, 15, new MNKRuleset(ScoringLength: 5, IsBoardDoneWhenScored: true))
        ],
        isKriegspiel: false,
        isSynchronousMode: false
    );
}