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

---

## 1. Prerequisites

| Tool | Minimum version | Why it's needed | Download / install |
|---|---|---|---|
| **PowerShell** | **7.0+** (latest recommended) | The script declares `#Requires -Version 7.0`; uses `Set-StrictMode -Version Latest`, ternary/null-coalescing operators, `&&`/`\|\|`. Windows PowerShell 5.1 will **not** run it. | [github.com/PowerShell/PowerShell/releases](https://github.com/PowerShell/PowerShell/releases/download/v7.6.3/PowerShell-7.6.3-win-x64.msi) · or `winget install Microsoft.PowerShell` |
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
$Sub     = 'dd0bc517-5cc7-4b56-bd6a-68e6140db7b3'   # your subscription id
$Rg      = 'DEV2'
$Vault   = 'WWExecutionEngine'

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
az account set --subscription dd0bc517-5cc7-4b56-bd6a-68e6140db7b3
cd D:\ExecutionEngine\Scripts
```

> The script can resolve `SubscriptionId` / `TenantId` from `az account show` when you omit them, so
> setting the active subscription here is what targets the deployment.

### Step 1 — Deploy FOR REAL (creates billable resources)

```powershell
$ts = Get-Date -Format 'yyyyMMdd-HHmmss'
Start-Transcript -Path ".\logs\deploy-$ts.log" | Out-Null

.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -StorageAccount stwwenginetestv1 -AppName wwenginetestv1 `
  -PublishPath 'D:\ExecutionEngine\AzureFunctionsPackage-3.0.1.200' `
  -AuthConfigPath 'D:\ExecutionEngine\Deploy-WwExecutionEngine.authconfig.json' `
  -SecureConfigPath 'C:\ProgramData\Warewolf\Server Settings\secure.config' `
  -WorkflowsSourcePath 'C:\ProgramData\Warewolf\Resources' `
  -LicenseConfigPath 'D:\ExecutionEngine\Warewolf License.secureconfig' `
  -EncryptResources:$true -VerifyDecryption `
  -KeyVaultName WWExecutionEngine -KeyVaultSecretName WWExecutionEngineTestSecret `
  -EnableAppInsights:$true `
  -EnableElasticsearch:$true -ElasticsearchSourcePath 'D:\ExecutionEngine\ElasticsearchLoggingSource.bite' `
  -LogDir 'D:\ExecutionEngine\Scripts\logs' `
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
| 2 | Provision Entra ID + Easy Auth (from `-AuthConfigPath`) |
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
$summary = (Get-ChildItem 'D:\ExecutionEngine\Scripts\logs\deploy-WwExecutionEngine-*.summary.json' |
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
| `-PublishPath` | string | **required** | The already-published package — a **folder** or a **`.zip`** of the publish output (a zip is extracted to a sibling folder, which becomes the package dir). |
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
| `-AuthConfigPath` | string | prompt | JSON describing `GroupPermissions` + `UserAssignments` for `Configure-WwExecutionAuth.ps1`. |

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

### Example A — Dry run (no Azure changes; inspect the plan)

Same arguments as the real deploy, plus `-DryRun`. Prints the full masked plan and writes a
`.dryrun.summary.json` you can review before committing.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -StorageAccount stwwenginetestv1 -AppName wwenginetestv1 `
  -PublishPath 'D:\ExecutionEngine\AzureFunctionsPackage-3.0.1.200' `
  -DryRun
```

### Example B — Minimal deploy (no encryption, no Elasticsearch, no App Insights)

The smallest real deploy: just infra + package, auth from a config, plaintext secure.config
auto-encrypted on staging.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -StorageAccount stwwenginetestv1 -AppName wwenginetestv1 `
  -PublishPath 'D:\ExecutionEngine\AzureFunctionsPackage-3.0.1.200' `
  -AuthConfigPath 'D:\ExecutionEngine\Deploy-WwExecutionEngine.authconfig.json' `
  -SecureConfigPath 'C:\ProgramData\Warewolf\Server Settings\secure.config' `
  -WorkflowsSourcePath 'C:\ProgramData\Warewolf\Resources' `
  -EnableAppInsights:$false -EnableElasticsearch:$false `
  -PublishMethod Zip
```

### Example C — First-time encrypted deploy (generate key, verify in memory)

Encrypt **once** with a brand-new AES key, verifying decryption in memory. App Insights on.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -StorageAccount stwwenginetestv1 -AppName wwenginetestv1 `
  -PublishPath 'D:\ExecutionEngine\AzureFunctionsPackage-3.0.1.200' `
  -AuthConfigPath 'D:\ExecutionEngine\Deploy-WwExecutionEngine.authconfig.json' `
  -SecureConfigPath 'C:\ProgramData\Warewolf\Server Settings\secure.config' `
  -WorkflowsSourcePath 'C:\ProgramData\Warewolf\Resources' `
  -LicenseConfigPath 'D:\ExecutionEngine\Warewolf License.secureconfig' `
  -EncryptResources:$true -VerifyDecryption -GenerateNewKey `
  -KeyVaultName WWExecutionEngine -KeyVaultSecretName WWExecutionEngineTestSecret `
  -EnableAppInsights:$true `
  -PublishMethod Zip
```

### Example D — Re-deploy already-encrypted sources (do NOT re-encrypt)

Sources were encrypted on a previous run. Leave `-EncryptResources` **off**, but still pass the Key
Vault so the runtime app settings + managed-identity role are wired for decryption.

```powershell
.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup DEV2 -Location southafricanorth `
  -StorageAccount stwwenginetestv1 -AppName wwenginetestv1 `
  -PublishPath 'D:\ExecutionEngine\AzureFunctionsPackage-3.0.1.200' `
  -AuthConfigPath 'D:\ExecutionEngine\Deploy-WwExecutionEngine.authconfig.json' `
  -SecureConfigPath 'C:\ProgramData\Warewolf\Server Settings\secure.config' `
  -WorkflowsSourcePath 'C:\ProgramData\Warewolf\Resources' `
  -EncryptResources:$false `
  -KeyVaultName WWExecutionEngine -KeyVaultSecretName WWExecutionEngineTestSecret `
  -EnableAppInsights:$true `
  -PublishMethod Zip
```

### Example E — Full deploy via splatting (avoids the backtick gotcha)

Identical to the Step 1 deploy but as a hashtable — no line-continuation backticks to get wrong, and
easy to diff/version.

```powershell
$deploy = @{
  ResourceGroup           = 'DEV2'
  Location                = 'southafricanorth'
  StorageAccount          = 'stwwenginetestv1'
  AppName                 = 'wwenginetestv1'
  PublishPath             = 'D:\ExecutionEngine\AzureFunctionsPackage-3.0.1.200'
  AuthConfigPath          = 'D:\ExecutionEngine\Deploy-WwExecutionEngine.authconfig.json'
  SecureConfigPath        = 'C:\ProgramData\Warewolf\Server Settings\secure.config'
  WorkflowsSourcePath     = 'C:\ProgramData\Warewolf\Resources'
  LicenseConfigPath       = 'D:\ExecutionEngine\Warewolf License.secureconfig'
  EncryptResources        = $true
  VerifyDecryption        = $true
  KeyVaultName            = 'WWExecutionEngine'
  KeyVaultSecretName      = 'WWExecutionEngineTestSecret'
  EnableAppInsights       = $true
  EnableElasticsearch     = $true
  ElasticsearchSourcePath = 'D:\ExecutionEngine\ElasticsearchLoggingSource.bite'
  LogDir                  = 'D:\ExecutionEngine\Scripts\logs'
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
| Entra **users** referenced in `UserAssignments` | Auth config JSON | Each is resolved by UPN (`az ad user show`); a missing user fails that row (Stage 6 never aborts the whole run). |
| **Existing Key Vault** | `-KeyVaultName` **without** `-EncryptResources` | Re-deploy of already-encrypted sources requires the vault to already exist — the script **throws** rather than creating it. |
| Tooling: PowerShell 7+, Azure CLI (logged in), `func` (only for `-PublishMethod Func`) | Local install | Pre-flight throws if `az` (or `func`, when selected) is absent. |

> **Dry run note.** `-DryRun` creates nothing. A Key Vault / secret are only treated as "reachable"
> if they already exist; otherwise encryption is deferred to a real run and the source is staged in
> plaintext for preview.

---

## Related docs

- [Deployment-Steps.txt](Deployment-Steps.txt) — terse end-to-end runbook (incl. crash-safe summary + rollback notes).
- [Scripts/README.md](../Scripts/README.md) — script-suite overview.
- [README-Encryption.md](README-Encryption.md) · [README-Authentication.md](README-Authentication.md) · [README-ApplicationInsights.md](README-ApplicationInsights.md).
