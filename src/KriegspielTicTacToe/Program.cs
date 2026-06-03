namespace KriegspielTicTacToe;

using System.CommandLine;
using System.CommandLine.Invocation;
using OneOf;

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
            Options.ScoringLengthOption,
            Options.IsBoardDoneWhenScoredOption,
            Options.BoardsNumberOption,
            Options.JoinAsPlayerOption,
            Options.SynchronousModeOption
        };

        rootCommand.SetAction((parseResult) => {
                var file = parseResult.GetValue(Options.StateFileOption)!;
                var doForceNewGame = parseResult.GetValue(Options.ForceNewGameOption);
                var players = parseResult.GetValue(Options.PlayersOption)!;
                var size = parseResult.GetValue(Options.SizeOption);
                var boardsNumber = parseResult.GetValue(Options.BoardsNumberOption);
                var scoringLength = parseResult.GetValue(Options.ScoringLengthOption);
                var isBoardDoneWhenScored = parseResult.GetValue(Options.IsBoardDoneWhenScoredOption);
                var joinAsPlayer = parseResult.GetValue(Options.JoinAsPlayerOption);

                var isRandomPlayer = parseResult.GetValue(Options.RandomOption);
                var isSynchronousMode = parseResult.GetValue(Options.SynchronousModeOption);

                var boardBuilders = new Model.BoardBuilder[boardsNumber!.Value];

                var playerList = new List<Model.Player>();
                foreach(var player in players!) {
                    playerList.Add(new Model.Player(player));
                }
                var playerArray = playerList.ToArray();

                var joinAsPlayerUnion = joinAsPlayer == null
                    ? OneOf<Model.Player, LocalHotseatGame>.FromT1(new LocalHotseatGame())
                    : OneOf<Model.Player, LocalHotseatGame>.FromT0(new Model.Player(joinAsPlayer));

                for(var i = 0; i < boardsNumber!; i+=1) {
                    boardBuilders[i] = new Model.BoardBuilder(size!.Value, size!.Value, scoringLength, isBoardDoneWhenScored);
                }
                
                GameLogic.RunGame (
                    file,
                    doForceNewGame,
                    playerArray,
                    boardBuilders,
                    joinAsPlayerUnion,
                    isRandomPlayerOrder: isRandomPlayer,
                    isSynchronousMode: isSynchronousMode
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
