using KriegspielTicTacToe.Model.Template;

namespace KriegspielTicTacToe.Model.MNKGame;

[ModelSerializable]
public record MNKActionFactory
: GameActionFactoryForSpace {
	public override string Name => "play space";

	public override GameAction Create(sbyte boardIndex, sbyte col, sbyte row)
    => new MNKAction(boardIndex, col, row);
}