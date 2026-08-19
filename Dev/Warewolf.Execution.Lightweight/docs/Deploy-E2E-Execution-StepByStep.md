# E2E Execution Plan — Execution Engine + QueueProcessor (isolated parallel run `wwengine2` / `wwqp2-`)

Manually executable, step-by-step derivation of
[Deploy-E2E-Verification-Runbook.md](Deploy-E2E-Verification-Runbook.md), targeted at the **live
Azure state observed on 2026-08-06** and parameterised for an **isolated parallel run** that leaves
the existing 2026-08-05 deployment completely untouched.

| | |
|---|---|
| **Automated equivalent** | [E2E-Harness-README.md](E2E-Harness-README.md) — `New-WwE2EStaging.ps1` + `Invoke-WwE2EVerification.ps1` do all of this and score it. **Prefer the harness**; use this document to understand or debug what it does |
| Companion summary | [Deploy-E2E-Execution-Summary.md](Deploy-E2E-Execution-Summary.md) — fill in as you go |
| Companion rollback | [Deploy-E2E-Rollback-Commands.md](Deploy-E2E-Rollback-Commands.md) — Phase F lives there, in full |
| Parameter reference | [Deploy-RunGuide.md](Deploy-RunGuide.md) |
| Design rationale | [QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md](QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md) |

> **Every step is numbered and has an explicit expected result.** Steps marked 🔴 **change live Azure
> or Entra state** — do not run one without deliberate approval. Steps marked 🟢 are read-only and
> safe to run unattended. Steps marked 🟡 change local files only.

---

## 0. Observed starting state (verified 2026-08-06, read-only)

This plan is written against **facts**, not assumptions. Everything below was queried live.

### 0.1 The 2026-08-05 run is still deployed — and must stay untouched

| Resource | State on 2026-08-06 |
|---|---|
| `wwengine1` (Function App) | **Stopped** — `/Public` and `/Secure` both return `403 Site Disabled` |
| `stwwengine1`, `wwengine1-ai` | present, tagged `wwx-test-run=wwx-20260805-040246` |
| Entra app `wwengine1-auth` | present — appId `ab0810f7-8caa-46cf-b3d9-c5dd6a1429b4`, SP objectId `a5db272b-88bd-4697-a6f2-70ac158095f1` |
| `wwqp-ordersuccessqueue` | present, revision `…--0000009` `Healthy`, **0 replicas**, cpu 0.5, max 3 |
| `wwqp-orderfailurequeue` | present, `Healthy`, **0 replicas**, cpu 0.5, max 1 |
| ACR repo `warewolf/queueprocessor` | present, tag `20260805-220738` |

⚠️ Those two `wwqp-*` apps hold live `rabbitmq` scale rules on `order-success-queue` and
`order-failure-queue`, and they forward to the **stopped** `wwengine1`. If this run reused those
queue names, the old apps would wake, take a share of the messages, receive a non-2xx from a stopped
engine, and **dead-letter and ack** them — silently destroying acceptance criteria 9 and 12.
**This is why the run uses dedicated queue names (§0.5).**

### 0.2 Shared platform resources — reused, never created, never deleted

| Resource | Verified |
|---|---|
| RG `DEV2` | `Succeeded`, `southafricanorth` — **shared**: AKS, `WarewolfServer*`, Key Vault, ACA env |
| Key Vault `WWExecutionEngine` | present, RBAC; secret `WWExecutionEngineTestSecret` enabled (updated 2026-08-06) |
| Secret `rabbitmq-uri` | **already exists** (created 2026-08-04) → this run creates **nothing** in Phase A |
| ACR `tudev2containerregistry` | present; 15 repositories incl. `warewolf` and `warewolf/queueprocessor` |
| ACA env `dev2-cae` | `Succeeded`, `South Africa North`, `vnet: null`, profiles `[Consumption]`, workspace cid `405a4834-1554-4f2a-b5eb-31789fffaa9b` |
| Log Analytics `workspace-2028k` | bound to `dev2-cae` transitively — nothing to wire |
| Neighbours in `dev2-cae` | `sharepoint-wiremock` (0.5 × min 1, always on), `exchange-wiremock` (0.5 × min 0) |
| Dev broker | `4.tcp.eu.ngrok.io:20313` — **TCP reachable** on 2026-08-06 |
| Local Warewolf Server | `http://localhost:3142` responds `401` (running, Windows auth) |

### 0.3 Staging root `G:\Deployment` — complete

| Path | Verified |
|---|---|
| `apps\ExecutionEngine` | engine publish present (`Warewolf.Execution.Lightweight.dll`) |
| `apps\QueueProcessor` | worker publish present, **0 `.bite` files** ✅ |
| `triggers` | 2 files (see §0.4) |
| `sources` | `RabbitMQSourceAshley.bite`, ID `fa5f49d7-f6d7-422c-b08e-17b094d82f1d`, ConnectionString is **DPAPI** → `-EncryptStagedSettings` must run **on this machine** |
| `settings` | `secure.config` (= `secure.config.cloud.json`), `Deploy-WwExecutionEngine.authconfig.json`, `Warewolf License.secureconfig` |
| `C:\ProgramData\Warewolf\Resources` | `Hello World.bite`, `rabbit\{RabbitProcess,RabbitProcessFailure,RabbitPublish,RabbitConsume}.bite` — **all trigger workflows staged** ✅ |

`secure.config` grants (verified):

| Group | Resource | IsServer | View | Execute |
|---|---|---|---|---|
| `Warewolf Administrators` | — | true | ✅ | ✅ |
| `Public` | — | true | ✅ | ✅ |
| `Warewolf_QueueProcessor` | — | true | ✅ | ✅ |
| `Warewolf_ClientApps` | `Hello World` | false | ✅ | ✅ |
| `Public` | `Hello World` | false | ✅ | ✅ |

`rabbit\RabbitProcess`, `rabbit\RabbitProcessFailure` and `rabbit\RabbitPublish` carry **no**
Execute-bearing resource entry, so they resolve against the **global** map, where `Public`'s
server-level `Execute` is always OR'd in. **No per-workflow row is required, and none should be
added** — adding one narrows access (§2 of the source runbook). `authconfig.json` defines
`Warewolf_QueueProcessor`, so the optional app role is assignable.

