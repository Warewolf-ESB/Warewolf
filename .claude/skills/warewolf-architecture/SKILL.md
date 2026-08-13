---
name: warewolf-architecture
description: Warewolf architecture deep reference — the two execution engines (Lightweight Azure Functions vs Dev2.Server), the Lightweight auth/authorization model (EasyAuth → claims → policy, 500-not-403/WOLF-8418, apis.json discovery), shared activity/runtime/data/driver libraries, Studio layers, and test-project conventions. Invoke when analysing, navigating, or changing any subsystem, or reasoning about auth/policy behaviour.
---

# Warewolf architecture

Warewolf is a .NET 8 SOA/ESB platform with a visual flow-based designer. `Dev/` contains ~133 source projects and ~55 test projects. Workflows authored in Warewolf Studio / Warewolf Web Studio (Angular) are executed as `.bite` files.

## Two execution engines

Same activity/runtime libraries, different hosting models:

| | **Lightweight** | **Server** |
|---|---|---|
| Project | `Warewolf.Execution.Lightweight` | `Dev2.Server` |
| Host | **Azure Functions v4 isolated worker** (.NET 8 `Exe`) | Windows service / bare-metal process |
| Port | 7071 (Functions default) | 3142 |
| Auth | JWT (HMAC-SHA256), Entra ID, anonymous `/Public/*`, function-key `/Services/*` | Internal Warewolf auth |
| Entry point | `Program.cs` → `HostBuilder` → `StartupOrchestrator` | `Dev2.Server` startup |
| Build switch | (included in `-ServerTests`) | `.\Compile.ps1 -Server` |

Both consume `Dev2.Activities`, `Dev2.Core`, `Dev2.Runtime.*`, and the shared driver/data libraries.

### Azure-side satellite workers

Two additional deployables exist **only on the Azure path**; each replaces an on-prem child process
and calls the engine as a daemon (managed identity + Entra app role). The on-prem Server equivalents
are untouched.

| | `Warewolf.Execution.EngineJobProcessor` | `Warewolf.Execution.QueueProcessor` |
|---|---|---|
| Replaces | `hangfireserver.exe` | `N × QueueWorker.exe` (+ `QueueWorkerMonitor`) |
| Host | Azure **Function App** (TimerTrigger) | Azure **Container Apps**, Linux container |
| Scaling | singleton timer | **KEDA `rabbitmq` scaler, 0→N replicas, one app per queue-trigger** |
| Engine role | `Warewolf_JobProcessor` — **global-scope** `Execute` row | `Warewolf_QueueProcessor` — **per-workflow** `Execute` row, one MI per app |
| Engine route | POST `/secure/resume/{jobId}` | POST `/Secure/{workflow}.json` |
| RabbitMQ client | n/a | **`RabbitMQ.Client` 7.x, worker-local** — deliberately NOT `Warewolf.Driver.RabbitMQ` (pinned to 5.1.2; `QueueingBasicConsumer`, used by `DsfConsumeRabbitMQActivity`, was removed in v6, so the shared package is not upgraded) |
| Config source | staged `Settings/persistencesettings*.bite` | staged `Settings/triggers*.bite` + `{QueueSourceId}.bite` |

Both consume the engine's Key Vault WFAES stack as **linked shared source** (two `Exe` projects
cannot reference each other). The QueueProcessor does **not** link the engine's logger sinks:
`ExecutionLoggerBase` depends on Functions invocation correlation, so it implements its own
`Dev2Logger.ExternalSink` against the same `EXECUTIONLOGLEVEL`/`ENABLE*` env-var contract.
Neither worker references `Dev2.Data` (it would re-pin `RabbitMQ.Client`).

### Lightweight (`Warewolf.Execution.Lightweight`)

Azure Function App with five function classes under `Functions/`:
- `WorkflowHttpFunction` — six HTTP triggers: anonymous `/Public/*`, JWT-secured `/Secure/*`, function-key `/Services/*`, by-name with suffixes (`.debug`, `.xml`, `.api`), root `/apis.json`
- `LoginFunction` — POST `/login` → JWT token
- `LicensingHttpFunction` — Chargebee subscription / license gate
- `LogFileFunction` — execution log retrieval
- `DropboxOAuthFunction` — OAuth callback flow

Startup sequence (7 steps in `Program.cs`): load config → bootstrap console logger → build host → run `StartupOrchestrator` → upgrade to composite logger (Console + App Insights + Elasticsearch + Audit) → license check → `host.RunAsync()`.

Auth middleware pipeline: EasyAuth redirect → claims builder → policy enforcement. Sensitive config optionally encrypted via Azure Key Vault–backed AES.

**Authorization model** (`WorkflowAuthPolicyLoader` / `WorkflowPolicyMatcher`, secure.config-driven):
- **Scope** — a workflow uses the **resource** role map (per-workflow, non-global entries) *exclusively* when it has any Execute-bearing resource entry; otherwise it uses the **global** role map (`IsServer=true` entries). A resource entry forms a policy only when it carries the `Execute` flag — a View-only resource entry does **not** override the global grant.
- **Public group** is always OR'd into the effective permissions of the active scope.
- **Denials are wrapped as HTTP 500** (nested `Error{…}`), not 403 — the 403 path is commented out pending WOLF-8418. 401/400/503 use a flat `{error,message,path,correlationId}` body.
- **Discovery** (`apis.json`) requires **both View and Execute** (View-only or Execute-only ⇒ not discoverable); only `ResourceType="WorkflowService"` resources are listed.

