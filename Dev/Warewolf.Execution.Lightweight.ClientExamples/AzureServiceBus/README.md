# Warewolf Execution Engine — Azure Service Bus client example

A complete, runnable **.NET 8 Azure Functions v4 isolated-worker** sample that is triggered by an
**Azure Service Bus queue** message and calls the **Warewolf Execution Engine** over HTTP using an
**app-only (client-credentials) token** acquired through **Managed Identity**
(`DefaultAzureCredential`), with a client-secret fallback for local development.

---

## Why a Service Bus-*triggered worker* (and not "Service Bus calls the engine")

Azure Service Bus is a **message broker**. It transports messages between producers and consumers.
It **cannot acquire an OAuth token and it cannot make an outbound HTTP call** to the engine on its
own. So the realistic, production pattern is:

> A **producer** drops a message on a queue → a **compute trigger** (this Functions worker) reads
> the message, **authenticates to Entra ID**, and **calls the engine over HTTP**.

This sample *is* that compute trigger.

```
 ┌────────────┐      enqueue       ┌──────────────────────┐     trigger     ┌─────────────────────────────┐
 │  Producer  │  ───────────────▶  │  Azure Service Bus    │  ────────────▶  │  This Functions worker       │
 │ (any app)  │  {workflow,inputs} │  queue:               │   (message)     │  WorkflowQueueTrigger        │
 └────────────┘                    │  wwexecution-queue    │                 │                             │
                                   └──────────────────────┘                 │  1. parse message            │
                                                                            │  2. get app-only token  ─────┼──┐
                                                                            │  3. call engine /secure  ◀───┼┐ │
                                                                            └─────────────────────────────┘│ │
                                                                                                           │ │ token (client credentials,
                                                                                                           │ │ via Managed Identity)
                                            HTTP GET /secure/{wf}.json + Bearer                            │ ▼
 ┌──────────────────────────────┐  ◀──────────────────────────────────────────────────────────────────────┘
 │ Warewolf Execution Engine     │                                            ┌───────────────────────────┐
 │ WWExecutionEngine.azure...    │  ◀── validates Bearer / Easy Auth ───────  │   Microsoft Entra ID      │
 │ (Entra ID + Easy Auth)        │                                            │ login.microsoftonline.com │
 └──────────────────────────────┘                                            └───────────────────────────┘
```

---

## The engine API

Base URL (configurable): `https://WWExecutionEngine.azurewebsites.net`

| Route | Auth | Notes |
|---|---|---|
| `GET /public/{workflow}.json` | none (anonymous) | Callable per message via `"route": "public"` |
| `GET\|POST /secure/{workflow}.json` | `Authorization: Bearer <token>` (Entra) | **Default** — `"route": "secure"` (or omitted) |
| `GET\|POST /services/{workflow}.json` | Bearer token **and** `x-functions-key` header | Not selectable from the queue; the typed client still supports it |
| `GET /apis.json` | — | Discovery |

The worker chooses the route from each message's optional `route` field (see **Message contract**),
mirroring the AzureFunction sample's `run` (secure) / `runpublic` (public) proxies. Sample calls:

```
GET /secure/Hello%20World.json?Name=FromServiceBus     ("route": "secure" or omitted)
Authorization: Bearer eyJ...

GET /public/Hello%20World.json?Name=FromServiceBus     ("route": "public")
```

---

## Message contract

The queue message body is JSON:

```json
{
  "route": "secure",
  "workflow": "Hello World",
  "inputs": { "Name": "FromServiceBus" }
}
```

- `route` (optional) — target engine route: `secure` (default when omitted/blank) or `public`
  (anonymous). Any other value throws — see **dead-lettering** below.
- `workflow` (required) — the workflow name. Each `/`-separated segment is URL-encoded for you, so
  spaces (`Hello World` → `Hello%20World`) and folder-qualified names (`data/sales` → `data/sales.json`) both work.
- `inputs` (optional) — a string map sent as query-string parameters.

Call a public (anonymous) workflow by setting the route:

```json
{ "route": "public", "workflow": "Hello World", "inputs": { "Name": "FromServiceBus" } }
```

A message with invalid JSON, a missing `workflow`, or an unsupported `route` throws — see
**dead-lettering** below.

---

## Authentication & token lifecycle

- **Authority:** `https://login.microsoftonline.com/{TenantId}`
- **Token audience (`aud`):** `api://{ResourceAppId}` — the engine's app registration
- **App-only scope:** `api://{ResourceAppId}/.default`

### How tokens flow (the centrepiece)

The typed `IWwExecutionClient` never touches tokens. All auth is handled by
[`Auth/WwExecutionTokenHandler.cs`](Auth/WwExecutionTokenHandler.cs), a `DelegatingHandler` attached
to the HTTP client pipeline. On every request it:

1. **Acquires** an app-only token for `api://{ResourceAppId}/.default` via the injected
   `TokenCredential` (`DefaultAzureCredential` → Managed Identity in Azure).
2. **Caches** the token in-memory and **reuses** it until just before expiry. Refresh happens
   `TokenRefreshSkewSeconds` (default 120s) ahead of `ExpiresOn`, single-flighted with a semaphore
   so a cold cache doesn't fan out to Entra.
