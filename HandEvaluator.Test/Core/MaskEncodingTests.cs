using HoldemHand;
using Shouldly;
using Xunit;

namespace HandEvaluator.Test;

/// <summary>
/// P0-D: the bitmask encoding is the contract every other API depends on.
/// Covers single-bit masks, tables, parse/format round-trips, suit/rank offsets,
/// <see cref="Hand.BitCount"/>, <see cref="Hand.ValidateHand"/> and the
/// <see cref="Hand"/> property graph.
/// </summary>
[Trait(Traits.Category, Traits.Fast)]
public class MaskEncodingTests
{
    [Fact]
    public void Mask_IsAlwaysASingleBit()
    {
        for (int i = 0; i < 52; i++)
        {
            Hand.Mask(i).ShouldBe(1UL << i);
            Hand.BitCount(Hand.Mask(i)).ShouldBe(1);
        }
    }

    [Fact]
    public void CardMasksTable_MatchesMask()
    {
        for (int i = 0; i < 52; i++)
            Hand.CardMasksTable[i].ShouldBe(Hand.Mask(i));
    }

    [Fact]
    public void LookupTables_HaveExpectedLengths()
    {
        Hand.CardMasksTable.Length.ShouldBe(52);
        Hand.CardTable.Length.ShouldBe(52);
    }

    [Fact]
    public void EveryCard_ParseFormatRoundTrips()
    {
        for (int i = 0; i < 52; i++)
        {
            string card = Hand.CardTable[i];
            ulong mask = Hand.ParseHand(card);

            mask.ShouldBe(Hand.Mask(i));
            Hand.ParseCard(card).ShouldBe(i);

            // format -> parse -> format round trip
            Hand.MaskToString(mask).ShouldBe(card);
            Hand.ParseHand(Hand.MaskToString(mask)).ShouldBe(mask);
        }
    }

    [Fact]
    public void CardSuitAndRank_MapTwoToEachSuitAndRank()
    {
        Hand.CardSuit(Hand.ParseCard("2c")).ShouldBe(Hand.Clubs);
        Hand.CardSuit(Hand.ParseCard("2d")).ShouldBe(Hand.Diamonds);
        Hand.CardSuit(Hand.ParseCard("2h")).ShouldBe(Hand.Hearts);
        Hand.CardSuit(Hand.ParseCard("2s")).ShouldBe(Hand.Spades);

        Hand.CardRank(Hand.ParseCard("2c")).ShouldBe(Hand.Rank2);
        Hand.CardRank(Hand.ParseCard("Tc")).ShouldBe(Hand.RankTen);
        Hand.CardRank(Hand.ParseCard("Jc")).ShouldBe(Hand.RankJack);
        Hand.CardRank(Hand.ParseCard("Qc")).ShouldBe(Hand.RankQueen);
        Hand.CardRank(Hand.ParseCard("Kc")).ShouldBe(Hand.RankKing);
        Hand.CardRank(Hand.ParseCard("Ac")).ShouldBe(Hand.RankAce);
    }

    [Fact]
    public void Ranks_AreContiguousWithinASuit()
    {
        // CardTable is ordered by suit then rank; ranks 2..A must be consecutive.
        for (int i = 0; i < 12; i++)
            (Hand.ParseCard(Hand.CardTable[i + 1]) - Hand.ParseCard(Hand.CardTable[i])).ShouldBe(1);
    }

    [Theory]
    [InlineData("As Ad Ks Kd Qh", 5)]
    [InlineData("2c 3d 4h 5s 6c 7d 8h", 7)]
    [InlineData("Ac", 1)]
    public void ParseHand_RefCards_CountsDistinctCards(string hand, int expectedCards)
    {
        int cards = 0;
        ulong mask = Hand.ParseHand(hand, ref cards);

        cards.ShouldBe(expectedCards);
        Hand.BitCount(mask).ShouldBe(expectedCards);
        mask.ShouldBe(Hand.ParseHand(hand));
    }

    [Fact]
    public void ParseHand_PocketBoardOverload_MatchesConcatenation()
    {
        int cards = 0;
        ulong mask = Hand.ParseHand("As Ad", "Ks Kd Qh", ref cards);

        cards.ShouldBe(5);
        Hand.BitCount(mask).ShouldBe(5);
        mask.ShouldBe(Hand.ParseHand("As Ad Ks Kd Qh"));
    }

    [Fact]
    public void BitCount_Boundaries()
    {
        Hand.BitCount(0UL).ShouldBe(0);
        Hand.BitCount((1UL << 52) - 1).ShouldBe(52);
        Hand.BitCount(ulong.MaxValue).ShouldBe(64);
        Hand.BitCount(Hand.Mask(0) | Hand.Mask(13) | Hand.Mask(51)).ShouldBe(3);
    }

