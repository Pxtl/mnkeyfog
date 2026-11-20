using MnkeyFog.Model.Server;

namespace MnkeyFog.Bsky;

/// <summary>
/// A no-op <see cref="IGameSessionStore"/> that never persists. Used when
/// persistence is not configured.
/// </summary>
public sealed class NoOpGameSessionStore : IGameSessionStore {
    public IEnumerable<PersistedGameSession> LoadAll() => Enumerable.Empty<PersistedGameSession>();

    public void Save(PersistedGameSession session) { }

    public void Delete(string code) { }
}
