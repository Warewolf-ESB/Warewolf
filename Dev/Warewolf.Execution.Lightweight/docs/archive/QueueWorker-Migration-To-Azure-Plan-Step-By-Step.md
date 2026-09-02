# QueueWorker → Azure Migration — Step-by-Step Plan

Phased implementation plan for migrating Warewolf's RabbitMQ **queue-trigger** processing from
`WarewolfServer + N × QueueWorker.exe` (Server-hosted process supervisor) to a dedicated
**Azure Functions isolated worker** (`Warewolf.Execution.QueueProcessor`) that consumes RabbitMQ
via the platform trigger binding and invokes workflows against the **Execution Engine**
(Azure Function App, EasyAuth + Entra ID).

Companion documents:
- [HangFire-Migration-To-Azure-Plan-Step-By-Step.md](HangFire-Migration-To-Azure-Plan-Step-By-Step.md)
  — the sibling migration this plan mirrors (suspend/resume → `Warewolf.Execution.EngineJobProcessor`).
- [HangeFire-Azure-Architecture.md](HangeFire-Azure-Architecture.md)
  — target-architecture patterns (security model, MI two-part authorization, Key Vault encryption,
  deploy-script conventions) reused verbatim here.

> **Status: SUPERSEDED (2026-07-30) — retained as the recorded evaluation of the Azure Functions
> option.** The §2.7 hosting fork was decided in favour of **Option C — Azure Container Apps + KEDA**,
> which replaces **decision #2** (Functions RabbitMQ trigger binding → self-hosted consume loop in a
> Linux container). The live, implementable plan is
> [QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md](QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md).
> Sections §1 (analysis) and §2.7 (hosting comparison) remain valid and are still referenced; §3
> (phases), §4 decision #2, and §5 are superseded by that document.
>
> **Original status: PLANNED — not started.** This document is the analysis + phased plan only; no
> production code has been written. It records the confirmed migration decisions (see §4) and the
> residual open items (see §5) that must be resolved as each phase is implemented. Every phase ends
> with its acceptance criteria green and the affected docs/scripts synchronized (CLAUDE.md change-sync
> rule).

---

## 0. Scope and non-goals

**In scope**
- A new dedicated Azure Functions isolated-worker app, **`Warewolf.Execution.QueueProcessor`**, that:
  - consumes a RabbitMQ queue via the **RabbitMQ trigger binding** (platform-managed consume, ack,
    prefetch, and scale-out) — replacing the hand-rolled `EventingBasicConsumer` loop in
    `Warewolf.Driver.RabbitMQ/RabbitConnection.cs`;
  - maps each message to workflow inputs (reusing the existing mapping logic from
    `Warewolf.Common.Framework48/WarewolfWebRequestForwarder.cs`);
  - invokes the target workflow on the **Execution Engine** over HTTPS, authenticated with the
    worker's **system-assigned managed identity** + an Entra **app role** (`Warewolf_QueueProcessor`),
    replacing the trigger's stored basic-auth username/password;
  - preserves the **dead-letter** behaviour (message routed to a dead-letter queue on business
    failure) and adds broker-native poison handling.
