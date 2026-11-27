namespace MnkeyFog.Model;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using OneOf;
using OneOf.Types;

/// <summary>
/// Base class containing shared play management logic for retirement and turn tracking.
/// </summary>
[ModelSerializable]
public class PlayersState {
    #region constructor
    [Obsolete(
        "Empty constructor puts members in invalid state needs them to be replaced by initializers."
    )]
    public PlayersState() {
        PlayerInfos = [];
        PlayManager = RoundRobinPlayManager.Instance;
    }

    public PlayersState(IReadOnlyList<PlayerInfo> playerInfos, PlayManager playManager)
    : this(playerInfos, playManager, isRandomPlayerOrder: false) { }

    public PlayersState(IReadOnlyList<PlayerInfo> playerInfos, PlayManager playManager, bool isRandomPlayerOrder) {
        if (isRandomPlayerOrder) {
            playerInfos = playerInfos.Shuffle().ToList();
        }
        PlayerInfos = playerInfos;
        PlayManager = playManager;
    }

    /// <summary>
    /// Copy-constructor.
    /// </summary>
    public PlayersState(PlayersState playersState) {
        playersState.PlayManager.ConfirmHasImmutableAttribute();

        PlayerInfos = playersState.PlayerInfos;
        PlayManager = playersState.PlayManager;
        RoundIndex = playersState.RoundIndex;
        ResignedPlayerIndicesSet = new HashSet<int>(playersState.ResignedPlayerIndicesSet);
        PlayedPlayerIndicesSet = new HashSet<int>(playersState.PlayedPlayerIndicesSet);
    }

    #endregion

    #region data members
    [MemberNotNull(nameof(_indicesByPlayer))]
    [MemberNotNull(nameof(_playerIndicesSet))]
    [JsonProperty(TypeNameHandling = TypeNameHandling.None, ItemTypeNameHandling = TypeNameHandling.None)] //non-polymorphic.
    public IReadOnlyList<PlayerInfo> PlayerInfos {
        get; init {
            // Validation: ToDictionary will throw ArgumentException on non-unique key.
            _ = value.ToDictionary(p => p.CommandName, StringComparer.OrdinalIgnoreCase);

            var indicesByPlayer = new KeyValuePair<PlayerInfo, int>[value.Count];
            for (var i = 0; i < value.Count; i += 1) {
                indicesByPlayer[i] = new KeyValuePair<PlayerInfo, int>(value[i], i);
            }

            _indicesByPlayer = indicesByPlayer.ToImmutableDictionary();
            _playerIndicesSet = Enumerable.Range(0, value.Count).ToImmutableHashSet();

            field = value;
        }
    } = new List<PlayerInfo>();
    private ImmutableDictionary<PlayerInfo, int> _indicesByPlayer;
    private ImmutableHashSet<int> _playerIndicesSet;

    /// <summary>
    /// Play-manager is the pluggable player-turn-order management system.
    /// </summary>
    public PlayManager PlayManager { get; init; }

