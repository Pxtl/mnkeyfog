# TEMP

This is a working scratch file documenting the `MnkeyFog.Text` extraction
performed on branch `bs/bsky-service`. It is gitignored (the repo's root
`.gitignore` has a `temp/` pattern that is case-insensitive on this machine,
but the file name `TEMP.md` is tracked because it is in the staging area;
if you are reading this you are looking at the committed copy — which is
fine, this file is the deliverable the user asked for).

Do **not** delete this file until the `bs/bsky-service` branch is merged.

## Bsky bot (TODO, awaiting user sign-off)

Decisions the user still needs to sign off on:

- (1) Should the `[#<code>]` tag appear in **every** move result, or only in
  `!board` / `!new` / `!join`? The current implementation stamps it on
  `!new`, `!join`, `!board`, and every move outcome. Move-results with a
  code tag are the ones that get forwarded/quoted when a player re-shares a
  board, so the tag travelling with the move result is what makes the
  `g=<code>` selector actually reachable. But it does add ~5 chars per
  message. The user should decide whether that is acceptable in a high-
  volume bot.
- (2) Should there be a dedicated `!code` command that simply returns the
  current code for the sender's active game? This would be a small
  convenience: `!code` in a DM → `[#B7]` if the sender has one active game,
  `your active games are [#B7] [#C1]` if they have several, a friendly
  error if they have none. Currently the user gets the code on `!board`
  and `!play`, but has to ask for a board to learn their code — a `!code`
  command would let them learn it without revealing a board state.

## MnkeyFog.Text extraction reconciliation

### What was extracted

New project `src/MnkeyFog.Text/` (net10.0, references `MnkeyFog.Model` +
`Newtonsoft.Json 13.0.3` only) now contains the **sole** canonical copies of:

| Type | Where it lives | Notes |
|---|---|---|
| `BoardRenderer` | `MnkeyFog.Text` | Pipe-drawing engine. **New** member: `public static Func<GameView, sbyte, sbyte, sbyte, string> CellNameResolver { get; set; }` — a hook each consumer sets in a static ctor to choose how unoccupied cells are named. Default: `CommandNameTool.SpaceCommandName` (the CLI's fog-aware resolver). |
| `BoardRendererConfiguration` | `MnkeyFog.Text` | `FullPipes` / `Borderless` / `HashPipes` presets. Behaviour unchanged. |
| `ModelTypesBinder` | `MnkeyFog.Text` | (was `KnownTypesBinder` in CommandLine and `ModelTypesBinder` in BskyService). Renamed to `ModelTypesBinder`. A `superset` binder: permits every Model type `MnkeyFog.Model` exposes *and* the two `MnkeyFog.Text.Persistence` DTOs. Single `ModelTypesBinder.Instance` used by both consumers. |
| `PersistedSession` | `MnkeyFog.Text.Persistence` | (was `MnkeyFog.BskyService.Persistence.Public sealed record` `internal sealed record` → **`public sealed record`** so `BskyService.GameStore` can reference it) |
| `PersistedPlayerDid` | `MnkeyFog.Text.Persistence` | (same visibility lift as above) |

### What was **not** extracted (stayed in CommandLine)

| Type | Why it stayed |
|---|---|
| `CommandNameTool` | Already in `MnkeyFog.Model` (the task's reconciliation note references it; it is not in CommandLine, it was there all along). Stays in Model. No change. |
| `InputUtility` / `InputParser` | CLI-only: reads keyboard lines and converts them to moves. Bsky has its own `ActionParser`. Different shape, not a shared concern. |
| `StateStorage` | CLI-only: file-backed save/load with a `.lock` sidecar. Bsky has `GameStore.Save/LoadPersisted` with its own persistence scheme. The *pattern* (`lock` file + JSON) is reusable, but the code itself is keyed to the CLI's `GameState` + file path.
| `MnkeyFog.CommandLine.Program` / `MnkeyFog.CommandLine` | The `System.CommandLine` CLI entrypoint. Bsky uses the `idunno.Bluesky` Jetstream SDK. No overlap. |

### Reconciliation flag

The user's note refers to an *upstream* `maash/mnkeyfog` repo that already
had this `MnkeyFog.Text` extraction done. That repo is **not reachable from
this box** (SSH auth fails; the remote reports `repo not found`). So this
branch **recreates** the project from the shape described in the user's
note. **If `maash/mnkeyfog` is reachable from another machine, the files
inside `src/MnkeyFog.Text/*.cs` and `src/MnkeyFog.Text/Persistence/*.cs` on
this branch should be diff'd against that upstream before the merge. In
particular, `ModelTypesBinder`'s list of registered types is a superset
assertion and should be reconciled type-by-type.**

## Also-extractable for future services (one-liner rationale per item)

If a third consumer (web frontend, Discord, Slack) shows up, these pieces
of `MnkeyFog.CommandLine` and `MnkeyFog.Model` would also be natural
extraction candidates:

- **`StateStorage` → `MnkeyFog.Text.Persistence` (as a base class or
  interface).** The lock-file + JSON round-trip pattern is exactly what a
  long-lived bot needs, and `GameStore.Save` is already reimplementing the
  same pattern by hand. A shared `ILockableStateStorage<T>` in
  `MnkeyFog.Text.Persistence` would let both CLI and bot share it.
  *Rationale: today's `GameStore.LoadPersisted` and `StateStorage.LoadState`
  are ~80% duplicated code paths doing the same thing (read a JSON file,
  bind by type, reconstruct a `GameState`).*
- **`CommandNameTool.BuildPlayerToCommandNameMap` → `MnkeyFog.Model`** is
  already there; **`CommandNameTool.SpaceCommandName`** (the resolver the
  Text hook defaults to) is also in Model. No change needed; just flag that
  *if* the Model namespace moves in a future refactor, the Text hook's
  default lambda (its initializer) needs to follow or be re-pointed.
- **The OneOf-based input/echo helpers in `InputUtility` (CLI).** The
  `OneOf` result-type idiom (`CommandError`, `AlreadyPlayed`,
  `BoardIsDone`, etc.) is *already* a shared convention (Bsky uses the
  same pattern). A small `MnkeyFog.Text.Results` namespace with the
  canonical OneOf result-type aliases would remove the small drift between
  CLI and Bsky error records. *Rationale: Bsky currently has its own
  `CommandError` record in `MnkeyFog.BskyService` — that *could* be shared
  but is currently tiny and stable, so not extracted this round. Note it
  for the next pass.)*
- **`StateStorage`'s `TouchFile`/`GetLockFilePath` helpers** — pure file
  utility, no game-state coupling. Could move to
  `MnkeyFog.Text.Persistence.FileLock` next pass.

## What the `MnkeyFog.Text` project does **not** pull in

- `MnkeyFog.Model` — the model is a peer dependency, not something this
  project owns or re-exports. Consumers still `using MnkeyFog.Model;`
  directly.
- `System.CommandLine`, `idunno.Bluesky`, `idunno.AtProto` — the two
  consumer-side SDKs. The Text project is a leaf and depends only on
  `MnkeyFog.Model` + `Newtonsoft.Json`.
- Any `BskyService` or `CommandLine` types. Consumers reach into their own
  namespaces; Text never reaches back.

## Verification

Built clean: `dotnet build` → 0 warnings / 0 errors (all six projects in
the solution, plus the new `MnkeyFog.BskyService.Tests` project).

All tests pass (`dotnet run --project`, one per test project):

- `tests/MnkeyFog.Model.Tests` — 126/126 pass.
- `tests/MnkeyFog.BskyService.Tests` — 52/52 pass (new project, this
  round).
