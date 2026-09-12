# Service Bus Trigger Architecture — Transport-Trusted, In-Process

## Purpose

Trigger Warewolf workflow executions directly from an Azure Service Bus message,
in-process (no broker-to-HTTP hop). Trust is **transport-only**: a message is
authorized to execute simply because it arrived on the configured queue — Azure RBAC
governing who may send to that queue is the entire authorization decision. There is no
per-message caller token, no cryptographic validation step, and no per-workflow
permission check for this trigger.

This is a deliberate, security-relevant design choice (superseding an earlier,
now-removed per-caller-token design — see "History" below), not an oversight: see
"Threat model coverage" for exactly what this does and does not protect against.

## Relationship to the existing Service Bus integration

Another Service-Bus-related component already exists in this repo and is **unchanged**
by this trigger:

| | **Shovel Bridge** (`Warewolf.Execution.ServiceBusWorker`) | **This trigger** (`Warewolf.Execution.Lightweight`) |
|---|---|---|
| Queue | `wwexecution-queue` (default) | `wwexecution-secure-trigger-queue` (default, distinct queue) |
| Identity on the message | None — worker's own Managed Identity calls Lightweight's HTTP route | None — same shape, no per-caller identity |
| Authorization granularity | Whatever the worker's app-only identity is allowed (coarse, system-level) | None — every message on the queue executes unconditionally |
| Transport | Message → separate Function App → HTTP call → Lightweight | Message → **in-process** trigger inside Lightweight (no HTTP hop) |
| Use case | Bridging existing RabbitMQ producers / system-to-system integration jobs | A Service-Bus-native producer needs the engine to execute a named workflow directly, in-process |

Both trigger paths now share the same transport-only trust shape; they differ only in
whether execution happens via an HTTP hop (Shovel Bridge) or in-process (this trigger).
Both can run side by side against the same Lightweight deployment; they use different
queues and are independently enabled/disabled.

## The one trust boundary