    public int RoundIndex { get; set; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.None, ItemTypeNameHandling = TypeNameHandling.None)] //non-polymorphic.

    public HashSet<int> ResignedPlayerIndicesSet { get; init; } = new HashSet<int>();

    [JsonProperty(TypeNameHandling = TypeNameHandling.None, ItemTypeNameHandling = TypeNameHandling.None)] //non-polymorphic.

    public HashSet<int> PlayedPlayerIndicesSet { get; init; } = new HashSet<int>();
    #endregion

    #region methods
    /// <summary>
    /// Advance to the next player's turn.  Must notify if current player is
    /// resigning because that effects how the turn counter is incremented.
    /// </summary>
    /// <remarks>
    /// The logic here is tricky. If the current player resigns, then their slot
    /// is removed from the index and we skip incrementing.  But that means
    /// there are 2 "index 0" turns, so we can't use "index 0" as new round in
    /// that case.
    /// </remarks>
    public void EndTurn(GameState gameState, Player currentPlayer, out bool hasStateChanged) {
        MarkPlayerPlayed(currentPlayer);
        PlayManager.EndedTurn(gameState, out hasStateChanged);
    }

    public void MarkPlayerPlayed(Player player) {
        if (PlayedPlayerIndicesSet.Contains(player.Index)) {
            throw new InvalidOperationException($"Player {player.Info} has already played");
        }
        PlayedPlayerIndicesSet.Add(player.Index);
    }

    public void EndRound(GameState gameState, out bool hasStateChanged) {
        RoundIndex += 1;
        PlayedPlayerIndicesSet.Clear();
        PlayManager.EndedRound(gameState, out hasStateChanged);
    }

    /// <summary>
    /// Test if the given player has resigned.
    /// </summary>
    public bool IsResignedPlayer(Player player)
    => IsResignedPlayer(player.Index);

    public bool IsResignedPlayer(int playerIndex)
    => ResignedPlayerIndicesSet.Contains(playerIndex);

    /// <summary>
    /// Mark the given player as resigned. If it is the current player's turn,
    /// do *not* call NextTurn.
    /// </summary>
    public void ResignPlayer(Player player) {
        ResignPlayer(player.Index);
    }

    public void ResignPlayer(int playerIndex) {
        ResignedPlayerIndicesSet.Add(playerIndex);
    }

    /// <summary>
    /// True if the given player is able to take a turn.
    /// </summary>
    public bool CanTakeTurn(int? playerIndex)
    => playerIndex != null && PlayersAvailableForTurn.Any(p => p.Index == playerIndex);

    /// <summary>
    /// The identity (CommandName) of the player occupying the given mark index.
    /// The display Mark is a separate concern computed by the ModelNamer.
    /// </summary>
    public string GetMark(int? markIndex) {
        if (markIndex == Space.ImpasseMarkIndex) {
            return Space.ImpasseChar.ToString();
        } else if (markIndex.HasValue) {
            return PlayerInfos[markIndex.Value].CommandName;
        } else {
            return Space.EmptyMarkString;
        }
    }

    public Player GetPlayer(int playerIndex)
    => new Player(PlayerInfos[playerIndex], playerIndex);

    public Player GetPlayer(string mark)
    => GetPlayer(new PlayerInfo(mark));

    public Player GetPlayer(PlayerInfo player)
    => new Player(player, _indicesByPlayer[player]);
    #endregion

    #region helper properties
    [JsonIgnore()]
    public IEnumerable<int> PlayerIndices => Enumerable.Range(0, PlayerInfos.Count);

    [JsonIgnore()]
    public IEnumerable<Player> PlayersIndexed {
        get {
            for (var i = 0; i < PlayerInfos.Count; i += 1) {
                yield return new Player(PlayerInfos[i], i);
            }
        }
    }

    [JsonIgnore()]
    public int NumberOfActivePlayers
    => ActivePlayers.Count();

    [JsonIgnore()]
    public ImmutableHashSet<int> ActivePlayerIndices
    => _playerIndicesSet.Except(ResignedPlayerIndicesSet);

    /// <summary>
    /// Get all of the current active players.  Order is consistent.
    /// </summary>
    [JsonIgnore()]
    public IEnumerable<Player> ActivePlayers
    => PlayersIndexed.Where(p => ActivePlayerIndices.Contains(p.Index));

    [JsonIgnore()]
    public IEnumerable<Player> PlayersAvailableForTurn
    => PlayManager.PlayersAvailableForTurn(this);

    [JsonIgnore()]
    public bool IsRoundOver
    => PlayersAvailableForTurn.Count() == 0;

    /// <summary>
    /// Abstract GameStateText property - implemented by subclasses.
    /// </summary>
    [JsonIgnore()]
    public string GameStateText
    => PlayManager.GameStateText(this);
    #endregion
}
