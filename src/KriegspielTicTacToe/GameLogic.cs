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
                    var currentPlayerIsDoneTurn = false;
                    currentPlayerIsDoneTurn = DoPlayerTurnLoop(state, currentPlayer, sharedStateFilePath.FullName);

                    if (currentPlayerIsDoneTurn) {
                        var hasStateChanged = false;
                        using (var stateStorage = new StateStorage(sharedStateFilePath.FullName)) {
                            state = stateStorage.State;
                            state.PlayManager.EndTurn(currentPlayer, out hasStateChanged);
                        }

                        if (hasStateChanged) {
                            Console.Out.WriteLine(
                                BoardRenderer.DrawBoards(state, currentPlayer, activeBoardIndex: null, maxWidth: Console.BufferWidth)
                            );
                        }

                        if (state.PlayManager.IsRoundOver) {
                            var hasRoundStateChanged = false;
                            using (var stateStorage = new StateStorage(sharedStateFilePath.FullName)) {
                                state = stateStorage.State;
                                state.PlayManager.EndRound(out hasRoundStateChanged);
                            }
                            if (hasRoundStateChanged) {
                                InputUtility.PauseAndPressAnyKey();
                                Console.Clear();
                                Console.Out.WriteLine(state.GameStateText);
                                Console.Out.WriteLine("Executing synchronous moves.");
                            }
                        }

                        if (!state.IsGameOver) {
                            joinAsPlayer.Switch(
                                player => { },
                                localHotseatGame =>
                                {
                                    InputUtility.PauseAndPressAnyKey();
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
        Thread.Sleep(1000);
        sharedStateFilePath.Delete();
    }

    private static bool DoPlayerTurnLoop(TicTacToeState state, Player currentPlayer, string sharedStateFilePath) {
        var currentPlayerIsDoneTurn = false;
        while (!currentPlayerIsDoneTurn)
        {
            Console.Out.WriteLine(state.GameStateText);
            Console.Out.WriteLine($"Player {currentPlayer.Mark}, take your turn.");
            
            var activeBoardIndex = state.SingleActiveBoardIndex;

            if (!activeBoardIndex.HasValue) {
                //player picks a board.
                Console.Out.WriteLine(
                    BoardRenderer.DrawBoards(state, currentPlayer, activeBoardIndex, maxWidth: Console.BufferWidth)
                );
                var availableBoardCommands = state.BoardNames;
                var boardCommand = InputUtility.ReadCommandInputWithAddedStandardPlayerCommands(
                    "Press numeric key(s) to pick a board, or 'r' to resign.",
                    state.BoardNames
                );
                boardCommand.Switch(
                    result => {
                        if(result.Value == "r") {
                            currentPlayerIsDoneTurn = true;
                            using (var stateStorage = new StateStorage(sharedStateFilePath)) {
                                state = stateStorage.State;
                                state.PlayManager.ResignPlayer(currentPlayer);
                            }
                        } else {
                            state.SelectBoard(result.Value).Switch(
                                notFound => {
                                    currentPlayerIsDoneTurn = false;
                                    Console.WriteLine($"That is not a valid board.  Please pick an incomplete board.");
                                },
                                boardIsDone => {
                                    currentPlayerIsDoneTurn = false;
                                    Console.WriteLine($"That board is already complete.");
                                },
                                boardIndex => {
                                    activeBoardIndex = boardIndex.Value;
                                }
                            );
                        }
                    },
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
                Console.Out.WriteLine(
                    BoardRenderer.DrawBoards(state, currentPlayer, boardIndex, maxWidth: Console.BufferWidth)
                );
                var spaceCommand = InputUtility.ReadCommandInputWithAddedStandardPlayerCommands(
                    "Press numeric key(s) to play a space, or 'r' to resign.",
                    state.Boards[boardIndex].SpaceNames
                );
                spaceCommand.Switch(
                    result => {
                        using (var stateStorage = new StateStorage(sharedStateFilePath)) {
                            state = stateStorage.State;
                            if(result.Value == "r") {
                                currentPlayerIsDoneTurn = true;
                                state.PlayManager.ResignPlayer(currentPlayer);
                            } else {
                                state.PlaySpace(boardIndex, result.Value, currentPlayer).Switch(
                                    notFound => {
                                        currentPlayerIsDoneTurn = false;
                                        Console.WriteLine("Invalid space.");
                                    },
                                    actionQueuedSuccessfully => {
                                        currentPlayerIsDoneTurn = true;
                                        Console.WriteLine($"Played on board {boardCode}, space {result.Value}");
                                    }, 
                                    newlyLearned => {
                                        currentPlayerIsDoneTurn = true;
                                        Console.WriteLine($"Space already taken by player '{newlyLearned.Value}'.");
                                    },
                                    alreadyPlayed => {
                                        currentPlayerIsDoneTurn = false;
                                        Console.Out.WriteLine($"Invalid space, that space is already known to player {currentPlayer}");
                                    }
                                );
                            }
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

    internal static OneOf<Result<Player>, GameIsOver> DoPlayerChooserLoop(PlayManager playManager) {
        // Use ModelToKeyUtility for clean, testable key mapping
        var playerToCommand = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(playManager.PlayersAvailableForTurn);

        var commandToPlayer = playerToCommand
            .ToOrderedDictionary(
                pair => pair.Value,
                pair => pair.Key,
                StringComparer.OrdinalIgnoreCase);

        while (true) {
            if (playManager.PlayersAvailableForTurn.Count() == 1) {
                var currentPlayer = playManager.PlayersAvailableForTurn.Single();
                InputUtility.PauseAndPressAnyKey(prompt: $"Player {currentPlayer} ready?");
                Console.WriteLine();
                return new Result<Player>(currentPlayer);
            }
            if (!playManager.PlayersAvailableForTurn.Any()) {
                throw new InvalidOperationException("No players are available to take a turn.");
            }

            Console.Out.WriteLine(playManager.GameStateText);

            // Display all available players with alternate key hints for non-typeable marks
            var playerDisplayList = playManager.PlayersAvailableForTurn
                .Select(p => {
                    var altKey = playerToCommand[p];
                    var keyDisplay = altKey.Equals(p.Mark, StringComparison.OrdinalIgnoreCase)
                        ? ""
                        : $" ({altKey})";
                    return $"Player {p.Mark}{keyDisplay}";
                });

            var prompt = "Who will take the next turn? Press the player's key to take their turn (or press 'q' to quit the game for everyone)." + Environment.NewLine
                + string.Join(" ", playerDisplayList);
            var validCommands = ((IEnumerable<string>)["q"]).Concat(commandToPlayer.Keys);
            var commandResult = InputUtility.ReadCommandInputLoop(prompt, validCommands);

            if (commandResult == "q") {
                Console.Out.WriteLine("Quitting.");
                return new GameIsOver();
            } else {
                return new Result<Player>(commandToPlayer[commandResult]);
            }
        }
    }
}

public struct LocalHotseatGame { }

public struct GameIsOver { }
