# Hangfire → Azure Architecture — Suspend/Resume on the Execution Engine

Target architecture for migrating Warewolf's suspend/resume (Hangfire) capability from
`WarewolfServer + hangfireserver.exe` to the **Execution Engine** (Azure Function App) plus a new
dedicated **ExecutionEngineJobProcessor** Function App.

Companion document: [HangFire-Migration-To-Azure-Plan-Step-By-Step.md](archive/HangFire-Migration-To-Azure-Plan-Step-By-Step.md)
(phased implementation plan, test matrix, decisions log — archived: Phases 0-8 are now fully implemented).

---

## 1. Current architecture (on-prem Server)

```
┌─────────────────────────────┐         ┌──────────────────────────────┐
│  Warewolf Server (3142)     │ spawns  │  hangfireserver.exe          │
│                             ├────────►│  (console app, monitored     │
│  Workflow execution         │         │   with auto-restart)         │
│  ┌───────────────────────┐  │         │  ┌────────────────────────┐  │
│  │ SuspendExecution      │  │         │  │ Hangfire Background    │  │
│  │ Activity              │──┼──┐      │  │ JobServer (workers)    │  │
│  └───────────────────────┘  │  │      │  │  + Dashboard (5001)    │  │
│  ┌───────────────────────┐  │  │      │  │  + ResumptionAttribute │  │
│  │ ManualResumption      │  │  │      │  └───────────┬────────────┘  │
│  │ Activity              │──┼──┤      │              │ due job:      │
│  └───────────────────────┘  │  │      │              │ ResumeWorkflow│
└─────────────────────────────┘  │      │              ▼               │
                                 │      │  WorkflowResume.Execute      │
                                 │      │  (ResourceCatalog, in-proc)  │
                                 ▼      └──────────────┬───────────────┘
                        ┌────────────────┐             │
                        │ SQL Server     │◄────────────┘
                        │ Hangfire schema│   shared machine + file system
                        └────────────────┘
```

Key mechanics (evidence):

- Suspend packages `{resourceID, environment, startActivityId, versionNumber, currentuserprincipal}`
  and creates a Hangfire job in `ScheduledState`; JobId = SuspensionId
  (`Dev2.Activities/Activities/SuspendExecutionActivity.cs:151-186`,
  `Warewolf.Driver.Persistence/Drivers/HangfireScheduler.cs:382-401`).
- hangfireserver.exe hosts Hangfire workers + dashboard (`Warewolf.HangfireServer/Dashboard.cs:63-88`)
  and is spawned/monitored by the Server (`Dev2.Server/HangfireServerMonitor.cs`).
- Timed resume executes **inside hangfireserver.exe** via `HangfireScheduler.ResumeWorkflow`
  → `WorkflowResume.Execute` → `ResumableExecutionContainer` (find node by `startActivityId`,
  continue with restored environment) — `HangfireScheduler.cs:428-482`,
  `Dev2.Runtime.Services/ESB/Management/Services/WorkflowResume.cs`,
  `Dev2.Runtime/ESB/Execution/WfExecutionContainer.cs:434-479`.
- Sensitive values are protected with **Windows DPAPI** (machine-bound).

### 1.1 The three resume methods and their synchronicity (evidence-based)

