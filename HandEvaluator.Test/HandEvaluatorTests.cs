using System.Linq;
using HoldemHand;
using Shouldly;
using Xunit;
using static HandEvaluator.Test.TestConsts;
namespace HandEvaluator.Test
{
    public class HandEvaluatorTests
    {
        [Fact]
        public void Hand_Empty()
        {
            var h = new Hand();
            h.Board.ShouldBe("");
            h.BoardMask.ShouldBe(0ul);
            h.PocketCards.ShouldBe("");
            h.PocketMask.ShouldBe(0ul);
        }

        [Fact]
        public void Hand_RoyalFlush()
        {
            var h1 = new Hand("As Ks", "Qs Js Ts");
            var h2 = new Hand("As Ks Qs", "Js Ts");
            var h3 = new Hand("As Ks Qs Js", "Ts");
            h1.HandTypeValue.ShouldBe(Hand.HandTypes.StraightFlush);
            h2.HandTypeValue.ShouldBe(Hand.HandTypes.StraightFlush);
            h3.HandTypeValue.ShouldBe(Hand.HandTypes.StraightFlush);
        }
        [Fact]
        public void ValidateHand()
        {
            Hand.ValidateHand("Ad Ah Qh Qd 2s").ShouldBeTrue();
            Hand.ValidateHand("Boogers").ShouldBeFalse();
            Hand.ValidateHand("2d 2h").ShouldBeTrue();
            Hand.ValidateHand("Ak Ak Ah Ah Qs").ShouldBeFalse();
        }
        [Fact]
        public void ParseHand()
        {
            var index = 0;
            var hand = "As Ad Ac 2d";
            Hand.NextCard(hand, ref index).ShouldBe(Hand.ParseCard("As"));
            Hand.NextCard(hand, ref index).ShouldBe(Hand.ParseCard("Ad"));
            Hand.NextCard(hand, ref index).ShouldBe(Hand.ParseCard("Ac"));
            Hand.NextCard(hand, ref index).ShouldBe(Hand.ParseCard("2d"));
        }
        [Fact]
        public void DescriptionFromMask()
        {
            Hand.DescriptionFromMask(Hand.ParseHand("ad kd 2d kh qh 3h qc")).ShouldContain("Pair");
        }
        [Fact]
        public void DescriptionFromHand()
        {
            Hand.DescriptionFromHand("ad kd 2d kh qh 3h qc").ShouldContain("Pair");
        }
        [Fact]
        public void Evaluate()
        {
            Hand.Evaluate(ul46).ShouldBeLessThan(Hand.Evaluate(sAces));
        }
        [Fact]
        public void EqualOperator()
        {
            Hand h1 = new Hand("ac as", "4d 5d 6c 7c 8d");
            Hand h2 = new Hand("td js", "4d 5d 6c 7c 8d");
            Hand h3 = new Hand("2s 2d", "2h 2c qs ks 8d");

            (h1 == h2).ShouldBeTrue();
            (h2 == h3).ShouldBeFalse();
        }
        [Fact]
        public void NotEqualOperator()
        {
            Hand h1 = new Hand("ac as", "4d 5d 6c 7c 8d");
            Hand h2 = new Hand("td js", "4d 5d 6c 7c 8d");
            Hand h3 = new Hand("2s 2d", "2h 2c qs ks 8d");

            (h1 != h2).ShouldBeFalse();
            (h2 != h3).ShouldBeTrue();
        }
        [Fact]
        public void GTOrLTOperator()
        {
            Hand h1 = new Hand("ac as", "4d 5d 6c 7c 8d");
            Hand h2 = new Hand("td js", "4d 5d 6c 7c 8d");
            Hand h3 = new Hand("2s 2d", "2h 2c qs ks 8d");

            (h1 > h2).ShouldBeFalse();
            (h1 >= h2).ShouldBeTrue();
            (h2 < h3).ShouldBeTrue();
        }
    }
}
