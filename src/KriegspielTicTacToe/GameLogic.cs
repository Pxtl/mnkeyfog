namespace KriegspielTicTacToe;

using KriegspielTicTacToe.Model;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Game logic implementation.
/// </summary>
internal static class GameLogic {
    public static void RunGame(FileInfo sharedStateFilePath, bool doForceNewGame, char[] players, bool isRandomPlayer, IEnumerable<BoardBuilder> boardBuilders, char? joinAsPlayer) {
        TicTacToeState state;
        if(sharedStateFilePath.Exists && !doForceNewGame) {
            Console.Out.WriteLine($"Loaded saved game!");
            state = StateUtility.LoadState(sharedStateFilePath.FullName);
        } else {
            Console.Out.WriteLine($"Starting new game!");
            state = new TicTacToeState(players, isRandomPlayer, boardBuilders);
            StateUtility.SaveState(state, sharedStateFilePath.FullName);
        }

        if(joinAsPlayer.HasValue) {
            Console.Out.WriteLine($"Joining game-file '{sharedStateFilePath.FullName}' as player '{joinAsPlayer.Value}'.");
        } else {
            Console.Out.WriteLine($"Running in hotseat mode.");
        }
        
        Console.Out.WriteLine($"Players are {string.Join(", ", state.Players)}.");
        
        bool isDone = false;
        var doNextTurn = true;
        while(!isDone) {
            if(doNextTurn) {
                if(joinAsPlayer.HasValue) {
                    if(!state.Players.Contains(joinAsPlayer.Value)) {
                        throw new ApplicationException($"Invalid player join, player {joinAsPlayer.Value} is not a player in this game.");
                    }
                    bool isDoneWaiting = false;
                    Console.Out.Write("Waiting for your turn.");

                    while(!isDoneWaiting) {
                        state = StateUtility.LoadState(sharedStateFilePath.FullName);
                        if(joinAsPlayer.Value == state.CurrentTurnPlayer || state.IsGameOver) {
                            isDoneWaiting = true;
                        } else {
                            Console.Out.Write(".");
                            Thread.Sleep(100);
                        }
                    }
                    Console.Out.WriteLine();
                    if(state.IsGameOver) {
                        isDone = true;
                    } else {
                        Console.Out.WriteLine($"Player {joinAsPlayer.Value} it is your turn.");
                    }
                } else {
                    Console.Out.WriteLine($"Player {state.CurrentTurnPlayer} ready?  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    Console.WriteLine();
                }
            }
            var playerToRender = state.CurrentTurnPlayer;
            
            if (!isDone) {
                var activeBoardIndex = state.SingleActiveBoardIndex;
                if (!activeBoardIndex.HasValue) {
                    BoardRenderer.DrawBoards(state, playerToRender, activeBoardIndex);
                    var boardCommand = InputUtility.ReadCommandKeys("Press numeric key(s) to pick a board, or 'r' to resign.", 1);
                    boardCommand.Switch (
                        charCode => {
                            doNextTurn = true;
                            state.ResignPlayer(state.CurrentTurnPlayer);
                        },
                        boardCode => state.SelectBoard(boardCode).Switch (
                            notFound => {
                                doNextTurn = false;
                                Console.WriteLine($"That is not a valid board.  Please pick an incomplete board from 1 to {state.Boards.Count}.");
                            },
                            boardIsDone => {
                                doNextTurn = false;
                                Console.WriteLine($"That board is already complete.  Please pick a board that has a number on it.");
                            },
                            result => {
                                activeBoardIndex = result.Value;
                            }
                        ),
                        unknown => {
                            doNextTurn = false;
                        }
                    );
                }

                if(activeBoardIndex.HasValue) {
                    var boardIndex = activeBoardIndex.Value;
                    BoardRenderer.DrawBoards(state, playerToRender, boardIndex);
                    var spaceCommand = InputUtility.ReadCommandKeys("Press numeric key(s) to play a space, or 'r' to resign.", state.Boards[boardIndex].SpaceIndexCodeLength);
                    spaceCommand.Switch (
                        charCode => {
                            doNextTurn = true;
                            state.ResignPlayer (state.CurrentTurnPlayer);
                        },
                        spaceCode => {
                            state.PlaySpace(boardIndex, spaceCode).Switch(
                                success => {
                                    doNextTurn = true;
                                    Console.WriteLine($"Played on board {boardIndex + 1}, space {spaceCode}.");
                                }, result => {
                                    doNextTurn = true;
                                    Console.WriteLine($"You've just discovered that this space is already taken by player '{result.Value}'.");
                                }, alreadyPlayed => {
                                    doNextTurn = false;
                                    Console.Out.WriteLine($"Invalid square, that square is already known to player {state.CurrentTurnPlayer}");
                                }, notFound => {
                                    doNextTurn = false;
                                    Console.WriteLine($"That is not a valid square on the board.");
                                }
                            );
                        },
                        unknown => {
                            doNextTurn = false;
                        }
                    );
                }
            }
            
            if (doNextTurn) {
                if (!state.IsResignedPlayer (playerToRender)) {
                    state.NextTurn();
                }

                if (!joinAsPlayer.HasValue || playerToRender == joinAsPlayer.Value) {
                    StateUtility.SaveState(state, sharedStateFilePath.FullName);
                }

                BoardRenderer.DrawBoards(state, playerToRender, activeBoardIndex:null);
                
                if (!state.IsGameOver) {
                    if (!joinAsPlayer.HasValue) {
                        Console.Out.WriteLine($"Press any key to continue...");
                        Console.ReadKey(intercept: true);
                        Console.Clear();
                    }
                } else {
                    Console.Out.WriteLine(state.GameStateText);
                    isDone = true;
                }
            }
        }
        
        Console.Out.WriteLine("Game over.");
        Thread.Sleep(1000);
        sharedStateFilePath.Delete();
    }
}
