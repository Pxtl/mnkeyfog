namespace MnkeyFog.Model;

public interface IPlayActionResult {
    /// <summary>
    /// True if the UI should re-render the board(s).
    /// </summary>
    bool IsViewChanged { get; }
    /// <summary>
    /// True if the player's turn is over.
    /// </summary>
    bool IsTurnDone { get; }
    public string GetResultText(PlayersState playersState);
}

public record struct Resigned(int PlayerIndex)
: IPlayActionResult {
    public bool IsViewChanged => true;
    public bool IsTurnDone => true;
    public string GetResultText(PlayersState playersState)
    => $"Player {playersState.GetPlayer(PlayerIndex)} is resigning.";
}
public record struct Quitting()
: IPlayActionResult {
    public bool IsViewChanged => true;
    public bool IsTurnDone => true;
    public string GetResultText(PlayersState playersState)
    => "Quitting.  Use 'load' to resume later.";
}
public record struct Enqueued(bool IsViewChanged)
: IPlayActionResult {
    public bool IsTurnDone => true;
    public string GetResultText(PlayersState playersState)
    => $"Made move.";
}
public record struct CannotTakeTurn(int PlayerIndex)
: IPlayActionResult {
    public bool IsViewChanged => false;
    public bool IsTurnDone => false;
    public string GetResultText(PlayersState playersState)
    => $"The player {playersState.GetPlayer(PlayerIndex)} can not take a turn right now.";
};
public record struct PositionAlreadyPlayed(int PlayerIndex)
: IPlayActionResult {
    public bool IsViewChanged => false;
    public bool IsTurnDone => false;
    public string GetResultText(PlayersState playersState)
    => $"Invalid space, that space is already known to player {playersState.GetPlayer(PlayerIndex)}.";
};
public record struct NewlyLearned(int MarkIndex)
: IPlayActionResult {
    public bool IsViewChanged => true;
    public bool IsTurnDone => true;
    public string GetResultText(PlayersState playersState)
    => $"Space already filled: '{playersState.GetCommandName(MarkIndex)}'.";
};
public struct BoardIsDone
: IPlayActionResult {
    public bool IsViewChanged => false;
    public bool IsTurnDone => false;
    public string GetResultText(PlayersState playersState)
    => "That board is already complete.";
}
public struct InvalidCommand(string CommandText)
: IPlayActionResult {
    public bool IsViewChanged => false;
    public bool IsTurnDone => false;
    public string GetResultText(PlayersState playersState)
    => $"Invalid command: {CommandText}";
}
public struct NullResult()
: IPlayActionResult {
    public bool IsViewChanged => throw new InvalidOperationException($"{nameof(NullResult)} should have been replaced, its members should never be used.");
    public bool IsTurnDone => throw new InvalidOperationException($"{nameof(NullResult)} should have been replaced, its members should never be used.");
    public string GetResultText(PlayersState playersState) => throw new InvalidOperationException($"{nameof(NullResult)} should have been replaced, its members should never be used.");
}
