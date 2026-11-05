namespace MnkeyFog.Model.Server;

using MnkeyFog.Model;

/// <summary>
/// Persistence shape for a hosted game session, used by the bsky server.
/// Carries the game state plus the host's bookkeeping: the session's short
/// <see cref="Code"/>, the key of the public thread it was started from
/// (<see cref="ThreadKey"/>), the creator's id, start time, and the
/// id-to-player-index mapping.
/// </summary>
[ModelSerializable]
public sealed record PersistedGameSession(
    string Code,
    string ThreadKey,
    string CreatorId,
    DateTimeOffset StartedAt,
    List<PersistedPlayer> Players,
    GameState GameState
);
