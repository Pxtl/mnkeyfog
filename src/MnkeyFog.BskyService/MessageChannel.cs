namespace MnkeyFog.BskyService;

/// <summary>
/// How the game service talks back: DM reply vs public post, and which board
/// view to render. <paramref name="IsPublic"/> is true for public mention
/// threads, where the bot must be careful not to leak secret-game information.
/// </summary>
internal sealed record MessageChannel(
    Func<string, Task> RespondAsync,
    Func<GameSession, CancellationToken, Task<OneOf<BoardView, BoardHidden>>> RenderBoardForAsync,
    bool IsPublic
);
