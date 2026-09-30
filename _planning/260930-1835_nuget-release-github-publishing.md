# Plan: Prepare HandEvaluator for NuGet Release via GitHub (GitHub Actions + NuGet.org)

- **Timestamp:** 260930-1835
- **Target repo:** `C:\ck\HandEvaluator` (current branch `net10-uplift`; default `master`)
- **Planning agent:** plan only — no source modified, nothing committed, no packages published.
- **Worker:** every action below is for `@worker` to execute. Snippets are ready to paste but are
  **not applied** by this plan.

---

## 1. Original request (verbatim)

> Create a detailed, worker-ready plan to prepare the HandEvaluator repository for release to
> NuGet, published via GitHub (GitHub Actions + NuGet.org). This is a PLANNING task only — do NOT
> modify source code, do NOT commit. Write the plan to
> `_planning/{YYMMDD-HHmm}_nuget-release-github-publishing.md` following the repo convention (see
> existing files in `_planning/`). Present relative file paths in the plan.
>
> **Repository context (already gathered — verify as needed)** … _(full context supplied by the
> requester; summarized in §3 below and re-verified against the working tree)._

---

## 2. Summary of what will be done

1. Add NuGet packaging metadata + SourceLink + tag-driven versioning to
   `HandEvaluator/HandEvaluator.csproj`, and mark `HandEvaluator.SpeedTest` as non-packable.
2. Adopt **tag-driven versioning (MinVer)** so a `v*` git tag produces both the package version and
   the GitHub Release name.
3. Add two GitHub Actions workflows: continuous build/test/pack (`ci.yml`) and tag-triggered
   publish (`publish.yml`).
4. Establish symbols/source story: `.snupkg` + SourceLink + `EmbedUntrackedSources`.
5. Define GitHub repository setup (branch strategy, tags, environments, secrets/trusted publishing,
   branch protection, release notes).
6. Define pre-flight local validation (dry-run pack, inspect `.nupkg`, prerelease then stable).
7. Update documentation (`README.md` install snippet + badges; add `RELEASING.md`).
8. Execute in phases: **Phase 0 scaffolding → Phase 1 local pack → Phase 2 CI → Phase 3 publish**.

**Blocking discovery (read §3 first):** the `HandEvaluator` package ID is **already owned on
NuGet.org by a different user (`codevision`)** at version `1.0.1` (2017). Publishing under the
plain `HandEvaluator` ID will fail. This plan therefore recommends a **new, owner-scoped PackageId**
(`Ck.HandEvaluator`) while keeping `AssemblyName=HandEvaluator` and
`RootNamespace=HoldemHand`. This is the single most important decision and must be confirmed in
**Phase 0**.

---

## 3. Key facts verified

| Item                   | Value / evidence                                                                                                                                                                 |
| ---------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SDK installed locally  | `10.0.401` (also `10.0.400`, `8.0.424`) — `dotnet --list-sdks`                                                                                                                   |
| Library TFM            | `net10.0`; `dotnet build`/`pack` supported by installed SDK                                                                                                                      |
| Solution               | `HandEvaluator.sln` — 3 projects (`HandEvaluator`, `HandEvaluator.Test`, `HandEvaluator.SpeedTest`)                                                                              |
| Library                | `HandEvaluator/HandEvaluator.csproj`: `AssemblyName=HandEvaluator`, `RootNamespace=HoldemHand`, `DocumentationFile=Documentation/$(AssemblyName).xml`; **no packaging metadata** |
| Test project           | `HandEvaluator.Test/HandEvaluator.Test.csproj`: xUnit `2.9.2`, Shouldly `4.2.1`, Microsoft.NET.Test.Sdk `17.12.0`, coverlet.collector `6.0.2`, `IsPackable=false`                |
| SpeedTest project      | `HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj`: `OutputType=Exe`, net10.0, references library, **`IsPackable` not set (would pack by default)**                        |
| Test traits            | `HandEvaluator.Test/Traits.cs` defines `Category` = `Fast`/`Slow`/`Oracle`/`Perf`; default gate is `--filter "Category=Fast"`                                                    |
| License                | `LICENSE` = GNU Lesser GPL **v3**; `COPYING.LESSER` ends with `SPDX-License-Identifier: LGPL-3.0-only` → use `LGPL-3.0-only`                                                     |
| README                 | `README.md` (~1,618 lines / ~52 KB); currently says "published to NuGet" and uses `dotnet add package HandEvaluator`                                                             |
| Tooling                | `dotnet-tools.json` pins `csharpier 1.3.0`; `.editorconfig` present                                                                                                              |
| Remote / branches      | `origin = https://github.com/captainkout/HandEvaluator.git`; `master` (default), `net10-uplift`; **no tags**; working tree clean                                                 |
| Existing NuGet package | **`HandEvaluator` 1.0.1 exists**, owner `codevision` (Andrey Kurdyumov), last updated 2017-06-02, targets `netstandard1.0`/`net20`. Not owned by `captainkout`.                  |
| Image assets           | **None** in the repo (`git ls-files` shows no `.png/.svg/.ico/.jpg`). A `PackageIcon` requires adding one, or omit `PackageIcon`.                                                |
| Planning convention    | `_planning/YYMMDD-HHmm_<kebab-case-slug>.md`                                                                                                                                     |

