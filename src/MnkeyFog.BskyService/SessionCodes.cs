// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService;

/// <summary>
/// Short session-code generation. Codes are an *offset-by-1* unpadded RFC 4648
/// base32 encoding: the code for value <c>n</c> is the standard big-endian
/// base32 digit string of <c>n - 1</c>, with the special case that value 1
/// (i.e. <c>n - 1 = 0</c>) encodes to the single character "A".
///
/// Because the digit count is based on <c>n - 1</c>, one character covers
/// values 1–32 and two characters cover values 33–1024, giving a
/// <see cref="MaxShortCodeValue" /> of 1024 (the last value still expressible
/// in two characters).
/// </summary>
public static class SessionCodes {
    /// <summary>RFC 4648 base32 alphabet (A-Z, 2-7), no '=' padding character.</summary>
    public static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

    /// <summary>
    /// Largest sequence value that still encodes to two or fewer base32
    /// characters. One char covers 1–32, two chars cover 33–1024, so three
    /// characters begin at 1025.
    /// </summary>
    public const int MaxShortCodeValue = 1024;

    /// <summary>Default length of the longer fallback codes.</summary>
    public const int LongCodeLength = 6;

    /// <summary>
    /// Encode a positive sequence value (counting from 1) as an unpadded base32
    /// string using the offset-by-1 convention: value 1 → "A", value 2 → "B",
    /// …, value 32 → "7", value 33 → "BA", value 1024 → "77",
    /// value 1025 → "BAA".
    /// </summary>
    /// <param name="value">Sequence value, at least 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="value" /> is less than 1.</exception>
    public static string ToBase32(int value) {
        ThrowIfNotPositive(value);

        // The code is the standard big-endian base32 of (value - 1).
        // When value == 1, (value - 1) == 0 and the digit string is the single
        // character "A" (alphabet index 0).
        int n = value - 1;
        if (n == 0) {
            return "A";
        }

        var chars = new char[32];
        var length = 0;
        while (n > 0) {
            chars[length] = Alphabet[n & 0x1F];
            n >>= 5;
            length++;
        }
        // Least-significant character was written first; flip for output.
        Array.Reverse(chars, 0, length);
        return new string(chars, 0, length);
    }

    /// <summary>
    /// Decode an unpadded base32 string (emitted by <see cref="ToBase32"/>) back
    /// to its sequence value, or null if it is not valid base32 (empty, or
    /// contains a character outside <see cref="Alphabet"/>, ignoring case).
    /// </summary>
    /// <remarks>
    /// The decoded value is one greater than the standard base32 numeric value
    /// of the digit string, matching the offset-by-1 convention used by
    /// <see cref="ToBase32"/>.
    /// </remarks>
    public static int? FromBase32(string? code) {
        if (string.IsNullOrEmpty(code)) {
            return null;
        }

        int valueMinusOne = 0;
        foreach (var raw in code.ToUpperInvariant()) {
            var index = Array.IndexOf(Alphabet, raw);
            if (index < 0) {
                return null;
            }
            valueMinusOne = (valueMinusOne << 5) | index;
        }
        return valueMinusOne + 1;
    }

    /// <summary>
    /// Number of base32 characters a value needs under the offset-by-1 convention.
    /// Value 0 and negatives return 0. One character carries values 1–32, two
    /// characters carry 33–1024, three carry 1025–32768, and so on.
    /// </summary>
    public static int LengthOfValue(int value) {
        if (value < 1) {
            return 0;
        }
        int n = value - 1;
        if (n == 0) {
            return 1;
        }
        var length = 0;
        while (n > 0) {
            length++;
            n >>= 5;
        }
        return length;
    }

    /// <summary>Cryptographically random code of the given length, from the base32 alphabet.</summary>
    public static string RandomLongCode(int length = LongCodeLength) {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (var i = 0; i < length; i++) {
            chars[i] = Alphabet[bytes[i] & 0x1F];
        }
        return new string(chars);
    }

    private static void ThrowIfNotPositive(int value) {
        if (value < 1) {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be at least 1.");
        }
    }
}
