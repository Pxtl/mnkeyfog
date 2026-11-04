// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging.Abstractions;

namespace MnkeyFog.BskyService.Tests;

/// <summary>
/// Subclass of <see cref="BskyGameService"/> used by tests to set up the game
/// store and to intercept DM sends.
/// </summary>
internal sealed class TestableGameService : BskyGameService {
    public TestableGameService()
        : base(NullLogger.Instance) { }

    /// <summary>
    /// The service's <see cref="BskyBotService.GameStore"/>, exposed for test
    /// setup (creating games, joining players).
    /// </summary>
    public GameStore Store => GameStore;

    /// <summary>DMs captured via <see cref="SendDmToPlayerAsync"/> override.</summary>
    public List<(string Did, string Text)> DmLog { get; } = new();

    public override Task SendDmToPlayerAsync(string playerDid, string text, CancellationToken cancellationToken = default) {
        DmLog.Add((playerDid, text));
        return Task.CompletedTask;
    }
}