Who can put a message on the queue / who can receive from it. Enforced entirely by
Azure — Service Bus namespace RBAC (`Azure Service Bus Data Sender` / `Data Receiver`)
via Managed Identity, and (optionally) namespace network rules. The trigger's
listen-side connection uses the Functions
[identity-based connection](https://learn.microsoft.com/azure/azure-functions/functions-bindings-service-bus)
convention (`ServiceBusConnection__fullyQualifiedNamespace`), never a shared-access-key
connection string, so no broker secret exists to leak.

There is deliberately no second, message/caller-level boundary: once a message passes
the transport boundary, its content is trusted outright. Controlling who holds "Azure
Service Bus Data Sender" on the configured queue/namespace is the only access control
that matters for this trigger — scope it as tightly as the deployment allows.

## History — why there is no per-caller authorization here any more

An earlier pass at this trigger validated a per-caller Entra bearer token carried in
each message (`Authorization`/`jti` application properties), evaluated through the same
`IWorkflowPolicyMatcher.Evaluate(...)` singleton the HTTP `/secure` and `/services`
routes use, so the authorization decision stayed centralized in one component rather
than drifting across a second implementation. That design was replaced by the
transport-only model above: this trigger no longer calls `IWorkflowPolicyMatcher` at
all, and `EntraBearerTokenValidator`/`ServiceBusEntraAuthOptions` are no longer part of
this trigger's flow (the general HTTP `/secure` routes still use
`EntraBearerTokenValidator`/`EntraAuthOptions` for their own, unrelated per-caller
authentication — only this trigger's dedicated Service-Bus-audience path was removed).

## Topology

```
Any producer that can send to the configured queue
   │  publishes ServiceBusWorkflowMessage as the message body — no token, no caller identity
   ▼
Azure Service Bus queue (wwexecution-secure-trigger-queue, dead-lettering enabled)
   │  ServiceBusTrigger — Managed Identity listen connection (RBAC: Azure Service Bus Data Receiver)
   │  RBAC "Azure Service Bus Data Sender" on this queue/namespace is the ENTIRE trust boundary
   ▼
ServiceBusWorkflowTriggerFunction  (Warewolf.Execution.Lightweight, in-process)
   1. Parse JSON body                          → malformed          → dead-letter + record
   2. Idempotency check (CorrelationId)         → already processed  → complete, no re-execution
   3. Wait for a free execution slot (SemaphoreSlim, ServiceBusTriggerOptions.MaxConcurrentExecutions,
      default 8) → caps how many workflows this instance runs at once so a large burst is processed
      at a sustainable rate instead of overwhelming a single (typically Consumption-plan) instance's
      thread pool all at once; Service Bus's own durable queue absorbs the rest while they wait. The
      wait itself is bounded too (ServiceBusTriggerOptions.SlotWaitTimeout, default = ExecutionTimeout)
      → claim released, TimeoutException thrown, same outcome as step 4's timeout below — a slot held
      by a genuinely deadlocked execution (step 4 cannot reclaim it either) would otherwise leave a
      queued delivery waiting forever with no result, no error, and nothing ever reaching the DLQ
      (observed directly in the 1000-message ShovelBridge load test incident of 2026-08-25, even with
      step 4's timeout already in place).
   4. IWorkflowExecutor.Execute(...) in-process (ExecutingPrincipal = WorkflowClaimsPrincipal.Anonymous()
      — no per-caller identity to stamp), run on the thread pool and bounded by
      ServiceBusTriggerOptions.ExecutionTimeout (default 5 min) — Execute has no cancellation seam, so
      a timed-out execution is abandoned (left running in the background until it finishes on its own),
      not cancelled; its concurrency slot from step 3 is only released once it actually finishes, not
      merely once the timeout fires.
      ├─ timeout                  → claim released, TimeoutException thrown — SB extension retry/backoff
      │  applies, eventually dead-lettering once maxDeliveryCount is exhausted instead of the delivery
      │  waiting forever with no result ever recorded
      ├─ transient failure (IsTransientFailure, e.g. OutOfMemoryException under Consumption-plan
      │  cold-start memory pressure) → thrown, NOT recorded, NOT dead-lettered — SB extension
      │  retry/backoff applies, same as an unexpected exception below
      ├─ business/activity failure → dead-letter + record (Failed)     — non-transient, not retried
      ├─ unexpected exception      → rethrown                          — SB extension retry/backoff applies
      └─ success                   → complete message + record (Succeeded)
   ▼
IServiceBusReplayAndResultStore (Hangfire-backed when Config.Persistence is enabled, in-memory otherwise)
   ▲
   │  GET /secure/servicebus-result/{correlationId}
ServiceBusResultFunction  (ordinary HTTP function, [RequireWorkflowPermission(View)], same authorization middleware)
```

## Message contract

```json
{
  "inputs": { "Name": "FromServiceBus" },
  "correlationId": "optional-caller-supplied-id"
}
```

- There is no `workflow` field (WOLF-8517). Every message on the configured queue runs the
  SAME workflow — named by `Settings/servicebustrigger.bite`'s `WorkflowName` (see
  "Configuration reference" below), resolved directly to a `.bite` file under the engine's
  workflows directory (`WorkflowFunctionHelper.CreateRequestByName`) — no `secure.config`
  permission check happens for this trigger at all (see "Threat model coverage"); the name
  is used purely to locate the file to execute.
- `inputs` (optional) — a string map, passed through identically to how HTTP query-string
  inputs are handled today (`WorkflowFunctionHelper.CreateRequestByName`).
- `correlationId` (optional) — caller-supplied idempotency/polling key. If omitted, the
  trigger falls back to the Service Bus message's native `CorrelationId` property, then
  `MessageId`.

There is no caller-identity field in this contract, on the message body or its
application properties — trust is decided entirely by arrival on the configured queue,
before the message is even parsed.

## Result delivery — polling only (no push)

The result of an execution is retrieved by polling `GET
/secure/servicebus-result/{correlationId}`, not pushed back to a reply queue or webhook.
This was a deliberate, explicitly-scoped-down decision (approved by the user during
design) to avoid introducing a new push-delivery mechanism (reply queue, webhook
registrations, SignalR, etc.) in this pass. A caller that needs the outcome:

1. Publishes the message with a `correlationId` it generates itself.
2. Polls the result endpoint (itself protected by the ordinary HTTP authorization
   pipeline, requiring `WorkflowPermission.View` on the workflow) until a terminal
   status is returned, or a 404 if the message hasn't been processed yet. This trigger
   only ever produces `Malformed`, `Succeeded`, or `Failed` now — `Denied`/`InvalidToken`
   are dead status values, left over from the removed per-caller-token design (see
   "History" above); removing them from `ServiceBusTriggerStatus` itself was left out
   of scope for this pass.

