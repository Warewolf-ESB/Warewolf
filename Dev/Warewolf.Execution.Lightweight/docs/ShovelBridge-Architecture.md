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
WorkflowQueueTrigger (Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus)
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
  `Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/README.md` and
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
- **Shovel health monitoring is not yet automated.** A stalled or continuously-erroring
  shovel (e.g. after an SAS key rotation that wasn't propagated) silently stops
  triggering workflows. `Configure-RabbitMqShovel.ps1` polls the shovel's `running`
  state once at configuration time, but there is no ongoing alerting yet — consider the
  RabbitMQ Prometheus exporter or a periodic `GET /api/shovels/{vhost}` health check.
- **No end-to-end integration test yet** chaining a real RabbitMQ + Shovel + Service Bus
  + the worker + the Lightweight engine — the two provisioning scripts have full Pester
  suites (`Scripts/Tests/Deploy-WwExecutionServiceBusWorker.Tests.ps1`,
  `Scripts/Tests/Configure-RabbitMqShovel.Tests.ps1`) and have each been proven against
  real infrastructure manually, but a fully automated E2E test would need a dedicated
  disposable Service Bus namespace (or emulator) and is a larger follow-up effort.

## See also

- `Scripts/README.md` — script index.
- `Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/README.md` — the
  worker's own architecture, auth, and message-contract documentation.
- `docs/KB-ClientApps-Configuration.md` §2.6 — the worker's Entra app registration.
- `docs/KeyRotationRunbook.md` — SAS/key rotation conventions.
