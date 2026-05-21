namespace KriegspielTicTacToe;

using KriegspielTicTacToe.Model;
using OneOf;
using OneOf.Types;
using System.IO;

/// <summary>
/// Game logic implementation.
/// </summary>
internal static class GameLogic {
    public static void RunGame(
        FileInfo sharedStateFilePath,
        bool doForceNewGame,
        char[] players,
        IEnumerable<BoardBuilder> boardBuilders,
        OneOf<char, LocalHotseatGame> joinAsPlayer,
        bool isRandomPlayerOrder,
        bool isSynchronousMode
    ) {
        TicTacToeState state;
        if(sharedStateFilePath.Exists && !doForceNewGame) {
            state = StateStorage.LoadState(sharedStateFilePath.FullName);
            Console.Out.WriteLine($"Loaded saved game!");
        } else {
            // Convert char[] to Player[]
            var playerList = new List<Player>(players.Select(c => new Player(c.ToString())));
            state = new TicTacToeState(playerList.ToArray(), boardBuilders, 
                isRandomPlayerOrder: isRandomPlayerOrder,
                isSynchronousMode: isSynchronousMode
            );
            Console.Out.WriteLine("Starting new game!");
            StateStorage.SaveState(state, sharedStateFilePath.FullName);
        }

        Console.Out.WriteLine(joinAsPlayer.Match(
            playerChar => $"Joining game-file '{sharedStateFilePath.FullName}' as player '{playerChar}'.",
            localHotseatGame => "Running in hotseat mode."
        ));
        
        Console.Out.WriteLine($"Players are {string.Join(", ", state.PlayManager.Players)}.");

        bool isGameOver = false;
        while (!isGameOver) {
            var currentPlayerChosen = joinAsPlayer.Match(
                playerChar => {
                    var currentPlayer = new Player(playerChar.ToString());
                    if (!state.PlayManager.Players.Contains(currentPlayer)) {
                        throw new ApplicationException($"Invalid player join, player {currentPlayer} is not a player in this game.");
                    }
                    bool isDoneWaiting = false;
                    Console.Out.Write("Waiting for your turn.");

                    while (!isDoneWaiting) {
                        state = StateStorage.LoadState(sharedStateFilePath.FullName);
                        if (state.PlayManager.PlayersAvailableForTurn.Contains(currentPlayer)
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
                        ? OneOf<Result<char>, GameIsOver>.FromT1(new GameIsOver())
                        : new Result<char>(currentPlayer.Value);
                },
                localHotseatGame => new Result<char>(DoPlayerChooserLoop(state.PlayManager))
            );

            currentPlayerChosen.Switch(
                playerResult => {
                    var currentPlayerChar = playerResult.Value;
                    var currentPlayer = new Player(currentPlayerChar);
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
                                playerChar => { },
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
                        boardIsDone => {
                            currentPlayerIsDoneTurn = false;
                            Console.WriteLine($"That board is already complete.");
                        },
                        result => {
                            activeBoardIndex = result.Value;
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
                spaceCommand.Switch(
                    charCode => {
                        currentPlayerIsDoneTurn = true;
                        using (var stateStorage = new StateStorage(sharedStateFilePath)) {
                            state = stateStorage.State;
                            state.PlayManager.ResignPlayer(currentPlayer);
                        }
                    },
                    spaceCode => {
                        using (var stateStorage = new StateStorage(sharedStateFilePath)) {
                            state = stateStorage.State;
                            state.PlaySpace(boardIndex, spaceCode, currentPlayer).Switch(
                                actionQueuedSuccessfully => {
                                    currentPlayerIsDoneTurn = true;
                                    Console.WriteLine($"Played on board {boardCode}, space {spaceCode}.");
                                }, 
                                resultNewlyLearned => {
                                    currentPlayerIsDoneTurn = true;
                                    Console.WriteLine($"Space already taken by player '{resultNewlyLearned.Value}'.");
                                },
                                alreadyPlayed => {
                                    currentPlayerIsDoneTurn = false;
                                    Console.Out.WriteLine($"Invalid space, that space is already known to player {currentPlayer}");
                                },
                                notFound => {
                                    currentPlayerIsDoneTurn = false;
                                    Console.WriteLine("Invalid space.");
                                }
                            );
                        }
                    },
                    unknown => {
                        currentPlayerIsDoneTurn = false;
                    }
                );
            }
        }

        return currentPlayerIsDoneTurn;
    }

    internal static char DoPlayerChooserLoop(PlayManager playManager) {
        while(true) {
            if (playManager.PlayersAvailableForTurn.Count() == 1) {
                var currentPlayer = playManager.PlayersAvailableForTurn.Single();
                Console.Out.WriteLine($"Player {currentPlayer} ready? Press any key to continue...");
                Console.ReadKey(intercept: true);
                Console.WriteLine();
                return currentPlayer.Value;
            }
            if (playManager.PlayersAvailableForTurn.Count() == 0) {
                throw new InvalidOperationException("No players are available to take a turn.");
            }

            Console.Out.WriteLine($"Player(s) { string.Join(" and ", playManager.PlayersAvailableForTurn) } have not taken their turn.");
            Console.Out.WriteLine("Who will take the next turn? Press the player's character key to take their turn.");
            var key = Console.ReadKey();

            // have to convert to string for case-insensitive comparison.
            var playerAsString = playManager
                .PlayersAvailableForTurn
                .Select(c => c.Value)
                .SingleOrDefault(c => c.Equals(key.KeyChar.ToString(), StringComparison.OrdinalIgnoreCase));

            if (playerAsString != null) {
                // write blank line
                Console.Out.WriteLine();
                return playerAsString;
            }
        }
    }
}

public struct LocalHotseatGame {}

public struct GameIsOver {}