Result-queue / webhook push delivery is explicitly out of scope for this implementation
and is called out as a candidate follow-up.

## Backing store

`IServiceBusReplayAndResultStore` covers **correlation-id idempotency** (business-level):
the same logical request, if redelivered by Service Bus (at-least-once delivery) or
resubmitted by a naive retrying producer, is not re-executed; the previously recorded
result is returned instead. (The store's interface still exposes a jti-replay method
from the removed per-caller-token design — see "History" above — but this trigger no
longer calls it.)

When `Config.Persistence` (the engine's optional Hangfire/SQL persistence store) is
enabled, this is backed by Hangfire's `JobStorage` (hash entries), giving durability and
cross-instance consistency for free. When persistence is not configured, it falls back
to an in-process `ConcurrentDictionary` — durable and correct for a single instance only,
appropriate for local development or single-instance deployments.

**Known limitation:** Hangfire hash entries written by this store have no TTL / automatic
eviction in this pass — there is no expiry-sweep background job. This is an accepted
trade-off (unbounded growth over very long uptimes) rather than a correctness gap; a
follow-up could add a scheduled cleanup job if this becomes an operational concern.

## Configuration reference

WOLF-8516: the simple tunables below are no longer individual env vars — see the
sub-table. `WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES` is the one exception,
deliberately left as a standalone env var (its own incident-scarred, `host.json`-derived
resolution chain — see "Failure classification" below). There is no Entra tenant/audience
configuration for this trigger any more — the general HTTP path's `WAREWOLF_ENTRA_CONFIG`
is unrelated to it.

| App setting | Purpose |
|---|---|
| `ServiceBusConnection__fullyQualifiedNamespace` | Identity-based Service Bus connection (Managed Identity in Azure, developer credentials locally). No connection string / SAS key is used. |
| `WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES` | WOLF-8512: operator escape hatch overriding `ClaimStaleAfter` directly, bypassing its runtime-resolved default entirely. Normally left unset — see "Failure classification" below for how the default is inferred automatically. |
| `AzureWebJobs.ServiceBusWorkflowTrigger.Disabled` | Standard Azure Functions convention to disable the trigger entirely (e.g. when no Service Bus namespace is provisioned) with zero code changes. `ServiceBusTriggerDefinition.Load()` (see below) returns `null` without reading its config file when this is set. |

WOLF-8517: the queue name is no longer `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE` either. It, and
the workflow the trigger runs, are sourced SOLELY from `Settings/servicebustrigger.bite`
(`ServiceBusTriggerDefinition.cs`) — a plain-JSON, deploy-bundled file (no secret in it, so
never WFAES-encrypted), loaded once at cold start **before** the host is built. Its
`QueueName` is injected into configuration under the `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE`
key (`HostBuilderExtensions.ConfigureWarewolf`) so the `[ServiceBusTrigger("%...%")]`
attribute indirection resolves exactly as it did when that was a real app setting; its
`WorkflowName` is registered as a DI singleton and consumed directly by
`ServiceBusWorkflowTriggerFunction` — **the message body no longer names the workflow**
(see "Message contract" above). A missing file, or either field blank, is fatal — refuses
to start rather than silently defaulting.

**"Deploy-bundled" means exactly that — baked into the package by
`Deploy-WwExecutionEngine.ps1`'s `-ServiceBusTriggerQueueName`/`-ServiceBusTriggerWorkflowName`
(Phase 3), not written to an already-deployed app's file system afterwards.** WOLF-8517's
first pass tried the latter — `Enable-ServiceBusSecureTrigger.ps1` writing this file into a
live Function App via the Kudu VFS API (`az rest ... /api/vfs/site/wwwroot/...`) — which
cannot work whenever `WEBSITE_RUN_FROM_PACKAGE=1` (the normal zip-deploy mode this repo
uses) mounts `wwwroot` read-only. Confirmed live against `WarewolfServer-UAT`
(2026-09-12): the write failed with `UnauthorizedAccessException: Access to the path is
denied.` `Enable-ServiceBusSecureTrigger.ps1` still owns the queue/RBAC/app-setting side
(its `-TriggerQueueName` must match whatever `-ServiceBusTriggerQueueName` the deploy was
given), but no longer touches this file at all.

The remaining tunables are sourced SOLELY from `Settings/executionengine.settings.json`'s
`serviceBusTrigger` section (deploy-bundled JSON file — see `HostEnvironmentConfig.cs` and
`ServiceBusTriggerOptions.FromEnvironment`) — **no env-var fallback**, they are not App
Settings any more:

| `serviceBusTrigger` field | Purpose |
|---|---|
| `executionTimeoutSeconds` | Hard time budget for a single workflow execution before it is treated as failed and left for Service Bus's own retry/backoff (default: 300 = 5 minutes). See flow step 4. |
| `maxConcurrentExecutions` | How many workflow executions this instance runs concurrently; excess deliveries wait for a free slot rather than all starting at once (default: 8). See flow step 3. |
| `slotWaitTimeoutSeconds` | Hard bound on how long a delivery waits for a free execution slot before it is treated as failed (default: same as `executionTimeoutSeconds`). See flow step 3. |
| `settlementTimeoutSeconds` | WOLF-8512: hard time budget for a single settlement call (`CompleteMessageAsync`/`DeadLetterMessageAsync`), using an independent timeout over `CancellationToken.None` rather than the host invocation's own token (default: 30). See "Failure classification" below. |

`host.json` requires `extensions.serviceBus.autoCompleteMessages: false` so the trigger's
explicit `CompleteMessageAsync` / `DeadLetterMessageAsync` calls (via the injected
`ServiceBusMessageActions`) take effect instead of the host auto-completing the message on
successful return.

## Failure classification (WOLF-8512)

The 1000-message ShovelBridge load test surfaced two distinct root causes for
lost/misclassified results, each fixed independently. (A third, since-removed root
cause — a cancellation/network-shaped failure during per-message token validation being
misclassified as a terminal `InvalidToken` — no longer applies: this trigger no longer
validates a token at all. See "History" above.)

**A — the claim-staleness window can outlive the delivery budget.** `ClaimStaleAfter` (how long
`IServiceBusReplayAndResultStore.TryClaim` honours a claim before treating it as abandoned by a
dead/hung attempt) used to be a hard-coded 20-minute constant, sized against an assumed 10-minute
`functionTimeout`. If the queue's own `MaxDeliveryCount × LockDuration` delivery budget is smaller
than `ClaimStaleAfter`, a redelivery can never actually take the claim over — the queue exhausts
`MaxDeliveryCount` and dead-letters the message first, with **no result ever recorded**. Fixed two
ways:

- `ServiceBusTriggerOptions.ClaimStaleAfter` now defaults to the host's *actual* resolved
  `functionTimeout` + 1 minute of margin — see `ResolveHostFunctionTimeout`'s five-tier strategy
  (explicit override → `AzureFunctionsJobHost__functionTimeout` app setting → `host.json` on disk →
  `WEBSITE_SKU` plan-default fallback → 10-minute hard fallback), so it tracks a Consumption ↔ Flex
  Consumption move or a `functionTimeout` change automatically, with zero code change. The resolved
  value and which tier produced it are logged at startup (auditable in App Insights).
- `Enable-ServiceBusSecureTrigger.ps1` now takes `-LockDuration` alongside its existing
  `-MaxDeliveryCount`, and asserts `MaxDeliveryCount × LockDuration > ClaimStaleAfter` before
  provisioning anything — failing the deploy loudly rather than silently creating a queue that can
  never self-heal a dead/hung attempt.

**B — settlement bound to the host's own cancellation token.** Every `CompleteMessageAsync`/
`DeadLetterMessageAsync` call used the trigger's own `cancellationToken`. During a shutdown/drain
that token is already cancelled, so the settlement call throws immediately and the message is left
unsettled even though its outcome was already decided. Fixed by routing every settlement call
through `SettleAsync`, which builds an independent `CancellationTokenSource` (sized by
`ServiceBusTriggerOptions.SettlementTimeout`, default 30s) over `CancellationToken.None`. This is
safe because the result is **always** persisted before settlement is attempted — if settlement
times out, the message is simply left unsettled, the lock expires, the message redelivers, and the
idempotency dedupe check (see flow step 2) finds the already-saved result and completes it then:
self-healing at the cost of one extra delivery, never a lost or duplicated outcome.

### The three Service Bus signals

| Intent | Action in code | Effect |
|---|---|---|
| Retry (transient) | throw; settle nothing; save no result; release the claim | extension abandons → `DeliveryCount++` → queue auto-DLQs at `MaxDeliveryCount` |
| Give up (poison/business) | save result, then dead-letter (timeout-bounded settlement) | straight to DLQ, no retry |
| Done | save result, then complete (timeout-bounded settlement) | removed from queue |

**Non-negotiable invariants:**

1. A transient failure must **never** call `SaveResult` — a persisted result satisfies the
   idempotency dedupe check on redelivery and permanently prevents re-execution.
2. A terminal failure must **always** call `SaveResult` **before** settling, so a settlement
   timeout is self-healing.
3. Settlement must never use the host `cancellationToken`.

### Dead-letter triage sub-reasons

Every terminal path's `deadLetterReason` is `"{Status}:{SubReason}"` (e.g.
`Malformed:BodyNotJson`) instead of the bare status, so DLQ entries sharing a status are
distinguishable without parsing `deadLetterErrorDescription`. Existing filters matching on the bare
status still match, since it remains the string's prefix.

