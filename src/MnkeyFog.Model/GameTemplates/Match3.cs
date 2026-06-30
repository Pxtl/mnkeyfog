using MnkeyFog.Model.Template;
using MnkeyFog.Model.MNKGame;

namespace MnkeyFog.Model;
public static partial class GameTemplates {
    public static GameTemplate Match3 {get;} = new MNKTemplate(
        "match3",
        "Simple synchronous match-3 game on an 8x8 board.",
        [2,3,4,5,6], //playercount.
        [
            new BoardBuilder(8, 8, new MNKBoardRuleset(ScoringLength:3))
        ],
        
        isKriegspiel: false,
        isSynchronousMode: true
    );
}