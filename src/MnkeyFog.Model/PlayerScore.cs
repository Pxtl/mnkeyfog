namespace MnkeyFog.Model;

/// <summary>
/// Score for a single player
/// </summary>
[ModelSerializable]
public record struct PlayerScore(Player Player, int Score) {
    public static implicit operator ScoreCard(PlayerScore p) => new ScoreCard(p);   
}
