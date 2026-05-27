namespace KriegspielTicTacToe;

using KriegspielTicTacToe.Model;
using OneOf;
using OneOf.Types;

/// <summary>
/// Game logic implementation.
/// </summary>
internal static class GameLogic {
    public static void RunGame(
        FileInfo sharedStateFilePath,
        bool doForceNewGame,
        Player[] players,
        IEnumerable<BoardBuilder> boardBuilders,
        OneOf<Player, LocalHotseatGame> joinAsPlayer,
        bool isRandomPlayerOrder,
        bool isSynchronousMode
    ) {
        TicTacToeState state;
        if(sharedStateFilePath.Exists && !doForceNewGame) {
            state = StateStorage.LoadState(sharedStateFilePath.FullName);
            Console.Out.WriteLine($"Loaded saved game!");
        } else {
            state = new TicTacToeState(players, boardBuilders, 
                isRandomPlayerOrder: isRandomPlayerOrder,
                isSynchronousMode: isSynchronousMode
            );
            Console.Out.WriteLine("Starting new game!");
            StateStorage.SaveState(state, sharedStateFilePath.FullName);
        }

        Console.Out.WriteLine(joinAsPlayer.Match(
            player => $"Joining game-file '{sharedStateFilePath.FullName}' as player '{player}'.",
            localHotseatGame => "Running in hotseat mode."
        ));
        
        Console.Out.WriteLine($"Players are {string.Join(", ", state.PlayManager.Players)}.");

        bool isGameOver = false;
        while (!isGameOver) {
            var currentPlayerChosen = joinAsPlayer.Match(
                player => {
                    if (!state.PlayManager.Players.Contains(player)) {
                        throw new ApplicationException($"Invalid player join, player {player} is not a player in this game.");
                    }
                    bool isDoneWaiting = false;
                    Console.Out.Write("Waiting for your turn.");

                    while (!isDoneWaiting) {
                        state = StateStorage.LoadState(sharedStateFilePath.FullName);
                        if (state.PlayManager.PlayersAvailableForTurn.Contains(player)
                            ||
                            state.IsGameOver
                        ) {
                            isDoneWaiting = true;
                        } else {
                            Console.Out.Write(".");
                            Thread.Sleep(100);
                        }
                    }
                    Console.Out.WriteLine();
                    return (state.IsGameOver)
                        ? OneOf<Result<Player>, GameIsOver>.FromT1(new GameIsOver())
                        : new Result<Player>(player);
                },
                localHotseatGame => DoPlayerChooserLoop(state.PlayManager)
            );

            currentPlayerChosen.Switch(
                playerResult => {
                    var currentPlayer = playerResult.Value;
                    var currentPlayerIsDoneTurn = DoPlayerTurnLoop(state, currentPlayer, sharedStateFilePath.FullName);

                    if (currentPlayerIsDoneTurn) {
                        var hasStateChanged = false;
                        using (var stateStorage = new StateStorage(sharedStateFilePath.FullName)) {
                            state = stateStorage.State;
                            state.PlayManager.EndTurn(currentPlayer, out hasStateChanged);
                        }

                        if (hasStateChanged) {
                            BoardRenderer.DrawBoards(state, currentPlayer, activeBoardIndex: null);
                        }

                        if (state.PlayManager.IsRoundOver) {
                            var hasRoundStateChanged = false;
                            using (var stateStorage = new StateStorage(sharedStateFilePath.FullName)) {
                                state = stateStorage.State;
                                state.PlayManager.EndRound(out hasRoundStateChanged);
                            }
                            if (hasRoundStateChanged) {
                                Console.Out.WriteLine($"Press any key to continue...");
                                Console.ReadKey(intercept: true);
                                Console.Clear();
                                Console.Out.WriteLine("Round complete, executing synchronous moves.");
                            }
                        }

                        if (!state.IsGameOver) {
                            joinAsPlayer.Switch(
                                player => { },
                                localHotseatGame =>
                                {
                                    Console.Out.WriteLine($"Press any key to continue...");
                                    Console.ReadKey(intercept: true);
                                    Console.Clear();
                                }
                            );
                        } else {
                            Console.Out.WriteLine(state.GameStateText);
                            isGameOver = true;
                        }
                    }
                },
                gameIsOver => {
                    isGameOver = true;
                }
            );                    
        }
        
        Console.Out.WriteLine("Game over.");
        Thread.Sleep(1000);
        sharedStateFilePath.Delete();
    }

