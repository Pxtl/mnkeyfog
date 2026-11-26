# TODO

Ready to work on.  By the time an item is here Pxtl is
confident about it (likely to approve). Each item is one branch.

> **Adding items:** if an item comes from a specific PR, task, or review, note
> where it came from (e.g. "from PR #16 review"). General backlog items need no
> origin.

## Approved Items

The following items are fully approved and can be taken up immediately.

- **Pre-provided `BoardModelNames`:**
  - Background: Currently board command names are always boardindex+1.  This is
    too inflexible.
  - Solution: Allow the `ModelNamer` to take an enumerable of a new `ModelName`
    object in its constructor.  `ModelName` has 2 members: `DisplayName` and
    `CommandName`.  The `CommandName` is what the user types to select that
    model.  A `DisplayName` is what is shown on the board.
    - So this means that boards will no longer have a single Name beyond their
      index, they will have a `DisplayName` and a `CommandName`.  Many existing
      functions will need to be duplicated, and which to use will have to be
      considered carefully (is it for key entry by user? Or is it for display on
      the screen?)
    - The `BoardModelNames` array/list/(immutable?) must be at least as long
      as the number of boards, or else rendering is an error.
    - Commands that take boards will select by the `CommandName` of the board.
      Building multi-char locations out of commands (like space command names)
      will also use the board's `CommandName`.
    - Where the board's `ModelName` is displayed to the user (eg by the
      boardrenderer), the game will show the `DisplayName`.
    - The default (parameterless) constructor `ModelNamer` will mimic current
      behaviour (`CommandName = DisplayName = boardIndex + 1`), but alternately
      callers will be able to name the boards whatever they want (eg "P", "Q",
      "R" boards - probably the future default will be "UVWQYZMNT" to avoid X
      and O and R and the number chars that are used for spaces).

- **TextBoardRenderer → `MnkeyFog.Text.GameRenderer`** (from PR #11 review)
  (depends on **Refactor BoardRenderer:**, do not start on this until that PR is
  merged or else you will face conflicts): 
  - Move what can be moved into `MnkeyFog.Text` as new class `GameRenderer` with
    a public `GameState + PlayerId → string` method. This class would provide a
    consolidated place for generating messages about gamestate within a given
    character-budget. This could be used for both Bsky *and* CommandLine, since
    currently they have redundant implementations.
  - Basic game render - listed in order of occurrance.  Lower items are
    eliminated if they exceed character budget. 
    - Gamename (if any, not applicable in CommandLine)
    - Event message (eg "player X resigned" or "game over, player Y wins" or
      "Players X, Y, and Z can take their turns" or "Player X's turn").
    - Board Render (if visible in this context, for example not shown in public
      posts in Kriegspiel games)
    - list of active players
    - list of resigned players

- Next phase: **`RowModelNames` and`ColumnModelNames`**.  Depends on
  **Pre-provided `BoardModelNames`**, do not start this until above is merged.
  When calculating the `SpaceCommandName` or `SpaceDisplayName`, the
  `RowModelNames` and `ColumnModelNames` will be used against the row/column
  indices to determine the display names and command names of a row or column
  just like how a boardname is done above.
  - RowModelNames and ColumnModelNames will not be used in boards where
    `IsSpaceNamingNumpadLayout`, only used on larger boards.
  - The extension method
    ```cs
    GetSpaceName(this GameView gameView, BoardView board, sbyte col, sbyte row, ModelNamer namer)
    ```
    will have to call back into `ModelNamer` to fetch the underlying name of
    the row and column and board.
  - Default (parameterless) constructor will have to assume largest possible
    board and zero-pad.  A static factory-method or alternate constructor that
    creates shorter (1-digit) row and column names will need to be available.
  - Again, throw exception if trying to render beyond the limits of the list of
    commands.

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

- **BoardRenderer Constructor**.  BoardRenderer should take both a namer *and* a
  BoardRendererConfiguration during construction.

- **ModelNamer for Players**.  Much like rows, columns, and boards, Player Names
  shall be managed by the ModelNamer.  Currently, most Player-related
  constructions take a Mark or a list of Marks.  The Mark shall instead be the
  DisplayName of a Player.Name, while the CommandName shall be the name provided
  during construction of the game (BSky or CLI).  Default (null) implementation
  shall be that DisplayName = CommandName.

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
