using System.Text.RegularExpressions;
using idunno.AtProto;
using idunno.Bluesky;
using Microsoft.Extensions.Logging;

namespace MnkeyFog.BskyService;

/// <summary>
/// Hosts MnkeyFog games on Bluesky with a strict public/DM split.
///</summary>
/// <remarks>
/// A game is <i>started</i> in a public thread (an @mention of the bot with
/// <c>!new</c>); this is the only place a session is created, and the public
/// post carries the short <see cref="SessionCodes"/> code and a spectator
/// board. Players then <i>join</i> in that same public thread with <c>!join</c>.
/// All actual <i>moves</i> happen in a private DM to the bot so that fogged
/// (hidden) information is never leaked to other players.
///
/// A DM thread cannot carry a code distinct from the list of participants, so
/// the bot resolves a session two ways: (a) an explicit <c>g=<code></c> (or
/// <c>game=</c> / <c>gamecode=</c>) argument selects a specific session by its
/// short code, or (b) if the sender is in exactly one active session it is
/// used implicitly. If the sender is in several, the bot DMs the list of codes
/// and asks which one. Public <c>!play</c> attempts on a secret game are
/// refused with a warning pointing the player to DMs.
///</remarks>
public sealed class BskyGameService : BskyBotService {
    private const int MaxMentionsPerPost = 10;
    private static readonly Regex GameCodeAssignmentRegex =
        new(@"^g(?:a(m?e)?code)?\s*=\s*[A-Za-z0-9]+$", RegexOptions.IgnoreCase);

    private const string HelpText =
        "Commands:\n"
        + "  !new [game] [players]  — start a new game (public threads only)\n"
        + "  !join                  — join the game in this thread (public threads)\n"
        + "  !play <space> <space>  — play a move (DMs)\n"
        + "  !board                 — show the board\n"
        + "  !help                  — show this help\n"
        + "  !quit                  — end the game\n"
        + "Use g=<code> on any command to target a specific game (DMs): !play 5 g=5F\n"
        + "In DMs the ! prefix is optional — just send '5' to play space 5 (your only game).";

    public BskyGameService(ILogger logger) : base(logger) { }

    #region overrides
    protected internal override Task OnDirectMessageAsync(
        string conversationId,
        string senderDid,
        string message,
        CancellationToken cancellationToken
    ) {
        var channel = new MessageChannel(
            RespondAsync: text => SendDirectMessageAsync(conversationId, text, cancellationToken),
            RenderBoardForAsync: (session, ct) => RenderBoardForDmAsync(senderDid, session, ct),
            IsPublic: false
        );
        return HandleMessageAsync(senderDid, message, channel, cancellationToken, isPrefixOptional: true);
    }

    protected internal override Task OnMentionAsync(
        AtUri postUri,
        string authorDid,
        string message,
        CancellationToken cancellationToken
    ) {
        var channel = new MessageChannel(
            RespondAsync: text => PostAsync(text, cancellationToken),
            RenderBoardForAsync: (session, ct) => RenderPublicBoardAsync(session, cancellationToken),
            IsPublic: true
        );
        return HandleMessageAsync(authorDid, message, channel, cancellationToken, isPrefixOptional: false, uriKey: postUri.ToString());
    }
    #endregion

    #region rendering
    private Task<string> RenderBoardForDmAsync(string senderDid, GameSession session, CancellationToken _) {
        var view = session.GetPlayerIndex(senderDid) is int idx
            ? session.GetViewFor(senderDid)
            : session.GetSpectatorView();
        return Task.FromResult(TextBoardRenderer.Render(view));
    }

    private Task<string> RenderPublicBoardAsync(GameSession session, CancellationToken _) {
        var board = TextBoardRenderer.Render(session.GetSpectatorView());
        var dids = session.PlayerDids.Keys.Take(MaxMentionsPerPost).ToList();
        var mentions = string.Join(" ", dids.Select(did => $"@{did}"));
        return Task.FromResult(mentions.Length == 0 ? board : $"{board}\n{mentions}");
    }
    #endregion

    #region dispatch
    private Task HandleMessageAsync(
        string senderDid,
        string message,
        MessageChannel channel,
        CancellationToken cancellationToken,
        bool isPrefixOptional,
        string? uriKey = null
    ) => ActionParser.ParseCommand(message, isPrefixOptional).Match(
        command => HandleCommandAsync(senderDid, command, channel, cancellationToken, uriKey),
        _ => isPrefixOptional
            ? PlayPlainMovesAsync(senderDid, message, channel, cancellationToken)
            : Task.CompletedTask
    );

