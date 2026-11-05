// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService;

/// <summary>
/// A clean, system-agnostic unpadded RFC 4648 base32 encoding (A-Z, 2-7),
/// big-endian, no '=' padding. Value 0 encodes to the single character "A".
/// </summary>
public static class Base32 {
    /// <summary>RFC 4648 base32 alphabet (A-Z, 2-7), no '=' padding character.</summary>
    public static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

    /// <summary>
    /// Encode a non-negative integer as an unpadded big-endian base32 string.
    /// Value 0 encodes to "A".
    /// </summary>
    public static string Encode(int value) {
        ThrowIfNegative(value);

        Span<char> buffer = stackalloc char[8]; // int is 32 bits: ceil(32/5) = 7 digits.
        var n = value;
        var length = 0;
        do {
            buffer[length++] = Alphabet[n & 0x1F];
            n >>= 5;
        } while (n > 0);
        buffer[..length].Reverse();
        return new string(buffer[..length]);
    }

    /// <summary>
    /// Decode an unpadded base32 string (case-insensitive) to its value, or null
    /// if it is empty or contains a character outside <see cref="Alphabet"/>.
    /// </summary>
    public static int? Decode(string? code) {
        if (string.IsNullOrEmpty(code)) {
            return null;
        }

        var value = 0;
        foreach (var raw in code.ToUpperInvariant()) {
            var index = Array.IndexOf(Alphabet, raw);
            if (index < 0) {
                return null;
            }
            value = (value << 5) | index;
        }
        return value;
    }

    /// <summary>
    /// Number of base32 digits a non-negative value needs (0 needs 1).
    /// </summary>
    public static int LengthOfValue(int value) {
        ThrowIfNegative(value);

        var n = value;
        var length = 0;
        do {
            length++;
            n >>= 5;
        } while (n > 0);
        return length;
    }

    private static void ThrowIfNegative(int value) {
        if (value < 0) {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be non-negative.");
        }
    }
}
