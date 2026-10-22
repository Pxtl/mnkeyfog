// SPDX-License-Identifier: MIT

using MnkeyFog.Model;
using MnkeyFog.Model.MNKGame;
using MnkeyFog.Model.Template;
using MnkeyFog.Model.Views;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;

namespace MnkeyFog.BskyService.Tests;

/// <summary>
/// Subclass of <see cref="BskyGameService"/> used by tests to access private
/// render methods and to intercept DM sends.
/// </summary>
internal sealed class TestableGameService : BskyGameService {
    public TestableGameService()
        : base(NullLogger.Instance) { }

    /// <summary>DMs captured via <see cref="SendDmToPlayerAsync"/> override.</summary>
    public List<(string Did, string Text)> DmLog { get; } = new();

    public override Task SendDmToPlayerAsync(string playerDid, string text, CancellationToken cancellationToken = default) {
        DmLog.Add((playerDid, text));
        return Task.CompletedTask;
    }

    /// <summary>Invokes the private <c>RenderPublicBoardAsync</c> via reflection.</summary>
    public Task<OneOf<BoardView, BoardHidden>> TestRenderPublicBoardAsync(GameSession session) {
        var mi = typeof(BskyGameService).GetMethod(
            "RenderPublicBoardAsync",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (Task<OneOf<BoardView, BoardHidden>>)mi.Invoke(this, [session, default])!;
    }
}
