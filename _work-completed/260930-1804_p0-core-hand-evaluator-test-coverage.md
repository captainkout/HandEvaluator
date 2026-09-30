# P0 Core Hand Evaluator Test Coverage

- **Timestamp:** 260930-1804
- **Branch:** `net10-uplift`
- **Scope:** Implement test coverage for P0-A through P0-F of
  `_planning/260930-1746_hand-evaluator-test-improvement-plan.md`. No library source,
  XML docs, or existing tests modified; nothing committed.

---

## 1. Original Prompt / Request

> Implement test coverage for ALL P0-level candidates from the test-improvement plan at
> `_planning/260930-1746_hand-evaluator-test-improvement-plan.md`. The P0 sections are
> §3 P0-A through P0-F, with structure/traits guidance in §4–§6.
>
> Scope — implement P0 groups only (not P1/P2/P3):
> - **P0-A** Core `Hand.Evaluate` category boundaries for all 9 hand types (5/6/7-card), including royal flush.
> - **P0-B** Kickers and tie-breaks.
> - **P0-C** Wheel, steel wheel, straight boundaries.
> - **P0-D** Mask encoding/parse/format invariants.
> - **P0-E** Suit-isomorphism / permutation invariance.
> - **P0-F** Compare/equal/description API.
>
> Verify API signatures before writing tests, use the existing `HandEvaluator.Test`
> project, reorganize into `Core/`, add `Traits.cs` and `Fixtures/`. Tag the fast suite.
> Do not implement P2-B (5-card enumeration). Fix the `IsConnected` latent test bug only if
> a touched file. Verify in Release and Debug. Write a completion report.

---

## 2. What Was Done

- Read the plan in full and inspected the public API in
  `HandEvaluator/HandEvaluator.cs` via targeted reads/greps (not the whole 42k-line file).
- Confirmed signatures/semantics empirically with a throwaway console probe (deleted
  afterwards) before writing assertions.
- Added the P0 test suite under `HandEvaluator.Test/Core/` plus shared scaffolding
  (`Traits.cs`, `Fixtures/CardMasks.cs`, `Fixtures/KnownHands.cs`).
- All new P0 tests are tagged `[Trait("Category","Fast")]`; the tagged suite runs in
  **~54 ms** (Release), well within the "seconds" requirement.
- Verified in both configurations. Added `#if DEBUG` guards for Debug-only argument
  validation so both Release and Debug runs are green.

### Files added

| Path | Purpose |
| --- | --- |
| `HandEvaluator.Test/Traits.cs` | `Category` trait constants (`Fast`/`Slow`/`Oracle`/`Perf`) |
| `HandEvaluator.Test/Fixtures/CardMasks.cs` | Helpers: parse, `MakeHand(mask)`, all 24 suit permutations, `Binom(n,k)` |
| `HandEvaluator.Test/Fixtures/KnownHands.cs` | Theory data: category examples (5/6/7-card), ordered pairs, equal pairs, representatives, descriptions |
| `HandEvaluator.Test/Core/EvaluateCategoryTests.cs` | P0-A |
| `HandEvaluator.Test/Core/EvaluateKickerTests.cs` | P0-B |
| `HandEvaluator.Test/Core/WheelAndStraightFlushTests.cs` | P0-C |
| `HandEvaluator.Test/Core/MaskEncodingTests.cs` | P0-D |
| `HandEvaluator.Test/Core/SuitPermutationTests.cs` | P0-E |
| `HandEvaluator.Test/Core/CompareAndDescriptionTests.cs` | P0-F |

Namespaces stay flat (`HandEvaluator.Test`) to match the existing tests; folders mirror §4.

### Not changed

- `HandEvaluator/HandEvaluator.cs`, `HandAnalysis.cs`, `HandIterator.cs`, `PocketHands.cs`,
  XML docs, `File.csproj`, existing test files, and `TestConsts.cs` are untouched.

---

## 3. Test Counts per P0 Group

Counted from `dotnet test --logger "console;verbosity=detailed"` (theory cases expanded).

| Group | Class | Debug | Release |
| --- | --- | ---: | ---: |
| P0-A | `EvaluateCategoryTests` | 91 | 91 |
| P0-B | `EvaluateKickerTests` | 36 | 36 |
| P0-C | `WheelAndStraightFlushTests` | 11 | 11 |
| P0-D | `MaskEncodingTests` | 29 | 23 |
| P0-E | `SuitPermutationTests` | 13 | 13 |
| P0-F | `CompareAndDescriptionTests` | 30 | 30 |
| **New P0 total** | | **210** | **204** |
| Existing (untagged) tests | | 20 | 20 |
| **Suite total** | | **230** | **224** |

The 6-test Debug/Release delta is the `#if DEBUG` argument-validation group in
`MaskEncodingTests`.

---

## 4. Commands Run and Results

All commands run from the repo root `C:\ck\HandEvaluator`.