### 0.4 Source triggers — scaling parameters need **no** change

| File | Name | Queue | Workflow | Concurrency → `maxReplicas` | Prefetch | Durable | DLQ |
|---|---|---|---|---|---|---|---|
| `03fb9052-…bite` | `OrderSuccessQueue` | `order-success-queue` | `rabbit\RabbitProcess` | **3** | **1** ✅ | true | `order-success-queue-errors` |
| `12345678-…bite` | `OrderFailureQueue` | `order-failure-queue` | `rabbit\RabbitProcessFailure` | **1** | **1** ✅ | true | `order-failure-queue-errors` |

Both already have `Prefetch = 1`, so **§0.1 of the source runbook requires no trigger edits** for
scaling. Both use `MapEntireMessage: true` with an input named `message` — **no `@` prefix**, so the
forwarder sends a JSON body, which the engine binds. No multipart refusal.

Expected derived scale (per §4.8): `min 0`, `max 3` / `max 1`, `value = MaxConcurrency = 1`,
`mode=QueueLength`, `protocol=amqp`, **no `activationValue`**.

### 0.5 Isolation contract for this run

**Created by this run (all names verified free on 2026-08-06):**

| Kind | Name |
|---|---|
| Function App | `wwengine2` |
| Storage | `stwwengine2` (`nameAvailable: true`) |
| App Insights | `wwengine2-ai` |
| Entra app registration | `wwengine2-auth` (0 existing matches) + its SP + Easy Auth client secret |
| Container App | `wwqp2-ordersuccessqueue` |
| Container App | `wwqp2-orderfailurequeue` |
| ACR repository | `warewolf/queueprocessor-e2e` — **separate repo**, so teardown can never delete the 2026-08-05 image |
| RabbitMQ queues | `order-success-queue-e2e`, `order-failure-queue-e2e` (+ `-errors` DLQs) — created **by the publisher**, non-durable, on first publish |
| Local trigger copies | `G:\Deployment\triggers-e2e\*.bite` |

**Reused, never created, never deleted:** `DEV2`, `WWExecutionEngine`, `WWExecutionEngineTestSecret`,
`rabbitmq-uri`, `dev2-cae`, `tudev2containerregistry`, `workspace-2028k`,
`G:\Deployment\sources`, `G:\Deployment\settings`, `C:\ProgramData\Warewolf\Resources`.

**Untouched by this run:** `wwengine1`, `stwwengine1`, `wwengine1-ai`, `wwengine1-auth`,
`wwqp-ordersuccessqueue`, `wwqp-orderfailurequeue`, `warewolf/queueprocessor`, `sharepoint-wiremock`,
`exchange-wiremock`, `tudev2-kubernetes`, every `WarewolfServer*`.

### 0.6 The non-durable-queue test (explicit requirement of this run)

The new queues do **not** exist on the broker. `PublishRabbitMQActivity` creates a queue only when a
passive declare fails, and it creates it with `IsDurable = False`
([PublishRabbitMQActivity.cs:210-224](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L210-L224)).
The triggers declare `Durable: true`. **This is not a conflict**, and the run proves it:

