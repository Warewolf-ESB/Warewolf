# Shovel Bridge Architecture — RabbitMQ → Azure Service Bus → Lightweight Execution Engine

## Purpose

Trigger Warewolf workflow executions on the **Lightweight** (Azure Functions) execution
engine from RabbitMQ messages, without building a native RabbitMQ trigger binding for
Azure Functions (none exists — Azure Functions natively supports Service Bus, Storage
Queues and Event Hubs, but not RabbitMQ). Instead, RabbitMQ's **Shovel** plugin bridges
messages from an existing RabbitMQ source queue to an Azure Service Bus queue, which
*does* have a first-party Functions trigger.

## Why not extend `QueueWorker.exe`?

The **Server** engine (`Dev2.Server`) already has a mature, production RabbitMQ trigger:
`QueueWorkerMonitor` spawns one `QueueWorker.exe` child process per Studio-authored
trigger resource, which consumes a RabbitMQ queue and forwards each message as an HTTP
call to `Dev2.Server`'s `/secure/{workflow}.json` route (see
`Dev/Dev2.Server/QueueProcessorMonitor.cs`, `Dev/Warewolf.QueueWorker/`). That mechanism
is a long-running, supervised child process — a model that has no equivalent inside a
**serverless** Azure Functions app. Lightweight has nothing to host a persistent
listener, so `QueueWorker.exe` cannot be reused or repointed at it without giving up
Lightweight's serverless deployment model. The Shovel bridge preserves that model:
Service Bus is a durable, managed message store, and the Functions runtime spins up
compute only when a message arrives.

## Topology

```
RabbitMQ producer (existing/unchanged)
   │  publishes JSON: {"route":"secure|public","workflow":"...","inputs":{...}}
   ▼
RabbitMQ source queue (AMQP 0.9.1)
   │  Shovel plugin (rabbitmq_shovel + rabbitmq_shovel_management)
   │  dest-protocol: amqp10
   │  dest-uri: amqps://<shovel-send-policy>:<SAS-key>@<namespace>.servicebus.windows.net:5671/?sasl=plain
   │  dest-address: <ServiceBusQueueName>  (default: wwexecution-queue)
   ▼
Azure Service Bus queue (dead-lettering + max-delivery-count configured)
   │  ServiceBusTrigger (Managed Identity listen connection, by default)
   ▼
WorkflowQueueTrigger (Warewolf.Execution.ServiceBusWorker)
   │  parses message → acquires Entra token (Managed Identity) → HTTP call
   ▼
Lightweight Execution Engine (Warewolf.Execution.Lightweight, /secure or /public route)
```

## Message contract

Shovel forwards message bytes **verbatim** — it cannot reshape payloads. The RabbitMQ
producer must publish exactly the JSON `WorkflowQueueTrigger` expects:

```json
{ "route": "secure", "workflow": "Hello World", "inputs": { "Name": "FromRabbitMq" } }
```

- `route` (optional) — `secure` (default) or `public`.
- `workflow` (required) — the workflow name.
- `inputs` (optional) — a string map sent as query-string parameters.

If an existing RabbitMQ producer uses a different schema, it must be changed to emit
this contract, or an adapter service must sit between the producer and the source
queue — Shovel itself cannot transform messages.

## Provisioning

Two scripts under `Scripts/`, run in order:

1. **`Deploy-WwExecutionServiceBusWorker.ps1`** — provisions the Service Bus namespace,
   the destination queue (dead-lettering, configurable max delivery count), the
   worker's Function App (Managed Identity by default for the *listen* side), and a
   queue-scoped **Send-only** SAS authorization rule (default name `shovel-send`) for
   the Shovel's destination credential. See the script's own `.SYNOPSIS`/`.DESCRIPTION`
   for full parameter reference.
2. **`Configure-RabbitMqShovel.ps1`** — configures the dynamic RabbitMQ Shovel parameter
   via the RabbitMQ Management HTTP API (`PUT /api/parameters/shovel/{vhost}/{name}`),
   pointing at the existing RabbitMQ source queue and the Service Bus queue + SAS rule
   from step 1 (fetching the key live via `az`, or accepting it directly). See the
   script's own `.SYNOPSIS`/`.DESCRIPTION` for full parameter reference, including the
   one-time, broker-host `rabbitmq-plugins enable rabbitmq_shovel
   rabbitmq_shovel_management` prerequisite that this script cannot automate remotely.

Both scripts follow the repo's params-first/prompt-if-missing, `-DryRun`, masked-summary
+ transcript conventions (see `Scripts/README.md`).

## Security

- The Shovel's destination credential is a **queue-scoped, Send-only** SAS rule —
  least privilege; it can never Listen or Manage.
- The worker's Service Bus **listen** connection uses **Managed Identity** by default
  (`ServiceBusConnection__fullyQualifiedNamespace` + `ServiceBusConnection__credential
  =managedidentity`) — no SAS secret in the Function App's own settings.
- The worker calls the Lightweight engine via Managed Identity too (see
  `Warewolf.Execution.ServiceBusWorker/README.md` and
  `docs/KB-ClientApps-Configuration.md` §2.6) — role assignment
  (`Warewolf_ClientApps`) is a separate operator step via
  `Configure-WwExecutionAuth-Clients.ps1`.
- No secret is ever written to disk by either provisioning script; SAS keys are held
  in memory only (`SecureString` parameters, `ConvertFrom-SecureStringPlain` scoped to
  the smallest possible lifetime, cleared in a `finally` block).
- Follow `docs/KeyRotationRunbook.md` conventions for rotating the `shovel-send` SAS
  key; rotating it requires re-running `Configure-RabbitMqShovel.ps1` (or a future
  `-RotateOnly` mode) so the Shovel picks up the new key.

