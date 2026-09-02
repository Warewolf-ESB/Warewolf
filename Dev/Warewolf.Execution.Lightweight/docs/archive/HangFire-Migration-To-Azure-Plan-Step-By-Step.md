# Hangfire Migration to Azure — Step-by-Step Plan

Phased implementation plan for migrating Warewolf's suspend/resume (Hangfire) capability from
`WarewolfServer + hangfireserver.exe` to the **Execution Engine** (Azure Function App) plus a new
dedicated **ExecutionEngineJobProcessor** Function App.

Companion document: [HangeFire-Azure-Architecture.md](HangeFire-Azure-Architecture.md)
(target architecture, security model, runtime flows, state machine, failure modes).

> **Status: Phases 0–4 IMPLEMENTED.**
> Phases 0–2: perf-counter runtime seam, AES encrypt hook, persistence settings +
> cold-start hydration (open item 3 resolved: in-memory `LightweightPersistenceSettings`,
> no runtime writes) — with approved unit tests.
> Phase 3: authenticated principal → `DsfDataObject.ExecutingUser` (named fallback for
> anonymous), `ResourceID`/`VersionNumber` extracted from the `.bite`, additive
> `IJobValuesEnricher` seam stamping engine-resume metadata (`engineWorkflowName`,
> `engineWorkflowFilePath`, `engineExecutionId`, `engineSuspendedAtUtc`), and a
> disabled-persistence guard (scheduler name cleared in memory so suspend workflows
> still parse without a database).
> Phase 4: new `Warewolf.Execution.EngineJobProcessor` Function App (TimerTrigger
> `%JOB_POLL_SCHEDULE%` poll of due Scheduled jobs → fire-and-forget POST to
> `/secure/resume/{jobId}` with MI token, awaiting only the ack, not workflow completion (Phase 5
> made the engine respond synchronously — 200/500, with a slow response mapped to `AckTimeout`);
> fail-only reaper via
> `%JOB_REAPER_SCHEDULE%`/`%JOB_STALE_MINUTES%`), sharing the engine's settings pair and
> Key Vault decryption stack via linked sources; added to `ServerTests.sln`.
> Phase 3–4 tests are implemented and green: driver 36/36 (enricher seam on/off),
> engine unit 639/639 (ResolveExecutingUser, ExtractResourceIdentity,
> SuspendSnapshotContext/enricher, principal-attach + body-injection guard), new
> `Warewolf.Execution.EngineJobProcessor.Tests` 17/17 (settings, dispatch outcomes
> 202/409/500/timeout/exception, due-selection boundary, fail-only reaper CAS),
> engine integration 292/292. An end-to-end suspend workflow test is deferred until a
> suspend `.bite` test resource exists (none in the repo; the chain is covered at the
> driver + unit level). NOTE: `Dev/.azure/pipeline.yml` needs a job for the new test
> project (pending).
> **Phase 5 IMPLEMENTED:** `ResumptionExecutor` (engine) serves all three resume paths on
> the lightweight pipeline — POST `/secure/resume/{suspensionId}`
> (`WorkflowResumeFunction`, full auth pipeline, `[RequireWorkflowPermission(Execute)]`,
> global-scope role row required in secure.config) claims via atomic CAS
> Scheduled→Processing (404/409 on miss/loss), executes the continuation
> **synchronously**, records `Succeeded`/`Failed` (fail-only), and responds on
> completion (200/500 — an isolated-worker function cannot emit an early 202 and keep
> executing; caller disconnects don't abort execution; the processor maps its short ack
> timeout to a benign `AckTimeout` outcome that reconciles next tick). Manual
> resumption: `IResumptionExecutor` seam in the driver — `ResumeJob` (no-override)
> executes in-process on the engine with the exact `WorkflowResume.Execute` contract;
> `ManualResumeWithOverrideJob` runs the override continuation synchronously against
> the caller's merged environment from `StartActivityId` (engine equivalent of the
> Server's sub-execution pipeline); Server paths byte-for-byte unchanged (no seam
> registered there). Claim/success states are driver-provided
> `ExternalProcessingState`/`ExternalSucceededState` (canonical Hangfire names + data
> keys; Hangfire's own ctors are worker-internal). Storage resolution centralised in
> `HangfireStorageFactory` (engine + processor).
> **Phase 5 tests IMPLEMENTED and green** (19 new): engine `ResumptionExecutorTests` +
> `WorkflowResumeFunctionTests` (13) — TryClaim NotFound/Claimed/Conflict, ExecuteClaimed
> fail-only recording, `Execute(values)` never-throws, `ResolveWorkflowFile` precedence,
> override no-op, and the route's 503/404/409/500 mappings over `Hangfire.MemoryStorage`;
> driver `HangfireSchedulerResumeSeamTests` + `ExternalJobStatesTests` (6) — the
> `IResumptionExecutor` seam on both manual paths (used vs Server default; fail-and-throw;
> override fires after ManuallyResumed) and the external states' Hangfire-contract parity +
> monitoring-API round-trip. Full suites green: driver 42/42, engine unit 652/652
> (processor 18/18, integration 292/292 unchanged — no production code touched this phase).
> The route's **200/Succeeded** mapping and a true suspend→poll→resume flow need a deployed
> suspend `.bite` + SQL-backed storage, so they remain the deferred end-to-end integration
> test (Phase 8) rather than a brittle unit fixture.
> **Phase 7 scripts + docs IMPLEMENTED:** new `Scripts/Deploy-WwJobProcessor.ps1`
> (self-contained orchestrator for the poller/reaper Function App — infra + system MI +
> Key Vault wiring + stage/WFAES-encrypt the persistence pair + `JOB_*`/`ENGINE_RESUME_*`
> app settings + deploy + verify; persistence source files prompted when not passed) plus a
> `-DeployJobProcessor` companion toggle (default off) + `-EnablePersistence`/persistence
> staging added to `Deploy-WwExecutionEngine.ps1` (Phase 3.5b, mirroring the Elasticsearch
> source; new Phase 6 calls the child). Role assignment stays the existing daemon flow
> (`Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity
> -AppRolesToAssign Warewolf_JobProcessor` — no script change needed). Docs synced:
> `Scripts/README.md`, `Deploy-RunGuide.md`, `Deploy-EndToEnd-Runbook.md` (new §7 JobProcessor
> deploy+authorize section, teardown extended). Both scripts parse-clean and pass the
> `-LoadFunctionsOnly` hook.
> **Phase 7 tests + CI IMPLEMENTED:** `Tests/Deploy-WwJobProcessor.Tests.ps1` (Pester 5 —
> static/ValidateSet, helpers, DryRun end-to-end via az-shim, persistence-pair staging into
> the temp staging dir, fail-loud prompt validation) is **green 23/23**; the engine suite
> pins the new persistence/JobProcessor params and adds the separate-publish-dir collision
> guard cases, now **green 76/76** (combined deploy Pester **99/99**). CI
> now runs `Warewolf.Execution.EngineJobProcessor.Tests` in the dedicated "Lightweight
> Execution + JobProcessor Unit Tests" job (`pipeline.yml` — added to that job's `-Projects`
> and excluded from the big unit bucket so it runs exactly once). **Phase 7 is complete.**
>
> **Phase 7 refinement (deploy isolation):** both orchestrators now publish each app to its
> own directory and stage into a **fresh OS-temp dir** (never mutating the publish output),
> preparing/uploading the engine and JobProcessor zips **separately**; `-DeployJobProcessor`
> fails loudly at plan time if `-JobProcessorPublishPath` collides with the engine's
> `-PublishPath` (decision #12). Phase 8 (the end-to-end suspend→poll→resume fixture +
> remaining scenarios) remains planned.
> Phase 6 (LocalDB/InMemory storage) was delivered alongside earlier phases.
>
> **External design review addressed (2026-07-21):** the two blocking review items are already
> satisfied by the implementation — the resume route executes SYNCHRONOUSLY then responds (no
> post-202 background work: `WorkflowResumeFunction`), and it is fully auth-enforced by riding the
> existing `/secure` prefix + registry (`WorkflowAuthorizationMiddleware`,
> `ServiceCollectionExtensions` registers `WorkflowResumeFunction`). The review's remaining points
> are captured as decision #13 (dual-host DB isolation), open items #3–#6 (hosting tier;
> deploy-interruption + fail-only volume; `Failed`-job operator surface; optional queue-triggered
> resume), the Phase 1 WFAES key-rotation limitation, and the Phase 8 "run duplicate-prevention on
> SQL, not `MemoryStorage`" note. No production code change was required for correctness or security.
>
> **Phase 8 IMPLEMENTED (2026-07-22) — no production code changed.** 9 new tests:
> *8a (gap-fill, MemoryStorage/mocks):* engine `TryClaim` returns **Conflict** for `Failed`/
> `Succeeded`/`ManuallyResumed` jobs (not-claimable guards); processor proves a reaped `Failed`
> job and an in-flight `Processing` job are **never re-selected** by `JobPoll` (real `MemoryStorage`);
> encryption gains a known-answer WFAES **interop vector** (Encrypt-Config.ps1 byte layout), a
> **rotated/mismatched-key → clean `AuthenticationTagMismatchException`** (validates the key-rotation
> limitation — never silent-wrong), and a non-WFAES pass-through guard; driver seam proves `ResumeJob`
> **finalises state only after the continuation completes** (synchronous, no early ack).
> *8a audit:* the six `CalculateResumptionDate` options and state-guard `ErrorResource` messages are
> already covered by `HangfireSchedulerTests` — no duplication added.
> *8b-1 (end-to-end resume):* `ResumeEndToEndTests` seeds a suspend job on real Hangfire storage and
> drives claim → **execute a real workflow continuation** → `Succeeded` with real outputs.
> *8b-3:* override-ordering parity **resolved by code-read** (open item #1).
> **Deferred (with reasons):** the **SQL-atomicity concurrency** test (concurrent double-claim → one win)
> is verified-by-design (SQL `ChangeState` CAS) and left to a manual/integration LocalDB run — the CI
> job provisions no SQL, so a unit-project version would be Inconclusive by default.
>
> **Phase 8 upgraded to the REAL demo artifacts (2026-07-23).** The user supplied real workflows
> (`hangfiredemo\Suspend Execution Example.bite` — a `SuspendExecutionActivity` → Assign → the
> `Hello World` sub-workflow → a Write File; `Manual Resumption Tool Example.bite`; and the
> `hangfiredb` SqlDatabase source). `ResumeEndToEndTests` now runs the **real** Suspend Execution
> Example (committed under `TestResources\hangfiredemo\` with its `Hello World` sub-workflow):
> `ScheduledResume_…` claims + executes the real continuation to `Succeeded`; `ManualResume_…ViaSeam`
> drives the manual no-override seam (`IResumptionExecutor.Execute`). Hermetic on `Hangfire.MemoryStorage`
> — the continuation's `D:\jobdata.txt` Write File is redirected to a temp file at stage time; the
> suspend job is seeded with exactly the values `SuspendExecutionActivity` persists
> (startActivityId = `NextNodes.First`). **Production fix required + made (decision #14):** loading a
> suspend workflow instantiates `SuspendExecutionActivity`, whose ctor built the Hangfire scheduler
> eagerly (`PersistenceExecution()` → `GetScheduler()`), forcing a live SQL connection just to *parse* a
> workflow on the engine's resume path — now lazy (each method already had the `?? GetScheduler()`
> fallback). **Deployment config:** the engine + JobProcessor `Settings\persistencesettingsdbsource.bite`
> now carry the real `hangfiredb` source (integrated-security `(local)\sqlexpress`; **no credentials** —
> decrypted from the operator's DPAPI source and confirmed secret-free), and the three demo workflows are
> staged under the engine `Resources\hangfiredemo\`.
> *Regression:* driver 43/43; the 11 `SuspendExecutionActivityTests` failures observed are **pre-existing
> environment/locale issues** (9 = ACL write to the machine `persistencesettings.json` when
> `webServerBasePath` is unset; 2 = a dd-MM-yyyy vs yyyy-MM-dd date-format mismatch) — proven identical on
> the un-modified ctor, so **not** caused by the lazy-ctor change.
> Suites green: engine 25/25 (incl. real scheduled + manual resume), driver seam 4/4, processor JobPoll 4/4.

---

## 0. Scope and non-goals

**In scope**
- `SuspendExecutionActivity` executing inside the Execution Engine (unchanged activity code).
- Replacement of `hangfireserver.exe` with a dedicated timer-driven Function App
  (`ExecutionEngineJobProcessor`) that polls Hangfire SQL storage and fire-and-forget invokes the
  engine's new secured resume route.
- `ManualResumptionActivity` working inside the engine with **behaviour-preserving synchronous
  semantics** (see §Phase 5).
- Key Vault AES-256-GCM (`WFAES::`) replacing Windows DPAPI for persisted job data.
- SQL Server job storage in production; LocalDB for development; `Hangfire.InMemory` for tests.

**Non-goals**
- No change to the on-prem Server / hangfireserver.exe path — it remains as-is.
- No replacement of Hangfire's SQL schema (jobs remain readable by existing tooling).
- No automatic retry of failed/timed-out jobs (**fail-only** — resolved decision).
- No Hangfire Dashboard hosting in Azure Functions (ops use SQL-connected dashboard or App
  Insights).

---

## 1. Key analysis results the plan is built on (evidence + confidence)

### 1.1 The driver is already deployed inside the engine
`Dev2.Activities.csproj` references `Warewolf.Driver.Persistence` (L556) and `Dev2.Runtime.Services`
(L540); the engine references `Dev2.Activities`. The whole persistence closure already compiles,
publishes, and cold-starts in the deployed Function App. *(High confidence — observed in csproj files.)*

### 1.2 Performance counters are the one sandbox blocker
`HangfireScheduler`'s ctor eagerly builds real Windows perf counters
(`HangfireScheduler.cs:90-95, 110-122`) via `PerformanceCounterCategory.Create`
(`Dev2.Infrastructure/PerformanceCounters/PerformanceCounterCategoryWrapper.cs:29`) — Windows-only,
admin-privileged, blocked by the App Service sandbox. **Decision: the engine uses no perf counters
at all** (Azure telemetry replaces them). Mechanism: runtime seam (recommended) — lazy counter
construction + engine registers a no-op `IWarewolfPerformanceCounterLocater`
(`EmptyCounter` already exists); the existing `CustomContainer.Get<> == null` check
(`HangfireScheduler.cs:420`) short-circuits. Conditional compilation (`#if` + build flavor) is the
documented fallback if the code must be physically absent. Server path untouched.
*(High confidence.)*

### 1.3 `ExecutingUser` — null is fatal at suspend, harmless elsewhere
- `SuspendExecutionActivity.cs:152` dereferences `_dataObject.ExecutingUser.Identity.Name` →
  **NullReferenceException** if unset. The engine currently never sets it (zero references in
  `Warewolf.Execution.Lightweight`). **Cannot be ignored — must be populated before suspend.**
- Normal engine execution is unaffected by null (auth bypassed in
  `AzureFunctionExecutionContainer.CanExecute → true`, audit falls back to
  `?? Thread.CurrentPrincipal`, `WfExecutionContainer.cs:76`).
- Resolution: populate `DsfDataObject.ExecutingUser` from the authenticated Entra principal
  (`WorkflowClaimsPrincipal : ClaimsPrincipal` → `IPrincipal`). Persist the **name only**
  (encrypted) — never the token (expires; storing tokens at rest is a security anti-pattern).
  On resume, rebuild `GenericPrincipal` from the name — exact parity with
  `HangfireScheduler.cs:221-226`. *(High confidence.)*

### 1.4 The three resume methods — synchronicity and fire-and-forget impact

| Method | Sync/async today | Executes continuation? | Fire-and-forget impact |
|---|---|---|---|
| `ResumeWorkflow` (`HangfireScheduler.cs:428-482`) | Sync inside a Hangfire worker | Yes, inline (L448) | **None** — its return value is consumed only by Hangfire; no workflow awaits it. Safe to replace with processor → engine fire-and-forget |
| `ResumeJob` (L228-324) | **Sync, blocking** — `workflowResume.Execute` (L282-283) completes before returning `"Success"` | Yes | **Must stay synchronous.** `ManualResumptionActivity`'s `Response`/error state feeds the next activity of the calling workflow; async ack would break workflow correctness |
| `ManualResumeWithOverrideJob` (L326-380) | Sync, fast | **No** — marks `ManuallyResumed` (final) only; continuation runs via `StartActivityId` set at `ManualResumptionActivity.cs:164`, consumed at `EsbServicesEndpoint.cs:250-257` *(ordering: medium confidence)* | State-write parity is trivial; the engine must execute the continuation **synchronously in the same request** via the shared executor |

**Resolved semantic decision:** manual resumption is **synchronous** in the new design (no async
ack). Only the scheduled path (processor-triggered) is fire-and-forget — and nothing observes its
result today, so no existing execution behaviour changes. *(High confidence on ResumeJob/
ResumeWorkflow; medium on override-continuation ordering — Phase 8 includes a parity test.)*

### 1.5 Duplicate prevention and the timeout question
- Existing: Hangfire's fetch lease (`SlidingInvisibilityTimeout = 5 min`,
  `Warewolf.HangfireServer/Program.cs:190, 202`) means a **crashed** worker's job becomes visible
  again and is **re-executed** (at-least-once). `[AutomaticRetry(Attempts = 0)]` suppresses only
  exception-based retries.
- New: **a timed-out job is NOT re-invoked.** The poller selects only due `Scheduled` jobs; a
  claimed job (`Processing`) is invisible to it. The reaper marks stale `Processing` jobs
  **`Failed`** (terminal) with reason + error logging + alert; humans decide re-scheduling / tier
  upgrade. Deliberate, safer deviation — the code itself warns *"re-queue with caution"*
  (`HangfireScheduler.cs:467-477`). Duplicate execution is structurally impossible: singleton
  timer + atomic CAS claim (`ChangeState(jobId, Processing, expected: Scheduled)`) + terminal
  states. **The CAS-to-`Processing` is net-new code, NOT existing parity** — today's resume guards
  are read-then-throw (TOCTOU) and the `ChangeState`-with-expected-state primitive is used elsewhere
  only to reach `Failed`/`ManuallyResumed`; nothing transitions a job *to* `Processing` today. This
  is a deliberate improvement, implemented in `ResumptionExecutor.TryClaim`. Its atomicity relies on
  SQL-level CAS (see the Phase 8 duplicate-prevention note — not testable on `MemoryStorage`).
  *(High confidence — the claim is new; the primitive is pre-existing.)*

---

## 2. Phases

### Phase 0 — Perf-counter removal + sandbox smoke test
1. Make counter construction lazy in `HangfireScheduler` (move `GetPerformanceCounter()` out of the
   ctor into the Server-only resume path).
2. Engine startup registers a no-op `IWarewolfPerformanceCounterLocater` (`EmptyCounter`-based)
   into `CustomContainer`.
3. Deployed smoke test: `new HangfireScheduler()` against a test SQL DB inside the sandbox —
   verifies no counter code executes.

**Acceptance:** driver constructs cleanly in the Azure sandbox; Server build/tests unchanged.

### Phase 1 — Encryption (Key Vault AES hooks)
4. New `FileEncryptionHelper` — AES-256-GCM encrypt with the `KeyVaultSecretManager` key, emitting
   `WFAES::Base64([12-byte nonce][ciphertext][16-byte tag])` — byte-format identical to
   `FileDecryptionHelper` / `Encrypt-Config.ps1`.
5. Register `DpapiWrapper.AesEncryptHook` at cold start beside the decrypt hook
   (`KeyVaultStartupExtensions.cs:47`). Fix the stale "AES-256-CBC" doc comment on the hook
   (`DPAPIWrapper.cs:31-36`).
6. Result: zero crypto changes in activities/driver — `Encrypt`/`Decrypt`/`CanBeDecrypted` route
   through the hooks (`DPAPIWrapper.cs:98-101, 131-132, 167-168`).

**Constraint (document):** DPAPI-suspended (on-prem) jobs cannot resume in Azure and vice-versa.

**Known limitation — no key version in the envelope.** The `WFAES::Base64([12-byte nonce]
[ciphertext][16-byte tag])` envelope carries **no key id/version**, so rotating the Key Vault AES
key invalidates decryption of every already-suspended (possibly months-old) job. A random 96-bit
nonce per operation is correctly enforced (`FileEncryptionHelper.cs`) so GCM nonce-reuse is not a
risk. Adding a key-version prefix is **out of scope here**: the envelope is a platform-wide interop
format shared with `Encrypt-Config.ps1`, `FileDecryptionHelper`, and every existing WFAES source
(secure.config, `ElasticsearchLoggingSource.bite`), so versioning it is a separate cross-cutting
change. Until then, treat Key Vault key rotation as a **breaking event for in-flight suspended
jobs** — drain/resume them before rotating (see `KeyRotationRunbook.md`).

### Phase 2 — Persistence settings
7. `Settings/persistencesettings.json` — Enable, scheduler=Hangfire, EncryptDataSource, reaper /
   budget knobs.
8. `Settings/persistencesettingsdbsource.bite` — Hangfire SQL `DbSource`; `ConnectionString`
   WFAES-encrypted and runtime-decrypted **exactly like** `Settings/ElasticsearchLoggingSource.bite`
   (same deploy-script encryption pass, same `AesDecryptHook` path —
   `ElasticsearchLoggingOptions.cs:79`).
9. Cold-start hydration of `Config.Persistence` via `PersistenceConfigLoader.Initialize()` —
   reads the two files from the app's deployed `Settings/` folder (`{AppContext.BaseDirectory}\Settings`)
   and **replaces** `Config.Persistence` with an in-memory `LightweightPersistenceSettings` (read-only;
   never written back — run-from-package safe). `PersistenceSettings.SettingsPath` (`Dev2.Common/Config.cs:87`),
   `Config.AppDataPath`, and `webServerBasePath` are **not** used. The JobProcessor runs the identical
   loader at its own cold start (`ProcessorStartup.InitializeAsync`).

### Phase 3 — Suspend inside the Execution Engine
10. **Populate `ExecutingUser`** on the engine's `DsfDataObject` from `WorkflowClaimsPrincipal`
    (mandatory — §1.3); name from claims (`preferred_username`/UPN for users, `appid` for daemons).
11. Verify/complete `ResourceID`, `VersionNumber`, `Environment` on the engine's data object;
    `SuspendExecutionActivity` returning `null` halts the executor's walk.
12. Extend persisted job values — keep the 5 legacy keys, add engine snapshot keys (workflow
    name/path, executionId, serialized partial `WorkflowExecutionResult`/`DebugStepResult`s).
13. Suspend HTTP response: outputs as of the suspend point, `Result` = SuspensionId.

### Phase 4 — `ExecutionEngineJobProcessor` (new dedicated Function App)
14. Project skeleton (Windows Consumption Y1, dotnet-isolated 8) mirroring the engine's setup;
    references `Warewolf.Driver.Persistence` for storage/state types; reads the Phase-2 settings.
15. `JobPollFunction` — `[TimerTrigger("%JOB_POLL_SCHEDULE%")]` (default 1 min, configurable):
    query storage for **due Scheduled** jobs (UTC comparison); for each, POST
    `{ENGINE_RESUME_BASEURL}/secure/resume/{jobId}` with an MI-acquired bearer token. The engine
    responds **synchronously** (200 = claimed + executed; 409 = already claimed, a benign duplicate);
    the processor **waits only a short ack window** (`%ENGINE_RESUME_TIMEOUT_SECONDS%`, default 15 s)
    and maps an elapsed window to a benign `AckTimeout` that reconciles next tick — it never blocks on
    workflow completion. Enumeration is `IMonitoringApi.ScheduledJobs` paged (100/page) with a
    client-side UTC `EnqueueAt` filter — `ScheduledJobs` returns *all* scheduled, not just due, so
    each tick pages the full Scheduled set; acceptable at expected volumes, revisit (indexed
    due-query) if the scheduled backlog grows large. CAS-ing a job to `Processing` with **no running
    `BackgroundJobServer`** (neither app hosts one) is intentional — it means Hangfire's own
    orphan-requeue never fires, so fail-only is enforced structurally and the reaper is the sole
    stale-recovery path.
16. `ReaperFunction` — stale `Processing` older than `%JOB_STALE_MINUTES%` → `Failed` with reason
    (*"exceeded execution budget (functionTimeout)"* where applicable) + audit + App Insights
    alert. **Fail-only** — never auto re-schedule.
17. Best practices baked in as acceptance criteria: claim-before-work CAS; singleton timer; each
    tick bounded well under the function timeout; idempotency keyed on jobId; structured events per
    state transition with `correlationId = SuspensionId`; no post-response work; short-lived SQL
    connections with transient retry + jitter; UTC everywhere (note the legacy local-time mix in
    `CalculateResumptionDate`, `HangfireScheduler.cs:387-389, 505-531` — keep parity, pin the
    processor's due-comparison to UTC); least-privilege MI + minimal SQL role.

### Phase 5 — Engine resume route + shared executor; manual resumption synchronous
18. `ResumptionExecutor` (engine): jobId → read args → decrypt via hooks → rebuild
    environment/principal → parse `.bite` → flatten chain → **skip to node
    `UniqueID == startActivityId`** → execute continuation — mirrors
    `ResumableExecutionContainer.EvalInner/FindActivity` (`WfExecutionContainer.cs:457-479`) on the
    lightweight pipeline. Separate resume mode/route; the normal execution path is untouched.
19. `WorkflowResumeFunction` — POST `/secure/resume/{suspensionId}`, under the `/secure` prefix so
    the full auth pipeline applies (`[RequireWorkflowPermission(Execute)]`; route in
    `RouteAuthorizationRegistry`; global-scope Execute row for role `Warewolf_JobProcessor` in
    `secure.config`; denials → 500 per WOLF-8418). Handler is **synchronous**: **503** if persistence
    disabled → **claim (CAS Scheduled→Processing; 404 unknown / 409 if lost)** → execute the
    continuation → record `Succeeded`/`Failed` (fail-only) + audit → respond **200** (with outputs)
    or **500** — an isolated-worker function cannot emit an early 202 and keep executing, and caller
    disconnects don't abort execution. Route stays open to human/manual callers via role-gating.
20. **Manual resumption — behaviour-preserving (per §1.4):**
    - No-override (`ResumeJob` parity): the driver seam invokes the **same handler/executor as the
      route, synchronously** (in-process preferred; HTTP self-call with wait as the uniform
      alternative). `Response = "Success"` only after the continuation completed; failures throw
      into the activity's error path. State guards + `ManuallyResumed` transition identical.
    - Override (`ManualResumeWithOverrideJob` parity): env merge + `OverrideDataFunc` run unchanged
      in the activity; the engine executes the continuation **synchronously in the same request**
      with the merged environment via the shared executor (engine equivalent of the
      `StartActivityId` / `EsbServicesEndpoint.cs:250-257` pipeline); then `ManuallyResumed`
      (final) with the encrypted merged environment.
    - Documented trade-off: synchronous manual resumption consumes the calling request's
      function-timeout budget; overruns error out with logging (fail-only; tier upgrade is a
      manual decision).
21. Merge the pre-suspend snapshot + post-resume outputs into the final `WorkflowExecutionResult`;
    version mismatch against the deployed `.bite` logs a warning (engine has no version catalog —
    accepted, documented difference).

### Phase 6 — Job storage environments
22. Production: Azure SQL / SQL Server via `Hangfire.SqlServer` — schema unchanged.
23. Local development: SQL Server Express **LocalDB** — connection-string-only change in the
    persistence DbSource; full provider fidelity, zero code.
24. Automated tests: **`Hangfire.MemoryStorage`** (the version already used by
    `Warewolf.Driver.Persistence.Tests`) injected via the internal test ctors — the driver's
    `HangfireScheduler(client, jobStorage, …)` and the engine's `ResumptionExecutor(logger,
    jobStorage, client, workflowsDirectory)`. Seed jobs must carry `[AutomaticRetry(Attempts = 0)]`
    (as the real `ResumeWorkflow` does) or Hangfire's global retry filter reschedules a `Failed`
    job back to `Scheduled` and masks the engine's fail-only recording.

### Phase 7 — Deployment + documentation sync
25. New deploy script (or mode) for the JobProcessor: app + system-assigned MI, app settings
    (`JOB_POLL_SCHEDULE`, `JOB_STALE_MINUTES`, `ENGINE_RESUME_BASEURL`), settings staging; role
    registration via the existing daemon flow (`Configure-WwExecutionAuth-Clients.ps1
    -ClientType Daemon -DaemonUseManagedIdentity`, role `Warewolf_JobProcessor`).
26. Engine deploy additions: stage + WFAES-encrypt `persistencesettingsdbsource.bite` (same pass as
    the Elasticsearch source), resume-route auth wiring, persistence app settings.
27. Documentation to update in lockstep: `Deploy-RunGuide.md`, **`Deploy-EndToEnd-Runbook.md`**
    (new JobProcessor section following its existing §4/§5 daemon pattern with
    `Warewolf_JobProcessor` + the two-part authorization contract), `Part1-Architecture.md`,
    `README-Encryption.md`, deployment script `Tests/`, and the `warewolf-architecture` /
    `warewolf-deploy` skills. This plan + the architecture doc live beside them in `docs/`.

### Phase 8 — Test scenarios (IMPLEMENTED 2026-07-22 — see the Phase 8 status note above)
Parity-driven: every scenario maps an existing behaviour to the new implementation. The matrix
below is the original plan; implementation status and the two deferred items are summarised in the
Phase 8 status note at the top of this document.

| Area | Scenarios |
|---|---|
| Suspend | All six `enSuspendOption` date calculations (parity with `CalculateResumptionDate`); `EncryptData` on/off → `WFAES::` values; persistence-disabled + missing-next-node errors (`ErrorResource` parity); `SaveDataFunc` runs when `AllowManualResumption`; `ExecutingUser` populated (no NRE) |
| State guards | Resume attempt on `Succeeded`/`ManuallyResumed`/`Enqueued`/`Processing` → same `ErrorResource` messages (`HangfireScheduler.cs:162-175` parity) |
| Duplicate prevention | Concurrent double-POST → exactly one 200 + one 409; overlapping ticks → single claim; not-yet-due never selected (UTC boundary); `Failed` cannot be claimed. **Must run on LocalDB / SqlServer, NOT `Hangfire.MemoryStorage`** — the whole claim guarantee rests on SQL-level CAS atomicity (`ChangeState` with expected old-state), which `MemoryStorage`/`InMemory` do not model faithfully; a passing InMemory concurrency test would be false confidence |
| Resume execution | Starts exactly at `startActivityId`, predecessors skipped; environment restore parity incl. override merge **order** (suspended env, then current env — `ManualResumptionActivity.cs:162-166`); `GenericPrincipal` rebuilt from persisted name; final result merges pre/post-suspend parts |
| Manual resumption | Both paths through the shared handler; **synchronous semantics asserted** — `Response` set only after continuation completes (no-override), correct values visible to the next activity; override-ordering parity vs Server (validates the §1.4 medium-confidence item); `ManuallyResumedState` written with encrypted env |
| Reaper & timeout | Stale `Processing` → `Failed` with budget-exceeded reason, error in logs/response, **no re-invocation by the poller** (explicit test); fail-only verified |
| Encryption | GCM round-trip; `Encrypt-Config.ps1` interop; DPAPI (Server) job in Azure → clean, explicit error (cross-host constraint) |
| Processor unit | Due-job query correctness; MI token client (mocked); env-var configuration (`%JOB_POLL_SCHEDULE%`, `%JOB_STALE_MINUTES%`) |
| End-to-end | In-process harness: suspend → poll tick → resume route → `Succeeded`, on `Hangfire.InMemory` and LocalDB |

---

## 3. Decisions log (resolved)

| # | Decision |
|---|---|
| 1 | Reaper is **fail-only** — no automatic retry/re-scheduling of failed or timed-out jobs |
| 2 | Function-timeout overruns **error out** (response + logging + alert); hosting-tier upgrade is a **manual team decision** |
| 3 | The resume route **stays open** for human/manual external callers via role-gating |
| 4 | Dedicated Function App **ExecutionEngineJobProcessor**; engine owns claim + execution + status recording |
| 5 | Timer cadence 1 min, configurable via `%JOB_POLL_SCHEDULE%` |
| 6 | Engine uses **no performance counters** (runtime seam; conditional compile as fallback) |
| 7 | **Manual resumption stays synchronous** (both paths); only the scheduled path is fire-and-forget |
| 8 | Persist principal **name** (encrypted), never tokens |
| 9 | Production storage SQL Server; dev LocalDB; tests `Hangfire.MemoryStorage` |
| 10 | `Config.Persistence` **hydration = in-memory load from the app's own `Settings/` folder** via `PersistenceConfigLoader.Initialize()` (`{AppContext.BaseDirectory}\Settings\persistencesettings.json` + `…dbsource.bite`), which **replaces** `Config.Persistence` with a `LightweightPersistenceSettings`. Neither "copy-to-`SettingsPath`" nor "`AppDataPath`/`webServerBasePath` override" is used; `PersistenceSettings.SettingsPath` is bypassed. Identical mechanism in the engine (`StartupOrchestrator`) and the processor (`ProcessorStartup`); settings are read-only and never written back (run-from-package safe) |
| 11 | `webServerBasePath` / `Config.AppDataPath` are **intentionally not used** by the engine or processor — settings load read-only from the deployed `Settings/` folder, so no writable app-data root is required and the Azure read-only-package-mount concern does not arise |
| 12 | The engine and processor deploys **each publish to their own directory** and never mutate the publish output: both scripts copy/extract the publish output into a **fresh temp staging dir** (`ww{executionengine\|jobprocessor}-stage-<AppName>-<stamp>`), stage secrets/settings there, zip **that**, upload it separately to their own Function App, then remove the staging dir on a successful real run (dry-run keeps a `-dryrun` preview). `Deploy-WwExecutionEngine.ps1 -DeployJobProcessor` resolves `-JobProcessorPublishPath` at plan time and **fails loudly** if it equals the engine `-PublishPath` (would otherwise zip engine binaries into the processor app) |
| 14 | **`PersistenceExecution()` builds its scheduler lazily** (not in the ctor). The XAML/activity parser instantiates every node — including `SuspendExecutionActivity`, which news up a `PersistenceExecution` — merely to LOAD a workflow, and the engine's resume path parses the `.bite` to find the continuation node. An eager ctor forced a live Hangfire SQL connection just to parse. Each method already resolves the scheduler via `?? GetScheduler()`, so behaviour is only deferred to first real suspend/resume, not changed (shared Server + engine code; driver 43/43 green after the change) |
| 13 | **Dual-host DB isolation.** The Azure engine + processor use a **separate Hangfire database** from the on-prem Server/`hangfireserver.exe`. The poller claims *every* due `Scheduled` job in its configured store and there is **no origin tag/filter** in code — so a *shared* DB during any coexistence window would let the Azure poller claim on-prem **DPAPI**-encrypted jobs (fail-decrypt → `Failed`, and symmetrically on-prem workers claim Azure **AES** jobs), and Phase-3 snapshot keys would reach on-prem `ResumeWorkflow` unexpectedly. Isolation is enforced operationally via each host's own persistence `ConnectionString`. If a shared DB is ever genuinely required, it must first gain an **origin tag on each job + per-poller origin filter** (code change, separate decision) — the DPAPI↔AES cross-host error (Phase 8 "Encryption" row) is the *safety net*, not a coexistence strategy |

## 4. Open items

| # | Item | Status |
|---|---|---|
| 1 | Override-mode continuation **ordering** on the Server (was medium confidence) | **Resolved by code-read (Phase 8b-3).** The env merge is *shared* activity code — `ManualResumptionActivity.cs:162-166` restores the suspended env then overlays the current env, identical on both hosts. Both hosts write `ManuallyResumed` **before** the override continuation executes: the driver's `ManualResumeWithOverrideJob` transitions state at `HangfireScheduler.cs:376`, *then* the continuation runs — on the engine via the `IResumptionExecutor` seam (`:383`), on the Server via the `StartActivityId` sub-execution (`EsbServicesEndpoint.cs:250-256`, reached after `ManualResumptionActivity` line 170). Engine ordering is pinned by `HangfireSchedulerResumeSeamTests`. A live FullServer runtime trace remains optional confirmation before final sign-off, but is no longer blocking. |
| 2 | In-process vs HTTP-self-call for the manual-resumption seam (behaviour identical by design) | Decide in Phase 5 detailed design |
| 3 | **Hosting tier.** Consumption (Y1) caps the engine's `functionTimeout` at 10 min — the ceiling for **both** a synchronous manual resume and any scheduled continuation — and a cold Consumption app can misfire/delay the processor's `TimerTrigger` heartbeat. Long-running suspend/resume workflows will hit this. Evaluate Premium / always-ready for one or both apps | Operational decision; not blocking Phases 0–7 |
| 4 | **Deploy/scale interruption is the common failure, not crashes.** Function Apps recycle on every deployment + scale event; under fail-only (decisions #1/#2) an in-flight scheduled resume interrupted by a deploy is reaped to `Failed` with no auto-redelivery (on-prem's 5-min `SlidingInvisibilityTimeout` auto-recovered it). Add a deploy-drain step and document the expected `Failed`-job volume during cutover; a queue-triggered resume (see below) would restore safe redelivery for non-terminal jobs only | Operational + optional re-architecture |
| 5 | **Operator surface for `Failed` jobs.** Fail-only assumes "a human re-schedules" but the Hangfire Dashboard is a non-goal in Azure — define the requeue/re-schedule tool (SQL-connected dashboard, script, or admin route) | Define before production cutover |
| 6 | **Queue-triggered resume (alternative to synchronous HTTP).** The current synchronous execute-then-respond model is correct (no post-response work — `WorkflowResumeFunction`), but a `[QueueTrigger]`/`[ServiceBusTrigger]` engine handler would additionally give guaranteed-complete-or-redeliver semantics for non-terminal jobs (mitigating open item #4) without reintroducing unsafe re-queue of terminal jobs. Larger change; evaluate against the operational cost of fail-only | Architectural decision; deferred |

## 5. Suggested sequencing

Critical path: **Phase 0 → 1 → 2**, then engine-side (3, 5) and processor-side (4) in parallel;
6 alongside; 7-8 close out. Each phase ends with its acceptance criteria green and the affected
docs/scripts updated (change-synchronization rule).
