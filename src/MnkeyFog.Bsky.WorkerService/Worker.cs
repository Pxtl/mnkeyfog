using MnkeyFog.Bsky;

namespace MnkeyFog.Bsky.WorkerService;

/// <summary>
/// Hosts the <see cref="BskyGameService"/> bot for the lifetime of the
/// process: created (logged in and polling) on start, disposed on stop.
/// </summary>
public class Worker(ILogger<Worker> logger, BskyBotOptions options, IGameSessionStore store) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await using var bot = await BskyGameService.CreateAsync(logger, options, store, stoppingToken);
        try {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        } catch (OperationCanceledException) {
            // expected on shutdown
        }
    }
}
