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

> **Admin consent & public client note:** Device Code flow requires two things on the SPA registration:
> (1) `isFallbackPublicClient: true` — without it Entra ID demands a `client_secret` on the token
> endpoint (`AADSTS7000218`) even though the user authenticated successfully; and
> (2) `user_impersonation` delegated permission with admin consent granted — without it the poll
> receives `consent_required` and loops forever.
> Both are set automatically by the updated provisioning script.
> See [Troubleshooting](#troubleshooting) for fix-it commands if your registration predates the fix.

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
# Only authorization_pending / slow_down should keep the loop going.
# Any other error (consent_required, access_denied, expired_token, etc.) is fatal — break immediately.
$TOKEN = $null
do {
    Start-Sleep -Seconds $deviceResponse.interval
    try {
        $tokenResponse = Invoke-RestMethod `
            -Method Post `
            -Uri "$Authority/oauth2/v2.0/token" `
            -ContentType 'application/x-www-form-urlencoded' `
            -Body @{
                grant_type  = 'urn:ietf:params:oauth:grant-type:device_code'
                client_id   = $SpaClientId
                device_code = $deviceResponse.device_code
            }
        $TOKEN = $tokenResponse.access_token
    } catch {
        # Parse the error body from the exception
        $errBody = $null
        try { $errBody = $_.ErrorDetails.Message | ConvertFrom-Json } catch {}
        $errCode = if ($errBody) { $errBody.error } else { '' }

        if ($errCode -in @('authorization_pending', 'slow_down')) {
            # Normal — user has not yet completed sign-in; keep polling
            if ($errCode -eq 'slow_down') { Start-Sleep -Seconds 5 }
            continue
        }

        # Fatal error — surface it and stop
        $msg = if ($errBody) { "$errCode : $($errBody.error_description)" } else { $_.Exception.Message }
        Write-Error "Token request failed: $msg"
        break
    }
} until ($TOKEN)

if ($TOKEN) {
    Invoke-RestMethod `
        -Uri "$FunctionAppUrl/secure/Hello%20World.json?Name=User" `
        -Headers @{ Authorization = "Bearer $TOKEN" }
} else {
    Write-Warning "No token acquired — check the error above and review admin consent."
}
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
INTERVAL=$(echo "$DEVICE" | jq -r '.interval')

# Step 2 — poll for token; break on fatal errors, continue only on authorization_pending / slow_down
TOKEN=""
while true; do
  sleep "$INTERVAL"
  RESPONSE=$(curl -s -X POST "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
    --data-urlencode "grant_type=urn:ietf:params:oauth:grant-type:device_code" \
    --data-urlencode "client_id=${SPA_CLIENT_ID}" \
    --data-urlencode "device_code=${DEVICE_CODE}")

  ERR=$(echo "$RESPONSE" | jq -r '.error // empty')

  if [ -z "$ERR" ]; then
    # Success
    TOKEN=$(echo "$RESPONSE" | jq -r '.access_token')
    break
  elif [ "$ERR" = "authorization_pending" ]; then
    continue   # User hasn't authenticated yet — keep polling
  elif [ "$ERR" = "slow_down" ]; then
    INTERVAL=$((INTERVAL + 5))
    continue
  else
    # Fatal: consent_required, access_denied, expired_token, etc.
    echo "Token request failed: $ERR — $(echo "$RESPONSE" | jq -r '.error_description')" >&2
    break
  fi
done

if [ -n "$TOKEN" ]; then
  curl -H "Authorization: Bearer ${TOKEN}" \
    "https://${FUNCTION_APP_NAME}.azurewebsites.net/secure/Hello%20World.json?Name=User"
else
  echo "No token acquired — check error above and review admin consent." >&2
fi
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
| `Remove-WwExecutionAuth-Clients.ps1` | Remove client app registrations (cleanup) |
| `Get-WwExecutionToken.ps1` | Acquire tokens + call workflows (PowerShell) |
| `Example-ClientApps-OrdersSales.ps1` | End-to-end example with two clients |

---

## Granting Admin Consent Manually

If the provisioning script printed `[WRN] Admin consent failed (Forbidden)` — which happens when the
logged-in account is not a Global Administrator or Privileged Role Administrator — grant consent
manually using one of the methods below. **This must be done before Device Code or Authorization Code
flows will succeed.**

### Method 1 — Azure Portal (recommended for non-admins)

1. Go to **Azure Portal → Microsoft Entra ID → App registrations**
2. Open the client app (e.g. `wwexecution-spa` or `wwexecution-web`)
3. Select **API permissions** in the left menu
4. Click **Grant admin consent for \<your tenant\>**
5. Confirm the dialog — all listed permissions turn green with a ✓

Repeat for each client app that received the `[WRN]`.

### Method 2 — Azure CLI (Global Admin required)

```powershell
# Grant admin consent for a specific app by its client ID
az ad app permission admin-consent --id <client-app-id>
```

### Method 3 — User consent (for single-tenant dev tenants without an admin)

If your tenant allows user consent, the first user to sign in via Device Code or browser will be
prompted by Microsoft to consent on their own behalf. This works only when your tenant's
**user consent settings** allow consent for verified publishers or all apps.

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
| **`AADSTS7000218` during Device Code token poll** | The SPA app registration is missing `isFallbackPublicClient: true`. Without it Entra ID treats the app as confidential and demands a `client_secret` on the token endpoint even for public flows. **Fix:** re-run `Configure-WwExecutionAuth-Clients.ps1 -ClientType SPA` — the script now sets `isFallbackPublicClient: true` in the same PATCH that configures `spa.redirectUris`. To fix an existing registration without re-running: `az rest --method PATCH --url "https://graph.microsoft.com/v1.0/applications/<object-id>" --headers "Content-Type=application/json" --body '{"isFallbackPublicClient":true}'` |
| **Device Code poll loop runs forever** | The loop received a fatal error that was silently swallowed. The fixed loop in this doc checks `error_code` and breaks on anything other than `authorization_pending` / `slow_down`. Common causes: **`AADSTS7000218`** (`isFallbackPublicClient` not set — see above) or **admin consent not granted** (`consent_required` — see below). |
| `consent_required` after user authenticates | Admin consent was not granted for the SPA (or Confidential) client's `user_impersonation` permission. Grant it via Portal or CLI — see [Granting Admin Consent Manually](#granting-admin-consent-manually). |
| `[WRN] Admin consent failed (Forbidden)` during provisioning | The signed-in account is not a Global Administrator. Grant consent manually — see [Granting Admin Consent Manually](#granting-admin-consent-manually). |

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
