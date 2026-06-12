# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Collaboration rules

- **Never assume.** When a request is ambiguous — missing context, unclear scope, or multiple valid interpretations — ask a focused clarifying question before proceeding.
- **Show evidence and confidence.** For any non-trivial claim about the codebase (architecture, behaviour, root cause), cite the file and line range you read and state your confidence level (e.g. *high — observed directly in code*, *medium — inferred from surrounding code*, *low — haven't verified*).
- **Always plan before coding.** For any change beyond a trivial fix, produce a written plan (affected files, approach, test strategy) and wait for approval before touching code.

## Expert context

Work at an enterprise-grade level against this stack:

- **.NET 8** (C# 12) — nullable reference types enabled, modern language features preferred
- **Azure Functions v4 isolated worker** — for the lightweight engine specifically
- **Clean Architecture** — respect layer boundaries; domain logic must not depend on infrastructure
- **Code quality** — prefer refactoring and optimisation over adding new abstractions; reduce duplication; improve readability without changing behaviour unless asked

## Model selection

Always pick the model tier that matches the task. If the preferred model is unavailable, stop and ask the user to choose before proceeding.

| Task type | Model | Current model ID |
|---|---|---|
| **Heavy** — deep reasoning, root-cause analysis, architectural design, complex multi-file refactors, security review | Latest Claude **Opus** | `claude-opus-4-8` |
| **Normal** — routine edits, simple bug fixes, test stubs, documentation, straightforward single-file changes | Latest Claude **Sonnet** | `claude-sonnet-4-6` |

**If the targeted model is unavailable:** do not fall back silently — prompt the user with:
> "Model `<model-id>` is not available. Please select a model to continue: Opus / Sonnet / other."

## Build

All builds run through `Compile.ps1` at the repo root. It wraps `dotnet restore` + `dotnet publish` and handles post-processing (native DLL pinning, resource copying, test-host bootstrapping).

**Prerequisites:** .NET 8 SDK, Visual Studio 2022/2026 (MSBuild), `nuget.exe` on PATH or at `C:\Windows\nuget.exe`.

```powershell
# Build test binaries (most common — required before running tests)
.\Compile.ps1 -ServerTests

# Build the server only
.\Compile.ps1 -Server

# Build the Studio desktop application
.\Compile.ps1 -Studio

# Build everything (slow — use before a release or after cross-cutting changes)
.\Compile.ps1

# Release build with auto-versioning from git tags
.\Compile.ps1 -Release -Config Release -AutoVersion -Target Rebuild

# Regenerate SpecFlow .feature.cs code-behind files after editing .feature files
.\Compile.ps1 -RegenerateSpecFlowFeatureFiles
```

Solution–switch–output mapping:

| Switch | Solution | Output |
|---|---|---|
| `-ServerTests` | `Dev\ServerTests.sln` | `Bin\ServerTests\` |
| `-Server` | `Dev\Server.sln` | `Bin\Server\` |
| `-Studio` | `Dev\Studio.sln` | `Bin\Studio\` |
| `-Release` | `Dev\Release.sln` | `Bin\Release\` |
| `-UITesting` | `Dev\UITesting.sln` | `Bin\UITesting\` |
| `-Web` | `Dev\Web.sln` | `Bin\Web\` |

See [Compile.md](Compile.md) for the full parameter reference and troubleshooting.

## Tests

All test runs go through `TestRun.ps1`. Run it from inside `Bin\ServerTests\` (direct mode) or from the repo root (catalog mode).

```powershell
# Run a single test project
cd Bin\ServerTests
.\TestRun.ps1 -Projects "Dev2.Activities.Tests"

# Filter by name or category
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Filter "FullyQualifiedName~CalculateActivity"
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Category "UnitTest"
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -ExcludeCategories "CannotParallelize","Integration"

# Run a comma-separated list of exact test names
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -TestsToRun "TestName1,TestName2"

# Run with code coverage (produces TestResults\Cobertura.xml)
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Coverage

# Run a specific CI pipeline job locally
.\TestRun.ps1 -Jobs "Unit_Tests"

# List all available CI jobs
.\TestRun.ps1 -List

# Run all CI jobs (full local pipeline)
.\TestRun.ps1
```

Jobs that require external dependencies (FTP, SQL Server, MySQL, Elasticsearch, RabbitMQ, Redis) use `-Start*` flags — e.g. `-StartFTPServer`, `-StartMySQLServer`. These run as Docker containers on Linux runtime or natively on Windows.

See [TestRun.md](TestRun.md) for the full parameter reference and troubleshooting.

## Integration tests (engine-dependent)

Any test project whose name ends in `.Integration.Tests` (e.g. `Warewolf.Execution.Lightweight.Integration.Tests`) requires the execution engine to be running before tests start. Follow this sequence every time.

### Step 1 — Start the execution engine

**Lightweight engine (port 7071)** — required by `Warewolf.Execution.Lightweight.Integration.Tests`:

```powershell
# Option A: Azure Functions Core Tools (preferred — install once with: npm i -g azure-functions-core-tools@4)
cd Dev\Warewolf.Execution.Lightweight
func start --port 7071

# Option B: dotnet run fallback (if func CLI is not installed)
cd Dev\Warewolf.Execution.Lightweight
dotnet run --project Warewolf.Execution.Lightweight.csproj
```

Verify the engine is up before running tests:
```powershell
Invoke-WebRequest -Uri "http://localhost:7071" -UseBasicParsing -TimeoutSec 5
# Expect: StatusCode 200 or 404 (any HTTP response means the host is ready)
```

Free port 7071 if it is already occupied:
```powershell
$conn = Get-NetTCPConnection -LocalPort 7071 -State Listen -ErrorAction SilentlyContinue
if ($conn) { Stop-Process -Id $conn.OwningProcess -Force }
```

### Step 2 — Run the tests

With the engine running on port 7071, execute tests using `dotnet test` directly (integration test projects are not compiled into `Bin\ServerTests\`, so `TestRun.ps1` is not used for them):

```powershell
cd Dev

# All tests in the integration test project
dotnet test "Warewolf.Execution.Lightweight.Integration.Tests\Warewolf.Execution.Lightweight.Integration.Tests.csproj" --no-build --logger "console;verbosity=normal"

# All tests in a specific class
dotnet test "Warewolf.Execution.Lightweight.Integration.Tests\..." --no-build --filter "FullyQualifiedName~WebGetToolIntegrationTests"

# A single named test
dotnet test "Warewolf.Execution.Lightweight.Integration.Tests\..." --no-build --filter "FullyQualifiedName=Warewolf.Execution.Lightweight.Integration.Tests.WebGetToolIntegrationTests.TC001_Get_CustomHeader_Echoed"

# Multiple named tests (use | inside the filter string)
dotnet test "Warewolf.Execution.Lightweight.Integration.Tests\..." --no-build --filter "FullyQualifiedName~TC001|FullyQualifiedName~TC002"

# A test category
dotnet test "Warewolf.Execution.Lightweight.Integration.Tests\..." --no-build --filter "TestCategory=Integration"
```

### Step 3 — After every test run: failure summary and fix

**Always produce a failure summary** whenever any tests fail:

```
## Test run summary
Total: N | Passed: N | Failed: N | Skipped: N | Duration: Xm Ys

### Failed tests
| # | Test name | File:line | Error (first line) |
|---|---|---|---|
| 1 | SomeTest | SomeFile.cs:42 | Assert.AreEqual failed … |

### Failure analysis
For each failure:
- Classification: network | auth | workflow-not-found | data-assertion | timeout | configuration | other
- Root cause: <2-3 sentences citing the specific error message and code path>
- Recommended fix: <concrete next step>
```

**After producing the summary, always prompt:**
> "Would you like me to fix the failed tests? If yes, I will produce a plan for each failure before making any changes."

### Common failure patterns

| Error | Likely cause | Fix |
|---|---|---|
| HTTP 500 `resolved permissions [Execute] do not satisfy required [View, Execute]` | `secure.config` missing `Public` group `View=true` | Add `View: true` to the `Public` group entry, or set `WAREWOLF_BYPASS_SECURE_CONFIG=true` |
| `No connection could be made` / `Connection refused` on port 7071 | Engine not started or still booting | Wait for the engine to be reachable before running tests |
| HTTP 404 on a workflow path | Workflow `.bite` file missing from `Resources/` | Verify the workflow file exists; check `workflow-index.json` |
| `Assert.AreEqual failed` with mismatched values | Workflow logic or output mapping changed | Read the workflow XML and align test expectations |

## Architecture

Warewolf is a .NET 8 SOA/ESB platform with a visual flow-based designer. The `Dev/` directory contains ~133 source projects and ~55 test projects across these layers:

### Two execution engines

There are two distinct ways to execute Warewolf workflows. They share the same activity and runtime libraries but have completely different hosting models:

| | **Lightweight** | **Server** |
|---|---|---|
| Project | `Warewolf.Execution.Lightweight` | `Dev2.Server` |
| Host | **Azure Functions v4 isolated worker** (.NET 8 `Exe`) | Windows service / bare-metal process |
| Port | 7071 (Functions default) | 3142 |
| Auth | JWT (HMAC-SHA256), Entra ID, anonymous `/Public/*`, function-key `/Services/*` | Internal Warewolf auth |
| Entry point | `Program.cs` → `HostBuilder` → `StartupOrchestrator` | `Dev2.Server` startup |
| Build switch | (included in `-ServerTests`) | `.\Compile.ps1 -Server` |

Both engines consume `Dev2.Activities`, `Dev2.Core`, `Dev2.Runtime.*`, and the shared driver/data libraries.

#### Lightweight (`Warewolf.Execution.Lightweight`)
Azure Function App with five function classes under `Functions/`:
- `WorkflowHttpFunction` — six HTTP triggers: anonymous `/Public/*`, JWT-secured `/Secure/*`, function-key `/Services/*`, by-name with suffixes (`.debug`, `.xml`, `.api`), root `/apis.json`
- `LoginFunction` — POST `/login` → JWT token
- `LicensingHttpFunction` — Chargebee subscription / license gate
- `LogFileFunction` — execution log retrieval
- `DropboxOAuthFunction` — OAuth callback flow

Startup sequence (7 steps in `Program.cs`): load config → bootstrap console logger → build host → run `StartupOrchestrator` → upgrade to composite logger (Console + App Insights + Elasticsearch + Audit) → license check → `host.RunAsync()`.

Auth middleware pipeline: EasyAuth redirect → claims builder → policy enforcement. Sensitive config optionally encrypted via Azure Key Vault–backed AES.

#### Server (`Dev2.Server`)
Full-featured SOA/ESB server. Wraps `Dev2.Runtime.*` and exposes REST APIs on port 3142. Hosts the workflow catalogue used by the WPF Studio designer.

### Shared activity and runtime libraries
- **`Dev2.Runtime.*`** — Workflow execution, variable resolution, configuration management, WebServer hosting
- **`Dev2.Activities`** / **`Dev2.Activities.Designers`** — 100+ built-in microservice activities (file operations, data manipulation, API calls, DB access). Each activity is drag-droppable in the Studio designer.

### Studio (WPF desktop client)
- **`Warewolf.Studio.ViewModels`** / **`Warewolf.Studio.Views`** — MVVM pair for the designer UI
- **`Warewolf.Studio.Core`** / **`Warewolf.Studio.CustomControls`** / **`Warewolf.Studio.Themes.Luna`** — Supporting UI components
- **`Warewolf.Studio.AntiCorruptionLayer`** — Isolation between Studio and server communication

### Data and drivers
- **`Dev2.Data.*`** — Data access abstractions and variable/expression engine
- **`Warewolf.Driver.*`** — Pluggable connectors: SQL Server, Oracle, MySQL, Elasticsearch, Redis, RabbitMQ
- **`Dev2.Infrastructure`** — Service discovery, scheduling

### Shared libraries
- **`Dev2.Common`** / **`Dev2.Core`** — Shared utilities, interfaces, and base types
- **`Warewolf.Language.Parser`** / **`Warewolf.Parsing`** — Variable and expression resolution; the language parser is written in **F#** (`WarewolfLanguage.fs`, `WarewolfLanguageLex.fs`)
- **`Warewolf.Logger`** — Structured logging (Serilog-based)

### Tests
Test projects follow the naming convention:
- `Dev2.*.Tests` / `Warewolf.*.Tests` — Unit and integration tests (MSTest)
- `*.Specs` / `Warewolf.Tools.Specs` — BDD acceptance tests (SpecFlow `.feature` files)
- `Warewolf.UIBindingTests.*` — UI binding tests for each connector type (SQL, Oracle, MySQL, Elasticsearch, etc.)

SpecFlow tests require a running engine. The `ServerType` parameter controls which: `-ServerType FullServer` (Server, port 3142) or `-ServerType LightweightExecution` (lightweight, port 7071).

### CI/CD
- `Dev/.azure/pipeline.yml` — Main Azure DevOps pipeline; parsed by `TestRun.ps1` catalog mode to reproduce CI jobs locally
- Stage 1 compiles; Stage 2 runs parallel test jobs including unit tests, activity tests, engine specs, and dependency-specific jobs

## Commit messages

Format: `{branch-number}-#{Feature|Fix}-{remaining-branch-name}`

| Tag | When to use |
|---|---|
| `#Feature` | New feature, addition, or non-trivial edit that is not a defect fix |
| `#Fix` | Bug fix, build error fix, or test failure fix |

Example — branch `8433-Fix-Tests-Failure-Warewolf.Execution.Lightweight.Tests`, fixing build errors:

```
8433-#Fix-Fix-Tests-Failure-Warewolf.Execution.Lightweight.Tests

- Fixed CS7036 in ServiceCollectionExtensions.cs: AuditLogger DI registration changed from new AuditLogger() to AddSingleton<AuditLogger>()
- Fixed CS0535 in CoreInfrastructureTests.cs / WorkflowAuthPolicyTests.cs: added missing LogTrace stubs to NoOpLogger and TrackingLogger
- Fixed AuditLogger.LogKeyVaultError argument order: exception was passed as format arg, corrected to _logger.LogError(ex, message)
```

Rules:
- First line is the subject — branch number, tag, then the remaining branch name verbatim (no description of your own).
- Body bullet points summarise each logical change; one bullet per file or concern.
- Keep subject under 72 characters if possible; body lines under 100.
