# Releasing HandEvaluator

The library is published to NuGet.org as **`Ck.HandEvaluator`** (assembly and root
namespace stay `HandEvaluator` / `HoldemHand`) using **tag-driven versioning**
([MinVer](https://github.com/adamralph/minver)) and GitHub Actions.

> The plain `HandEvaluator` NuGet package ID is owned by a different author
> (`codevision`, version `1.0.1`, last updated 2017). It cannot be published to.
> All releases therefore use the owner-scoped ID `Ck.HandEvaluator`.

## Versioning: the tag is the source of truth

[MinVer](https://github.com/adamralph/minver) derives the package version from the
nearest reachable git tag (default prefix `v`). There is no `<Version>` in
`HandEvaluator/HandEvaluator.csproj` on purpose.

| Tag                          | Resulting package version |
| ---------------------------- | ------------------------- |
| `v1.0.0`                     | `1.0.0` (stable)          |
| `v1.0.0-alpha.1`             | `1.0.0-alpha.1`           |
| `v1.2.0` + unreleased commits| `1.2.0-alpha.0.N`         |
| no reachable tag             | `0.0.0-alpha.0.N`         |

Because MinVer needs the full history and all tags, **every workflow checkout must
use `fetch-depth: 0`**. A shallow clone silently produces `0.0.0-alpha…` versions.

Tag convention: annotated SemVer tags `vMAJOR.MINOR.PATCH[-prerelease]`, e.g.
`v1.0.0-alpha.1`, then `v1.0.0`.

## Local dry run

No tag is required to build; you only need a tag to observe the final version.

```bash
# restores + builds; XML docs are generated and packed to lib/net10.0/HandEvaluator.xml
dotnet restore HandEvaluator.sln
dotnet build HandEvaluator.sln -c Release -p:TreatWarningsAsErrors=true -p:ContinuousIntegrationBuild=true

# fast release-gate tests (Slow/Oracle/Perf are intentionally excluded)
dotnet test HandEvaluator.Test/HandEvaluator.Test.csproj -c Release --no-build --filter "Category=Fast"

# produce Ck.HandEvaluator.<version>.nupkg and .snupkg in artifacts/
dotnet pack HandEvaluator/HandEvaluator.csproj -c Release -o artifacts

# inspect the produced packages (Windows PowerShell)
# Add-Type -AssemblyName System.IO.Compression.FileSystem
# [IO.Compression.ZipFile]::OpenRead((Get-ChildItem artifacts/*.nupkg).FullName).Entries.FullName
```

Expected package contents:

- `lib/net10.0/HandEvaluator.dll`
- `lib/net10.0/HandEvaluator.xml` (XML docs)
- `README.md`, `LICENSE`, `COPYING`, `COPYING.LESSER` at the package root
- nuspec: `id=Ck.HandEvaluator`, `license=LGPL-3.0-only`, repository URL, no dependencies
- `.snupkg` contains `lib/net10.0/HandEvaluator.pdb` (SourceLink-enabled)

## Continuous integration

`.github/workflows/ci.yml` runs on pushes and pull requests to `master` and
`net10-uplift`: restore → Release build with `TreatWarningsAsErrors=true` and
`ContinuousIntegrationBuild=true` → `dotnet test --filter "Category=Fast"` → pack →
upload `artifacts/*` and test results.

## Publishing a release (human-approved)

1. **Merge `net10-uplift` → `master`** (recommended) and release from `master`.
   Tagging a feature branch is possible but discouraged.
2. **Ensure CI is green** on the release commit.
3. **Create and push an annotated tag.** Start with a prerelease to exercise the
   pipeline end-to-end:
   ```bash
   git tag -a v1.0.0-alpha.1 -m "1.0.0-alpha.1"
   git push origin v1.0.0-alpha.1
   ```
   Then, once validated on NuGet.org, cut the stable release:
   ```bash
   git tag -a v1.0.0 -m "1.0.0"
   git push origin v1.0.0
   ```
4. The `publish.yml` workflow (tag `v*`) re-runs restore → build → fast tests → pack,
   pushes `*.nupkg` and `*.snupkg` to NuGet.org, and creates a GitHub Release from
   the tag.

## Required GitHub / NuGet.org configuration

The publish workflow is gated by the GitHub Environment **`nuget-release`** with
required reviewers. Configure one of the following authentication methods:

- **Trusted Publishing (recommended, no long-lived secret).** On nuget.org add a
  Trusted Publishing policy for owner `captainkout`, repository
  `captainkout/HandEvaluator`, workflow file `publish.yml`, environment
  `nuget-release`. The workflow uses `NuGet/login@v1` and `id-token: write`.
- **API key fallback.** Create a scoped NuGet.org API key (push new packages under
  the `Ck.*` prefix) and store it as the repository secret `NUGET_API_KEY`. Replace
  the `NuGet/login` step usage with `${{ secrets.NUGET_API_KEY }}` and remove the
  `id-token: write` permission.

Other repository setup:

- Protect `master` (require PR + the `build-test-pack` status check); optionally
  protect `net10-uplift`; prevent tag deletion.
- Restrict the `nuget-release` environment to `master`/tags with required reviewers.
- Keep the GitHub repo URL (`https://github.com/captainkout/HandEvaluator`) in sync
  with `RepositoryUrl` in `HandEvaluator/HandEvaluator.csproj`.

## Notes

- Versions are immutable on NuGet.org; the push steps use `--skip-duplicate` so
  re-runs are idempotent.
- Packages are currently **unsigned** (no Authenticode/author signing), so
  `dotnet nuget verify` is not applicable.
- Consumers of the old `codevision` `HandEvaluator` 1.0.1 package will not
  auto-upgrade; the new ID must be adopted explicitly.
