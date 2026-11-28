// SPDX-License-Identifier: MIT

using MnkeyFog.Model;
using MnkeyFog.Model.MNKGame;
using MnkeyFog.Model.Template;

namespace MnkeyFog.Bsky.Tests;

public class TurnTimeoutTests {
    private static CasualMNKTemplate CasualTemplate() {
        var b = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        return new CasualMNKTemplate("mnk", "Casual 3x3", [1, 2, 3], [b], isKriegspiel: false, isSynchronousMode: false);
    }

    private static MNKTemplate PlainTemplate() {
        var b = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        return new MNKTemplate("mnk", "Plain 3x3", [1, 2, 3], [b], isKriegspiel: false, isSynchronousMode: false);
    }

    private static GameSession CreateGame(TestableGameService svc, string creator, IGameTemplate template, int players = 2) {
        var result = svc.Store.CreateGame($"at://did:plc:root/{Guid.NewGuid():N}", creator, players, template, TurnInterval.Default);
        return result.Match(
            created => created.Session,
            error => throw new Exception($"CreateGame failed: {error.Message}"));
    }

    [Fact]
    public async Task Timeout_CasualTemplate_AppliesPassAndDms() {
        var svc = new TestableGameService();
        var session = CreateGame(svc, "did:plc:p1", CasualTemplate(), 2);
        session.Join("did:plc:p2");

        await svc.CheckAndApplyTurnTimeoutsAsync(now: DateTimeOffset.UtcNow.AddDays(1), cancellationToken: TestContext.Current.CancellationToken);

        // The current player (creator, index 0) timed out and was passed for.
        svc.DmLog.Should().HaveCount(1);
        svc.DmLog[0].Did.Should().Be("did:plc:p1");
        svc.DmLog[0].Text.Should().Contain("Passing");
        // After the pass, the turn advances to the joiner (index 1).
        session.GameState.PlayersState.PlayersAvailableForTurn.Single().Index.Should().Be(1);
    }

    [Fact]
    public async Task Timeout_PlainTemplate_ResignsAndDms() {
        var svc = new TestableGameService();
        var session = CreateGame(svc, "did:plc:p1", PlainTemplate(), 2);
        session.Join("did:plc:p2");

        await svc.CheckAndApplyTurnTimeoutsAsync(now: DateTimeOffset.UtcNow.AddDays(1), cancellationToken: TestContext.Current.CancellationToken);

        // The current player (creator, index 0) timed out and was resigned.
        svc.DmLog.Should().HaveCount(1);
        svc.DmLog[0].Did.Should().Be("did:plc:p1");
        svc.DmLog[0].Text.Should().Contain("Resigning");
        // Resigning one of two players ends the game.
        session.GameState.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public async Task Timeout_NotExpired_NoAction() {
        var svc = new TestableGameService();
        var session = CreateGame(svc, "did:plc:p1", CasualTemplate(), 2);
        session.Join("did:plc:p2");
        var turnStartedAt = session.TurnStartedAt;

        // 'now' is one minute into the interval, so the turn has not timed out.
        await svc.CheckAndApplyTurnTimeoutsAsync(now: turnStartedAt.AddMinutes(1), cancellationToken: TestContext.Current.CancellationToken);

        svc.DmLog.Should().BeEmpty();
        session.GameState.IsGameOver.Should().BeFalse();
        // The current player is unchanged.
        session.GameState.PlayersState.PlayersAvailableForTurn.Single().Index.Should().Be(0);
    }
}
