# End-to-End Runbook — Execution Engine + Daemon Client Registration

A single, copy-paste **PowerShell 7 + Azure CLI** runbook that:

1. Deploys the **Warewolf Execution Engine** (Azure Function App) with Entra ID + Easy Auth, and
2. *(optional)* Creates a **client caller** Azure Function App, enables its **system-assigned Managed Identity**, and registers it as a **Daemon** client of the engine so it can call `/secure/*` workflows using Entra ID authentication + authorization.

> This runbook is the "manual, step-by-step" companion to the reference guide
> [`Deploy-RunGuide.md`](Deploy-RunGuide.md). For the full parameter reference, role/privilege
> matrix, encryption/Key Vault detail, and troubleshooting, see that guide. This document strings the
> pieces together into one runnable sequence.

**Two-part authorization contract (read this first).** An app-only (daemon/MI) caller is authorized by **two** things that must both be in place:

- **Token side** — the caller's Managed Identity is assigned the engine app role **`Warewolf_ClientApps`**, so its token carries `roles: ["Warewolf_ClientApps"]`. A **roleless** app-only caller is **rejected** by the engine (denials are wrapped as **HTTP 500**, WOLF-8418).
- **Policy side** — the engine's `secure.config` has a `WindowsGroupPermissions` row with `WindowsGroup = "Warewolf_ClientApps"`, per-workflow (`IsServer=false`), `Execute=true` for each workflow the client may call. Permissions come from `secure.config`, **not** from the token.

The shipped `Deploy-WwExecutionEngine.authconfig.example.json` (GroupPermissions → creates the app role) and `secure.config.example.json` (the matching Execute row) already include `Warewolf_ClientApps`.

---

## 0. Prerequisites

| Tool | Min version | Notes |
|---|---|---|
| PowerShell | **7.0+** | Scripts declare `#Requires -Version 7.0`. Windows PowerShell 5.1 will **not** run them. |
| Azure CLI (`az`) | **2.55.0+** | All cloud provisioning runs through `az`. Must be logged in. |
| Azure Functions Core Tools (`func`) | 4.x | Only if you publish the client app with `func`, or the engine with `-PublishMethod Func`. |

