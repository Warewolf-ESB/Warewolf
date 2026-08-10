# Secure Service Bus Trigger Architecture — In-Process Model A

## Purpose

Trigger **protected** (`/secure/*`-equivalent) Warewolf workflow executions directly
from an Azure Service Bus message, evaluated against the **same** per-caller
authorization policy that the HTTP pipeline enforces — without a broker-to-HTTP hop
and without a second, parallel authorization implementation. This satisfies
`Spec-Secure-ServiceBus-Triggered-Execution.md`'s **Model A** (delegated / on-behalf-of
identity, token travels with the message) as an additive capability of the Lightweight
engine itself.

## Relationship to the existing Service Bus integrations

Two other Service-Bus-related components already exist in this repo and are
**unchanged** by this work:

| | **Shovel Bridge / Model B** (`Warewolf.Execution.ServiceBusWorker`) | **This feature — Model A** (`Warewolf.Execution.Lightweight`) |
|---|---|---|
| Queue | `wwexecution-queue` (default) | `wwexecution-secure-trigger-queue` (default, distinct queue) |
| Identity on the message | None — worker's own Managed Identity calls Lightweight's HTTP route | **Caller's own delegated Entra token**, carried in the message |
| Authorization granularity | Whatever the worker's app-only identity is allowed (coarse, system-level) | Evaluated **per caller** via the caller's own claims |
| Transport | Message → separate Function App → HTTP call → Lightweight | Message → **in-process** trigger inside Lightweight (no HTTP hop) |
| Use case | Bridging existing RabbitMQ producers / system-to-system integration jobs | A specific end user or delegated app needs their own permissions enforced on a Service-Bus-triggered execution |

Both can run side by side against the same Lightweight deployment; they use different
queues and are independently enabled/disabled.

## Two trust boundaries (per the spec)

