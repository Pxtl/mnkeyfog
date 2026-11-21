# TODO

Ready to work on, pending approval — by the time an item is here Pxtl is
confident about it (likely to approve). Each item is one branch.

> **Adding items:** if an item comes from a specific PR, task, or review, note
> where it came from (e.g. "from PR #16 review"). General backlog items need no
> origin.

- **TextBoardRenderer → `MnkeyFog.Text`** (from PR #11 review): move the
  header/footer/budget into `MnkeyFog.Text` as a public `GameState + PlayerId →
  string` method. The board-drawing already delegates; only the Bsky-specific
  header/footer/budget would move.
- **`DrawBoards` player-list flexibility** (`MnkeyFog.Text/BoardRenderer`; from
  the bsky branch work): `DrawBoards(GameView, int maxRenderWidth)` renders every
  board in a single player's `GameView`. Make it take a game state plus a list of
  players (or a subset of boards) and render each view, instead of one call per
  player.
- **Dead `player` local** (`MnkeyFog.Model/CommandNameTool.SpaceCommandName`;
  from the bsky branch work; requires model change): `var player =
  gameView.PlayerIndex;` then `player = gameView.IsGameOver ? null : player;`
  (comment: "show for all players if the game is over") is computed but never
  read — the rest of the method only checks `gameView.CanTakeTurn`. Remove the
  dead assignment or wire up the game-over "show all marks" visibility it was
  meant to gate.
- **Error logging for undecodable persisted sessions**
  (`MnkeyFog.Bsky/GameStore.LoadPersisted`; from PR #16 review): the
  `catch (Exception) { continue; }` that skips a persisted session that can't be
  reconstructed is silent. Add an error/warning log so an operator knows a
  session was skipped at startup (the store entry stays on disk for manual
  inspection).
- **Deterministic Random AI** Many of the AIs use randomness (one as main strat,
  some as fallback).  Introduce optional random seed settings to these AIs so
  that when run in testing they will have deterministic pseudorandom behaviour.