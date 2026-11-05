namespace MnkeyFog.BskyService;

/// <summary>
/// Abstraction for sending a DM to a specific player (by DID), independent of
/// the currently-handled inbound conversation. Needed so that when player A
/// makes a move in a secret (kriegspiel) game, the bot can DM player B their
/// updated fogged view even though the inbound message came from A.
/// </summary>
public interface IPlayerNotifier {
    /// <summary>
    /// DM <paramref name="playerDid"/> with <paramref name="text"/>. If no
    /// 1:1 conversation exists yet, a warning is logged and the message is
    /// silently dropped.
    /// </summary>
    Task SendDmToPlayerAsync(string playerDid, string text, CancellationToken cancellationToken = default);
}
