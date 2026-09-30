# HandEvaluator — Comprehensive Test-Improvement Plan

- **Timestamp:** 260930-1746
- **Branch:** `net10-uplift`
- **Planning agent:** plan only — no code/tests written, no source modified, nothing committed.

---

## 1. Original Prompt / Request

> Create a comprehensive test-improvement plan for the HandEvaluator repo at `C:\ck\HandEvaluator`
> (branch `net10-uplift`). Read the existing test files and skim the public API surface of the
> three library files to understand what is and isn't covered. Produce a prioritized plan listing
> concrete, high-value test candidates organized by area (core `Hand.Evaluate`, `HandAnalysis`,
> `HandIterator`, `PocketHands`, property/invariant/oracle tests). For each group specify what to
> test, why it matters, example test cases with concrete inputs, expected assertions, and note
> where an oracle/reference is needed. Recommend test project structure, packages, and phasing.
> Call out risks. Write the plan into `_planning/` using the repo naming convention. Do NOT write
> code/tests, modify source, or commit.

---

## 2. Summary of What Was Done

- Inspected the repo layout on branch `net10-uplift` (working tree clean at inspection time).
- Read all four existing test files and the test `.csproj`.
- Skimmed the public API surface of the four library source files via grep (declarations, XML
  docs, enum bodies) rather than reading the 42k-line generated file in full.
- Identified the `_planning/` naming convention: `YYMMDD-HHmm_<kebab-case-slug>.md`
  (existing files: `260930-1631_net10-migration-readme-api-docs-lgpl-license.md`,
  `260930-1710_hand-evaluator-speedtest-console-app.md`,
  `260930-1733_2player-throughput-benchmark.md`).
- Produced this prioritized, phased plan.

### Source / test inventory

| Path | Size | Role |
| --- | --- | --- |
| `HandEvaluator/HandEvaluator.cs` | ~42,700 lines | Core evaluator: `Hand` partial class, `HandTypes` enum, parse/description/`Evaluate`/`EvaluateType`, operators, mask tables |
| `HandEvaluator/HandAnalysis.cs` | ~824 lines | `HandOdds`, `Outs`/`OutsMask`, `HandPlayerOpponentOdds`, `HandPotential`, `IsSuited`/`IsConnected`/`GapCount` |
| `HandEvaluator/HandIterator.cs` | ~1,747 lines | `Hands(...)`, `RandomHands(...)`, `PocketHand169Type`, `PocketHand169Enum` |
| `HandEvaluator/PocketHands.cs` | ~5,257 lines | `PocketHands` collection + grouping/range/169 helpers, many operators |
| `HandEvaluator.Test/*.cs` | 4 files, ~250 lines total | xUnit + Shouldly tests |
| `HandEvaluator.SpeedTest` | console | Throughput/perf harness (not a correctness suite) |

### Existing coverage (what the 4 test files touch)

`HandEvaluatorTests.cs`: empty `Hand`, royal-flush construction, `ValidateHand(string)`,
`ParseHand(string)`/`ParseCard`/`NextCard`, `DescriptionFromMask`/`DescriptionFromHand`,
`Evaluate(ulong)` (one comparison), `==`/`!=`/`>`/`>=`/`<`.

`HandAnalysisTests.cs`: `HandOdds` (loose ordering assertions only), `Outs`/`OutsMask` (one case),
`IsSuited`, `GapCount`, `HandPlayerOpponentOdds(ulong,ulong)` (one hard-coded value), `HandPotential`
(one ordering assertion). Note: `IsConnected` test actually calls `IsSuited` three times — a latent
test bug to fix.

`HandIteratorTests.cs`: `Hands(int)` counts for n=2, `Hands(shared,dead,int)` count for n=2,
`RandomHands(int,int)` (asserts all 100 random 2-card hands ≤ AA — weak, unseeded).

`TestConsts.cs`: a handful of string/mask fixtures.

### Public API with **zero** test references (explicit list)

Core `Hand` / `HandEvaluator.cs`:
`MaskValue` (property), `PocketMask` is touched indirectly but `MaskValue`/`BoardMask` setter paths
untested; `HandValue` property, `Description` property, `ToString()`, `Mask(int)`,
`MaskToString(ulong)`, `MaskToDescription(ulong)`, `DescriptionFromHandValue(uint)`,
`DescriptionFromHandValueInternal(uint)`, `HandType(uint)`, `TopCard(uint)`, `EvaluateType(ulong)`,
`EvaluateType(ulong,int)`, `Evaluate(string)`, `Evaluate(ulong,int)` (indirect only),
`CompareTo(object)`, `Equals(object)`, `GetHashCode()`, `BitCount(ulong)`,
`ValidateHand(string,string)` (2-arg overload), `ParseHand(string, ref int)`,
`ParseHand(string,string, ref int)`, `CardRank(int)`, `CardSuit(int)`,
`PreCalcPlayerOdds` / `PreCalcOppOdds`, `QueryPerformanceCounter` / `QueryPerformanceFrequency`,
`CardMasksTable`, `TwoCardTable`, `Pocket169Table`, `CardTable`, all rank/suit constants
(`Hearts`, `Rank2`…`RankAce`, `NumberOfCards`).

