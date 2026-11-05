// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService;

/// <summary>
/// Session-code generation. Codes are an *offset-by-1* unpadded base32
/// encoding: the code for sequence value <c>n</c> (counting from 1) is the
/// standard base32 of <c>n - 1</c>, so value 1 → "A", value 32 → "7", value
/// 33 → "BA". One character covers values 1–32 and two characters cover values
/// 33–1024 (<see cref="MaxShortCodeValue"/>).
/// </summary>
public static class SessionCodes {
    /// <summary>
    /// Largest sequence value that still encodes to two or fewer base32
    /// characters. One char covers 1–32, two chars cover 33–1024, so three
    /// characters begin at 1025.
    /// </summary>
    public const int MaxShortCodeValue = 1024;

    /// <summary>
    /// Encode a positive sequence value (counting from 1) using the offset-by-1
    /// convention: value 1 → "A", …, value 1024 → "77", value 1025 → "BAA".
    /// </summary>
    /// <param name="value">Sequence value, at least 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="value" /> is less than 1.</exception>
    public static string ToBase32(int value) {
        ThrowIfNotPositive(value);
        return Base32.Encode(value - 1);
    }

    /// <summary>
    /// Decode a session code (emitted by <see cref="ToBase32"/>) back to its
    /// sequence value, or null if it is not valid base32 (empty, or contains a
    /// character outside the base32 alphabet, ignoring case).
    /// </summary>
    public static int? FromBase32(string? code) {
        return Base32.Decode(code) is { } value ? value + 1 : null;
    }

    /// <summary>
    /// Number of base32 characters a value needs under the offset-by-1
    /// convention. Value 0 and negatives return 0.
    /// </summary>
    public static int LengthOfValue(int value) {
        if (value < 1) {
            return 0;
        }
        return Base32.LengthOfValue(value - 1);
    }

    private static void ThrowIfNotPositive(int value) {
        if (value < 1) {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be at least 1.");
        }
    }
}
