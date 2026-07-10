# Azure Function Client — calling the Warewolf Execution Engine downstream

A complete, runnable **.NET 8 Azure Functions v4 isolated-worker** app that calls the
**Warewolf Execution Engine** (`wwexecution`) as a downstream service. It demonstrates the
recommended **app-only / Managed Identity** pattern: no secrets at rest in production, token
acquisition and refresh handled automatically, and a typed `HttpClient` that injects the
`Authorization: Bearer` header (plus `x-functions-key` for `/services/*`) on every call.

It exposes four functions (all **`AuthorizationLevel.Anonymous`** — no function key required):

| Function | What it does |
|---|---|
| `CallWorkflowOnHttpTrigger.RunAsync` | HTTP proxy — `GET\|POST /api/run/{*workflow}` forwards the query string to the engine's `/secure/{workflow}.json` and returns the engine's response. Supports folder-qualified workflow names, e.g. `/api/run/data/sales`. |
| `CallWorkflowOnHttpTrigger.RunPublic` | HTTP proxy — `GET\|POST /api/runpublic/{*workflow}` forwards the query string to the engine's anonymous `/public/{workflow}.json` route. |
| `CallWorkflowOnHttpTrigger.GetInfo` | `GET /api/info` — acquires the downstream engine's Bearer token and returns it decoded, as JSON: `{ rawToken, tokenType, expiresOn, header, claims }`. Diagnostic only — see the security note below. |
| `CallWorkflowOnTimer` | Scheduled call — every hour (`0 0 * * * *`) invokes a configured workflow on `/secure`. |

> ⚠️ **All functions are Anonymous** — no `?code=<functionkey>` is required to invoke them. In
> particular, `GET /api/info` returns a **live Bearer token** for the downstream engine to anyone
> who can reach the endpoint. Treat `/api/info` as diagnostic-only and restrict/remove public
> access before exposing this function app in production (network restrictions, APIM, or
> re-scoping it back to `AuthorizationLevel.Function`).

---

## Architecture

```
   Caller / Scheduler                  THIS Azure Function (isolated worker)                 Warewolf Execution Engine
 ┌────────────────────┐        ┌────────────────────────────────────────────────┐        ┌──────────────────────────┐
 │ curl / browser     │  HTTP  │  CallWorkflowOnHttpTrigger  (run/{*workflow})    │        │  Easy Auth + Entra ID    │
 │ /api/run/...       │ ─────► │  CallWorkflowOnTimer        (0 0 * * * *)         │        │  middleware              │
 └────────────────────┘        │                    │                             │        │                          │
                               │                    ▼                             │        │  GET  /public/{wf}.json  │
                               │   IWwExecutionDownstreamService (typed client)   │        │  GET|POST /secure/{wf}   │
                               │                    │                             │ Bearer │  GET|POST /services/{wf} │
                               │                    ▼                             │ ─────► │  GET  /apis.json         │
                               │   HttpClient pipeline                            │  (+x-  └──────────────────────────┘
                               │     └─ WwExecutionTokenHandler (DelegatingHandler)│ funcs-
                               │            │  acquire / cache / refresh token     │  key)
                               │            ▼                                      │
                               │     DefaultAzureCredential  ──► Managed Identity  │
                               │            (fallback) ──────► MSAL client-creds    │
                               └────────────────────────────────────────────────┘
```

**Clean-architecture roles**

