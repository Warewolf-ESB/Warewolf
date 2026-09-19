# `Scripts/` — wwexecution provisioning toolkit

This folder is the operational source of truth for everything the
`Warewolf.Execution.Lightweight` function app needs at the Azure / Entra
control plane.

| Script                                        | Purpose                                                                 |
|-----------------------------------------------|-------------------------------------------------------------------------|
| `Deploy-WwExecutionEngine.ps1`                | **End-to-end deployment orchestrator** — runs every step in `docs/Deployment-Steps.txt` (RG → storage → Function App → App Insights → Entra/Easy Auth → publish, with optional Key Vault setup + resource encryption). Thin: reuses the scripts below. **Bound scale-out against your broker:** `-FunctionAppScaleLimit` caps Consumption instances. The RabbitMQ connection cache is keyed per broker endpoint, so a single-broker workload holds **one connection per instance** whatever `maxConnections` says — instance count is what exhausts a vhost's connection limit (2026-09-15: 48 instances vs a 40-connection cap dead-lettered the losers). **Concurrency caps need two flags, not one:** `-ServiceBusMaxConcurrentCalls` is inert on its own because Azure Functions' dynamic concurrency overrides every static `host.json` concurrency setting — pair it with `-DisableDynamicConcurrency` (the plan output warns if you don't). It **does not build**: publish first, and make sure `-PublishPath` holds a *fresh* publish — a stale directory silently ships an old engine (2026-09-15: a correct deploy shipped a pre-WOLF-8517 binary that dead-lettered 5,000/5,000 messages). |
| `Deploy-WwExecutionEngine.authconfig.example.json` | Template for the orchestrator's `-AuthConfigPath` (GroupPermissions + UserAssignments). Each GroupPermissions key becomes an MI-assignable app role; the `Warewolf_ClientApps` entry is the dedicated role for app-only client apps (daemon/MI callers get it via `Configure-WwExecutionAuth-Clients.ps1 -AppRolesToAssign`, and `secure.config` must grant the matching `WindowsGroup` Execute on the workflows they call). |
| `persistencesettings.uat.json`                | **Git-tracked `Enable: true` override** for `-PersistenceSettingsPath` when deploying to `WarewolfServer-UAT` — identical to the repo default (`Settings/persistencesettings.json`) except persistence is turned on. Consumed by `Dev/.azure/pipeline-LOADTEST.yml`'s `Deploy_UAT` stage, which copies it to a file literally named `persistencesettings.json` first (the orchestrator validates the source file's exact leaf name). See `docs/Deploy-UAT-Redeploy-Spec.md` §14 for why this is mandatory on every full UAT redeploy. |
| `persistencesettingsdbsource.uat.bite`        | **Git-tracked** `-PersistenceDbSourcePath` for `WarewolfServer-UAT` — a `DbSource` pointing at the dedicated Hangfire job-store database `wwexecution-uat-hangfire`. Its `ConnectionString` is WFAES-encrypted at rest (decryptable only via this app's own Key Vault key at runtime), same precedent as the already-committed `Settings/ElasticsearchLoggingSource.bite`, so it's safe to commit despite carrying a real DB credential. Consumed by `Deploy_UAT`, which copies it to `persistencesettingsdbsource.bite` first (exact leaf name required). |
| `Deploy-WwJobProcessor.ps1`                   | **ExecutionEngineJobProcessor deploy orchestrator** — provisions the poller/reaper Function App (RG → storage → Function App + system-assigned MI → App Insights → Key Vault wiring → stage + WFAES-encrypt the persistence settings pair → app settings → publish). Standalone, or invoked by `Deploy-WwExecutionEngine.ps1 -DeployJobProcessor`. Same *params-first, prompt-if-missing*, `-DryRun`, masked-summary + transcript conventions. The persistence source files (`persistencesettings.json`, `persistencesettingsdbsource.bite`) are **prompted when not passed**. Role assignment (`Warewolf_JobProcessor`) is a separate step via `Configure-WwExecutionAuth-Clients.ps1` — see the runbook. |
| `Deploy-WwExecutionServiceBusWorker.ps1`      | **Service Bus worker deploy orchestrator ("shovel bridge")** — provisions the Service Bus-triggered Function App published from `Warewolf.Execution.ServiceBusWorker` (RG → storage → Function App + system-assigned MI → App Insights → Service Bus namespace/queue with dead-lettering → RBAC or SAS listen auth → app settings → publish). Also provisions a queue-scoped **Send-only SAS rule** (`shovel-send` by default) — the credential a RabbitMQ Shovel plugin uses as its AMQP 1.0 destination to bridge RabbitMQ messages into this queue. `-ServiceBusQueueName` is live-wired to the worker's `[ServiceBusTrigger]` binding via the `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE` app setting (not just queue provisioning), and four `-ServiceBusTrigger*` parameters (`MaxConcurrentCalls`/`PrefetchCount`/`MaxAutoLockRenewalMinutes`/`AutoCompleteMessages`) tune the trigger's binding via the `AzureFunctionsJobHost__extensions__serviceBus__*` app-setting override convention, defaulting to `host.json`'s committed values. Same *params-first, prompt-if-missing*, `-DryRun`, masked-summary + transcript conventions as `Deploy-WwJobProcessor.ps1`. No dedicated Rollback companion (same as the JobProcessor) — the run summary JSON records what to tear down manually. Role assignment (`Warewolf_ClientApps`) is a separate step — see `docs/KB-ClientApps-Configuration.md` §2.6. Can also be chained automatically from `Deploy-WwExecutionEngine.ps1 -DeployServiceBusWorker` (see below) — note the companion form does not forward `-ServiceBusQueueName`/`-ServiceBusTrigger*`, only the standalone form does. |
| `Configure-RabbitMqShovel.ps1`                | **RabbitMQ → Service Bus shovel bridge configurator** — configures a dynamic RabbitMQ Shovel (via the RabbitMQ Management HTTP API, `PUT /api/parameters/shovel/{vhost}/{name}`) that forwards messages from an existing RabbitMQ source queue (AMQP 0.9.1) to the Service Bus queue provisioned by `Deploy-WwExecutionServiceBusWorker.ps1` (AMQP 1.0), per Microsoft's documented RabbitMQ-to-Service-Bus bridging pattern. Fetches the destination SAS key live via `az` (or accepts it directly as a SecureString) — never writes it to disk. Verifies the source queue exists and polls the shovel's running state after applying. Requires the `rabbitmq_shovel`/`rabbitmq_shovel_management` plugins to already be enabled on the broker (one-time, broker-host admin action — the script probes for this and gives the exact `rabbitmq-plugins enable` instruction if missing). Same conventions: params-first/prompt-if-missing, `-DryRun`, masked summary + transcript, `-LoadFunctionsOnly` test hook. Validated against a live `rabbitmq:3-management` Docker container (see docs/ShovelBridge-Architecture.md). |
| `Monitor-RabbitMqShovel.ps1`                  | **Shovel health monitor** — a standalone script the customer/operator schedules (Task Scheduler, cron, Azure Automation runbook, etc.) since RabbitMQ is customer/on-prem infrastructure, not something a serverless Azure Function can reliably poll. Polls `GET /api/shovels/{vhost}` for the named shovel; `running` = healthy (exit 0), anything else or missing = unhealthy (throws, non-zero exit). Optionally emits a `ShovelHealthCheck` Application Insights custom event via the plain HTTP `/v2/track` ingestion API (no SDK) — a failure event on every unhealthy check, plus an optional heartbeat event on healthy checks via `-SendHeartbeatOnHealthy` — so an Azure Monitor alert rule can watch `customEvents` for `healthy == "false"` (or a heartbeat gap). Read-only against RabbitMQ (GET only); never mutates broker state. Same conventions: params-first/prompt-if-missing, `-DryRun`, masked summary + transcript, `-LoadFunctionsOnly` test hook. |
| `Enable-ServiceBusSecureTrigger.ps1`         | **Turns the engine's in-process Service Bus trigger on for an ALREADY-DEPLOYED app** — idempotently creates/converges the trigger queue (dead-lettering, `-MaxDeliveryCount`, `-LockDuration`), grants the app's system-assigned managed identity **Azure Service Bus Data Receiver** on the namespace, and sets the one app setting the binding needs (`ServiceBusConnection__fullyQualifiedNamespace`). Without the last two the trigger never fires and bridged messages sit on the queue untouched — a failure that looks exactly like a broken shovel and is not one (build 30756, `wwprofilerlw`). Enforces the WOLF-8512 delivery-budget invariant (`MaxDeliveryCount x LockDuration` must exceed `functionTimeout + 1min`) by reading `../host.json`, so it must be run from a folder one level under one — the published scripts zip does not ship `host.json`, which is why the profiler pipeline stages one out of the engine package first. It does **not** stage `Settings/servicebustrigger.bite` (that is `Deploy-WwExecutionEngine.ps1`'s `-ServiceBusTriggerQueueName`/`-ServiceBusTriggerWorkflowName` at deploy time; the original Kudu VFS write cannot work under `WEBSITE_RUN_FROM_PACKAGE=1`), and it does **not** grant *Data Sender* — that grant is the trigger's entire authorization boundary and is left to the operator to scope. **Run automatically by `ProfilerService-CI-UAT-LightweightExecutionEngine.yml` against `wwprofilerlw` only**; a manual, reviewed step for every other app. Automating it costs a privilege: Contributor cannot write role assignments, so that pipeline's SP holds ABAC-conditioned `Role Based Access Control Administrator` on DEV2, limited to the Data Receiver role. `-DryRun`, `-NonInteractive`, `-LoadFunctionsOnly` test hook. |
| `Test-WwSinkDelivery.ps1`                     | **Sink delivery assertion** — proves a workflow run actually published to its RabbitMQ sink queue, for the post-deploy smoke test in `Dev/.azure/ProfilerService-CI-UAT-LightweightExecutionEngine.yml`. Asserts on the queue's DEPTH DELTA across the run (`-ReportDepthOnly` captures the before value), because the management API's `POST .../get` serves from the queue HEAD while a just-published message is at the TAIL — peeking 50 of a 19,100-deep backlog failed a deploy whose publish had succeeded (build 30750). When the queue is at or below `-PeekThreshold` the payload is also searched for the run's own token, upgrading the result to run-attributable. Read-only: GET, plus at most a `reject_requeue_true` peek that returns every message it reads (`ack_requeue_true`, despite the name, CONSUMES on LavinMQ). `-LoadFunctionsOnly` test hook; pwsh 7+ only (5.1 cannot survive CloudAMQP's TLS renegotiation). |
| `Provision-WwPersistenceDatabase.ps1`         | **Hangfire persistence database, provisioned or CHECKED.** Creates the Azure SQL database engine persistence needs (serverless GP by default) plus the allow-Azure-services firewall rule, on the SQL server the environment's own DbSource already names — so no new server or credential is introduced. `-CheckOnly` is the read-only half the Profiler pipelines' preflight mode runs, reporting in their BLOCKER / EXPECTED-LATER / REPORT vocabulary: it resolves the server from the connection string's FQDN (never from the hostname's first label — the resource group is not derivable at all), probes permission with an ARM `what-if` against exactly the resource the real run would create, and checks the pivot — whether the engine's SQL login **is** the server administrator. That last one decides whether the pipeline can finish the job: `az sql db create` leaves a database only the admin can log into, any other login needs a `CREATE USER … FOR LOGIN` **inside** it, and the Profiler agents carry no SQL client (this repo's own `Invoke-WwSqlQuery` shells to Windows PowerShell 5.1 for `System.Data.SqlClient`, so it is Windows-only). A mismatch therefore fails as a BLOCKER carrying the exact statement to run once by hand. Idempotent — an existing database is never re-created or re-scaled, and nothing here drops or alters anything. `-LoadFunctionsOnly` test hook. |
| `Test-WwShovelBridgeDelivery.ps1`             | **ShovelBridge delivery assertion** — proves a DEPLOYED bridge actually carries a message, for the post-deploy smoke test in `Dev/.azure/ProfilerService-CI-UAT-LightweightExecutionEngine.yml`. Publishes ONE `{"inputs":{…}}` message to the RabbitMQ source queue over the management API (`POST /api/exchanges/{vhost}/amq.default/publish`, `delivery_mode` 2 — never AMQP: RabbitMQ.Client 7 stack-overflows on teardown under .NET 10), then asserts three things in order: the source queue returns to baseline (the **shovel** consumed it), `deadLetterMessageCount` does **not** rise (the trigger **accepted** it — a rise is the WOLF-8517 contract-mismatch signature, where the bridge is perfectly healthy and every message is discarded), and `activeMessageCount` returns to 0 (the trigger settled the delivery). The dead-letter check is deliberately evaluated FIRST, because a dead-lettered message also drains both queues and would otherwise read as a pass. Exists because everything else in that pipeline passes over a completely dead bridge — the HTTP smoke test never touches it, and `Configure-RabbitMqShovel.ps1`'s Phase 3 only warns (see the 2026-09-18 `wwprofilerlw` entry in `docs/ShovelBridge-Architecture.md`). Service Bus counts come from `az servicebus queue show`, so the caller must be az-authenticated (AzureCLI@2); omit the namespace/queue/RG set and only the shovel leg is asserted — which the script then says out loud rather than implying a fuller proof. Pair it with `Test-WwSinkDelivery.ps1` on the same `-CorrelationToken` to cover the workflow itself. `-LoadFunctionsOnly` test hook.  |
| `Deploy-WwQueueProcessor.ps1`                 | **RabbitMQ QueueProcessor deployment** — Azure Container Apps, **one app per queue-trigger**, autoscaled 0→N by the KEDA `rabbitmq` scaler. Pointed at a trigger file, a folder of trigger files, or a manifest; derives `maxReplicas` from the trigger's `Concurrency` and the KEDA target from its `Prefetch`. Replaces `N × QueueWorker.exe` for the Azure path (on-prem unchanged). Before the (~6-minute) ACR build it asserts the publish output's .NET major matches the Dockerfile's base-image major — a mismatch there surfaces only as ACA `CrashLoopBackOff`, because the container exits before it opens a RabbitMQ connection. Both sides are currently **.NET 10** (`net10.0` project, `mcr.microsoft.com/dotnet/aspnet:10.0` base image). |
| `Deploy-WwEngineAndQueueProcessor.ps1`        | **Combined engine + QueueProcessor deploy** — a thin manual wrapper that runs `Deploy-WwExecutionEngine.ps1`, reads back the two files that deploy produces (the run `*.summary.json` and `Configure-WwExecutionAuth.output.json`), asserts them, and wires `Deploy-WwQueueProcessor.ps1` from the result. Exists because the values passing between the two deploys — engine endpoint, Entra `ClientId`, App Insights connection string, broker secret uri — are **produced by the first and required by the second**; a worker deployed with an empty `-EngineResourceAppId` starts, consumes messages and fails every engine call. Asserts the summary is `status=completed` and not a dry run, and that the auth output (**one fixed file, overwritten by every engine deploy**) belongs to this app. Emits a consolidated `deploy-both-<stamp>.handover.json` that the app-role grant, the token scripts and `Rollback-WwExecutionEngine.ps1 -SummaryPath` all read. Placeholder variable block; `y/N` gate before each real deploy; `-SkipEngine` / `-SkipQueueProcessor` / `-DryRun` / `-NonInteractive`. Not to be combined with `Deploy-WwExecutionEngine.ps1 -DeployRabbitMqTriggers` — that is the same job done in one call, without the assertions. See [`docs/Deploy-Both-RunGuide.md`](../docs/Deploy-Both-RunGuide.md). |
| `Tests/Deploy-WwEngineAndQueueProcessor.Tests.ps1` | Pester 5 suite (53 tests) for the combined wrapper via `-LoadFunctionsOnly` — placeholder guards, zip extraction and the stray-`.bite` package guard, both splat builders validated against the real orchestrator `param()` blocks, every capture assertion (`status` ≠ completed, dry-run summary, mismatched `appName`, **stale auth output from a previous deploy**, missing `ClientId`), and the handover file's shape plus the guarantee that it never records a secret value. Runs offline. |
| `Invoke-WwEnginePreWarm.ps1`                  | **Consumption-plan engine warm-up**, serving two callers with two auth shapes: `pipeline-LOADTEST.yml`'s `ShovelBridgeLoadTest_ExternalServiceBus` job passes an already-minted `-EngineBaseUrl`/`-AccessToken`; `Invoke-WwQueueLoadTest.ps1` (queue-processor path) passes `-EngineAppName`/`-EngineAppId` (+ `-ResourceGroup`/`-TenantId`) and the script mints its own token via `WwE2E.Common.psm1`'s `Get-E2EEngineToken`. Calls a workflow's `Secure/{route}` endpoint sequentially until latency stabilizes (Phase A — proves one instance is warm), then **ramps** across `-ConcurrentRounds` rounds to `-TargetConcurrency` (Phase B — forces the Consumption plan to scale out to roughly the instance count a coming burst needs). The ladder is `ceil(Target / 2^(Rounds - i))` — `20` over 3 rounds is `5 → 10 → 20` — and the **last** round is always the full target, so the final-round verdict still means "clean at target concurrency". Opening straight at the target was itself a cold-start trigger: the 2026-08-24 `pipeline-LOADTEST` run recorded `OK=22/60` at a 41.7 s median. Exists to prevent the cold-start/scale-out failure signature root-caused in `docs/ShovelBridge-Architecture.md`'s 2026-08-16 entry (Service Bus `MessageLockLost`, Roslyn `OutOfMemoryException`, cascading token-validation cancellations). **`-InputParameters` is REQUIRED for any route other than the default `rabbit/RabbitProcess`.** The built-in body carries only `message` — RabbitProcess's sole DataList input — so against any other workflow it binds nothing that workflow declares and every call fails on that workflow's own missing variables: a real-looking failure that says nothing about engine warmth. It has now caused two false diagnoses against the profiler's `RequestForProfiling` (2026-09-14, 119/119 "Invalid Calculate"; build 30690, `Scalar value { MandateDRI } is NULL`). A `{guid}` token in any string value is replaced with a **fresh GUID per call** — use it for fields keying an upsert (e.g. the profiler's `MandateId`, which keys `usp_InsertProfilerRecommendation` on `(MandateId, ForPaymentCycle)`) so warm-up calls do not all collapse onto one hot row. It is **not** `Test-ShovelBridgeE2E`'s `{correlationGuid}` message-template token, which is expanded by the burst publisher and arrives here as a literal. `-DryRun` prints the phase plan and the ramp ladder without any HTTP call. **Always exits 0 once parameters validate** — an incomplete warm-up only warns; a per-call `-TimeoutSec` expiry is a counted `TIMEOUT` result, not an error (before 2026-08-24 one such timeout killed the whole pipeline step, because `-SkipHttpErrorCheck` suppresses non-2xx *status codes* only). `-LoadFunctionsOnly` test hook for Pester. |
| `Rollback-WwExecutionEngine.ps1`              | **Teardown companion** — deletes ONLY what a deploy run created (summary-/tag-driven), in dependency order, with a leak check. Existing resources are preserved. |
| `New-WwE2EStaging.ps1`                        | **E2E harness — staging.** Builds a disposable staging tree (settings, generated triggers + broker source, optional `dotnet publish`) from repo templates, stamps a unique run suffix, validates readiness and emits `staging-manifest.json`. Only the Warewolf licence and broker credentials are hand-supplied; the generated source is **plaintext** so DPAPI never blocks the Linux worker. See [`docs/E2E-Harness-README.md`](../docs/E2E-Harness-README.md). |
| `Invoke-WwE2EVerification.ps1`                | **E2E harness — orchestrator.** Consumes the manifest, then deploys engine + workers, pre-creates broker topology, publishes real messages, watches KEDA scale 0→N→0, runs the drain and unacked-visibility probes, and **scores 18 acceptance criteria** to JSON + markdown. `-DryRun` by default (`-Execute` to deploy); teardown is opt-in via `-TeardownWhenDone`. Exit code 0 = all criteria passed. |
| `WwE2E.Common.psm1`                           | Shared helpers for the harness — AMQP topology/depth (with the `System.Threading.RateLimiting` shared-framework load), client-credentials engine tokens, Log Analytics evidence queries, and an `az` wrapper that **refuses `--query`** because cmd.exe mangles JMESPath. Sets `AZURE_EXTENSION_USE_DYNAMIC_INSTALL=yes_without_prompt` **at module load**: `az monitor log-analytics query` lives in an extension, and without this az stops to ask permission to install it — on stdin, with stderr suppressed — which hung a reviewer's run for 30+ minutes at the last step. |
| `Invoke-WwQueueLoadTest.ps1`                  | **Queue load test — the reviewer entry point.** `-MessageCount` sizes the run. Verifies Azure login, then Engine / ACA / KEDA / broker / database and **stops on blockers** before publishing (a competing consumer, a KEDA rule on the wrong queue, a stopped engine — each of which otherwise produces a result that looks like a product defect). Takes a `MAX(JobLogId)` watermark instead of truncating, pre-warms, publishes, waits for **rows** to stop rising rather than for the queue to empty, then reconciles the manifest against worker logs, engine logs, the database and the DLQ into **four buckets** that must sum to the published count. Exit code carries the verdict. Phase 0 verifies the required **az extensions** (`containerapp`, `log-analytics`, `application-insights`) and names the step each missing one blocks; every run writes a **transcript** to `<OutputDir>\<runLabel>.log` started before Phase 0, and `-StepTimeoutSeconds` bounds the report step so a blocked call fails the run rather than hanging it. Every default reproduces RUN 2 (100/100, 0 dead-lettered). See [`docs/LoadTest-Guide.md`](../docs/LoadTest-Guide.md). |
| `WwLoadTest.Defaults.psd1`                    | RUN 2 defaults for the load test, pre-filled at every prompt. Loaded with `Import-PowerShellDataFile` (parsed as **data**, so code slipped into it fails to load rather than running). **Contains nothing secret** — the broker password comes from the source `.bite` and the SQL connection string from `-SqlConnectionString` / `$env:WWLOADTEST_SQLCONNECTION` / a masked prompt. |
| `Publish-WwQueueBurst.ps1`                    | Publishes a burst and writes the manifest that turns the run report from a *count* into a *reconciliation*. Stamps a unique AMQP `CorrelationId` per message — the only key present on **every** worker log line including dead-letter lines, and the only handle on a deliberate-failure message whose body is empty by design. Builds and validates the manifest **before** connecting, and writes only what was actually published. Publishes to the exchange **named after the queue** with an empty routing key, matching `PublishRabbitMQActivity` — and because a missing exchange makes that publish silently unroutable, it now **ensures** the exchange/queue/binding first (`Confirm-E2EQueueTopology`; `-SkipTopologyEnsure` opts out). |
| `Invoke-WwEnginePreWarm.ps1`                  | Two-phase warm-up for a Consumption-plan engine: sequential until latency settles (measured cold start **64,757 ms** → ~3,100 ms warm), then **ramping** to the target concurrency to force scale-out. Every 502/503/504 in RUN 1 came from that cold window. **Executes the real workflow**, so take the row watermark after it, not before. Non-fatal by contract — read the verdict, not the exit code. |
| `Get-WwQueueRunReport.ps1`                    | **Per-message run report** — read-only and **time-window driven** (`-LastMinutes`, or `-StartUtc`/`-EndUtc`), so it re-renders any past burst still inside workspace retention rather than needing to watch one live. Reconstructs each delivery from `ContainerAppConsoleLogs_CL` into `txn → replica → revision → startedUtc → durationMs → outcome`, plus replica distribution, per-queue percentiles, reliability counts and a CSV + JSON. With `-ExpectedManifest` it **reconciles** against what was published (missing / duplicated / wrong-outcome) instead of merely counting what appears in the logs. `-IncludeEngine` joins to the engine's App Insights on `ExecutionId`. The **`EngStatus`/`EngBody`** columns carry the HTTP status and response body the engine returned **as the worker saw them**, parsed from the container logs — so a `DeadLettered(acked)` row shows *why* without needing App Insights at all (a 401/403 there can never be explained engine-side: the request never reached the app). |
| `Run-WwProfilerExactLoad.ps1`                 | **Control load run — the clean measurement.** Where `Invoke-WwQueueLoadTest.ps1` publishes the *randomised* profiler body, this publishes the one body proven to execute cleanly, so a throughput number is not contaminated by `Profiler.usp_GetPublicHolidaysInDateRange` not existing in `WarewolfDevOpsTestDb` (that combination returns 200, is acked, and leaves **no row** — it disappears silently). Only `MandateId` varies, because it is the reconciliation key. Takes a `MAX(JobLogId)` watermark **before** publishing and **again after the pre-warm**, publishes via `Publish-WwProfilerExact.ps1`, drains on **two independent clocks** (queue empty *and* rows quiet — an empty queue is never the signal, since 2xx→ack and non-2xx→dead-letter+ack both drain it), then reconciles the manifest against `dbo.jobs1`. Pre-warms the **workflow route derived from `-Url`** by default, with inputs parsed from the same `-Url` — `Public/apis.json` is discovery-only and never populates `WorkflowExecutor._workflowPool`, so it leaves the first real message paying the XAML compile and activity parse. `-PreWarmConcurrency` (default **2**) is forwarded as `-TargetConcurrency`: the script previously passed none and silently inherited the pre-warm script's **6**, the documented main source of the Y1 `Insufficient memory` OOMs. A workflow route that resolves to zero inputs **throws** rather than warming an empty datalist (119/119 silent failures on 2026-09-14, swallowed because the pre-warm is non-fatal by design). `-DrainQuietSeconds` must exceed the Service Bus `lockDuration` (PT5M) or a redelivery gap reads as completion — the default **45** is far too short for a run long enough to trigger instance recycling. `-LoadFunctionsOnly` test hook. |
| `Publish-WwProfilerExact.ps1`                 | Publishes N exact-shape profiler messages and writes the manifest the reconciliation depends on. Wraps the fixture in the engine's documented `{"inputs":{…}}` envelope — a **flat** body is the trap this guards, and it fails differently per engine build (pre-WOLF-8517 dead-letters it as `Malformed`; current builds accept it but bind **nothing**, so the workflow runs against an empty datalist and fails on its own missing variables). Builds and validates the manifest **before** connecting, and writes only what was actually published. Runs the publish in an isolated **child process**: RabbitMQ.Client 7.0.0 on .NET 10 reliably crashes during connection teardown, so the result and manifest are both written *ahead* of the close calls and the launcher reads the result file rather than the exit code. **Ensures the exchange/queue/binding before publishing**, and with `-RabbitMqManagementUri` **proves** the burst against the broker's own `message_stats.publish` counter instead of its own loop count — on 2026-09-19 a queue declared without its same-named exchange swallowed 20,000 messages while the script reported every one as published. The counter is polled (`Wait-E2EPublishDelta`), because the management stats database refreshes on an interval rather than synchronously with the publish. |
| `Measure-WwProfilerRun.ps1`                   | Post-hoc analysis of a completed exact-load run — reads the run's manifest and `dbo.jobs1` rows and reports the per-message reconciliation without re-running anything. |
| `Tests/Invoke-WwQueueLoadTest.Tests.ps1`      | Pester 5 suite for the load test's decision logic via `-LoadFunctionsOnly` — concurrency arithmetic, four-bucket classification, verdict rules, pre-flight blockers, connection-string masking, percentiles. Includes four regressions: **numeric parameters must be nullable** (a bare `[int]` defaults to `0`, indistinguishable from an explicit `0`, so every unbound int silently overrode its default with zero); **a run that published nothing must FAIL** (every "no failures" condition is vacuously true on an empty run); **no `[datetime]::TryParse` with a `[ref]` on an untyped variable** (PowerShell cannot bind it — asserted against the AST, since the helper's doc comment names the broken pattern deliberately); **warm-up traffic must be excluded** (two watermarks, the reported pre-warm row count, the report-window clamp, and no row deletion anywhere); **a failure cause must be diagnosed, never assumed** (`Resolve-E2EBrokerUri` returns `$null` for six reasons — a mistyped filename was being blamed on WFAES encryption, sending the reader after a Key Vault problem that did not exist); **reconciliation must be scoped to this run's label** (a shared `-OutputDir` merged two runs' manifests and reported a flawless 1000/1000 run as `FAIL — 20 LOST`); and **`TRY_CONVERT` style 127 requires the `T` separator** (a space-separated timestamp column silently yielded NULL, blanking every latency figure). Runs offline. |
| `Tests/Publish-WwQueueBurst.Tests.ps1`        | Pester 5 suite for the manifest invariants the reconciliation depends on — unique transaction ids across a 100-message burst, distinct bodies (identical ones make duplicate *execution* invisible in the database), empty failure bodies, and the parameter guards. Runs offline. |
| `Tests/Run-WwProfilerExactLoad.Tests.ps1`     | Pester 5 suite for the control load run's pure helpers via `-LoadFunctionsOnly` (`Get-WwProfilerFieldsFromUrl`, `Get-WwProfilerPreWarmRoute`) plus four regressions, all for defects that were **silent** — each made a run report a successful warm-up that had warmed nothing: **`-PreWarmConcurrency` must default to 2**, not silently inherit the pre-warm script's 6 (the documented Y1 OOM trigger); **`-PreWarmRoute` must default to empty** so it derives the *workflow* route from `-Url`, because the previous `Public/apis.json` default is discovery-only and never populates the per-workflow pool; **the empty-datalist guard must require both a non-discovery route and zero inputs**; and **the run summary must record `preWarmRoute`/`preWarmInputCount`/`preWarmConcurrency`**, without which two runs are not comparable. Empty query values (`RequestType=`, `FASGroup=`) are asserted as *kept*, since dropping them warms a different workflow shape than the publish sends. Runs offline — no engine, no broker, no SQL. |
| `Tests/Provision-WwPersistenceDatabase.Tests.ps1` | Pester 5 suite for the provisioner's DECISIONS, not its az plumbing: FQDN→server resolution (case, trailing dot, no match, and ambiguity throwing rather than picking one), the admin-login pivot and the remediation text it carries, verdict classification (a failed BLOCKER fails the run; an EXPECTED-LATER absence must NOT, or the first preflight against a bare environment reads as a broken pipeline), serverless-vs-provisioned SKU assembly, the what-if template describing exactly the target resource, authorization-failure recognition (so an unrelated error is never reported as a permissions problem), and that a password cannot reach a summary. Runs offline. |
| `Tests/Publish-WwProfilerExact.Tests.ps1`     | Pester 5 suite for the exact-shape publisher — the `{"inputs":{…}}` envelope, manifest invariants (unique txn ids and unique `MandateId`s, since a collision in either makes duplicate *execution* invisible), and the child-process launcher contract. Runs offline. |
| `Tests/Deploy-WwExecutionEngine.Tests.ps1`    | Pester 5 suite for the orchestrator (helpers + DryRun end-to-end). Covers both `host.json` concurrency mutations and the asymmetry between them: `Update-HostJsonServiceBusConcurrency` **no-ops** when `extensions.serviceBus` is absent (absent means "binding not configured"), while `Update-HostJsonDynamicConcurrency` **creates** a missing `concurrency` block (absent means "inherit the host default", and the point of the call is to pin it). Also asserts the plan-time warning that fires when `-ServiceBusMaxConcurrentCalls` is passed *without* `-DisableDynamicConcurrency` — the trap that let a requested concurrency of 2 run at 170. Run: `Invoke-Pester -Path ./Tests/Deploy-WwExecutionEngine.Tests.ps1`. |
| `Tests/Deploy-WwJobProcessor.Tests.ps1`       | Pester 5 suite for the JobProcessor orchestrator (static/ValidateSet, helpers via `-LoadFunctionsOnly`, DryRun end-to-end with az-shim, persistence-pair staging + fail-loud prompt validation). Run: `Invoke-Pester -Path ./Tests/Deploy-WwJobProcessor.Tests.ps1`. |
| `Tests/Configure-RabbitMqShovel.Tests.ps1`    | Pester 5 suite for the shovel configurator (AMQP URI builders, plugin-probe warning, DryRun end-to-end with a shimmed RabbitMQ management API). |
| `Tests/Test-WwSinkDelivery.Tests.ps1`         | Pester 5 suite (19 tests) for the sink delivery assertion — attributable/non-attributable confirmations, the peek being skipped above `-PeekThreshold`, the `-DepthBefore` sentinel refusing to pass silently, empty-password and unreachable-broker guards, and `%2f` vhost encoding. All broker calls shimmed. |
| `Tests/Test-WwShovelBridgeDelivery.Tests.ps1` | Pester 5 suite (27 tests) for the ShovelBridge delivery assertion — the `{"inputs":{…}}` envelope (including that empty fields survive and the body is byte-stable), the publish request shape, and every verdict branch. The two that matter most are regressions for real incidents and must fail DIFFERENTLY: a source queue that never drains names the **Shovel** leg and the `src-uri` (2026-09-18, `wwprofilerlw`), while a rising dead-letter count names the **Engine** leg (WOLF-8517). Both `Invoke-RestMethod` and `az` are shimmed, so it runs offline. |
| `Tests/Monitor-RabbitMqShovel.Tests.ps1`      | Pester 5 suite for the shovel health monitor (`ConvertFrom-AppInsightsConnectionString` parsing, healthy/unhealthy/missing-shovel DryRun end-to-end with a shimmed RabbitMQ management API, telemetry-echo payload shape, secret masking). |
| `Tests/Integration/Test-ShovelBridgeE2E.ps1` | **RabbitMQ → Shovel → Service Bus end-to-end test** — proves the bridge works against real containers/brokers (not mocks): brings up RabbitMQ (`-RabbitMqMode Container`, the default: a fresh `rabbitmq:3-management` container with shovel + management plugins pre-baked into a bind-mounted `enabled_plugins` file so the node only ever boots once; `-RabbitMqMode External`: an already-running, caller-supplied broker reached via `-ExternalRabbitMqManagementUri`/`-ExternalRabbitMqUsername`/`-ExternalRabbitMqPassword` — no docker needed for the RabbitMQ side), stands up the destination (`-DestinationMode Emulator`: local Azure Service Bus emulator + its Azure SQL Edge metadata-store dependency; `-DestinationMode ExternalServiceBus`: a caller-provisioned real namespace/queue), configures the shovel by dot-sourcing `Configure-RabbitMqShovel.ps1 -LoadFunctionsOnly`, then runs the `Warewolf.Execution.ServiceBusWorker.E2EHarness` console app to either (a) publish one or more (`-MessageCount`, default 1) uniquely-marked messages and verify they all arrive on the Service Bus queue via the `Azure.Messaging.ServiceBus` SDK, or (b) with `-VerifyWorkflowExecution`, publish `-MessageCount` real workflow-trigger messages and poll the target Lightweight engine's own `GET /secure/servicebus-result/{correlationId}` endpoint (bounded-concurrency, since there is no bulk results API) until ALL of them report a Succeeded execution — proving the FULL pipeline including actual workflow execution, not just message arrival. All readiness polling is HTTP-only (`/api/overview`, `/api/shovels`) — never `docker exec rabbitmqctl`/`rabbitmq-plugins`, which was found to race the node's own cookie-file handling during boot and crash it. Docker is skipped entirely (and never required on PATH) when `-RabbitMqMode External` is combined with `-DestinationMode ExternalServiceBus` — this is what lets the test run on a Microsoft-hosted Windows agent, whose Docker daemon only supports Windows containers and can't pull the Linux-only images this script otherwise uses. Wired into CI as the `ShovelBridgeE2ETest` job in `Dev/.azure/pipeline.yml` (`-RabbitMqMode Container -DestinationMode Emulator`), the `ShovelBridgeE2ETest_ExternalServiceBus` job in `Dev/.azure/pipeline-CLOUD.yml` (`-RabbitMqMode External -DestinationMode ExternalServiceBus`, against a local, choco-installed RabbitMQ Windows service on that same hosted agent — same technique as `TestRun.ps1`'s `Start-HostRabbitMQServer`, plus enabling the shovel plugins — rather than a standing external broker, with an additive second `-VerifyWorkflowExecution` leg proving a single message actually executes on the UAT engine), and the `ShovelBridgeLoadTest_ExternalServiceBus` job in `Dev/.azure/pipeline-LOADTEST.yml` (same setup, but `-VerifyWorkflowExecution -MessageCount 20000` against the same UAT engine and shared verification queue, requiring all 20,000 workflow-trigger messages to execute successfully — a fully end-to-end load test, not just bridge throughput). **At load-test volumes, mint the result-poll token LATE:** `GET /Secure/servicebus-result/{id}` stays Entra-protected (the trigger's transport-boundary trust covers the *message* path only), and an access token lives 60–90 min while a 20,000-message drain takes ~90 — so a token minted up front expires mid-poll and, because `401` is terminal not transient (WOLF-8418), marks every pending correlationId a spurious `HttpError` that looks just like mass message loss. Pass `-ResultPollTenantId`/`-ResultPollClientId`/`-ResultPollResourceAppId`/`-ResultPollClientSecret` *instead of* `-ResultPollAuthToken` and the script publishes (no token needed), waits for the queue to drain (`-DrainQuietSeconds`, keep it ≥360 — a locked message isn't counted active and the `PT5M` lock means redeliveries arrive ~5 min apart), mints a fresh token, then polls. Requires persistence on the target engine, or a deferred poll 404s on most ids. `-SkipTeardown` leaves any containers running for local debugging. |
| `Tests/Integration/Test-ServiceBusTriggerCrashRecovery.ps1` | **Service Bus trigger crash-recovery test (WOLF-8512)** — proves that killing the engine's host process mid-execution leaves the claim row in the stalled shape the silent-death breadcrumb depends on. Stands up Azure SQL Edge (the Hangfire claim store) and the Azure Service Bus emulator, plus a deliberately slow local HTTP endpoint for the triggered workflow to block on; stages a throwaway engine deploy (the committed `Settings/`/`Resources/` are never touched); starts the engine with `func start`; sends ONE message straight to the queue via the E2EHarness's `--mode servicebus-send` (no RabbitMQ/Shovel — this test makes no claim about the bridge); waits until the workflow has actually called the slow endpoint, so execution is provably in flight rather than merely started; then kills the whole process tree and asserts against Hangfire that a claim exists at a non-terminal stage with no `sbtrigger:result:*` row. The endpoint-call check is what stops the test passing vacuously — a workflow that stalled for some unrelated reason would otherwise leave an identical claim signature. The delay is an HTTP endpoint rather than a command-line tool because the emulator/SQL Edge images are Linux-only (so this must run on a Linux agent) and `DsfExecuteCommandLineActivity` hardcodes `cmd.exe`. Wired into CI as the `ServiceBusTriggerCrashRecoveryTest` job in `Dev/.azure/pipeline.yml`. `-SkipTeardown` leaves the containers, staged deploy and engine log for inspection. |
| `Tests/Integration/Test-ServiceBusTriggerCrashRecovery.Tests.ps1` | Pester 5 suite (13 tests) for the crash-recovery script's build helpers via `-LoadFunctionsOnly` — `Get-WwEngineBuildOutputPath` and `Invoke-DotnetBuild`. `dotnet` is shadowed with a tracking function, so it runs offline with no SDK, docker, SQL Edge, emulator or func CLI. Both suites are regressions for build 30657 (2026-09-13), where the Linux job failed with nothing in the log but `dotnet build failed for the Execution Engine (exit 1)`: the staged deploy was read from `bin/output`, which `dotnet build` never produces (it only ever existed on a dev box holding a stale publish), and the build itself was piped to `Out-Null`, so the MSB3030 that actually broke it — a case-mismatched `ssl generation\` path in `Dev2.Runtime.Services.csproj`, harmless on Windows and fatal on a case-sensitive agent — never reached the log. Verified by reintroducing both defects: 9 of 13 tests fail, 13 of 13 pass with them fixed. |
| `Tests/Rollback-WwExecutionEngine.Tests.ps1`  | Pester 5 suite for the rollback script (ownership resolver + DryRun teardown). |
| `Tests/Invoke-WwEnginePreWarm.Tests.ps1`      | Pester 5 suite for the engine warm-up script (`Get-WarmupUri`, `Format-WarmupRequestBody`, `Update-LatencyStreak`, `Test-WarmupRoundClean`, `ConvertTo-WarmupResult`, `Get-WarmupConcurrencyLadder`, `Format-WarmupCodeSummary` via `-LoadFunctionsOnly`; required-parameter and `-DryRun` phase-plan checks). Includes three regressions for the 2026-08-24 load-test failure: **a per-call timeout must not fail the step** (runs the real script against the non-routable `10.255.255.1` through a wrapper reproducing the ADO step's `$ErrorActionPreference = 'Stop'` + `&` shape, and asserts exit code **0** — the pre-fix script exits 1); **every `Invoke-WebRequest` must sit in a try/catch AND pass `-ErrorAction Stop`** (asserted against the AST — inside a `ForEach-Object -Parallel` runspace `$ErrorActionPreference` is `Continue`, so without `Stop` the exception is non-terminating and skips the catch entirely); and **the ramp must always finish at the full target** (or the final-round verdict would silently mean "clean at something less than target"). Plus the build 30690 regressions for `-InputParameters`/`{guid}`: a token becomes a parseable `uniqueidentifier`, successive calls differ, non-token and non-string values pass through untouched, and **the caller's hashtable is not mutated** (it crosses the `ForEach-Object -Parallel` boundary by reference, so in-place expansion would race across runspaces). An **AST guard** also asserts `Format-WarmupRequestBody` calls none of the script's other helpers, because Phase B re-materialises that one function from its own source text — a sibling call would throw `CommandNotFoundException` in every parallel call and read as a uniformly failing warm-up. Runs offline. Run: `Invoke-Pester -Path ./Tests/Invoke-WwEnginePreWarm.Tests.ps1`. |
| `Tests/Configure-WwExecutionAuth.Tests.ps1`   | Pester 5 suite for the auth script's helpers (e.g. `Resolve-AssignmentUser` Stage 6 guard) via `-LoadFunctionsOnly`. |
| `Configure-WwExecutionAuth.ps1`               | End-to-end Entra + Easy Auth + secure-config provisioning (idempotent). |
| `Configure-WwExecutionAuth-Clients.ps1`       | Provisions client app registrations by **type** (SPA / Confidential / Daemon / Console). The **Daemon / Managed Identity** path can enable a client Function App's system-assigned MI (`-DaemonFunctionAppName`/`-DaemonFunctionAppResourceGroup`) or bind an existing SP (`-ManagedIdentityObjectId`), and defaults `-AppRolesToAssign` to `Warewolf_ClientApps`. |
| `Configure-WwExecutionAuth-ClientApps.ps1`    | **Orchestrator** — one registration per client-**example** app (Angular, React, WebMvc, Console, AzureFunction, ServiceBus); prompts, provisions via the by-type script, validates access (`/secure/{workflow}.json`), and writes a masked summary + transcript. See [`docs/KB-ClientApps-Configuration.md`](../docs/KB-ClientApps-Configuration.md). |
| `Tests/Configure-WwExecutionAuth-ClientApps.Tests.ps1` | Pester 5 suite for the orchestrator's pure helpers (`Get-ClientAppDefinitions`, `Get-ValidationPlan`, `Format-ClientAppSummary`) via `-LoadFunctionsOnly`. |
| `Configure-WwExecutionAuth-debug.ps1`         | Debug variant with extra diagnostic dumps.                              |
| `Configure-WwExecutionAuth-local.ps1` etc.    | Local dev wrappers used by individual contributors.                     |
| `Cleanup-WwExecutionAuth.ps1`                 | Tear down all artifacts created by the configure script.                |
| `Encrypt-Config.ps1`                          | Encrypts `secure.config` plaintext into the deployable form, and converts `.bite` sources from DPAPI/plaintext to **WFAES** (AES-256-GCM, key in Key Vault). Two modes: **attribute** (default — rewrites the `ConnectionString` of a `<Source>` element) and **`-WholeFile`** (encrypts the entire file; required for **queue-trigger JSON**, which has no `<Source>` element and is otherwise *silently skipped*). `-VerifyOnly` round-trips in memory without writing. |
| `Tests/Encrypt-Config.Tests.ps1`              | Pester 5 suite for `Encrypt-Config.ps1` — `-WholeFile` round-trip, idempotency, tamper detection (AES-GCM), attribute-mode regression, and the single-file-folder StrictMode fix. Runs offline (`az` is shadowed). |
| `Setup-EntraAuth.ps1`                         | Lightweight subset for Entra app + role provisioning only.              |
| `KeyVaultSetup.ps1`, `KeyVaultSetup.azcli`    | One-time KV bootstrap for SecretKey storage.                            |
| `Generate-WorkflowIndex.ps1`                  | Builds workflow discovery index used by `apis.json` route.              |
| `secure.config.example.json`                  | Documented plaintext shape (CFG-04, CFG-05).                            |
| `secure.config.schema.json`                   | JSON Schema used to validate `secure.config` in CI (CFG-07).            |
| `authsettingsV2.json`                         | Reference Easy Auth V2 settings template.                               |

---

## End-to-end deployment (`Deploy-WwExecutionEngine.ps1`)

A single orchestrator that runs the whole of `docs/Deployment-Steps.txt`, reusing
`Setup-ApplicationInsights.ps1`, `Configure-WwExecutionAuth.ps1`,
`Encrypt-Config.ps1` and `Generate-WorkflowIndex.ps1` (Phase 3.6 — writes
`Resources\workflow-index.json` into the staged package so it ships inside the
deploy zip). Inputs follow a *params-first, prompt-if-missing* model and
the script resolves **every** decision, prints a masked summary, and asks once to
proceed **before** any change. It does **not** build the project — publish once
yourself and point `-PublishPath` at that folder (or a `.zip` of it).

> **Breaking change:** `-ResourceGroup`, `-Location`, `-StorageAccount`,
> `-AppName` and `-PublishPath` no longer have defaults (and `-KeyVaultSecretName`
> is required when encrypting). Non-interactive callers must pass them explicitly.

> **.NET runtime.** New Function Apps are created on `dotnet-isolated` **10** / Functions v4,
> matching the engine's `net10.0` `<TargetFramework>` (the JobProcessor and ServiceBusWorker
> companions do the same). `--runtime-version` applies **only at create**, so an app that
> already exists keeps the runtime it was born with — deploying a `net10.0` package onto a
> `v8.0` host produces a deploy that reports success and an engine that fails at startup with
> *"You must install or update .NET to run this application."* Every run reports the app's
> current runtime in the Phase 0.5 plan; pass **`-UpgradeRuntimeVersion`** to reconcile an
> existing app to `netFrameworkVersion v10.0` (**the app restarts**, which is why it is
> opt-in). Requires an az CLI that knows the stack — verify with
> `az functionapp list-runtimes --os windows --query "[?runtime=='dotnet-isolated'].version"`.

