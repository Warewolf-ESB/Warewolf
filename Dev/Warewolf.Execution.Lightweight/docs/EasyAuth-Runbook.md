# Runbook — `wwexecution`: From Azure Configuration to Deployment & Verified Tests

> Companion to `docs/EasyAuth-Entra-Tutorial.md`. This document is the executable
> sequence — every step has a command, an expected outcome, and a rollback hook.

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  Stage 0  Prerequisites                                                      │
│  Stage 1  Provision Azure resources (RG, Storage, Function App)              │
│  Stage 2  Run Configure-WwExecutionAuth.ps1  (Entra + Easy Auth)             │
│  Stage 3  Build the function (.NET 8 isolated worker)                        │
│  Stage 4  Publish secure.config alongside the package                        │
│  Stage 5  Deploy the function (zip deploy / func azure functionapp publish)  │
│  Stage 6  Smoke tests — anonymous, browser, token                            │
│  Stage 7  Policy enforcement tests — group OR / permission AND               │
│  Stage 8  Observability check (App Insights traces)                          │
│  Stage 9  Rollback / rotate                                                  │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Stage 0 — Prerequisites

| Tool | Minimum version | Verify |
|---|---|---|
| Azure CLI | 2.55 | `az version` |
| .NET SDK | 8.0.100 | `dotnet --version` |
| Azure Functions Core Tools | 4.0.5455 | `func --version` |
| PowerShell | 7.2 | `pwsh --version` |
| Logged-in identity | Owner on the subscription **and** `Application.ReadWrite.All` in the tenant | `az ad signed-in-user show` |

```powershell
az login
az account set --subscription '<your-subscription-id>'
```

Expected: subscription is selected, `az account show` returns it as the active one.

---

## Stage 1 — Provision the Azure resources

Create or confirm the three resources the function depends on. **Skip the steps that already exist**; the script in Stage 2 assumes the Function App `wwexecution` is present.

```powershell
# Variables — must match those at the top of Configure-WwExecutionAuth.ps1
$Loc      = 'southafricanorth'
$Rg       = 'rg-warewolf'
$Stg      = 'stwwexec01'                 # 3-24 lowercase
$AppName  = 'wwexecution'

# 1.1 Resource group
az group create --name $Rg --location $Loc

# 1.2 Storage account (required by Functions)
az storage account create `
    --name $Stg --resource-group $Rg --location $Loc `
    --sku Standard_LRS --kind StorageV2 --min-tls-version TLS1_2

# 1.3 Function App on Consumption (Y1) - Free tier, .NET 8 isolated
az functionapp create `
    --name $AppName --resource-group $Rg --consumption-plan-location $Loc `
    --storage-account $Stg `
    --runtime dotnet-isolated --runtime-version 8 --functions-version 4 `
    --os-type Windows --https-only true

# 1.4 Optional: Application Insights (strongly recommended)
az monitor app-insights component create `
    --app "$AppName-ai" --location $Loc --resource-group $Rg --kind web
$aiKey = az monitor app-insights component show `
    --app "$AppName-ai" --resource-group $Rg --query connectionString -o tsv
# Deploy the connection string under the deliberately non-standard name so the Functions host's
# auto-AI pipeline stays dormant, and turn AI on with ENABLEAPPLICATIONINSIGHTS (the authoritative switch).
az functionapp config appsettings set `
    --name $AppName --resource-group $Rg `
    --settings "WAREWOLF_APPINSIGHTS_CONNECTION_STRING=$aiKey" "ENABLEAPPLICATIONINSIGHTS=true"
```

Expected: `az functionapp show -n $AppName -g $Rg --query state` returns `Running`.

---

## Stage 2 — Provision Entra ID + Easy Auth

Edit the variables at the top of `Scripts/Configure-WwExecutionAuth.ps1` to match Stage 1, then run it from the repository root.

```powershell
./Scripts/Configure-WwExecutionAuth.ps1
```

What happens, in order:

