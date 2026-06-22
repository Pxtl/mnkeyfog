# TODO

## Automated Testing

## Game Type Files
Create new GameType object that describes a single GameType for KSTTT, where a
GameType includes the BoardBuilder list and the isSynchronousMode flag.  Future
configurations will be added to gametype as they develop.

### New GameType Model Object
- Create the GameType model object described above.
- Add a new Constructor for TicTacToeState that wraps the following constructor:
    ```
    public TicTacToeState(
        Player[] players,
        IEnumerable<BoardBuilder> boardBuilders,
        bool isRandomPlayerOrder,
        bool isSynchronousMode
    ) {...}
    ```
    with
    ```
    public TicTacToeState(
        Player[] players,
        GameType gameType,
        bool isRandomPlayerOrder
    ) {...}
    ```
- Refactor other constructors by moving their components that are part of
  GameType into the GameType constructors.
- Remove the other TicTacToeState constructors so that all calls to
  TicTacToeState constructor use the constructors
    ```
    public TicTacToeState(
        Player[] players,
        GameType gameType,
        bool isRandomPlayerOrder
    ) {...}
    ```
