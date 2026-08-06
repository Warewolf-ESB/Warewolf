# QueueWorker → Azure Container Apps + KEDA (RabbitMQ scaler) — Step-by-Step Plan

Phased implementation plan for migrating Warewolf's RabbitMQ **queue-trigger** processing from
`WarewolfServer + N × QueueWorker.exe` (Windows process supervisor) to a **Linux container worker**
(`Warewolf.Execution.QueueProcessor`) hosted on **Azure Container Apps (ACA)**, autoscaled
**0 → N replicas** by the **KEDA `rabbitmq` scaler**, invoking workflows against the **Execution
Engine** (Azure Function App, EasyAuth + Entra ID) with a **managed identity**.

Companion documents:
- [QueueWorker-Migration-To-Azure-Plan-Step-By-Step.md](QueueWorker-Migration-To-Azure-Plan-Step-By-Step.md)
  — **superseded**; retained as the recorded evaluation of the Azure Functions RabbitMQ trigger-binding
  option. Its §1 analysis and §2.7 hosting comparison remain valid.
- [HangFire-Migration-To-Azure-Plan-Step-By-Step.md](HangFire-Migration-To-Azure-Plan-Step-By-Step.md)
  and [HangeFire-Azure-Architecture.md](HangeFire-Azure-Architecture.md) — the sibling migration whose
  MI + Entra app-role authorization, WFAES/Key Vault secret handling, linked-shared-source pattern, and
  deploy-script conventions are reused here.