    private static bool DoPlayerTurnLoop(TicTacToeState state, Player currentPlayer, string sharedStateFilePath) {
        var currentPlayerIsDoneTurn = false;
        while (!currentPlayerIsDoneTurn)
        {
            Console.Out.WriteLine($"Player {currentPlayer}, take your turn.");
            
            var activeBoardIndex = state.SingleActiveBoardIndex;

            if (!activeBoardIndex.HasValue) {
                //player picks a board.
                BoardRenderer.DrawBoards(state, currentPlayer, activeBoardIndex);
                var boardCommand = InputUtility.ReadCommandKeys("Press numeric key(s) to pick a board, or 'r' to resign.", 1);
                boardCommand.Switch(
                    charCode =>
                    {
                        currentPlayerIsDoneTurn = true;
                        using (var stateStorage = new StateStorage(sharedStateFilePath)) {
                            state = stateStorage.State;
                            state.PlayManager.ResignPlayer(currentPlayer);
                        }
                    },
                    boardCode => state.SelectBoard(boardCode).Switch(
                        notFound => {
                            currentPlayerIsDoneTurn = false;
                            Console.WriteLine($"That is not a valid board.  Please pick an incomplete board.");
                        },
                        boardingIsDone => {
                            currentPlayerIsDoneTurn = false;
                            Console.WriteLine($"That board is already complete.");
                        },
                        boardIndex => {
                            activeBoardIndex = boardIndex.Value;
                        }
                    ),
                    unknown => {
                        currentPlayerIsDoneTurn = false;
                    }
                );
            }

            // activeBoardIndex will be null if the user has to retry or the
            // user is resigning. In both cases we skip asking them about
            // which space they wish to play.
            if (activeBoardIndex.HasValue) {
                var boardIndex = activeBoardIndex.Value;
                var boardCode = boardIndex + 1;
                BoardRenderer.DrawBoards(state, currentPlayer, boardIndex);
                var spaceCommand = InputUtility.ReadCommandKeys("Press numeric key(s) to play a space, or 'r' to resign.", state.Boards[boardIndex].SpaceIndexCodeLength);
                var spaceString = spaceCommand;
                spaceCommand.Switch(
                    charCode => {
                        currentPlayerIsDoneTurn = true;
                        using (var stateStorage = new StateStorage(sharedStateFilePath)) {
                            state = stateStorage.State;
                            state.PlayManager.ResignPlayer(currentPlayer);
                        }
                    },
                    boarding => state.PlaySpace(boardIndex, boarding, currentPlayer).Switch(
                        notFound => {
                            currentPlayerIsDoneTurn = false;
                            Console.WriteLine("Invalid space.");
                        },
                        actionQueuedSuccessfully => {
                            currentPlayerIsDoneTurn = true;
                            Console.WriteLine($"Played on board {boarding}, space {spaceString}");
                        }, 
                        resultNewlyLearned => {
                            currentPlayerIsDoneTurn = true;
                            Console.WriteLine($"Space already taken by player '{resultNewlyLearned.Value}'.");
                        },
                        alreadyPlayed => {
                            currentPlayerIsDoneTurn = false;
                            Console.Out.WriteLine($"Invalid space, that space is already known to player {currentPlayer}");
                        }
                    ),
                    unknown => {
                        currentPlayerIsDoneTurn = false;
                    }
                );
            }
        }

        return currentPlayerIsDoneTurn;
    }

    internal static OneOf<Result<Player>, GameIsOver> DoPlayerChooserLoop(PlayManager playManager) {
        var playerToKey = ModelToKeyUtility.BuildPlayerToKeyMap(playManager.PlayersAvailableForTurn);

        var keyToPlayer = playerToKey
            .ToDictionary(
                pair => pair.Value,
                pair => pair.Key,
                StringComparer.OrdinalIgnoreCase);

        while (true) {
            if (playManager.PlayersAvailableForTurn.Count() == 1) {
                var currentPlayer = playManager.PlayersAvailableForTurn.Single();
                Console.Out.WriteLine($"Player {currentPlayer} ready? Press any key to continue...");
                Console.ReadKey(intercept: true);
                Console.WriteLine();
                return new Result<Player>(currentPlayer);
            }
            if (!playManager.PlayersAvailableForTurn.Any()) {
                throw new InvalidOperationException("No players are available to take a turn.");
            }

            // Display all available players with alternate key hints for non-typeable marks
            var playerDisplayList = playManager.PlayersAvailableForTurn
                .Select(p =>
                {
                    var altKey = playerToKey[p];
                    var keyDisplay = altKey.Equals(p.Mark, StringComparison.OrdinalIgnoreCase)
                        ? ""
                        : $" ({altKey})";
                    return $"Player {p.Mark}{keyDisplay}";
                });
            Console.Out.WriteLine($"Player(s) {string.Join(" and ", playerDisplayList)} have not taken their turn.");
            Console.Out.WriteLine("Who will take the next turn? Press the player's key to take their turn.");

            var keyRead = Console.ReadKey();

            // Handle Escape key to quit the game
            if (keyRead.Key == ConsoleKey.Escape)
            {
                Console.Out.WriteLine("Quitting.");
                return new GameIsOver();
            }

            var keyStr = keyRead.KeyChar.ToString();

            if (!keyToPlayer.TryGetValue(keyStr, out var selectedPlayer)) {   
                // No player matched - inform user
                Console.Out.WriteLine($"No player found for '{keyStr}'. Please try again.");
                continue;
            } else {
                Console.Out.WriteLine();
                return new Result<Player>(selectedPlayer);
            }
        }
    }
}

public struct LocalHotseatGame { }

public struct GameIsOver { }
