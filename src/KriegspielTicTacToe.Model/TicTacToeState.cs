using OneOf;
using OneOf.Types;

namespace KriegspielTicTacToe.Model;

public record TicTacToeState {
    public TicTacToeState(
        char[] players,
        IEnumerable<BoardBuilder> boardBuilders,
        bool isRandomPlayerOrder,
        bool isSynchronousMode
    ) {
        List<Player> playerList = players
            .Select(c => new Player(c.ToString()))
            .ToList();
        playerList = playerList.ToList();
        if(isRandomPlayerOrder) { 
            Random.Shared.Shuffle((Player[])playerList.ToArray());
        }
        PlayManager = isSynchronousMode
            ? new SynchronizedPlayManager(playerList.AsReadOnly())
            : new RoundRobinPlayManager(playerList.AsReadOnly());
        Boards = boardBuilders.Select(b => new Board(b)).ToList();
        Initialize();
    }

    public TicTacToeState(
        Player[] players,
        IEnumerable<BoardBuilder> boardBuilders,
        bool isRandomPlayerOrder,
        bool isSynchronousMode
    ) {
        if(isRandomPlayerOrder) { 
            Random.Shared.Shuffle((Player[])players); // explicit generic
        }
        PlayManager = (isSynchronousMode)
            ? new SynchronizedPlayManager(players.AsReadOnly())
            : new RoundRobinPlayManager(players.AsReadOnly());
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

    public OneOf<NotFound, Result<int>> GetBoardIndexByName(string boardName) {
        var boardNameAsInt = int.Parse(boardName);
        var boardIndex = boardNameAsInt - 1;
        return (boardIndex >= 0 && boardIndex < Boards.Count) 
            ? new Result<int>(boardIndex)
            : new NotFound();
    }

    public IEnumerable<string> BoardNames {
        get {
            for(var i = 1; i <= Boards.Count; i += 1) {
                yield return i.ToString();
            }
        }
    }
        
    public Board GetBoardByIndex(int boardIndex) => Boards[boardIndex];

    public OneOf<NotFound, BoardIsDone, Result<int>> SelectBoard(string boardName)
        => GetBoardIndexByName(boardName).Match(
            notFound => new NotFound(),
            indexResult => GetBoardByIndex(indexResult.Value).IsDone
                ? OneOf<NotFound, BoardIsDone, Result<int>>.FromT1(new BoardIsDone())
                : new Result<int>(indexResult.Value)
        );

    public OneOf<NotFound, ActionQueuedSuccessfully, Result<Player>, AlreadyPlayed> PlaySpace(
        int boardIndex,
        string spaceName,
        Player player
    ) => PlaySpace(boardIndex, int.Parse(spaceName), player);
        
    private OneOf<NotFound, ActionQueuedSuccessfully, Result<Player>, AlreadyPlayed> PlaySpace(
        int boardIndex,
        int spaceNameAsInt,
        Player player
    ) {
        if (spaceNameAsInt <= 0) {
            return new NotFound();
        }
        var board = Boards[boardIndex];
        if (board.TryGetCoordinatesFromSpaceNameAsInt(spaceNameAsInt, out var col, out var row)) {
            return PlaySpace(boardIndex, col, row, player);
        }
        return new NotFound();
    }

    public OneOf<NotFound, ActionQueuedSuccessfully, Result<Player>, AlreadyPlayed> PlaySpace(
        int boardIndex,
        int col,
        int row,
        Player player
    ) {
        var space = Boards[boardIndex].Spaces[col, row];
        if (space.IsKnownToPlayer(player)) {
            return new AlreadyPlayed();
        } else if (space.Mark != null) {
            space.MakeKnownToPlayer(player);
            return new Result<Player>(space.Mark);
        } else {
            PlayActionBuffer.Add(new TicTacToePlayAction(boardIndex, col, row, player));
            return new ActionQueuedSuccessfully();
        }
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
        ? (Winner == null
            ? "Game over. Tie game."
            : $"Game over. {Winner} wins."
        )
        : (PlayManager.GameStateText 
            + Environment.NewLine
            + ResignedPlayersText
        );

    public string ResignedPlayersText
        => PlayManager.ResignedPlayersSet.Count > 0
        ? $"Resigned players: {string.Join(", ", PlayManager.ResignedPlayersSet.OrderBy(p => p.Mark))}"
        : "";
    
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
        //make sure all players are in the scorecard even those with 0.
        => new ScoreCard(PlayManager.ActivePlayers.Select(p => new PlayerScore(p, 0))) 
            // can't use ienumerable.sum on non-numeric objects, operators don't work that way.
            + Boards.Select(b => b.ScoreCard).SumScoreCards();
}
public struct AlreadyPlayed;
public struct BoardIsDone;
public struct ActionQueuedSuccessfully;