`HandAnalysis.cs`:
`HandPlayerOpponentOdds(string,string,...)` overload, `HandPotentialOpp`.

`HandIterator.cs`:
`PocketHand169Type(ulong)`, `RandomHands(ulong,ulong,int,int)` (shared/dead overload),
`RandomHands(ulong,ulong,int,double)`, `RandomHands(int,double)` duration overloads,
`PocketHand169Enum` mapping, `Pocket169Table`.

`PocketHands.cs` (almost entirely untested):
all 5 constructors, `AllHands`, `Connected`, `Suited`, `Offsuit`, `Pair`, `Gap1`/`Gap2`/`Gap3`/`Gap`,
`Group1`…`Group8`, `GroupNone`, `Count`, indexer `this[int]`, `GroupType(ulong)`,
`PocketCard169StringToEnum(string)`, `WinOdds(ulong)`, `PocketHand169TypeCount(ulong)`,
`IsConnected`/`IsSuited`/`GapCount`, `Pocket169(string)`, `FindFixCard169`, `FixCard169`,
`BuildFix169Table`, `PocketCards`, `PocketCards169`, `Condense169`, `RemoveDead`, `Card169Max`,
`Card169Min`, `PocketCards169Wild`, `PocketCards169Range`, `PocketCard169Range`, `Group`,
`GroupRange`, every operator (`|`, `+`, `&`, `-`, `!`, `==`, `!=`, `<`/`<=`/`>`/`>=` against
double / `PocketHand169Enum` / `GroupTypeEnum`), `LT`/`LE`/`GT`/`GE`, `Hands169` overloads,
`GetEnumerator`, `ToArray`, `Contains`, `GetHashCode`, `Equals`, implicit `ulong[]` conversion,
`GroupTypeEnum`.

> Note: `README.md` documents `Hand.GetRandomHand(ulong,int,System.Random)` as public, but the
> source declares it `static private` (`HandIterator.cs:1618`). Verify before writing any test
> against it; likely a README/XML doc drift issue.

---

## 3. Prioritized Test Candidates

Priority tiers: **P0** = correctness of the core evaluator (must have, cheap).
**P1** = analysis/iterator semantics and invariants. **P2** = `PocketHands` + oracle/count suites.
**P3** = property-based/nice-to-have.

### P0-A. Core `Hand.Evaluate` — category boundaries (5/6/7 cards)

**What:** Every `HandTypes` value is produced correctly, and `HandType(handval)` matches
`EvaluateType`. Cover high card → straight flush, including royal flush (ace-high straight flush).

**Why:** This is the library's entire reason to exist; current tests only assert one comparison and
flushes. Category boundaries (e.g., trips vs two pair vs full house when duplicates ≥ 3) are exactly
where the bit-twiddling logic in `Evaluate` can regress.

**Cases & expected assertions** (use `Hand.Evaluate(string)` + `Hand.HandType` / `Hand.EvaluateType`):

| Hand | Expected `HandType` |
| --- | --- |
| `"2c 7d 9h Js Kd"` | `HighCard` |
| `"2c 2d 7h Js Kd"` | `Pair` |
| `"2c 2d 7h 7s Kd"` | `TwoPair` |
| `"2c 2d 2h Js Kd"` | `Trips` |
| `"5c 6d 7h 8s 9d"` | `Straight` |
| `"2c 5c 7c Jc Kc"` | `Flush` |
| `"2c 2d 2h 7s 7d"` | `FullHouse` |
| `"2c 2d 2h 2s Kd"` | `FourOfAKind` |
| `"5c 6c 7c 8c 9c"` | `StraightFlush` |
| `"Ac Kc Qc Jc Tc"` | `StraightFlush` (royal) |

Assert `Hand.HandType(Hand.Evaluate(h)) == Hand.EvaluateType(Hand.ParseHand(h))`.
Assert `Hand.EvaluateType(mask)` equals `Hand.EvaluateType(mask, BitCount(mask))`.

**Oracle:** none needed — hand type is self-evident from the string.

### P0-B. Core `Hand.Evaluate` — kickers and tie-breaks

**What:** Ordering within a category by kickers / top cards. This is where "equal value" must be
exact (drives `==` and `HandOdds` tie accounting).

