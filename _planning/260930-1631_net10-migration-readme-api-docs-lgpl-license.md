# Plan: Migrate to .NET 10, Move API Docs into README, Add LGPL-3.0 License

**Plan ID:** 260930-1631
**Repo:** `C:\ck\HandEvaluator`
**Branch:** `master` (clean except untracked `.opencode/`)
**Author of plan:** planner agent
**Executor:** @worker

---

## 1. Original Request

> Update the solution to .NET 10, move the generated XML documentation content into
> the README as human-readable API documentation (while keeping the XML file because
> it is used for NuGet packaging), and add the GNU Lesser General Public License v3.0
> (LGPL-3.0) as the project license.

Clarifications already confirmed with the user (these are firm — do **not** change them):

1. Update **BOTH** `HandEvaluator` and `HandEvaluator.Test` from `net5.0` to `net10.0`.
   Update test NuGet packages as needed so the solution builds and tests run on the
   current SDK (`dotnet build` + `dotnet test` must pass).
2. Move the documentation **content** from
   `HandEvaluator/Documentation/HandEvaluator.xml` into `README.md` (human readable),
   but **keep the XML file in place** — it is the compiler-generated
   `DocumentationFile` used for NuGet packaging. **Do not delete it.**
3. Add **LGPL-3.0** as the project license: full `LICENSE` at repo root + a short
   License section in `README.md`.

> **Decision / discrepancy to record:** The user's original message said "Apache-2.0",
> but when asked to confirm they explicitly chose **LGPL-3.0**. The plan and the
> resulting work use **LGPL-3.0**. Apache-2.0 must **not** be used or referenced.

---

## 2. Summary Of What Will Be Done

1. Edit both `.csproj` files: `net5.0` → `net10.0`.
2. Bump the four test-only NuGet packages to versions compatible with .NET 10 / the
   10.0.4xx SDK (test SDK, xunit, xunit runner, coverlet; Shouldly optionally).
3. Verify baseline: `dotnet build` then `dotnet test` on the upgraded solution; fix any
   API/analyzer/obsolete compile issues so the build succeeds.
4. Rewrite `README.md`:
   - Keep the existing project description / provenance line.
   - Add Install / Quick start (from the `<example>` blocks in the XML).
   - Add an **API Reference** section generated from the 474 XML `<member>` entries,
     grouped by type and converted to readable Markdown (prose summaries, fenced C#
     examples, parameter lists). `<exclude/>` members are dropped.
   - Add a **License** section for LGPL-3.0.
5. Add a full-text `LICENSE` file (LGPL-3.0) at the repo root; optionally a
   `COPYING.LESSER` note per LGPL convention.
6. Re-run `dotnet build` + `dotnet test` as the final acceptance check.
7. Write the worker's own completion report to `_planning/`.

**No source-code logic changes are intended.** Only project files, documentation, and
license files change (plus whatever minimal code/test fixes are required to compile
under the newer SDK).

---

## 3. Decisions Made

| # | Decision | Rationale |
|---|----------|-----------|
| D1 | Target `net10.0` (not net8/net9) | SDK 10.0.401 is installed; user asked for .NET 10. |
| D2 | License = **LGPL-3.0** | User explicitly confirmed, overriding the earlier "Apache-2.0" wording. |
| D3 | Use SPDX id `LGPL-3.0-only` in README/NuGet metadata | Simple, unambiguous; note `-or-later` as an open follow-up. |
| D4 | Keep `HandEvaluator/Documentation/HandEvaluator.xml` and the `<DocumentationFile>` property untouched | It is the compiler-generated doc output used by the NuGet package. |
| D5 | README API reference is a **point-in-time, hand/script-generated mirror** of the XML | XML stays the source of truth for NuGet; README is for humans. Add a generated-from note. |
| D6 | Upgrade test packages to current net10-compatible versions | xunit 2.4.1 / runner 2.4.3 / Test SDK 16.7.1 / coverlet 1.3.0 predate net10. |
| D7 | Do **not** rewrite `.sln` VS version unless the build requires it | VS header is cosmetic; avoid unnecessary churn. |

---

## 4. Concrete File-by-File Edits

All paths are relative to `C:\ck\HandEvaluator`.

### 4.1 `HandEvaluator/HandEvaluator.csproj`

Change the target framework only; leave `AssemblyName`, `RootNamespace`, and
`DocumentationFile` exactly as they are.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>HandEvaluator</AssemblyName>
    <RootNamespace>HoldemHand</RootNamespace>
    <DocumentationFile>Documentation/$(AssemblyName).xml</DocumentationFile>
  </PropertyGroup>

