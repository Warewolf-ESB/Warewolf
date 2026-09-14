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
- **A body that is not this envelope is bound verbatim instead** — see "Non-envelope bodies"
  below. The envelope is the documented shape, not the only one that works.
- `correlationId` (optional) — caller-supplied idempotency/polling key. If omitted, the
  trigger falls back to the Service Bus message's native `CorrelationId` property, then
  `MessageId`.

There is no caller-identity field in this contract, on the message body or its
application properties — trust is decided entirely by arrival on the configured queue,
before the message is even parsed.

### Non-envelope bodies (fixed 2026-09-14)

A JSON body that carries no `inputs` property — a flat business payload, or anything with
nested/recordset shape — is handed to the executor **verbatim** as
`WorkflowExecutionRequest.RawInputPayload` (`ServiceBusWorkflowTriggerFunction.ResolveRawInputPayload`),
exactly as `Http.WorkflowFunctionHelper.ExtractFromBody` already did for the HTTP routes. So
this flat body binds the same fifteen variables the equivalent HTTP query string does:

```json
{ "ForPaymentCycle": "202402", "MandateDRI": "22", "ResultPaymentCycle": "202401", … }
```

**Why this was not always true.** The trigger previously bound inputs *only* from the envelope.
A flat body is still valid JSON, so it deserialized without error into a message with
`Inputs == null`, and the workflow then ran against an **empty datalist** — not rejected as
malformed, just executed with nothing bound. The failure therefore surfaced as a business
failure deep inside the workflow, attributed to the workflow rather than to the transport.

What that looks like in practice is a workflow failing on its own missing variables. For
`ProfilerServices/ProfilerWrapper/RequestForProfiling` it is:

```
Scalar value { MandateDRI } is NULL: { DebitRunID }
Scalar value { ResultPaymentCycle } is NULL: { ExtractScheduleId }
Invalid Calculate
```

(its `Map Remaining Fields for CE (6)` assign evaluates
`!~calculation~![[ApplicableMonth]]*1!~~calculation~!`; an empty `ApplicableMonth` leaves the
calculator the un-parseable expression `*1`, and
`DsfDotNetMultiAssignActivity.PerformCalcForAtom` throws `ErrorResource.InvalidCalculate`.)
**`Invalid Calculate` here is the signature of "executed with zero inputs bound" — not a data
defect, not schema drift, and nothing wrong with the workflow.** Reproduced directly on
2026-09-14: the same `/Public` route returns `hasErrors:false` with its full query string, and
exactly this error without it.

> **Not the cause of the 2026-09-14 profiler load-test dead-letters.** Those 4998/5000 messages
> were rejected **earlier**, as `Malformed`, by a `wwuat3` deployment predating WOLF-8517 — its
> message contract still required a `workflow` field, so a flat body never reached execution at
> all (`Reason=Message is missing the required 'workflow' field`, observed live on that
> deployment). The binding gap documented here is a **separate, latent** defect in the same
> area: it is what a flat producer would hit once that deployment catches up with this source.
> Both are payload-contract mismatches; only one of them caused that run.

**Boundary:** parity with HTTP is not complete. The body is still deserialized into
`ServiceBusWorkflowMessage` first, so a **non-JSON body (e.g. XML) is still dead-lettered as
`Malformed:BodyNotJson`** and never reaches the fallback, even though the HTTP routes accept
it. Deliberate, and pinned by `Run_XmlBody_StillDeadLettersAsMalformed`.

This asymmetry also defeated the premise of the RabbitMQ→Service Bus Shovel bridge
(`ShovelBridge-Architecture.md`), which exists precisely so that **existing RabbitMQ producers
can trigger the engine unmodified** — such producers publish flat business payloads, never a
Warewolf envelope.

> **Related trap, same signature.** `Invoke-WwEnginePreWarm.ps1`'s built-in body carries a
> single `message` input, which only its default workflow (`rabbit/RabbitProcess`) declares.
> Pointed at any other route it warms a path that fails on the workflow's own missing
> variables — producing this same `Invalid Calculate` text for an unrelated reason. Pass
> `-InputParameters` whenever `-WorkflowRoute` is not the default.

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