- The pump performs **no `QueueDeclare`** — only `BasicQos` → `BasicConsume`
  ([RabbitMqMessagePump.cs:113-142](../../Warewolf.Execution.QueueProcessor/Messaging/RabbitMqMessagePump.cs#L113-L142)).
  A consumer never asserts queue arguments, so a `Durable=true` trigger consumes a **non-durable**
  queue with no `PRECONDITION_FAILED`.
- The DLQ publisher declares **passive-first** and only creates the DLQ (with `DeadLetterDurable`)
  when it is genuinely absent
  ([RabbitMqDeadLetterPublisher.cs:118-146](../../Warewolf.Execution.QueueProcessor/Messaging/RabbitMqDeadLetterPublisher.cs#L118-L146)).

The one real consequence: **the queue must pre-exist before `BasicConsume`, and before the KEDA
`rabbitmq` scaler's passive declare can report a depth.** Step **E0** therefore publishes one
warm-up message per queue immediately after Phase C, which creates the queues. Until E0 runs,
replicas legitimately sit at 0 and the scaler may log a missing-queue error — **that is expected,
not a fault.**

### 0.7 Correction carried into this plan

The source runbook's **E2** command is wrong for this workflow. `rabbit\RabbitPublish`'s DataList
declares inputs **`queue`** and **`total`** (outputs `output`, `c`), and the message body is the
literal `hello [[c]]`. So publishing five messages is **one** call with `?queue=<q>&total=5` — not
five calls with `?QueueName=`. Recorded as a doc-sync item in the summary.

---

## 1. Step 0 — Session variables (🟢 run once per shell)

```powershell
# ── Identity / subscription ───────────────────────────────────────────────────
az login                                            # skip if already signed in
$Sub      = (az account show --query id       -o tsv)   # dd0bc517-5cc7-4b56-bd6a-68e6140db7b3
$TenantId = (az account show --query tenantId -o tsv)   # ca0cc53b-9af4-4067-bcdf-be9c648450d1
az account set --subscription $Sub

# ── Reused foundation (NEVER created, NEVER deleted) ──────────────────────────
$Rg        = 'DEV2'
$Loc       = 'southafricanorth'
$Kv        = 'WWExecutionEngine'
$KvSecret  = 'WWExecutionEngineTestSecret'
$Acr       = 'tudev2containerregistry'
$AcaEnv    = 'dev2-cae'
$Workspace = 'workspace-2028k'

# ── Staging ───────────────────────────────────────────────────────────────────
$Stage         = 'G:\Deployment'
$LogDir        = "$Stage\logs\e2e-wwengine2"        # dedicated: keeps this run's summaries unambiguous
$SecureConfig  = "$Stage\settings\secure.config"
$AuthConfig    = "$Stage\settings\Deploy-WwExecutionEngine.authconfig.json"
$LicenseConfig = "$Stage\settings\Warewolf License.secureconfig"
$WorkflowsSrc  = 'C:\ProgramData\Warewolf\Resources'
$SourceDir     = "$Stage\sources"                   # reused as-is
$TriggerSrcDir = "$Stage\triggers"                  # ORIGINALS - read only, never modified
$TriggerDir    = "$Stage\triggers-e2e"              # THIS RUN's isolated copies (created in Step 0.2)

# ── CREATED by this run ───────────────────────────────────────────────────────
$EngineApp     = 'wwengine2'
$EngineStorage = 'stwwengine2'
$EngineAi      = 'wwengine2-ai'
$EngineAuthApp = 'wwengine2-auth'
# FRESH publish directories, deliberately NOT the 2026-08-05 ones: publishing over an existing
# tree leaves stale files behind, and it would make the "0 .bite files" invariant an inherited
# result rather than a real test of the current csproj. The old dirs stay as a known-good fallback.
$EnginePublish = "$Stage\apps\ExecutionEngine2"
$QpPublish     = "$Stage\apps\QueueProcessor2"      # MUST differ from $EnginePublish
$EngineUrl     = "https://$EngineApp.azurewebsites.net"
$QpPrefix      = 'wwqp2-'
$ImageRepo     = 'warewolf/queueprocessor-e2e'

# ── This run's dedicated queues (created by the publisher, non-durable) ───────
$QueueSuccess  = 'order-success-queue-e2e'
$QueueFailure  = 'order-failure-queue-e2e'

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
```

**Expected:** `$Sub` and `$TenantId` populate; no errors.

---

## 2. Step 0.1 — Preflight (🟢 read-only)

```powershell
# Tooling
pwsh  --version ; az version --query '"azure-cli"' -o tsv
az extension list --query "[].name" -o tsv           # containerapp is CORE - no `az extension add` needed

# Publish inputs
Test-Path "$EnginePublish\Warewolf.Execution.Lightweight.dll"      # True
Test-Path "$QpPublish\Warewolf.Execution.QueueProcessor.dll"       # True
(Get-ChildItem $QpPublish -Recurse -Filter *.bite).Count           # MUST be 0
$EnginePublish -ne $QpPublish                                      # True

# Settings inputs
$SecureConfig, $AuthConfig, $LicenseConfig | ForEach-Object { "{0,-70} {1}" -f $_, (Test-Path -LiteralPath $_) }

# Every trigger workflow must be staged, or Phase E cannot execute anything
Get-ChildItem $TriggerSrcDir -Filter *.bite | ForEach-Object {
    $wf = (Get-Content $_.FullName -Raw | ConvertFrom-Json).WorkflowName
    [pscustomobject]@{ Trigger=$_.Name; Workflow=$wf
                       Staged=(Test-Path -LiteralPath (Join-Path $WorkflowsSrc "$wf.bite")) }
} | Format-Table -AutoSize                                          # Staged must be True for BOTH

# This run's names must be free
az functionapp list --query "[?name=='$EngineApp'].name" -o tsv     # empty
az storage account check-name --name $EngineStorage --query nameAvailable -o tsv   # true
az ad app list --display-name $EngineAuthApp --query "length(@)" -o tsv            # 0
az containerapp list -g $Rg --query "[?starts_with(name,'$QpPrefix')].name" -o tsv # empty
az acr repository list --name $Acr -o tsv | Select-String 'queueprocessor-e2e'     # no match
```

**Expected:** every assertion above holds. **Any failure stops the run** — do not proceed to Phase B.

### Step 0.3 — 🟡 Publish both apps

Neither deploy script builds .NET, so the publish must exist before Phase B/C.

```powershell
dotnet publish Dev\Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj -c Release -o $EnginePublish
dotnet publish Dev\Warewolf.Execution.QueueProcessor\Warewolf.Execution.QueueProcessor.csproj -c Release -o $QpPublish
```

**Run them one at a time, not concurrently** — the two projects share transitive project references
(`Dev2.Common`, `Dev2.Data`, …) and parallel builds contend on the same `obj\` intermediate output.

Then **re-assert the invariants against the new directories**:

```powershell
Test-Path "$EnginePublish\Warewolf.Execution.Lightweight.dll"     # True
Test-Path "$QpPublish\Warewolf.Execution.QueueProcessor.dll"      # True
(Get-ChildItem $QpPublish -Recurse -Filter *.bite).Count          # MUST be 0
```

> The `.NET 8` SDK (`8.0.423`) and the `10.0.302` SDK are both installed and there is **no
> `global.json`**, so `dotnet publish` uses SDK 10 to build these `net8.0` projects. That resolves
> correctly here; if a future SDK breaks it, pin with a temporary `global.json`.

---

## 3. Step 0.2 — Build this run's isolated trigger copies (🟡 local files only)

Creates `$TriggerDir` from the originals with **new `TriggerId`s** and **new queue names**. The
originals in `$TriggerSrcDir` are read and never written.

```powershell
New-Item -ItemType Directory -Force -Path $TriggerDir | Out-Null

$map = @(
  @{ Src='03fb9052-7fe4-4e8b-ac18-53779b0ebcba.bite'
     NewId='2bdec488-5780-48a5-b262-ea1800028b03'
     Queue=$QueueSuccess; Dlq="$QueueSuccess-errors" }
  @{ Src='12345678-7fe4-4e8b-ac18-53779b0ebcba.bite'
     NewId='3ecb4759-3a13-4881-89ea-f940308cff08'
     Queue=$QueueFailure; Dlq="$QueueFailure-errors" }
)

foreach ($m in $map) {
    $t = Get-Content (Join-Path $TriggerSrcDir $m.Src) -Raw | ConvertFrom-Json
    $t.Id              = $m.NewId
    $t.TriggerId       = $m.NewId
    $t.QueueName       = $m.Queue
    $t.DeadLetterQueue = $m.Dlq
    # Name, WorkflowName, Concurrency, Prefetch, Durable, Inputs and source ids are DELIBERATELY
    # unchanged - Durable stays TRUE so this run proves a Durable=true trigger consumes the
    # publisher's NON-DURABLE queue (see section 0.6).
    $t | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $TriggerDir "$($m.NewId).bite") -Encoding UTF8
}

# Verify the copies
Get-ChildItem $TriggerDir -Filter *.bite | ForEach-Object {
    $t = Get-Content $_.FullName -Raw | ConvertFrom-Json
    [pscustomobject]@{
        File=$_.Name; Name=$t.Name; Queue=$t.QueueName; Workflow=$t.WorkflowName
        Concurrency=$t.Concurrency; Prefetch=$t.Prefetch
        SourceId=$t.QueueSourceId; SinkId=$t.QueueSinkId; DLQ=$t.DeadLetterQueue
        Durable=($t.Options|Where-Object Name -eq 'Durable').Value
        DlqDurable=($t.DeadLetterOptions|Where-Object Name -eq 'Durable').Value
    }
} | Format-Table -AutoSize
```

**Expected:**

| File | Name | Queue | Workflow | Conc | Prefetch | DLQ | Durable |
|---|---|---|---|---|---|---|---|
| `2bdec488-….bite` | `OrderSuccessQueue` | `order-success-queue-e2e` | `rabbit\RabbitProcess` | 3 | 1 | `order-success-queue-e2e-errors` | True |
| `3ecb4759-….bite` | `OrderFailureQueue` | `order-failure-queue-e2e` | `rabbit\RabbitProcessFailure` | 1 | 1 | `order-failure-queue-e2e-errors` | True |

`QueueSourceId` and `QueueSinkId` must both remain `fa5f49d7-f6d7-422c-b08e-17b094d82f1d`, which
resolves to `$SourceDir\RabbitMQSourceAshley.bite`.

Derived Container App names: `wwqp2-ordersuccessqueue`, `wwqp2-orderfailurequeue` (prefix + trigger
`Name` slug; the two `Name`s differ, so no collision escalation occurs).

---

## 4. Phase A — Confirm what is reused (🟢 creates nothing)

Unlike the source runbook, **this run creates nothing in Phase A**: `rabbitmq-uri` already exists.

```powershell
# A1 - reused resources fit for purpose
az group show --name $Rg --query properties.provisioningState -o tsv                  # Succeeded
az keyvault secret show --vault-name $Kv --name $KvSecret --query attributes.enabled -o tsv  # true
az acr show --name $Acr -g $Rg --query "{loc:location,sku:sku.name,publicNet:publicNetworkAccess,state:provisioningState}" -o json
az containerapp env show --name $AcaEnv -g $Rg --query "{state:properties.provisioningState,loc:location,vnet:properties.vnetConfiguration,profiles:properties.workloadProfiles[].name,wsCid:properties.appLogsConfiguration.logAnalyticsConfiguration.customerId}" -o json

# Data-plane role on the vault (control-plane Owner is NOT enough)
$me = (az ad signed-in-user show --query id -o tsv)
az role assignment list --assignee $me --scope (az keyvault show --name $Kv --query id -o tsv) `
    --query "[].roleDefinitionName" -o tsv        # need Key Vault Secrets Officer (or User)

# A2 - the AMQP secret KEDA needs already exists; just resolve its VERSIONLESS uri
az keyvault secret show --vault-name $Kv --name 'rabbitmq-uri' --query "{name:name,enabled:attributes.enabled}" -o json
$vaultUri        = (az keyvault show --name $Kv --query properties.vaultUri -o tsv)
$RabbitSecretUri = ($vaultUri.TrimEnd('/')) + '/secrets/rabbitmq-uri'
$RabbitSecretUri            # https://wwexecutionengine.vault.azure.net/secrets/rabbitmq-uri

# A3 - environment tenancy: confirm this run's names are absent and see the neighbours
az containerapp list -g $Rg --query "[?contains(properties.environmentId,'$AcaEnv')].{name:name,min:properties.template.scale.minReplicas,max:properties.template.scale.maxReplicas,cpu:properties.template.containers[0].resources.cpu}" -o table
```

**Expected:** RG `Succeeded`; secret `true`; ACR `southafricanorth` / `Basic` / `Enabled` /
`Succeeded`; env `Succeeded` / `South Africa North` / `vnet: null` / `[Consumption]` /
cid `405a4834-…`; the app list shows `sharepoint-wiremock`, `exchange-wiremock`,
`wwqp-ordersuccessqueue`, `wwqp-orderfailurequeue` and **no `wwqp2-*`**.

**Environment core budget — this run doubles the ACA footprint.** The deploy script prints
`Σ (maxReplicas × cpu)` for **its own apps only**; add the rest by hand:

| | cores at peak |
|---|---|
| `sharepoint-wiremock` (0.5 × 1, `min 1` — always on) | 0.5 |
| `exchange-wiremock` (0.5 × 1, `min 0`) | 0.5 |
| **existing** `wwqp-ordersuccessqueue` (0.5 × 3) + `wwqp-orderfailurequeue` (0.5 × 1) | 2.0 |
| **this run** `wwqp2-ordersuccessqueue` (0.5 × 3) + `wwqp2-orderfailurequeue` (0.5 × 1) | 2.0 |
| **environment peak** | **5.0** |

Only this run's 2.0 and the 0.5 always-on floor are actually incurred — the old `wwqp-*` apps stay at
0 replicas because **nothing publishes to their queues**. Check headroom:

```powershell
az quota show --scope "/subscriptions/$Sub/providers/Microsoft.App/locations/$Loc" `
              --resource-name ManagedEnvironmentConsumptionCores -o table 2>$null
# If unavailable in-region: az vm list-usage --location $Loc -o table
```

---

## 5. Phase B — Execution Engine `wwengine2`

### B1 — Dry run (🟢 changes nothing)

```powershell
cd Dev\Warewolf.Execution.Lightweight\Scripts

.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $Rg -Location $Loc `
  -AppName $EngineApp -StorageAccount $EngineStorage `
  -PublishPath $EnginePublish `
  -TenantId $TenantId `
  -AuthConfigPath $AuthConfig `
  -SecureConfigPath $SecureConfig `
  -LicenseConfigPath $LicenseConfig `
  -WorkflowsSourcePath $WorkflowsSrc `
  -KeyVaultName $Kv -KeyVaultSecretName $KvSecret `
  -EncryptResources:$true -VerifyDecryption `
  -LogDir $LogDir `
  -DryRun
```

**Review the Phase 0.5 PLAN before approving anything.** Confirm:

- `App name` = `wwengine2`, `Storage` = `stwwengine2`, `Resource group` = `DEV2`.
- `Key Vault` = `WWExecutionEngine`, secret `WWExecutionEngineTestSecret`, **created: no**.
- `Workflows source` = `C:\ProgramData\Warewolf\Resources` — **must not be `(none)`**, or every
  route 404s.
- `Encrypt sources (this run)` = **YES**, `Verify decryption` = yes.
- Entra app to create = `wwengine2-auth`.

> `-EncryptResources` operates on a **temp staging copy**
> ([Deploy-WwExecutionEngine.ps1:1322-1327](../Scripts/Deploy-WwExecutionEngine.ps1#L1322-L1327),
> [:1403-1415](../Scripts/Deploy-WwExecutionEngine.ps1#L1403-L1415)) — `C:\ProgramData\Warewolf\Resources`
> is **never modified**. It must still run **on this machine**, because DPAPI-encrypted `.bite` files
> can only be converted to WFAES where they were created.

### B2 — 🔴 Deploy for real

Re-run **the identical command without `-DryRun`**.

**Creates:** `stwwengine2`, `wwengine2`, `wwengine2-ai`, Entra app `wwengine2-auth` + SP + an Easy
Auth client secret in `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET`, app settings, and a WFAES pass over
the staged workflow resources.

**Expected:** `status: completed`, `lastPhase: Phase 5  Verify`, encryption reports **N/N verified
decryptable**, and a summary at `$LogDir\deploy-WwExecutionEngine-<stamp>.summary.json` with
`created.storageAccount = true`, `created.functionApp = true`, `created.appInsights = true`,
`created.entraApp = true`, `created.resourceGroup = false`, `created.keyVault = false`.

### B3 — 🟢 Capture the identifiers Phase C and the rollback need

```powershell
$Summary     = (Get-ChildItem $LogDir -Filter 'deploy-WwExecutionEngine-*.summary.json' |
                Where-Object Name -notmatch 'dryrun' | Sort-Object LastWriteTime | Select-Object -Last 1).FullName
$Summary
$RunId       = (Get-Content $Summary -Raw | ConvertFrom-Json).runId          # wwx-<stamp>
$EngineAppId = (az ad app list --display-name $EngineAuthApp --query "[0].appId" -o tsv)
$EngineSpId  = (az ad sp  list --display-name $EngineAuthApp --query "[0].id"    -o tsv)
$AiConn      = (az monitor app-insights component show --app $EngineAi -g $Rg --query connectionString -o tsv)
"summary=$Summary`nrunId=$RunId`nappId=$EngineAppId`nspId=$EngineSpId"
```

**Record all four in the summary document — the rollback depends on `$Summary` and `$EngineAppId`.**

### B4 — 🟢 Verify the three route families

There is **no `/api` prefix** — `host.json` sets `routePrefix: ""`.

```powershell
$smoke = 'Hello World'

# 1. Public - must succeed anonymously                      -> 200
Invoke-WebRequest "$EngineUrl/Public/$([uri]::EscapeDataString($smoke)).json?Name=e2e" |
    Select-Object -ExpandProperty Content

# 2. Secure - must be REJECTED without a token              -> 401
(Invoke-WebRequest "$EngineUrl/Secure/$([uri]::EscapeDataString($smoke)).json?Name=e2e" -SkipHttpErrorCheck).StatusCode

# 3. Secure - must SUCCEED with an app-only token           -> 200
$tok = (az account get-access-token --resource "api://$EngineAppId" --query accessToken -o tsv)
Invoke-WebRequest "$EngineUrl/Secure/$([uri]::EscapeDataString($smoke)).json?Name=e2e" `
                  -Headers @{ Authorization = "Bearer $tok" } | Select-Object -ExpandProperty Content

# 4. Services - function key required                       -> 401 without the header
(Invoke-WebRequest "$EngineUrl/Services/$([uri]::EscapeDataString($smoke)).json" -SkipHttpErrorCheck).StatusCode
```

### B5 — 🟢 The check that actually predicts Phase E

```powershell
foreach ($wf in (Get-ChildItem $TriggerDir -Filter *.bite |
                 ForEach-Object { (Get-Content $_.FullName -Raw | ConvertFrom-Json).WorkflowName })) {
    $route = ($wf -replace '\\','/') -split '/' | ForEach-Object { [uri]::EscapeDataString($_) }
    $r = Invoke-WebRequest "$EngineUrl/Secure/$($route -join '/').json" `
                           -Headers @{ Authorization = "Bearer $tok" } -SkipHttpErrorCheck
    "{0,-40} {1}  {2}" -f $wf, $r.StatusCode, ($r.Content -replace '\s+',' ')
}
```

**Read the body, not the status code.** 500 has three different causes:

| Body contains | Meaning | Fix |
|---|---|---|
| `Workflow file not found: …\Resources\<name>.xml` | **not staged** | fix `WorkflowName`, or restage `-WorkflowsSourcePath` |
| nested `Error{…}` with no execution detail | **not authorized** (WOLF-8418 wraps denial as 500) | add the `secure.config` grant — but see the narrowing warning in §0.3 |
| a workflow message, e.g. `Scalar value { message } is NULL` | **staged, authorized, and it EXECUTED** | ✅ this is the pass condition here |

For `rabbit/RabbitProcess` and `rabbit/RabbitProcessFailure` the **expected** result is the third
row — they run and fail on their unbound input, which proves deployment correctness.

### B6 — 🟢 Confirm the publisher workflow is reachable through the engine

```powershell
(Invoke-WebRequest "$EngineUrl/Public/rabbit/RabbitPublish.json" -SkipHttpErrorCheck).StatusCode
```

**Expected:** not 404. (A 500 with a workflow-level body is fine — it means found, authorized,
executed without inputs.) E2 depends on this route.

---

## 6. Phase C — QueueProcessor, one Container App per trigger

### C1 — Dry run over the isolated trigger folder (🟢 changes nothing)

```powershell
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup $Rg -Location $Loc `
  -AcaEnvironment $AcaEnv -AcrName $Acr `
  -PublishPath $QpPublish `
  -ImageRepository $ImageRepo `
  -AppNamePrefix $QpPrefix `
  -TriggerPath $TriggerDir `
  -QueueSourcePath $SourceDir `
  -EngineBaseUrl $EngineUrl `
  -EngineResourceAppId $EngineAppId `
  -EngineTenantId $TenantId `
  -KeyVaultName $Kv -KeyVaultSecretName $KvSecret -EncryptStagedSettings `
  -RabbitMqSecretUri $RabbitSecretUri -InlineRabbitMqSecret `
  -ScalingMode Elastic `
  -EnableAppInsights -AppInsightsConnectionString $AiConn `
  -LogDir $LogDir `
  -DryRun
```

**The plan output is the review gate.** Confirm:

- `Triggers resolved : 2`, and the two names are `OrderSuccessQueue` / `OrderFailureQueue`.
- `Sources to stage : 1` — id `fa5f49d7-f6d7-422c-b08e-17b094d82f1d`, referenced by both triggers.
  A gap aborts with `Unresolved RabbitMQ source(s); nothing was deployed` **before** anything is
  created.
- Per app: `max=3 min=0 value=1` and `max=1 min=0 value=1`.
- App names `wwqp2-ordersuccessqueue`, `wwqp2-orderfailurequeue` — **no rename advisory**.
- Image `tudev2containerregistry.azurecr.io/warewolf/queueprocessor-e2e:<stamp>` — **`-e2e` suffix
  present**, so the 2026-08-05 image is untouched.
- **No prefetch advisory** (both triggers are already `Prefetch = 1`).
- Peak cores for this run = **2.0**; add 3.0 for the neighbours (§Phase A).

> **Do not pass `-MaxReplicas`.** Scale comes from the trigger's `Concurrency`; an explicit override
> is flagged as an exception because it overrides the authored concurrency contract.
>
> **`-InlineRabbitMqSecret` is required in this tenant.** CAE is enforced, and ACA's secret-sync path
> cannot answer a claims challenge — a runtime `keyvaultref` fails repeatedly with
> `401 AKV10203 … CaeAuthorizationFailed` and KEDA never reads the queue depth. The switch copies the
> URI out of Key Vault at deploy time. Key Vault stays the source of truth; rotation then needs a
> redeploy.
>
> **`-EncryptStagedSettings` applies two modes**: attribute mode on `sources\{id}.bite` (XML
> `<Source ConnectionString=…>` → `WFAES::…`) and **whole-file** mode on `triggers\{id}.bite` (JSON,
> no `<Source>` element). Both use the same `$KvSecret`. Every staged file is re-read with
> `-VerifyOnly` **before** the image builds. Required here because the staged source is **DPAPI**,
> which a Linux container can never read.

### C2 — 🔴 Deploy for real

Re-run **the identical command without `-DryRun`**.

**Creates:** ACR repo `warewolf/queueprocessor-e2e` + one image tag; two Container Apps in the shared
`dev2-cae` with system-assigned identities; **two RBAC writes per app** — `AcrPull` on the registry
and `Key Vault Secrets User` on the vault; the `rabbitmq-connection` secret; env vars; and the KEDA
scale rule (Phase D).

**Expected:** both apps report `status: deployed` in
`$LogDir\deploy-WwQueueProcessor-<stamp>.summary.json`, each with `minReplicas 0`,
`maxReplicas 3`/`1`, `targetLength 1`, `prefetch 1`, `scalingMode Elastic`, and a `principalId`.
**Record both `principalId`s** — the rollback cleans their role assignments.

### C3 — 🟢 The optional app-role grant (defence in depth, NOT a prerequisite)

Measured on the 2026-08-05 run: **the grant was not required.** `rabbit\*` carries no
Execute-bearing resource entry, so it resolves against the global map and `Public`'s server-level
`Execute` is OR'd in, which covers any authenticated caller. Skip it, or apply it deliberately:

```powershell
$apps = @(az containerapp list -g $Rg --query "[?starts_with(name,'$QpPrefix')].name" -o tsv)
$roleId = (az ad app show --id $EngineAppId --query "appRoles[?value=='Warewolf_QueueProcessor'].id | [0]" -o tsv)

foreach ($app in $apps) {                                  # 🔴 DIRECTORY CHANGE
    $mi = (az containerapp show --name $app -g $Rg --query identity.principalId -o tsv)
    az rest --method POST `
      --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$mi/appRoleAssignments" `
      --body (@{ principalId=$mi; resourceId=$EngineSpId; appRoleId=$roleId } | ConvertTo-Json)
}
```

⚠️ **Do not add a per-workflow `secure.config` row "to be safe".** Once a workflow has any
Execute-bearing resource entry it uses the resource map *exclusively* and stops inheriting the global
grant — a row naming the wrong group would **lock the worker out** of a workflow it can currently
execute.

### C4 — 🟢 Verify provisioning (necessary, **not** sufficient)

```powershell
foreach ($app in $apps) {
    az containerapp revision list --name $app -g $Rg `
        --query "[0].{rev:name,healthy:properties.healthState,active:properties.active}" -o table
}
az containerapp show -n $apps[0] -g $Rg --query "properties.template.containers[0].image" -o tsv
```

> ⚠️ **`Healthy` at `minReplicas = 0` proves nothing about the workload.** No container has started.
> Three separate startup failures hid behind a `Healthy` revision on this migration: exit **150**
> (`runtime:8.0` base image lacking `Microsoft.AspNetCore.App`), exit **2** (`Settings/` staged then
> discarded), and `no such file or directory` (Windows publish has no Linux apphost). **Never report
> Phase C complete on configuration alone — E0 forces a replica and reads its output.**

---

## 7. Phase D — Verify the KEDA scale rules (🟢 read-only)

Phase C already applied them. This confirms the numbers.

```powershell
foreach ($app in $apps) {
    az containerapp show --name $app -g $Rg --query `
      "{app:name,min:properties.template.scale.minReplicas,max:properties.template.scale.maxReplicas,rules:properties.template.scale.rules}" -o json
}
```

**Expected, per app:**

| App | min | max | rule type | `queueName` | `mode` | `value` | `protocol` | `activationValue` |
|---|---|---|---|---|---|---|---|---|
| `wwqp2-ordersuccessqueue` | **0** | **3** | `rabbitmq` | `order-success-queue-e2e` | `QueueLength` | **1** | `amqp` | **absent** |
| `wwqp2-orderfailurequeue` | **0** | **1** | `rabbitmq` | `order-failure-queue-e2e` | `QueueLength` | **1** | `amqp` | **absent** |

Exactly **one** rule per app, `--scale-rule-auth host=rabbitmq-connection`.
`activationValue` must stay unset — it defaults to 0, so a **single** message wakes the app. Setting
it would strand a low-volume queue at zero indefinitely.

Governing formula: `desiredReplicas = clamp( ceil(queueLength / value), minReplicas, maxReplicas )`.
With `value = 1` this is `min(queueLength, Concurrency)` — one replica per waiting message, capped at
the on-prem parity ceiling, and **0 when empty**.

---

## 8. Phase E — End-to-end proof

### E0 — 🔴 Create the queues and force the first replica (the non-durable test)

The queues do not exist yet. This publishes **one** message to each, which creates them
**non-durable** via `PublishRabbitMQActivity`, and proves the `Durable=true` worker consumes them.

```powershell
# Publish via the ENGINE (proves engine -> broker end to end)
Invoke-WebRequest "$EngineUrl/Public/rabbit/RabbitPublish.json?queue=$QueueSuccess&total=1" -SkipHttpErrorCheck |
    Select-Object -ExpandProperty StatusCode
Invoke-WebRequest "$EngineUrl/Public/rabbit/RabbitPublish.json?queue=$QueueFailure&total=1" -SkipHttpErrorCheck |
    Select-Object -ExpandProperty StatusCode

# Fallback publisher, if the engine route is unavailable - the LOCAL Warewolf Server:
#   Invoke-WebRequest "http://localhost:3142/secure/rabbit/RabbitPublish.json?queue=$QueueSuccess&total=1" `
#       -UseDefaultCredentials -AllowUnencryptedAuthentication -TimeoutSec 30
```

> `RabbitPublish`'s inputs are **`queue`** and **`total`**; the body is the literal `hello [[c]]`.
> One call publishes `total` messages — this is the §0.7 correction to the source runbook's E2.

Then pin a replica long enough to read its cold-start output — a replica that lives under ~a minute
frequently never reaches Log Analytics, and `az containerapp logs show` is a **live tail** that never
replays:

```powershell
$app = "$($QpPrefix)ordersuccessqueue"
az containerapp update -n $app -g $Rg --min-replicas 1      # 🔴 temporary
az containerapp logs show --name $app -g $Rg --tail 60
az containerapp update -n $app -g $Rg --min-replicas 0      # 🔴 RESTORE - do not skip
```

**Expected cold-start lines, in this order:**

```
[QueueProcessor-SourceCatalog] Source catalog cached 1 RabbitMQ source(s) at startup from '/app/Settings/sources' …
[QueueProcessor-ConfigLoader]  loading triggers from '/app/Settings/triggers' (filter '*.bite'); sources from '/app/Settings/sources'
[QueueProcessor-ConfigLoader]  resolved trigger 'OrderSuccessQueue' (2bdec488-…): queue='order-success-queue-e2e', prefetch=1, durable=True, source=amqp://user@host:port/
[QueueProcessor-Pump]          Consuming queue 'order-success-queue-e2e' (prefetch 1, maxConcurrency 1, consumerTag '…')
```

The catalog line **must precede** the loader line. Confirm **no password** appears (the connection is
described as `amqp://user@host:port/`) and **no** `ENGINE__TENANTID is not set` warning.

✅ **Non-durable acceptance:** `Consuming queue …` with **no** `PRECONDITION_FAILED — inequivalent
arg 'durable'` proves a `Durable=true` trigger consumes a publisher-created **non-durable** queue.

### E1 — 🟢 Zero baseline

```powershell
az containerapp replica list --name $app -g $Rg --query "length(@)" -o tsv     # expect 0
```

Allow up to ~5 minutes after the queue empties — ACA's cool-down is not configurable. If it never
reaches 0: the queue is not empty, or `--min-replicas` was left at 1 by E0.

### E2 — 🔴 Publish a burst

```powershell
Invoke-WebRequest "$EngineUrl/Public/rabbit/RabbitPublish.json?queue=$QueueSuccess&total=5" -SkipHttpErrorCheck |
    Select-Object -ExpandProperty StatusCode
```

### E3 — 🟢 Watch KEDA scale from zero

ACA polls at roughly 30 s, so allow 30–60 s for the first replica.

```powershell
1..18 | ForEach-Object {
    "{0}  replicas={1}" -f (Get-Date -Format HH:mm:ss),
        (az containerapp replica list --name $app -g $Rg --query "length(@)" -o tsv)
    Start-Sleep -Seconds 10
}
```

Confirm **two** things:

1. **Activation** — the count leaves 0 without intervention (proves `activationValue` is default).
2. **Scaling** — peak = `min(ceil(5 / 1), 3)` = **3**. A peak of 1 for a 5-message backlog is the
   `value`-too-high symptom, not a broken scaler.

**Record the peak.**

### E3a — 🟢 Measure `T`

Take the median `durationMs` from the execution logs. That is `T` in `value ≈ max(1, floor(L / T))`.
Until measured, any `value` above 1 is guesswork.

### E3b — 🟢 Does `QueueLength` count unacked messages?

With one long-running message in flight and an otherwise empty queue:

```powershell
az containerapp replica list --name $app -g $Rg --query "length(@)" -o tsv
```

If it drops to 0 mid-execution, the scaler counts **ready only** and the drain path (E7) is the
safety net — so `-ShutdownGraceSeconds` must comfortably exceed the longest workflow. If it stays
at 1, unacked messages hold the replica open. **Record which.**

### E4 — 🟢 Confirm execution

```powershell
az containerapp logs show --name $app -g $Rg --tail 100 --follow:$false |
    Select-String 'Queue execution (starting|succeeded|failed)'
```

**The reliable evidence is App Insights `dependencies`** — short-lived replicas often never reach
Log Analytics:

```powershell
$q = "dependencies | where timestamp > ago(1h) | where name has 'RabbitProcess'" +
     " | summarize calls=count() by resultCode, tostring(success)"
az monitor app-insights query --app $EngineAi -g $Rg --analytics-query $q -o json
```

**Expected:** one `Queue execution succeeded` per message (each with `txn=` and `durationMs=`), and
`resultCode=200  success=True  calls=6` (1 from E0 + 5 from E2).

### E5 — 🟢 Confirm the queue drained and nothing was dead-lettered

> ⚠️ **A drained queue is NOT evidence of success.** The contract is *2xx → ack* and *non-2xx →
> dead-letter **and** ack*. **Both drain the queue and both scale back to zero.** Distinguish them by
> the `dependencies` `resultCode` above, or by the depth of `order-success-queue-e2e-errors` (the
> trigger's own DLQ). A **stuck** main queue is the opposite problem: nacked and redelivered,
> visible as the app pinned at `RunningAtMaxScale`.

### E6 — 🟢 Watch it scale back to zero

```powershell
1..36 | ForEach-Object {
    $n = (az containerapp replica list --name $app -g $Rg --query "length(@)" -o tsv)
    "{0}  replicas={1}" -f (Get-Date -Format HH:mm:ss), $n
    if ($n -eq '0') { 'reached zero'; break }
    Start-Sleep -Seconds 10
}
```

Allow **up to ~5 minutes**. ACA's cool-down is platform-managed and not exposed as a flag — a slow
scale-in is **not** a fault. Reaching 0 is the cost saving that justifies the whole migration.

### E7 — 🔴 Graceful drain (recommended before production)

```powershell
$rev = (az containerapp revision list --name $app -g $Rg --query "[?properties.active].name | [0]" -o tsv)
az containerapp revision restart --name $app -g $Rg --revision $rev
az containerapp logs show --name $app -g $Rg --tail 60 |
    Select-String 'Cancelled consumer|Drain of|still in flight'
```

`Drain of '<queue>' completed cleanly` is the good outcome. `Drain window … elapsed with N
message(s) still in flight` means those messages will be **redelivered and may run twice** — raise
`-ShutdownGraceSeconds` or lower `-EngineTimeoutSeconds`, keeping
`EngineTimeout ≤ ShutdownGrace < TerminationGracePeriod`.

### Acceptance criteria

| # | Criterion | Step |
|---|---|---|
| 1 | `/Public/*` returns 200 anonymously | B4 |
| 2 | `/Secure/*` returns 401 without a token and 200 with one | B4 |
| 3 | Both trigger workflows resolve (500 with a **workflow-level** body = pass) | B5 |
| 4 | Both revisions `Healthy`; catalog + loader + pump lines logged; no tenant warning | E0 |
| 5 | **One app per trigger**, each with exactly one `rabbitmq` rule | D |
| 6 | `min = 0`; `max` = 3 / 1; `value` = 1 | D |
| 7 | No `activationValue` set | D |
| 8 | **A `Durable=true` trigger consumes the publisher's NON-DURABLE queue** — no `PRECONDITION_FAILED` | E0 |
| 9 | Replicas sit at 0 on an empty queue | E1 |
| 10 | Publishing raises replicas above 0 with **no** manual intervention | E3 |
| 11 | Peak replicas = `min(ceil(messages / value), maxReplicas)` = **3** | E3 |
| 12 | `T` (median `durationMs`) recorded | E3a |
| 13 | Whether `QueueLength` counts unacked messages recorded | E3b |
| 14 | One `Queue execution succeeded` per message; `resultCode=200 success=True` | E4 |
| 15 | Work queue drains to 0; DLQ does **not** grow | E5 |
| 16 | Replicas return to 0 within the cool-down | E6 |
| 17 | A forced restart drains cleanly | E7 |
| 18 | The 2026-08-05 run is **byte-for-byte untouched** | rollback doc §5 |

Criteria **5–8** and **11** are what this run exists to prove beyond "it deployed".

---

## 9. Phase F — Teardown

**In [Deploy-E2E-Rollback-Commands.md](Deploy-E2E-Rollback-Commands.md).** Do not improvise it —
`DEV2` is shared and a wrong command destroys an AKS cluster, the live `WarewolfServer*` apps, or the
2026-08-05 deployment.

---

## 10. Approval inventory — every command that changes state

| Step | Command | Effect |
|---|---|---|
| 0.2 | trigger-copy script | 🟡 **Local only** — writes `G:\Deployment\triggers-e2e`. Originals untouched |
| A | *(none)* | Phase A creates **nothing** — `rabbitmq-uri` already exists |
| B2 | `Deploy-WwExecutionEngine.ps1` (no `-DryRun`) | 🔴 Creates `stwwengine2`, `wwengine2`, `wwengine2-ai`, **Entra app `wwengine2-auth`** + SP + client secret; WFAES-encrypts a staged copy of the workflow resources |
| C2 | `Deploy-WwQueueProcessor.ps1` (no `-DryRun`) | 🔴 `az acr build` → **new repo `warewolf/queueprocessor-e2e`** in the reused registry; **2 Container Apps** into the shared `dev2-cae`; **4 RBAC writes** (`AcrPull` ×2, `Key Vault Secrets User` ×2); WFAES-encrypts staged triggers/sources |
| C3 | `az rest … appRoleAssignments` | 🔴 **Directory change** — optional, and measured as **not required** |
| E0 | `RabbitPublish` ×2, `containerapp update --min-replicas 1/0` | 🔴 **Creates 2 non-durable queues + 2 DLQs** on the dev broker; temporarily pins a replica |
| E2 | `RabbitPublish` `total=5` | 🔴 Publishes 5 real messages |
| E7 | `az containerapp revision restart` | 🔴 Restarts this run's revision only |
| F | see rollback doc | 🔴 **Targeted deletes only.** ⛔ never `az group delete` |

Read-only throughout (safe unattended): `az account show`, `az * show`, `az * list`,
`az containerapp logs show`, `az containerapp replica list`, `az role assignment list`,
`az quota show`, `az monitor app-insights query`, and any script run with **`-DryRun`**.
Note `Encrypt-Config.ps1` **reads** the Key Vault secret (data-plane read) even in a dry run; it
never writes to Key Vault unless `-GenerateKeys` is passed, which this run never does.
