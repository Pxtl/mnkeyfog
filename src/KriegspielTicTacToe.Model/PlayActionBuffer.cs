namespace KriegspielTicTacToe.Model;

public class PlayActionBuffer {
    public List<TicTacToePlayAction> Actions {get;private set;} = [];

    public void Add(TicTacToePlayAction action) {
        Actions.Add(action);
    }

    public void Clear() {
        Actions.Clear();
    }

    public void ExecutePendingActions() {
        if (Actions.Count == 0) return;
               
        foreach (var action in Actions) {
            var board = GameState!.GetBoardByIndex(action.BoardIndex);
            var space = board.Spaces[action.Col, action.Row];
            
            if (board.IsDone) {
                continue;
            }
            
            if (Actions.Any(otherA => 
                otherA.BoardIndex == action.BoardIndex
                && otherA.Row == action.Row
                && otherA.Col == action.Col
                && otherA.Value != action.Value)
            ) {
                space.MarkChar = '█';
                foreach(var player in GameState.PlayManager.Players) {
                    space.MakeKnownToPlayer(player.Value);    
                }
                continue;
            }
            
            space.MarkChar = action.Value;
            space.MakeKnownToPlayer(action.Value);
        }

        Clear();
    }

    [JsonIgnore()]
    public TicTacToeState? GameState { get; internal set; }
}
