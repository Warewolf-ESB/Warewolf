# End-to-End Deployment & Verification Runbook — Execution Engine + RabbitMQ QueueProcessor

Deploys and **proves** the full Azure path in one sitting, into a **disposable** resource group you
delete at the end:

| Phase | Deliverable | Verified by |
|---|---|---|
| **A** | Disposable foundation — RG, ACR, ACA environment, Key Vault | `az … show` returns `Succeeded` |
| **B** | **Execution Engine** — workflow API over `/Public/*` and `/Secure/*` | anonymous 200 on `/Public`, 401 without a token on `/Secure`, 200 with one |
| **C** | **QueueProcessor** — one Container App per RabbitMQ trigger, triggers + sources baked in | revision `Healthy`, source catalog logged at startup |
| **D** | **ACA scale rule (KEDA)** per trigger — `Elastic`, `minReplicas 0`, tuned per **§4** | `az containerapp show` reports one `rabbitmq` rule with the intended `min`/`max`/`value` |
| **E** | **End-to-end proof** — publish to RabbitMQ, KEDA scales 0→N, message executes | replica count rises from 0, `Queue execution succeeded` in logs, queue drains |
| **F** | **Teardown** — surgical, by name and run tag | targeted `az containerapp delete` + `Rollback-WwExecutionEngine.ps1`. ⛔ **never `az group delete`** |

> ### ⚡ There is now an automated harness for this
> Unless you specifically want to drive the phases by hand, use
> [E2E-Harness-README.md](E2E-Harness-README.md) instead — `New-WwE2EStaging.ps1` +
> `Invoke-WwE2EVerification.ps1` run these phases, score the acceptance criteria and write a summary,
> with a unique run suffix so two reviewers never collide. It also has the corrections this runbook
> predates: broker topology must be **pre-created** (§S11), the documented
> `az account get-access-token` step **cannot work** on a fresh app registration (§S9), and execution
> evidence comes from **Log Analytics**, not App Insights `traces` (§S12). Details in
> [Deploy-E2E-Execution-Summary.md](Deploy-E2E-Execution-Summary.md).

> ### Read before running
> - **Everything in Phase A is billable** while it exists. Phase F removes all of it in one command
>   *because* every resource lives in one dedicated group. Do not point this runbook at a shared
>   resource group — Phase F would delete more than it created.
> - **On "KEDA or ACA":** these are not alternatives. **Azure Container Apps runs KEDA internally**;
>   an ACA scale rule of type `rabbitmq` *is* the KEDA RabbitMQ scaler, which is why its metadata is
>   `queueName` / `mode` / `value`. "Separate instances for each trigger" therefore means **one
>   Container App per trigger**, each with its own scale rule — which is what Phase C/D do. A
>   standalone KEDA operator would mean owning an AKS cluster and is **not** implemented here.
> - **Target scaling shape** — chosen in **§0.1**, justified in **§4**, verified in **§4.8** and **E3**:
>
>   | | Value | Effect |
>   |---|---|---|
>   | `minReplicas` | **0** | Scales in to zero whenever the queue is empty |
>   | `maxReplicas` | trigger `Concurrency` | On-prem parity ceiling |
>   | `MaxConcurrency` | **1** | One workflow per replica — dispatch is serial per channel |
>   | `Prefetch` | **1** | No parked messages: fastest scale-out, smallest redelivery window |
>   | KEDA `value` | **1** (or `floor(L/T)`) | `desiredReplicas = min(queueLength, maxReplicas)` |
>
>   **Scale out, not up.** `Prefetch` is settable **only in the trigger `.bite`** — not by a flag —
>   so settle it in §0.1 before Phase C.
> - Parameter reference for both scripts: [Deploy-RunGuide.md](Deploy-RunGuide.md).
>   Day-to-day (non-verification) deployment: [Deploy-EndToEnd-Runbook.md](Deploy-EndToEnd-Runbook.md).
>   Design rationale: [QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md](QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md).

---

## 0. Inputs

Run this block **once** per session; every later step reuses it. Values marked **ASK** are the ones
the runbook cannot derive — everything else is defaulted.

```powershell
# ── Identity / subscription ───────────────────────────────────────────────────
az login                                          # interactive
$Sub      = (az account show --query id -o tsv)
$TenantId = (az account show --query tenantId -o tsv)
az account set --subscription $Sub

# ── Foundation ────────────────────────────────────────────────────────────────
# ONLY the resource group and the Key Vault are reused. Everything else this run touches is
# created BY this run and removed by Phase F.
# DEV2 is SHARED — it also holds an AKS cluster and the live WarewolfServer / -UAT apps, so
# Phase F is surgical: NEVER `az group delete` this group.
$Rg       = 'DEV2'                        # REUSED (allowed)
$Loc      = 'southafricanorth'
$Kv       = 'WWExecutionEngine'           # REUSED (allowed), RBAC-enabled
$KvSecret = 'WWExecutionEngineTestSecret' # REUSED key — see the note below

# REUSED to avoid creating billable duplicates (all verified present, southafricanorth, DEV2):
$Acr      = 'tudev2containerregistry'     # REUSED registry — Basic, admin enabled, public access
$AcaEnv   = 'dev2-cae'                    # REUSED ACA environment — Consumption profile, no VNet
$Workspace = 'workspace-2028k'            # REUSED workspace — $AcaEnv ALREADY logs here (cid
                                          # 405a4834-1554-4f2a-b5eb-31789fffaa9b); nothing to bind

# CREATED by this run: no environment, no registry, no workspace. Only the Container Apps
# themselves (Phase C), the engine resources (Phase B), and one Key Vault secret.
#
# ⚠️ $AcaEnv is SHARED — it already hosts `sharepoint-wiremock` (cpu 0.5, min 1) and
#    `exchange-wiremock` (cpu 0.5, min 0). Phase F deletes Container Apps by the `wwqp-` prefix
#    only and NEVER deletes the environment.

# ── Staging root (all deployment inputs live here) ────────────────────────────
$Stage      = 'G:\Deployment'

# ── Execution Engine ─────────────────────────────────────────────────────────
$EngineApp     = 'wwengine1'                      # globally unique (…azurewebsites.net); confirmed free
$EngineStorage = 'stwwengine1'                    # 3-24 lowercase alphanumerics; confirmed free
$EnginePublish = "$Stage\apps\ExecutionEngine"
$EngineUrl     = "https://$EngineApp.azurewebsites.net"

# Engine package inputs (see the staging checklist below — all three are REQUIRED here)
$SecureConfig  = "$Stage\settings\secure.config"  # staged copy; identical to secure.config.cloud.json
$AuthConfig    = "$Stage\settings\Deploy-WwExecutionEngine.authconfig.json"
$LicenseConfig = "$Stage\settings\Warewolf License.secureconfig"
$WorkflowsSrc  = 'C:\ProgramData\Warewolf\Resources'   # the workflows the triggers call

# ── QueueProcessor ───────────────────────────────────────────────────────────
$QpPublish  = "$Stage\apps\QueueProcessor"        # MUST differ from $EnginePublish
$TriggerDir = "$Stage\triggers"
$SourceDir  = "$Stage\sources"

# ── RabbitMQ (for the KEDA scale rule only) ──────────────────────────────────
# KEDA cannot use a managed identity against RabbitMQ, so it needs an AMQP URI as a secret.
# PRODUCTION MUST BE amqps:// — see the go-live gate in the plan.
$RabbitUri  = 'amqp://testuser:test123@4.tcp.eu.ngrok.io:20313/'   # dev broker; NOT TLS
```

**Publish both apps before starting.** The deploy scripts do not build .NET:

```powershell
dotnet publish Dev\Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj -c Release -o $EnginePublish
dotnet publish Dev\Warewolf.Execution.QueueProcessor\Warewolf.Execution.QueueProcessor.csproj -c Release -o $QpPublish
```

> `$QpPublish` **must** be a different directory from `$EnginePublish` — different apps, different
> csproj. The engine deploy resolves both at plan time and fails loudly on a collision.
>
> The QueueProcessor's committed `Settings/` sample is deliberately **excluded** from publish output,
> so it can never leak into the image. Verify with
> `(Get-ChildItem $QpPublish -Recurse -Filter *.bite).Count` — it must be **0**. Real config arrives
> in Phase C.

### 0.0 Staging checklist

Everything the two deploys consume, and the check that it is actually usable:

| Path | Contents | Verify |
|---|---|---|
| `$Stage\apps\ExecutionEngine` | engine publish | `Warewolf.Execution.Lightweight.dll` present |
| `$Stage\apps\QueueProcessor` | worker publish | `.exe` present **and 0 `.bite` files** |
| `$Stage\triggers` | queue-trigger `.bite` | every `QueueSourceId` resolves in `$Stage\sources` |
| `$Stage\sources` | RabbitMQ source `.bite` | matched by `ID`, so the Studio filename is fine |
| `$Stage\settings` | `secure.config.cloud.json`, `Deploy-WwExecutionEngine.authconfig.json`, `Warewolf License.secureconfig` | valid JSON; `authconfig` defines every app role you intend to assign |

Two package inputs are **not optional for this runbook**, and both are easy to miss because the
deploy succeeds without them:

- **`-WorkflowsSourcePath`** — the engine publish contains **no `Resources/` folder at all**, so a
  deployed engine serves **zero workflows** and every `/Public/*` and `/Secure/*` call 404s. The
  workflows the triggers invoke must be staged from `$WorkflowsSrc`; the deploy also generates
  `Resources\workflow-index.json` from them for `apis.json` discovery.
- **`-AuthConfigPath`** — each `GroupPermissions` key becomes an assignable Entra app role. If
  `Warewolf_QueueProcessor` is not in this file, Phase C's role grant has nothing to assign.

Confirm the trigger workflows are actually stageable before deploying:

```powershell
Get-ChildItem $TriggerDir -Filter *.bite | ForEach-Object {
    $wf = (Get-Content $_.FullName -Raw | ConvertFrom-Json).WorkflowName
    $path = Join-Path $WorkflowsSrc "$wf.bite"
    [pscustomobject]@{ Trigger = $_.Name; Workflow = $wf; Staged = (Test-Path -LiteralPath $path) }
} | Format-Table -AutoSize
```

