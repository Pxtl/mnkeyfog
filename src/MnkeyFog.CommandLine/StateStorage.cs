using Newtonsoft.Json;

namespace MnkeyFog.CommandLine;

/// <summary>
/// Loads and saves game state. Can be used with static load/save or as instance
/// using-clause for quick load/save cycles. TODO: File locking.
/// </summary>
public class StateStorage : IDisposable {
    private static JsonSerializerSettings _settings = new JsonSerializerSettings {
        TypeNameHandling = TypeNameHandling.Objects,
        Formatting = Formatting.Indented,
        SerializationBinder = KnownTypesBinder.Instance
    };
    public GameState State {get; private set;}
    public string FilePath {get;}
    public static string GetLockFilePath(string filePath)
        => filePath + ".lock";
    public StateStorage(string filePath) {
        FilePath = filePath;
        State = LoadState(FilePath, withLock:true);
    }

    public StateStorage(string filePath, GameState state) {
        FilePath = filePath;
        State = state;
        TouchFile(GetLockFilePath(FilePath));
    }

    public StateStorage(string filePath, out GameState state) : this(filePath) {
        state = State;
    }

    public void Dispose() {
        SaveState(State, FilePath);
    }

    /// <summary>
    /// Default file location for the game state.  You will have to delete this file manually if it should become corrupted.
    /// </summary>
    public static FileInfo DefaultStateFilePath {
        get => new FileInfo(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MnkeyFog.json"
            )
        );
    }
    
    public static GameState LoadState(string filePath, bool withLock = false) {
        int waitCount = 0;
        while (File.Exists(GetLockFilePath(filePath))) {
            if (waitCount == 0) {
                Console.Out.WriteLine("Waiting for file to unlock");
            }
            Console.Out.Write(".");
            Thread.Sleep(100);
            waitCount += 1;
        }
        if (withLock) {
            TouchFile(GetLockFilePath(filePath));
        }

        using var sr = new StreamReader(filePath);
        var result = StringToState<GameState>(sr.ReadToEnd());
        return result;
    }

    public static void SaveState(GameState state, string filePath) {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(fileStream);
        writer.Write(StateToString(state));
        if (File.Exists(GetLockFilePath(filePath))) {
            File.Delete(GetLockFilePath(filePath));
        }
    }

    public static TState StringToState<TState>(string stateString)
	=> JsonConvert.DeserializeObject<TState>(stateString, _settings)!;

	public static string StateToString<TState>(TState state)
	=> JsonConvert.SerializeObject(state, _settings);

    public static void TouchFile(string lockFilePath) {
        if(!File.Exists(lockFilePath)) {
            File.Create(lockFilePath).Close(); // close immediately 
        }

        File.SetLastWriteTimeUtc(lockFilePath, DateTime.UtcNow);
    }
}