| Method (`HangfireScheduler.cs`) | Invoked by | Sync/async | Executes the continuation? | Return contract |
|---|---|---|---|---|
| `ResumeWorkflow` (L428-482) | Hangfire worker when a scheduled job is due | Synchronous inside the worker | **Yes** — `WorkflowResume.Execute` inline (L448) | `"Success"`/`"Failed"` consumed **only by Hangfire** (job result + state). No workflow awaits it — the suspended workflow's original caller already got its response at suspend time |
| `ResumeJob` (L228-324) | `ManualResumptionActivity`, no-override path (`ManualResumptionActivity.cs:176`) | **Synchronous, blocking** | **Yes** — `workflowResume.Execute` (L282-283) completes **before** the method returns | Returns `"Success"` only after the resumed continuation finished; any failure throws → the activity records the error into the calling workflow's environment. **The next activity in the calling workflow depends on this value/error state** |
| `ManualResumeWithOverrideJob` (L326-380) | `ManualResumptionActivity`, override path (`ManualResumptionActivity.cs:170`) | Synchronous, fast | **No** — only writes `ManuallyResumedState` (final) carrying the encrypted merged environment (L364-370) | `"Success"` = state recorded. The continuation is executed separately: the activity sets `StartActivityId` on the data object (`ManualResumptionActivity.cs:164`), consumed at the sub-execution boundary (`Dev2.Runtime/ESB/Control/EsbServicesEndpoint.cs:250-257`) via `ResumableExecutionContainer` *(exact ordering: medium confidence — inferred from the single consumption site)* |

**Design consequence:** the scheduled path (`ResumeWorkflow`) can safely become fire-and-forget;
the manual paths **must keep their synchronous, in-request semantics** or calling workflows
misbehave. See §4.3.

---

## 2. Target architecture (Azure)

```
                       Entra ID (tokens, app roles)
                    ┌────────────────────────────────┐
                    │  Engine app registration       │
                    │  roles: Warewolf_ClientApps,   │
                    │  Warewolf_JobProcessor,        │
                    │  Warewolf_ResumeOperators(opt) │
                    └───────┬──────────────┬─────────┘
                            │ MI token     │ user/daemon tokens
┌───────────────────────────┴───┐      ┌───┴──────────────────────────────────┐
│ ExecutionEngineJobProcessor   │      │ Execution Engine (Function App)      │
│ (dedicated Function App,      │      │                                      │
│  Windows Consumption Y1)      │      │  HTTP routes:                        │
│                               │      │   /Public/* /Secure/* /Services/*    │
│ ┌───────────────────────────┐ │ fire │   /Resume/{suspensionId}  ◄─── NEW   │
│ │ JobPollFunction           │ │ -and-│      (claim → 202 → execute →        │
│ │ TimerTrigger              │ │forget│       record final status)           │
│ │ %JOB_POLL_SCHEDULE%       │─┼──────►                                      │
│ │ (default 1 min, singleton)│ │ POST │  SuspendExecutionActivity (in-proc)  │
│ └───────────────────────────┘ │      │  ManualResumptionActivity (in-proc,  │
│ ┌───────────────────────────┐ │      │   synchronous via shared executor)   │
│ │ ReaperFunction            │ │      │  ResumptionExecutor (start-at-node)  │
│ │ stale Processing → Failed │ │      │  FileEncryption/DecryptionHelper     │
│ │ %JOB_STALE_MINUTES%       │ │      │  (AES-256-GCM, WFAES::)              │
│ └───────────────────────────┘ │      └───────┬──────────────────┬───────────┘
└───────────────┬───────────────┘              │                  │
                │ poll due Scheduled jobs      │ job CRUD +       │ AES key
                │ + reaper state writes        │ state CAS        │ (secret)
                ▼                              ▼                  ▼
       ┌─────────────────────────────────────────────┐   ┌──────────────┐
       │ SQL Server / Azure SQL — Hangfire schema    │   │ Azure Key    │
       │ (unchanged; jobs, states, args)             │   │ Vault        │
       └─────────────────────────────────────────────┘   └──────────────┘
                App Insights / Elasticsearch / Audit: both apps
```

### 2.1 Components

