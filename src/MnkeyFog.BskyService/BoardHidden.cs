namespace MnkeyFog.BskyService;

/// <summary>
/// The board must not be posted on this channel because the game is secret
/// (kriegspiel). The caller should post a placeholder message instead and DM
/// each player their own fogged view.
/// </summary>
internal record BoardHidden;
