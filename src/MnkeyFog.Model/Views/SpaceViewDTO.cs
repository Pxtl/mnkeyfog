namespace MnkeyFog.Model.Views;

/// <summary>
/// The bare per-cell payload for a board space: just the mark, as seen by this
/// view's player (null when the space is empty or fogged from the player).
///
/// A full <see cref="SpaceView"/> also carries col, row, boardIndex and
/// playerIndex, but those are implied by this cell's position within the
/// owning <see cref="BoardView"/> (which holds the boardIndex, the playerIndex
/// and the 2-D layout). Storing only the mark keeps serialized per-player game
/// views small: <see cref="BoardView.SpaceDTOs"/> holds one of these per cell,
/// and <see cref="BoardView.GetSpaceView(sbyte, sbyte)"/> /
/// <see cref="BoardView.AsSpaceViewEnumerable"/> rebuild the full
/// <see cref="SpaceView"/> on demand.
/// </summary>
public readonly struct SpaceViewDTO {
    /// <summary>
    /// The mark occupying the space as seen by this view's player, or null if
    /// the space is empty or fogged from the player.
    /// </summary>
    public int? MarkIndex { get; }

    public SpaceViewDTO(int? markIndex) {
        MarkIndex = markIndex;
    }
}
