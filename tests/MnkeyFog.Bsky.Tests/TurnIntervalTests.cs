// SPDX-License-Identifier: MIT

namespace MnkeyFog.Bsky.Tests;

public class TurnIntervalTests {
    [Theory]
    [InlineData("20m", 20 * 60)]
    [InlineData("1d2h3m", 1 * 86400 + 2 * 3600 + 3 * 60)]
    [InlineData("90m", 90 * 60)]
    [InlineData("1h", 3600)]
    [InlineData("1w", 7 * 86400)]
    [InlineData("1w1d", 8 * 86400)]
    [InlineData("0s", 0)]
    [InlineData("  1d2h  ", 1 * 86400 + 2 * 3600)]
    [InlineData("1D2H3M", 1 * 86400 + 2 * 3600 + 3 * 60)]
    public void TryParse_ValidInput_Parses(string input, long expectedSeconds) {
        var ok = TurnInterval.TryParse(input, out var interval);
        ok.Should().BeTrue();
        interval.Value.TotalSeconds.Should().Be(expectedSeconds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("12")]
    [InlineData("1x")]
    [InlineData("3m1d")]
    [InlineData("1m1m")]
    [InlineData("m")]
    [InlineData("1m2")]
    [InlineData("1w 2h")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input) {
        TurnInterval.TryParse(input, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, "0s")]
    [InlineData(20 * 60, "20m")]
    [InlineData(1 * 86400 + 2 * 3600 + 3 * 60, "1d2h3m")]
    [InlineData(7 * 86400, "1w")]
    [InlineData(8 * 86400, "1w1d")]
    [InlineData(3600, "1h")]
    [InlineData(90 * 60, "1h30m")]
    public void Format_KnownValues_Formats(long seconds, string expected) {
        TurnInterval.Format(TimeSpan.FromSeconds(seconds)).Should().Be(expected);
    }

    [Fact]
    public void Default_IsTwentyMinutes() {
        TurnInterval.Default.Value.Should().Be(TimeSpan.FromMinutes(20));
        TurnInterval.Default.ToString().Should().Be("20m");
    }

    [Fact]
    public void ToString_FormatsValue() {
        var interval = new TurnInterval(TimeSpan.FromMinutes(20));
        interval.ToString().Should().Be("20m");
    }
}
