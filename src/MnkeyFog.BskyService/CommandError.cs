namespace MnkeyFog.BskyService;

/// <summary>
/// A user-facing error message for a rejected bot command.
/// </summary>
public record CommandError(string Message) {
    public override string ToString() => Message;
}
