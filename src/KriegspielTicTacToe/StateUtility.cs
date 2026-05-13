namespace KriegspielTicTacToe;

using KriegspielTicTacToe.Model;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Loads and saves game state.
/// </summary>
internal static class StateUtility {
    /// <summary>
    /// Default file location for the game state.  You will have to delete this file manually if it should become corrupted.
    /// </summary>
    public static FileInfo DefaultStateFilePath {
        get => new FileInfo(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KriegspielTicTacToe.json"
            )
        );
    }
    
    public static TicTacToeState LoadState(string filePath) {
        using var sr = new StreamReader(filePath);
        return JsonConvert.DeserializeObject<TicTacToeState>(sr.ReadToEnd())!;
    }

    public static void SaveState(TicTacToeState state, string filePath) {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(fileStream);
        writer.Write(JsonConvert.SerializeObject(state));
    }
}
