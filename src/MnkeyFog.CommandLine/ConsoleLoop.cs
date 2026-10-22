using MnkeyFog.Model.PlayerAIs;
using MnkeyFog.Model.Template;
using MnkeyFog.Text;
using OneOf;
using OneOf.Types;
using PxtlCa.SystemCollectionsExtensions;

namespace MnkeyFog.CommandLine;

/// <summary>
/// Game logic implementation.
/// </summary>
internal static class ConsoleLoop {
    public static void RunGame(
        FileInfo sharedStateFilePath,
        GameState state,
        OneOf<PlayerInfo, LocalHotseatGame> joinAsPlayer,
        OrderedDictionary<PlayerInfo, IPlayerAI> aiPlayers
    ) {
        StateStorage.SaveState(state, sharedStateFilePath.FullName);

        Console.Out.WriteLine(joinAsPlayer.Match(
            player => $"Joining game-file '{sharedStateFilePath.FullName}' as player '{player}'.",
            localHotseatGame => "Running in hotseat mode."
        ));

        Console.Out.WriteLine($"Players are {string.Join(", ", state.PlayersState.PlayerInfos)}.");
        Console.Out.WriteLine($"Game is {state.GameTemplate.CommandName}.");
        Console.Out.WriteLine($"Description: {state.GameTemplate.Description}");
        foreach (var aiPlayer in aiPlayers) {
            Console.Out.WriteLine($"AI Player {aiPlayer.Key.Mark}: {aiPlayer.Value.Description}.");
        }

        bool isGameOver = false;
        while (!isGameOver) {
            // AI players take their turns.
            var isDoneAITurns = false;
            while (!isDoneAITurns) {
                isDoneAITurns = true;
                foreach (var player in state.PlayersState.PlayersAvailableForTurn) {
                    if (aiPlayers.TryGetValue(player.Info, out var playerAI)) {
                        //if any AI player can take their turn, we're not done
                        //AI turns.  Keep attempting until no AI player does a
                        //turn.
                        isDoneAITurns = false;
                        var attemptCount = 0;
                        while (state.PlayersState.CanTakeTurn(player.Index)) {
                            using (var stateStorage = new StateStorage(sharedStateFilePath.FullName, out state)) {
                                var gameView = state.GetView(player.Index);
                                if (attemptCount > AIGameRunner.MaxPlayerAIAttemptCount) {
                                    // resign if the player AI can't figure out a legal move.
                                    gameView.ResignPlayer();
                                } else {
                                    attemptCount += 1;
                                    playerAI.Attempt(gameView);
                                }
                            }
                        }
                        Console.Out.WriteLine($"AI Player {player.Info} has finished their turn.");
                    }
                }
            }

            var currentPlayerChosen = joinAsPlayer.Match(
                playerInfo => {
                    if (!state.PlayersState.PlayerInfos.Contains(playerInfo)) {
                        throw new ApplicationException($"Invalid player join, player {playerInfo} is not a player in this game.");
                    }
                    var player = state.PlayersState.GetPlayer(playerInfo);
                    bool isDoneWaiting = false;
                    Console.Out.Write("Waiting for your turn.");

                    //wait loop.
                    while (!isDoneWaiting) {
                        state = StateStorage.LoadState(sharedStateFilePath.FullName);
                        if (state.PlayersState.PlayersAvailableForTurn.Contains(player)
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
                    return state.IsGameOver
                        ? OneOf<Result<Player>, RoundIsOver, GameIsOver>.FromT2(new GameIsOver())
                        : new Result<Player>(player);
                },
                localHotseatGame => DoPlayerChooserLoop(state.PlayersState)
            );

            currentPlayerChosen.Switch(
                playerResult => {
                    var currentPlayer = playerResult.Value;
                    DoPlayerTurnLoop(ref state, currentPlayer, sharedStateFilePath.FullName);
                },
                roundIsOver => {
                    //no-op.
                },
                gameIsOver => {
                    isGameOver = true;
                }
            );
            //execute round-end stuff.
            if (state.PlayersState.IsRoundOver) {
                var hasRoundStateChanged = false;
                using (var stateStorage = new StateStorage(sharedStateFilePath.FullName, out state)) {
                    state.EndRound(out hasRoundStateChanged);
                }
                if (hasRoundStateChanged) {
                    InputUtility.PauseAndPressAnyKey("Round over.");
                    Console.Clear();
                    Console.Out.WriteLine(state.GameStateText);
                    Console.Out.WriteLine("Executing synchronous moves.");
                }
            }
            if (!state.IsGameOver) {
                joinAsPlayer.Switch(
                    player => { },
                    localHotseatGame => {
                        InputUtility.PauseAndPressAnyKey();
                        Console.Clear();
                    }
                );
            } else {
                Console.Out.WriteLine(state.GameStateText);
                var boardRenderer = new BoardRenderer();
                Console.Out.WriteLine(boardRenderer.DrawBoards(state.GetSpectatorView(), maxRenderWidth: Console.BufferWidth));
                isGameOver = true;
            }
        }

        Thread.Sleep(1000);
        sharedStateFilePath.Delete();
    }

    private static void DoPlayerTurnLoop(ref GameState state, Player player, string sharedStateFilePath) {
        IPlayActionResult? playActionResult = null;
        while (playActionResult == null || !playActionResult.IsTurnDone) {
            Console.Out.WriteLine(state.GameStateText);
            Console.Out.WriteLine($"Player {player.Info}, take your turn.");
            var gameView = state.GetView(player);
            var boardRenderer = new BoardRenderer();
            Console.Out.WriteLine(boardRenderer.DrawBoards(gameView, maxRenderWidth: Console.BufferWidth));
            playActionResult = DoPlayerAction(ref state, player, sharedStateFilePath);
            Console.Out.WriteLine(playActionResult.GetResultText(state.PlayersState));
        }
        var isViewChanged = playActionResult.IsViewChanged;
        if (isViewChanged) {
            var boardRenderer = new BoardRenderer();
            Console.Out.WriteLine(boardRenderer.DrawBoards(state.GetView(player), maxRenderWidth: Console.BufferWidth));
        }
    }

    private static IPlayActionResult DoPlayerAction(ref GameState state, Player player, string sharedStateFilePath) {
        var gameView = state.GetView(player);
        var actionFactories = gameView.AvailableActions; ;

        if (actionFactories.Count() == 1) {
            var actionFactory = actionFactories.Single();

            if (actionFactory is GameActionFactoryForBoard actionFactoryForBoard) {
                return DoBoardSelection(ref state, player, sharedStateFilePath, actionFactoryForBoard);
            } else if (actionFactory is GameActionFactoryForSimple actionFactoryForSimple) {
                using (var stateStorage = new StateStorage(sharedStateFilePath, out state)) {
                    return actionFactoryForSimple.Create().Attempt(state, player);
                }
            } else if (actionFactory is GameActionFactoryForSpace actionFactoryForSpace) {
                return DoSpaceSelection(ref state, player, sharedStateFilePath, actionFactoryForSpace);
            } else {
                throw new InvalidOperationException("Unknown or unsupported Action Factory.");
            }
        } else {
            //TODO: ActionFactories picker.
            throw new NotImplementedException("Multiple Action Factories is not supported yet.");
        }
    }

    private static IPlayActionResult DoSpaceSelection(ref GameState state, Player player, string sharedStateFilePath, GameActionFactoryForSpace actionFactory) {
        var gameView = state.GetView(player);
        var spaceCommand = InputUtility.ReadCommandInputWithAddedStandardPlayerCommands(
                "Press numeric key(s) to play a space, or 'r' to resign, or 'q' to save game and quit.",
                gameView.SpaceNames
        );
        using (var stateStorage = new StateStorage(sharedStateFilePath, out state)) {
            gameView = state.GetView(player);
            var gameViewForSwitch = gameView; //workaround for can't use refs in lambdas.
            return spaceCommand.Match(
                result => {
                    if ("r".Equals(result.Value, StringComparison.OrdinalIgnoreCase)) {
                        return gameViewForSwitch.ResignPlayer();
                    } else if ("q".Equals(result.Value, StringComparison.OrdinalIgnoreCase)) {
                        return Quit();
                    } else if (gameView.TryGetCoordinatesFromSpaceName(result.Value, out sbyte boardIndex, out var col, out var row)) {
                        return actionFactory.Create(boardIndex, col, row).Attempt(stateStorage.State, player);
                    } else {
                        return new InvalidCommand(result.Value);
                    }
                },
                invalidCommand => {
                    return invalidCommand;
                }
            );
        }
    }

    internal static OneOf<Result<Player>, RoundIsOver, GameIsOver> DoPlayerChooserLoop(PlayersState playerState) {
        // Use ModelToKeyUtility for clean, testable key mapping
        var playerIndexToCommand = CommandNameTool.BuildPlayerToCommandNameMap(playerState.PlayersAvailableForTurn);

        var commandToPlayerIndex = playerIndexToCommand
            .ToOrderedDictionary(
                p => p.Value,
                p => p.Key,
                StringComparer.OrdinalIgnoreCase
            );

        while (true) {
            if (playerState.PlayersAvailableForTurn.Count() == 1) {
                var currentPlayer = playerState.PlayersAvailableForTurn.Single();
                InputUtility.PauseAndPressAnyKey(prompt: $"Player {currentPlayer} ready?");
                Console.WriteLine();
                return new Result<Player>(currentPlayer);
            }
            if (playerState.IsRoundOver) {
                return new RoundIsOver();
            }

            Console.Out.WriteLine(playerState.GameStateText);

            // Display all available players with alternate key hints for non-typeable marks
            var playerDisplayList = playerState.PlayersAvailableForTurn
                .Select(p => {
                    var altKey = playerIndexToCommand[p.Index];
                    var keyDisplay = altKey.Equals(p.Info.Mark, StringComparison.OrdinalIgnoreCase)
                        ? ""
                        : $" ({altKey})";
                    return $"Player {p.Info.Mark}{keyDisplay}";
                });

            var prompt = "Who will take the next turn? Press the player's key to take their turn (or press 'q' to quit the game for everyone)."
                + Environment.NewLine
                + string.Join(" ", playerDisplayList);
            var validCommands = ((IEnumerable<string>)["q"]).Concat(commandToPlayerIndex.Keys);
            var commandResult = InputUtility.ReadCommandInputLoop(prompt, validCommands);

            if ("q".Equals(commandResult, StringComparison.OrdinalIgnoreCase)) {
                Quit();
            } else {
                return new Result<Player>(playerState.GetPlayer(commandToPlayerIndex[commandResult]));
            }
        }
    }

    internal static IPlayActionResult DoBoardSelection(ref GameState state, Player player, string sharedStateFilePath, GameActionFactoryForBoard actionFactory) {
        if (state.SingleActiveBoardIndex.HasValue) {
            var boardIndex = state.SingleActiveBoardIndex.Value;
            using (var stateStorage = new StateStorage(sharedStateFilePath, out state)) {
                return actionFactory.Create(boardIndex).Attempt(state, player.Index);
            }
        } else {
            var gameView = state.GetView(player);
            var availableBoardCommands = gameView.BoardNames;
            var boardCommand = InputUtility.ReadCommandInputWithAddedStandardPlayerCommands(
                "Press numeric key(s) to pick a board, 'r' to resign, or 'q' to save game and quit.",
                availableBoardCommands
            );
            using (var stateStorage = new StateStorage(sharedStateFilePath, out state)) {
                gameView = state.GetView(player);
                return boardCommand.Match(
                    result => {
                        if ("r".Equals(result.Value, StringComparison.OrdinalIgnoreCase)) {
                            gameView = stateStorage.State.GetView(player);
                            return gameView.ResignPlayer();
                        } else if ("q".Equals(result.Value, StringComparison.OrdinalIgnoreCase)) {
                            Quit();
                            return new Quitting();
                        } else {
                            return gameView.AttemptBoard(result.Value).Match(
                                boardViewResult
                                => actionFactory.Create(boardViewResult.Value.BoardIndex).Attempt(stateStorage.State, player),
                                boardIsDone => boardIsDone,
                                invalidCommand => invalidCommand
                            );
                        }
                    },
                    invalidCommand => {
                        return invalidCommand;
                    }
                );
            }
        }
    }

    private static Quitting Quit() {
        Console.WriteLine("Quitting.  Use 'load' to resume later.");
        Environment.Exit(0);
        return new Quitting();
    }
}

public struct LocalHotseatGame { }

public struct GameIsOver { }

public struct RoundIsOver { }