> **Note on `net10.0`:** the local SDK is stable (10.0.400/401) and NuGet.org already computes
> `net10.0` compatibility for the existing package, so `net10.0` is publishable. GitHub-hosted
> runners should be pinned via `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'` rather than
> relying on the image default. Optionally add `global.json` (Phase 0) to pin the SDK exactly.

---

## 4. Decisions made

| #   | Decision                                                                                                                                           | Rationale / alternatives                                                                                                                                                                                                                                                                                               |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D1  | **Use a new PackageId: `Ck.HandEvaluator`** (confirm exact string with maintainer). Keep `AssemblyName=HandEvaluator`, `RootNamespace=HoldemHand`. | The plain `HandEvaluator` ID is taken by `codevision`; NuGet IDs are first-come, permanent, and cannot be assumed. Alternatives: contact the existing owner for a transfer (slow, unlikely), or `HoldemHand.HandEvaluator`. **Fallback if transfer succeeds:** set `PackageId` to `HandEvaluator` and bump to `1.0.2`. |
| D2  | **Tag-driven versioning with MinVer** (default tag prefix `v`).                                                                                    | One source of truth: the git tag. Alternatives: Nerdbank.GitVersioning (more config/weight), manual `VersionPrefix` (error-prone). MinVer needs full git history in CI (`fetch-depth: 0`).                                                                                                                             |
| D3  | **Symbols via `.snupkg` + SourceLink (GitHub)**, plus `EmbedUntrackedSources=true` and `PublishRepositoryUrl=true`.                                | Gives consumers "step into source" links on GitHub. Embedded PDBs would bloat the main package; `.snupkg` is the NuGet-recommended route.                                                                                                                                                                              |
| D4  | **`PackageLicenseExpression=LGPL-3.0-only`** (not `PackageLicenseFile`).                                                                           | Verified by the `SPDX-License-Identifier` line in `COPYING.LESSER` and the LGPL v3 text in `LICENSE`. Recommendation: also add `COPYING` (GPL v3 text) at repo root because LGPL-3.0 incorporates GPL-3.0 by reference (see `COPYING.LESSER` text).                                                                    |
| D5  | **Two workflows:** `ci.yml` (push/PR) and `publish.yml` (tags `v*`).                                                                               | Separation of concerns; publish is gated by tag + GitHub Environment approvals.                                                                                                                                                                                                                                        |
| D6  | **Publishing auth: NuGet Trusted Publishing (OIDC) recommended; `NUGET_API_KEY` secret as fallback.**                                              | Trusted publishing removes a long-lived secret. If unavailable, use a scoped, short-lived API key stored as `NUGET_API_KEY`.                                                                                                                                                                                           |
| D7  | **`HandEvaluator.SpeedTest` set to `IsPackable=false`**; library explicitly `IsPackable=true`.                                                     | Prevents accidental publish of the console app / test project.                                                                                                                                                                                                                                                         |
| D8  | **Tests gate publishing** (`Category=Fast` must pass before pack/push).                                                                            | Do not publish untested bits. `Slow`/`Oracle`/`Perf` stay out of the release gate (opt-in/nightly).                                                                                                                                                                                                                    |
| D9  | **First release is a prerelease (`1.0.0-alpha.1`), then stable (`1.0.0`).**                                                                        | Proves the pipeline end-to-end without burning the stable version.                                                                                                                                                                                                                                                     |

