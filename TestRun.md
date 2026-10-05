# Running tests locally with `TestRun.ps1`

`TestRun.ps1` (at the repo root) is the same script the Azure DevOps pipeline
invokes. Running it locally reproduces a CI job bit-for-bit. This doc is a
quick-start; the script itself has the authoritative parameter list.

## Prerequisites

- Windows 10/11 or Windows Server 2019/2022 — PowerShell 5.1 or 7+.
- .NET 8 SDK on `PATH` (provides `dotnet`, `dotnet-coverage`).
- Built test binaries in `Bin\ServerTests\` — produced by `Compile.ps1` (catalog
  mode auto-runs this when the output directory is empty).
- Docker Desktop **if** you use `-Runtime Linux`, `-InContainer`, or any
  sidecar that runs in a container (the default on most deps).
- Python 3 on `PATH` **if** you use Windows-native FTP/FTPS deps
  (`-StartFTPServer` / `-StartFTPSServer` with `-Runtime Windows`).

The script auto-installs `dotnet-coverage` and
`dotnet-reportgenerator-globaltool` as global tools on first run.

## Two modes, picked from the arguments

| Mode | How you trigger it | What it does |
| --- | --- | --- |
| **Direct** | Pass `-Projects` / `-Assemblies` / `-TestsToRun` / `-ExcludeProjects` / any `-Start*` dep flag / `-InContainer` | Runs `vstest.console` (or MTP) against the named assemblies — `dotnet test` on a Linux host. Manages SUT lifecycle. This is what `pipeline.yml` calls. |
| **Catalog** | Pass none of the above, or pass `-Jobs` / `-List` | Parses `Dev\.azure\pipeline.yml`, builds a job catalog, dispatches each job, merges coverage, generates a report. |

Direct mode is for "run these specific tests now." Catalog mode is for "do
what CI does, locally."

## Common scenarios

### Run one project (no coverage)

```powershell
cd Bin\ServerTests
&.\TestRun.ps1 -Projects "Dev2.Activities.Tests"
```

Output TRX lands in `.\TestResults\`.

### Run a subset by filter or category

```powershell
&.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Filter "FullyQualifiedName~CalculateActivity"
&.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Category "UnitTest"
&.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -ExcludeCategories "CannotParallelize","Integration"
```

`-TestsToRun "Name1,Name2"` runs a comma-separated list of exact test names —
useful when iterating on a small set of failures.

### Run with coverage

```powershell
&.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Coverage
```

Produces `TestResults\Cobertura.xml` after the run.

### Run a project in a Linux container

```powershell
&.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Runtime Linux
# or equivalently
&.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -InContainer
```

The first run builds the `warewolf-coverage-env` image (slow once, cached
after).

### Run natively on a Linux host

On a Linux host (every Test job in `pipeline.yml` runs on `ubuntu-latest`)
`-Runtime` defaults to `Linux`, and direct mode runs the
assemblies **natively with `dotnet test`** — same filter, settings, coverage
(`--collect "Code Coverage"`) and `-RetryCount` handling as vstest, no
container. `-Start*` deps run as Docker containers on the host ports the specs
expect (FTPS is native pyftpdlib, so it needs `python`/`python3` with `pip`,
and port 1010 needs `sysctl net.ipv4.ip_unprivileged_port_start=1000` when not
running as root — the pipeline's unpack step sets it). `-StartMSSQLServer ""`
runs `mssql/server:2022-latest` and provisions the same fixture as the Windows
path (`testUser` login, `Dev2TestingDB` from `dev2testingdb.bacpac` via the
`microsoft.sqlpackage` dotnet tool, plus the deterministic seed); `-StartFTPServer`
creates every `FOR*TESTING` folder the specs use. `-ServerType LightweightExecution`
works with the linux-x64 func CLI (`-FuncExePath .../func-cli/func`); put
`~/.dotnet/tools` on `PATH` so `dotnet-coverage` resolves.

### How `-RetryCount` retries

After each pass the TRX is merged and only the tests with a `Failed`, `Error`,
`Timeout` or `Aborted` outcome are retried. Skipped (`NotExecuted`) and
`Inconclusive` results are not, because a rerun can't change them; if nothing
else is left to retry, the loop stops. A retry runs only the assemblies that
hold those failures. It falls back to the full list when the TRX doesn't record
where a failed test lives. Every assembly costs about 30 s of testhost and
coverage start-up even when the filter matches nothing in it, so re-running all
38 made each retry about 19 minutes (CI build 30903). On the Docker path, a
retry keeps the running `sqlserver` container when it still holds the full
fixture, instead of re-creating it and re-importing the bacpac. The first pass
always starts a fresh container.

`-CreateUNCPath`, `-UseRegionalSettings` and `-CreateLocalSchedulerAdmin` are
Windows-only: on Linux they log a warning and are skipped (UNC rows in the
File/Folder specs report Inconclusive). Pass paths with `/` — native tools
(`dotnet test`, `dotnet-coverage`, `func`) treat `\` as a filename character.

```bash
cd Bin/ServerTests
pwsh ./TestRun.ps1 -Projects "Warewolf.Tools.Specs" -Filter "TestCategory=CopyFileFromSFTP" `
  -Coverage:$false -StartSFTPServer -StartFTPServer -StartFTPSServer
```

