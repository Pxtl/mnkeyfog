using MnkeyFog.Model.Server;

namespace MnkeyFog.Bsky;

/// <summary>
/// Blob-storage abstraction for persisted game sessions (crash recovery). The
/// default implementation is filesystem-backed (<see cref="FileSystemGameSessionStore"/>);
/// an alternative backend (e.g. Azure Blob Storage) can be injected.
/// </summary>
public interface IGameSessionStore {
    /// <summary>Load all persisted sessions. Corrupt entries are skipped.</summary>
    IEnumerable<PersistedGameSession> LoadAll();

    /// <summary>Persist a session, keyed by its code. Overwrites any existing entry.</summary>
    void Save(PersistedGameSession session);

    /// <summary>Delete the persisted session with the given code. No-op if absent.</summary>
    void Delete(string code);
}