</Project>
```

- `<TargetFramework>`: `net5.0` → **`net10.0`**.
- **KEEP** `<DocumentationFile>Documentation/$(AssemblyName).xml</DocumentationFile>`.
- (Follow-up, optional) If this project is packed to NuGet, add:
  ```xml
  <PackageLicenseExpression>LGPL-3.0-only</PackageLicenseExpression>
  ```
  Do **not** add this unless the worker confirms the project is packed. See Risks.

### 4.2 `HandEvaluator.Test/HandEvaluator.Test.csproj`

Change the target framework and bump packages. Recommended versions (verify by letting
`dotnet restore`/`dotnet build` resolve; if a chosen version is unavailable, pick the
nearest newer stable — the exact patch is not critical):

| Package | Current | Recommended (net10) |
|---------|---------|---------------------|
| `Microsoft.NET.Test.Sdk` | 16.7.1 | **17.12.0** (or newer 17.x) |
| `Shouldly` | 4.0.3 | **4.2.1** (optional; 4.0.3 also loads on net10) |
| `xunit` | 2.4.1 | **2.9.2** |
| `xunit.runner.visualstudio` | 2.4.3 | **2.8.2** |
| `coverlet.collector` | 1.3.0 | **6.0.2** |

Resulting project (keep `IncludeAssets` / `PrivateAssets` blocks as-is):

```xml
<TargetFramework>net10.0</TargetFramework>
...
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Shouldly" Version="4.2.1" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="coverlet.collector" Version="6.0.2">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

### 4.3 `LICENSE` (new, repo root)

- Add the **full, unmodified** LGPL-3.0 text.
- Source of truth: `https://www.gnu.org/licenses/lgpl-3.0.txt`
  (the worker may `webfetch` it, or copy from the local .NET/Git installation if a copy
  is available). The file must contain the complete license, including the "GNU LESSER
  GENERAL PUBLIC LICENSE / Version 3, 29 June 2007" header and the full terms.
- Because LGPL-3.0 is legally a set of additional permissions on top of GPL-3.0, add a
  short header comment at the top of `LICENSE` or a separate `COPYING.LESSER` file:

  > This library is free software; you can redistribute it and/or modify it under the
  > terms of the GNU Lesser General Public License as published by the Free Software
  > Foundation; either version 3 of the License, or (at your option) any later version.
  > You should also have received a copy of the GNU General Public License along with
  > this library (see the GNU GPL v3 text, e.g. `COPYING`).
  >
  > SPDX-License-Identifier: LGPL-3.0-only

- Recommended layout (LGPL convention):
  - `LICENSE` → LGPL-3.0 full text (SPDX: `LGPL-3.0-only`).
  - `COPYING.LESSER` → pointer/short note (optional but conventional).
  - Optionally also add `COPYING` with GPL-3.0 text. **Follow-up**, not required by the
    user; note it in the completion report.

### 4.4 `README.md` (rewrite)

Preserve the existing provenance sentence, then add sections in this order:

1. **`# HandEvaluator`** + existing description paragraph (keep the CodeProject
   attribution and "put it on nuget" note, cleaned up).
2. **`## Installation`** — `dotnet add package HandEvaluator` (only if package id is
   `HandEvaluator`; otherwise omit/label as "NuGet").
