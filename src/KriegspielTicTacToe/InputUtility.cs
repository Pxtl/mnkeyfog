namespace KriegspielTicTacToe;

using System;
using OneOf;
using OneOf.Types;

/// <summary>
/// Reads command keys from keyboard.
/// </summary>
internal static class InputUtility {
    public static OneOf<char, int, Unknown> ReadCommandKeys(string prompt, int codeLength)
        => ReadCommandKeys(prompt, codeLength, new[]{'r'});

    public static OneOf<char, int, Unknown> ReadCommandKeys(string prompt, int codeLength, char[] validCommandChars) {
        if (validCommandChars == null) {
            validCommandChars = new[]{'r'};
        }
        Console.Out.WriteLine(prompt);
        
        var sb = new System.Text.StringBuilder();
        var numericCode = -1;
        var isNumeric = false;
        do {
            var key = Console.ReadKey();
            if(key.KeyChar >= '0' && key.KeyChar <= '9') {
                sb.Append(key.KeyChar);
            } else if(validCommandChars.Contains(key.KeyChar)){
                Console.Out.WriteLine();
                return key.KeyChar;
            } else {
                Console.Out.WriteLine();
                Console.Out.WriteLine("Invalid command.");
                return new Unknown();
            }
            isNumeric = int.TryParse(sb.ToString(), out numericCode);
        } while (sb.Length < codeLength && isNumeric);
        Console.Out.WriteLine();
        return numericCode;
    }
}