    [Theory]
    [InlineData("Ad Ah Qh Qd 2s")]
    [InlineData("2d 2h")]
    [InlineData("10c")]
    [InlineData("Ac 2d 3h 4s 5c 6d 7h")]
    public void ValidateHand_AcceptsWellFormedHands(string hand)
    {
        Hand.ValidateHand(hand).ShouldBeTrue();
    }

    [Theory]
    [InlineData("As As")] // duplicate
    [InlineData("Ak Ak Ah Ah Qs")] // duplicate, mixed case
    [InlineData("Boogers")] // malformed token
    [InlineData("As Q")] // rank without suit
    [InlineData("As 1c")] // bad "10" prefix
    public void ValidateHand_RejectsDuplicatesAndMalformedTokens(string hand)
    {
        Hand.ValidateHand(hand).ShouldBeFalse();
    }

    [Fact]
    public void ValidateHand_EmptyAndNull_AreRejected()
    {
        Hand.ValidateHand("").ShouldBeFalse();
        Hand.ValidateHand("   ").ShouldBeFalse();
        Hand.ValidateHand((string)null).ShouldBeFalse();
    }

    [Fact]
    public void ValidateHand_DoesNotEnforceSevenCardMaximum_Characterization()
    {
        // Characterization: ValidateHand checks token syntax and duplicate cards only;
        // it does NOT cap the card count. The evaluator itself caps at 7 (see Debug tests).
        Hand.ValidateHand("Ac 2d 3h 4s 5c 6d 7h 8s").ShouldBeTrue();
        Hand.ValidateHand("Ac 2d 3h 4s 5c 6d 7h 8s 9c").ShouldBeTrue();
    }

    [Fact]
    public void Hand_PropertyGraph_IsConsistent()
    {
        var hand = new Hand("As Ad", "Ks Kd Qh");

        hand.PocketCards.ShouldBe("As Ad");
        hand.Board.ShouldBe("Ks Kd Qh");

        hand.PocketMask.ShouldBe(Hand.ParseHand("As Ad"));
        hand.BoardMask.ShouldBe(Hand.ParseHand("Ks Kd Qh"));

        hand.MaskValue.ShouldBe(Hand.ParseHand("As Ad Ks Kd Qh"));
        hand.MaskValue.ShouldBe(hand.PocketMask | hand.BoardMask);

        hand.HandValue.ShouldBe(Hand.Evaluate(hand.MaskValue));
        hand.HandTypeValue.ShouldBe((Hand.HandTypes)Hand.HandType(hand.HandValue));
        hand.Description.ShouldBe(Hand.DescriptionFromMask(hand.MaskValue));
    }

#if DEBUG
    [Fact]
    public void ValidateHand_PocketBoardOverload_ThrowsOnNullOrEmpty_InDebug()
    {
        Should.Throw<System.ArgumentNullException>(() => Hand.ValidateHand(null, "Ks"));
        Should.Throw<System.ArgumentNullException>(() => Hand.ValidateHand("As", null));
        Should.Throw<System.ArgumentNullException>(() => Hand.ValidateHand("", "Ks"));
        Should.Throw<System.ArgumentNullException>(() => Hand.ValidateHand("As", "  "));
    }

    [Fact]
    public void ParseHand_DuplicateCards_ThrowsInDebug()
    {
        Should.Throw<System.ArgumentException>(() => Hand.ParseHand("As As"));
    }

    [Fact]
    public void Evaluate_InvalidCardCount_ThrowsInDebug()
    {
        ulong eightCards = (1UL << 8) - 1;

        Should.Throw<System.ArgumentOutOfRangeException>(() => Hand.Evaluate(eightCards, 8));
        Should.Throw<System.ArgumentOutOfRangeException>(() => Hand.Evaluate(eightCards, 0));
        Should.Throw<System.ArgumentOutOfRangeException>(() => Hand.Evaluate(eightCards));
    }

    [Fact]
    public void EvaluateType_EmptyMask_ThrowsInDebug()
    {
        // Only the one-argument overload validates the card count in Debug; the
        // two-argument overload performs no validation (characterized below).
        Should.Throw<System.ArgumentException>(() => Hand.EvaluateType(0UL));
    }

    [Fact]
    public void DescriptionFromMask_NoCards_ThrowsInDebug()
    {
        Should.Throw<System.ArgumentOutOfRangeException>(() => Hand.DescriptionFromMask(0UL));
    }

    [Fact]
    public void CardRankAndSuit_OutOfRange_ThrowInDebug()
    {
        Should.Throw<System.ArgumentOutOfRangeException>(() => Hand.CardRank(-1));
        Should.Throw<System.ArgumentOutOfRangeException>(() => Hand.CardSuit(53));
    }
#endif
}
