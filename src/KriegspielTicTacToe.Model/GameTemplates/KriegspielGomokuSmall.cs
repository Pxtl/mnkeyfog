using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.MNKGame;

namespace KriegspielTicTacToe.Model;
public static partial class GameTemplates {
    public static GameTemplate KriegspielGomokuSmall {get;} = new MNKTemplate(
        "kriegspiel-gomoku-small",
        "Freestyle gomoku but with synchronous & kriegspiel play. Any line of 5 on a 9x9 board wins.",
        [2,3,4,5,6], //playercount.
        [
            new BoardBuilder(9, 9, new MNKBoardRuleset(ScoringLength: 5, IsBoardDoneWhenScored: true))
        ],
        isKriegspiel: true,
        isSynchronousMode: true
    );
}