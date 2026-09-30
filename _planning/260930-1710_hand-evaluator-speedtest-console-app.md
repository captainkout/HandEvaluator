# Plan: HandEvaluator Speed Test Console App

Timestamp: 260930-1710
Target repo: `C:\ck\HandEvaluator` (branch `master`)

---

## 1. Original request (verbatim)

> We want a speed test console app that evaluates how long it takes to evaluate hands with this library.
> For the test draw 9 cards 1000000 times. The first two will be for playerA, the next two will be for playerB, and the remainder are the community cards shared between them.
> Start a timer and evaluate the list of situations. Stop the timer. Print the results for Matches won by PlayerA, PlayerB, and ties. Then show the amount of time it took along with the time per evaluation.

---

## 2. Summary of what will be done

Add a new SDK-style console project `HandEvaluator.SpeedTest` (folder `HandEvaluator.SpeedTest/`, target `net10.0`) that references the existing `HandEvaluator` library. The program:

1. Builds a list of 1000000 "situations" **before** timing. Each situation is 9 distinct cards drawn from a 52-card deck (partial Fisher–Yates shuffle):
   - cards[0..1] -> Player A pocket
   - cards[2..3] -> Player B pocket
   - cards[4..8] -> community / board
2. Starts a `System.Diagnostics.Stopwatch`, loops over the list, and for each situation evaluates **each player's best 7-card hand** as `mask = pocket | board` via `Hand.Evaluate(mask, 7)`. Compares the two `uint` values:
   - `a > b` -> Player A win
   - `a < b` -> Player B win
   - `a == b` -> tie
     Stops the stopwatch after the loop.
3. Prints: Player A wins, Player B wins, ties, total elapsed time (ms), evaluations per second, and average time per evaluation.

Deterministic by default via a fixed RNG seed (`const int Seed = 12345`).

---

## 3. Key facts verified

- Solution `HandEvaluator.sln` contains `HandEvaluator` (GUID `{42905660-E186-40D9-AB63-5E180AF6818F}`) and `HandEvaluator.Test` (`{185F186A-B729-4EF1-AAF3-29704F80936D}`); both already `net10.0`.
- Library public API in `HandEvaluator/HandEvaluator.cs` (namespace `HoldemHand`), verified:
  - `public static uint Evaluate(ulong cards)` (line 1150) and `public static uint Evaluate(ulong cards, int numberOfCards)` (line 1220).
  - `public static ulong ParseHand(string hand)` (line 362).
  - `public static string MaskToString(ulong mask)` (line 1024), `String DescriptionFromMask(ulong)` (line 685).
  - `public static readonly ulong[] CardMasksTable` (line 42614) — 52 entries, `1UL << index`.
  - `public static readonly string[] CardTable` (line 42674) — 52 ASCII strings ("2c" .. "As").
- Existing csproj conventions: `net10.0`, tabs / 4-space, no implicit usings; `ProjectReference` pattern shown in `HandEvaluator.Test.csproj`.

---

## 4. Decisions made

- **Project name/folder:** `HandEvaluator.SpeedTest` / `HandEvaluator.SpeedTest/`.
- **Target:** `net10.0` (matches solution).
- **Reference:** `ProjectReference` to `..\HandEvaluator\HandEvaluator.csproj`.
- **Timer scope:** card generation is OUTSIDE the timed region. Only the evaluate-and-compare loop is timed, exactly as the user described ("Start a timer and evaluate the list of situations").
- **Per-evaluation denominator:** print BOTH labeled metrics so the stat is unambiguous:
  - per situation = `elapsed / 1000000`
  - per hand evaluation = `elapsed / 2000000` (2 evaluations per situation)
- **Determinism:** fixed `const int Seed = 12345` with `new Random(Seed)`. Optional CLI args (count, seed) may be added if trivial, but defaults must reproduce the requested 1000000 / fixed seed.
- **Adding to solution:** prefer CLI (`dotnet new console`, `dotnet sln add`) over hand-editing `.sln` GUID/config blocks, to avoid malformed GUIDs. Hand-edit only if CLI unavailable.
- **No library changes:** do NOT modify `HandEvaluator.cs`, its XML docs, or existing behavior.

