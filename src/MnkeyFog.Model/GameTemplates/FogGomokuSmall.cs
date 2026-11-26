using MnkeyFog.Model.Template;
using MnkeyFog.Model.MNKGame;

namespace MnkeyFog.Model;

public static partial class GameTemplates {
    public static GameTemplate FogGomokuSmall { get; } = new CasualMNKTemplate(
        "fog-gomoku-small",
        "Freestyle gomoku but with synchronous & kriegspiel play. Any line of 5 on a 11x11 board wins.",
        [2, 3, 4, 5, 6], //playercount.
        [
            new BoardBuilder(11, 11, new MNKBoardRuleset(ScoringLength: 5, IsBoardDoneWhenScored: true))
        ],
        isKriegspiel: true,
        isSynchronousMode: true
    );
}
