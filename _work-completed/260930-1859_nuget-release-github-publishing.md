# NuGet Release / GitHub Publishing — Phases 0 & 1 + CI Workflow

- **Timestamp:** 260930-1859
- **Branch:** `net10-uplift` (working tree was clean before starting; nothing committed or pushed)
- **Source of truth:** `_planning/260930-1835_nuget-release-github-publishing.md`
- **Scope:** Implement plan Phases 0 and 1 locally, add the Phase 2 CI workflow, add the
  Phase 3 `publish.yml` (no live credentials). No publishing, tagging, committing, or
  pushing was performed.

---

## 1. Original prompt / request

> Implement the plan at `_planning/260930-1835_nuget-release-github-publishing.md` for the
> HandEvaluator repo (`C:\ck\HandEvaluator`, current branch `net10-uplift`). … Implement
> Phases 0 and 1 locally plus the CI workflow (Phase 2 files). Apply the packaging metadata
> to `HandEvaluator/HandEvaluator.csproj`, add `IsPackable=false` to the SpeedTest project,
> add `global.json`, add `.github/workflows/ci.yml`, validate restore/build/test/pack, verify
> package contents, update documentation (`README.md` + `RELEASING.md`). Publishing/tagging/
> pushing are explicitly out of scope. Write a completion report using relative paths.

---

## 2. What was done

### Files changed / added

| Path | Change | Why |
| --- | --- | --- |
| `HandEvaluator/HandEvaluator.csproj` | Replaced with full NuGet packaging metadata | Plan §5.1 — identity, license/links, readme, symbols/SourceLink, deterministic build, MinVer |
| `HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj` | Added `<IsPackable>false</IsPackable>` | Plan §5.2 — it is a console app and must not be packed |
| `global.json` (new) | Pins SDK `10.0.400`, `rollForward: latestFeature` | Plan §5.4 — reproducible CI SDK selection |
| `COPYING` (new) | Canonical GNU GPLv3 text | Plan D4 — LGPL-3.0 references GPLv3; needed so `COPYING.LESSER`'s statement is satisfied |
| `.github/workflows/ci.yml` (new) | Restore → Release build (warnings-as-errors + CI build) → Fast tests → pack → artifact upload | Plan §9.1 |
| `.github/workflows/publish.yml` (new) | Tag `v*` → build → Fast tests → pack → push nupkg/snupkg → GitHub Release | Plan §9.2; OIDC trusted publishing, **no secrets embedded** |
| `README.md` | Added NuGet/CI badges; updated install snippet to `Ck.HandEvaluator`; updated the "published to NuGet" sentence; corrected the API-reference provenance line | Plan §12 |
| `RELEASING.md` (new) | Tag convention, MinVer behavior, CI/publish flow, GitHub Environment + NuGet auth setup, local dry-run | Plan §12 |
| `.gitignore` | Ignore `artifacts/` and `.local-feed/` | Keep pack output/validation feed out of the working tree |

### Library csproj metadata applied (plan §5.1)

- Identity: `PackageId=Ck.HandEvaluator`, `Authors`/`Company=captainkout`,
  `Product=HandEvaluator`, `Copyright=Copyright (c) captainkout`, full `Description`,
  `PackageTags`.
- License/links: `PackageLicenseExpression=LGPL-3.0-only`, `PackageProjectUrl`,
  `RepositoryUrl`, `RepositoryType=git`.
- `PackageReadmeFile=README.md` + `<None Include="..\README.md" ...>`.
- Symbols/source: `PublishRepositoryUrl`, `EmbedUntrackedSources`, `IncludeSymbols`,
  `SymbolPackageFormat=snupkg`.
- Repro build: `Deterministic=true`, `IsPackable=true`.
- Pack items: `README.md`, `LICENSE`, `COPYING`, `COPYING.LESSER` at package root.

---

## 3. Decisions made and recorded

### D1 — Package ID: `Ck.HandEvaluator` (confirmed)

`dotnet package search HandEvaluator` returns the package owned by `codevision`
(v1.0.1). `dotnet package search Ck.HandEvaluator` returns **“No results found.”**, so
`Ck.HandEvaluator` is free and was chosen exactly as the plan recommends. Assembly name
(`HandEvaluator`) and root namespace (`HoldemHand`) are unchanged. This is still subject
to the maintainer's final sign-off, but it is available and the README/RELEASING docs
name it.

### Package versions used (latest stable at implementation time)

- `MinVer` **8.0.0** (plan suggested 6.0.0; 8.0.0 is the latest stable — `8.1.0-alpha.1`
  is prerelease and was avoided).
