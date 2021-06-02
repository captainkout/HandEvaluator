using System.Linq;
using HoldemHand;
using Shouldly;
using Xunit;
using static HandEvaluator.Test.TestConsts;

namespace HandEvaluator.Test
{
    public class HandAnalysisTests
    {

        [Fact]
        public void HandOdds()
        {
            var pockets = new string[] { "As Ad", "Ks Kd", "Ah Kh" };
            var board = "";
            var dead = "";
            var wins = new long[pockets.Length];
            var ties = new long[pockets.Length];
            var losses = new long[pockets.Length];
            var totalHands = 0L;

            Hand.HandOdds(pockets, board, dead, wins, ties, losses, ref totalHands);

            wins[0].ShouldBeGreaterThan(losses[0]);
            wins[1].ShouldBeLessThan(wins[2]);
        }

        [Fact]
        public void HandOuts()
        {
            var villain = Hand.ParseHand("As Ad");
            var hero = Hand.ParseHand("Ks Kd");

            Hand.Outs(hero, Hand.ParseHand("Qs 6s 2d"), new ulong[] { villain }).ShouldBe(2);
        }

        [Fact]
        public void OutsMask()
        {
            var villain = Hand.ParseHand("As Ad");
            var hero = Hand.ParseHand("Ks Kd");

            Hand.OutsMask(hero, Hand.ParseHand("Qs 6s 2d"), new ulong[] { villain }).ShouldBe(Hand.ParseHand("Kh kc"));
        }

        [Fact]
        public void IsSuited()
        {
            var notSuited = Hand.ParseHand("As Ad");
            var isSuited = Hand.ParseHand("As Ks");
            Hand.IsSuited(notSuited).ShouldBeFalse();
            Hand.IsSuited(isSuited).ShouldBeTrue();
        }

        [Fact]
        public void IsConnected()
        {
            var notConnected = Hand.ParseHand("As Ad");
            var isConnected = Hand.ParseHand("As Ks");
            var notConnected2 = Hand.ParseHand("As 2d");
            Hand.IsSuited(notConnected).ShouldBeFalse();
            Hand.IsSuited(isConnected).ShouldBeTrue();
            Hand.IsSuited(notConnected2).ShouldBeFalse();
        }

        [Fact]
        public void GapCount()
        {
            Hand.GapCount(ulAces).ShouldBe(-1);
            Hand.GapCount(ulAK).ShouldBe(0);
            Hand.GapCount(ulA2).ShouldBe(0);
            Hand.GapCount(ul46).ShouldBe(1);
        }
        [Fact]
        public void HandPlayerOpponentOdds()
        {
            var player = new double[9];
            var opponent = new double[9];
            Hand.HandPlayerOpponentOdds(ulAK, 0ul, ref player,ref opponent);

            // this is a precalced value for each handType (1=Pair)
            player[1].ShouldBe(0.295079403218692);


            // this should be 100% chance for quads(7) with roughly 10% tie, and zero for everything else
            Hand.HandPlayerOpponentOdds(ulAK, Hand.ParseHand("2c 2d 2h 2s"), ref player, ref opponent);

            player.Where(d => d == 0.0).Count().ShouldBe(8);
            (player.Sum() + opponent.Sum()).ShouldBe(1);
        }
        [Fact]
        public void HandPotential()
        {
            var player = ulAK;
            var board = Hand.ParseHand("Qs Qd Ts 6h");
            var pos = 0.0;
            var neg = 0.0;
            HoldemHand.Hand.HandPotential(player, board, out pos, out neg);
            pos.ShouldBeLessThan(neg);
        }
    }
}