**Cases & expected assertions:**

- Pair: `"Ac Ad Kc 7h 2s"` > `"Ac Ad Qc 7h 2s"` (K kicker beats Q), and each greater than
  `"Ac Ad Jc 7h 2s"`.
- Two pair: `"Ac Ad Kc Kh 2s"` > `"Ac Ad Qc Qh Ks"` (higher second pair wins);
  `"Ac Ad Kc Kh Qs"` == `"Ah As Kd Ks Qd"` (same ranks, different suits → equal value).
- Trips: `"Ac Ad Ah Kc 2s"` > `"Ac Ad Ah Qc Js"` (kicker).
- Quads + kicker: `"Ac Ad Ah As Kc"` > `"Ac Ad Ah As Qc"`.
- Full house ordering: `"Kc Kd Kh 2c 2d"` > `"Qc Qd Qh Ac Ad"` (trips rank dominates);
  `"Kc Kd Kh Ac Ad"` > `"Kc Kd Kh 2c 2d"`.
- Flush ordering: compare 5th card (`"Ac Kc Qc Jc 9c"` > `"Ac Kc Qc Jc 8c"`).
- `HandType` must be equal for the tied pair; the numeric `Evaluate` values must be equal.
- `Hand.CompareTo` must agree with the operator overloads.

**Oracle:** none.

### P0-C. Core `Hand.Evaluate` — wheel, steel wheel, straight boundaries

**What:** Ace-low straight (`A-2-3-4-5`) and ace-low straight flush ("steel wheel") are recognized,
and the high card is correctly reported as a 5, not an Ace. Also ensure `A-K-Q-J-T` is the highest
straight and that disconnected cards do not form a straight.

**Cases & expected assertions:**

- `Hand.EvaluateType(Hand.ParseHand("Ac 2d 3h 4s 5c"))` == `Straight`;
  `Hand.TopCard(Hand.Evaluate(...))` == `Hand.Rank5`. A wheel must lose to a `6-high` straight:
  `"2c 3d 4h 5s 6c"` > wheel.
- Steel wheel `"Ac 2c 3c 4c 5c"` is `StraightFlush`; a `6-high` straight flush beats it.
- Royal `"Ac Kc Qc Jc Tc"` beats `"Kc Qc Jc Tc 9c"` straight flush.
- Non-straights: `"Ac 2d 3h 4s 6c"` == `HighCard`; `"Ac Kd Qh Js 9c"` == `HighCard`.
- 7-card wheel: `"Ac 2d 3h 4s 5c 9d Kh"` == `Straight`.
- Duplicate-rank straights with paired board must not beat a true straight (e.g.
  `"5c 5d 6h 7s 8d 9c Ac"` → `Straight` 9-high, not trips).

**Oracle:** none.

### P0-D. Mask encoding / parse / format invariants

**What:** The bitmask encoding is the contract every other API depends on.

**Cases & expected assertions:**

- `Hand.Mask(i)` is a single bit and `Hand.BitCount(Hand.Mask(i)) == 1` for i in 0..51.
- `Hand.CardMasksTable[i] == Hand.Mask(i)`.
- For each of the 52 card strings generated as rank × suit, `Hand.MaskToString(Hand.ParseHand(card))`
  round-trips to the canonical card, and `Hand.ParseHand(Hand.MaskToString(...))` round-trips the mask.
- Suit/rank offsets: `Hand.CardSuit(Hand.ParseCard("2c")) == Hand.Clubs`,
  `Hand.CardRank(Hand.ParseCard("2c")) == Hand.Rank2`, same for all four suits; ranks are
  contiguous `Rank2..RankAce`.
- `Hand.ParseHand(h, ref cards)` returns a mask with `BitCount == cards` and `cards` equals the
  number of distinct cards.
- `Hand.ValidateHand`: duplicates rejected (`"As As"`), >7 cards rejected, malformed tokens rejected,
  empty string handling; pocket/board `ValidateHand(pocket,board)` overlap rejected.
- `Hand.BitCount((1UL<<52)-1) == 52`; `Hand.BitCount(0)==0`.
- `Hand.MaskValue`, `PocketMask`, `BoardMask`, `PocketCards`, `Board` on a constructed `Hand`
  are mutually consistent (`MaskValue == PocketMask | BoardMask`).

**Oracle:** none. (Counts are authority: 52, 1326, 22100, 270725, 2598960.)

### P0-E. Suit-isomorphism / permutation invariance

**What:** `Hand.Evaluate` returns the same value for a hand with suits permuted (hand values must not
depend on which suit is which). This is a powerful, cheap correctness property.

**Cases & expected assertions:**

- Generate all 24 suit permutations of a representative hand from each category; assert all
  `Hand.Evaluate` values are equal and `HandType`/`TopCard` unchanged.
