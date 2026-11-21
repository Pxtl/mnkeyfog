namespace MnkeyFog.Model.PlayerAIs;

[ModelSerializable]
public class EagerAI : MonteCarloAI {
    public EagerAI() : base() { }

    public EagerAI(int seed) : base(seed) { }

    public override string Description => "Eager, difficulty 4";
    public override int MaxDepth => 1;
}
