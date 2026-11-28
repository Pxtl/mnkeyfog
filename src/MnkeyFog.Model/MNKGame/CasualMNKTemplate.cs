using System.ComponentModel;
using MnkeyFog.Model.Template;

namespace MnkeyFog.Model.MNKGame;

/// <summary>
/// A casual MNK game template: the standard <see cref="MNKActionFactory">space
/// move</see> plus a <see cref="PassActionFactory">pass move</see>.
/// </summary>
[ModelSerializable]
[ImmutableObject(true)]
public record CasualMNKTemplate
: MNKTemplate {
    public CasualMNKTemplate(
        string commandName,
        string description,
        IEnumerable<int> legalPlayerCounts,
        IReadOnlyList<BoardBuilder> boardBuilders,
        bool isKriegspiel,
        bool isSynchronousMode
    ) : base(commandName, description, legalPlayerCounts, boardBuilders, isKriegspiel, isSynchronousMode) {
    }

    /// <summary>
    /// A casual MNK game's timeout action is a pass: a player who exceeds the
    /// turn interval passes their turn.
    /// </summary>
    public override string? TimeoutActionCommandName => "pass";

    public override IEnumerable<GameActionFactory> GetAvailableActions(GameState gameState, int playerIndex)
    => gameState.PlayersState.CanTakeTurn(playerIndex)
        ? [new MNKActionFactory(), new PassActionFactory()]
        : [];
}