| Terminal path | `deadLetterReason` |
|---|---|
| Message body is not valid JSON | `Malformed:BodyNotJson` |
| Message body deserialized to null (e.g. literal `null`) | `Malformed:NullPayload` |
| Business/activity failure | `Failed:ExecutionFailed` |

(The `InvalidToken:*`/`Denied:*` sub-reasons from the removed per-caller-token design no
longer occur — see "History" above.)

### Silent-death diagnostic breadcrumb (`RecordClaimStage`) — re-wired in (build 30651, 2026-09-12)

The 1000-message ShovelBridge load test had repeatedly produced correlation ids that were
claimed (`TryClaim` succeeds) and then simply vanished — no result, no dead-letter, no
exception (`docs/ShovelBridge-Architecture.md`'s running log, 2026-08-30 through
2026-09-11 entries) — consistent with the host process dying (suspected OOM under
Consumption-plan burst pressure) before reaching any of this function's own failure-path
logging. Two independent live investigations (2026-08-31, 2026-09-11) additionally found
**Application Insights had captured zero telemetry of any kind for the engine for the
entire run window**, so the `ILogger.LogInformation` checkpoints bracketing the call into
`IWorkflowExecutor.Execute` were not on their own a reliable trail for a recurrence.

`IServiceBusReplayAndResultStore.RecordClaimStage(correlationId, stage)` mirrors such
checkpoints into a `lastStage`/`lastStageAtUtc` pair merged into the same
`sbtrigger:claim:<correlationId>` Hangfire hash row `TryClaim` already writes (no new key,
no distributed lock — best-effort, must never fail or delay the execution it observes).
That row is durable SQL, written synchronously, independent of the telemetry pipeline.

