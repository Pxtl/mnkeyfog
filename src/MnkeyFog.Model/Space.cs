using System.Collections.Specialized;

namespace MnkeyFog.Model;

/// <summary>
/// class to represent a space on the board.
/// </summary>
[ModelSerializable]
public sealed record Space {
    public const int ImpasseMarkIndex = -1;
    public const char ImpasseChar = '█';

    public static readonly int? EmptyMarkIndex = null;
    public static readonly string EmptyMarkString = " ";

    #region constructors
    public Space() { }
    public Space(Space space) {
        MarkIndex = space.MarkIndex;
        _knownToPlayerIndicesSet = space._knownToPlayerIndicesSet;
    }
    #endregion
    #region data members
    /// <summary>
    /// The current state of the space - null means available.
    /// '█' means it's an impasse (two players contested this space in same round).
    /// </summary>
    public int? MarkIndex { get; set; }

    private BitVector32 _knownToPlayerIndicesSet;
    [JsonConverter(typeof(BitVector32Converter))]
    public BitVector32 KnownToPlayerIndicesSet {
        get => _knownToPlayerIndicesSet;
        init {
            _knownToPlayerIndicesSet = value;
        }
    }
    #endregion

    /// <summary>
    /// Test if this space is known to the given player.
    /// </summary>
    public bool IsKnownToPlayerIndex(int? playerIndex)
        => (playerIndex == null) || KnownToPlayerIndicesSet[BitVector32.CreateMask(playerIndex.Value)];

    /// <summary>
    /// Mark this space as known to the given player.
    /// </summary>
    public void MakeKnownToPlayerIndex(int playerIndex) {
        _knownToPlayerIndicesSet[1 << playerIndex] = true;
    }

    /// <summary>
    /// Get the display value of this space for the given player.
    /// Show always if the player is null.
    /// </summary>
    public string ToString(Player? player, PlayersState playersState)
        => (player == null || IsKnownToPlayerIndex(player.Index))
            ? playersState.GetCommandName(MarkIndex)
            : EmptyMarkString;
}
