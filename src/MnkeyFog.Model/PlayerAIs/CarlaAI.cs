namespace MnkeyFog.Model.PlayerAIs;

[ModelSerializable]
public class CarlaAI : MonteCarloAI {
    public CarlaAI() : base() { }

    public CarlaAI(int seed) : base(seed) { }

    public override string Description => "Carla, difficulty 6";
    public override int MaxDepth => 4;
}