**Roles the signed-in user needs** (see [Deploy-RunGuide §Required roles](Deploy-RunGuide.md#required-roles--privileges) for the full matrix):

- **Contributor** (+ **User Access Administrator** when a Key Vault is involved) on the subscription/RG — control plane.
- **Application Administrator** (or Graph `Application.ReadWrite.All`) — to create/patch Entra app registrations, app roles, and **app-role assignments**.

```powershell
$PSVersionTable.PSVersion   # >= 7.0
az version                  # >= 2.55
az account show             # confirms login
```

> **Run PowerShell 7 as Administrator.** Some `az` and role-assignment operations require elevation.

---

## 1. Session setup (run once)

```powershell
az login

# Resolve tenant + subscription from the current az context
$TenantId         = az account show --query tenantId -o tsv
$SubscriptionId   = az account show --query id -o tsv

# ── Engine targeting ────────────────────────────────────────────────────────
$ResourceGroup    = '<your-resource-group>'      # e.g. DEV2
$Location         = 'southafricanorth'
$StorageAccount   = '<your-storage-account>'     # 3-24 lowercase chars
$AppName          = '<your-function-app-name>'   # engine app, globally unique

# ── Engine inputs (already-published package + config files) ─────────────────
$ScriptsDir              = '<path>\ExecutionEngine\Scripts'
$PublishPath             = '<path>\ExecutionEngine\AzureFunctionsPackage-<version>.zip'  # folder or .zip
$AuthConfigPath          = "$ScriptsDir\Deploy-WwExecutionEngine.authconfig.json"
$SecureConfigPath        = 'C:\ProgramData\Warewolf\Server Settings\secure.config'       # or plaintext JSON (auto-encrypted)
$WorkflowsSourcePath     = 'C:\ProgramData\Warewolf\Resources'
$LicenseConfigPath       = '<path>\ExecutionEngine\Warewolf License.secureconfig'
$ElasticsearchSourcePath = '<path>\ExecutionEngine\ElasticsearchLoggingSource.bite'
$KeyVaultName            = '<your-key-vault-name>'
$KeyVaultSecretName      = '<your-kv-secret-name>'
$ExecutionLogLevel       = 'ERROR'               # TRACE|DEBUG|INFO|WARN|ERROR|FATAL|OFF
$LogDir                  = "$ScriptsDir\logs"

az account set --subscription $SubscriptionId
Set-Location $ScriptsDir
```

> Adjust `$ScriptsDir` if your extracted deployment scripts live elsewhere. Every command below runs
> from `$ScriptsDir`.

---

## 2. Prepare the engine's auth + permission config

The daemon client only works if **`Warewolf_ClientApps`** exists in **both** files below.

1. **Auth config** — copy the example, drop the `_comment`, keep/edit your groups + users. Ensure
   `GroupPermissions` includes `"Warewolf_ClientApps": []`.

   ```powershell
   Copy-Item "$ScriptsDir\Deploy-WwExecutionEngine.authconfig.example.json" $AuthConfigPath
   # then edit $AuthConfigPath: remove the "_comment" block, set your real UPNs/groups.
   ```

2. **secure.config** — your role→workflow permission map. Ensure it has a per-workflow row:
   `WindowsGroup = "Warewolf_ClientApps"`, `IsServer=false`, `View=true`, `Execute=true` for each
   workflow the daemon will call. See `secure.config.example.json` for the shape. A **plaintext** JSON
   passed to the deploy is validated then **auto-AES-encrypted** on staging; an already-encrypted file
   is validated and staged as-is.

> **Sanitized role values.** Entra replaces runs of disallowed characters in an app-role *value* with
> `_`. The `secure.config` `WindowsGroup` must use the **sanitized** value (e.g. `Warewolf ClientApps`
> → `Warewolf_ClientApps`).

---

## 3. Deploy the Execution Engine

**(Optional) Pre-create the engine Function App.** The deploy script provisions the resource
group, storage, and Function App automatically when they don't exist, so you can normally skip
straight to [§3a](#3a-dry-run-no-changes-inspect-the-plan). Pre-create them manually only to
control the hosting plan / OS up front — the engine runs on a **Windows** Consumption (Y1) plan:

```powershell
az group create --name $ResourceGroup --location $Location | Out-Null

az storage account create --name $StorageAccount --resource-group $ResourceGroup `
  --location $Location --sku Standard_LRS --kind StorageV2 --min-tls-version TLS1_2 | Out-Null

az functionapp create --name $AppName --resource-group $ResourceGroup `
  --consumption-plan-location $Location --storage-account $StorageAccount `
  --runtime dotnet-isolated --runtime-version 8 --functions-version 4 `
  --https-only true --os-type Windows | Out-Null
```

### 3a. Dry run (no changes; inspect the plan)

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath `
  -AuthConfigPath $AuthConfigPath `
  -SecureConfigPath $SecureConfigPath `
  -DryRun
```

### 3b. Real deploy

First-time deploy — encrypt sources once, wire Key Vault for runtime decryption, and enable
Application Insights (this is the command from the reference deployment):

```powershell
$ts = Get-Date -Format 'yyyyMMdd-HHmmss'
Start-Transcript -Path "$LogDir\deploy-$ts.log" | Out-Null

.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath `
  -AuthConfigPath $AuthConfigPath `
  -SecureConfigPath $SecureConfigPath `
  -WorkflowsSourcePath $WorkflowsSourcePath `
  -LicenseConfigPath $LicenseConfigPath `
  -EncryptResources:$true -VerifyDecryption `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -EnableAppInsights:$true `
  -ExecutionLogLevel $ExecutionLogLevel `
  -EnableElasticsearch:$false `
  -LogDir $LogDir `
  -PublishMethod Zip `
  -NonInteractive

Stop-Transcript | Out-Null
```

> **Subsequent deploys:** sources are already encrypted — drop `-EncryptResources` and
> `-VerifyDecryption`, but keep `-KeyVaultName`/`-KeyVaultSecretName` so the engine's managed
> identity can still decrypt them at runtime. To turn on Elasticsearch logging, pass
> `-EnableElasticsearch:$true -ElasticsearchSourcePath $ElasticsearchSourcePath` (the file must be
> named exactly `ElasticsearchLoggingSource.bite`).
> For a minimal deploy (no encryption / App Insights) and other variants, see
> [Deploy-RunGuide Examples C–E](Deploy-RunGuide.md#4-worked-parameter-examples).
> **Backtick gotcha:** every continued line except the last needs a trailing `` ` ``. Prefer splatting
> (Deploy-RunGuide Example E) to avoid it.

This provisions: resource group, storage, Function App (`dotnet-isolated` 8, Functions v4, HTTPS-only),
the Entra app registration + service principal + OAuth scope + **app roles (one per GroupPermissions
key, incl. `Warewolf_ClientApps`)**, Easy Auth, app settings, and deploys the package. With the flags
above it also provisions **Application Insights**, and a **Key Vault + system-assigned managed identity**
(granted `Key Vault Secrets User`) so encrypted sources can be decrypted at runtime.

### 3c. Capture the engine identifiers (needed for client registration)

```powershell
$TenantId      = az account show --query tenantId -o tsv
# The engine's ResourceAppId == the Easy Auth (AAD) clientId on the Function App:
$ResourceAppId = az webapp auth show -g $ResourceGroup -n $AppName `
                   --query 'identityProviders.azureActiveDirectory.registration.clientId' -o tsv
$EngineUrl     = "https://$AppName.azurewebsites.net"
"TenantId      = $TenantId"
"ResourceAppId = $ResourceAppId"
"EngineUrl     = $EngineUrl"
```

> **`az webapp auth show` output shape varies by CLI extension version.** Some installs return the
> config with a `properties.` wrapper. If the query above returns blank, use
> `--query 'properties.identityProviders.azureActiveDirectory.registration.clientId'` instead.

### 3d. Smoke-test the engine

```powershell
# Public route (no auth) — should return workflow JSON:
curl "$EngineUrl/public/Hello%20World.json?Name=Anon"
# Secure route without a token — should be 401/redirect (proves auth is enforced):
curl -i "$EngineUrl/secure/Hello%20World.json?Name=Anon"
```

---

## 4. (Optional) Create the client caller Function App

Skip to [§5](#5-register-the-client-as-a-daemon) if you already have a client Function App — note its
name + resource group and pass them there instead.

```powershell
# ── Client caller targeting ──────────────────────────────────────────────────
$ClientRg        = $ResourceGroup                 # or a separate RG
$ClientStorage   = '<client-storage-account>'     # 3-24 lowercase chars, globally unique
$ClientAppName   = '<client-function-app-name>'   # globally unique

# Create RG (idempotent) — only if using a new one
az group create --name $ClientRg --location $Location | Out-Null

# Storage for the client Function App
az storage account create --name $ClientStorage --resource-group $ClientRg `
  --location $Location --sku Standard_LRS --kind StorageV2 --min-tls-version TLS1_2 | Out-Null

# Consumption Function App, .NET 8 isolated, Functions v4, HTTPS-only, Windows plan
az functionapp create --name $ClientAppName --resource-group $ClientRg `
  --storage-account $ClientStorage --consumption-plan-location $Location `
  --runtime dotnet-isolated --runtime-version 8 --functions-version 4 `
  --https-only true --os-type Windows | Out-Null

"Client Function App created: $ClientAppName (rg: $ClientRg)"
```

> The client-caller **sample code** is `Warewolf.Execution.Lightweight.ClientExamples/AzureFunction`.
> Publish it to `$ClientAppName` with `func azure functionapp publish $ClientAppName` (from that
> project folder) or your CI. It authenticates with `DefaultAzureCredential` → Managed Identity in
> Azure, so **no secret** is stored.

---

## 5. Register the client as a Daemon (Entra auth + role)

This performs **your two required steps** automatically:
**(1)** enable the client Function App's system-assigned Managed Identity, and
**(2)** assign it the engine app role so its token carries `roles: ["Warewolf_ClientApps"]`.

`-AppRolesToAssign` **defaults to `Warewolf_ClientApps`** for the daemon path; it **fails loudly** if
that role does not exist on the engine (i.e. if you skipped it in the auth config at [§2](#2-prepare-the-engines-auth--permission-config)).

### Option A — orchestrator (recommended for the AzureFunction example)

```powershell
.\Configure-WwExecutionAuth-ClientApps.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId `
  -Apps azurefunction `
  -AzureFunctionClientAppName $ClientAppName `
  -AzureFunctionClientResourceGroup $ClientRg `
  -FunctionAppName $AppName -FunctionAppResourceGroup $ResourceGroup `
  -NonInteractive
```

### Option B — by-type script (single daemon registration)

```powershell
.\Configure-WwExecutionAuth-Clients.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId `
  -ClientType Daemon -DaemonUseManagedIdentity `
  -DaemonFunctionAppName $ClientAppName `
  -DaemonFunctionAppResourceGroup $ClientRg `
  -NonInteractive
```

> If you already have the MI's SP **object id**, pass `-ManagedIdentityObjectId <id>` instead of the
> Function-App name/RG (and, for Option A, `-AzureFunctionMiObjectId <id>`).

### Option C — pure `az` CLI (manual equivalent, no scripts)

```powershell
# Step 1 — enable the client's system-assigned MI and read its principalId
$CallerPrincipalId = az functionapp identity assign `
  --name $ClientAppName --resource-group $ClientRg `
  --query principalId -o tsv

# Step 2 — resolve the engine SP + the Warewolf_ClientApps app-role id, then assign it
$EngineSpId = az ad sp show --id $ResourceAppId --query id -o tsv
$AppRoleId  = az ad sp show --id $ResourceAppId `
  --query "appRoles[?value=='Warewolf_ClientApps'].id | [0]" -o tsv

if ([string]::IsNullOrWhiteSpace($AppRoleId)) {
    throw "Warewolf_ClientApps app role not found on the engine. Add it to the auth config and re-deploy (see step 2)."
}

$body = @{ principalId = $CallerPrincipalId; resourceId = $EngineSpId; appRoleId = $AppRoleId } |
        ConvertTo-Json -Compress
$bodyFile = [System.IO.Path]::GetTempFileName()
[System.IO.File]::WriteAllText($bodyFile, $body, (New-Object System.Text.UTF8Encoding $false))

az rest --method POST `
  --url "https://graph.microsoft.com/v1.0/servicePrincipals/$EngineSpId/appRoleAssignedTo" `
  --headers "Content-Type=application/json" `
  --body "@$bodyFile"

Remove-Item $bodyFile -Force
```

> The scripts (Options A/B) do exactly this, plus fail-loud role resolution, replication waits, and a
> masked summary JSON — prefer them over Option C unless you need the raw calls.

---

## 6. Configure + test the client caller

Set the client Function App's settings so it targets the engine, then exercise it.

```powershell
az functionapp config appsettings set --name $ClientAppName --resource-group $ClientRg --settings `
  "WwExecution:BaseUrl=$EngineUrl" `
  "WwExecution:TenantId=$TenantId" `
  "WwExecution:ResourceAppId=$ResourceAppId" `
  "WwExecution:Scope=api://$ResourceAppId/.default" | Out-Null
# WwExecution:Scope is optional — it defaults to api://<ResourceAppId>/.default when unset; set it
# explicitly only to target a different scope.
# Leave WwExecution:ClientId / :ClientSecret unset in Azure — the Managed Identity handles auth.
```

Test (the sample app exposes `GET /api/run/{workflow}` which proxies to the engine's `/secure`):

```powershell
curl "https://$ClientAppName.azurewebsites.net/api/run/Hello%20World?Name=FromDaemon"
```

**Expected:** the engine's JSON for the workflow. If you get a denial, see the table below.

| Symptom | Cause | Fix |
|---|---|---|
| `401` from engine | Token `aud` mismatch | Confirm `WwExecution:ResourceAppId` ⇒ `api://<ResourceAppId>`. |
| `403` / `500` denial | Caller MI has **no** `Warewolf_ClientApps` role, **or** `secure.config` has no matching `Execute` row for that workflow | Re-run [§5](#5-register-the-client-as-a-daemon); confirm the `secure.config` row from [§2](#2-prepare-the-engines-auth--permission-config). Denials are wrapped as **500** (WOLF-8418). |
| `DefaultAzureCredential` fails locally | No local identity | `az login` locally, or set `WwExecution:ClientId`/`:ClientSecret` (MSAL fallback, dev only). |

---

## 7. Teardown

```powershell
# Remove the client registration / role assignment (safe to re-run):
.\Remove-WwExecutionAuth-Clients.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId -ClientType Daemon

# Roll back the engine deployment (PREVIEW by default — only removes what the run created):
$summary = (Get-ChildItem "$LogDir\deploy-WwExecutionEngine-*.summary.json" |
            Where-Object { $_.Name -notlike '*dryrun*' } |
            Sort-Object LastWriteTime | Select-Object -Last 1).FullName
.\Rollback-WwExecutionEngine.ps1 -SummaryPath $summary
```

---

## Related docs

- [Deploy-RunGuide.md](Deploy-RunGuide.md) — full engine-deploy reference (parameters, roles, encryption, troubleshooting).
- `Scripts/Configure-WwExecutionAuth-Clients.ps1` / `Configure-WwExecutionAuth-ClientApps.ps1` — client registration (`-?` for help).
- `Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/README.md` — the daemon client sample.
- `docs/KB-ClientApps-Configuration.md` — per-example client configuration reference.