> **Storage wiring.** `--storage-account` binds **only at create**, exactly like the runtime
> above, so an app that already exists keeps whatever storage account it was born with and
> `-StorageAccount` says nothing about it. Every run reads the app's `AzureWebJobsStorage` at
> Phase 0.5 and prints a **`Storage wiring`** line; Phase 1 then acts on it:
>
> | Finding | What Phase 1 does |
> |---|---|
> | app already points at `-StorageAccount` | nothing |
> | app points at an account that **does not exist** | **repoints** `AzureWebJobsStorage` + `WEBSITE_CONTENTAZUREFILECONNECTIONSTRING` at `-StorageAccount` and ensures a content share |
> | app points at a **different account that exists** | reports `STORAGE MISMATCH`, changes nothing (repointing a live app would orphan its host locks, timers and deployed package — re-run with `-StorageAccount <that account>` if it is the intended one) |
> | `AzureWebJobsStorage` unreadable | reports `STORAGE UNVERIFIED`, changes nothing |
>
> Build 30739 is why: `wwprofilerlw` had been created against `stwwprofilerlw`, which a later
> cleanup sweep deleted. The plan printed `StorageAccount : wwprofilerlwsa` and
> `[+] Storage account 'wwprofilerlwsa' already exists.` — both true, both about an account the
> app had never used — and Phase 4 then died inside Kudu with
> `500 ... System.IO.IOException: The network path was not found.` On a Consumption (Y1) plan
> the content share is mandatory, so an app whose storage account is gone cannot be deployed to
> at all. The decision is also recorded in the run summary under `storage`
> (`target` / `before` / `action`).

