namespace MnkeyFog.Model.Views;

public record SpaceView
: GameObjectView {
    public SpaceView(Space space, Player? player, sbyte col, sbyte row)
    : base(player) {
        Col = col;
        Row = row;
        Mark = space.IsKnownToPlayer(Player)
            ? space.Mark
            : null;
    }
    #region data properties
    public sbyte Col { get; init; }
    public sbyte Row { get; init; }
	public string? Mark { get; init; }
    #endregion
}