### Server (`Dev2.Server`)

Full-featured SOA/ESB server. Wraps `Dev2.Runtime.*`, exposes REST APIs on port 3142, hosts the workflow catalogue used by the WPF Studio designer.

## Shared activity and runtime libraries
- **`Dev2.Runtime.*`** — workflow execution, variable resolution, configuration management, WebServer hosting
- **`Dev2.Activities`** / **`Dev2.Activities.Designers`** — 100+ built-in microservice activities (file ops, data manipulation, API calls, DB access); each is drag-droppable in the Studio designer

### ⚠️ Activity instances are NOT safe to share across concurrent executions

`ActivityParser.Parse` **clones nothing**. It walks the `Flowchart` via
`WorkflowInspectionServices.GetActivities()` and returns references to the *same* `Dsf*Activity`
objects ([ActivityParser.cs:192-202](../../../Dev/Dev2.Activities/Activities/ActivityParser.cs#L192-L202)).
Any caller caching a `DynamicActivity` and parsing it per request therefore hands **one shared activity
graph** to every concurrent execution.

That matters because activities hold **per-execution state in instance fields**. All six database
activities do:

```csharp
public IServiceExecution ServiceExecution { get; protected set; }          // instance field
BeforeExecutionStart(...) { ServiceExecution = new DatabaseServiceExecution(dataObject); }
ExecutionImpl(...)        { ServiceExecution.Execute(out execErrors, update); }
```

Two concurrent executions overwrite each other's `ServiceExecution`, and the loser runs against the
winner's `DsfDataObject` — so its output variable is silently never written. Measured 2026-08-11 on a
SQL workflow: sequential 10/10 pass; concurrency 4/6/10 → 3/4, 4/6, 8/10, the failures reporting
`Object reference not set…` plus `Error with variables in input. [[JobLogId]]`. A workflow with no such
state (an Assign) passes 20/20 at concurrency 20, which is why single-message tests never catch it.

- **Lightweight** — fixed. `WorkflowExecutor` pools *prepared workflows*; each execution rents one
  exclusively and returns it in a `finally`. Three call sites: `WorkflowExecutor`, `ResumptionExecutor`,
  `LightweightEsbChannel` (nested sub-workflows — the most exposed, one callee usually has many callers).
- **Server** — **still exposed.** `ResourceActivityCache` shares parsed chains the same way and
  `Dev2.Activities` was deliberately not changed. Treat concurrent execution of one workflow with a
  DB/service activity as unsafe there until fixed.

When adding an activity, keep per-execution state on the `DsfDataObject`, never on the activity.

## Studio (WPF desktop client)
- **`Warewolf.Studio.ViewModels`** / **`Warewolf.Studio.Views`** — MVVM pair for the designer UI
- **`Warewolf.Studio.Core`** / **`Warewolf.Studio.CustomControls`** / **`Warewolf.Studio.Themes.Luna`** — supporting UI components
- **`Warewolf.Studio.AntiCorruptionLayer`** — isolation between Studio and server communication

## Data and drivers
- **`Dev2.Data.*`** — data access abstractions and variable/expression engine
- **`Warewolf.Driver.*`** — pluggable connectors: SQL Server, Oracle, MySQL, Elasticsearch, Redis, RabbitMQ
- **`Dev2.Infrastructure`** — service discovery, scheduling

## Shared libraries
- **`Dev2.Common`** / **`Dev2.Core`** — shared utilities, interfaces, base types
- **`Warewolf.Language.Parser`** / **`Warewolf.Parsing`** — variable/expression resolution; the language parser is **F#** (`WarewolfLanguage.fs`, `WarewolfLanguageLex.fs`)
- **`Warewolf.Logger`** — structured logging (Serilog-based)

## Tests (naming conventions)
- `Dev2.*.Tests` / `Warewolf.*.Tests` — unit and integration tests (MSTest)
- `*.Specs` / `Warewolf.Tools.Specs` — BDD acceptance tests (SpecFlow `.feature` files; require a running engine)
- `Warewolf.UIBindingTests.*` — UI binding tests per connector type (SQL, Oracle, MySQL, Elasticsearch, …)

## CI/CD
- `Dev/.azure/pipeline.yml` — main Azure DevOps pipeline; parsed by `TestRun.ps1` catalog mode to reproduce CI jobs locally. Stage 1 compiles; Stage 2 runs parallel test jobs (unit, activity, engine specs, dependency-specific).

See also: `warewolf-deploy` (Lightweight engine Azure deployment) and the docs under `Dev/Warewolf.Execution.Lightweight/docs/` (`Part1-Architecture.md`, `Part2-DataFlowDiagrams.md`).
