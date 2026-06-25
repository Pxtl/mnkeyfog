using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.MNKGame;

namespace KriegspielTicTacToe.Model;
public static partial class GameTemplates {
    public static GameTemplate Weinersmith {get;} = new MNKTemplate(
        "weinersmith",
        "Zach Weinersmith's Kriegspiel Tic-Tac-Toe.",
        [2], //legal player-counts
        [
            new BoardBuilder(3, 3, new MNKRuleset()),
            new BoardBuilder(3, 3, new MNKRuleset()),
            new BoardBuilder(3, 3, new MNKRuleset())
        ],
        isKriegspiel: true,
        isSynchronousMode: false
    );
}