| File | Responsibility |
|---|---|
| `WwExecutionCaller.csproj` | Project + package references (Worker, Http/Timer extensions, Azure.Identity, MSAL, Extensions.Http). |
| `WwExecutionCallerOptions.cs` | Strongly-typed config (`BaseUrl`, `TenantId`, `ResourceAppId`, `Scope`, optional `ClientId`/`ClientSecret`, optional `FunctionKey`). |
| `Auth/WwExecutionTokenHandler.cs` | `DelegatingHandler` — acquires/caches/refreshes the app-only token and injects headers. |
| `IWwExecutionDownstreamService.cs` | Typed client contract (`ExecuteSecureAsync` / `ExecuteServicesAsync`) + `WwExecutionResult`. |
| `WwExecutionDownstreamService.cs` | Typed-`HttpClient` implementation — shapes URLs, reads responses. |
| `Functions/CallWorkflowOnHttpTrigger.cs` | HTTP-trigger proxy to `/secure` (`RunAsync`) and `/public` (`RunPublic`), plus the token-inspection endpoint (`GetInfo`). |
| `Functions/CallWorkflowOnTimer.cs` | Timer-trigger scheduled call. |
| `Middleware/ExceptionHandlingMiddleware.cs` | Global `IFunctionsWorkerMiddleware` — converts unhandled exceptions into short/detailed HTTP responses (see "Error handling" below). |
| `Program.cs` | `HostBuilder` + DI: options binding/validation, `TokenCredential`, token handler, typed `AddHttpClient`, exception-handling middleware. |
| `host.json` / `local.settings.json` | Function host + local dev settings. |

---

## Engine routes

| Route | Auth required | Example |
|---|---|---|
| `/public/{workflow}.json` | None | `GET /public/Hello%20World.json?Name=Anon` |
| `/secure/{workflow}.json` | `Authorization: Bearer <token>` | `GET /secure/Hello%20World.json?Name=FromFunction` |
| `/services/{workflow}.json` | Bearer **and** `x-functions-key` | `POST /services/ProcessOrder.json` |
| `/apis.json` | Discovery | `GET /apis.json` |

This sample calls `/secure` from both triggers. To call `/services`, set `WwExecution:FunctionKey`
and use `ExecuteServicesAsync` — the token handler injects `x-functions-key` automatically.

---

## Authentication

- **Authority:** `https://login.microsoftonline.com/{TenantId}`
- **Audience (`aud`):** `api://{ResourceAppId}`
- **App-only scope:** `api://{ResourceAppId}/.default`

### Token chain (`DefaultAzureCredential`)

`WwExecutionTokenHandler` requests the token through `DefaultAzureCredential`, which tries each
source in order and uses the first that succeeds:

```
DefaultAzureCredential
  → Environment vars  (AZURE_CLIENT_ID + AZURE_CLIENT_SECRET + AZURE_TENANT_ID)
  → Workload Identity (AKS / federated)
  → Managed Identity  ← USED IN PRODUCTION (set AZURE_CLIENT_ID for a user-assigned MI)
  → Azure CLI         ← USED IN LOCAL DEV  (run `az login`)
  → Azure PowerShell / Visual Studio / VS Code
Fallback (outside the chain): MSAL ConfidentialClientApplication client-credentials,
  enabled only when WwExecution:ClientId + WwExecution:ClientSecret are configured (local dev).
```

### Token lifecycle & auto-injection

- The handler **caches** the most recent `AccessToken` and only re-acquires when it is missing or
  within **5 minutes** of expiry. Concurrent requests are collapsed behind a `SemaphoreSlim`, so at
  most one token request is in flight.
- On every outgoing request it sets `Authorization: Bearer <token>`. For URLs containing
  `/services/`, it additionally adds `x-functions-key` from `WwExecution:FunctionKey`.
- The typed `HttpClient` (registered in `Program.cs` via
  `AddHttpClient<IWwExecutionDownstreamService, WwExecutionDownstreamService>().AddHttpMessageHandler<WwExecutionTokenHandler>()`)
  carries the engine `BaseAddress`, so call sites only pass a workflow name + query string.

---

## Required: assign an app role to the caller's Managed Identity

The engine's authorization middleware **rejects roleless callers**. The caller's Managed Identity
(or daemon app) must be granted an **app role** on the engine's resource service principal — an
app-only token with no `roles` claim is denied even when the `aud` is correct.

The engine's app roles are the **group names** defined in the deploy auth config
(`GroupPermissions` in `Scripts/Deploy-WwExecutionEngine.authconfig.example.json`, applied by
`Configure-WwExecutionAuth.ps1`) plus the fixed `Permission.*` catalog. The example config ships a
dedicated **`Warewolf_ClientApps`** group for app-only client apps like this one. Authorization is
a two-part contract:

