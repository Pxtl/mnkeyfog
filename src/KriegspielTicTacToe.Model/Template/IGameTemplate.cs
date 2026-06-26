namespace KriegspielTicTacToe.Model.Template;

public interface IGameTemplate {
    string? CommandName { get; }
	string? Description { get; }
	IEnumerable<int> LegalPlayerCounts { get; }
    PlayManagerFactory PlayManagerFactory { get;}
    IEnumerable<GameActionFactory> GetAvailableActions(GameState gameState, Player player);
    
    IReadOnlyList<Board> CreateBoards();
	void InitializeGame (GameState gameState);
}