**Re-wired into `ServiceBusWorkflowTriggerFunction` again after build 30651 (2026-09-12)
reproduced the same signature** (`...-000304`, `...-000307` claimed, never resolved — see
`docs/ShovelBridge-Architecture.md`'s 2026-09-12 entry). This time App Insights ingestion
was healthy overall (thousands of trace/request rows for the other 998 messages), yet the
two dying executions left literally nothing — not even the unconditional `ClaimAcquired`
log line that fires immediately after a successful claim — reinforcing that an abrupt
process death can lose telemetry that was never flushed, independent of whether the app's
telemetry pipeline is healthy in general. It had previously been disconnected after the
2026-09-11 load test run passed cleanly; the capability itself (interface method,
implementation, `GetClaimStageForTests` test seam) was kept in
`ServiceBusReplayAndResultStore` throughout, so this re-wiring only touched
`ServiceBusWorkflowTriggerFunction`'s three checkpoint call sites (covered by
`ServiceBusWorkflowTriggerFunctionTests`'s "Silent-death diagnostic breadcrumb" test
group). A future occurrence can be diagnosed directly via:

```sql
SELECT [Key], [Field], [Value]
FROM HangFire.Hash
WHERE [Key] = 'sbtrigger:claim:<correlationId>'
```

`lastStage = "AboutToExecute (...)"` with no `ExecutionReturned` ever recorded would mean
the process died inside (or was killed during) `IWorkflowExecutor.Execute` itself — no
further code in this function ever runs to record more than that. `lastStage =
"ClaimAcquired"` with nothing further would mean it died while waiting for a concurrency
slot (there is no token validation or policy check between claim and execution any more).

