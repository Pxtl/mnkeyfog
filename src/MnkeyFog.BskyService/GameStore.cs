using System.Diagnostics.CodeAnalysis;
using MnkeyFog.Model.Template;
using MnkeyFog.Model.Server;
using Newtonsoft.Json;

namespace MnkeyFog.BskyService;

/// <summary>
/// Thread-safe in-memory store of game sessions keyed by short session code.
///
/// Each game is created in a public mention thread (its URI key is the public
/// anchor); players then locate it from DMs by their own DID (when they are in
/// exactly one active game) or by the explicit code ("g=<code>" argument) when
/// they are in several.
///
/// Codes are assigned by a monotonic counter: the counter's base32 value while
/// it is two characters or fewer, otherwise a recycled code from an
/// already-completed session, otherwise a longer random code.
///
/// When a persistence directory is configured, every change is mirrored to a
/// JSON file (one per active game) and reloaded on construction for crash
/// recovery. Completed-but-not-disposed sessions are kept in memory (so their
/// codes can be recycled) but are not persisted.
/// </summary>
public sealed class GameStore {
    /// <summary>CompletedAt is null while the session is active; set once it has finished.</summary>
    private sealed record ActiveSession(string Code, GameSession Session, DateTimeOffset? CompletedAt);

    /// <summary>All sessions (active and completed-but-not-disposed), keyed by code. Codes are unique in this map.</summary>
    private readonly ConcurrentDictionary<string, ActiveSession> _sessions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>DID to the set of codes of games that DID plays in.</summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _byDid = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Public thread URI key to code (set when the game is created in that thread).</summary>
    private readonly ConcurrentDictionary<string, string> _byUri = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Monotonic counter: number of games ever started (natural codes are its base32). Never decreases.</summary>
    private long _counter = 0;

    private readonly string? _persistDirectory;

    private readonly object _allocationLock = new();

