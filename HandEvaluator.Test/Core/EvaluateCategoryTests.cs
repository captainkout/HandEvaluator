using HoldemHand;
using Shouldly;
using Xunit;

namespace HandEvaluator.Test;

/// <summary>
/// P0-A: every <see cref="Hand.HandTypes"/> value is produced correctly for
/// 5/6/7-card hands, and the fast category-only APIs agree with <see cref="Hand.Evaluate"/>.
/// </summary>
[Trait(Traits.Category, Traits.Fast)]
public class EvaluateCategoryTests
{
    [Theory]
    [MemberData(nameof(KnownHands.AllCategoryExamples), MemberType = typeof(KnownHands))]
    public void Evaluate_ProducesExpectedHandType(string hand, Hand.HandTypes expected)
    {
        var mask = Hand.ParseHand(hand);

        ((Hand.HandTypes)Hand.HandType(Hand.Evaluate(mask))).ShouldBe(expected);
        ((Hand.HandTypes)Hand.HandType(Hand.Evaluate(hand))).ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(KnownHands.AllCategoryExamples), MemberType = typeof(KnownHands))]
    public void EvaluateType_AgreesWithEvaluateAndHandType(string hand, Hand.HandTypes expected)
    {
        var mask = Hand.ParseHand(hand);

        Hand.EvaluateType(mask).ShouldBe(expected);
        Hand.EvaluateType(mask).ShouldBe((Hand.HandTypes)Hand.HandType(Hand.Evaluate(mask)));
    }

    [Theory]
    [MemberData(nameof(KnownHands.AllCategoryExamples), MemberType = typeof(KnownHands))]
    public void EvaluateType_OverloadsAgree(string hand, Hand.HandTypes expected)
    {
        var mask = Hand.ParseHand(hand);

        Hand.EvaluateType(mask).ShouldBe(Hand.EvaluateType(mask, Hand.BitCount(mask)));
        Hand.EvaluateType(mask, Hand.BitCount(mask)).ShouldBe(expected);
    }

    [Fact]
    public void RoyalFlush_IsStraightFlush_ForEverySuit()
    {
        // Royal flush = ace-high straight flush, one per suit.
        var royals = new[]
        {
            "Ac Kc Qc Jc Tc",
            "Ad Kd Qd Jd Td",
            "Ah Kh Qh Jh Th",
            "As Ks Qs Js Ts",
        };

        foreach (var royal in royals)
        {
            var mask = Hand.ParseHand(royal);
            Hand.EvaluateType(mask).ShouldBe(Hand.HandTypes.StraightFlush);
            ((Hand.HandTypes)Hand.HandType(Hand.Evaluate(mask))).ShouldBe(
                Hand.HandTypes.StraightFlush
            );
            Hand.TopCard(Hand.Evaluate(mask)).ShouldBe((uint)Hand.RankAce);
        }
    }
}
