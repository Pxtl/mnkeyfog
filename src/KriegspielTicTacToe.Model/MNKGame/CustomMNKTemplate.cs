using System.Text;
using KriegspielTicTacToe.Model.Template;

namespace KriegspielTicTacToe.Model.MNKGame;

/// <summary>
/// Represents a game type configuration including board builders and play mode
/// settings for an MNK game such as tic tac toe.  Adds dynamically-constructed
/// description to the base.<see
/// href="https://en.wikipedia.org/wiki/M,n,k-game">WP: MNK Game</see>
/// </summary>
[ModelSerializable]
public record CustomMNKTemplate : MNKTemplate {
    public CustomMNKTemplate(
        IReadOnlyList<BoardBuilder> boardBuilders,
        bool isKriegspiel,
        bool isSynchronousMode
    ) : base("custom", "dummy description replaced within constructor", DefaultLegalPlayerCounts, boardBuilders, isKriegspiel, isSynchronousMode) {
        Description = "Custom MNK game."
            + Environment.NewLine
            + string.Join(Environment.NewLine, BoardBuildersToStrings())
            + Environment.NewLine
            + $"kriegspiel: {IsKriegspiel}"
            + Environment.NewLine
            + $"synchronous: {isSynchronousMode}";
    }

    private IEnumerable<string> BoardBuildersToStrings() {
        for(var i = 0; i < BoardBuilders.Count; i+=1) {
            yield return BoardBuilders[i].ToString(ModelToCommandNameUtility.GetBoardNameFromIndex(i));
        }
    }
}