1. **Token side** — the caller's MI holds the group app role, so its token carries
   `roles: ["Warewolf_ClientApps"]`.
2. **Policy side** — the engine's `secure.config` has a `WindowsGroupPermissions` row with
   `WindowsGroup` equal to that role value (per-workflow, `IsServer=false`) granting
   `View`/`Execute` on each workflow this client may call. Permissions always come from
   `secure.config`, not from the token.

Find the IDs, then create the assignment:

```bash
# Resource SP (the engine's app registration) object id
RESOURCE_SP_ID=$(az ad sp show --id "api://<ResourceAppId>" --query id -o tsv)

# App role id exposed by the engine (the group role for client apps)
APP_ROLE_ID=$(az ad sp show --id "api://<ResourceAppId>" \
  --query "appRoles[?value=='Warewolf_ClientApps'].id | [0]" -o tsv)

# Caller's Managed Identity service principal object id
#   system-assigned: read it from the function app's identity
CALLER_MI_SP_ID=$(az functionapp identity show -g <rg> -n <caller-func-app> --query principalId -o tsv)
#   user-assigned:   az identity show -g <rg> -n <mi-name> --query principalId -o tsv

# Assign the role
az rest --method POST \
  --url "https://graph.microsoft.com/v1.0/servicePrincipals/${RESOURCE_SP_ID}/appRoleAssignedTo" \
  --headers "Content-Type=application/json" \
  --body "{\"principalId\":\"${CALLER_MI_SP_ID}\",\"resourceId\":\"${RESOURCE_SP_ID}\",\"appRoleId\":\"${APP_ROLE_ID}\"}"
```

Alternatively let the provisioning scripts do all of the above — **including enabling the
Function App's managed identity (Step 1) and looking up its `principalId`** — for you:

```powershell
# By-type script: enable the client Function App's system-assigned MI, read its
# principalId, and assign the role. -AppRolesToAssign defaults to 'Warewolf_ClientApps'.
Scripts/Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity `
  -DaemonFunctionAppName <caller-func-app> -DaemonFunctionAppResourceGroup <rg>

# …or with an already-known MI SP object id:
Scripts/Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity `
  -ManagedIdentityObjectId <mi-sp-object-id> -AppRolesToAssign "Warewolf_ClientApps"

# …or via the per-example orchestrator:
Scripts/Configure-WwExecutionAuth-ClientApps.ps1 -Apps azurefunction `
  -AzureFunctionClientAppName <caller-func-app> -AzureFunctionClientResourceGroup <rg>
```

All routes assign the role for you and fail loudly if it does not exist on the resource app.
See the parent [ClientExamples README](../README.md).

---

## Setup

### 1. Configure

Edit `local.settings.json` (local) or App Settings (Azure):

| Setting | Value |
|---|---|
| `WwExecution:BaseUrl` | `https://WWExecutionEngine.azurewebsites.net` |
| `WwExecution:TenantId` | Entra tenant GUID |
| `WwExecution:ResourceAppId` | Engine resource app Client ID |
| `WwExecution:Scope` | *(blank → `api://{ResourceAppId}/.default`)* |
| `WwExecution:ScheduledWorkflow` | Workflow invoked by the timer (default `Hello World`) |
| `WwExecution:FunctionKey` | *(only for `/services/*`)* |
| `WwExecution:ClientId` / `:ClientSecret` | *(MSAL fallback, local dev only — leave blank to use `az login`)* |
| `AZURE_CLIENT_ID` | *(user-assigned MI client id in Azure; omit for system-assigned)* |

> ⚠️ **Never commit `local.settings.json` with real secrets.** It is git-ignored (see `.gitignore`).
> In Azure, prefer Managed Identity (no secret) and store any unavoidable secret in Key Vault via
> an App Setting Key Vault reference.

### 2. Local development

