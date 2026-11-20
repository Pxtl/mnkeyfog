
namespace MnkeyFog.Model.Views;

public record SpaceView
: GameObjectView {
    public SpaceView(int? markIndex, int? playerIndex, sbyte col, sbyte row, sbyte boardIndex)
    : base(playerIndex) {
        Col = col;
        Row = row;
        BoardIndex = boardIndex;
        MarkIndex = markIndex;
    }
    #region data properties
    public sbyte Col { get; init; }
    public sbyte Row { get; init; }
    public sbyte BoardIndex { get; init; }
    public int? MarkIndex { get; init; }
    #endregion
}
