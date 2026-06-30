using MnkeyFog.Model.Views;
using OneOf;
using OneOf.Types;

namespace MnkeyFog.Model;

/// <summary>
/// Non-generic interface for <see cref="GameState{TState, TTemplate, TAction}"/> 
/// </summary>
public interface IGameState {
    void Initialize();

    #region Data Members

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    PlayManager PlayManager { get; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    IReadOnlyList<Board> Boards { get; }
    #endregion

    GameView GetView(Player? player);

    Board GetBoardByIndex(sbyte boardIndex);
    
    [JsonIgnore()]
    bool IsGameOver { get; }

    [JsonIgnore()]
    IEnumerable<Player> Winners { get; }
}