```bash
az login                 # populates the Azure CLI credential used by DefaultAzureCredential
func start               # starts the worker on http://localhost:7071
```

### 3. Azure deployment

1. Deploy the function app (Windows Consumption plan — `az functionapp create … --os-type Windows`)
   and **enable a Managed Identity** (system- or user-assigned).
   *(The provisioning scripts can enable the system-assigned MI for you — pass
   `-DaemonFunctionAppName`/`-DaemonFunctionAppResourceGroup`; see "assign an app role" above.)*
2. Grant that identity the engine app role (see above).
3. Set the `WwExecution:*` App Settings (incl. optional `WwExecution:Scope`). Leave
   `ClientId`/`ClientSecret` unset — MI handles auth.

> For the full copy-paste sequence — create the caller app, register its MI as a Daemon, and set its
> App Settings — see the
> [End-to-End Runbook §4–§6](../../Warewolf.Execution.Lightweight/docs/Deploy-EndToEnd-Runbook.md#4-optional-create-the-client-caller-function-app).

---

## Test

```bash
# HTTP proxy → engine /secure/Hello World.json?Name=CallerTest
curl "http://localhost:7071/api/run/Hello%20World?Name=CallerTest"

# Folder-qualified workflow → engine /secure/data/sales.json?Name=CallerTest
curl "http://localhost:7071/api/run/data/sales?Name=CallerTest"

# Anonymous public proxy → engine /public/Hello World.json?Name=CallerTest
curl "http://localhost:7071/api/runpublic/Hello%20World?Name=CallerTest"

# Decoded downstream Bearer token + claims, as JSON
curl "http://localhost:7071/api/info"

# Force the full stack trace instead of the short "{ExceptionType}: {Message}" error body
curl "http://localhost:7071/api/run/Hello%20World?showerror"
```

Expected: the engine's JSON for the Hello World workflow, echoing `Name=CallerTest`. The timer
fires automatically every hour (watch the `func start` console for `CallWorkflowOnTimer` logs).

---

## Error handling

Every function is wrapped by a single global `IFunctionsWorkerMiddleware`
(`Middleware/ExceptionHandlingMiddleware.cs`), so an unhandled exception never surfaces as an
opaque host-level failure:

| Scenario | Response |
|---|---|
| Unhandled exception, HTTP trigger, default | `500` with a short body: `{ExceptionType}: {Message}` |
| Unhandled exception, HTTP trigger, `showerror` present anywhere in the query string | `500` with the full `Exception.ToString()` (type, message, stack trace, inner exceptions) |
| Unhandled exception, `CallWorkflowOnTimer` (no HTTP response to write to) | Logged and swallowed — the next timer tick retries, matching the function's own existing try/catch |

`showerror` is a presence check (case-insensitive) — `?showerror`, `?showerror=1`, and
`?showerror=true` are all equivalent. Omit it (the default) in any environment where stack traces
shouldn't be exposed to callers.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| `401` from engine | Token `aud` mismatch — confirm `WwExecution:ResourceAppId` ⇒ `api://<ResourceAppId>`. |
| `403` / denial (engine may wrap as `500`) | Caller MI has **no app role** on the resource SP — assign one (see above) — or `secure.config` has no `WindowsGroup` row matching the role with `Execute=true` for the requested workflow. |
| `DefaultAzureCredential` fails locally | Run `az login`, or set `WwExecution:ClientId` + `:ClientSecret` for the MSAL fallback. |
| `x-functions-key` missing on `/services` | Set `WwExecution:FunctionKey` and call `ExecuteServicesAsync`. |
| `302` redirect instead of data | Non-browser clients must send `Authorization: Bearer` — the handler does this automatically; verify the token was acquired (check logs). |

---

## See also

- [Parent ClientExamples README](../README.md) — all client types and OAuth flows.
- `Scripts/Configure-WwExecutionAuth.ps1` / `Configure-WwExecutionAuth-Clients.ps1` — provisioning.
- `docs/README-Authentication.md` — end-to-end auth architecture.
