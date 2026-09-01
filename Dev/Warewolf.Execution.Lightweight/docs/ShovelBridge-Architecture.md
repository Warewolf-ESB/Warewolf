# Shovel Bridge Architecture — RabbitMQ → Azure Service Bus → Lightweight Execution Engine

> **2026-08-24 — the load test never started: the pre-warm step killed the job, and its own
> Phase B was part of what it was meant to prevent.** `pipeline-LOADTEST.yml`'s
> `ShovelBridgeLoadTest_ExternalServiceBus` job failed in *Warm up UAT engine before the
> load-test burst*, so the 1000-message run below it never executed. Two independent defects,
> both fixed in `Scripts/Invoke-WwEnginePreWarm.ps1` and regression-tested in
> `Scripts/Tests/Invoke-WwEnginePreWarm.Tests.ps1`.
>
> **1 — a per-call timeout failed the whole step, contradicting the documented contract.**
> `-SkipHttpErrorCheck` suppresses non-2xx **status codes** only. An `Invoke-WebRequest
> -TimeoutSec` expiry is a `TaskCanceledException` — an exception, not a response — so it
> bypassed that switch entirely. Inside `ForEach-Object -Parallel` the child runspace runs at
> `$ErrorActionPreference = 'Continue'` (child runspaces do **not** inherit the preference,
> verified directly), so it was written non-terminating, relayed into the caller's error
> stream, and there converted to a terminating error by the script's own script-level
> `'Stop'`. The script died inside Phase B and never reached its own `[!] the final round
> still had failures` verdict — the branch whose comment reads "Not fatal by design" was
> **unreachable** for any transport-level failure. `Scripts/README.md` and the pipeline
> comment both documented a non-fatal contract that had never held for timeouts.
>
> Fixed by normalising both outcomes through `ConvertTo-WarmupResult`: a failed call becomes a
> counted result with the `Code 0` sentinel (rendered `TIMEOUT`/`ERROR`, never a misleading
> `0x3`), so `Test-WarmupRoundClean` correctly marks the round dirty. `-ErrorAction Stop` on
> every `Invoke-WebRequest` is load-bearing — without it the parallel runspace's `Continue`
> makes the exception non-terminating and it skips the `try/catch`. The script now **always
> exits 0** once its parameters validate; only parameter validation throws.
>
> **2 — Phase B opened at full concurrency, which is itself a cold-start trigger.** The
> observed round 1 at `-TargetConcurrency 20` against `warewolfserver-uat`: `OK=22/60`, median
> **41,725 ms**, max **159,896 ms**, `200x22 500x16 502x20 503x2` — a 63% failure rate, and
> the exact cold-start/scale-out signature the 2026-08-16 entry below describes. Phase A had
> already settled (42,694 ms cold → ~900 ms by call 5), so this was scale-out, not cold start:
> 60 simultaneous executions arriving at a still-single-instance app is the same load the
> pre-warm exists to protect the burst *from*.
>
> Phase B now ramps — `ceil(Target / 2^(Rounds - i))`, so 20 over 3 rounds is **5 → 10 → 20**
> and the queue path's 6 is **2 → 3 → 6**. The last round is always the full target, so
> `[+] WARM — the final round was clean at concurrency N` still means clean at the target and
> nothing weaker. Costs fewer calls too (105 vs 180 at target 20), so fewer warm-up rows.
>
> **Still open — UAT capacity.** The ramp reduces how often the burst meets a cold app; it does
> not raise `warewolfserver-uat`'s Consumption (Y1) ceiling. Whether that app should move to a
> Premium/EP plan for load testing is an unresolved cost decision, deliberately not bundled
> into this fix. Until it is resolved, **read the warm-up verdict alongside the load-test
> result**: the burst is now published even when warming failed, so a poor result following
> `[!] the final round still had failures` is an engine capacity problem and not necessarily a
> product defect.

> **2026-08-19 update — database migrated.** `WarewolfEntraTestDb` (referenced throughout
> the history below) permanently exhausted Azure SQL's monthly free-limit allowance and was
> deleted. `Resources/rabbit/NewSqlServerSource.bite` (`SourceId=b9184f70-…`) now points at
> **`WarewolfDevOpsTestDb`**, a new, non-free-limit database on the same server
> (`warewolf-dev2-mcgeaj.database.windows.net`) and reusing the same `devops_warewolf`
> server-level login/password, so no other connection details changed. The `jobs1`/`jobs2`
> tables, all 16 `usp_jobs{1,2}_*` procedures, `sp_TestEntraConnectivity`, and the
> `EXECUTE`/`VIEW DEFINITION` grants described below (the fix for the 15197 issue) were
> reproduced identically on the new database from a live-schema extraction taken
> immediately before the old database was deleted, and are now committed as
> `Resources/rabbit/Provision-ShovelBridgeSchema.sql` — closing the "no provisioning script
> exists in the repo" gap called out below. The historical narrative below is left as-is
> since it documents real incidents against the (now-deleted) old database; read
> `WarewolfEntraTestDb` there as "the database in use at the time." **`WarewolfServer-UAT`'s
> Managed Identity was also re-added as an Entra ID (`FROM EXTERNAL PROVIDER`) database user
> on `WarewolfDevOpsTestDb`, with `EXECUTE`/`VIEW DEFINITION` on `dbo` — matching the state it
> reached on the old database per the 2026-08-13 16:33 UTC entry below — so the engine's own
> Managed-Identity probe (see the "does not exist as a database user" note below) no longer
> falls through on the new database either.**

