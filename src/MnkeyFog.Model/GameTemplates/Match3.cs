using MnkeyFog.Model.Template;
using MnkeyFog.Model.MNKGame;

namespace MnkeyFog.Model;

public static partial class GameTemplates {
    public static GameTemplate Match3 { get; } = new CasualMNKTemplate(
        "match3",
        "Mnkeyfog Match-3: A simple synchronous match-3 game on an 8x8 board. Score as many length-3 segments as you can.",
        [2, 3, 4, 5, 6], //playercount.
        [
            new BoardBuilder(8, 8, new MNKBoardRuleset(ScoringLength:3))
        ],

        isKriegspiel: false,
        isSynchronousMode: true
    );
}
