namespace KriegspielTicTacToe;

using System.CommandLine;

/// <summary>
/// Command-line option definitions.
/// </summary>
internal static class Options {
    public static Option<FileInfo> StateFileOption = new ("--file", "-f") {
        Description = "Path to the json file where gamestate is stored.  Will be resumed automatically if you kill the game (ctrl-C). " 
            + "Use a fileshare for network multiplayer.",
        DefaultValueFactory = (ArgumentResult) => StateStorage.DefaultStateFilePath
    };

    public static Option<bool> ForceNewGameOption = new ("--force", "-F") {
        Description = "Force a new game instead of loading the game at the gamestate file.  Will replace gamestate file."
    };

    public static Option<string[]> PlayersOption = new("--players", "-p") {
        Description = "Players mark characters.  Provide them space-separated, eg '-p A B C X Y Z' for a 6-player game.",
        DefaultValueFactory = (result) => ["X", "O"],
        CustomParser = result => {
            if (result.Tokens.Count <= 1) {
                result.AddError("There must be at least 2 mark characters.");
                return null;
            } else if (result.Tokens.Any(t => t.Value.Length == 0 && t.Value.Length > 1)) {
                result.AddError("Player mark characters must be 1 chars long.");
                return null;
            } else if(result.Tokens.Select(t => t.Value).Distinct().Count() != result.Tokens.Count) {
                result.AddError("Player mark characters must be distinct from each other.");
                return null;
            } else if(result.Tokens.Any(t => short.TryParse(t.Value, out short _))) {
                result.AddError("Player mark characters must not be numeric.");
                return null;
            } else {
                return result.Tokens.Select(t => t.Value).ToArray();
            }
        },
        AllowMultipleArgumentsPerToken = true
    };

    public static Option<bool> RandomOption = new("--random", "-r") {
        Description = "Randomize player order."
    };

    public static Option<sbyte?> SizeOption = new("--size", "-z") {
        Description = "Board size.  Default is 3x3.",
        DefaultValueFactory = result => 3,
        CustomParser = result => {
            if(sbyte.TryParse(result.Tokens.Single().Value, out sbyte size)) {
                if(2<=size && size <= 10) {
                    return size;
                }
            }
            result.AddError("Size must be a number from 2 to 10");
            return null;
        }
    };

    public static Option<sbyte?> ScoringLengthOption = new Option<sbyte?>("--scoringlength", "-l") {
        Description = "How many spaces in a row you must take to score a point. "
            + "Leave blank to use the full board width.",
        DefaultValueFactory = result => null
    };

    public static Option<bool> IsBoardDoneWhenScoredOption = new ("--boarddonewhenscored", "-d") {
        Description = "If set, the the board is done and closed to new moves when a line is scored."
    };

    public static Option<sbyte?> BoardsNumberOption = new ("--boards", "-b") {
        Description = "Number of boards.",
        DefaultValueFactory = result => 3,
        CustomParser = result => {
            if(sbyte.TryParse(result.Tokens.Single().Value, out sbyte size)) {
                if(2<=size && size <= 9) {
                    return size;
                }
            }
            result.AddError("Boards must be a number from 1 to 9");
            return null;
        }
    };

    public static Option<string> JoinAsPlayerOption = new ("--join", "-j") {
        Description = "Join as given player char mark. Must match a mark in players list. Hotseat mode if not provided."
    };

    public static Option<bool> SynchronousModeOption = new ("--synchronous", "-y") {
        Description = "Moves do not execute until all players in a round have taken a turn. " 
        + "If two players move to the same square, that square becomes an impasse marker visible to all."
    };
}