Pass `-InContainer` to keep the old containerised runner instead.

### Run a job that needs a dep (FTP, SQL Server, etc.)

Pass the matching `-Start*` switch. On `-Runtime Windows` (default) most deps
run natively on the host; on `-Runtime Linux` they run as containers.

```powershell
# File-and-folder specs need FTP + UNC share
&.\TestRun.ps1 `
  -Projects "Warewolf.Tools.Specs" `
  -Filter "TestCategory=FileAndFolder" `
  -StartFTPServer -CreateUNCPath
```

Switches: `-StartFTPServer`, `-StartFTPSServer`, `-StartSFTPServer`,
`-StartSambaShare`, `-StartMySQLServer`, `-StartElasticsearchServer`,
`-StartRabbitMQServer`, `-StartRedisServer`, `-StartExchangeConnector`,
`-StartMSSQLServer ""`.

`-StartSFTPServer` stands up a local server on `localhost:2222` (`dev2` /
`Q/ulw&]`, same account every other local dep server uses, with a writable
`upload` folder) that the `sftp://localhost:2222/upload/...` rows in the
FileAndFolder `.feature` files connect to — same pattern as
`-StartFTPServer`'s `ftp://localhost:21/...`. On `-Runtime Windows` (the
hosted `windows-2022` default) it's a chroot-jailed Windows OpenSSH `sftp-server`
user; on `-Runtime Linux` it's an `atmoz/sftp` container.

After startup it runs an **SFTP probe** with the product's own client library
(`Renci.SshNet.dll` from the test output): log in as `dev2`, list `/`, write and
delete a file in `/upload`. The File tools swallow the underlying SSH error (a
broken server only ever shows up as `Recursive Directory Create Failed` or
`No such file` in the specs), so the probe's `SFTP probe failed ...` pipeline
error is where the real cause surfaces. On Windows, sshd also logs to
`%ProgramData%\ssh\logs\sshd.log` (`LogLevel VERBOSE`), which ships in the
job's `*_ServerLogs` artifact.

### Run a job under the lightweight execution engine

```powershell
&.\TestRun.ps1 `
  -Projects "Warewolf.Execution.Lightweight.Specs" `
  -ServerType LightweightExecution `
  -LightweightExecutionDir (Resolve-Path .).Path
```

`-ServerType FullServer` boots the full `Warewolf Server.exe` instead. The
script waits for port 7071 (lightweight) or 3142 (full) before running tests
and tears the engine down in a `finally` block.

### List every CI job available locally

```powershell
&.\TestRun.ps1 -List
&.\TestRun.ps1 -List -IncludeDisabled   # also show commented-out jobs
```

### Run one (or several) CI jobs locally

```powershell
&.\TestRun.ps1 -Jobs "Unit_Tests"
&.\TestRun.ps1 -Jobs "Unit_Tests","Server_Tests"
```

Coverage from all selected jobs is merged into `coverage\merged\` and an HTML
report is opened (unless `$env:TF_BUILD` is set).

### Run the full local equivalent of the CI pipeline

```powershell
&.\TestRun.ps1                          # all jobs, Windows runtime, Pattern A
&.\TestRun.ps1 -Runtime Linux           # all jobs in Linux containers, parallel
&.\TestRun.ps1 -Runtime Linux -MaxParallel 8
```

## The two runtimes (catalog mode)

| Knob | Pattern | SUT | Deps | Parallel |
| --- | --- | --- | --- | --- |
| `-Runtime Windows` (default) | A — closest to CI | Bare-metal Windows | Native (pyftpdlib, OpenSSH, chocolatey MySQL/ES) or Linux containers depending on `-LegacyWindowsDeps` | Forced **sequential** for any EngineSpec / sidecar / host-dep job (port 7071, FTP, UNC collide) |
| `-Runtime Linux` | B — fully containerised | Linux container per job | Linux containers, attached via `--network=container:<test>` so tests see `localhost:PORT` | Safe; default `-MaxParallel 4` |

If catalog mode picks the wrong runtime for what you want, set `-Runtime`
explicitly. `-SUTRuntime` is the deprecated alias.

## Catalog-mode flag cheatsheet

```
-Jobs <names...>     # default = all jobs
-List                # print catalog and exit (no run)
-IncludeDisabled     # also load jobs commented out in pipeline.yml
-SkipBuild           # don't auto-run Compile.ps1
-SkipReport          # don't run reportgenerator at the end
-NoParallel          # force sequential
-MaxParallel N       # parallel slot cap (Linux only really benefits)
-ReportFormat        # Html (default) | Badges | Cobertura | TextSummary
                     # | HtmlSummary | MarkdownSummary
