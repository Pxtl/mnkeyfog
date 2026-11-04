// SPDX-License-Identifier: MIT

using MnkeyFog.Model;
using MnkeyFog.Model.MNKGame;
using MnkeyFog.Model.Template;

namespace MnkeyFog.BskyService.Tests;

public class BskyGameServiceTests {
    private static List<PlayerInfo> TwoPlayers() => new() { new("X"), new("O") };

    private static MNKTemplate KriegspielTemplate() {
        var b = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        return new MNKTemplate([b], isSynchronousMode: false, isKriegspiel: true);
    }

    private static MNKTemplate PlainTemplate() {
        var b = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        return new MNKTemplate("mnk", "Plain 3x3", [1, 2, 3], [b], isKriegspiel: false, isSynchronousMode: false);
    }

    private static GameSession CreateGame(TestableGameService svc, string creator, IGameTemplate template, int players = 2) {
        var result = svc.Store.CreateGame($"at://did:plc:root/{Guid.NewGuid():N}", creator, players, template);
        return result.Match(
            created => created.Session,
            error => throw new Exception($"CreateGame failed: {error.Message}"));
    }

    [Fact]
    public void IsSecret_Kriegspiel_IsTrue() {
        var s = new GameSession("A", "at://u", "creator", TwoPlayers(), KriegspielTemplate());
        s.IsSecret.Should().BeTrue();
    }

    [Fact]
    public void IsSecret_Plain_IsFalse() {
        var s = new GameSession("B", "at://u", "creator", TwoPlayers(), PlainTemplate());
        s.IsSecret.Should().BeFalse();
    }

