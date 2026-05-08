# README — Authentication & Authorization Setup

End-to-end guide for setting up Azure Entra ID authentication on the **wwexecution** Azure Function App and configuring client applications to call secured workflows.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Azure Function App (wwexecution)                      │
│                                                                              │
│  ┌──────────────────┐   ┌───────────────────────┐   ┌───────────────────┐  │
│  │ EasyAuth Redirect │──▶│ ClaimsPrincipal Builder│──▶│ Workflow Authz     │  │
│  │ Middleware        │   │ Middleware             │   │ Middleware         │  │
│  └──────────────────┘   └───────────────────────┘   └───────────────────┘  │
│         │                        │                           │               │
│    /public/* → pass         Parses:                   Enforces:              │
│    /secure/* → 401/redirect  • X-MS-CLIENT-PRINCIPAL   • secure.config      │
│    /services/* → 401/redirect • Authorization: Bearer   • Group membership   │
│                                                         • Permission flags   │
└─────────────────────────────────────────────────────────────────────────────┘

┌────────────────┐    ┌─────────────────┐    ┌────────────────┐
│  Browser User  │    │  SPA (PKCE)     │    │  Daemon/Service│
│  (Easy Auth)   │    │  (Device Code)  │    │  (Client Creds)│
└───────┬────────┘    └────────┬────────┘    └───────┬────────┘
        │                      │                      │
        ▼                      ▼                      ▼
   /.auth/login/aad     Bearer JWT token       Bearer JWT token
   (redirect flow)      (delegated scope)      (app roles)
```

---

## Step-by-Step Setup

### Step 1: Create the Azure Function App

Create the function app in Azure (if not already done):

```bash
az functionapp create \
  --name wwexecution \
  --resource-group rg-warewolf \
  --storage-account stwarewolf \
  --consumption-plan-location westus2 \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4
```

### Step 2: Provision Entra ID for the Function App

Run the server-side provisioning script to configure the Entra app registration, Easy Auth, app roles, and app settings:

```powershell
az login

./Scripts/Configure-WwExecutionAuth.ps1 `
    -SubscriptionId "<subscription-guid>" `
    -TenantId "<tenant-guid>" `
    -ResourceGroupName "rg-warewolf" `
    -FunctionAppName "wwexecution" `
    -GroupPermissions @{
        'Developers' = @('Permission.View','Permission.Execute','Permission.Contribute')
        'Operators'  = @('Permission.View','Permission.Execute')
    } `
    -NonInteractive -SkipSmokeTest
```

**Output**: `Scripts/Configure-WwExecutionAuth.output.json` with the `ClientId` (Resource App ID).

📖 Full details: [AzureProvisioning-FunctionApp.md](AzureProvisioning-FunctionApp.md)

### Step 3: Deploy the Function App Code

```powershell
cd Warewolf.Execution.Lightweight
dotnet publish -c Release -o ./publish
cd publish
func azure functionapp publish wwexecution
```

### Step 4: Configure secure.config

Upload a `secure.config` that defines workflow-level access policies:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SecureConfig>
  <WindowsGroupPermissions>
    <WindowsGroup Name="Developers">
      <Permissions>
        <Permission Resource="Hello World" View="true" Execute="true" Contribute="true" />
        <Permission Resource="ProcessOrder" View="true" Execute="true" />
      </Permissions>
    </WindowsGroup>
    <WindowsGroup Name="Operators">
      <Permissions>
        <Permission Resource="Hello World" View="true" Execute="true" />
      </Permissions>
    </WindowsGroup>
  </WindowsGroupPermissions>
</SecureConfig>
```

### Step 5: Provision Client App Registrations

Register the client applications that will call the function app:

```powershell
# Get the Resource App ID from the previous step's output
$output = Get-Content ./Scripts/Configure-WwExecutionAuth.output.json | ConvertFrom-Json
$ResourceAppId = $output.ClientId

# Provision all client types
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId $ResourceAppId `
    -TenantId "<tenant-guid>"
```

**Output**: `Scripts/Configure-WwExecutionAuth-Clients.output.json` with client IDs and secrets.

📖 Full details: [AzureProvisioning-ClientApps.md](AzureProvisioning-ClientApps.md)

### Step 6: Acquire Tokens and Call Workflows

> **PowerShell note:** `curl` in PowerShell is an alias for `Invoke-WebRequest`, not the real `curl.exe`.
> All examples below use `Invoke-RestMethod` (native PowerShell) **and** `curl.exe` (bash/Git-Bash/WSL).
> Pick the block that matches your shell.

#### Load variables from output files (PowerShell — run once before any option)

```powershell
# Server-side output (TenantId, FunctionAppName)
$output1 = Get-Content ./Scripts/Configure-WwExecutionAuth.output.json | ConvertFrom-Json
$TenantId       = $output1.TenantId
$FunctionAppUrl = "https://$($output1.FunctionAppName).azurewebsites.net"

# Client-side output (ClientIds, secrets, scope, authority)
$output2        = Get-Content ./Scripts/Configure-WwExecutionAuth-Clients.output.json | ConvertFrom-Json
$ResourceAppId  = $output2.ResourceAppId
$Scope          = $output2.Scope      # "api://<ResourceAppId>/.default"
$Authority      = $output2.Authority  # "https://login.microsoftonline.com/<TenantId>"

$daemon         = $output2.Clients.Daemon
$DaemonClientId = $daemon.ClientId
$DaemonSecret   = $daemon.ClientSecret

$spa            = $output2.Clients.SPA
$SpaClientId    = $spa.ClientId

$web            = $output2.Clients.Confidential
$WebClientId    = $web.ClientId
$WebSecret      = $web.ClientSecret
```

#### Option A: Daemon (Client Credentials) — for services

**PowerShell** *(requires variables loaded above)*
```powershell
$tokenResponse = Invoke-RestMethod `
    -Method Post `
    -Uri "$Authority/oauth2/v2.0/token" `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{
        grant_type    = 'client_credentials'
        client_id     = $DaemonClientId
        client_secret = $DaemonSecret
        scope         = $Scope
    }

$TOKEN = $tokenResponse.access_token

Invoke-RestMethod `
    -Uri "$FunctionAppUrl/secure/Hello%20World.json?Name=Service" `
    -Headers @{ Authorization = "Bearer $TOKEN" }
```

**bash / Git-Bash / WSL**
```bash
# Load from output files (requires jq)
TENANT_ID=$(jq -r       '.TenantId'                         ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
RESOURCE_APP_ID=$(jq -r '.ResourceAppId'                    ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
DAEMON_CLIENT_ID=$(jq -r '.Clients.Daemon.ClientId'         ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
DAEMON_SECRET=$(jq -r    '.Clients.Daemon.ClientSecret'     ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
FUNCTION_APP_NAME=$(jq -r '.FunctionAppName'                ./Scripts/Configure-WwExecutionAuth.output.json)

TOKEN=$(curl -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
  --data-urlencode "grant_type=client_credentials" \
  --data-urlencode "client_id=${DAEMON_CLIENT_ID}" \
  --data-urlencode "client_secret=${DAEMON_SECRET}" \
  --data-urlencode "scope=api://${RESOURCE_APP_ID}/.default" \
  | jq -r '.access_token')

curl -H "Authorization: Bearer ${TOKEN}" \
  "https://${FUNCTION_APP_NAME}.azurewebsites.net/secure/Hello%20World.json?Name=Service"
```

#### Option B: Device Code — for interactive CLI usage

**PowerShell** *(requires variables loaded above)*
```powershell
# Step 1 — request device code
$deviceResponse = Invoke-RestMethod `
    -Method Post `
    -Uri "$Authority/oauth2/v2.0/devicecode" `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{
        client_id = $SpaClientId
        scope     = $Scope
    }

Write-Host $deviceResponse.message   # shows the URL and user code

# Step 2 — poll until the user completes sign-in
do {
    Start-Sleep -Seconds $deviceResponse.interval
    $tokenResponse = try {
        Invoke-RestMethod `
            -Method Post `
            -Uri "$Authority/oauth2/v2.0/token" `
            -ContentType 'application/x-www-form-urlencoded' `
            -Body @{
                grant_type  = 'urn:ietf:params:oauth:grant-type:device_code'
                client_id   = $SpaClientId
                device_code = $deviceResponse.device_code
            }
    } catch { $null }
} until ($tokenResponse)

$TOKEN = $tokenResponse.access_token

Invoke-RestMethod `
    -Uri "$FunctionAppUrl/secure/Hello%20World.json?Name=User" `
    -Headers @{ Authorization = "Bearer $TOKEN" }
```

**bash / Git-Bash / WSL**
```bash
# Load from output files (requires jq)
TENANT_ID=$(jq -r       '.TenantId'                     ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
RESOURCE_APP_ID=$(jq -r '.ResourceAppId'                ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
SPA_CLIENT_ID=$(jq -r   '.Clients.SPA.ClientId'         ./Scripts/Configure-WwExecutionAuth-Clients.output.json)
FUNCTION_APP_NAME=$(jq -r '.FunctionAppName'            ./Scripts/Configure-WwExecutionAuth.output.json)

# Step 1 — request device code
DEVICE=$(curl -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/devicecode" \
  --data-urlencode "client_id=${SPA_CLIENT_ID}" \
  --data-urlencode "scope=api://${RESOURCE_APP_ID}/.default")

echo "$DEVICE" | jq -r '.message'   # shows the URL and user code
DEVICE_CODE=$(echo "$DEVICE" | jq -r '.device_code')

# Step 2 — poll for token after user completes sign-in
TOKEN=$(curl -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
  --data-urlencode "grant_type=urn:ietf:params:oauth:grant-type:device_code" \
  --data-urlencode "client_id=${SPA_CLIENT_ID}" \
  --data-urlencode "device_code=${DEVICE_CODE}" \
  | jq -r '.access_token')

curl -H "Authorization: Bearer ${TOKEN}" \
  "https://${FUNCTION_APP_NAME}.azurewebsites.net/secure/Hello%20World.json?Name=User"
```

#### Option C: Browser — automatic redirect

Simply navigate to `https://wwexecution.azurewebsites.net/secure/Hello%20World.json` in a browser. Azure Easy Auth will redirect to the Microsoft login page and return you to the workflow after authentication.

📖 Full curl examples: [ClientsExamples.md](ClientsExamples.md)

---

## Authentication Flows Supported

| Flow | Client Type | How It Works |
|---|---|---|
| **Browser redirect** | Any browser | Easy Auth intercepts, redirects to `login.microsoftonline.com`, sets `X-MS-CLIENT-PRINCIPAL` on callback |
| **Bearer JWT** | SPA, Web, Daemon | Client acquires token via MSAL/curl, passes `Authorization: Bearer <jwt>`; middleware validates locally using OIDC metadata |
| **Easy Auth + Token** | Hybrid | Easy Auth validates the token at the platform level AND the app validates it again in middleware (defense in depth) |

---

## How Authorization Works

1. **Route classification**:
   - `/public/*` — no auth required
   - `/secure/*` and `/services/*` — token required

2. **Principal extraction** (in order):
   - `X-MS-CLIENT-PRINCIPAL` header (Easy Auth browser flow)
   - `Authorization: Bearer <jwt>` header (API clients)

3. **Policy enforcement** (from `secure.config`):
   - Caller's groups (from token `roles` claim) matched against `WindowsGroup` entries
   - Required permissions checked per-workflow

---

## Route Summary

| Route Pattern | Auth | Example |
|---|---|---|
| `/public/{workflow}.json` | None | `GET /public/Hello%20World.json?Name=Anon` |
| `/secure/{workflow}.json` | Required | `GET /secure/Hello%20World.json?Name=Auth` |
| `/services/{workflow}.json` | Required | `POST /services/ProcessOrder.json` |

---

## Scripts Reference

| Script | Purpose |
|---|---|
| `Configure-WwExecutionAuth.ps1` | Provision function app Entra ID + Easy Auth |
| `Configure-WwExecutionAuth-Clients.ps1` | Provision client app registrations |
| `Get-WwExecutionToken.ps1` | Acquire tokens + call workflows (PowerShell) |
| `Example-ClientApps-OrdersSales.ps1` | End-to-end example with two clients |

---

## Troubleshooting

| Problem | Solution |
|---|---|
| 401 on `/secure/*` with valid token | Check `WAREWOLF_ENTRA_AUDIENCE` matches token `aud` claim |
| 403 with valid token | Add caller's group/role to `secure.config` for that workflow |
| `AADSTS700054` on browser login | Re-run `Configure-WwExecutionAuth.ps1` (fixes ID token issuance) |
| `AADSTS650057` acquiring token | Re-run `Configure-WwExecutionAuth.ps1` (adds user_impersonation scope) |
| `AADSTS7000216` — client_secret required | Variable name contains a dot (e.g. `${daemon.ClientSecret}`). PowerShell dereferences dots as properties; use a plain name like `$DaemonSecret`. Also ensure you are not using the `curl` alias — use `Invoke-RestMethod` or `curl.exe` explicitly. |
| Token has no `roles` claim | Daemon SP needs app role assignments; user needs group assignments |
| 302 redirect from API client | Add `Authorization: Bearer <token>` header (middleware returns 401 for API clients, 302 for browsers) |
| `curl` flags ignored in PowerShell | `curl` in PowerShell is an alias for `Invoke-WebRequest`. Use `Invoke-RestMethod` or call `curl.exe` explicitly. |

---

## Middleware Validation Summary

The auth middleware pipeline correctly supports both authentication modes:

✅ **Non-browser (token-based) clients**: `EasyAuthRedirectMiddleware` detects the absence of browser navigation headers and returns HTTP 401 JSON (not a redirect). `ClaimsPrincipalBuilderMiddleware` validates the Bearer JWT using OIDC discovery keys. `WorkflowAuthorizationMiddleware` enforces secure.config policies.

✅ **Browser-based clients**: Azure Easy Auth (platform-level) intercepts unauthenticated requests and redirects to the Entra login page. After authentication, it injects `X-MS-CLIENT-PRINCIPAL`. `ClaimsPrincipalBuilderMiddleware` parses this header. `WorkflowAuthorizationMiddleware` enforces the same policies.

Both paths produce an identical `WorkflowClaimsPrincipal` with normalized groups and permissions, ensuring consistent authorization regardless of how the caller authenticated.

---

## See Also

- [AzureProvisioning-FunctionApp.md](AzureProvisioning-FunctionApp.md) — detailed function app provisioning
- [AzureProvisioning-ClientApps.md](AzureProvisioning-ClientApps.md) — client registration details
- [ClientsExamples.md](ClientsExamples.md) — complete curl examples
- [Part5-ClientTokenManagement.md](Part5-ClientTokenManagement.md) — MSAL integration patterns
