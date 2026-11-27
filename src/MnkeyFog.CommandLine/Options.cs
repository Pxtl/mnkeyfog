using System.CommandLine;

namespace MnkeyFog.CommandLine;

/// <summary>
/// Command-line option definitions.
/// </summary>
internal static class Options {
    static Options() {
        StateFileOption.AcceptExistingOnly();
    }

    public static Option<FileInfo> StateFileOption = new("--file", "-f") {
        Description = "Path to the json file where gamestate is stored.  Will be resumed automatically if you kill the game (ctrl-C). "
            + "Use a fileshare for network multiplayer.",
        DefaultValueFactory = (argumentResult) => StateStorage.DefaultStateFilePath,
        Recursive = true,
    };

    public static Option<string> JoinAsPlayerOption = new("--join", "-j") {
        Description = "Join as given player char mark. Must match a mark in players list. Hotseat mode if not provided.",
        Recursive = true
    };

    public static Option<string[]> PlayersOption = new("--players", "-p") {
        Description = "Players mark characters.  Provide them space-separated, eg '-p A B C X Y Z' for a 6-player game.",
        DefaultValueFactory = (result) => ["X", "O"],
        CustomParser = ParsePlayerMarkArray,
        Recursive = true,
        AllowMultipleArgumentsPerToken = true
    };

    public static Option<string[]> AI1PlayersOption = CreatePlayerAIOption("1", "'Randy' AI (random, very weak)");
    public static Option<string[]> AI2PlayersOption = CreatePlayerAIOption("2", "'Clod' AI (created by llm, weak)");
    public static Option<string[]> AI3PlayersOption = CreatePlayerAIOption("3", "'Aster' AI (corrected llm algo, weak)");
    public static Option<string[]> AI4PlayersOption = CreatePlayerAIOption("4", "'Eager' AI (random unless it can score this turn, easy)");
    public static Option<string[]> AI5PlayersOption = CreatePlayerAIOption("5", "'Monty' AI (depth-3 monte carlo algo, moderate)");
    public static Option<string[]> AI6PlayersOption = CreatePlayerAIOption("6", "'Carla' AI (depth-4 monte carlo algo, difficult)");

    public static Option<bool> RandomOption = new("--random", "-r") {
        Description = "Randomize player order.",
        Recursive = true
    };

    public static Option<sbyte?> SizeOption = new("--size", "-z") {
        Description = "Board size.  Default is 3x3.",
        DefaultValueFactory = result => 3,
        CustomParser = result => {
            if (sbyte.TryParse(result.Tokens.Single().Value, out var size)) {
                if (2 <= size && size <= 30) {
                    return size;
                }
            }
            result.AddError("Size must be a number from 2 to 30");
            return null;
        }
    };

    public static Option<sbyte?> ScoringLengthOption = new Option<sbyte?>("--scoringlength", "-l") {
        Description = "How many spaces in a row you must take to score a point. "
            + "Leave blank to use the full board width.",
        DefaultValueFactory = result => null
    };

    public static Option<bool> IsBoardDoneWhenScoredOption = new("--boarddonewhenscored", "-d") {
        Description = "If set, the the board is done and closed to new moves when a line is scored."
    };

    public static Option<bool> IsKriegspiel = new("--kriegspiel", "-k") {
        Description = "If set, players can only see pieces they've placed or touched."
    };

    public static Option<sbyte?> BoardsNumberOption = new("--boards", "-b") {
        Description = "Number of boards.",
        DefaultValueFactory = result => 3,
        CustomParser = result => {
            if (sbyte.TryParse(result.Tokens.Single().Value, out var size)) {
                if (1 <= size && size <= 9) {
                    return size;
                }
            }
            result.AddError("Boards must be a number from 1 to 9");
            return null;
        }
    };

    public static Option<bool> SynchronousModeOption = new("--synchronous", "-y") {
        Description = "Moves do not execute until all players in a round have taken a turn. "
        + "If two players move to the same square, that square becomes an impasse marker visible to all."
    };

    private static string[]? ParsePlayerMarkArray(System.CommandLine.Parsing.ArgumentResult result) {
        if (result.Tokens.Count <= 1) {
            result.AddError("There must be at least 2 mark characters.");
            return null;
        } else if (result.Tokens.Any(t => t.Value.Length == 0 && t.Value.Length > 1)) {
            result.AddError("Player mark characters must be 1 chars long.");
            return null;
        } else if (result.Tokens.Select(t => t.Value).Distinct().Count() != result.Tokens.Count) {
            result.AddError("Player mark characters must be distinct from each other.");
            return null;
        } else if (result.Tokens.Any(t => t.Value.Length == 1 && char.IsDigit(t.Value[0]))) {
            result.AddError("Player mark characters cannot be numeric (0-9).");
            return null;
        } else {
            return result.Tokens.Select(t => t.Value).ToArray();
        }
    }

    private static Option<string[]> CreatePlayerAIOption(string optionChar, string description)
    => new($"--ai{optionChar}", $"-{optionChar}") {
        Description = $"Difficulty {optionChar} {description} player mark characters.  Provide them space-separated like with players.",
        DefaultValueFactory = (result) => [],
        CustomParser = ParsePlayerMarkArray,
        Recursive = true,
        AllowMultipleArgumentsPerToken = true
    };
}
