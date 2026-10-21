# TEMP – Extract Reconciliation & Bsky Bot TODO

Draft. Do not delete until the extraction branch is merged (see
[PLAN.md](PLAN.md) → "BSky bot? Chat Bot?"). This file records why the
shared text-rendering and JSON-persistence helpers were pulled out of
`MnkeyFog.CommandLine` and `MnkeyFog.BskyService` into a new `MnkeyFog.Text`
project, what is done, and what still needs doing before the Bsky bot can be
considered production-usable.

## 1. Why `MnkeyFog.Text` exists

Both `MnkeyFog.CommandLine` (the interactive CLI) and `MnkeyFog.BskyService`
(the Bluesky bot) need to:

1. **Render a board** to a string of unicode box-drawing characters (with
   fog-aware cell names for the CLI and space identifiers for the bot), and
2. **Serialize / deserialize game state** to/from JSON for persistence.

Both had their own copies of these facilities. Neither project could depend
on the other: CLI is a `System.CommandLine` console app, BskyService depends
on `idunno.Bluesky` + `idunno.AtProto` (Jetstream). Depending one on the other
would have forced the wrong dependency direction. So the shared pieces were
lumped into a new leaf project `MnkeyFog.Text` that depends only on
`MnkeyFog.Model`. That was the missing middle layer.

## 2. What was moved

| File | From | To | Notes |
|---|---|---|---|
| `BoardRenderer.cs` | `MnkeyFog.CommandLine` | `MnkeyFog.Text` | Pipe-drawing engine; added `public static CellNameResolver` hook so each consumer can substitute cell-name formatting without touching the renderer body. |
| `BoardRendererConfiguration.cs` | `MnkeyFog.CommandLine` | `MnkeyFog.Text` | Per-board width / line-height config. |
| `KnownTypesBinder.cs` → `ModelTypesBinder.cs` | `MnkeyFog.CommandLine` | `MnkeyFog.Text` | Renamed to reflect that it no longer binds *only* CommandLine-local types; it now also registers the persistence DTOs (`PersistedSession`, `PersistedPlayerDid`) that live in this project. |
| `Persistence/PersistedSession.cs` | `MnkeyFog.BskyService` | `MnkeyFog.Text/Persistence` | `internal sealed record` → `public sealed record` so `BskyService.GameStore` can reference it when it implements `ISave`. |
| `Persistence/PersistedPlayerDid.cs` | `MnkeyFog.BskyService` | `MnkeyFog.Text/Persistence` | Same visibility lift. |

Files deleted (superseded by the Text project):

- `MnkeyFog.CommandLine/BoardRenderer.cs`
- `MnkeyFog.CommandLine/BoardRendererConfiguration.cs`
- `MnkeyFog.CommandLine/KnownTypesBinder.cs`
- `MnkeyFog.BskyService/ModelTypesBinder.cs`
- `MnkeyFog.BskyService/Persistence/PersistedSession.cs`
- `MnkeyFog.BskyService/Persistence/PersistedPlayerDid.cs`

## 3. What stayed put and why

- `MnkeyFog.CommandLine/CommandNameTool.cs` – CLI-only formatting (`SpaceCommandName` etc.) for the human keyboard. The Text project exposes a `func` hook instead of taking a dependency on this.
- `MnkeyFog.CommandLine/InputParser.cs` – reads a keyboard line, applies the board's cell names, and dispatches a move. CLI-specific.
- `MnkeyFog.BskyService/ActionParser.cs` – parses `!board`, `!join`, `!play`, etc. out of Bluesky messages. Bot-specific.
- `MnkeyFog.BskyService/BskyGameService.cs` – the top-level bot. Bot-specific.
- `MnkeyFog.BskyService/BskyBotService.cs` – base class for the bot (Jetstream subscribe, DM channel). Bot-specific.
- `MnkeyFog.BskyService/GameStore.cs` – the in-memory + on-disk registry of games that the bot owns. Bot-specific (CLI has no equivalent).
- `MnkeyFog.BskyService/GameSession.cs` – per-game session wrapper the bot uses. Bot-specific.
- `MnkeyFog.BskyService/SessionCodes.cs` – bot's short-code allocation (≤2 base32 chars for the first 1023 codes, 6-char random fallback). Bot-specific: CLI has no codes, no multi-tenant store.

## 4. `CellNameResolver` hook

`MnkeyFog.Text.BoardRenderer` has a public static `CellNameResolver`:

```csharp
public static Func<GameView, sbyte, sbyte, sbyte, string> CellNameResolver { get; set; }
    = CommandNameTool.SpaceCommandName;
```

The default is the CLI's fog-aware `CommandNameTool.SpaceCommandName` so the
CLI keeps working unmodified (the CLI never sets the hook). `BskyService` sets
it to `TextBoardRenderer.BskyCellName` in its static ctor, so the same renderer
produces `A1`, `B2`, … cell names for bot posts. The hook is `static` because
exactly one of the two consumers ever runs in a single process.

## 5. Project dependency graph after the move

```
MnkeyFog.Model  (core game-state, views, templates)
    ▲
    │
MnkeyFog.Text  (BoardRenderer, ModelTypesBinder, Persistence DTOs)
    ▲                         ▲
    │                         │
MnkeyFog.CommandLine     MnkeyFog.BskyService
```