- Test the wheel and steel wheel under all suit permutations.
- Compare a hand against its own rank-only permutation where possible (for flush/straight-flush the
  permutation must map the flush suit consistently — assert equality holds).

**Oracle:** none (invariant).

### P0-F. Compare/equal/description API

**What:** `==`, `!=`, `<`, `<=`, `>`, `>=`, `CompareTo`, `Equals`, `GetHashCode`, and description
strings. Current tests cover operators only partially and never `CompareTo`/`Equals`/`GetHashCode`.

**Cases & expected assertions:**

- `CompareTo` sign agrees with operators for a table of ordered hands.
- `Equals` true for same-rank different-suit hands; `GetHashCode` equal for equal hands.
- `DescriptionFromHand` / `DescriptionFromMask` / `DescriptionFromHandValue` all return the same
  string for the same hand; include rank names in output (e.g., quad aces contains `"Four of a Kind"`).
- `MaskToDescription(mask)` == `DescriptionFromMask(mask)` for a sample of each category.
- `Hand.ToString()` equals `Description` after construction.

**Oracle:** description wording is implementation-defined; assert substrings and cross-API
consistency rather than exact prose (exact strings only for stable, well-known cases).
**Follow-up:** `MaskToDescription` vs `DescriptionFromMask` may differ intentionally (e.g. abbreviated
rank names); capture as characterization tests after confirming with the maintainer.

### P1-A. `HandIterator` completeness / uniqueness / counting

**What:** `Hands(n)` and `Hands(shared,dead,n)` enumerate every valid combination exactly once with
correct counts and cardinality.

**Cases & expected assertions:**

- Counts: `Hands(1)=52`, `Hands(2)=1326`, `Hands(3)=22100`, `Hands(4)=270725`,
  `Hands(5)=2598960` (5 is **Slow** — see phasing). `Hands(0)` yields exactly one element `0UL`.
- Uniqueness: `Hands(3).ToHashSet().Count == 22100`.
- Cardinality: every yielded mask has `Hand.BitCount == n`.
- Constraints: `Hands(shared=As, dead=Ks, n=2)` → count `C(50,1)=50`, all contain `As`, none contain
  `Ks`. `Hands(shared=As Ah, dead=0, n=2)` → 1 element equal to `As|Ah`.
  `Hands(shared=As, dead=Ks, n=1)` → shared already satisfies n; verify documented behavior
  (`shared` must be in the hand — confirm whether n counts total cards including shared; the code
  adds `shared` after choosing `n - BitCount(shared)` cards).
- Out-of-range n behavior: in `Debug`, `Hands(8)` throws `ArgumentOutOfRangeException`.
- `TwoCardTable`/`CardMasksTable` lengths (1326 / 52) and that `Hands(2)` yields exactly the
  `TwoCardTable` contents.

**Oracle:** binomial counts / known constants.

### P1-B. `HandIterator` random hands (invariants only, no exact values)

**What:** `RandomHands(n,trials)` returns exactly `trials` masks, each with `n` bits, respecting
`shared`/`dead`.

**Cases & expected assertions:**

- `RandomHands(5, 1000).Count() == 1000`; all have `BitCount == 5`.
- `RandomHands(shared=As, dead=Ks, ncards=2, trials=500)`: every result contains `As`, none contain
  `Ks`, all `BitCount == 2`, `BitCount(shared|dead|result) == 3`.
- `RandomHands(7, 100)`: every result has 7 distinct cards (no duplicate bits).
- Determinism is **not** expected (`new Random()` is unseeded) — do **not** assert exact values.
- Guard: avoid `ncards` so large that `GetRandomHand` could spin (ncards ≤ 52 minus dead). Add a
  documented boundary test only if behavior is well-defined.

**Oracle:** none — invariants only.

### P1-C. `HandAnalysis.HandOdds` — invariants + known equities

**What:** Exhaustive board enumeration produces self-consistent win/tie/loss tallies and matches
published equities.

**Why:** Current test only asserts loose ordering. Strong invariants catch regressions cheaply.

**Cases & expected assertions:**

- **Invariant (all inputs):** for each player i,
  `wins[i] + ties[i] + losses[i] == totalHands`. For each board, each player is win/tie/loss exactly
  once, so this must hold.
- Sum of wins across players plus ties accounted correctly; `totalHands` equals
  `C(remaining, 5 - boardCount)` for two players with distinct pockets and no dead.
- Flop 2-player, e.g. `As Ad` vs `Ks Kd` on `"Qs 6s 2d"`: `totalHands == C(45,2)=990`; assert
  `wins[0]+ties[0]+losses[0] == 990` and `wins[0] > wins[1]`.
