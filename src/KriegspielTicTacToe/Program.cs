using System.CommandLine;
using System.CommandLine.Invocation;
using OneOf;
using KriegspielTicTacToe.Model.Template;
using System.CommandLine.Parsing;
using Sundew.Base.Collections;
using KriegspielTicTacToe.Model.PlayerAIs;

namespace KriegspielTicTacToe;

/// <summary>
/// Application entry point.
/// </summary>
public class Program {
    static FileInfo? StateFilePath { get; set; }
    static string? JoinAsPlayer { get; set; }
    static GameState? GameState { get; set; } = null;
    static OrderedDictionary<Player, IPlayerAI> AIPlayers = new OrderedDictionary<Player, IPlayerAI>();

    public static int Main(string[] args) {
        var gameCommand = new Command("game", "Start a new game using a pre-defined game template.") {
            Options = { //these are recursive options that will be inherited by subcommands.
                Options.PlayersOption,
                Options.RandomOption,
            }
        };

        var rootCommand = new RootCommand("This is a command-line game that implements kriegspiel m,n,k-games such as Zach Weinersmith's 'Kriegspiel Tic Tac Toe'") {
            Options = { //these are recursive options that will be inherited by subcommands.
                Options.StateFileOption,
                Options.JoinAsPlayerOption,
                Options.AI1PlayersOption,
                Options.AI2PlayersOption
            }, // rootCommand has no Action.  This makes all child commands required.  Be nice if that was documented somewhere.
            Subcommands = {
                gameCommand,
                new Command("custom") {
                    Options = {
                        Options.PlayersOption,
                        Options.RandomOption,

                        Options.SizeOption,
                        Options.BoardsNumberOption,
                        Options.ScoringLengthOption,
                        Options.IsBoardDoneWhenScoredOption,
                        Options.IsKriegspiel,
                        Options.SynchronousModeOption
                    },

                    Action = new CommandHandler((parseResult) => {
                        ParseRootOptions(parseResult); 
                        ParsePlayerListOptions(parseResult, out var players, out bool isRandomPlayerOrder);

                        var size = parseResult.GetValue(Options.SizeOption);
                        var boardsNumber = parseResult.GetValue(Options.BoardsNumberOption);
                        var scoringLength = parseResult.GetValue(Options.ScoringLengthOption);
                        var isBoardDoneWhenScored = parseResult.GetValue(Options.IsBoardDoneWhenScoredOption);
                        var isKriegspiel = parseResult.GetValue(Options.IsKriegspiel);
                        var isSynchronousMode = parseResult.GetValue(Options.SynchronousModeOption);

                        var boardBuilders = new BoardBuilder[boardsNumber!.Value];
                        for(var i = 0; i < boardsNumber!; i+=1) {
                            boardBuilders[i] = new BoardBuilder(
                                size!.Value,
                                size!.Value,
                                new MNKBoardRuleset(scoringLength, isBoardDoneWhenScored)
                            );
                        }

                        GameState = new GameState(
                            players,
                            new CustomMNKTemplate(boardBuilders, isKriegspiel: isKriegspiel, isSynchronousMode: isSynchronousMode),
                            isRandomPlayerOrder: isRandomPlayerOrder
                        );
                        return parseResult.Errors.Count;
                    }), 
                },
                new Command("load") {
                    Action = new CommandHandler(parseResult => {
                        ParseRootOptions(parseResult);
                        if(StateFilePath!.Exists) {
                            GameState = StateStorage.LoadState(StateFilePath.FullName);
                            Console.Out.WriteLine($"Loaded saved game!");
                            return parseResult.Errors.Count;
                        } else {
                            Console.Error.WriteLine($"Cannot load game, file '{StateFilePath.FullName}' not found.");
                            return -1;
                        }
                    })
                }
            }
        };

        gameCommand.Subcommands.AddRange(
            GameTemplates.GetBuiltInGameTemplates().Select(template => new Command(template.CommandName!, template.Description) {
                    Action = new CommandHandler(parseResult => {
                        ParseRootOptions(parseResult); 
                        ParsePlayerListOptions(parseResult, out var players, out bool isRandomPlayerOrder);
                        GameState = new GameState(players, template, isRandomPlayerOrder);
                        return parseResult.Errors.Count;
                    })
                }
            )
        );

        var parseResult = rootCommand.Parse(
            args, 
            new ParserConfiguration() {EnablePosixBundling = true}
        );
        var invocationResult = parseResult.Invoke();

        if (parseResult.Action is System.CommandLine.Help.HelpAction) {
            return 0;
        }

        if(invocationResult != 0) {
            return invocationResult;
        }

        var joinAsPlayerUnion = JoinAsPlayer == null
            ? OneOf<Player, LocalHotseatGame>.FromT1(new LocalHotseatGame())
            : OneOf<Player, LocalHotseatGame>.FromT0(new Player(JoinAsPlayer));
            
        if (StateFilePath != null && GameState != null) {
            ConsoleLoop.RunGame (
                StateFilePath!,
                GameState!,
                joinAsPlayerUnion,
                AIPlayers
            );
            return 0;
        } else {
            Console.Error.WriteLine("No Gamestate or State File Path available.  Aborting...");
            return -1;
        }
    }

	private static void ParsePlayerListOptions(ParseResult parseResult, out Player[] players, out bool isRandomPlayerOrder)
	{
		players = parseResult
            .GetValue(Options.PlayersOption)!
            .Select(p => new Player(p))
            .ToArray();
		isRandomPlayerOrder = parseResult.GetValue(Options.RandomOption);
	}

	private static void ParseRootOptions(ParseResult parseResult) {
        StateFilePath = parseResult.GetValue(Options.StateFileOption);
        JoinAsPlayer = parseResult.GetValue(Options.JoinAsPlayerOption);
        var ai1Players = parseResult.GetValue(Options.AI1PlayersOption) ?? [];
        var ai2Players = parseResult.GetValue(Options.AI2PlayersOption) ?? [];
        AIPlayers.AddRange(ai1Players.Select(a => new KeyValuePair<Player, IPlayerAI>(new Player(a), new RandomAI())));
        AIPlayers.AddRange(ai2Players.Select(a => new KeyValuePair<Player, IPlayerAI>(new Player(a), new ClodAI())));
    }

    private class CommandHandler(Func<ParseResult, int> action) : SynchronousCommandLineAction {
        public override int Invoke(ParseResult parseResult) => action(parseResult);
    }
}