---

## 5. Steps for the worker

### Step 1 — Create the console project

From repo root:

```bash
dotnet new console -n HandEvaluator.SpeedTest -o HandEvaluator.SpeedTest -f net10.0
```

Then overwrite `HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj` so it matches repo conventions (SDK-style, no implicit usings, reference the library):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>HandEvaluator.SpeedTest</RootNamespace>
    <AssemblyName>HandEvaluator.SpeedTest</AssemblyName>
    <Nullable>disable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\HandEvaluator\HandEvaluator.csproj" />
  </ItemGroup>

</Project>
```

(If `dotnet new console` produced an unused `using` / top-level statement, replace `Program.cs` entirely per Step 2.)

### Step 2 — Write `HandEvaluator.SpeedTest/Program.cs`

Structure (an explicit `Main` is fine and clearer than top-level statements):

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using HoldemHand;

namespace HandEvaluator.SpeedTest
{
    internal static class Program
    {
        private const int Seed = 12345;
        private const int SituationCount = 1000000;

        private sealed class Situation
        {
            public ulong PlayerA;   // pocket A | board (7 cards)
            public ulong PlayerB;   // pocket B | board (7 cards)
        }

        private static void Main()
        {
            List<Situation> situations = BuildSituations(SituationCount, Seed);

            int aWins = 0, bWins = 0, ties = 0;

            // Timed region: evaluation only. Generation above is NOT timed.
            Stopwatch sw = Stopwatch.StartNew();
            foreach (Situation s in situations)
            {
                uint a = Hand.Evaluate(s.PlayerA, 7);
                uint b = Hand.Evaluate(s.PlayerB, 7);
                if (a > b) aWins++;
                else if (a < b) bWins++;
                else ties++;
            }
            sw.Stop();

            double ms = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine("HandEvaluator speed test");
            Console.WriteLine($"Situations: {SituationCount}");
            Console.WriteLine($"Matches won by PlayerA: {aWins}");
            Console.WriteLine($"Matches won by PlayerB: {bWins}");
            Console.WriteLine($"Ties: {ties}");
            Console.WriteLine($"Total elapsed time: {ms:F3} ms");
            Console.WriteLine($"Time per situation (elapsed / {SituationCount}): {ms / SituationCount:F6} ms");
            Console.WriteLine($"Time per hand evaluation (elapsed / {SituationCount * 2}): {ms / (SituationCount * 2):F6} ms");
        }

        // Build 1000000 situations; each is 9 distinct cards from a 52-card deck.
        private static List<Situation> BuildSituations(int count, int seed)
        {
            var rng = new Random(seed);
            var deck = new int[52];
            var list = new List<Situation>(count);

            for (int n = 0; n < count; n++)
            {
                for (int i = 0; i < 52; i++) deck[i] = i;
                // Partial Fisher-Yates: pick first 9 distinct cards.
                for (int i = 0; i < 9; i++)
                {
                    int j = rng.Next(i, 52);
                    (deck[i], deck[j]) = (deck[j], deck[i]);
                }

                ulong a = Hand.CardMasksTable[deck[0]] | Hand.CardMasksTable[deck[1]];
                ulong b = Hand.CardMasksTable[deck[2]] | Hand.CardMasksTable[deck[3]];
                ulong board = 0;
                for (int i = 4; i < 9; i++) board |= Hand.CardMasksTable[deck[i]];

                list.Add(new Situation { PlayerA = a | board, PlayerB = b | board });
            }
            return list;
        }
    }
}
```

Notes:

- `deck[i]` is an index 0..51; `Hand.CardMasksTable[index]` yields that card's bit. `(a,b) = (b,a)` tuple swap syntax is C# 7+ and works with net10.0.
- Keep the `Stopwatch` strictly around the `foreach`; do not include list generation or console output.
- Optionally print the 9 dealt cards using `Hand.CardTable[deck[i]]` for debugging, but keep it out of the timed loop (or drop it).

### Step 3 — Add project to the solution

Prefer CLI:

```bash
dotnet sln HandEvaluator.sln add HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj
```