- **Known equity (oracle):** heads-up preflop `As Ad` vs `Ks Kd` → AA ≈ 81.95% / KK ≈ 18.05%
  (with small tie %). Assert within ±0.5% after converting tallies to fractions, or mark as
  exact-enumeration characterization once the value is captured. Preflop enumeration is
  `C(48,5)=1,712,304` boards → **Slow**.
- Duplicate card input throws (Debug): pocket/pocket, pocket/board, pocket/dead, board/dead.
- Empty board and 3/4/5-card board all run without error; 5-card board `totalHands == 1`.
- Dead-card exclusion: dead cards never appear in enumerated boards (indirectly verified by
  totalHands decreasing by the right binomial).

**Oracle:** published preflop equities (e.g., standard poker-equity references); or capture once as
a golden value and treat as characterization. Exact values for exhaustive enumeration should be
deterministic, so golden values are acceptable.

### P1-D. `HandAnalysis.Outs` / `OutsMask`

**What:** Known draw counts; `Outs == BitCount(OutsMask)`; opponents block outs.

**Cases & expected assertions:**

- Flush draw, no opponents: hero `"As Ks"` on `"Qs 6s 2d"` → 9 spade outs.
- Open-ended straight draw: hero `"9c 8d"` on `"7h 6s 2d"` → 8 outs.
- Gutshot: hero `"9c 5d"` on `"8h 6s 2d"` → 4 outs (a 7).  *(Verify exact semantics against the
  implementation's definition of "improves the hand" — it iterates single cards and compares hand
  values; a card that improves but is already beaten by an opponent is excluded.)*
- `Outs(...) == Hand.BitCount(OutsMask(...))` for each of the above.
- Existing case already covers K/K vs A/A on `Qs 6s 2d` → 2 outs (`Kh`,`Kc`); keep as a regression
  anchor and additionally assert `OutsMask` equals exactly `Kh|Kc` (already present).
- Opponent-blocked: hero flush draw with an opponent holding one spade → outs < 9 and the held
  spade is absent from `OutsMask`.
- Illegal board sizes (turn/river, i.e. ncards ≠ 5 or 6) throw in Debug.

**Oracle:** hand-derived combinatorial counts; the exact "improves" definition should be pinned
down from `HandAnalysis.cs:159-225` before finalizing gutshot/open-ended numbers.

### P1-E. `HandAnalysis.HandPlayerOpponentOdds` and `HandPotential`

**What:** Distribution vectors are well-formed and deterministic; `HandPotential` returns
normalized opposing potentials.

**Cases & expected assertions:**

- Both overloads (mask and string) produce identical vectors for identical inputs.
- `player.Length == 9`, `opponent.Length == 9`; each sums to 1.0 within epsilon (existing test
  checks `player.Sum()+opponent.Sum()==1` for one case; generalize).
- Known characterization: `HandPlayerOpponentOdds(ulAK, 0, ...)` has `player[Pair] ≈ 0.295079...`
  (existing) — keep as golden, add a tolerance and cross-check that no entry is NaN/negative.
- Quad board `"2c 2d 2h 2s"`: `player[FourOfAKind] == 1.0` (or near, allowing ties) for hero AK;
  assert exactly 8 of 9 entries are zero as in the existing test but make it exact.
- `HandPotential(pocket, board, out ppot, out npot)`: `0 ≤ ppot,npot ≤ 1`; with a 5-card board
  potentials should be 0 (no more cards) — confirm semantics; monotonic sanity: a made draw should
  have `ppot > 0`.
- Determinism: no seed involved (exhaustive), so repeated calls give identical results.

**Oracle:** existing golden value; confirm semantics from source before asserting river behavior.

### P2-A. `PocketHands` collection semantics

**What:** Construction, counting, enumeration, equality, operators, and 169-combo mapping.

**Cases & expected assertions:**

- `new PocketHands(Hand.ParseHand("As Ad")).Count == 1`;
  `new PocketHands(ulong[]{AsAd, KsKd}).Count == 2`.
- `PocketHands.AllHands.Count == 1326`; `AllHands.Offsuit` etc. counts derived from known combo
  math (pairs 13×6=78, suited 78 combos, offsuit 78×12=936, total 1326).
- Properties: `Pair`, `Suited`, `Offsuit`, `Connected`, `Gap1/Gap2/Gap3/Gap` partition `AllHands`
  (sum of counts == 1326); set relationships (`Suited` ∩ `Offsuit` empty; `Pair` ⊂ `Connected`).
- Indexer/`ToArray`/`GetEnumerator`/`Contains`/`GetHashCode`/`Equals` consistency.
- `GroupType(Hand.ParseHand("As Ad")) == GroupTypeEnum.Group1`;
  `GroupType(Hand.ParseHand("7d 2c")) == GroupTypeEnum.GroupNone` (or Group8 — confirm).