```powershell
az login

# 1) Publish once (outside the orchestrator)
dotnet publish ..\Warewolf.Execution.Lightweight.csproj -c Release -o D:\ExecutionEngine\Publish

# 2) Dry-run the full pipeline (prints every action, mutates nothing)
./Deploy-WwExecutionEngine.ps1 -ResourceGroup DEV2 -Location southafricanorth `
    -StorageAccount stwwenginetest -AppName wwenginetest `
    -PublishPath D:\ExecutionEngine\Publish -DryRun

# 3) Non-interactive deploy (folder or .zip publish source)
./Deploy-WwExecutionEngine.ps1 `
    -ResourceGroup DEV2 -Location southafricanorth `
    -StorageAccount stwwenginetest -AppName wwenginetest `
    -PublishPath D:\ExecutionEngine\Publish `
    -AuthConfigPath ./Deploy-WwExecutionEngine.authconfig.json `
    -SecureConfigPath 'C:\cfg\secure.config' `
    -LicenseConfigPath 'C:\cfg\Warewolf License.secureconfig' `
    -WorkflowsSourcePath 'C:\Warewolf\Resources' `
    -EncryptResources:$true -VerifyDecryption -KeyVaultName kv-wwengine-test -KeyVaultSecretName wwengine-aes-key `
    -EnableAppInsights:$true `
    -EnableElasticsearch:$true -ElasticsearchSourcePath 'C:\cfg\ElasticsearchLoggingSource.bite' `
    -NonInteractive
