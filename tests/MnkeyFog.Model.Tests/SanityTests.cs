namespace MnkeyFog.Model.Tests;

/// <summary>
/// Bare minimum test to confirm testrunner is working.
/// </summary>
public class SanityTests {
    [Fact]
    public void BasicTest_Tautology()
    {
        true.Should().BeTrue();
    }
}