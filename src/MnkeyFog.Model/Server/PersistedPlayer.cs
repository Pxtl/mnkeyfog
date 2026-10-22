namespace MnkeyFog.Model.Server;

/// <summary>
/// One participant mapping within a persisted <see cref="PersistedGameSession"/>,
/// used by the bsky server. Maps the player's id to the index of their
/// <see cref="MnkeyFog.Model.IGameState"/> player slot.
/// </summary>
[ModelSerializable]
public sealed record PersistedPlayer(string Id, int PlayerIndex);
