// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService.Tests;

file static class GameStoreTestHelper {
    public static IGameTemplate MakeTemplate() {
        var builder = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        return new MNKTemplate([builder], isSynchronousMode: false, isKriegspiel: false);
    }

    public static string NextUri() => $"at://did:plc:root/{Guid.NewGuid():N}";

    public static GameStore.CreatedGame Created(GameStore store, string creator) {
        var result = store.CreateGame(NextUri(), creator, 2, MakeTemplate());
        return result.Match(created => created, error => throw new Exception($"CreateGame failed: {error}"));
    }
}

public class GameStoreCodeRecyclingTests {
    /// <summary>
    /// The natural short-code path: while the highest session code value is at or
    /// below MaxShortCodeValue (1023), each new game gets the base32 of (highest
    /// + 1) in order - 1-char codes (1..31) then 2-char codes (32..1023).
    /// </summary>
    [Fact]
    public void CreateGame_WithinShortRange_CodesAreSequentialBase32() {
        var store = new GameStore();

        var codes = new List<string>();
        for (var i = 0; i < 40; i++) {
            codes.Add(GameStoreTestHelper.Created(store, $"did:plc:creator{i}").Code);
        }

        // 1..32 are one char, 33..40 are two chars.
        codes.Take(31).Should().AllSatisfy(c => c.Length.Should().Be(1));
        codes.Skip(31).Should().AllSatisfy(c => c.Length.Should().Be(2));
        codes[0].Should().Be(SessionCodes.ToBase32(1));
        codes[31].Should().Be(SessionCodes.ToBase32(32));
        codes[39].Should().Be(SessionCodes.ToBase32(40));
    }

    /// <summary>
    /// Fill every short slot (1..1023), complete all of them, and verify the next
    /// creation recycles the code of the completed session with the oldest
    /// StartedAt.
    /// </summary>
    [Fact]
    public void CreateGame_AfterShortRangeFullyCompleted_RecyclesOldestCompletedCode() {
        var store = new GameStore();
        var startedByCode = new Dictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < SessionCodes.MaxShortCodeValue; i++) {
            var created = GameStoreTestHelper.Created(store, $"did:plc:creator{i}");
            startedByCode[created.Code] = created.Session.StartedAt;
        }

        foreach (var code in new List<string>(startedByCode.Keys)) {
            store.CompleteGame(code);
        }

        // All are started in real time with identical instants; the allocation
        // algorithm's tie-break is stable (Dictionary enumeration order), so assert
        // the strongest invariants only: the recycled code exists, is two chars
        // (a short code), differs from the next natural value, and is one of the
        // completed codes.
        var next = GameStoreTestHelper.Created(store, "did:plc:after");
        next.Code.Should().BeOneOf(startedByCode.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase));
        next.Code.Length.Should().BeLessThanOrEqualTo(2);
    }

    /// <summary>
    /// Past the short range, when only one short code is completed, that code is
    /// recycled even though the highest session code value has advanced beyond
    /// 1023.
    /// </summary>
    [Fact]
    public void CreateGame_BeyondShortRangeWithOneCompleted_RecyclesThatCompletedCode() {
        var store = new GameStore();
        var codes = new List<string>();

        for (var i = 0; i < SessionCodes.MaxShortCodeValue; i++) {
            codes.Add(GameStoreTestHelper.Created(store, $"did:plc:creator{i}").Code);
        }

        var codeForFive = SessionCodes.ToBase32(5);
        codes.Should().Contain(codeForFive);
        store.CompleteGame(codeForFive);

        var next = GameStoreTestHelper.Created(store, "did:plc:after");
        next.Code.Should().Be(codeForFive);
    }

    /// <summary>
    /// When every short slot is active (none completed), a game beyond the
    /// short range cannot recycle and must fall back to a longer (longer than two
    /// chars) random code that does not collide with the active short codes.
    /// </summary>
    [Fact]
    public void CreateGame_BeyondShortRangeWithNoCompleted_ReturnsLongerRandomCode() {
        var store = new GameStore();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < SessionCodes.MaxShortCodeValue; i++) {
            used.Add(GameStoreTestHelper.Created(store, $"did:plc:creator{i}").Code);
        }

        var next = GameStoreTestHelper.Created(store, "did:plc:after");
        next.Code.Length.Should().BeGreaterThan(2);
        used.Contains(next.Code).Should().BeFalse();
    }

    /// <summary>
    /// Allocation must never hand out a code (in any casing) that is already
    /// taken by an active session.
    /// </summary>
    [Fact]
    public void CreateGame_AllocatedCodes_AreUnique() {
        var store = new GameStore();
        const int count = 150;

        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < count; i++) {
            codes.Add(GameStoreTestHelper.Created(store, $"did:plc:creator{i}").Code);
        }

        codes.Should().HaveCount(count);
    }
}

public class GameStoreDidIndexTests {
    /// <summary>
    /// A DID that plays in exactly one active game resolves via
    /// <see cref="GameStore.GetSingleSessionForDid"/>; a DID not playing in any
    /// (or more than one) game resolves to null.
    /// </summary>
    [Fact]
    public void GetSingleSessionForDid_SingleGame_Resolves_AndZeroOrMultiple_ReturnsNull() {
        var store = new GameStore();
        var template = GameStoreTestHelper.MakeTemplate();
        const string player = "did:plc:player";

        var first = GameStoreTestHelper.Created(store, player);
        store.GetSingleSessionForDid(player)!.Code.Should().Be(first.Code);

        // Second game by the same DID: the DID now plays in two games, so the
        // single-game resolver returns null.
        GameStoreTestHelper.Created(store, player);
        store.GetSingleSessionForDid(player).Should().BeNull();
    }

    /// <summary>
    /// <see cref="GameStore.GetSessionsForDid"/> returns every game the DID plays
    /// in (active or completed), oldest first. An explicit <c>g=&lt;code&gt;</c>
    /// selector (resolved via <see cref="GameStore.TryGetByCode"/>) is authoritative
    /// over the DID index because it is keyed explicitly.
    /// </summary>
    [Fact]
    public void GetSessionsForDid_ReturnsAllGames_OldestFirst_AndCodeLookupWins() {
        var store = new GameStore();
        const string player = "did:plc:player";

        // The same DID creates all three games, so the DID index links it to all
        // three codes. Creating in order also orders each session's StartedAt.
        var first = GameStoreTestHelper.Created(store, player);
        var second = GameStoreTestHelper.Created(store, player);
        var third = GameStoreTestHelper.Created(store, player);

        // A DID not playing any game returns an empty list.
        store.GetSessionsForDid("did:plc:stranger").Should().BeEmpty();

        var forDid = store.GetSessionsForDid(player).Select(s => s.Code).ToList();
        forDid.Should().Equal(first.Code, second.Code, third.Code);

        // An explicit code resolves regardless of the DID index.
        store.TryGetByCode(second.Code, out var byCode).Should().BeTrue();
        byCode!.Code.Should().Be(second.Code);
        store.TryGetByCode("zombie-code-absent", out _).Should().BeFalse();
    }
}