Verify `HandEvaluator.sln` now has a `Project(...)` entry and matching `ProjectConfigurationPlatforms` lines for Debug/Release x Any CPU/x64/x86. If hand-editing is required instead:

- Add project block (GUID must be uppercase, braces included):
  `Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "HandEvaluator.SpeedTest", "HandEvaluator.SpeedTest\HandEvaluator.SpeedTest.csproj", "{<NEW-GUID>}"`
- For each of the 6 configurations (Debug/Release x Any CPU/x64/x86) add `.ActiveCfg` and `.Build.0` lines mirroring the existing projects, mapping all to `Any CPU`.

### Step 4 — Restore, build, run

```bash
dotnet restore
dotnet build HandEvaluator.sln -c Release
dotnet run --project HandEvaluator.SpeedTest -c Release
```

Expected:

- Build: `0 Error(s)`.
- App prints the 7 lines above.
- `aWins + bWins + ties == 1000000`.
- `ms > 0`, per-evaluation values small (likely microseconds-to-low-ms; release build).
- No exceptions.

### Step 5 — Sanity checks

- [ ] Sum of A wins + B wins + ties equals 1000000.
- [ ] Ties are a small fraction (7-card hold'em with board; typically dozens, not hundreds).
- [ ] A/B wins roughly balance (not exactly equal).
- [ ] Rerun yields identical counts (fixed seed).
- [ ] `git status` shows only new `HandEvaluator.SpeedTest/` files and the edited `HandEvaluator.sln`; no library files changed.
- [ ] Confirm `.gitignore` does not accidentally exclude the new project (it shouldn't).

### Step 6 — Completion report

Write the worker completion report to `_work-completed/` following the worker's existing naming convention, including: files created/edited, exact commands run, observed output, and confirmation that the library was untouched.

---

## 6. Checklist

- [ ] `HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj` created, `net10.0`, references library.
- [ ] `HandEvaluator.SpeedTest/Program.cs` implements draw + timed evaluate/compare + metrics.
- [ ] Timer covers ONLY the evaluation loop (generation outside).
- [ ] 9 distinct cards per draw; 2/2/5 split; 1000000 draws; fixed seed.
- [ ] Compares `Hand.Evaluate(mask, 7)` for both players.
- [ ] Prints A wins, B wins, ties, total ms, and per-evaluation time (both denominators labeled).
- [ ] Project added to `HandEvaluator.sln`.
- [ ] `dotnet restore` + `dotnet build HandEvaluator.sln -c Release` succeed (0 errors).
- [ ] `dotnet run --project HandEvaluator.SpeedTest -c Release` prints expected metrics.
- [ ] No changes to `HandEvaluator` library or its docs.
- [ ] Worker completion report written to `_work-completed/`.

---

## 7. Risks / follow-ups

- **In-solution vs standalone:** adding to `HandEvaluator.sln` is the recommended default; if the maintainers prefer a standalone benchmark, it can be removed later with `dotnet sln HandEvaluator.sln remove`.
- **Per-evaluation denominator ambiguity:** the request says only "time per evaluation". Plan prints both per-situation and per-hand-evaluation, clearly labeled. Confirm wording if a single number is preferred.
- **DEBUG-only `Evaluate` range assertion:** `Evaluate(mask, 7)` must be within 1..7 cards; our 7-card masks are valid, so no throw. Do not accidentally pass board-only (5) or pocket-only (2) masks.
- **RNG determinism:** fixed seed gives reproducible counts. If the intent is a general benchmark, expose `--count`/`--seed` CLI args as a follow-up.
- **Timer noise:** 1000000 iterations may complete very fast; JIT/startup is excluded because the timer wraps only the loop, but consider a warm-up call if results look noisy. Do not change semantics.
- **Future:** could extend to benchmark `Hand.HandOdds` enumeration separately, but that is explicitly out of scope here.
- **`CardMasksTable` index mapping:** indices 0..51 are the canonical card order used by `CardTable`; using an int deck of 0..51 is safe and avoids parsing overhead.

---

## 8. Verification commands (copy/paste)

```bash
dotnet restore
dotnet build HandEvaluator.sln -c Release
dotnet run --project HandEvaluator.SpeedTest -c Release
```
