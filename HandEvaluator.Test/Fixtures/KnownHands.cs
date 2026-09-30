using System.Collections.Generic;
using HoldemHand;

namespace HandEvaluator.Test
{
    /// <summary>
    /// Human-checkable hand fixtures used as <c>[Theory]</c> data. Every entry is a
    /// self-evident hand whose category/order can be verified without an oracle.
    /// </summary>
    public static class KnownHands
    {
        /// <summary>
        /// One representative of every <see cref="Hand.HandTypes"/> value at 5, 6 and 7
        /// cards (including royals). Used for category-boundary coverage.
        /// </summary>
        public static IEnumerable<object[]> AllCategoryExamples
        {
            get
            {
                // 5-card
                yield return new object[] { "2c 7d 9h Js Kd", Hand.HandTypes.HighCard };
                yield return new object[] { "2c 2d 7h Js Kd", Hand.HandTypes.Pair };
                yield return new object[] { "2c 2d 7h 7s Kd", Hand.HandTypes.TwoPair };
                yield return new object[] { "2c 2d 2h Js Kd", Hand.HandTypes.Trips };
                yield return new object[] { "5c 6d 7h 8s 9d", Hand.HandTypes.Straight };
                yield return new object[] { "2c 5c 7c Jc Kc", Hand.HandTypes.Flush };
                yield return new object[] { "2c 2d 2h 7s 7d", Hand.HandTypes.FullHouse };
                yield return new object[] { "2c 2d 2h 2s Kd", Hand.HandTypes.FourOfAKind };
                yield return new object[] { "5c 6c 7c 8c 9c", Hand.HandTypes.StraightFlush };
                yield return new object[] { "Ac Kc Qc Jc Tc", Hand.HandTypes.StraightFlush };

                // 6-card
                yield return new object[] { "2c 7d 9h Js Kd 4h", Hand.HandTypes.HighCard };
                yield return new object[] { "2c 2d 7h Js Kd 4h", Hand.HandTypes.Pair };
                yield return new object[] { "2c 2d 7h 7s Kd 4h", Hand.HandTypes.TwoPair };
                yield return new object[] { "2c 2d 2h Js Kd 4h", Hand.HandTypes.Trips };
                yield return new object[] { "5c 6d 7h 8s 9d Tc", Hand.HandTypes.Straight };
                yield return new object[] { "2c 5c 7c Jc Kc 9c", Hand.HandTypes.Flush };
                yield return new object[] { "2c 2d 2h 7s 7d 9c", Hand.HandTypes.FullHouse };
                yield return new object[] { "2c 2d 2h 2s Kd 9c", Hand.HandTypes.FourOfAKind };
                yield return new object[] { "5c 6c 7c 8c 9c Td", Hand.HandTypes.StraightFlush };
                yield return new object[] { "Ac Kc Qc Jc Tc 9c", Hand.HandTypes.StraightFlush };

                // 7-card
                yield return new object[] { "2c 7d 9h Js Kd 4h 3c", Hand.HandTypes.HighCard };
                yield return new object[] { "2c 2d 7h Js Kd 4h 3c", Hand.HandTypes.Pair };
                yield return new object[] { "2c 2d 7h 7s Kd 4h 3c", Hand.HandTypes.TwoPair };
                yield return new object[] { "2c 2d 2h Js Kd 4h 3c", Hand.HandTypes.Trips };
                yield return new object[] { "5c 6d 7h 8s 9d Td 2c", Hand.HandTypes.Straight };
                yield return new object[] { "2c 5c 7c Jc Kc 9c 3h", Hand.HandTypes.Flush };
                yield return new object[] { "2c 2d 2h 7s 7d 9c 3h", Hand.HandTypes.FullHouse };
                yield return new object[] { "2c 2d 2h 2s Kd 9c 3h", Hand.HandTypes.FourOfAKind };
                yield return new object[] { "5c 6c 7c 8c 9c 2d 3h", Hand.HandTypes.StraightFlush };
                yield return new object[] { "Ac Kc Qc Jc Tc 2d 3h", Hand.HandTypes.StraightFlush };
            }
        }

