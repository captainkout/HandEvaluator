using HoldemHand;
using Shouldly;
using Xunit;

namespace HandEvaluator.Test;

/// <summary>
/// P0-B: within a category, higher kickers / top cards order correctly, and
/// rank-identical hands in different suits must have exactly equal values.
/// </summary>
[Trait(Traits.Category, Traits.Fast)]
public class EvaluateKickerTests
{
    [Theory]
    [MemberData(nameof(KnownHands.OrderedHandPairs), MemberType = typeof(KnownHands))]
    public void Evaluate_OrdersSameCategoryHands(string weaker, string stronger)
    {
        Hand.Evaluate(weaker).ShouldBeLessThan(Hand.Evaluate(stronger));

        // Both hands must stay in the same category; ordering must be about kickers only.
        ((Hand.HandTypes)Hand.HandType(Hand.Evaluate(weaker))).ShouldBe(
            (Hand.HandTypes)Hand.HandType(Hand.Evaluate(stronger))
        );
    }

    [Theory]
    [MemberData(nameof(KnownHands.OrderedHandPairs), MemberType = typeof(KnownHands))]
    public void Operators_AgreeWithEvaluateOnOrderedHands(string weaker, string stronger)
    {
        var w = CardMasks.MakeHand(weaker);
        var s = CardMasks.MakeHand(stronger);

        (w < s).ShouldBeTrue();
        (w <= s).ShouldBeTrue();
        (s > w).ShouldBeTrue();
        (s >= w).ShouldBeTrue();
        (w == s).ShouldBeFalse();
        (w != s).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(KnownHands.EqualHandPairs), MemberType = typeof(KnownHands))]
    public void Evaluate_EqualForSameRanksInDifferentSuits(string a, string b)
    {
        var ma = Hand.ParseHand(a);
        var mb = Hand.ParseHand(b);

        Hand.Evaluate(ma).ShouldBe(Hand.Evaluate(mb));
        Hand.EvaluateType(ma).ShouldBe(Hand.EvaluateType(mb));
        Hand.TopCard(Hand.Evaluate(ma)).ShouldBe(Hand.TopCard(Hand.Evaluate(mb)));
    }

    [Theory]
    [MemberData(nameof(KnownHands.EqualHandPairs), MemberType = typeof(KnownHands))]
    public void EqualRankHands_AreEqualUnderHandAPIs(string a, string b)
    {
        var ha = CardMasks.MakeHand(a);
        var hb = CardMasks.MakeHand(b);

        ha.HandValue.ShouldBe(hb.HandValue);
        (ha == hb).ShouldBeTrue();
        (ha != hb).ShouldBeFalse();
        ha.Equals(hb).ShouldBeTrue();
        ha.GetHashCode().ShouldBe(hb.GetHashCode());
    }
}
