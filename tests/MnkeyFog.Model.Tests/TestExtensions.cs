using System.Linq;
namespace MnkeyFog.Model.Tests;

internal static class TestExtensions {   
    internal static Player[] ToPlayersArray(this char[] chars)
        => chars.Select(c => new Player(c.ToString())).ToArray();
}