| Command | Result |
| --- | --- |
| `dotnet test HandEvaluator.Test -c Release` | **Passed** — 224 passed / 0 failed, ~104 ms |
| `dotnet test HandEvaluator.Test -c Debug` | **Passed** — 230 passed / 0 failed, ~132 ms |
| `dotnet test HandEvaluator.Test -c Release --filter "Category=Fast"` | **Passed** — 204 passed / 0 failed, **54 ms** |
| `dotnet test HandEvaluator.Test -c Debug --filter "Category=Fast"` | **Passed** — 210 passed / 0 failed, **56 ms** |

No compiler warnings in the final build.

---

## 5. API / Semantic Deviations from the Plan

The plan's expectations were checked against actual source behavior; the following
deviations were found and handled (behavior characterized, not "fixed", since library
source must not change):

1. **`Hand.CompareTo` is broken for every input (latent defect).**
   `CompareTo` performs `if (h == null) return -1;` on a `Hand`, which invokes the
   user-defined `operator==`. That operator dereferences both operands, so the null check
   itself throws:
   - Debug: `System.ArgumentNullException` (the `#if DEBUG` guard in `operator==`).
   - Release: `System.NullReferenceException`.
   Consequently the P0-B/P0-F requirement that "`CompareTo` must agree with the operator
   overloads" cannot be satisfied. `CompareAndDescriptionTests.CompareTo_IsBroken_LatentDefect_Characterization`
   asserts it throws `SystemException` in both configurations, with a comment describing
   the defect. Operators themselves all work and are covered.

2. **`ValidateHand` does not enforce a 7-card maximum.** The plan's P0-D bullet
   ">7 cards rejected" is incorrect: `ValidateHand` only checks token syntax and duplicate
   cards. `ValidateHand("Ac 2d 3h 4s 5c 6d 7h 8s 9c")` returns `true`. Characterized in
   `MaskEncodingTests.ValidateHand_DoesNotEnforceSevenCardMaximum_Characterization`; the
   evaluator's own 7-card cap is covered by Debug-only `Evaluate`/`EvaluateType` tests.

3. **`Hand.ToString()` returns card notation, not the description.** The plan's P0-F bullet
   "`Hand.ToString()` equals `Description`" is wrong: `ToString()` returns
   `"<pocket> <board>"`. Characterized in
   `CompareAndDescriptionTests.ToString_ReturnsCardNotation_NotDescription_Characterization`.

4. **Description cross-API equality holds only for non-flush categories.** For Flush and
   Straight Flush, `DescriptionFromMask` adds suit/high-card detail (e.g.
   `"Flush (Clubs) with King high"`) while `DescriptionFromHandValue` returns the generic
   `"A flush"` / `"A straight flush"`. Tests assert exact/cross-API equality for
   HighCard–FourOfAKind and substring + per-API checks for Flush/Straight Flush.

5. **Only the one-argument `EvaluateType(ulong)` validates in Debug.** The
   `EvaluateType(ulong, int)` overload has no validation, so `EvaluateType(0UL, 0)` returns
   a value rather than throwing. Debug test asserts only the one-argument overload throws.

6. **`MaskToDescription` == `DescriptionFromMask`** was confirmed (the obsolete wrapper
   delegates and only adds a Debug range check), so the plan's follow-up concern about an
   intentional difference does **not** apply.

7. **Debug-only validation split.** Null/empty `ValidateHand(pocket, board)`, duplicate
   `ParseHand`, out-of-range card counts (`Evaluate`), empty-mask `EvaluateType`/
   `DescriptionFromMask`, and out-of-range `CardRank`/`CardSuit` throw only under `#if DEBUG`.
   These tests are compiled only in Debug so Release stays green.

---

## 6. P0 Pieces Deliberately Deferred

- **None of P0-A..P0-F was skipped.** All bullets from §3 P0 are represented, adjusted for
  the semantic deviations above.
- **P2-B (full 5-card enumeration / frequency oracle)** was intentionally **not**
  implemented, per the task and because it is not P0. The `Traits.Slow`/`Traits.Oracle`
  constants are in place for when it is.
- **Latent `IsConnected` test bug** (calls `IsSuited` three times in
  `HandAnalysisTests.cs`) was **left in place and is noted here**: it was not in a file
  touched by this task, and the instruction was to fix it only if a touched file. Recommend
  fixing it when P1 analysis tests are implemented.
- **`CompareTo` was not fixed** (would require modifying library source).

---

## 7. Follow-ups

1. Fix `Hand.CompareTo` (use `ReferenceEquals(h, null)` or `obj is not Hand`) so it no
   longer throws; then tighten the characterization test into an ordering assertion.
2. Fix the `IsConnected` test in `HandAnalysisTests.cs` when P1 work begins.
3. Consider extending `TestConsts.cs` or consolidating shared fixtures if P1/P2 add more
   theory data.
4. Wire the CI default gate to `dotnet test --filter "Category=Fast"`.