> **Status: Phases 1–4, 6, 7, 8, 9 IMPLEMENTED; Phase 10 partially; Phase 0 items 1–3 DONE
> (2026-08-04). Phase 0 items 4–6, Phase 5 and Phase 11 outstanding (blocked on Azure/tenant/
> production access).**
>
> **Phase 0 items 1–3 verified against the live broker** (`4.tcp.eu.ngrok.io:20313`, source
> `1a82a341-…`, queue `order-queue`) using the committed sample in §2.3.1 with a stub engine:
> publish → consume → map → POST → 200 → **ack**, measured as **1 publish ⇒ exactly 1 delivery ⇒
> 1 POST ⇒ 1 ack** (no redelivery, nothing dead-lettered). Confirmed in the same run:
> the URL is built as `POST /secure/hangfiredemo/Hello%20World.json` (per-segment escaping — a
> space must not break the route), `MapEntireMessage` produced `{"Name":"hello"}` against the
> workflow's real input, and the source resolves via the **ID-scan** branch from an operator-named
> file. **Item 2 (dispatch concurrency) is answered:** with `prefetch=5` and
> `WORKER__MAXCONCURRENCY=1`, deliveries were strictly serial (~2.2 s apart, no overlap), so
> **one replica ≈ one workflow at a time and `maxReplicas = Concurrency` is the correct parity
> mapping** — not `Concurrency × ProcessorCount`.
>
> **Two configuration defects found and fixed during that verification**, both the same class —
> *empty string treated as a value instead of as "unset"*:
> 1. `WwExecutionTokenHandler` passed a blank `ENGINE__TENANTID` straight into
>    `TokenRequestContext`, making **every** credential fail with *"Invalid tenant id provided"*.
>    Blank is the documented default for a system-assigned managed identity, so this would have
>    failed in Azure exactly as it did locally — and would have looked like a Phase 5 role problem.
> 2. `Program.cs` read `QUEUE__SETTINGSPATH` / `TRIGGERSSUBPATH` / `TRIGGERFILTER` with `??`, so
>    the `""` placeholders in `appsettings.json` beat the intended defaults and silently resolved
>    the settings folder against the **process working directory**. Now read via `ReadString`
>    (whitespace = unset) and anchored to `AppContext.BaseDirectory` — see §2.3.
>
> **Config delivery decided: BAKE INTO THE IMAGE** (§2.3). Trigger + every referenced source are
> staged into a temp tree and baked by `az acr build`. Rejected for this worker: Azure Files mount,
> blob+MI fetch at startup, and mount-overrides-image — all three add a per-cold-start cost or a
> second source of truth, and `minReplicas = 0` makes cold start the common path, not the rare one.
>
> **Also settled in this pass:** sources moved to their own `Settings/sources/` folder (§2.3) with
> the flat layout kept as a fallback; queue durability documented as trigger-driven and
> per-queue (§2.3.1); the optimum scale/prefetch shape recorded (§2.3.2); `ENGINE__TENANTID` now
> always set by the deploy (defaulted from `az account show`, prompted otherwise) with a
> startup warning when absent. The tenant prompt deliberately runs **after** trigger-argument
> validation, so cheap deterministic errors still fail before anything queries Azure or prompts.
>
> **Sources are cached at startup** by `RabbitMqSourceCatalog` (§2.3): one scan of
> `Settings/sources/` (then the legacy root), every `*.bite` indexed by its `ID` attribute, and the
> trigger's `QueueSourceId`/`QueueSinkId` served from memory. The deploy now resolves every
> referenced source at **plan time**, so a missing source aborts the run instead of leaving a
> half-deployed fleet behind the trigger that failed.
>
> **Test results (all green):** `Warewolf.Execution.QueueProcessor.Tests` **59/59** (was 38 —
> +21 covering durability independence, the `sources/` folder and its legacy fallback, the source
> catalog's caching/skip/duplicate/diagnostic behaviour, the "empty means unset" rule,
> blank-tenant normalisation, and the prefetch/concurrency defaults);
> deploy Pester **231/231** (was 221, and the stale `triggers*.bite` assertion is corrected —
> the script's `*.bite` default was right, the test was not)
> across all suites — new `Deploy-WwQueueProcessor.Tests.ps1` **34/34**, existing engine suite
> **76 → 86** with the queue-companion parameter-surface additions, and every pre-existing suite
> unchanged. Both projects are in `ServerTests.sln` (6 configurations each); the unit suite is
> routed to exactly one CI job (added to the Lightweight job's `-Projects`, **excluded** from the
> wildcard unit bucket so it cannot run twice).
> **Still open in Phase 10:** drain-path tests for `RabbitMqMessagePump` — it creates its own
> connection, so a small injectable-channel seam is required first; proposed, not implemented.
> Two script bugs were found by the tests and fixed: the peak-core `Measure-Object` calculation,
> and `New-StagingDirectory` colliding for the same app within one second (now suffixed).
>
> **Implemented and build-verified:** new `Dev/Warewolf.Execution.QueueProcessor` (13 files, builds
> 0 errors) — generic `IHost` + `QueueConsumerService` with the five-step drain, `TriggerBiteReader`
> (`$type`/`$id` binder onto local DTOs), `RabbitMqSourceOptions.FromBiteFile`,
> `QueueConfigurationLoader`, `RabbitMqMessagePump` on **RabbitMQ.Client 7.1.2**,
> `RabbitMqDeadLetterPublisher` (long-lived channel), `WwExecutionTokenHandler` +
> `EngineWorkflowClient` (POST + multipart), `EngineForwarder`, `AuditingConsumerDecorator`,
> `QueueProcessorLogSink` + `QueueProcessorCorrelation`, `Dockerfile`, `docker-compose.yml`.
> Phase 8: new `Scripts/Deploy-WwQueueProcessor.ps1` and `-DeployRabbitMqTriggers` on
> `Deploy-WwExecutionEngine.ps1` (all five §8.3 touch points) — both parse clean, pass
> `-LoadFunctionsOnly`, and the **existing engine Pester suite is green 76/76** (no regression).
> A `-DryRun` over the real `MandateCollectionSuccessTrigger` derives
> `max=5`/`min=0`/`value=10`/`prefetch=10` and emits the expected create + secret + env + KEDA calls.
> A smoke run of the worker against the real trigger + source files validated the `#{…}` token guard
> (exit 2, actionable), the `$type`/`$id` parse, `\`→`/` workflow normalisation, source resolution by
> GUID (password never logged), and the TLS-off parity warning.
>
> **Deviation from decision #9 (recorded 2026-08-03, see §1.8):** the engine's `IExecutionLogger`
> sink stack could **not** be consumed as linked shared source. `ExecutionLoggerBase` reads
> `InstanceCorrelationContext`/`InstanceCorrelationMiddleware` — Azure **Functions** invocation
> correlation — so linking it would drag `Microsoft.Azure.Functions.Worker` into a container that is
> not a Function App. Only the two host-agnostic contract files (`ExecutionLogLevel.cs`,
> `LoggingConfiguration.cs`) plus the whole Key Vault stack are linked; the sink is implemented
> locally against the identical environment-variable contract. Decision #9 is amended accordingly.
> Also corrected: `Dev2Logger` lives in **`Dev2.Diagnostics`** (namespace `Dev2.Common`), not
> `Dev2.Common`, so the worker references that project as the engine does.
>
> Tests are **proposed, never auto-created** (CLAUDE.md / `warewolf-test`): each phase presents its
> test plan and waits for go-ahead. **No tests have been written** — Phase 10 is pending approval.
>
> **Decisions closed 2026-08-03** (full log §5): ACA + KEDA is locked; RabbitMQ is reached exactly as
> `PublishRabbitMQActivity` reaches it, from a `{RabbitMQSource}.bite` connection string; engine auth is
> EasyAuth + Entra ID + managed identity per the Hangfire pattern; logging is `Dev2Logger` wired to the
> engine's `CompositeExecutionLogger` stack; the shared RabbitMQ client is **not** upgraded — the worker
> ships a **new** .NET 8-native consumer on **`RabbitMQ.Client` 7.x** and every existing function stays
> byte-for-byte unchanged; trigger definitions load from `Settings/triggers/*.bite` and sources from
> `Settings/{source}.bite`; scaling is **Elastic (`minReplicas = 0`) for every trigger**; cutover is
> **per-trigger, one queue at a time** (Phase 11); TLS runs at parity now with an **AMQPS go-live gate**
> before the first production trigger is enabled. See §6 for the finalization record.
>
> **§2.8 is the concurrency/scaling walkthrough** — how per-trigger `Concurrency` (worked through with
> OrdersQueue = 5 and TasksQueue = 10) becomes `maxReplicas`, how the existing *fixed* fleet is
> reproduced or made elastic, and how the production KEDA rules for multiple queues are defined and
> behave at each queue depth. **§2.8.7** covers how `Prefetch` bounds useful parallelism and therefore
> determines the KEDA target; **§2.8.8** walks a real trigger file field by field; **§2.6.1** is the
> graceful-shutdown design with timelines for both the clean and the grace-exceeded case.
>
> **§8.1–§8.3 and §9.1 are the deployment contract** — the `Deploy-WwQueueProcessor.ps1` parameter
> surface and the three ways a trigger file is pointed at it, copy-paste examples for one trigger and for
> a whole folder, the `-DeployRabbitMqTriggers` toggle that makes the engine deploy fan out over every
> pointed trigger, and the new runbook §8.

---

## 0. Scope and non-goals

**In scope**
- A new **Linux container** worker `Dev/Warewolf.Execution.QueueProcessor/` (.NET 8, generic `IHost` +
  `BackgroundService`) that:
  - consumes its RabbitMQ queue with a **new, worker-local async consumer** on **`RabbitMQ.Client` 7.x**
    (async `IChannel`, .NET 8-native), implementing the **existing** `Warewolf.Interfaces` abstractions
    (`IConsumer`, `IStreamConfig`, `IPublisher`, `IQueueConnection`) so semantics stay comparable
    (§1.6 explains why the shared 5.1.2 driver cannot be reused **and** upgraded);
  - resolves its trigger definition from `Settings/triggers/*.bite` and its broker connection from a
    `Settings/{RabbitMQSource}.bite` `ConnectionString` — parsed exactly as
    [`ElasticsearchLoggingOptions.FromBiteFile`](../Logging/ElasticsearchLoggingOptions.cs#L73-L124)
    already does in the engine;
  - maps each message to workflow inputs with the **existing** `MessageToInputsMapper`;
  - **POSTs** to the engine's `/Secure/{workflow}.json` route authenticated by the container app's
    **system-assigned managed identity** + Entra app role `Warewolf_QueueProcessor`;
  - logs through **`Dev2Logger`** with `ExternalSink` bound to the engine's `CompositeExecutionLogger`
    (Console + App Insights + Elasticsearch + Audit), replacing the WebSocket audit publisher;
  - preserves today's **dead-letter-then-ack** semantics for business failures and
    **no-ack → redelivery** for transport failures;
  - handles **SIGTERM** deterministically (stop consuming → drain in-flight → exit).
- **One Container App per queue-trigger**, all sharing **one ACA environment**.
- **KEDA `rabbitmq` scale rule** per app: `minReplicas = 0`, `maxReplicas = N`.
- Deployment: `Dockerfile` + ACR + **`Deploy-WwQueueProcessor.ps1`** (pointed at a trigger file, a folder
  of trigger files, or a manifest — §8.1/§8.2) + Pester tests, mirroring
  [`Deploy-WwJobProcessor.ps1`](../Scripts/Deploy-WwJobProcessor.ps1); plus a **`-DeployRabbitMqTriggers`
  companion toggle** on [`Deploy-WwExecutionEngine.ps1`](../Scripts/Deploy-WwExecutionEngine.ps1) that
  fans the child script out over every pointed trigger (§8.3), and a new **§8** in
  [`Deploy-EndToEnd-Runbook.md`](Deploy-EndToEnd-Runbook.md) (§9.1).

**Non-goals**
- **No change to any existing *code* path.** The two existing *deployment* artefacts that do change —
  `Deploy-WwExecutionEngine.ps1` (additive, inert without the new switch) and
  `Deploy-EndToEnd-Runbook.md` — are listed with their test/doc obligations in §8.
- **Untouched:** `Dev/Warewolf.QueueWorker/`,
  [`Dev2.Server/QueueProcessorMonitor.cs`](../../Dev2.Server/QueueProcessorMonitor.cs),
  [`Warewolf.Driver.RabbitMQ`](../../Warewolf.Driver.RabbitMQ/RabbitConnection.cs), the three RabbitMQ
  activities, and the shared `RabbitMQ.Client` package version all stay exactly as they are
  (decision #7 — "keep existing functions as is").
- **No broker migration** — RabbitMQ retained; no Azure Service Bus.
- **No `QueueWorker.exe` lift-and-shift** — rejected with evidence in §1.5.
- No exactly-once or ordered delivery (RabbitMQ competing consumers are at-least-once, unchanged).
- No Windows containers (ACA is Linux-only).

---

## 1. Analysis (evidence + confidence)

### 1.1 How the RabbitMQ Publish and Consume **activities** work

In-workflow activities, separate from the queue-trigger path. Both resolve a `RabbitMQSource` from the
resource catalog and open their own short-lived connection.

**Publish** — [`PublishRabbitMQActivity.PerformExecution`](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L159-L242):
1. Resolves the source by `RabbitMQSourceResourceId`, with an `AmbientSourceLoader` fallback (L168-175)
   → `ErrorResource.RabbitSourceHasBeenDeleted` if unresolved.
2. Requires evaluated `QueueName` + `Message` (L182-186).
3. Builds the `ConnectionFactory` from **exactly five source fields** — `HostName`, `Port`, `UserName`,
   `Password`, `VirtualHost` (L188-192). **No TLS, no `Ssl` option.** *This is the connection shape the
   container worker must reproduce (decision #3/#5).*
4. Idempotent topology: `ExchangeDeclarePassive` → on `OperationInterruptedException`
   `ExchangeDeclare(Direct, durable, autoDelete)`; same passive-probe for the queue; `QueueBind` only if
   either was newly created (L198-224).
5. Publishes `Persistent = true` with `CorrelationId = GetCorrelationID()` (L226-229).
6. `GetCorrelationID()` (L244-265): `Manual` → evaluate the expression; `CustomTransactionID` (or any
   non-empty `dataObject.CustomTransactionID`) → that value; else `dataObject.ExecutionID`.

[`DsfPublishRabbitMQActivity`](../../Dev2.Activities/Activities/RabbitMQ/Publish/DsfPublishRabbitMQActivity.cs#L134-L189)
is the legacy variant: identical connection shape, but it **always** re-declares exchange/queue/bind (no
passive probe — a re-declare with different arguments against an existing queue fails) and sets no
`CorrelationId`.

**Consume** — [`DsfConsumeRabbitMQActivity`](../../Dev2.Activities/Activities/RabbitMQ/Consume/DsfConsumeRabbitMQActivity.cs#L179-L371):
same connection shape (L214-226); `Prefetch` empty/0 becomes **`ushort.MaxValue`** (L249-253); three
drain modes — **ReQueue** (`BasicGet`, never acked, L257-282), **TimeOut**
(`QueueingBasicConsumer.Queue.Dequeue(timeoutMs, …)`, L336-371), **default** (`MessageCount` snapshot
then dequeue, `BasicAck(lastTag, multiple: prefetch != 1)`, L291-329); outputs go to a scalar, a
recordset, or `ResponseManager`, and the message's `CorrelationId` is written to
`dataObject.CustomTransactionID` (L424) — which a downstream Publish then picks up.

*Confidence: High. Implication: the activities are **unaffected** by this migration; the only shared
concern is the no-TLS connection shape (§1.7).*

### 1.2 How `QueueWorker.exe` consumes a queue and processes messages

`Dev/Warewolf.QueueWorker` is a **`WinExe`, net8.0** assembly named `QueueWorker`. One process = one
trigger.

1. **Args** — `-c <triggerId>` (required), `-v`, `-s <serverUrl>` (defaults to
   `http://{Dns.GetHostName()}:{WebServerPort}`) — [CommandLineArguments.cs:26-48](../../Warewolf.QueueWorker/CommandLineArguments.cs#L26-L48).
2. **Connects to the Warewolf Server over SignalR**; on failure it **exits 0 without consuming** —
   [Program.cs:94-110](../../Warewolf.QueueWorker/Program.cs#L94-L110).
3. **Loads the trigger** from `{QueueTriggersPath}\{triggerId}.bite` via `TriggersCatalog`, which
   **DPAPI-decrypts** it ([WorkerContext.cs:51-60](../../Warewolf.QueueWorker/WorkerContext.cs#L51-L60),
   [TriggersCatalog.cs:274-280](../../Dev2.Runtime.Services/Hosting/TriggersCatalog.cs#L274-L280)),
   deriving `WorkflowUrl = {server}/secure/{WorkflowName}.json`, `Username`/`Password`, the RabbitMQ
   `Source` and dead-letter `Sink`, `QueueConfig`/`DeadLetterConfig`, `Inputs`, `MapEntireMessage`
   ([WorkerContext.cs:78-139](../../Warewolf.QueueWorker/WorkerContext.cs#L78-L139)).
4. **Hot reload by suicide** — a `FileSystemWatcher` on the trigger file: `Created` → `Exit(1)`,
   `Changed`/`Deleted`/`Renamed` → `Exit(0)`; the supervisor restarts it
   ([Program.cs:128-136](../../Warewolf.QueueWorker/Program.cs#L128-L136)).
5. **Pipeline** ([Program.cs:224-256](../../Warewolf.QueueWorker/Program.cs#L224-L256)):
   `RabbitConnection.StartConsuming(config, LoggingConsumerWrapper(WarewolfWebRequestForwarder))`.
   The forwarder maps the body, POSTs with **Windows-integrated/basic `NetworkCredential`** and an
   **infinite timeout**, and on non-success publishes the mapped body to the dead-letter sink
   ([WarewolfWebRequestForwarder.cs:58-68, 94-111](../../Warewolf.Common.Framework48/WarewolfWebRequestForwarder.cs#L58-L111),
   [HttpClientFactory.cs:27-66](../../Warewolf.Common/HttpClientFactory.cs#L27-L66)). A single `@object`
   input with `MapEntireMessage` is sent as `multipart/form-data` (L100-108). Dead-letter publishing
   **opens a fresh connection + channel per failure** ([Program.cs:258-280](../../Warewolf.QueueWorker/Program.cs#L258-L280)).
6. **The loop** — [`RabbitConnection.StartConsuming`](../../Warewolf.Driver.RabbitMQ/RabbitConnection.cs#L43-L110):
   `EventingBasicConsumer`, `autoAck:false`, `BasicAck` only on `ConsumerResult.Success` (L65-68); an
   in-process `SemaphoreSlim(ProcessorCount × 5)` (L48-49, 61-73); a `ConsumerCancelled` latch (L75-80);
   and a **10-minute watchdog `Timer`** that `QueueDeclarePassive`s and **throws on the timer thread**
   → process dies → supervisor restarts (L84-109). QoS/topology come from
   [`RabbitConfig.CreateChannel`](../../Warewolf.Driver.RabbitMQ/RabbitConfig.cs#L31-L42).

**Exact failure semantics today — the parity contract this migration must preserve:**

| Outcome | Path | Broker effect |
|---|---|---|
| 2xx | forwarder `Success` → wrapper `Success` | **acked**, removed |
| Non-2xx | forwarder dead-letters the mapped body and returns `Failed`; the wrapper logs the error and still returns **`Success`** ([LoggingConsumerWrapper.cs:64-79](../../Warewolf.QueueWorker/LoggingConsumerWrapper.cs#L64-L79)) | **acked** — no redelivery; the copy lives in the dead-letter queue |
| Exception (unreachable, credential failure, mapping throw) | wrapper catch → `Failed` (L81-90) | **not acked** → redelivered when the channel/connection drops |
| Queue deleted / consumer cancelled | watchdog throws | process crash → restart → re-declare + reconsume |

*Confidence: High — including the non-obvious "business failure is acked".*

### 1.3 How **multiple instances** are run

A Server-hosted process supervisor, structurally identical to `hangfireserver.exe`:

- `QueueWorkerMonitor` is created at [ServerLifecycleManager.cs:105](../../Dev2.Server/ServerLifecycleManager.cs#L105),
  started at `:274`, shut down at `:524`; configs come from `TriggersCatalog.Instance.Queues`
  ([QueueConfigLoader.cs:17-29](../../Dev2.Server/QueueConfigLoader.cs#L17-L29)); it reacts to
  `OnChanged` (kill + recreate), `OnDeleted`, `OnCreated`
  ([QueueProcessorMonitor.cs:34-51](../../Dev2.Server/QueueProcessorMonitor.cs#L34-L51)).
- **Breadth:** one `ProcessThreadList` per trigger; `Concurrency == 0` ⇒ the trigger is skipped entirely
  ([WorkerMonitor.cs:46-64](../../Warewolf.Common/OS/WorkerMonitor.cs#L46-L64)).
- **Depth:** `Concurrency` processes, **hard-capped at `Environment.ProcessorCount`**
  ([ProcessThreadList.cs:101-111](../../Warewolf.Common/OS/ProcessThreadList.cs#L101-L111)), reconciled
  by a 1-second loop ([WorkerMonitor.cs:86-101](../../Warewolf.Common/OS/WorkerMonitor.cs#L86-L101),
  [ProcessThreadList.cs:69-99](../../Warewolf.Common/OS/ProcessThreadList.cs#L69-L99)).
- Each process is `QueueWorker.exe -c "{triggerId}"`
  ([QueueProcessorMonitor.cs:78-82](../../Dev2.Server/QueueProcessorMonitor.cs#L78-L82)), started on its
  own thread with stdout/stderr piped to `WarewolfLogger` and tracked by a job object
  ([ProcessMonitor.cs:78-133](../../Warewolf.Common/OS/ProcessMonitor.cs#L78-L133)).
- Same-queue parallelism is pure **RabbitMQ competing consumers**.

**Effective parallelism.** `N = min(Concurrency, ProcessorCount)` processes × per-process in-flight. The
per-process figure is the subtle part: the delivery handler **blocks** (`resultTask.Wait()`,
[RabbitConnection.cs:61-73](../../Warewolf.Driver.RabbitMQ/RabbitConnection.cs#L61-L73)) and
`RabbitMQ.Client` **5.1.2** dispatches a channel's consumer callbacks **sequentially**, so the
`SemaphoreSlim(ProcessorCount × 5)` is effectively never contended and real per-process concurrency is
**1 workflow at a time**; prefetch only buffers locally. ⇒ **the throughput knob today is the process
count.** *(High confidence on the blocking handler and the pinned version; medium-high on serial
dispatch — Phase 0 measures it, because it sets the KEDA parity target `maxReplicas ≈ Concurrency`.)*

### 1.4 Why ACA + KEDA is a near-exact structural match

| Today (Server + `.exe`) | ACA + KEDA |
|---|---|
| `WorkerMonitor` 1-second restart loop | ACA replica restart policy |
| `Concurrency` processes capped at `ProcessorCount` | `maxReplicas` (per app, not CPU-capped — an intentional improvement) |
| `Concurrency == 0` ⇒ not run | `min=max=0` / app stopped |
| Broker round-robins across processes | Broker round-robins across **replicas** — identical semantics |
| Trigger file change → `Exit` → restart | New **ACA revision** → rolling replica replacement |
| Server catalog + SignalR as config source | Staged `Settings/*.bite` + env vars + ACA secrets |
| Watchdog crash → supervisor restart | Container exit → replica restart |
| Idle worker still holds a process | **Scale to zero — no charge while the queue is empty** |
| Manual `Concurrency` capacity choice | **Queue-length autoscale** `ceil(queueLength / value)` |

### 1.5 Why `QueueWorker.exe` cannot simply be containerized

Five blockers, each observed in code:

1. **DPAPI is Windows-only.** [`DpapiWrapper`](../../Warewolf.Security/Encryption/DPAPIWrapper.cs#L20-L146)
   uses `ProtectedData` with `DataProtectionScope.LocalMachine` — `PlatformNotSupportedException` on
   Linux. Both the trigger `.bite` and the `RabbitMQSource` `ConnectionString` are DPAPI blobs on-prem.
   The existing seam is `AesDecryptHook`/`AesEncryptHook` for `WFAES::` values
   ([DPAPIWrapper.cs:28-37, 99-101, 132-133](../../Warewolf.Security/Encryption/DPAPIWrapper.cs#L28-L37))
   — the same mechanism the Hangfire migration used. Note the asymmetry: `CanBeDecrypted` swallows the
   platform exception and returns `false`, but `Decrypt` **throws** — so a DPAPI value reaching the
   container fails loudly, not silently (good, but it must be guarded with a clear message).
2. **The Server dependency is structural** — no Server, no start; sources come through the Server's
   catalog proxy ([WorkerContext.cs:85-113](../../Warewolf.QueueWorker/WorkerContext.cs#L85-L113)).
3. **Win32 P/Invoke on the `-v` path** — `ConsoleWindow` calls `kernel32!AllocConsole`
   ([ConsoleWindow.cs:20-42](../../Warewolf.Common/ConsoleWindow.cs#L20-L42)); `OutputType` is `WinExe`.
4. **Audit is tethered to the on-prem logging service** — `ExecutionLogger : NetworkLogger` publishes
   over a WebSocket pool and reads `Config.Server.ExecutionLogLevel` from a Server settings file
   ([NetworkLogger.cs:31-43](../../Warewolf.Common.NetStandard20/Logger/NetworkLogger.cs#L31-L43)).
5. **Auth model mismatch** — basic/Windows `NetworkCredential` vs the engine's Entra bearer token; and
   `WarewolfWebRequestForwarder` lives in `Warewolf.Common.Framework48`, whose closure includes
   `Dev2.Studio.Core`/`Dev2.Studio.Interfaces`
   ([Warewolf.Common.Framework48.csproj:25-31](../../Warewolf.Common.Framework48/Warewolf.Common.Framework48.csproj#L25-L31)).

### 1.6 Why the worker ships a **new** consumer instead of reusing the driver *(decision #7)*

The earlier draft of this plan proposed reusing `RabbitConnection` verbatim. Confirming decision #7
("make it .NET 8 compatible; keep existing functions as is if a new consumer is to be used") makes that
impossible, for a concrete reason:

- `Warewolf.Driver.RabbitMQ` is pinned to **`RabbitMQ.Client` 5.1.2**
  ([Warewolf.Driver.RabbitMQ.csproj:18](../../Warewolf.Driver.RabbitMQ/Warewolf.Driver.RabbitMQ.csproj#L18)),
  a 2019 release: synchronous `IModel`, no async consumers, no `ConsumerDispatchConcurrency`, dated TLS
  defaults.
- **Upgrading it is not additive.** `QueueingBasicConsumer` — used by `DsfConsumeRabbitMQActivity`
  (L301, L338) — was **removed in `RabbitMQ.Client` 6.0**, and 7.x further replaces `IModel` with
  `IChannel` and makes the API async-first. A shared upgrade therefore forces edits to shipped activity
  code, which decision #7 forbids.
- **A mixed reference is also impossible.** `Dev2.Data` project-references `Warewolf.Driver.RabbitMQ`
  *and* the `RabbitMQ.Client` package
  ([Dev2.Data.csproj:307, 350](../../Dev2.Data/Dev2.Data.csproj#L307-L350)), so any project pulling
  `Dev2.Data` gets 5.1.2; NuGet unifies to the highest version in the app graph, and types compiled
  against 5.1.2 would fail to load against 7.x.

⇒ **The worker references neither `Warewolf.Driver.RabbitMQ` nor `Dev2.Data`.** It implements a
worker-local `Infrastructure/Rabbit/` consumer + publisher on **`RabbitMQ.Client` 7.x** (decision #24),
against the **same `Warewolf.Interfaces` abstractions** (`IConsumer`, `IStreamConfig`, `IPublisher`,
`IQueueConnection` —
[Warewolf.Interfaces.csproj:3-27](../../Warewolf.Interfaces/Warewolf.Interfaces.csproj#L3-L27): net8.0,
Fleck + Newtonsoft only, **zero project references**, no RabbitMQ dependency). Everything existing stays
on 5.1.2 and is not touched.

Choosing 7.x rather than the 6.8.x line (which would have kept the synchronous `IModel` /
`EventingBasicConsumer` shape for easier line-by-line comparison) buys three things that matter here:
`IChannel` with **async consumers** — so the blocking `resultTask.Wait()` pattern
([RabbitConnection.cs:61-73](../../Warewolf.Driver.RabbitMQ/RabbitConnection.cs#L61-L73)) disappears
rather than being reproduced; **`ConsumerDispatchConcurrency`**, which is what makes
`WORKER__MAXCONCURRENCY > 1` genuinely implementable instead of theoretical (§1.3 shows it is
unreachable today); and current TLS defaults for the §1.7 AMQPS gate. The cost is that parity with the
old loop is reviewed **by behaviour** (the Phase 3 parity suite) rather than by diff — which is why that
suite is an explicit acceptance criterion. *(High confidence on the version-conflict mechanics; the exact
7.x patch level is pinned in the Phase 0 spike report.)*

### 1.7 Connection shape, `.bite` loading, and the TLS reality *(decisions #3, #5, note 1)*

The confirmed source format is a plain Warewolf source `.bite`:

```xml
<Source ID="1a82a341-…" Name="Warewolf DevOps RabbitMQ Source" ResourceType="RabbitMQSource"
        ConnectionString="HostName=server.ngrok.io;Port=20313;UserName=testuser;Password=test123;VirtualHost=/"
        Type="RabbitMQSource" …>
```

Two facts follow, and the second is a security finding:

1. **The engine already has the exact reader for this.**
   [`ElasticsearchLoggingOptions.FromBiteFile`](../Logging/ElasticsearchLoggingOptions.cs#L73-L124)
   loads the `XElement`, reads `ConnectionString`, calls `CanBeDecrypted()` → `DpapiWrapper.Decrypt`
   (so a `WFAES::` value works once the Key Vault hook is wired, and a plaintext value passes straight
   through), then splits on `;` and the first `=`. The worker's `RabbitMqSourceOptions.FromBiteFile`
   is the same code shape over `HostName;Port;UserName;Password;VirtualHost` — **no `Dev2.Data`
   reference, no `RabbitMQSource` type, no RabbitMQ.Client pin.**
2. **This connection string carries no TLS, and neither does the activity.** The example is a plain
   AMQP endpoint on an ngrok TCP tunnel (port 20313, not 5671) with the password in cleartext, and
   [`PublishRabbitMQActivity`](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L188-L192)
   never sets `ConnectionFactory.Ssl`. **Reproducing it faithfully means credentials and message bodies
   cross the public internet unencrypted.** The plan therefore implements TLS as an **opt-in that
   defaults off** (byte-parity with the activity), enabled by any of: a `UseSsl=true` token in the
   connection string, `Port == 5671`, or the `RABBITMQ__USESSL` env override.

   **Resolved posture (decision #25): parity now, AMQPS gate before production.** Dev and test connect
   exactly as the activity does today, using the supplied source unchanged — so no broker work blocks
   Phases 0–9. Before the **first production trigger is enabled** (the Phase 11 gate), the broker or
   tunnel must terminate TLS and the app must run with `RABBITMQ__USESSL=true` against an `amqps`
   endpoint. That is a hard go/no-go, not a recommendation.
   *(High confidence — both observations are direct code/config reads.)*

**Trigger `.bite` files** ([note 1](#5-decisions-log)) are DPAPI-encrypted **JSON** written by
[`TriggersCatalog.SaveTriggerQueue`](../../Dev2.Runtime.Services/Hosting/TriggersCatalog.cs#L293-L305)
through `Dev2JsonSerializer`, which uses `TypeNameHandling.Objects` +
`PreserveReferencesHandling.Objects`
([Dev2JsonSerializer.cs:26-42](../../Dev2.Infrastructure/Communication/Dev2JsonSerializer.cs#L26-L42)) —
so the payload contains `$type`/`$id`/`$ref` tokens naming concrete assemblies
(`Warewolf.Trigger.Queue.TriggerQueue`, the `Warewolf.Options` option types, the input types).
Referencing `Warewolf.Trigger.Queue` to deserialize them is not viable: it drags `Dev2.Data`,
`Dev2.Infrastructure`, `Dev2.Core` and `Warewolf.UI`
([Warewolf.Trigger.Queue.csproj:22-29](../../Warewolf.Trigger.Queue/Warewolf.Trigger.Queue.csproj#L22-L29))
— and therefore RabbitMQ.Client 5.1.2 again (§1.6). Two viable readers, both assembly-free:

- **A (recommended)** — a `TriggerBiteReader` using Newtonsoft with `TypeNameHandling.Auto` and a custom
  `ISerializationBinder` mapping the handful of known `$type` names onto worker-local DTOs. The staged
  file stays **byte-identical** to what the Server writes (only re-encrypted DPAPI → WFAES).
- **B (fallback)** — deploy-time **transcode**: the deploy script (running on Windows, where DPAPI and
  the real types are available) emits a flat, versioned `queuetrigger.bite` carrying only the ten fields
  the worker actually consumes, WFAES-encrypted.

Option A is preferred because it keeps one source of truth; Phase 0 validates it against a real file
(`$ref` handling is the risk). *(High confidence on the serializer settings; medium on A's `$ref`
behaviour — hence the spike.)* A **real** trigger definition is walked through field by field in
**§2.8.8**, which confirms the `$id`/`$type` shape and surfaces two further requirements the sample
exposes: the `#{…}` release-substitution token in `Concurrency` (so the file is not valid JSON until the
pipeline substitutes it — the reader and the `maxReplicas` derivation both run *after* substitution) and
the `\` separators in `WorkflowName`.

### 1.8 The logging stack the worker must adopt *(decision #6, note 2)*

The engine's pattern, to be reproduced exactly:

1. `LoggingConfiguration.FromEnvironment()` reads `ENABLECONSOLELOGGING`, `ENABLEAPPLICATIONINSIGHTS`,
   `ENABLEELASTICSEARCHLOGGING`, `EXECUTIONLOGLEVEL`, `STRUCTURED_LOGS`, `ASPNETCORE_ENVIRONMENT`, and
   the `Settings/ElasticsearchLoggingSource.bite` path
   ([LoggingConfiguration.cs:82-104](../Logging/LoggingConfiguration.cs#L82-L104)).
2. A **bootstrap** console sink is installed **first** so no startup log is lost:
   `Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(bootstrapConsoleLogger)` plus
   `Dev2Logger.CorrelationPrefixProvider` ([Program.cs:19-27](../Program.cs#L19-L27)).
3. After startup (once the Key Vault AES hook is wired), the sink is **upgraded** to the full
   `CompositeExecutionLogger` — Console + App Insights + Elasticsearch + Audit
   ([Program.cs:106-118](../Program.cs#L106-L118)).
4. Every `Dev2Logger.Debug/Info/Warn/Error/Fatal` call site in the codebase then flows to those sinks
   unchanged, via [`Dev2LoggerSinkAdapter`](../Logging/Dev2LoggerSinkAdapter.cs#L21-L80).
5. App Insights uses the deliberately non-standard `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`
   ([Program.cs:61-90](../Program.cs#L61-L90)); audit events ride a dedicated always-on
   [`AuditExecutionLogger`](../Logging/AuditExecutionLogger.cs#L21-L50) with `EventId 9000`.

`Dev2Logger` lives in `Dev2.Common`, whose closure contains **no RabbitMQ dependency**
([Dev2.Common.csproj:194-244](../../Dev2.Common/Dev2.Common.csproj#L194-L244)) — so referencing it does
not reintroduce the §1.6 version pin. The sink classes live inside the engine Function App project, so
they are consumed the way the JobProcessor already consumes the Key Vault stack: **linked shared
sources**, not a project reference
([Warewolf.Execution.EngineJobProcessor.csproj:60-72](../../Warewolf.Execution.EngineJobProcessor/Warewolf.Execution.EngineJobProcessor.csproj#L60-L72)).
*(High confidence — all read directly.)*

---

## 2. Target architecture

```
                                Entra ID (tokens, app roles)
                        ┌──────────────────────────────────────────┐
                        │ Engine app registration                  │
                        │ roles: Warewolf_ClientApps,              │
                        │        Warewolf_JobProcessor,            │
                        │        Warewolf_QueueProcessor  ◄── NEW  │
                        └───────────────┬──────────────────────────┘
                                        │ MI token
   RabbitMQ (public endpoint, AMQP today / AMQPS opt-in §1.7)
   ┌──────────────────────────────┐     │ api://<engine>/.default
   │ work queue     ◄──── KEDA poll (queue length)
   │ <queue>.dlq    ◄── app-level dead-letter publish (engine non-2xx)
   │ DLX/poison     ◄── broker-side delivery limit (new safety net)
   └───────┬──────────────────────┘
           │ consume (competing consumers)
           ▼
   ┌──────────────────────────────────────────────────────────────┐
   │ Azure Container Apps environment                              │
   │  ┌────────────────────────────────────────────────────────┐   │
   │  │ Container App: wwqp-<trigger>   (ingress DISABLED)      │   │
   │  │  scale: min 0 / max N, rule type=rabbitmq              │   │
   │  │  identity: system-assigned MI                          │   │
   │  │  replicas 0..N  ── each ≡ one old QueueWorker.exe        │   │
   │  │   ┌──────────────────────────────────────────────┐     │   │
   │  │   │ Warewolf.Execution.QueueProcessor (net8 Linux)│    │   │
   │  │   │  Settings/triggers/*.bite  → trigger config     │    │   │
   │  │   │  Settings/{source}.bite   → broker connection  │    │   │
   │  │   │   1. NEW async consumer (RabbitMQ.Client 7.x)  │    │   │
   │  │   │   2. MessageToInputsMapper (REUSED)            │    │   │
   │  │   │   3. POST engine /Secure/{wf}.json  ───────────┼────┼───┼──► Execution
   │  │   │   4. ack / dead-letter / no-ack                │    │   │    Engine
   │  │   │   5. SIGTERM → stop → drain → exit             │    │   │   (Function App,
   │  │   │   6. Dev2Logger → CompositeExecutionLogger     │    │   │    EasyAuth+Entra)
   │  │   └──────────────────────────────────────────────┘     │   │
   │  └────────────────────────────────────────────────────────┘   │
   │  … one Container App per queue-trigger, same environment …     │
   └──────────────────────────────────────────────────────────────┘
        secrets: keyvaultref → Key Vault (WFAES key, broker creds)
        image:  Azure Container Registry, MI pull (AcrPull)
```

### 2.1 Components

| Component | Hosting | Responsibility |
|---|---|---|
| **`Warewolf.Execution.QueueProcessor`** (new) | ACA Container App, Linux, .NET 8 generic host, one app per queue-trigger, ingress disabled | Consume; map; POST the engine with an MI bearer token; ack / dead-letter / no-ack; graceful drain; `Dev2Logger` telemetry |
| **ACA environment** | One per Warewolf installation (Consumption profile) | Shared runtime; KEDA polls the broker even at 0 replicas |
| **KEDA `rabbitmq` rule** | Per app | `ceil(queueLength / value)` bounded by min/max; scale to zero |
| **Execution Engine** | Existing Function App | `/Secure/{*name}` accepts **GET and POST** at `AuthorizationLevel.Anonymous` with `[RequireWorkflowPermission(View\|Execute)]` ([WorkflowHttpFunction.cs:136-141](../Functions/WorkflowHttpFunction.cs#L136-L141)); **unchanged** except the `secure.config` role grant |
| **RabbitMQ broker** | Existing, public endpoint | Work queue + dead-letter queue (+ DLX); management API only if the scaler needs `protocol=http` |
| **Azure Key Vault** | Existing | WFAES key for the staged `.bite` files; broker credential secret for the KEDA rule |
| **Azure Container Registry** | New or existing | Worker image; MI pull, no admin credentials |

### 2.2 What replaces what

| Today | ACA replacement |
|---|---|
| `QueueWorkerMonitor` + `ProcessThreadList` + `ProcessMonitor` | **Not used in Azure** (kept for on-prem); ACA replicas + KEDA |
| SignalR connect to Server | **Removed** — staged `Settings/*.bite` + env vars |
| `TriggersCatalog` DPAPI `.bite` load | `TriggerBiteReader` over `Settings/triggers/*.bite`, WFAES via the Key Vault hook (§1.7) |
| `RabbitMQSource` (DPAPI, `Dev2.Data`) | `RabbitMqSourceOptions.FromBiteFile` over `Settings/{source}.bite` (§1.7) |
| `RabbitConnection.StartConsuming` (5.1.2) | **New** async consumer on **`RabbitMQ.Client` 7.x**, same `IConsumer`/`IStreamConfig` contracts (§1.6) |
| `MessageToInputsMapper` | **REUSED** unchanged ([MessageToInputsMapper.cs:26-58](../../Warewolf.Common/MessageToInputsMapper.cs#L26-L58)) |
| `WarewolfWebRequestForwarder` (basic auth, `Framework48`) | New `EngineForwarder : IConsumer` — same mapping + multipart rule, MI bearer auth, bounded timeout |
| `HttpClientFactory` + `NetworkCredential` | Typed `HttpClient` + [`WwExecutionTokenHandler`](../../Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/Auth/WwExecutionTokenHandler.cs#L24-L134) |
| `LoggingConsumerWrapper` → WebSocket audit | `AuditingConsumerDecorator` → `Dev2Logger` → `CompositeExecutionLogger` (§1.8) |
| Trigger-file watcher `Exit` | New ACA revision (rolling update) |
| 10-min watchdog crash → restart | Equivalent watchdog in the new consumer + container exit → replica restart |
| `ChildProcessTracker` job object | Not needed (container boundary) |

### 2.3 Configuration contract

Staged files under `{AppContext.BaseDirectory}/Settings/` (read-only, never written back — the
run-from-package discipline of Hangfire decision #10/#11):

| File | Content |
|---|---|
| `Settings/triggers/{TriggerId}.bite` | Queue-trigger definition(s), **WFAES-encrypted** at deploy time |
| `Settings/sources/{QueueSourceId}.bite` | Broker source; `ConnectionString` plaintext or WFAES (§1.7) |
| `Settings/sources/{QueueSinkId}.bite` | Dead-letter sink source, staged **only when it differs** from the work source |
| `Settings/ElasticsearchLoggingSource.bite` | Same file the engine stages, for the Elasticsearch sink |

**Delivery: baked into the image.** `Deploy-WwQueueProcessor.ps1` stages the tree above into a
fresh temp directory and `az acr build` bakes it in — no Azure Files mount, no blob fetch at
startup. The deciding constraint is `minReplicas = 0`: any mount or download is paid on **every**
0→1 scale and adds a dependency that can stop a replica starting at all. The image tag therefore
pins the config version, and a trigger edit means a new revision — which is the audit trail.
Alternatives were evaluated and rejected for this worker: an Azure Files mount (change triggers
without a rebuild, but SMB on every cold start), blob + MI at startup (central rotation, but a
network hop per scale-from-zero and a new code path), and mount-overrides-image (two sources of
truth, and a mount silently shadows the baked folder).

Sources sit in **their own folder**, not the `Settings/` root, so a source file can never be
mistaken for a trigger during discovery or the reverse. `Settings/` is still searched for sources
**after** `Settings/sources/`, so a deployment staged under the earlier flat layout keeps starting.
Within each folder both `{sourceId}.bite` (what the deploy writes) and an operator-named Studio
file (matched by scanning for the `ID` attribute) resolve.

**Sources are read once at startup and cached** by `RabbitMqSourceCatalog` (registered as a
singleton, so the scan is part of cold start). Every `*.bite` under the probe folders is parsed and
indexed by its `ID` attribute; the trigger's `QueueSourceId` / `QueueSinkId` are then in-memory
lookups. Three reasons this is a cache and not a per-reference lookup:

1. **No repeated parsing.** A trigger references a source twice, and the old resolution re-read and
   re-parsed every file per reference.
2. **The source files stop being a live dependency.** Nothing re-reads a `.bite` after startup, so
   a broker reconnect or a dead-letter publish cannot fail on a removed or half-written file
   *mid-message* — the failure mode moves to cold start, where it is attributable.
3. **Fail fast, with the whole picture.** A missing id reports every folder searched *and* every id
   that is present, rather than a bare "not found" that sends operators digging through an image
   layer. Non-RabbitMQ `.bite` files in the tree (the Elasticsearch source, trigger files) are
   skipped rather than aborting the scan, and a duplicated id keeps the first — deterministic,
   because `sources/` is probed before the legacy root.

**The deploy resolves every referenced source at plan time.** `QueueSourceId` and `QueueSinkId` for
*all* triggers are resolved, listed with the triggers that reference them, and any gap aborts the
run before a single Container App is created. Previously a missing source for trigger #3 surfaced
only after #1 and #2 had been deployed, leaving a half-deployed fleet; a run is now all-or-nothing.
A sink-source gap deserves particular care — it yields a replica that starts and works until the
first failure, then cannot dead-letter.

Environment variables / ACA secrets:

| Setting | Notes |
|---|---|
| `QUEUE__SETTINGSPATH` | Root of the staged tree. Default `{AppContext.BaseDirectory}/Settings` — **base directory, never `Environment.CurrentDirectory`**, because cwd differs between `dotnet run`, the container (`WORKDIR /app`) and a debugger launch |
| `QUEUE__TRIGGERSSUBPATH` | Trigger sub-folder, default `triggers`. Absolute paths are honoured as-is |
| `QUEUE__SOURCESSUBPATH` | Source sub-folder, default `sources`. Absolute paths are honoured as-is |
| `QUEUE__TRIGGERFILTER` | Trigger glob, default `*.bite` |
| `QUEUE__TRIGGERID` | Selects the active trigger when more than one `Settings/triggers/*.bite` is staged; optional when exactly one is present |
| *(prefetch — no env var)* | Read by the worker straight from the staged trigger's `Prefetch` (empty/`< 1`/unparseable → **1**; `0` would mean *unlimited* in AMQP) and applied as `BasicQos(prefetchCount)`. Deliberately **not** an environment variable, so the trigger stays the single source of truth; it is also the basis of the KEDA `value` (§2.8.7) |
| `QUEUE__DEADLETTERNAME` | From the trigger's `DeadLetterQueue` |
| `MAPPING__ENTIREMESSAGE`, `MAPPING__INPUTS` | From `MapEntireMessage` / `Inputs` |
| `ENGINE__BASEURL`, `ENGINE__WORKFLOW`, `ENGINE__RESOURCEAPPID` | Engine address, workflow name (`\` normalised to `/` — §2.8.8), token audience `api://{ResourceAppId}/.default` |
| `ENGINE__TENANTID` | **Set explicitly by the deploy** (`-EngineTenantId`, defaulted from `az account show`, prompted if that yields nothing). Blank is legal *only* for a system-assigned managed identity, where the tenant is implied; for a user-assigned identity, the local az CLI credential or a client secret, a blank tenant makes every credential in the chain fail with *"Invalid tenant id provided"* — which surfaces at the **first message** and reads like a missing app role. The worker warns at startup when it is absent, and `QueueProcessorOptions.EffectiveTenantId` normalises blank → `null` so the platform can still infer it |
| `ENGINE__TIMEOUTSECONDS` | **Replaces today's infinite timeout** (§2.6); must satisfy `ENGINE__TIMEOUTSECONDS ≤ WORKER__SHUTDOWNGRACESECONDS < terminationGracePeriodSeconds` (§2.6.1) and stay under the engine's `functionTimeout` |
| `RABBITMQ__USESSL` | TLS override (§1.7); default derives from the connection string / port |
| `WORKER__MAXCONCURRENCY` | In-replica in-flight cap; **default 1** = today's measured behaviour (§1.3) |
| `WORKER__SHUTDOWNGRACESECONDS` | Must be ≤ the app's `terminationGracePeriodSeconds` |
| `KEYVAULT__*` | Same variables the engine/JobProcessor use for the AES key |
| `ENABLECONSOLELOGGING`, `ENABLEAPPLICATIONINSIGHTS`, `ENABLEELASTICSEARCHLOGGING`, `EXECUTIONLOGLEVEL`, `STRUCTURED_LOGS`, `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` | **Identical names and semantics to the engine** (§1.8) |
| `rabbitmq-connection` (ACA secret, Key Vault ref) | Consumed by the **KEDA scale rule only** — KEDA cannot use a managed identity against RabbitMQ |

> **Empty string means UNSET.** Every reader above goes through `ReadString`/`IsNullOrWhiteSpace`,
> never `??`. This is not stylistic: `appsettings.json` ships several keys as `""` placeholders so
> the contract is discoverable, and ACA/Docker turn an unset variable into an empty string as
> readily as into a missing one. With `??`, `QUEUE__SETTINGSPATH=""` won over the intended default
> and silently made the settings folder relative to the process working directory. The same defect
> class bit `ENGINE__TENANTID` — an empty tenant makes **every** credential in the chain fail with
> *"Invalid tenant id provided"*, and blank is the documented default for a system-assigned
> managed identity, so it would have failed in Azure exactly as it did locally.

#### 2.3.1 Queue durability comes from the trigger, never from an assumption

A queue is durable or not **according to the trigger**, and the two option lists are independent:

| Trigger field | Drives |
|---|---|
| `Options[Durable].Value` | `QueueDeclare(durable:)` for the **work** queue |
| `DeadLetterOptions[Durable].Value` | `QueueDeclare(durable:)` for the **dead-letter** queue |

The same applies to `Exclusive` and `AutoDelete`. Conflating the two lists, or falling back to the
option's `Default` field instead of its `Value`, makes the broker reject the re-declare with
`PRECONDITION_FAILED — inequivalent arg 'durable'` and the replica **cannot consume at all** (risk
**R5**, reproduced live during Phase 0 with a `durable:true` declare against a non-durable queue).
Undeclared options resolve to `false`, not to `Default`. Pinned by
`Trigger_DurableIsReadIndependentlyForTheWorkQueueAndTheDeadLetterQueue`,
`ResolvedConfiguration_SurfacesTheTwoDurabilityFlagsSeparately` and
`Trigger_UndeclaredDurableDefaultsToFalseRatherThanTheOptionDefaultField`, over a fixture whose
work queue is durable + exclusive while its dead-letter queue is not.

#### 2.3.2 Optimum runtime settings

| Setting | Optimum | Why |
|---|---|---|
| `minReplicas` | **0** | Scale to zero between bursts; the whole point of the ACA move |
| `maxReplicas` | **= trigger `Concurrency`** | Preserves the on-prem ceiling exactly. Dispatch is serial per channel (measured: deliveries ~2.2 s apart, no overlap), so one replica ≈ one workflow — **not** `× ProcessorCount` |
| `WORKER__MAXCONCURRENCY` | **1** | The measured on-prem behaviour. Raising it is a deliberate throughput change and needs `Prefetch` raised to match |
| trigger `Prefetch` | **= `MaxConcurrency`** (so 1) | A prefetch above the in-flight cap **parks** messages in one replica where no other replica can take them. That costs twice: KEDA sees fewer available messages so scale-out is **delayed** (`value = Prefetch × MaxConcurrency`), and a drain has more buffered messages to nack — a wider redelivery window |
| KEDA `value` | **1** | Falls out of `Prefetch × MaxConcurrency`; `replicas = ceil(queueLength / value)` capped at `maxReplicas` |

The rule in one line: **scale OUT, not UP.** `Prefetch = 0` would mean *unlimited* in AMQP — one
replica would take the whole queue and defeat autoscaling entirely — so it is coerced to 1, as are
null/blank/unparseable values. Prefetch above `MaxConcurrency` is reported as an advisory by both
the deploy (plan time) and the worker (cold start); it is not fatal, because the trigger stays the
single source of truth.

#### 2.3.3 Committed local-development sample

`Dev/Warewolf.Execution.QueueProcessor/Settings/` carries a working sample so a fresh clone runs
without hand-assembling config:

```
Settings/sources/Warewolf DevOps RabbitMQ Source.bite   ID 1a82a341-…, the shared dev broker
Settings/triggers/03fb9052-….bite                       'order-queue' → 'hangfiredemo/Hello World'
```

Three properties of this sample are deliberate:

- **Copied to build output, never to publish output** (`CopyToPublishDirectory=Never`). `dotnet
  publish` feeds the container image, and this source holds a plaintext dev broker password.
  Excluding it stops a secret entering an image layer *and* stops a real deployment silently
  falling back to the dev broker if the deploy script failed to stage its own files.
- **The operator-named file exercises the ID-scan branch** rather than the `{QueueSourceId}.bite`
  path the deploy script writes. Both stay covered.
- **`Durable=false`**, matching the live `order-queue` — see §2.3.1.

`docker-compose.yml` mounts `./Settings.local/` (git-ignored), **not** this folder, because compose
brings up its own broker — mounting the committed sample would bypass it and consume from the
shared dev queue.

### 2.4 Scaling: KEDA rule and parity math

```bash
az containerapp create \
  --name wwqp-<trigger-slug> --resource-group <rg> --environment <aca-env> \
  --image <acr>.azurecr.io/warewolf/queueprocessor:<digest> \
  --registry-server <acr>.azurecr.io --registry-identity system \
  --system-assigned --ingress disabled \
  --min-replicas 0 --max-replicas <N> \
  --secrets "rabbitmq-connection=keyvaultref:https://<kv>.vault.azure.net/secrets/<name>,identityref:system" \
  --env-vars "QUEUE__TRIGGERID=<guid>" "ENGINE__BASEURL=https://<engine>" ... \
  --scale-rule-name rabbitmq-len --scale-rule-type rabbitmq \
  --scale-rule-metadata "queueName=<queue>" "mode=QueueLength" "value=<K>" "protocol=amqp" \
  --scale-rule-auth "host=rabbitmq-connection"
```

**Parity math.** Today's ceiling is `min(Concurrency, ProcessorCount)` concurrent invocations per
trigger (§1.3), so:
- `maxReplicas = Concurrency` — the `ProcessorCount` cap disappears, so `maxReplicas` is honoured in full;
  raising it *above* `Concurrency` is an exception path (decision #23). **Fully worked, with two real
  triggers, the scaling modes, and the production `az` rules: §2.8.**
- `value` = target messages per replica: `replicas = clamp(ceil(queueLength / value), min, max)` — so the
  ceiling is never exceeded and a small backlog runs proportionally fewer replicas. **`value` is derived
  from the trigger, not guessed: `value = Prefetch × WORKER__MAXCONCURRENCY`** — the number of messages
  one replica actually claims. §2.8.7 explains why ignoring `Prefetch` here over- or under-provisions.
- `WORKER__MAXCONCURRENCY = 1` for exact parity. Because the new client is async-first, raising it is
  now a *real* option (unlike today) — but it is a deliberate throughput change requiring the workflow
  to be concurrency-safe, and `Prefetch` must then be ≥ `WORKER__MAXCONCURRENCY`.
- `Prefetch` comes straight from the trigger (`BasicQos(prefetchCount, global: false)`, per consumer) and
  is the **granularity of work distribution** — it bounds how much backlog one replica absorbs, and
  therefore how many replicas can usefully run (§2.8.7).

**Knobs to pin in Phase 0** (medium confidence until measured against the KEDA version the ACA
environment actually ships — do not treat as settled): whether `mode=QueueLength` counts unacknowledged
messages; `excludeUnacknowledged` / `mode=MessageRate` requiring `protocol=http` (management API);
`activationValue`; the environment's polling interval and cool-down (these set worst-case 0 → 1
latency); `vhostName`/`useRegex` when the queue is vhost-qualified.

> **ACA Jobs alternative.** Event-driven ACA **Jobs** (`--trigger-type Event`, same scale rule) start,
> drain, and exit per execution — cheaper for bursty triggers but losing long-lived competing-consumer
> parity. **Container App (long-running) for v1**; Jobs documented as a per-trigger option.

### 2.5 Security model *(decision #4 — EasyAuth + Entra, as per the Hangfire plan)*

1. **Token side.** The Container App's **system-assigned managed identity** is assigned the engine app
   role **`Warewolf_QueueProcessor`**; its app-only token for `api://<engine-app-id>/.default` carries
   `roles: ["Warewolf_QueueProcessor"]`. Registration uses the existing daemon flow with **no script
   change**: `Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity
   -AppRolesToAssign Warewolf_QueueProcessor`.
2. **Policy side.** `secure.config` grants `Warewolf_QueueProcessor` **Execute** on the target workflow
   scope; `WorkflowAuthorizationMiddleware` enforces it. Per WOLF-8418 a denial surfaces as **HTTP 500**,
   not 403 — the worker must classify it as a **permanent** failure (dead-letter), never as transient.

Token acquisition is not new code — reuse
[`WwExecutionTokenHandler`](../../Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/Auth/WwExecutionTokenHandler.cs#L24-L134)
(DefaultAzureCredential, single-flight refresh, expiry skew, bearer injection) with the DI wiring from
[`ClientExamples/AzureServiceBus/Program.cs:11-76`](../../Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/Program.cs#L11-L76).
The typed client there is **GET-only** and gains a **POST** variant (JSON body; `multipart/form-data`
for the single `@object` + `MapEntireMessage` case, per
[WarewolfWebRequestForwarder.cs:100-108](../../Warewolf.Common.Framework48/WarewolfWebRequestForwarder.cs#L100-L108));
its per-segment URL escaping for folder-qualified workflow names is already correct.

Other requirements: ACR pull via `--registry-identity system` (`AcrPull`), non-root container, no
ingress, pinned image digest in production revisions. **The trigger's per-trigger
`Username`/`Password` are not carried to Azure** — every message executes as the worker's MI
(decision #4).

### 2.6 Delivery semantics — parity, plus two deliberate improvements

| Case | Worker behaviour | Broker effect |
|---|---|---|
| Engine 2xx | `Success` | **ack** |
| Engine non-2xx (incl. a WOLF-8418 500 denial) | publish the mapped body to the dead-letter queue, then `Success` — parity with [LoggingConsumerWrapper.cs:64-79](../../Warewolf.QueueWorker/LoggingConsumerWrapper.cs#L64-L79) | **ack** |
| Transport / token / mapping failure | `Failed` | **no ack** → redelivered |
| SIGTERM mid-flight | stop deliveries, drain in-flight within the grace window, close | in-flight completes; unstarted stays unacked |
| Queue deleted / consumer cancelled | watchdog throws | replica exits → restarted |

**Improvement 1 — bounded engine timeout.** Today's `Timeout.InfiniteTimeSpan`
([WarewolfWebRequestForwarder.cs:97](../../Warewolf.Common.Framework48/WarewolfWebRequestForwarder.cs#L97))
parks a whole process on one hung workflow; `ENGINE__TIMEOUTSECONDS` maps a timeout to the transport
class (no ack → redelivery).

**Improvement 2 — graceful shutdown.** Nothing handles shutdown today; in ACA, scale-in and revision
rollouts are routine rather than exceptional. Detailed design, timelines, and worked examples: **§2.6.1**.

**Poison handling.** Since non-2xx is acked and dead-lettered at the app level, only exception-class
messages can loop. Configure a broker-side **DLX + delivery limit** (quorum-queue `x-delivery-limit` or
a classic-queue DLX policy) — safety the `.exe` never had. Broker configuration, not code.

#### 2.6.1 Graceful shutdown, in detail

**Why this is new work.** Today a `QueueWorker.exe` process only ever *dies*: the supervisor kills it
when the trigger changes ([ProcessMonitor.Kill](../../Warewolf.Common/OS/ProcessMonitor.cs#L137-L167)
loops `p.Kill()` up to 10 times), the watchdog throws on its timer thread, or `Environment.Exit` fires
from the file watcher ([Program.cs:131-134](../../Warewolf.QueueWorker/Program.cs#L131-L134)). There is
no shutdown handler anywhere in the worker. That is *tolerable* on-prem because it happens rarely — an
operator edited a trigger — and because RabbitMQ's manual-ack contract silently repairs it: an unacked
message is requeued when the connection dies, so the work is simply redone.

**Why it stops being tolerable in ACA.** The same event now happens constantly and by design:

| Trigger for shutdown | Frequency in ACA |
|---|---|
| KEDA scale-in (backlog drained: 5 replicas → 2 → 0) | Every quiet period |
| Revision rollout (new image, changed env var, changed `maxReplicas`) | Every deploy |
| Platform-initiated replica move (node maintenance, rebalance) | Occasionally, unannounced |

ACA's contract is the standard container one: **SIGTERM, then SIGKILL after the termination grace
period** (default 30 s; configurable — pin the maximum in Phase 0). Without a handler, every one of those
events kills a replica mid-message. The consequences are not data loss — the manual-ack contract still
protects the message — but they are real:

1. **Duplicate workflow executions.** The engine already received the POST and may have completed the
   workflow; because the ack never happened, the message is requeued and a second replica runs it again.
   Anything non-idempotent (a payment capture, an outbound notification, an insert without a unique key)
   happens twice.
2. **Broken audit trail.** The first attempt's execution never records a completion, so the audit shows
   an orphaned start with no outcome — indistinguishable from a genuine failure.
3. **Wasted engine capacity.** The abandoned execution keeps running inside the engine to completion
   (nobody is listening for its response), competing with the retry.
4. **Latency spikes on scale-in.** All messages prefetched by the dying replica (up to `Prefetch`, i.e.
   **10** for the reference trigger) sit invisible-but-unacked until the connection actually closes,
   rather than being handed to a surviving replica immediately.

**The design.** `QueueConsumerService` (a `BackgroundService`) hooks `IHostApplicationLifetime` and
implements a five-step drain:

```
SIGTERM (from ACA)
  │
  1. StopAsync / ApplicationStopping fires; set _draining = true
  │
  2. BasicCancel(consumerTag)          → broker stops delivering to THIS replica
  │                                      (it keeps delivering to the survivors)
  3. BasicNack(requeue: true) every    → prefetched-but-not-started messages go back
  │  buffered delivery not yet started   to the queue NOW, not at connection close
  │
  4. await in-flight count == 0, bounded by WORKER__SHUTDOWNGRACESECONDS
  │      each in-flight message finishes its engine POST and acks normally
  │
  5. Dispose the connection (the driver's Dispose already waits for any in-flight
  │  watchdog callback before closing — RabbitConnection.cs:119-158)
  ▼
process exits 0, well before SIGKILL
```

Step 3 is the part that has no equivalent today and is worth the code: it converts a scale-in from
"10 messages go dark for as long as the teardown takes" into "1 message finishes, 9 are instantly
available to other replicas".

**Worked example — scale-in on the reference trigger** (`profiler.mandatecollectionsuccess.request`,
`Prefetch = 10`, `WORKER__MAXCONCURRENCY = 1`, `WORKER__SHUTDOWNGRACESECONDS = 60`,
`terminationGracePeriodSeconds = 90`, `ENGINE__TIMEOUTSECONDS = 45`).

The backlog has drained, so KEDA scales 5 → 2 and ACA sends SIGTERM to three replicas. Replica #4 holds
one message being processed (POSTed to the engine 4 s ago) and 9 prefetched:

| t | Event | State |
|---|---|---|
| 0.00 s | SIGTERM received; `ApplicationStopping` fires | 1 in-flight, 9 buffered |
| 0.02 s | `BasicCancel` | broker routes new deliveries to replicas #1–#2 only |
| 0.05 s | `BasicNack(requeue: true)` × 9 | **9 messages immediately re-queued** and picked up by the survivors |
| 0.05–6.2 s | wait for in-flight | engine still executing `MandateCollectionSuccessConsume` |
| 6.20 s | engine returns 200 → `BasicAck` | in-flight = 0 |
| 6.25 s | dispose connection, host stops, `exit 0` | — |
| — | ACA never needs SIGKILL (6.25 s ≪ 90 s) | **0 duplicates, 0 orphaned audit rows** |

**Worked example — the grace window is exceeded.** Same replica, but the workflow is slow and the engine
has not answered by t = 60 s:

| t | Event | Outcome |
|---|---|---|
| 60.0 s | grace expires; stop waiting | the in-flight message is **deliberately not acked** |
| 60.1 s | dispose connection, exit 0 | message requeued → another replica re-runs it → **one duplicate execution** |
| — | log a `WARN` naming the queue, `Warewolf-Execution-Id`, and the elapsed time | the duplicate is *visible*, not silent |

This is the honest failure mode of at-least-once delivery, and it is why the timeout budget must nest:

```
ENGINE__TIMEOUTSECONDS  ≤  WORKER__SHUTDOWNGRACESECONDS  <  terminationGracePeriodSeconds
        45 s                          60 s                          90 s
```

With `ENGINE__TIMEOUTSECONDS (45) ≤ grace (60)`, any in-flight call is *guaranteed* to resolve — success
or timeout — inside the drain window, so the "grace exceeded" row above becomes unreachable in practice.
The outer margin (`90 > 60`) leaves room for connection teardown and process exit before SIGKILL.
Phase 8's deploy script validates this ordering and fails loudly if the three values are inconsistent.

**What this does *not* fix.** SIGKILL (grace exhausted), a node failure, or a network partition can still
duplicate a delivery — RabbitMQ competing consumers are at-least-once and always were (§1.2). Graceful
shutdown removes the *routine, self-inflicted* duplicates (every scale-in, every deploy); it does not
make the pipeline exactly-once. Workflows must stay idempotent or redelivery-tolerant, keyed on the
`Warewolf-Execution-Id` header the worker forwards (risk R2).

### 2.7 Cost model

ACA Consumption has a monthly free grant per subscription and **charges nothing at 0 replicas**, so an
idle queue is near-$0 versus an always-on App Service/Premium plan (the superseded Options A/B). All
worker apps share **one** environment, so M triggers ≠ M plans. Remainders: ACR, Key Vault operations,
App Insights ingestion, the broker itself. *Exact figures must be priced at plan time — no numbers
asserted here.*

### 2.8 Worked example — two triggers, from `Concurrency` to KEDA replicas

Worked end-to-end for the two triggers **OrdersQueue (`Concurrency = 5`)** and
**TasksQueue (`Concurrency = 10`)**, defined in their respective `triggers*.bite` files.

#### 2.8.1 What happens today

Both trigger files are loaded by `TriggersCatalog` and handed to `QueueWorkerMonitor`, which builds one
`ProcessThreadList` per trigger and spawns `Concurrency` copies of `QueueWorker.exe -c "{triggerId}"`
— **capped per trigger at `Environment.ProcessorCount`**
([ProcessThreadList.cs:101-111](../../Warewolf.Common/OS/ProcessThreadList.cs#L101-L111)):

```
CalculateProcessCount():  expected = Concurrency
                          if (expected > Environment.ProcessorCount) expected = ProcessorCount
```

On an **8-core** Warewolf Server:

| Trigger | `Concurrency` | Processes actually started | Concurrent workflows (§1.3: 1 per process) |
|---|---|---|---|
| OrdersQueue | 5 | **5** | 5 |
| TasksQueue | 10 | **8** — silently truncated by the CPU cap | 8 |
| | | **13 processes** on one machine | **13** |

Three properties of this model matter for the migration:

1. **The cap is per trigger, not aggregate.** TasksQueue is truncated 10 → 8, yet the box still runs
   13 processes competing for 8 cores. Adding a third trigger oversubscribes further — there is no
   global admission control.
2. **The fleet is fixed and always on.** All 13 processes exist whether the queues hold 10,000 messages
   or zero; the 1-second monitor loop restarts any that die
   ([WorkerMonitor.cs:86-101](../../Warewolf.Common/OS/WorkerMonitor.cs#L86-L101)).
3. **`Concurrency = 0` disables the trigger entirely** — no `ProcessThreadList` is created
   ([WorkerMonitor.cs:55-58](../../Warewolf.Common/OS/WorkerMonitor.cs#L55-L58)).

#### 2.8.2 The mapping

| Today | ACA + KEDA | Note |
|---|---|---|
| Trigger `.bite` | One **Container App** per trigger | `wwqp-orders`, `wwqp-tasks` — same ACA environment |
| `Concurrency` | **`maxReplicas`** | Read from the trigger `.bite` at deploy time (§2.8.5) |
| `min(Concurrency, ProcessorCount)` truncation | **gone** | `maxReplicas = 10` really means 10; each replica has its own CPU/memory allocation |
| 1 workflow per process | `WORKER__MAXCONCURRENCY` (**default 1**) | Effective in-flight = `replicas × WORKER__MAXCONCURRENCY` |
| Fixed always-on fleet | `minReplicas` | `0` = elastic, `= maxReplicas` = fixed parity (§2.8.3) |
| `Concurrency = 0` | `min = max = 0`, or the app stopped | Trigger disabled |
| Server's 1-second restart loop | ACA replica restart policy | — |
| Broker round-robins across processes | Broker round-robins across **replicas** | Identical competing-consumer semantics |

#### 2.8.3 Scaling modes — Elastic is the standard, the rest are exceptions

| Mode | Setting | Behaviour | Status |
|---|---|---|---|
| **B — Elastic** | `minReplicas = 0`, `maxReplicas = Concurrency` | Same **ceiling** as today, but replicas exist only while there is a backlog; nothing runs (and nothing is billed) when the queue is empty | ✅ **THE STANDARD — applied to every trigger** (decision #23). `-ScalingMode Elastic` is the script default |
| **A — Fixed parity** | `minReplicas = maxReplicas = Concurrency` | Exactly today's model: a constant fleet of 5 / 10 replicas, no autoscale, no scale-to-zero. The KEDA rule is inert (already at max) | ⚠ **By exception only** — requires an explicit `-ScalingMode Fixed` plus a recorded justification. **This is the direct answer to "fixed concurrency in the config file"**: full parity is available, it is simply not the default |
| **W — Warm** | `minReplicas = 1`, `maxReplicas = Concurrency` | One always-on consumer, so the first message after an idle period never waits for a KEDA poll + container start; still scales out and back to 1 | ⚠ **By exception only** — the documented remedy if a specific queue's cold-start latency proves unacceptable in production (risk R1) |
| **C — Raised ceiling** | `minReplicas = 0`, `maxReplicas > Concurrency` | Deliberately exceeds the old capacity — legitimate now that the `ProcessorCount` cap is gone and replicas do not share one box | ⚠ **By exception only** — never before load-testing the **engine**, which becomes the bottleneck (risk R10) |

Elastic everywhere was chosen deliberately: `maxReplicas = Concurrency` means the migration can never do
more work concurrently than the on-prem deployment already did (no downstream system sees added load),
and `minReplicas = 0` means an idle trigger costs nothing. The trade-off — **cold-start latency on the
first message of an idle period, accepted for all triggers** — is recorded as risk R1, with Mode W as the
per-queue escape hatch if measurements say otherwise.

#### 2.8.4 The two production scale rules

Two apps, one environment, each with its own `rabbitmq` rule. Secrets are per-app, so both apps carry
their own `keyvaultref` to the *same* broker credential.

```bash
# ── OrdersQueue: Concurrency 5 → maxReplicas 5, target 5 messages per replica ──
az containerapp create \
  --name wwqp-orders --resource-group rg-warewolf --environment aca-warewolf \
  --image acrwarewolf.azurecr.io/warewolf/queueprocessor@sha256:<digest> \
  --registry-server acrwarewolf.azurecr.io --registry-identity system \
  --system-assigned --ingress disabled \
  --cpu 0.5 --memory 1.0Gi \
  --min-replicas 0 --max-replicas 5 \
  --secrets "rabbitmq-connection=keyvaultref:https://kv-warewolf.vault.azure.net/secrets/rabbitmq-uri,identityref:system" \
  --env-vars "QUEUE__TRIGGERID=<orders-trigger-guid>" \
             "ENGINE__BASEURL=https://wwengine.azurewebsites.net" \
             "WORKER__MAXCONCURRENCY=1" \
  --scale-rule-name orders-backlog --scale-rule-type rabbitmq \
  --scale-rule-metadata "queueName=OrdersQueue" "mode=QueueLength" "value=5" \
                        "protocol=amqp" "activationValue=0" \
  --scale-rule-auth "host=rabbitmq-connection"

# ── TasksQueue: Concurrency 10 → maxReplicas 10, target 10 messages per replica ──
az containerapp create \
  --name wwqp-tasks --resource-group rg-warewolf --environment aca-warewolf \
  --image acrwarewolf.azurecr.io/warewolf/queueprocessor@sha256:<digest> \
  --registry-server acrwarewolf.azurecr.io --registry-identity system \
  --system-assigned --ingress disabled \
  --cpu 0.5 --memory 1.0Gi \
  --min-replicas 0 --max-replicas 10 \
  --secrets "rabbitmq-connection=keyvaultref:https://kv-warewolf.vault.azure.net/secrets/rabbitmq-uri,identityref:system" \
  --env-vars "QUEUE__TRIGGERID=<tasks-trigger-guid>" \
             "ENGINE__BASEURL=https://wwengine.azurewebsites.net" \
             "WORKER__MAXCONCURRENCY=1" \
  --scale-rule-name tasks-backlog --scale-rule-type rabbitmq \
  --scale-rule-metadata "queueName=TasksQueue" "mode=QueueLength" "value=10" \
                        "protocol=amqp" "activationValue=0" \
  --scale-rule-auth "host=rabbitmq-connection"
```

**How KEDA turns those into replicas.** Each app is polled independently:

```
desiredReplicas = clamp( ceil(queueLength / value), minReplicas, maxReplicas )
```

`value` (KEDA's `queueLength` target) is **messages per replica**, not a replica count — so it controls
*how eagerly* the app fans out, while `maxReplicas` controls *how far*:

```
saturation depth = maxReplicas × value      # backlog at which the app reaches maxReplicas
value            = desired_saturation_depth / maxReplicas
```

**OrdersQueue** (`max 5`, `value 5` → saturates at 25):

| Queue depth | `ceil(d / 5)` | Replicas | Concurrent workflows |
|---|---|---|---|
| 0 | 0 | **0** (after cool-down) | 0 — nothing billed |
| 1 | 1 | 1 | 1 |
| 8 | 2 | 2 | 2 |
| 12 | 3 | 3 | 3 |
| 25 | 5 | **5** (max) | 5 |
| 250 | 50 | **5** (clamped) | 5 — drains 5-wide, exactly today's ceiling |

**TasksQueue** (`max 10`, `value 10` → saturates at 100):

| Queue depth | `ceil(d / 10)` | Replicas | Concurrent workflows |
|---|---|---|---|
| 0 | 0 | **0** | 0 |
| 1 | 1 | 1 | 1 |
| 35 | 4 | 4 | 4 |
| 70 | 7 | 7 | 7 |
| 100 | 10 | **10** (max) | 10 — *beats today's truncated 8* |
| 1 000 | 100 | **10** (clamped) | 10 |

Tuning `value` changes only responsiveness: OrdersQueue with `value = 1` hits 5 replicas at a depth of
5 instead of 25 (faster drain, more replica churn); with `value = 20` it needs a depth of 100 (calmer,
deeper backlogs). `activationValue` is the separate 0 → 1 threshold — keep it `0` so a single message
wakes the app; raise it to ignore trivial backlogs.

#### 2.8.5 Where the numbers come from — one source of truth

`Concurrency` stays in the trigger `.bite`. `Deploy-WwQueueProcessor.ps1` (Phase 8) reads it while
staging the trigger file and derives the scale settings, so operators keep editing the same artefact
they edit today:

```
trigger.bite { QueueName = "OrdersQueue", Concurrency = 5, Prefetch = "1" }
        │
        ├─► --max-replicas 5                     (Concurrency)
        ├─► --min-replicas 0                     (mode B default; -ScalingMode Fixed ⇒ 5)
        ├─► --scale-rule-metadata queueName=OrdersQueue
        └─► BasicQos prefetchCount=1             (read from the staged trigger)
```

`-ScalingMode` **defaults to `Elastic`** (decision #23); `Fixed`, `Warm`, and an explicit
`-MaxReplicas` above `Concurrency` are exception paths that the script records in its deployment summary
so the deviation is visible afterwards. `-MinReplicas`, `-TargetQueueLength`, and `-MaxConcurrency` cover
the remaining tuning cases. `Concurrency = 0` in the trigger deploys the app with `min = max = 0`
(disabled, mirroring today).

#### 2.8.6 Multiple queues in production — isolation, quotas, and the anti-pattern

- **Independent scaling.** ACA scale settings are **per app**, so a TasksQueue spike adds TasksQueue
  replicas only. This is a genuine improvement over the superseded Functions/Dedicated option, where
  scale-out was per App Service *plan* and one hot queue scaled every app sharing it.
- **Peak footprint.** Both queues busy ⇒ 5 + 10 = **15 replicas**, each with its own `--cpu`/`--memory`
  — versus 13 processes sharing one 8-core box today. Idle ⇒ **0 replicas** in Mode B.
- **Environment quota is the new global limit.** The old `ProcessorCount` cap is replaced by the ACA
  environment / subscription core quota: `Σ (maxReplicas × cpu)` across all worker apps must fit
  (here `15 × 0.5 = 7.5` cores at peak). Confirm the actual regional quota before setting `maxReplicas`
  and request an increase if needed — *verify at plan time; the default varies by subscription and
  region (medium confidence, Phase 0 item).*
- **Adding a third trigger** is one more `az containerapp create` with its own queue name and
  `maxReplicas` — no shared-capacity re-planning, no restart of the other apps.
- **Anti-pattern: one app consuming both queues.** ACA does allow several scale rules per app, but the
  effective replica count becomes the **maximum across rules** and every replica runs *all* consumers —
  so an OrdersQueue backlog would spin up replicas that also consume TasksQueue, and the two triggers
  could no longer be scaled, tuned, deployed, or disabled independently. **One app per trigger**
  (decision #10) is deliberate.
- **Ops note.** Scale settings live on the **revision**: changing `maxReplicas` or the rule creates a
  new revision and rolls the replicas (the graceful drain in §2.6 is what makes that safe). Bumping a
  trigger's `Concurrency` is therefore a redeploy, not a live edit — the ACA equivalent of today's
  "trigger file changed → `Environment.Exit` → supervisor restarts the processes".

#### 2.8.7 The ceiling, the target, and `Prefetch` — how many replicas actually start

**`Concurrency` is a ceiling, never a fixed allocation** (in the default Mode B). The replica count is
recomputed from the live backlog on every KEDA poll:

```
replicas = clamp( ceil(queueLength / value), minReplicas, maxReplicas )
                  └──── demand ────┘          └──── 0 ────┘  └ Concurrency ┘
```

So a `Concurrency = 10` trigger sitting on a 3-message backlog runs **fewer than 10 replicas** — exactly
the behaviour asked for. What decides *how many fewer* is `value`, not `maxReplicas`.

**`Prefetch` is what makes `value` a real constraint rather than a free choice.** The trigger's `Prefetch`
becomes `BasicQos(prefetchSize: 0, prefetchCount: Prefetch, global: false)`
([RabbitConfig.cs:34-37](../../Warewolf.Driver.RabbitMQ/RabbitConfig.cs#L34-L37)) — **per consumer**,
with the existing parse rules (empty → `1`, `< 1` → `1`). With the reference trigger's `Prefetch = "10"`,
**one replica claims up to 10 messages the instant it connects.** Those messages are delivered-unacked:
they are no longer available to anyone else. Consequently:

> **Prefetch bounds useful parallelism.** You cannot get 10 replicas working on a 10-message backlog when
> `Prefetch = 10` — the first replica takes all ten and the other nine find an empty queue.

This is **not** a new ACA behaviour. It is exactly what happens today: 5 `QueueWorker.exe` processes with
`Prefetch = 10` against a 12-message burst means the first available process buffers ~10 and the second
takes 2 — **three of the five processes do nothing**. Same broker physics, same numbers.

**Derived rule.** `value` should equal the messages **one replica will claim**:

```
value = Prefetch × WORKER__MAXCONCURRENCY          # reference trigger: 10 × 1 = 10
```

Then `ceil(queueLength / value)` is literally "how many replicas are needed to hold this backlog".

**Reference trigger** (`Prefetch = 10`, `WORKER__MAXCONCURRENCY = 1`, `Concurrency = 5` after token
substitution ⇒ `maxReplicas = 5`, `value = 10`):

| Backlog | `ceil(q / 10)` | Replicas | Why it is right |
|---|---|---|---|
| 0 | 0 | **0** | nothing billed |
| 1 | 1 | 1 | one replica, one message |
| 8 | 1 | **1** | all 8 fit in one replica's prefetch buffer — a second replica would idle |
| 10 | 1 | 1 | buffer exactly full |
| 25 | 3 | 3 | 10 + 10 + 5 |
| 50 | 5 | **5** (ceiling) | matches today's `Concurrency` |
| 500 | 50 → clamped | **5** | drains 5-wide, never exceeding the on-prem ceiling |

**What goes wrong if `value` ignores `Prefetch`.** Same trigger with `value = 1`:

| Backlog | `ceil(q / 1)` | Replicas started | Replicas with work |
|---|---|---|---|
| 8 | 8 → clamped | **5** | ~1 — the first replica prefetched all 8 |

Five replicas billed, one working, plus four cold starts and four scale-in drains — pure waste. The
mirror-image error (`value` ≫ `Prefetch`) under-provisions: the backlog grows well past what the running
replicas can buffer before KEDA reacts.

**Choosing `Prefetch` is therefore choosing the granularity of work distribution:**

| `Prefetch` in trigger | Derived `value` | Backlog at full fan-out (`max = 5`) | Character |
|---|---|---|---|
| `1` | 1 | 5 | Finest fan-out — the closest thing to one replica per message, most even distribution, most replica churn. Best for slow/expensive workflows |
| `3` | 3 | 15 | Balanced; a good general default |
| `5` | 5 | 25 | Fewer, busier replicas |
| `10` (reference) | 10 | 50 | Coarsest — one replica absorbs 10. Best for short, cheap, high-rate messages |

Changing `Prefetch` in the trigger file therefore changes **two** deployed behaviours — the worker's `BasicQos` prefetch and
the derived `value` — and is a redeploy (new revision), like any other trigger change (§2.8.6).

**Two further couplings to respect:**
- **Metric fidelity.** If the KEDA `QueueLength` metric counts unacknowledged messages, a large prefetch
  keeps the metric high while the messages are already claimed → replicas are held up longer than needed.
  If it excludes them, the metric collapses the moment they are prefetched → premature scale-in and
  flapping. **A small prefetch makes the metric track reality under either semantic** — which is why
  Phase 0 item 4 measures which one the shipped scaler uses before any production rule is written.
- **Scale-in cost.** A dying replica must re-queue up to `Prefetch − 1` buffered messages (§2.6.1 step 3).
  Prefetch 10 means 9 messages briefly in limbo on every scale-in; prefetch 1 means none.
- **If `WORKER__MAXCONCURRENCY > 1`** (a deliberate throughput change, Mode C territory), then
  `Prefetch` must be **≥ `WORKER__MAXCONCURRENCY`** or the replica starves itself — it cannot run 4
  messages concurrently while the broker only lets it hold 2.

#### 2.8.8 The reference trigger file, field by field

Using the real `MandateCollectionSuccessTrigger` definition, this is where every field lands:

| Trigger field | Value | Destination in the ACA worker |
|---|---|---|
| `$id` / `$type` | `Warewolf.Trigger.Queue.TriggerQueue, …`, `Warewolf.Options.OptionBool, Warewolf.Data`, `Warewolf.Core.ServiceInput, Warewolf.Core` | Confirms §1.7: the file is `Dev2JsonSerializer` output. The `TriggerBiteReader`'s `ISerializationBinder` maps exactly these three type names onto worker-local DTOs — **no reference to those assemblies** |
| `TriggerId` | `1ac40da8-…` | `QUEUE__TRIGGERID`; also the staged file name |
| `Name` | `MandateCollectionSuccessTrigger` | Container App name slug (`wwqp-mandatecollectionsuccess`), log/telemetry dimension |
| `QueueName` | `profiler.mandatecollectionsuccess.request` | KEDA `--scale-rule-metadata queueName=…` **and** the consumer's `QueueDeclare`/`BasicConsume` |
| `WorkflowName` | `ProfilerWrapper\Queue\MandateCollectionSuccessConsume` | `ENGINE__WORKFLOW` → POST `/Secure/ProfilerWrapper/Queue/MandateCollectionSuccessConsume.json`. **⚠ separator normalisation required** — see below |
| `Concurrency` | `#{WarewolfProfilerMandateCollectionSuccessTriggerConcurrency}` | `--max-replicas`. **⚠ release-time token** — see below |
| `Prefetch` | `"10"` | the worker's `BasicQos(prefetchCount)` **and** the derived KEDA `value` (§2.8.7) |
| `UserName` / `Password` | `null` / `null` | **Nothing to carry over.** With no username today, `HttpClientFactory` sets `UseDefaultCredentials = true` ([HttpClientFactory.cs:31-33](../../Warewolf.Common/HttpClientFactory.cs#L31-L33)) — the Server's own machine identity. In ACA that role is taken by the app's managed identity, so decision #4 costs this trigger nothing |
| `Options` | `Durable = true` | `QueueDeclare(durable: true, exclusive: false, autoDelete: false)` — `OptionTo<RabbitConfig>` only assigns properties whose names match an option ([WorkerContext.cs:141-176](../../Warewolf.QueueWorker/WorkerContext.cs#L141-L176)), so `Exclusive`/`AutoDelete` stay `false`. These values **must** be reproduced exactly or `QueueDeclare` fails against the existing queue (risk R5) |
| `QueueSourceId` | `0b142714-…` | `Settings/{0b142714-…}.bite` → the five connection fields (§1.7) |
| `QueueSinkId` | **same GUID as `QueueSourceId`** | Dead-letter uses the *same* broker source ⇒ the deploy stages **one** source file, and the worker can share a single connection for consume + dead-letter publish |
| `DeadLetterQueue` | `profiler.mandatecollectionsuccess.error.request` | `QUEUE__DEADLETTERNAME` for the §2.6 dead-letter publish |
| `DeadLetterOptions` | `Durable = true` | Dead-letter `QueueDeclare` arguments |
| `MapEntireMessage` | `true` | `MAPPING__ENTIREMESSAGE=true` |
| `Inputs[0]` | `PayloadRequest` (required, `EmptyIsNull`) | `MAPPING__INPUTS`. With `MapEntireMessage = true`, `MessageToInputsMapper` produces `{"PayloadRequest": "<raw message>"}` ([MessageToInputsMapper.cs:31-34](../../Warewolf.Common/MessageToInputsMapper.cs#L31-L34)) |
| `ResourceId` | `d4708089-…` | The workflow resource id — telemetry/audit correlation only; the engine is addressed by name |

Two concrete implementation requirements fall out of this file:

1. **⚠ `Concurrency` is a release-time substitution token.** `#{WarewolfProfilerMandateCollection…}` is
   replaced by the release pipeline, which means (a) the file **is not valid JSON until substituted**, so
   the deploy script must read it *after* substitution, and (b) `--max-replicas` must be derived from the
   substituted value. The script therefore **fails loudly if any `#{…}` token survives** into the staged
   `.bite`, rather than defaulting `maxReplicas` to something arbitrary. This also means the existing
   release variable stays the single knob for capacity — operators change
   `WarewolfProfilerMandateCollectionSuccessTriggerConcurrency` exactly as they do today, and the ACA
   ceiling follows.
2. **⚠ `WorkflowName` uses `\` separators.** Today `WorkerContext.WorkflowUrl` simply concatenates
   ([WorkerContext.cs:80](../../Warewolf.QueueWorker/WorkerContext.cs#L80)) and `System.Uri` normalises
   the backslashes to `/` in the HTTP path, so it works by accident. The reference client escapes
   **per segment split on `/` only**
   ([WwExecutionClient.cs:70-73](../../Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/WwExecutionClient.cs#L70-L73)),
   so an unnormalised `\` would be percent-encoded to `%5C`, producing one bogus segment
   (`ProfilerWrapper%5CQueue%5CMandateCollectionSuccessConsume.json`) that the engine's `/Secure/{*name}`
   route cannot resolve. The worker must **normalise `\` → `/` before splitting**; Phase 10 pins this with
   a test using this exact workflow name. *(High confidence on the escaping behaviour; medium-high that
   `Uri` normalisation is what saves the current code — either way the new path needs the normalisation.)*

---

## 3. Phases

### Phase 0 — Spike + baseline measurements *(no production code)*
1. Throwaway Linux-container harness on **`RabbitMQ.Client` 7.x** (decision #24): connect using the §1.7
   connection shape, consume, ack. **Pin the exact patch level here.**
2. Measure per-channel dispatch concurrency on 7.x (with `ConsumerDispatchConcurrency = 1`) and on 5.1.2
   — validates §1.3, sets the `maxReplicas` parity target, and confirms that
   `WORKER__MAXCONCURRENCY = 1` reproduces today's one-workflow-at-a-time behaviour.
3. Validate **`TriggerBiteReader` Option A** (`ISerializationBinder` → worker DTOs) against a **real**
   trigger `.bite` produced by the Studio/Server, including `$id`/`$ref`. If it fails, fall back to
   Option B (deploy-time transcode) and record why.
4. On a scratch ACA app with a `rabbitmq` scale rule, record for the shipped KEDA version: `mode` /
   `protocol` semantics (does `QueueLength` include unacked?), `activationValue`, polling interval,
   cool-down, observed 0 → 1 latency.
5. Platform plumbing: verify Key Vault-referenced ACA secrets resolve with a system MI, and ACR pull via
   `--registry-identity system`; and record the **ACA environment / subscription core quota** for the
   target region, checked against `Σ (maxReplicas × cpu)` for the planned trigger set (§2.8.6) — that
   quota is the new global capacity limit replacing the per-trigger `ProcessorCount` cap.

**Acceptance:** a spike report pinning (a) the client version, (b) the concurrency baseline, (c) the
trigger-reader decision, (d) exact KEDA metadata, (e) 0 → 1 latency, (f) working secret + image-pull
identity + the environment core quota versus the planned `maxReplicas` set. **Gates Phase 1.**

### Phase 1 — Worker skeleton (Clean Architecture, config-only)
6. New project `Dev/Warewolf.Execution.QueueProcessor/` — .NET 8 `Exe`, generic `IHost`. Layers:
   **Domain** (`IConsumer` decorators, mapping — no infra types) / **Application**
   (`QueueConsumerService : BackgroundService`, options) / **Infrastructure** (`Rabbit/`, engine HTTP
   client, config readers, telemetry). Domain must not reference RabbitMQ or HTTP types.
7. `QueueProcessorOptions` with `ValidateDataAnnotations().ValidateOnStart()` — fail fast on missing
   settings.
8. `IHostApplicationLifetime`-driven **graceful shutdown** — the full five-step drain of §2.6.1
   (`_draining` flag → `BasicCancel` → `BasicNack(requeue: true)` on buffered-but-unstarted deliveries →
   bounded wait on the in-flight counter → dispose), with a `WARN` naming queue,
   `Warewolf-Execution-Id`, and elapsed time whenever the grace window is exceeded. The host — not a
   client-library thread — owns process lifetime (today's `.exe` relies on the RabbitMQ client's threads
   to stay alive: [Program.cs:136-140](../../Warewolf.QueueWorker/Program.cs#L136-L140) returns
   immediately after `StartConsuming`).
9. Add to `ServerTests.sln` + a `Dev/.azure/pipeline.yml` job, mirroring what was done for
   `Warewolf.Execution.EngineJobProcessor.Tests`.

**Acceptance:** starts in a Linux container, consumes/acks with a stub consumer, exits cleanly on
SIGTERM while draining. Server build/tests unchanged.

### Phase 2 — Config from `Settings/*.bite` + Key Vault
10. Link the Key Vault stack exactly as the JobProcessor does
    ([csproj:60-72](../../Warewolf.Execution.EngineJobProcessor/Warewolf.Execution.EngineJobProcessor.csproj#L60-L72)):
    `KeyVaultSecretManager.cs`, `FileDecryptionHelper.cs`, `KeyVaultCredentialFactory.cs`,
    `KeyVaultCredentialOptions.cs`; wire `DpapiWrapper.AesDecryptHook` at startup **before** any
    `.bite` is read.
11. `RabbitMqSourceOptions.FromBiteFile` — the §1.7 reader (mirroring
    [`ElasticsearchLoggingOptions.FromBiteFile`](../Logging/ElasticsearchLoggingOptions.cs#L73-L124)),
    parsing `HostName;Port;UserName;Password;VirtualHost`, plus the TLS opt-in rule.
12. `TriggerBiteReader` over `Settings/triggers/*.bite` per the Phase 0 decision; `QUEUE__TRIGGERID`
    selects when several are staged.
13. **Fail-fast guards:** a DPAPI-shaped (base64, non-`WFAES::`) value or an unparsable connection
    string produces an actionable startup error, never a deep `PlatformNotSupportedException` or a
    mystery connection failure (§1.5 blocker 1).

**Acceptance:** the worker resolves queue, prefetch, workflow, inputs, dead-letter, and broker
credentials entirely from its own staged files — **no Server, no catalog, no DPAPI**; secrets never
appear in logs.

### Phase 3 — The new consumer *(decision #7)*
14. `Infrastructure/Rabbit/`: `RabbitMqConnection : IQueueConnection`, `RabbitMqConsumerHost`,
    `RabbitMqPublisher : IPublisher`, `RabbitMqStreamConfig : IStreamConfig` — on the pinned client,
    async end-to-end (no `.Wait()`), implementing the **existing** `Warewolf.Interfaces` contracts.
15. Behavioural parity with [`RabbitConnection`](../../Warewolf.Driver.RabbitMQ/RabbitConnection.cs#L43-L110):
    manual ack only on `Success`; `BasicQos` prefetch; `QueueDeclare` with the trigger's
    durable/exclusive/autoDelete/arguments; a consumer-cancelled latch; and the connection watchdog —
    but surfaced as a **liveness signal** plus clean exit rather than an unhandled timer-thread throw.
16. Automatic recovery + heartbeat + a client-provided connection name (`wwqp-<trigger>-<replica>`) so
    broker-side connections are attributable.
17. **Zero edits** to `Warewolf.Driver.RabbitMQ`, `Dev2.Data`, the activities, or `QueueWorker.exe`;
    the worker references neither `Warewolf.Driver.RabbitMQ` nor `Dev2.Data` (§1.6).

**Acceptance:** the new consumer passes a parity test-suite written against the same scenarios as the
old loop; the shared 5.1.2 packages and all existing suites are untouched and green.

### Phase 4 — Mapping + engine invocation (MI token)
18. Reference `Warewolf.Common` and use `MessageToInputsMapper` **unchanged** (JSON / XML /
    whole-message), preserving
    [`BuildPostBody`](../../Warewolf.Common.Framework48/WarewolfWebRequestForwarder.cs#L85-L92)
    semantics. Do **not** reference `Warewolf.Common.Framework48` (§1.5 blocker 5).
19. Port `WwExecutionTokenHandler` + `WwExecutionOptions`; add `PostSecureAsync` (JSON body +
    `multipart/form-data` for the single `@object` case).
20. `EngineForwarder : IConsumer` — map → POST → classify into the three §2.6 classes; bounded timeout;
    no retry-on-non-2xx (that is the dead-letter path).
21. Correlation: ensure `Warewolf-Execution-Id`, pass through `Warewolf-Custom-Transaction-Id` (the
    consumer populates it from the message `CorrelationId`, as
    [RabbitConnection.cs:56-57](../../Warewolf.Driver.RabbitMQ/RabbitConnection.cs#L56-L57) does today —
    and which `PublishRabbitMQActivity` set on the publishing side, §1.1), and add `traceparent`.

**Acceptance:** a queued message drives a real `/Secure/{workflow}` execution on a deployed engine using
the worker's MI; 2xx acks; correlation ids join both sides of the call.

### Phase 5 — Engine authorization wiring
22. Define the `Warewolf_QueueProcessor` app role on the engine app registration; grant **Execute** for
    the target workflow scope in `secure.config` (the `/Secure/*` route itself needs no registry
    change — [WorkflowHttpFunction.cs:136-141](../Functions/WorkflowHttpFunction.cs#L136-L141)).
23. Assign the role to the app's system MI via the existing daemon flow (§2.5) — no script change.
24. Negative test: a roleless identity is refused (HTTP 500, WOLF-8418) and classified **permanent**.

**Acceptance:** the worker's token executes the workflow; unauthorized principals are refused; other
engine routes and clients unaffected.

### Phase 6 — Logging, audit, dead-letter, poison *(decision #6)*
25. Link the engine's logging stack (`LoggingConfiguration.cs`, `ExecutionLogLevel.cs`,
    `IExecutionLogger.cs`, `ExecutionLoggerBase.cs`, `ConsoleExecutionLogger.cs`,
    `AzureExecutionLogger.cs`, `ElasticsearchExecutionLogger.cs`, `ElasticsearchLogDocument.cs`,
    `AuditExecutionLogger.cs`, `CompositeExecutionLogger.cs`, `ApplicationInsightsLogFilter.cs`,
    `Dev2LoggerSinkAdapter.cs`) as **linked sources**, per the JobProcessor precedent. Reproduce the
    engine's two-stage bootstrap→composite `Dev2Logger.ExternalSink` swap (§1.8); the Functions-specific
    `ConfigureFunctionsApplicationInsights()` is replaced by the plain worker-service AI registration.
26. Replace `LoggingConsumerWrapper`'s WebSocket audit with an `AuditingConsumerDecorator` that emits
    the same signal (start / success / failure, duration, exception, `QueueRunStatus`) through
    `Dev2Logger`, so it lands in Console + App Insights + Elasticsearch + the audit sink.
27. `DeadLetterPublisher` on the new client with a **long-lived** connection/channel (today opens one
    per failure — [Program.cs:273-279](../../Warewolf.QueueWorker/Program.cs#L273-L279)), publishing the
    **mapped** post body (byte-parity with
    [WarewolfWebRequestForwarder.cs:64](../../Warewolf.Common.Framework48/WarewolfWebRequestForwarder.cs#L64))
    plus diagnostic headers (status code, engine correlation id, attempt count).
28. Broker-side DLX + delivery limit for the exception class (§2.6).

**Acceptance:** every message yields start/success/failure telemetry with `correlationId = executionId`
in the same sinks as the engine; engine non-2xx → body dead-lettered **and** original acked; repeated
exceptions → poison queue; no message silently lost.

### Phase 7 — Container image + local compose
29. Multi-stage `Dockerfile` (sdk:8.0 → runtime:8.0), non-root user, `DOTNET_ENVIRONMENT` honoured,
    following the existing container precedent ([engine/docker/Dockerfile](../engine/docker/Dockerfile),
    [Dev2.Server/Dockerfile.linux](../../Dev2.Server/Dockerfile.linux)).
30. `docker-compose.yml`: worker + RabbitMQ (management image) + a stub engine (WireMock is already used
    by [the integration tests](../../Warewolf.Execution.Lightweight.Integration.Tests/docker/wiremock/Dockerfile)).
31. CI: build + push to ACR by digest, scan, publish the digest as a pipeline output.

**Acceptance:** `docker compose up` processes a message end-to-end locally; the CI image runs unmodified
in ACA.

### Phase 8 — Deployment automation
32. `Scripts/Deploy-WwQueueProcessor.ps1` mirroring [`Deploy-WwJobProcessor.ps1`](../Scripts/Deploy-WwJobProcessor.ps1):
    resolve/create the ACA environment + ACR; build/push or accept a digest; **stage + WFAES-encrypt**
    the trigger and source `.bite` files into a **fresh temp staging dir** (never mutating publish
    output — Hangfire decision #12); create/update the Container App with system MI, Key Vault-ref
    secrets, env vars, the KEDA rule, min/max replicas, `terminationGracePeriodSeconds`; assign
    `AcrPull` + Key Vault access; verify a healthy revision; `-DryRun` and `-LoadFunctionsOnly` support.
33. **Derive the scale settings from the trigger `.bite`** (§2.8.5, §2.8.7): `maxReplicas = Concurrency`,
    `queueName = QueueName`, the worker's `BasicQos(prefetchCount) = Prefetch`, and **`value = Prefetch × MaxConcurrency`**;
    `-ScalingMode Fixed|Elastic|Warm` selects `minReplicas` (`= maxReplicas` / `0` / `1`), with
    `-MaxReplicas`/`-MinReplicas`/`-TargetQueueLength`/`-MaxConcurrency` overrides for the Mode C and
    tuning cases. `Concurrency = 0` deploys `min = max = 0` (disabled), mirroring
    [WorkerMonitor.cs:55-58](../../Warewolf.Common/OS/WorkerMonitor.cs#L55-L58).
    **Four fail-loud plan-time guards:** (a) any surviving `#{…}` release token in the staged `.bite`
    (§2.8.8); (b) `Σ (maxReplicas × cpu)` over the recorded environment quota (Phase 0 item 5);
    (c) a timeout ordering that violates
    `ENGINE__TIMEOUTSECONDS ≤ WORKER__SHUTDOWNGRACESECONDS < terminationGracePeriodSeconds` (§2.6.1);
    (d) `Prefetch < MaxConcurrency` (a self-starving replica, §2.8.7).
34. **Engine-deploy companion toggle** — add `-DeployRabbitMqTriggers` to
    [`Deploy-WwExecutionEngine.ps1`](../Scripts/Deploy-WwExecutionEngine.ps1), mirroring the existing
    `-DeployJobProcessor` pattern **exactly**, so one engine deploy can fan out Container Apps for **all**
    trigger files it is pointed at. Integration points in §8.3.
35. Rollback: `az containerapp revision` / app delete, driven by the **per-app** summary JSON each child
    run writes (`deploy-WwQueueProcessor-<app>-<stamp>.summary.json`) so a single trigger can be rolled
    back without touching its siblings — same summary schema + run tags as
    `Rollback-WwExecutionEngine.ps1`.
36. Pester: new `Tests/Deploy-WwQueueProcessor.Tests.ps1` (static/ValidateSet, helpers, trigger discovery
    across all three pointing modes, name-slug derivation + collision, DryRun end-to-end via the az-shim,
    `.bite` staging, the four fail-loud guards) **and additions to the existing engine suite**
    (`Deploy-WwExecutionEngine.Tests.ps1`, currently green 76/76) pinning the new parameters, the
    publish-path collision guard, and the fan-out loop — **proposed for approval**.

**Acceptance:** both scripts parse clean, pass `-LoadFunctionsOnly`, Pester green (new suite + the
extended engine suite), and a real run deploys working workers for every pointed trigger from a clean
subscription state.

#### 8.1 `Deploy-WwQueueProcessor.ps1` — parameter surface and how the trigger file is pointed at it

Conventions follow the existing scripts: `Read-Required` prompting when a value is omitted
interactively, `-NonInteractive` to fail instead of prompt, `-DryRun` to print the plan and change
nothing, `-LoadFunctionsOnly` as the Pester hook, `-LogDir` for the transcript + summary JSON.

**Trigger pointing — three mutually-exclusive modes** (this is the parameter set that answers "how does
the script know which trigger to deploy"):

| Parameter | Meaning | Result |
|---|---|---|
| `-TriggerFilePath <file>` | One explicit trigger `.bite` | **One** Container App |
| `-TriggerPath <folder>` (+ `-TriggerFilter`, default `triggers*.bite`) | A folder of trigger files | **One Container App per matching file** |
| `-TriggerManifestPath <json>` | Explicit list with per-trigger overrides | One app per manifest entry, overrides applied |

`-TriggerId <guid>` narrows a folder or manifest to a single trigger (useful for the per-trigger cutover
of Phase 11). Zero matches is a **hard error**, never a silent no-op.

```powershell
# ── Targeting ───────────────────────────────────────────────────────────────
-ResourceGroup <rg>  -Location <region>
-AcaEnvironment <name>              # created if absent (Consumption workload profile)
-AcrName <name>                     # created if absent
-PublishPath <folder|zip>           # worker publish output; builds + pushes the image
   | -Image <acr>.azurecr.io/warewolf/queueprocessor@sha256:<digest>   # or reuse a built image
-AppNamePrefix wwqp-                # default

# ── Trigger + source input ──────────────────────────────────────────────────
-TriggerFilePath | -TriggerPath [-TriggerFilter] | -TriggerManifestPath
-TriggerId <guid>                   # optional narrowing
-QueueSourcePath <folder|file>      # where {QueueSourceId}.bite lives (default: the trigger's folder)

# ── Engine wiring ───────────────────────────────────────────────────────────
-EngineBaseUrl https://<engine>.azurewebsites.net
-EngineScope   api://<engine-app-id>/.default
-EngineTimeoutSeconds 45

# ── Scaling (derived from the trigger; overrides are exception paths) ────────
-ScalingMode Elastic|Fixed|Warm     # default Elastic (decision #23)
-MaxReplicas -MinReplicas -TargetQueueLength -MaxConcurrency -Cpu -Memory
-ShutdownGraceSeconds 60  -TerminationGracePeriodSeconds 90

# ── Secrets / identity ──────────────────────────────────────────────────────
-KeyVaultName <kv> -KeyVaultSecretName <name>   # WFAES key, same pair as the engine
-RabbitMqSecretUri https://<kv>.vault.azure.net/secrets/rabbitmq-uri   # KEDA rule credential
-UseSsl                                          # else derived from the source (§1.7)

# ── Ops ─────────────────────────────────────────────────────────────────────
-DryRun -NonInteractive -LogDir <dir> -ContinueOnTriggerError -LoadFunctionsOnly
```

**Per-trigger work loop** (one iteration per resolved trigger file): read + WFAES-decrypt the trigger →
resolve `{QueueSourceId}.bite` (and `{QueueSinkId}.bite` when it differs) → derive app name, queue,
prefetch, `maxReplicas`, `value` → stage the `.bite` pair into a **fresh OS-temp dir** (never mutating
the publish output — Hangfire decision #12) → create/update the Container App with system MI, Key
Vault-ref secrets, env vars, and the KEDA rule → grant `AcrPull` + Key Vault access to that app's MI →
verify the revision reaches Healthy → write its own summary JSON. The image is built and pushed **once**
per run and reused by every trigger.

**App-name derivation.** `wwqp-` + slug of the trigger `Name` (fallback `QueueName`): lowercased,
non-alphanumerics → `-`, repeats collapsed, trimmed, truncated to fit ACA's 32-character limit; when
truncation occurs, a 4-character hash of the `TriggerId` is appended to keep it unique. A collision
**within one run** is a hard error. `MandateCollectionSuccessTrigger` ⇒
`wwqp-mandatecollectionsuccesstrigg-a3f1` (illustrative — the exact slug is pinned by the Phase 8 tests).

#### 8.2 Worked examples

```powershell
# ── A. Dry run for ONE trigger (inspect the plan, change nothing) ────────────
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -PublishPath D:\QueueProcessor\Publish `
  -TriggerFilePath 'C:\ProgramData\Warewolf\Triggers\Queue\1ac40da8-3b56-45f8-a1aa-00e6864db38b.bite' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl $EngineUrl -EngineScope "api://$ResourceAppId/.default" `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri" `
  -DryRun

# Plan output (illustrative):
#   Trigger      : MandateCollectionSuccessTrigger (1ac40da8-…)
#   Queue        : profiler.mandatecollectionsuccess.request
#   Workflow     : ProfilerWrapper/Queue/MandateCollectionSuccessConsume
#   Concurrency  : 5   -> --max-replicas 5     (ScalingMode Elastic -> --min-replicas 0)
#   Prefetch     : 10  -> BasicQos prefetchCount=10, KEDA value=10  (Prefetch x MaxConcurrency)
#   Dead-letter  : profiler.mandatecollectionsuccess.error.request
#   Source       : 0b142714-… (HostName=server.ngrok.io;Port=20313)  TLS: off (parity)
#   Container App: wwqp-mandatecollectionsuccesstrigg-a3f1

# ── B. Real deploy of that ONE trigger ──────────────────────────────────────
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -PublishPath D:\QueueProcessor\Publish `
  -TriggerFilePath 'C:\ProgramData\Warewolf\Triggers\Queue\1ac40da8-….bite' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl $EngineUrl -EngineScope "api://$ResourceAppId/.default" `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri" `
  -LogDir $LogDir -NonInteractive

# ── C. ALL triggers in a folder — one Container App each ────────────────────
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -Image "acrwarewolf.azurecr.io/warewolf/queueprocessor@sha256:<digest>" `
  -TriggerPath 'C:\ProgramData\Warewolf\Triggers\Queue' -TriggerFilter 'triggers*.bite' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl $EngineUrl -EngineScope "api://$ResourceAppId/.default" `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri" `
  -LogDir $LogDir -NonInteractive
# -> wwqp-orders (max 5), wwqp-tasks (max 10), wwqp-mandatecollectionsuccesstrigg-a3f1 (max 5) …

# ── D. Manifest with per-trigger exceptions ─────────────────────────────────
.\Deploy-WwQueueProcessor.ps1 ... -TriggerManifestPath .\queue-triggers.manifest.json
```

```json
// queue-triggers.manifest.json — only deviations need to be stated
{
  "triggers": [
    { "file": "C:\\ProgramData\\Warewolf\\Triggers\\Queue\\orders.bite" },
    { "file": "C:\\ProgramData\\Warewolf\\Triggers\\Queue\\tasks.bite",
      "scalingMode": "Warm",
      "justification": "first-message latency SLA - risk R1 exception" },
    { "file": "C:\\ProgramData\\Warewolf\\Triggers\\Queue\\mandate.bite",
      "cpu": "1.0", "memory": "2.0Gi" }
  ]
}
```

#### 8.3 `Deploy-WwExecutionEngine.ps1` integration — `-DeployRabbitMqTriggers`

Mirrors `-DeployJobProcessor` point for point, so the engine deploy can provision the engine **and** every
queue worker in one command. Five touch points in the existing script:

1. **Param block** — a new section after the JobProcessor block
   ([Deploy-WwExecutionEngine.ps1:225-237](../Scripts/Deploy-WwExecutionEngine.ps1#L225-L237)):
   ```powershell
   # ── RabbitMQ queue triggers (optional companion deploy) ───────────────────
   # When -DeployRabbitMqTriggers, after the engine deploy this calls
   # Deploy-WwQueueProcessor.ps1 ONCE PER TRIGGER FILE resolved from
   # -QueueTriggerPath/-QueueTriggerFilePath/-QueueTriggerManifestPath, passing the
   # shared context (subscription/tenant/RG/location/Key Vault/engine URL + scope).
   [switch] $DeployRabbitMqTriggers,
   [string] $QueueTriggerPath,                       # folder of trigger .bite files
   [string] $QueueTriggerFilter = 'triggers*.bite',
   [string] $QueueTriggerFilePath,                   # or a single file
   [string] $QueueTriggerManifestPath,               # or a manifest
   [string] $QueueSourcePath,                        # {QueueSourceId}.bite location
   [string] $AcaEnvironment,
   [string] $AcrName,
   [string] $QueueProcessorPublishPath,              # MUST differ from engine PublishPath
   [string] $QueueProcessorImage,                    # or a pre-built digest
   [string] $RabbitMqSecretUri,
   [ValidateSet('Elastic','Fixed','Warm')] [string] $QueueScalingMode = 'Elastic',
   [switch] $ContinueOnQueueTriggerError,
   ```
2. **Child-script path** — `$QueueProcessorScript = Join-Path $ScriptDir 'Deploy-WwQueueProcessor.ps1'`,
   beside `$JobProcessorScript` ([:702](../Scripts/Deploy-WwExecutionEngine.ps1#L702)).
3. **Plan-time validation** — a block mirroring the JobProcessor collision guard
   ([:836-853](../Scripts/Deploy-WwExecutionEngine.ps1#L836-L853)), run **before the engine is deployed**:
   prompt for the trigger source via `Read-Required`; **enumerate the trigger files and fail if zero
   match**; fail if any resolved `.bite` still contains a `#{…}` release token (§2.8.8); fail if
   `-QueueProcessorPublishPath` resolves to the engine `-PublishPath` **or** the JobProcessor path; and
   echo the resolved trigger list (name → queue → concurrency) so the operator sees the fan-out before
   anything is created.
4. **Plan summary + summary JSON** — a `Write-Host` line beside the JobProcessor one
   ([:924-927](../Scripts/Deploy-WwExecutionEngine.ps1#L924-L927)):
   `Deploy QueueProcessors : yes -> Deploy-WwQueueProcessor.ps1 (3 triggers: orders, tasks, mandate…)`,
   plus `deployRabbitMqTriggers` / `queueProcessorApps[]` (trigger, app, queue, maxReplicas) in the
   summary JSON ([:667-675](../Scripts/Deploy-WwExecutionEngine.ps1#L667-L675)) so rollback and audit see
   them.
5. **New Phase 7** — immediately after the Phase 6 JobProcessor block
   ([:1440-1482](../Scripts/Deploy-WwExecutionEngine.ps1#L1440-L1482)), same shape:
   ```powershell
   if ($DeployRabbitMqTriggers) {
       Write-Phase 'Phase 7  Deploy RabbitMQ QueueProcessors (companion, one per trigger)'
       foreach ($trigger in $resolvedQueueTriggers) {
           $qpParams = [ordered]@{
               ResourceGroup = $ResourceGroup; Location = $Location
               AcaEnvironment = $AcaEnvironment; AcrName = $AcrName
               TriggerFilePath = $trigger.Path; QueueSourcePath = $QueueSourcePath
               EngineBaseUrl = $engineUrl; EngineScope = $EngineResumeScope ?? "api://$appId/.default"
               ScalingMode = $QueueScalingMode; RabbitMqSecretUri = $RabbitMqSecretUri
           }
           # + Image/PublishPath, KeyVault pair, EnableAppInsights, NonInteractive, DryRun
           Invoke-ChildScript -Path $QueueProcessorScript `
             -Label "Deploy-WwQueueProcessor.ps1 ($($trigger.Name))" -Parameters $qpParams
       }
       Write-Note 'Reminder: grant EACH QueueProcessor MI the engine role Warewolf_QueueProcessor (runbook §9).'
   }
   ```
   **Fail-fast by default** — the first failing trigger aborts the loop (the engine and any already-created
   apps stay); `-ContinueOnQueueTriggerError` deploys the remainder and exits non-zero with a per-trigger
   result table. The `containerapp` az extension is checked/installed **inside the child**, so an engine
   deploy without the toggle gains no new prerequisite.

### Phase 9 — Documentation sync *(mandatory)*
37. Update `Scripts/README.md` (new script entry + the engine's new parameters),
    [`Deploy-RunGuide.md`](Deploy-RunGuide.md) (full parameter reference for both scripts, added to its
    worked-examples section), [`Deploy-EndToEnd-Runbook.md`](Deploy-EndToEnd-Runbook.md) (**new §8, spec
    in §9.1 below**), `Part1-Architecture.md` (third deployable), and the **`warewolf-architecture`** /
    **`warewolf-deploy`** skills.
38. Document the **coexistence rule** (risk R3) and the **per-trigger cutover runbook** (Phase 11): a
    queue must never be consumed by both on-prem `QueueWorker.exe` processes and ACA replicas — the
    queue-world analogue of Hangfire decision #13 — plus the shadow-deploy → swap → soak → rollback
    sequence and the broker consumer-count check that proves only one side is attached.
39. Document the **TLS posture** (decision #25) and the **production go-live gate** (Phase 11 item 41) as
    a signed-off checklist, not prose.
40. Publish the **concurrency-mapping runbook** from §2.8 for operators: `Concurrency` → `maxReplicas`,
    `value = Prefetch × MaxConcurrency`, Elastic as the standard with the Fixed/Warm/raised-ceiling
    **exception register** (which queue, which mode, why), how to pick `activationValue`, the environment
    core quota, and the fact that changing `Concurrency` or `Prefetch` is a redeploy (new revision), not a
    live edit.

#### 9.1 `Deploy-EndToEnd-Runbook.md` — new §8 spec

The runbook currently ends with §7 (JobProcessor, optional) and §8 (Teardown). Insert
**§8 "(Optional) RabbitMQ QueueProcessors — deploy + authorize"** directly after §7, following its
structure verbatim, and **renumber Teardown to §9**. Content:

- **Opening contract paragraph**, mirroring §7's. Same two-part authorization, role
  **`Warewolf_QueueProcessor`** — with one important difference from the JobProcessor that must be
  spelled out: the JobProcessor needs a **global-scope** (`IsServer=true`) Execute row because the resume
  route has no per-workflow entry, whereas a queue worker calls a **named workflow**, so `secure.config`
  needs a **per-workflow** (`IsServer=false`) `View`+`Execute` row for **each** trigger's `WorkflowName`
  (e.g. `ProfilerWrapper\Queue\MandateCollectionSuccessConsume`). Getting this wrong yields the WOLF-8418
  **HTTP 500** denial, not a 403.
- **§8a Publish + build**: `dotnet publish Dev/Warewolf.Execution.QueueProcessor -c Release -o …`, then
  either let the script build/push the image or pass a pre-built digest.
- **§8b Deploy per trigger** — the §8.2 examples A (dry run, one trigger), B (real, one trigger) and
  C (folder fan-out), showing plainly **which parameter points at the trigger file**.
- **§8c Deploy as an engine companion** — the one-command form, beside §7a's `-DeployJobProcessor` example:
  ```powershell
  .\Deploy-WwExecutionEngine.ps1 ... `
    -DeployRabbitMqTriggers `
    -QueueTriggerPath 'C:\ProgramData\Warewolf\Triggers\Queue' `
    -QueueSourcePath  'C:\ProgramData\Warewolf\Resources\Sources' `
    -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
    -QueueProcessorPublishPath D:\QueueProcessor\Publish `
    -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri"
  ```
- **§8d Authorize each MI.** Unlike the single JobProcessor, this creates **one system MI per trigger
  app**, so the role assignment is a **loop** over the deployed app names — the script's summary JSON
  lists them:
  ```powershell
  $apps = (Get-Content $qpSummary | ConvertFrom-Json).queueProcessorApps.app
  foreach ($app in $apps) {
      .\Configure-WwExecutionAuth-Clients.ps1 `
        -ResourceAppId $ResourceAppId -TenantId $TenantId `
        -ClientType Daemon -DaemonUseManagedIdentity `
        -DaemonContainerAppName $app -DaemonContainerAppResourceGroup $ResourceGroup `
        -AppRolesToAssign Warewolf_QueueProcessor -NonInteractive
  }
  ```
  ⚠ `Configure-WwExecutionAuth-Clients.ps1` today takes `-DaemonFunctionAppName`/`-DaemonFunctionAppResourceGroup`.
  A **Container App** MI is read with `az containerapp identity show` rather than
  `az functionapp identity assign`, so this needs either new `-DaemonContainerApp*` parameters or the
  existing `-ManagedIdentityObjectId` path (which already works unchanged if the caller resolves the
  principal id first). **Decide in Phase 8**; the `-ManagedIdentityObjectId` route requires **no script
  change** and is the fallback.
- **§8e Verify**: `az containerapp list -o table`, `az containerapp revision list`,
  `az containerapp logs show --follow`, replica count against queue depth, and a published message
  reaching the workflow end-to-end.
- **§8f Go-live gate** — the Phase 11 item 41 checklist as a signed-off table.
- **§9 Teardown additions**: `az containerapp delete` per app, plus the per-app role removal via
  `Remove-WwExecutionAuth-Clients.ps1`, and the per-app summary-driven rollback from Phase 8 item 35.

### Phase 10 — Test matrix (proposed; nothing written without go-ahead)

| Area | Scenarios |
|---|---|
| New-consumer parity | Ack only on success; prefetch bounds unacked; `QueueDeclare` argument mismatch fails loudly; consumer-cancelled → clean exit; automatic recovery after a broker bounce |
| No-regression | `Warewolf.Driver.RabbitMQ`, the three activities, and `QueueWorker.exe` unchanged and green (`Warewolf.Trigger.Queue.Tests`, activity + Server suites); shared `RabbitMQ.Client` version unchanged in the lock files |
| Config loading | `triggers*.bite` → correct trigger (incl. `$type`/`$ref`); `{source}.bite` → correct 5 connection fields; WFAES decrypt; DPAPI value → actionable failure; multi-trigger selection by `QUEUE__TRIGGERID` |
| TLS | Default off = parity; `UseSsl`/port 5671/env override each enable it; certificate failure surfaces clearly |
| Mapping | JSON / XML / whole-message parity with `MessageToInputsMapper`; single `@object` → `multipart/form-data`; malformed body → classified failure, not a crash |
| Auth | MI token accepted; roleless → 500 (WOLF-8418) classified **permanent**; token cache refresh at the skew boundary; acquisition failure → transient |
| Dead-letter | Non-2xx → mapped body in the dead-letter queue **and** original acked; payload byte-parity with today |
| Poison/transport | Unreachable/timeout → no ack → redelivered; repeated exception → DLX; zero loss on every path |
| Shutdown/scale-in (§2.6.1) | SIGTERM with 1 in-flight + 9 buffered (`Prefetch = 10`) → `BasicCancel` stops new deliveries, the **9 are nacked/requeued immediately** and picked up by a surviving replica, the 1 completes and acks, process exits 0 → **zero duplicates**; grace exceeded → in-flight deliberately unacked + `WARN` emitted (one visible duplicate, no loss); revision rollout under load loses nothing; SIGKILL path still redelivers (at-least-once, documented) |
| Prefetch ↔ scaling (§2.8.7) | `value` derived as `Prefetch × MaxConcurrency`; backlog 8 with `Prefetch 10` → **1** replica (not 5); backlog 50 → 5 (ceiling); `Prefetch 1` → per-message fan-out; `Prefetch < MaxConcurrency` rejected at deploy time; changing `Prefetch` updates both the worker's `BasicQos` prefetch and `value` |
| Trigger-file fidelity (§2.8.8) | Reference `MandateCollectionSuccessTrigger` parses (`$id`/`$type`/`$ref`); `Durable=true` → `QueueDeclare(true,false,false)`; `QueueSinkId == QueueSourceId` → one staged source; `MapEntireMessage` + `PayloadRequest` → body `{"PayloadRequest":"<raw>"}` (**not** multipart — no `@` prefix); `WorkflowName` with `\` separators resolves to `/Secure/ProfilerWrapper/Queue/MandateCollectionSuccessConsume.json`; an unsubstituted `#{…}` token fails the deploy |
| Cutover (Phase 11) | On-prem `Concurrency → 0` stops that trigger's processes while other triggers keep running; the ACA app and the on-prem processes are never both consuming (verified by broker consumer count); rollback (`Concurrency` restored, ACA app stopped) returns to the previous state with no message loss |
| Scaling | 0 → 1 on first message (record latency); depth D, target K → ~`ceil(D/K)` replicas ≤ max; clamped at `maxReplicas` for very deep backlogs; drain → 0 after cool-down; `maxReplicas = Concurrency` reproduces the measured baseline |
| Concurrency mapping (§2.8) | `Concurrency 5`/`10` → `maxReplicas 5`/`10` derived from the trigger `.bite`; **Mode A** (`min = max`) holds a constant fleet and never scales; **Mode B** returns to 0; `Concurrency = 0` → disabled app; `-MaxReplicas` override wins; quota guard fails loudly |
| Multi-queue independence | An OrdersQueue backlog scales `wwqp-orders` only — `wwqp-tasks` replica count and in-flight work are unaffected (and vice-versa); both apps saturate concurrently to 5 + 10 without interfering |
| Logging | `Dev2Logger` calls reach Console + AI + Elasticsearch + Audit with the engine's level semantics; audit events carry `EventId 9000`; worker and engine traces correlate |
| Deploy — child script (§8.1) | DryRun end-to-end via the az-shim; all three trigger-pointing modes resolve the expected file set; `-TriggerId` narrows a folder to one; **zero matches throws**; app-name slug derivation + truncation-with-hash + in-run collision error; `.bite` pair staged into a fresh temp dir with the publish output unmutated; idempotent re-run; per-app summary JSON written |
| Deploy — engine toggle (§8.3) | New parameters pinned (names, types, `ValidateSet` on `-QueueScalingMode`, default `triggers*.bite`); plan-time guards fire **before** the engine deploy (zero triggers, surviving `#{…}` token, publish-path collision with the engine **and** with the JobProcessor path); fan-out invokes the child once per trigger with the resolved engine URL + scope; **fail-fast by default**, `-ContinueOnQueueTriggerError` completes the rest and exits non-zero; toggle **off** ⇒ engine deploy byte-identical to today (no new prerequisite, no `containerapp` extension needed) |
| Deploy — authorization loop (§9.1) | Every deployed app's MI receives `Warewolf_QueueProcessor`; a missing per-workflow `secure.config` row yields the WOLF-8418 **500** (not 403) and is diagnosed as such in the runbook table |

### Phase 11 — Per-trigger cutover + production go-live gate *(decision #22)*

Repeated **once per queue-trigger**, never in parallel across triggers:

41. **Go-live gate (first production trigger only).** Hard blockers, all four required:
    (a) the broker/tunnel terminates TLS and the app runs `RABBITMQ__USESSL=true` against `amqps`
    (decision #25); (b) the DLX / delivery-limit policy exists on the target queue (§2.6);
    (c) `ENGINE__TIMEOUTSECONDS ≤ WORKER__SHUTDOWNGRACESECONDS < terminationGracePeriodSeconds` is
    verified on the deployed revision (§2.6.1); (d) App Insights shows the worker's start/success/failure
    events with correlation ids joining the engine's traces (§1.8).
42. **Deploy and verify in shadow.** Deploy the trigger's Container App with `maxReplicas = 0` (consuming
    nothing), confirm it starts, reads its staged `.bite` files, acquires an MI token, and reaches the
    engine — while the on-prem processes still own the queue.
43. **Swap.** Raise the app to `maxReplicas = Concurrency` (Elastic, `min = 0`) **and** set the on-prem
    trigger's `Concurrency` to `0` in the same change window. Setting it to 0 stops that trigger's
    processes ([WorkerMonitor.cs:55-58](../../Warewolf.Common/OS/WorkerMonitor.cs#L55-L58)) without
    touching any other trigger — this is the mechanism that keeps risk R3 (double consumption) closed.
    Verify with the broker's consumer count that exactly one side is attached.
44. **Observe** for an agreed soak period: replica count tracking backlog, dead-letter depth, duplicate
    rate, cold-start latency on the first message after an idle gap (validating the risk-R1 acceptance —
    if it fails here, this is where Mode W is applied to *this* queue only).
45. **Rollback path**, rehearsed before the first cutover: stop/scale the ACA app to 0 and restore the
    on-prem `Concurrency`. Unacked messages return to the queue, so nothing is lost in either direction.
    Then move to the next trigger.

**Acceptance:** each trigger is served by exactly one side at all times; the soak period shows no
duplicate-execution regression versus the on-prem baseline; the go-live gate is signed off once, before
the first production trigger.

---

## 4. Sequencing

Critical path **0 → 1 → 2 → 3 → 4 → 5**. Phase 6 can start once 4 is green; Phase 7 in parallel from
Phase 3; Phase 8 needs 7; Phases 9–10 close out the build. **Phase 0 gates everything** — it pins the
7.x patch level, the trigger-reader approach, the concurrency baseline, and the KEDA metadata that later
phases depend on. **Phase 11 then repeats per trigger** (decision #22): the build phases happen once, the
cutover phase happens once per queue, and the go-live gate (item 41) is cleared once before the first
production trigger.

---

## 5. Decisions log

| # | Decision | Source |
|---|---|---|
| 1 | Broker stays **RabbitMQ** | carried forward |
| 2 | Host = **ACA Container App + KEDA `rabbitmq`**; supersedes the Functions trigger binding | **Q2** |
| 3 | Broker reached over a **public endpoint using the same connection shape as `PublishRabbitMQActivity`** (`HostName;Port;UserName;Password;VirtualHost`); **TLS is opt-in, default off** for parity, with a documented production recommendation to enable it | **Q3** + §1.7 |
| 4 | Engine auth = **EasyAuth + Entra ID + system MI** with app role `Warewolf_QueueProcessor`, exactly as the Hangfire JobProcessor; the trigger's basic-auth credentials are dropped | **Q4** |
| 5 | Broker connection comes from a **`{RabbitMQSource}.bite` `ConnectionString`**, read with the engine's existing `FromBiteFile` pattern — **no `Dev2.Data` reference** | **Q5** + §1.7 |
| 6 | Logging = **`Dev2Logger` + `ExternalSink` → `CompositeExecutionLogger`** (Console + App Insights + Elasticsearch + Audit), same env-var names as the engine; the WebSocket audit publisher is replaced | **Q6** + §1.8 |
| 7 | **The shared `RabbitMQ.Client` is not upgraded.** The worker ships a **new** .NET 8-native async consumer on a pinned current client and references neither `Warewolf.Driver.RabbitMQ` nor `Dev2.Data`; every existing function stays unchanged | **Q7** + §1.6 |
| 8 | Trigger definitions load from **`Settings/triggers/*.bite`**, sources from **`Settings/{source}.bite`**, WFAES-encrypted at deploy time and read-only at runtime | **note 1** |
| 9 | **AMENDED at implementation time (2026-08-03).** The **Key Vault stack** is consumed as linked shared source per the JobProcessor precedent. The engine's **logger sinks are not**: `ExecutionLoggerBase` depends on Functions invocation correlation (`InstanceCorrelationContext`/`InstanceCorrelationMiddleware`), so linking them would pull `Microsoft.Azure.Functions.Worker` into a non-Functions container. Only `ExecutionLogLevel.cs` + `LoggingConfiguration.cs` (the level semantics and env-var contract — the parts that would actually drift) are linked; the sink itself is `QueueProcessorLogSink`, local to the worker, correlating on the **message** rather than a function invocation | §1.8, implementation |
| 10 | **One Container App per queue-trigger**, one shared ACA environment | §2.1 |
| 11 | Delivery semantics preserved exactly (non-2xx → dead-letter **and ack**; exception → no ack) | §1.2 |
| 12 | Two deliberate improvements: **bounded engine timeout** and **graceful SIGTERM drain** | §2.6 |
| 13 | **On-prem path untouched** — `QueueWorker.exe`, `QueueWorkerMonitor`, and the activities are non-goals | §0 |
| 14 | **Deliverable of this task = this document**; no production code until approved | CLAUDE.md |
| 15 | **`Concurrency` maps to `maxReplicas`**, derived from the trigger `.bite` at deploy time (one source of truth). Three per-trigger modes: **Fixed** (`min = max`, exact today-parity), **Elastic** (`min = 0`, **default**), **Warm** (`min = 1`). The old `min(Concurrency, ProcessorCount)` truncation is deliberately dropped — `maxReplicas = Concurrency` is the honoured ceiling | §2.8 |
| 16 | The **ACA environment / subscription core quota** replaces `ProcessorCount` as the global capacity limit; the deploy script fails loudly when `Σ (maxReplicas × cpu)` would exceed it | §2.8.6 |
| 17 | **One queue per Container App** — never several queues in one app, because ACA scale is per app and multiple rules resolve to the maximum across rules, coupling unrelated triggers | §2.8.6 |
| 18 | **The KEDA target is derived, not chosen: `value = Prefetch × WORKER__MAXCONCURRENCY`.** `Prefetch` (per-consumer `BasicQos`) is the granularity of work distribution and therefore bounds useful parallelism; ignoring it over-provisions (replicas that find an empty queue) or under-provisions (backlog outgrows the running buffers) | §2.8.7 |
| 19 | **Timeout budgets nest:** `ENGINE__TIMEOUTSECONDS ≤ WORKER__SHUTDOWNGRACESECONDS < terminationGracePeriodSeconds`, validated at deploy time — this makes "grace window exceeded" unreachable in normal operation and leaves room for teardown before SIGKILL | §2.6.1 |
| 20 | **The drain re-queues buffered messages explicitly** (`BasicCancel` → `BasicNack(requeue: true)`) rather than waiting for the connection close, so a scale-in hands `Prefetch − 1` messages straight to the surviving replicas instead of parking them | §2.6.1 |
| 21 | **`Concurrency` stays a release-pipeline variable.** The trigger `.bite` carries a `#{…}` substitution token; the deploy reads the file **after** substitution and fails loudly on any surviving token, so operators keep tuning capacity through the existing release variable | §2.8.8 |
| 22 | **Cutover is per-trigger, one queue at a time.** Deploy → verify → set the on-prem trigger's `Concurrency` to 0 → monitor → next queue. Smallest blast radius, per-queue rollback, and the coexistence rule (risk R3) is enforced one queue at a time | **final answer**, Phase 11 |
| 23 | **Scaling is Elastic (`minReplicas = 0`) for every trigger.** `Fixed`, `Warm`, and a raised ceiling are exception paths requiring an explicit script switch and a recorded justification. Cold-start latency on the first message of an idle period is **accepted platform-wide** (risk R1) | **final answer**, §2.8.3 |
| 24 | **The new consumer targets `RabbitMQ.Client` 7.x** (async `IChannel`, async consumers, `ConsumerDispatchConcurrency`, current TLS defaults) — not the 6.8.x sync line. Parity with the old loop is therefore proven **by behaviour** in the Phase 3 parity suite rather than by diff | **final answer**, §1.6 |
| 25 | **TLS: parity now, AMQPS required before production go-live.** Dev/test uses the supplied plaintext source unchanged so no broker work blocks Phases 0–9; the Phase 11 gate hard-blocks the first production trigger until the broker/tunnel terminates TLS and `RABBITMQ__USESSL=true` | **final answer**, §1.7 |
| 26 | **The trigger file is the script's input, in three explicit modes:** `-TriggerFilePath` (one app), `-TriggerPath` + `-TriggerFilter` (one app per matching file), `-TriggerManifestPath` (per-trigger overrides), narrowed by `-TriggerId`. Zero matches is a **hard error**, never a silent no-op | §8.1 |
| 27 | **`Deploy-WwExecutionEngine.ps1` gains `-DeployRabbitMqTriggers`**, mirroring `-DeployJobProcessor` at all five touch points (param block, child path, plan-time guard, plan/summary output, a new Phase 7 fan-out loop). One engine deploy can provision the engine **and** a Container App per pointed trigger; fail-fast unless `-ContinueOnQueueTriggerError`; the `containerapp` az extension is checked **inside the child** so an engine deploy without the toggle gains no prerequisite. **This modifies an existing tested script**, so its Pester suite is extended in lockstep | §8.3 |
| 28 | **Each Container App has its own system MI**, so `Warewolf_QueueProcessor` is assigned **per app in a loop** — and, unlike the JobProcessor's global-scope row, `secure.config` needs a **per-workflow** (`IsServer=false`) `View`+`Execute` row for every trigger's `WorkflowName` | §9.1 |

---

## 6. Finalization record

**No open items.** Every question raised during analysis has been answered; the plan is ready for Phase 0.

| Question | Answer | Where it lands |
|---|---|---|
| Document placement | `Dev/Warewolf.Execution.Lightweight/docs/` (both plans) | This file + the superseded companion |
| Host | **ACA + KEDA** (supersedes the Functions trigger binding) | Decision #2 |
| Broker connectivity | Same shape as `PublishRabbitMQActivity`, from a `{RabbitMQSource}.bite` connection string | Decisions #3, #5, §1.7 |
| Caller identity to the engine | EasyAuth + Entra ID + system MI, role `Warewolf_QueueProcessor` | Decision #4, §2.5 |
| Logging / audit | `Dev2Logger` → `CompositeExecutionLogger`, engine env-var semantics | Decision #6, §1.8 |
| RabbitMQ client | **7.x async-first**, worker-local; nothing shared upgraded | Decisions #7, #24, §1.6 |
| Trigger + source config | `Settings/triggers/*.bite` + `Settings/{source}.bite`, WFAES at rest | Decision #8, §1.7, §2.8.8 |
| Concurrency → replicas | `maxReplicas = Concurrency`; `value = Prefetch × MaxConcurrency` | Decisions #15, #18, §2.8 |
| Scaling mode | **Elastic (`min = 0`) everywhere**; Fixed/Warm/raised-ceiling by exception | Decision #23, §2.8.3 |
| Cutover | **Per-trigger, one queue at a time** | Decision #22, Phase 11 |
| TLS | **Parity now, AMQPS gate before production go-live** | Decision #25, §1.7, Phase 11 |

Three items remain **deliberately deferred to a spike or a separate change**, and none of them blocks
implementation: the exact 7.x patch level and the shipped KEDA scaler semantics (both measured in
Phase 0); the `TriggerBiteReader` Option A-vs-B outcome (Phase 0 item 3, with B pre-agreed as the
fallback); and TLS support on the shared `RabbitMQSource` for the in-engine activities (Phase 2 item 12 —
a Server-affecting change that gets its own plan and tests).

---

## 7. Risks and mitigations

| # | Risk | Mitigation |
|---|---|---|
| R1 | **Cold-start latency at 0 replicas** — a message can wait one KEDA polling interval plus container start; genuinely new behaviour versus always-on processes. **Accepted platform-wide** (decision #23: Elastic everywhere) | Quantified in Phase 0 item 4 and re-measured per queue during the Phase 11 soak (item 44). If a specific queue's latency proves unacceptable, Mode W (`minReplicas = 1`) is applied **to that queue only** — an exception with a recorded justification, not a default |
| R2 | **Scale-in duplicates work** — a replica killed mid-flight leaves the message unacked → another replica redoes it. At-least-once is unchanged, but ACA scales in far more often than processes died | Graceful drain (§2.6) + `terminationGracePeriodSeconds` above the worker grace window + small prefetch. Document that workflows must be idempotent or redelivery-tolerant |
| R3 | **Double-consumption during cutover** | §6 cutover rule: on-prem `Concurrency = 0` as the ACA app goes live |
| R4 | **Plaintext broker traffic** (§1.7) — the confirmed connection shape has no TLS and the tunnel endpoint is public | **Closed by decision #25**: parity in dev/test, and a hard go/no-go gate (Phase 11 item 41a) blocks the first production trigger until the broker/tunnel terminates TLS and `RABBITMQ__USESSL=true`. Residual exposure is limited to non-production traffic |
| R5 | **KEDA metadata semantics differ from the docs** (unacked counting, activation threshold) → flapping or stuck-at-zero | Phase 0 measures the shipped behaviour before any production rule is written |
| R6 | **Trigger `.bite` `$type`/`$ref` deserialization** without the original assemblies | Phase 0 item 3 validates Option A against a real file; Option B (deploy-time transcode) is the pre-agreed fallback |
| R7 | **New consumer ≠ old consumer** in some edge case — heightened by the 7.x async idiom (decision #24), which cannot be diffed line-by-line against the 5.1.2 loop | Phase 3 parity suite written against the same scenarios as the old loop, and treated as a **blocking** acceptance criterion; the old loop stays available on-prem for side-by-side comparison during the Phase 11 soak |
| R8 | **Linked-source drift** if the engine's logging/Key Vault files change shape | Same exposure the JobProcessor already carries; both apps compile the same files, so a breaking change fails the build immediately (that is the point of linking over copying) |
| R9 | **Secret sprawl** across M container apps | Single Key Vault, `keyvaultref` secrets, MI access — no secret in an image or a script |
| R10 | **Engine becomes the bottleneck** once replicas exceed the old `ProcessorCount` cap | Set `maxReplicas` from the measured baseline; load-test the engine before raising it (its hosting tier is an existing Hangfire-plan open item) |

---

## 8. Change-synchronization checklist (CLAUDE.md)

- **New code:** `Dev/Warewolf.Execution.QueueProcessor/` (+ `.Tests`), `Dockerfile`,
  `docker-compose.yml`, `Scripts/Deploy-WwQueueProcessor.ps1` (+ Pester tests).
- **Referenced, unchanged:** `Dev2.Common` (`Dev2Logger`), `Warewolf.Security` (`DpapiWrapper`),
  `Warewolf.Interfaces` (stream contracts), `Warewolf.Common` (`MessageToInputsMapper`); linked sources
  from `Warewolf.Execution.Lightweight` (Key Vault + logging).
- **Explicitly NOT changed:** `Warewolf.Driver.RabbitMQ`, `Dev2.Data`, the three RabbitMQ activities,
  `Dev/Warewolf.QueueWorker/`, `Dev2.Server/QueueProcessorMonitor.cs`, and the shared `RabbitMQ.Client`
  version (decision #7). Any PR touching these files is out of scope for this migration.
- **Modified existing artefacts** (each needs its tests/docs updated in lockstep — §8.3, §9.1):
  - [`Scripts/Deploy-WwExecutionEngine.ps1`](../Scripts/Deploy-WwExecutionEngine.ps1) — the
    `-DeployRabbitMqTriggers` companion toggle + its four other touch points (decision #27). Additive and
    **inert when the switch is absent**, but it is a working, tested script: its Pester suite
    (`Deploy-WwExecutionEngine.Tests.ps1`, green 76/76) must be extended to pin the new parameters and
    guards.
  - [`docs/Deploy-EndToEnd-Runbook.md`](Deploy-EndToEnd-Runbook.md) — new §8 and Teardown renumbered to §9.
  - *Possibly* `Scripts/Configure-WwExecutionAuth-Clients.ps1` — only if the
    `-DaemonContainerApp*` parameters are preferred over the existing `-ManagedIdentityObjectId` path
    (open sub-decision recorded in §9.1; the no-change route is the fallback).
- **Tests:** new worker unit + integration suites; new `Tests/Deploy-WwQueueProcessor.Tests.ps1`;
  **additions to `Tests/Deploy-WwExecutionEngine.Tests.ps1`**; `Dev/.azure/pipeline.yml` job.
  Existing `Warewolf.Trigger.Queue.Tests`, activity, and Server suites must stay green.
- **Docs:** `Scripts/README.md`, `Deploy-RunGuide.md`, `Deploy-EndToEnd-Runbook.md`,
  `Part1-Architecture.md`, this plan, and the superseded companion plan's banner.
- **Skills:** `warewolf-architecture` (third deployable + queue-trigger topology), `warewolf-deploy`
  (ACA deploy flow, ACR, KEDA rule, rollback).