    [Fact]
    public async Task RenderPublic_Kriegspiel_ReturnsBoardHidden() {
        var svc = new TestableGameService();
        var s = new GameSession("C", "at://u", "creator", TwoPlayers(), KriegspielTemplate());
        s.Join("d2");
        (await svc.RenderPublicBoardAsync(s, TestContext.Current.CancellationToken)).IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task RenderPublic_Plain_ReturnsBoardView() {
        var svc = new TestableGameService();
        var s = new GameSession("D", "at://u", "creator", TwoPlayers(), PlainTemplate());
        s.Join("d2");
        (await svc.RenderPublicBoardAsync(s, TestContext.Current.CancellationToken)).IsT0.Should().BeTrue();
    }

    [Fact]
    public void IsViewChanged_EnqueuedTrue_IsTrue() =>
        Assert.True(new Enqueued(true, "1").IsViewChanged);

    [Fact]
    public void IsViewChanged_EnqueuedFalse_IsFalse() =>
        Assert.False(new Enqueued(false, "1").IsViewChanged);

    [Fact]
    public void IsViewChanged_NewlyLearned_IsTrue() =>
        Assert.True(new NewlyLearned(1).IsViewChanged);

    [Fact]
    public void IsViewChanged_Resigned_IsTrue() =>
        Assert.True(new Resigned(0).IsViewChanged);

    [Fact]
    public void IsViewChanged_PositionAlreadyPlayed_IsFalse() =>
        Assert.False(new PositionAlreadyPlayed(0).IsViewChanged);

    [Fact]
    public void IsViewChanged_CannotTakeTurn_IsFalse() =>
        Assert.False(new CannotTakeTurn(0).IsViewChanged);

    [Fact]
    public void IsViewChanged_Quitting_IsTrue() =>
        Assert.True(new Quitting().IsViewChanged);

    [Fact]
    public async Task SendDmToPlayer_CapturesDm() {
        var svc = new TestableGameService();
        await svc.SendDmToPlayerAsync("did:plc:p1", "hello", TestContext.Current.CancellationToken);
        svc.DmLog.Should().HaveCount(1);
        svc.DmLog[0].Did.Should().Be("did:plc:p1");
        svc.DmLog[0].Text.Should().Be("hello");
    }

    [Fact]
    public void BoardView_HoldsText() =>
        Assert.Equal("  . . .", new BoardView("  . . .").Text);

    [Fact]
    public void BoardHidden_IsUnit() =>
        Assert.NotNull(new BoardHidden());

    // ── !list tests ──

    [Fact]
    public async Task List_NoGames_DmOnlyMessage() {
        var svc = new TestableGameService();
        await svc.TestHandleMessageAsync("did:plc:p1", "!list", TestContext.Current.CancellationToken);
        svc.TestedResponses.Single().Should().Contain("not in any games");
    }

    [Fact]
    public async Task List_PublicThread_Refused() {
        var svc = new TestableGameService();
        await svc.TestHandleMentionAsync("at://u", "did:plc:p1", "!list", TestContext.Current.CancellationToken);
        svc.TestedResponses.Single().Should().Contain("DM-only");
    }

    [Fact]
    public async Task List_OneGame_ShowsCodeTemplateAndTurn() {
        var svc = new TestableGameService();
        var template = PlainTemplate();
        var session = CreateGame(svc, "did:plc:p1", template);
        session.Join("did:plc:p2");
        svc.Store.JoinWithIndex(session.Code, "did:plc:p2");

        await svc.TestHandleMessageAsync("did:plc:p2", "!list", TestContext.Current.CancellationToken);
        var response = svc.TestedResponses.Single();
        response.Should().Contain($"[{session.Code}]").And.Contain(template.CommandName).And.Contain("player O");
    }

    [Fact]
    public async Task List_YourTurn_ShowsTurnIndicator() {
        var svc = new TestableGameService();
        var template = PlainTemplate();
        var session = CreateGame(svc, "did:plc:p1", template);
        session.Join("did:plc:p2");
        svc.Store.JoinWithIndex(session.Code, "did:plc:p2");

        // p1 (player X) has already played, so p2 (player O) is next.
        var moves = ActionParser.ParsePlay(session.GetViewFor("did:plc:p1"), ["5"]);
        foreach (var move in moves) {
            session.Attempt(0, move);
            session.GameState.ExecutePendingActions();
        }
        if (session.GameState.PlayersState.IsRoundOver) {
            session.GameState.EndRound(out _);
        }

        await svc.TestHandleMessageAsync("did:plc:p2", "!list", TestContext.Current.CancellationToken);
        svc.TestedResponses.Single().Should().Contain("your turn");
    }

    [Fact]
    public async Task List_MultipleGames_ShowsAllInOrder() {
        var svc = new TestableGameService();
        var template = PlainTemplate();
        var s1 = CreateGame(svc, "did:plc:p1", template);
        var s2 = CreateGame(svc, "did:plc:p1", template);
        s1.Join("did:plc:p2");
        s2.Join("did:plc:p2");
        svc.Store.JoinWithIndex(s1.Code, "did:plc:p2");
        svc.Store.JoinWithIndex(s2.Code, "did:plc:p2");

        await svc.TestHandleMessageAsync("did:plc:p2", "!list", TestContext.Current.CancellationToken);
        var response = svc.TestedResponses.Single();
        response.Should().Contain($"[{s1.Code}]").And.Contain($"[{s2.Code}]");
    }

    [Fact]
    public async Task List_StartedJustNow_ShowsJustNow() {
        var svc = new TestableGameService();
        var session = CreateGame(svc, "did:plc:p1", PlainTemplate());
        session.Join("did:plc:p2");
        svc.Store.JoinWithIndex(session.Code, "did:plc:p2");

        await svc.TestHandleMessageAsync("did:plc:p2", "!list", TestContext.Current.CancellationToken);
        svc.TestedResponses.Single().Should().Contain("just now");
    }

    [Fact]
    public void FormatGameLine_MultipleGames_IsConsistent() {
        var template = PlainTemplate();
        var s1 = new GameSession("E1", "at://u1", "did:plc:p1", TwoPlayers(), template);
        s1.Join("did:plc:p2");
        var line = BskyGameService.FormatGameLine(s1, "did:plc:p2");
        line.Should().Contain("[E1]").And.Contain(template.CommandName).And.Contain("player O");
    }
}
