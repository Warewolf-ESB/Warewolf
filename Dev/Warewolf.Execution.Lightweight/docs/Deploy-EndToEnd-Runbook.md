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

## 7. (Optional) ExecutionEngineJobProcessor — deploy + authorize

The **ExecutionEngineJobProcessor** (`Warewolf.Execution.EngineJobProcessor`) is the
dedicated timer-driven Function App that replaces `hangfireserver.exe`: it polls Hangfire
SQL storage for **due `Scheduled`** suspend/resume jobs and fire-and-forget POSTs each to
the engine's `/secure/resume/{jobId}` route (managed-identity bearer token), and reaps
stale `Processing` jobs to `Failed` (fail-only). It is a **daemon caller of the engine**,
so it obeys the SAME two-part authorization contract as any client — just with the role
**`Warewolf_JobProcessor`** and the resume route:

- **Token side** — the processor's system-assigned MI must hold the engine app role
  `Warewolf_JobProcessor` (roleless ⇒ HTTP **500**, WOLF-8418).
- **Config side** — the engine's `secure.config` must grant that role a **global-scope**
  (`IsServer=true`) `Execute` row, because the resume route has no per-workflow resource
  entry (see [§2](#2-prepare-the-engines-auth--permission-config)). Add `Warewolf_JobProcessor`
  to the engine's auth config + `secure.config` before this step.

### 7a. Deploy the processor

Publish first (the script does **not** build), then deploy standalone:

```powershell
dotnet publish Dev/Warewolf.Execution.EngineJobProcessor/Warewolf.Execution.EngineJobProcessor.csproj -c Release -o D:\JobProcessor\Publish

.\Deploy-WwJobProcessor.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount stwwjobproc -AppName $JobProcessorApp `
  -PublishPath D:\JobProcessor\Publish `
  -EngineResumeBaseUrl $EngineUrl `
  -EngineResumeScope "api://$ResourceAppId/.default" `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName
  # -PersistenceSettingsPath / -PersistenceDbSourcePath are PROMPTED if omitted
```

…or as a companion of the engine deploy (runs after the engine, reusing the engine's URL +
Key Vault + persistence pair; prompts for anything not passed):

```powershell
.\Deploy-WwExecutionEngine.ps1 ... -EnablePersistence `
  -DeployJobProcessor -JobProcessorAppName $JobProcessorApp `
  -JobProcessorPublishPath D:\JobProcessor\Publish `
  -EngineResumeScope "api://$ResourceAppId/.default"
```

### 7b. Authorize the processor MI (role `Warewolf_JobProcessor`)

Mirror the daemon registration from [§5](#5-register-the-client-as-a-daemon), substituting
the role — `-AppRolesToAssign` **fails loudly** if `Warewolf_JobProcessor` does not exist on
the engine (add it in [§2](#2-prepare-the-engines-auth--permission-config) first):

```powershell
.\Configure-WwExecutionAuth-Clients.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId `
  -ClientType Daemon -DaemonUseManagedIdentity `
  -DaemonFunctionAppName $JobProcessorApp `
  -DaemonFunctionAppResourceGroup $ResourceGroup `
  -AppRolesToAssign Warewolf_JobProcessor `
  -NonInteractive
```

### 7c. Verify

```powershell
# Functions registered (JobPoll + JobReaper timers):
az functionapp function list --name $JobProcessorApp --resource-group $ResourceGroup -o table
```

A successful dispatch shows the engine returning **200** (claimed + executed) or **409**
(benign duplicate); a stale `Processing` job is failed by the reaper — never re-run.

---

## 8. (Optional) RabbitMQ QueueProcessors — deploy + authorize

The **QueueProcessor** (`Warewolf.Execution.QueueProcessor`) is the Linux **container** worker that
replaces `N × QueueWorker.exe` for the Azure path: it consumes one RabbitMQ queue trigger and POSTs
the mapped message to the engine's `/Secure/{workflow}.json` route with a managed-identity token.
It runs on **Azure Container Apps**, autoscaled **0 → N replicas** by the KEDA `rabbitmq` scaler,
**one Container App per queue-trigger**. The on-prem Server + `QueueWorker.exe` path is unchanged.

It is a **daemon caller of the engine**, so it obeys the same two-part authorization contract as any
client — with role **`Warewolf_QueueProcessor`** and **one important difference from the JobProcessor**:

- **Token side** — each Container App's system-assigned MI must hold the engine app role
  `Warewolf_QueueProcessor` (roleless ⇒ HTTP **500**, WOLF-8418). Because there is one app *per
  trigger*, there is one MI per trigger, so the assignment is a **loop** ([§8d](#8d-authorize-each-app-loop)).
- **Config side** — unlike the JobProcessor's **global-scope** (`IsServer=true`) row, a queue worker
  calls a **named workflow**, so `secure.config` needs a **per-workflow** (`IsServer=false`)
  `View=true` + `Execute=true` row for **each** trigger's `WorkflowName`
  (e.g. `ProfilerWrapper\Queue\MandateCollectionSuccessConsume`). Add `Warewolf_QueueProcessor` to the
  engine's auth config + `secure.config` at [§2](#2-prepare-the-engines-auth--permission-config) before
  this step.

> **Alternative to §8a–8c: the combined wrapper.** [`Deploy-WwEngineAndQueueProcessor.ps1`](../Scripts/Deploy-WwEngineAndQueueProcessor.ps1)
> runs the engine deploy and the QueueProcessor deploy as two separate, asserted calls instead of the
> `-DeployRabbitMqTriggers` fan-out below — it verifies the engine summary reached `status=completed`
> and that `Configure-WwExecutionAuth.output.json` belongs to the app just deployed (that file is a
> single fixed path, overwritten by every engine run) before wiring the worker. It writes
> `<LogDir>\deploy-both-<stamp>.handover.json`, which already carries the `spObjectId` and the
> `Warewolf_QueueProcessor` role id that §8d needs — see [`Deploy-Both-RunGuide.md`](Deploy-Both-RunGuide.md).
> Use one path or the other, never both: each provisions its own set of Container Apps.

> **Go-live gate.** Before the first **production** trigger is enabled, all five must hold: the broker
> terminates TLS and the app runs `RABBITMQ__USESSL=true` against `amqps`; a DLX / delivery-limit policy
> exists on the queue; `ENGINE__TIMEOUTSECONDS ≤ WORKER__SHUTDOWNGRACESECONDS < terminationGracePeriodSeconds
> < the engine's functionTimeout` is verified **on the deployed revision**; every trigger declares a
> `DeadLetterQueue`; and App Insights shows the worker's events correlating with the engine's traces.
> Details in the migration plan (Phase 11).

**Timeout chain — defaults and how to verify.** All four values, outermost last:

| Setting | Default | Where it lives |
|---|---|---|
| `ENGINE__TIMEOUTSECONDS` | **180** | Container App env var (`-EngineTimeoutSeconds`) |
| `WORKER__SHUTDOWNGRACESECONDS` | **210** | Container App env var (`-ShutdownGraceSeconds`) |
| `terminationGracePeriodSeconds` | **240** | Container App **template property** (`-TerminationGracePeriodSeconds`) |
| `functionTimeout` | **600** (`00:10:00`) | the **engine's** `host.json` |

Raised from 45/60/90 on 2026-08-11 after 45 s proved under-sized in a live run. Sizing is measured, not
guessed — see the migration plan's §2.6 table.

> ⚠️ **Verify the third one on the live app, not in the plan output.** `terminationGracePeriodSeconds`
> is a template property, not an env var, and until 2026-08-11 the deploy script validated and printed
> it but never sent it to Azure — so deployments silently ran on ACA's 30 s default and SIGKILL could
> arrive mid-drain. Check it explicitly:
>
> ```powershell
> az containerapp show -g <rg> -n <app> -o json |
>     ConvertFrom-Json | ForEach-Object { $_.properties.template.terminationGracePeriodSeconds }
> ```
>
> An empty result means the default is in force and the drain budget is not what the plan claimed.

### 8a. Publish + build

The deploy script does **not** build the .NET project (same rule as the other scripts):

```powershell
dotnet publish Dev/Warewolf.Execution.QueueProcessor/Warewolf.Execution.QueueProcessor.csproj `
  -c Release -o D:\QueueProcessor\Publish
```

The script then builds and pushes the container image **once per run** via `az acr build` (no local
Docker daemon needed) and reuses it for every trigger. To reuse an already-built image, pass
`-Image <acr>.azurecr.io/warewolf/queueprocessor@sha256:<digest>` and omit `-PublishPath`/`-AcrName`.

### 8b. Deploy per trigger — how the trigger file is pointed at the script

Three mutually exclusive modes; `-TriggerId` narrows a folder/manifest to one trigger (the per-trigger
cutover path). **Zero matches is a hard error**, never a silent no-op.

| Parameter | Result |
|---|---|
| `-TriggerFilePath <file>` | one Container App |
| `-TriggerPath <folder>` (+ `-TriggerFilter`, default `triggers*.bite`) | **one Container App per matching file** |
| `-TriggerManifestPath <json>` | one per manifest entry, with per-trigger overrides |

```powershell
# ── Dry run for ONE trigger: prints the plan, changes nothing ────────────────
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -PublishPath D:\QueueProcessor\Publish `
  -TriggerFilePath 'C:\ProgramData\Warewolf\Triggers\Queue\1ac40da8-3b56-45f8-a1aa-00e6864db38b.bite' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl $EngineUrl -EngineResourceAppId $ResourceAppId `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri" `
  -DryRun
```

The plan shows exactly what will be derived from the trigger file, so you can check capacity before
anything is created:

```
    Triggers resolved           : 1
      - wwqp-mandatecollectionsucce-8065   queue='profiler.mandatecollectionsuccess.request' max=5 min=0 value=10 prefetch=10
    Peak cores (max x cpu)      : 2.5
```

`max` = the trigger's **Concurrency**; `min` = 0 (Elastic, the standard); `value` = **Prefetch ×
MaxConcurrency**, i.e. the KEDA target in messages per replica, so
`replicas = ceil(queueLength / value)` capped at `max`.

```powershell
# ── Real deploy of that one trigger ─────────────────────────────────────────
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -PublishPath D:\QueueProcessor\Publish `
  -TriggerFilePath 'C:\ProgramData\Warewolf\Triggers\Queue\1ac40da8-….bite' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl $EngineUrl -EngineResourceAppId $ResourceAppId `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri" `
  -EncryptStagedSettings `
  -LogDir $LogDir -NonInteractive

# ── ALL triggers in a folder: one Container App each, one shared image + env ──
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -AcaEnvironment aca-warewolf `
  -Image "acrwarewolf.azurecr.io/warewolf/queueprocessor@sha256:<digest>" `
  -TriggerPath 'C:\ProgramData\Warewolf\Triggers\Queue' -TriggerFilter 'triggers*.bite' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl $EngineUrl -EngineResourceAppId $ResourceAppId `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri" `
  -EncryptStagedSettings -LogDir $LogDir -NonInteractive
```

> **Fail-fast.** The first failing trigger aborts the loop (already-created apps stay). Pass
> `-ContinueOnTriggerError` to deploy the remainder and exit non-zero with a per-trigger table.
>
> **`Concurrency = 0`** deploys the app with `min = max = 0` — disabled, mirroring the on-prem meaning.
>
> **Scaling exceptions** (`-ScalingMode Fixed|Warm`, or `-MaxReplicas` above `Concurrency`) are printed
> in the plan as `(EXCEPTION: …)` so a deviation from the Elastic standard is visible afterwards.

### 8c. Deploy as an engine companion (one command)

`-DeployRabbitMqTriggers` fans the child script out over **every** trigger it is pointed at, after the
engine deploy — the same pattern as `-DeployJobProcessor` in [§7a](#7a-deploy-the-processor):

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath -AuthConfigPath $AuthConfigPath -SecureConfigPath $SecureConfigPath `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -DeployRabbitMqTriggers `
  -QueueTriggerPath 'C:\ProgramData\Warewolf\Triggers\Queue' `
  -QueueSourcePath  'C:\ProgramData\Warewolf\Resources\Sources' `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -QueueProcessorPublishPath D:\QueueProcessor\Publish `
  -RabbitMqSecretUri "https://$KeyVaultName.vault.azure.net/secrets/rabbitmq-uri" `
  -QueueEngineResourceAppId $ResourceAppId
```

The engine's plan phase lists the fan-out **before** anything is created, and **fails loudly** at plan
time on: zero matching trigger files; any surviving `#{…}` release token (Concurrency must already be
substituted, since `maxReplicas` is derived from it); or a `-QueueProcessorPublishPath` that collides
with the engine's `-PublishPath` **or** the JobProcessor's. Add `-ContinueOnQueueTriggerError` to keep
going past a failing trigger. The `containerapp` CLI extension is installed by the **child**, so an
engine deploy without this switch gains no new prerequisite.

### 8d. Authorize each app (loop)

One MI per app, so iterate the deployed apps — the summary JSON lists them.

> If you deployed via the combined wrapper, take the app list, the engine SP object id and the role
> id straight from its handover file instead of re-querying Graph — the role-grant snippet in
> [`Deploy-Both-RunGuide.md`](Deploy-Both-RunGuide.md) §5 does exactly that.

```powershell
$qpSummary = (Get-ChildItem "$LogDir\deploy-WwQueueProcessor-*.summary.json" |
              Sort-Object LastWriteTime | Select-Object -Last 1).FullName
$apps = (Get-Content $qpSummary | ConvertFrom-Json).queueProcessorApps |
        Where-Object { $_.status -eq 'deployed' }

foreach ($app in $apps) {
    # A Container App's MI principalId is already captured in the summary; if absent, read it:
    $principalId = $app.principalId
    if (-not $principalId) {
        $principalId = az containerapp show --name $app.app --resource-group $ResourceGroup `
                         --query identity.principalId -o tsv
    }

    .\Configure-WwExecutionAuth-Clients.ps1 `
      -ResourceAppId $ResourceAppId -TenantId $TenantId `
      -ClientType Daemon -DaemonUseManagedIdentity `
      -ManagedIdentityObjectId $principalId `
      -AppRolesToAssign Warewolf_QueueProcessor `
      -NonInteractive
}
```

> `Configure-WwExecutionAuth-Clients.ps1` takes `-DaemonFunctionAppName`/`-DaemonFunctionAppResourceGroup`
> for Function Apps, which use `az functionapp identity assign`. A **Container App** MI is read with
> `az containerapp identity show`, so the `-ManagedIdentityObjectId` path above is used instead — it
> works today with **no script change**.

### 8e. Verify

```powershell
# Apps, revisions, and current replica counts
az containerapp list --resource-group $ResourceGroup --query "[?starts_with(name,'wwqp-')].{name:name,replicas:properties.template.scale}" -o table
az containerapp revision list --name wwqp-<slug> --resource-group $ResourceGroup -o table

# Live logs (startup should show the resolved trigger, then 'Consuming queue ...')
az containerapp logs show --name wwqp-<slug> --resource-group $ResourceGroup --follow
```

A healthy cold start logs the resolved configuration and, if the source is not TLS, an explicit
unencrypted-traffic warning:

```
QueueConfigurationLoader resolved trigger 'MandateCollectionSuccessTrigger' (…):
  queue='profiler.mandatecollectionsuccess.request',
  workflow='ProfilerWrapper/Queue/MandateCollectionSuccessConsume',
  prefetch=10, concurrency=5, durable=True, mapEntireMessage=True,
  source=amqp://testuser@server.ngrok.io:20313/, tls=False
Consuming queue 'profiler.mandatecollectionsuccess.request' (prefetch 10, maxConcurrency 1, …)
```

Then publish a message to the queue and confirm the workflow executed on the engine.

| Symptom | Cause | Fix |
|---|---|---|
| Replica exits with `CONFIGURATION ERROR: … unsubstituted release token` | The staged trigger `.bite` still contains `#{…}` | Substitute release variables **before** deploying; the script guards this at plan time too |
| `… is neither plaintext JSON nor WFAES-encrypted` | A Windows **DPAPI** trigger/source reached the container | Re-stage with `-EncryptStagedSettings` (WFAES via the engine's Key Vault key) |
| `500` from the engine on every message | MI lacks `Warewolf_QueueProcessor`, **or** `secure.config` has no per-workflow `Execute` row | [§8d](#8d-authorize-each-app-loop) + [§2](#2-prepare-the-engines-auth--permission-config). Denials are wrapped as **500** (WOLF-8418), not 403 |
| Replicas start but idle while the queue has messages | `value` too small relative to `Prefetch` — the first replica claimed the backlog | Set `value ≈ Prefetch × MaxConcurrency` (the script's default) or lower `Prefetch` |
| Stays at 0 replicas with a backlog | No `-RabbitMqSecretUri`, so the KEDA rule cannot authenticate to the broker | Re-run with the Key Vault secret URI |
| Duplicate executions after a deploy/scale-in | Drain window too short for the workflow | Raise `-ShutdownGraceSeconds` (and `-TerminationGracePeriodSeconds`) or lower `-EngineTimeoutSeconds`. **Check the live value first** — `terminationGracePeriodSeconds` was never applied before 2026-08-11, so an older revision may be on ACA's 30 s default |
| Queue stops draining; one consumer attached, replica healthy, depth static | A delivery was left unacked and `Prefetch=1` blocks every further delivery | Fixed 2026-08-11 — the pump now nacks + requeues and dead-letters once the retry is spent. On an **older image** the only recovery is `az containerapp revision restart`, which requeues the stuck message |
| Everything dead-letters but the queue drains to zero | A business failure is dead-lettered **and acked**, so a drained queue proves nothing | Compare `succeeded` vs `deadLettered` in `Get-WwQueueRunReport.ps1`, never queue depth alone |

---

## 8.5 (Optional) Shovel bridge — Service Bus worker + RabbitMQ shovel

The **shovel bridge** lets an existing on-prem/customer **RabbitMQ** broker feed the engine
without exposing RabbitMQ to Azure or running a queue-worker container per trigger. It has two
first-class parts, both documented in full in
[`docs/ShovelBridge-Architecture.md`](ShovelBridge-Architecture.md):

- **Azure side — `Deploy-WwExecutionServiceBusWorker.ps1`** provisions the Service Bus-triggered
  Function App (`Warewolf.Execution.ServiceBusWorker/`): a Service Bus namespace/queue with
  dead-lettering, Managed Identity listen auth, and a queue-scoped **Send-only SAS rule**
  (`shovel-send` by default) — the credential the RabbitMQ Shovel plugin uses as its AMQP 1.0
  destination. Like the JobProcessor, it is a **daemon caller of the engine** and needs the
  **`Warewolf_ClientApps`** app role + a matching `secure.config` `Execute` row
  ([§2](#2-prepare-the-engines-auth--permission-config)).
- **RabbitMQ side — `Configure-RabbitMqShovel.ps1`** configures a dynamic Shovel (RabbitMQ
  Management HTTP API) that forwards an existing source queue (AMQP 0.9.1) to the Service Bus
  queue above (AMQP 1.0). Requires the `rabbitmq_shovel`/`rabbitmq_shovel_management` plugins
  enabled on the broker (one-time, broker-host admin action).

### 8.5a Deploy the Service Bus worker

Publish first (the script does **not** build) — its publish output **MUST differ** from the
engine's:

```powershell
dotnet publish Dev/Warewolf.Execution.ServiceBusWorker/Warewolf.Execution.ServiceBusWorker.csproj -c Release -o D:\ServiceBusWorker\Publish

.\Deploy-WwExecutionServiceBusWorker.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount stwwsbworker -AppName $ServiceBusWorkerApp `
  -PublishPath D:\ServiceBusWorker\Publish `
  -ServiceBusNamespace $ServiceBusNamespace -ServiceBusQueueName wwexecution-queue `
  -CreateShovelSendRule $true `
  -WwExecutionBaseUrl $EngineUrl -WwExecutionTenantId $TenantId `
  -WwExecutionResourceAppId $ResourceAppId
```

…or as a companion of the engine deploy (runs after the engine, reusing the engine's URL/tenant;
prompts for anything not passed):

```powershell
.\Deploy-WwExecutionEngine.ps1 ... `
  -DeployServiceBusWorker -ServiceBusWorkerAppName $ServiceBusWorkerApp `
  -ServiceBusWorkerPublishPath D:\ServiceBusWorker\Publish `
  -ServiceBusWorkerStorageAccount stwwsbworker
```

`-ServiceBusQueueName` is live-wired to the trigger (not just queue provisioning), and four
`-ServiceBusTrigger*` parameters tune the trigger's binding (concurrency/prefetch/lock-renewal/
auto-complete) without editing `host.json` — both only available on the **standalone** worker
invocation above, not the engine-companion form. See
[`docs/ShovelBridge-Architecture.md`](ShovelBridge-Architecture.md) § "Trigger binding
configuration" for the full list, defaults, and an override-reliability caveat.

### 8.5b Authorize the worker MI (role `Warewolf_ClientApps`)

Mirror the daemon registration from [§5](#5-register-the-client-as-a-daemon) — `-AppRolesToAssign`
**fails loudly** if `Warewolf_ClientApps` does not exist on the engine (it ships in the example
authconfig, see [§2](#2-prepare-the-engines-auth--permission-config)):

```powershell
.\Configure-WwExecutionAuth-Clients.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId `
  -ClientType Daemon -DaemonUseManagedIdentity `
  -DaemonFunctionAppName $ServiceBusWorkerApp `
  -DaemonFunctionAppResourceGroup $ResourceGroup `
  -AppRolesToAssign Warewolf_ClientApps `
  -NonInteractive
```

See `docs/KB-ClientApps-Configuration.md` §2.6 for the worker's own `appsettings.json` shape and
token-acquisition details.

### 8.5c Configure the RabbitMQ shovel

Fetches the destination `shovel-send` SAS key live via `az` — never writes it to disk:

```powershell
.\Configure-RabbitMqShovel.ps1 `
  -RabbitMqManagementUri "https://<broker-host>:15671" `
  -RabbitMqUsername <user> -RabbitMqPassword <SecureString> `
  -SourceQueue <existing-rabbitmq-queue> `
  -ServiceBusNamespace $ServiceBusNamespace -ServiceBusQueueName wwexecution-queue `
  -ServiceBusResourceGroup $ResourceGroup -ServiceBusSasKeyName shovel-send
```

### 8.5d Verify

```powershell
# Shovel running state:
az servicebus queue show --namespace-name $ServiceBusNamespace --resource-group $ResourceGroup --name wwexecution-queue -o table

# Or use the standalone health monitor (schedule via Task Scheduler/cron/Azure Automation —
# RabbitMQ is customer/on-prem infra, not something a Function can reliably poll):
.\Monitor-RabbitMqShovel.ps1 -RabbitMqManagementUri "https://<broker-host>:15671" `
  -RabbitMqUsername <user> -RabbitMqPassword <SecureString> -ShovelName <name>
```

Publish a message to the RabbitMQ source queue and confirm it arrives on the Service Bus queue
and is executed on the engine. A `500` from the engine means the worker's MI lacks
`Warewolf_ClientApps` or `secure.config` has no matching `Execute` row (denials are wrapped as
**500**, WOLF-8418 — same as every other daemon caller in this runbook).

---

## 9. Teardown

```powershell
# Remove the client registration / role assignment (safe to re-run):
.\Remove-WwExecutionAuth-Clients.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId -ClientType Daemon

# Remove the JobProcessor role assignment (if deployed — §7):
.\Remove-WwExecutionAuth-Clients.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId -ClientType Daemon

# Roll back the engine deployment (PREVIEW by default — only removes what the run created):
$summary = (Get-ChildItem "$LogDir\deploy-WwExecutionEngine-*.summary.json" |
            Where-Object { $_.Name -notlike '*dryrun*' } |
            Sort-Object LastWriteTime | Select-Object -Last 1).FullName
.\Rollback-WwExecutionEngine.ps1 -SummaryPath $summary

# Roll back the JobProcessor deployment (same summary schema + run tags):
$jpSummary = (Get-ChildItem "$LogDir\deploy-WwJobProcessor-*.summary.json" -ErrorAction SilentlyContinue |
              Where-Object { $_.Name -notlike '*dryrun*' } |
              Sort-Object LastWriteTime | Select-Object -Last 1).FullName
if ($jpSummary) { .\Rollback-WwExecutionEngine.ps1 -SummaryPath $jpSummary }

# Remove the QueueProcessor Container Apps (§8) — per app, so one trigger can be
# torn down without touching its siblings:
$qpSummary = (Get-ChildItem "$LogDir\deploy-WwQueueProcessor-*.summary.json" -ErrorAction SilentlyContinue |
              Sort-Object LastWriteTime | Select-Object -Last 1).FullName
if ($qpSummary) {
    $qpApps = (Get-Content $qpSummary | ConvertFrom-Json).queueProcessorApps
    foreach ($app in $qpApps) {
        # Role assignment first (the MI disappears with the app):
        if ($app.principalId) {
            .\Remove-WwExecutionAuth-Clients.ps1 `
              -ResourceAppId $ResourceAppId -TenantId $TenantId -ClientType Daemon `
              -ManagedIdentityObjectId $app.principalId
        }
        az containerapp delete --name $app.app --resource-group $ResourceGroup --yes
    }
    # The ACA environment is shared by every queue worker — only delete it when the LAST
    # app is gone and no other Warewolf install uses it:
    # az containerapp env delete --name aca-warewolf --resource-group $ResourceGroup --yes
}

# Remove the shovel bridge (if deployed — §8.5):
# 1. Delete the RabbitMQ shovel itself (broker-side, via the Management API — no script
#    companion; use the RabbitMQ management UI/API DELETE /api/parameters/shovel/{vhost}/{name}).
# 2. Remove the worker's role assignment (safe to re-run):
.\Remove-WwExecutionAuth-Clients.ps1 `
  -ResourceAppId $ResourceAppId -TenantId $TenantId -ClientType Daemon
# 3. Roll back the Service Bus worker deployment (no dedicated Rollback companion, same as the
#    JobProcessor — the run summary JSON records what to tear down manually):
$sbwSummary = (Get-ChildItem "$LogDir\deploy-WwExecutionServiceBusWorker-*.summary.json" -ErrorAction SilentlyContinue |
               Where-Object { $_.Name -notlike '*dryrun*' } |
               Sort-Object LastWriteTime | Select-Object -Last 1).FullName
if ($sbwSummary) { .\Rollback-WwExecutionEngine.ps1 -SummaryPath $sbwSummary }
```

---

## Related docs

- [HangfireDemo-Deploy-Validate-Runbook.md](HangfireDemo-Deploy-Validate-Runbook.md) — deploy **with persistence** + validate the suspend/resume demo (suspend → poll → scheduled resume → manual resumption).
- [Deploy-RunGuide.md](Deploy-RunGuide.md) — full engine-deploy reference (parameters, roles, encryption, troubleshooting).
- [ShovelBridge-Architecture.md](ShovelBridge-Architecture.md) — shovel bridge topology, security model, and troubleshooting (§8.5).
- `Scripts/Configure-WwExecutionAuth-Clients.ps1` / `Configure-WwExecutionAuth-ClientApps.ps1` — client registration (`-?` for help).
- `Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/README.md` — the daemon client sample.
- `docs/KB-ClientApps-Configuration.md` — per-example client configuration reference.
