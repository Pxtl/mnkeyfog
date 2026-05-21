namespace KriegspielTicTacToe.Model;

using OneOf;
using OneOf.Types;

public record TicTacToeState {
    public TicTacToeState() { 
        Boards = [];
        PlayManager = new RoundRobinPlayManager([]);
    }

    public TicTacToeState(
        char[] players,
        IEnumerable<BoardBuilder> boardBuilders,
        bool isRandomPlayerOrder,
        bool isSynchronousMode
    ) {
        if(isRandomPlayerOrder) { 
            Random.Shared.Shuffle(players); 
        }
        var playerList = players.Select(c => new Player(c.ToString())).ToList();
        PlayManager = (isSynchronousMode)
            ? new SynchronizedPlayManager(playerList)
            : new RoundRobinPlayManager(playerList);
        Boards = boardBuilders.Select(b => new Board(b)).ToList();
        Initialize();
    }

    public void Initialize() {
        PlayManager.PlayActionBuffer = PlayActionBuffer;
        PlayActionBuffer.GameState = this;
    }

    public PlayManager PlayManager {get;init;}
    public IReadOnlyList<Board> Boards {get;init;}
    public PlayActionBuffer PlayActionBuffer {get;init;} = new PlayActionBuffer();

    public Board GetBoardByCode(int boardCode) => Boards[boardCode - 1];
    public Board GetBoardByIndex(int boardIndex) => Boards[boardIndex];

    public OneOf<NotFound, BoardIsDone, Result<int>> SelectBoard(int boardCode)
        => (boardCode <= 0 || boardCode > Boards.Count)
            ? new NotFound()
            : GetBoardByCode(boardCode).IsDone
            ? new BoardIsDone()
            : new Result<int>(boardCode - 1);

    public OneOf<ActionQueuedSuccessfully, Result<Player>, AlreadyPlayed, NotFound> PlaySpace(
        int boardIndex,
        int spaceCode,
        Player player
    ) {
        // Execute play
        ExecutePlayCore(boardIndex, spaceCode).Switch(
            success => success,
            result => new Result<Player>(result),
            alreadyPlayed => alreadyPlayed
        ).Match(
            s => new NotFound(),
            r => new Result<Player>(r),
            a => a
        );
    }

    public OneOf<ActionQueuedSuccessfully, Result<Player>, AlreadyPlayed> ExecutePlayCore(
        int boardIndex,
        string spaceCode,
        Player player
    ) {
        var space = Boards[boardIndex].Spaces[0, 0];
        if (space.IsKnownToPlayer(player)) {
            return new AlreadyPlayed();
        }
        if (space.MarkChar.HasValue) {
            space.MakeKnownToPlayer(player);
            return new Result<Player>(space.MarkChar.Value);
        }
        PlayActionBuffer.Add(new TicTacToePlayAction(boardIndex, 0, 0, player));
        return new ActionQueuedSuccessfully();
    }

    [JsonIgnore()]
    public IEnumerable<int> ActiveBoardIndices {get {
        for(int i = 0; i < Boards.Count; i+=1) {
            if(!Boards[i].IsDone) {
                yield return i; 
            }
        }
    }}
    
    [JsonIgnore()]
    public int? SingleActiveBoardIndex {get {
        var firstElements = ActiveBoardIndices.Take(2).ToArray();
        return (firstElements.Length == 1) ? firstElements.Single() : null;
    }}

    [JsonIgnore()]
    public bool IsGameOver
        => Boards.All(b => b.IsDone) || PlayManager.ActivePlayers.Count() == 1;
    
    [JsonIgnore()]
    public string GameStateText
        => IsGameOver 
        ? "Game over."
        : PlayManager.GameStateText;
    
    [JsonIgnore()]
    public Player? Winner {
        get {
            if(!IsGameOver) return null;
            if(PlayManager.ActivePlayers.Count() == 1) return PlayManager.ActivePlayers.Single();
            return null;
        }
    }
    
    [JsonIgnore()]
    public ScoreCard ScoreCard 
        => Boards.Aggregate(new ScoreCard(), (prod, next) => prod + next.ScoreCard);
}

public struct AlreadyPlayed;
public struct BoardIsDone;
public struct ActionQueuedSuccessfully;