    private async Task HandleCommandAsync(
        string senderDid,
        CommandToken command,
        MessageChannel channel,
        CancellationToken cancellationToken,
        string? uriKey
    ) {
        var (explicitCode, remaining) = ExtractGameCode(command.Arg);

        switch (command.Command) {
            case "new":
                await HandleNewAsync(senderDid, remaining, channel, cancellationToken, uriKey);
                break;
            case "join":
                await HandleJoinAsync(senderDid, explicitCode, channel, cancellationToken, uriKey);
                break;
            case "play":
                await HandlePlayAsync(senderDid, remaining, explicitCode, channel, cancellationToken, uriKey);
                break;
            case "board":
                await HandleBoardAsync(senderDid, explicitCode, channel, cancellationToken, uriKey);
                break;
            case "help":
                await channel.RespondAsync(HelpText);
                break;
            case "quit":
                await HandleQuitAsync(senderDid, explicitCode, channel, cancellationToken, uriKey);
                break;
            default:
                await channel.RespondAsync($"Unknown command '!{command.Command}'. See !help.");
                break;
        }
    }
    #endregion

    #region commands
    private async Task HandleNewAsync(
        string senderDid,
        IReadOnlyList<string> commandArgs,
        MessageChannel channel,
        CancellationToken cancellationToken,
        string? uriKey
    ) {
        if (uriKey is null) {
            await channel.RespondAsync(
                "!new works only in *public* threads (reply to the post where you mentioned me). " +
                "You cannot start a game in a DM.");
            return;
        }

        var templateName = commandArgs.Count > 0 ? commandArgs[0].ToLowerInvariant() : "tictactoe";
        var template = GameTemplates.GetBuiltInGameTemplates()
            .FirstOrDefault(t => string.Equals(t.CommandName, templateName, StringComparison.OrdinalIgnoreCase));

        if (template is null) {
            var known = string.Join(", ", GameTemplates.GetBuiltInGameTemplates().Select(t => t.CommandName));
            await channel.RespondAsync($"Unknown game '{templateName}'. Known: {known}");
            return;
        }

        var playerCount = 2;
        if (commandArgs.Count > 1 && int.TryParse(commandArgs[1], out var count) && count is >= 2 and <= 4) {
            playerCount = count;
        }

        var result = GameStore.CreateGame(uriKey, senderDid, playerCount, template);
        await result.Match(
            async created => await channel.RespondAsync(
                $"Started {template.CommandName} ({playerCount}p) — code: **[{created.Code}]**\n"
                + await channel.RenderBoardForAsync(created.Session, cancellationToken)
                + "\nJoin in this thread with **!join**, then play in DMs: **!play <space>** (use g=[code] if you have several games)."),
            error => channel.RespondAsync(error.Message)
        );
    }

    private async Task HandleJoinAsync(
        string senderDid,
        string? explicitCode,
        MessageChannel channel,
        CancellationToken cancellationToken,
        string? uriKey
    ) {
        var session = await ResolveSessionAsync(senderDid, explicitCode, channel, cancellationToken, uriKey);
        if (session is null) {
            return;
        }

        var result = session.Join(senderDid);
        await result.Match(
            async player => {
                GameStore.JoinWithIndex(session.Code, senderDid);
                GameStore.Save(session.Code);
                await channel.RespondAsync(
                    $"You joined **[{session.Code}]** as **'{player.Value.Mark}'**\n"
                    + await channel.RenderBoardForAsync(session, cancellationToken)
                    + "\nPlay in DMs: **!play <space>** (bare space tokens also work if this is your only game).");
            },
            error => channel.RespondAsync(error.Message)
        );
    }

    private async Task HandlePlayAsync(
        string senderDid,
        IReadOnlyList<string> commandArgs,
        string? explicitCode,
        MessageChannel channel,
        CancellationToken cancellationToken,
        string? uriKey
    ) {
        var session = await ResolveSessionAsync(senderDid, explicitCode, channel, cancellationToken, uriKey);
        if (session is null) {
            return;
        }

        if (channel.IsPublic) {
            await channel.RespondAsync(
                $"WARNING: game **[{session.Code}]** is played in DMs to keep moves private. "
                + "Do **not** play in this public thread — post to the bot in a DM instead (include g=[code] if you have several games).");
            return;
        }

        var playerIndex = session.GetPlayerIndex(senderDid);
        if (playerIndex is null) {
            await channel.RespondAsync($"You are not in game **[{session.Code}]**. Join it in the thread where it was started with **!join**.");
            return;
        }

        await ExecuteMovesAsync(senderDid, commandArgs, session, playerIndex.Value, channel, cancellationToken);
    }

    private async Task HandleBoardAsync(
        string senderDid,
        string? explicitCode,
        MessageChannel channel,
        CancellationToken cancellationToken,
        string? uriKey
    ) {
        var session = await ResolveSessionAsync(senderDid, explicitCode, channel, cancellationToken, uriKey);
        if (session is null) {
            await channel.RespondAsync(
                "No game to show. In DMs: **!board g=<code>** (or just **!board** if you have one game). "
                + "In public threads use **!board** in the thread where the game was started.");
            return;
        }
        await channel.RespondAsync(await channel.RenderBoardForAsync(session, cancellationToken));
    }

