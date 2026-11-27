namespace MnkeyFog.Model.Template;

public interface IGameTemplate {
    string? CommandName { get; }
    string? Description { get; }
    string DefaultActionCommandName { get; }
    IEnumerable<int> LegalPlayerCounts { get; }
    PlayManager PlayManager { get; }
    IEnumerable<GameActionFactory> GetAvailableActions(GameState gameState, int playerIndex);

    IReadOnlyList<Board> CreateBoards();
    void InitializeGame(GameState gameState);
}