1. **App registration** — creates `wwexecution-auth` (or upgrades existing). Picks up `appId` and the SP object ID.
2. **API exposure** — sets `identifierUris = ["api://<clientId>"]`.
3. **App-role reconcile** — declarative replace of the eight roles (`WarewolfAdministrators`, `PUBLIC`, six `Permission.*`). Existing role IDs are preserved so live assignments stay valid.
4. **Service principal** — creates the SP if absent (required for `appRoleAssignment`).
5. **User assignments** — for each user in `$UserAssignments`, posts an `appRoleAssignment` for the group role plus every permission role mapped to that group.
6. **Client secret** — generates a 1-year secret if no secret survives the next 30 days (or when called with `-RotateSecret`).
7. **Easy Auth** — substitutes `<TENANT_ID>` / `<CLIENT_ID>` into `Scripts/authsettingsV2.json` and PUTs it to `Microsoft.Web/.../authsettingsV2`.
8. **App settings** — sets `WAREWOLF_ENTRA_TENANT_ID`, `WAREWOLF_ENTRA_AUDIENCE`, `WAREWOLF_ENTRA_CLIENT_ID`, `WAREWOLF_SECURE_CONFIG`, and (when rotated) `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET`. `WAREWOLF_ENTRA_CLIENT_ID` carries the bare client GUID as an extra accepted `aud` value — Entra has been observed (2026-08-18) minting tokens whose `aud` is the bare GUID rather than the `api://<clientId>` URI form, and without this setting `EntraAuthOptions.ValidAudiences` only contains the URI form, so every caller is rejected with 401 regardless of role assignment.
9. **Output** — writes `Scripts/Configure-WwExecutionAuth.output.json` containing `clientId`, `audience`, role IDs, and assignment summary.

Expected output (excerpt):

```json
{
  "ClientId":  "8b6e2c44-5c8a-4a9e-9d7b-...",
  "Audience":  "api://8b6e2c44-5c8a-4a9e-9d7b-...",
  "AppRoles": [
    { "value": "WarewolfAdministrators", "id": "..." },
    { "value": "PUBLIC", "id": "..." },
    { "value": "Permission.View", "id": "..." }
    // ...
  ]
}
```

Manual verification:

```powershell
# Easy Auth is on
az webapp auth show --name $AppName --resource-group $Rg `
    --query "{enabled:platform.enabled,clientId:identityProviders.azureActiveDirectory.registration.clientId}"

# App settings are present
az functionapp config appsettings list -n $AppName -g $Rg `
    --query "[?contains(name,'WAREWOLF') || name=='MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'].name"
```

Expected: `enabled = true`, `clientId` matches the script output, all four settings listed.

---

## Stage 3 — Build the function

```powershell
dotnet build .\Warewolf.Execution.Lightweight.csproj -c Release
dotnet publish .\Warewolf.Execution.Lightweight.csproj -c Release -o .\bin\publish
```