- **One deployed Function App per queue-trigger** (decision #3): the queue name and RabbitMQ
  connection are app settings; the rest of the trigger definition is staged into the app.
- RabbitMQ reached over **TLS/AMQPS** at a publicly-reachable broker endpoint (decision #5), with
  credentials in **Azure Key Vault** — the same connection shape the existing activities use
  (`DsfConsumeRabbitMQActivity.PerformExecution`, `Dev2.Activities/Activities/RabbitMQ/Consume/DsfConsumeRabbitMQActivity.cs:213-226`).
- Deployment script `Deploy-WwQueueProcessor.ps1` + Pester tests, mirroring `Deploy-WwJobProcessor.ps1`.

**Non-goals**
- **No change to the on-prem Server / QueueWorker.exe path** — it remains as-is for on-prem
  deployments (`Dev2.Server/QueueProcessorMonitor.cs`, `Dev/Warewolf.QueueWorker/`).
- **No broker migration** — RabbitMQ is retained (decision #1); Azure Service Bus is explicitly not
  adopted here.
- **No change to the RabbitMQ Publish/Consume activities' behaviour** — they continue to run
  in-engine (`PublishRabbitMQActivity`, `DsfPublishRabbitMQActivity`, `DsfConsumeRabbitMQActivity`);
  they only benefit from the shared TLS + Key Vault source-connectivity work in Phase 1.
- No dynamic multi-queue-per-app catalog host (deferred — see §5 open item #1).
- No in-order/exactly-once delivery guarantees (RabbitMQ competing-consumer semantics are
  at-least-once, unchanged from today).

---

## 1. Key analysis results the plan is built on (evidence + confidence)

### 1.1 QueueWorker.exe is a Server-tethered, single-trigger RabbitMQ consumer
`Dev/Warewolf.QueueWorker/` is a console `WinExe` (assembly `QueueWorker`,
`Warewolf.QueueWorker.csproj:1-6`). One process services exactly one queue trigger:

1. Parses `-c <triggerId> -s <serverUrl> -v` (`CommandLineArguments.cs:19-45`).
2. **Connects to the Warewolf Server over SignalR** — `ServerProxyFactory.New(serverEndpoint)` →
   `IEnvironmentConnection.ConnectAsync(Guid.Empty)` (`Program.cs:95-108`). *The worker cannot start
   without a reachable Server.*
3. Loads the `ITriggerQueue` config from the triggers/resource catalog by `TriggerId`; derives the
   RabbitMQ `Source`, the target `WorkflowUrl = {server}/secure/{WorkflowName}.json`, and the
   per-trigger basic-auth `UserName`/`Password` (`WorkerContext.cs:52-80`).
4. Watches the trigger `.bite` file; any change/delete/rename calls `Environment.Exit`, so the
   supervisor restarts the process (hot config reload) (`Program.cs:124-135`).
5. Builds the forwarding pipeline: `WarewolfWebRequestForwarder` maps the message to workflow inputs
   and **POSTs to the workflow URL** with basic auth; on a **non-success** response it publishes the
   original message to a **dead-letter sink** (`WarewolfWebRequestForwarder.cs:57-67`). It is wrapped
   by `LoggingConsumerWrapper` for execution-history auditing (`LoggingConsumerWrapper.cs:38-90`).

*Confidence: High — observed directly.*

### 1.2 The consume loop: manual ack, in-process throttle, prefetch, self-healing watchdog
`RabbitConnection.StartConsuming` (`Warewolf.Driver.RabbitMQ/RabbitConnection.cs:44-110`):
- `EventingBasicConsumer`, **manual ack** (`autoAck:false`); `BasicAck` only when the consumer
  returns `ConsumerResult.Success`.
- In-process concurrency bound by `SemaphoreSlim(Environment.ProcessorCount * 5)`.
- Prefetch/QoS applied per channel via `BasicQos(prefetchCount)` (`RabbitConfig.cs:31-42`).
- A 10-minute watchdog `Timer` calls `QueueDeclarePassive`; a dead/cancelled consumer throws → the
  **process crashes → the supervisor restarts it** (`RabbitConnection.cs:82-110`). This "crash to
  recover" is the resilience model.
- The broker connection is built from `RabbitMQSource` host/port/user/password/vhost with **no TLS**
  today (`Dev2.Data/ServiceModel/RabbitMQSource.cs:38-51`; default port 5672).

*Confidence: High — observed directly.*

### 1.3 Publish/Consume activities share the same source and are engine-side (unaffected)
`PublishRabbitMQActivity` / `DsfPublishRabbitMQActivity` and `DsfConsumeRabbitMQActivity` are
in-workflow activities that resolve a `RabbitMQSource` and open a `ConnectionFactory`
(`DsfConsumeRabbitMQActivity.cs:213-226`, `PublishRabbitMQActivity.cs:186-231`). They run **inside
the engine**, not in QueueWorker, so this migration does not change them — but they share the RabbitMQ
**source connectivity** concern, so the Phase 1 TLS + Key Vault work benefits them too.

*Confidence: High — observed directly.*

### 1.4 Multiple instances = a Server-hosted process supervisor (direct analog of hangfireserver.exe)
This is why the migration mirrors the Hangfire one so cleanly:
- `QueueWorkerMonitor : WorkerMonitor` is created and started inside the Server lifecycle —
  `ServerLifecycleManager.cs:105` (create) and `:274` (`Start()`), immediately beside the Hangfire
  monitor at `:276`.
- Configs are `TriggersCatalog.Instance.Queues` (`QueueConfigLoader.cs:17-30`); the monitor reacts to
  `OnChanged/OnCreated/OnDeleted` (`QueueProcessorMonitor.cs:34-52`).
- Per trigger, `WorkerMonitor` (`Warewolf.Common/OS/WorkerMonitor.cs:44-62`) builds a
  `QueueProcessThreadList` that spawns **`Concurrency` OS processes, capped at
  `Environment.ProcessorCount`** (`ProcessThreadList.cs:100-113`). `Concurrency` comes from
  `IJobConfig` via `ITrigger : IJobConfig` (`IJobConfig.cs:16-20`, `ITrigger.cs:16`).
- Each `QueueProcessThread : ProcessMonitor` launches a separate `QueueWorker.exe -c "{Id}"`
  (`QueueProcessorMonitor.cs:74-82`); a 1-second loop restarts dead processes
  (`WorkerMonitor.cs:86-100`, `ProcessThreadList.cs:64-98`).

**Net parallelism** = (N competing-consumer processes per trigger, N = `Concurrency` ≤ CPU count) ×
(per-process in-flight bound `ProcessorCount × 5` + prefetch). Multiple triggers each get an
independent process set; multiple consumers on the same queue are load-balanced by RabbitMQ.

*Confidence: High — observed directly.*

### 1.5 What is fundamentally different from the Hangfire migration
The Hangfire processor is **poll-based** (TimerTrigger → SQL store → claim → resume) because Hangfire
is a SQL job store. RabbitMQ is **push/event-based**, so the queue processor is **event-driven** and
therefore **simpler**:

| Hangfire processor concern | Queue processor equivalent |
|---|---|
| `JobPollFunction` (TimerTrigger polling SQL) | **`RabbitMQTrigger`** binding (broker pushes messages) |
| Atomic CAS claim (`Scheduled→Processing`) to prevent duplicates | Not needed — RabbitMQ delivers each message to exactly one competing consumer; ack removes it |
| `ReaperFunction` (stale `Processing → Failed`) | Not needed — no claim/lease store; unacked messages auto-requeue on the broker |
| SQL Server / Hangfire schema | No datastore of our own; RabbitMQ is the queue |
| Fire-and-forget POST + short ack window | **Synchronous** POST to the engine per message; ack on success (see §2.6) |
| Fail-only (no auto-retry) | RabbitMQ redelivery + DLX gives at-least-once; business failures → dead-letter (parity with today) |

What **is** reused verbatim from the Hangfire architecture: the **MI two-part authorization contract**
(`Warewolf_JobProcessor` → here `Warewolf_QueueProcessor`), the **Key Vault AES/WFAES** secret
handling for the persisted source, the **deploy-script pattern** (`Deploy-WwJobProcessor.ps1` →
`Deploy-WwQueueProcessor.ps1`), and the **engine-invocation client** already shipped as a reference
implementation in `Dev/Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/`
(`WwExecutionTokenHandler`, `WwExecutionDownstreamService`).

*Confidence: High — the reference client and Hangfire processor exist and were read directly.*

---

## 2. Target architecture (Azure)

```
                         Entra ID (tokens, app roles)
                    ┌────────────────────────────────────┐
                    │  Engine app registration           │
                    │  roles: Warewolf_ClientApps,        │
                    │  Warewolf_JobProcessor,             │
                    │  Warewolf_QueueProcessor  ◄── NEW   │
                    └───────────────┬────────────────────┘
                                    │ MI token (api://<engine>/.default,
                                    │           roles:[Warewolf_QueueProcessor])
   RabbitMQ (TLS / AMQPS 5671, public endpoint)                │
   ┌───────────────────────────┐                               │
   │  work queue  ──────────────┼──push──►┌──────────────────────────────────┐
   │  <queue>.dlq (dead-letter) │◄─────────┤ Warewolf.Execution.QueueProcessor│
   │  <queue>-poison (DLX)      │◄─────────┤ (dedicated Function App,         │
   └───────────────────────────┘          │  dotnet-isolated 8, one PER       │
              ▲   credentials (Key Vault)  │  queue-trigger)                   │
              │                            │ ┌───────────────────────────────┐ │
              │                            │ │ QueueTriggerFunction          │ │
              │                            │ │ [RabbitMQTrigger("%QUEUE%")]  │ │
              │                            │ │  1. map body → inputs         │ │
              │                            │ │  2. POST engine /Secure/{wf}  │─┼──┐
              │                            │ │  3. ack / dead-letter         │ │  │ HTTPS +
              │                            │ └───────────────────────────────┘ │  │ Bearer(MI)
              │                            └───────────────┬──────────────────┘  │
              │ dead-letter publish (business failure)     │ secrets             ▼
              └────────────────────────────────────────────┘         ┌────────────────────────┐
                                          ┌──────────────┐           │ Execution Engine        │
                                          │ Azure Key    │           │ (Function App)          │
                                          │ Vault        │           │  /Public /Secure /Services│
                                          │ (broker creds│           │  WorkflowHttpFunction   │
                                          │  + AES key)  │           │  (EasyAuth + Entra ID)  │
                                          └──────────────┘           └────────────────────────┘
                          App Insights / Elasticsearch / Audit: worker + engine
```

### 2.1 Components

| Component | Hosting | Responsibility |
|---|---|---|
| **`Warewolf.Execution.QueueProcessor`** (new) | Dedicated Azure Function App, dotnet-isolated 8, **one app per queue-trigger** | Consume the configured RabbitMQ queue via the trigger binding; map message → inputs; invoke the engine's `/Secure/{workflow}` route with an MI bearer token; ack on success; dead-letter on business failure; nack/DLX on transport failure |
| **Execution Engine** (`Warewolf.Execution.Lightweight`) | Existing Azure Function App | Executes the target workflow via `WorkflowHttpFunction` `/Secure/*`; authorizes the worker's `Warewolf_QueueProcessor` role; unchanged except for the `secure.config`/route grant (Phase 4) |
| **RabbitMQ broker** | Self-hosted / CloudAMQP, **public TLS endpoint (AMQPS 5671)** | The queue transport (unchanged); work queue + dead-letter queue + DLX/poison queue |
| **Azure Key Vault** | Existing | RabbitMQ connection secret; AES key if the staged source `.bite` is WFAES-encrypted (Phase 2) |
| **Entra ID** | Existing engine app registration | New `Warewolf_QueueProcessor` app role for the worker's system-assigned MI |

### 2.2 What replaces QueueWorker.exe

| QueueWorker.exe / Server responsibility | Azure replacement |
|---|---|
| Server-hosted `WorkerMonitor` + N `QueueWorker.exe` processes (`QueueProcessorMonitor.cs`, `ProcessThreadList.cs`) | **Platform scale-out** — Functions scale controller runs 1..N instances of the trigger; no supervisor, no `.exe`, no `ProcessMonitor` |
| SignalR connect to Server for catalog (`Program.cs:95-108`) | **Removed** — trigger definition is staged into the app (Phase 2); the worker never connects to a Server |
| `EventingBasicConsumer` + manual ack + semaphore + prefetch (`RabbitConnection.cs:44-110`) | **`RabbitMQTrigger` binding** — the extension owns consume, ack/nack, prefetch (`host.json`), and concurrency |
| 10-min `QueueDeclarePassive` watchdog + process restart | **Platform** connection management + health; Functions restarts the instance |
| POST `{server}/secure/{wf}.json` + **basic auth** (`WarewolfWebRequestForwarder.cs`) | POST engine `/Secure/{wf}.json` + **MI Entra token** via the reference `WwExecutionTokenHandler` |
| Dead-letter publish on non-success (`WarewolfWebRequestForwarder.cs:57-67`) | **Preserved** app-level dead-letter publish (business failure) + broker DLX (transport/poison) |
| Execution-history audit (`LoggingConsumerWrapper.cs`) | App Insights custom events + existing composite/audit logger (Phase 6) |
| `Concurrency` (≤ CPU) processes | `host.json` `extensions.rabbitMQ.prefetchCount` (per-instance) × instance scale-out (`functionAppScaleLimit`) — see §2.5 |

### 2.3 Security model (two-part authorization — reused from the Hangfire daemon pattern)

1. **Token side** — the worker Function App's **system-assigned managed identity** is assigned the
   engine app role **`Warewolf_QueueProcessor`**; its token
   (`api://<engine-app-id>/.default`) carries `roles: ["Warewolf_QueueProcessor"]`. Registration uses
   the existing `Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity
   -AppRolesToAssign Warewolf_QueueProcessor` flow (no script change — same as the JobProcessor).
2. **Policy side** — the engine's `secure.config` role map grants `Warewolf_QueueProcessor` **Execute**
   on the target workflow route(s); the route is registered in `RouteAuthorizationRegistry` and
   enforced by `WorkflowAuthorizationMiddleware`. Denials are wrapped as **HTTP 500** (WOLF-8418), so
   the worker treats non-2xx uniformly (dead-letter / retry per §2.6).

Token acquisition + header injection is **not new code** — it is the shipped reference handler
`Dev/Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/Auth/WwExecutionTokenHandler.cs`:
`DefaultAzureCredential` (MI in Azure, Azure CLI locally) with an MSAL client-credentials fallback for
local dev, token caching with a 5-minute expiry buffer, and automatic `Authorization: Bearer`
injection. The worker reuses this handler and the typed `WwExecutionDownstreamService` (extended with
a **POST** variant — the sample only does GET, `WwExecutionDownstreamService.cs:37-66`).

> **Credentials constraint:** the per-trigger basic-auth `UserName`/`Password` stored on the trigger
> (`ITriggerQueue.UserName/Password`, `WorkerContext.cs:76-77`) are **not carried to Azure**. The
> worker's identity to the engine is its MI + app role. If a workflow needs the original caller
> identity, that must be modelled explicitly (open item #4).

### 2.4 RabbitMQ connectivity (TLS + Key Vault)

- The broker is reached over **AMQPS (TLS, port 5671)** at a public endpoint (decision #5), the same
  connection shape the activities already use (`DsfConsumeRabbitMQActivity.PerformExecution`,
  `DsfConsumeRabbitMQActivity.cs:213-226`).
- The RabbitMQ **trigger binding** takes a **connection string app setting** (e.g.
  `amqps://user:pass@host:5671/vhost`) resolved from Key Vault via a Key Vault reference; the worker
  never holds the secret in plaintext config.
- For the **dead-letter publish** path (and for the activities' shared source), TLS support must be
  added to `RabbitMQSource.GetConnectionFactory` (`RabbitMQSource.cs:38-51`), driven by the source /
  connection string, with the credential decrypted from Key Vault (WFAES) exactly like the Hangfire
  persistence `DbSource` (Phase 1).

### 2.5 Concurrency & scaling parity

| Today | Azure |
|---|---|
| `Concurrency` processes (≤ CPU) | Instance scale-out (`functionAppScaleLimit` / plan max) — the Functions scale controller adds instances under load |
| `SemaphoreSlim(ProcessorCount × 5)` per process | `host.json` `extensions.rabbitMQ.prefetchCount` — concurrent in-flight dispatch per instance |
| `BasicQos` prefetch per channel | Same `prefetchCount` knob |
| RabbitMQ round-robins across competing processes | RabbitMQ round-robins across competing **instances** — identical broker semantics |

Mapping rule (to document per trigger): `effective_max_in_flight ≈ prefetchCount × maxInstances`.
Choose `prefetchCount` and the app's scale limit to reproduce the previous `Concurrency × ProcessorCount × 5`
ceiling. **Message ordering is not guaranteed** (unchanged).

> **Resolved (§2.7):** the isolated `Microsoft.Azure.Functions.Worker.Extensions.RabbitMQ` binding scales
> out **only on Elastic Premium / Dedicated** and **requires Runtime Scale Monitoring**
> (`functionsRuntimeScaleMonitoringEnabled=1`) — Consumption / Flex Consumption are unsupported. Pin the
> extension version and its default `prefetchCount`/dead-letter behaviour in Phase 0.

### 2.6 Delivery guarantees, ack, dead-letter, poison

- **At-least-once**, matching today (manual ack after success; a crash mid-processing requeues). The
  workflow invocation must therefore be **idempotent or redelivery-tolerant** — carry the
  `Warewolf-Execution-Id` / `Warewolf-Custom-Transaction-Id` headers to the engine for
  dedupe/audit exactly as `LoggingConsumerWrapper` does today (`LoggingConsumerWrapper.cs:42-49`).
- **Success (engine 2xx)** → function returns normally → the binding acks → message removed.
- **Business failure (engine non-2xx)** → **app-level dead-letter publish** of the original body to
  the configured dead-letter queue (parity with `WarewolfWebRequestForwarder.cs:57-67`), then ack the
  original (so it is not also requeued). *(Decision #6 — preserve today's semantics.)*
- **Transport failure / unhandled exception** (engine unreachable, token failure) → **throw** → the
  binding nacks → RabbitMQ requeues; after the broker/extension retry budget the message goes to the
  **DLX/poison** queue. This adds resilience the `.exe` lacked (it only had crash-restart).

### 2.7 Hosting plan, free-tier reality, and parallel / multi-worker options

> Resolves open items #1 (many triggers → many apps), #2 (scaling knobs), and #6 (hosting plan).

**Hard platform constraint (decisive).** The RabbitMQ **trigger binding** is **not** available on any
free or serverless Functions tier:

- *"The RabbitMQ bindings are only fully supported on **Elastic Premium** and **Dedicated (App Service)**
  plans. **Flex Consumption and Consumption plans aren't yet supported.**"* —
  [RabbitMQ trigger for Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-bindings-rabbitmq-trigger).
- To scale the trigger past one instance, *"the **Runtime Scale Monitoring** setting must be enabled"*
  (`functionsRuntimeScaleMonitoringEnabled = 1`) — same doc.
- The RabbitMQ binding *"doesn't support Microsoft Entra authentication and managed identities"*: the
  **broker** connection string must come from **Key Vault** (confirms §2.4 / decision #5). The MI is only
  for the **engine** call, never for RabbitMQ.
- Dead-letter *"can't be controlled or configured from the RabbitMQ trigger… pre-configure the queue…
  in RabbitMQ"* (DLX) — confirms §2.6 / decision #6.

Consequently **there is no $0 "Free tier" that runs the RabbitMQ trigger**:

| "Free" candidate | Verdict |
|---|---|
| **Consumption** (has the 1 M-exec / 400 k-GB-s monthly free grant) | ❌ RabbitMQ trigger unsupported |
| **Flex Consumption** | ❌ RabbitMQ trigger unsupported |
| **App Service Free (F1) / Shared (D1)** | ❌ No **Always On** → non-HTTP (RabbitMQ) triggers stop when the app idles; the trigger also requires Premium/Dedicated |

*Confidence: High — stated verbatim in the binding doc; the Always-On / Dedicated requirement for non-HTTP
triggers is in the [hosting-plan comparison](https://learn.microsoft.com/azure/azure-functions/functions-scale).*

**Three viable hosting options** (ranked by cost, lowest first):

| # | Host | RabbitMQ consume | Cost floor | Parallelism (one queue) | Idle to $0 | Notes |
|---|---|---|---|---|---|---|
| **A** | **Functions — Dedicated (App Service) Basic B1** | Trigger binding | Lowest fixed (one small always-on instance) | **Manual** scale-out ≤ 3 × `prefetchCount` | No (Always On) | Keeps decision #2. Autoscale needs **Standard S1+** (≤ 10). Many apps share **one** plan. |
| **B** | **Functions — Elastic Premium EP1** | Trigger binding | Highest (≥ 1 warm instance 24/7) | **Event-driven autoscale** ≤ ~100 × `prefetchCount` (needs Runtime Scale Monitoring) | No (min 1) | Best for spiky / high volume; no cold start. Up to 100 apps per plan. |
| **C** | **Azure Container Apps + KEDA `rabbitmq` scaler** | **Existing consume loop** (`RabbitConnection.StartConsuming`) as a container worker — **not** the trigger binding | **Near-free** — 180 k vCPU-s + 360 k GiB-s + 2 M req/month free; **$0 while idle** | **Event-driven autoscale 0 → N** replicas by **queue length** × in-proc semaphore | **Yes** | Closest to "Free tier"; best competing-consumer parity; **re-opens decision #2**. |

*Evidence: Container Apps free grant + scale-to-zero billing — [Billing in Azure Container Apps](https://learn.microsoft.com/azure/container-apps/billing);
KEDA queue-length autoscale + custom rule — [Scaling in Azure Container Apps](https://learn.microsoft.com/azure/container-apps/scale-app)
and the [KEDA RabbitMQ scaler](https://keda.sh/docs/latest/scalers/rabbitmq-queue/). Confidence: High.*

**Why Option C is the true "Free-tier" fit.** Container Apps' Consumption plan bills only vCPU-/GiB-seconds
*above* the monthly free grant and **charges nothing while scaled to zero**; the **KEDA `rabbitmq` scaler**
adds replicas as `desiredReplicas = ceil(queueLength / targetLength)` and removes them (down to zero) after
the cool-down. This reproduces today's model almost exactly — **replicas = the old competing-consumer
processes** — and each replica keeps the existing in-process `SemaphoreSlim`/prefetch throttle
(`RabbitConnection.cs:44-110`), run **verbatim** inside the container rather than re-implemented as a
trigger. It does, however, **replace locked decision #2** (trigger binding → self-hosted consume loop), so
it needs sign-off before Phases 0–5 are rewritten.

**Parallel & multiple workers — deployment and spin-up.** Two independent axes, both supported by every
option above:

- **Breadth — multiple queues (M workers):** one deployed unit **per queue-trigger** (decision #3). The
  deploy script takes the trigger identity as a parameter and is invoked once per trigger over a manifest
  (resolves open item #1); all units share **one** App Service / Premium plan or **one** Container Apps
  environment — so M queues do **not** mean M plans.
- **Depth — parallel on one queue (N competing consumers):** platform scale-out of that unit ×
  per-instance in-flight (`prefetchCount` / semaphore). RabbitMQ round-robins across the instances exactly
  as across the `.exe` processes today: `effective_max_in_flight ≈ per_instance_prefetch × instances`.

| | Option A — Dedicated B1/S1 | Option B — Premium EP1 | Option C — Container Apps + KEDA |
|---|---|---|---|
| **Deploy one worker** | `az functionapp create --plan <plan> --name qp-<queue>` + app settings + assign MI role + zip-deploy | same, on an EP plan | `az containerapp create --name qp-<queue> --environment <env> --image <img>` + system MI |
| **Deploy many queues** | loop the deploy script over the trigger manifest; reuse one plan | same | loop over the manifest; reuse one environment |
| **Spin up parallel (one queue)** | **B1:** manual `--number-of-workers N` (≤ 3, per-plan); **S1+:** autoscale rules (≤ 10) | event-driven autoscale: `functionsRuntimeScaleMonitoringEnabled=1`, `functionAppScaleLimit=N` (≤ ~100) | KEDA rule `--scale-rule-type rabbitmq --scale-rule-metadata "queueName=… value=…"`, `--min-replicas 0 --max-replicas N` |
| **Per-instance concurrency** | `host.json` `extensions.rabbitMQ.prefetchCount` | same | existing `SemaphoreSlim(ProcessorCount×5)` + `BasicQos` |
| **Idle cost** | pays for ≥ 1 always-on instance | pays for ≥ 1 warm instance | **$0 at zero replicas** |

> **Isolation note (Option A depth):** Basic/Standard scale-out is **per App Service plan**, so scaling one
> hot queue scales every app on that plan. Put hot queues on their own plan, or use Option B/C where scale
> is **per app**.

**Recommendation.**
- If **free / near-free is a hard requirement** (accepting that we own the consume loop — which Warewolf
  already has in `Warewolf.Driver.RabbitMQ`) → **Option C — Container Apps + KEDA `rabbitmq`**. Best cost,
  scale-to-zero, cleanest competing-consumer parity. *(Re-opens decision #2.)*
- If **keeping the Functions trigger binding** (decision #2) matters more than $0 → **Option A on Basic B1**
  for dev / low-steady volume (small fixed cost), promoting hot queues to **Standard S1** (autoscale) or
  **EP1** (Option B) for spiky / high volume.

> **Fork decision required** before the decisions log is re-locked and Phases 0–5 adjusted: **Option A/B
> (Functions + trigger binding, small fixed cost)** vs **Option C (Container Apps + KEDA, near-free,
> scale-to-zero)**.

---

## 3. Phases

Each phase lists its deliverable and acceptance criteria. Tests are **proposed, not auto-created** —
present the per-phase test plan and wait for go-ahead before writing tests (CLAUDE.md / warewolf-test).

### Phase 0 — Worker Function App skeleton + RabbitMQ trigger
1. New project `Dev/Warewolf.Execution.QueueProcessor/` (dotnet-isolated 8, `FunctionsApplication`
   host), mirroring `Dev/Warewolf.Execution.EngineJobProcessor/` structure and its `ServerTests.sln`
   membership + `Dev/.azure/pipeline.yml` test job.
2. Add `Microsoft.Azure.Functions.Worker.Extensions.RabbitMQ`; a single
   `QueueTriggerFunction` with `[RabbitMQTrigger("%QUEUE_NAME%", ConnectionStringSetting = "RabbitMQConnection")]`
   that logs and no-ops (skeleton).
3. `host.json` `extensions.rabbitMQ` block (prefetch, DLX toggle); pin the extension version and
   record its default ack/dead-letter behaviour.

**Acceptance:** the app builds, cold-starts locally against a dev RabbitMQ over AMQPS, and consumes +
acks a test message (no engine call yet).

### Phase 1 — RabbitMQ TLS + Key Vault source connectivity
4. Add **TLS/AMQPS** support to `RabbitMQSource.GetConnectionFactory` (`RabbitMQSource.cs:38-51`) —
   `Ssl.Enabled`/port 5671 or `amqps://` URI — without changing on-prem defaults.
5. Resolve the broker connection secret from **Key Vault** (Key Vault reference app setting for the
   trigger binding; WFAES-decrypt for the staged source `.bite` used by the dead-letter publisher),
   reusing the engine's `AesDecryptHook` stack.

**Acceptance:** worker + activities connect over TLS with no plaintext secret in config; on-prem
non-TLS path unchanged; Server build/tests unchanged.

### Phase 2 — Trigger-config staging (one app per trigger)
6. Stage the queue-trigger definition into the app at deploy time (mirroring the Hangfire persistence
   source staging): app settings `QUEUE_NAME`, `RabbitMQConnection` (Key Vault ref), `WORKFLOW_NAME`
   (or full engine route), `MAP_ENTIRE_MESSAGE`, `DEAD_LETTER_QUEUE`, `PREFETCH`; plus the trigger
   `.bite` (or a discrete inputs-mapping setting) providing `Inputs`/`MapEntireMessage`
   (`ITriggerQueue`, `Warewolf.Interfaces/Triggers/Queue/ITriggerQueue.cs:18-32`).
7. Cold-start loader reads these into a strongly-typed options object (read-only; run-from-package
   safe), analogous to `PersistenceConfigLoader`.

**Acceptance:** the worker resolves queue name, workflow, inputs, dead-letter, and prefetch entirely
from its own deployed settings — no Server dependency.

### Phase 3 — Message → inputs mapping + engine invocation (MI token)
8. Reuse the message-to-inputs mapping from `WarewolfWebRequestForwarder`
   (`MessageToInputsMapper`, JSON/XML/whole-message modes, `WarewolfWebRequestForwarder.cs:82-89`) —
   extract it to a shared, framework-neutral helper if needed (it currently lives in
   `Warewolf.Common.Framework48`).
9. Reuse the reference `WwExecutionTokenHandler` + `WwExecutionDownstreamService`
   (`ClientExamples/AzureFunction/`), adding a **POST** method (body or `multipart/form-data` for a
   single `@object` input, mirroring `WarewolfWebRequestForwarder.SendEventToWarewolf`,
   `WarewolfWebRequestForwarder.cs:91-110`). Configure the typed `HttpClient` with the engine base
   address + the token handler in `Program.cs`.
10. Forward the `Warewolf-Execution-Id` / `Warewolf-Custom-Transaction-Id` correlation headers.

**Acceptance:** a message on the queue triggers a real `/Secure/{workflow}` execution on a deployed
engine using the worker's MI; success acks, non-2xx is surfaced to the dead-letter path (Phase 5).

### Phase 4 — Engine authorization wiring
11. Define the `Warewolf_QueueProcessor` app role on the engine app registration; grant it **Execute**
    on the target workflow route in `secure.config`; register the route in `RouteAuthorizationRegistry`.
12. Assign the role to the worker's system MI via the existing daemon flow
    (`Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity
    -AppRolesToAssign Warewolf_QueueProcessor`).

**Acceptance:** the worker's token is accepted for the workflow; an unauthorized principal is rejected
(HTTP 500 per WOLF-8418); the engine's other routes are unaffected.

### Phase 5 — Dead-letter, poison, and retry semantics
13. Implement the **app-level dead-letter publish** on engine non-2xx (parity with
    `WarewolfWebRequestForwarder.cs:57-67`) using the RabbitMQ driver publisher
    (`RabbitPublisher`/`RabbitConnection.NewPublisher`) against the configured dead-letter queue.
14. Configure a broker **DLX/poison** queue for transport failures + the extension's retry budget;
    document the two failure classes (§2.6) and their routing.

**Acceptance:** engine business failure → message in the dead-letter queue + original acked; transport
failure → nack → requeue → DLX after N attempts; no message silently lost.

### Phase 6 — Telemetry / audit parity
15. Emit execution-history audit events (`ExecutionHistory`/`ExecutionInfo`, as
    `LoggingConsumerWrapper.cs:60-102` does) via the engine's composite/audit logger and App Insights
    custom events (start/success/failure with `correlationId = executionId`).

**Acceptance:** each processed message produces the same audit signal as today, visible in App Insights
+ the existing audit sink.

### Phase 7 — Deployment + documentation sync
16. New `Dev/Warewolf.Execution.Lightweight/Scripts/Deploy-WwQueueProcessor.ps1` (self-contained
    orchestrator: infra + system MI + Key Vault wiring + stage trigger settings/`.bite` + `QUEUE_*`/
    `ENGINE_*` app settings + deploy + verify), mirroring `Deploy-WwJobProcessor.ps1`. Because the
    model is **one app per trigger**, the script takes the trigger identity as a parameter and is
    invoked once per queue-trigger (document the loop/automation for many triggers — open item #1).
17. Publish each app to its own directory and stage into a fresh temp dir (never mutating publish
    output), mirroring the deploy-isolation decision (#12) in the Hangfire plan.
18. Sync docs: `Scripts/README.md`, `Deploy-RunGuide.md`, `Deploy-EndToEnd-Runbook.md` (new
    QueueProcessor deploy+authorize section), `Part1-Architecture.md`, and the `warewolf-architecture`
    / `warewolf-deploy` skills.

**Acceptance:** `Deploy-WwQueueProcessor.ps1` parses clean, passes `-LoadFunctionsOnly`, and its Pester
tests are green (mirroring `Deploy-WwJobProcessor.Tests.ps1`).

### Phase 8 — Test matrix
Parity-driven — every scenario maps an existing behaviour to the new implementation:

| Area | Scenarios |
|---|---|
| Consume/ack | Message consumed → engine 2xx → acked → removed; prefetch bounds in-flight; competing instances load-balance (no double-processing of one message) |
| Mapping | JSON / XML / whole-message (`MapEntireMessage`) → correct inputs (parity with `MessageToInputsMapper`); single `@object` input → `multipart/form-data` |
| Auth | Worker MI token accepted for `Warewolf_QueueProcessor`; missing/wrong role → 500 (WOLF-8418); token cache refresh at expiry buffer |
| TLS/secret | AMQPS connect with Key Vault-sourced credential; no plaintext secret; on-prem non-TLS unchanged |
| Dead-letter | Engine non-2xx → original in dead-letter queue + acked; assert body parity with input |
| Poison/transport | Engine unreachable / token failure → nack → requeue → DLX after N attempts; no loss |
| Idempotency | Redelivery carries the same `Warewolf-Execution-Id`; document/verify dedupe expectation |
| Config | All of queue/workflow/inputs/dead-letter/prefetch resolved from app settings + staged `.bite`; no Server dependency |
| Deploy | `Deploy-WwQueueProcessor.ps1` DryRun end-to-end (az-shim); settings staging; fail-loud on missing params |
| Telemetry | Start/success/failure audit events emitted with correlation id |

---

## 4. Decisions log (resolved)

| # | Decision |
|---|---|
| 1 | **Broker stays RabbitMQ** (self-hosted / CloudAMQP) — no Azure Service Bus migration |
| 2 | **Host = Azure Functions isolated worker** using the RabbitMQ **trigger binding** (`Microsoft.Azure.Functions.Worker.Extensions.RabbitMQ`) — mirrors the `EngineJobProcessor` Function App |
| 3 | **One deployed Function App per queue-trigger** — queue name + connection are app settings; the trigger definition is staged into the app (no dynamic multi-queue host in v1) |
| 4 | **Execution auth = worker system MI + Entra app role `Warewolf_QueueProcessor`** calling the engine `/Secure/{workflow}` route — replaces the trigger's stored basic-auth username/password |
| 5 | **RabbitMQ reached over TLS/AMQPS at a public endpoint**; broker credentials in Key Vault; same connection shape as `DsfConsumeRabbitMQActivity.PerformExecution` |
| 6 | **Dead-letter parity** — engine non-2xx → app-level dead-letter publish + ack original (as today); transport failures → nack → DLX/poison |
| 7 | **Reuse the shipped reference client** (`WwExecutionTokenHandler`, `WwExecutionDownstreamService`) for engine invocation, extended with a POST method — no bespoke token code |
| 8 | **Deliverable = this plan document only**; no production code until the plan is approved |

> **Hosting caveat (see §2.7):** decision #2 assumes the RabbitMQ **trigger binding**, which is available
> **only on Elastic Premium / Dedicated — no free tier**. If a free / scale-to-zero host is required,
> **Option C (Azure Container Apps + KEDA `rabbitmq`)** supersedes decision #2 by running the existing
> consume loop as a container worker. This fork is **pending sign-off**.

## 5. Open items

| # | Item | Status |
|---|---|---|
| 1 | **Many triggers → many apps.** One-unit-per-trigger (decision #3) means M queue-triggers = M deployed units. | **Resolved (§2.7)** — deploy script loops over a trigger manifest, one unit per trigger, all sharing one App Service/Premium plan or one Container Apps environment (M queues ≠ M plans). Still implemented in Phase 7. |
| 2 | **RabbitMQ extension scaling knobs.** Which plans scale the trigger out, and what enables it. | **Resolved (§2.7)** — the trigger scales out **only on Elastic Premium / Dedicated** and **requires Runtime Scale Monitoring** (`functionsRuntimeScaleMonitoringEnabled=1`); Consumption / Flex are unsupported. Pin extension version + default `prefetchCount`/DLX in Phase 0. |
| 3 | **Input-mapping source of truth.** Whether `Inputs`/`MapEntireMessage` come from a staged trigger `.bite` (full fidelity) or discrete app settings (simpler). `MessageToInputsMapper` currently lives in `Warewolf.Common.Framework48` — decide extract-vs-reference | Decide in Phase 2/3 |
| 4 | **Caller identity to the workflow.** The trigger's basic-auth identity is dropped in favour of the worker MI. If a workflow depends on the original username, model it explicitly (header/claim) | Decide before cutover |
| 5 | **Dead-letter transport.** Whether the app-level dead-letter publish reuses `RabbitMQSource`+driver (needs TLS from Phase 1) or the broker DLX alone; and whether both are needed | Decide in Phase 5 |
| 6 | **Hosting plan (incl. "Free tier").** Which host runs the worker. | **Resolved (§2.7)** — **no free / serverless Functions tier hosts the RabbitMQ trigger** (Consumption/Flex unsupported; F1 has no Always On). Viable: **A** Functions/Dedicated **B1** (lowest fixed), **B** Functions/Premium **EP1** (spiky), **C** **Container Apps + KEDA `rabbitmq`** (near-free, scale-to-zero). **Awaiting fork sign-off** — Option C re-opens decision #2. |

## 6. Suggested sequencing

Critical path: **Phase 0 → 1 → 2 → 3 → 4**, then dead-letter/telemetry (5, 6) in parallel; deploy +
docs (7) and the test matrix (8) close out. Phases 5–6 can proceed once Phase 3 (engine invocation)
is green. Each phase ends with its acceptance criteria met and the affected docs/scripts synchronized.
