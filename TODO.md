# TODO

Ready to work on.  By the time an item is here Pxtl is
confident about it (likely to approve). Each item is one branch.

> **Adding items:** if an item comes from a specific PR, task, or review, note
> where it came from (e.g. "from PR #16 review"). General backlog items need no
> origin.

## Approved Items

The following items are fully approved and can be taken up immediately.

- **Row/Column BoardRenderer Mode**.  Depends on **`RowModelNames` and`ColumnModelNames` and 
  `BoardModelNames` and `MnkeyFog.Text.GameRenderer`**
  - The board does not render the `SpaceDisplayName` on spaces at all. Instead,
    RowDisplayNames are rendered next to the row, and ColumnDisplayNames are
    rendered above the columns. eg (assuming a 4x4 board that is the first of
    several (so the board name is displayed) and has a displayname "U")
    ```
     U┌───┬───┬───┬───┐
      │ A4│ B4│ C4│ D4│
      ├───┼───┼───┼───┤
      │ A3│ B3│ C3│ D3│
      ├───┼───┼───┼───┤
      │ A2│ B2│ C2│ D2│
      ├───┼───┼───┼───┤
      │ A1│ B1│ C1│ D1│
      └───┴───┴───┴───┘
    ```
    becomes (note change in indentation and location of board name)
    ```
    ┌───┬───┬───┬───┬───┐
    │ U │ A │ B │ C │ D │
    ├───┼───┼───┼───┼───┤
    │ 4 │   │   │   │   │
    ├───┼───┼───┼───┼───┤
    │ 3 │   │   │   │   │
    ├───┼───┼───┼───┼───┤
    │ 2 │   │   │   │   │
    ├───┼───┼───┼───┼───┤
    │ 1 │   │   │   │   │
    └───┴───┴───┴───┴───┘
    ```
- **Pass moves**.  In MNKAction, create a new ActionFactory and Action for
  "Pass".  `PassActionFactory` will be a `GameActionFactoryForSimple`.  Full
  `MNKTemplate` will *not* have the `PassActionFactory`, but a new base template
  type `CasualMNKTemplate` will be added. `CasualMNKTemplate` will have the
  `PassActionFactory`.
  - "Pass" is a no-op move for the player.
  - Games being played on templates with multiple actions available will need a
    way to specify which ActionFactory the player is using, both in console
    *and* in Bsky.
  - It may be appropriate to have the `GameTemplate` offer some way to indicate
    the default action, so that `BSky` can assume an action when the player has
    not provided one.
  - `GameActionFactory.Name` shall become a ModelName, with the commandname of
    the existing `MNKAction` be "space". So the command becomes `!play space <x>
    <y>`, or `!play pass` to pass.  The games `KriegspielTicTacToe`,
    `BasicTicTacToe`, and `FreestyleGomoku` shall remain on `MNKTemplate` while
    all other templates shall be on `CasualMNKTemplate`.

- **ModelNamer for Players**.  Player marks are managed by the `ModelNamer`,
  but **not** as `ModelName` objects (unlike boards/rows/columns) — `ModelName`
  doesn't map cleanly here.  Design:
  - A player joins a game (or a game is created with a list of players) carrying
    a **`CommandName`**: the human-typeable name — their BSky handle, or the
    mark char they picked in the CLI.
  - `ModelNamer` holds a list of `string` **PlayerMarks** (not `ModelName`),
    assigned by `playerIndex`.  The PlayerMark is what's drawn on the board.
    In a text message a player displays as
    `$"{player.CommandName}({namer.GetPlayerMark(playerIndex, playersState)})"`.
  - `GetPlayerMark(playerIndex, playersState)` default (no marks provided):
    use the `CommandName` if it is 1 char, else the `playerIndex`.
  - A **numeric 1-char `CommandName` is a user error** — notify the user it's
    not allowed.  Won't arise in BSky (handles are multi-char), possible in CLI.
  - `PlayerInfo`/`Player` expose `CommandName` (`Player` also `Index`).  The mark
    lookup needs the player's `CommandName` by index (via `PlayersState` or a
    full `Player`).  Caching the namer-derived Mark on `Player` is an **open
    question** (very unsure).
  - The existing CommandLine non-typeable-Mark code **stays** (makes a little
    less sense now that there's a built-in mark/name distinction, but the CLI
    generally uses the default mode: each player picks a 1-char Mark — really
    `PlayerInfo.CommandName` — and the namer returns that CommandName as the Mark).

- **Emoji Styling**.  Depends on "ModelNamer for PLayers" & "BoardRenderer
  Constructor" and "Row/Column BoardRenderer Mode". BSky BoardRenderer shall
  take `BoardRendererConfiguration` and namer constructor parameters to produce
  boards that look like this:
  
  Use fullwidth text for rows & space, number-buttons for columns.  Small black
  square for blank spaces, Full black square for lines between spaces if
  gridding.

  ```
  　0️⃣1️⃣2️⃣3️⃣4️⃣5️⃣6️⃣7️⃣8️⃣9️⃣🔟
  Ａ▪️⭕▪️▪️▪️▪️▪️❎▪️▪️▪️
  Ｂ▪️▪️▪️▪️💠▪️▪️▪️🟫▪️▪️
  Ｃ▪️▪️⭐▪️▪️▪️▪️▪️▪️▪️🩷
  Ｄ▪️▪️▪️▪️▪️💧▪️▪️▪️▪️▪️
  Ｅ▪️✴️▪️▪️▪️▪️▪️▪️🔳▪️▪️
  Ｆ▪️▪️▪️▪️🌀▪️▪️▪️▪️▪️▪️
  Ｇ▪️▪️▪️▪️▪️▪️🔘▪️▪️▪️❇️
  Ｈ▪️🛑▪️▪️▪️▪️▪️▪️▪️▪️▪️
  Ｉ▪️▪️▪️▪️🔶▪️▪️▪️🧿▪️▪️
  Ｊ▪️▪️▪️▪️▪️▪️▪️▪️▪️🟣▪️
  Ｋ▪️▪️▪️▪️▪️▪️▪️▪️▪️▪️▪️
  ```

  List in planned player order:

  　❎⭕💠⭐🔳🔶❇️🌀🟣🩷🛑🟫✴️🔘💧🧿

  Upper limit for size for this mode shall be 11.

  

## Feasible Items

The following items are not quite ready to start but are generally on the
road-map to-do.

- None.
