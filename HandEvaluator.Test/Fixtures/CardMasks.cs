using System.Collections.Generic;
using System.Linq;
using HoldemHand;

namespace HandEvaluator.Test;

/// <summary>
/// Test-only helpers for working with card masks: parsing, suit permutations
/// and binomial counts. This class does not touch library source.
/// </summary>
public static class CardMasks
{
    /// <summary>Parse a space-separated card notation list into a hand mask.</summary>
    public static ulong Parse(string cards) => Hand.ParseHand(cards);

    /// <summary>Parse each notation string in <paramref name="hands"/>.</summary>
    public static IReadOnlyList<ulong> ParseAll(params string[] hands) =>
        hands.Select(Hand.ParseHand).ToList();

    /// <summary>
    /// Wraps a card mask in a <see cref="Hand"/> instance. The whole mask is stored
    /// as the pocket so <see cref="Hand.HandValue"/> is evaluated for exactly those cards.
    /// </summary>
    public static Hand MakeHand(ulong mask)
    {
        var hand = new Hand();
        hand.PocketMask = mask;
        return hand;
    }

    /// <summary>Wraps a card notation string in a <see cref="Hand"/> instance.</summary>
    public static Hand MakeHand(string cards) => MakeHand(Hand.ParseHand(cards));

    /// <summary>
    /// Relabels the four suits of <paramref name="mask"/> using
    /// <paramref name="permutation"/> (old suit index -&gt; new suit index).
    /// </summary>
    public static ulong PermuteSuits(ulong mask, int[] permutation)
    {
        ulong result = 0UL;
        for (int i = 0; i < 52; i++)
        {
            if ((mask & (1UL << i)) == 0UL)
                continue;

            int rank = i % 13;
            int suit = i / 13;
            result |= 1UL << (rank + permutation[suit] * 13);
        }
        return result;
    }

    /// <summary>All 24 permutations of the four suit indices.</summary>
    public static IEnumerable<int[]> SuitPermutations() => Permute(new[] { 0, 1, 2, 3 });

    /// <summary>Binomial coefficient C(n, k) using integer arithmetic.</summary>
    public static long Binom(int n, int k)
    {
        if (k < 0 || k > n)
            return 0L;
        if (k > n - k)
            k = n - k;

        long result = 1L;
        for (int i = 1; i <= k; i++)
            result = result * (n - k + i) / i;
        return result;
    }

    private static IEnumerable<int[]> Permute(int[] items)
    {
        if (items.Length == 1)
        {
            yield return (int[])items.Clone();
            yield break;
        }

        for (int i = 0; i < items.Length; i++)
        {
            int head = items[i];
            int[] rest = items.Where((_, idx) => idx != i).ToArray();
            foreach (int[] tail in Permute(rest))
                yield return new[] { head }.Concat(tail).ToArray();
        }
    }
}
