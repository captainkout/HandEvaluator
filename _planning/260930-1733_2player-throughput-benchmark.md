# Plan: 2-player throughput benchmark for SpeedTest + README results

**Timestamp:** 260930-1733
**Repo:** `C:\ck\HandEvaluator` (branch `net10-uplift`)
**Worker target:** `@worker`
**Worker completion report:** `_work-completed/` (per the worker's own convention)

---

## 1. Original request (verbatim)

> Add an option to the SpeedTest to run as many possible 2player evaluations in 1 second and get the count. Run for a few different seeds and report. Add the results to the readme.me in a section immediately prior to API Reference.

---

## 2. Summary of what will be done

1. **Extend `HandEvaluator.SpeedTest/Program.cs`** with a new CLI-driven throughput mode that runs as many two-player (heads-up) seven-card evaluations as possible in exactly one wall-clock second, and prints the count.
   - A **two-player evaluation ("situation")** = two `Hand.Evaluate(mask, 7)` calls (Player A + Player B) over a shared five-card board.
   - Primary reported metric = **two-player evaluations / second**; secondary = implied **single `Hand.Evaluate` calls / second** (= 2×).
   - The existing no-argument behavior (1,000,000-situation timed benchmark with win/tie tallies) stays **unchanged**.
2. **Run the throughput mode for 5 seeds** (`1`, `42`, `777`, `12345`, `2026`) in a Release build and capture the counts, plus the machine/runtime context.
3. **Add a `## Performance` section to `README.md` immediately before the `## API Reference` heading** (currently line 44), with a table of seed → count, a machine-dependence caveat, and the exact reproduce command.
4. Do **not** modify the `HandEvaluator` library source or XML docs. Do **not** commit.

**Current state verified:**
- `HandEvaluator.SpeedTest/Program.cs` = 93 lines. `Main()` is `private static void Main()` (no args), constants `Seed = 12345`, `SituationCount = 1000000`; `BuildSituations(int count, int seed)` builds a `List<Situation>`; the timed region is only the `foreach` that calls `Hand.Evaluate(...)` twice per situation.
- `README.md` top-level headings: `# HandEvaluator` (1), `## Installation` (5), `## Quick start` (11), `## API Reference` (44). The `## Performance` section goes at line 44, immediately before `## API Reference`.
- `git status` shows an uncommitted working tree (README already modified, SpeedTest untracked). **Do not commit.**

---

## 3. Step-by-step work

### Step 1 — Refactor `Program.cs` entry point to accept args (keep default intact)

Change the entry point from `private static void Main()` to `private static int Main(string[] args)` and split the current benchmark body into a named method. Reuse the existing `Seed`/`SituationCount` constants as defaults.

Target shape:

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using HoldemHand;

namespace HandEvaluator.SpeedTest
{
	internal static class Program
	{
		private const int Seed = 12345;                 // default seed
		private const int SituationCount = 1000000;      // default benchmark size
		private const int ThroughputPoolSize = 65536;    // reusable pool for throughput mode

		private struct Situation
		{
			public ulong PlayerA;   // pocket A | board (7 cards)
			public ulong PlayerB;   // pocket B | board (7 cards)
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
			Console.WriteLine("  HandEvaluator.SpeedTest                       # default 1,000,000-situation benchmark");
			Console.WriteLine("  HandEvaluator.SpeedTest --throughput [--seed N]  # 2-player evals/sec for 1 second");
		}

		// Existing winner/tie benchmark; body moved verbatim from the old Main(),
		// with `seed` now a parameter instead of the const.
		private static void RunDefaultBenchmark(int seed)
		{
			List<Situation> situations = BuildSituations(SituationCount, seed);

			int aWins = 0, bWins = 0, ties = 0;

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

		// ... new RunThroughput(int seed) here (Step 2) ...

		// BuildSituations stays unchanged.
	}
}
```

Notes:
- `SituationCount` is a `const int`, so `SituationCount * 2` is a compile-time int (2,000,000) — no overflow.
- Keep the existing `BuildSituations` method exactly as-is.

### Step 2 — Add the throughput mode (`RunThroughput`)

Design decisions to implement and document:
- **Pool is built before timing** (RNG / generation excluded from the count).
- Pool size = `ThroughputPoolSize` (65,536 situations). Sized to fit comfortably in cache while still large enough to avoid trivial repetition.
- **JIT/library warm-up:** run one untimed pass over the pool before starting the stopwatch, so tiered JIT and any lazy initialization do not distort the first second.
- **Timing:** wall-clock `Stopwatch`. Loop passes over the pool until `sw.Elapsed >= 1s`, then stop. The loop condition is checked once per full pool pass, so the measured window may overshoot 1s by at most one pool pass; document this.
- Count whole 2-player situations completed.

Code sketch to add inside the class:

```csharp
private static void RunThroughput(int seed)
{
	TimeSpan window = TimeSpan.FromSeconds(1.0);
	List<Situation> pool = BuildSituations(ThroughputPoolSize, seed);

	int aWins = 0, bWins = 0, ties = 0;

	// Untimed warm-up pass (JIT / lazy init).
	for (int i = 0; i < pool.Count; i++)
	{
		uint a = Hand.Evaluate(pool[i].PlayerA, 7);
		uint b = Hand.Evaluate(pool[i].PlayerB, 7);
		if (a > b) aWins++; else if (a < b) bWins++; else ties++;
	}

	aWins = 0; bWins = 0; ties = 0;

	long situations = 0;
	Stopwatch sw = Stopwatch.StartNew();
	while (true)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			uint a = Hand.Evaluate(pool[i].PlayerA, 7);
			uint b = Hand.Evaluate(pool[i].PlayerB, 7);
			if (a > b) aWins++; else if (a < b) bWins++; else ties++;
		}
		situations += pool.Count;
		if (sw.Elapsed >= window) break;
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
```

Definitions to state in code comments/README:
- "2-player evaluation" = one heads-up situation = `Hand.Evaluate(A|board, 7)` + `Hand.Evaluate(B|board, 7)` = 2 single-eval calls.

### Step 3 — Build

```bash
dotnet build HandEvaluator.sln -c Release
```

Expected: build succeeds, 0 warnings/errors introduced by these changes.

### Step 4 — Sanity-check default mode still works

```bash
dotnet run --project HandEvaluator.SpeedTest -c Release
```

Expected: prints `HandEvaluator speed test`, `Situations: 1000000`, win/tie tallies, and timing lines — identical output format to before. This confirms the refactor preserved default behavior.

### Step 5 — Run throughput mode for each seed

Run each of the following and record the `2-player evaluations / second` value AND the reported wall-clock window (should be `~1.00x` s):

```bash
dotnet run --project HandEvaluator.SpeedTest -c Release -- --throughput --seed 1
dotnet run --project HandEvaluator.SpeedTest -c Release -- --throughput --seed 42
dotnet run --project HandEvaluator.SpeedTest -c Release -- --throughput --seed 777
dotnet run --project HandEvaluator.SpeedTest -c Release -- --throughput --seed 12345
dotnet run --project HandEvaluator.SpeedTest -c Release -- --throughput --seed 2026
```

**Sanity checks:**
- Every count is `> 0` and clearly in the tens/hundreds of thousands to millions per second range.
- Counts across seeds are within a small band of one another (seeds select different card combinations but the evaluator cost is essentially data-independent; large spread would indicate noise or a bug).
- The `Wall-clock window` printed is `>= 1.000` and close to it.
- Re-running one seed (e.g. `42`) yields a count within a few percent — this is a throughput benchmark, so minor variance is expected.
- If variance is high (> ~5%), re-run each seed once and report the observed value; note the machine context. Do not attempt statistical rigour.

### Step 6 — Capture environment context (for the README caveat)

```bash
dotnet --version
```

Also capture the OS and CPU description. Easiest cross-platform approach: state the OS name and a short CPU string. Under PowerShell:

```powershell
$env:OS; $env:PROCESSOR_IDENTIFIER
```

If unavailable, the worker may add a one-line `RuntimeInformation.OSDescription` / `ProcessArchitecture` print to the throughput output (optional, no library change), or simply record the machine description manually in the completion report. Keep the README caveat generic if the exact CPU string is not obtainable.

### Step 7 — Insert `## Performance` into `README.md` immediately before `## API Reference`

Current lines 42–45:

```markdown
```
                      <-- (closing fence of Quick start at line 42)
                      <-- (blank line 43)
## API Reference       <-- (line 44)
```

Insert the following between the blank line (43) and `## API Reference` (44). Use a leading blank line so Markdown spacing matches the rest of the document. **Fill the table with the values observed in Step 5** (do not leave placeholders in the committed file):

````markdown
## Performance

The `HandEvaluator.SpeedTest` console project includes a throughput mode that measures how many
heads-up (two-player) 7-card evaluations can be completed in one wall-clock second. One two-player
evaluation performs two `Hand.Evaluate(mask, 7)` calls — one for each player's seven cards (two
pocket cards plus the shared five-card board).

Measured on `<OS> (<CPU>)`, `.NET <version>`, Release build:

| Seed | 2-player evaluations / second | Single `Hand.Evaluate` calls / second |
|------|------------------------------:|--------------------------------------:|
| 1     | <n> | <2n> |
| 42    | <n> | <2n> |
| 777   | <n> | <2n> |
| 12345 | <n> | <2n> |
| 2026  | <n> | <2n> |

These figures are machine-dependent and are provided as an order-of-magnitude reference, not a
guarantee. Reproduce on your own machine with:

```bash
dotnet run --project HandEvaluator.SpeedTest -c Release -- --throughput --seed <seed>
```
````

Constraints:
- Do **not** alter the `## Quick start` block above or any `## API Reference` content below.
- Use plain integers with thousands separators or plain digits consistently (pick one; plain digits is simplest).
- Keep the backtick fence around the reproduce command (nested code block) exactly as above.

### Step 8 — Final verification

```bash
dotnet build HandEvaluator.sln -c Release
git diff --stat
git status --short
```

Confirm:
- `HandEvaluator/` library sources and XML docs are **not** modified.
- Only `HandEvaluator.SpeedTest/Program.cs` and `README.md` changed.
- The new README section sits immediately before `## API Reference`.
- Nothing staged/committed.

### Step 9 — Write completion report

Write the worker's completion report to `_work-completed/` (per the worker convention) including: files changed, the observed per-seed counts, the command used, the environment context, and any deviations from this plan.

---

## 4. Decisions made / rationale

- **CLI surface:** `--throughput` flag plus `--seed <N>`; no args = existing benchmark unchanged. This is the least surprising extension and preserves current behavior exactly.
- **Metric definition:** primary = two-player evaluations (situations) per second; secondary = single `Hand.Evaluate` calls per second (×2). This matches the user's "2player evaluations" wording while remaining unambiguous.
- **Pool pre-generated and recycled outside the timed window** so the count measures evaluation throughput only, not RNG/generation. Pool = 65,536; a pass may overshoot 1s slightly, which is documented.
- **Warm-up pass** before timing to avoid JIT skew.
- **Seeds chosen:** 1, 42, 777, 12345, 2026 (five, as requested "a few").
- **README heading name:** `## Performance` (concise, standard).
- **No library changes, no commit** — per the request/constraints.

## 5. Risks / follow-ups

- **CLI convention:** `--seed` as a separate arg vs `--throughput <seed>`. The plan uses `--throughput --seed N`; adjust only if the worker finds the repo has an existing arg convention (none found).
- **Timer precision / overshoot:** checking elapsed once per pool pass can overshoot 1s by up to one pass. Acceptable; the reported wall-clock window makes it visible. If exactness is desired, check `sw.Elapsed` every 4,096 situations — minor complexity, not required.
- **Benchmark noise:** counts vary run-to-run; README numbers are machine-dependent. Caveat included. Do not gate on exact numbers.
- **JIT/tiered compilation** can make short runs lower than steady-state; the warm-up pass mitigates this.
- **README numbers will go stale / be machine-specific.** This is expected and explicitly caveated; a follow-up could parametrise or auto-generate them later.
- **Original win/tie benchmark:** not re-reported in the README by this plan; only the throughput results are added. If the user later wants the 1M-situation win/tie results documented too, that is a separate follow-up.
- **Branch note:** current branch is `net10-uplift` (working tree already dirty). Worker must not commit.

## 6. Worker checklist

- [ ] `Program.cs`: `Main(string[] args)` parses `--throughput`, `--seed N`, `-h/--help`; unknown args error out with usage.
- [ ] Existing default benchmark body moved to `RunDefaultBenchmark(int seed)` and behavior unchanged.
- [ ] `RunThroughput(int seed)` added: untimed warm-up, pre-built pool of 65,536, 1-second wall-clock `while` loop, counts 2-player situations, prints per-second + single-eval metrics.
- [ ] `BuildSituations` unchanged.
- [ ] `dotnet build HandEvaluator.sln -c Release` succeeds.
- [ ] Default run output unchanged (`dotnet run --project HandEvaluator.SpeedTest -c Release`).
- [ ] Five seed runs executed; counts captured.
- [ ] `dotnet --version` and machine context captured.
- [ ] `README.md`: `## Performance` inserted immediately before `## API Reference`, table filled, caveat and reproduce command present.
- [ ] `HandEvaluator` library + XML docs untouched; only `Program.cs` and `README.md` changed.
- [ ] Nothing committed.
- [ ] Completion report written to `_work-completed/`.
