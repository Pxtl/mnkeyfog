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
- If the corresponding base32 code is already held by an *active* session the
  allocation is rejected and a 6-character cryptographically random code is
  issued instead (the long-code fallback).
- When a session is completed its code returns to the free pool and the
  counter's natural progression re-uses it on the next matching step.

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
| `!code` *(proposed, not yet implemented)* | Return only the code(s) for the sender's active game(s), without revealing board state. |

## Chatbot could be stateless if there's bidirectional serialization of gamestate.

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

*Short-term fix (see bs/bsky-service branch):* the bot must stop posting
the de-fogged spectator view to a public thread for kriegspiel games.
