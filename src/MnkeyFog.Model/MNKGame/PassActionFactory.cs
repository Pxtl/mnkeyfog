using MnkeyFog.Model.Template;

namespace MnkeyFog.Model.MNKGame;

/// <summary>
/// Factory for <see cref="PassAction"/> — the no-op move that lets a player
/// skip their turn.
/// </summary>
[ModelSerializable]
public record PassActionFactory
: GameActionFactoryForSimple {
    public override ModelName Name => new("pass", "pass");

    public override GameAction Create() => new PassAction();
}