Expected: `bin\publish\` contains `host.json`, `worker.config.json`, the function DLLs, and a `Resources/` folder with the workflows. No build warnings about unresolved references in `Auth/*`.

---

## Stage 4 — Publish `secure.config` alongside the package

The runtime locates `secure.config` via:

1. `$env:WAREWOLF_SECURE_CONFIG` if set (preferred — file mount or shared path)
2. `AppContext.BaseDirectory\secure.config` otherwise (deployed alongside binaries)

The example file (`Scripts\secure.config.example.json`) is **plain text**. Two production options:

* **Option A — Encrypted alongside the package.** Open `Warewolf Studio → Settings → Security`, paste the JSON content, save. Studio writes the encrypted `secure.config` blob. Copy the encrypted file into `bin\publish\` before deploying.
* **Option B — Azure File mount.** Create an Azure File share, upload the encrypted `secure.config`, mount it on the Function App at `/mnt/warewolf-config`, and set:

```powershell
az functionapp config appsettings set -n $AppName -g $Rg `
    --settings "WAREWOLF_SECURE_CONFIG=/mnt/warewolf-config/secure.config"
```

The PowerShell setup script defaults `WAREWOLF_SECURE_CONFIG` to `D:\home\site\wwwroot\secure.config` — Option A's path.

> **Sanity check** — `SecureConfigLoader` falls back to `SecureConfigData.AllowAll` (open-access) if decryption fails. After Stage 5, the App Insights startup trace must read `WorkflowAuthPolicyLoader initialised with N workflow policies` where `N >= 1`. If `N == 0`, your file is missing or unencrypted.

---

## Stage 5 — Deploy

Two equivalent options:

```powershell
# Option 1 — Functions Core Tools
cd .\bin\publish
func azure functionapp publish $AppName --dotnet-isolated --no-build

# Option 2 — Zip deploy via az CLI
Compress-Archive -Path .\bin\publish\* -DestinationPath .\bin\publish.zip -Force
az functionapp deployment source config-zip `
    -n $AppName -g $Rg --src .\bin\publish.zip
```

Expected: `Deployment completed successfully` and within 30-60 s the Functions runtime cold-starts. Confirm:

```powershell
az functionapp function list -n $AppName -g $Rg --query "[].name"
```

Should list at least: `ExecuteService`, `ExecuteSecureWorkflow`, `ExecutePublicWorkflow`, `ExecuteRootApisJson`, `ExecuteWorkflowByName`, `ExecuteWorkflow`.

---

## Stage 6 — Smoke tests

### 6.1 Anonymous public route — should always succeed

```bash
curl -i 'https://wwexecution.azurewebsites.net/Public/Hello%20World.json?Name=Sehul'
```

Expected: `HTTP/1.1 200 OK`, JSON body containing the workflow output. No `WWW-Authenticate` header.

### 6.2 Secure route without auth — should return 401

```bash
curl -i 'https://wwexecution.azurewebsites.net/Secure/order.json?id=111'
```

Expected:

```
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer realm="warewolf"
Content-Type: application/json
{"error":"unauthorized","message":"A valid Bearer token is required.","path":"/Secure/order.json"}
```

This proves `EasyAuthRedirectMiddleware` is intercepting before Easy Auth's redirect can fire.

### 6.3 Browser flow — interactive sign-in

1. Open a private browser window.
2. Navigate to `https://wwexecution.azurewebsites.net/Secure/order.json?id=111`.
3. Watch the redirects: `…/Secure/order.json` → `/.auth/login/aad` → `https://login.microsoftonline.com/<tenant>/oauth2/…` → consent → `…/.auth/login/aad/callback` → `…/Secure/order.json`.
4. Sign in as `ashley.lewis@theunlimited.co.za`. **Expected**: `200 OK`, workflow JSON. Easy Auth session cookie set.
5. Sign out, repeat as `aakash.gaikwad@theunlimited.co.za`. **Expected**: `403 forbidden { workflow:"order", required:"View, Execute" }`.

### 6.4 Token flow — non-browser

Acquire a delegated user token (PowerShell shown; same idea with MSAL or `curl`):

```powershell
$ClientId   = '<from Stage 2 output>'
$Audience   = "api://$ClientId/.default"
$Tenant     = '<tenant-guid>'

$resp = Invoke-RestMethod `
    -Uri  "https://login.microsoftonline.com/$Tenant/oauth2/v2.0/token" `
    -Method Post `
    -Body @{
        client_id  = $ClientId
        scope      = $Audience
        username   = 'ashley.lewis@theunlimited.co.za'
        password   = '<password>'
        grant_type = 'password'   # ROPC — testing only; do NOT use in prod
    }
$token = $resp.access_token
```

For app-only:

```powershell
$resp = Invoke-RestMethod `
    -Uri  "https://login.microsoftonline.com/$Tenant/oauth2/v2.0/token" `
    -Method Post `
    -Body @{
        client_id     = $ClientId
        client_secret = '<from Stage 2>'
        scope         = $Audience
        grant_type    = 'client_credentials'
    }
$token = $resp.access_token
```

Call the function:

```powershell
curl.exe -i `
    -H "Authorization: Bearer $token" `
    'https://wwexecution.azurewebsites.net/Secure/order.json?id=111'
```

Expected: same outcomes as 6.3 for the same caller identity. Inspect the JWT at <https://jwt.ms> — `roles` claim should contain `WarewolfAdministrators`, `Permission.View`, `Permission.Execute`, `Permission.Administrator` for ashley.

### 6.5 Discovery endpoints

```bash
curl 'https://wwexecution.azurewebsites.net/Public/apis.json'
curl -H "Authorization: Bearer $token" 'https://wwexecution.azurewebsites.net/Secure/apis.json'
```

Expected: the public list contains only workflows visible to the `Public` group (per `secure.config`); the secure list reflects what the caller can View per `PermissionChecker.HasUserViewPermission`.

---

## Stage 7 — Policy enforcement matrix

Run after 6.4 succeeds. For each row, repeat the curl in 6.4 with that user's token.

| Caller | Workflow | Expected status | Reason |
|---|---|---|---|
| ashley.lewis (admin) | `Secure/order.json` | `200` | Per-workflow row has `View|Execute|Administrator` |
| ashley.lewis (admin) | `Secure/payments.json` | `403 forbidden no policy` | No policy row exists in `secure.config` for `payments` |
| aakash.gaikwad (public) | `Secure/order.json` | `403 forbidden Insufficient group membership` | `PUBLIC` row excluded from `AllowedGroups` (no `Execute`); aakash UPN entry only has `View` |
| Anonymous | `Secure/order.json` | `401 unauthorized` | No principal at all |
| ashley.lewis (admin) | `Public/order.json` | `200` | Public route bypasses all auth |
| aakash.gaikwad (public) | `Public/order.json` | `200` | Public route bypasses all auth |
| Anonymous | `Public/order.json` | `200` | Public route bypasses all auth |

If any row deviates, the next stage tells you why.

---

## Stage 8 — Observability check

```powershell
# Tail App Insights traces (KQL)
az monitor app-insights query `
    --app "$AppName-ai" -g $Rg `
    --analytics-query 'traces
        | where timestamp > ago(15m)
        | where message has_any ("Authorised", "Group check failed",
                                  "Permission check failed", "Principal built")
        | project timestamp, severityLevel, message
        | order by timestamp desc'
```

What to look for:

* `Principal built: User=ashley.lewis@… Authenticated=True Groups=[WarewolfAdministrators, Permission.View, …]` — `ClaimsPrincipalBuilderMiddleware` decoded the Easy Auth header correctly.
* `Authorised 'ashley.lewis@…' for workflow 'order'` — middleware passed.
* `Group check failed for 'aakash.gaikwad@…' on 'order'. Has=[PUBLIC] Allowed=[WarewolfAdministrators, ashley.lewis@…]` — exactly the matrix expected for the negative case.
* `WorkflowAuthPolicyLoader initialised with N workflow policies` (one-shot at startup) — confirms `secure.config` loaded.

If you see `secure.config not loaded` followed by `WorkflowAuthPolicyLoader initialised with 0 workflow policies`, your file is missing or unencrypted; the engine is in open-access mode.

---

## Stage 9 — Rollback / rotate

| Operation | Command |
|---|---|
| Rotate the client secret | `./Scripts/Configure-WwExecutionAuth.ps1 -RotateSecret` |
| Disable Easy Auth temporarily | `az webapp auth update -n $AppName -g $Rg --enabled false` |
| Re-enable Easy Auth | `az webapp auth update -n $AppName -g $Rg --enabled true` |
| Remove all role assignments for a user | `az rest -m DELETE -u "https://graph.microsoft.com/v1.0/users/<oid>/appRoleAssignments/<assignmentId>"` |
| Revert to previous deployment | `az functionapp deployment list -n $AppName -g $Rg` then `az functionapp deployment source config-zip` with the prior package |
| Delete the registration entirely | `az ad app delete --id $ClientId` (then re-run Stage 2) |

---

## End-to-end sequence diagram

```
Browser                    Azure Front-end          Easy Auth          Functions Worker            Entra ID
    │                            │                     │                       │                       │
    │  GET /Secure/order.json    │                     │                       │                       │
    ├───────────────────────────►│                     │                       │                       │
    │                            │ pass-through        │                       │                       │
    │                            ├────────────────────►│                       │                       │
    │                            │                     │ no session cookie     │                       │
    │                            │                     ├──────────────────────►│                       │
    │                            │                     │                       │ EasyAuthRedirectMW    │
    │                            │                     │                       │ → 401 (no Bearer)     │
    │                            │                     │  302 /.auth/login/aad │                       │
    │  302 to login.microsoft…   │  ◄─────────────────┤                       │                       │
    │  ◄─────────────────────────┤                     │                       │                       │
    │  user signs in             │                     │                       │                       │
    ├──────────────────────────────────────────────────────────────────────────────────────────────────►│
    │                                                                                                  │ issues
    │  302 with auth code  ◄──────────────────────────────────────────────────────────────────────────│ id+access tokens
    │  /.auth/login/aad/callback │                     │                       │                       │
    ├───────────────────────────►├────────────────────►│ exchanges code        │                       │
    │                            │                     ├──────────────────────────────────────────────►│
    │                            │                     │  tokens stored        │                       │
    │  302 → /Secure/order.json  │                     │                       │                       │
    │  ◄─────────────────────────┤                     │                       │                       │
    │  GET with Easy Auth cookie │                     │                       │                       │
    ├───────────────────────────►├────────────────────►│  validates cookie,    │                       │
    │                            │                     │  injects X-MS-CLIENT-PRINCIPAL                │
    │                            │                     ├──────────────────────►│                       │
    │                            │                     │                       │ ClaimsPrincipalBuilder│
    │                            │                     │                       │ → WorkflowClaimsPrinc.│
    │                            │                     │                       │ WorkflowAuthorization │
    │                            │                     │                       │ → policy match        │
    │                            │                     │                       │ → workflow execution  │
    │  200 OK + JSON  ◄──────────┤                     │                       │                       │
```

The same diagram applies to the Bearer-token path with two changes: the browser is replaced by the API client, and the `302 → login.microsoft…` step is done out-of-band by that client (or its IdP library) before the original request is sent with `Authorization: Bearer …`.
