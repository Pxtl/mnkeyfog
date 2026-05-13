namespace KriegspielTicTacToe;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;

/// <summary>
/// Application entry point.
/// </summary>
class Program {
    public static int Main(string[] args) {
        var rootCommand = new RootCommand("This is a simple command-line implementation of Zach Weinersmith's proposed game 'Kriegspiel Tic Tac Toe'"){
            Options.StateFileOption,
            Options.ForceNewGameOption,
            Options.PlayersOption,
            Options.RandomOption,
            Options.SizeOption,
            Options.BoardsNumberOption,
            Options.JoinAsPlayerOption
        };

        rootCommand.SetAction((parseResult) => {
                var file = parseResult.GetValue(Options.StateFileOption)!;  // has non-null default value.
                var doForceNewGame = parseResult.GetValue(Options.ForceNewGameOption);
                var players = parseResult.GetValue(Options.PlayersOption)!; // has non-null default value.
                var isRandomPlayer = parseResult.GetValue(Options.RandomOption);
                var size = parseResult.GetValue(Options.SizeOption);
                var boardsNumber = parseResult.GetValue(Options.BoardsNumberOption);
                var joinAsPlayer = parseResult.GetValue(Options.JoinAsPlayerOption);

                var boardBuilders = new Model.BoardBuilder[boardsNumber!.Value];

                for(var i = 0; i < boardsNumber!; i+=1) {
                    boardBuilders[i] = new Model.BoardBuilder(size!.Value, size!.Value);
                }
                GameLogic.RunGame (
                    file,
                    doForceNewGame,
                    players.Select(p => p.Single()).ToArray(),
                    isRandomPlayer,
                    boardBuilders,
                    (joinAsPlayer ?? "").Cast<char?>().SingleOrDefault()
                );
            }
        );

        return rootCommand.Parse(
            args, 
            new ParserConfiguration() {EnablePosixBundling = true}
        )
            .Invoke();
    }
}
