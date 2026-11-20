
namespace MnkeyFog.Model.Views;

public record SpaceView
: GameObjectView {
    public SpaceView(Space space, int? playerIndex, sbyte col, sbyte row, sbyte boardIndex)
    : base(playerIndex) {
        Col = col;
        Row = row;
        BoardIndex = boardIndex;
        MarkIndex = space.IsKnownToPlayerIndex(playerIndex)
            ? space.MarkIndex
            : null;
    }
    #region data properties
    public sbyte Col { get; init; }
    public sbyte Row { get; init; }
    public sbyte BoardIndex { get; init; }
    public int? MarkIndex { get; init; }
    #endregion
}
