# Client Examples — wwexecution Azure Function App

Complete, runnable examples for every client type and every OAuth 2.0 flow
supported by the **Warewolf wwexecution** Azure Function App.

---

## Quick Reference

| What you want to do | Use this |
|---|---|
| Test from PowerShell / terminal | [PowerShell script](#powershell-all-flows) |
| Build an Angular SPA | [Angular / MSAL Angular](#angular-spa) |
| Build a React SPA | [React / MSAL React](#react-spa) |
| Build a server-rendered .NET 8 web app | [.NET 8 Web App (MVC)](#net-8-web-app-mvc) |
| Build a .NET 8 desktop / console tool | [.NET 8 Console](#net-8-console-app) |
| Call wwexecution from another Azure Function | [Azure Function client](#azure-function-client) |
| Trigger wwexecution from an Azure Service Bus queue | [Azure Service Bus worker](#azure-service-bus-worker) |

---

## OAuth 2.0 Flows Supported

| Flow | Use case | Token type | Section |
|---|---|---|---|
| **Browser Redirect (Easy Auth)** | Any browser user | Delegated | [A](#a-browser-redirect-easy-auth) |
| **Client Credentials** | Daemon, CI/CD, service | App-only | [B](#b-client-credentials-daemon) |
| **Device Code** | CLI, headless terminal | Delegated | [C](#c-device-code-interactive-cli) |
| **Auth Code + PKCE** | SPA (Angular, React) | Delegated | [D](#d-authorization-code--pkce-spa) |
| **Auth Code (confidential)** | Server-rendered web app | Delegated | [E](#e-authorization-code-confidential) |
| **On-Behalf-Of (OBO)** | Web API → wwexecution | Delegated | [F](#f-on-behalf-of-obo) |
| **Managed Identity** | Azure VM/App Service/AKS | App-only | [G](#g-managed-identity) |
| **Federated Identity** | GitHub Actions / AKS WI | App-only | [H](#h-federated-identity-credentials) |
| **ROPC** | Dev/test only | Delegated | [I](#i-ropc-dev-only) |

---

## Route Summary

| Route | Auth required | Example URL |
|---|---|---|
| `/public/{workflow}.json` | No | `GET /public/Hello%20World.json?Name=Anon` |
| `/secure/{workflow}.json` | Yes | `GET /secure/Hello%20World.json?Name=User` |
| `/services/{workflow}.json` | Yes | `POST /services/ProcessOrder.json` |

---

## Setup: Load Values from Output Files

All examples read from the JSON files produced by the provisioning scripts.

**PowerShell:**

```powershell
# Load server-side output (TenantId, FunctionAppName, ResourceAppId)
$s = Get-Content ./Scripts/Configure-WwExecutionAuth.output.json | ConvertFrom-Json
$TenantId       = $s.TenantId
$ResourceAppId  = $s.ClientId          # the resource app's ClientId
$FunctionAppUrl = "https://$($s.FunctionAppName).azurewebsites.net"

# Load client-side output (SPA, Confidential, Daemon credentials)
$c = Get-Content ./Scripts/Configure-WwExecutionAuth-Clients.output.json | ConvertFrom-Json
$Scope             = $c.Scope           # "api://<ResourceAppId>/.default"
$Authority         = $c.Authority       # "https://login.microsoftonline.com/<TenantId>"
$DaemonClientId    = $c.Clients.Daemon.ClientId
$DaemonSecret      = $c.Clients.Daemon.ClientSecret
$SpaClientId       = $c.Clients.SPA.ClientId
$WebClientId       = $c.Clients.Confidential.ClientId
$WebSecret         = $c.Clients.Confidential.ClientSecret
```

**bash / Git-Bash / WSL:**

```bash
TENANT_ID=$(jq -r '.TenantId'                       ./Scripts/Configure-WwExecutionAuth.output.json)
RESOURCE_APP_ID=$(jq -r '.ClientId'                 ./Scripts/Configure-WwExecutionAuth.output.json)
FUNCTION_APP=$(jq -r '.FunctionAppName'             ./Scripts/Configure-WwExecutionAuth.output.json)
SCOPE=$(jq -r '.Scope'                              ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
DAEMON_CLIENT_ID=$(jq -r '.Clients.Daemon.ClientId' ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
DAEMON_SECRET=$(jq -r '.Clients.Daemon.ClientSecret'./Scripts/Configure-WwExecutionAuth-Clients.output.json)
SPA_CLIENT_ID=$(jq -r '.Clients.SPA.ClientId'       ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
```

---

## PowerShell (All Flows)

**Script:** [`Scripts/Get-WwExecutionToken-AllFlows.ps1`](../Warewolf.Execution.Lightweight/Scripts/Get-WwExecutionToken-AllFlows.ps1)

Covers all 9 flows (A–I) with both PowerShell and `curl.exe` examples.  Auto-loads
values from the provisioning output JSON files when present.

```powershell
# Print all examples (documentation mode — no HTTP calls)
./Scripts/Get-WwExecutionToken-AllFlows.ps1

# Execute a specific flow against the live function app
./Scripts/Get-WwExecutionToken-AllFlows.ps1 -Section B -Execute  # Client Credentials
./Scripts/Get-WwExecutionToken-AllFlows.ps1 -Section C -Execute  # Device Code
```

### A — Browser Redirect (Easy Auth)

No scripting needed. Simply open a `/secure/*` URL in a browser:

```
https://wwexecution.azurewebsites.net/secure/Hello%20World.json?Name=Browser
```

Azure Easy Auth intercepts the request, redirects to Entra login, and returns
with the `X-MS-CLIENT-PRINCIPAL` header set.  The function app processes it automatically.

| Endpoint | Effect |
|---|---|
| `/.auth/login/aad` | Trigger login manually |
| `/.auth/logout` | Sign out |
| `/.auth/me` | Inspect session claims (browser session only) |

---

### B — Client Credentials (Daemon)

```powershell
$TokenEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"

$tokenResp = Invoke-RestMethod -Method Post -Uri $TokenEndpoint `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{
        grant_type    = 'client_credentials'
        client_id     = $DaemonClientId
        client_secret = $DaemonSecret
        scope         = $Scope
    }
$TOKEN = $tokenResp.access_token

# Call the function
Invoke-RestMethod `
    -Uri "$FunctionAppUrl/secure/Hello%20World.json?Name=Daemon" `
    -Headers @{ Authorization = "Bearer $TOKEN" }
```

**curl.exe:**

```bash
TOKEN=$(curl.exe -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=${DAEMON_CLIENT_ID}&client_secret=${DAEMON_SECRET}&scope=${SCOPE}" \
  | jq -r '.access_token')

curl.exe -H "Authorization: Bearer ${TOKEN}" \
  "https://${FUNCTION_APP}.azurewebsites.net/secure/Hello%20World.json?Name=Daemon"
```

---

### C — Device Code (Interactive CLI)

```powershell
$deviceResp = Invoke-RestMethod -Method Post `
    -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/devicecode" `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{ client_id = $SpaClientId; scope = $Scope }

Write-Host $deviceResp.message   # "Go to https://microsoft.com/devicelogin and enter code XXXXXXXX"

$TOKEN = $null
do {
    Start-Sleep -Seconds $deviceResp.interval
    try {
        $tr = Invoke-RestMethod -Method Post `
            -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" `
            -ContentType 'application/x-www-form-urlencoded' `
            -Body @{
                grant_type  = 'urn:ietf:params:oauth:grant-type:device_code'
                client_id   = $SpaClientId
                device_code = $deviceResp.device_code
            }
        $TOKEN = $tr.access_token
    } catch {
        $errBody = $null; try { $errBody = $_.ErrorDetails.Message | ConvertFrom-Json } catch {}
        $errCode = if ($errBody) { $errBody.error } else { '' }
        if ($errCode -in @('authorization_pending','slow_down')) {
            if ($errCode -eq 'slow_down') { Start-Sleep 5 }; continue
        }
        Write-Error "Token failed: $errCode"; break
    }
} until ($TOKEN)

Invoke-RestMethod -Uri "$FunctionAppUrl/secure/Hello%20World.json?Name=User" `
    -Headers @{ Authorization = "Bearer $TOKEN" }
```

---

### D — Authorization Code + PKCE (SPA)

See the [Angular](#angular-spa) and [React](#react-spa) examples — MSAL handles
PKCE automatically.  For manual testing:

```powershell
# Generate PKCE challenge
$bytes         = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$CodeVerifier  = [Convert]::ToBase64String($bytes) -replace '[+/=]',{ @{'+'=>'-';'/'=>'_';'='=>''}[$args[0].Value] }
$sha256        = [Security.Cryptography.SHA256]::Create()
$CodeChallenge = [Convert]::ToBase64String($sha256.ComputeHash([Text.Encoding]::ASCII.GetBytes($CodeVerifier))) `
                   -replace '[+/=]',{ @{'+'=>'-';'/'=>'_';'='=>''}[$args[0].Value] }

# 1. Open this URL in a browser, copy the `code` from the redirect
$AuthUrl = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/authorize" +
           "?client_id=$SpaClientId&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A4200" +
           "&scope=$([Uri]::EscapeDataString($Scope))&code_challenge=$CodeChallenge&code_challenge_method=S256"
Start-Process $AuthUrl

# 2. Exchange the code
$AuthCode  = Read-Host "Paste the code from the redirect URL"
$tokenResp = Invoke-RestMethod -Method Post `
    -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{
        grant_type    = 'authorization_code'
        client_id     = $SpaClientId
        code          = $AuthCode
        redirect_uri  = 'http://localhost:4200'
        code_verifier = $CodeVerifier
        scope         = $Scope
    }
$TOKEN = $tokenResp.access_token
```

---

### E — Auth Code Confidential / F — OBO / G — Managed Identity / H — Federated / I — ROPC

See [`Scripts/Get-WwExecutionToken-AllFlows.ps1`](../Warewolf.Execution.Lightweight/Scripts/Get-WwExecutionToken-AllFlows.ps1)
sections E through I for full PowerShell and `curl.exe` examples.

---

## Angular SPA

**Location:** [`Warewolf.Execution.Lightweight.ClientExamples/Angular17/`](Angular17/)

Uses `@azure/msal-angular` with the `MsalInterceptor` to attach Bearer tokens
automatically to all HTTP calls matching `/secure/*` and `/services/*`.

```bash
# Install
npm install @azure/msal-browser @azure/msal-angular

# Configure
# Edit src/environments/environment.ts — set tenantId, spaClientId, resourceAppId

# Run
ng serve
```

| File | Purpose |
|---|---|
| `src/environments/environment.ts` | Entra ID and function app config |
| `src/app/auth/auth.config.ts` | MSAL instance, guard, interceptor factories |
| `src/app/app.module.ts` | DI wiring of MSAL providers |
| `src/app/app.component.ts` | Login / logout, account info |
| `src/app/workflow/workflow.service.ts` | HTTP calls to wwexecution |
| `src/app/workflow/workflow.component.ts` | Demo UI |
| `src/app/app-routing.module.ts` | Route guard wiring |

**Key pattern:** `MsalInterceptor` + `protectedResourceMap`

```typescript
// auth.config.ts
const protectedResourceMap = new Map([
  [`${functionAppUrl}/secure/`,   [scope]],
  [`${functionAppUrl}/services/`, [scope]],
]);
```

---

## React SPA

**Location:** [`Warewolf.Execution.Lightweight.ClientExamples/React/`](React/)

Uses `@azure/msal-react` with a custom `useWorkflowApi` hook.  MSAL handles
silent token acquisition with automatic interactive fallback.

```bash
npm install @azure/msal-browser @azure/msal-react
# Edit src/authConfig.ts — set clientId, tenantId, resourceAppId
npm start
```

| File | Purpose |
|---|---|
| `src/authConfig.ts` | MSAL config + scopes + function app URL |
| `src/index.tsx` | `MsalProvider` wrapper + active account init |
| `src/App.tsx` | Login / logout, `AuthenticatedTemplate` |
| `src/useWorkflowApi.ts` | Token acquisition + fetch calls hook |
| `src/WorkflowCaller.tsx` | Demo UI component |

**Key pattern:** `useWorkflowApi` hook with `acquireTokenSilent` + redirect fallback

```typescript
const token = await instance.acquireTokenSilent({ scopes, account });
const resp  = await fetch(url, { headers: { Authorization: `Bearer ${token.accessToken}` } });
```

---

## .NET 8 Web App (MVC)

**Location:** [`Warewolf.Execution.Lightweight.ClientExamples/DotNetWebMvc/`](DotNetWebMvc/)

A server-rendered ASP.NET Core MVC app — a **confidential client** — that signs
users in with **Microsoft.Identity.Web** (Authorization Code flow) and calls
wwexecution on the signed-in user's behalf. Tokens are cached server-side and
auto-injected into the downstream call via `ITokenAcquisition` / `IDownstreamApi`.

```bash
# Configure appsettings.json — AzureAd (ClientId, ClientSecret) + WwExecution scopes
dotnet run    # browse https://localhost:5001
```

| File | Purpose |
|---|---|
| `WwExecutionWebMvc.csproj` | Project + Microsoft.Identity.Web packages |
| `appsettings.json` | AzureAd + downstream WwExecution config |
| `Program.cs` | `AddMicrosoftIdentityWebApp().EnableTokenAcquisitionToCallDownstreamApi()` |
| `Services/WwExecutionService.cs` | Token acquisition + HTTP calls (Bearer auto-inject) |
| `Controllers/WorkflowController.cs` | `[Authorize]` demo action + challenge handling |
| `Views/…` | Razor views, login partial, layout |

**Key pattern:** delegated token for `api://<ResourceAppId>/user_impersonation`,
acquired per-request via `ITokenAcquisition.GetAccessTokenForUserAsync` with
incremental-consent (`MicrosoftIdentityWebChallengeUserException`) handling.

---

## .NET 8 Console App

**Location:** [`Warewolf.Execution.Lightweight.ClientExamples/DotNetConsole/`](DotNetConsole/)

Uses **MSAL.NET** (`Microsoft.Identity.Client`) and **Azure.Identity**
(`DefaultAzureCredential`) with a typed `WwExecutionService`.

```bash
# Configure appsettings.json — fill in TenantId, ResourceAppId, client credentials
dotnet run
```

| File | Purpose |
|---|---|
| `WwExecutionClient.csproj` | Project + package references |
| `appsettings.json` | Entra ID + client credential config |
| `WwExecutionClientOptions.cs` | Typed config (bound from appsettings) |
| `TokenAcquirer.cs` | 7 token acquisition flows |
| `WwExecutionService.cs` | HTTP calls to wwexecution |
| `Program.cs` | Demo: runs all examples |

**TokenAcquirer flows:**

| Method | Flow |
|---|---|
| `ClientCredentialsAsync` | App-only (daemon) |
| `DeviceCodeAsync` | Interactive user (headless) |
| `InteractiveAsync` | Browser popup |
| `OnBehalfOfAsync` | OBO — preserves user identity |
| `ManagedIdentityAsync` | `DefaultAzureCredential` (MI in Azure) |
| `RopcAsync` | Username + password (dev only) |
| `TrySilentAsync` | Re-use cached token |

---

## Azure Function Client

**Location:** [`Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/`](AzureFunction/)

A .NET 8 isolated Azure Function that calls wwexecution as a downstream
service.  Uses `DefaultAzureCredential` → **Managed Identity** in Azure
and **az CLI credentials** locally.  No secrets required in production.

```bash
# Local dev
az login
func start

# Test
curl "http://localhost:7071/api/run/Hello%20World?Name=CallerTest"
```

| File | Purpose |
|---|---|
| `WwExecutionCaller.csproj` | Project + packages |
| `Program.cs` | DI, `DefaultAzureCredential`, `AddHttpClient` |
| `WwExecutionCallerOptions.cs` | Typed config |
| `WwExecutionDownstreamService.cs` | Token + HTTP calls (MI + fallback) |
| `Functions/CallWorkflowOnHttpTrigger.cs` | HTTP proxy trigger |
| `Functions/CallWorkflowOnTimer.cs` | Scheduled trigger |
| `local.settings.json` | Local dev settings |
| `host.json` | Function host config |

**Token chain:**

```
DefaultAzureCredential
  → Env vars (AZURE_CLIENT_ID + AZURE_CLIENT_SECRET)
  → Workload Identity (AKS)
  → Managed Identity  ← used in production Azure Function
  → Azure CLI         ← used in local development
  → Visual Studio
Fallback: MSAL Client Credentials (WwExecution:DaemonClientId set)
```

**Required: assign app role to the caller MI:**

```bash
az rest --method POST \
  --url "https://graph.microsoft.com/v1.0/servicePrincipals/<resource-sp-id>/appRoleAssignedTo" \
  --headers "Content-Type=application/json" \
  --body "{\"principalId\":\"<caller-mi-sp-id>\",\"resourceId\":\"<resource-sp-id>\",\"appRoleId\":\"<role-id>\"}"
```

---

## Azure Service Bus worker

**Location:** [`Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/`](AzureServiceBus/)

> **Why a worker?** Azure Service Bus is a message broker — it cannot hold an
> Entra token or make HTTP calls itself. The realistic pattern is a **Service
> Bus–triggered worker** (a .NET 8 isolated Function) that, on each message,
> acquires an **app-only token** and calls wwexecution over HTTP.

A `ServiceBusTrigger` Function reads a `{ "workflow": "...", "inputs": {...} }`
message and calls a secure workflow. Auth is **Managed Identity** via
`DefaultAzureCredential` (client-secret fallback for local dev). The token is
cached/refreshed and auto-injected by a `DelegatingHandler`.

```bash
# Local dev
az login
func start

# Enqueue a test message onto the 'wwexecution-queue' queue:
#   { "workflow": "Hello World", "inputs": { "Name": "FromServiceBus" } }
```

| File | Purpose |
|---|---|
| `WwExecutionServiceBusWorker.csproj` | Isolated-worker project + Extensions.ServiceBus + Azure.Identity |
| `Program.cs` | DI: `TokenCredential`, token handler, typed `HttpClient` |
| `Auth/WwExecutionTokenHandler.cs` | App-only token acquire/cache/refresh + Bearer auto-inject |
| `Functions/WorkflowQueueTrigger.cs` | `[ServiceBusTrigger]` → calls `/secure/{workflow}.json` |
| `WwExecutionClient.cs` | Typed engine client (public/secure/services) |

**Token chain:** identical to the Azure Function client — `DefaultAzureCredential`
(Managed Identity in Azure, az CLI locally), scope `api://<ResourceAppId>/.default`.
The worker's MI/daemon SP **must** be assigned an app role on the resource SP (see
the `az rest … appRoleAssignedTo` snippet above, or
[`Configure-WwExecutionAuth-Clients.ps1 -DaemonUseManagedIdentity`](../Warewolf.Execution.Lightweight/Scripts/Configure-WwExecutionAuth-Clients.ps1)).

---

## Troubleshooting

| Problem | Solution |
|---|---|
| `401` on `/secure/*` | Token `aud` does not match `WAREWOLF_ENTRA_AUDIENCE`. Check `api://<ResourceAppId>` |
| `403` on `/secure/*` | Caller's roles not in `secure.config` for this workflow |
| `AADSTS7000218` (device code) | SPA registration missing `isFallbackPublicClient: true` |
| `consent_required` (device code) | Grant admin consent for `user_impersonation` on SPA registration |
| `AADSTS650057` | Resource app missing `user_impersonation` scope — re-run `Configure-WwExecutionAuth.ps1` |
| `DefaultAzureCredential` fails locally | Run `az login` or set `AZURE_CLIENT_ID` + `AZURE_CLIENT_SECRET` env vars |
| `302 redirect` from API client | Must send `Authorization: Bearer <token>` header — middleware returns 401 for non-browser clients |
| `curl` flags ignored in PowerShell | `curl` in PowerShell is an alias for `Invoke-WebRequest`. Use `Invoke-RestMethod` or `curl.exe` |

---

## See Also

- [`Scripts/Get-WwExecutionToken-AllFlows.ps1`](../Warewolf.Execution.Lightweight/Scripts/Get-WwExecutionToken-AllFlows.ps1) — all flows in one script
- [`Scripts/Configure-WwExecutionAuth.ps1`](../Warewolf.Execution.Lightweight/Scripts/Configure-WwExecutionAuth.ps1) — provision function app + Entra
- [`Scripts/Configure-WwExecutionAuth-Clients.ps1`](../Warewolf.Execution.Lightweight/Scripts/Configure-WwExecutionAuth-Clients.ps1) — provision client registrations
- [`docs/README-Authentication.md`](../Warewolf.Execution.Lightweight/docs/README-Authentication.md) — end-to-end auth architecture
- [`docs/Part5-ClientTokenManagement.md`](../Warewolf.Execution.Lightweight/docs/Part5-ClientTokenManagement.md) — MSAL integration patterns