3. **Injects** `Authorization: Bearer <token>` — and, when `FunctionKey` is set, the
   `x-functions-key` header required by `/services/*`.

### Local dev vs Azure

| Environment | Credential | How |
|---|---|---|
| **Azure** | Managed Identity | `DefaultAzureCredential` resolves the system- or user-assigned MI. Set `WwExecution:ManagedIdentityClientId` for a user-assigned identity. |
| **Local** | az CLI / VS / env | `DefaultAzureCredential` falls through to your developer login. |
| **Local (daemon)** | client secret | Set `WwExecution:UseClientSecretFallback=true` plus `ClientId`/`ClientSecret` → uses `ClientSecretCredential`. |

---

## ⚠️ Required: assign an app role to the caller

The engine's authorization middleware **rejects a roleless caller**. Acquiring a valid token is
**not** sufficient — the caller's identity (the worker's Managed Identity, or your daemon app)
**must be assigned an app role on the engine's resource service principal**.

Use the engine's provisioning script:

```powershell
# Managed Identity caller (the deployed Function App's identity)
./Configure-WwExecutionAuth-Clients.ps1 -DaemonUseManagedIdentity -ManagedIdentityObjectId <mi-object-id>

# or a daemon app (client id + secret) caller
./Configure-WwExecutionAuth-Clients.ps1 -DaemonClientId <app-id>
```

The script creates/assigns the required app role on the engine's resource service principal so the
caller's token carries a `roles` claim the engine accepts. Without it you'll see the engine return a
denial (currently surfaced as **HTTP 500** with a nested `Error{…}` body, pending WOLF-8418), not a
clean 403.

> `Configure-WwExecutionAuth-Clients.ps1` ships with the engine; `-DaemonUseManagedIdentity` wires
> up the MI path and the corresponding app-role assignment.

---

## Configuration

Bound to `WwExecutionOptions` from the `WwExecution` section (`appsettings.json` / app settings /
env vars). See [`local.settings.json`](local.settings.json) for the full set.

| Key | Required | Purpose |
|---|---|---|
| `WwExecution:BaseUrl` | yes | Engine base URL |
| `WwExecution:TenantId` | yes | Entra tenant id |
| `WwExecution:ResourceAppId` | yes | Engine app-registration id (drives audience + scope) |
| `WwExecution:Scope` | no | Override scope; defaults to `api://{ResourceAppId}/.default` |
| `WwExecution:FunctionKey` | no | `x-functions-key` for `/services/*` |
| `WwExecution:UseClientSecretFallback` | no | `true` to use ClientId/ClientSecret locally |
| `WwExecution:ClientId` / `ClientSecret` | no | Daemon credentials (fallback only) |
| `WwExecution:ManagedIdentityClientId` | no | User-assigned MI client id |
| `WwExecution:TokenRefreshSkewSeconds` | no | Refresh lead time (default 120) |
| `ServiceBusConnection` | yes | SB connection string or `…__fullyQualifiedNamespace` for identity-based |

> **`local.settings.json` must never be committed.** It is for local dev only; put real values in
> the Function App's application settings (and secrets in Key Vault) when deployed.

---

## Run it locally

Prerequisites: .NET 8 SDK, Azure Functions Core Tools v4, an Azure Service Bus namespace with a
queue named `wwexecution-queue`, and (for local auth) `az login` or a daemon client secret.

```bash
cd Dev/Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus

# 1. Fill in local.settings.json (tenant, resource app id, SB connection, etc.)
# 2. Build & run the worker
func start
```

### Enqueue a test message

Using the Azure CLI:

```bash
# Secure route (default — route omitted)
az servicebus queue message send \
  --namespace-name <your-namespace> \
  --queue-name wwexecution-queue \
  --body '{ "workflow": "Hello World", "inputs": { "Name": "FromServiceBus" } }'

# Public route (anonymous)
az servicebus queue message send \
  --namespace-name <your-namespace> \
  --queue-name wwexecution-queue \
  --body '{ "route": "public", "workflow": "Hello World", "inputs": { "Name": "FromServiceBus" } }'
```

The worker logs the engine's JSON response. To exercise the engine directly (anonymous, no token):

```
GET https://WWExecutionEngine.azurewebsites.net/public/Hello%20World.json?Name=Direct
```

---

## Dead-lettering

The trigger **does not swallow exceptions**. A bad message (invalid JSON, missing `workflow`, or an
unsupported `route`) or a failed engine call throws; the Functions runtime abandons the message, and after the queue's
max-delivery-count the broker moves it to the **dead-letter sub-queue** for inspection — the correct
behaviour for poison messages.

---

## Files

| File | Purpose |
|---|---|
| `WwExecutionServiceBusWorker.csproj` | Isolated-worker project (net8.0, Exe) |
| `Program.cs` | Host + DI: credential, token handler, typed client, options |
| `WwExecutionOptions.cs` | Strongly-typed configuration |
| `Auth/WwExecutionTokenHandler.cs` | DelegatingHandler — acquire/cache/refresh/inject token |
| `IWwExecutionClient.cs` / `WwExecutionClient.cs` | Typed engine client |
| `Functions/WorkflowQueueTrigger.cs` | Service Bus queue trigger |
| `host.json` / `local.settings.json` / `appsettings.json` | Runtime + config |
