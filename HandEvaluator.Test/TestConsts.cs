using HoldemHand;

namespace HandEvaluator.Test
{
    public static class TestConsts
    {
        public static string sAces = "Ad As";
        public static string sKings = "Kd Ks";
        public static string sAK = "Ah Kc";
        public static string sA2 = "Ac 2c";
        public static string s46 = "4c 6h";

        public static ulong ulAces = Hand.ParseHand("Ad As");
        public static ulong ulKings = Hand.ParseHand("Kd Ks");
        public static ulong ulAK = Hand.ParseHand("Ah Kc");
        public static ulong ulA2 = Hand.ParseHand("Ac 2c");
        public static ulong ul46 = Hand.ParseHand("4c 6h");
    }
}