        /// <summary>Pairs of same-category hands where the first is strictly weaker.</summary>
        public static IEnumerable<object[]> OrderedHandPairs
        {
            get
            {
                // Pair: kicker ordering
                yield return new object[] { "Ac Ad Qc 7h 2s", "Ac Ad Kc 7h 2s" };
                yield return new object[] { "Ac Ad Jc 7h 2s", "Ac Ad Qc 7h 2s" };

                // Two pair: higher second pair wins
                yield return new object[] { "Ac Ad Qc Qh Ks", "Ac Ad Kc Kh 2s" };

                // Trips: kicker ordering
                yield return new object[] { "Ac Ad Ah Qc Js", "Ac Ad Ah Kc 2s" };

                // Quads: kicker ordering
                yield return new object[] { "Ac Ad Ah As Qc", "Ac Ad Ah As Kc" };

                // Full house: trips rank dominates, then pair rank
                yield return new object[] { "Qc Qd Qh Ac Ad", "Kc Kd Kh 2c 2d" };
                yield return new object[] { "Kc Kd Kh 2c 2d", "Kc Kd Kh Ac Ad" };

                // Flush: fifth card ordering
                yield return new object[] { "Ac Kc Qc Jc 8c", "Ac Kc Qc Jc 9c" };

                // Straight: 6-high beats the wheel
                yield return new object[] { "Ac 2d 3h 4s 5c", "2c 3d 4h 5s 6c" };

                // Straight flush: 6-high beats the steel wheel
                yield return new object[] { "Ac 2c 3c 4c 5c", "2c 3c 4c 5c 6c" };

                // Straight flush: royal beats king-high
                yield return new object[] { "Kc Qc Jc Tc 9c", "Ac Kc Qc Jc Tc" };
            }
        }

        /// <summary>Pairs of hands with identical ranks in different suits (equal values).</summary>
        public static IEnumerable<object[]> EqualHandPairs
        {
            get
            {
                yield return new object[] { "Ac Ad Kc Kh Qs", "Ah As Kd Ks Qd" };
                yield return new object[] { "2c 2d 7h Js Kd", "2h 2s 7d Jc Kh" };
                yield return new object[] { "Ac Ad Ah Kc 2s", "Ah As Ad Kd 2h" };
                yield return new object[] { "Ac Ad Ah As Kc", "Ah As Ad Ac Kd" };
                yield return new object[] { "2c 5c 7c Jc Kc", "2s 5s 7s Js Ks" };
                yield return new object[] { "5c 6c 7c 8c 9c", "5s 6s 7s 8s 9s" };
                yield return new object[] { "5c 6d 7h 8s 9c", "5d 6s 7c 8h 9d" };
            }
        }

        /// <summary>Representative hands (one per category plus wheel/steel wheel) for permutation tests.</summary>
        public static IEnumerable<object[]> RepresentativeHands
        {
            get
            {
                yield return new object[] { "2c 7d 9h Js Kd", Hand.HandTypes.HighCard };
                yield return new object[] { "2c 2d 7h Js Kd", Hand.HandTypes.Pair };
                yield return new object[] { "2c 2d 7h 7s Kd", Hand.HandTypes.TwoPair };
                yield return new object[] { "2c 2d 2h Js Kd", Hand.HandTypes.Trips };
                yield return new object[] { "5c 6d 7h 8s 9d", Hand.HandTypes.Straight };
                yield return new object[] { "2c 5c 7c Jc Kc", Hand.HandTypes.Flush };
                yield return new object[] { "2c 2d 2h 7s 7d", Hand.HandTypes.FullHouse };
                yield return new object[] { "2c 2d 2h 2s Kd", Hand.HandTypes.FourOfAKind };
                yield return new object[] { "5c 6c 7c 8c 9c", Hand.HandTypes.StraightFlush };
                yield return new object[] { "Ac Kc Qc Jc Tc", Hand.HandTypes.StraightFlush };
                yield return new object[] { "Ac 2d 3h 4s 5c", Hand.HandTypes.Straight }; // wheel
                yield return new object[] { "Ac 2c 3c 4c 5c", Hand.HandTypes.StraightFlush }; // steel wheel
            }
        }

        /// <summary>
        /// Stable, well-known description strings for the non-flush categories
        /// (flush/straight-flush descriptions include suit names and are asserted by substring instead).
        /// </summary>
        public static IEnumerable<object[]> DescriptionExamples
        {
            get
            {
                yield return new object[] { "2c 7d 9h Js Kd", "High card: King" };
                yield return new object[] { "Ac Ad Kc 7h 2s", "One pair, Ace" };
                yield return new object[]
                {
                    "Ac Ad Kc Kh Qs",
                    "Two pair, Ace's and King's with a Queen for a kicker",
                };
                yield return new object[] { "Ac Ad Ah Kc 2s", "Three of a kind, Ace's" };
                yield return new object[] { "5c 6d 7h 8s 9d", "A straight, Nine high" };
                yield return new object[] { "Kc Kd Kh 2c 2d", "A fullhouse, King's and Two's" };
                yield return new object[] { "Ac Ad Ah As Kc", "Four of a kind, Ace's" };
            }
        }
    }
}