    private async Task HandleQuitAsync(
        string senderDid,
        string? explicitCode,
        MessageChannel channel,
        CancellationToken cancellationToken,
        string? uriKey
    ) {
        var session = await ResolveSessionAsync(senderDid, explicitCode, channel, cancellationToken, uriKey);
        if (session is null) {
            await channel.RespondAsync("No game to quit. Use **!quit** in the thread where the game was started, or in a DM with **g=<code>**.");
            return;
        }
        GameStore.EndGame(session.Code);
        await channel.RespondAsync($"Game **[{session.Code}]** ended.");
    }

    private async Task PlayPlainMovesAsync(
        string senderDid,
        string message,
        MessageChannel channel,
        CancellationToken cancellationToken
    ) {
        var session = GameStore.GetSingleSessionForDid(senderDid);
        if (session is null) {
            return;
        }

        var playerIndex = session.GetPlayerIndex(senderDid);
        if (playerIndex is null) {
            return;
        }

        var playerView = session.GetViewFor(senderDid);
        var moves = ActionParser.ParsePlay(playerView, ActionParser.Tokenize(message));
        if (moves.Count == 0) {
            return;
        }

        await ExecuteMovesAsync(senderDid, ActionParser.Tokenize(message), session, playerIndex.Value, channel, cancellationToken);
    }

    private async Task ExecuteMovesAsync(
        string senderDid,
        IReadOnlyList<string> tokens,
        GameSession session,
        int playerIndex,
        MessageChannel channel,
        CancellationToken cancellationToken
    ) {
        var playerView = session.GetViewFor(senderDid);
        var moves = ActionParser.ParsePlay(playerView, tokens);
        if (moves.Count == 0) {
            await channel.RespondAsync(
                $"No valid spaces in game **[{session.Code}]**. Usage: **!play <space>** — e.g. **!play 5** or **!play 1A**.");
            return;
        }

        var resultTexts = new List<string>();
        foreach (var move in moves) {
            var result = session.Attempt(playerIndex, move);
            resultTexts.Add(result.GetResultText(session.GameState.PlayersState));
            session.GameState.ExecutePendingActions();
        }

        if (session.GameState.PlayersState.IsRoundOver) {
            session.GameState.EndRound(out _);
        }

        await channel.RespondAsync(
            $"Game **[{session.Code}]**\n"
            + string.Join("\n", resultTexts)
            + "\n"
            + await channel.RenderBoardForAsync(session, cancellationToken));

        if (session.GameState.IsGameOver) {
            GameStore.CompleteGame(session.Code);
        } else {
            GameStore.Save(session.Code);
        }
    }
    #endregion

    #region helpers
    /// <summary>
    /// Resolve the session for <paramref name="senderDid"/> — an explicit <c>g=<code></c>
    /// argument first, then the public-thread key (mentions), then the DID index
    /// (exactly one active game). When the DID index has several games the sender
    /// is told which game to re-target with <c>g=<code></c>. Returns null when
    /// the sender has no resolvable session.
    /// </summary>
    private async Task<GameSession?> ResolveSessionAsync(
        string senderDid,
        string? explicitCode,
        MessageChannel channel,
        CancellationToken cancellationToken,
        string? uriKey
    ) {
        if (explicitCode is not null) {
            if (GameStore.TryGetByCode(explicitCode, out var fromCode)) {
                return fromCode;
            }
            await channel.RespondAsync($"No game with code **{explicitCode}**. Check the code in the post where it was started.");
            return null;
        }

        if (uriKey is not null && GameStore.TryGetByUri(uriKey, out var fromUri)) {
            return fromUri;
        }

        var single = GameStore.GetSingleSessionForDid(senderDid);
        if (single is not null) {
            return single;
        }

        var all = GameStore.GetSessionsForDid(senderDid);
        if (all.Count > 1) {
            var listing = string.Join("\n", all.Select(s => $"  [{s.Code}] — {s.GameState.GameTemplate.CommandName}"));
            await channel.RespondAsync(
                $"You are in {all.Count} games. Which one?\n{listing}\n" +
                $"Re-send with the code, e.g. **!play 5 g=5F**.");
        }
        return null;
    }

    private static (string? Code, IReadOnlyList<string> RemainingArgs) ExtractGameCode(IReadOnlyList<string> tokens) {
        for (var i = 0; i < tokens.Count; i++) {
            if (GameCodeAssignmentRegex.IsMatch(tokens[i])) {
                var code = tokens[i].Split('=', 2)[1].ToUpperInvariant();
                return (code, tokens.Where((_, index) => index != i).ToList());
            }
        }
        return (null, tokens);
    }
    #endregion
}