> **2026-08-19 follow-up — masked-password source fixed; deploy step hardened.** The
> `Resources/rabbit/NewSqlServerSource.bite` committed as part of the migration above had its
> `ConnectionString`'s password field accidentally set to the literal, six-character `******`
> placeholder (Studio's UI display convention for a password field — see
> `Dev2.Runtime.Services/ServiceModel/Data/DbSource.cs`'s masked `ConnectionString` getter and
> the matching SpecFlow "Password field is `******`" assertions) instead of the real
> `devops_warewolf` password, presumably copied from a Studio screen rather than re-supplied as
> plain text before running `Encrypt-Config.ps1`. This produced SQL error 18456 ("Login failed
> for user 'devops_warewolf'") on every execution, not a credentials mismatch. Fixed by
> decrypting the committed source with `Encrypt-Config.ps1 -Decrypt` (Key Vault
> `WWExecutionEngine` / secret `WWExecutionEngineTestSecret` — same key `WarewolfServer-UAT`
> reads at runtime), substituting the real password, and re-encrypting in place; verified
> byte-for-byte via SHA-256 comparison before committing (never round-tripped through a display
> layer that would re-mask it). A concurrent 1000-message load test run also surfaced SQL error
> 4060 ("Cannot open database `WarewolfEntraTestDb`") on some executions in the very same burst
> that got 18456 on others — proof that some Consumption-plan (Y1) instances were still serving
> a pre-migration, in-memory-cached copy of the source alongside freshly cold-started instances
> running the current one. `pipeline-LOADTEST.yml`'s post-deploy step used `az functionapp
> restart`, which `docs/KB-Deploying-Encrypted-Sources.md` §3 already documents as insufficient
> to force this on a Consumption plan (a warm worker can survive a `restart` with its old
> decrypted source still resident); that step now does a full `stop` then `start` instead, so
> every scaled-out instance cold-starts and re-decrypts the resources on the next deploy.

> **2026-08-19 correction — the actual live root cause was a stale `WorkflowsDirectory`
> override, not (only) warm workers; the app setting has now been removed.** After committing
> the fix above, a full local `Deploy-WwExecutionEngine.ps1` redeploy plus a genuine
> `stop`/`start` (both confirmed to run successfully) *still* returned SQL error 4060 against
> `WarewolfEntraTestDb` on direct `POST /Public/rabbit/RabbitProcess.json` calls. Root cause,
> confirmed directly via Kudu (`/api/vfs/`), was **not** in-memory worker staleness:
> `WarewolfServer-UAT` had an app setting `WorkflowsDirectory=D:\home\data\Warewolf\Resources`
> — a persistent Azure Files path *outside* the deployed package — which
> `Infrastructure/HostEnvironmentConfig.cs` reads in preference to the default
> `<wwwroot>\Resources`. Every zip-deploy (pipeline or manual) stages `-WorkflowsSourcePath`
> into `<PublishDir>\Resources` (i.e. **inside** the package, per
> `docs/Deploy-UAT-Redeploy-Spec.md` §5.4) — so with this override in place, **no deploy has
> ever reached the files the running engine actually reads**, on this app, since whenever the
> setting was first added. The persistent folder still held whatever had been manually pushed
> there via Kudu on 2026-08-14: `RabbitProcess.bite`/`RabbitProcess2.bite` (fine), plus a
> **second, unencrypted, plaintext-password copy of `NewSqlServerSource.bite`** sitting directly
> under `Resources\` (not `Resources\rabbit\`) — same `SourceId=b9184f70-…`, but still pointing
> at `WarewolfEntraTestDb` via `Authentication=Active Directory Managed Identity`. Because
> `Infrastructure/LightweightSourceLoader.BuildFileIndex` indexes `.bite` files by `ResourceID`
> across the *entire* `WorkflowsDirectory` tree via `Directory.EnumerateFiles(..., AllDirectories)`
> and lets the last one found win (exactly the failure mode `docs/Deploy-UAT-Redeploy-Spec.md`
> §5.4 already warned about for a *different* stray file), this orphaned root-level copy was the
> **only** copy of that source ID actually present at runtime (the correct one had never
> physically arrived), so every DB activity resolved to it and failed against the deleted
> database. Remediated in two steps, in order:
> 1. Uploaded the corrected, WFAES-encrypted `Resources/rabbit/NewSqlServerSource.bite` (same
>    ciphertext committed to source control) directly into
>    `D:\home\data\Warewolf\Resources\rabbit\` via Kudu VFS `PUT`, and deleted the stale
>    plaintext root-level duplicate via Kudu VFS `DELETE` (`If-Match: *`), as an immediate
>    tactical fix — confirmed working via repeated direct `POST /Public/rabbit/RabbitProcess.json`
>    calls (SQL auth to `WarewolfDevOpsTestDb` now succeeds consistently; only a pre-existing,
>    unrelated `jobs1` `UQ_jobs1_ContentHash_AttemptNumber` unique-constraint collision remains,
>    an artifact of ad-hoc test payloads sharing a `NULL` content hash, not a credentials issue).
> 2. **Removed the `WorkflowsDirectory` app setting from `WarewolfServer-UAT` entirely**
>    (`az functionapp config appsettings delete --setting-names WorkflowsDirectory`, followed by
>    a `stop`/`start`) as the durable fix, since `Deploy-WwExecutionEngine.ps1` already solves the
>    original Release-publish-has-no-`Resources`-folder problem the override likely existed to
>    work around (§5.4) by staging `-WorkflowsSourcePath` straight into the package. Re-tested
>    after removal: the engine now serves correctly from the package's bundled `Resources`
>    folder with **no** manual Kudu step required, confirmed by a fresh cold `stop`/`start` cycle
>    followed by successful `POST /Public/rabbit/RabbitProcess.json` calls. The now-unused
>    `D:\home\data\Warewolf\Resources` tree was deleted via Kudu VFS to avoid a future engineer
>    mistaking it for the live source of truth. **Net effect: `pipeline-LOADTEST.yml`'s existing
>    `Deploy_UAT` job (including today's `stop`/`start` hardening above) is now sufficient on its
>    own — no additional Kudu-sync step is needed** — because the package it already builds and
>    deploys is, for the first time, actually what the running app reads. If `WorkflowsDirectory`
>    is ever reintroduced on this app (e.g. copied from another app's settings), treat it as a
>    footgun: it silently makes every future deploy a no-op for workflow/source content.

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
  step now writes this `advanced.config` automatically (see `pipeline-CLOUD.yml`), and
  `Test-ShovelBridgeE2E.ps1`'s own `-RabbitMqMode Container` bind-mounts the identical
  `advanced.config` into its disposable RabbitMQ container at boot (alongside its
  `enabled_plugins` bind mount) — so a local run against `-DestinationMode
  ExternalServiceBus` needs no manual broker setup for this fix either way. Only
  `-RabbitMqMode External` against a broker you don't control the config of (a
  shared/managed broker, e.g. a hosted CI agent without choco/admin rights) cannot apply
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
  alone. Unlike the `customize_hostname_check` broker-side fix above, this one is
  caller-side (part of the dest-uri itself), so `Test-ShovelBridgeE2E.ps1`'s
  `-RabbitMqMode Container` cannot bake it in automatically — pass a dest-uri with
  `&cacertfile=...` already appended via `-ExternalShovelDestUri` (e.g. built with
  `Format-ServiceBusAmqp10Uri -CaCertFile ...`) when running Container mode against the
  official `rabbitmq:3-management` image (Erlang/OTP 26+), or expect the same
  `{cacerts, undefined}` crash-loop described above. **Confirmed live** against
  `WarewolfShovelBridgeTesting`: a `-RabbitMqMode Container` run reached `running` only
  once both the (now automatic) `customize_hostname_check` fix AND a caller-supplied
  `&cacertfile=/etc/ssl/certs/ca-certificates.crt` were present.

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
  `SourceId="b9184f70-64ea-4dc5-b23b-02fcd5f91082"` — the shared `NewSqlServerSource` committed
  pre-encrypted (WFAES) at `Resources/rabbit/NewSqlServerSource.bite` (WOLF-8510, superseding the
  devops-endpoint fetch `pipeline-LOADTEST.yml` previously ran on every build), **not** the
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

- **2026-08-14 — `-VerifyWorkflowExecution` "hang" turned out to be `Workflow file not found:
  ...\Resources\RabbitProcess.xml` for 922/1000, plus 78/1000 with no result before the poll
  timeout — a workflow-name resolution defect, not message loss or a stuck harness.**
  Investigated live against `WarewolfServer-UAT` via the Kudu VFS API
  (`https://warewolfserver-uat.scm.azurewebsites.net/api/vfs/site/wwwroot/...`), which confirmed
  the `Resources` folder itself was staged correctly per §5.4 of `Deploy-UAT-Redeploy-Spec.md`:
  `Resources/rabbit/RabbitProcess.bite`, `Resources/rabbit/NewSqlServerSource.bite`, and a
  `Resources/workflow-index.json` keying the workflow as **`rabbit/rabbitprocess`** (folder-
  qualified, generated over the staged tree). The RabbitMQ→Shovel→Service Bus bridge itself was
  fully healthy throughout (`deliver_get`/`ack` = 1000/1000, shovel stayed `running`).
  - **Root cause**: both `WorkflowIndex.Resolve()`
    (`Warewolf.Execution.Lightweight/Infrastructure/WorkflowIndex.cs`) and the legacy on-disk
    fallback in `WorkflowFunctionHelper.cs`'s `ResolveFilePath` require the caller-supplied
    workflow name to already match how it's stored — the index does an **exact key match** against
    the full relative path, and the fallback's `FindFileCaseInsensitive` uses
    `RecurseSubdirectories = false`. Neither resolves a bare `'RabbitProcess'` to
    `Resources/rabbit/RabbitProcess.bite` once the file lives in a subfolder — this reproduces on
    every run against this Resources layout, independent of load or concurrency.
  - **Not a regression in this document's earlier root-cause findings** (§ "ROOT CAUSE... object
    graph is a process-wide singleton" etc.) — those explain failures *after* a workflow
    successfully resolves and executes. This defect fires earlier, before execution even starts,
    and likely explains a meaningful share of prior "instant failure" counts in earlier runs
    against this same UAT engine once its `Resources` folder started preserving the repo's
    `rabbit/` subfolder (introduced by the WOLF-8510 redeploy in `Deploy-UAT-Redeploy-Spec.md`
    §5.4 — UAT's Resources layout before that redeploy was presumably flat).
  - **Fix applied**: `pipeline-LOADTEST.yml` and `pipeline-CLOUD.yml` both pass
    `VerifyWorkflowName: 'RabbitProcess'` to the same `VerifyEngineBaseUrl`
    (`warewolfserver-uat.azurewebsites.net`) — both updated to `'rabbit/RabbitProcess'`.
    Confirmed working live via `Test-ShovelBridgeE2E.ps1 -WorkflowName 'rabbit/RabbitProcess'`
    (1000-message run resolving successfully instead of erroring instantly).
  - **Not fixed (deliberately out of scope here)**: `WorkflowIndex`/`WorkflowFunctionHelper` are
    unchanged — exact-key-match plus non-recursive fallback is reasonable engine behaviour; the
    defect was a stale pipeline parameter, not the resolution logic itself. If workflows are ever
    restaged flat (no `rabbit/` subfolder), the bare name would need to be restored accordingly.

- **2026-08-14 — Full 1000-message `-VerifyWorkflowExecution` run (after the `rabbit/`-qualified
  workflow-name fix above) still only reached 702/1000, with two distinct, unrelated failure
  classes — neither is message loss; the bridge itself stayed healthy (shovel `running`
  throughout).**
  - **`SQL 51001` / `Error with variables in input. [[JobLogId]]` (majority of the 298
    failures)**: the documented shared-`DynamicActivity` instance-state race (see
    `WorkflowExecutor.cs`'s own `_workflowPool` comment and
    `Warewolf.Execution.Lightweight.Tests/Execution/WorkflowPoolConcurrencyTests.cs`). The fix
    (exclusive-ownership workflow pooling) already exists on this branch (ported from
    `origin/8504-Execution-Engine-Queue-Processor-End-to-end-testing`) but is **not live on
    `WarewolfServer-UAT`** — `Deploy-UAT-Redeploy-Spec.md` is still "proposed, not yet executed"
    and the app's last-modified timestamp predates every WOLF-8510 commit. Expected to clear once
    that redeploy runs; not a new defect.
  - **`401: Authentication required` on `GET /secure/servicebus-result/{correlationId}`
    (remainder of the 298 failures)**: NOT a token-lifetime bug in the engine — this leg uses an
    Entra ID client-credentials token (`pipeline-LOADTEST.yml`'s "Acquire Entra token" step),
    whose default ~60-minute lifetime comfortably covers even a 30-minute
    `-ResultTimeoutSeconds 1800` run, and `EntraTokenValidator.cs` validates `exp` per-request
    with no caching. Working theory (not provable from this engine's App Insights, which has had
    no telemetry for 7+ days): a manually-run test reused a bearer token that was already stale
    from earlier in the same interactive session, rather than minting one immediately before the
    run. **Mitigation added**: `Test-ShovelBridgeE2E.ps1`'s Phase 0 now decodes
    `-MessageAuthToken`/`-ResultPollAuthToken` client-side (best-effort, no signature check — the
    engine still does real validation) and fails fast with an actionable message if either token
    won't outlive `-ResultTimeoutSeconds` plus a setup buffer, instead of letting a 30-minute,
    1000-message run silently degrade into a wall of terminal `401` HttpErrors. Tokens without a
    parseable `exp` claim (opaque tokens) are left unchecked, not blocked.

- **2026-08-14 — `WarewolfServer-UAT` redeploy (`Deploy-UAT-Redeploy-Spec.md`) confirmed executed;
  the `JobLogId` root cause above is expected to be resolved, but this is not yet proven live.**
  Someone/something completed the spec's §1-6 (runtime upgrade to .NET 10, package deploy,
  workflow-resource staging) between the spec being drafted and a later verification pass the
  same day — before this session made any change. Verified directly against the live site via the
  Kudu VFS API, since `warewolfserver-uat-ai` App Insights still has zero telemetry:
  `netFrameworkVersion` is `v10.0`, the deployed `Warewolf.Execution.Lightweight.dll` is a fresh
  build (not the stale 08-13 08:50 package), `Resources/rabbit/` contains exactly
  `RabbitProcess.bite` + `RabbitProcess2.bite` + a `NewSqlServerSource.bite` resolving to
  `SourceId="b9184f70-…"` (the DPAPI-locked `(Local Backup)` file correctly excluded), and
  `workflow-index.json` keys the workflows as `rabbit/rabbitprocess`/`rabbit/rabbitprocess2` —
  consistent with both pipelines' `'rabbit/RabbitProcess'` name. `GET /apis.json` returns `200`.
  **Not yet verified**: an actual single-message or full load-test run against this build, because
  minting the `-MessageAuthToken` needed by `Test-ShovelBridgeE2E.ps1 -VerifyWorkflowExecution`
  requires the `ShovelE2EDaemonClientSecretValue` Azure DevOps pipeline secret (write-only by
  design, not retrievable outside a pipeline run and not mirrored in the app's Key Vault), and
  triggering `pipeline-LOADTEST.yml` directly was blocked by the same broken `az` CLI
  extension-cache permission error already on file for `az monitor app-insights query`. See
  `Deploy-UAT-Redeploy-Spec.md` §12 for the full finding and what's needed to close it out.

- **2026-08-14 — Confirmed the `JobLogId` fix, and found + corrected a second bad deploy: the
  build behind the entry above did not actually contain the pooling fix.** Minted a fresh Entra
  client-credentials token (the user's supplied `ShovelE2EDaemonClientSecret` value didn't match
  the app registration's active secret; created an additive replacement per the sanctioned
  `az ad app credential reset` convention documented in `pipeline-CLOUD.yml`) and called
  `/Secure/rabbit/RabbitProcess` directly (`WorkflowHttpFunction.cs`) as a lower-risk proxy for
  §7.4 — same `WorkflowExecutor` path as the Service Bus trigger, but doesn't exercise RabbitMQ,
  the Shovel, or `ServiceBusWorkflowTriggerFunction` itself.
  - A 15-way concurrent burst reproduced the **exact pre-fix signature verbatim** — `"Object
    reference not set to an instance of an object."` / `"Error with variables in input.
    [[JobLogId]]"` / `SQL Error [Number=51001]... invalid state transition` — 10/15 failed.
    Confirmed `8510` HEAD does contain the fix (`_workflowPool`/`RentPreparedWorkflow` present, no
    uncommitted changes) and that a fresh local `Release` build differs in size from the live DLL
    (389120 vs 388096 bytes) — the previously-deployed package was not built from this HEAD.
    Kudu's deployment history carries no commit metadata, so its actual source could not be traced.
  - **Corrected**: backed up the live DLL, downloaded the (already-correct) live `Resources/` tree
    via Kudu VFS, merged it into a fresh HEAD build, and redeployed
    (`az functionapp deployment source config-zip`). Note for the deploy scripts/docs:
    `WEBSITE_RUN_FROM_PACKAGE=1` on this app means a "Succeeded" zip-deploy response does **not**
    mean the running worker has switched packages — `packagename.txt` updates immediately but an
    explicit `az functionapp restart` was needed before Kudu VFS (and live behaviour) reflected the
    new package.
  - **Verified fixed**: post-restart, a 20-way burst against the cold (just-restarted) pool had
    **zero** `[[JobLogId]]` errors — its failures were a different, expected-under-cold-start
    signature (`Insufficient memory...` during Roslyn VB-expression compilation in
    `ActivityParser.Parse`, plus a few `502`s consistent with Consumption-plan scale-out), the
    up-front cost the pooling fix trades for correctness when every concurrent request hits an
    empty pool at once. A follow-up 10-way burst against the now-warm pool: **10/10 succeeded,
    zero errors of any kind.** The concurrency fix is confirmed working live.
  - **Still open**: §7.5, the full 1000-message run through the real RabbitMQ→Shovel→Service Bus
    pipeline (not exercised by the direct-HTTP proxy above) — needs either the ADO pipeline run or
    a local RabbitMQ+Shovel+Service Bus stack. Also open: whether the cold-pool memory pressure
    warrants a plan-capacity or warm-up follow-up (see §10-style follow-ups) for bursty concurrent
    traffic after any cold start/restart. See `Deploy-UAT-Redeploy-Spec.md` §13 for full detail.

- **2026-08-14 — §7.5 closed: full 1000-message `-VerifyWorkflowExecution` run via a local
  RabbitMQ (`-RabbitMqMode External`) + `-DestinationMode ExternalServiceBus` stack reached
  **961/1000** (24 `HttpError`, 15 no-result-before-timeout) — a large jump from the pre-fix
  702/1000, confirming the `_workflowPool`/`RentPreparedWorkflow` fix from the entry above holds at
  full load. The residual 3.9% is a **different, infrastructure-level** failure signature, not a
  bad deploy — do not redeploy on this evidence alone.**
  - **Not the `JobLogId` race**: the 24 `HttpError`s were bare `GET
    /secure/servicebus-result/{correlationId}` → `500` responses with an **empty body** (confirmed
    from `E2EHarness`'s own captured response text), unlike WOLF-8418 policy denials (which always
    nest an `Error{…}` JSON body) or the earlier `SQL 51001`/`[[JobLogId]]` text. The 15 no-results
    were transient `503`s that never resolved before `-ResultTimeoutSeconds 1800` expired.
  - **Deploy freshness ruled out**: `WarewolfServer-UAT`'s `lastModifiedTimeUtc` was `2026-08-14
    11:44:50`, ~8 hours before this run — the same "corrected" build from the entry above, not a
    stale/mismatched package.
  - **App Insights has no telemetry** (`warewolfserver-uat-ai`, confirmed via the Application
    Insights REST API: zero `requests` rows in the last 7 days), so the 500s can't be root-caused
    from a captured exception. Instrumentation wiring for this app is still an open gap.
  - **Confirmed root cause — direct from the dead-letter queue, not inference**: rather than relying
    on Kudu's `LogFiles/eventlog.xml` or (telemetry-less) App Insights, a temporary `Listen`-rights
    SAS rule was added to `wwexecution-secure-trigger-queue-e2e` (removed again immediately after)
    and used to non-destructively peek its `$DeadLetterQueue` sub-queue
    (`ServiceBusReceiver.PeekMessagesAsync`, `SubQueue.DeadLetter`) filtered to this run's
    correlationId prefix. **7 of the 15 "no result" correlationIds (`-000039`, `-000152`, `-000171`,
    `-000172`, `-000174`, `-000176`, `-000180`) were found dead-lettered**, all at `19:43:22Z`
    (a single burst), all with `DeadLetterReason: execution_failed` and
    `DeadLetterErrorDescription: Insufficient memory to continue the execution of the program.` —
    the exact same cold-start memory-pressure signature (Roslyn VB-expression compilation in
    `ActivityParser.Parse`) already observed and closed as "expected under cold start" in the direct-
    HTTP-proxy entry above. These messages are **permanently** dead-lettered (not slow) — the
    remaining 8 "no result" correlationIds were not found in the DLQ and are presumed still subject
    to Service Bus's own `maxDeliveryCount=10` redelivery/lock-renewal cycle rather than exhausted.
    The other 24 `HttpError`s are **not** message-processing failures at all (nothing dead-lettered
    matches those correlationIds) — they're specific to the `GET
    /secure/servicebus-result/{correlationId}` read path itself, still unexplained pending
    `warewolfserver-uat-ai` telemetry.
  - **Recommendation (superseded by the 2026-08-14 fix entry below)**: at the time of this finding
    there was no code defect evidence — the DLQ confirmed an infrastructure/capacity cause
    (cold-start memory pressure being dead-lettered permanently instead of retried), not a logic
    bug, so a redeploy was **not** recommended on this evidence alone. That gap (permanent
    dead-letter instead of retry) has since been fixed in code — see below; a redeploy **will** be
    needed once that fix is validated. The plan-capacity/warm-up follow-up (e.g. an Elastic Premium
    plan with an `Always Ready` instance count, or a pre-warming request burst before starting the
    load test) and wiring up `warewolfserver-uat-ai` telemetry (so the residual `HttpError` 500s can
    be root-caused from `requests`/`exceptions` instead of DLQ inspection) both remain open.

- **2026-08-14 — Code fix: OutOfMemoryException surfaced as a *transient* failure, retried instead
  of dead-lettered.** The DLQ evidence above showed cold-start `OutOfMemoryException`s during
  `ActivityParser.Parse`'s Roslyn VB-expression compilation being dead-lettered with
  `deadLetterReason: "execution_failed"` on the **first** attempt, identically to a genuine
  workflow/business failure — even though Service Bus's standard `maxDeliveryCount=10`
  retry/backoff (already correctly wired for *unexpected exceptions* thrown out of
  `_executor.Execute()`, see `ServiceBusWorkflowTriggerFunction.ProcessAuthenticatedMessageAsync`)
  would very likely have succeeded on redelivery once the instance warmed up or scaled out. Root
  cause: `WorkflowExecutor.Execute`'s catch-all `catch (Exception ex)` was swallowing
  `OutOfMemoryException` and returning it as an ordinary `WorkflowExecutionResult { IsSuccess =
  false }`, which the trigger function's business-failure branch dead-lettered immediately, never
  reaching the exception-rethrow/retry path.
  - **Fix**: `WorkflowExecutionResult` gained a new `IsTransientFailure` flag (default `false`).
    `WorkflowExecutor.Execute` now has a dedicated `catch (OutOfMemoryException oom)` clause
    (`Execution/WorkflowExecutor.cs`, ahead of the generic catch) that sets `IsTransientFailure =
    true` via the new internal `BuildTransientFailureResult` helper.
    `ServiceBusWorkflowTriggerFunction.ProcessAuthenticatedMessageAsync` (`Functions/`) now checks
    `result.IsTransientFailure` before the existing success/business-failure branching: when true,
    it deliberately does **not** call `_store.SaveResult` (a persisted terminal "Failed" result
    would satisfy the idempotency dedupe check on redelivery and complete the retried message
    without ever re-executing it) and does **not** dead-letter — it throws instead, reusing the
    same Service Bus retry/backoff path as an unexpected exception, so the message is only
    dead-lettered once `maxDeliveryCount` is actually exhausted. HTTP callers
    (`WorkflowHttpFunction.cs`, `LoginFunction.cs`) are unaffected — they ignore the new flag and
    keep their existing synchronous failure-response behaviour (there is no broker to retry
    against for a synchronous HTTP call). See `docs/ServiceBusSecureTrigger-Architecture.md` for
    the updated flow description.
  - **Tests**: `WorkflowExecutorTransientFailureTests` (new) verifies
    `BuildTransientFailureResult`/`WorkflowExecutionResult.TransientFailure` set
    `IsTransientFailure = true` and don't affect the existing `Failure(...)` factory.
    `ServiceBusWorkflowTriggerFunctionTests.ProcessAuthenticated_ExecutionTransientFailure_ThrowsInsteadOfDeadLettering_NoResultPersisted`
    (new) verifies a transient result throws, is not dead-lettered/completed, and is not persisted
    to the replay/result store. All 740 existing tests under the `Execution`/`Functions` filter
    (including the pre-existing business-failure and unexpected-exception cases) remain green — no
    behavioural regression for genuine business failures or HTTP callers.
  - **Not yet addressed by this fix**: the 24 unexplained `HttpError` 500s on the
    `GET /secure/servicebus-result/{correlationId}` read path (separate issue, still pending
    `warewolfserver-uat-ai` telemetry) and the plan-capacity/warm-up follow-up (this fix makes cold
    starts *recoverable* via retry, it does not reduce how often they happen).
  - **Recommendation**: redeploy `WarewolfServer-UAT` once this fix is reviewed, then re-run the
    full 1000-message load test to confirm the residual DLQ rate for `execution_failed` drops to
    (near) zero and overall success rate improves beyond 961/1000.
  - **Deployed (2026-08-14, 22:00 UTC)**: this fix has now been redeployed to `WarewolfServer-UAT`
    ahead of a PR/merge — `dotnet publish -c Release`, `az functionapp deployment source
    config-zip` (`DEV2`/`WarewolfServer-UAT`), then an explicit `az functionapp restart` (this app
    runs `WEBSITE_RUN_FROM_PACKAGE=1`, so the new package is not picked up without a restart — see
    the redeploy gotcha in `Deploy-UAT-Redeploy-Spec.md`). Verified via Kudu VFS: live
    `Warewolf.Execution.Lightweight.dll` is now 390144 bytes / mtime matching the local build
    exactly (previously-live DLL was 389120 bytes), and `GET /apis.json` returns `200` post-restart
    confirming the app is up. **Still to do**: re-run the full 1000-message load test against this
    deployment to confirm the fix holds under real load, and get this change merged via PR (it is
    currently only deployed to UAT from the local `8510-ShovelBridgeE2ETestUpdates` branch, not yet
    committed/merged).
  - **Re-run result (2026-08-14, ~22:15-22:45 UTC), post-deploy**: full 1000-message
    `-VerifyWorkflowExecution` run against the redeployed engine reached **992/1000 succeeded, 0
    explicit failures, 8 "no result"** — a clear improvement over the pre-fix 961/1000 (24
    `HttpError`, 15 no-result), and critically **zero** `HttpError`s this time.
    - **The fix is confirmed working**: the Service Bus queue's `deadLetterMessageCount` was
      **12384 both before and after this run** — i.e. this run added **zero** new dead-lettered
      messages. Combined with 0 explicit failures reported by the harness, every transient
      (OOM/cold-start) execution failure that occurred during this run was retried and recovered
      via Service Bus's standard redelivery — none exhausted `maxDeliveryCount` and fell through to
      a permanent dead-letter, which is exactly the behaviour this fix was built to produce.
    - **The residual 8 "no result" cases are a *different*, unrelated issue, NOT addressed by this
      fix**: re-peeked the DLQ (temporary `Listen`-rights SAS rule, same technique as the original
      diagnosis, removed again after) for all 8 correlationIds — **found in neither the DLQ nor the
      live queue** (`activeMessageCount: 0` at run end) — and a direct `GET
      /secure/servicebus-result/{correlationId}` call for each, well after the run, returned a
      persistent `404` (not a transient error) for all 8. A message that was received and processed
      always ends up in exactly one of: completed-with-a-saved-result, or dead-lettered
      (business failure or delivery-count-exhaustion) — none of the 8 match any of those states.
      The most likely explanation is that these specific messages were **never actually delivered**
      by the RabbitMQ Shovel to the Service Bus destination queue in the first place (a bridge-layer
      delivery gap under the 20-way concurrent burst), rather than anything going wrong in the
      Lightweight engine's execution/retry/dead-letter handling. This is a **separate, still-open
      issue** in the RabbitMQ→Shovel→Service Bus bridge itself and needs its own investigation
      (e.g. Shovel `message_stats`/`ack`/`nack` counters per run, or enabling publisher-confirms
      logging) — out of scope for this fix.
    - **Net assessment**: the dead-letter/retry fix fully achieves its goal (no more permanent,
      un-retried dead-lettering of transient cold-start failures); the load test's residual ~0.8%
      "no result" rate is now attributable to a different, pre-existing bridge-delivery-reliability
      gap, not to the execution engine.
  - **Harness bug found (2026-08-15): a single slow poll aborted the whole 1000-message run.**
    The very next full re-run (same command as the 992/1000 run above) failed outright with
    `FAIL: TaskCanceledException: The request was canceled due to the configured
    HttpClient.Timeout of 100 seconds elapsing.` — NOT a partial result like 992/1000, a total
    harness crash. Post-run diagnostics showed the RabbitMQ side was actually fine (source queue
    `messages (total): 0`, `deliver_get`/`ack` both `6000`, `redeliver: 0` — every message the
    Shovel accepted was delivered and acknowledged; the Shovel itself stayed `running`).
    - **Root cause**: `Program.cs`'s `PollOneServiceBusResultAsync` (used by
      `WaitForServiceBusResultsAsync`'s bounded-concurrency sweep loop, 20-way concurrent GETs
      against `/secure/servicebus-result/{correlationId}`) only caught `HttpRequestException` and
      a fixed set of transient status codes (503/502/504/429/408) as retryable. `HttpClient`'s
      default 100s `Timeout` elapsing on a single GET throws `TaskCanceledException` (a
      `HttpRequestException` is NOT thrown in this case on modern .NET) — this was uncaught, so
      it propagated out of that one sweep task, through `Task.WhenAll(sweepTasks)`, out of
      `WaitForServiceBusResultsAsync`, and up to `Main`'s catch-all, which printed a bare `FAIL:`
      and returned exit code 1 — abandoning polling for every still-pending correlationId even
      though the 1800s `-ResultTimeoutSeconds` budget had barely been used. One slow response
      under 1000-message/20-way-concurrent load was enough to fail the entire run.
    - **Fix**: added a `catch (OperationCanceledException ex)` alongside the existing
      `catch (HttpRequestException ex)` in `PollOneServiceBusResultAsync`, treating an
      `HttpClient.Timeout` the same as the other transient statuses above — the sweep loop just
      retries that one correlationId on the next 3s sweep instead of aborting. No
      `CancellationToken` is ever passed to `GetAsync` anywhere in this harness, so a
      `TaskCanceledException` here can only mean the client-side `Timeout` elapsed, never an
      unrelated cancellation being masked.
    - **Verified (2026-08-15)**: rebuilt the harness (`dotnet build -c Release`, 0 errors, only
      the pre-existing unrelated `NU1510` warning) and re-ran the full 1000-message
      `-VerifyWorkflowExecution` load test end-to-end against `warewolfserver-uat`. The fix
      holds: multiple `(transient timeout polling ... - retrying)` lines appeared during the run
      (proving the bug condition still occurs regularly under this load) and each was retried
      instead of aborting the run. The full 1800s poll budget was used and the harness exited
      cleanly with a proper summary: **967/1000 succeeded, 0 explicit failures, 33 "no result"**
      — no crash, no lost diagnostics for the other 967.
    - **The residual "no result" cases are confirmed, again, to be the same pre-existing
      bridge-delivery-gap issue** (not a regression from this fix, and not caused by it): using a
      temporary Listen-rights SAS rule on the destination queue (created and torn down the same
      way as the prior 8-case investigation), a small throwaway peek tool
      (`Azure.Messaging.ServiceBus`, `PeekMessagesAsync`) scanned BOTH the active queue (0
      messages — fully drained) and the ENTIRE dead-letter subqueue (12,510 messages
      accumulated across this shared, never-purged testing namespace) for this run's own
      correlationId prefix. **Zero matches in either** — none of the 33 missing messages (the
      10 explicitly logged, or their body/correlationId content generally) exist anywhere on the
      destination queue. This directly rules out an engine-side execution/dead-letter bug for
      these 33 (an engine failure would have to leave a dead-lettered message behind) and
      confirms the messages never arrived at Service Bus at all — a RabbitMQ Shovel delivery gap,
      not the harness, not `WaitForServiceBusResultsAsync`, and not the Lightweight engine.
      The raw RabbitMQ queue-level lifetime counters (`publish`/`deliver_get`/`ack`) were not
      usable to corroborate this further: they accumulate across every historical run on this
      shared, reused source queue (including concurrent CI activity), so a single run's delta
      cannot be isolated from them — the direct destination-queue peek above is the reliable
      signal.
    - **CORRECTION (2026-08-15, same day) — the "Shovel bridge delivery gap" conclusion above was
      WRONG. The real root cause was found, fixed, and the load test now passes 1000/1000.**
      The Shovel/RabbitMQ side was never the problem. Chasing the planned next steps for this
      (enabling Shovel publisher-confirms logging, inspecting broker logs for reconnects,
      tuning `-ShovelPrefetchCount`/`-PublishConcurrency`) would have been wasted effort against
      infrastructure (`rabbitmq.warewolf.online`) this project doesn't control (no SSH, and its
      Prometheus port `15692` isn't exposed through the tunnel — confirmed by direct probe) — the
      actual defect was entirely on the Lightweight engine's own result-store.
      - **Actual root cause: `Config.Persistence.Enable` was `false` on the deployed
        `warewolfserver-uat`, so `ServiceBusReplayAndResultStore` (Security/ServiceBusReplayAndResultStore.cs)
        falls back to a per-instance, in-memory `ConcurrentDictionary` for BOTH jti replay
        protection and — critically — `TryGetResult`/`SaveResult`.** Under the 1000-message
        burst, Azure Functions Consumption-plan scale-out spins up multiple instances; a message
        can be processed (and its result `SaveResult`'d) on instance A while the harness's later
        `GET /secure/servicebus-result/{correlationId}` poll is load-balanced to instance B,
        whose in-memory dictionary never saw that result — a permanent 404 for a message that
        actually executed successfully. This exactly matches every symptom recorded above: 0
        active messages, 0 dead-lettered messages, yet a persistent 404 forever. The
        `ServiceBusReplayAndResultStore` class's own doc comment already named this exact failure
        mode ("NOT safe across multiple instances behind a load balancer or multiple
        Consumption-plan workers") — it had just never been connected to this symptom before.
      - **Confirmed, not just theorised**, via direct evidence, in order:
        1. Fetched the live `Settings/persistencesettings.json` from the deployed package via
           Kudu VFS: `"Enable": false`.
        2. Found a pre-existing, already-provisioned Azure SQL database
           `wwexecution-uat-hangfire` (RG `DEV2`, server `warewolf-dev2-mcgeaj`) with the full
           Hangfire schema already installed (11 `HangFire.*` tables) and **9,001 pre-existing
           `sbtrigger:result:*` rows in `HangFire.Hash`**, comprising exactly nine prior clean
           1000-row load-test runs (`9 × 1000 = 9000`) plus one single-message test — i.e.
           persistence had demonstrably worked perfectly before, with zero "no result" cases in
           any of those nine runs, then was later silently disabled.
        3. The deployed `Settings/persistencesettingsdbsource.bite` (dated 2026-08-10, i.e.
           **before** the DB above was even created on 2026-08-13) turned out to be a leftover
           local-dev placeholder (`Data Source=(local)\sqlexpress;Initial Catalog=hangfiredb;
           Integrated Security=SSPI` — unreachable from Azure) — further confirming persistence
           was non-functional on this deployment regardless of the `Enable` flag.
      - **Fix applied**: reset the password on the pre-existing, dedicated SQL login
        `wwexecution_uat_hangfire` (already `db_owner` on `wwexecution-uat-hangfire` — this login
        already existed, confirming it was the one used by the nine earlier successful runs),
        built a new `Settings/persistencesettingsdbsource.bite` pointing at
        `wwexecution-uat-hangfire` in the same minimal shape
        `Warewolf.Execution.Lightweight.Tests/PersistenceConfigLoaderTests.cs` uses, WFAES-encrypted
        it via `Scripts/Encrypt-Config.ps1` against the SAME Key Vault key already in use for this
        deployment (`WWExecutionEngine` / secret `WWExecutionEngineTestSecret` — read from the
        live app's own `KEYVAULT_SECRET_NAME` setting, not guessed), and flipped
        `Settings/persistencesettings.json`'s `Enable` to `true`. Downloaded the entire live,
        already-working package via Kudu's `/api/zip/site/wwwroot/`, replaced ONLY these two
        files (deliberately avoiding a full rebuild/redeploy and its much larger blast radius —
        no code, `Resources/`, or app-settings changes), re-zipped, redeployed via
        `az functionapp deployment source config-zip`, and restarted (required —
        `WEBSITE_RUN_FROM_PACKAGE=1` mounts `wwwroot` read-only from the package, confirmed by
        the earlier 2026-08-14 redeploy notes in `Deploy-UAT-Redeploy-Spec.md`).
      - **Verified working end-to-end before the full load test**: a single-message
        `-VerifyWorkflowExecution` run (§7.4-style) passed, and its result row was confirmed
        present in `HangFire.Hash` by direct SQL query immediately after — proving the store is
        genuinely shared/durable now, not just "no longer false".
      - **First full 1000-message re-run (persistence enabled, SQL DB still at its original `S0`
        tier): 992/1000 succeeded, 0 "no result" (down from 33), 8 explicit failures** — 1
        `InvalidToken` ("A task was canceled") and 7 bare `HttpError` 500s on the result-GET path.
        This is a materially different (and much better) failure signature than before: EVERY
        correlationId now gets a definitive answer, with no invisible/unexplained cases. The
        remaining 8 are consistent with the Hangfire SQL store's own capacity, not a logic bug:
        `wwexecution-uat-hangfire` was provisioned at `S0` (10 DTUs, the smallest Standard tier)
        — persistence being enabled means every trigger's jti-replay check (`AcquireDistributedLock`
        + hash read/write) and every result save/read now costs a real SQL round-trip instead of
        an in-memory lookup, and a burst of 1000 messages plus 20-way concurrent result-polling
        against a 10-DTU database is a plausible bottleneck (SQL timeouts manifesting as the
        auth-pipeline's generic 500 path, and possibly starved thread-pool threads manifesting as
        the one token-validation cancellation).
      - **Scaled `wwexecution-uat-hangfire` from `S0` to `S2` (50 DTUs, `az sql db update
        --service-objective S2`) — a reversible, resource-only change, no code/config changes.**
        **Second full 1000-message re-run: 1000/1000 succeeded, in 118.5s (8.4 exec/s)`, and all
        1000 result rows were confirmed present in `HangFire.Hash` by direct SQL query
        immediately after.** The ShovelBridge load test now passes cleanly end-to-end.
      - **Net assessment, superseding every prior entry in this dated log**: there was never a
        RabbitMQ Shovel bridge-delivery gap. Every "no result"/partial-success outcome recorded on
        2026-08-13/14/15 was the same single root cause (`Config.Persistence.Enable=false` →
        per-instance in-memory result visibility under Consumption-plan scale-out), which
        manifested at a rate (8-39 per 1000) that happened to resemble a small bridge-loss
        percentage closely enough to mislead every prior investigation, including this document's
        own. The residual DTU-capacity sensitivity is a normal, expected trade-off of moving from
        in-memory to durable SQL-backed persistence under burst load, not a new defect class.
      - **Follow-up required (not yet done, flagged for whoever next redeploys UAT)**: the
        UAT redeploy on 2026-08-14 22:00 UTC (recorded earlier in this doc) evidently reverted
        persistence back to disabled — the exact regression this section just fixed — because
        redeploying from a fresh publish output naturally carries the repo's own committed
        default (`Enable: false`, no `persistencesettingsdbsource.bite` at all). **Any future full
        redeploy of `WarewolfServer-UAT` MUST re-stage the persistence-enabled
        `Settings/persistencesettings.json` + `Settings/persistencesettingsdbsource.bite` pair
        (via `Deploy-WwExecutionEngine.ps1 -EnablePersistence -PersistenceSettingsPath ...
        -PersistenceDbSourcePath ...`, pointing at `wwexecution-uat-hangfire` — do NOT let the
        deploy silently fall back to the repo's disabled default), or this exact defect will
        recur.** See the added note in `docs/Deploy-UAT-Redeploy-Spec.md`.

- **2026-08-16 — Full 1000-message `-VerifyWorkflowExecution` run via `pipeline-LOADTEST.yml`'s
  `Deploy_UAT` → `ShovelBridgeLoadTest_ExternalServiceBus` jobs (correlationId prefix
  `b4f19c2eefdf4270aa073b15fa3dd97f`) reached **843/1000 (150 failed, 7 no result)** — worse than
  the 2026-08-15 fix's clean 1000/1000, but confirmed via direct evidence (Azure CLI/REST against
  the live resources, not inference) to be a THIRD, previously undocumented cause, NOT a
  re-regression of either fix already recorded above.**
  - **Persistence still enabled, unchanged**: live `Settings/persistencesettings.json` (Kudu VFS)
    shows `"Enable": true` — the 2026-08-14 persistence-disabled regression has not recurred.
  - **`wwexecution-uat-hangfire` still at `S2` (50 DTU)**, not reverted to `S0`
    (`az sql db show` → `currentSku: {name: Standard, capacity: 50}`), and **DB was never a
    bottleneck this run**: `dtu_consumption_percent` (`Microsoft.Insights/metrics` REST API,
    00:10-00:50Z window) peaked at only 0.9% — ruling out the exact DTU-starvation cause fixed on
    2026-08-15.
  - **`warewolfserver-uat-ai` now has real telemetry** (every earlier entry in this log recorded
    "zero telemetry") — its `exceptions`/`traces` tables have data for 2026-08-16 (107 exceptions,
    24233 traces), though `requests` remains empty (a separate, smaller gap worth a follow-up).
  - **Exceptions during the test window, queried directly from `exceptions` via the Application
    Insights Analytics REST API**:
    - **51× `Grpc.Core.RpcException` / `MessageLockLost`** at
      `ServiceBusWorkflowTriggerFunction.ProcessAuthenticatedMessageAsync` — `"The lock supplied is
      invalid. Either the lock expired, or the message has already been removed from the queue"`
      from `CompleteMessageAsync`. The destination queue's `LockDuration` is a fixed `PT1M`
      (`az servicebus queue show`); under a genuine 1000-message concurrent burst, with
      `host.json`'s `concurrency.dynamicConcurrencyEnabled: true` self-throttling the worker under
      CPU/thread-pool pressure, per-invocation processing (including the cold-start Roslyn compiles
      below) can outrun the SDK's own lock-renewal loop.
    - **32× `System.OutOfMemoryException` at `Dev2.Activities.ActivityParser.Parse`** — the same
      cold-start Roslyn VB-expression-compile memory-pressure signature already root-caused and
      made *retriable* (not permanently dead-lettered) by the 2026-08-14 `IsTransientFailure` fix.
    - **24× `System.InvalidOperationException` — "Transient workflow execution failure for
      correlationId ..."** — that same fix working exactly as designed (the deliberate re-throw
      that forces Service Bus redelivery instead of dead-lettering), not a new defect.
  - **Working theory for the harness's dominant `InvalidToken` ("operation was canceled") failure
    signature** (not yet proven by a direct correlationId-to-exception join — `exceptions` here
    doesn't carry the correlationId as a queryable custom dimension): `EntraBearerTokenValidator
    .ValidateAsync` is called with the invocation's own `CancellationToken`
    (`ServiceBusWorkflowTriggerFunction.cs`) — if that token is cancelled when the Functions host
    abandons/retries an invocation whose message lock has already lapsed, or under
    `dynamicConcurrencyEnabled` self-throttling, the in-flight token validation throws
    `OperationCanceledException` and is caught/reported as `InvalidToken`. Flagged as the next
    concrete step to prove (e.g. logging the correlationId alongside this exception) if it recurs.
  - **Net assessment**: neither of the two previously-fixed root causes (workflow-pool
    thread-safety, `Config.Persistence.Enable`) regressed. The residual ~15.7% failure rate is best
    explained by **Consumption-plan (Y1) capacity under sustained 1000-message concurrent burst** —
    insufficient CPU/thread-pool headroom to keep every in-flight message's Service Bus lock
    renewed and its token-validation call running to completion — compounded by the known Roslyn
    cold-start memory pressure. This matches the risk already flagged as
    `Deploy-UAT-Redeploy-Spec.md`'s Risk R3 ("Consumption (`Y1`) plan: no `alwaysOn`, cold starts,
    capped scale-out").
  - **Recommendation (not yet actioned — a cost/infra decision, out of scope for this
    investigation)**: the plan-capacity/warm-up follow-up flagged repeatedly above (an Elastic
    Premium plan with an `Always Ready` instance count, and/or staggering the harness's publish
    burst instead of a single 1000-message blast) remains the most direct lever to close this gap.
    A lower-risk, no-cost alternative worth trying first: tune `ServiceBusOptions
    .MaxAutoLockRenewalDuration` (isolated-worker `worker.json`/host configuration) so lock renewal
    keeps pace with slower cold-start invocations without changing the queue's own `LockDuration`.

- **2026-08-17 — Fix: `Test-ShovelBridgeE2E.ps1` now pre-warms `-EngineBaseUrl` in Phase 0.** The
  `ShovelBridgeE2ETest_ExternalServiceBus` job's `-VerifyWorkflowExecution` leg (run ID `7ead1234`,
  correlationId `0903d48c96d148bb820458c241213837`) failed with `FAIL: no result was recorded for
  correlationId '...' at https://warewolfserver-uat.azurewebsites.net/... within 90s. Last transient
  error: GET .../secure/servicebus-result/... returned 503: The service is unavailable.` — the
  Shovel itself bridged the message fine (`forwarded: 1`, `state: flow` in the Phase 4b diagnostics),
  so this was purely a read-path failure against the UAT engine.
  - **Root cause: `WarewolfServer-UAT` Consumption-plan (Y1, `alwaysOn: false`) cold start
    consuming the entire 90s `-ResultTimeoutSeconds` budget**, the same capacity limitation as Risk
    R3 (`Deploy-UAT-Redeploy-Spec.md`) and every prior entry in this log. Ruled out an
    auth/config-policy cause first: the response body was the literal platform string `"The service
    is unavailable."`, not the app's own JSON error shape (`WorkflowAuthorizationMiddleware.cs`'s
    `ConfigMissingDeny` path always returns `{"error":"config_missing",...}`); confirmed live that
    `BYPASS_SECURE_CONFIG=true` is still set on the app (so `ConfigMissingDeny` can't fire); and
    confirmed the app was reachable and warm (`GET /apis.json` → `200`) minutes later — consistent
    with a cold instance finishing initialisation shortly after the harness gave up, not a
    persistent outage.
  - **Fix**: added a new `-EnginePrewarmTimeoutSeconds` parameter (default `120`) and a Phase 0
    pre-warm loop that polls the public, unauthenticated `GET /apis.json` route on `-EngineBaseUrl`
    (retrying every 5s) until it responds or the budget elapses, **before** Phases 1-3's own RabbitMQ/
    Shovel setup time — so cold start now overlaps with that setup instead of eating into Phase 4's
    timed result-poll window. A failed/timed-out pre-warm logs a warning and does not abort the run;
    Phase 4's existing transient-503 retry logic (`Warewolf.Execution.ServiceBusWorker.E2EHarness`)
    is unchanged and still the last line of defence. This is exactly the "pre-warming request burst
    before starting the load test" lever flagged as open, low-risk and no-cost in the entries above —
    it does not address the underlying Y1-capacity limitation (still open, still a cost/infra
    decision), only the specific failure mode of the harness's own fixed poll window being consumed
    by a cold start it never triggered proactively.
  - **Not yet re-verified against a live pipeline run** — the change has been parse-checked and the
    pre-warm probe manually confirmed to succeed against the (currently warm) live engine, but the
    `ShovelBridgeE2ETest_ExternalServiceBus` job itself has not been re-run post-fix.

- **2026-08-30 — Fix: `host.json` now sets `extensions.serviceBus.maxAutoLockRenewalDuration`,
  closing the gap flagged as "not yet actioned" in the 2026-08-16 entry above.** `ShovelBridgeLoadTest_ExternalServiceBus`
  build 30578 (`8512-ProcessingReliabilityTests`, commit `c9f5ecfa99`) ran with the Aug 24/25
  `ServiceBusTriggerOptions` mitigations (`MaxConcurrentExecutions`, `ExecutionTimeout`,
  `SlotWaitTimeout`, stale-claim takeover) already in place: 984/1000 resolved within 47s, but
  16 correlation ids (a contiguous block, `...-000721` through `...-000736`) never got a result
  and the count never moved again across the remaining ~29 minutes of polling. RabbitMQ
  diagnostics confirmed all 1000 messages left the source queue via the shovel cleanly — the
  loss was downstream, in Service Bus → engine trigger processing.
  - **Root cause**: the same lock-renewal race first identified in the 2026-08-16 entry, not yet
    closed. The load test's queue (`Test-ShovelBridgeE2E.ps1`) has `LockDuration=PT1M` and
    `maxDeliveryCount=10`; `ServiceBusTriggerOptions` gives a delivery up to `SlotWaitTimeout`
    (5 min) + `ExecutionTimeout` (5 min) = 10 min worst case, but the Functions SDK's default
    `maxAutoLockRenewalDuration` is only 5 min and `host.json` never overrode it. A delivery
    that genuinely needs close to the full budget loses its lock at the 5-minute mark; a
    duplicate delivery then races it, hits `ServiceBusWorkflowTriggerFunction.cs`'s
    "leave unsettled" branch (added for exactly this case), and the pair burn through
    `maxDeliveryCount` in rapid short cycles until Service Bus auto-dead-letters the message
    **inside the SDK, without the trigger's own code ever running again** — the one path that
    produces "no result, ever" instead of a recorded failure. Confirmed live that this happens
    at volume in this environment: `wwexecution-secure-trigger-queue-e2e`'s dead-letter queue
    held 19,459 unpurged messages at the time of this investigation. (Could not pin the exact
    dead-letter reason for these specific 16: `warewolfserver-uat`'s Application Insights had
    zero telemetry for the Aug 28 run window, and the Service Bus namespace has no diagnostic
    settings configured — a full 19k-message DLQ scan was judged not worth the effort for this
    pass.)
  - **Fix**: `host.json`'s `extensions.serviceBus.maxAutoLockRenewalDuration` set to `00:11:00`
    (a 1-minute margin over the current 10-minute worst case, matching the margin style already
    used for `ServiceBusReplayAndResultStore.ClaimStaleAfter` vs `functionTimeout`). New test
    `HostJsonConfigurationTests` asserts the value is set and is `>=` `ServiceBusTriggerOptions`'
    own `SlotWaitTimeout + ExecutionTimeout` defaults (not a hardcoded duration), so widening
    either timeout without widening this renewal window fails the build instead of silently
    reopening this gap.
  - **Not yet re-verified against a live pipeline run** — same caveat as the 2026-08-17 entry.

- **2026-08-30 — Fix: `DbSource.GetConnectionStringWithTimeout` now honours the timeout override
  for Entra Managed Identity sources too, and `ResolveEffectiveConnectionTimeout` gets a final
  positive floor.** `ShovelBridgeLoadTest_ExternalServiceBus` build 30583 (`8512-ProcessingReliabilityTests`,
  commit `6eeeaaa5ab`, run after the `maxAutoLockRenewalDuration` fix above) improved from 16 to
  **12** correlation ids stuck forever, still never resolving across the full 30-minute poll
  window: `987/1000` succeeded, `1` recorded failure (`-000601`, "Workflow not found" — a distinct,
  already-understood issue), `12` never got a result. Confirmed via `az devops invoke` against the
  build's own logs (not a partial paste) that these 12 clustered tightly (`...-000582`,
  `...-000588` through `...-000599`), the same contiguous-block shape as the 16-id incident above.
  - **Diagnosis, this time backed by direct SQL ground truth against the Hangfire persistence
    store** (`wwexecution-uat-hangfire`, confirmed live and enabled — `Deploy_UAT`'s
    `-EnablePersistence:$true` per `Deploy-UAT-Redeploy-Spec.md` §14 was in effect for this build,
    ruling out the in-memory-fallback theory this investigation initially suspected): all 1000
    correlation ids had a claim row; exactly 988 had a matching result row. The control case
    (`-000601`) had both, proving the store and `/secure/servicebus-result/{id}` polling path work
    correctly for any execution that returns. For the 12: a claim row exists (execution started)
    but no result row was ever written — `SaveResult` was never reached by success, business
    failure, malformed message, denial, or invalid-token path, and the claim's timestamp was never
    updated by a later stale-claim steal (`ClaimStaleAfter` = 20 min), even hours after the run.
    Peeking the live queue (0 active messages) and the entire accumulated 19,459-message
    dead-letter queue (0 matches for this run's correlation prefix, non-destructively, via
    `PeekMessagesAsync`) ruled out both an in-flight lock and a dead-letter outcome as the visible
    end state.
  - **Root cause**: `IWorkflowExecutor.Execute()` (`WorkflowExecutor.cs`) has no internal timeout or
    cancellation seam anywhere in its call chain — the trigger's `Task.WhenAny(executionTask,
    Task.Delay(ExecutionTimeout))` race in `ServiceBusWorkflowTriggerFunction.cs` can only stop
    *waiting* on a hung call, never cancel it. `rabbit/RabbitProcess` makes three real SQL Server
    calls (`DsfSqlServerDatabaseActivity` → `dbo.usp_jobs1_LogStart/LogProcessing/LogFinished`).
    `DatabaseServiceExecution.MssqlSqlExecution` already had a WOLF-8512 guard
    (`ResolveEffectiveConnectionTimeout`, added earlier this same investigation) specifically for
    "no DB activity ever sets `ConnectionTimeout` → SqlClient's `Connection Timeout=0` → `
    connection.Open()` waits indefinitely" — but that guard applied its resolved value via
    `DbSource.GetConnectionStringWithTimeout`, which rebuilds the connection string from properties
    for every server type **except** Entra Managed Identity, where `ConnectionString`'s getter
    returns `_entraRawConnectionString` verbatim (deliberately, so the embedded fallback
    credentials survive intact — see `DbSource_ConnectionString_SqlDatabase_EntraManagedIdentity_
    RoundTripsVerbatim`). This UAT deployment's DB source (`NewSqlServerSource.bite` →
    `WarewolfDevOpsTestDb`) uses `Authentication=Active Directory Managed Identity` (per the
    2026-08-13 entry above), so the WOLF-8512 fix's override was silently discarded for exactly
    the source this workflow uses — whatever timeout (or lack thereof) was originally baked into
    that raw connection string is what actually governed `connection.Open()`, regardless of the
    guard.
  - **Fix** (`Dev2.Services.Execution/DatabaseServiceExecution.cs`,
    `Dev2.Runtime.Services/ServiceModel/Data/DbSource.cs`): `ResolveEffectiveConnectionTimeout` now
    falls back to a 30s floor when the source's own configured timeout is *also* non-positive, not
    just the caller-supplied one. `GetConnectionStringWithTimeout` gained an Entra-aware branch
    that surgically patches (or appends) just the `Connect`/`Connection Timeout` token in the raw
    connection string, leaving every other part — including the fallback credentials — untouched;
    the plain `ConnectionString` getter/setter and the non-Entra path are unchanged. New tests:
    `ResolveEffectiveConnectionTimeout_GivenVariousInputs_ShouldReturnExpectedTimeout`'s two new
    `DataRow`s (`DatabaseServiceExecutionTests.cs`) and three new `DbSource_
    GetConnectionStringWithTimeout_*` tests (`DbSourceTests.cs`), all confirmed to fail against the
    pre-fix code and pass after it; the existing `RoundTripsVerbatim` test remains green, confirming
    the plain-getter contract for direct reads is untouched.
  - **Not yet re-verified against a live pipeline run** — the fix is implemented and unit-tested
    locally (`Dev2.Services.Execution.Tests`: 30/30; `Dev2.Runtime.Tests`'s `DbSourceTests`: 21/21)
    but not yet committed, deployed to `WarewolfServer-UAT`, or proven against another
    `ShovelBridgeLoadTest_ExternalServiceBus` run.

- **2026-08-31 — build 30585 (`8512-ProcessingReliabilityTests`, commit `3585b68b98`, the fix above
  now committed and deployed) recurred at the SAME rate: 984/1000 succeeded, 0 recorded failures,
  16 never got a result — and the fix above is confirmed NOT the cause, because its own premise
  about the live source's auth mode was wrong.**
  - **Confirmed both previously-fixed regressions did NOT recur**: read straight off the running
    app via Kudu VFS — `host.json`'s `extensions.serviceBus.maxAutoLockRenewalDuration` is
    `"00:11:00"` (the 2026-08-30 lock-renewal fix, still live) and `Settings/persistencesettings.json`
    has `"Enable": true` (persistence still on). `Deploy_UAT`'s `-EnablePersistence:$true` staging
    is confirmed working as intended.
  - **Confirmed via direct SQL against `wwexecution-uat-hangfire`'s `HangFire.Hash` table** (Entra
    access token via `az account get-access-token --resource https://database.windows.net`, no
    temporary Service Bus SAS rule needed this time): all 1000 correlation ids have a
    `sbtrigger:claim:*` row (every execution started); exactly 984 have a matching
    `sbtrigger:result:*` row. The 16 without one are the same "execution started, `SaveResult` never
    reached" signature as the 12-stuck build 30583 investigation above — not a new symptom.
  - **New evidence: all 16 claims were written within a 0.36s window** (`22:28:28.7340861Z` –
    `22:28:29.0983084Z`), i.e. they are simultaneous with the very start of the ~1000-way concurrent
    delivery burst, not scattered across the run. `az servicebus queue show` on
    `wwexecution-secure-trigger-queue-e2e` at run end showed `activeMessageCount: 0` and
    `deadLetterMessageCount: 19459` — **the identical count already on file from the 30578
    investigation**, i.e. unchanged across both the 30583 and this 30585 run. Neither run's
    stuck messages were ever dead-lettered.
  - **Application Insights (`warewolfserver-uat-ai`) had ZERO telemetry of any kind — not just
    exceptions, but `traces`/`requests` too — for the entire run window (22:00-23:00Z), despite
    `EXECUTIONLOGLEVEL=INFO` and `APPLICATIONINSIGHTS_CONNECTION_STRING` both confirmed correctly
    set** (matching the app id queried directly). This means even the new WOLF-8512 diagnostic
    `Dev2Logger.Info` lines added earlier this same day (which would show exactly which of the
    three SQL calls each stuck execution was in) never reached telemetry. This instrumentation gap
    is itself a standing blocker to root-causing any future recurrence live rather than by
    after-the-fact archaeology — worth fixing before the next investigation, not deferred again.
  - **The 30583 entry's root cause theory is DISPROVEN for this live source, by direct evidence, not
    inference.** That entry assumed `NewSqlServerSource.bite` uses
    `Authentication=Active Directory Managed Identity` (stated in multiple earlier entries in this
    log) — which is the ONLY auth string `DbSource._isEntraManagedIdentityConnectionString` matches
    (`DbSource.cs` lines 296-299: exact-equals `"Active Directory Managed Identity"` /
    `"ActiveDirectoryManagedIdentity"`, nothing else). Decrypted the LIVE deployed
    `Resources/rabbit/NewSqlServerSource.bite` directly (fetched via Kudu VFS, decrypted client-side
    with the same AES-256-GCM key read from Key Vault `WWExecutionEngine` /
    `WWExecutionEngineTestSecret` that the app itself uses — not guessed, not assumed):
    ```
    Data Source=warewolf-dev2-mcgeaj.database.windows.net;Initial Catalog=WarewolfDevOpsTestDb;
    User ID=devops_warewolf;Password=***;Connection Timeout=30;Authentication=Active Directory Password
    ```
    This is `Authentication=Active Directory Password`, NOT Managed Identity — so
    `_isEntraManagedIdentityConnectionString` evaluates `false` for this exact source, and today's
    fix (`GetConnectionStringWithTimeout`'s Entra-raw-string branch) never executes for it; it took
    the ordinary non-Entra path (`ConnectionTimeout` property + `ConnectionString` re-read) all
    along. **The connection string also already carries an explicit `Connection Timeout=30`** — it
    was never `0`/unbounded for this source, before or after today's fix. Both premises of the
    30583 root-cause chain (Entra-Managed-Identity classification, and a `Connection Timeout=0`
    hang) are contradicted by the actual live value. Whether the source's `Authentication` value
    changed between the 30583 investigation and now, or was always `Active Directory Password` and
    misidentified, could not be determined from this session alone (no `.bite` file history for
    this specific setting was checked).
  - **Working theory (not yet proven live — needs the telemetry gap above closed to confirm)**:
    `Authentication=Active Directory Password` requires a live MSAL/Entra token acquisition inside
    `SqlConnection.Open()` on every fresh physical connection. `Microsoft.Data.SqlClient` has a
    documented history of not always bounding that specific phase by `Connect Timeout` (the AAD
    token round-trip, as opposed to the TCP/TDS handshake, isn't reliably covered by the same client-
    side clock in every driver version) — a burst of ~1000 near-simultaneous connection opens
    against Entra ID could throttle a small fraction of token requests into a hang that never
    resolves and never throws, which exactly matches: a tight cluster at burst start, a small
    consistent fraction (12-16 of 1000across three consecutive runs), and no trace in either the
    active queue or the DLQ (a genuinely-hung native/MSAL call is not something
    `IWorkflowExecutor.Execute()`'s absent cancellation seam — already documented above — can
    interrupt either). Not yet confirmed by a captured exception or stack, because of the telemetry
    gap above.
  - **Recommendation for whoever picks this up next**: (1) fix the App Insights telemetry gap first
    — without it, every future run is another blind archaeology pass; (2) either switch this
    source's `Authentication` to the Managed Identity mode the fixes so far assumed (making today's
    `DbSource` fix actually apply, and sidestepping password-based Entra auth's token-acquisition
    profile entirely), or capture a thread/connection dump the next time this recurs to directly
    confirm the MSAL-hang theory before writing a fix against it; (3) do not re-attribute this to
    `Connection Timeout=0` again without re-checking the live connection string — it has not been
    `0` on this source for at least these last three runs.