- `Microsoft.SourceLink.GitHub` **10.0.401** (plan suggested 8.0.0; 10.0.401 is the
  latest stable and matches the installed SDK feature band).

### D5/Icon — `PackageIcon` removed (no asset)

The repo contains no icon image and no `images/` directory. Rather than add a
placeholder, `<PackageIcon>` and the icon `<None>` item were **omitted**, as permitted
by the plan/Step 0.4. The nupkg therefore has no `icon.png`; this is intentional. To add
one later: place a 128×128 PNG at `images/icon.png`, then restore
`<PackageIcon>icon.png</PackageIcon>` and `<None Include="..\images\icon.png" ...>`.

### COPYING — added

`COPYING.LESSER` exists and explicitly states that the GPLv3 text (conventionally
`COPYING`) should accompany the work. The canonical GPLv3 text was fetched from
`https://www.gnu.org/licenses/gpl-3.0.txt` and committed as `COPYING`; both
`COPYING` and `COPYING.LESSER` are packed at the package root.

### `TreatWarningsAsErrors` — kept (`true`)

`dotnet build HandEvaluator.sln -c Release -p:TreatWarningsAsErrors=true
-p:ContinuousIntegrationBuild=true` completed with **0 Warning(s), 0 Error(s)**, so the
flag is retained in both workflows (no suppression needed).

### `DocumentationFile` — deviation from the literal snippet (documented)

Plan §5.1 suggested `<DocumentationFile>$(AssemblyName).xml</DocumentationFile>`. Tested
literally, that path resolves **relative to the project directory** and writes
`HandEvaluator/HandEvaluator.xml` into the source tree (untracked pollution). It was
therefore implemented as `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
only, which lets the SDK default `DocumentationFile` to
`$(OutputPath)$(AssemblyName).xml` (`obj/Release/net10.0/HandEvaluator.xml`) and packs it
to `lib/net10.0/HandEvaluator.xml` — the actual goal of the change.

**Related finding (plan’s premise was inaccurate):** the *original*
`Documentation/$(AssemblyName).xml` path **also** packs to `lib/net10.0/HandEvaluator.xml`
(NuGet flattens the file name), so the change was not strictly required to hit
`lib/net10.0/`. A side effect is that the tracked file
`HandEvaluator/Documentation/HandEvaluator.xml` is no longer regenerated by builds and
becomes stale. See “Open decisions / follow-ups”.

### Default branch — recommendation only (not acted on)

Recommend merging `net10-uplift` → `master` and tagging the release commit on `master`
(plan D9/§11). No merge was performed. Until then, a tagged build reports the branch as
`net10-uplift` in the nuspec `repository` element (observed).

---

## 4. Commands run and results

```bash
dotnet package search HandEvaluator --source https://api.nuget.org/v3/index.json
# => HandEvaluator 1.0.1, owner codevision  (ID is taken)

dotnet package search Ck.HandEvaluator --source https://api.nuget.org/v3/index.json
# => "No results found."  (ID is available)

dotnet restore HandEvaluator.sln
# => restore succeeded

dotnet build HandEvaluator.sln -c Release
# => Build succeeded. 0 Warning(s), 0 Error(s)

dotnet test HandEvaluator.Test/HandEvaluator.Test.csproj -c Release --no-build --filter "Category=Fast"
# => Passed! Failed: 0, Passed: 204, Skipped: 0, Total: 204  (~60–120 ms)
#    (`HandEvaluator.Test/Traits.cs` defines Category = Fast/Slow/Oracle/Perf; trait gate is real.)

dotnet build HandEvaluator.sln -c Release -p:TreatWarningsAsErrors=true -p:ContinuousIntegrationBuild=true
# => Build succeeded. 0 Warning(s), 0 Error(s)   (flag kept)

dotnet pack HandEvaluator/HandEvaluator.csproj -c Release -o artifacts -p:ContinuousIntegrationBuild=true
# => Ck.HandEvaluator.0.0.0-alpha.0.5.nupkg
#    Ck.HandEvaluator.0.0.0-alpha.0.5.snupkg
#    (version is 0.0.0-alpha.0.5 because no v* tag exists — expected per MinVer)

dotnet msbuild HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj -getProperty:IsPackable
# => false