## Threat model coverage

| Threat | Mitigation |
|---|---|
| **Anyone with queue-send rights runs any workflow, unconditionally** | **Accepted by design, not mitigated.** This is the entire point of the transport-only trust model (see "History" above) — the only control is who holds "Azure Service Bus Data Sender" on the configured queue/namespace. Scope that role assignment as tightly as the deployment allows. |
| Duplicate delivery (Service Bus at-least-once) causes duplicate side effects | Correlation-id idempotency check short-circuits re-execution |
| Malformed/garbage messages wedge the queue | Malformed messages are dead-lettered immediately, not retried |
| A workflow that will always fail (bad inputs) exhausts retry budget pointlessly | Business/activity failures are dead-lettered directly (non-transient); only genuinely unexpected exceptions and results flagged `IsTransientFailure` (e.g. `OutOfMemoryException` under cold-start memory pressure) are rethrown for the SB extension's retry/backoff |
| Broker credentials leak | Managed-Identity-only connection (`__fullyQualifiedNamespace`), no connection string/SAS key in any config |
| Stolen/replayed token reused across messages | N/A — there is no per-message token any more (removed with the per-caller-token design; see "History" above) |

## End-to-end verification (RabbitMQ Shovel → Service Bus → this trigger)

`Test-ShovelBridgeE2E.ps1` / `Run-ShovelBridgeE2E-DevOpsRabbit.ps1`
(`Scripts/Tests/Integration/`) prove RabbitMQ→Shovel→Service Bus **message arrival**
by default — they do not exercise this trigger. Passing `-VerifyWorkflowExecution`
extends the same pipeline to prove actual **workflow execution** through this trigger:

1. The `Warewolf.Execution.ServiceBusWorker.E2EHarness` console app (still the tool
   both scripts shell out to) gains a `--mode workflow-execution` path alongside its
   original arrival-proof `--mode arrival` (default, unchanged, still what CI's
   `pipeline*.yml` jobs use).
2. In this mode the harness publishes the real `ServiceBusWorkflowMessage` contract
   (`{"inputs","correlationId"}`) to the RabbitMQ source queue.
   `-MessageAuthToken` is OPTIONAL: since this trigger no longer validates a
   per-message caller token, setting it only proves the trigger genuinely ignores the
   `Authorization` header when present — it has no effect on whether the message
   executes.
   > **WOLF-8517 follow-up (not yet done):** the harness/scripts still build a
   > `"workflow"` field into this payload and pass `-WorkflowName` as a message value.
   > Now that the workflow comes solely from `Settings/servicebustrigger.bite` (see
   > "Configuration reference" above), the harness needs to (a) drop `"workflow"` from
   > the published body and (b) verify the deployed trigger file's `WorkflowName`
   > matches `-WorkflowName` instead of relying on the message to select it. Flagged
   > here rather than changed silently — needs its own review pass.
3. The harness does **not** open its own Service Bus receiver on the trigger queue in
   this mode — Service Bus queues are single-consumer, so a second listener could steal
   the message before this trigger's own Managed-Identity subscription consumes it.
   Instead the harness polls this trigger's own `GET /secure/servicebus-result/{correlationId}`
   endpoint (see "Result delivery" above) — an ordinary HTTP `/secure/*` route, still
   Entra-protected, authenticated via the REQUIRED `-ResultPollAuthToken` — and passes
   only when the terminal status is `Succeeded`.
4. Pass-through parameters on both scripts: `-VerifyWorkflowExecution`,
   `-WorkflowName`, `-WorkflowInputsJson`, `-CorrelationId` (auto-generated if
   omitted), `-MessageAuthToken` (optional), `-EngineBaseUrl`, `-ResultPollAuthToken`
   (required), `-ResultTimeoutSeconds`. See each script's comment-based help for full
   parameter docs.

**Prerequisite — this trigger must already be provisioned on the target Function App**
(queue exists, managed identity has `Azure Service Bus Data Receiver`, and the
`ServiceBusConnection__fullyQualifiedNamespace` app setting is set) before
`-VerifyWorkflowExecution` can pass. `Scripts/Enable-ServiceBusSecureTrigger.ps1` does
exactly that, idempotently, against an already-deployed app — run it (ideally with
`-DryRun` first) as a separate, reviewed step; it is **not** invoked automatically by
either E2E script or any CI job. It does not grant "Azure Service Bus Data Sender" to
anyone — that assignment remains separate manual steps, detailed next.

