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
        return new MNKTemplate([b], isSynchronousMode: false, isKriegspiel: false);
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
        (await svc.TestRenderPublicBoardAsync(s)).IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task RenderPublic_Plain_ReturnsBoardView() {
        var svc = new TestableGameService();
        var s = new GameSession("D", "at://u", "creator", TwoPlayers(), PlainTemplate());
        s.Join("d2");
        (await svc.TestRenderPublicBoardAsync(s)).IsT0.Should().BeTrue();
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
}
