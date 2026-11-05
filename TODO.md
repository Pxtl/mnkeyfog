# TODO

Approved-to-implement ideas. Each item = one branch. Do not touch
`MnkeyFog.Model` internals without Pxtl's approval; see CONTRIB.md.


## BskyService — `!list` command  *(IMPLEMENTED — commit `0d72084`)*

New player-facing command on the bsky bot. Purpose: for a multi-game player,
see *which* games they are in (code, template, turn state, start time) without
having to ask for a board per game. `!board` already covers the
"show me my board" path (resolves by uriKey or DID), so this is **only** the
list view — no dedicated `!view`.

*Status: the command is implemented and committed (`0d72084`). The open
questions below (TZ source, timestamp format, "last move" column) remain open
for a follow-up pass.*

### `!list`

- **No arg.**
- Resolve via `GameStore.GetSessionsForDid(senderDid)` — same source the DID
  resolution on `!play`/`!board` already uses, so no new lookup plumbing.
- For each game post one line with:
  - `[#\u003ccode\u003e]` — the code in canonical (upper-case) form, so `g=\u003ccode\u003e` copy-paste is trivial.
  - The template/command name (e.g. `tictactoe`, `gomoku`).
  - Turn state: `YOUR TURN`, `done (winner: X)`, or `waiting`.
  - `started \u003chuman\u003e ago` using `Session.StartedAt` (see open TZ question below).
- If the sender has zero games: single line "you are not in any games. See
  me in a public thread to start one."
- Ordering: oldest-first (confirm `GetSessionsForDid` returns them in this
  order; if not, sort in the handler before rendering).
- **Open timestamp questions (per Pxtl, left open for the next pass):**
  - *Which reference time?* Options:
    1. **Relative only** (e.g. "2h ago") — simplest, no TZ involved.
    2. **Absolute + sender-local TZ** — needs a per-DID TZ source (see below).
    3. **Absolute + bot-local TZ** — trivial but wrong for most users.
  - *"Last move" column:* `GameSession` currently only exposes `StartedAt`
    — no move log is surfaced. If we want this column it's a Model change
    (`LastMoveAt` or an action-log field) and/or a cross-cutting
    concern. **Leave as open — confirm scope with Pxtl before touching
    Model.**

### Open question: per-DID timezone source

`idunno.Bluesky` / atproto does **not** expose a user's timezone in the
standard profile record. What is available without extra work:

- `app.bsky.actor.getProfile` → `profile.displayName`, `profile.description`,
  `avatar`, `indexedAt`, `createdAt`. No TZ field.
- `app.bsky.actor.getPreferences` (actor-specific, requires a **token** for
  that user, not an OAuth app token) → `threadViewMode`, `postViewMode`,
  `adultContentEnabled`, `sensitiveMediaEnabled`, `timeline` settings, …
  No TZ field in the schema.
- **`rsvp` / `app.bsky.chat.*`** — the DM API does not carry TZ either.

So a per-sender localized time display is **not** a trivial atproto lookup.
Realistic options when the decision on absolute time lands:

1. **Skip it** — display UTC, or relative time, or bot-local TZ. (easiest.
   Recommended for now.)
2. **Let the player set their own TZ in the bot.** A `!tz \u003cname\u003e` command that
   stores an IANA TZ name in a per-DID prefs file (alongside `GameStore`
   persistence). This is the only *reliable* per-user TZ source the bot can
   own, and it doubles as a future anchor for other per-player preferences
   (default template, quiet hours, etc.). Cost: small Model change (add a
   prefs file/schema) + one new command.
3. **Heuristic from displayName / location string.** Fragile, unreliable,
   don't do it.

Decision **not yet made** — this and the "last move" column are both Model
touches that need Pxtl's green light before they land on this branch. The
`!list` MVP (code/template/turn-state + `StartedAt` relative or UTC) is
safe to implement now.


## Out of scope

- `!code` — user explicitly declined; `!new`/`!join` replies already put the
  code in the player's DM history. No dedicated `!code`.
- `!view \u003ccode\u003e` — user explicitly declined; `!board` already covers
  "show me my board" (resolves by thread anchor, with a DID fallback for
  single-game senders), so `!view` would be redundant. If a future need
  arises for an explicit-code board fetch from a non-player (e.g. a spectator
  who only has the code), revisit then.
- Anything that adds move-log history to `MnkeyFog.Model` (see open question
  above).

## Done (this branch)

- **Kriegspiel board-leak fix** (`8d9af7c`) — `OneOf<BoardView, BoardHidden>`
  render path; `SendDmToPlayerAsync` on `BskyBotService` (lazy 1:1
  conversation cache); `ExecuteMovesAsync` DMs every player their fogged
  view on `IsViewChanged == true` for secret games. 77 + 123 + 3 = 203 tests
  pass.
- **`!quit`** — present in `KnownCommands`; `!complete` removed (game
  completion is automatic via `IsGameOver`).

## Follow-ups (from PR #11 review)

Cross-repo items (Pxtl to action — I cannot push to other repos):

- **pxtl.ca-collections**: add `ConcurrentHashSet<T>` (wrapper over
  `ConcurrentDictionary<T, bool>`). The GameStore description said "set" but it's
  a `ConcurrentDictionary`.
- **Pxtl/dotnet-repo-template.props**: extract coding-standard elements
  (analyzers, implicit usings, nullable) from `MnkeyFog.Text.csproj` into a
  `.props`.
- **Pxtl/dotnet-repo-template**: backport template-file changes
  (`CONTRIB.md`, `AGENTS.md`, `.gitignore`, `.editorconfig`).

Design decisions (Pxtl to decide):

- **`!resign`**: `!quit` is incoherent for online games; `!resign` should
  abandon-while-game-continues. May need `MnkeyFog.Model` to allow resign at
  any time (Model is leaf — needs approval).
- **TextBoardRenderer → MnkeyFog.Text**: move the header/footer/budget into
  `MnkeyFog.Text` as a public `GameState + PlayerId → string` method (the
  board-drawing already delegates; only the Bsky-specific header/footer/budget
  would move).
- **`CellNameResolver` override**: the Bsky override always shows the space
  name on empty cells; the `MnkeyFog.Text` default only shows it when
  `CanTakeTurn && !board.IsDone`. Decide whether to remove the override (changes
  Bsky display for the non-turn player).

## Notes (this branch)

Small refactor candidates / cleanups found while working the bsky branch. Not
yet implemented; each is a note for a future pass.

- **`DrawBoards` player-list flexibility** (`MnkeyFog.Text/BoardRenderer`):
  `DrawBoards(GameView, int maxRenderWidth)` renders every board in a *single*
  player's `GameView`. It could be more flexible about the player list it draws
  for — e.g. take a game state plus a list of players (or a subset of boards) and
  render each view, instead of one call per player.
- **Dead `player` local** (`MnkeyFog.Model/CommandNameTool.SpaceCommandName`):
  `var player = gameView.PlayerIndex;` then `player = gameView.IsGameOver ? null
  : player;` (comment: "show for all players if the game is over") is computed
  but never read — the rest of the method only checks `gameView.CanTakeTurn`.
  Either remove the dead assignment or wire up the game-over "show all marks"
  visibility it was clearly meant to gate. (Model change — needs approval.)
- **Standard coding-standards `.props`**: factor the shared build/analysis
  settings (nullable, implicit usings, analyzers, langversion, etc.) into a
  `.props` imported by the csprojs so the standard coding-standards config lives
  in one place — like a `dotnet-repo-template.props`. (Relates to the cross-repo
  `Pxtl/dotnet-repo-template.props` follow-up above.)
