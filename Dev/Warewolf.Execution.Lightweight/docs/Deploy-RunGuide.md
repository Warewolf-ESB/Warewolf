# Deploy-WwExecutionEngine — Step-by-Step Run Guide

End-to-end guide for running
[`Scripts/Deploy-WwExecutionEngine.ps1`](../Scripts/Deploy-WwExecutionEngine.ps1) — the
orchestrator that provisions Azure infrastructure, configures auth, stages + encrypts the package
contents, applies the engine's environment variables, and deploys an **already-published** package
to a Warewolf Workflow Execution Engine Azure Function App.

> **Download the Warewolf Azure Execution Engine.** Download the Warewolf Azure Execution Engine build from
> [warewolf.io/release-notes](https://warewolf.io/release-notes), extract the `.zip`, and
> point the extracted folder (or the `.zip` itself) via `-PublishPath` to the Deployment Script (`Scripts/Deploy-WwExecutionEngine.ps1`).

> **Download the Deployment Scripts.** Download the Warewolf Azure Execution Engine deployment scripts from
> [warewolf.io/release-notes](https://warewolf.io/release-notes), and extract the `.zip`
> so that the scripts land in `D:\ExecutionEngine\Scripts`.


**Contents**

1. [Prerequisites](#1-prerequisites)
2. [Step-by-step execution](#2-step-by-step-execution)
3. [Parameter reference](#3-parameter-reference)
4. [Worked parameter examples](#4-worked-parameter-examples)
5. [Resources: created by the script vs. assumed pre-existing](#5-resources-created-by-the-script-vs-assumed-pre-existing)
6. [Troubleshooting](#6-troubleshooting)

---

## 1. Prerequisites

| Tool | Minimum version | Why it's needed | Download / install |
|---|---|---|---|
| **PowerShell** | **7.0+** (latest recommended) | The script declares `#Requires -Version 7.0`; uses `Set-StrictMode -Version Latest` and PowerShell 7 ternary operators. Windows PowerShell 5.1 will **not** run it. | [github.com/PowerShell/PowerShell/releases](https://github.com/PowerShell/PowerShell/releases/download/v7.6.3/PowerShell-7.6.3-win-x64.msi) · or `winget install Microsoft.PowerShell` |
| **Azure CLI (`az`)** | **2.55.0+** (latest recommended) | All cloud provisioning (resource group, storage, Function App, App Insights, Key Vault, app settings, zip-deploy) runs through `az`. Must be logged in (`az login`). | [aka.ms/installazurecliwindows](https://aka.ms/installazurecliwindowsx64) · or `winget install Microsoft.AzureCLI` |
| **Azure Functions Core Tools (`func`)** | **4.x** | **Only** required when you choose `-PublishMethod Func` (advanced/opt-in). The default `Auto`/`Zip` path uses `az` zip-deploy and does **not** need `func`. | [github.com/Azure/azure-functions-core-tools](https://go.microsoft.com/fwlink/?linkid=2174087) · or `winget install Microsoft.Azure.FunctionsCoreTools` |

**Azure-side prerequisites**

- An Azure subscription to deploy into.
- You must be **logged in** and pointed at the right subscription (see Step 0 below).
- The signed-in user must hold the roles below — see [Required roles & privileges](#required-roles--privileges).

### Required roles & privileges

The deploy chain touches **three independent planes** (Azure RBAC, Key Vault data plane, and
Microsoft Entra ID). Azure RBAC roles never grant Entra permissions and vice-versa, so the user
needs grants from each plane that applies to the flags you run with. The script grants itself only
the Key Vault **data-plane** role — every other role below must already be on the user before you run.

| Plane | Role | When needed | Why |
|---|---|---|---|
| **Azure RBAC** (control) | **Contributor** at **subscription** scope (or RG scope if the RG already exists) | Always | Creates the resource group, storage, Function App, App Insights, managed identity, app settings, zip-deploy, and Easy Auth. Creating the RG itself needs subscription scope. |
| **Azure RBAC** (control) | **User Access Administrator** (or **Owner** / **Role Based Access Control Administrator**), scope covering the Key Vault (RG or subscription) | When a Key Vault is in play (`-EncryptResources`, or any `-KeyVaultName`) | The script runs `az role assignment create` (`Microsoft.Authorization/roleAssignments/write`), which **Contributor does not include**. This is the most commonly-missed grant. |
| **Key Vault** (data) | **Key Vault Secrets Officer** on the vault | When a Key Vault is in play | Reads/writes the AES-key secret. The deploy **auto-assigns this to the signed-in user** — *but only if the user already has User Access Administrator / Owner above.* Pre-grant it manually if not. |
| **Microsoft Entra** (directory) | **Application Administrator** (or Graph `Application.ReadWrite.All`) | Unless `-SkipAuthProvisioning` | Creates/updates the Entra app registration, service principal, OAuth scope, app roles, and client secret. |
| **Microsoft Entra** (directory) | Directory **read users** + write user **appRoleAssignments** (covered by Application Administrator + default directory read) | When `UserAssignments` are provided (Stage 6) | Looks up each user by UPN and assigns app roles. Avoidable with `-SkipUserAssignment` / empty `UserAssignments`. |

> **Minimal path.** With `-SkipAuthProvisioning` **and** no Key Vault (no encryption), **Contributor alone**
> is enough — no User Access Administrator, no Entra role.

> **Owner shortcut.** Granting **Owner** at subscription scope covers both Contributor and User Access
> Administrator in one role. It does **not** cover the Entra (directory) roles — those are always separate.

**Grant commands (an Admin runs these for the deploying user)**

```powershell
# --- Resolve the target user's object id ---
$UserOid = az ad user show --id 'deployer@yourtenant.com' --query id -o tsv
$Sub     = '<your-subscription-id>'   # e.g. 00000000-0000-0000-0000-000000000000
$Rg      = '<your-resource-group>'
$Vault   = '<your-key-vault-name>'

# === A. Azure RBAC (control plane) ===
# Simplest: Owner at subscription = Contributor + role-assignment rights in one grant
az role assignment create --assignee $UserOid --role 'Owner' --scope "/subscriptions/$Sub"

# --- OR least-privilege: the two roles separately ---
az role assignment create --assignee $UserOid --role 'Contributor' --scope "/subscriptions/$Sub"
az role assignment create --assignee $UserOid --role 'User Access Administrator' --scope "/subscriptions/$Sub"
# (If the RG already exists you may scope these to
#  "/subscriptions/$Sub/resourceGroups/$Rg" instead — but RG *creation* needs subscription scope.)

# === B. Key Vault data plane (optional pre-grant; otherwise the deploy self-grants it
#         IF the user has User Access Administrator / Owner. The vault must exist for this scope to resolve.) ===
az role assignment create --assignee $UserOid --role 'Key Vault Secrets Officer' `
  --scope "/subscriptions/$Sub/resourceGroups/$Rg/providers/Microsoft.KeyVault/vaults/$Vault"

# === C. Entra directory role: Application Administrator ===
# roleDefinitionId 9b895d92-2cd3-44c7-9d02-a6ac2d5ea5c3 = Application Administrator (fixed template id)
az rest --method POST `
  --url 'https://graph.microsoft.com/v1.0/roleManagement/directory/roleAssignments' `
  --headers 'Content-Type=application/json' `
  --body "{`"principalId`":`"$UserOid`",`"roleDefinitionId`":`"9b895d92-2cd3-44c7-9d02-a6ac2d5ea5c3`",`"directoryScopeId`":`"/`"}"
```

> The admin running **step C** must themselves be **Privileged Role Administrator** or **Global
> Administrator**. Azure RBAC grants (steps A/B) require **Owner** or **User Access Administrator** at
> the target scope.

**Encryption-specific prerequisite**

- `-EncryptResources` / `-VerifyDecryption` re-encrypt sources from plain/DPAPI to WFAES. **DPAPI-encrypted
  `.bite` sources can only be re-encrypted on the machine that originally created them.**

**Verify your toolchain**

```powershell
$PSVersionTable.PSVersion      # >= 7.0
az version                     # azure-cli >= 2.55
func --version                 # 4.x  — only if you will use -PublishMethod Func
az account show                # confirms you are logged in
```

---

## 2. Step-by-step execution

### Step 0 — Log in and select the subscription

> **Run PowerShell 7+ as Administrator.** Right-click the PowerShell 7 shortcut and choose
> **Run as administrator** — some Azure CLI and role-assignment operations require elevated privileges.

> All examples given in this RunGuide assume that Deployment Scripts are located at: `D:\ExecutionEngine\Scripts`


```powershell
# PowerShell 7+ (run as Administrator), Azure CLI installed and logged in
az login

# ── Set your deployment values ONCE — every example in this guide reuses these ──
# Run this whole block in your session first; later snippets reference the $vars.
$SubscriptionId          = '<your-subscription-id>'      # e.g. 00000000-0000-0000-0000-000000000000
$ResourceGroup           = '<your-resource-group>'       # e.g. DEV2
$Location                = 'southafricanorth'
$StorageAccount          = '<your-storage-account>'      # 3-24 lowercase chars
$AppName                 = '<your-function-app-name>'    # globally unique
$PublishPath             = 'D:\ExecutionEngine\AzureFunctionsPackage-3.0.1.200'
$AuthConfigPath          = 'D:\ExecutionEngine\Deploy-WwExecutionEngine.authconfig.json'
$SecureConfigPath        = 'C:\ProgramData\Warewolf\Server Settings\secure.config'
$WorkflowsSourcePath     = 'C:\ProgramData\Warewolf\Resources'
$LicenseConfigPath       = 'D:\ExecutionEngine\Warewolf License.secureconfig'
$KeyVaultName            = '<your-key-vault-name>'
$KeyVaultSecretName      = '<your-kv-secret-name>'
$ElasticsearchSourcePath = 'D:\ExecutionEngine\ElasticsearchLoggingSource.bite'
$LogDir                  = 'D:\ExecutionEngine\Scripts\logs'

az account set --subscription $SubscriptionId
cd D:\ExecutionEngine\Scripts
```

> **Set values once.** The block above defines every reused value as a `$variable`; all the
> deploy snippets below (Step 1 and Examples A–F) reference those variables, so you only edit a
> value in one place. Run the block in the same PowerShell session before the snippets.

> The script can resolve `SubscriptionId` / `TenantId` from `az account show` when you omit them, so
> setting the active subscription here is what targets the deployment.

### Step 1 — Deploy FOR REAL (creates billable resources)

```powershell
$ts = Get-Date -Format 'yyyyMMdd-HHmmss'
Start-Transcript -Path ".\logs\deploy-$ts.log" | Out-Null

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
  -EnableElasticsearch:$true -ElasticsearchSourcePath $ElasticsearchSourcePath `
  -LogDir $LogDir `
  -PublishMethod Zip

Stop-Transcript | Out-Null
```

> ⚠️ **Line-continuation gotcha.** Every line of a multi-line PowerShell call **except the last** must
> end with a backtick `` ` ``. In the original snippet the `-LogDir …` line was missing its trailing
> backtick, so `-PublishMethod Zip` would be parsed as a *separate statement* and silently dropped
> (the deploy would fall back to `-PublishMethod Auto`). The block above has the backtick added — keep
> it. Easiest way to avoid this entirely is **splatting** (see [§4](#4-worked-parameter-examples)).

**What this run does (6 phases):**

| Phase | Action |
|---|---|
| 0 / 0.5 | Pre-flight (`az` login, resolve subscription/tenant) → **PLAN**: resolve every decision, print a masked summary, ask once to proceed |
| 1 | Create resource group, storage, Function App, Application Insights |
| 2 | Provision Microsoft Entra ID app registration, service principal, OAuth scope, app roles, and Easy Auth from the `-AuthConfigPath` JSON |
| 3 | Stage secure.config (validate / auto-encrypt), workflow resources (+ optional WFAES encryption), Elasticsearch source, **generate `Resources\workflow-index.json`** (bundled into the zip), env vars |
| 4 | Deploy the package to the Function App (zip-deploy) |
| 5 | Verify — endpoint banner + optional HTTP probe |

**Outputs (written to `-LogDir`):**

- A timestamped **transcript** log.
- A masked **`*.summary.json`** — written **before the first mutation, after every phase, and on
  failure** (crash-safe). It records `status` (`in-progress`/`completed`/`failed`), `lastPhase`,
  `error`, and a `created` map (`true` = this run created it, `false` = pre-existing). The rollback
  script consumes this file — see Step 2.

> **Dry run first (recommended).** Add `-DryRun` to print every mutating action without executing it.
> Read-only probes still run; a `.dryrun.` transcript + summary are still written so you can inspect
> the plan.

### Step 2 — Rollback the real run (preview teardown)

First locate the **real** run's summary (exclude dry-run files):

```powershell
$summary = (Get-ChildItem (Join-Path $LogDir 'deploy-WwExecutionEngine-*.summary.json') |
            Where-Object { $_.Name -notlike '*dryrun*' } |
            Sort-Object LastWriteTime | Select-Object -Last 1).FullName
$summary    # confirm it's the REAL run's summary
```

Then run the rollback. **By default the rollback is a preview** — it shows `[DRYRUN]`-prefixed delete
actions and changes nothing. Inspect the output before committing to a real teardown.

```powershell
$ts = Get-Date -Format 'yyyyMMdd-HHmmss'
Start-Transcript -Path ".\logs\rollback-$ts.log" | Out-Null
.\Rollback-WwExecutionEngine.ps1 -SummaryPath $summary
Stop-Transcript | Out-Null
```

The rollback **only deletes resources this deployment created** (`created = true` in the summary)
**that still exist**. Pre-existing resources (`created = false`) and never-actually-created resources
(intent recorded but the create failed) are skipped. It **never deletes the resource group by default**.

> A summary with `status = failed` or `in-progress` is recognised as a **PARTIAL run**: the rollback
> prints a partial-run note and cleans up exactly what that interrupted run managed to create. This is
> the crash-safe behaviour — even a Phase 4 failure leaves a summary the rollback can act on.

To actually perform the teardown, re-run with the rollback script's execute switch (see its own
`-?`/help for the exact confirm flag) once you're satisfied with the preview.

---

## 3. Parameter reference

All parameters of [`Deploy-WwExecutionEngine.ps1`](../Scripts/Deploy-WwExecutionEngine.ps1). The model
is **"params first, prompt if missing"** — omitted values are prompted interactively unless
`-NonInteractive` is set, in which case a missing **required** value throws early.

> **Breaking change:** `-ResourceGroup`, `-Location`, `-StorageAccount`, `-AppName`, `-PublishPath`
> (and `-KeyVaultSecretName` when encrypting) have **no defaults**. Non-interactive callers must pass
> them explicitly.

### Targeting

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-SubscriptionId` | string | from `az account show` | Azure subscription GUID. |
| `-TenantId` | string | from `az account show` | Microsoft Entra tenant GUID. |
| `-ResourceGroup` | string | **required** | Resource group to create / use. |
| `-Location` | string | **required** | Azure region, e.g. `southafricanorth`. |
| `-StorageAccount` | string | **required** | Storage account name (3–24 lowercase chars). |
| `-AppName` | string | **required** | Function App name (globally unique). |

### Publish source

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-PublishPath` | string | **required** | The already-published package — a **folder** or a **`.zip`** of the publish output. The publish output is never modified: it is copied/extracted into a fresh temp staging dir (`wwexecutionengine-stage-<AppName>-<stamp>`) that is zipped, uploaded, and removed on a successful real run (kept, with a `-dryrun` suffix, under `-DryRun`). |
| `-PublishMethod` | `Auto` \| `Zip` \| `Func` | `Auto` | `Auto`/`Zip` both use `az` zip-deploy (config-zip) — correct for the pre-built artifact. `Func` uses `func azure functionapp publish --dotnet-isolated --no-build` and is **advanced/opt-in only** (it expects a project source dir and fails on a pre-built package). |

### Application Insights

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-AppInsightsName` | string | `<AppName>-ai` | Application Insights resource name. |
| `-EnableAppInsights` | bool (nullable) | prompt | Provision App Insights and enable worker telemetry (`ENABLEAPPLICATIONINSIGHTS=true` + `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`). |

> **Note:** the engine reads a deliberately **non-standard** `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`,
> so the Azure portal's *Function App → Monitoring → Application Insights* blade will always show a
> **"Turn On Application Insights"** button — **by design**. Telemetry still flows; do **not** click
> "Turn On" (it would start the host's own parallel pipeline). Verify in the App Insights resource's
> own *Logs*/*Live Metrics* blades instead.

### Auth provisioning

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-SkipAuthProvisioning` | switch | off | Skip Entra ID + Easy Auth provisioning entirely. |
| `-AuthConfigPath` | string | prompt | JSON describing `GroupPermissions` + `UserAssignments` for `Configure-WwExecutionAuth.ps1`. Each `GroupPermissions` key becomes an app role assignable to users **and** to app-only client apps (daemon / Managed Identity); the example ships a dedicated `Warewolf_ClientApps` group for those callers — assign it to the client SP via `Configure-WwExecutionAuth-Clients.ps1 -AppRolesToAssign` (the daemon / Managed Identity path defaults to `Warewolf_ClientApps` when the switch is omitted, and can enable a client Function App's system-assigned MI directly via `-DaemonFunctionAppName`/`-DaemonFunctionAppResourceGroup`), and grant the matching `WindowsGroup` row `Execute` in `secure.config`. See `Deploy-WwExecutionEngine.authconfig.example.json`. |

### Package inputs

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-SecureConfigPath` | string | prompt | secure.config to deploy. An AES-encrypted file is validated (must be engine-decryptable) and staged as-is; a **plaintext** JSON file is validated then **auto-AES-encrypted** before staging. |
| `-WorkflowsSourcePath` | string | prompt | Folder of workflow `.bite` resource files; staged into `<PublishDir>/Resources`. |
| `-LicenseConfigPath` | string | prompt (optional) | Path to a `Warewolf License.secureconfig`. Copied to the publish root. **Optional** — if omitted, the license check (default ON) may fail at startup. |

### Encryption / Key Vault

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-EncryptResources` | bool (nullable) | **false** | When set, this run encrypts **all** deployable sources (workflow `.bite` ConnectionStrings, the Elasticsearch source, etc.) from plain/DPAPI to WFAES via the Key Vault key. **Encrypt once** — leave off on subsequent deploys (sources stay encrypted and are staged as-is). |
| `-VerifyDecryption` | switch | off | After encryption, verify **in memory** that the engine's Key Vault key decrypts every value (no plaintext written to disk). Only meaningful with `-EncryptResources`. |
| `-KeyVaultName` | string | conditional | Key Vault the engine decrypts WFAES sources with at runtime. **Required** when `-EncryptResources` is set, and whenever the deployed sources are already WFAES-encrypted (so app settings + managed-identity role are wired). |
| `-KeyVaultSecretName` | string | **required when `-KeyVaultName` used** | Secret holding the AES key material. No default — name it explicitly. |
| `-GenerateNewKey` | switch | off (implied for a new vault) | Generate a fresh AES key (`Encrypt-Config -GenerateKeys`). |

### Elasticsearch logging

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-EnableElasticsearch` | bool (nullable) | prompt | Enable Elasticsearch logging. Requires `-ElasticsearchSourcePath` **and** Key Vault (the source's ConnectionString is WFAES-encrypted before deploy). |
| `-ElasticsearchSourcePath` | string | conditional | Path to the Elasticsearch source. The file **must be named exactly `ElasticsearchLoggingSource.bite`** (the engine reads that exact path). |

### Suspend/resume persistence (Hangfire)

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-EnablePersistence` | bool (nullable) | prompt | Stage the persistence settings pair so the engine's resume route can run. When on, both source files are prompted if not passed. The DbSource `ConnectionString` is WFAES-encrypted in the **same pass** as the Elasticsearch source (requires `-EncryptResources` + Key Vault to encrypt; otherwise staged as-is). |
| `-PersistenceSettingsPath` | string | prompt | Path to `persistencesettings.json` (Enable/scheduler/flags). Must be named exactly that. Staged as-is. |
| `-PersistenceDbSourcePath` | string | prompt | Path to `persistencesettingsdbsource.bite` (Hangfire SQL `DbSource`). Must be named exactly that. |

### ExecutionEngineJobProcessor (optional companion)

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-DeployJobProcessor` | switch | off | After the engine deploy, also run `Deploy-WwJobProcessor.ps1` for the poller/reaper Function App, passing the shared context (subscription/tenant/RG/location/Key Vault/persistence pair + the engine URL as `-EngineResumeBaseUrl`). The child prompts for anything not supplied. |
| `-JobProcessorAppName` | string | prompt (child) | Function App name for the processor. |
| `-JobProcessorPublishPath` | string | prompt / required for companion | Folder/.zip of the processor's `dotnet publish` output. **Must be a different directory than the engine's `-PublishPath`** (different app/csproj); the engine resolves it at plan time and fails loudly on a collision. Required (no prompt) under `-NonInteractive`. |
| `-JobProcessorStorageAccount` | string | prompt (child) | Storage account for the processor. |
| `-EngineResumeScope` | string | prompt (child) | MI token scope the processor uses to call the resume route, e.g. `api://<engine-app-id>/.default`. |

> Role assignment for the processor's MI (`Warewolf_JobProcessor`) + the matching global
> `Execute` row in `secure.config` is a **separate** step — see
> [Deploy-EndToEnd-Runbook.md](Deploy-EndToEnd-Runbook.md) §7. `Deploy-WwJobProcessor.ps1`
> can also be run **standalone** (see its `-?` help and the `Scripts/README.md` section).

### RabbitMQ QueueProcessor (optional companion)

Fans out **one Azure Container App per queue-trigger**, autoscaled 0 → N by the KEDA `rabbitmq`
scaler, replacing `N × QueueWorker.exe` on the Azure path. Runs as Phase 7 of the engine deploy,
*after* the engine is live (each app needs the engine URL). The on-prem Server + `QueueWorker.exe`
path is untouched.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-DeployRabbitMqTriggers` | switch | off | After the engine deploy, run `Deploy-WwQueueProcessor.ps1` **once per pointed trigger file**, passing the shared context (subscription/tenant/RG/location/Key Vault + the engine URL). |
| `-QueueTriggerPath` | string | — | Folder of trigger `.bite` files → one Container App **per matching file**. |
| `-QueueTriggerFilter` | string | `*.bite` | Glob applied to `-QueueTriggerPath`. **Zero matches is a hard error**, never a silent no-op. |
| `-QueueTriggerFilePath` | string | — | A **single** trigger file → exactly one Container App. |
| `-QueueTriggerManifestPath` | string | — | JSON manifest → one app per entry, with per-trigger overrides. |
| `-QueueSourcePath` | string | prompt | Folder holding the `RabbitMQSource` `.bite` files referenced by each trigger — **both `QueueSourceId` and `QueueSinkId`**. Every referenced source is copied into the image at `Settings/sources/{sourceId}.bite`; a sink source that is not staged yields a replica that starts but cannot dead-letter. |
| `-AcaEnvironment` | string | prompt | Container Apps environment that hosts the workers. |
| `-AcrName` | string | prompt | Container registry; the image is built with `az acr build`. |
| `-QueueProcessorPublishPath` | string | prompt / required for companion | `dotnet publish` output of `Warewolf.Execution.QueueProcessor`. **Must differ from the engine's `-PublishPath`.** Required (no prompt) under `-NonInteractive`. |
| `-QueueProcessorImage` | string | derived | Override the image tag instead of building. |
| `-QueueEngineResourceAppId` | string | engine's app id | Token audience — `api://{id}/.default`. |
| `-RabbitMqSecretUri` | string | prompt | Key Vault secret URI for the broker URI, surfaced as an ACA secret. **Consumed by the KEDA scale rule only** — KEDA cannot use a managed identity against RabbitMQ. |
| `-QueueScalingMode` | `Elastic`\|`Fixed`\|`Warm` | `Elastic` | `Elastic` = `minReplicas 0`. `Fixed`/`Warm` are exception paths and are flagged in the plan output. |
| `-ContinueOnQueueTriggerError` | switch | off | Keep deploying the remaining triggers when one fails. Off = stop at the first failure. |

The three targeting parameters are **mutually exclusive**. Everything scale-shaped is
**derived from the trigger file** so it stays the single source of truth:
`maxReplicas = Concurrency` and KEDA `value = Prefetch × MaxConcurrency`. The worker reads
`Prefetch` straight from the staged trigger for its per-consumer QoS — there is no prefetch env var,
so the trigger stays the single source of truth.
`Concurrency = 0` deploys `min = max = 0` (disabled), mirroring on-prem. Peak core usage
(`Σ maxReplicas × cpu`) is printed so it can be checked against the environment quota.

**Config is baked into the image**, not volume-mounted or fetched at startup — with
`minReplicas = 0` any mount or download is paid on every 0→1 scale and adds a dependency that can
stop a replica starting. The image tag therefore pins the config version, and a trigger edit means
a new revision. Staged layout: `Settings/triggers/{TriggerId}.bite` +
`Settings/sources/{sourceId}.bite`.

**Optimum shape:** `minReplicas 0`, `maxReplicas = Concurrency`, and trigger
`Prefetch = MaxConcurrency` (both 1 unless a workflow is measured safe to run concurrently).
Dispatch is serial per channel — measured, deliveries ~2.2 s apart with no overlap — so a larger
prefetch adds no throughput; it raises the KEDA target, **delaying** scale-out, and leaves more
buffered messages to nack on drain. A prefetch above the cap is a plan-time advisory, not an error.

**Tenant id:** the engine deploy always forwards its resolved `-TenantId` to the companion as
`-EngineTenantId`. Blank is legal only for a *system-assigned* managed identity; for any other
credential a blank tenant fails with *"Invalid tenant id provided"* at the first message, which
looks like a missing app role rather than a config gap.

> Role assignment for **each** app's MI (`Warewolf_QueueProcessor`) plus a **per-workflow**
> (`IsServer=false`) `View`+`Execute` row in `secure.config` for every trigger's `WorkflowName`
> is a **separate** step — see [Deploy-EndToEnd-Runbook.md](Deploy-EndToEnd-Runbook.md) §8.
> Note this differs from the JobProcessor, which uses one **global** row.
> `Deploy-WwQueueProcessor.ps1` can also be run **standalone**.

### Logging / feature env vars

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-EnableConsoleLogging` | bool (nullable) | prompt | Set `ENABLECONSOLELOGGING=true`. |
| `-ExecutionLogLevel` | `TRACE`\|`DEBUG`\|`INFO`\|`WARN`\|`ERROR`\|`FATAL`\|`OFF` | `INFO` | `EXECUTIONLOGLEVEL` — drives the engine's console + App Insights logging in code (the isolated worker does **not** read host.json). |
| `-LicenseCheckEnabled` | bool (nullable) | engine default `true` | `WAREWOLF_LICENSE_CHECK_ENABLED`. |
| `-StructuredLogs` | bool (nullable) | `true` | `STRUCTURED_LOGS` — JSON console output. |
| `-AlignHostJsonLogLevel` | switch | off | **Opt-in.** Also rewrite the published `host.json` logLevel to the mapped level. Tunes only the Functions **host** process verbosity — not needed for the engine's own logging. |

### Logging output / control

| Parameter | Type | Default | Description |
|---|---|---|---|
| `-LogDir` | string | `<PublishDir>\..\deploy-logs` | Directory for the transcript + summary. |
| `-NonInteractive` | switch | off | Never prompt; a missing required value throws early and clearly. |
| `-DryRun` | switch | off | Print every mutating action without executing it. Read-only probes still run; a `.dryrun.` transcript + summary are written. |
| `-LoadFunctionsOnly` | switch | off | **Test hook.** Dot-source the script to define its helpers and return *before* any cloud/filesystem action (used by the Pester suite). Not for normal runs. |

> **Fixed / non-configurable env vars** (no parameters): `ASPNETCORE_ENVIRONMENT` is always
> `Production`; `BYPASS_SECURE_CONFIG`, `WAREWOLF_SUPER_ADMIN_ENABLED` and `SkipFailureToRetrieveSecret`
> are always `false`. Development-only `DEBUG_*` settings were removed; `WEBSITE_INSTANCE_ID` /
> `AZURE_FUNCTIONS_ENVIRONMENT` are platform-managed.

---

## 4. Worked parameter examples

> All examples below reuse the `$variables` defined once in [Step 0](#step-0--log-in-and-select-the-subscription) — run that block in your session first.

### Example A — Dry run (no Azure changes; inspect the plan)

Same arguments as the real deploy, plus `-DryRun`. Prints the full masked plan and writes a
`.dryrun.summary.json` you can review before committing.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath `
  -DryRun
```

### Example B — Minimal deploy (no encryption, no Elasticsearch, no App Insights)

The smallest real deploy: just infra + package, auth from a config, plaintext secure.config
auto-encrypted on staging.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath `
  -AuthConfigPath $AuthConfigPath `
  -SecureConfigPath $SecureConfigPath `
  -WorkflowsSourcePath $WorkflowsSourcePath `
  -EnableAppInsights:$false -EnableElasticsearch:$false `
  -PublishMethod Zip
```

### Example C — First-time encrypted deploy (generate key, verify in memory)

Encrypt **once** with a brand-new AES key, verifying decryption in memory. App Insights on.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath `
  -AuthConfigPath $AuthConfigPath `
  -SecureConfigPath $SecureConfigPath `
  -WorkflowsSourcePath $WorkflowsSourcePath `
  -LicenseConfigPath $LicenseConfigPath `
  -EncryptResources:$true -VerifyDecryption -GenerateNewKey `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -EnableAppInsights:$true `
  -PublishMethod Zip
```

### Example D — Re-deploy already-encrypted sources (do NOT re-encrypt)

Sources were encrypted on a previous run. Leave `-EncryptResources` **off**, but still pass the Key
Vault so the runtime app settings + managed-identity role are wired for decryption.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location `
  -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath `
  -AuthConfigPath $AuthConfigPath `
  -SecureConfigPath $SecureConfigPath `
  -WorkflowsSourcePath $WorkflowsSourcePath `
  -EncryptResources:$false `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -EnableAppInsights:$true `
  -PublishMethod Zip
```

### Example E — Full deploy via splatting (avoids the backtick gotcha)

Identical to the Step 1 deploy but as a hashtable — no line-continuation backticks to get wrong, and
easy to diff/version.

```powershell
$deploy = @{
  ResourceGroup           = $ResourceGroup
  Location                = $Location
  StorageAccount          = $StorageAccount
  AppName                 = $AppName
  PublishPath             = $PublishPath
  AuthConfigPath          = $AuthConfigPath
  SecureConfigPath        = $SecureConfigPath
  WorkflowsSourcePath     = $WorkflowsSourcePath
  LicenseConfigPath       = $LicenseConfigPath
  EncryptResources        = $true
  VerifyDecryption        = $true
  KeyVaultName            = $KeyVaultName
  KeyVaultSecretName      = $KeyVaultSecretName
  EnableAppInsights       = $true
  EnableElasticsearch     = $true
  ElasticsearchSourcePath = $ElasticsearchSourcePath
  LogDir                  = $LogDir
  PublishMethod           = 'Zip'
}

$ts = Get-Date -Format 'yyyyMMdd-HHmmss'
Start-Transcript -Path ".\logs\deploy-$ts.log" | Out-Null
.\Deploy-WwExecutionEngine.ps1 @deploy
Stop-Transcript | Out-Null
```

### Example F — Non-interactive (CI / unattended)

Force an early, explicit throw on any missing required value instead of prompting.

```powershell
.\Deploy-WwExecutionEngine.ps1 @deploy -NonInteractive
```

---

## 5. Resources: created by the script vs. assumed pre-existing

The orchestrator is a **thin deployer**: it provisions cloud infrastructure and wires it up, but it
does **not** build the package and does **not** create your input files or your identities. Every
infrastructure resource is existence-checked first and recorded in the run summary's `created` map
(`true` = this run created it; `false` = found pre-existing) so the rollback only ever removes what
this run actually created.

### Created by the script (when absent)

| Resource | Condition | Notes |
|---|---|---|
| Resource group | Always | Created in `-Location` if it doesn't exist. |
| Storage account | Always | `Standard_LRS`, `StorageV2`, TLS 1.2. |
| Function App | Always | Consumption (Y1), `dotnet-isolated` 8, Functions v4, HTTPS-only. |
| Application Insights | When `-EnableAppInsights` | Default name `<AppName>-ai`; worker telemetry wired via `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`. |
| System-assigned managed identity | When a Key Vault is in play | Enabled on the Function App so the engine can read the key at runtime. |
| **Key Vault** | **Only with `-EncryptResources`** | RBAC-authorized vault. If a vault is needed but missing and `-EncryptResources` is **off**, the script **throws** (it will not create one). |
| **AES key (stored as a Key Vault *secret*)** | **Only with `-EncryptResources`, real run** | The "key" is AES-256 material written as a JSON **secret** (`-KeyVaultSecretName`). The script does **not** create a Key Vault *cryptographic key* object — no `az keyvault key create` is ever called. |
| Key Vault role assignments | When a Key Vault is in play | `Key Vault Secrets User` → Function App identity (always); `Key Vault Secrets Officer` → current user (only when encrypting). |
| Entra app registration, service principal, OAuth scope, app roles, client secret, Easy Auth config | Unless `-SkipAuthProvisioning` | Provisioned by `Configure-WwExecutionAuth.ps1`. Idempotent — re-runs upgrade in place. |
| App settings / environment variables | Always | The engine's env-var map (see [§3](#3-parameter-reference)). |
| `Resources\workflow-index.json` | When workflows are staged | Generated over the staged `Resources` and bundled into the deploy zip. |

### Assumed to already exist (the script validates and **throws** if missing — it does not create these)

| Item | Supplied via | Behaviour if missing |
|---|---|---|
| Azure subscription + active `az login` | `az login` (Step 0) | Pre-flight throws: "Not logged in to Azure CLI." |
| The signed-in user's **roles/privileges** | Admin grants (see [Required roles & privileges](#required-roles--privileges)) | Not self-granted (except the Key Vault data-plane role). Azure returns authorization errors mid-run. |
| **Published package** | `-PublishPath` (folder or `.zip`) | Throws — you must `dotnet publish` yourself first; the script never builds. |
| `secure.config` | `-SecureConfigPath` | Validated (plaintext auto-encrypted; encrypted validated decryptable); throws if unreadable/invalid. |
| Workflow `.bite` resources | `-WorkflowsSourcePath` | Throws if the path doesn't exist. |
| `Warewolf License.secureconfig` | `-LicenseConfigPath` | **Optional.** If omitted, the license check (default ON) may fail at startup. |
| `ElasticsearchLoggingSource.bite` | `-ElasticsearchSourcePath` | Required when `-EnableElasticsearch`; throws if missing or not named exactly `ElasticsearchLoggingSource.bite`. |
| Auth config JSON | `-AuthConfigPath` | Throws if specified but not found. If omitted, auth is provisioned with empty group/user maps. |
| Entra **users** referenced in `UserAssignments` | Auth config JSON | Each is resolved by UPN (`az ad user show`). A missing/typo'd UPN is **skipped with a warning** and Stage 6 continues — check the log so an intended user isn't silently skipped. |
| **Existing Key Vault** | `-KeyVaultName` **without** `-EncryptResources` | Re-deploy of already-encrypted sources requires the vault to already exist — the script **throws** rather than creating it. |
| Tooling: PowerShell 7+, Azure CLI (logged in), `func` (only for `-PublishMethod Func`) | Local install | Pre-flight throws if `az` is absent or not logged in; `func` is checked at publish time and throws then when `-PublishMethod Func` is selected. |

> **Dry run note.** `-DryRun` creates nothing. A Key Vault / secret are only treated as "reachable"
> if they already exist; otherwise encryption is deferred to a real run and the source is staged in
> plaintext for preview.

---

## 6. Troubleshooting

Common failures and how to resolve them. Most issues trace back to a missing prerequisite
([§1](#1-prerequisites)) or a role/privilege gap ([Required roles & privileges](#required-roles--privileges)).

| Symptom | Likely cause | Resolution |
|---|---|---|
| Script aborts citing `#Requires -Version 7.0` | Launched under Windows PowerShell 5.1 | Open **PowerShell 7+** and confirm with `$PSVersionTable.PSVersion`. 5.1 cannot run the script (see [§1](#1-prerequisites)). |
| `az` calls fail with *"Please run 'az login'"* | Not logged in to Azure CLI | Run `az login`, then `az account set --subscription <id>` (Step 0). |
| Deploy lands in the wrong subscription | Active subscription not set | `az account set --subscription <id>`, or pass `-SubscriptionId` explicitly. |
| `az role assignment create` fails with an authorization error mid-run | Signed-in user lacks **User Access Administrator** / **Owner** over the Key Vault scope | Have an admin grant it (see [Required roles & privileges](#required-roles--privileges) step A). This is the most commonly-missed grant. |
| Entra provisioning fails (app registration / role / secret) | User lacks **Application Administrator** (or Graph `Application.ReadWrite.All`) | Grant the Entra role (step C), or run with `-SkipAuthProvisioning` if auth is provisioned separately. |
| A user in `UserAssignments` is silently not assigned | UPN typo / user not in directory | Stage 6 **skips with a warning** and continues — check the transcript so an intended user isn't missed. |
| Phase 4 zip-deploy fails or times out | Large package, transient network, or Function App still starting | Inspect the transcript in `-LogDir`; re-run — the deploy is idempotent and existence-checks every resource. |
| `-PublishMethod Func` fails immediately | `func` not installed, or pointed at a pre-built package | Use the default `Auto`/`Zip` (az zip-deploy) for the pre-built artifact; `Func` is advanced/opt-in and expects a project source dir. |
| Engine returns HTTP 503 after deploy | Missing / invalid `secure.config` | Verify `-SecureConfigPath` resolves to a valid file (plaintext is auto-encrypted; an encrypted file must be engine-decryptable). |
| Workflows blocked: *"valid Warewolf license required"* | No license staged (the check is ON by default) | Supply a valid `-LicenseConfigPath`. **There is no auto-generated test license** — if omitted, the license check may fail at startup. |
| WFAES decryption fails at runtime | Key Vault app settings or managed-identity role not wired | Re-deploy passing `-KeyVaultName` / `-KeyVaultSecretName` (leave `-EncryptResources` **off** for already-encrypted sources — see [Example D](#example-d--re-deploy-already-encrypted-sources-do-not-re-encrypt)). |
| DPAPI re-encryption fails | DPAPI sources can only be re-encrypted on the machine that created them | Run `-EncryptResources` on the original machine, or re-export the sources as plaintext first. |
| App Insights blade shows *"Turn On Application Insights"* | **By design** — the engine reads the non-standard `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` | Do **not** click "Turn On". Verify telemetry in the App Insights resource's own *Logs* / *Live Metrics* blades ([§3](#application-insights)). |
| Rollback deletes the wrong resources | A dry-run summary was selected | Confirm the summary is the **real** run's file (exclude `*dryrun*`); the rollback only removes resources with `created = true` and previews by default (Step 2). |

---

## Related docs

- [Deploy-EndToEnd-Runbook.md](Deploy-EndToEnd-Runbook.md) — single copy-paste PS7 + `az` sequence covering engine deploy **and** daemon client-app registration (create client Function App → enable MI → assign `Warewolf_ClientApps`).
- [Deployment-Steps.txt](Deployment-Steps.txt) — terse end-to-end runbook (incl. crash-safe summary + rollback notes).
- [Scripts/README.md](../Scripts/README.md) — script-suite overview.
- [README-Encryption.md](README-Encryption.md) · [README-Authentication.md](README-Authentication.md) · [Warewolf-Lightweight-Logging-Guide.md](Warewolf-Lightweight-Logging-Guide.md) — all logging (console, App Insights, Elasticsearch, audit).