-CoverageDir <path>  # default: <repo>\coverage
```

## Where output goes

- **Direct mode** — TRX + per-run `.coverage` snapshots in `.\TestResults\`
  (override with `-TestResultsDir`). With `-Coverage`, a merged
  `Cobertura.xml` is produced alongside.
- **Catalog mode** — per-job folders under `coverage\<job-slug>\`, merged
  cobertura at `coverage\merged\all_merged.cobertura.xml`, HTML report at
  `coverage\report\index.html` (auto-opened unless running under Azure DevOps).

## Troubleshooting

- **`Could not find any assemblies matching: ...`** — direct mode resolves
  `-Projects` relative to `$PWD`. `cd` into `Bin\ServerTests` (or wherever
  the DLLs live) before invoking, or pass `-BinDir`.
- **`Bin\ServerTests is empty and -SkipBuild was specified`** — drop
  `-SkipBuild`, or run `.\Compile.ps1 -ServerTests` first.
- **"Bare-metal Windows ... forcing sequential"** — expected: catalog mode
  refuses to fan out jobs that share host state (port 7071, FTP, UNC). Use
  `-Runtime Linux` if you want parallelism.
- **Docker not running** in Linux/container mode — start Docker Desktop; the
  script exits early with a clear message.
- **Stale `ww-cov-*` containers/networks** — the `finally` block removes
  anything tagged with the current `RunId`. If a prior run was killed (Ctrl-C
  before cleanup), run `docker ps -a --filter "name=ww-cov-"` and `docker rm
  -f` the leftovers. Same for `docker network ls`.
- **EngineSpec job (`-ServerType LightweightExecution` + `-CoverageDir`/`-EngineSessionId`)
  passes its pre-run stability check, then 503s on every route the moment `dotnet test`
  starts, identically on every retry** — `-Coverage` defaults to `$true`, so the SpecFlow
  test host's own `dotnet test` also runs `--collect "Code Coverage"` *while* the engine is
  already wrapped in its own `dotnet-coverage collect -- func start`. Two concurrent, unisolated
  coverage profiler sessions on the same bare-metal agent corrupt the engine's assembly loading
  at startup. Since the code under test runs inside the engine process (already separately
  instrumented), the test host's own coverage buys nothing — pass `-Coverage:$false` on the
  job. All EngineSpec jobs in `Dev/.azure/pipeline.yml` do this as of build 30818's fix.
- **FTP/FTPS port 21 / 1010 already bound** — another `pythonw.exe` from a
  previous run is still listening. `taskkill /im pythonw.exe /f` then re-run.
- **`vstest.console.exe not found`** — direct mode bootstraps it via NuGet
  into `.\Microsoft.TestPlatform\` on first run; ensure NuGet is on `PATH` or
  pass `-NuGet <path>`.
- **`zero tests ran (exit 8)`** in `-InContainer` mode — typically the filter
  didn't match anything in that assembly. Treated as a warning, not a failure.

## Where to look in the script

- Parameter block + mode router: top of `TestRun.ps1` and the
  `# Mode router` section near the bottom.
- Pipeline parser: `ConvertFrom-PipelineYaml`.
- Job dispatch: `Invoke-JobByRuntime` → `Invoke-WindowsBareMetalJob`,
  `Invoke-LinuxUnitJob`, `Invoke-LinuxEngineSpecJob`.
- Dep startup: `Start-Host*` / `Stop-Host*` functions.
- SUT lifecycle: `Start-LightweightExecution`, `Start-WarewolfServer`,
  `Stop-Engine`.