WOLF-8517: the workflow the trigger runs is sourced SOLELY from `Settings/servicebustrigger.bite`
(`ServiceBusTriggerDefinition.cs`) — a plain-JSON, deploy-bundled file (no secret in it, so
never WFAES-encrypted), loaded once at cold start **before** the host is built. Its
`WorkflowName` is registered as a DI singleton and consumed directly by
`ServiceBusWorkflowTriggerFunction` — **the message body no longer names the workflow**
(see "Message contract" above). A missing file, or either field blank, is fatal — refuses
to start rather than silently defaulting.

**Correction (2026-09-14) — `QueueName` could NOT drop the app setting; it still needs one.**
The original WOLF-8517 design (immediately above, kept for history) intended `QueueName` to
be sourced solely from the file too, injected into configuration via
`HostBuilderExtensions.ConfigureWarewolf`'s `ConfigureAppConfiguration` callback so the
`[ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%")]` attribute indirection would
resolve it "exactly as it did when that was a real app setting." **This does not work on the
dotnet-isolated worker model and never has.** Attribute placeholders like
`%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%` are resolved by the out-of-process Functions Host at
function-indexing time — before the isolated worker process (where `ConfigureWarewolf`'s
in-memory `IConfiguration` entry lives) is even started. The host only ever sees real Azure
App Settings (or `local.settings.json` locally), never anything the worker injects into its
own process. Deployed live against `wwuat3` (`DEV2`) with only the file staged: the function
failed to index outright —
```
Error indexing method 'Functions.ServiceBusWorkflowTrigger'. Microsoft.Azure.WebJobs.Host:
'%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%' does not resolve to a value.
The 'ServiceBusWorkflowTrigger' function is in error... failed indexing and will be disabled.
```
— silently, with no host-level diagnostic visible unless `APPLICATIONINSIGHTS_CONNECTION_STRING`
is also set (the app's own `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` does not cover Functions
Host telemetry). The queue is never consumed and no error surfaces anywhere else.

**Fixed:** `Deploy-WwExecutionEngine.ps1` now ALSO writes `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE`
as a real app setting (from `-ServiceBusTriggerQueueName`) whenever trigger config is
supplied, in addition to staging `servicebustrigger.bite`. `WorkflowName` is unaffected by
any of this — it is read via DI inside the worker process, not through an attribute
placeholder, so the file alone is sufficient for it. The in-memory `ConfigureAppConfiguration`
injection in `HostBuilderExtensions.cs` is left in place (harmless, just insufficient alone)
rather than removed, since it costs nothing and doing so is a separate cleanup.

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
| `heartbeatIntervalSeconds` | How often an in-flight execution refreshes its breadcrumb and this instance refreshes its own liveness heartbeat (default: 5). |
| `instanceLivenessTimeoutSeconds` | WOLF-8512 (build 30658): how long a claim's owning instance may go without a heartbeat before the claim is treated as orphaned and taken over immediately, without waiting out `ClaimStaleAfter` (default: six heartbeat intervals, floored at 30s). See **D** under "Failure classification" below. |

### Settlement is governed by the attribute, not `host.json`

The trigger settles every message explicitly via the injected `ServiceBusMessageActions`, and
deliberately leaves some deliveries unsettled so the lock expires and they redeliver. **That
behaviour is enabled by `AutoCompleteMessages = false` on the `[ServiceBusTrigger]` attribute.**

`host.json`'s `extensions.serviceBus.autoCompleteMessages: false` is *not* sufficient and is *not*
what enforces this: it sets the global `ServiceBusOptions`, while the dotnet-isolated host applies a
per-function override — defaulting to `true` — that takes precedence. Build 30658 ran with the two
disagreeing and silently destroyed every message that took an unsettled return. See **C** under
"Failure classification" for the full account and the three guards now in place.

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