---

## 5. Exact project-file changes

> `@worker`: apply these edits in Phase 0. Do **not** run/publish in this phase.

### 5.1 `HandEvaluator/HandEvaluator.csproj` (library — the packaged project)

Replace the file contents with the following (additions are the packaging metadata, items, and
package references). Keep the existing `AssemblyName`/`RootNamespace` and the documentation file.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>HandEvaluator</AssemblyName>
    <RootNamespace>HoldemHand</RootNamespace>

    <!-- Documentation: emit XML docs beside the assembly so NuGet packs them. -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <DocumentationFile>$(AssemblyName).xml</DocumentationFile>

    <!-- Identity / discovery -->
    <PackageId>Ck.HandEvaluator</PackageId>
    <Authors>captainkout</Authors>
    <Company>captainkout</Company>
    <Product>HandEvaluator</Product>
    <Copyright>Copyright (c) captainkout</Copyright>
    <Description>Fast Texas Holdem hand evaluation and analysis. Port of the poker.eval / Keith Rule hand evaluator; exposes Hand.Evaluate, hand odds, outs, opponents' odds, hand iterators, and 169 pocket-hand helpers.</Description>
    <PackageTags>poker;holdem;texas-holdem;hand-evaluator;cards;equity;odds;ranking</PackageTags>

    <!-- License / links -->
    <PackageLicenseExpression>LGPL-3.0-only</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/captainkout/HandEvaluator</PackageProjectUrl>
    <RepositoryUrl>https://github.com/captainkout/HandEvaluator.git</RepositoryUrl>
    <RepositoryType>git</RepositoryType>

    <!-- Readme + icon embedded in the package -->
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>

    <!-- Symbols / source -->
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>

    <!-- Reproducible builds -->
    <Deterministic>true</Deterministic>
    <!-- CI sets -p:ContinuousIntegrationBuild=true; local builds stay false. -->

    <IsPackable>true</IsPackable>
  </PropertyGroup>

  <!-- Files placed at the package root -->
  <ItemGroup>
    <None Include="..\README.md" Pack="true" PackagePath="\" />
    <None Include="..\images\icon.png" Pack="true" PackagePath="\" />
    <None Include="..\LICENSE" Pack="true" PackagePath="\" />
    <None Include="..\COPYING.LESSER" Pack="true" PackagePath="\" />
  </ItemGroup>

  <!-- Tag-driven versioning + source link -->
  <ItemGroup>
    <PackageReference Include="MinVer" Version="6.0.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

Notes / caveats the worker must handle:

- **`DocumentationFile`:** the original value was `Documentation/$(AssemblyName).xml`, which would
  place the XML under a `Documentation/` subfolder inside `lib/net10.0/`. The snippet above changes
  it to the default `$(AssemblyName).xml` so NuGet packs `lib/net10.0/HandEvaluator.xml` next to the
  DLL. **Verify in Phase 1** (see §8) and revert if maintainers prefer the old path (then add an
  explicit pack item mapping it into `lib/net10.0/`).
- **`icon.png`:** if the maintainer does not want to add an icon, **remove `<PackageIcon>` and the
  icon `<None>` item** — a missing `PackageIcon` file makes `dotnet pack` fail. If adding one, use a
  128×128 PNG at `images/icon.png`. See Phase 0 Step 0.4.
- **Package versions:** `MinVer` and `Microsoft.SourceLink.GitHub` versions are the latest-known
  stable at planning time. `@worker` should check for a newer stable at implementation time; do not
  use prerelease versions.
- **`COPYING` (GPL v3):** `COPYING.LESSER` states the GPL v3 text should accompany the work. If
  `COPYING` does not exist, either add it (from
  `https://www.gnu.org/licenses/gpl-3.0.txt`) and pack it, or drop the `COPYING.LESSER` item.

### 5.2 `HandEvaluator.SpeedTest/HandEvaluator.SpeedTest.csproj`

Add `<IsPackable>false</IsPackable>` to the existing `PropertyGroup`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>HandEvaluator.SpeedTest</RootNamespace>
    <AssemblyName>HandEvaluator.SpeedTest</AssemblyName>
    <Nullable>disable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\HandEvaluator\HandEvaluator.csproj" />
  </ItemGroup>
