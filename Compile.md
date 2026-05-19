# Building Warewolf locally with `Compile.ps1`

`Compile.ps1` (at the repo root) is the same script the Azure DevOps pipeline
invokes to produce every output Warewolf ships or tests against. It wraps
`dotnet restore` + `dotnet publish` for a curated set of `.sln`/`.csproj`
files, then post-processes the output (runtime patching, native DLL pinning,
test-host bootstrapping, resource copying). This doc is a quick-start; the
script itself has the authoritative parameter list.

## Prerequisites

- Windows 10/11 or Windows Server 2019/2022 — PowerShell 5.1 or 7+.
- .NET 8 SDK on `PATH` (provides `dotnet`).
- Visual Studio 2022 (Enterprise / Professional / Community / BuildTools) —
  only really needed for the few targets that still use legacy MSBuild
  features; `dotnet publish` does the heavy lifting.
- `nuget.exe` somewhere — on `PATH`, at `C:\Windows\nuget.exe`, or passed via
  `-NuGet`. The script auto-downloads it to `C:\Windows\nuget.exe` if it can
  write there.
- Git on `PATH` (only required when using `-AutoVersion` / `-CustomVersion`).

The script can auto-discover MSBuild via `vswhere`, then by walking the
Enterprise → Professional → Community → BuildTools edition list, then by
checking `$env:MSBuildPath`, then via registry. If everything fails it
**prompts interactively** — pass `-MSBuildPath` explicitly to avoid that.

## What it builds

Each solution / project maps to a fixed output folder under `Bin\`:

| Switch | Solution | Output folder |
| --- | --- | --- |
| `-AcceptanceTesting` | `Dev\AcceptanceTesting.sln` | `Bin\AcceptanceTesting\` |
| `-UITesting` | `Dev\UITesting.sln` | `Bin\UITesting\` |
| `-Server` | `Dev\Server.sln` | `Bin\Server\` |
| `-Studio` | `Dev\Studio.sln` | `Bin\Studio\` |
| `-Release` | `Dev\Release.sln` | `Bin\Release\` |
| `-Web` | `Dev\Web.sln` | `Bin\Web\` |
| `-NewServerNet6` | `Dev\NewServerNet6.sln` | `Bin\NewServerNet6\` |
| `-ServerTests` | `Dev\ServerTests.sln` | `Bin\ServerTests\` |
| `-StudioProject` | `Dev\Dev2.Studio\Dev2.Studio.csproj` | `Bin\StudioProject\` |
| `-COMIPCProject` | `Dev\Warewolf.COMIPC\Warewolf.COMIPC.csproj` | `Bin\COMIPCProject\` |

**Passing no solution switch builds every solution above.** Pick one (or
more) switches to scope the build.

## Common scenarios

### Build the test outputs (what `TestRun.ps1` consumes)

```powershell
.\Compile.ps1 -ServerTests
```

Produces `Bin\ServerTests\` populated with every `Warewolf.*`/`Dev2.*` test
DLL plus `Microsoft.TestPlatform` and `TestRun.ps1`. This is what catalog
mode of `TestRun.ps1` auto-runs when `Bin\ServerTests\` is empty.

### Build the server only

```powershell
.\Compile.ps1 -Server
```

### Build everything (server + studio + tests + web)

```powershell
.\Compile.ps1
```

No switch = full build. Slow; usually only useful before a release or after
large cross-cutting changes.

### Release build with auto-version

```powershell
.\Compile.ps1 -Release -Config Release -AutoVersion -Target Rebuild
```

`-AutoVersion` writes `Dev\AssemblyCommonInfo.cs` + `.fs` with a freshly
generated version derived from the most recent four-part git tag (build
number incremented, uniqueness verified against `gitlab.com/warewolf/warewolf`
and `github.com/Warewolf-ESB/Warewolf`).

Use `-CustomVersion "1.2.3.4"` to pin the version string instead of letting
the script derive it.

### Cross-compile for Linux

```powershell
.\Compile.ps1 -ServerTests -Runtime linux-x64
```

`linux-x64` publishes **self-contained** (embeds the runtime); `win-x64`
publishes **framework-dependent**. Defaults: `win-x64` on Windows hosts,
`linux-x64` on Linux/macOS.

If you publish the server self-contained for Linux but want to run the EXE on
Windows for testing, the script auto-patches
`Bin\<out>\Warewolf Server.runtimeconfig.json` to framework-dependent mode
after publish.

### Build the Studio MSBuild project (not the solution)

```powershell
.\Compile.ps1 -StudioProject
```

Useful when you only want `Bin\StudioProject\` without dragging in the rest
of the solution graph.

### Regenerate SpecFlow `.feature.cs` files

```powershell
.\Compile.ps1 -RegenerateSpecFlowFeatureFiles
```

Restores `AcceptanceTesting.sln`, runs `specflow.exe generateAll` on every
`Dev\*Specs` and `Dev\Warewolf.UIBindingTests.*` project, then fixes up a
`DeploymentItem` quoting quirk in the generated files. Run this after
editing `.feature` files if your IDE doesn't regenerate code-behinds.

### Project-specific (non-flat) outputs

```powershell
.\Compile.ps1 -ServerTests -ProjectSpecificOutputs
```

`-ProjectSpecificOutputs` keeps each project's output under its own
`bin\<Config>\` folder instead of consolidating into `Bin\<OutputFolderName>\`.
The pipeline uses this for the Release build.

## Parameter cheatsheet

```
-MSBuildPath <path>      # explicit MSBuild.exe (defaults to VS2022 Enterprise; auto-discovered)
-NuGet <path>            # explicit nuget.exe   (defaults to PATH or %windir%\nuget.exe)
-Config <Debug|Release>  # default Debug
-Target <msbuild-target> # forwarded as /t:<target> (e.g. Rebuild, Clean)
-Runtime <rid>           # linux-x64 (self-contained) or win-x64 (framework-dependent)
                         # default: win-x64 on Windows, linux-x64 on Linux/macOS
