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
- **`state: "flow"` is a healthy, running shovel — not a failure — and all status polling
  in this codebase treats it as equivalent to `state: "running"`.** RabbitMQ reports a
  connected, actively-forwarding shovel as `"flow"` whenever its own internal flow-control
  is currently throttling it (e.g. transient backpressure from the destination); it is a
  normal operational substate, not `terminated`/`starting`. Polling code that only accepts
  `state -eq 'running'` produces an intermittent **false-negative timeout** — the CI job
  fails with "did not reach the 'running' state" even though the last observed state shows
  the shovel connected end-to-end (`"forwarded"`/`"pending"` populated, `src_uri`/`dest_uri`
  both present, no `terminated`/error). `Test-ShovelBridgeE2E.ps1` (Phase 3),
  `Configure-RabbitMqShovel.ps1` (Phase 3), and `Monitor-RabbitMqShovel.ps1` all check
  `state -in @('running', 'flow')` for exactly this reason.
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
  containers only). That job runs on an `ubuntu-latest` pool rather than the
  `windows-2022` pool used everywhere else in this pipeline: all three containers it
  starts (`rabbitmq:3-management`, `azure-sql-edge`, and the Service Bus emulator) are
  Linux-only images, and Microsoft-hosted `windows-2022` agents' Docker daemon only
  supports Windows containers (no Hyper-V/WSL2 Linux-container backend, no in-place
  daemon switch) — it fails to pull any of them ("no matching manifest for
  windows/amd64", "could not find plugin bridge"). Unlike RabbitMQ, neither SQL Edge nor
  the Service Bus emulator has a Windows-native (choco or otherwise) substitute, so
  switching only RabbitMQ to the choco/`-RabbitMqMode External` technique used by
  `ShovelBridgeE2ETest_ExternalServiceBus` (below) would still leave this job broken; a
  Linux pool fixes all three at once with no script changes. All readiness polling is done purely over the RabbitMQ management HTTP API
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
  queue (bridge connectivity/plumbing) — it does not, on its own, execute a Warewolf workflow
  or exercise any engine trigger. `-VerifyWorkflowExecution` (both scripts) extends this same
  pipeline to additionally prove workflow *execution* through the in-process Model A trigger
  (`ServiceBusWorkflowTriggerFunction`) — see
  `docs/ServiceBusSecureTrigger-Architecture.md` § "End-to-end verification" for the harness
  mode, new parameters, and the separate `Enable-ServiceBusSecureTrigger.ps1` provisioning
  prerequisite. This mode IS wired into CI: as a second, additive leg in
  `ShovelBridgeE2ETest_ExternalServiceBus` (`Dev/.azure/pipeline-CLOUD.yml`, a single message)
  and as the sole leg of `ShovelBridgeLoadTest_ExternalServiceBus`
  (`Dev/.azure/pipeline-LOADTEST.yml`, 1000 messages) — both target the same
  already-provisioned UAT engine and its pre-configured `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE`.

  **Load testing:** `Test-ShovelBridgeE2E.ps1`'s `-MessageCount` (default 1) applies to BOTH
  modes:
  - Without `-VerifyWorkflowExecution`: publishes that many uniquely-marked messages and
    waits for all of them to arrive, proving the shovel bridge's own throughput, independent
    of workflow execution.
  - With `-VerifyWorkflowExecution`: publishes that many distinct workflow-trigger messages
    and requires ALL of them to report a Succeeded execution result from the target engine —
    a fully end-to-end load test (bridge throughput AND workflow-execution correctness at
    scale), polled concurrently (bounded parallelism) since the engine's own
    `GET /secure/servicebus-result/{correlationId}` endpoint has no bulk/batch variant.

  **Throughput knobs at load-test volumes:** the dominant cost of a large `-MessageCount` run
  is the harness's own publish loop — by default it publishes one message at a time via a
  blocking RabbitMQ Management API HTTP call, which is far slower than the Shovel's own
  bridging. Two independent knobs, both plumbed through the script:
  - `-PublishConcurrency` (default 1, unchanged sequential behaviour) bounds how many of those
    publishes are in flight at once (`--publish-concurrency` on the harness,
    `PublishManyBoundedAsync` in `Program.cs`) — this is the knob that actually speeds up the
    publish phase, since it's usually the bottleneck, not the Shovel.
  - `-ShovelPrefetchCount` (default 5, the Shovel's original hardcoded value) sets the
    Shovel's own `src-prefetch-count` — how many unacknowledged messages it keeps pipelined
    between the RabbitMQ source queue and the Service Bus destination given `ack-mode:
    on-confirm`. Raising this is a secondary tune that only helps once publishing is no
    longer the limiting factor (i.e. combined with `-PublishConcurrency > 1`).
  `ShovelBridgeLoadTest_ExternalServiceBus` (`Dev/.azure/pipeline-LOADTEST.yml`) sets
  `PublishConcurrency: 20` and `ShovelPrefetchCount: 50`; the single-message
  `ShovelBridgeE2ETest_ExternalServiceBus` job leaves both at their sequential/original
  defaults since concurrency doesn't matter for one message.

  Wired into CI as the `ShovelBridgeLoadTest_ExternalServiceBus` job in
  `Dev/.azure/pipeline-LOADTEST.yml`, which reuses the exact same
  `-RabbitMqMode External -DestinationMode ExternalServiceBus` setup as
  `ShovelBridgeE2ETest_ExternalServiceBus` above (local choco RabbitMQ, the same
  `WarewolfShovelBridgeTesting` namespace) plus its `-VerifyWorkflowExecution` leg's own
  Entra daemon-token acquisition, but with `-MessageCount 1000`. It uses its own dedicated
  RabbitMQ-side queue/shovel names (suffixed `-loadtest`) so it never collides with the
  ordinary single-message E2E test's shovel running concurrently on the same broker/agent
  type, but DELIBERATELY reuses the exact same Service Bus verification queue/SAS rule as
  `ShovelBridgeE2ETest_ExternalServiceBus`'s own `-VerifyWorkflowExecution` leg — that queue
  name must match the target engine's single, fixed `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE`
  app setting, so it cannot be uniquely suffixed per job. Running both jobs concurrently is
  safe: each only ever sends messages (via its own distinct shovel), the target engine's
  single Managed Identity subscription is the only consumer of either job's messages, and
  correlationIds keep each job's own results distinct.

- **`-VerifyWorkflowExecution` failure signature: `usp_jobs1_LogStart` "no text for object"
  (SQL error 15197) cascading into `[[JobLogId]]` null-variable errors under load.**
  `RabbitProcess.bite` (the workflow both `-VerifyWorkflowExecution` legs execute) calls
  three stored procedures in sequence — `dbo.usp_jobs1_LogStart`, `usp_jobs1_LogProcessing`,
  `usp_jobs1_LogFinished`. All three of its DB activities bind to
  `SourceId="b9184f70-64ea-4dc5-b23b-02fcd5f91082"` — the shared `NewSqlServerSource` that
  `pipeline-CLOUD.yml` downloads from the devops endpoint, **not** the
  `NewSqlServerSource (Local Backup)` source (`d3f6a2e1-…`) sitting alongside it in
  `Resources/rabbit/`. That bundled source is unreferenced by any activity, and its
  connection string is DPAPI-encrypted under its author's Windows account, so it cannot be
  read or used on another machine. The procedures therefore run against whatever
  `b9184f70-…` resolves to on the target engine (e.g. UAT's
  `warewolf-dev2-mcgeaj.database.windows.net` / `WarewolfEntraTestDb`, a **serverless**
  `GP_S_Gen5` tier database on Azure SQL's free-limits offer, `autoPauseDelay: 60` minutes,
  which cannot be disabled while free-limit-enrolled). Every SQL Server DB activity
  execution unconditionally runs `sp_helptext` against the target procedure first
  (`DatabaseServiceExecution.MssqlIsStoredProcForXmlResult` / `MssqlGetSqlForProcedure`,
  `Dev/Dev2.Services.Execution/DatabaseServiceExecution.cs`) to detect a `FOR XML` result
  shape. During the 2026-08-13 1000-message load test run, this failed for effectively all
  1000 executions with SQL Server error 15197 ("There is no text for object
  'dbo.usp_jobs1_LogStart'.") raised by `sp_helptext` itself.

  **Root cause: the connecting principal holds `EXECUTE` but not `VIEW DEFINITION`.** An
  earlier revision of this document attributed the failure to a serverless auto-resume race
  and called it transient. That was a misdiagnosis, disproved by direct inspection of
  `WarewolfEntraTestDb` on 2026-08-13:

  | Check | Result |
  |---|---|
  | `OBJECTPROPERTY(...,'IsEncrypted')` on all 8 `usp_jobs1_*` | `0` — not encrypted |
  | `sys.sql_modules` row visible / `definition` | row present / `NULL` |
  | `HAS_PERMS_BY_NAME('dbo.usp_jobs1_LogStart','OBJECT','VIEW DEFINITION')` | `0` |
  | `HAS_PERMS_BY_NAME('dbo.usp_jobs1_LogStart','OBJECT','EXECUTE')` | `1` |
  | Roles held by `devops_warewolf` | `db_datareader`, `db_datawriter` only |
  | `dbo.jobs1` row count | `0` — no execution has ever succeeded |

  `sp_helptext` was reproduced failing with 15197 against a **warm, idle, single-connection**
  database, which rules out load, concurrency and auto-resume. `definition IS NULL` combined
  with `IsEncrypted = 0` and a visible catalog row has exactly one cause: the caller lacks
  `VIEW DEFINITION`. This is deterministic and per-principal — retrying can never clear it.

  Note also that **`WarewolfServer-UAT` does not exist as a database user** in
  `WarewolfEntraTestDb` (`sys.database_principals` contains only `devops_warewolf`), so the
  engine never authenticates as its Managed Identity here — it falls through to the SQL-auth
  fallback path in `MssqlSqlExecution`. Any future claim that a grant was applied "to the
  UAT managed identity" on this database should be checked against that.

  The previous 15197-as-transient "fix" actively made the load test worse: each of the 1000
  executions burned 3 attempts with 5s + 10s backoff while **holding its pooled connection**,
  against the `MaxPoolSize = 100` set in `Dev/Dev2.Services.Sql/SqlConnectionWrapper.cs`.
  That is the origin of the run's other two error classes — "Timeout expired ... max pool
  size was reached" and "[Post-Login] complete=29162". **Now fixed** by (a) removing 15197
  from `AzureSqlTransientErrorRetry`'s transient set, (b) removing the retry wrapper around
  the metadata lookup, and (c) making `MssqlIsStoredProcForXmlResult` degrade gracefully —
  an unreadable definition now falls back to the standard non-`FOR XML` read path with a
  warning instead of failing the execution, since `EXECUTE` is what actually matters and
  `VIEW DEFINITION` is a separate permission. To restore genuine `FOR XML` *detection* on
  this database (only needed if a procedure really does return `FOR XML`), run:
  `GRANT VIEW DEFINITION ON SCHEMA::dbo TO devops_warewolf;`

  Because the recordset output the first step would have produced
  (`[[dbo_usp_jobs1_LogStart().JobLogId]]`) is never populated when it fails, the *next*
  step (`usp_jobs1_LogProcessing`, which requires `[[JobLogId]]` as an input) fails
  separately with `NullValueInVariableException` ("Error with variables in input.
  `[[JobLogId]]`") — so most executions in a load test surface only that generic cascading
  error rather than the original SQL exception; check Application Insights / at least one
  failed correlationId's raw `Error` text for the underlying SQL error number to find the
  true cause of a similar failure. Confirm the RabbitMQ→Service Bus legs (Phases 1-3)
  succeeded first (they are independent of this SQL dependency) before investigating the
  DB source.

- **`usp_jobs1_LogStart` never wrote `MessageContentHash`, capping `dbo.jobs1` at one row.**
  The procedure computes `@hash = HASHBYTES('SHA2_256', @MessageContent)`, takes its applock
  on it and queries by it — but omitted `MessageContentHash` from its own `INSERT` column
  list, so the column stayed `NULL` on every row. Two consequences compounded: the attempt
  lookup `WHERE MessageContentHash = @hash` never matched (so `AttemptNumber` was always
  `1`), and `UQ_jobs1_ContentHash_AttemptNumber` — UNIQUE, **unfiltered**, on
  `(MessageContentHash, AttemptNumber)` — treats `NULL`s as equal. The second row ever
  inserted therefore failed with error 2601, "duplicate key ... `(<NULL>, 1)`", regardless of
  message content. Reproduced directly, and consistent with `dbo.jobs1` having zero rows.
  **Fixed** on `WarewolfEntraTestDb` (2026-08-13) by adding `MessageContentHash` / `@hash` to
  that `INSERT`; verified that distinct contents now coexist and that a repeated content
  correctly yields `AttemptNumber = 2`.

- **All messages in a load run shared one message body, serialising the whole run.**
  `pipeline-LOADTEST.yml` passed a literal `VerifyWorkflowInputsJson: '{"message":"loadtest"}'`,
  and the harness applied that same map to every message — only `correlationId` varied, and
  `correlationId` never reaches the workflow's `[[message]]` input. All 1000 executions
  therefore hashed to the same value and queued behind a **single exclusive `sp_getapplock`**
  (15s timeout) in `usp_jobs1_LogStart`, and were semantically recorded as 1000 retry
  attempts of one job. **Fixed** by adding a `{correlationId}` placeholder to
  `--workflow-inputs-json` (`WorkflowInputTemplate` in the E2E harness, unit-tested in
  `Warewolf.Execution.ServiceBusWorker.Tests`) and setting the pipeline to
  `'{"message":"loadtest-{correlationId}"}'`. The harness now prints a warning if a
  multi-message run omits the placeholder. **Use the placeholder for any `-MessageCount > 1`
  run.**

- **`RabbitProcess2.bite`'s schema — now provisioned.** It calls `dbo.usp_jobs2_LogStart` /
  `_LogProcessing` / `_LogFinished`, and neither the `jobs2` table nor any `usp_jobs2_*`
  procedure previously existed on `WarewolfEntraTestDb` — only the `jobs1` set did, so that
  workflow failed 100% of the time. **Provisioned on 2026-08-13** as an exact clone of
  `jobs1`: the table (same columns, `PK_jobs2`, `UQ_jobs2_ContentHash_AttemptNumber`,
  `IX_jobs2_ContentHash`, `IX_jobs2_Status_CreatedAtUtc`) plus all 8 procedures, cloned
  **after** the `MessageContentHash` fix above so the defect was not copied forward.
  `EXECUTE` is granted at `dbo` schema level, so `devops_warewolf` picked the new procedures
  up automatically. Note this schema exists only as live database state — there is no
  provisioning script for it in the repo, which is why its absence went unnoticed; adding one
  is worthwhile follow-up work.

- **2026-08-13 16:33 UTC 1000-message `-VerifyWorkflowExecution` run: only 89/1000 succeeded,
  but the DB-side fixes above are confirmed working — the bottleneck has moved upstream of SQL.**
  Investigated live against `WarewolfEntraTestDb` using an owner-level `az login` session
  (`sqlcmd ... --authentication-method=ActiveDirectoryAzCli`), which is the more reliable way to
  inspect this database going forward since it doesn't depend on `devops_warewolf`'s grants:
  - `WarewolfServer-UAT` **does now exist** as an Entra database principal (`EXTERNAL_USER`) with
    `EXECUTE` and `VIEW DEFINITION` granted database-wide — the "does not exist as a database
    user" finding earlier in this doc is now stale; something provisioned it since.
  - `usp_jobs1_LogStart` / `usp_jobs1_LogProcessing` definitions match the fixes described above
    (hash-based `INSERT`, graceful FOR XML fallback) — confirmed by reading `OBJECT_DEFINITION`
    directly.
  - `dbo.jobs1` after the run: **106 rows total** (`JobLogId` 8–113, no gaps), 99 `FINISHED`,
    3 `PROCESSING`, 4 `STARTED`, only 3 duplicate `MessageContentHash` pairs (i.e. the
    `{correlationId}` templating fix is working — hashes are essentially unique per message).
  - **Only 106 of the 1000 published messages ever reached `usp_jobs1_LogStart`'s `INSERT` at
    all.** Since a gapless identity range means no attempt got as far as the `INSERT` and then
    rolled back (an `sp_getapplock` timeout throws *before* the `INSERT`, consuming no identity
    value), this rules out DB-side lock contention as the dominant cause — with per-message
    unique hashes there is no reason for `sp_getapplock` contention across different messages
    anyway. The other ~894 executions never made it to a SQL call at all.
  - Conclusion: **DB permissions and stored-procedure logic are no longer the bottleneck** for
    the majority of failures — see the corrected/superseding finding immediately below, which
    identifies the actual dominant cause via Application Insights telemetry (the App Insights
    lookup above initially appeared empty only because `az monitor app-insights query`'s
    `--analytics-query` silently mishandles multi-line PowerShell here-strings; single-line
    queries against `warewolfserver-uat-ai` in resource group `DEV2` work fine and the resource
    has telemetry going back well beyond this run).

- **ROOT CAUSE (2026-08-13, confirmed via `warewolfserver-uat-ai` Application Insights):
  the Lightweight engine's `DynamicActivity`/`IDev2Activity` object graph is a process-wide
  singleton per workflow file, and several Dev2 activity base classes hold per-execution state
  in plain mutable instance fields — so concurrent executions of the same workflow race on the
  same objects.** Of ~20,080 exceptions logged on 2026-08-13, **17,620 (88%) are exactly
  `"Object reference not set to an instance of an object."` in `ActivityName = "DsfNativeActivity
  {serviceName} Assign (1)"` on the `ServiceBusWorkflowTrigger` function** — dwarfing the SQL
  pool-timeout (220) and 51001 (22) counts documented above. This, not DB/engine "capacity", is
  the actual dominant cause of the 89/1000 load-test result. Root cause chain, confirmed by
  direct code inspection:
  1. `Warewolf.Execution.Lightweight/Execution/WorkflowExecutor.cs` — `_dynamicActivityCache` is
     a `static ConcurrentDictionary<string, DynamicActivity>`: the compiled XAML object graph for
     a given workflow file is a **process-wide singleton**, intentionally cached because
     `ActivityXamlServices.Load` is expensive. The accompanying comment asserts this is safe
     because a fresh `IDev2Activity` chain is parsed "each time" — true only in the sense of a
     fresh list of *references*.
  2. `Dev/Dev2.Activities/Activities/ActivityParser.cs` (`Parse` →
     `WorkflowInspectionServices.GetActivities(dynamicActivity)`, and
     `ParseToLinkedFlatList`'s handling of `DsfForEachActivity.DataFunc.Handler`) only *walks*
     the existing object graph — it never clones activity node objects. Every concurrent
     execution of the same workflow therefore reuses the **exact same `DsfForEachActivity` and
     `DsfDotNetMultiAssignActivity` ("Assign (1)") instances**.
  3. `Dev/Dev2.Activities/Activities/DsfNativeActivity.cs` (the base class behind
     `DsfDotNetMultiAssignActivity` and effectively every built-in Dev2 activity) declares
     `protected List<DebugItem> _debugInputs`/`_debugOutputs` as **plain mutable instance
     fields** (not WF `Variable<T>`/context-scoped state), and
     `DsfDotNetMultiAssignActivity.ExecuteTool` (`Dev/Dev2.Activities/Activities/
     DsfDotNetMultiAssignActivity.cs:88-89`) calls `_debugOutputs.Clear()` / `_debugInputs.Clear()`
     followed by `.Add(...)` on every execution, with **no synchronization**.
  4. Two concurrent executions of the same workflow racing `List<T>.Clear()`/`.Add()` on the
     same shared list is a textbook data race that manifests as an intermittent, generic
     `NullReferenceException` — exactly the observed symptom. It concentrates on "Assign (1)"
     here because that step sits inside a 10-iteration `ForEach` (`RabbitProcess.bite`'s "For
     Each" 1..10 wrapping "Assign (1)"), multiplying re-entrancy per execution, but the hazard is
     generic to **any** activity derived from `DsfNativeActivity<T>` run concurrently on this
     engine — not specific to RabbitProcess/ShovelBridge.
  - **This is a correctness bug in shared `Dev2.Activities` code (used by both the Lightweight
    and Server engines), not a ShovelBridge-specific or DB-specific issue**, and not something
    pipeline concurrency knobs (`ShovelPrefetchCount`/`PublishConcurrency`) can work around except
    by accident (lower concurrency = narrower race window = fewer failures, not zero).

- **FIXED (2026-08-13) — ported from `8504-Execution-Engine-Queue-Processor-End-to-end-testing`.**
  That branch independently found and fixed this exact defect (own comment/tests cite the same
  `RabbitProcess`/`[[JobLogId]]` symptoms and near-identical concurrency measurements, 2026-08-11/12)
  before this investigation reached the fix stage. Rather than merging the whole 8504 branch —
  which deletes this ShovelBridge harness (`Test-ShovelBridgeE2E.ps1`, this doc,
  `RabbitProcess.bite`, `Configure-RabbitMqShovel.ps1`) in favour of an unrelated QueueProcessor
  E2E rewrite, and bundles an orthogonal usage-telemetry refactor into the same file — only the
  pooling fix itself was ported into `Warewolf.Execution.Lightweight/Execution/WorkflowExecutor.cs`
  (plus its two callers, `ResumptionExecutor.cs` and `LightweightEsbChannel.cs`), leaving this
  branch's own recent work (e.g. `PatchNestedAuthorizationServices`) untouched.
  - **Fix shape**: the shared `_dynamicActivityCache` (one `DynamicActivity` singleton per workflow
    path, handed to every concurrent execution) is replaced by a **rent/return pool**
    (`_workflowPool: ConcurrentDictionary<string, ConcurrentBag<PreparedWorkflow>>`). Each execution
    calls `WorkflowExecutor.RentPreparedWorkflow(path, xaml)` to take **exclusive ownership** of a
    `PreparedWorkflow` (the compiled `DynamicActivity` + its parsed `IDev2Activity` chain, kept
    together since parsing doesn't clone — the chain *is* the same object graph), executes against
    it, and calls `ReturnPreparedWorkflow(path, prepared)` in a `finally` so it's released even on
    exception paths. No two concurrent executions can ever hold the same instance, so the shared
    mutable-field race (`_debugInputs`/`_debugOutputs`, `ServiceExecution`) cannot occur.
  - **Reuse preserved**: a returned instance is reused by the next rent for the same path, so
    sequential executions (the common case) still avoid re-compiling XAML — only concurrent
    executions of the *same* workflow each get their own instance.
  - **Bounded pool**: retained (idle) instances per path are capped at `MaxPooledPerWorkflow`
    (default 8, override via `WAREWOLF_WORKFLOW_POOL_MAX`) — renting is never throttled (so
    exclusivity holds under any concurrency), but instances beyond the cap are simply not kept on
    return, preventing unbounded memory growth on an Azure Functions Consumption instance.
  - **Deliberately NOT fixed**: the underlying defect — plain mutable instance fields on shared
    `Dev2.Activities` objects (`DsfNativeActivity._debugInputs`/`_debugOutputs`,
    `DatabaseServiceExecution` assignment, etc.) — remains latent in `Dev2.Activities`. This fix
    only guarantees the Lightweight engine never lets two executions touch the same instance; it
    does **not** make those objects thread-safe. `Dev2.Server` (on-prem) and Studio are unaffected
    by this change and remain exposed to the same class of bug if they ever execute the same
    workflow concurrently within one process.
  - **Tests**: ported `WorkflowPoolConcurrencyTests.cs` (11 tests) from 8504 into
    `Warewolf.Execution.Lightweight.Tests/Execution/` — asserts on object identity (no SQL Server,
    engine, or network needed): rent-without-return yields distinct `DynamicActivity`/
    `IDev2Activity` chain instances, including under real concurrency (8 callers via a `Barrier`);
    rent-after-return reuses the same instance; pool grows only to peak concurrency and never
    beyond `MaxPooledPerWorkflow`; renting still succeeds (and stays exclusive) beyond the cap;
    return is null-tolerant; path normalisation (casing) shares one pool. All 743 existing
    `Warewolf.Execution.Lightweight.Tests` and the 8
    `WorkflowExecutorEndToEndTests`/`Warewolf.Execution.Lightweight.Integration.Tests` pass
    unchanged after the port.
  - **Not yet re-validated against the live pipeline**: this fix has not yet been re-run through
    `pipeline-LOADTEST.yml`'s ShovelBridge job against `warewolf-dev2-mcgeaj`/`WarewolfServer-UAT`
    to confirm the 89/1000 result recovers to ~1000/1000. That re-run is the next concrete step
    before considering this issue closed end-to-end.

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
  the `ShovelBridgeE2ETest_ExternalServiceBus` job, and a fully end-to-end 1000-message
  workflow-execution load-test variant is wired into `pipeline-LOADTEST.yml`'s `Load_Test`
  stage as the `ShovelBridgeLoadTest_ExternalServiceBus` job.


## See also

- `docs/Deploy-EndToEnd-Runbook.md` §8.5 — copy-paste deploy/authorize/verify/teardown walkthrough.
- `Scripts/README.md` — script index.
- `Warewolf.Execution.ServiceBusWorker/README.md` — the
  worker's own architecture, auth, and message-contract documentation.
- `docs/KB-ClientApps-Configuration.md` §2.6 — the worker's Entra app registration.
- `docs/KeyRotationRunbook.md` — SAS/key rotation conventions.
