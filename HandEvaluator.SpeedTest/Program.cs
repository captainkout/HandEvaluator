using System;
using System.Collections.Generic;
using System.Diagnostics;
using HoldemHand;

namespace HandEvaluator.SpeedTest;

internal static class Program
{
    private const int Seed = 12345; // default seed
    private const int SituationCount = 1000000; // default benchmark size
    private const int ThroughputPoolSize = 65536; // reusable pool for throughput mode

    private struct Situation
    {
        public ulong PlayerA; // pocket A | board (7 cards)
        public ulong PlayerB; // pocket B | board (7 cards)
    }

    private static int Main(string[] args)
    {
        int seed = Seed;
        bool throughput = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--throughput":
                    throughput = true;
                    break;
                case "--seed":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out seed))
                    {
                        Console.Error.WriteLine("--seed requires an integer value.");
                        return 1;
                    }
                    break;
                case "-h":
                case "--help":
                    PrintUsage();
                    return 0;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    PrintUsage();
                    return 1;
            }
        }

        if (throughput)
        {
            RunThroughput(seed);
        }
        else
        {
            RunDefaultBenchmark(seed);
        }
        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(
            "  HandEvaluator.SpeedTest                       # default 1,000,000-situation benchmark"
        );
        Console.WriteLine(
            "  HandEvaluator.SpeedTest --throughput [--seed N]  # 2-player evals/sec for 1 second"
        );
    }

    // Existing winner/tie benchmark; body moved verbatim from the old Main(),
    // with `seed` now a parameter instead of the const.
    private static void RunDefaultBenchmark(int seed)
    {
        List<Situation> situations = BuildSituations(SituationCount, seed);

        int aWins = 0;
        int bWins = 0;
        int ties = 0;

        // Timed region: evaluation only. Generation above is NOT timed.
        Stopwatch sw = Stopwatch.StartNew();
        foreach (Situation s in situations)
        {
            uint a = Hand.Evaluate(s.PlayerA, 7);
            uint b = Hand.Evaluate(s.PlayerB, 7);
            if (a > b)
            {
                aWins++;
            }
            else if (a < b)
            {
                bWins++;
            }
            else
            {
                ties++;
            }
        }
        sw.Stop();

        double ms = sw.Elapsed.TotalMilliseconds;
        Console.WriteLine("HandEvaluator speed test");
        Console.WriteLine($"Situations: {SituationCount}");
        Console.WriteLine($"Matches won by PlayerA: {aWins}");
        Console.WriteLine($"Matches won by PlayerB: {bWins}");
        Console.WriteLine($"Ties: {ties}");
        Console.WriteLine($"Total elapsed time: {ms:F3} ms");
        Console.WriteLine(
            $"Time per situation (elapsed / {SituationCount}): {ms / SituationCount:F6} ms"
        );
        Console.WriteLine(
            $"Time per hand evaluation (elapsed / {SituationCount * 2}): {ms / (SituationCount * 2):F6} ms"
        );
    }

    // Throughput mode: run as many heads-up (two-player) 7-card evaluations as possible
    // in one wall-clock second. A "2-player evaluation" = two Hand.Evaluate(mask, 7)
    // calls (Player A + Player B) over a shared five-card board. The pool is built and
    // warmed up outside the timed window; the loop condition is checked once per full
    // pool pass, so the measured window can overshoot 1s by at most one pass.
    private static void RunThroughput(int seed)
    {
        TimeSpan window = TimeSpan.FromSeconds(1.0);
        List<Situation> pool = BuildSituations(ThroughputPoolSize, seed);

        int aWins = 0,
            bWins = 0,
            ties = 0;

        // Untimed warm-up pass (JIT / lazy init).
        for (int i = 0; i < pool.Count; i++)
        {
            uint a = Hand.Evaluate(pool[i].PlayerA, 7);
            uint b = Hand.Evaluate(pool[i].PlayerB, 7);
            if (a > b)
                aWins++;
            else if (a < b)
                bWins++;
            else
                ties++;
        }

        aWins = 0;
        bWins = 0;
        ties = 0;

        long situations = 0;
        Stopwatch sw = Stopwatch.StartNew();
        while (true)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                uint a = Hand.Evaluate(pool[i].PlayerA, 7);
                uint b = Hand.Evaluate(pool[i].PlayerB, 7);
                if (a > b)
                    aWins++;
                else if (a < b)
                    bWins++;
                else
                    ties++;
            }
            situations += pool.Count;
            if (sw.Elapsed >= window)
                break;
        }
        sw.Stop();

        long evaluations = situations * 2;

        Console.WriteLine("HandEvaluator throughput test (2-player, 7-card)");
        Console.WriteLine($"Seed: {seed}");
        Console.WriteLine($"Pool size: {pool.Count}");
        Console.WriteLine($"Wall-clock window: {sw.Elapsed.TotalSeconds:F3} s");
        Console.WriteLine($"2-player evaluations: {situations}");
        Console.WriteLine($"2-player evaluations / second: {situations}");
        Console.WriteLine($"Single Hand.Evaluate calls: {evaluations}");
        Console.WriteLine($"Single Hand.Evaluate calls / second: {evaluations}");
        Console.WriteLine($"PlayerA wins: {aWins}, PlayerB wins: {bWins}, ties: {ties}");
    }

    // Build situations; each is 9 distinct cards drawn from a 52-card deck.
    private static List<Situation> BuildSituations(int count, int seed)
    {
        Random rng = new Random(seed);
        int[] deck = new int[52];
        List<Situation> list = new List<Situation>(count);

        for (int n = 0; n < count; n++)
        {
            for (int i = 0; i < 52; i++)
            {
                deck[i] = i;
            }

            // Partial Fisher-Yates: pick first 9 distinct cards.
            for (int i = 0; i < 9; i++)
            {
                int j = rng.Next(i, 52);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }

            ulong a = Hand.CardMasksTable[deck[0]] | Hand.CardMasksTable[deck[1]];
            ulong b = Hand.CardMasksTable[deck[2]] | Hand.CardMasksTable[deck[3]];
            ulong board = 0;
            for (int i = 4; i < 9; i++)
            {
                board |= Hand.CardMasksTable[deck[i]];
            }

            list.Add(new Situation { PlayerA = a | board, PlayerB = b | board });
        }
        return list;
    }
}