</Project>
```

### 5.3 `HandEvaluator.Test/HandEvaluator.Test.csproj`

No change required (`IsPackable=false` already present). Optional: add
`<TreatWarningsAsErrors>` handling only if the CI flags warnings.

### 5.4 `global.json` (new, repo root)

Pins the SDK and enables `setup-dotnet` caching. Create `global.json`:

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

> `rollForward: latestFeature` lets a machine with `10.0.401` succeed while keeping CI on the 10.0
> feature band. If maintainers do not want an SDK pin, skip this file and remove `cache: true` from
> the workflows.

---

## 6. Versioning approach (D2)

**Chosen: MinVer (tag-driven).**

- Tags are the source of truth. `MinVer` derives the package version from the nearest reachable
  tag (default prefix `v`):
  - Tag `v1.0.0` → package version `1.0.0`.
  - Tag `v1.0.0-alpha.1` → package version `1.0.0-alpha.1`.
  - No tag reachable → `0.0.0-alpha.0.N` (prevents accidental "stable" publishes).
- MinVer injects `Version`, `AssemblyVersion`, `FileVersion`, and `InformationalVersion` at build
  time — no need to hardcode `<Version>` in the `.csproj`.
- **CI requirement:** `actions/checkout` must use `fetch-depth: 0` and fetch tags, otherwise MinVer
  sees no tags and emits `0.0.0-*`.
- The `publish.yml` workflow is triggered by the same `v*` tag; the tag name doubles as the GitHub
  Release name.

**Alternatives (documented, not chosen):**

- _Nerdbank.GitVersioning_ — more control (heights, public releases) but more configuration.
- _Manual `VersionPrefix`_ — add `<VersionPrefix>1.0.0</VersionPrefix>` and push with
  `dotnet pack -p:Version=${{ github.ref_name }}`; simplest but easy to drift from a tag.

**Tag convention to adopt:** annotated SemVer tags `vMAJOR.MINOR.PATCH[-prerelease]`, e.g.
`v1.0.0-alpha.1`, `v1.0.0`.

---

## 7. Symbols & source (D3)

- Produce a symbol package `Ck.HandEvaluator.<version>.snupkg` (via
  `IncludeSymbols=true` + `SymbolPackageFormat=snupkg`).
- Add `Microsoft.SourceLink.GitHub` (private asset) so the PDB maps to exact GitHub commits;
  `PublishRepositoryUrl=true` writes the repo URL into the nuspec.
- `EmbedUntrackedSources=true` embeds any generated sources (e.g., assembly info) so SourceLink
  links never break.
- Prefer **`.snupkg` over embedded PDB**: smaller main package, NuGet.org-hosted symbols, and
  debugger source-link support. Embedded PDB is an acceptable fallback if `snupkg` upload is
  blocked.

---

## 8. Phased execution order (for @worker)

### Phase 0 — Scaffolding (no publishing)

**Step 0.1 — Confirm the PackageId decision (blocking).**
Run and record output:

```bash
dotnet package search HandEvaluator --source https://api.nuget.org/v3/index.json
```

Confirm `HandEvaluator` is taken by `codevision`. Get maintainer sign-off on
`Ck.HandEvaluator` (or chosen alternative). **Do not proceed until confirmed.**

**Step 0.2 — Decide default branch / merge strategy.**
Decide whether `net10-uplift` is merged into `master` (recommended: merge, then release from
`master`) or whether releasing from `net10-uplift` is acceptable. This affects branch protection and
tag placement (tags must point at the release commit).

**Step 0.3 — Apply project changes.**
Apply §5.1, §5.2, and (optionally) §5.4.

**Step 0.4 — Add the icon (or drop `PackageIcon`).**
If using an icon, add `images/icon.png` (128×128 PNG, transparent background). Otherwise remove
`<PackageIcon>` + the icon `<None>` item from §5.1.

**Step 0.5 — Restore & build to prove metadata is valid.**

```bash
dotnet restore HandEvaluator.sln
dotnet build HandEvaluator.sln -c Release
```

Expected: `0 Error(s)`. Fix any `MinVer`/SourceLink/`PackageIcon` errors before Phase 1.

**Step 0.6 — Decide on `TreatWarningsAsErrors`.**
Run `dotnet build HandEvaluator.sln -c Release -p:TreatWarningsAsErrors=true`. If existing warnings
fail the build, either fix them or drop the flag from the workflows (and note it). Do not silently
suppress.

### Phase 1 — Pack & validate locally

**Step 1.1 — Dry-run pack.**

```bash
dotnet pack HandEvaluator/HandEvaluator.csproj -c Release -o artifacts
```

**Step 1.2 — Inspect the `.nupkg` and `.snupkg`.** See §10 for the exact checks.

**Step 1.3 — Push to a local folder feed** (validates nuspec + symbols without touching NuGet.org).

```bash
mkdir .local-feed
dotnet nuget push "artifacts/*.nupkg" --source .local-feed
dotnet nuget push "artifacts/*.snupkg" --source .local-feed
```

**Step 1.4 — Definition-of-done check for Phase 1** (§12) before moving on.

### Phase 2 — CI workflow

**Step 2.1 — Add `.github/workflows/ci.yml`** (§9.1).
**Step 2.2 — Push the branch; confirm the workflow runs green** (restore → build → Fast tests →
pack → artifact upload).
**Step 2.3 — Run a throwaway pack as a CI artifact** and spot-check it (same checks as §10).

### Phase 3 — Publish

**Step 3.1 — Configure NuGet auth** (§11): either trusted publishing or `NUGET_API_KEY`.
**Step 3.2 — Create the GitHub Environment `nuget-release`** with required reviewers.
**Step 3.3 — Add `.github/workflows/publish.yml`** (§9.2).
**Step 3.4 — Cut the first prerelease:** tag `v1.0.0-alpha.1` on the release commit; push the tag.
Watch the workflow; confirm the package appears (marked prerelease) on NuGet.org.
**Step 3.5 — Cut the stable release:** tag `v1.0.0`; push; confirm stable package + GitHub Release.
**Step 3.6 — Post-release:** update `README.md` badges/install line, create release notes.

---

## 9. Workflows (ready-to-paste, applied by @worker)

### 9.1 `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
    branches: [master, net10-uplift]
  pull_request:
    branches: [master, net10-uplift]

