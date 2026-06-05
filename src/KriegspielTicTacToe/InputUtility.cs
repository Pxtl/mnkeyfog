namespace KriegspielTicTacToe;

using OneOf;
using OneOf.Types;
using Sundew.Base;

/// <summary>
/// Reads command keys from keyboard.
/// </summary>
internal static class InputUtility {
    public static void PauseAndPressAnyKey(string? prompt = null) {
        if(prompt != null && !prompt.EndsWith(' ')) {
            prompt = $"{prompt} ";
        }
        if (Console.IsInputRedirected) {
            Console.Out.WriteLine($"{prompt}Press enter to continue...");
            Console.ReadLine();
        } else {
            Console.Out.WriteLine($"{prompt}Press any key to continue...");
            Console.ReadKey(intercept: true);
        }
    }

    /// <summary>
    /// Works like <see cref="ReadCommandInput"/> but with the standard "r" for
    /// retire command added.
    /// </summary>
    /// <returns>
    /// The chosen command, in the casing listed in the `validCommands` list as
    /// a <see cref="Result{T}"/>, or <see cref="Unknown"/>.
    /// </returns>
    public static OneOf<Result<string>, Unknown> ReadCommandInputWithAddedStandardPlayerCommands(string prompt, IEnumerable<string> validCommands)
        => ReadCommandInput(prompt, validCommands.Union(["r"]));

    /// <summary>
    /// Works like <see cref="ReadCommandInput"/> but loop if an invalid command is provided.
    /// </summary>
    /// <returns>
    /// The chosen command, in the casing listed in the `validCommands` list as
    /// a <see cref="Result{T}"/>, or <see cref="Unknown"/>.
    /// </returns>
    public static string ReadCommandInputLoop(string prompt, IEnumerable<string> validCommands) {
        while(true) {
            string? loopResult = null;
            ReadCommandInput(prompt, validCommands).Switch(
                result => {
                    loopResult = result.Value;
                },
                unknown => {
                    // do nothing, the ReadCommandInput already printed "Invalid Command"
                }
            );
            if(loopResult.HasValue) {
                return loopResult;
            }
        }
    }

    /// <summary>
    /// Prompt the user to enter a command.  Undefined behaviour if commands are
    /// substrings of each other.  Uses ReadKey if input is not redirected (so
    /// user does not need to press "enter" the command is locked in as soon as
    /// they type it, but no backspace support).  Uses ReadLine if necessary.
    /// </summary>
    /// <returns>
    /// The chosen command, in the casing listed in the `validCommands` list as
    /// a <see cref="Result{T}"/>, or <see cref="Unknown"/>.
    /// </returns>
    public static OneOf<Result<string>, Unknown> ReadCommandInput(string prompt, IEnumerable<string> validCommands) {
        // using dictionary instead of set so we can get canonical casing for the command.
        var commandSet = validCommands.ToDictionary(s => s, StringComparer.OrdinalIgnoreCase);
        Console.Out.WriteLine(prompt);

        if (Console.IsInputRedirected) {
            // if we lack access to ReadKey
            var inputStr = Console.ReadLine()!.Trim();
            if (commandSet.TryGetValue(inputStr, out var foundCommandStr)) {
                Console.Out.WriteLine();
                return new Result<string>(foundCommandStr);
            } else {
                Console.Out.WriteLine();
                Console.Out.WriteLine("Invalid command.");
                return new Unknown();
            }
        } else {
            // if we have access to ReadKey
            var maxCommandLength = validCommands.Max(str => str.Length);
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < maxCommandLength; i += 1) {
                var key = Console.ReadKey();
                sb.Append(key.KeyChar);
                var currentBuiltCommand = sb.ToString().Trim();
                if (commandSet.TryGetValue(currentBuiltCommand, out var foundCommandStr)) {
                    Console.Out.WriteLine();
                    return new Result<string>(foundCommandStr);
                }
            }
            //user has reached length of longest command, no matches.
            Console.Out.WriteLine();
            Console.Out.WriteLine("Invalid command.");
            return new Unknown();   
        }
    }
}