- `PocketCard169StringToEnum("AA") == PocketAA`, `"AKs"`, `"AKo"` map correctly; invalid string →
  `None`.
- `PocketCard169Range(PocketAA, Pocket22).Count == 78` (13 pairs × 6 combos).
- `PocketHands.Pocket169("AKs").Count == 4`; `PocketCards("AKs")` expands to the 4 suited combos.
- `Condense169(new PocketHands(AllHands))` has 169 entries;
  `PocketHand169TypeCount(mask)` for one member equals its combo multiplicity (pairs 6, suited 4,
  offsuit 12).
- Operators (`|`, `+`, `&`, `-`, `!`, comparisons, `==`/`!=`) round-trip against set semantics on
  small explicit sets.
- `RemoveDead(dead, PocketHands)` removes every combo intersecting dead; `Card169Max`/`Card169Min`/
  `PocketCards169Wild` behavior documented.
- `PocketHand169Type(single combo)` maps to the correct `PocketHand169Enum`.

**Oracle:** binomial combo counts; explicit small sets.

### P2-B. Oracle / frequency-count cross-checks (the big correctness net)

**What:** Enumerate all 5-card hands with `Hand.Hands(5)` and tally `Hand.EvaluateType`. Compare to
the published 5-card frequency table. Optionally enumerate all 7-card hands (Slow) and compare to the
published 7-card table.

**Why:** The 42k-line generated table file cannot be hand-verified; category frequencies are the
strongest available independent oracle.

**Expected 5-card counts** (sum 2,598,960):

| Category | Count |
| --- | --- |
| StraightFlush | 40 |
| FourOfAKind | 624 |
| FullHouse | 3,744 |
| Flush | 5,108 |
| Straight | 10,200 |
| Trips | 54,912 |
| TwoPair | 123,552 |
| Pair | 1,098,240 |
| HighCard | 1,302,540 |

**Expected 7-card counts** (sum 133,784,560, `Slow`):

| Category | Count |
| --- | --- |
| StraightFlush | 41,584 |
| FourOfAKind | 224,848 |
| FullHouse | 3,473,184 |
| Flush | 4,047,644 |
| Straight | 6,180,020 |
| Trips | 6,461,620 |
| TwoPair | 31,433,400 |
| Pair | 58,627,800 |
| HighCard | 23,294,460 |

Also assert the 5-card royal count is 4 (one per suit) and the 5-card steel-wheel count is 4.
Add a cross-check of a sample of hands against an independent evaluator (Two Plus Two evaluator,
`phe`, or Python `treys`) — see "Oracle strategy" below.

**Oracle:** published frequency tables + an independent evaluator for samples. **Generated tables
cannot be hand-verified** — this is the mitigation.

### P3-A. Property-based / invariant tests

**What:** Generalize the above into property tests over random masks.

- `Evaluate` is total for `1 ≤ BitCount ≤ 7` and never throws (Release) / throws only for invalid
  in Debug.
- For random 5–7 card masks: `Hand.HandType(Evaluate(m)) == EvaluateType(m)`.
- Monotonicity sanity: adding a card never decreases the best 5-card category (i.e.,
  `EvaluateType(m | card) >= EvaluateType(m)` in enum order) — verify this is actually a valid
  property for 7-card evaluation before enforcing.
- Suit permutation invariance over random hands.
- `Evaluate(m)` equal for any two masks with identical rank multiset and flush/straight structure.
- Compare `Hand.Equal` against `Evaluate` equality over random pairs.

**Framework:** optional `FsCheck.Xunit` (or `CsCheck`) — recommend adding it, but keep property
tests in a separate trait/file so the core suite has no extra dependency if the team prefers.

### P3-B. Performance regression guard

**What:** Reuse `HandEvaluator.SpeedTest` as a smoke check rather than a unit test; optionally add a
loose upper-bound test (`Evaluate` throughput) tagged `Category=Perf` and excluded from default CI.

**Why:** The repo already has a throughput benchmark (`README.md` §performance). Avoid flaky
timing assertions in the main suite; keep them opt-in.

---

## 4. Test Project Structure, Helpers, Packages

Keep the existing `HandEvaluator.Test` project; reorganize into folders (namespaces can stay flat
`HandEvaluator.Test` to minimize churn, or mirror folders).

Suggested layout:

```
HandEvaluator.Test/
  Core/
    EvaluateCategoryTests.cs
    EvaluateKickerTests.cs
    WheelAndStraightFlushTests.cs
    MaskEncodingTests.cs
    SuitPermutationTests.cs
    CompareAndDescriptionTests.cs
  Analysis/
    HandOddsTests.cs
    OutsTests.cs
    HandPlayerOpponentOddsTests.cs
    HandPotentialTests.cs
  Iterators/
    HandsEnumerationTests.cs
    RandomHandsTests.cs
  PocketHands/
    PocketHandsCollectionTests.cs
    PocketHandsGroupingTests.cs
    PocketHands169Tests.cs
    PocketHandsOperatorTests.cs
  Reference/
    HandCategoryFrequencyTests.cs   // Category=Slow
    EvaluatorCrossCheckTests.cs      // Category=Oracle (uses embedded reference data)
    reference-hand-values.json       // generated oracle samples (committed)
  Fixtures/
    KnownHands.cs        // [Theory] MemberData tables (ordered hand sets, category examples)
    CardMasks.cs         // helpers: parse list, suit permutations, binomial C(n,k)
    TestConsts.cs        // existing, extend
  Traits.cs              // Category trait constants
```

**Helpers:**

- `CardMasks.ParseAllNotation(...)`, `PermuteSuits(mask, permutation)`, `Binom(n,k)`,
  `AllRankMultisets()`.
- `KnownHands.OrderedHands` — a `MemberData` array of ordered hands used by compare/kicker tests.
- Golden data in `Reference/reference-hand-values.json` generated offline (see below).

**Packages (add):**

- `FsCheck.Xunit` (or `CsCheck`) — property tests (P3). Optional but recommended.
- `Xunit.SkippableFact` — only if duration-based `RandomHands` tests are kept and must be skipped on
  non-Windows (the duration overloads P/Invoke `Kernel32.dll`).
- Existing xUnit 2.9.2 / Shouldly 4.2.1 / coverlet are sufficient for P0–P2.

**Test categorization:**

- `[Trait("Category","Fast")]` — default; target < 10 s total.
- `[Trait("Category","Slow")]` — full 5-card enumeration and preflop `HandOdds`.
- `[Trait("Category","Oracle")]` — independent-evaluator cross-checks.
- `[Trait("Category","Perf")]` — throughput guards.
- Document CI invocation: `dotnet test --filter "Category=Fast"` for the default gate.

**Oracle strategy (no code written now):**

1. Generate reference hand values offline with a known-correct evaluator:
   - Python `treys` / `phevaluator`, or the Two Plus Two evaluator, or compare against a second
     .NET implementation.
2. Emit a deterministic sample (`tools/generate-reference.*` — to be added by the worker) of
   e.g. 10k random 7-card masks plus all 5-card category counts, committed as JSON/CSV under
   `HandEvaluator.Test/Reference/`.
3. `EvaluatorCrossCheckTests` asserts `Hand.Evaluate` ordering agrees with the reference; ties in
   the reference must be ties in ours.
4. Keep the reference generation out of the test run (deterministic committed artifact), so the
   suite has no Python dependency.

---

## 5. Phasing & Execution Order

> Quick wins first; heavier enumeration/oracle suites gated behind traits.

**Phase 0 — Scaffolding (small).**
Add `Traits.cs`, `Fixtures/CardMasks.cs`, extend `TestConsts`, fix the `IsConnected` test that
currently calls `IsSuited`. No library changes.

**Phase 1 — Fast core correctness (P0).**
P0-A category boundaries, P0-B kickers, P0-C wheel/steel wheel, P0-D mask encoding, P0-F
compare/description. Runtime: milliseconds–seconds. This is the highest value-per-line and should
be merged first.

**Phase 2 — Invariants (P0-E, P1-A, P1-B).**
Suit permutation invariance, iterator counts/uniqueness/constraints, random-hand invariants.
The `Hands(5)` count test may take seconds — tag `Slow` if needed.

**Phase 3 — Analysis (P1-C, P1-D, P1-E).**
`HandOdds` invariants (flop first) + preflop equity golden (Slow), Outs known draws, player/opponent
odds invariants, HandPotential bounds.

**Phase 4 — PocketHands (P2-A).**
Collection semantics, grouping, 169 mapping, operators.

**Phase 5 — Oracle / frequency counts (P2-B).**
5-card full enumeration (Fast-to-Slow boundary; expect low single-digit seconds). 7-card full
enumeration is `Slow`/manual — likely minutes; run on demand, not in default CI.

**Phase 6 — Property-based + perf (P3).**
Add FsCheck/CsCheck; property tests for totality, monotonicity, permutation invariance; keep perf
guards opt-in.

**Runtime constraints / notes:**

- `Hand.Hands(5)` enumerates 2,598,960 masks — fine once, but avoid evaluating each with a
  string-reparse; parse is not needed since `Hands` yields masks.