| Component | Hosting | Responsibility |
|---|---|---|
| **Execution Engine** (`Warewolf.Execution.Lightweight`) | Azure Function App, Windows Consumption (Y1), dotnet-isolated 8 | Workflow execution incl. `SuspendExecutionActivity` and `ManualResumptionActivity`; **`/Resume/{suspensionId}` route**: atomic claim → execute continuation → record final job status + audit |
| **ExecutionEngineJobProcessor** (new) | Dedicated Azure Function App, Windows Consumption (Y1) | Timer-driven poll of Hangfire storage for **due Scheduled jobs**; fire-and-forget invoke of the engine's resume route (MI bearer token); **fail-only reaper** for stale `Processing` jobs |
| **Hangfire SQL storage** | Azure SQL / SQL Server (prod), SQL LocalDB (dev), `Hangfire.InMemory` (tests) | Job store — schema unchanged; single source of truth for the job state machine |
| **Azure Key Vault** | existing | AES-256-GCM key (secret) for `WFAES::` encryption of persisted environments/principals and the persistence DbSource |
| **Entra ID** | existing engine app registration | `Warewolf_JobProcessor` app role for the processor's system-assigned MI; optional `Warewolf_ResumeOperators` role for human/manual callers |

### 2.2 What replaces hangfireserver.exe

| hangfireserver.exe responsibility | Azure replacement |
|---|---|
| `BackgroundJobServer` workers polling SQL | `JobPollFunction` (TimerTrigger, `%JOB_POLL_SCHEDULE%`, singleton via host lock) |
| Scheduled→Enqueued promotion + execution | Engine `/Resume` route: atomic claim (`ChangeState` Scheduled→Processing) + `ResumptionExecutor` |
| `ResumptionAttribute` audit/failure filter | State-transition audit events in the engine's composite logger + App Insights custom events |
| Crash recovery (invisibility-timeout re-fetch) | **Fail-only reaper** (deliberate change — see §5.2) |
| Hangfire Dashboard (5001) | Not hosted in Functions. Read-only dashboard can run anywhere with SQL access; App Insights workbook/alerts for ops |

---

## 3. Security model

Two-part authorization contract (same pattern as daemon clients — see
[Deploy-EndToEnd-Runbook.md](Deploy-EndToEnd-Runbook.md) §"Two-part authorization contract"):

1. **Token side** — the processor Function App's **system-assigned managed identity** is assigned
   the engine app role **`Warewolf_JobProcessor`**; its token (`api://<engine-app-id>/.default`)
   carries `roles: ["Warewolf_JobProcessor"]`. Registration uses the existing
   `Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity` flow.
2. **Policy side** — the engine's `secure.config` role map grants that role Execute on the resume
   route (route registered in `RouteAuthorizationRegistry`, enforced by
   `WorkflowAuthorizationMiddleware`). Denials are wrapped as **HTTP 500** (WOLF-8418).

The route stays **open to human/manual external callers** via role-gating (e.g. a
`Warewolf_ResumeOperators` role assignable to users/groups) — same endpoint, different principal.

**Encryption:** Windows DPAPI is replaced by the Key Vault-backed AES hooks:
`DpapiWrapper.AesEncryptHook` (new `FileEncryptionHelper`, AES-256-GCM →
`WFAES::Base64(nonce|ciphertext|tag)`) and the already-registered `AesDecryptHook`
(`Warewolf.Execution.Lightweight/Infrastructure/KeyVaultStartupExtensions.cs:47`). The activities
and driver need **no crypto changes** — `DpapiWrapper.Encrypt/Decrypt/CanBeDecrypted` route through
the hooks (`Warewolf.Security/Encryption/DPAPIWrapper.cs:98-101, 131-132, 167-168`).

> Constraint: jobs suspended by the on-prem Server (DPAPI) cannot be resumed in Azure, and
> vice-versa — each host can only decrypt its own format.

**Principals, not tokens:** `ExecutingUser` is populated from the authenticated Entra principal
(`WorkflowClaimsPrincipal : ClaimsPrincipal`); only the **principal name** is persisted (encrypted).
Bearer tokens are never stored — suspensions can outlive token lifetime by months, and tokens at
rest are a security anti-pattern. On resume a `GenericPrincipal` is rebuilt from the name, exactly
as today (`HangfireScheduler.cs:221-226`).

---

## 4. Runtime flows

### 4.1 Suspend (inside the engine)

