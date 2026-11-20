using MnkeyFog.Model.Server;
using Newtonsoft.Json;

namespace MnkeyFog.Bsky;

/// <summary>
/// Filesystem-backed <see cref="IGameSessionStore"/>: one JSON file per game in a
/// directory.
/// </summary>
/// <remarks>
/// On-disk layout: <c>&lt;dir&gt;/&lt;code&gt;.json</c> — one JSON file per active
/// game, named by its code. Active games are written on every save and their files
/// deleted when the game is completed or disposed; all files are reloaded on
/// construction.
/// </remarks>
public sealed class FileSystemGameSessionStore : IGameSessionStore {
    private readonly string _directory;
    private static readonly JsonSerializerSettings _persistenceSettings = new() {
        TypeNameHandling = TypeNameHandling.Objects,
        SerializationBinder = KnownTypesBinder.Instance,
    };

    public FileSystemGameSessionStore(string directory) {
        _directory = directory;
    }

    public IEnumerable<PersistedGameSession> LoadAll() {
        var result = new List<PersistedGameSession>();
        if (!Directory.Exists(_directory)) {
            return result;
        }

        foreach (var file in Directory.GetFiles(_directory, "*.json")) {
            PersistedGameSession? persisted;
            try {
                persisted = JsonConvert.DeserializeObject<PersistedGameSession>(File.ReadAllText(file), _persistenceSettings);
            } catch (Exception ex) when (ex is Newtonsoft.Json.JsonException or IOException) {
                continue; // corrupted file: skip it rather than fail startup
            }
            if (persisted is not null) {
                result.Add(persisted);
            }
        }
        return result;
    }

    public void Save(PersistedGameSession session) {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(GetPersistPath(session.Code), JsonConvert.SerializeObject(session, _persistenceSettings));
    }

    public void Delete(string code) {
        File.Delete(GetPersistPath(code));
    }

    private string GetPersistPath(string code) {
        // Session codes are base32 alphanumeric and already filesystem-safe.
        return Path.Combine(_directory, $"{code}.json");
    }
}
