using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MnkeyFog.Model.Template;

namespace MnkeyFog.BskyService;

/// <summary>
/// Parses bot-directed text messages into commands and game actions.
///
/// Commands are the first whitespace token of a message, prefixed with '!' in
/// public chats (where the prefix is required). In DMs the prefix is optional.
/// Remaining tokens are the command's arguments; tokens naming a valid space
/// (e.g. "5", "1A", "2B") are moves.
///
/// A "g=<code>" (or "game=" / "gamecode=") argument on any command names the
/// session code it applies to — used when a player is in more than one game.
/// </summary>
public static partial class ActionParser {
    /// <summary>Command names the bot recognizes (without the '!' prefix).</summary>
    public static readonly IReadOnlyList<string> KnownCommands = ["new", "join", "play", "board", "help", "quit"];

    [GeneratedRegex(@"^space\s*[=:]\s*", RegexOptions.IgnoreCase)]
    private static partial Regex SpaceAssignmentPrefixRegex();

    [GeneratedRegex(@"^g(?:a(m?e)?code)?\s*=\s*[A-Za-z0-9]+$", RegexOptions.IgnoreCase)]
    private static partial Regex GameCodeAssignmentPrefixRegex();

    /// <summary>
    /// Parse a command token. Returns a <see cref="CommandToken"/> with the
    /// lowercased command name (without the '!') and the whitespace-split
    /// argument tokens, or <see cref="None"/> if the message is not a command.
    /// In public chats the message must start with '!'. When
    /// <paramref name="isPrefixOptional"/> is true (DMs), the '!' prefix is
    /// optional: a message whose first token names a known command counts as a
    /// command even without the prefix.
    /// </summary>
    public static OneOf<CommandToken, None> ParseCommand(string message, bool isPrefixOptional = false) {
        var trimmed = message.Trim();
        var hasPrefix = trimmed.StartsWith('!');
        if (hasPrefix) {
            trimmed = trimmed[1..];
        } else if (!isPrefixOptional) {
            return new None();
        }

        var spaceIndex = trimmed.IndexOf(' ');
        var command = (spaceIndex < 0 ? trimmed : trimmed[..spaceIndex]).ToLowerInvariant();
        if (!hasPrefix && !KnownCommands.Contains(command)) {
            return new None();
        }

        var arg = spaceIndex < 0
            ? (IReadOnlyList<string>)[]
            : [.. trimmed[(spaceIndex + 1)..].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)];
        return new CommandToken(command, arg);
    }

    /// <summary>Whitespace-tokenize a message into argument/move tokens.</summary>
    public static IReadOnlyList<string> Tokenize(string text)
        => [.. text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)];

    /// <summary>
    /// Extract the game selector from a command's arguments, if present. A
    /// "g=<code>" (or "game=" / "gamecode=") argument names the game code for
    /// this command; it is case-insensitive and is removed from the argument
    /// list. Returns (null, tokens) if no game-code argument appears.
    /// </summary>
    public static (string? Code, IReadOnlyList<string> RemainingArgs) ExtractGameCode(IReadOnlyList<string> tokens) {
        var codeRegex = GameCodeAssignmentPrefixRegex();
        for (var i = 0; i < tokens.Count; i++) {
            if (codeRegex.IsMatch(tokens[i])) {
                var code = tokens[i].Split('=', 2)[1].ToUpperInvariant();
                var remaining = tokens.Where((_, index) => index != i).ToList();
                return (code, remaining);
            }
        }
        return (null, tokens);
    }

    /// <summary>
    /// Parse move tokens (as accepted by the <c>!play</c> command, e.g. "5",
    /// "1A 2B", "space=NN") into game actions. The player's view is used to
    /// resolve space names (board prefixes etc.); fog is not a factor — fogged
    /// spaces remain playable, per Kriegspiel rules.
    /// </summary>
    public static IReadOnlyList<GameAction> ParsePlay(GameView playerView, IReadOnlyList<string> tokens) {
        var moves = new List<GameAction>();
        GameActionFactoryForSpace? factory = playerView.AvailableActions
            .OfType<GameActionFactoryForSpace>()
            .FirstOrDefault();

        if (factory is null) {
            return moves;
        }

        foreach (var rawToken in tokens) {
            var token = SpaceAssignmentPrefixRegex().Replace(rawToken, "");
            if (TryCreateMove(factory, playerView, token, out var move)) {
                moves.Add(move);
            }
        }

        return moves;
    }

    /// <summary>
    /// Resolve a space name to a move action. Returns false if the token is not
    /// a valid space name. Whether the move is allowed is decided by the model
    /// at attempt time (e.g. Known-occupied spaces return PositionAlreadyPlayed).
    /// The model's lookup throws on invalid board-name prefixes (multi-board
    /// games), so this is guarded.
    /// </summary>
    private static bool TryCreateMove(
        GameActionFactoryForSpace factory,
        GameView playerView,
        string token,
        [NotNullWhen(true)] out GameAction? move
    ) {
        move = null;
        try {
            if (!playerView.TryGetCoordinatesFromSpaceName(token, out sbyte boardIndex, out sbyte col, out sbyte row)) {
                return false;
            }
            move = factory.Create(boardIndex, col, row);
            return true;
        } catch (ArgumentException) {
            return false;
        }
    }
}

/// <summary>
/// A parsed bot command: the lowercased command name (without the '!') and the
/// whitespace-split argument tokens following it.
/// </summary>
public sealed record CommandToken(string Command, IReadOnlyList<string> Arg);
