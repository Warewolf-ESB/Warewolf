# Local Full-Coverage Runs

How to reproduce the full Azure DevOps test-coverage build on a workstation. The
single entry point is `TestRun.ps1` at the repo root — it runs both in **direct
mode** (what each pipeline step calls) and **catalog mode** (parses
`Dev/.azure/pipeline.yml` and dispatches every job locally with merge + report).

## TL;DR

```powershell
# Full local coverage run, all jobs, Windows-native dependencies (closest to CI):
./TestRun.ps1

# When it finishes:
#   coverage/merged/all_merged.cobertura.xml   <- merged Cobertura XML
#   coverage/report/index.html                  <- HTML report (auto-opens)
```

That's it. The rest of this document is detail.

---

## Prerequisites

| Tool                | Why                                                      | Install                                      |
| ------------------- | -------------------------------------------------------- | -------------------------------------------- |
| .NET 8 SDK          | Build + run tests                                        | https://dotnet.microsoft.com/download        |
| `dotnet-coverage`   | Instrumentation + merge                                  | auto-installed by `TestRun.ps1` if missing   |
| `reportgenerator`   | HTML/Cobertura/Markdown report                           | auto-installed if missing                    |
| MSBuild 2022        | Used by `Compile.ps1` on Windows                         | Visual Studio 2022 (Build Tools is fine)     |
| Docker Desktop      | Required only for `-Runtime Linux` or sidecar containers | https://docs.docker.com/desktop/             |
| Python 3            | Optional — strips test/3rd-party noise from merged XML   | any 3.x                                      |

`TestRun.ps1` will auto-install `dotnet-coverage` and `reportgenerator` as
global tools on first run.

## Quickstart — full coverage in one command

```powershell
./TestRun.ps1
```

What this does:

1. Auto-detects empty `Bin/ServerTests` and runs `Compile.ps1 -ServerTests
   -Runtime win-x64` if needed.
2. Parses every `- job:` block from `Dev/.azure/pipeline.yml` into a catalog.
3. Dispatches each job sequentially (Windows runtime forces sequential when any
   EngineSpec, sidecar or host-dep job is present — port 7071 / FTP / UNC
   collisions otherwise).
4. Writes per-job Cobertura XML under `coverage/<job-slug>/`.
5. Merges everything into `coverage/merged/all_merged.cobertura.xml` via
   `dotnet-coverage merge`.
6. Filters test/3rd-party noise using `Dev/.azure/filter_coverage.py` (if
   Python is available).
7. Runs `reportgenerator` to produce `coverage/report/index.html` and opens it.

Expect 1.5–3 hours wall-clock end-to-end on a typical dev box. Use partial runs
(below) for faster iteration.

## Modes

### Catalog mode (the full-coverage path)

Triggered when no `-Projects`/`-Assemblies` is passed, or when `-Jobs ...` is
given. Parses `pipeline.yml` and runs jobs through one of three internal
runners:

- `Invoke-WindowsBareMetalJob` — Pattern A (default). Runs vstest on the host;
  spins up Windows-native deps (pyftpdlib, OpenSSH, MSSQL, Samba, Exchange) via
  the `Start-Host*Server` helpers; runs Docker sidecars only for services with
  no native equivalent. Closest to CI.
- `Invoke-LinuxUnitJob` — runs the unit-test DLL inside `warewolf-coverage-env`
  with `dotnet-coverage` instrumentation.
- `Invoke-LinuxEngineSpecJob` — spawns a per-job docker network
  (`ww-cov-{slug}-{runid}`) plus a sleep-keepalive test container, attaches
  sidecars with `--network=container:<test-container>`, runs the lightweight
  engine + specs.

### Direct mode (what CI calls)

Triggered by passing `-Projects` or `-Assemblies`. Identical surface to what
the Azure DevOps tasks invoke — use it to reproduce a single job exactly:

```powershell
./TestRun.ps1 `
  -Projects "Dev2.*.Tests","Warewolf.*.Tests" `
  -Filter "TestCategory!=CannotParallelize&TestCategory!=Multithread" `
  -StartElasticsearchServer `
  -Coverage
```

## The runtime knob (`-Runtime`)

| Value     | SUT location           | Deps                            | Parallel-safe? |
| --------- | ---------------------- | ------------------------------- | -------------- |
| `Windows` | bare-metal host        | Native (pyftpdlib, OpenSSH, …)  | No — forces sequential when any EngineSpec/sidecar/host-dep job is selected |
| `Linux`   | container per job      | Docker sidecars on per-job net  | Yes — default `-MaxParallel 4` |

Default is `Windows`. Pick `Linux` for parallel runs at the cost of slightly
different test environment vs. CI. `-SUTRuntime` is a deprecated alias.

## Common flags (catalog mode)

```
-Jobs <names...>     # default = All jobs in pipeline.yml
-List                # print catalog and exit (no tests run)
-IncludeDisabled     # include jobs currently commented out in pipeline.yml
-SkipBuild           # skip Compile.ps1, reuse existing Bin/ServerTests
-SkipReport          # skip reportgenerator (merge still happens)
-NoParallel          # force sequential
-MaxParallel N       # parallel cap (Linux runtime only)
-ReportFormat X      # Html (default), Cobertura, Badges, TextSummary,
                     # HtmlSummary, MarkdownSummary
-CoverageDir <dir>   # default: ./coverage
-PipelineYml <path>  # default: Dev/.azure/pipeline.yml
```

