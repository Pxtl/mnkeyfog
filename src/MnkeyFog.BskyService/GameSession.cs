// SPDX-License-Identifier: MIT

using MnkeyFog.Model;
using MnkeyFog.Model.MNKGame;
using MnkeyFog.Model.Template;

namespace MnkeyFog.BskyService;

/// <summary>
/// Wraps a <see cref="GameState"/> with Bluesky metadata: the session's unique
/// code, the public thread where it was started, who created it, and a mapping
/// from player DIDs to player indices.
/// </summary>
public sealed class GameSession {
    #region data members
    /// <summary>The session's unique (short base32) code, e.g. "5F".</summary>
    public string Code { get; }

    /// <summary>
    /// The public-thread key (root post URI) where this game was started. Used
    /// to resolve "!join" / plain public mentions back to the game without
    /// requiring the player to know the code.
    /// </summary>
    public string UriKey { get; }

    public GameState GameState { get; }
    public string CreatorDid { get; }
    public DateTimeOffset StartedAt { get; }

    /// <summary>Maps Bluesky DIDs to player indices in the game.</summary>
    public IReadOnlyDictionary<string, int> PlayerDids => _playerByDid;
    private readonly ConcurrentDictionary<string, int> _playerByDid = new();
    #endregion

    public GameSession(string code, string uriKey, string creatorDid, IReadOnlyList<PlayerInfo> playerMarks, IGameTemplate template) {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(uriKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(creatorDid);

        Code = code;
        UriKey = uriKey;
        CreatorDid = creatorDid;
        StartedAt = DateTimeOffset.UtcNow;
        GameState = new GameState(playerMarks.ToArray(), template, isRandomPlayerOrder: false);

        // Creator claims the first player slot.
        _playerByDid[creatorDid] = 0;
    }

    /// <summary>Restore a session from persisted state (crash recovery).</summary>
    internal GameSession(string code, string uriKey, string creatorDid, DateTimeOffset startedAt, IReadOnlyDictionary<string, int> playerDids, GameState gameState) {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(uriKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(creatorDid);
        ArgumentNullException.ThrowIfNull(playerDids);
        ArgumentNullException.ThrowIfNull(gameState);

        Code = code;
        UriKey = uriKey;
        CreatorDid = creatorDid;
        StartedAt = startedAt;
        GameState = gameState;

        foreach (var (did, index) in playerDids) {
            _playerByDid[did] = index;
        }
    }

    #region player management
    /// <summary>
    /// Have the DID behind <paramref name="did"/> claim the next unclaimed player slot.
    /// </summary>
    public OneOf<Result<PlayerInfo>, CommandError> Join(string did) {
        ArgumentException.ThrowIfNullOrWhiteSpace(did);

        lock (_playerByDid) {
            if (_playerByDid.TryGetValue(did, out var existingIndex)) {
                return new Result<PlayerInfo>(GameState.PlayersState.PlayerInfos[existingIndex]);
            }

            var claimedIndices = new HashSet<int>(_playerByDid.Values);
            for (var i = 0; i < GameState.PlayersState.PlayerInfos.Count; i++) {
                if (!claimedIndices.Contains(i)) {
                    _playerByDid[did] = i;
                    return new Result<PlayerInfo>(GameState.PlayersState.PlayerInfos[i]);
                }
            }

            return new CommandError($"Game {Code} is full ({GameState.PlayersState.PlayerInfos.Count} players).");
        }
    }

    /// <summary>Get the player index for a Bluesky DID, or null if the DID is not playing.</summary>
    public int? GetPlayerIndex(string did) {
        return PlayerDids.TryGetValue(did, out var index) ? index : null;
    }
    #endregion

    #region game views
    /// <summary>Get the player-private (fogged) view for the given DID.</summary>
    public GameView GetViewFor(string did) {
        return GameState.GetView(GetPlayerIndex(did));
    }

    /// <summary>Get the fully-fogged spectator view (hides everything from everyone).</summary>
    public GameView GetSpectatorView() {
        return GameState.GetSpectatorView();
    }
    #endregion

    /// <summary>
    /// True if the game hides information from players (Kriegspiel fog) or
    /// plays synchronously, so moves should not be made in public threads.
    /// </summary>
    public bool IsSecret
        => GameState.GameTemplate.PlayManager is SynchronizedPlayManager
            || GameState.GameTemplate is MNKTemplate { IsKriegspiel: true };

    #region commands
    /// <summary>Attempt a move for the given player. Returns the play result.</summary>
    public IPlayActionResult Attempt(int playerIndex, GameAction action) {
        return GameState.Attempt(action.GetPlayerAction(playerIndex));
    }
    #endregion
}