## Known risks / open work

- **Correlation-id / custom property survival across the AMQP 0.9.1 → AMQP 1.0
  translation is an accepted risk** (explicit decision — not spiked further). This is a
  known lossy point in protocol bridging; revisit with a dedicated spike if end-to-end
  tracing through the bridge becomes a hard requirement.
- **Shovel health monitoring — closed.** A stalled or continuously-erroring shovel (e.g.
  after a SAS key rotation that wasn't propagated) used to silently stop triggering
  workflows with no further signal. `Configure-RabbitMqShovel.ps1` still polls the
  shovel's `running` state once at configuration time; ongoing health is now covered by
  `Scripts/Monitor-RabbitMqShovel.ps1` — a standalone script the operator schedules
  (Task Scheduler / cron / Azure Automation) since RabbitMQ is customer/on-prem
  infrastructure a serverless Function can't reliably poll. It re-checks
  `GET /api/shovels/{vhost}` and, on an unhealthy or missing shovel, emits a
  `ShovelHealthCheck` Application Insights custom event (`healthy=false`) via the plain
  HTTP `/v2/track` ingestion API and throws (non-zero exit) so the scheduler can alert;
  `-SendHeartbeatOnHealthy` can also emit a heartbeat event on healthy checks. Covered by
  `Scripts/Tests/Monitor-RabbitMqShovel.Tests.ps1` (16 tests). The Azure Monitor alert
  rule itself (watching `customEvents` for `healthy == "false"`, or a heartbeat gap) is a
  one-time operator setup step, not something this script provisions.
- **End-to-end integration test — closed.** `Scripts/Tests/Integration/Test-ShovelBridgeE2E.ps1`
  chains a real RabbitMQ container (shovel + management plugins pre-baked into a
  bind-mounted `enabled_plugins` file), a real shovel (configured via
  `Configure-RabbitMqShovel.ps1 -LoadFunctionsOnly`), the local Azure Service Bus emulator
  (+ its Azure SQL Edge metadata-store dependency), and the
  `Warewolf.Execution.ServiceBusWorker.E2EHarness` console app, which publishes a
  uniquely-marked message to RabbitMQ and polls the Service Bus queue for it via the
  `Azure.Messaging.ServiceBus` SDK — proving the bridge actually delivers a message
  end-to-end, against real containers, not mocks. Wired into CI as the
  `ShovelBridgeE2ETest` job in `Dev/.azure/pipeline.yml` (Emulator mode, local containers
  only). All readiness polling is done purely over the RabbitMQ management HTTP API
  (`/api/overview`, `/api/shovels`) — deliberately never via `docker exec rabbitmqctl` /
  `docker exec rabbitmq-plugins`, since those CLI tools each spin up their own short-lived
  Erlang node to talk to the broker over distribution, and doing so repeatedly while the
  main node is still booting was found to race its `.erlang.cookie` handling and crash it.
  The script also supports an `-DestinationMode ExternalServiceBus` variant, against a
  real (caller-provisioned) Service Bus namespace/queue. This is now wired into CI as the
  `ShovelBridgeE2ETest_ExternalServiceBus` job in the "Test on Azure" stage of
  `Dev/.azure/pipeline-CLOUD.yml`, against the dedicated `WarewolfShovelBridgeTesting`
  namespace (resource group `DEV2`). That job logs in to Azure CLI with the same service
  principal already used elsewhere in that pipeline (`AzureClientId`/`AzureClientSecret`/
  `AzureTenantId`), then idempotently ensures a single fixed queue
  (`wwexecution-queue-e2e`) and its two queue-scoped SAS rules exist — `shovel-e2e-send`
  (Send-only, used to build `-ExternalShovelDestUri`) and `shovel-e2e-listen` (Listen-only,
  used to build `-ExternalServiceBusConnectionString`) — mirroring the same least-privilege
  Send/Listen rule split `Deploy-WwExecutionServiceBusWorker.ps1` uses. Nothing is deleted
  after the run: the queue/rules are provisioned once and reused by every run against that
  dedicated testing namespace, rather than provisioned/torn down per-run.

## Promotion status

This worker was originally built as a client example and has been promoted to a
**first-class, officially supported** component of the Lightweight execution engine:

- Relocated from `Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus` to the
  top-level `Warewolf.Execution.ServiceBusWorker/` project (still built via
  `.\Compile.ps1 -ServerTests`, part of `Dev\ServerTests.sln`).
- Dedicated unit test project and CI job — see `Dev/.azure/pipeline.yml`
  (`LightweightExecutionUnitTests` job).
- Deploy chaining via `Deploy-WwExecutionEngine.ps1 -DeployServiceBusWorker`, mirroring
  `-DeployJobProcessor`.
- Shovel health monitoring is closed (`Scripts/Monitor-RabbitMqShovel.ps1`), and the
  automated E2E test is closed (`Scripts/Tests/Integration/Test-ShovelBridgeE2E.ps1`,
  wired into CI as the `ShovelBridgeE2ETest` job) — see "Known risks / open work" above.
  The `ExternalServiceBus`-mode variant against the real `WarewolfShovelBridgeTesting`
  Service Bus namespace is now wired into `pipeline-CLOUD.yml`'s "Test on Azure" stage as
  the `ShovelBridgeE2ETest_ExternalServiceBus` job.


## See also

- `Scripts/README.md` — script index.
- `Warewolf.Execution.ServiceBusWorker/README.md` — the
  worker's own architecture, auth, and message-contract documentation.
- `docs/KB-ClientApps-Configuration.md` §2.6 — the worker's Entra app registration.
- `docs/KeyRotationRunbook.md` — SAS/key rotation conventions.