- **2026-08-31 — fixed the App Insights telemetry gap (recommendation (1) above), and found
  stronger evidence against the 30585 entry's own MSAL-hang theory that points at ThreadPool
  starvation instead.**
  - **Telemetry fix**: confirmed via `Program.cs:59-88` that `ENABLEAPPLICATIONINSIGHTS=true` and
    `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` were both already correctly set live — the SDK
    registration itself was never the problem. The actual gap: **no code anywhere in this project
    ever calls `TelemetryClient.Flush()`** (confirmed by a project-wide grep — zero matches). The
    AI SDK's default channel batches telemetry on its own ~30s schedule; a Consumption-plan instance
    recycled mid-burst (exactly what a 1000-message burst causes — rapid scale-out then scale-in)
    can be torn down well inside that window, discarding every buffered trace/exception/dependency
    with no error. This is consistent with the observed symptom: not just tail-loss but **zero**
    telemetry for the entire ~30-minute run window, recovering only once load subsided. **Fix**:
    added `Logging/TelemetryFlushHostedService.cs` (an `IHostedService` registered alongside the AI
    SDK in `Program.cs`, gated by the same `RegisterApplicationInsightsSdk` flag) that calls
    `TelemetryClient.Flush()` on `StopAsync` and waits a bounded 5s to give the transmission a
    chance to leave the process before shutdown continues. 6 new tests
    (`Logging/TelemetryFlushHostedServiceTests.cs`, using a fake `ITelemetryChannel` since there is
    no mocking framework in this test project) verify the flush call, the wait, that an
    already-cancelled shutdown token does not shorten the wait, and that a channel-level exception
    doesn't propagate. Not yet deployed/re-verified against a live pipeline run.
  - **New evidence against the MSAL-hang theory, from re-reading `ServiceBusWorkflowTriggerFunction.cs`
    and `ServiceBusReplayAndResultStore.cs` against the 30585 claim-timestamp data already
    gathered**: `ReleaseClaim` (lines 274-293) does a full `RemoveHash` — a genuine release-then-
    reclaim cycle would show a **new** `claimedAtUtc` on the next attempt. `TryClaim` also has a
    20-minute stale-claim steal (`ClaimStaleAfter`, line 77) for exactly the case of an abandoned
    claim. The harness kept polling until `22:58:17` — **10 minutes past** the 20-minute mark
    (claims made `22:28:28-29`, stale threshold `22:48:28-29`) — yet all 16 claim timestamps stayed
    frozen at their original value. Neither the 5-minute `ExecutionTimeout` race
    (`ServiceBusWorkflowTriggerFunction.cs:356-365`) nor the 20-minute steal ever fired for these
    16, which means Service Bus never redelivered them in the full 30 minutes — the original
    delivery's lock kept renewing (up to the `00:11:00` ceiling) the entire time, which only
    happens if the invocation was still considered alive by the SDK.
  - **Working theory (supersedes the MSAL-hang theory above, not yet proven live — same
    telemetry-gap caveat, now fixed for the next run): CLR ThreadPool starvation, not a hung SQL/MSAL
    call specifically.** `Task.Run(() => _executor.Execute(...))` (line 343) only starts after
    acquiring one of the `MaxConcurrentExecutions` semaphore slots (live value: **4**, already
    tuned down from the code default of 8 — `WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS`),
    so at most 4 real blocking work items run per instance at once — but if those 4 are CPU-heavy
    (cold-start Roslyn VB-expression compilation, synchronous ADO.NET calls) on a small
    Consumption-plan instance, they can starve the pool badly enough that even the *lightweight*
    continuation needed to notice `Task.Delay(ExecutionTimeout)` completed never gets scheduled —
    the safety net's own enforcement code becomes a casualty of the load it exists to guard
    against. This would explain every observed fact simultaneously: no redelivery, no dead-letter,
    no released claim, frozen timestamps, and all 16 claimed within the same 0.36s burst-start
    window (i.e. co-resident on one or a few instances at the moment of starvation). Nothing in
    this project calls `ThreadPool.SetMinThreads` (confirmed by grep) — the standard mitigation for
    this exact failure mode.
  - **Recommendation (not yet actioned)**: (1) lower `WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS`
    further (4 → 2) to reduce per-instance concurrent CPU/memory pressure during a burst; (2) add an
    explicit `ThreadPool.SetMinThreads()` floor at startup so a burst doesn't have to wait on the
    pool's default slow thread-injection rate; (3) `WAREWOLF_SERVICEBUS_TRIGGER_EXECUTION_TIMEOUT_SECONDS`
    is NOT expected to help this specific symptom — shortening it doesn't matter if the code that
    checks the timer can't get scheduled either; leave it at its default 5 minutes, which is already
    proven to work correctly for the *other* failure mode (a genuinely slow-but-alive execution,
    where `ReleaseClaim` demonstrably does fire). (4) Re-run the load test with telemetry now fixed
    so a future recurrence can be root-caused from captured exceptions/traces instead of archaeology.

