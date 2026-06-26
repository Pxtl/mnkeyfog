namespace KriegspielTicTacToe.Model;

public interface IPlayActionResult {
    /// <summary>
    /// True if the UI should re-render the board(s).
    /// </summary>
    bool IsViewChanged { get; }
    /// <summary>
    /// True if the player's turn is over.
    /// </summary>
    bool IsTurnDone { get; }
    public string ResultText { get; }
}

public record struct Resigned(Player Player)
: IPlayActionResult {
    public bool IsViewChanged => true;
    public bool IsTurnDone => true;
    public string ResultText => $"Player {Player} is resigning.";
}
public record struct Quitting()
: IPlayActionResult {
    public bool IsViewChanged => true;
    public bool IsTurnDone => true;
    public string ResultText => "Quitting.  Use 'load' to resume later.";
}
public record struct Enqueued(bool IsViewChanged, string SpaceName) 
: IPlayActionResult {
    public bool IsTurnDone => true;
    public string ResultText => $"Played space {SpaceName}.";
}
public record struct AlreadyPlayed(Player Player)
: IPlayActionResult {
    public bool IsViewChanged => false;
    public bool IsTurnDone => false;
    public string ResultText => $"Invalid space, that space is already known to player {Player}.";
};
public record struct NewlyLearned(string Mark)
: IPlayActionResult {
    public bool IsViewChanged => true;
    public bool IsTurnDone => true;
    public string ResultText => $"Space already filled: '{Mark}'.";
};
public struct BoardIsDone
: IPlayActionResult {
    public bool IsViewChanged => false;
    public bool IsTurnDone => false;
    public string ResultText => "That board is already complete.";
}
public struct InvalidCommand(string CommandText)
: IPlayActionResult {
    public bool IsViewChanged => false;
    public bool IsTurnDone => false;
    public string ResultText => $"Invalid command: {CommandText}";
}
public struct NullResult()
: IPlayActionResult {
    public bool IsViewChanged => throw new InvalidOperationException($"{nameof(NullResult)} should have been replaced, its members should never be used.");
    public bool IsTurnDone => throw new InvalidOperationException($"{nameof(NullResult)} should have been replaced, its members should never be used.");
    public string ResultText => throw new InvalidOperationException($"{nameof(NullResult)} should have been replaced, its members should never be used.");
}