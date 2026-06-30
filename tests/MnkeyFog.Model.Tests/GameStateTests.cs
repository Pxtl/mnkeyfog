namespace MnkeyFog.Model.Tests;

public class GameStateTests {
    [Fact]
    public void Constructor_EmptyBoards() {
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate(Array.Empty<BoardBuilder>(), isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        state.Boards.Should().BeEmpty();
        state.PlayManager.Players.Should().Contain(new Player("X")).And.Subject.Should().Contain(new Player("O"));
        state.PlayManager.ActivePlayers.Should().Contain(new Player("X")).And.Subject.Should().Contain(new Player("O"));
    }
}
