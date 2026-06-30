using MnkeyFog.Model.Template;
using MnkeyFog.Model.MNKGame;

namespace MnkeyFog.Model;
[ModelSerializable]
public static partial class GameTemplates {
    public static GameTemplate FreestyleGomoku {get;} = new MNKTemplate(
        "freestyle-gomoku",
        "Standard freestyle gomoku. Any line of 5 on a 15x15 board.",
        [2], //playercount.
        [
            new BoardBuilder(15, 15, new MNKBoardRuleset(ScoringLength: 5, IsBoardDoneWhenScored: true))
        ],
        isKriegspiel: false,
        isSynchronousMode: false
    );
}