```
Caller ── POST /Secure/MyWorkflow ──► Engine
  Engine: WorkflowExecutor walks activities
    └─ SuspendExecutionActivity.Execute
         ├─ snapshot environment (+ engine snapshot keys, partial WorkflowExecutionResult)
         ├─ DpapiWrapper.Encrypt → AesEncryptHook → WFAES:: values (when EncryptData)
         ├─ HangfireScheduler.ScheduleJob → Hangfire job, ScheduledState(resumeAtUtc)
         ├─ Result := SuspensionId (JobId); SaveDataFunc runs when AllowManualResumption
         └─ returns null → activity walk stops
  Engine ──► HTTP response: outputs as of suspend point, Result = SuspensionId
```

### 4.2 Scheduled (timed) resume — fire-and-forget

```
JobPollFunction (timer tick, singleton)
  ├─ query Hangfire storage: Scheduled jobs with EnqueueAt (UTC) <= now
  └─ for each: POST /secure/resume/{jobId}  (MI token, Warewolf_JobProcessor)
       └─ await ONLY the ack (short timeout), then move on          ◄ fire-and-forget
            200/202 → Dispatched · 409 → AlreadyClaimed (benign)
            ack timeout → AckTimeout (engine still executing; state reconciles next tick)
            other/network → Failed (job stays Scheduled; retried next tick)

Engine POST /secure/resume/{jobId}   (WorkflowResumeFunction)
  ├─ CLAIM: ChangeState(jobId, Processing, expected: Scheduled)     ◄ atomic CAS
  │     └─ lost → 409 {state}, nothing executes (duplicate-proof) · unknown id → 404
  ├─ ResumptionExecutor: read job args → decrypt (AES hooks) → rebuild env+principal
  │     → resolve .bite (engineWorkflowFilePath, else resource-ID scan)
  │     → parse → flatten chain → SKIP to node UniqueID == startActivityId
  │     → execute continuation SYNCHRONOUSLY
  ├─ record final state: Succeeded / Failed (fail-only) — CAS expecting Processing
  └─ respond ON COMPLETION: 200 {suspensionId,state,outputs} · 500 {state:"Failed",error}
```

