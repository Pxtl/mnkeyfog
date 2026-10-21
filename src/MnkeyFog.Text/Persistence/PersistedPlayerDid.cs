namespace MnkeyFog.Text.Persistence;

/// <summary>One DID-to-player-index mapping within a persisted <c>PersistedSession</c>.</summary>
public sealed record PersistedPlayerDid(string Did, int PlayerIndex);