3. **`## Quick start`** — use the class-level `<example>` at
   `HandEvaluator/Documentation/HandEvaluator.xml` lines 8–39 (the `Hand h1` /
   `Hand h2` comparison program). Put it in a fenced ```` ```csharp ```` block.
4. **`## API Reference`**
   - Intro note, e.g.:
     > This API reference is generated from the compiler documentation file at
     > [`HandEvaluator/Documentation/HandEvaluator.xml`](HandEvaluator/Documentation/HandEvaluator.xml).
     > That XML file is the source of truth and is also shipped with the NuGet package.
   - One `###` subsection per public type, in this order (5 `T:` entries in the XML):
     1. `Hand` (class; `HoldemHand.Hand`)
     2. `Hand.HandTypes` (nested enum)
     3. `Hand.PocketHand169Enum` (nested enum)
     4. `PocketHands` (class; `HoldemHand.PocketHands`)
     5. `PocketHands.GroupTypeEnum` (nested enum)
   - Within a type, group members by member kind:
     - Constructors, Methods, Properties, Fields, and nested types.
   - Do **not** create sections/entries for members whose body is `<exclude/>`
     (71 occurrences in the XML). Drop them entirely.

   **Conversion rules (XML → Markdown):**

   | XML | Markdown |
   |-----|----------|
   | `name="T:HoldemHand.Hand"` | `### Hand` (use the last type segment; show full name once) |
   | `M:` | method entry; convert the mangled name + parameter types to a readable signature using the `<param name>` values |
   | `P:` | property entry |
   | `F:` | field/constant entry |
   | `<summary>` | normal prose paragraph(s); collapse/trim indentation |
   | `<param name="x">desc</param>` | `- **x**: desc` bullet list (or `**Parameters:**` before the signature) |
   | `<returns>` | `**Returns:** desc` line |
   | `<remarks>` | prose under a `**Remarks:**` label |
   | `<example><code> ... </code></example>` | fenced ```` ```csharp ```` block, removing the ~12-space XML indentation but preserving code |
   | `<exclude/>` | drop the whole member |
   | `<see cref="..."/>`, `<see langword="..."/>` | inline code (`code`) / plain text |
   | `<c>...</c>` | inline `` `...` `` |

   Example conversion (illustrative):

   ```
   member name="M:HoldemHand.Hand.Outs(System.UInt64,System.UInt64,System.UInt64[])"
        <summary>Returns the number of outs possible with the next card.</summary>
        <param name="player">Players pocket cards</param>
        <param name="board">The board (must contain either 3 or 4 cards)</param>
        <param name="opponents">A list of zero or more opponent cards.</param>
        <returns>The count of the number of single cards that improve the current hand.</returns>
   ```
   becomes:
   ```markdown
   #### `Hand.Outs(ulong player, ulong board, ulong[] opponents)`
   Returns the number of outs possible with the next card.

   - **player**: Players pocket cards
   - **board**: The board (must contain either 3 or 4 cards)
   - **opponents**: A list of zero or more opponent cards.

   **Returns:** The count of the number of single cards that improve the current hand.
   ```

   - Mangled type mapping: `System.UInt64`→`ulong`, `System.UInt32`→`uint`,
     `System.Int32`→`int`, `System.Int64`→`long`, `System.Double`→`double`,
     `System.String`→`string`, trailing `@`→`out`.
   - The worker may either (a) hand-edit the README following the table above, or
     (b) write a one-off local script (e.g. PowerShell or C#) that parses the XML and
     emits the Markdown, then run it and paste the output. Either is acceptable; the
     final `README.md` must be well-formed Markdown. If a script is used, do **not**
     commit it unless it is genuinely useful (keep the diff focused).
5. **`## License`** — short section:
   ```markdown
   ## License

   This project is licensed under the **GNU Lesser General Public License v3.0**
   (LGPL-3.0). See [`LICENSE`](LICENSE) for the full text.

   SPDX-License-Identifier: LGPL-3.0-only
   ```

---

## 5. Exact Commands The Worker Should Run

Run from `C:\ck\HandEvaluator` (use the `workdir` parameter, not `cd`).

### 5.1 Pre-flight (record baseline)
```bash
git status --short
dotnet --version
```

### 5.2 Restore / build after edits
```bash
dotnet restore HandEvaluator.sln
dotnet build HandEvaluator.sln -c Release --no-restore
```
**Expected:** `Build succeeded.` with `0 Error(s)`. Warnings are acceptable but the
worker should inspect them; if any are new and caused by the migration, fix them.
If `dotnet` rejects a pinned package version, bump to the nearest newer stable and
re-run restore (this is expected to be the only likely iteration).

### 5.3 Run tests
```bash
dotnet test HandEvaluator.sln -c Release --no-build
```
**Expected:** all tests pass, e.g. `Passed! - Failed: 0, Passed: N, Skipped: 0`.

(If `--no-build` fails because test discovery needs a fresh build, drop the flag.)

### 5.4 Optional license / formatting sanity checks
```bash
git status --short
```
**Expected:** modified `HandEvaluator/HandEvaluator.csproj`,
`HandEvaluator.Test/HandEvaluator.Test.csproj`, `README.md`; new `LICENSE`
(and optional `COPYING.LESSER`). `HandEvaluator/Documentation/HandEvaluator.xml`
must remain tracked and unchanged (it may be regenerated by the build — if so, verify
`git diff` is empty or only whitespace).

### 5.5 Confirm the XML is still present and used
```bash
ls HandEvaluator/Documentation/HandEvaluator.xml
```
The `<DocumentationFile>` property must still be present in
`HandEvaluator/HandEvaluator.csproj`.

---

## 6. Worker Checklist

- [ ] **B1** Confirm repo root and `git status --short`; branch `master`.
- [ ] **B2** `HandEvaluator/HandEvaluator.csproj`: `net5.0` → `net10.0`; keep
      `DocumentationFile` and all other properties.
- [ ] **B3** `HandEvaluator.Test/HandEvaluator.Test.csproj`: `net5.0` → `net10.0`.
- [ ] **B4** Bump test packages (Test.Sdk 17.12+, xunit 2.9.2, runner 2.8.2,
      coverlet 6.0.2, Shouldly 4.2.1 or leave 4.0.3).