Any `Staged = False` is a guaranteed runtime failure: the worker consumes the message, POSTs to a
route that does not exist, receives a non-2xx, and **dead-letters the message and acks it**. The
queue drains and nothing runs.

### Which triggers will be deployed?

Phase C creates **one Container App per trigger file**. Confirm the set, and the values each app
derives from it, *before* deploying:

```powershell
Get-ChildItem $TriggerDir -Filter *.bite | ForEach-Object {
    $t = Get-Content $_.FullName -Raw | ConvertFrom-Json
    [pscustomobject]@{
        File        = $_.Name
        Name        = $t.Name
        Queue       = $t.QueueName
        Workflow    = $t.WorkflowName
        Concurrency = $t.Concurrency      # -> maxReplicas
        Prefetch    = $t.Prefetch         # -> KEDA value (x MaxConcurrency)
        SourceId    = $t.QueueSourceId    # -> must exist in $SourceDir
        SinkId      = $t.QueueSinkId      # -> also staged when it differs
        DeadLetter  = $t.DeadLetterQueue
        Durable     = ($t.Options     | Where-Object Name -eq 'Durable').Value
        DlqDurable  = ($t.DeadLetterOptions | Where-Object Name -eq 'Durable').Value
    }
} | Format-Table -AutoSize
```

Check three things in that table:

1. **`Durable` / `DlqDurable` must match the live queues.** They are read independently — `Options`
   drives the work queue, `DeadLetterOptions` the dead-letter queue. A mismatch is rejected by the
   broker as `PRECONDITION_FAILED — inequivalent arg 'durable'` and the replica cannot consume *at
   all*.
2. **`Concurrency = 0` means DISABLED** (on-prem parity). Such a trigger deploys as `min = max = 0`
   with no scale rule — intentional, not a failure.
3. **`Prefetch` should be 1.** Anything higher parks messages inside one replica, which *delays*
   scale-out and widens the redelivery window on drain — see **§4.2**. The deploy prints an advisory,
   but the fix is in the trigger, so decide it now (§0.1).

### 0.1 Fix the scaling parameters before deploying, not after

`Prefetch` is **not** a deploy parameter — it reaches the worker only through the staged trigger
`.bite`. Changing it after Phase C means editing the trigger and redeploying, so settle it here.
Full reasoning and the `value` formula are in **§4**; the short version:

| Knob | Target | Changed by |
|---|---|---|
| `minReplicas` | **0** | `-ScalingMode Elastic` |
| `MaxConcurrency` | **1** | `-MaxConcurrency` (default) |
| `Prefetch` | **1** | **editing the trigger `.bite`** |
| `maxReplicas` | trigger `Concurrency` | `-MaxReplicas` / manifest |
| `value` | **1**, or `floor(L / T)` for a latency budget `L` | `-TargetQueueLength` / manifest |

List the triggers that need attention:

```powershell
Get-ChildItem $TriggerDir -Filter *.bite | ForEach-Object {
    $t = Get-Content $_.FullName -Raw | ConvertFrom-Json
    $p = [int]($t.Prefetch ?? 1); if ($p -lt 1) { $p = 1 }
    if ($p -gt 1 -and [int]$t.Concurrency -gt 0) {
        [pscustomobject]@{
            Trigger        = $t.Name
            Prefetch       = $p
            MaxReplicas    = $t.Concurrency
            KedaValue      = $p * 1
            ParkedPerReplica = $p - 1
            Action         = "set Prefetch=1 in $($_.Name), or pass -TargetQueueLength 1 as an interim"
        }
    }
} | Format-Table -AutoSize
```

Every row is a trigger whose replicas will **park messages** where the scaler cannot see them
(`protocol=amqp` counts READY messages only) and which are **nacked and redelivered** if that replica
drains. A worked example is in **§4.7**.

> **This no longer costs you parallelism.** `value` used to derive from `Prefetch × MaxConcurrency`,
> which throttled scale-out below saturation — a 5-message burst on a `Concurrency 5, Prefetch 10`
> trigger ran on **one** replica where on-prem's 5 always-running workers would have used **five**.
> `value` now derives from **`MaxConcurrency` alone**, so fan-out matches on-prem at every load level
> and `-TargetQueueLength 1` is no longer needed as a workaround. The old batching behaviour is still
> reachable deliberately via `-TargetQueueLength` (or a per-trigger `targetQueueLength` override) when
> messages are short and cold starts dominate.

Fixing `Prefetch` in the triggers is still worth doing before production — it is now purely about
parked messages and redelivery on drain, not throughput.

---

## 1. Phase A — Confirm what is reused (creates no infrastructure)

Every platform resource this run needs already exists: the resource group, the Key Vault, the
registry, the ACA environment, and its Log Analytics workspace. Phase A therefore **provisions
nothing** — it verifies the five reused resources are fit for purpose and creates a single Key Vault
secret (`rabbitmq-uri`) that KEDA needs. The first billable compute appears in Phase B.

**A1. Confirm the reused resources and your access:**

```powershell
az group show --name $Rg --query properties.provisioningState -o tsv                          # Succeeded
az keyvault secret show --vault-name $Kv --name $KvSecret --query attributes.enabled -o tsv  # true

# Reused registry: must be in $Loc (so the ACR Tasks build and the ACA pull stay in-region) and
# must allow public network access (ACA pulls over the internet, not a private endpoint).
az acr show --name $Acr -g $Rg `
    --query "{loc:location, sku:sku.name, publicNet:publicNetworkAccess, state:provisioningState}" -o json

# Reused environment. Four things must hold:
#   state            Succeeded        — a Failed env accepts app creates then never starts them
#   loc              $Loc             — an app cannot live in a different region to its env
#   vnet             null             — no VNet, so replicas reach the engine and the broker directly
#   workloadProfiles Consumption      — scale-to-zero requires the Consumption profile
az containerapp env show --name $AcaEnv -g $Rg `
    --query "{state:properties.provisioningState, loc:location, vnet:properties.vnetConfiguration,
              profiles:properties.workloadProfiles[].name,
              wsCid:properties.appLogsConfiguration.logAnalyticsConfiguration.customerId}" -o json

# Data-plane role on the vault (control-plane Owner is NOT enough for secrets)
$me = (az ad signed-in-user show --query id -o tsv)
az role assignment list --assignee $me `
    --scope (az keyvault show --name $Kv --query id -o tsv) `
    --query "[].roleDefinitionName" -o tsv        # need Key Vault Secrets Officer (or User to read)
```

**A2. Confirm this run's names are free inside the shared environment and registry.** Container App
names are unique per environment, and `az containerapp create` on an existing name *updates* it —
so a clash silently reconfigures someone else's app instead of failing:

```powershell
# Existing tenants of the shared environment. Expect exactly sharepoint-wiremock and
# exchange-wiremock; none of this run's `wwqp-*` names may already appear.
az containerapp list -g $Rg `
    --query "[?contains(properties.environmentId,'$AcaEnv')].{name:name, min:properties.template.scale.minReplicas, max:properties.template.scale.maxReplicas, cpu:properties.template.containers[0].resources.cpu}" -o table

# The image repository must not collide with one of the 14 already in the reused registry.
az acr repository list --name $Acr -o tsv        # must NOT contain 'warewolf/queueprocessor'
```

**Environment core budget, now shared.** The script prints `Σ (maxReplicas × cpu)` for *its own*
apps only, so add the neighbours by hand:

| | cores at peak |
|---|---|
| `sharepoint-wiremock` (0.5 × 1, `min 1` — always on) | 0.5 |
| `exchange-wiremock` (0.5 × 1, `min 0`) | 0.5 |
| this run: 2 apps × `-MaxReplicas 3` × `-Cpu 0.5` | 3.0 |
| **environment peak** | **4.0** |

Comfortably inside the Consumption quota, and the two `min 0` classes mean the steady-state cost
outside a test burst stays at the 0.5 core `sharepoint-wiremock` already holds.

> ⚠️ **`warewolf` and `warewolf/queueprocessor` are two different repositories.** The reused registry
> already has a `warewolf` repo that this run must not touch; the deploy's default
> `-ImageRepository warewolf/queueprocessor` nests *beside* it, not inside it. The consequence is in
> Phase F: deleting `--repository warewolf` would destroy a shared image. Always pass the full
> `warewolf/queueprocessor` path.

**A3. Nothing to create.** There is no `az containerapp env create`, no `az acr create` and no
`az monitor log-analytics workspace create` in this run. Two notes on why the earlier prerequisites
also fell away:

```powershell
# `az containerapp` is CORE in this CLI version — `az extension list` shows only application-insights
# and authV2, yet `az containerapp env show` works. No `az extension add --name containerapp` needed.
az extension list --query "[].name" -o tsv

# The workspace needs no --logs-workspace-id/-key either: $AcaEnv is ALREADY bound to $Workspace,
# so reusing the environment reuses the workspace transitively.
```

**A4. Confirm the environment is unchanged and record the log target.** Because the environment is
shared, every Phase E log query must filter by `ContainerAppName_s` — the tables also carry the two
wiremock apps' rows:

```powershell
az containerapp env show --name $AcaEnv -g $Rg `
    --query "{state:properties.provisioningState, wsCid:properties.appLogsConfiguration.logAnalyticsConfiguration.customerId, staticIp:properties.staticIp}" -o json
# state Succeeded; wsCid 405a4834-1554-4f2a-b5eb-31789fffaa9b; staticIp is the SHARED egress address
```

> **The environment's outbound IP is shared.** All replicas — this run's and the wiremock apps' —
> egress from `$AcaEnv`'s static IP. Harmless here (the engine authenticates with Easy Auth and the
> dev broker has no allowlist), but if you ever put an IP restriction on the engine, allowlisting
> that address admits every app in the environment, not just this run's.