Both leaf consumers now reach the shared renderer and the shared JSON binder
without depending on each other.

## 6. New test project

`tests/MnkeyFog.BskyService.Tests/`

- Framework: xUnit v3 (`xunit.v3.mtp-v2` 3.2.2) + FluentAssertions 7 + MTP
  runner, matching `tests/MnkeyFog.Model.Tests` per [AGENTS.md](AGENTS.md).
- `dotnet run` (not `dotnet test`) per the AGENTS OOM-avoidance note.
- Two test classes, 52 tests total:
  - `SessionCodesTests` – known-value encodings, 2-char range, 3-char spill,
    `ToBase32`/`FromBase32` round-trip, `InvalidOrMissing_ReturnsNull`, `6`
    -char random codes, 10 k uniqueness sweep, and the boundary-corrected
    `LengthOfValue` helper.
  - `GameStoreTests` – `GameStoreCodeRecyclingTests` (short-range allocation
    sequential-ness, recycle-oldest-completed when short range is full,
    no-completed short-range fallback to a 6-char random code, code
    uniqueness on 150 allocations) and `GameStoreDidIndexTests` (single-game
    vs. zero/multi resolution, DID-index multi-game listing, code-vs-DID
    priority).

## 7. `LengthOfValue` off-by-one fix

The original implementation of
`SessionCodes.LengthOfValue` was
`value == 0 ? 0 : (int)Math.Ceiling(1 + Math.Log(value, 32))`, which returns
one digit too many whenever `log(value, 32)` is exactly integer (i.e. at powers
of 32). The digit count in base-32 is instead the smallest `d` such that
`32^(d-1) <= value < 32^d`, which is `ceil(log(value + 1, 32))`. Fixed in this
branch; covered by the new boundary tests (`31`, `32`, `1023`, `1024`,
`32767`, `32768`).

## 8. Bsky bot TODO (open, not addressed here)

Items left on the critical path before the Bsky bot is production-usable:

- **Persistence is not fully wired.** `GameStore.Save(code)` writes a single
  game to disk, but there is no `Restore` / `LoadAll` on startup (see
  `GameStore.LoadPersisted`, currently a no-op stub). Persisted sessions should
  be re-hydrated when the bot restarts. The persistence DTOs themselves are in
  `MnkeyFog.Text` and ready.
- **Jetstream connection resilience.** `BskyBotService` subscribes to
  `$imMessage` and `$createPost`/`$updatePost` via `JetstreamSubscribe`. There
  is no reconnection policy: a dropped socket silently stops the bot.
  Add a `System.Threading.Timer`-based exponential-backoff reconnect.
- **Rate-limit acknowledgement.** Bluesky's `idunno.AtProto` returns
  rate-limit headers; we currently ignore 429s and just keep sending.
  Add a `SemaphoreSlim`-gated request channel and honour `Retry-After`.
- **Mention-parse hardening.** `BskyGameService.ParsePlainMoveMentions`
  currently trusts the AT Proto `facets` array. If a post has a facet that
  does not line up with the message body (rare but legal), the move parse
  can desync. Guard with a facet-length sanity check before the parse.
- **Board render width.** The bot currently renders boards with the same
  80-col CLI width. Bluesky posts are wider than a terminal but the board
  still wraps. Consider a narrower `BoardRendererConfiguration` for the bot
  (e.g. width 64) or a `!board --narrow` flag.
- **Secret / public game distinction in the UI.** `BskyGameService.HandlePlayAsync`
  refuses plain-text `!play` for secret games with a warning; the warning
  string is hard-coded. Consider surfacing the secret state in the `!board`
  render itself (e.g. a `🔒` badge in the title).
- **`!quit` semantics.** `!quit` currently calls `EndGame` which removes the
  session from the in-memory store. Confirm this is the desired lifecycle
  (a quit player vs. a game-end) before shipping.
- **Tests for `!help` and `!board` paths.** The two test classes above cover
  the store and the code allocation; the service-level handlers
  (`BskyGameService.Handle*Async`) are not yet unit-tested. They would need
  a fake `MessageChannel` and a fixture `BskyGameService`.

## 9. Things I *did not* touch

- `MnkeyFog.Model` – untouched per AGENTS. All model / view / template code is
  identical before and after.
- `MnkeyFog.CommandLine/InputParser.cs`, `CommandNameTool.cs` – not moved, not
  modified, only their `using` directives updated to reach `MnkeyFog.Text` for
  the shared renderer.
- `MnkeyFog.BskyService` Jetstream / mention parsing – per AGENTS, no
  rewrites. Only `TextBoardRenderer`, `BskyGameService` (one IDE0055 fix on
  line 133), and `GameStore` (compile fixes for out-params, `ContainsKey`,
  completed-session lifecycle).
- The `!board`, `!help`, `!quit`, `!new`, `!join`, `!play` command set –
  unchanged. `ActionParser.KnownCommands` still lists exactly those six.

## 10. Verification

This branch, at the time of writing, builds clean (0 warnings / 0 errors) and
runs all tests green:

- `tests/MnkeyFog.Model.Tests` – 126/126 pass.
- `tests/MnkeyFog.BskyService.Tests` – 52/52 pass.