**Why synchronous (design refinement):** the isolated-worker HTTP model sends the
response when the invocation completes — an early 202 + background execution would be
killable at any point after the response. A caller disconnect (the processor's short ack
timeout) does NOT abort a running invocation, so synchronous execution is the reliable
option; the processor's `AckTimeout` outcome covers continuations that outrun the ack
window, and the reaper covers host kills. The claim states are driver-provided
`ExternalProcessingState`/`ExternalSucceededState` — canonical Hangfire state names and
data keys (Hangfire's own `ProcessingState`/`SucceededState` ctors are worker-internal),
so monitoring, the dashboard, the reaper, and the existing state guards see them as
normal states.

No workflow consumes `ResumeWorkflow`'s return value today (§1.1), so fire-and-forget introduces
**no behavioural change** on this path.

### 4.3 Manual resumption — synchronous, in-request (behaviour-preserving)

`ManualResumptionActivity` is part of a calling workflow; its `Response`/error state feeds the
**next activity**. Both manual paths therefore remain synchronous in the engine, via the
`IResumptionExecutor` seam registered into `CustomContainer` at startup (the Server
registers nothing and keeps its `WorkflowResume` endpoint unchanged):

```
Calling workflow (running in the engine)
  └─ ManualResumptionActivity
       ├─ no-override: HangfireScheduler.ResumeJob — state guards identical to today —
       │    → seam: ResumptionExecutor.Execute(values) IN-PROCESS, SYNCHRONOUSLY
       │    ├─ execute continuation to completion (serialized ExecuteMessage back,
       │    │    exactly the WorkflowResume.Execute contract)
       │    ├─ mark ManuallyResumed(final) — parity with ResumeJob (L313-314)
       │    └─ return "Success" / throw  ◄ Response only after completion (parity)
       └─ override: merge envs + run OverrideDataFunc (unchanged, in-activity)
            ├─ HangfireScheduler.ManualResumeWithOverrideJob: state guards →
            │    mark ManuallyResumed(final) with encrypted merged env (parity L364-370)
            └─ seam: ResumptionExecutor.ExecuteOverrideContinuation — continuation runs
                 SYNCHRONOUSLY with the caller's merged environment, starting at the
                 activity-set StartActivityId (engine equivalent of the Server's
                 EsbServicesEndpoint.cs:250-257 sub-execution pipeline); StartActivityId
                 reset afterwards
```

Trade-off (documented): synchronous manual resumption consumes the calling request's
function-timeout budget; a continuation that exceeds it errors out with logging (fail-only, human
decision on tier upgrade).

### 4.4 Job state machine

```
            ┌────────────┐  poll: due only   ┌────────────┐
 suspend ──►│ Scheduled  │──── CAS claim ───►│ Processing │
            └─────┬──────┘   (single winner) └──┬───┬─────┘
                  │                             │   │
                  │ manual resume (sync)        │   │ reaper: stale > %JOB_STALE_MINUTES%
                  │ same CAS claim              │   │ (incl. functionTimeout kills)
                  ▼                             ▼   ▼
            ┌───────────────┐          ┌───────────┐ ┌─────────┐
            │ ManuallyResumed│(final)  │ Succeeded │ │ Failed  │(final, fail-only:
            └───────────────┘          └───────────┘ └─────────┘ never auto-retried)
```

Guards (parity with `HangfireScheduler.cs:162-175, 245-258, 341-354`): any resume attempt against
`Succeeded` / `ManuallyResumed` / `Enqueued` / `Processing` is rejected with the same
`ErrorResource` messages as today.

---

## 5. Failure modes

### 5.1 Duplicate prevention (MUST hold)

1. **Single poller** — TimerTrigger is a singleton per app (host blob-lease lock).
2. **Atomic claim before any execution** — `ChangeState(jobId, Processing, expected: Scheduled)`
   in SQL is the single serialization point; overlapping ticks, processor crash-and-repost, and
   concurrent human calls collapse to exactly one winner (losers get 409).
3. **Terminal states** — `Failed`/`Succeeded`/`ManuallyResumed` cannot be claimed.

### 5.2 Engine timeout mid-execution — is the job re-invoked? **No (by design).**

*Existing Hangfire behaviour:* a worker holds a fetch lease (`SlidingInvisibilityTimeout = 5 min`,
`Warewolf.HangfireServer/Program.cs:190, 202`) renewed while running; if the process dies, renewal
stops, the job becomes visible and **another worker re-fetches and re-executes it**
(at-least-once). `[AutomaticRetry(Attempts = 0)]` (`HangfireScheduler.cs:428`) suppresses only
*exception-based* retries, not crash-requeue. *(High confidence on mechanism — config observed +
documented Hangfire semantics; exact edge timing medium.)*

*New behaviour (deliberate change, resolved decision "fail-only"):* the poller selects **only due
`Scheduled` jobs**, so a claimed job in `Processing` is invisible to it. If the engine's
functionTimeout (10 min on Y1) kills the execution, the job stays `Processing` until the reaper
marks it **`Failed`** with reason *"exceeded execution budget (functionTimeout)"* + error logging +
App Insights alert. **It is never re-invoked automatically** — a human decides (re-schedule
manually or upgrade tier). Rationale: partially-executed continuations have side effects; the
existing code itself warns *"these jobs may not be very safe to resume … re-queue with caution"*
(`HangfireScheduler.cs:467-477`). Fail-only makes the unsafe path impossible by default.

### 5.3 Other failures

| Failure | Handling |
|---|---|
| POST from processor never arrives (network) | Job stays `Scheduled` → picked up again next tick (safe: claim not yet taken) |
| Engine throws during execution | Engine records `Failed` immediately (exception path), audit logged |
| Processor tick overruns / overlaps | Singleton timer + CAS claim → no duplicates |
| SQL transient errors | Short-lived connections + transient retry with jitter in both apps |
| Key Vault unavailable at cold start | Engine startup fails fast (existing behaviour for encrypted sources) |

---

## 6. Configuration

| Setting | Where | Purpose |
|---|---|---|
| `Settings/persistencesettings.json` | Engine + Processor | Enable, scheduler=Hangfire, EncryptDataSource, PrepareSchemaIfNecessary. When `Enable=false` the loader clears the scheduler name in memory so suspend/resume workflows still parse without a database |
| `Settings/persistencesettingsdbsource.bite` | Engine + Processor | Hangfire SQL `DbSource`; `ConnectionString` WFAES-encrypted and runtime-decrypted exactly like `Settings/ElasticsearchLoggingSource.bite` |
| `%JOB_POLL_SCHEDULE%` | Processor app setting | NCRONTAB poll cadence (default `0 */1 * * * *`) — TimerTrigger reads `%VAR%` natively |
| `%JOB_REAPER_SCHEDULE%` | Processor app setting | NCRONTAB reaper cadence (default `30 */5 * * * *`) |
| `%JOB_STALE_MINUTES%` | Processor app setting | Reaper threshold for stale `Processing` → `Failed` (default 15) |
| `%ENGINE_RESUME_BASEURL%` | Processor app setting | Engine base URL for the resume route |
| `%ENGINE_RESUME_SCOPE%` | Processor app setting | Token scope for the engine's app registration, e.g. `api://<engine-app-id>/.default` |
| `%ENGINE_RESUME_TIMEOUT_SECONDS%` | Processor app setting | Claim-ack HTTP timeout (default 15) — the processor never waits for workflow completion |
| `%ENGINE_RESUME_AUTH_DISABLED%` | Processor app setting | Development only: skip bearer-token acquisition |
| `AZURE_KEYVAULT_NAME` / `KEYVAULT_SECRET_NAME` / `DEBUG_AZURE_KEYVAULT_SECRET` | both apps (identical names) | AES key material (Key Vault; debug bypass for local dev) |

**Engine-resume metadata (additive job keys, stamped at suspend time via the
`IJobValuesEnricher` seam):** `engineWorkflowName`, `engineWorkflowFilePath`,
`engineExecutionId`, `engineSuspendedAtUtc` — alongside the five unchanged legacy keys,
so Server-created and engine-created jobs stay schema-compatible.

**Storage tiers:** production = Azure SQL/SQL Server (`Hangfire.SqlServer`, schema unchanged);
local dev = SQL Server Express **LocalDB** (connection-string-only change, full provider fidelity);
automated tests = **`Hangfire.InMemory`** via a storage-factory seam (the DI ctor already accepts
any `JobStorage`, `HangfireScheduler.cs:81-88`).

---

## 7. Compatibility notes (verified)

- `Warewolf.Driver.Persistence` is `net8.0` and is **already deployed inside the engine** —
  `Dev2.Activities.csproj` references it (L556) and the engine references `Dev2.Activities`.
  Build/deploy compatibility is proven de facto.
- **Performance counters are the one sandbox blocker** — `HangfireScheduler`'s ctor eagerly builds
  real Windows counters (`HangfireScheduler.cs:90-95, 110-122`;
  `PerformanceCounterCategoryWrapper.cs:29` calls `PerformanceCounterCategory.Create`, which is
  Windows-only **and blocked by the App Service sandbox**). Resolution: engine registers a no-op
  `IWarewolfPerformanceCounterLocater` (the codebase ships `EmptyCounter`) and counter construction
  becomes lazy — Server behaviour untouched. Azure telemetry (App Insights) replaces counters.
- Both Function Apps run **Windows** Consumption (deploy script: `--os-type Windows`,
  `Deploy-WwExecutionEngine.ps1:951-953`), so Windows-targeted packages (EventLog etc.) are inert
  but harmless. functionTimeout on Y1 is capped at 10 minutes (see §5.2).