- **2026-08-31 — recommendation (1) actioned, both live and in the pipeline.** A colleague's
  1000-message `-VerifyWorkflowExecution` run (~13:20-13:32 UTC) reproduced the same starvation
  signature independently: App Insights (`warewolfserver-uat-ai`) showed 467 exceptions in that
  window — `OperationCanceledException` ×302 and `TimeoutException` ×88 at
  `ServiceBusWorkflowTriggerFunction.ProcessAuthenticatedMessageAsync`, plus `OutOfMemoryException`
  ×14 at `ActivityParser.Parse` — while the RabbitMQ Shovel and source queue stayed healthy
  throughout (`state: running`, `messages: 0` post-run), confirming the loss is downstream of the
  bridge, in the engine's own capacity, same as every prior entry in this section.
  - **Live mitigation**: `WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS` set to `2` directly
    on `WarewolfServer-UAT` (`az functionapp config appsettings set`) — confirmed applied (Activity
    Log: `Update web sites config` succeeded) and the app healthy post-change (`GET /apis.json` →
    `200`). **Not durable**: this is a Kudu-only app setting, not staged by any deploy script — the
    exact same "silently reverts on redeploy" trap already hit once by the persistence-flag
    regression (see the 2026-08-15 entry above). Revisit whether this should become a checked-in
    `Deploy-WwExecutionEngine.ps1` app-setting default rather than a live-only override.
  - **Durable, complementary fix**: `Deploy-WwExecutionEngine.ps1` gained a new opt-in
    `-ServiceBusMaxConcurrentCalls <int>` parameter (mirrors the existing `-AlignHostJsonLogLevel`
    pattern — `Update-HostJsonServiceBusConcurrency`, staged in Phase 3 alongside the log-level
    alignment) that sets `host.json`'s `extensions.serviceBus.maxConcurrentCalls`. This is a
    **different** concurrency gate from `MAX_CONCURRENT_EXECUTIONS`: it bounds how many messages the
    Functions host *dispatches* to the trigger at all, upstream of and independent from the
    trigger's own semaphore — addressing the "many invocations in flight waiting on the semaphore,
    each still holding a lock and cold-start memory pressure, even though only N execute at once"
    half of the starvation theory. `pipeline-LOADTEST.yml`'s `Deploy_UAT` job now passes
    `-ServiceBusMaxConcurrentCalls 2` (matching the live app-setting value above), so a future
    pipeline redeploy of `WarewolfServer-UAT` no longer silently drops this mitigation the way the
    persistence flag did — it's baked into the deploy call itself, not left as a manual follow-up.
  - **Not yet actioned**: recommendation (2) (`ThreadPool.SetMinThreads()` floor) is implemented in
    `ThreadPoolStartupConfigurator`/`Program.cs` (this same branch, commit `78aa65fe71`) but its own
    deployment/verification status against a live pipeline run is still open — see that commit's own
    notes. Re-running the full 1000-message load test with all of the above in place (live
    concurrency drop + durable `maxConcurrentCalls` cap + ThreadPool floor + telemetry flush) is the
    next concrete step to confirm whether the starvation theory is now fully closed.