env:
  DOTNET_NOLOGO: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true

jobs:
  build-test-pack:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0 # MinVer needs full history + tags

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"
          cache: true # requires global.json

      - name: Restore
        run: dotnet restore HandEvaluator.sln

      - name: Build (Release)
        run: >
          dotnet build HandEvaluator.sln -c Release --no-restore
          -p:TreatWarningsAsErrors=true
          -p:ContinuousIntegrationBuild=true

      - name: Test (Fast)
        run: >
          dotnet test HandEvaluator.Test/HandEvaluator.Test.csproj -c Release --no-build
          --filter "Category=Fast"
          --logger "trx;LogFileName=test-results.trx"
          --collect:"XPlat Code Coverage"

      - name: Pack
        run: >
          dotnet pack HandEvaluator/HandEvaluator.csproj -c Release --no-build
          -o artifacts -p:ContinuousIntegrationBuild=true

      - name: Upload package artifacts
        uses: actions/upload-artifact@v4
        with:
          name: nupkg
          path: artifacts/*

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: "**/*.trx"
```

Notes:

- `--no-build` in `dotnet test` is valid only because the solution was built in the previous step.
- `Category=Fast` matches `Traits.Fast` in `HandEvaluator.Test/Traits.cs`. `Slow`/`Oracle`/`Perf`
  are intentionally excluded; optionally add a separate scheduled `workflow_dispatch`/nightly job
  running the full suite.
- If `TreatWarningsAsErrors` is dropped per Step 0.6, remove that line.
- Coverage requires `coverlet.collector` (already referenced in the test project).

### 9.2 `.github/workflows/publish.yml`

```yaml
name: Publish to NuGet

on:
  push:
    tags:
      - "v*"

permissions:
  contents: read
  id-token: write # required for NuGet Trusted Publishing (OIDC)