dotnet --version
# => 10.0.401   (global.json pins 10.0.400 + rollForward: latestFeature)
```

### Package contents verified (via PowerShell `System.IO.Compression.ZipFile`)

`.nupkg` (`artifacts/Ck.HandEvaluator.0.0.0-alpha.0.5.nupkg`):

```
Ck.HandEvaluator.nuspec
COPYING
COPYING.LESSER
LICENSE
README.md
lib/net10.0/HandEvaluator.dll
lib/net10.0/HandEvaluator.xml
_rels/.rels
[Content_Types].xml
package/services/metadata/core-properties/nuget.psmdcp
```

`.snupkg` (`artifacts/Ck.HandEvaluator.0.0.0-alpha.0.5.snupkg`):

```
Ck.HandEvaluator.nuspec
lib/net10.0/HandEvaluator.pdb
_rels/.rels
[Content_Types].xml
package/services/metadata/core-properties/nuget.psmdcp
```

Nuspec metadata confirmed: `id=Ck.HandEvaluator`, `license=LGPL-3.0-only`,
`projectUrl=https://github.com/captainkout/HandEvaluator`,
`repository type=git url=... commit=...`, `readme=README.md`,
`dependencies` empty (no runtime deps), `authors=captainkout`.

PDB SourceLink check: the string
`raw.githubusercontent.com/captainkout/HandEvaluator` was found inside
`lib/net10.0/HandEvaluator.pdb`, confirming SourceLink mapping.

### Local feed + install smoke test (plan Step 1.3 / §10.6)

```bash
dotnet nuget push artifacts/Ck.HandEvaluator.0.0.0-alpha.0.5.nupkg --source C:\ck\HandEvaluator\.local-feed
# => Your package was pushed.

# throwaway console app in C:\Users\ck\AppData\Local\Temp\opencode\he-smoke
dotnet add ... package Ck.HandEvaluator --prerelease --source C:\ck\HandEvaluator\.local-feed
# => Installed Ck.HandEvaluator 0.0.0-alpha.0.5
dotnet run ...
# => h1: Two pair, King's and Queen's with a Ace for a kicker
#    h2: Two pair, Queen's and Three's with a King for a kicker
#    h1 > h2: True
```

The temporary `.local-feed` directory and smoke-test project were deleted after
validation; `artifacts/` was regenerated by the final pack (and is now gitignored).

---

## 5. Open decisions / follow-ups

1. **Package ID sign-off.** `Ck.HandEvaluator` is confirmed free and is used throughout;
   the maintainer should still confirm the exact ID before the first publish.
2. **Icon.** `PackageIcon` was intentionally omitted (no asset present). Add
   `images/icon.png` (128×128) and re-enable the property/items when ready.
3. **Orphaned tracked docs XML.** `HandEvaluator/Documentation/HandEvaluator.xml` is a
   tracked, build-generated file referenced by the old README text. With the SDK-default
   `DocumentationFile`, builds no longer refresh it. Options: (a) delete it from tracking
   and rely on the packed `lib/net10.0/HandEvaluator.xml` (README already updated to say
   so), or (b) revert to `Documentation/$(AssemblyName).xml` if the repo wants to keep
   committing generated docs. Not changed here beyond the README wording.
4. **Branch strategy.** Recommend merging `net10-uplift` → `master` before the first
   release and tagging `master`. Not performed.
5. **First-release sequence.** Per plan D9, publish `v1.0.0-alpha.1` first, validate on
   NuGet.org, then `v1.0.0`.

---

## 6. Remaining human steps to publish (Phase 3 — not done)

1. Review/merge these changes (`net10-uplift` → `master` recommended), then let CI run
   green on GitHub.
2. Create the GitHub Environment **`nuget-release`** with required reviewers.
3. Configure NuGet auth — either a NuGet.org **Trusted Publishing policy** (owner
   `captainkout`, repo `captainkout/HandEvaluator`, workflow `publish.yml`, environment
   `nuget-release`) or store a scoped `NUGET_API_KEY` secret and switch `publish.yml` to
   Option B.
4. Push annotated tags to trigger the release workflow:
   `git tag -a v1.0.0-alpha.1 -m "1.0.0-alpha.1" && git push origin v1.0.0-alpha.1`
   then `v1.0.0` for stable.
5. `.github/workflows/publish.yml` is already present with no live secrets; only the
   environment/auth configuration above is required for it to succeed.

---

## 7. Verification summary

- `dotnet restore` — success.
- `dotnet build -c Release` — success, **0 warnings / 0 errors**.
- `dotnet build … -p:TreatWarningsAsErrors=true -p:ContinuousIntegrationBuild=true` —
  success, **0 warnings / 0 errors**.
- `dotnet test --filter "Category=Fast"` — **204/204 passed**.
- `dotnet pack` — produces `.nupkg` + `.snupkg` with `HandEvaluator.dll`,
  `HandEvaluator.xml`, README/LICENSE/COPYING/COPYING.LESSER, correct nuspec, and the PDB
  + SourceLink in the snupkg.
- Local-feed install + `Hand` invocation smoke test — passed.