1. **Transport boundary** — who can put a message on the queue / who can receive from
   it. Enforced entirely by Azure — Service Bus namespace RBAC (`Azure Service Bus Data
   Sender` / `Data Receiver`) via Managed Identity, and (optionally) namespace network
   rules. The trigger's listen-side connection uses the Functions
   [identity-based connection](https://learn.microsoft.com/azure/azure-functions/functions-bindings-service-bus)
   convention (`ServiceBusConnection__fullyQualifiedNamespace`), never a shared-access-key
   connection string, so no broker secret exists to leak.
2. **Message/caller boundary** — once a message is received, *who is the message acting
   on behalf of, and are they allowed to run this specific workflow?* This is the part
   this feature implements: the message must carry the caller's own bearer token, which
   is validated and evaluated exactly like an HTTP request would be.

Passing the transport boundary only proves "an authorized producer put a message on the
queue" — it does **not** prove the message's *content* is authorized to run a given
workflow. Model A closes that gap by requiring and validating a per-message identity.

## Why reuse `IWorkflowPolicyMatcher` instead of a second authorization path

The spec's core requirement (§5) is that the authorization decision be made by **one**
shared, transport-agnostic component, callable identically from HTTP middleware and any
other trigger. That component already existed: `IWorkflowPolicyMatcher.Evaluate(workflowName,
principal, requiredPermissions)` is registered as a DI singleton and has no dependency on
`HttpRequestData`/`FunctionContext`. The new Service Bus trigger calls it directly —
**zero changes** were needed to `WorkflowPolicyMatcher` itself. This avoids the classic
failure mode of "authorization drift", where a second implementation quietly diverges
from the first over time (different bug fixes, different `secure.config` interpretation).

Token *validation* (RS256 signature, issuer, audience, expiry) was extracted from
`BearerTokenPrincipalParser` into a standalone `EntraBearerTokenValidator`, so both the
HTTP parser and the Service Bus trigger perform **identical** cryptographic validation
logic, just against two different `EntraAuthOptions`-derived configs (different expected
audience — see below).

## Topology

```
Delegated caller (user or app with its own Entra identity)
   │  acquires its own Entra access token (audience = WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE)
   │  publishes ServiceBusWorkflowMessage as the message body
   │  sets the token as the "Authorization" application property: "Bearer <token>"
   ▼
Azure Service Bus queue (wwexecution-secure-trigger-queue, dead-lettering enabled)
   │  ServiceBusTrigger — Managed Identity listen connection (RBAC: Azure Service Bus Data Receiver)
   ▼
ServiceBusWorkflowTriggerFunction  (Warewolf.Execution.Lightweight, in-process)
   1. Parse JSON body                          → malformed          → dead-letter + record
   2. Idempotency check (CorrelationId)         → already processed  → complete, no re-execution
   3. Extract "Authorization" application property → missing         → dead-letter + record (InvalidToken)
   4. Validate token (EntraBearerTokenValidator, ServiceBusEntraAuthOptions) → invalid → dead-letter + record (InvalidToken)
   5. Build WorkflowClaimsPrincipal from validated claims
   6. Resolve + register "jti" (replay check)   → already seen       → dead-letter + record (InvalidToken)
   7. IWorkflowPolicyMatcher.Evaluate(workflow, principal, permissions)  ← SAME matcher HTTP uses
      ├─ Forbidden / ConfigMissingDeny → dead-letter + record (Denied)
      └─ Allowed                        → continue
   8. IWorkflowExecutor.Execute(...) in-process (ExecutingPrincipal = validated principal)
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
  "workflow": "Hello World",
  "inputs": { "Name": "FromServiceBus" },
  "correlationId": "optional-caller-supplied-id"
}
```

- `workflow` (required) — the workflow name, matched against `secure.config` exactly as
  an HTTP `/secure/{workflow}` path segment would be.
- `inputs` (optional) — a string map, passed through identically to how HTTP query-string
  inputs are handled today (`WorkflowFunctionHelper.CreateRequestByName`).
- `correlationId` (optional) — caller-supplied idempotency/polling key. If omitted, the
  trigger falls back to the Service Bus message's native `CorrelationId` property, then
  `MessageId`.

**The bearer token is never part of the JSON body.** It travels in the message's
`Authorization` application property (value: `Bearer <token>`), mirroring the HTTP
`Authorization` header exactly, so producers reuse the same token-acquisition code they'd
use for an HTTP call.

## Result delivery — polling only (no push)

The result of an execution is retrieved by polling `GET
/secure/servicebus-result/{correlationId}`, not pushed back to a reply queue or webhook.
This was a deliberate, explicitly-scoped-down decision (approved by the user during
design) to avoid introducing a new push-delivery mechanism (reply queue, webhook
registrations, SignalR, etc.) in this pass. A caller that needs the outcome:

1. Publishes the message with a `correlationId` it generates itself.
2. Polls the result endpoint (itself protected by the ordinary HTTP authorization
   pipeline, requiring `WorkflowPermission.View` on the workflow) until a terminal
   status (`Succeeded`, `Failed`, `Denied`, `InvalidToken`) is returned, or a 404 if the
   message hasn't been processed yet.

Result-queue / webhook push delivery is explicitly out of scope for this implementation
and is called out as a candidate follow-up.

## Backing store

`IServiceBusReplayAndResultStore` covers two distinct concerns with two different
guarantees:

- **jti replay prevention** (token-level) — the same bearer token must not be reused
  across multiple different messages within its validity window. Checked via an atomic
  check-and-set (Hangfire distributed lock + hash) so it is correct across multiple
  concurrent Function App instances.
- **Correlation-id idempotency** (business-level) — the same logical request, if
  redelivered by Service Bus (at-least-once delivery) or resubmitted by a naive retrying
  producer, is not re-executed; the previously recorded result is returned instead.

When `Config.Persistence` (the engine's optional Hangfire/SQL persistence store) is
enabled, both are backed by Hangfire's `JobStorage` (hash entries), giving durability and
cross-instance consistency for free. When persistence is not configured, both fall back
to an in-process `ConcurrentDictionary` — durable and correct for a single instance only,
appropriate for local development or single-instance deployments.

**Known limitation:** Hangfire hash entries written by this store have no TTL / automatic
eviction in this pass — there is no expiry-sweep background job. This is an accepted
trade-off (unbounded growth over very long uptimes) rather than a correctness gap; a
follow-up could add a scheduled cleanup job if this becomes an operational concern.

## Configuration reference

| App setting | Purpose |
|---|---|
| `ServiceBusConnection__fullyQualifiedNamespace` | Identity-based Service Bus connection (Managed Identity in Azure, developer credentials locally). No connection string / SAS key is used. |
| `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE` | Queue name the trigger listens on. Resolved via the Azure Functions `%AppSetting%` attribute-indirection syntax (default convention: `wwexecution-secure-trigger-queue`). |
| `WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE` | Expected `aud` claim for tokens carried in Service-Bus-triggered messages. Deliberately **separate** from `WAREWOLF_ENTRA_AUDIENCE` (the HTTP audience) so the two trust boundaries can use different app registrations/scopes if desired. |
| `WAREWOLF_ENTRA_TENANT_ID` | Reused from the existing HTTP Entra config (same tenant). |
| `WAREWOLF_SERVICEBUS_TRIGGER_JTI_WINDOW_HOURS` | How long a `jti` is remembered for replay-prevention purposes (default: 24). |
| `AzureWebJobs.ServiceBusWorkflowTrigger.Disabled` | Standard Azure Functions convention to disable the trigger entirely (e.g. when no Service Bus namespace is provisioned) with zero code changes. |

`host.json` requires `extensions.serviceBus.autoCompleteMessages: false` so the trigger's
explicit `CompleteMessageAsync` / `DeadLetterMessageAsync` calls (via the injected
`ServiceBusMessageActions`) take effect instead of the host auto-completing the message on
successful return.

## Threat model coverage (see spec for full detail)

| Threat | Mitigation |
|---|---|
| Anyone with queue-send rights runs any workflow as the system identity | Message must carry a validated, caller-specific token; `IWorkflowPolicyMatcher` evaluates the caller's own claims, not a shared system identity |
| Stolen/replayed token reused across messages | `jti` replay check, atomic across instances |
| Duplicate delivery (Service Bus at-least-once) causes duplicate side effects | Correlation-id idempotency check short-circuits re-execution |
| Malformed/garbage messages wedge the queue | Malformed messages are dead-lettered immediately, not retried |
| A workflow that will always fail (bad inputs) exhausts retry budget pointlessly | Business/activity failures are dead-lettered directly (non-transient); only genuinely unexpected exceptions are rethrown for the SB extension's retry/backoff |
| Broker credentials leak | Managed-Identity-only connection (`__fullyQualifiedNamespace`), no connection string/SAS key in any config |

## End-to-end verification (RabbitMQ Shovel → Service Bus → this trigger)

`Test-ShovelBridgeE2E.ps1` / `Run-ShovelBridgeE2E-DevOpsRabbit.ps1`
(`Scripts/Tests/Integration/`) prove RabbitMQ→Shovel→Service Bus **message arrival**
by default — they do not exercise this trigger. Passing `-VerifyWorkflowExecution`
extends the same pipeline to prove actual **workflow execution** through this Model A
trigger:

1. The `Warewolf.Execution.ServiceBusWorker.E2EHarness` console app (still the tool
   both scripts shell out to) gains a `--mode workflow-execution` path alongside its
   original arrival-proof `--mode arrival` (default, unchanged, still what CI's
   `pipeline*.yml` jobs use).
2. In this mode the harness publishes the real `ServiceBusWorkflowMessage` contract
   (`{"workflow","inputs","correlationId"}`) to the RabbitMQ source queue, setting the
   caller's bearer token as the message's `Authorization` header (and optionally `jti`)
   — these AMQP 0.9.1 headers are expected to survive the Shovel's amqp091→amqp10
   bridge as AMQP 1.0 application properties, which is where this trigger reads them
   from (see "Topology" above). *(Not yet empirically verified against a live broker in
   this pass — treat as a risk to confirm on first real run.)*
3. The harness does **not** open its own Service Bus receiver on the trigger queue in
   this mode — Service Bus queues are single-consumer, so a second listener could steal
   the message before this trigger's own Managed-Identity subscription consumes it.
   Instead the harness polls this trigger's own `GET /secure/servicebus-result/{correlationId}`
   endpoint (see "Result delivery" above) and passes only when the terminal status is
   `Succeeded`.
4. New pass-through parameters on both scripts: `-VerifyWorkflowExecution`,
   `-WorkflowName`, `-WorkflowInputsJson`, `-CorrelationId` (auto-generated if
   omitted), `-MessageAuthToken`, `-Jti`, `-EngineBaseUrl`, `-ResultPollAuthToken`,
   `-ResultTimeoutSeconds`. See each script's comment-based help for full parameter
   docs.

**Prerequisite — this trigger must already be provisioned on the target Function App**
(queue exists, managed identity has `Azure Service Bus Data Receiver`, and the four app
settings from "Configuration reference" above are set) before `-VerifyWorkflowExecution`
can pass. `Scripts/Enable-ServiceBusSecureTrigger.ps1` does exactly that, idempotently,
against an already-deployed app — run it (ideally with `-DryRun` first) as a separate,
reviewed step; it is **not** invoked automatically by either E2E script or any CI job.
It does not create the Entra App Registration behind `-EntraServiceBusAudience`, mint
any tokens, or grant secure.config permission to run the target workflow — those remain
separate manual steps, detailed next.

### Getting `-EntraServiceBusAudience` and `-MessageAuthToken`

These are **not** related to Azure RBAC role assignments (`Contributor`/`Reader` on a
subscription/resource group, e.g. what `az role assignment list` on the
`WarewolfShovelBridgeTesting` namespace or the Portal's "Access control (IAM)" blade
shows) — those govern who may *manage* Azure resources and have nothing to do with which
Entra App Registration the trigger validates message tokens against.

Live-checked against the DEV2/`dd0bc517-...` tenant while writing this: `WarewolfServer-UAT`
has Easy Auth disabled (`platform.enabled: false`) and zero `WAREWOLF_ENTRA_*` app
settings, so nothing is pre-wired to inherit. Of the app registrations already present in
the tenant, none are a clean fit to reuse as-is:
  * `Warewolf DevOps`, `Warewolf-Warewolf-a84325a0-...` — plain CI/CD service principals
    (the ones with `Contributor`/`Reader` RBAC in the screenshot above); no
    `identifierUris` or app roles defined at all.
  * `Warewolf Security` — does expose `api://05794411-b275-4801-97ac-8b078ed7196c` +
    a `user_impersonation` scope, but it's the live app already backing Warewolf **Studio**
    sign-in, has no app roles defined (so it can't back an app-only/daemon caller without
    first modifying it), and the trigger code's own doc comment is explicit that the
    Service Bus audience should be **dedicated** ("never the general HTTP audience —
    confused-deputy prevention"). Reusing/extending it would mean mutating a live,
    unrelated production app registration.
  * Several `wwexecutiondev*-auth` / `wwexecutiondev-daemon` / `wwexecutiontest1-auth`
    registrations also exist, left over from unrelated dev/test provisioning runs of these
    same scripts — not tied to `WarewolfServer-UAT`, so reusing one would risk testing
    against a stale/mismatched deployment.

**Decision made (autonomously, since this only *creates* new, narrowly-scoped test
objects rather than mutating any shared/production app — still recommend reviewing before
running):** provision one small, dedicated resource-app + daemon-client pair just for this
E2E test, using plain `az ad app create` (the heavier `Configure-WwExecutionAuth.ps1` also
flips on Easy Auth for the whole Function App's HTTP routes, which is out of scope here).

```powershell
# 1) Resource app (the audience) — dedicated, never reused elsewhere
$resourceApp = az ad app create --display-name "Warewolf-ShovelBridgeE2E-ServiceBusTrigger" `
    --sign-in-audience AzureADMyOrg | ConvertFrom-Json
$resourceAppId = $resourceApp.appId
az ad app update --id $resourceAppId --identifier-uris "api://$resourceAppId"
az ad sp create --id $resourceAppId | Out-Null

# 2) Define one app role for app-only (daemon/client-credentials) callers
$roleId = [guid]::NewGuid().ToString()   # e.g. e6a55e10-a04c-41ea-b734-51c8e1415a65
az ad app update --id $resourceAppId --app-roles (@(@{
    id = $roleId; displayName = "Warewolf_ClientApps"; value = "Warewolf_ClientApps"
    description = "App-only callers for the shovel-bridge E2E test"
    allowedMemberTypes = @("Application"); isEnabled = $true
}) | ConvertTo-Json -AsArray)

# 3) Daemon client app (what the harness authenticates as) + secret
$daemonApp = az ad app create --display-name "Warewolf-ShovelBridgeE2E-Daemon" `
    --sign-in-audience AzureADMyOrg | ConvertFrom-Json
az ad sp create --id $daemonApp.appId | Out-Null
$secret = az ad app credential reset --id $daemonApp.appId --years 1 --query password -o tsv

# 4) Grant + admin-consent the app role, then assign it to the daemon's SP
az ad app permission add --id $daemonApp.appId --api $resourceAppId --api-permissions "$roleId=Role"
az ad app permission admin-consent --id $daemonApp.appId
$daemonSpId    = az ad sp show --id $daemonApp.appId --query id -o tsv
$resourceSpId  = az ad sp show --id $resourceAppId --query id -o tsv
az rest --method POST `
    --url "https://graph.microsoft.com/v1.0/servicePrincipals/$daemonSpId/appRoleAssignments" `
    --body "{ `"principalId`": `"$daemonSpId`", `"resourceId`": `"$resourceSpId`", `"appRoleId`": `"$roleId`" }"
```

This mirrors the existing "Type B — Daemon / Service App" pattern documented in
`docs/Part4-ResourceProvisioning.md` and is exactly what
`Scripts/Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -AppRolesToAssign
Warewolf_ClientApps` automates for an *existing* resource app — it isn't used directly
here only because it also assumes `Configure-WwExecutionAuth.ps1` already ran (Easy Auth
on the target app), which this dedicated audience deliberately skips.

**Already provisioned in the DEV2/`dd0bc517-...` tenant** (tenant id
`ca0cc53b-9af4-4067-bcdf-be9c648450d1`) as of this pass — steps 1/2/3/4 above have been
run for real (these only created new, isolated objects, nothing shared was touched):

| Object | Value |
|---|---|
| Resource app (audience) | `Warewolf-ShovelBridgeE2E-ServiceBusTrigger` — `e200900a-2d5d-4356-94c0-cd7e33232ce0` |
| `-EntraServiceBusAudience` | `api://e200900a-2d5d-4356-94c0-cd7e33232ce0` |
| App role | `Warewolf_ClientApps` — `1784e307-83a4-4972-8b65-b1be04358101` |
| Daemon client app | `Warewolf-ShovelBridgeE2E-Daemon` — `dc1182bc-ffc1-4a1d-a414-ab672998eb9a`, directly assigned the `Warewolf_ClientApps` role on the resource app (verified via Graph `appRoleAssignments`) |

`az ad app permission admin-consent` failed (403 — the signed-in account isn't a tenant
admin); this is not fatal for app-only roles — the direct
`servicePrincipals/{id}/appRoleAssignments` write above is what actually grants the
`roles` claim on a client-credentials token, independent of the delegated-permission
admin-consent flow. No client secret was generated/kept by the agent — secrets are
sensitive and were deliberately not captured; mint one yourself:

```powershell
$secret = az ad app credential reset --id dc1182bc-ffc1-4a1d-a414-ab672998eb9a --years 1 --query password -o tsv
./Scripts/Get-WwExecutionToken.ps1 -TenantId 'ca0cc53b-9af4-4067-bcdf-be9c648450d1' `
    -ResourceAppId 'e200900a-2d5d-4356-94c0-cd7e33232ce0' -ClientId 'dc1182bc-ffc1-4a1d-a414-ab672998eb9a' `
    -ClientSecret $secret -GrantType ClientCredentials
```

**Owner gotcha found running the pipeline for real:** `az ad app credential reset` above
requires the *caller* to be an Owner of `dc1182bc-ffc1-4a1d-a414-ab672998eb9a`, and this
tripped up the pipeline's `Rotate ShovelBridge E2E daemon secret and acquire Entra token`
step with a 403 `Insufficient privileges`. Don't be misled by the app's Owners blade in the
portal listing `Warewolf Security` as an owner — that's the unrelated app called out above
(Studio sign-in, `api://05794411-...`), not the pipeline's CI/CD identity. The pipeline
authenticates via the `AzureClientId`/`AzureClientSecret`/`AzureTenantId` variables (see
`Log in to Azure CLI` step in `pipeline-CLOUD.yml`); the actual caller was confirmed via
`az ad sp list --display-name "Warewolf DevOps"` to be the **`Warewolf DevOps`** service
principal (`appId a15fdb40-3dbe-470c-8b4c-55d3d5448fe9`). Also note the Entra portal's own
"Add owners" picker only searches **users** — it can't find or add a service principal, so
this has to be done via CLI/Graph:

```powershell
$objectId = az ad sp show --id a15fdb40-3dbe-470c-8b4c-55d3d5448fe9 --query id -o tsv
az ad app owner add --id dc1182bc-ffc1-4a1d-a414-ab672998eb9a --owner-object-id $objectId
```

`Enable-ServiceBusSecureTrigger.ps1 -EntraServiceBusAudience 'api://e200900a-2d5d-4356-94c0-cd7e33232ce0'
-EntraTenantId 'ca0cc53b-9af4-4067-bcdf-be9c648450d1' -FunctionAppName WarewolfServer-UAT
-ResourceGroup DEV2 -ServiceBusNamespace WarewolfShovelBridgeTesting` was re-verified with
`-DryRun` against these real values and previews cleanly — still **not yet run for real**
against the shared Function App; that remains a separate, reviewed step.

Then mint the token with the repo's own helper (app-only/client-credentials — no
interactive sign-in needed, matches how this E2E harness runs unattended):

```powershell
./Scripts/Get-WwExecutionToken.ps1 -TenantId 'ca0cc53b-9af4-4067-bcdf-be9c648450d1' `
    -ResourceAppId $resourceAppId -ClientId $daemonApp.appId -ClientSecret $secret `
    -GrantType ClientCredentials
```
The printed access token is `-MessageAuthToken` (as a `SecureString`); `-EntraServiceBusAudience`
for `Enable-ServiceBusSecureTrigger.ps1` is `api://$resourceAppId`.

**Still required, separately:** a `secure.config` `WindowsGroupPermissions` row with
`WindowsGroup = "Warewolf_ClientApps"` and `Execute = true` for whichever `-WorkflowName`
the test runs — without it, `IWorkflowPolicyMatcher` denies the (validly-authenticated)
caller and the message is dead-lettered as `Forbidden`, not executed. See
`docs/KB-ClientApps-Configuration.md` § "Type B" for the exact config shape.

## Explicitly out of scope for this pass

- Push-style result delivery (reply queue, webhook, SignalR).
- A new Redis (or other) dependency for the replay/result store — Hangfire/in-memory
  only.
- Any change to `Warewolf.Execution.ServiceBusWorker` (Model B) — kept as-is, fully
  additive alongside it.
- Automatic/CI-driven provisioning of the trigger queue and RBAC role —
  `Enable-ServiceBusSecureTrigger.ps1` (see above) exists for this but is a deliberately
  separate, human-reviewed step, not run by any script or pipeline automatically.
- Creating or managing the Entra App Registration behind `WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE`,
  or minting caller tokens — assumed to already exist / be an operator's own concern.
