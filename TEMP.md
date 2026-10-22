# TEMP

Working notes for the current branch. Read once, then append as needed.
Deleted on merge.

## bsky-kriegspiel-leak-fix (iteration 4+)

### Done so far
- **Items 1-5 of checklist**: all core implementation complete.
  - `SendDmToPlayerAsync` on `BskyBotService` with conversation cache.
  - `SendDmToPlayerAsync` made `virtual` so tests can override it.
  - `RenderPublicBoardAsync` returns `OneOf<BoardView, BoardHidden>`;
    `BoardHidden` when `session.IsSecret` (no de-fogged render).
  - `RenderBoardForDmAsync` same — `BoardHidden` for non-player on secret.
  - `ExecuteMovesAsync` tracks `IsViewChanged` (OR'd) and, when
    `anyViewChanged && session.IsSecret`, DMs every player their own
    fogged view via `SendDmToPlayerAsync`.
  - `BskyGameService` no longer `sealed` (was breaking test subclass).
  - `InternalsVisibleTo("MnkeyFog.BskyService.Tests")` in
    `src/MnkeyFog.BskyService/AssemblyInfo.cs`.
- **Item 6 (tests)**: `BskyGameServiceTests.cs` — 26 tests pass:
  - `IsSecret` true for kriegspiel, false for plain.
  - `RenderPublicBoardAsync` returns `BoardHidden` for kriegspiel,
    `BoardView` for plain.
  - `IPlayActionResult.IsViewChanged` truth table (7 cases).
  - `SendDmToPlayerAsync` override captures DMs in `TestableGameService`.
  - `BoardView`/`BoardHidden` record sanity checks.
- **Item 7 (build+test)**:
  - `dotnet build MnkeyFog.slnx` → 0 errors, 0 warnings.
  - `tests/MnkeyFog.BskyService.Tests` → 77/77 passed.
  - `tests/MnkeyFog.Model.Tests` → 123/123 passed.
  - `tests/MnkeyFog.CommandLine.Tests` → 3/3 passed.
- **Item 8 (commit)**: awaiting final review before commit.

### New files
- `src/MnkeyFog.BskyService/AssemblyInfo.cs` — `InternalsVisibleTo`
- `src/MnkeyFog.BskyService/BoardView.cs` — `internal record BoardView(string Text)`
- `src/MnkeyFog.BskyService/BoardHidden.cs` — `internal record BoardHidden;`
- `src/MnkeyFog.BskyService/IPlayerNotifier.cs` — `IPlayerNotifier` interface
- `tests/MnkeyFog.BskyService.Tests/BskyGameServiceTests.cs`
- `tests/MnkeyFog.BskyService.Tests/TestableGameService.cs`

### Modified files
- `src/MnkeyFog.BskyService/BskyBotService.cs`
- `src/MnkeyFog.BskyService/BskyGameService.cs`
- `src/MnkeyFog.BskyService/MessageChannel.cs`
- `tests/MnkeyFog.BskyService.Tests/GlobalUsings.cs`

