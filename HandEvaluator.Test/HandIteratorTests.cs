using System.Linq;
using HoldemHand;
using Shouldly;
using Xunit;
using static HandEvaluator.Test.TestConsts;

namespace HandEvaluator.Test
{
    public class HandIteratorTests
    {
        [Fact]
        public void Hands()
        {
            Hand.Hands(2).ToList().Count().ShouldBe(1326);
            Hand.Hands(0ul, Hand.ParseHand("As"), 2).ToList().Count().ShouldBe(1275);
        }

        [Fact]
        public void RandomHands()
        {
            var ac = Hand.Evaluate(ulAces, 2);
            // how many hands are worse or equal to As Ad. Should be all.
            Hand.RandomHands(2, 100).Select(u =>
            {
                var ev = Hand.Evaluate(u, 2);
                return ev;
            }).Aggregate(0, (acc, val) =>
                 Hand.Evaluate(ulAces, 2) >= val ? acc + 1 : acc

                )
            .ShouldBe(100);
        }
    }
}
