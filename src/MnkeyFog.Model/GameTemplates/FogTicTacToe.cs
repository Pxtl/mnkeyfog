using MnkeyFog.Model.Template;
using MnkeyFog.Model.MNKGame;

namespace MnkeyFog.Model;
public static partial class GameTemplates {
    public static GameTemplate FogTicTacToe {get;} = new MNKTemplate(
        "fog-tictactoe",
        "Zach Weinersmith's Kriegspiel Tic-Tac-Toe, but with more players and synchronous play.",
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