```
> First-time deploy shown (encrypt once). On later deploys drop `-EncryptResources`
> + `-VerifyDecryption` (sources already encrypted, staged as-is) but keep
> `-KeyVaultName`/`-KeyVaultSecretName` so the engine decrypts them at runtime.

### Deploy targets

- **`warewolfserver`** — deployed by `pipeline-CLOUD.yml`'s `Deploy_And_Test_Azure_Functions`
  job on every CI run against `develop`/`7580*`. That job is `checkout: none`, so it runs
  `Deploy-WwExecutionEngine.ps1` from the **`DeployScripts` pipeline artifact** the `Build`
  stage stages out of this folder — i.e. the copy belonging to the commit being built.
  Until 2026-09-14 it instead downloaded a hard-coded
  `Warewolf-ExecutionEngine-Deployment-Scripts-v2.0.zip` from blob storage, pinning it to a
  2026-06-26 snapshot of these scripts; every argument added here after that date failed the
  deploy with `A parameter cannot be found that matches parameter name '<X>'`
  (WOLF-8512, builds 30660-30666 — see `docs/Deploy-UAT-Redeploy-Spec.md` §15). **Both
  pipelines now run the built commit's copy of these scripts; do not reintroduce a
  version-pinned script source.**
- **`WarewolfServer-UAT`** — deployed by `pipeline-LOADTEST.yml`'s `Deploy_UAT` stage on
  every run of that pipeline, immediately before `Load_Test`. Automated per
  `docs/Deploy-UAT-Redeploy-Spec.md` §10 — previously this app was ONLY updated by manual,
  undocumented deploys, which is what let a source fix go un-deployed and silently fail an
  otherwise-passing load test (see that spec's §9-§13). The Hangfire persistence DB source
  (`Scripts/persistencesettingsdbsource.uat.bite`) is committed to source control like
  `Settings/ElasticsearchLoggingSource.bite` — its `ConnectionString` is WFAES-encrypted at
  rest and only decryptable via this app's own Key Vault key at runtime, so it does not need
  Secure File / secret-variable handling. No manual operator step is required.

Highlights:

- **Publish source** — `-PublishPath` accepts a folder or a `.zip`. No `dotnet publish`
  is run by the script. The publish output is **never modified**: every run copies (or
  extracts) it into a **fresh staging dir under the OS temp path**
  (`wwexecutionengine-stage-<AppName>-<stamp>-<pid>-<token>`, unique per run), prepares the package there, zips **that**,
  uploads it, then removes the staging dir on a successful real run. This keeps each
  zip clean and lets the engine and the JobProcessor stage in **separate** directories.
- **secure.config** — an already-AES-encrypted file is validated (must be engine-
  decryptable) and staged as-is; a plaintext-JSON file is validated then AES-encrypted
  automatically; an undecryptable file is a hard error.
- **License** — `-LicenseConfigPath` is prompted (Enter to skip) and, when supplied,
  copied to the publish root as `Warewolf License.secureconfig`. Optional: if omitted,
  the engine's license check (default on) may fail at startup.
- **Encryption (optional, default OFF)** — `-EncryptResources` is a **single** switch
  for **all** sources (workflows + Elasticsearch + others): it converts `.bite`
  connection strings to WFAES via Key Vault. **Encrypt once** — on later deploys leave
  it off and sources are staged as-is. `-VerifyDecryption` (default OFF) adds an
  optional **in-memory** round-trip decrypt (no plaintext on disk). Whenever sources
  are WFAES-encrypted you must still pass `-KeyVaultName`/`-KeyVaultSecretName` on every
  deploy so the engine's managed identity can decrypt them at runtime (the dev
  *Secrets Officer* role + key generation are added only when `-EncryptResources` is set,
  and that role grant is **skipped when the deploying identity can already read the key** —
  so a pre-entitled CI service principal never needs RBAC-write on the vault).
  **The identity running the deploy needs `get` on the secret itself when
  `-EncryptResources` is set** — encryption reads the key material, whereas passing
  `-KeyVaultName`/`-KeyVaultSecretName` alone only wires the engine's *runtime* managed
  identity. A CI service principal that has deployed fine for months can therefore fail
  the first time the switch is turned on.
  **`-VerifyDecryption` is folder-wide — think before enabling it in CI.** It decrypts *every*
  encrypted value under `Resources/` and fails the deploy on any single failure (there is no
  per-file verify switch). That is excellent for a source set you own end to end, and a liability
  when the publish output also ships committed blobs your pipeline never exercises: one stale
  blob encrypted under an older or different key then fails an otherwise-healthy deploy.
  `pipeline-LOADTEST.yml` deliberately passes `-EncryptResources` *without* it for exactly this
  reason.
  **Leaving `-EncryptResources` on permanently is safe** despite the "encrypt once"
  guidance above, and is sometimes necessary — `pipeline-LOADTEST.yml` generates a
  broker source at stage time with a plaintext connection string (so a live credential
  never enters git) and relies on this switch to convert it before upload.
  `Encrypt-Config.ps1` **skips** files already carrying the `WFAES::` prefix, so
  already-encrypted sources are not double-encrypted. The one thing to keep out of a
  staged set is a **DPAPI**-encrypted source: it can never decrypt in Azure (which is why
  `Resources/rabbit/NewSqlServerSource (Local Backup).bite` is explicitly excluded from
  staging).
- **Environment variables** — feature toggles drive the engine's app settings.
  Console logging and Application Insights default **on**; Elasticsearch logging
  defaults **off** (enabling it requires `-ElasticsearchSourcePath`; Key Vault only if
  the source is WFAES-encrypted).
  `ASPNETCORE_ENVIRONMENT` is fixed to `Production`. `BYPASS_SECURE_CONFIG`,
  `WAREWOLF_SUPER_ADMIN_ENABLED` and `SkipFailureToRetrieveSecret` are **not set at
  all** (not in logs/summary, not on the app) — the engine defaults them to
  disabled when absent; an admin adds one manually only if ever needed. App
  Insights' `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` (not the standard name) is
  **auto-read** from the App Insights resource, never prompted. See the env-var
  matrix in `docs/Deployment-Steps.txt`.
- **`WAREWOLF_RABBITMQ_CACHE`** — the RabbitMQ publish connection cache, and the ONE
  setting that controls it (all eight knobs live inside its JSON). Written by every
  engine deploy from `-RabbitMqCacheConfig`, which defaults to
  `{"enabled":true,"channelPool":false,…}`: connections are reused across
  `PublishRabbitMQActivity` executions (~6 of the 8 broker round-trips a publish costs),
  while **channel pooling ships off** pending sign-off on decision 2 of
  `docs/RabbitMQ-ConnectionCache-Plan.md` — see that plan's 2026-09-14 amendment for the
  benchmark and what turning it on is worth. Pass `-RabbitMqCacheConfig ''` to omit the
  setting entirely and leave the engine on the uncached path. Note the **engine's own**
  default is OFF when the variable is absent, which is what the on-prem server and any
  hand-made app get; only this script turns it on.
- **Dry-run parity** — `-DryRun` produces the **same** outputs as a real run except
  it creates/uploads nothing in Azure: files are prepared into a temp staging dir
  (`wwexecutionengine-stage-<AppName>-<stamp>-<pid>-<token>-dryrun`, leaving your publish output
  untouched) that is **kept** for inspection, and a transcript + `*.dryrun.summary.json`
  (with `"dryRun": true`) are written so the rollback can be exercised from a dry-run
  summary. Encryption runs only when the Key Vault key is reachable, else it's staged
  unencrypted and deferred.
- **Logging** — `-ExecutionLogLevel` is prompted when interactive and sets `EXECUTIONLOGLEVEL`,
  which drives the engine's console + App Insights logging **in code** (the isolated worker
  does not read host.json). Rewriting the published host.json logLevel is **opt-in** via
  `-AlignHostJsonLogLevel` (only tunes the Functions host process). A timestamped transcript +
  masked `*.summary.json` are written under `-LogDir` for **both** dry-run and real runs;
  secret values are redacted everywhere.
- **Crash-safe summary** — the `*.summary.json` is written *before* the first mutating
  action and re-written after every phase and from the failure handler, so it **always
  exists** no matter where a run stops. It carries `status` (`in-progress`/`completed`/
  `failed`), `lastPhase`, and `error`. The `created` map records creation **intent**
  (set before each create call), so a deploy that fails mid-run (e.g. Phase 4) can still
  be rolled back directly from its summary.
- **Deploy** — `az` zip-deploy (config-zip) by default (Auto/Zip) — the correct
  method for the pre-built publish artifact. `func azure functionapp publish` is
  opt-in only (`-PublishMethod Func`); it expects a project source dir and fails on
  a pre-built package, so it is **not** used automatically.
- **Teardown** — every created resource is tagged `wwx-test-run=<runId>` and
  recorded in `summary.json`'s `created` map. `Rollback-WwExecutionEngine.ps1
  -SummaryPath <summary.json>` deletes **only** those (auth → Key Vault delete+purge
  → App Insights → Function App → storage), preserves pre-existing resources, never
  deletes the resource group unless the run created it **and** `-DeleteResourceGroup`
  is passed, and finishes with a leak check. Dry-run it first. A **partial/failed**
  deploy summary (`status = failed`/`in-progress`) is accepted too — rollback notes the
  partial run, then cleans only the recorded created-map (absent/never-created resources
  are skipped, pre-existing ones preserved).

**Tests** (Pester 5):

```powershell
Install-Module Pester -MinimumVersion 5.0 -Scope CurrentUser   # one-time
Invoke-Pester -Path ./Tests                                    # all suites
Invoke-Pester -Path ./Tests/Deploy-WwExecutionEngine.Tests.ps1
```

These run in CI as the **`Deploy Script Pester Tests`** job (`Dev/.azure/pipeline.yml`), on
`ubuntu-latest` in the **Compile** stage — the suites need no build output, so they run in
parallel with the compile rather than waiting for it, and a failure fails the build. Every suite
is offline (`az`/`func`/`dotnet` are shadowed with tracking functions) and green on both Linux
and Windows, so keep them that way: a suite that needs a live service, a real SDK or a Windows-only
API belongs in an integration harness under `Tests/Integration/`, not here.

---

## ExecutionEngineJobProcessor deployment (`Deploy-WwJobProcessor.ps1`)

The dedicated poller/reaper Function App (`Warewolf.Execution.EngineJobProcessor`) that
replaces `hangfireserver.exe`: it polls Hangfire SQL storage for due `Scheduled`
suspend/resume jobs and fire-and-forget POSTs them to the engine's
`/secure/resume/{jobId}` route (managed-identity bearer token), and reaps stale
`Processing` jobs to `Failed` (fail-only).

Publish first, then deploy (the script does **not** build):

```powershell
dotnet publish Dev/Warewolf.Execution.EngineJobProcessor/Warewolf.Execution.EngineJobProcessor.csproj -c Release -o D:\JobProcessor\Publish

