// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging.Abstractions;
using MnkeyFog.Model.Server;

namespace MnkeyFog.Bsky.Tests;

public class GameSessionTurnTimingTests {
    private static MNKTemplate MakeTemplate() {
        var builder = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        return new MNKTemplate([builder], isSynchronousMode: false, isKriegspiel: false);
    }

    private static PlayerInfo[] TwoPlayers() => [new("X"), new("O")];

    [Fact]
    public void NewSession_HasIntervalAndTurnStartedAtNow() {
        var interval = new TurnInterval(TimeSpan.FromMinutes(30));
        var before = DateTimeOffset.UtcNow;
        var s = new GameSession("A", "at://u", "creator", TwoPlayers(), MakeTemplate(), interval);
        var after = DateTimeOffset.UtcNow;

        s.Interval.Should().Be(interval);
        s.TurnStartedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void MarkTurnStarted_ResetsTurnStartedAtToNow() {
        var s = new GameSession("A", "at://u", "creator", TwoPlayers(), MakeTemplate(), TurnInterval.Default);
        var before = DateTimeOffset.UtcNow;
        s.MarkTurnStarted();
        s.TurnStartedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void CreateGame_PassesIntervalToSession() {
        var store = new GameStore();
        var interval = new TurnInterval(TimeSpan.FromMinutes(45));
        var created = store
            .CreateGame("at://u", "did:plc:c", 2, MakeTemplate(), interval)
            .Match(c => c.Session, e => throw new Exception(e.Message));
        created.Interval.Should().Be(interval);
    }

    [Fact]
    public void GetActiveSessions_ReturnsOnlyActiveSessions() {
        var store = new GameStore();
        var active = store.CreateGame("at://u1", "did:plc:c1", 2, MakeTemplate(), TurnInterval.Default)
            .Match(c => c.Session, e => throw new Exception(e.Message));
        var completed = store.CreateGame("at://u2", "did:plc:c2", 2, MakeTemplate(), TurnInterval.Default)
            .Match(c => c.Session, e => throw new Exception(e.Message));
        store.CompleteGame(completed.Code);

        store.GetActiveSessions().Should().ContainSingle().Which.Should().Be(active);
    }

    [Fact]
    public void PersistLoad_RoundTrip_PreservesIntervalAndTurnStartedAt() {
        var capturing = new CapturingStore();
        var store = new GameStore(capturing, NullLogger.Instance);
        var interval = new TurnInterval(TimeSpan.FromMinutes(33));
        var created = store
            .CreateGame("at://u", "did:plc:c", 2, MakeTemplate(), interval)
            .Match(c => c.Session, e => throw new Exception(e.Message));
        var expectedTurnStartedAt = created.TurnStartedAt;

        var loadStore = new GameStore(new LoadOnlyStore(capturing.Saved), NullLogger.Instance);
        var loaded = loadStore.GetActiveSessions().Single();

        loaded.Code.Should().Be(created.Code);
        loaded.Interval.Should().Be(interval);
        loaded.TurnStartedAt.Should().Be(expectedTurnStartedAt);
    }

    [Fact]
    public void PersistLoad_OldFormatWithoutInterval_DefaultsToTwentyMinutesAndStartedAt() {
        var startedAt = DateTimeOffset.UtcNow;
        var old = new PersistedGameSession(
            Code: "abc",
            ThreadKey: "at://thread",
            CreatorId: "did:plc:c",
            StartedAt: startedAt,
            Players: [new PersistedPlayer("did:plc:c", 0)],
            GameState: new GameState(TwoPlayers(), MakeTemplate(), isRandomPlayerOrder: false)
        );
        // Interval and TurnStartedAt are the record defaults (null): the old on-disk format.

        var loadStore = new GameStore(new LoadOnlyStore([old]), NullLogger.Instance);
        var loaded = loadStore.GetActiveSessions().Single();

        loaded.Interval.Value.Should().Be(TimeSpan.FromMinutes(20));
        loaded.TurnStartedAt.Should().Be(startedAt);
    }
}

file sealed class CapturingStore : IGameSessionStore {
    public List<PersistedGameSession> Saved { get; } = [];
    public IEnumerable<PersistedGameSession> LoadAll() => [];
    public void Save(PersistedGameSession session) => Saved.Add(session);
    public void Delete(string code) { /* no-op for the test */ }
}

file sealed class LoadOnlyStore : IGameSessionStore {
    private readonly List<PersistedGameSession> _sessions;
    public LoadOnlyStore(IEnumerable<PersistedGameSession> sessions) => _sessions = [.. sessions];
    public IEnumerable<PersistedGameSession> LoadAll() => _sessions;
    public void Save(PersistedGameSession session) { /* no-op for the test */ }
    public void Delete(string code) { /* no-op for the test */ }
}