> **Reusing the registry does not remove the AcrPull grant.** The Container Apps are created with
> `--registry-identity system`, so each app's managed identity needs `AcrPull` on `$Acr`
> regardless of who owns the registry — `Deploy-WwQueueProcessor.ps1` performs that grant. A
> long-lived registry often carries broad grants that *mask* a missing one, which is exactly the
> failure that looks like a bad image build rather than a missing role.

> **Phase F must NOT delete `$AcaEnv`, `$Acr` or `$Workspace`.** All three are shared: the
> environment hosts the two wiremock apps, the registry holds the AKS ingress and connector-test
> images, and the workspace backs the environment itself. Teardown removes only this run's Container
> Apps (by the `wwqp-` prefix) and its single image repository.

> **The AES key is the one deliberate reuse beyond the rule.** `$KvSecret` is a pre-existing secret
> shared with earlier deployments — reused by explicit decision because it is already proven to
> decrypt these exact files. The consequence to keep in mind: rotating it later re-encrypts for
> *those* deployments too, so **Phase F must never delete it**. If isolation matters more than
> convenience on a future run, pass a new `-KeyVaultSecretName` together with `-GenerateNewKey`.

**The one thing to create: the AMQP URI secret for KEDA.** The scale rule authenticates to RabbitMQ
with a connection URI — KEDA cannot use a managed identity against RabbitMQ — so it must exist as a
Key Vault secret that the Container App's identity can read:

```powershell
az keyvault secret set --vault-name $Kv --name 'rabbitmq-uri' --value $RabbitUri -o none

# VERSIONLESS uri on purpose: `--query id` returns a VERSION-PINNED url, which would freeze the
# Container App on the secret version current at deploy time — rotating the broker credential would
# then need a new revision. Versionless lets ACA pick up the new version on its own.
$vaultUri        = (az keyvault show --name $Kv --query properties.vaultUri -o tsv)
$RabbitSecretUri = ($vaultUri.TrimEnd('/')) + '/secrets/rabbitmq-uri'
$RabbitSecretUri     # https://wwexecutionengine.vault.azure.net/secrets/rabbitmq-uri
```

Verify without echoing the credential:

```powershell
az keyvault secret show --vault-name $Kv --name 'rabbitmq-uri' `
    --query "{name:name, enabled:attributes.enabled}" -o json
```

> This secret is **separate** from `$KvSecret`. `$KvSecret` is the AES key that decrypts the staged
> `.bite` files inside the worker; `rabbitmq-uri` is read by the ACA **platform** to evaluate the
> scale rule. Different consumers, different purposes — do not merge them.
>
> `Deploy-WwQueueProcessor.ps1` grants each app's managed identity `Key Vault Secrets User` on this
> vault automatically after creating the app. Without that grant the app deploys, reports Healthy,
> and silently never leaves 0 replicas.

**Only if the engine's own resources are missing** (they are not, at time of writing):
`wwengine1` and `stwwengine1` do not exist yet, and `Deploy-WwExecutionEngine.ps1` **creates both
itself** in its Phase 1 — so there is no need to pre-create them with `az`. Pre-creating is
harmless but redundant.

**Check the environment core quota before Phase C.** Each replica costs CPU; peak demand is
`Σ (maxReplicas × cpu)` across all trigger apps, and ACA rejects the deployment if the environment
cannot satisfy it. The deploy script prints the figure — compare it against:

```powershell
az quota show --scope "/subscriptions/$Sub/providers/Microsoft.App/locations/$Loc" `
             --resource-name ManagedEnvironmentConsumptionCores -o table 2>$null
# If unavailable in your region: az vm list-usage --location $Loc -o table
```

**Verify Phase A:**

```powershell
az group show --name $Rg --query properties.provisioningState -o tsv          # Succeeded
az containerapp env show --name $AcaEnv -g $Rg --query properties.provisioningState -o tsv  # Succeeded
az acr show --name $Acr -g $Rg --query provisioningState -o tsv              # Succeeded
```

---

## 2. Phase B — Execution Engine (`/Public/*` and `/Secure/*`)

**Dry run first** — prints the whole plan and changes nothing:

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
  -DryRun
```

> **`-WorkflowsSourcePath` is not optional here** (§0.0): without it the engine serves no workflows
> and every route in the verification below 404s.
>
> **`-EncryptResources` must run on the machine that created any DPAPI-encrypted `.bite`.** DPAPI is
> Windows-only and machine/user-scoped, so re-encryption to WFAES cannot be done elsewhere. On a
> repeat deploy of already-WFAES sources, drop `-EncryptResources`.

Review the plan, then re-run **without `-DryRun`** to deploy for real. The script provisions the
Function App, applies app settings, runs `Configure-WwExecutionAuth.ps1` for Entra/Easy Auth, and
writes a summary JSON usable by `Rollback-WwExecutionEngine.ps1`.

### Verify the three route families

The engine exposes workflows over three routes with **different** auth models:

> ⚠️ **There is no `/api` prefix.** The engine's `host.json` sets
> `extensions.http.routePrefix: ""`, so the routes are `/Public/…`, `/Secure/…`, `/Services/…`.
> Verified live: `/Public/Hello World.json` → **200**, `/api/Public/Hello World.json` → **404**.

| Route | Auth | Expected |
|---|---|---|
| `/Public/{workflow}.json` | anonymous | **200** with no credentials |
| `/Secure/{workflow}.json` | Entra JWT (Easy Auth) | **401** without a token, **200** with one |
| `/Services/{workflow}.json` | function key | 401 without `x-functions-key` |

#### How workflow inputs bind (all four verified live)

| Form | Binds? |
|---|---|
| `GET ?message=x` (query string) | ✅ |
| `POST {"inputParameters":{"message":"x"}}` | ✅ documented envelope |
| `POST {"message":"x"}` (**flat**) | ✅ — what every on-prem queue worker sends |
| `POST <DataList><message>x</message></DataList>` (**XML**) | ✅ |
| `POST message=x&other=2` (**form-urlencoded**) | ✅ bound as input parameters |
| `multipart/form-data` | ❌ **not supported** — the worker now refuses to start rather than dead-letter |

The flat and XML forms only work because the engine now keeps the caller's **raw body** and hands it
to `ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload` — exactly what
`Dev2.Runtime.WebServer` does with `WebRequestTO.RawRequestPayload`. Previously the body was
deserialised into the `WorkflowExecutionRequest` DTO, so anything that wasn't the envelope was
**silently discarded** (Newtonsoft ignores unknown members): the workflow ran with *no* inputs and
failed on its first variable, e.g. `Scalar value { message } is NULL`. That single difference also
made **XML** bodies and **nested/recordset** inputs unreachable over HTTP, even though the shared
helper supports both.

> ⚠️ **`multipart/form-data` is the one on-prem body form this engine does not bind.** A trigger whose
> single input is `@`-prefixed with `MapEntireMessage` posts multipart (parity with
> `WarewolfWebRequestForwarder.cs:100-108`), and the full server handles it via
> `SubmittedData.ExtractKeyValuePairForPostMethod`.
>
> The worker now **refuses to start** on such a trigger rather than accepting it:
> ```
> Trigger '<name>' maps the entire message to the '@'-prefixed input '@object', which the
> forwarder sends as multipart/form-data. The Warewolf Execution Engine does not bind multipart
> bodies, so every message would be dead-lettered while appearing to process normally.
> ```
> That is deliberate. Silently, the engine returns non-2xx for every message, the worker
> dead-letters **and acks** each one, the queue drains and the app scales back to zero — a total data
> diversion that looks like a clean run. **Fix by renaming the input without the `@` prefix** (it is
> then sent as a JSON body, which binds), or point that trigger at the full Server.
>
> Multipart support was not added to the engine: it needs boundary parsing plus a new dependency, for
> a shape no current trigger uses, whereas `form-urlencoded` was a few lines and is now supported.

> 🔒 **The route is authoritative.** A body- or query-supplied `workflowFilePath` is ignored on any
> route that names its workflow. Previously it won, so a caller authorised for one workflow could
> execute another — authorization is evaluated on the route-derived name *before* the body is parsed.
> The generic `/workflow` route (no route name) still accepts an explicit path by design.

Use a workflow that is **both** staged (§0.0) **and** authorized. `Hello World` qualifies: it sits at
the root of `$WorkflowsSrc` and `secure.config` carries per-workflow rows for it (`Public` and
`Warewolf_ClientApps`). Adjust the path if your `secure.config` differs.

```powershell
$smoke = 'Hello World'          # must be staged AND authorized; check secure.config rows

# 1. Public — must succeed anonymously
Invoke-WebRequest "$EngineUrl/Public/$([uri]::EscapeDataString($smoke)).json?Name=e2e" |
    Select-Object -ExpandProperty Content