**Second, separate prerequisite:** the target app must have been deployed (or redeployed)
with `Deploy-WwExecutionEngine.ps1 -ServiceBusTriggerQueueName`/
`-ServiceBusTriggerWorkflowName` matching `Enable-ServiceBusSecureTrigger.ps1`'s
`-TriggerQueueName` — otherwise `Settings/servicebustrigger.bite` either doesn't name this
workflow at all (every message fails `Workflow not found`) or names a queue nothing is
provisioned for. `Enable-ServiceBusSecureTrigger.ps1` no longer sets this file itself (see
"Configuration reference" above for why the original post-deploy write couldn't work).

### Getting `-ResultPollAuthToken` (superseded Entra provisioning history)

An earlier pass at this trigger required a `-MessageAuthToken` validated against a
dedicated Entra audience, and this section walked through provisioning that audience
(`Warewolf-ShovelBridgeE2E-ServiceBusTrigger`, app id
`e200900a-2d5d-4356-94c0-cd7e33232ce0`, app role `Warewolf_ClientApps`,
`1784e307-83a4-4972-8b65-b1be04358101`) plus a daemon client app
(`Warewolf-ShovelBridgeE2E-Daemon`, `dc1182bc-ffc1-4a1d-a414-ab672998eb9a`) in the
DEV2/`ca0cc53b-9af4-4067-bcdf-be9c648450d1` tenant, and a long incident history around
minting/rotating that daemon's client secret from the pipeline. That per-message-token
audience is no longer needed by this trigger — see "History" above — so the dedicated
`Warewolf-ShovelBridgeE2E-ServiceBusTrigger` resource app is now unused and is a
**candidate for manual deprovisioning** (a tenant-admin action, out of repo scope; not
scripted here).

The daemon client app (`dc1182bc-...`) and its `ShovelE2EDaemonClientSecret` pipeline
secret are still referenced by `pipeline-CLOUD.yml`/`pipeline-LOADTEST.yml`'s "Acquire
Entra token for ShovelBridge E2E daemon" step, now solely to mint `-ResultPollAuthToken`
for the still Entra-protected `GET /secure/servicebus-result` polling call. That step
requests a token scoped to `api://$(ShovelE2EResourceAppId)/.default` — the SAME
dedicated (now otherwise-unused) resource app id as before — unchanged by this pass.
**Verify live, before relying on it, that this scope is actually valid for the general
HTTP `/secure/*` audience** on the target engine (`WAREWOLF_ENTRA_CONFIG.audience`) —
it may need updating to the general HTTP audience's app id if the two were never the
same, since minting against the (now-unused) dedicated Service Bus audience was
previously only exercised as a side effect of `-ResultPollAuthToken` defaulting from
`-MessageAuthToken`, not as a deliberately verified configuration in its own right.

Full provisioning steps, the `az ad app credential reset` Owner/Graph-permission
incident history, and the eventual switch to a once-minted, non-rotated daemon secret
are preserved in this file's git history (see the commit that introduced the
transport-only trust model) for anyone deprovisioning or re-auditing these objects.

## Explicitly out of scope for this pass

- Push-style result delivery (reply queue, webhook, SignalR).
- A new Redis (or other) dependency for the replay/result store — Hangfire/in-memory
  only.
- Any change to `Warewolf.Execution.ServiceBusWorker` (Model B) — kept as-is, fully
  additive alongside it.
- Automatic/CI-driven provisioning of the trigger queue and RBAC role —
  `Enable-ServiceBusSecureTrigger.ps1` (see above) exists for this but is a deliberately
  separate, human-reviewed step, not run by any script or pipeline automatically.
- Automatic/CI-driven provisioning of who holds "Azure Service Bus Data Sender" on the
  configured queue/namespace — this is now the trigger's entire authorization boundary
  (see "The one trust boundary" above) and is deliberately left to the operator to scope
  and grant manually, same as the RBAC role above.
- Deprovisioning the now-unused `Warewolf-ShovelBridgeE2E-ServiceBusTrigger` Entra
  resource-app registration and its `Warewolf_ClientApps` app role — flagged as a manual
  tenant-admin follow-up (see "Getting `-ResultPollAuthToken`" above), not scripted here.
