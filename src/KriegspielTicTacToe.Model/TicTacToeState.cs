namespace KriegspielTicTacToe.Model;

using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;

/// <summary>
/// Object that models the full state of the game.  Is serialized into a json file so all players can share reading it.
/// </summary>
public record TicTacToeState {
    #region constructors
    /// <summary>
    /// This constructor is only used by the serializer, never use it.
    /// </summary>
    public TicTacToeState()
    {
        Boards = [];
    }
 
    /// <summary>
    /// Construct a new gamestate object.  Note that there is no protection at this level against impossible values.
    /// </summary>
    public TicTacToeState(char[] players, bool isRandomPlayer, IEnumerable<BoardBuilder> boardBuilders) {
        if(isRandomPlayer) {
            Random.Shared.Shuffle(players);
        }
        Players = players.ToList();
        Boards = boardBuilders.Select(b => new Board(b)).ToList();
    }
    #endregion

    #region main data properties
    public IReadOnlyList<char> Players {get;init;} = new List<char>();

    private int _currentTurnPlayerIndex = 0;
    /// <summary>
    /// index within list of *active* players - does not include resigned players.
    /// </summary>
    public int CurrentTurnPlayerIndex {
        get { return _currentTurnPlayerIndex; }
        set { 
            _currentTurnPlayerIndex = value; 
            RefreshCurrentPlayerTurnIndex();
        }
    }

    public HashSet<char> ResignedPlayersSet {get;init;} = new HashSet<char>();
    public IReadOnlyList<Board> Boards {get;init;}
    
    #endregion

    #region methods
    /// <summary>
    /// Advance to the next player's turn.  Do not call this if the current
    /// player has resigned.
    /// </summary>
    public void NextTurn() {
        CurrentTurnPlayerIndex += 1;
    }

    /// <summary>
    /// It's possible that the current player turn can exceed the number of
    /// players.  In that event, wrap around.
    /// </summary>
    public void RefreshCurrentPlayerTurnIndex()
        => _currentTurnPlayerIndex = CurrentTurnPlayerIndex % ActivePlayers.Count();
    
    /// <summary>
    /// Test if the given player has resigned.
    /// </summary>
    public bool IsResignedPlayer(char player) 
        => ResignedPlayersSet.Contains(player);

    /// <summary>
    /// Mark the given player as resigned.
    /// </summary>
    public void ResignPlayer(char player) {
        ResignedPlayersSet.Add(player);
        RefreshCurrentPlayerTurnIndex();
    }
    
    public Board GetBoardByCode(int boardCode)
        => Boards[boardCode-1];

    public OneOf<NotFound, BoardIsDone, Result<int>> SelectBoard(int boardCode)
        => (boardCode <= 0 || boardCode > Boards.Count)
            ? new NotFound()
            // doNextTurn = false;
            // Console.WriteLine ($"That is not a valid board.  Please pick an incomplete board from 1 to {state.Boards.Count}.");
            : GetBoardByCode(boardCode).IsDone
            ? new BoardIsDone()
            : new Result<int>(boardCode - 1);

    public OneOf<Success, Result<char>, AlreadyPlayed, NotFound> PlaySpace(int boardIndex, int spaceCode) {
        var board = Boards[boardIndex];
        if (board.TryGetCoordinatesFromSpaceIndexCode(spaceCode, out var col, out var row)) {
            return PlaySpace(boardIndex, col, row)
                .Match<OneOf<Success, Result<char>, AlreadyPlayed, NotFound>>( //have to provide return-type when going from OneOf to OneOf
                    success => success,
                    result => result,
                    alreadyPlayed => alreadyPlayed
                );
        } else {
            return new NotFound();
        }
    }
    
    public OneOf<Success, Result<char>, AlreadyPlayed> PlaySpace(int boardIndex, int col, int row) {
        var board = Boards[boardIndex];
        if (board.Spaces[col,row].MarkChar == CurrentTurnPlayer) {
            return new AlreadyPlayed();
        } else {
            board.Spaces[col,row].MakeKnownToPlayer(CurrentTurnPlayer);
            var foundMark = board.Spaces[col,row].MarkChar;
            if (foundMark.HasValue) {
                return new Result<char>(foundMark.Value);
            } else {
                board.Spaces[col,row].MarkChar = CurrentTurnPlayer;
                return new Success();
            }
        }
    }
    #endregion

    #region helper properties
    
    /// <summary>
    /// Returns a list of the active board indices. 0-based.
    /// </summary>
    [JsonIgnore()]
    public IEnumerable<int> ActiveBoardIndices {get {
        for(int i = 0; i < Boards.Count; i+=1) {
            if(!Boards[i].IsDone) {
                yield return i;
            }
        }
    }}
    
    /// <summary>
    /// Returns null if there are 0 or multiple active boards. Board Index if there's 1.
    /// </summary>
    [JsonIgnore()]
    public int? SingleActiveBoardIndex {get {
        var firstElements = ActiveBoardIndices.Take(2).ToArray();
        return (firstElements.Length == 1)
            ? firstElements.Single()
            : null;
    }}

    /// <summary>
    /// Get all of the current active players.  Order is consistent.
    /// </summary>
    [JsonIgnore()]
    public IEnumerable<char> ActivePlayers
        => Players.Except(ResignedPlayersSet);
        
    /// <summary>
    /// Get the mark-char of the current-turn player.
    /// </summary>
    [JsonIgnore()]
    public char CurrentTurnPlayer
        => ActivePlayers.ElementAt(CurrentTurnPlayerIndex);
    
    [JsonIgnore()]
    public ScoreCard ScoreCard 
        => Boards.Aggregate(new ScoreCard(), (prod, next) => prod + next.ScoreCard);
    
    [JsonIgnore()]
    /// <summary>
    /// Get the winner of the game.  Returns null if nobody has won yet or the
    /// game was a tie.  Note this is a heavy calculation and is not cached, but
    /// computers are fast.  TODO: Optimization.
    /// </summary>
    public char? Winner {
        get 
        {
            if(!IsGameOver) {
                return null;
            }

            if(ActivePlayers.Count() == 1) {
                return ActivePlayers.Single();
            }
            
            var highestScore = ScoreCard.HighestScore;
            if(highestScore.HasValue) {
                return highestScore.Value.Player;
            }
            
            //no winner found.
            return null;
        }
    }
    
    /// <summary>
    /// Returns true if the game has ended, whether by tie or by winner.
    /// </summary>
    [JsonIgnore()]
    public bool IsGameOver
        => Boards.All(b => b.IsDone)
        || ActivePlayers.Count() == 1;
    
    /// <summary>
    /// Provides a short text summary of the current game-state. Particularly useful when the game is over.
    /// </summary>
    [JsonIgnore()]
    public string GameStateText
        => Winner.HasValue
            ? $"Player '{Winner}' wins!"
            : IsGameOver 
            ? "Tie game."
            : $"Player '{CurrentTurnPlayer}' turn.";
    
    #endregion
}


/// <summary>
/// Empty result struct for OneOf, used when a player tries to play on a space they already played.
/// </summary>
public struct AlreadyPlayed;

/// <summary>
/// Empty result struct for OneOf, used when the player tries to select a board that is done.
/// </summary>
public struct BoardIsDone;