./Deploy-WwJobProcessor.ps1 `
  -ResourceGroup   DEV2 `
  -Location        southafricanorth `
  -StorageAccount  stwwjobproc `
  -AppName         wwjobproc `
  -PublishPath     D:\JobProcessor\Publish `
  -EngineResumeBaseUrl https://wwengine.azurewebsites.net `
  -EngineResumeScope   api://<engine-app-id>/.default `
  -KeyVaultName    kv-warewolf `
  -KeyVaultSecretName dp-keyring-v1
  # -PersistenceSettingsPath / -PersistenceDbSourcePath are PROMPTED if omitted
```

Highlights:

- **Same conventions as the engine deploy** — `Write-Phase` banners, `Invoke-Az`
  wrapper, secret masking, `-DryRun` (produces the same logs + `*.summary.json` with a
  `dryRun: true` flag), and a *params-first, prompt-if-missing* model that resolves
  everything, prints a masked plan, and asks once before any change.
- **Persistence pair, prompted** — `persistencesettings.json` (staged as-is) and
  `persistencesettingsdbsource.bite` (ConnectionString WFAES-encrypted with the engine's
  Key Vault key when `-EncryptResources`, exactly like the Elasticsearch source) are
  prompted when not passed, and validated to their exact filenames.
- **Own publish output + clean zip** — the processor is a **different** Function App
  built from a **different** csproj, so it must be published to a **separate directory**
  from the engine. Like the engine, it never mutates the publish output: it stages into
  a fresh temp dir (`wwjobprocessor-stage-<AppName>-<stamp>`), zips that, uploads it, and
  removes the staging dir on a successful real run — so the engine and processor prepare
  and upload their zips independently.