# 2. Secure — must be REJECTED without a token
(Invoke-WebRequest "$EngineUrl/Secure/$([uri]::EscapeDataString($smoke)).json?Name=e2e" `
                   -SkipHttpErrorCheck).StatusCode        # expect 401

# 3. Secure — must SUCCEED with an app-only token
$tok = (az account get-access-token --resource "api://$EngineAppId" --query accessToken -o tsv)
Invoke-WebRequest "$EngineUrl/Secure/$([uri]::EscapeDataString($smoke)).json?Name=e2e" `
                  -Headers @{ Authorization = "Bearer $tok" } |
    Select-Object -ExpandProperty Content

# 4. The trigger workflows must resolve too, or Phase E cannot execute anything
foreach ($wf in (Get-ChildItem $TriggerDir -Filter *.bite |
                 ForEach-Object { (Get-Content $_.FullName -Raw | ConvertFrom-Json).WorkflowName })) {
    $route = ($wf -replace '\\','/') -split '/' | ForEach-Object { [uri]::EscapeDataString($_) }
    $url   = "$EngineUrl/Secure/$($route -join '/').json"
    $r     = Invoke-WebRequest $url -Headers @{ Authorization = "Bearer $tok" } -SkipHttpErrorCheck
    "{0,-40} {1}  {2}" -f $wf, $r.StatusCode, ($r.Content -replace '\s+',' ')
}
```

Step 4 is the check that actually predicts Phase E. **Read the body, not the status code** — the
status is 500 for three completely different causes:

| Body contains | Meaning | Fix |
|---|---|---|
| `Workflow file not found: …\Resources\<name>.xml` | **not staged** — the path does not exist in the deployed `Resources` | correct `WorkflowName` in the trigger, or restage `-WorkflowsSourcePath` |
| nested `Error{…}` with no execution detail | **not authorized** (WOLF-8418 wraps the denial) | add the `secure.config` grant — see the scope rule below |
| a workflow-level message, e.g. `Scalar value { x } is NULL` | **staged and authorized; it executed** and failed on its inputs | fix the trigger's input mapping, not the deployment |

That third row is the one that misleads: it looks like a failure but proves the deployment is
correct. Verified live on this run — `/Public/rabbit/RabbitProcess.json` returned
`"Scalar value { message } is NULL"`, i.e. found, authorized, executed.

`$EngineAppId` is the Entra application id printed by `Configure-WwExecutionAuth.ps1` (also in its
output JSON). Capture it now — Phase C needs it:

```powershell
$EngineAppId = '<clientId from Configure-WwExecutionAuth output>'
```

#### Phase B actuals — recorded from the completed run (2026-08-05)

| | |
|---|---|
| Endpoint | `https://wwengine1.azurewebsites.net` |
| Engine app MI principalId | `311105bd-1e0a-44d5-9ee5-2b2e5ab0be08` |
| Entra app **display name** | **`wwengine1-auth`** — *not* `wwengine1`; `az ad app delete` in Phase F must target this |
| `$EngineAppId` (clientId) | `ab0810f7-8caa-46cf-b3d9-c5dd6a1429b4` |
| Audience | `api://ab0810f7-8caa-46cf-b3d9-c5dd6a1429b4` |
| **SP objectId** | `a5db272b-88bd-4697-a6f2-70ac158095f1` — required by the Phase C app-role assignment |
| Run tag | `wwx-test-run=wwx-20260805-040246`, present on all three resources |
| Encryption | 100 resources scanned, 11 encrypted (5 DPAPI→WFAES, 6 plaintext), **11/11 verified decryptable** |

An extra artifact not in the original inventory: **Easy Auth created a client secret** on the app
registration, expiring **2027-08-05**, stored in the app setting
`MICROSOFT_PROVIDER_AUTHENTICATION_SECRET`. It is removed with the app registration in Phase F.

The staged triggers on this run target `rabbit\RabbitProcess` and `rabbit\RabbitProcessFailure`
(**not** the repo sample's `hangfiredemo\Hello World`, which does not exist in `$WorkflowsSrc`).
Both resolve, both map the whole message to an input named `message` — matching what the workflows
expect — and both are authorized through the global `Warewolf_QueueProcessor` `Execute` row.

> **A 500 where you expected 403 is a known behaviour, not a bug** (WOLF-8418). But do not assume
> every 500 is an authorization denial — triage by body using the table above.

> **The authorization scope rule decides whether you need a per-workflow row at all.** A workflow
> uses the **resource** role map *exclusively* when it has at least one **Execute-bearing** resource
> entry; with no such entry it falls back to the **global** (`IsServer=true`) map, and the `Public`
> group is always OR'd in. So a workflow with no resource row is already executable by any group
> holding a global `Execute` — which is how `Warewolf_QueueProcessor` reaches the `rabbit\*`
> workflows on this run without any per-workflow row. Adding a resource row to a workflow that
> previously relied on the global grant **narrows** access; a View-only resource entry does not.

---

## 3. Phase C — QueueProcessor, one Container App per trigger

The script is pointed at triggers in one of three **mutually exclusive** ways:

| Parameter | Result |
|---|---|
| `-TriggerFilePath <file>` | exactly one Container App |
| `-TriggerPath <folder>` `[-TriggerFilter *.bite]` | **one Container App per matching file** |
| `-TriggerManifestPath <json>` | one per manifest entry, with per-trigger overrides |

Add `-TriggerId <guid>` to narrow a folder or manifest to a single trigger (the per-trigger cutover
path). **Zero matches is a hard error**, never a silent no-op.

**Dry run over the whole folder first:**

```powershell
.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup $Rg -Location $Loc `
  -AcaEnvironment $AcaEnv -AcrName $Acr `
  -PublishPath $QpPublish `
  -TriggerPath $TriggerDir `
  -QueueSourcePath $SourceDir `
  -EngineBaseUrl $EngineUrl `
  -EngineResourceAppId $EngineAppId `
  -EngineTenantId $TenantId `
  -KeyVaultName $Kv -KeyVaultSecretName $KvSecret -EncryptStagedSettings `
  -RabbitMqSecretUri $RabbitSecretUri -InlineRabbitMqSecret `
  -ScalingMode Elastic `
  -DryRun
```

> **`-InlineRabbitMqSecret` is required in this tenant.** Without it the secret is wired as a runtime
> `keyvaultref`, and the ACA control plane must fetch it with the app's managed identity on every
> sync. This tenant enforces **Continuous Access Evaluation**, and ACA's sync path cannot answer a
> claims challenge, so it fails repeatedly:
> ```
> 401 AKV10203: Continuous access evaluation check failed. Please extract the claims challenge
> from the www-authenticate header to fetch a new token   (CaeAuthorizationFailed)
> ```
> Observed on **both** apps, tens of retries. The switch copies the URI out of Key Vault at deploy
> time (with the operator's CAE-capable credentials) into a plain Container App secret, so no runtime
> Key Vault fetch exists to fail. **Key Vault stays the source of truth; the cost is that rotating the
> secret now needs a redeploy.**
>
> Note the **worker's own** Key Vault access is unaffected — the Azure SDK inside the container does
> implement the claims-challenge exchange. Visible in App Insights as
> `GET /secrets/… 401` immediately followed by `200`. That asymmetry is why the fix targets only the
> platform-sync path.

**Scaling comes from the triggers — do not pass `-MaxReplicas`.** `value` derives from
`MaxConcurrency` (§4.3a), so `ceil(queueLength / 1)` capped at the trigger's `Concurrency` already
gives real fan-out: the success trigger's `Concurrency = 3` was observed scaling **0 → 3 → 0** on a
5-message batch. An explicit `-MaxReplicas` is flagged as an *exception* in the plan output precisely
because it overrides the authored concurrency contract; reserve it for a deliberate, justified test.

**`-EncryptStagedSettings` applies two different modes**, because the file types differ:

| Staged file | Shape | Mode | Result |
|---|---|---|---|
| `sources\{id}.bite` | XML with `<Source ConnectionString=…>` | attribute | that attribute becomes `WFAES::…` |
| `triggers\{id}.bite` | JSON, no `<Source>` element | **whole file** (`-WholeFile`) | the whole body becomes `WFAES::…` |

Attribute mode alone would **silently skip** the trigger, leaving it — and its stored
`UserName`/`Password` — plaintext in an image layer, and leaving a whole-file DPAPI trigger
unreadable on Linux. Both modes use the **same `$KvSecret` key**, so the worker decrypts either with
one `KEYVAULT__SECRETNAME`. Every staged file is then re-read with `-VerifyOnly` **before** the image
is built, so a file that encrypts but cannot be decrypted fails the deploy instead of the cold start.

> **DPAPI must be converted on the machine that created it.** DPAPI is Windows- and machine-scoped;
> a Linux container can never read it. The staged copy is decrypted here and re-encrypted as WFAES,
> which is why this deploy has to run from that machine. The originals in `$TriggerDir` /
> `$SourceDir` are never modified — the script encrypts a temp copy.

The plan output is the review gate. Confirm:

- **`Triggers resolved : N`** matches the table from §0.
- **`Sources to stage : M`**, listing each source id, its file, and the triggers referencing it.
  Every `QueueSourceId` **and** `QueueSinkId` must appear. A gap aborts the run with
  `Unresolved RabbitMQ source(s); nothing was deployed` — deliberately **before** any Container App
  is created, so a fan-out is all-or-nothing rather than half-deployed.
- **`max=<Concurrency> min=0 value=<MaxConcurrency>`** per app — and that each matches
  what §0.1/§4 said it should be. `min` must be **0** for every app (bar a `Concurrency = 0` trigger,
  which is `min = max = 0`).
- Peak cores against the quota from Phase A.
- Any **prefetch advisory**. Each one names a trigger whose backlog will use fewer replicas than it
  could — decide per §0.1 before continuing, because `Prefetch` cannot be changed by a flag.

Re-run without `-DryRun` to deploy. Per trigger the script:

1. copies **`-PublishPath` to a temp build context** and stages `Settings/triggers/{TriggerId}.bite` +
   `Settings/sources/{sourceId}.bite` into it for **every** trigger, then WFAES-encrypts them —
   your publish output is never mutated;
2. builds the image with `az acr build` **from that context**, so the config is baked in (no volume
   mount, no startup fetch: with `minReplicas = 0` either would be paid on every 0→1 scale);
3. creates the Container App with a system-assigned identity, the `rabbitmq-connection` secret **and
   the env vars in the same call**;
4. adds the scale rule (Phase D);
5. records a summary JSON.

> **The image is built from the PUBLISH OUTPUT, not the repo.** The Dockerfile is a single
> `FROM mcr.microsoft.com/dotnet/aspnet:8.0` stage that does `COPY . .` over the staged context, with
> `ENTRYPOINT ["dotnet", "Warewolf.Execution.QueueProcessor.dll"]`. Consequences worth knowing:
>
> * **A release build from <https://warewolf.io/release-notes> can be imaged directly** — unpack it
>   into `-PublishPath`, no source tree required. It must be a *framework-dependent* publish
>   containing `Warewolf.Execution.QueueProcessor.dll`; the script fails early if that is missing.
> * The `dotnet` muxer entrypoint is deliberate: a **Windows** publish has only the `.exe` apshost, so
>   `./Warewolf.Execution.QueueProcessor` would fail with *no such file or directory*.
> * **A code change needs a republish before deploying** — the image no longer compiles from source.
> * Nothing compiles inside the image, which also removes an entire class of failure: a csproj whose
>   `<Compile Include>` casing differs from the file on disk builds on Windows and fails only on a
>   case-sensitive filesystem. One such mismatch (`Dev2.Common`: `JsonUtils.cs` vs `JSONUtils.cs`) cost
>   a full build cycle to find; six more remain elsewhere in the repo.
> * All triggers go into the **one** shared image; each app selects its own via `QUEUE__TRIGGERID`.

### Grant each app's identity access to the engine

> **Measured on this run: the app-role grant was NOT required.** With no
> `Warewolf_QueueProcessor` app-role assignment on either worker identity, the engine returned
> **`200`** for all 8 `POST /secure/rabbit/RabbitProcess.json` calls (App Insights `dependencies`,
> `success=True`). The reason is the authorization scope rule (§2): these workflows carry no
> Execute-bearing *resource* entry, so they resolve against the **global** map — and the `Public`
> group's server-level `Execute` is always OR'd into the effective permissions, which covers any
> authenticated caller.
>
> Treat the grant as **defence in depth / least privilege**, not a prerequisite. It becomes genuinely
> required the moment a workflow gains an Execute-bearing resource row, because the global grant then
> stops applying to it (§3's narrowing warning). The deploy script's closing note still prompts for
> it deliberately.

The two grants, per app:

```powershell
$apps = (az containerapp list -g $Rg --query "[?starts_with(name,'wwqp-')].name" -o tsv)

foreach ($app in $apps) {
    $mi = (az containerapp show --name $app -g $Rg --query identity.principalId -o tsv)
    Write-Host "$app -> $mi"

    # 1. Entra app role on the engine registration
    az rest --method POST `
      --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$mi/appRoleAssignments" `
      --body (@{
          principalId = $mi
          resourceId  = '<engine service principal object id>'
          appRoleId   = '<Warewolf_QueueProcessor app role id>'
      } | ConvertTo-Json)
}
```

2. **`secure.config` permission for each trigger's `WorkflowName`.** Check what your staged
   `secure.config` already grants `Warewolf_QueueProcessor`:

```powershell
(Get-Content $SecureConfig -Raw | ConvertFrom-Json).WindowsGroupPermissions |
    Where-Object WindowsGroup -eq 'Warewolf_QueueProcessor' |
    Select-Object WindowsGroup, ResourceName, IsServer, View, Execute | Format-Table -AutoSize
```

   - A **global** row (`IsServer = True`) grants server-wide `View`+`Execute`. On this run
     `Warewolf_QueueProcessor` already has one, and **that is sufficient** — the `rabbit\*`
     workflows carry no Execute-bearing resource entry, so they resolve against the global map.
   - **Least privilege** is a **per-workflow** row (`IsServer = false`, `ResourceName` = the
     trigger's `WorkflowName`) for each trigger. Details:
     [Deploy-EndToEnd-Runbook.md](Deploy-EndToEnd-Runbook.md) §8.

   > ⚠️ **Adding a per-workflow row NARROWS access — it is not additive.** Once a workflow has any
   > Execute-bearing resource entry it uses the resource map *exclusively* and stops inheriting the
   > global grant. So a row that names only, say, `Warewolf_ClientApps` would **lock the worker out**
   > of a workflow it could previously execute. If you add one, it must name
   > `Warewolf_QueueProcessor` with `Execute = true`. Do not add rows "to be safe".

   If step 4 of the Phase B checks returned **500** for a trigger workflow, triage by response body
   first (§2) — a missing grant and a missing *file* both return 500, and only one of them is fixed
   here.

> A **roleless** caller is rejected, and per WOLF-8418 that surfaces as **HTTP 500**. The worker
> treats a non-2xx as a business failure: the message is **dead-lettered and acked**, so a missing
> role shows up as messages quietly landing in the dead-letter queue, not as a stuck main queue.
> Check the DLQ first when the main queue drains but nothing executes.

> ### ⚠️ A drained queue is NOT evidence of success
>
> The failure contract is *2xx → ack* and *non-2xx → dead-letter **and** ack*. **Both outcomes drain
> the main queue and both scale the app back to zero**, so `ScaledToZero` tells you only that messages
> were consumed — not that any workflow ran. Distinguish them by:
>
> 1. the `dependencies` query above (`resultCode`), or
> 2. the depth of the trigger's `DeadLetterQueue` — note it is the trigger's own
>    `DeadLetterQueue` field (e.g. `order-success-queue-errors`), **not** the other trigger's queue.
>
> A stuck main queue means the opposite problem: messages nacked and redelivered, which shows up as
> the app pinned at `RunningAtMaxScale` and never draining.

**Verify Phase C:**

```powershell
foreach ($app in $apps) {
    az containerapp revision list --name $app -g $Rg `
        --query "[0].{rev:name, healthy:properties.healthState, active:properties.active}" -o table
    az containerapp logs show --name $app -g $Rg --tail 40
}
```

> ### ⚠️ `healthState: Healthy` at `minReplicas = 0` proves NOTHING about the workload
>
> With scale-to-zero, `Healthy` / `ScaledToZero` describes **provisioning**, not a running process —
> no container has started, so nothing about the image has been exercised. Three separate startup
> failures hid behind a `Healthy` revision on this migration:
>
> | Symptom | Cause |
> |---|---|
> | exit **150** | `runtime:8.0` base image lacks `Microsoft.AspNetCore.App` (required transitively via `Dev2.Common`) |
> | exit **2** | `Settings/` staged, encrypted, then discarded — never added to the build context |
> | `no such file or directory` | Windows publish has no Linux apphost; entrypoint must be `dotnet <dll>` |
>
> **Never report Phase C complete on configuration alone.** Force a replica and read its output.
>
> ### Getting worker logs is harder than it looks
>
> * `az containerapp logs show --type console` is a **live tail**. Lines printed before you attach are
>   never replayed, so a short-lived replica's startup output is unobtainable this way.
> * Replicas that live under ~a minute frequently **never reach Log Analytics** —
>   `ContainerAppConsoleLogs_CL` came back empty for exactly the window being investigated.
> * To read a replica's console at all, pin it first:
>   `az containerapp update -n <app> -g $Rg --min-replicas 1` … then restore `--min-replicas 0`.
>
> **The reliable evidence is App Insights `dependencies`**, because the worker and engine share the
> component and outbound calls are recorded with their result codes:
>
> ```powershell
> $q = "dependencies | where timestamp > ago(1h) | where name has 'RabbitProcess'" +
>      " | summarize calls=count() by resultCode, tostring(success)"
> az monitor app-insights query --app "$EngineApp-ai" -g $Rg --analytics-query $q -o json
> ```
>
> A successful run looks like `resultCode=200  success=True  calls=<n>`. This is the check that
> actually distinguishes success from dead-lettering — see the note below on why draining does not.

In the logs, expect at cold start — in this order:

```
[QueueProcessor-SourceCatalog] Source catalog cached N RabbitMQ source(s) at startup from '/app/Settings/sources' …
[QueueProcessor-ConfigLoader]  loading triggers from '/app/Settings/triggers' (filter '*.bite'); sources from '/app/Settings/sources'
[QueueProcessor-ConfigLoader]  resolved trigger '<Name>' (<id>): queue='…', prefetch=1, durable=…, source=amqp://user@host:port/
[QueueProcessor-Pump]          Consuming queue '<queue>' (prefetch 1, maxConcurrency 1, consumerTag '…')
```

The catalog line **must precede** the loader line — sources are read once at startup and cached, so
nothing re-reads a `.bite` afterwards. Also confirm the log shows **no** password (the connection is
described as `amqp://user@host:port/`), and **no** `ENGINE__TENANTID is not set` warning.

---

## 4. Phase D — Scale configuration: choosing the best parameters

**One Container App per trigger, each with its own `rabbitmq` scale rule, `-ScalingMode Elastic`.**
Phase C already applied this; this section is how to choose the numbers and confirm them.

Scaling behaviour reduces to one formula:

```
desiredReplicas = clamp( ceil(queueLength / value), minReplicas, maxReplicas )
```

### 4.1 The five knobs — and where each is actually set

This matters before tuning anything: **`Prefetch` is not a deploy parameter.** It lives in the
trigger and reaches the worker only through the staged `.bite`, so changing it means editing the
trigger (or its release variable) and redeploying — not passing a flag.

| Knob | Controls | Set where | Default |
|---|---|---|---|
| **`minReplicas`** | Idle floor | `-ScalingMode Elastic` (or `-MinReplicas`) | **0** |
| **`maxReplicas`** | Parallelism ceiling | trigger `Concurrency`; override `-MaxReplicas` | `Concurrency` |
| **`MaxConcurrency`** | In-flight messages **per replica** | `-MaxConcurrency` → `WORKER__MAXCONCURRENCY` | **1** |
| **`Prefetch`** | Messages the broker may hand one replica | **trigger `.bite` only** | 1 (0/blank/invalid coerced to 1) |
| **`value`** | Queue depth per replica before adding another | derived from `MaxConcurrency`; override `-TargetQueueLength` | **`MaxConcurrency`** (Prefetch deliberately excluded — see §4.3a) |

### 4.2 Recommended values, and why

| Knob | Best practice | Reasoning |
|---|---|---|
| `minReplicas` | **0** | Required here: scale in to zero whenever the queue is empty. This is the entire cost argument for the ACA move. Costs a cold start on the first message after idle — accepted deliberately. |
| `MaxConcurrency` | **1** | Dispatch is **serial per channel** (measured: deliveries 2.2 s apart, no overlap). Raising it makes one replica run several workflows at once, which breaks the on-prem `Concurrency` ceiling and invalidates `maxReplicas = Concurrency`. **Scale out, not up.** |
| `Prefetch` | **1** (`= MaxConcurrency`) | A prefetch above the in-flight cap **parks** messages inside one replica. Those messages are unavailable to other replicas, so scale-out is *delayed*, and every parked message is nacked on drain — a wider redelivery window. Warewolf workflows run for seconds, so there is no round-trip cost worth amortising. |
| `maxReplicas` | `min(Concurrency, engine headroom, ACA quota)` | `Concurrency` preserves on-prem parity, but it is an **upper bound, not a target**: `maxReplicas` replicas × 1 workflow each is concurrent load on the engine. Sum across *all* trigger apps and check the engine can absorb it and the ACA environment has the cores. |
| `value` | **1** for lowest latency; raise to trade latency for cost (§4.3) | With `value = 1`, `desiredReplicas = queueLength` (capped) — one replica per queued message, i.e. maximum responsiveness. |

With the recommended set the rule becomes `desiredReplicas = min(queueLength, Concurrency)`: one
replica per waiting message, capped at the on-prem ceiling, and **0 when the queue is empty**.

### 4.3 Choosing `value` — the latency/cost dial

`value` is the one knob with a genuine trade-off. The default is **`MaxConcurrency`** — see §4.3a for
why `Prefetch` is excluded. The two are independent: `Prefetch` is how many messages a replica may
**hold**, `value` is how deep the queue must get before another replica is **added**.

A replica processes one message at a time, so in a latency budget `L` with per-message duration `T`
it clears `L / T` messages:

```
value ≈ max(1, floor(L / T))       L = acceptable wait for the LAST message in a burst
                                   T = median workflow duration (durationMs in the logs)
```

| Situation | `value` | Effect |
|---|---|---|
| Latency-critical (T ≈ L) | **1** | Replica per message. Fastest, most replicas, most cold starts. |
| Balanced, T ≈ 2 s, L ≈ 10 s | **5** | One replica absorbs 5 messages; a 6th adds a replica. |
| Cost-sensitive / bursty batch | `≥ 10` | Few replicas grind the backlog; latency grows with depth. |

Two caveats:

- **Do not raise `value` above `maxReplicas × Prefetch` in the hope of throttling** — it just delays
  scale-out. Cap concurrency with `maxReplicas`, which is what it is for.
- Raising `value` does **not** require raising `Prefetch`. Leave `Prefetch` at 1; a replica holding
  one message while KEDA waits for a deeper queue is exactly the intended shape.

### 4.3a Why `value` excludes `Prefetch` — the on-prem parity rule

The behaviour being replaced is `Concurrency` × `QueueWorker.exe`, each with `BasicQos(Prefetch)`.
Because dispatch is **serial per channel**, `Concurrency` is the number of **concurrent executions**,
and `Prefetch` only buffers.

`value` is the divisor in `ceil(queueLength / value)`, so it must be what a replica can **execute**
(`MaxConcurrency`), not what it can **hold** (`Prefetch × MaxConcurrency`). Dividing by `Prefetch`
starves parallelism *below saturation* — which is where bursty queues live:

Trigger `Concurrency: 10, Prefetch: 3`, **5** ready messages:

| | Replicas | Concurrent executions |
|---|---|---|
| on-prem (10 always-running workers) | — | **5** — five idle workers each take one |
| `value = 3` (old) | `ceil(5/3) = 2` | **2** — slower than today ❌ |
| `value = 1` (current) | `ceil(5/1) = 5` | **5** — parity ✓ |

At saturation the two agree (30 messages → 10 replicas either way), which is why the defect stayed
hidden: it only appears on partial load, and only as *slowness*, never as an error.

`maxReplicas = Concurrency` still caps concurrency at exactly the authored figure, so this raises
parallelism only up to what the trigger already permits — it never exceeds on-prem behaviour.

> Trading parallelism for fewer cold starts is legitimate for short messages — that is what §4.3 is
> for. It is now an explicit choice (`-TargetQueueLength`, or a per-trigger `targetQueueLength`
> override) rather than a silent default.

### 4.4 Scale-to-zero specifics

`minReplicas = 0` only behaves well if the activation threshold is left alone:

- KEDA distinguishes **activation** (`0 → 1`) from **scaling** (`1 → N`). `value` governs the
  latter; `activationValue` governs the former and **defaults to 0**, meaning any queue depth ≥ 1
  wakes the app. **Leave it at the default.** Setting `activationValue = 5` would strand a
  low-volume queue at zero replicas until five messages accumulated — messages would sit unprocessed
  indefinitely. The script does not set it, which is correct.
- `mode=QueueLength` with `protocol=amqp` takes its count from a passive queue declare, which
  reports **ready** messages. In-flight (unacked) messages are therefore expected **not** to hold a
  replica open by themselves. Confirm this in E3 — it is the one scaler semantic not yet measured in
  this environment, and it decides whether a long-running message can be scaled away underneath
  itself (the drain path in E7 is the safety net either way).
- **Polling and cool-down are platform-managed.** ACA runs KEDA with roughly a **30 s polling
  interval** and a **~5 min cool-down** before removing the last replica, and these are not exposed
  as `az containerapp` scale-rule flags. That is why E3 allows 30–60 s to see scale-up, and E6 can
  take several minutes to return to zero. **Neither is a fault** — do not tune around them.

### 4.5 Anti-patterns

| Don't | Why |
|---|---|
| Raise `Prefetch` to "go faster" | Serial dispatch means no extra throughput — only delayed scale-out and more drain nacks |
| Raise `MaxConcurrency` instead of `maxReplicas` | Breaks the parity ceiling and hides failures inside one replica |
| Set `minReplicas ≥ 1` to dodge cold starts | Forfeits the whole cost saving; use `Warm` only with a written justification |
| Set `maxReplicas` far above `Concurrency` | Moves the bottleneck onto the engine, and can exhaust the environment core quota |
| Set `activationValue > 0` | Low-volume queues never wake from zero |
| One Container App consuming several queues | Loses per-queue scaling, per-queue identity and per-queue blast radius |

### 4.6 Per-trigger overrides

When a trigger genuinely must deviate, use `-TriggerManifestPath` rather than global flags, so the
deviation is recorded per trigger and echoed in the plan output:

```json
{
  "triggers": [
    {
      "file": "C:\\ProgramData\\Warewolf\\Triggers\\Queue\\1ac40da8-....bite",
      "maxReplicas": 3,
      "targetQueueLength": 5,
      "maxConcurrency": 1,
      "scalingMode": "Elastic",
      "cpu": "0.5",
      "memory": "1.0Gi",
      "justification": "Engine capped at 3 concurrent for this workflow; 10s latency budget, T~2s"
    }
  ]
}
```

The outer `triggers` array and the `file` key are required — an empty or missing `triggers` array is
rejected with `declares no triggers`. Note there is **no `prefetch` key**: prefetch lives in the
trigger `.bite` (§4.1), and `targetQueueLength` is how you move `value` without touching it.

`-ScalingMode` selects the floor. **`Elastic` is the standard**; anything else is flagged as an
exception in the plan output and needs a justification:

| Mode | `minReplicas` | Use |
|---|---|---|
| **`Elastic`** (default) | **0** | Scale to zero when the queue is empty. **Use this.** |
| `Fixed` | `maxReplicas` | Always-hot. Forfeits scale-to-zero. |
| `Warm` | `1` | One replica resident to avoid cold start on the first message. |

### 4.7 Worked example — a real trigger

`MandateCollectionSuccessTrigger`: `Concurrency = 5`, `Prefetch = 10`, `T ≈ 2 s`.

| | As authored | Recommended | Why |
|---|---|---|---|
| `minReplicas` | 0 | **0** | unchanged |
| `maxReplicas` | 5 | **5** | parity ceiling, engine can absorb 5 |
| `MaxConcurrency` | 1 | **1** | serial dispatch |
| `Prefetch` | **10** | **1** | 9 messages park in one replica: invisible to the scaler, and up to 9 nacked per drain |
| `value` | **1** (derived from `MaxConcurrency`) | **1** (or 5 for a 10 s budget) | one replica per queued message, up to the `maxReplicas = 5` ceiling |

Concretely, 9 queued messages: `ceil(9/1) = 9`, capped at `maxReplicas 5` → **5 replicas, ~4 s to
clear**. Under the old `value = Prefetch × MaxConcurrency = 10` it was `ceil(9/10) = 1` replica and
~18 s — **~4× slower for the same `maxReplicas`**, purely from the divisor. That is why the derivation
changed (§4.3a).

`value` is now correct without any override. Fixing `Prefetch` in the trigger is still worth doing,
but it is now only about parked messages and redelivery on drain — not throughput.

### 4.7a How each Container App gets its name

`$AppNamePrefix` (default `wwqp-`) plus a slug, capped at ACA's 32 characters. The name is derived by
escalating **only as far as uniqueness requires**, so readable names survive:

| | Basis | Example |
|---|---|---|
| 1 | trigger `Name` | `wwqp-ordersuccessqueue` |
| 2 | on collision → `QueueName` | `wwqp-order-success-queue` |
| 3 | still colliding → `QueueName` + `TriggerId` hash | `wwqp-shared-queue-1f4c` |

> **Why step 2 exists.** A trigger's display `Name` is **not unique** — Warewolf names triggers after
> their logical purpose and lets the *queue* distinguish them, so two triggers routinely share one
> `Name`. This is not cosmetic: `az containerapp create` on an existing name **updates** that app, so
> an unresolved collision would leave one queue with **no worker at all**, and the symptom looks like
> a KEDA scaling fault rather than a naming one. Hit for real on this run — both staged triggers were
> named `OrderQueue` and derived `wwqp-orderqueue`.

The script reports every rename (`… collided with another trigger's, so it was derived from the queue
instead`), and still fails loud if two triggers share a `TriggerId`, which no rename can fix.

### 4.8 The rule the script applies

This **is** the KEDA RabbitMQ scaler:

```powershell
az containerapp update --name $app --resource-group $Rg `
  --min-replicas 0 --max-replicas <Concurrency> `
  --scale-rule-name rabbitmq-backlog `
  --scale-rule-type rabbitmq `
  --scale-rule-metadata queueName=<queue> mode=QueueLength value=<MaxConcurrency> protocol=amqp `
  --scale-rule-auth host=rabbitmq-connection
```

**Verify every app:**

```powershell
foreach ($app in $apps) {
    az containerapp show --name $app -g $Rg --query `
      "{app:name, min:properties.template.scale.minReplicas, max:properties.template.scale.maxReplicas, rules:properties.template.scale.rules}" -o json
}
```

Confirm, per app: `min = 0`; `max` = the trigger's `Concurrency` (or a justified override);
exactly **one** rule, `type: rabbitmq`, with `queueName` matching the trigger, `mode=QueueLength`,
`protocol=amqp`, the intended `value`, and **no `activationValue`**. A `Concurrency = 0` trigger
correctly shows `min = max = 0` and **no** rule.

---

## 5. Phase E — End-to-end proof (scale 0 → N and execute)

This is the phase that actually proves the migration. Pick one trigger and its queue:

```powershell
$app   = ($apps -split "`n")[0]
$queue = '<that trigger''s QueueName>'
```

**E1 — establish the zero baseline.** With an empty queue and `Elastic`, replicas must fall to 0:

```powershell
az containerapp replica list --name $app -g $Rg -o tsv | Measure-Object -Line   # expect 0
```

If it never reaches 0, the queue is not empty, or the mode is not `Elastic`.

**E2 — publish messages.** Any publisher works; via an existing Warewolf Server:

```powershell
1..5 | ForEach-Object {
    Invoke-WebRequest "http://localhost:3142/secure/rabbit/RabbitPublish.json?QueueName=$queue" `
        -UseDefaultCredentials -AllowUnencryptedAuthentication -TimeoutSec 30 | Out-Null
}
```

> Verify what your publish workflow actually sends. In our environment `RabbitPublish` ignores the
> `Message` query parameter and always publishes the fixed body `hello` — harmless, but it means the
> query string is not the payload.

**E3 — watch KEDA scale up from zero, and check the count against the formula.** ACA polls at
roughly 30 s, so allow 30–60 s for the first replica:

```powershell
$value = <the rule's value>      # from §4.8 verification
$max   = <the rule's maxReplicas>
1..18 | ForEach-Object {
    $n = (az containerapp replica list --name $app -g $Rg --query "length(@)" -o tsv)
    "{0}  replicas={1}" -f (Get-Date -Format HH:mm:ss), $n
    Start-Sleep -Seconds 10
}
```

Two things to confirm, not one:

1. **Activation** — the count leaves 0 after publishing. This proves `activationValue` is at its
   default, so a single message wakes the app (§4.4).
2. **Scaling** — the peak count matches `min(ceil(messagesPublished / $value), $max)`. With the
   recommended `value = 1` and 5 messages, expect `min(5, max)`. **A peak of 1 replica for a
   multi-message backlog is the `value`-too-high symptom from §4.7**, not a broken scaler — the
   messages still all process, just sequentially on one replica.

Record the peak; it is the evidence that the chosen `value` behaves as intended.

**E3a — measure `T`, so `value` can be chosen deliberately.** After E4, take the median `durationMs`
from the logs. That is `T` in the `value ≈ floor(L / T)` formula (§4.3); until it is measured, any
`value` above 1 is guesswork.

**E3b — does `QueueLength` count unacked messages?** The one unmeasured scaler semantic (§4.4). With
a single long-running message in flight and an otherwise empty queue, check whether the replica count
falls toward 0 while it is still executing:

```powershell
az containerapp replica list --name $app -g $Rg --query "length(@)" -o tsv
```

If it drops to 0 mid-execution, `QueueLength` counts **ready only** and the drain path (E7) is what
protects in-flight work — so `-ShutdownGraceSeconds` must comfortably exceed the longest workflow.
If it stays at 1, unacked messages hold the replica open. Record which; it decides how tight the
drain budget can safely be.

**E4 — confirm execution.**

```powershell
az containerapp logs show --name $app -g $Rg --tail 100 --follow:$false |
    Select-String 'Queue execution (starting|succeeded|failed)'
```

One `Queue execution succeeded` per message, each carrying `txn=` and `durationMs=`.

**E5 — confirm the queue drained and nothing was dead-lettered.** Check `<queue>` is at 0 and the
dead-letter queue has not grown. Messages in the DLQ mean the engine returned non-2xx — start with
the app-role grant in Phase C.

**E6 — watch it scale back to zero.** Once the queue is empty, replicas return to 0 after the
platform cool-down — allow **up to ~5 minutes** (§4.4); it is not exposed as a flag, so a slow
scale-in is not a fault:

```powershell
1..36 | ForEach-Object {
    $n = (az containerapp replica list --name $app -g $Rg --query "length(@)" -o tsv)
    "{0}  replicas={1}" -f (Get-Date -Format HH:mm:ss), $n
    if ($n -eq '0') { "reached zero"; break }
    Start-Sleep -Seconds 10
}
```

Reaching **0** is the cost saving that justifies the whole migration and confirms
`minReplicas = 0` is in effect. If it never reaches 0: the queue is not actually empty, or the mode
is not `Elastic`.

**E7 — graceful drain (optional but recommended before production).** Force a scale-in during
processing and confirm no message is lost or double-executed:

```powershell
az containerapp revision restart --name $app -g $Rg --revision <active-revision>
az containerapp logs show --name $app -g $Rg --tail 60 | Select-String 'Cancelled consumer|Drain of|still in flight'
```

`Drain of '<queue>' completed cleanly` is the good outcome. `Drain window … elapsed with N
message(s) still in flight` means those messages will be **redelivered and may run twice** — raise
`-ShutdownGraceSeconds` or lower `-EngineTimeoutSeconds`, keeping
`EngineTimeout ≤ ShutdownGrace < TerminationGracePeriod`.

### Phase E acceptance

| # | Criterion | Step |
|---|---|---|
| 1 | `/Public/*` returns 200 anonymously | B |
| 2 | `/Secure/*` returns 401 without a token and 200 with one | B |
| 3 | Every QueueProcessor revision is `Healthy`, with the catalog + pump lines logged and no tenant warning | C |
| 4 | **One app per trigger**, each with exactly one `rabbitmq` rule | D/§4.8 |
| 5 | Every app has `minReplicas = 0`; `maxReplicas` and `value` match the choices from §0.1/§4 | D/§4.8 |
| 6 | No `activationValue` is set (a single message must wake the app) | D/§4.8 |
| 7 | Replicas sit at **0** on an empty queue | E1 |
| 8 | Publishing raises replicas above 0 **without manual intervention** | E3 |
| 9 | **Peak replicas match `min(ceil(messages / value), maxReplicas)`** — the chosen `value` behaves as designed | E3 |
| 10 | `T` (median `durationMs`) recorded, so `value` is chosen from measurement not guesswork | E3a |
| 11 | Whether `QueueLength` counts unacked messages is recorded | E3b |
| 12 | One `Queue execution succeeded` per message | E4 |
| 13 | Work queue drains to 0; dead-letter queue does not grow | E5 |
| 14 | Replicas return to **0** within the cool-down | E6 |
| 15 | A forced restart drains cleanly (no stranded messages) | E7 |

Criteria **4–6, 9** are the ones this runbook exists to prove beyond "it deployed": that each trigger
scales independently, from zero, by the parameters you chose deliberately.

---

## 6. Phase F — Teardown (surgical only)

> ## ⛔ NEVER run `az group delete` on this resource group
>
> `DEV2` is **shared**. It holds, among other things, an AKS cluster (`tudev2-kubernetes`), the
> live `WarewolfServer`, `WarewolfServer-UAT` and `WarewolfServerExecution`, the Key Vault this
> deploy depends on (`WWExecutionEngine`), the ACA environment (`dev2-cae`) and the registry
> (`tudev2containerregistry`). Deleting the group would destroy all of it.
>
> Teardown removes **only what this run created**, by name and by run tag.

```powershell
# 1. Container Apps created by this run (one per trigger, all prefixed wwqp-)
az containerapp list -g $Rg --query "[?starts_with(name,'wwqp-')].name" -o tsv |
    ForEach-Object { az containerapp delete --name $_ --resource-group $Rg --yes }

# 2. The engine + storage created by this run — tag/summary driven, never by group
.\Rollback-WwExecutionEngine.ps1 -SummaryPath '<the summary json from Phase B>' -DryRun   # review
.\Rollback-WwExecutionEngine.ps1 -SummaryPath '<the summary json from Phase B>'           # execute
```

`Rollback-WwExecutionEngine.ps1` deletes only resources carrying this run's `wwx-test-run` tag, in
dependency order, and performs a leak check — pre-existing resources are preserved by design.

```powershell
# 3. ⛔ Do NOT run `az containerapp env delete --name $AcaEnv` — the environment is REUSED and still
#    hosts `sharepoint-wiremock` and `exchange-wiremock`. Step 1 above already removed this run's
#    apps from it, which is the whole of the environment-level cleanup.
az containerapp list -g $Rg --query "[?contains(properties.environmentId,'$AcaEnv')].name" -o tsv
#    ^ must print exactly sharepoint-wiremock and exchange-wiremock — no `wwqp-*` left behind

# ⛔ Do NOT run `az acr delete --name $Acr` — that registry holds the AKS ingress images and every
#    connector-test image. Remove only this run's repository:
az acr repository list --name $Acr -o tsv                       # confirm the repo name first
az acr repository delete --name $Acr --repository 'warewolf/queueprocessor' --yes
# ⛔ NOT `--repository warewolf` — that is a different, pre-existing, shared repository.

# ⛔ Do NOT delete $Workspace — $AcaEnv still logs to it. This run's rows age out on their own with
#    the workspace's 30-day retention.

# 4. App Insights created by the engine deploy. Its `managed-wwengine1-ai-ws` workspace is a hidden
#    resource owned by the component and is removed with it — no separate delete needed.
az monitor app-insights component delete --app wwengine1-ai -g $Rg 2>$null
```

**Deliberately left in place:** `DEV2`, `WWExecutionEngine` **and all its secrets** — including
`WWExecutionEngineTestSecret`, which other deployments depend on — plus the three reused shared
resources `dev2-cae`, `tudev2containerregistry` and `workspace-2028k`, and the two neighbouring apps
`sharepoint-wiremock` / `exchange-wiremock`. Also untouched: `tudev2-kubernetes`,
`law-wwenginecaller103`, and every `WarewolfServer*`.

> `rabbitmq-uri` was created by this run, so it *may* be deleted — but it is harmless to keep and
> re-usable next run. `WWExecutionEngineTestSecret` must **never** be deleted.

**Two things live outside the resource group** and survive any resource-level teardown:

```powershell
# a) The Entra app registration (a DIRECTORY object)
az ad app list --display-name $EngineApp --query "[].{name:displayName, id:appId}" -o table
az ad app delete --id $EngineAppId

# b) The role assignments granted to each Container App's managed identity.
#    Deleting the app removes the identity, which leaves the assignments orphaned rather than
#    dangerous — but clean them up so the vault's access list stays readable.
az role assignment list --scope $(az keyvault show --name WWExecutionEngine --query id -o tsv) `
    --query "[?principalType=='ServicePrincipal'].{principal:principalId, role:roleDefinitionName}" -o table
```

Verify afterwards that the shared resources are untouched:

```powershell
az resource list -g $Rg --query "[?starts_with(name,'wwqp-') || name=='$EngineApp' || name=='$EngineStorage'].name" -o tsv
# ^ should be EMPTY after teardown
az containerapp env show --name dev2-cae -g $Rg --query properties.provisioningState -o tsv   # Succeeded
az keyvault show --name WWExecutionEngine --query name -o tsv                                # present
```

---

## 7. Troubleshooting — failures actually seen in this stack

| Symptom | Cause | Fix |
|---|---|---|
| `PRECONDITION_FAILED — inequivalent arg 'durable'`; replica cannot consume | Trigger's `Durable` disagrees with the live queue | Match `Options[Durable]` / `DeadLetterOptions[Durable]` to the queues (§0 item 1) |
| `AzureCliCredential/ManagedIdentityCredential … Invalid tenant id provided` | Blank tenant with a non-system-assigned identity | Pass `-EngineTenantId`; blank is valid **only** for a system-assigned MI |
| `No trigger files matching '*.bite'` | Wrong `-TriggerPath`/`-TriggerFilter`, or `QUEUE__TRIGGERSSUBPATH` overridden | Triggers live in `Settings/triggers/`; sources in `Settings/sources/` |
| `The … source '<guid>' is not staged` at cold start | Source missing from the image | Ensure it is in `-QueueSourcePath`; the error lists folders searched **and** ids present |
| `Unresolved RabbitMQ source(s); nothing was deployed` | Plan-time guard fired | Working as designed — add the source, nothing was created |
| `/Secure` returns **500** with a valid token | Three different causes — see the 500 triage table in Phase B | Read the response **body** first; only the nested `Error{…}` shape is an authorization denial |
| Any route returns **404** | The `/api` prefix was used | `host.json` sets `routePrefix: ""` — drop `/api` |
| Workflow runs but every input is null (`Scalar value { x } is NULL`) | Body form the engine cannot bind — most likely `multipart`/`form-urlencoded` | Use the query string or a JSON body (§2 binding table) |
| Container exits **150**, CrashLoopBackOff | Base image lacks `Microsoft.AspNetCore.App` | Build `FROM …/aspnet:8.0`, not `…/runtime:8.0` |
| Container exits **2**, `No trigger files matching '*.bite'` | `Settings/` never reached the image | Confirm the plan prints `Build context staged: N trigger(s)` |
| Container fails `no such file or directory` on the entrypoint | Windows publish has no Linux apphost | Entrypoint must be `dotnet <dll>` |
| `OptionsValidationException … 'Engine:ResourceAppId is required'` on revision 1 only | env vars applied after create | Fixed — `create` now passes `--env-vars`; a *persisting* error means a genuinely missing value |
| Deploy dies mid-flight with `ConnectionResetError(10054)` | Transient Azure transport fault | Fixed — `Invoke-Az` retries transport errors 4× with backoff; re-run to repair a half-configured app |
| `az acr build` fails `[Errno 13] Permission denied … .vs\…vsidx` | `.dockerignore` not matching | Patterns need **no trailing slash** and **both** bare and `**/` forms; verify with `az acr build --debug` |
| KEDA never scales, secret sync logs `AKV10203 … CaeAuthorizationFailed` | Tenant enforces CAE; ACA sync cannot answer the claims challenge | Deploy with `-InlineRabbitMqSecret` (§3) |
| Main queue drains but no workflow runs; DLQ grows | Engine returns non-2xx; worker dead-letters **and acks** | Almost always the missing app role or `secure.config` row |
| Replicas never leave 0 | No scale rule, `Concurrency = 0`, `activationValue` set above the queue depth, or the `rabbitmq-connection` secret cannot authenticate | Check §4.8 output; confirm the Key Vault reference resolves |
| Replicas never return to 0 | Mode is not `Elastic`, or the queue is not empty | `-ScalingMode Elastic` gives `minReplicas 0`. Allow ~5 min cool-down (§4.4) |
| Scale-in looks "stuck" for minutes | ACA's cool-down is ~5 min and is **not** configurable | Expected; wait it out (§4.4) |
| Scale-up takes ~30 s to react | ACA's KEDA polling interval is ~30 s and is **not** configurable | Expected (§4.4) |
| `Drain window … still in flight` | Engine call outlived the drain budget | Keep `EngineTimeout ≤ ShutdownGrace < TerminationGracePeriod` |
| **Only 1 replica for a large backlog** | `value` too high — `ceil(queueLength / value)` rounds to 1 | Lower `value`: `-TargetQueueLength 1`, and set trigger `Prefetch = 1` (§4.7) |
| Many replicas each doing one quick message; cold starts dominate | `value` too low for the workload | Raise `value` to `floor(L / T)` using the `T` measured in E3a (§4.3) |
| Messages redelivered after a scale-in, workflows run twice | Parked (prefetched) messages nacked on drain | `Prefetch = 1` minimises the window (§4.2); raise `-ShutdownGraceSeconds` |
| ACA rejects the deployment on cores | Environment quota below `Σ (maxReplicas × cpu)` | The script's printed peak counts **only its own apps**. `dev2-cae` is shared — add the wiremock apps' 1.0 core (§A2 table) before comparing |
| `az containerapp create` fails naming a workload profile | `dev2-cae` is a workload-profiles environment whose only profile is `Consumption` | Pass `--workload-profile-name Consumption` (ACA normally defaults to it, so this should not fire) |
| A `wwqp-*` app already exists in `dev2-cae` | `az containerapp create` on an existing name **updates** rather than fails | Re-run §A2's app list; never reuse a neighbour's name |
| `Derived Container App name collision that disambiguation could not resolve` | Two triggers share a **`TriggerId`** — the only field a rename cannot disambiguate | Fix the duplicated `TriggerId` in the trigger files (§4.7a) |
| Deploy prints success but the shell reports **exit 1** | Fixed — `-AllowFail` probes used to leak a non-zero `$LASTEXITCODE`; the script now ends `exit 0` | If it recurs, check for a probing `az` read added after the last `exit 0` |

---

## 8. Approval inventory — every command that changes state

Nothing below has been run. Phases A–D create billable resources and modify your Entra directory;
Phase F deletes.

| Phase | Command | Effect |
|---|---|---|
| A | `az keyvault secret set --name rabbitmq-uri` | One new secret in the reused vault — **the only change Phase A makes**. No environment, registry or workspace is created |
| B | `Deploy-WwExecutionEngine.ps1` (no `-DryRun`) | Creates `stwwengine1` + `wwengine1`, app settings, **an Entra app registration**, and WFAES-encrypts the staged workflow resources |
| C | `Deploy-WwQueueProcessor.ps1` (no `-DryRun`) | `az acr build` — pushes **one new repository into the reused `tudev2containerregistry`** — + **one Container App per trigger (2), into the shared `dev2-cae`**; WFAES-encrypts staged triggers/sources; **grants each app MI `AcrPull` on the registry and `Key Vault Secrets User` on the vault** (two RBAC writes) |
| C | `az rest … appRoleAssignments` | **Directory change** — grants each MI the engine app role `Warewolf_QueueProcessor` |
| D | `az containerapp update --scale-rule-*` | Applies the KEDA rule (already done by Phase C) |
| E | `RabbitPublish` / `containerapp revision restart` | Publishes real messages to the dev broker; restarts a revision |
| F | `az containerapp delete` (×2), `Rollback-WwExecutionEngine.ps1`, `az ad app delete` | **Targeted deletes only.** ⛔ **No `az group delete`** — see Phase F |

| F | `az acr repository delete`, `az monitor app-insights component delete` | Removes **only this run's image repository** (⛔ never `az acr delete`) and App Insights. ⛔ **No `az containerapp env delete`** — `dev2-cae`, `tudev2containerregistry` and `workspace-2028k` all survive |

**Reused, never created or deleted:** `DEV2`, `WWExecutionEngine` (including
`WWExecutionEngineTestSecret`, which other deployments depend on), `dev2-cae` (this run adds two
Container Apps to it and removes only those), `tudev2containerregistry` (one new repository pushed
into it, removed in Phase F), and `workspace-2028k` (receives this run's logs; rows age out on the
30-day retention).

**Untouched by this run:** `sharepoint-wiremock`, `exchange-wiremock`, `law-wwenginecaller103`,
`tudev2-kubernetes`, `WarewolfServer*`.

Read-only throughout (safe to run unattended): `az account show`, `az * show`, `az * list`,
`az containerapp logs show`, `az role assignment list`, `az quota show`, and any script invoked with
**`-DryRun`**. Note `Encrypt-Config.ps1` **reads** the Key Vault secret (data-plane read) even in a
dry run of the deploy — it never writes to Key Vault unless `-GenerateKeys` is passed, which this
runbook never does.
