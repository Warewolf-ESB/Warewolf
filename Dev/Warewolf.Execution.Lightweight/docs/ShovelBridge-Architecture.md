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
   rabbitmq_shovel_management` prerequisite that this script cannot automate remotely,
   **and** the broker-host `advanced.config` TLS-hostname-check prerequisite described
   in "Security" below (also not automatable remotely) — without it, every shovel this
   script configures against a real Service Bus namespace fails to connect.

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
- **The destination namespace MUST keep local (SAS) authentication enabled**
  (`disableLocalAuth` / "Local authentication" = **off**, i.e. `false`). RabbitMQ's
  built-in Shovel plugin (both the AMQP 0.9.1 and AMQP 1.0 dialects) only supports
  SASL PLAIN with a `policy-name:key` credential pair — it has no Azure AD/OAuth
  client, so it cannot use Managed Identity or an Entra token. If a security
  baseline/policy flips `disableLocalAuth` to `true` on the namespace, the Shovel's
  `dest-uri` connection fails outright (surfaces as the Shovel going to `terminated`
  with reason `"failed to connect to destination"` — see
  `Scripts/Tests/Integration/Test-ShovelBridgeE2E.ps1`). This is orthogonal to, and
  does not conflict with, the worker's own **Managed Identity** listen connection
  above — only the Shovel's Send-side credential needs SAS. Check with
  `az servicebus namespace show --name <ns> --resource-group <rg> --query
  disableLocalAuth` and restore it with `az servicebus namespace update --name <ns>
  --resource-group <rg> --disable-local-auth false` if it drifts. The
  `ShovelBridgeE2ETest_ExternalServiceBus` CI job (below) self-heals this specific
  drift against the `WarewolfShovelBridgeTesting` testing namespace automatically —
  its `Ensure Service Bus namespace allows local (SAS) auth for the Shovel` step
  checks `disableLocalAuth` before every run and restores `false` if a policy has
  flipped it. Any *other* namespace (e.g. a real deployment target) still needs this
  checked/restored manually per the guidance above.
- **The broker needs a `customize_hostname_check` TLS fix in `advanced.config`, or
  every Shovel destination connection to a real Service Bus namespace fails.** This
  produces the *exact same* symptom as the `disableLocalAuth` drift above (Shovel
  `terminated`, reason `"failed to connect to destination"`), but is a completely
  different, unrelated root cause — **do not assume `disableLocalAuth` drift is the
  only explanation for that reason string.** Root cause: the Shovel's dest-uri
  (`amqps://...?sasl=plain`, no explicit `verify=` override) uses Erlang's default TLS
  peer verification (`verify_peer`), and Erlang's *default* certificate hostname check
  does a **literal** match against the certificate's SANs — it does not expand
  wildcards the way RFC 6125 (and Erlang's own `https`-specific match function) does.
  Azure Service Bus presents a certificate for `*.servicebus.windows.net` /
  `servicebus.windows.net`, which never literal-matches a real namespace FQDN (e.g.
  `mynamespace.servicebus.windows.net`), so the TLS handshake always fails with
  `{tls_alert,{bad_certificate,{bad_cert,{hostname_check_failed, ...}}}}` in the
  broker's own log (only visible via broker logs / `Admin > Shovel Status`, not the
  Management API's shovel-status summary). **Root-caused and reproduced live** against
  the real `WarewolfShovelBridgeTesting` namespace (RabbitMQ 4.3.4) by standing up a
  local RabbitMQ broker and configuring the identical dynamic shovel directly against
  that namespace: the connection fails identically without the fix below, and
  succeeds (shovel reaches `running`/`flow`, message delivered end-to-end) with it —
  confirmed with **full TLS peer/chain validation still enabled** (`verify_peer`, no
  downgrade to `verify_none`). Fix: add this to the broker's `advanced.config`
  (`%APPDATA%\RabbitMQ\advanced.config` on a Windows/choco install,
  `/etc/rabbitmq/advanced.config` on Linux) and restart the broker so it's read at
  boot:
  ```erlang
  [
    {amqp10_client, [
      {ssl_options, [
        {customize_hostname_check, [
          {match_fun, public_key:pkix_verify_hostname_match_fun(https)}
        ]}
      ]}
    ]}
  ].
  ```
  This applies Erlang's own wildcard-aware match function (built for `https`, but
  generically RFC 6125-correct) to `amqp10_client` connections, restoring correct
  wildcard-SAN acceptance without weakening certificate validation. The
  `ShovelBridgeE2ETest_ExternalServiceBus` CI job's `Install & start local RabbitMQ`
  step now writes this `advanced.config` automatically (see `pipeline-CLOUD.yml`).
  Any broker you don't control the config of (a shared/managed broker) cannot apply
  this fix — the only remaining option there is `-DestUriVerifyNone` (implemented as an
  opt-in switch on `Configure-RabbitMqShovel.ps1` / the E2E test scripts, appending
  `&verify=verify_none` to the dest-uri), which disables *all* peer certificate
  validation (not just the hostname check); not recommended outside of diagnosing this
  specific case.
