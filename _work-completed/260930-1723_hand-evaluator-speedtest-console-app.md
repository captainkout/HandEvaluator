# Completion Report: HandEvaluator Speed Test Console App

Timestamp: 260930-1723
Plan: `_planning/260930-1710_hand-evaluator-speedtest-console-app.md`
Repo: `C:\ck\HandEvaluator` (branch `master`)

---

## 1. Original request (verbatim)

> We want a speed test console app that evaluates how long it takes to evaluate hands with this library.
> For the test draw 9 cards 1000000 times. The first two will be for playerA, the next two will be for playerB, and the remainder are the community cards shared between them.
> Start a timer and evaluate the list of situations. Stop the timer. Print the results for Matches won by PlayerA, PlayerB, and ties. Then show the amount of time it took along with the time per evaluation.

Confirmed requirements from the assigning prompt:

- New SDK-style console project `HandEvaluator.SpeedTest` (folder `HandEvaluator.SpeedTest/`, `net10.0`), referencing `HandEvaluator`.
- Iteration count **1,000,000** (user confirmed this over the plan's literal 1000000 / any 1000 variant).
- Per situation: 9 **distinct** cards drawn from a 52-card deck via partial Fisher-Yates; `cards[0..1]` = Player A pocket, `cards[2..3]` = Player B pocket, `cards[4..8]` = community board.
- Build the full list of situations **first, outside the timed region**; time **only** the evaluate-and-compare loop with `System.Diagnostics.Stopwatch`.
- Each situation: `a = Hand.Evaluate(pocketA | board, 7)`, `b = Hand.Evaluate(pocketB | board, 7)`; `a>b` A win, `a<b` B win, else tie.
- Print A wins, B wins, ties, total elapsed ms, and time per evaluation with **both** denominators clearly labeled (per situation `elapsed/1000000`, per hand evaluation `elapsed/2000000`).
- Deterministic: fixed seed `12345` (`const int Seed = 12345`).
- Use `Hand.CardMasksTable[index]`; namespace `HandEvaluator.SpeedTest`, no implicit usings.
- Add project to `HandEvaluator.sln` (prefer `dotnet sln add`).
- **Do NOT modify the `HandEvaluator` library or its docs.**
- Do not commit. Do not add `.opencode/` or `_planning/` to git.

## 2. Summary of what was done

1. Created `HandEvaluator.SpeedTest/` with:
   - `HandEvaluator.SpeedTest.csproj` — SDK-style, `net10.0`, `OutputType Exe`, `Nullable disable`, `ProjectReference` to `..\HandEvaluator\HandEvaluator.csproj`, matching repo conventions.
   - `Program.cs` — explicit `Main`, `List<Situation>` built up front, timed evaluate/compare loop, metrics output.
2. Added the project to the solution with `dotnet sln HandEvaluator.sln add ...` (CLI, no hand-edited GUIDs).
3. Ran the plan's verification commands; all passed.
4. Ran the app twice to confirm deterministic counts; counts matched exactly (timing naturally varied).

### Implementation notes

- `Situation` is a `struct` with two `ulong` fields (`PlayerA`, `PlayerB`), each holding `pocket | board` (7-card mask), as permitted.
- Deck is an `int[52]`; partial Fisher–Yates picks the first 9 indices; masks come from `Hand.CardMasksTable[...]`. Swap uses C# tuple syntax.
- `Stopwatch` wraps only the `foreach` evaluate/compare loop. Generation and all console output are outside the timed region. No warm-up call was added (kept plan semantics; loop-only timing is stable at this scale).
- Both per-evaluation denominators are printed and explicitly labeled.

## 3. Files changed

Created:

- `HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj`
- `HandEvaluator.SpeedTest/Program.cs`

Modified (by tooling):

- `HandEvaluator.sln` — added the `HandEvaluator.SpeedTest` project entry and the 6 Debug/Release × Any CPU/x64/x86 `ProjectConfigurationPlatforms` mappings (all mapped to `Any CPU`).

Not modified by this task:

- `HandEvaluator/HandEvaluator.cs` (library source) — untouched.
- `HandEvaluator` XML documentation — untouched.
- No library behavior changes.

Note: `git status` shows pre-existing working-tree modifications to `HandEvaluator/HandEvaluator.csproj`, `HandEvaluator.Test/HandEvaluator.Test.csproj`, and `README.md` (e.g. `net5.0 → net10.0`). These were already present before this task and were **not** made by this work.

## 4. Commands run and observed output

```bash
dotnet sln HandEvaluator.sln add HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj
# Project `HandEvaluator.SpeedTest\HandEvaluator.SpeedTest.csproj` added to the solution.

dotnet restore
# All projects are up-to-date for restore.

dotnet build HandEvaluator.sln -c Release
# Build succeeded.
#     0 Warning(s)
#     0 Error(s)

dotnet run --project HandEvaluator.SpeedTest -c Release
```

Observed output (run 1):

```
HandEvaluator speed test
Situations: 1000000
Matches won by PlayerA: 479667
Matches won by PlayerB: 479769
Ties: 40564
Total elapsed time: 53.071 ms
Time per situation (elapsed / 1000000): 0.000053 ms
Time per hand evaluation (elapsed / 2000000): 0.000027 ms
```

Observed output (run 2, `--no-build`):

```
HandEvaluator speed test
Situations: 1000000
Matches won by PlayerA: 479667
Matches won by PlayerB: 479769
Ties: 40564
Total elapsed time: 46.002 ms
Time per situation (elapsed / 1000000): 0.000046 ms
Time per hand evaluation (elapsed / 2000000): 0.000023 ms
```

### Sanity checks

- `479667 + 479769 + 40564 = 1000000` ✔
- Elapsed > 0 in both runs ✔
- No exceptions ✔
- Deterministic: A/B/ties identical across runs (479667 / 479769 / 40564) ✔
- A/B wins roughly balanced (479667 vs 479769) ✔
- `HandEvaluator/HandEvaluator.cs` not in `git diff` — library untouched ✔
- `bin/`/`obj/` correctly excluded by `.gitignore`; only the two source files are untracked in the new folder ✔

### Note on tie count

The plan's rough sanity note expected ties in the "dozens, not hundreds" range; the observed value is **40,564** (~4.1%). This is the real split-pot rate for heads-up 7-card hold'em where both players share the board (many ties arise when the board plays or both players' best five come from shared cards), and is consistent with known hold'em split frequencies. All counts still sum to exactly 1,000,000, so the evaluator is behaving consistently; the plan's estimate was simply low. No code change was warranted.

## 5. Decisions made and follow-ups

- **Count:** used 1,000,000 iterations as confirmed by the user. The plan text's "1000000" is what was implemented; no truncated/1000 variant.
- **`Situation` as `struct`:** chosen over `class` for locality/perf; behavior identical for this benchmark.
- **Solution GUID type:** `dotnet sln add` emitted the classic C# project type GUID `{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}` (rather than the SDK-style `{9A19103F-...}` shown in the plan). This is valid and builds/restores cleanly, so it was left as generated (CLI preferred over hand-editing).
- **No warm-up:** deliberately omitted to keep the timer strictly around the evaluation loop per the request. If future runs need lower variance, an untimed warm-up `Hand.Evaluate` call could be added as a follow-up.
- **CLI args for count/seed:** not added (plan lists as optional follow-up); defaults are fixed at 1000000 / seed 12345.
- **No commits:** nothing was staged or committed. `.opencode/` and `_planning/` were left untracked and untouched.