- **System-assigned MI** is always enabled (Key Vault decrypt + engine token).
- **App settings** applied: `JOB_POLL_SCHEDULE`, `JOB_REAPER_SCHEDULE`,
  `JOB_STALE_MINUTES`, `ENGINE_RESUME_BASEURL`, `ENGINE_RESUME_SCOPE`,
  `ENGINE_RESUME_TIMEOUT_SECONDS`, `ENGINE_RESUME_AUTH_DISABLED`, plus
  `AZURE_KEYVAULT_NAME` / `KEYVAULT_SECRET_NAME` when a vault is in play.
- **Companion mode** — `Deploy-WwExecutionEngine.ps1 -DeployJobProcessor` runs this
  script after the engine deploy, passing the shared context (subscription/tenant/RG/
  location/Key Vault/persistence pair + the engine's own URL as `-EngineResumeBaseUrl`);
  the child prompts for anything not supplied (its own `AppName`/`PublishPath`/`StorageAccount`).
  The engine resolves `-JobProcessorPublishPath` at **plan time** and **fails loudly** if
  it is the **same directory** as the engine's `-PublishPath` (it would otherwise zip the
  engine's binaries into the processor app) — publish the two projects to separate folders.

**Authorization is a separate operator step** (the two-part contract): grant this app's
managed identity the engine app role `Warewolf_JobProcessor` and add the matching
global-scope `Execute` row to the engine's `secure.config`. See
[`docs/Deploy-EndToEnd-Runbook.md`](../docs/Deploy-EndToEnd-Runbook.md) (JobProcessor section).

Tested by `Tests/Deploy-WwJobProcessor.Tests.ps1` (Pester 5) following the same
`-LoadFunctionsOnly` + az-shim / `-DryRun` conventions as the engine suite.

---

## RabbitMQ QueueProcessor deployment (`Deploy-WwQueueProcessor.ps1`)

The Linux **container** worker (`Warewolf.Execution.QueueProcessor`) that replaces
`N × QueueWorker.exe` for the Azure path: it consumes one RabbitMQ queue trigger and POSTs
the mapped message to the engine's `/Secure/{workflow}.json` with a managed-identity token.
Hosted on **Azure Container Apps**, autoscaled **0 → N replicas** by the KEDA `rabbitmq`
scaler, **one Container App per queue-trigger**. The on-prem Server +
`QueueWorker.exe` path is untouched.

Publish first, then deploy (the script does **not** build the .NET project; it does build the
container image via `az acr build`):

```powershell
dotnet publish Dev\Warewolf.Execution.QueueProcessor\Warewolf.Execution.QueueProcessor.csproj -c Release -o D:\QueueProcessor\Publish

./Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup   DEV2 `
  -Location        southafricanorth `
  -AcaEnvironment  aca-warewolf `
  -AcrName         acrwarewolf `
  -PublishPath     D:\QueueProcessor\Publish `
  -TriggerPath     'C:\ProgramData\Warewolf\Triggers\Queue' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl   https://wwengine.azurewebsites.net `
  -EngineResourceAppId <engine-app-id> `
  -KeyVaultName WWExecutionEngine -KeyVaultSecretName wwaeskey -EncryptStagedSettings `
  -RabbitMqSecretUri https://wwexecutionengine.vault.azure.net/secrets/rabbitmq-uri
```

**How the trigger file is pointed at the script** (mutually exclusive; `-TriggerId` narrows a
folder/manifest to one trigger; **zero matches is a hard error**):

| Parameter | Result |
|---|---|
| `-TriggerFilePath <file>` | one Container App |
| `-TriggerPath <folder>` + `-TriggerFilter` (default `*.bite`) | one Container App **per matching file** |
| `-TriggerManifestPath <json>` | one per entry, with per-trigger overrides |

**Derived from the trigger file** (single source of truth — operators keep editing the trigger /
its release variable): `maxReplicas = Concurrency`; KEDA `value = Prefetch × MaxConcurrency`
(messages per replica, so `replicas = ceil(queueLength / value)` capped at `maxReplicas`);
The worker reads `Prefetch` from the staged trigger for its per-consumer QoS (no prefetch env
var — the trigger is the single source of truth). `-ScalingMode` defaults to `Elastic` (`minReplicas = 0`);
`Fixed`/`Warm` and any `-MaxReplicas` above `Concurrency` are exception paths and are flagged in
the plan output. `Concurrency = 0` deploys `min = max = 0` (disabled), mirroring on-prem.

**Fail-loud plan-time guards:** unsubstituted `#{…}` release token; derived app-name collision;
invalid timeout nesting (`-EngineTimeoutSeconds ≤ -ShutdownGraceSeconds < -TerminationGracePeriodSeconds`,
and `-EngineTimeoutSeconds <` the **engine's** `functionTimeout`); missing trigger/source file. Peak core
usage (`Σ maxReplicas × cpu`) is printed to check against the ACA environment quota.

**Failure handling.** Engine 2xx → ack. Engine non-2xx → dead-letter the mapped body **and** ack (so a
drained queue proves nothing on its own — compare `succeeded` vs `deadLettered`). A **transport** failure
(timeout, socket error) → nack with requeue, and dead-letter once `-MaxDeliveryAttempts` is spent.

`-MaxDeliveryAttempts` accepts only **1** (dead-letter at once) or **2** (requeue once, then dead-letter,
the default). Attempts are counted with the AMQP `redelivered` flag, which is a boolean — the broker
records that a message has been seen before, not how many times — so higher values are clamped with a
logged warning. Before 2026-08-11 a transport failure was left **unacked**, which with `Prefetch=1`
stopped the consumer permanently (34 messages stranded for 20+ minutes behind one message).

**Timeout defaults** (raised from 45/60/90 on 2026-08-11 after 45 s proved under-sized in a live run;
sized from measured engine latency, see the migration plan §2.6):

| Setting | Default |
|---|---|
| `-EngineTimeoutSeconds` | 180 |
| `-ShutdownGraceSeconds` | 210 |
| `-TerminationGracePeriodSeconds` | 240 |
| the engine's `functionTimeout` (`host.json`, not a script parameter) | 600 |

> ⚠️ `-TerminationGracePeriodSeconds` was validated and printed but **never applied** until 2026-08-11 —
> it is a Container App *template* property, not an env var, so `--set-env-vars` could not carry it.
> Older revisions run on ACA's 30 s default regardless of what the plan output claimed. Verify with
> `az containerapp show` and check `properties.template.terminationGracePeriodSeconds` is not empty.

Can also run as a **companion of the engine deploy**, fanning out over every pointed trigger:

```powershell
./Deploy-WwExecutionEngine.ps1 ... `
  -DeployRabbitMqTriggers `
  -QueueTriggerPath 'C:\ProgramData\Warewolf\Triggers\Queue' `
  -QueueSourcePath  'C:\ProgramData\Warewolf\Resources\Sources' `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -QueueProcessorPublishPath D:\QueueProcessor\Publish `
  -RabbitMqSecretUri https://wwexecutionengine.vault.azure.net/secrets/rabbitmq-uri
```

**What gets staged into the container** — config is **baked into the image**, not mounted or
fetched at startup (this worker scales to zero, so any share mount or blob round-trip would be paid
on every 0→1 scale and could stop a replica starting):

```
/app/Settings/triggers/<TriggerId>.bite      the queue-trigger definition
/app/Settings/sources/<QueueSourceId>.bite   every source the trigger references…
/app/Settings/sources/<QueueSinkId>.bite     …including the dead-letter sink, when it differs
```

Sources are prompted for with `-QueueSourcePath` and copied automatically for **both**
`QueueSourceId` and `QueueSinkId` — a sink source that is not staged gives a replica that starts
and then cannot dead-letter. **Every referenced source is resolved at plan time** and listed with
the triggers that reference it; any gap aborts the run before a single Container App is created, so
a fan-out over many triggers is all-or-nothing rather than half-deployed. At runtime the worker
reads all staged sources **once at startup** and caches them indexed by `ID`, so nothing re-reads a
`.bite` after cold start. The script writes `{sourceId}.bite`; the worker also resolves an
operator-named Studio file (e.g. `Warewolf DevOps RabbitMQ Source.bite`) by scanning for a matching
`ID` attribute. `Settings\` itself is still searched **after** `Settings\sources\`, so a deployment
staged under the earlier flat layout keeps working. Paths are overridable with
`QUEUE__SETTINGSPATH` / `QUEUE__TRIGGERSSUBPATH` / `QUEUE__SOURCESSUBPATH` / `QUEUE__TRIGGERFILTER`;
defaults resolve against **`AppContext.BaseDirectory`**, not the working directory, and folder
casing is matched case-insensitively (the worker is Linux, the deploy is Windows).

**Tenant id is always set.** `-EngineTenantId` defaults from `az account show` and is prompted if
that yields nothing, because a blank tenant is legal only for a *system-assigned* managed identity
— for anything else the credential chain fails with *"Invalid tenant id provided"* at the first
message, which reads like a missing app role rather than missing config.

**Optimum scale shape:** `minReplicas 0`, `maxReplicas = Concurrency`, and trigger
`Prefetch = MaxConcurrency` (both 1 unless a workflow is measured safe to run concurrently).
Dispatch is serial per channel, so a larger prefetch adds no throughput — it raises the KEDA target
(`value = Prefetch × MaxConcurrency`), which **delays** scale-out, and leaves more buffered messages
to nack on drain. Scale **out**, not up; a prefetch above the cap is flagged as a plan-time advisory.

**Testing locally without deploying:** `Dev\Warewolf.Execution.QueueProcessor\Settings\` holds a
committed working sample (dev broker source under `sources\` + an `order-queue` trigger under
`triggers\`) copied to the build output, so the worker runs straight from `bin\`. It is deliberately
**excluded from `dotnet publish`**, so it never reaches a container image. For `docker compose`,
copy it to `Settings.local\` (git-ignored) and repoint the `ConnectionString` at the compose broker.

**Authorization is done for you.** The script grants **each** app's managed identity the engine app
role `Warewolf_QueueProcessor` — once per deployed trigger, not once per run. Override the role with
`-EngineAppRole`, pass `-EngineSpObjectId` to skip the directory lookup, or `-SkipEngineAppRoleGrant`
to turn it off. A grant failure is **never fatal**: writing an `appRoleAssignment` needs
`AppRoleAssignment.ReadWrite.All` (or Privileged Role Administrator), which is a different privilege
from the Contributor rights the rest of the deploy uses, so the worker still deploys and the summary
prints the exact remediation command for every app that did not get the role. Each app's status is
recorded as `engineAppRole` in the summary JSON.

No `secure.config` change is needed: a workflow with **no** resource-scoped entry inherits the global
(`IsServer=true`) map, which the app role above satisfies. Adding a per-workflow row can **lock the
worker out** — a workflow that has any `Execute`-bearing resource entry uses the resource map
exclusively. Full walkthrough, verification, and teardown: `docs\Deploy-EndToEnd-Runbook.md` §8.

To **prove** the whole Azure path (engine `/Public` + `/Secure`, one Container App per trigger, the
ACA/KEDA scale rule, then a live scale-`0→N` test by publishing to the queue) in a disposable
resource group you delete afterwards, follow
[`docs/Deploy-E2E-Verification-Runbook.md`](../docs/Deploy-E2E-Verification-Runbook.md).

---

## Combined deploy (`Deploy-WwEngineAndQueueProcessor.ps1`)

Deploys the engine and the workers in one pass, wiring the second from what the first produced.
Edit the placeholder variable block at the top of the script, then:

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -DryRun -NonInteractive   # both plans, no changes
.\Deploy-WwEngineAndQueueProcessor.ps1 -EncryptResources         # first deploy of a source set
.\Deploy-WwEngineAndQueueProcessor.ps1                           # every deploy after that
.\Deploy-WwEngineAndQueueProcessor.ps1 -SkipEngine               # workers only, engine already up
```

**Why this exists rather than `-DeployRabbitMqTriggers`.** The engine orchestrator can deploy the
workers itself in one call. This wrapper makes the two calls separately so the handover between
them is *asserted* first — the engine deploy writes two machine-readable outputs, and both are
easy to misread:

| Output | Trap |
|---|---|
| `<LogDir>\deploy-WwExecutionEngine-<stamp>.summary.json` | rewritten after **every phase** with `status` = `in-progress`\|`completed`\|`failed`, so "the newest summary" is often an aborted run |
| `Configure-WwExecutionAuth.output.json` | **one fixed file in this folder, overwritten by every engine deploy** — not per-run, not in `LogDir`; it carries `ClientId` / `SpObjectId` / `AppRoles[].id` |

The wrapper asserts both, then merges them into `<LogDir>\deploy-both-<stamp>.handover.json` —
the file the `Warewolf_QueueProcessor` app-role grant, `Get-WwExecutionToken*.ps1` and
`Rollback-WwExecutionEngine.ps1 -SummaryPath` all read, so no downstream step re-derives an
Entra id by display-name lookup. Full usage, failure-mode table and post-deploy steps:
[`docs/Deploy-Both-RunGuide.md`](../docs/Deploy-Both-RunGuide.md).

> Do **not** set `-DeployRabbitMqTriggers` in the engine splat as well — you would get two sets
> of Container Apps.

---

## PRV-15 — Dry-run mode

```powershell
./Configure-WwExecutionAuth.ps1 -DryRun       # alias for -WhatIfOnly
./Configure-WwExecutionAuth.ps1 -WhatIfOnly   # original switch
```

Both forms print the resolved plan (Entra app, app roles, secret strategy,
app-settings, Easy Auth provider, smoke-test target) and exit without
mutating Azure.  Use as a pre-merge gate in CI.

## PRV-16 — Module split / future layout

`Configure-WwExecutionAuth.ps1` is intentionally a single file so it can
run in Cloud Shell or a deployment runner without fighting a module path.
It is internally organised by logical region:

```
Stage 0   Pre-flight           (Test-FunctionAppExists, az version)
Stage 1   Entra app create     (graph retry + dedupe)
Stage 2   ID-token issuance
Stage 3   API expose + scope
Stage 4   App-roles declarative replace
Stage 5   Service principal
Stage 6   User assignment
Stage 7   Client secret
Stage 8   App settings
Stage 9   Easy Auth V2
Stage 10  Verification
Stage 11  Smoke test
```

When this file exceeds 2,000 lines the recommended split is:

```
Modules/
    Wwx.Provisioning.Common.psm1     # Invoke-AzCli, ConvertFrom-AzJson, retry helpers
    Wwx.Provisioning.Entra.psm1      # stages 1..6
    Wwx.Provisioning.AppService.psm1 # stages 7..9
    Wwx.Provisioning.Verify.psm1     # stages 10..11
```

The single-script entry point is preserved as a thin orchestrator that
imports the modules.

## PRV-17 — Managed Identity

```powershell
./Configure-WwExecutionAuth.ps1 -UseManagedIdentity
```

Suppresses optional secret rotations beyond the minimum required by Easy
Auth's confidential-client flow.  Use this for production tenants that
prefer MI for app-only and downstream calls.  The function app's
system-assigned identity must already be enabled and granted any required
RBAC roles (e.g. on Key Vault).

## PRV-18 — Secret lifetime

```powershell
./Configure-WwExecutionAuth.ps1 -SecretLifetimeYears 2 -RotateSecret
```

Sets the validity period of any new client secret.  Range: 1–2 years
(Entra cap).  Combine with `-RotateSecret` to force rotation on every run.

---

## CFG-03 — `secure.config` CI/CD deployment

Recommended pipeline (Azure DevOps / GitHub Actions):

1. `Encrypt-Config.ps1 -Input ./secure.config.dev.json -Output ./secure.config`
2. `Test-Json -Path ./secure.config.dev.json -SchemaFile ./secure.config.schema.json`
3. Upload `secure.config` to Azure App Service `D:\home\site\wwwroot\` via
   `az webapp deploy --type static --src-path secure.config --target-path secure.config`.
4. The running function picks up the change automatically — `SecureConfigWatcher`
   debounces the file-system event and atomically swaps the policy snapshot
   (no app restart, no warm-up cost).

Sample CI step (GitHub Actions):

```yaml
- name: Validate secure.config shape
  shell: pwsh
  run: |
    Test-Json -Path Scripts/secure.config.example.json `
              -SchemaFile Scripts/secure.config.schema.json `
              -ErrorAction Stop

- name: Provisioning dry-run
  shell: pwsh
  env:
    AZURE_CREDENTIALS: ${{ secrets.AZURE_CREDENTIALS }}
  run: |
    az login --service-principal `
      -u $env:CLIENT_ID -p $env:CLIENT_SECRET --tenant $env:TENANT_ID
    ./Scripts/Configure-WwExecutionAuth.ps1 -DryRun -NonInteractive

- name: Encrypt and deploy secure.config
  shell: pwsh
  run: |
    ./Scripts/Encrypt-Config.ps1 -Input ./secure.config.${{ github.ref_name }}.json `
                                 -Output ./secure.config
    az webapp deploy --resource-group $env:RG --name $env:APP `
                     --type static --src-path ./secure.config `
                     --target-path secure.config
```

> The encryption key MUST be the same on the build runner and the function
> app.  Source it from Key Vault (`KeyVaultSetup.ps1` provisions the vault
> + access policy).