    /// <summary>Default marks used for players when starting a game.</summary>
    public static readonly string[] DefaultMarks = ["X", "O", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N"];

    private static readonly JsonSerializerSettings _persistenceSettings = new() {
        TypeNameHandling = TypeNameHandling.Objects,
        SerializationBinder = KnownTypesBinder.Instance,
    };

    /// <summary>A successfully created game: its code and the session.</summary>
    public sealed record CreatedGame(string Code, GameSession Session);

    #region constructors
    /// <summary>
    /// Create a store. When <paramref name="persistDirectory"/> is provided, previously
    /// persisted games are loaded from it and changes are saved back to it.
    /// </summary>
    public GameStore(string? persistDirectory = null) {
        _persistDirectory = persistDirectory;
        if (persistDirectory is not null) {
            LoadPersisted(persistDirectory);
        }
    }
    #endregion

    #region commands
    /// <summary>
    /// Create a new game anchored to the public thread <paramref name="uriKey"/>.
    /// Returns a <see cref="CommandError"/> if a session is already anchored to that
    /// thread or the template does not allow the requested player count.
    /// </summary>
    public OneOf<CreatedGame, CommandError> CreateGame(string uriKey, string creatorDid, int playerCount, IGameTemplate template) {
        if (!template.LegalPlayerCounts.Contains(playerCount)) {
            return new CommandError(
                $"Game '{template.CommandName}' supports player-counts: {string.Join(", ", template.LegalPlayerCounts)}."
            );
        }

        if (_byUri.ContainsKey(uriKey)) {
            return new CommandError("A game is already running in this thread. Use !quit to end it first.");
        }

        var players = DefaultMarks
            .Take(playerCount)
            .Select(mark => new PlayerInfo(mark))
            .ToArray();

        var code = AllocateCode();

        GameSession session;
        try {
            session = new GameSession(code, uriKey, creatorDid, players, template);
        } catch (ApplicationException ex) {
            return new CommandError(ex.Message);
        }

        if (!_sessions.TryAdd(code, new ActiveSession(code, session, null))) {
            // Lost a race for the code (cannot happen via AllocateCode, but be safe).
            return new CommandError($"A game with code {code} already exists. Try again.");
        }

        IndexDid(creatorDid, code);
        _byUri[uriKey] = code;
        PersistCounter();
        PersistSession(session);
        return new CreatedGame(code, session);
    }

    /// <summary>Try to retrieve a session by code (case-insensitive).</summary>
    public bool TryGetByCode(string code, [NotNullWhen(true)] out GameSession? session) {
        if (_sessions.TryGetValue(code, out var active)) {
            session = active.Session;
            return active.Session is not null;
        }
        session = null;
        return false;
    }

    /// <summary>Try to retrieve a session by the public thread URI it was created in (case-insensitive).</summary>
    public bool TryGetByUri(string uriKey, [NotNullWhen(true)] out GameSession? session) {
        if (_byUri.TryGetValue(uriKey, out var code)
            && _sessions.TryGetValue(code, out var active)) {
            session = active.Session;
            return session is not null;
        }
        session = null;
        return false;
    }

    /// <summary>Session of <paramref name="did"/> if it plays in exactly one, null otherwise (0 or >1).</summary>
    public GameSession? GetSingleSessionForDid(string did) {
        if (!_byDid.TryGetValue(did, out var codes) || codes.Count != 1) {
            return null;
        }

        var code = codes.Keys.Single();
        return _sessions.TryGetValue(code, out var active) ? active.Session : null;
    }

    /// <summary>All sessions (active or completed-but-not-disposed) that <paramref name="did"/> plays in, oldest first.</summary>
    public IReadOnlyList<GameSession> GetSessionsForDid(string did) {
        if (!_byDid.TryGetValue(did, out var codes)) {
            return [];
        }

        return _sessions.Values
            .Where(s => codes.ContainsKey(s.Code))
            .OrderBy(s => s.Session.StartedAt)
            .Select(s => s.Session)
            .ToList();
    }

    /// <summary>Index <paramref name="did"/> as playing in the session with the given code.</summary>
    public void JoinWithIndex(string code, string did) {
        IndexDid(did, code);
    }

    /// <summary>
    /// Mark the session with the given code as naturally completed. It stays in
    /// memory (its code is now recyclable) but is removed from disk. Use
    /// <see cref="DisposeGame"/> to remove it from memory entirely.
    /// </summary>
    public void CompleteGame(string code) {
        if (!_sessions.TryGetValue(code, out var active) || active.CompletedAt is not null) {
            return;
        }

        _sessions[code] = active with { CompletedAt = DateTimeOffset.UtcNow };
        DeletePersisted(code);
    }

    /// <summary>Dispose a session from the store entirely (player !quit, or after it completed).</summary>
    public bool DisposeGame(string code) {
        if (!_sessions.TryRemove(code, out var active)) {
            return false;
        }

        foreach (var did in active.Session.PlayerDids.Keys) {
            _byDid.TryGetValue(did, out var set);
            set?.TryRemove(active.Code, out _);
        }

        // Unanchor from the public thread if it still points here.
        if (_byUri.TryGetValue(active.Session.UriKey, out var stored) && stored.Equals(code, StringComparison.OrdinalIgnoreCase)) {
            _byUri.TryRemove(active.Session.UriKey, out _);
        }

        DeletePersisted(code);
        return true;
    }

    /// <summary>Persist the current state of an active session. No-op if completed or unknown.</summary>
    public void Save(string code) {
        if (!_sessions.TryGetValue(code, out var active) || active.CompletedAt is not null) {
            return;
        }

        PersistSession(active.Session);
    }
    #endregion

    #region code allocation
    /// <summary>
    /// Allocate a code for a new game: the counter's natural base32 code while it
    /// is two characters long and still free; otherwise the code of the
    /// oldest-started completed short-code session, provided that code has not
    /// already been re-issued to a different session; otherwise a longer random
    /// code.
    /// </summary>
    private string AllocateCode() {
        lock (_allocationLock) {
            var next = Interlocked.Increment(ref _counter);

            // Rule 1: natural short code, if the slot is free. The slot (base32 of
            // the counter) is not free when a current session — active or
            // completed-but-not-disposed — still holds that code.
            if (next <= SessionCodes.MaxShortCodeValue) {
                var natural = SessionCodes.ToBase32((int)next);
                if (!_sessions.ContainsKey(natural)) {
                    return natural;
                }
            }

            // Rule 2: recycle the oldest completed short-code session's code, but
            // only if that code still belongs to that completed session (after a
            // recycle the old code's session could have already been re-issued).
            var oldest = _sessions.Values
                .Where(s => s.CompletedAt is not null)
                .Where(s => s.Session.Code.Length <= 2)
                .Where(s => ReferenceEquals(_sessions[s.Code], s))
                .OrderBy(s => s.Session.StartedAt)
                .FirstOrDefault();
            if (oldest is not null) {
                _sessions.TryRemove(oldest.Code, out _); // re-issue the code
                return oldest.Code;
            }

            // Rule 3: fall back to the counter's natural code (three or more
            // characters past the short range). Unique because the counter only
            // ever increases.
            return SessionCodes.ToBase32((int)next);
        }
    }
    #endregion

    #region persistence
    // On-disk layout (when a persist directory is configured):
    //   <dir>/counter        — the last allocated counter value (plain text).
    //   <dir>/<code>.json    — one JSON file per active game, named by its code.
    // Active games are written on every save and their files deleted when the
    // game is completed or disposed; all files are reloaded on construction.
    private void LoadPersisted(string directory) {
        if (!Directory.Exists(directory)) {
            return;
        }

        // Counter file: the last allocated value, plain text.
        var counterPath = Path.Combine(directory, "counter");
        if (File.Exists(counterPath)) {
            long.TryParse(File.ReadAllText(counterPath).Trim(), out _counter);
        }

        foreach (var file in Directory.GetFiles(directory, "*.json")) {
            PersistedGameSession? persisted;
            try {
                persisted = JsonConvert.DeserializeObject<PersistedGameSession>(File.ReadAllText(file), _persistenceSettings);
            } catch (Exception ex) when (ex is Newtonsoft.Json.JsonException or IOException) {
                continue; // corrupted file: skip it rather than fail startup
            }
            if (persisted is null) {
                continue;
            }

            try {
                var session = new GameSession(
                    persisted.Code,
                    persisted.ThreadKey,
                    persisted.CreatorId,
                    persisted.StartedAt,
                    persisted.Players.ToDictionary(p => p.Id, p => p.PlayerIndex, StringComparer.OrdinalIgnoreCase),
                    persisted.GameState
                );
                if (!_sessions.TryAdd(session.Code, new ActiveSession(session.Code, session, null))) {
                    continue; // code already loaded: keep the first
                }
                foreach (var playerDid in session.PlayerDids.Keys) {
                    IndexDid(playerDid, session.Code);
                }
                _byUri[persisted.ThreadKey] = session.Code;
            } catch (Exception) {
                continue; // undecodable session: skip; the file stays for manual inspection
            }

            // A persisted code may outrun the stored counter (e.g. the counter
            // file failed to write last run) - never let the counter fall below a
            // decodable code that exists on disk.
            if (SessionCodes.FromBase32(persisted.Code) is { } codeValue && codeValue > _counter) {
                _counter = codeValue;
            }
        }
    }

    private void PersistSession(GameSession session) {
        if (_persistDirectory is null) {
            return;
        }

        try {
            Directory.CreateDirectory(_persistDirectory);
            var persisted = new PersistedGameSession(
                session.Code,
                session.UriKey,
                session.CreatorDid,
                session.StartedAt,
                [.. session.PlayerDids.Select(kvp => new PersistedPlayer(kvp.Key, kvp.Value))],
                session.GameState
            );
            var json = JsonConvert.SerializeObject(persisted, _persistenceSettings);
            File.WriteAllText(GetPersistPath(session.Code), json);
        } catch (IOException) {
            // best-effort persistence: a failed save must not break gameplay
        }
    }

    private void DeletePersisted(string code) {
        if (_persistDirectory is null) {
            return;
        }

        try {
            File.Delete(GetPersistPath(code));
        } catch (IOException) {
            // best-effort persistence
        }
    }

    private void PersistCounter() {
        if (_persistDirectory is null) {
            return;
        }

        try {
            Directory.CreateDirectory(_persistDirectory);
            File.WriteAllText(Path.Combine(_persistDirectory, "counter"), _counter.ToString());
        } catch (IOException) {
            // best-effort persistence
        }
    }

    private string GetPersistPath(string code) {
        // Session codes are base32 alphanumeric and already filesystem-safe.
        return Path.Combine(_persistDirectory!, $"{code}.json");
    }
    #endregion

    private void IndexDid(string did, string code) {
        _byDid
            .GetOrAdd(did, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase))
            [code] = 0;
    }
}