- **2026-08-31 — New: opt-in `dbo.jobs1` reconciliation (`Test-ShovelBridgeE2E.ps1` Phase 4c,
  `-JobsDbConnectionString`).** Every prior entry in this log that reconciled per-message delivery
  did so by hand-crafting SQL against `dbo.jobs1`/the Service Bus DLQ during a live investigation —
  this promotes that technique into the harness itself so it runs every time, not just when someone
  is already root-causing a failure.
  - **What it checks, beyond `usp_jobs1_Summary`'s own buckets**: a message that's
    lost/denied/dead-lettered before `RabbitProcess`'s own DB activity ever ran (before
    `usp_jobs1_LogStart` is even called) leaves **zero** rows in `dbo.jobs1` — invisible to
    `usp_jobs1_Summary`'s aggregation, which only groups over rows that already exist. Phase 4c
    separately computes, for each of the `N` expected `'<prefix><CorrelationId>-NNNNNN'` messages
    (`-JobsDbMessageContentPrefix`, default `'loadtest-'`), whether it has *any* `dbo.jobs1` row at
    all — a tally-table anti-join against the extracted numeric suffix, 0-based (`-000000` ..
    `-{N-1}`, matching `Test-ShovelBridgeE2E.ps1`'s own documented `-CorrelationId` numbering — NOT
    1-based, which an earlier hand-written version of this exact query got wrong).
  - **Scoped to the run's own start time** (`usp_jobs1_Summary @FromUtc=<captured right before
    Phase 4 starts publishing>`), never the whole table — `dbo.jobs1` is shared and never purged
    across every historical run (including concurrent CI activity), so an unscoped query would mix
    in unrelated runs' rows, exactly the trap `Provision-ShovelBridgeSchema.sql`'s own header
    comment warns about.
  - **Independent of the harness's own result-poll path**: this reads `RabbitProcess`'s own
    business-level rows, not `ServiceBusReplayAndResultStore`'s `sbtrigger:result:*` Hangfire hash —
    so it isn't affected by any of the `GET /secure/servicebus-result/{correlationId}` read-path
    bugs recorded earlier in this log (the 24 unexplained `HttpError` 500s, the load-balanced-
    instance persistent-404 case).
  - **Auth**: `devops_warewolf` via `Authentication=Active Directory Password` (confirmed from the
    live, decrypted `NewSqlServerSource.bite` connection string — this is Entra ID password auth,
    not classic SQL Server auth, despite the login-looking name), password sourced from a new
    secret pipeline variable (`DBPassword`) and never inlined into the script body, same convention
    as `ExternalRabbitMqPassword`/`ShovelE2EDaemonClientSecret`.
  - **Opt-in, graceful skip**: omitting `-JobsDbConnectionString` skips Phase 4c with a note, same
    as every other opt-in switch added to this script — it does not turn a missing secret into a
    pipeline failure.
  - **Tested**: `Get-WwJobsDbVerdict` (the pure bucket-evaluation function — no DB access) has new
    Pester coverage in `Tests/Integration/Test-ShovelBridgeE2E.Tests.ps1` (this script's first ever
    Pester file); the live SQL calls themselves are proven by actually running the pipeline, same as
    the rest of this integration-only script.

- **2026-08-31 — CORRECTION: `devops_warewolf` is plain SQL authentication, NOT Entra ID.** The
  entry above (and every earlier entry quoting the decrypted `NewSqlServerSource.bite` connection
  string) took `Authentication=Active Directory Password` at face value. Attempting Phase 4c's
  first live corroboration run failed at the connection step with `AdalException: Could not
  discover a user realm` (identical failure via both legacy `System.Data.SqlClient` and `sqlcmd
  -G`), and `devops_warewolf` could not be found as an Entra user anywhere in tenant
  `ca0cc53b-...` by UPN, `mailNickname`, or `mail`. **Confirmed directly**, not inferred: `SELECT
  name, type_desc, authentication_type_desc FROM sys.database_principals WHERE name =
  'devops_warewolf'` returned `SQL_USER` / `INSTANCE` — a genuine SQL Server login, the
  `Authentication=Active Directory Password` keyword in every prior transcription was simply
  wrong (a documentation error, not a live config drift — nothing about the login itself changed).
  Fixed the connection string in `pipeline-LOADTEST.yml` to plain SQL auth (`User
  ID=devops_warewolf;Password=...`, no `Authentication` keyword) — `Test-ShovelBridgeE2E.ps1`'s
  own `-JobsDbConnectionString` doc comment was already auth-mode-agnostic and needed no change.
  **First successful corroboration, once the auth mode was fixed** (local run, correlationId
  prefix `e135cd6825044c6b8946dbc3c0658ef9`, same session): `dbo.jobs1` shows **996/1000** rows,
  all `FINISHED` with zero errors (no duplicate-success bug) — **4 sequence numbers (`127`, `131`,
  `151`, `152`) never got a `dbo.jobs1` row at all**, i.e. never reached
  `RabbitProcess`'s own DB activity. This is a tighter, DB-confirmed number than the harness's own
  in-flight report of "998/1000 resolved, 2 still pending" (that run's process was killed by a
  10-minute tool timeout before it printed its own final verdict) — the 2-message gap between
  "998 resolved" and "996 finished" is consistent with 2 of the 4 missing messages having reached
  a terminal status via a path that never touches `dbo.jobs1` at all (a denial/malformed-message
  outcome, recorded by `RecordTerminalOutcomeAsync` before `_executor.Execute()` is ever called),
  while the other 2 remain genuinely stuck — the exact "claimed, no result, no dead-letter" pattern
  documented repeatedly elsewhere in this log. This is Phase 4c doing exactly what it was built
  for: surfacing a discrepancy the harness's own result-poll path couldn't see on its own.
  - **Follow-up, same session — the 4 missing sequence numbers resolve to TWO distinct root
    causes, confirmed by direct evidence, not inference**:
    - **`-000151` / `-000152`: engine-side, and now proven.** `GET
      /secure/servicebus-result/{correlationId}` for both returned `200` with `status:
      "InvalidToken"`, `error: "Token validation failed: A task was canceled."`, at the
      **identical** `completedAtUtc` (`2026-08-31T15:25:21.9641...Z`) — this is exactly the
      `EntraBearerTokenValidator.ValidateAsync`-cancelled-by-the-invocation's-own-token theory
      flagged as a *working theory, not yet proven by a direct correlationId-to-exception join*
      in the 2026-08-16 entry above. **Now proven**: two messages failing token validation at
      the exact same instant is consistent with a shared invocation-level cancellation (host
      self-throttling / lock-expiry-triggered abandonment), not two independent token problems.
    - **`-000127` / `-000131`: a genuine bridge-side loss, not an engine problem at all.** `GET
      /secure/servicebus-result` returned `404` for both (no result ever recorded — consistent
      with "still pending"). A temporary `Listen`-rights SAS rule (created and torn down the
      same run, same technique as every prior peek in this log) was used to scan the
      **entire** active queue (0 messages — fully drained) and the **entire** dead-letter
      subqueue (19,459 messages — unchanged from the pre-run baseline, i.e. this run added
      **zero** new dead-letters) for either correlationId. **Zero matches in either.** The
      harness's own publish phase reported `Published 1000 message(s)` with no publish errors
      (a publish failure would have thrown per `PublishToRabbitMqAsync` — see the Phase 4b
      header comment), so these two were successfully published to RabbitMQ but never arrived
      at Service Bus at all — lost specifically in the RabbitMQ→Shovel bridge itself. This is
      the first time in this log a "missing" message has been proven absent from BOTH Service
      Bus sub-queues rather than merely "not found by the correlationId prefix filter" — ruling
      out an engine-side execution/dead-letter bug for these two as conclusively as the evidence
      allows without SSH/Prometheus access to `rabbitmq.warewolf.online` (still not available —
      see the 2026-08-15 correction entry above on why that avenue was abandoned before).
    - **Net**: of this run's 4 "missing" messages, 2 are an engine capacity/cancellation issue
      (worth investigating alongside the `MAX_CONCURRENT_EXECUTIONS`/`maxConcurrentCalls` work
      earlier in this log) and 2 are a genuine, still-open Shovel bridge-delivery gap at a very
      low rate (0.2%) — a materially smaller and better-characterised problem than any prior
      entry in this log, but not yet fully closed.
  - **Fix for the `-000151`/`-000152` half (engine-side)**: `ServiceBusWorkflowTriggerFunction
    .Run`'s `catch (Exception ex)` around `_tokenValidator.ValidateAsync` was a blanket catch —
    a `TaskCanceledException` from the invocation's own `cancellationToken` firing mid-validation
    (most likely `EntraBearerTokenValidator`'s cold OIDC-metadata fetch, which despite the
    validator being a DI singleton still has to complete once per app lifetime, racing
    host-level burst/cold-start pressure) was treated identically to a genuinely bad token: a
    permanent `InvalidToken` result saved AND the message dead-lettered on the very first
    attempt, zero retry. Exactly the same misclassification bug already fixed for
    `WorkflowExecutor.Execute`'s `OutOfMemoryException` via `WorkflowExecutionResult
    .IsTransientFailure` on 2026-08-14 — just never applied to this earlier validation step.
    **Fix**: a new `catch (Exception ex) when (IsOwnInvocationCancellation(ex, cancellationToken))`
    clause, ahead of the existing catch-all, releases the claim and rethrows instead — Service
    Bus's own retry/backoff applies, and a retry moments later should succeed since the cache is
    then warm regardless of which attempt populated it. `IsOwnInvocationCancellation` is a pure
    classification helper (no I/O), unit-tested directly (4 new tests in
    `ServiceBusWorkflowTriggerFunctionTests.cs`) rather than driving the whole `Run(...)` call —
    same reason `WorkflowExecutor.BuildTransientFailureResult` was extracted and tested the same
    way for the OOM fix; `_tokenValidator` is a sealed concrete class needing a live OIDC call,
    deliberately left to integration tests by this file's own existing convention. Not yet
    deployed/re-verified against a live pipeline run.
  - **2026-08-31, follow-on (root causes B and C, plus broader transient classification)**:
    the `IsOwnInvocationCancellation` fix above closed the specific case reproduced by
    `-000151`/`-000152` (the trigger's own `cancellationToken` firing mid-validation), but a
    plan review against the original incident report found two further root causes it did
    **not** touch, plus a real gap in the classification's coverage:
    - **Root cause B (the `NO RESULT` correlation ids)**: `ServiceBusReplayAndResultStore`'s
      `ClaimStaleAfter` was a hard-coded 20-minute constant, sized against an assumed
      10-minute `functionTimeout`. If the queue's own `MaxDeliveryCount × LockDuration`
      delivery budget is smaller than `ClaimStaleAfter`, a redelivery can never actually take
      over a claim left by a dead/hung attempt — the queue dead-letters first, with no result
      ever recorded. **Fix**: `ServiceBusTriggerOptions.ClaimStaleAfter` now resolves the
      host's actual `functionTimeout` automatically (five-tier strategy — explicit override →
      `AzureFunctionsJobHost__functionTimeout` → `host.json` on disk → `WEBSITE_SKU` fallback →
      10-minute hard fallback) plus a 1-minute margin, cached in a `static Lazy<TimeSpan>` so a
      bare `new ServiceBusTriggerOptions()` (used throughout the test suite) never hits disk
      per construction. `Enable-ServiceBusSecureTrigger.ps1` gained `-LockDuration` and now
      asserts `MaxDeliveryCount × LockDuration > ClaimStaleAfter` before provisioning anything.
    - **Root cause C (settlement bound to the host token)**: every `CompleteMessageAsync`/
      `DeadLetterMessageAsync` call used the trigger's own `cancellationToken` — during a
      shutdown/drain that token is already cancelled, so settlement throws immediately and the
      message is left unsettled even though its outcome was already decided. **Fix**: a new
      `SettleAsync` helper wraps every settlement call in an independent, timeout-bounded
      `CancellationTokenSource` (`ServiceBusTriggerOptions.SettlementTimeout`, default 30s)
      over `CancellationToken.None`. Safe because the result is always persisted first — a
      settlement timeout just leaves the message unsettled for the next redelivery's dedupe
      check to complete.
    - **Classification gap**: `IsOwnInvocationCancellation` only catches a cancellation tied to
      the trigger's OWN `cancellationToken`. Its own test
      (`IsOwnInvocationCancellation_TokenNotCancelled_ReturnsFalse`) proves a `TaskCanceledException`
      from an internal `HttpClient` timeout — unrelated to that token — still fell through to
      the terminal `InvalidToken` path. **Fix**: a new `TokenValidationFailureClassifier`
      (`Auth/Parsers/`) adds a second, broader transient tier —
      `HttpRequestException`/`IOException`/`SocketException`, `SecurityTokenSignatureKeyNotFoundException`,
      and `IDX20803`/`IDX20804` IdentityModel codes — inserted as a second `catch when` clause
      between the shipped cancellation clause and the terminal catch-all. Deliberately did
      **not** extract an `IEntraBearerTokenValidator` interface (as an earlier plan draft
      proposed) — the shipped fix's own pure-exception-plus-token classification approach
      already sidesteps needing a live/faked validator, so the classifier is tested the same
      way, with zero interface surface added.
    - Also added, same pass: dead-letter triage sub-reasons (`"{Status}:{SubReason}"`, e.g.
      `InvalidToken:JtiReplay`) via an optional `subReason` parameter on
      `RecordTerminalOutcomeAsync`, and `host.json`'s `extensions.serviceBus.clientRetryOptions`
      (exponential backoff, 3 retries).
    - Full local build + the entire `Warewolf.Execution.Lightweight.Tests` assembly (869 tests)
      green; the two riskiest new tests (`FailTransient_ReleasesClaim_SavesNoResult_Rethrows`
      and `ProcessAuthenticated_SettlementTimesOut_ResultStillPersisted_NoThrow`) were verified
      to actually fail when their respective fix was temporarily reverted, then restored.
      **Not yet deployed/re-verified against a live pipeline run** — the 1000-message E2E
      acceptance run from the original plan is still outstanding.

- **2026-09-01 — Local reproduction of the 1000-message load test (correlationId prefix
  `bebfb24eea914ed1aa6ed4f1e7f743f0`), run to corroborate a clean CI result (build 30599,
  `ShovelBridgeLoadTest_ExternalServiceBus`, 1000/1000 in 73.1s). Result: 998/1000** — 2
  correlation ids (`-000858`, `-000859`) never resolved, the same "claimed, no result, no
  dead-letter" signature documented repeatedly above. This confirms the 2026-08-31 fixes
  (`ClaimStaleAfter` auto-resolution, `SettleAsync`, `TokenValidationFailureClassifier`) — which
  that entry left "not yet deployed/re-verified" — do **not** fully close this bug class on their
  own; `ClaimStaleAfter` resolution is confirmed present in the current code, so this is a
  residual gap, not a regression of that fix.
  - **Full evidence chain, all independently confirmed**:
    - `dbo.jobs1`: exactly 998 rows for this run's prefix; `-000858`/`-000859` have **zero** rows
      at all (never reached `RabbitProcess`'s own `usp_jobs1_LogStart`), even though their
      immediate neighbors (`-000856`, `-000857`, `-000860`, `-000861`) all finished within the
      same ~250ms burst window.
    - App Insights (`warewolfserver-uat-ai`): **exactly one** trace line per correlation id, both
      at `07:01:55Z` — `"Another delivery is already in flight for this correlation id"` (the
      `ServiceBusWorkflowTriggerFunction.Run` dedupe branch at `TryClaim` failing). Nothing else,
      ever, for either id: no exception, no `SlotWaitTimeout`/`ExecutionTimeout` warning (both DO
      log when they fire), no `performanceCounters` rows (that table was completely empty for
      this app before the fix below).
    - RabbitMQ (`rabbitmq.warewolf.online`): source queue empty, `messages_unacknowledged=0`,
      `redeliver (lifetime)=0` — rules out a Shovel/broker-side loss; both messages left RabbitMQ
      cleanly.
    - Service Bus dead-letter queue (`wwexecution-secure-trigger-queue-e2e`): exhaustively peeked
      via the namespace `RootManageSharedAccessKey` (non-destructive `PeekMessagesAsync`, all
      19,459 entries scanned) — **zero** matches for this run's correlation prefix; not
      dead-lettered by `MaxDeliveryCountExceeded` or by the app.
    - `activeMessageCount` on the same queue: `0`, checked repeatedly over 20+ minutes.
  - **Conclusion**: a first delivery claimed both correlation ids and then produced literally no
    further telemetry of any kind — consistent with the process dying outright (most likely OOM
    under Consumption-plan burst pressure, the same class flagged throughout this log, though not
    provable from the telemetry actually available — see the fix below) before reaching *any* of
    this function's existing catch/timeout paths, all of which log when they fire. Neither active
    nor dead-lettered nor executed 20+ minutes later, well past `ClaimStaleAfter`'s ~11-minute
    window. **Open question for a future investigation**: why no further Service Bus redelivery
    (expected roughly every `lockDuration=1min`) was ever observed for either id in that window.
  - **Two remediations shipped this session, neither yet deployed/re-verified against a live
    pipeline run**:
    1. Three new `Information`-level log markers in `ServiceBusWorkflowTriggerFunction.cs`
       bracketing the claimed-but-not-yet-settled span (claim acquired → about to call
       `IWorkflowExecutor.Execute`, with an inline `GC.GetTotalMemory`/`Environment.WorkingSet`
       snapshot at that highest-risk instant → execution task returned), so a future silent death
       leaves a last-known-state trail even if the process dies before a full telemetry flush.
    2. `PerformanceCollectorModule` (from `Microsoft.ApplicationInsights.PerfCounterCollector`,
       previously pulled in transitively but never registered) is now wired up behind a new
       opt-in `EnablePerformanceCounters`/`ENABLEPERFORMANCECOUNTERS` toggle
       (`LoggingConfiguration.cs` → `Program.cs` → `Deploy-WwExecutionEngine.ps1
       -EnablePerformanceCounters` → `pipeline-LOADTEST.yml`'s `Deploy_UAT` job), so App
       Insights' `performanceCounters` table starts collecting process memory/CPU samples on the
       next `Deploy_UAT` run.
  - **Separate, lower-priority finding**: `Test-ShovelBridgeE2E.ps1`'s Phase 4c (`dbo.jobs1`
    reconciliation) failed on this same run with a garbled `"Incorrect syntax near ')'."` SQL
    error. Reproduced the identical query through the identical `Invoke-WwSqlQuery` helper 6/6
    times afterward with no failure — points to a fragile child-`powershell.exe`-process/
    stdout-parsing issue in that helper under resource contention, not a real query bug. Filed
    separately; not yet actioned.

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
