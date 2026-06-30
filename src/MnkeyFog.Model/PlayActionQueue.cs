namespace MnkeyFog.Model;

#pragma warning disable CS0659, CS0661 
// Type overrides Object.Equals(object o) and operator == and operator != but
// does not override Object.GetHashCode(). We arenot overriding GetHashCode
// because that's for Dictionary keys and this is too mutable to be ever used
// for that.
[ModelSerializable]
public class PlayActionQueue : IPlayActionQueue {
#pragma warning restore CS0659, CS0661

    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.None)]
    public List<PlayerAction> Actions {get;private set;} = [];

    [JsonIgnore()]
    public GameState? GameState { get; internal set; }

    public void Add(PlayerAction action) {
        Actions.Add(action);
    }

    public void Clear() {
        Actions.Clear();
    }

    public void ExecutePendingActions() {
        if (Actions.Count == 0) {
            return;
        }
        if (GameState == null) {
            throw new InvalidOperationException("Must be initialized first.");
        }
               
        foreach (var action in Actions) {
            var collisions = Actions
                .Where(action.IsActionCollision)
                .ToList();
            if (collisions.Any()) {
                action.DoActionCollision(GameState, collisions!);
            } else {
                action.DoAction(GameState);
            }
        }

        Clear();
    }

    #region Equality
    // PlayActionBuffer when using record-based comparison fails at equality comparison,
    // so it must be implemented manually.
    public override bool Equals(object? obj) {
        if (obj == null) {
            return false;
        }
        
        if (obj is PlayActionQueue otherBuffer) {
            return Actions.SequenceEqual(otherBuffer.Actions);
        } else {
            return false;
        }
    }

    public static bool operator == (PlayActionQueue? a, PlayActionQueue? b)
    => (a == null) && (b == null) || (a?.Equals(b) ?? false);

    public static bool operator != (PlayActionQueue? a, PlayActionQueue? b)
    => !(a == b);
    #endregion
}