-AutoVersion             # derive next version from git tags, write AssemblyCommonInfo files
-CustomVersion <v>       # pin version explicitly (skip git derivation)
-GitCredential <u:t>     # user:token for pushing version tags (release builds)
-ProjectSpecificOutputs  # don't consolidate output into Bin\<solution>\
-Disablemaxcpucount      # reserved; currently no-op in the publish path
-RegenerateSpecFlowFeatureFiles   # run specflow generateAll across all *Specs projects

# Solution selectors (any combination; none = build all)
-AcceptanceTesting -UITesting -Server -Studio -Release -Web
-NewServerNet6 -ServerTests -StudioProject -COMIPCProject
```

## Where output goes

- `Bin\<OutputFolderName>\` — flat publish output (the default). Contains the
  binaries, native interop DLLs, copied `Resources - *` folders, and
  `TestRun.ps1`.
- `Bin\<OutputFolderName>\runtimes\` — RID-specific native assets
  (`Microsoft.Data.SqlClient`, `SQLite.Interop.dll`, etc.). The script
  duplicates the right one into the flat root so probing finds it regardless
  of host.
- `Bin\<OutputFolderName>\testhost.dll.config` — auto-generated for test
  outputs (Acceptance / ServerTests). Contains binding redirects + the secure
  settings block tests expect.
- `Dev\AssemblyCommonInfo.cs` / `.fs` — version files; rewritten only when
  `-AutoVersion` or `-CustomVersion` is set.

## Post-publish steps the script does for you

- Patches `Warewolf Server.runtimeconfig.json` to framework-dependent mode
  when a self-contained Linux publish needs to run on Windows.
- Copies `runtimes\unix\lib\net8.0\Microsoft.Data.SqlClient.dll` to the flat
  root for Linux publishes (so last-writer-wins doesn't shadow the unix
  implementation with the reference stub).
- Copies `runtimes\win-x64\native\SQLite.Interop.dll` to the flat root for
  Windows publishes.
- Copies `Resources - Release`, `Resources - ServerTests`, `Resources - UITests`,
  and `Resources - Load` into the output.
- Drops `TestRun.ps1` next to the binaries so the artifact is self-sufficient.
- For test outputs only: installs `Microsoft.TestPlatform` via NuGet (both a
  pinned 17.2.0 copy and a latest copy) and copies the MSTest TestAdapter
  DLLs from `%USERPROFILE%\.nuget\packages\mstest.testadapter\2.1.2\`.

## Troubleshooting

- **`NuGet not found`** — install `nuget.exe` somewhere on `PATH` or pass
  `-NuGet <path>`. The auto-download to `C:\Windows\nuget.exe` only works
  with admin rights.
- **`MSBuild not found` / interactive prompt** — pass `-MSBuildPath` with the
  full path to your VS 2022 `MSBuild.exe`. Setting `MSBuildPath` as a machine
  environment variable also works (the script reads it).
- **`dotnet publish failed for <sln>`** — script exits 1. Re-run with
  `-Target Rebuild` to force a clean build, or inspect the `dotnet`
  output above the failure (it's logged at `-v minimal`).
- **Wrong native DLL at runtime** (`Microsoft.Data.SqlClient` / SQLite) —
  `Bin\<out>\` is flat; the last project to publish wins. The script pins
  the correct RID-specific DLL for the chosen `-Runtime`, but if you mix
  RIDs in one folder you'll see the symptom. Wipe `Bin\<out>\` and re-publish.
- **`-AutoVersion` errors with "No local tags found in git history"** —
  the script needs at least one four-part tag (`X.Y.Z.B`) reachable from
  `HEAD`. Fetch tags (`git fetch --tags`) or pass `-CustomVersion` instead.
- **Stale assembly versions across `Bin\` outputs** — the `Compile_Release`
  pipeline job enforces a single version across every `Dev2.*` / `Warewolf.*`
  DLL. If your local build mixes versions (e.g. you re-ran `Compile.ps1`
  with a different `-AutoVersion` mid-flight), delete `Bin\` and rebuild.

## Where to look in the script

- Parameter block + known-solutions list: top of `Compile.ps1` (lines 1–37).
- NuGet / MSBuild auto-discovery: lines ~46–115.
- Versioning (auto + custom) + AssemblyCommonInfo writers: lines ~117–288.
- SpecFlow regeneration: lines ~290–308.
- Publish loop + runtime resolution: lines ~310–360.
- Self-contained Linux runtimeconfig patch: lines ~357–378.
- Native DLL pinning (SqlClient, SQLite): lines ~379–407.
- Test-output post-processing (TestPlatform, MSTest adapters,
  `testhost.dll.config`): lines ~413–500.