## Recipes

### List every job the catalog knows about

```powershell
./TestRun.ps1 -List
```

### Run a single job (reproduce one CI step locally)

```powershell
./TestRun.ps1 -Jobs Unit_Tests
./TestRun.ps1 -Jobs Copy_Tool_Specs_From_FTP
./TestRun.ps1 -Jobs Workflow_Execution_Specs,Subworkflow_Execution_Specs
```

### Fast iteration — skip rebuild and report

```powershell
./TestRun.ps1 -Jobs Unit_Tests -SkipBuild -SkipReport
```

### Parallel run (Linux runtime, 6 jobs at a time)

```powershell
./TestRun.ps1 -Runtime Linux -MaxParallel 6
```

### Only the unit-test buckets (no EngineSpec)

```powershell
./TestRun.ps1 -Jobs Unit_Tests,Activities_Tests,LightweightExecutionUnitTests, `
  Multithread_Unit_Tests,Infrastructure_Unit_Tests,Storage_Unit_Tests, `
  Auditing_Unit_Tests,Plugin_Handler_Unit_Tests,Gather_System_Information_Unit_Tests
```

### Merge an existing per-job set into a fresh report (no re-test)

```powershell
# Files already live under ./coverage/*/*.cobertura.xml — just re-merge:
$xmls = Get-ChildItem ./coverage -Recurse -Filter *.cobertura.xml |
  Where-Object { $_.FullName -notmatch '\\merged\\' -and $_.Name -notlike 'parts_*.cobertura.xml' }
dotnet-coverage merge @($xmls.FullName) `
  --output ./coverage/merged/all_merged.cobertura.xml `
  --output-format cobertura --nologo
reportgenerator -reports:./coverage/merged/all_merged.cobertura.xml `
  -targetdir:./coverage/report -reporttypes:Html
```

## Outputs

```
coverage/
  <job-slug>/                          per-job raw output
    <job-slug>.cobertura.xml           Cobertura (merged for that job)
    parts_*.cobertura.xml              raw vstest snapshots (excluded from merge)
  merged/
    all_merged.cobertura.xml           single combined Cobertura
  report/
    index.html                         HTML report (auto-opens locally)
    Summary*.txt / *.md                summary files (if Format includes them)
```

The `report/index.html` page is where you eyeball overall coverage. The
`merged/all_merged.cobertura.xml` is the machine-readable source for hotspot
analysis (see below).

## Finding hotspots

A hotspot here means *biggest uncovered surface*: many lines, low coverage.
Quick PowerShell pass over the merged XML:

```powershell
[xml]$cov = Get-Content ./coverage/merged/all_merged.cobertura.xml
$cov.coverage.packages.package | ForEach-Object {
  $pkg = $_.name
  $_.classes.class | ForEach-Object {
    $lines = $_.lines.line.Count
    $hit   = ($_.lines.line | Where-Object { [int]$_.hits -gt 0 }).Count
    if ($lines -ge 50) {
      [PSCustomObject]@{
        Package    = $pkg
        Class      = $_.name
        Lines      = $lines
        Uncovered  = $lines - $hit
        Coverage   = if ($lines) { [math]::Round($hit / $lines, 3) } else { 1 }
      }
    }
  }
} | Sort-Object Uncovered -Descending | Select-Object -First 30 | Format-Table
```

This ranks classes with ≥50 lines by absolute uncovered LOC. Use it to pick
targets for new tests.

## Troubleshooting

| Symptom                                                  | Cause / fix                                                                                   |
| -------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `Bin/ServerTests is empty and -SkipBuild was specified`  | Drop `-SkipBuild` so `Compile.ps1` runs once.                                                 |
| `Bare-metal Windows … forcing sequential`                | Expected — switch to `-Runtime Linux` for parallel or live with sequential.                   |
| `Docker is not running`                                  | Start Docker Desktop or pick `-Runtime Windows`.                                              |
| Port 7071 already in use                                 | A previous EngineSpec run leaked. `Get-Process func* | Stop-Process -Force` and rerun.        |
| Sidecar container name clash                             | Lingering containers from a prior crash. `docker ps -a --filter name=ww-cov- | docker rm -f`. |
| `reportgenerator` not found                              | Re-run `TestRun.ps1`; it `dotnet tool install --global dotnet-reportgenerator-globaltool`.    |
| Merged file shows no coverage for assembly X             | Filter in `coverage-settings.xml` excluded it, or X has no symbols. Check `Bin/ServerTests`.  |
| Python missing → noise in merged XML                     | Optional cleanup; report still works. Install Python 3 if you want filtered output.           |

## Related files

- `TestRun.ps1` — entry point (catalog + direct mode in one script).
- `Compile.ps1` — invoked automatically when `Bin/ServerTests` is empty.
- `Dev/.azure/pipeline.yml` — source of truth for the catalog.
- `Dev/.azure/filter_coverage.py` — strips test/3rd-party packages from the
  merged XML before reporting.
- `coverage-settings.xml` — `dotnet-coverage` include/exclude rules.