jobs:
  publish:
    runs-on: ubuntu-latest
    environment: nuget-release # add required reviewers / protection rules
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"
          cache: true

      - name: Restore
        run: dotnet restore HandEvaluator.sln

      - name: Build (Release)
        run: >
          dotnet build HandEvaluator/HandEvaluator.csproj -c Release --no-restore
          -p:TreatWarningsAsErrors=true
          -p:ContinuousIntegrationBuild=true

      - name: Test (Fast) — release gate
        run: >
          dotnet test HandEvaluator.Test/HandEvaluator.Test.csproj -c Release --no-build
          --filter "Category=Fast"

      - name: Pack
        run: >
          dotnet pack HandEvaluator/HandEvaluator.csproj -c Release --no-build
          -o artifacts -p:ContinuousIntegrationBuild=true

      # --- Option A: NuGet Trusted Publishing (recommended, no long-lived secret) ---
      - name: NuGet login
        id: login
        uses: NuGet/login@v1
        with:
          user: captainkout # NuGet.org username (owner)

      # --- Option B (fallback): use a stored API key instead of the two lines above ---
      # env:
      #   NUGET_API_KEY: ${{ secrets.NUGET_API_KEY }}

      - name: Push package
        run: >
          dotnet nuget push "artifacts/*.nupkg"
          --api-key ${{ steps.login.outputs.NUGET_API_KEY }}
          --source https://api.nuget.org/v3/index.json
          --skip-duplicate

      - name: Push symbols
        run: >
          dotnet nuget push "artifacts/*.snupkg"
          --api-key ${{ steps.login.outputs.NUGET_API_KEY }}
          --source https://api.nuget.org/v3/index.json
          --skip-duplicate

      - name: Create GitHub Release
        env:
          GH_TOKEN: ${{ github.token }}
        run: >
          gh release create "${GITHUB_REF_NAME}"
          artifacts/*.nupkg artifacts/*.snupkg
          --title "${GITHUB_REF_NAME}"
          --generate-notes
```

`@worker` notes:

- With **Option B**, replace `${{ steps.login.outputs.NUGET_API_KEY }}` with
  `${{ secrets.NUGET_API_KEY }}` in both push steps and drop the `NuGet/login` step and the
  `id-token: write` permission.
- `--skip-duplicate` makes re-runs idempotent (a version can never be overwritten on NuGet.org).
- Trusted publishing requires a matching **Trusted Publishing policy** configured on nuget.org
  (owner, repo `captainkout/HandEvaluator`, workflow `publish.yml`, environment `nuget-release`).

---

## 10. Pre-flight validation (Phase 1 & CI spot-checks)

1. **List package contents.** Confirm required entries exist:
   - `lib/net10.0/HandEvaluator.dll`
   - `lib/net10.0/HandEvaluator.xml` (XML docs)
   - `README.md` at package root
   - `icon.png` at package root (if `PackageIcon` kept)
   - `HandEvaluator.nuspec`
   - dependencies: **none** for the library (it has no `PackageReference` runtime deps)

   ```bash
   # bash/macOS/Linux
   unzip -l artifacts/*.nupkg
   unzip -l artifacts/*.snupkg
   ```

   On Windows without `unzip`:

   ```powershell
   Add-Type -AssemblyName System.IO.Compression.FileSystem
   [IO.Compression.ZipFile]::OpenRead((Get-ChildItem artifacts/*.nupkg).FullName).Entries.FullName
   ```

2. **Verify the nuspec metadata** (inside the package or from the build):

   ```bash
   dotnet msbuild HandEvaluator/HandEvaluator.csproj -t:GetPackageContents -getProperty:PackageId
   ```

   Confirm `PackageId=Ck.HandEvaluator`, correct `Authors`, `Description`,
   `PackageLicenseExpression=LGPL-3.0-only`, `RepositoryUrl`, and the version derived from the tag.

3. **Symbol package check.** The `.snupkg` must contain `lib/net10.0/HandEvaluator.pdb` and a
   `.nuspec`. SourceLink links resolve to `https://raw.githubusercontent.com/captainkout/HandEvaluator/...`.

4. **Signature verification.** `dotnet nuget verify` only validates **signed** packages; this package
   is unsigned, so it is not applicable unless code signing is added later. Note this explicitly.

5. **SemVer sanity.** Versions must be valid SemVer2; prerelease tags (`v1.0.0-alpha.1`) must be
   recognized as prerelease by NuGet (they are, due to the `-`).

6. **Local-feed install smoke test.** Point a throwaway project at `.local-feed`, run
   `dotnet add package Ck.HandEvaluator --prerelease`, and confirm restore succeeds and
   `Hand.Evaluate` is callable.

7. **Release sequence:** `v1.0.0-alpha.1` (prerelease) → validate on NuGet.org → `v1.0.0` (stable).

---

## 11. GitHub + NuGet.org setup checklist

- [ ] **Package ID:** confirm `Ck.HandEvaluator` is free
      (`dotnet package search Ck.HandEvaluator`); the first successful push reserves it.
- [ ] **Merge decision:** merge `net10-uplift` → `master` (recommended) and tag the release commit
      on the default branch; otherwise tag `net10-uplift` and document why.
- [ ] **Tags:** create annotated tags `v1.0.0-alpha.1`, then `v1.0.0`; push with
      `git push origin <tag>`.
- [ ] **Branch protection:** protect `master` (require PR + status checks: `build-test-pack`) and
      optionally `net10-uplift`. Prevent tag deletion.
- [ ] **GitHub Environment** `nuget-release`: add required reviewers, restrict to `master`/tags.
- [ ] **Auth:**
  - _Trusted publishing:_ on nuget.org add a Trusted Publishing policy for owner `captainkout`,
    repository `captainkout/HandEvaluator`, workflow file `publish.yml`, environment
    `nuget-release`.
  - _Or API key:_ create a scoped NuGet.org API key (push new packages: `CaptainKout.*`) and store
    it as the repository secret `NUGET_API_KEY`.
- [ ] **Repository metadata:** set the GitHub repo description/homepage to match
      `https://github.com/captainkout/HandEvaluator`; ensure `RepositoryUrl` in the csproj matches.
- [ ] **Release notes:** rely on `gh release create --generate-notes`; optionally add
      `release-drafter/release-drafter` for curated changelogs.
- [ ] **NuGet.org profile:** set package owner display / repository link once published.

---

## 12. Documentation updates

- [ ] `README.md`:
  - Update the install snippet from `dotnet add package HandEvaluator` to the new PackageId, e.g.
    `dotnet add package Ck.HandEvaluator`.
  - Add badges at the top (after the H1):
    ```markdown
    [![NuGet](https://img.shields.io/nuget/v/Ck.HandEvaluator.svg)](https://www.nuget.org/packages/Ck.HandEvaluator)
    [![NuGet downloads](https://img.shields.io/nuget/dt/Ck.HandEvaluator.svg)](https://www.nuget.org/packages/Ck.HandEvaluator)
    [![CI](https://github.com/captainkout/HandEvaluator/actions/workflows/ci.yml/badge.svg)](https://github.com/captainkout/HandEvaluator/actions/workflows/ci.yml)
    ```
  - Fix the sentence "then published to NuGet" to name the new package ID.
- [ ] Add `RELEASING.md` (new, repo root): tag convention (`vMAJOR.MINOR.PATCH[-pre]`), how
      MinVer derives versions, the `v*` push flow, trusted publishing/secret setup, environment
      approvals, and a local dry-run (`dotnet pack`).
- [ ] Optionally reference `RELEASING.md` from `README.md`'s contributing section.

---

## 13. Risks / open questions

1. **Package ID collision (HIGH).** `HandEvaluator` 1.0.1 is owned by `codevision`. Cannot publish
   under that ID without a transfer. Mitigation: new `Ck.HandEvaluator` ID; confirm with
   maintainer in Phase 0. _Open question:_ exact desired ID, and whether to pursue a transfer.
2. **`net10.0` on CI runners (MEDIUM).** Local SDK is stable `10.0.401`, but hosted runner images
   may lag. Mitigation: `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'` + `global.json`.
   _Open question:_ pin exact SDK or floating patch?
3. **License expression correctness (MEDIUM).** Verified `LGPL-3.0-only` from
   `COPYING.LESSER`'s SPDX line. LGPL-3.0 incorporates GPL-3.0, so the missing `COPYING` (GPL v3)
   text file should be added and packed. _Open question:_ does the maintainer want
   `PackageLicenseExpression` or an embedded `PackageLicenseFile`?
4. **README size/assets (LOW/MEDIUM).** `README.md` is ~52 KB; NuGet renders it, but images (none
   currently) must be absolute URLs when rendered. Ensure `PackageReadmeFile` path is exactly
   `README.md`.
5. **No icon asset (MEDIUM).** A `PackageIcon` without a file breaks `dotnet pack`. Either add
   `images/icon.png` or remove `PackageIcon`.
6. **`DocumentationFile` path (MEDIUM).** Custom `Documentation/...` path may place the XML in the
   wrong package folder. Verify in Phase 1; the snippet moves it to the default output path.
7. **Warnings-as-errors (LOW).** May fail CI if the codebase has pre-existing warnings. Decide in
   Step 0.6.
8. **Tests gating publishing (LOW).** Fast tests are the gate; `Slow`/`Oracle`/`Perf` are not run at
   release. Consider a nightly scheduled full-suite job.
9. **`net10-uplift` vs `master` (MEDIUM).** Releasing from a feature branch can confuse consumers
   and branch protection. Recommend merging to `master` first.
10. **MinVer history requirement (MEDIUM).** Forgetting `fetch-depth: 0` yields `0.0.0-alpha…`
    versions. Bake it into both workflows.
11. **API key leakage (LOW, unless Option B).** Prefer trusted publishing; if using a secret, scope
    the key and rotate it.
12. **Existing 1.0.1 users (LOW).** Consumers of the old `codevision` package will not auto-upgrade
    to the new ID; document the new package in the README/release notes.

---

## 14. Definition of done

**Phase 0**

- [ ] Package ID decision confirmed and recorded.
- [ ] `net10-uplift` merged to `master` (or explicit decision to release from the branch).
- [ ] Packaging metadata applied to `HandEvaluator/HandEvaluator.csproj`; `IsPackable=false` on
      `HandEvaluator.SpeedTest`.
- [ ] Icon added (or `PackageIcon` removed); `COPYING` license text added or pack item dropped.
- [ ] `dotnet build HandEvaluator.sln -c Release` → `0 Error(s)`.

**Phase 1**

- [ ] `dotnet pack` produces both `.nupkg` and `.snupkg`.
- [ ] `.nupkg` contains `lib/net10.0/HandEvaluator.dll`, `lib/net10.0/HandEvaluator.xml`,
      `README.md`, `icon.png` (if used), correct nuspec metadata (PackageId, license, repo URL).
- [ ] `.snupkg` contains the PDB.
- [ ] Local-feed install smoke test passes.

**Phase 2**

- [ ] `.github/workflows/ci.yml` runs green on push/PR (restore → build → Fast tests → pack →
      artifacts).
- [ ] CI artifact inspected; matches Phase 1 expectations.

**Phase 3**

- [ ] `.github/workflows/publish.yml` exists; `nuget-release` environment protected.
- [ ] `v1.0.0-alpha.1` published as prerelease on NuGet.org; symbols available.
- [ ] `v1.0.0` published as stable; GitHub Release created from the tag.
- [ ] `README.md` install snippet + badges updated; `RELEASING.md` added.

**Global**

- [ ] No `Slow`/`Oracle`/`Perf` tests in the release gate.
- [ ] `RepositoryUrl` in the package matches `captainkout/HandEvaluator`.

---

## 15. Verification commands (copy/paste)

```bash
# Identity / availability
dotnet package search HandEvaluator --source https://api.nuget.org/v3/index.json
dotnet package search Ck.HandEvaluator --source https://api.nuget.org/v3/index.json

# Build + test the release gate
dotnet restore HandEvaluator.sln
dotnet build HandEvaluator.sln -c Release -p:TreatWarningsAsErrors=true -p:ContinuousIntegrationBuild=true
dotnet test HandEvaluator.Test/HandEvaluator.Test.csproj -c Release --no-build --filter "Category=Fast"

# Pack (tag-driven version comes from git tags; no tag => 0.0.0-alpha.0.N)
git tag v1.0.0-alpha.1
dotnet pack HandEvaluator/HandEvaluator.csproj -c Release -o artifacts
unzip -l artifacts/*.nupkg
unzip -l artifacts/*.snupkg

# Local feed validation
dotnet nuget push "artifacts/*.nupkg" --source .local-feed
dotnet nuget push "artifacts/*.snupkg" --source .local-feed

# Publish (Phase 3, CI does this on tag push)
git push origin v1.0.0-alpha.1
```