- `Hand.Hands(7)` enumerates 133,784,560 masks — minutes; never in default CI.
- `HandOdds` preflop 2-player enumerates `C(48,5)` boards — seconds; flop/turn are cheap.
- Use `Hand.Evaluate(mask, cards)` / `EvaluateType(mask, cards)` to avoid repeated `BitCount`.
- Duration-based `RandomHands` tests are wall-clock dependent and non-deterministic — prefer
  trial counts; skip on non-Windows.

---

## 6. Risks & Mitigations

1. **Generated lookup tables (42k lines) can't be hand-verified.**
   Mitigation: frequency-count oracle (P2-B) + independent-evaluator cross-check on sampled hands
   (P2-B/P6). Also test the small, human-checkable tables: `TwoCardTable` length 1326,
   `CardMasksTable` length 52, `Pocket169Table` shape.

2. **Stochastic/flaky analysis tests.**
   `RandomHands` uses an unseeded `new Random()` — never assert exact random outputs; test
   invariants (count, bitcount, constraint compliance). `HandOdds`,
   `HandPlayerOpponentOdds`, and `HandPotential` are exhaustive → deterministic; exact/golden
   values with tolerance are safe. Avoid timing-based assertions except under `Category=Perf`.

3. **Platform-specific Win32 interop.**
   `RandomHands(..., double duration)` P/Invokes `Kernel32.dll` (`QueryPerformanceCounter`). These
   tests are Windows-only; skip elsewhere (`Xunit.SkippableFact` or a runtime OS guard). Consider
   whether the duration overloads should be tested at all in CI — coverage gain is low.

4. **Debug vs Release validation mismatch.**
   Argument validation in `Evaluate`, `EvaluateType`, `Hands`, `OutsMask`, `HandOdds` is wrapped in
   `#if DEBUG`. Tests asserting exceptions must run in Debug; in Release those inputs are silently
   unchecked. Document this explicitly and consider a `Release` test pass only for happy paths.

5. **Core `Hand` is a `partial` class split across 4 files with large static tables.**
   Keep tests grouped by the file that owns the API; a change in `HandAnalysis.cs` shouldn't fail
   iterator tests. Avoid over-coupling.

6. **Documentation drift.**
   `README.md` documents `GetRandomHand` as public but it is `private` in source; some README
   entries (`PreCalcPlayerOdds`, `handmask`, etc.) may be `internal`. Verify visibility before
   writing tests; if a genuinely useful API is internal, propose exposing it via `InternalsVisibleTo`
   as a follow-up rather than testing around it.

7. **Ambiguous semantics in `Outs` and `HandPotential`.**
   "Improves the hand" and river-potential behavior need to be pinned down from source before
   asserting exact values. Characterize current behavior first, then lock it in; flag anything that
   looks like a bug to the maintainer instead of encoding it as expected.

8. **Keeping the suite fast.**
   Strict default `Category=Fast` filter; slow/oracle tests opt-in. Prefer typed `Evaluate(mask, n)`
   overloads. Reuse theory data rather than regenerating permutations per test.

9. **No committing / no source changes** (per request). This document is the only artifact.

---

## 7. Decisions Made

- **Use the existing `HandEvaluator.Test` project** rather than creating a new one; reorganize within
  it so history and the existing project reference stay intact.
- **Shouldly + xUnit only for P0–P2.** Add FsCheck/CsCheck and (optionally) SkippableFact only when
  Phase 6 begins.
- **Frequency tables as the primary oracle** for the generated evaluator; an independent evaluator is
  used only for sampled cross-checks to avoid a runtime dependency on Python/third-party code.
- **No exact-value assertions for random APIs**; invariants only.
- **Trait-based selection** (`Fast`/`Slow`/`Oracle`/`Perf`) so CI stays fast and deterministic.
- **Do not encode suspected bugs as expectations** — characterize and escalate.

## 8. Follow-ups for the Worker

1. Confirm the exact semantics of `Outs`/`OutsMask` ("improves") and `HandPotential` on a complete
   board before locking golden values (read `HandAnalysis.cs:142-260`, `:759-824`).
2. Confirm `GroupType` mapping for the weakest hands (`GroupNone` vs `Group8`) and the expected
   `PocketCard169Range` counts.
3. Confirm whether `MaskToDescription` and `DescriptionFromMask` are intended to differ.
4. Verify visibility of README-documented-but-possibly-private APIs (`GetRandomHand`,
   `PreCalcPlayerOdds`, `HandPotentialOpp`, `QueryPerformance*`) and decide on
   `InternalsVisibleTo` if coverage is desired.
5. Decide CI test command and whether `Slow`/`Oracle` run on a nightly schedule.
6. Generate the committed reference-hand-value artifact with an independent evaluator (Phase 5).
