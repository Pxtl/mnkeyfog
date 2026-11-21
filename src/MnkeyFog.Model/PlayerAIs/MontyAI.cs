namespace MnkeyFog.Model.PlayerAIs;

[ModelSerializable]
public class MontyAI : MonteCarloAI {
    public MontyAI() : base() { }

    public MontyAI(int seed) : base(seed) { }

    public override string Description => "Monty, difficulty 5";
    public override int MaxDepth => 3;
}
