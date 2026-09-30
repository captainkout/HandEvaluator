using System.Collections.Generic;
using System.Linq;
using HoldemHand;
using Shouldly;
using Xunit;

namespace HandEvaluator.Test;

/// <summary>
/// P0-E: hand values must not depend on which suit is which. For a representative
/// hand from every category (plus wheel and steel wheel) all 24 suit relabelings must
/// yield identical <see cref="Hand.Evaluate"/> values and unchanged type/top card.
/// </summary>
[Trait(Traits.Category, Traits.Fast)]
public class SuitPermutationTests
{
    [Fact]
    public void SuitPermutations_AreExactly24DistinctRelabelings()
    {
        var permutations = CardMasks
            .SuitPermutations()
            .Select(p => string.Join(",", p))
            .ToHashSet();

        permutations.Count.ShouldBe(24);
    }

    [Theory]
    [MemberData(nameof(KnownHands.RepresentativeHands), MemberType = typeof(KnownHands))]
    public void Evaluate_IsInvariantUnderSuitPermutation(string hand, Hand.HandTypes expected)
    {
        ulong mask = Hand.ParseHand(hand);
        uint baseValue = Hand.Evaluate(mask);
        uint baseTopCard = Hand.TopCard(baseValue);

        Hand.EvaluateType(mask).ShouldBe(expected);

        foreach (int[] permutation in CardMasks.SuitPermutations())
        {
            ulong permuted = CardMasks.PermuteSuits(mask, permutation);

            Hand.Evaluate(permuted).ShouldBe(baseValue);
            Hand.EvaluateType(permuted).ShouldBe(expected);
            Hand.TopCard(Hand.Evaluate(permuted)).ShouldBe(baseTopCard);
            Hand.BitCount(permuted).ShouldBe(Hand.BitCount(mask));
        }
    }
}
