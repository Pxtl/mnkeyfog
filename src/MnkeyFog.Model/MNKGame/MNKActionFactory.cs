using MnkeyFog.Model.Template;

namespace MnkeyFog.Model.MNKGame;

[ModelSerializable]
public record MNKActionFactory
: GameActionFactoryForSpace {
    public override ModelName Name => new("space", "space");

    public override GameAction Create(sbyte boardIndex, sbyte col, sbyte row)
    => new MNKAction(boardIndex, col, row);
}
