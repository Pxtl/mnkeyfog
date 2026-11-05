// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService.Tests;

public class Base32Tests {
    [Theory]
    [InlineData(0, "A")]
    [InlineData(1, "B")]
    [InlineData(31, "7")]
    [InlineData(32, "BA")]
    [InlineData(1023, "77")]
    [InlineData(1024, "BAA")]
    public void Encode_KnownValues_ReturnsStandardBase32(int value, string expected) {
        Base32.Encode(value).Should().Be(expected);
    }

    [Fact]
    public void Encode_Negative_Throws() {
        var act = () => Base32.Encode(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("A", 0)]
    [InlineData("BA", 32)]
    [InlineData("77", 1023)]
    [InlineData("BAA", 1024)]
    public void Decode_KnownValues_ReturnsStandardValue(string code, int value) {
        Base32.Decode(code).Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("!")]
    [InlineData("0128")]
    public void Decode_InvalidOrMissing_ReturnsNull(string? code) {
        Base32.Decode(code).Should().BeNull();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(31, 1)]
    [InlineData(32, 2)]
    [InlineData(1023, 2)]
    [InlineData(1024, 3)]
    [InlineData(32768, 4)]
    public void LengthOfValue_KnownValues_ReturnsDigitCount(int value, int expected) {
        Base32.LengthOfValue(value).Should().Be(expected);
    }

    [Fact]
    public void EncodeDecode_RoundTrips() {
        foreach (var value in new[] { 0, 1, 31, 32, 1023, 1024, 32767, 32768, 123456 }) {
            Base32.Decode(Base32.Encode(value)).Should().Be(value);
        }
    }
}