- **Erlang/OTP 26+ brokers additionally need an explicit `cacertfile`/`cacerts` TLS
  option on the dest-uri, or the Shovel crash-loops with `{cacerts, undefined}`.** This
  is a SEPARATE, ADDITIONAL prerequisite from the hostname-check fix above — a broker
  can have the `customize_hostname_check` fix applied and still fail with this error,
  because OTP 26 changed `ssl_options` default behaviour: `verify_peer` (Erlang's
  default, used whenever dest-uri omits an explicit `verify=` override, as this script's
  dest-uri always does) now requires the caller to explicitly supply a CA trust store —
  it no longer falls back to any implicit/system store. Without one, the AMQP 1.0
  client fails immediately on every connection attempt with `{cacerts, undefined}` in
  the broker's log. **This produces a near-instant fail/retry loop that the Management
  API's shovel-status summary reports only as `"starting"`** (the same transient state
  it shows during a normal connection attempt) — polling `/api/shovels/{vhost}` every
  few seconds can miss the brief `terminated` state between retries entirely, making a
  crash-loop indistinguishable from a genuinely slow/hanging connection attempt without
  checking the broker's own log. **Root-caused live** against the real
  `WarewolfShovelBridgeTesting` namespace: the shovel remained stuck at `"starting"` per
  the Management API even after applying the `customize_hostname_check` fix above and
  restarting the broker, and only the broker's own log revealed the real
  `{cacerts, undefined}` reason. Fix: append a `cacertfile` (or `cacerts`) parameter
  pointing at a CA bundle to the dest-uri, e.g. `&cacertfile=/etc/ssl/certs/ca-certificates.crt`
  (the standard system CA bundle path on the Debian-based official RabbitMQ Docker
  image), then reconfigure the shovel with the updated dest-uri (e.g.
  `rabbitmqctl set_parameter shovel <name> '<json with fixed dest-uri>'`, or re-run
  `Configure-RabbitMqShovel.ps1` / the E2E test script with `-DestUriCaCertFile
  /etc/ssl/certs/ca-certificates.crt`, both of which append this the same way).
  Because the dest-uri's credential-bearing userinfo is masked by design in both
  `rabbitmqctl` and Management API output, this fix must be applied by whoever holds
  the real SAS key/credentials — it cannot be verified or reapplied from masked output
  alone.

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
  chains RabbitMQ (`-RabbitMqMode Container`, the default: a real disposable container with
  shovel + management plugins pre-baked into a bind-mounted `enabled_plugins` file; or
  `-RabbitMqMode External`: an already-running, caller-supplied broker), a real shovel
  (configured via `Configure-RabbitMqShovel.ps1 -LoadFunctionsOnly`), the local Azure Service
  Bus emulator (+ its Azure SQL Edge metadata-store dependency), and the
  `Warewolf.Execution.ServiceBusWorker.E2EHarness` console app, which publishes a
  uniquely-marked message to RabbitMQ and polls the Service Bus queue for it via the
  `Azure.Messaging.ServiceBus` SDK — proving the bridge actually delivers a message
  end-to-end, against a real broker/containers, not mocks. Wired into CI as the
  `ShovelBridgeE2ETest` job in `Dev/.azure/pipeline.yml` (Container + Emulator mode, local
  containers only). All readiness polling is done purely over the RabbitMQ management HTTP API
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
  `AzureTenantId`), then idempotently ensures the namespace still allows local (SAS)
  authentication (`disableLocalAuth=false` — see "Security" above; this is a documented
  drift risk if a security baseline/policy re-flips it, and would otherwise fail the
  Shovel's dest-uri connection AND the destination-queue existence check with 401s),
  then idempotently ensures a single fixed queue
  (`wwexecution-queue-e2e`) and its two queue-scoped SAS rules exist — `shovel-e2e-send`
  (Send-only, used to build `-ExternalShovelDestUri`) and `shovel-e2e-listen` (Listen-only,
  used to build `-ExternalServiceBusConnectionString`) — mirroring the same least-privilege
  Send/Listen rule split `Deploy-WwExecutionServiceBusWorker.ps1` uses. Nothing is deleted
  after the run: the queue/rules are provisioned once and reused by every run against that
  dedicated testing namespace, rather than provisioned/torn down per-run. That same job also
  runs RabbitMQ in `-RabbitMqMode External`, against a **local, choco-installed RabbitMQ
  Windows service on the hosted agent itself** (management API at `http://localhost:15672`,
  loopback-only `guest` user) instead of a docker-run `rabbitmq:3-management` container:
  Microsoft-hosted Windows (`windows-2022`) build agents' Docker daemon only supports Windows
  containers (no Hyper-V/WSL2 Linux-container backend on the hosted VM, and no in-place
  daemon switch available there), so it cannot pull that Linux-only image.
  `-RabbitMqMode External` only needs a reachable broker with management + shovel +
  shovel_management enabled and caller-supplied creds, so this reuses the same
  `choco install rabbitmq` + `rabbitmq.conf` (`transient_nonexcl_queues` re-permit) technique
  `TestRun.ps1`'s `Start-HostRabbitMQServer` already uses for other RabbitMQ-dependent CI jobs
  on Windows agents, plus explicitly enabling `rabbitmq_shovel`/`rabbitmq_shovel_management`
  via `rabbitmq-plugins.bat` (not needed by `Start-HostRabbitMQServer`'s own callers), **plus
  writing the `advanced.config` TLS-hostname-check fix described in "Security" above**
  (without it this job fails identically to how it did before that fix was found — Shovel
  `terminated`, `"failed to connect to destination"`). This previously pointed at a
  standing, self-hosted "Warewolf DevOps RabbitMQ Source" broker
  (`rabbitmq.warewolf.online`) reached over a non-Azure tunnel; that was replaced because its
  outbound network path to the real Service Bus destination was *assumed*
  unreliable/opaque from CI — the shovel would connect to the source fine but never reach
  the `running` state against Service Bus. In hindsight that broker was very likely hitting
  the same TLS-hostname-check bug documented above — or, per the `{cacerts, undefined}`
  finding also documented in "Security" above (confirmed live against this exact broker,
  after the hostname-check fix alone did not resolve it), the Erlang/OTP 26 explicit-CA
  requirement — rather than a genuine network problem.
  A broker local to the hosted agent still uses Microsoft's own outbound networking to
  Azure, which remains worth exercising regardless. With both RabbitMQ (local
  Windows service) and the Service Bus destination (external, real Azure) available, this job
  needs no docker at all.
  **Scope note:** as described above, this test proves message *arrival* on the Service Bus
  queue (bridge connectivity/plumbing) — it does not execute a Warewolf workflow or exercise
  any engine trigger. `-VerifyWorkflowExecution` (both scripts) extends this same pipeline to
  additionally prove workflow *execution* through the in-process Model A trigger
  (`ServiceBusWorkflowTriggerFunction`) — see
  `docs/ServiceBusSecureTrigger-Architecture.md` § "End-to-end verification" for the harness
  mode, new parameters, and the separate `Enable-ServiceBusSecureTrigger.ps1` provisioning
  prerequisite. That mode is not yet wired into either CI pipeline (it requires the target
  Function App to have the trigger provisioned and an Entra token available, which is a
  manual/reviewed setup step, not something CI can safely automate today).


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

- `docs/Deploy-EndToEnd-Runbook.md` §8.5 — copy-paste deploy/authorize/verify/teardown walkthrough.
- `Scripts/README.md` — script index.
- `Warewolf.Execution.ServiceBusWorker/README.md` — the
  worker's own architecture, auth, and message-contract documentation.
- `docs/KB-ClientApps-Configuration.md` §2.6 — the worker's Entra app registration.
- `docs/KeyRotationRunbook.md` — SAS/key rotation conventions.
