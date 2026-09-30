using HoldemHand;
using Shouldly;
using Xunit;

namespace HandEvaluator.Test
{
    /// <summary>
    /// P0-C: ace-low straights ("wheel"/"steel wheel"), the ace-high straight boundary,
    /// and the fact that the wheel's high card is a five (not an ace).
    /// </summary>
    [Trait(Traits.Category, Traits.Fast)]
    public class WheelAndStraightFlushTests
    {
        [Fact]
        public void Wheel_IsStraight_WithFiveAsHighCard()
        {
            var mask = Hand.ParseHand("Ac 2d 3h 4s 5c");

            Hand.EvaluateType(mask).ShouldBe(Hand.HandTypes.Straight);
            ((Hand.HandTypes)Hand.HandType(Hand.Evaluate(mask))).ShouldBe(Hand.HandTypes.Straight);

            // The wheel is five-high, NOT ace-high.
            Hand.TopCard(Hand.Evaluate(mask)).ShouldBe((uint)Hand.Rank5);
            Hand.TopCard(Hand.Evaluate(mask)).ShouldNotBe((uint)Hand.RankAce);
        }

        [Fact]
        public void SixHighStraight_BeatsWheel()
        {
            var wheel = Hand.Evaluate("Ac 2d 3h 4s 5c");
            var sixHigh = Hand.Evaluate("2c 3d 4h 5s 6c");

            sixHigh.ShouldBeGreaterThan(wheel);
            Hand.EvaluateType(Hand.ParseHand("2c 3d 4h 5s 6c")).ShouldBe(Hand.HandTypes.Straight);
        }

        [Fact]
        public void SteelWheel_IsStraightFlush()
        {
            var mask = Hand.ParseHand("Ac 2c 3c 4c 5c");

            Hand.EvaluateType(mask).ShouldBe(Hand.HandTypes.StraightFlush);
            Hand.TopCard(Hand.Evaluate(mask)).ShouldBe((uint)Hand.Rank5);
        }

        [Fact]
        public void SixHighStraightFlush_BeatsSteelWheel()
        {
            Hand.Evaluate("2c 3c 4c 5c 6c").ShouldBeGreaterThan(Hand.Evaluate("Ac 2c 3c 4c 5c"));
        }

        [Fact]
        public void RoyalFlush_BeatsKingHighStraightFlush()
        {
            var royal = Hand.Evaluate("Ac Kc Qc Jc Tc");
            var kingHigh = Hand.Evaluate("Kc Qc Jc Tc 9c");

            royal.ShouldBeGreaterThan(kingHigh);
            Hand.EvaluateType(Hand.ParseHand("Ac Kc Qc Jc Tc"))
                .ShouldBe(Hand.HandTypes.StraightFlush);
        }

        [Theory]
        [InlineData("Ac 2d 3h 4s 6c")] // four to a wheel, five missing
        [InlineData("Ac Kd Qh Js 9c")] // four to broadway, ten missing
        [InlineData("Ac 2d 3h 4s 7c")] // disconnected low cards
        public void DisconnectedOrIncompleteStraights_StayHighCard(string hand)
        {
            Hand.EvaluateType(Hand.ParseHand(hand)).ShouldBe(Hand.HandTypes.HighCard);
        }

        [Theory]
        [InlineData("Ac 2d 3h 4s 5c 9d")] // 6-card wheel
        [InlineData("Ac 2d 3h 4s 5c 9d Kh")] // 7-card wheel
        public void Wheel_IsRecognisedWithExtraCards(string hand)
        {
            var mask = Hand.ParseHand(hand);

            Hand.EvaluateType(mask).ShouldBe(Hand.HandTypes.Straight);
            Hand.TopCard(Hand.Evaluate(mask)).ShouldBe((uint)Hand.Rank5);
        }

        [Fact]
        public void PairedBoardStraight_IsStraightNotTrips()
        {
            // 5,5,6,7,8,9,A -> best hand is the nine-high straight, not a pair/trips.
            var mask = Hand.ParseHand("5c 5d 6h 7s 8d 9c Ac");

            Hand.EvaluateType(mask).ShouldBe(Hand.HandTypes.Straight);
            Hand.TopCard(Hand.Evaluate(mask)).ShouldBe((uint)Hand.Rank9);
        }
    }
}
