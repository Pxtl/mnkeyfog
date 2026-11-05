# PLAN

This document contains incomplete plans for new features.  When ready, they will be moved to TODO.

## GUI
Use Uno or Avalonia for a GUI. Google recommends Avalonia for sliding animations.

## Server
Currently the game is served by a simple JSON file, which is obviously a dumb
hack. Need a proper API.

 - Need a way to extract a player-view of the gamestate, that state can be sent
   to clients and renderers. KriegspielTicTacToe.PlayerViewModel?

## Incomplete Information Tic-Tac-Toe

https://www.smbc-comics.com/comic/incomplete

https://mastodon.social/@ZachWeinersmith/111896904870106666


## Drop Mode
A mode where pieces are placed in from the top, Drop-4 style.

## Emoji
Currently player spaces are 1 char long.  Emoji are 2 char long in most
monospaced fonts. Allow the players to pick an emoji or a 2-char-long
Mark.  Custom boardwidth?
- Will have to block Marks that are a substring of other Marks.

## BSky bot (implemented — design decisions)

The BSky chat bot is now implemented in `MnkeyFog.BskyService`.  Key design
decisions are documented here so they are not lost when the implementation
evolves.

### Session-code encoding

Codes use the same convention as `CommandNameTool`: an *offset-by-1* unpadded
RFC 4648 base32 encoding.  The code for sequence value `n` is the standard
big-endian base32 digit string of `n - 1`, so the alphabet covers values
1–32 in one character and 33–1024 in two characters.  `MaxShortCodeValue`
is 1024.

### Code allocation

- A monotonic counter assigns the next sequence value; the counter is never
  decremented.
- **Rule 1**: the counter's natural base32 code is used while it is two
  characters long and the slot is free (the slot check is defensive — the
  counter can serialize worse than the sessions).
- **Rule 2**: otherwise the oldest completed short-code session's code is
  recycled, only if it still belongs to that session.
- **Rule 3**: otherwise the counter's natural code is used (three or more
  characters, past the short range).

### Code resolution

- Messages may carry an explicit `g=<code>` selector (e.g. in a link
  `?g=B7`) which is resolved via `GameStore.TryGetByCode`.
- Without an explicit code the bot falls back to `GetSingleSessionForDid`:
  if the sender plays in exactly one active game that game is used; zero or
  multiple active games produce a friendly error.
- Codes are matched case-insensitively; internal storage uses the canonical
  upper-case form.

### Commands

| Command | Action |
|---------|--------|
| `!new` | Create a new game, return the board + short code tag. |
| `!join <code>` | Join an existing game by its short code. |
| `!board` | Show the current board (with `[#<code>]` tag). |
| `!play <x> <y>` | Play a move; result includes the code tag. |
| `!quit` | End the game early (player-initiated). The code is not recycled until a new game session is created. |
| `!list` *(proposed, in TODO)* | List the sender's active games: code, template, turn state. See `TODO.md`. |

### Command parsing

Commands are currently parsed by hand (split the message, read args). Longer-term:
leverage `System.CommandLine` for `!command` parsing. That removes the need to keep
the `g=<code>` format — `-g <code>` works just as well with a real arg parser.

## Chatbot could be stateless if there's bidirectional serialization of gamestate.

- Simple emoji mode without borders?  Render a black square for empty spaces?
  Or U+3000 space?  (BSky is not monospaced, so the pipe-drawn boards won't
  align.)
- API for chatbots?  Is there a standard one?  Needs a standard API including
  @s, DMs, and likes (for voting-based games).
    - There is not.  Have to DIY if I want to support multiple platforms. The
      same abstractions could back a general (AI) chatbot.
- Chatbot could be stateless if there's bidirectional serialization of
  gamestate (e.g. carried as invisible metadata on a BSky post). **Caution:**
  a kriegspiel game's private (fogged) state must not leak into public
  metadata — only the public view may be embedded.

## Spectator mode rework

The Model's `GameState.GetSpectatorView()` (= `GetView(null)`) is a fully
***de-fogged*** view, modeled after RTS/strategy-game spectators. That is fine
for a *synchronous* game (post the revealed board at the end of every
round/turn) but is **wrong** for a kriegspiel game in a public thread: it
reveals every space to every observer. The bsky bot currently calls
`GetSpectatorView()` for its public `!board`/post, which leaks the entire
board for kriegspiel games started in public threads.

Longer-term directions (any of them):

- **Merged-fog view** — a view that shows only data known to *every* player
  simultaneously. Useful for a public kriegspiel thread where players play
  in DMs but spectators (and the anchor post) can see the shared-known
  subset. This is a Model-level feature.
- **OneOf-typed view parameter** — change the view constructor to use
  `OneOf<int, SpectatorMode>` (or similar) so `DeFogged` and `MergedFog`
  (and any future mode) are first-class, replacing the current
  `Nullable<int> playerIndex` convention where `null` *means* de-fogged.
  This is the structural fix that makes merged-fog expressible without
  special-casing.
- **Post the revealed board at the right time.** For synchronous games the
  bot should post the de-fogged spectator board at the end of every round.
  For kriegspiel games, the bot must **not** post the spectator board until
  the game is complete; instead it DMs each player their private
  fogged view after any `IsViewChanged` result.

*Short-term fix (implemented, commit `8d9af7c`):* `BskyGameService`
returns `BoardHidden` (no post) for kriegspiel games; `ExecuteMovesAsync`
DMs each player their own fogged view on `IsViewChanged` via
`BskyBotService.SendDmToPlayerAsync`.
