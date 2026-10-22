namespace MnkeyFog.Text.Persistence;

using MnkeyFog.Model;

/// <summary>
/// Persistence DTO for a game session: game state plus the session's code, the
/// public thread where it was started, and player metadata.
/// A list of entries is used instead of a dictionary so that the polymorphic
/// type names stay resolvable by <c>KnownTypesBinder</c>.
/// </summary>
public sealed record PersistedSession(
    string GameCode,
    string UriKey,
    string CreatorDid,
    DateTimeOffset StartedAt,
    List<PersistedPlayerDid> PlayerDids,
    GameState GameState
);