**C — `AutoCompleteMessages` was `true` at runtime, so every unsettled return destroyed its
message (build 30658, 2026-09-13).** This is the root cause of the losses in builds 30649–30658 and
it silently negated invariants 1 and 3 below.

`host.json`'s `extensions.serviceBus.autoCompleteMessages: false` configures the **global**
`ServiceBusOptions` only. Under the dotnet-isolated worker the Functions host additionally applies a
**per-function** override that defaults to `true` unless the `[ServiceBusTrigger]` attribute sets it,
and the per-function value is the one that decides settlement. Because the attribute did not set it,
every host start logged:

```
The 'AutoCompleteMessages' option has been overriden to 'True' value for 'Functions.ServiceBusWorkflowTrigger' function.
```

— 31 starts across 19 instances in build 30658 — while the dumped global `ServiceBusOptions` still
read `"AutoCompleteMessages": false`. The two disagreed and nothing flagged it.

Consequence: the deliberate "return without settling, let the lock expire" path (the claim-conflict
branch in `Run`) had its message **auto-completed and destroyed** instead — no retry, no dead-letter,
no result, and *nothing in the DLQ to show for it*. Correlation ids `…-000235`, `…-000238` and
`…-000687` each received exactly two deliveries and then vanished, despite `maxDeliveryCount=10` and
`lockDuration=PT5M` on the queue. Because destruction leaves no trace, the harness could only report
"never got a result", which is indistinguishable from "still slow" — which is why this survived five
builds.

Fixed by declaring it on the attribute itself:

```csharp
[ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%",
                   Connection = "ServiceBusConnection",
                   AutoCompleteMessages = false)]
```

Guarded three ways, because the failure is invisible at runtime:

- A unit test asserts the attribute's `AutoCompleteMessages` is `false` by reflection.
- Startup logs the contract next to the host's own override line, so the two can be diffed in App
  Insights.
- `Test-ShovelBridgeE2E.ps1` Phase 4d reconciles the destination queue's dead-letter delta against
  the harness's own failure count. A run that loses messages but dead-letters none is this exact
  signature, and the phase says so by name.

> `host.json` is deliberately **not** annotated with a comment about this — the file has no comments
> today and the host's parser is not guaranteed to tolerate them. The attribute and this section are
> the source of truth.

**D — a claim orphaned by a worker crash blocked redelivery for the full staleness window (build
30658).** `ClaimStaleAfter` is a worst-case backstop (11 minutes by default), long enough never to
cut short a genuinely-running execution — but it cannot tell "dead" from "slow", so a claim left by a
crashed worker refused every redelivery until it expired. `TryClaim` now also consults the owning
instance's heartbeat row (`IsClaimOrphaned`):

| Signal | Meaning | Takeover |
|---|---|---|
| Owner heartbeating under a **different process id** than the claim recorded | the claiming worker process was replaced | immediate — no timing judgement needed |
| Owner has not heartbeated within `InstanceLivenessTimeout` | the instance scaled in or died | immediate |
| No owner recorded, or no heartbeat row at all | cannot distinguish dead from quiet | none — fall back to `ClaimStaleAfter` |

The process-id signal is the one that mattered in build 30658: on instances `813073bd` and
`5ae10c76` the Functions **host** survived (the role instance kept logging) while the isolated
**worker** crashed underneath it, so an instance-level liveness check alone would have missed it.
Ambiguity always resolves to "still held" — a false takeover means two concurrent executions of the
same message, which for the ProfilerMain load test is a duplicate DB row the test explicitly asserts
against.

**E — transient failures classified as terminal, then dead-lettered with no retry (build 30658).**
`WorkflowExecutor` flagged `IsTransientFailure` only for an `OutOfMemoryException` caught at the top
of `Execute`. Two other shapes of the same condition were being treated as permanent business
outcomes:

- **`"Workflow not found."`** came from a bare `File.Exists` miss. On an instance whose resources
  were not yet staged this is transient, but it was dead-lettered outright — losing `…-000688` while
  999 sibling messages ran the same workflow successfully, and failing **all 1000** messages in build
  30649. Now, a miss whose workflows directory is itself absent, empty, or unreadable returns a
  *transient* failure; only a populated directory genuinely lacking the workflow stays terminal.
  (That path's only log line was also commented out, which is why `…-000688`'s destruction left no
  trace anywhere — it now logs.)
- **An OOM raised inside the activity chain** is captured as `DsfDataObject.ExecutionException` and
  surfaced as the generic `"Workflow execution failed due to an unexpected error."`. Classification
  now walks the exception chain (`IsTransientExecutionException`) rather than depending on where the
  exception happened to surface. `…-000343` was lost this way, in the same second that `…-000342`
  hit a plainly-reported OOM on the same instance while that instance was logging `Stopping JobHost`.

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
4. `AutoCompleteMessages` must stay `false` **on the `[ServiceBusTrigger]` attribute** — not merely
   in `host.json`, which does not govern it. Invariants 1 and 3 both depend on an unsettled return
   meaning "redeliver me"; with auto-complete on it means "destroy me" (see **C** above).
5. A delivery must not take a claim it cannot honour: `Run` declines to claim once the host
   cancellation token signals drain, and releases the claim if drain begins in the window between
   that check and the claim being taken. Correlation ids `…-000687`/`…-000688` were claimed 1.1s
   *after* their instance logged `Stopping JobHost` and leaked when its worker went away.

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

### Watched automatically since build 30655 (2026-09-13)

The query above is no longer the only way this is found. `ServiceBusClaimWatchdogFunction`
(in the **JobProcessor** app, TimerTrigger `%SERVICEBUS_CLAIM_WATCHDOG_SCHEDULE%`, default
every 10 min) runs exactly that scan on a schedule: claims older than
`%SERVICEBUS_CLAIM_WATCHDOG_ALERT_AFTER_MINUTES%` (default 30) with no
`sbtrigger:result:<correlationId>` row are reported as a structured `LogError` plus a
`ServiceBusClaimStalled` Application Insights event, and marked with `watchdogAlertedAtUtc`
in their own claim hash so a persistently stalled claim is alerted once, not every tick.

It lives on a timer, and in the *other* app, for a specific reason established by the build
30655 investigation (`docs/ShovelBridge-Architecture.md`'s running log): Service Bus only
hands a message to a receiver that asks for it, so a Consumption-plan engine that has scaled
to zero after a burst has nothing listening and a message freed by a crashed host can sit
untouched indefinitely — build 30655's three correlation ids were still frozen 13 hours
later. A TimerTrigger is woken by the platform on schedule regardless of queue depth, so the
watchdog is not subject to the blind spot it is watching for.

It **detects and alerts only** — it never settles, dead-letters or re-drives the message,
matching the fail-only policy `ReaperFunction` already applies to suspend/resume jobs.

The watchdog also reports the instance/process cross-reference this section's breadcrumb was
extended for, as one of four diagnoses:

| Diagnosis | Meaning |
|---|---|
| `WorkerProcessRestarted` | The instance is alive under a **different** pid than the one that took the claim — the isolated worker process died mid-execution. This is build 30655's signature. |
| `InstanceStoppedHeartbeating` | The instance's own heartbeat stops at or before the claim's last checkpoint — the host went down with the execution. |
| `InstanceAliveExecutionStalled` | The instance kept heartbeating past the claim's last checkpoint under the same pid — the execution itself is hung. |
| `NoInstanceHeartbeat` | Nothing is recording liveness for the instance that took the claim. |

The behaviour this breadcrumb depends on is CI-enforced by
`Scripts/Tests/Integration/Test-ServiceBusTriggerCrashRecovery.ps1` (pipeline job
`ServiceBusTriggerCrashRecoveryTest`), which kills the engine's process tree mid-execution
against a local Service Bus emulator and asserts the claim is left at a non-terminal stage
with no result row.

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