- [ ] **B5** `dotnet restore` + `dotnet build -c Release` → succeeds, 0 errors.
- [ ] **B6** Fix any compile/analyzer/obsolete issues revealed by net10 (no logic
      changes unless unavoidable; document any that are).
- [ ] **B7** `dotnet test -c Release` → all tests pass.
- [ ] **B8** Extract API content from `HandEvaluator/Documentation/HandEvaluator.xml`
      (474 members, 2 top-level classes + 2 nested enums; drop all `<exclude/>`).
- [ ] **B9** Rewrite `README.md` per §4.4 (description, install, quick start, API
      reference grouped by type, license).
- [ ] **B10** Add `LICENSE` with full LGPL-3.0 text; optional `COPYING.LESSER`.
- [ ] **B11** Confirm `README.md` has a License section naming LGPL-3.0 (not Apache).
- [ ] **B12** Confirm `HandEvaluator/Documentation/HandEvaluator.xml` still exists and
      is unchanged.
- [ ] **B13** Re-run `dotnet build -c Release` and `dotnet test -c Release`.
- [ ] **B14** `git status --short`; confirm only intended files changed.
- [ ] **B15** Write `_planning/{YYMMDD-HHmm}_*.md` completion report (request, what was
      done, decisions/follow-ups).

---

## 7. Risks & Follow-ups

1. **License discrepancy (Apache vs LGPL):** User first wrote Apache-2.0, then
   confirmed LGPL-3.0. Work is LGPL-3.0. The completion report must repeat this.
2. **`-only` vs `-or-later`:** Plan uses `LGPL-3.0-only`. If the user meant
   "or later", the SPDX id and README line must change to `LGPL-3.0-or-later`.
3. **LGPL is a GPL overlay:** Strictly, LGPL-3.0 requires the GPL-3.0 text as well.
   Consider adding `COPYING` (GPL-3.0) and/or a `COPYING.LESSER` note. Follow-up.
4. **NuGet metadata:** No `PackageId`/`Version`/`Authors`/`PackageLicenseExpression`
   exists in `HandEvaluator.csproj`. If the project is packed to NuGet, add
   `<PackageLicenseExpression>LGPL-3.0-only</PackageLicenseExpression>` and other
   metadata. Otherwise skip; note as follow-up.
5. **README/API drift:** README docs are a mirror and will drift from the XML. The
   generated-from note plus keeping the XML mitigates this. Consider a future script
   to regenerate the README section.
6. **Test package availability:** Exact recommended versions may not exist in the
   configured feed. Worker should let `dotnet restore` drive and bump to nearest newer
   stable. Do not downgrade below versions that support net10.
7. **Warnings-as-errors:** No `TreatWarningsAsErrors` is set and `.editorconfig`
   suppresses CS1591. If the net10 SDK emits new analyzer warnings, they are warnings,
   not errors, but fix any that are clearly caused by the migration. Keep the build
   clean if cheap.
8. **XML regeneration:** Building with `DocumentationFile` set may regenerate
   `HandEvaluator.xml`. If the regenerated output differs (e.g. formatting), verify the
   diff is benign before committing; do not delete it.
9. **`.sln` VS header:** Left at `Visual Studio Version 16` (cosmetic). Optionally
   update if VS/`dotnet` complains; not expected.
10. **`.opencode/` is untracked:** Do not add or commit it as part of this work unless
    the user asks.

---

## 8. Completion Report (Planner)

**Request:** Produce an executable, step-by-step plan for the @worker to (a) migrate
both projects to .NET 10 with up-to-date test packages, (b) move the generated XML API
documentation content into `README.md` while keeping the XML for NuGet packaging, and
(c) add an LGPL-3.0 license.

**What was done:** Researched the repo (projects, package versions, XML doc structure –
474 `<member>` entries across 5 types, 71 `<exclude/>` entries, 12 `<example>` blocks,
`.editorconfig` warning suppressions, `.sln`, `.gitignore`), confirmed the installed
SDKs (8.0.424, 10.0.400, 10.0.401) and the clean git state, and wrote this plan to
`_planning/260930-1631_net10-migration-readme-api-docs-lgpl-license.md`.

**Decisions/follow-ups:** Recorded LGPL-3.0 as the license (overriding the initial
Apache-2.0 wording); chose `net10.0`; kept the XML `DocumentationFile`; specified
concrete test-package bumps and XML→Markdown conversion rules; listed open follow-ups
(`-only` vs `-or-later`, GPL overlay files, NuGet license metadata, README drift).
