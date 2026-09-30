using HoldemHand;
using Shouldly;
using Xunit;

namespace HandEvaluator.Test;

/// <summary>
/// P0-F: the comparison/equality API and the description API. Operators, Equals and
/// GetHashCode must be consistent with <see cref="Hand.Evaluate"/>, and the description
/// helpers must be cross-consistent (substring/cross-API checks preferred over exact prose).
/// </summary>
[Trait(Traits.Category, Traits.Fast)]
public class CompareAndDescriptionTests
{
    [Theory]
    [MemberData(nameof(KnownHands.OrderedHandPairs), MemberType = typeof(KnownHands))]
    public void AllComparisonOperators_AgreeWithEvaluate(string weaker, string stronger)
    {
        var w = CardMasks.MakeHand(weaker);
        var s = CardMasks.MakeHand(stronger);

        (w < s).ShouldBeTrue();
        (w <= s).ShouldBeTrue();
        (w > s).ShouldBeFalse();
        (w >= s).ShouldBeFalse();
        (s > w).ShouldBeTrue();
        (s >= w).ShouldBeTrue();
        (s < w).ShouldBeFalse();
        (s <= w).ShouldBeFalse();
        (w == s).ShouldBeFalse();
        (w != s).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(KnownHands.EqualHandPairs), MemberType = typeof(KnownHands))]
    public void EqualityOperatorsAndObjectMembers_AgreeForEqualHands(string a, string b)
    {
        var ha = CardMasks.MakeHand(a);
        var hb = CardMasks.MakeHand(b);

        (ha == hb).ShouldBeTrue();
        (ha != hb).ShouldBeFalse();
        (ha <= hb).ShouldBeTrue();
        (ha >= hb).ShouldBeTrue();
        ha.Equals(hb).ShouldBeTrue();
        ha.GetHashCode().ShouldBe(hb.GetHashCode());
    }

    [Fact]
    public void CompareTo_IsBroken_LatentDefect_Characterization()
    {
        // Latent defect (not fixed: library source must not change):
        //   public int CompareTo(object obj)
        //   {
        //       Hand h = obj as Hand;
        //       if (h == null) return -1;   // <-- invokes operator== with a null operand
        //       ...
        //   }
        // The user-defined == operator dereferences both operands, so the null check
        // inside CompareTo throws instead of returning -1. This happens for every input.
        //   Debug  -> System.ArgumentNullException (operator== has the #if DEBUG null guard)
        //   Release-> System.NullReferenceException (operator== dereferences the null operand)
        var weaker = CardMasks.MakeHand("Ac Ad Qc 7h 2s");
        var stronger = CardMasks.MakeHand("Ac Ad Kc 7h 2s");

        Should.Throw<System.SystemException>(() => weaker.CompareTo(stronger));
        Should.Throw<System.SystemException>(() => weaker.CompareTo(null));
    }

    [Theory]
    [MemberData(nameof(KnownHands.DescriptionExamples), MemberType = typeof(KnownHands))]
    public void DescriptionApis_AreCrossConsistent_ForNonFlushCategories(
        string hand,
        string expected
    )
    {
        ulong mask = Hand.ParseHand(hand);

        Hand.DescriptionFromHand(hand).ShouldBe(expected);
        Hand.DescriptionFromMask(mask).ShouldBe(expected);

#pragma warning disable CS0618 // DescriptionFromHandValue/MaskToDescription are obsolete but still part of the API surface
        Hand.DescriptionFromHandValue(Hand.Evaluate(mask)).ShouldBe(expected);
        Hand.MaskToDescription(mask).ShouldBe(expected);
#pragma warning restore CS0618
    }

    [Fact]
    public void Flush_DescriptionIncludesSuitAndHighCard()
    {
        const string hand = "2c 5c 7c Jc Kc";
        ulong mask = Hand.ParseHand(hand);

        Hand.DescriptionFromHand(hand).ShouldContain("Flush");
        Hand.DescriptionFromHand(hand).ShouldContain("Clubs");
        Hand.DescriptionFromHand(hand).ShouldContain("King");
        Hand.DescriptionFromMask(mask).ShouldBe(Hand.DescriptionFromHand(hand));

#pragma warning disable CS0618
        // DescriptionFromHandValue intentionally gives the generic form; MaskToDescription
        // delegates to DescriptionFromMask.
        Hand.DescriptionFromHandValue(Hand.Evaluate(mask)).ShouldBe("A flush");
        Hand.MaskToDescription(mask).ShouldBe(Hand.DescriptionFromMask(mask));
#pragma warning restore CS0618
    }

    [Fact]
    public void StraightFlush_DescriptionIncludesHighCard()
    {
        const string hand = "5c 6c 7c 8c 9c";
        ulong mask = Hand.ParseHand(hand);

        Hand.DescriptionFromHand(hand).ShouldContain("Nine");
        Hand.DescriptionFromMask(mask).ShouldContain("Nine");

#pragma warning disable CS0618
        Hand.DescriptionFromHandValue(Hand.Evaluate(mask)).ShouldBe("A straight flush");
#pragma warning restore CS0618
    }

    [Fact]
    public void ToString_ReturnsCardNotation_NotDescription_Characterization()
    {
        // Characterization: ToString() returns "<pocket> <board>", NOT the hand description.
        var hand = new Hand("As Ad", "Ks Kd Qh");

        hand.ToString().ShouldBe("As Ad Ks Kd Qh");
        hand.ToString().ShouldBe(hand.PocketCards + " " + hand.Board);
        hand.ToString().ShouldNotBe(hand.Description);
    }

    [Fact]
    public void HandDescription_MatchesDescriptionFromMask()
    {
        var hand = new Hand("As Ad", "Ks Kd Qh");

        hand.Description.ShouldBe(Hand.DescriptionFromMask(hand.MaskValue));
        hand.Description.ShouldContain("Two pair");
    }
}
