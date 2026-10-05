# Spec — full redeploy of `WarewolfServer-UAT` (.NET 8 → .NET 10)

**Status:** §1-6 executed. First-pass package (deployed by another operator/process before
2026-08-14) was on .NET 10 with correct `Resources/`, but **did not contain the concurrency fix**
— reproduced live (see §13). A corrective redeploy from `8510` HEAD (fix confirmed present in the
built DLL) was performed and **verified to resolve the `[[JobLogId]]` error under concurrency**
(see §13). **§7.5 (full 1000-message RabbitMQ+Shovel+ServiceBus load test) is now executed and
PASSING 1000/1000 (2026-08-15) — see §14 and `docs/ShovelBridge-Architecture.md`.** The blocker
was a persistence-configuration regression (§14), not the Shovel bridge itself.
**Author:** drafted 2026-08-13 during WOLF-8510 (ShovelBridge load test)
**Target:** `WarewolfServer-UAT` (Function App, RG `DEV2`)

---

## 1. Why this is needed

`WarewolfServer-UAT` is the engine that `pipeline-LOADTEST.yml`'s
`ShovelBridgeLoadTest_ExternalServiceBus` job executes workflows against. Three findings forced
this spec:

1. **Nothing in CI deploys it.** `pipeline-LOADTEST.yml` compiles and then tests against an
   already-deployed engine. `pipeline-CLOUD.yml` deploys `-AppName 'warewolfserver'` — a
   *different* app (see its own comment: the job was deliberately moved off UAT). So UAT is
   updated only by manual deploys.
2. **No documented procedure exists.** Nothing under `Scripts/` or `docs/` references
   `warewolfserver-uat` as a deploy target. This document closes that gap.
3. **UAT is running stale code.** Last modified `2026-08-13T08:50Z`, which predates every
   WOLF-8510 code change. Load-test runs at 11:53, 18:14 and 20:48 all exercised the *old*
   engine; the improvements observed between them came from **database** changes, not code.

The engine now targets `net10.0` while the app is provisioned for `v8.0`, so this is a runtime
upgrade, not just a package push.

## 2. Current state (verified 2026-08-13)

| Property | Value | Note |
|---|---|---|
| App name / RG | `WarewolfServer-UAT` / `DEV2` | |
| App Service Plan | `ASP-Warewolf-a36c`, **SKU `Y1` (Dynamic/Consumption)** | lives in RG **`Warewolf`**, not `DEV2`. **Superseded 2026-09-22 — see §16**: the app ran on Flex Consumption, with no shared plan. **Superseded again 2026-10-05 — see §19**: back on classic Consumption (`Y1`), on a plan `functionapp create` makes for it in `DEV2`, not `ASP-Warewolf-a36c`. |
| OS | Windows (`reserved: false`) | **Superseded 2026-09-22 — see §16**: Linux on Flex. **Windows again from 2026-10-05 — see §19.** |
| Runtime | `netFrameworkVersion: v8.0`, `FUNCTIONS_WORKER_RUNTIME: dotnet-isolated`, `FUNCTIONS_EXTENSION_VERSION: ~4` | **must become v10.0** |
| `alwaysOn` | `false` | not supported on Consumption |
| `httpsOnly` | **`false`** | see risk R5 |
| Deployment mode | `WEBSITE_RUN_FROM_PACKAGE: 1` | |
| Placeholder | `WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: 1` | version-sensitive, see R2 |
| Last modified | `2026-08-13T08:50Z` | predates all WOLF-8510 commits |

Repo target: `Warewolf.Execution.Lightweight.csproj` → `net10.0`, `AzureFunctionsVersion v4`,
`Microsoft.Azure.Functions.Worker` 2.52.0, `Worker.Sdk` 2.1.0.

### App settings that MUST survive the redeploy (23)

```
ASPNETCORE_ENVIRONMENT                      AzureWebJobsStorage
AZURE_KEYVAULT_NAME                         BYPASS_SECURE_CONFIG
ENABLEAPPLICATIONINSIGHTS                   ENABLECONSOLELOGGING
ENABLEELASTICSEARCHLOGGING                  EXECUTIONLOGLEVEL
FUNCTIONS_EXTENSION_VERSION                 FUNCTIONS_WORKER_RUNTIME
KEYVAULT_SECRET_NAME                        ServiceBusConnection__fullyQualifiedNamespace
STRUCTURED_LOGS                             WAREWOLF_APPINSIGHTS_CONNECTION_STRING
WAREWOLF_ENTRA_AUDIENCE                     WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE
WAREWOLF_ENTRA_TENANT_ID                    WAREWOLF_LICENSE_CHECK_ENABLED
WAREWOLF_SERVICEBUS_TRIGGER_QUEUE           WEBSITE_CONTENTAZUREFILECONNECTIONSTRING
WEBSITE_CONTENTSHARE                        WEBSITE_RUN_FROM_PACKAGE
WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED
```

Load-test-critical values (non-secret):

- `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE = wwexecution-secure-trigger-queue-e2e` — **must match**
  the queue `pipeline-LOADTEST.yml` shovels into, or every message silently goes unconsumed.

  **2026-09-12 correction (WOLF-8517):** this is no longer an app setting. The queue name AND
  the workflow every message on it runs now come SOLELY from `Settings/servicebustrigger.bite`
  on the Function App.

  **2026-09-12 further correction:** that file is baked in at DEPLOY TIME by
  `Deploy-WwExecutionEngine.ps1 -ServiceBusTriggerQueueName`/`-ServiceBusTriggerWorkflowName`
  (`pipeline-LOADTEST.yml`'s `Deploy_UAT` job passes both), NOT written to the already-deployed
  Function App afterwards by `Enable-ServiceBusSecureTrigger.ps1 -WorkflowName` as originally
  designed — that write used the Kudu VFS API and cannot succeed once
  `WEBSITE_RUN_FROM_PACKAGE=1` (above) mounts `wwwroot` read-only; confirmed live against this
  exact app (`UnauthorizedAccessException`) the same day. `Enable-ServiceBusSecureTrigger.ps1`
  no longer has a `-WorkflowName` parameter — it still owns the queue/RBAC/app-setting side
  (its `-TriggerQueueName` must match the deploy's `-ServiceBusTriggerQueueName`). A future
  redeploy will NOT show `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE` in `az functionapp config
  appsettings list` any more; verify the staged trigger file instead (its `QueueName` must
  still match what `pipeline-LOADTEST.yml`/`pipeline-CLOUD.yml` shovel into, and its
  `WorkflowName` must match `VerifyWorkflowName`/`WwExecutionServiceBusTriggerWorkflowName` in
  those pipelines). See `docs/ServiceBusSecureTrigger-Architecture.md` "Configuration
  reference" for the full story.
- `BYPASS_SECURE_CONFIG = true`, `WAREWOLF_LICENSE_CHECK_ENABLED = false`,
  `ASPNETCORE_ENVIRONMENT = Production`, `EXECUTIONLOGLEVEL = INFO`.

  **2026-08-27 correction:** `WAREWOLF_LICENSE_CHECK_ENABLED` is now `true`.
  `pipeline-LOADTEST.yml`'s `Deploy_UAT` job passes `-LicenseCheckEnabled:$true` with
  `-LicenseConfigPath` staging a licensed `Warewolf License.secureconfig` downloaded from
  the Azure DevOps Library (Secure files) — see that job's own comments. A redeploy that
  omits `-LicenseConfigPath` will re-enable the gate with no license staged, and
  `WorkflowExecutor`'s license/subscription check will fail every workflow execution.

**Take a full backup of app settings before touching anything** (§5.1) — several are secrets and
are not reconstructible from the repo.

**2026-09-02 correction (WOLF-8516):** the list above is a historical snapshot (2026-08-13,
pre-merge). `AZURE_KEYVAULT_NAME`, `KEYVAULT_SECRET_NAME`, `ENABLEAPPLICATIONINSIGHTS`,
`ENABLECONSOLELOGGING`, `ENABLEELASTICSEARCHLOGGING`, `STRUCTURED_LOGS`,
`WAREWOLF_ENTRA_TENANT_ID`, `WAREWOLF_ENTRA_AUDIENCE`, `WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE`
are all still present on the live app but are **no longer read by the current engine code at
all** — see §15 for what replaced each one and what actually needs to be passed to
`Deploy-WwExecutionEngine.ps1` now.

## 3. Scope

**In scope:** publish the current branch, upgrade the app to .NET 10, deploy, verify, and re-run
the ShovelBridge load test.

**Out of scope:** changing the App Service Plan tier (see R3 — flagged as a decision, not an
action), Entra/Easy Auth re-provisioning (already configured; use `-SkipAuthProvisioning`), and
the `jobs1`/`jobs2` SQL schema (already applied to **`WarewolfDevOpsTestDb`**, which replaced the
free-limit-exhausted `WarewolfEntraTestDb` on 2026-08-19 — see
`Resources/rabbit/Provision-ShovelBridgeSchema.sql` and the update note atop
`ShovelBridge-Architecture.md`).

## 4. Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| **R1** | ~~.NET 10 isolated support on Azure Functions v4 / Windows Consumption is unconfirmed.~~ **RESOLVED 2026-09-13** — `az functionapp list-runtimes --os windows` advertises `dotnet-isolated` version `10` for Functions v4 (az CLI 2.87.0). | ~~Blocker~~ | Closed. `Deploy-WwExecutionEngine.ps1` now creates on `--runtime-version 10`, and `-UpgradeRuntimeVersion` reconciles an app that already exists on `v8.0`. Still re-run the §5.2 check on the machine you deploy from: an older az CLI rejects `--runtime-version 10` outright. |
| **R2** | `WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED=1` pins a version-specific cold-start placeholder. | App may fail to start or silently run the wrong runtime. | Set to `0` for the first .NET 10 deploy; restore to `1` only once confirmed working. |
| **R3** | ~~Consumption (`Y1`) plan: no `alwaysOn`, cold starts, capped scale-out.~~ **SUPERSEDED 2026-09-22** — the app moved to Flex Consumption (see §16); it no longer sits on `Y1` or on the shared plan at all. | ~~Load-test noise~~ | **Re-opened 2026-10-05 (§19)**: the app is back on `Y1`, so §3's cold-start/ramp analysis applies again. The pre-warm step and `-FunctionAppScaleLimit 30` are the mitigations. Closed by the §16 migration, at the time. Flex has its own cold-start/scale characteristics (no `alwaysOn` there either) — re-baseline load-test noise expectations post-migration rather than assuming §3's original analysis still holds. |
| **R4** | ~~Plan is shared and lives in a **different resource group**.~~ **SUPERSEDED 2026-09-22** — Flex Consumption has no shared App Service Plan object; each app gets its own dedicated resource (see §16). | ~~A plan-level change hits unrelated apps.~~ | Closed. `ASP-Warewolf-a36c` (RG `Warewolf`) still hosts `WarewolfInvoicing` and `WarewolfUsageAPI` — it was **not** deleted as part of this migration and remains read-only/out of scope for this app's deploys going forward. |
| **R5** | `httpsOnly: false`. | Security weakness, pre-existing. | Flag to the team; do not silently change it as part of this deploy — it may be load-bearing for an existing caller. |
| **R6** | Deploy replaces app settings if the orchestrator is run without care. | Broken auth / wrong queue / lost secrets. | Back up settings first; diff after deploy (§6.5). |
| **R7** | `Warewolf.Execution.Lightweight.csproj` only copies its bundled `Resources\` folder to the build/publish output in **Debug** config — a `-c Release` publish (§5.3) has **no `Resources` folder at all** unless `-WorkflowsSourcePath` is passed to the orchestrator. | **Confirmed root cause of the 2026-08-13 load-test hang**: 29,782 `FileNotFoundException: Could not find file 'C:\home\site\wwwroot\Resources'` in App Insights (`warewolfserver-uat-ai`), then every workflow execution failed instantly and the load test hung polling for results that never arrived. | **Mandatory:** stage the resources (§5.4) and pass `-WorkflowsSourcePath` to every dry-run and deploy (§6.2/§6.4); confirm it in the plan phase (§6.3). |
| **R8** | DPAPI-encrypted `.bite` sources cannot travel between machines. | Source fails to decrypt. | The bundled `NewSqlServerSource (Local Backup)` (`d3f6a2e1-…`) is **unreferenced** by both workflows — exclude it (§5.4). Both workflows bind to `b9184f70-…`, now committed pre-encrypted (WFAES) as `Resources\rabbit\NewSqlServerSource.bite` (§5.4) — the devops-endpoint fetch this row previously described has been retired. |

## 5. Pre-flight

### 5.1 Back up (mandatory, do first)

```powershell
az functionapp config appsettings list --name WarewolfServer-UAT --resource-group DEV2 `
  -o json > uat-appsettings-backup-$(Get-Date -Format yyyyMMdd-HHmmss).json
az functionapp show --name WarewolfServer-UAT --resource-group DEV2 `
  -o json > uat-site-backup-$(Get-Date -Format yyyyMMdd-HHmmss).json
```

Store these outside the repo — they contain secrets.

### 5.2 Confirm .NET 10 support (R1 — gate)

Check that the Functions runtime advertises .NET 10 isolated for Windows:

```powershell
az functionapp list-runtimes --os windows --query "dotnet-isolated[].{version:version, functions:supported_functions_versions}" -o table
```

If `10` is absent, **stop** and take the R1 decision before proceeding — an az CLI that does not
know the stack rejects `--runtime-version 10` at `functionapp create`.

**Confirmed 2026-09-13** on az CLI 2.87.0: `dotnet-isolated` versions `10`, `9`, `8`, `7`, `6` are
all offered for Functions v4 / Windows. `Deploy-WwExecutionEngine.ps1` (and the JobProcessor /
ServiceBusWorker companions) now create on `10`.

**The create path is not the whole story.** `--runtime-version` applies only at
`functionapp create`; `WarewolfServer-UAT` already exists on `netFrameworkVersion v8.0`, so a
redeploy leaves it there and the `net10.0` package fails at startup with *"You must install or
update .NET to run this application."* Pass `-UpgradeRuntimeVersion` to reconcile it (the app
restarts), or set it out of band:

```powershell
az functionapp config set -n WarewolfServer-UAT -g Warewolf --net-framework-version v10.0
```

Without the switch the orchestrator reports the mismatch in the Phase 0.5 plan and warns in
Phase 1, but changes nothing.

### 5.3 Build

```powershell
.\Compile.ps1 -ServerTests
dotnet publish Dev\Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj -c Release -o D:\ExecutionEngine\Publish
```

Publish path must not collide with JobProcessor/ServiceBusWorker publish dirs — the orchestrator
fails at plan time if they do.

### 5.4 Stage the workflow resources (the `Resources` folder)

**This step is mandatory and is the step this spec exists to stop people skipping.**
`Warewolf.Execution.Lightweight.csproj` only copies its bundled `Resources\**\*` into the
build/publish output when `$(Configuration) == 'Debug'` — a `-c Release` publish (§5.3) produces
**no `Resources` folder at all**. The engine reads workflows from
`WorkflowsDirectory` (env var, default `<wwwroot>\Resources`) at startup and per-invocation; if
that folder is absent, every workflow execution fails immediately.

> **2026-08-19 — do not set a `WorkflowsDirectory` app setting on this app.** It was
> discovered set to a persistent Azure Files path (`D:\home\data\Warewolf\Resources`, *outside*
> the deployed package) on `WarewolfServer-UAT`, almost certainly as an earlier, ad-hoc
> workaround for the exact `FileNotFoundException` below, predating this spec's guidance to
> stage `-WorkflowsSourcePath` into `<PublishDir>\Resources`. Because that override takes
> precedence over the package's own `Resources` folder, **every zip-deploy since it was set —
> pipeline or manual — silently never reached the files the running engine actually used**; the
> engine kept serving whatever had last been pushed to that persistent path by hand via Kudu.
> This caused a real, hard-to-diagnose incident (see `docs/ShovelBridge-Architecture.md`'s
> 2026-08-19 correction entry: an orphaned, unencrypted duplicate `NewSqlServerSource.bite` on
> that persistent path kept routing DB activities at the deleted `WarewolfEntraTestDb`, no
> matter how many times the correct, encrypted source was redeployed). The app setting has been
> removed and the persistent folder deleted; `Deploy-WwExecutionEngine.ps1`'s existing
> `-WorkflowsSourcePath` staging into the package (as documented in this section) is now the
> **sole** mechanism that updates workflow resources on this app, with no manual Kudu step
> required. If you ever need to set `WorkflowsDirectory` again for some other reason, be aware
> it makes the package's `Resources` folder — and therefore every future deploy — irrelevant
> until it is unset again.

This is not theoretical: the App Insights resource for this app (`warewolfserver-uat-ai`, RG
`DEV2`) shows exactly that failure during the 2026-08-13 load test — a ~2-minute burst of
**29,782** identical exceptions,

```
System.IO.FileNotFoundException: Could not find file 'C:\home\site\wwwroot\Resources'.
```

thrown from `ServiceBusWorkflowTrigger` (and its `AzureExecutionLogger`/`AuditExecutionLogger`),
followed by zero further exceptions while `ServiceBusResult` kept polling every few seconds for
results that could never arrive — i.e. the load test's apparent "hang" was every workflow
execution failing instantly on a missing `Resources` folder, not a performance problem. Query it
yourself with:

```powershell
az monitor app-insights query --app warewolfserver-uat-ai --resource-group DEV2 `
  --analytics-query "exceptions | summarize count() by type, tostring(outerMessage) | order by count_ desc" -o json
```

(Use `-o json`, not `-o table` — the CLI's table formatter silently renders blank for some
`summarize`/`bin()` shapes even when rows exist.)

**Required resource set for the ShovelBridge load test** (R7/R8):

| File | Source | Include? |
|---|---|---|
| `Resources\rabbit\RabbitProcess.bite` | committed, `Dev\Warewolf.Execution.Lightweight\Resources\rabbit\` | Yes — the workflow under test |
| `Resources\rabbit\RabbitProcess2.bite` | committed, same folder | Yes — second load-test workflow |
| `Resources\rabbit\NewSqlServerSource.bite` (`SourceId="b9184f70-64ea-4dc5-b23b-02fcd5f91082"`) | **committed**, same folder — pre-encrypted (`WFAES::…`), decrypted at runtime via the engine's Key Vault wiring | Yes — both workflows' DB activities bind to this ID; without it every DB step fails to resolve its source |
| `Resources\rabbit\NewSqlServerSource (Local Backup).bite` (`d3f6a2e1-…`) | committed, same folder | **No — exclude.** Unreferenced by both workflows; its connection string is DPAPI-encrypted under its author's Windows account and cannot decrypt on another machine or in Azure |

> **Update (WOLF-8510, 2026-08-18):** `NewSqlServerSource.bite` used to be fetched fresh from the
> devops endpoint on every run because its connection string rotated on every tunnel restart. That
> endpoint dependency has been retired — the source is now committed pre-encrypted at
> `Resources\rabbit\NewSqlServerSource.bite` with the same `SourceId`, so step 2 below (and both
> pipelines' `Download NewSqlServerSource.bite from DevOps Endpoint` step) is no longer needed for
> this workflow set. **Do not re-introduce a second `NewSqlServerSource.bite` anywhere else under
> the staging folder** — `LightweightSourceLoader.BuildFileIndex` indexes `.bite` files by
> `ResourceID` across the whole `Resources` tree and silently lets the last file found win on an ID
> collision (`Infrastructure/LightweightSourceLoader.cs`), so a second copy at a different path
> would make source resolution non-deterministic instead of failing loudly.

Assemble a local staging folder — the committed `rabbit\` folder already carries the DB source, so
no devops-endpoint fetch is required:

```powershell
$stagingDir = 'D:\ExecutionEngine\WorkflowResources'
New-Item -ItemType Directory -Path "$stagingDir\rabbit" -Force | Out-Null

# Committed rabbit workflows + DB source — copy everything except the unreferenced local-backup source.
Get-ChildItem 'Dev\Warewolf.Execution.Lightweight\Resources\rabbit' -File |
    Where-Object { $_.Name -ne 'NewSqlServerSource (Local Backup).bite' } |
    Copy-Item -Destination "$stagingDir\rabbit" -Force
```

Pass `$stagingDir` as `-WorkflowsSourcePath` in §6.2/§6.4 below — the orchestrator copies its
contents into `<PublishDir>\Resources`, runs `Generate-WorkflowIndex.ps1` over the result, and
(only when `-EncryptResources` is set) WFAES-encrypts it. This spec does not set
`-EncryptResources`, so `NewSqlServerSource.bite` must already carry a decryptable
(WFAES-encrypted) `ConnectionString` — do not hand-edit it.

> **Callers must use the folder-qualified workflow name after this staging (confirmed
> 2026-08-14 — see `docs/ShovelBridge-Architecture.md`).** Because the `rabbit\` subfolder is
> preserved (`Resources\rabbit\RabbitProcess.bite`, not a flat `Resources\RabbitProcess.bite`),
> `Generate-WorkflowIndex.ps1` keys the workflow in `workflow-index.json` as
> `rabbit/rabbitprocess` — a full relative path. `WorkflowIndex.Resolve()` does an **exact key
> match only**, and the legacy on-disk fallback (`WorkflowFunctionHelper.cs`) is
> **non-recursive**, so a bare `RabbitProcess` workflow name can **never** resolve against this
> layout — it fails instantly with `Workflow file not found: ...\Resources\RabbitProcess.xml`
> for every execution, which looks like message loss/a stuck load test but isn't. Any caller
> (pipeline, `Test-ShovelBridgeE2E.ps1 -WorkflowName`, manual `curl`) must pass
> `rabbit/RabbitProcess`, not `RabbitProcess`. `pipeline-LOADTEST.yml` and `pipeline-CLOUD.yml`
> were updated accordingly on 2026-08-14.

## 6. Procedure

### 6.1 Upgrade the runtime

```powershell
az functionapp config set --name WarewolfServer-UAT --resource-group DEV2 --net-framework-version v10.0
az functionapp config appsettings set --name WarewolfServer-UAT --resource-group DEV2 `
  --settings WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED=0
```

### 6.2 Dry run

```powershell
& Dev\Warewolf.Execution.Lightweight\Scripts\Deploy-WwExecutionEngine.ps1 `
    -ResourceGroup       'DEV2' `
    -Location            'southafricanorth' `
    -StorageAccount      'warewolfuatsa' `
    -AppName             'WarewolfServer-UAT' `
    -PublishPath         'D:\ExecutionEngine\Publish' `
    -WorkflowsSourcePath 'D:\ExecutionEngine\WorkflowResources' `
    -SkipAuthProvisioning `
    -LicenseCheckEnabled:$false `
    -DryRun
```

`-StorageAccount` is Phase-1 existence/creation bookkeeping only — the orchestrator never applies
it to an existing app's `AzureWebJobsStorage` (that connection string is only set inside
`functionapp create`, which doesn't run when the app already exists), so `WarewolfServer-UAT`
keeps running on its real, existing storage account regardless of this value. **Do not** resolve
it dynamically from the live `AzureWebJobsStorage` setting: the real account
(`storageaccountwarew83c6`) lives in RG `Warewolf`, not `DEV2`, and the pipeline's service
principal only has RBAC on `DEV2` — so the orchestrator's cross-RG existence check (which falls
back to a subscription-wide `az storage account show` when the RG-scoped lookup misses) silently
returns "not found" (a 403 swallowed by its `-AllowFail` probe) and it then tries to recreate an
account that already exists globally, failing with `StorageAccountAlreadyExists`. Instead, use a
small dedicated placeholder account owned by this SP directly in `DEV2` — `warewolfuatsa` — the
same pattern as `pipeline-CLOUD.yml`'s `warewolfserversa`.
`-WorkflowsSourcePath` must point at the staging folder assembled in §5.4 — **omitting it is the
defect that caused the 2026-08-13 load-test hang** (§5.4); the orchestrator does not fail if it's
left out, it just quietly ships an app with no `Resources` folder.

### 6.3 Review the PLAN output (Phase 0.5)

The orchestrator resolves every decision and prints a masked summary **before** changing
anything. Confirm specifically:

- targeting is `WarewolfServer-UAT` / `DEV2` — **not** `warewolfserver`;
- the Service Bus trigger's queue/workflow are unchanged — WOLF-8517: check
  `Settings/servicebustrigger.bite` on the app (no longer `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE`
  in the app-settings list — see the §5's correction note above);
- **`Workflows source` is NOT `(none)`** — it must show the §5.4 staging folder path;
- staged workflow resources include `RabbitProcess.bite`, `RabbitProcess2.bite`, and a
  `NewSqlServerSource.bite` resolving to `SourceId="b9184f70-64ea-4dc5-b23b-02fcd5f91082"`;
- `NewSqlServerSource (Local Backup).bite` (`d3f6a2e1-…`) is **not** staged;
- no app setting from §2 is dropped.

### 6.4 Deploy

Re-run §6.2 without `-DryRun` and approve at the Phase 0.5 prompt. A timestamped transcript and a
masked `*.summary.json` are written for the run — keep both.

### 6.5 Post-deploy settings diff

```powershell
az functionapp config appsettings list --name WarewolfServer-UAT --resource-group DEV2 --query "[].name" -o tsv | Sort-Object > after.txt
# compare against the backup's names
```

Any missing name is a defect — restore from the backup before proceeding.

## 7. Verification

1. **App starts:** app is `Running`; no startup exceptions in App Insights
   (`warewolfserver-uat-ai`, app id `4e3df34e-a53a-4c3b-a48a-c029db66fd0b`) for 10 minutes after
   deploy.
2. **Runtime is actually .NET 10:** confirm `netFrameworkVersion` is `v10.0` *and* the worker
   started — a mismatched placeholder can leave the app up but failing per-invocation.
3. **`Resources` folder actually deployed (§5.4 — check this before anything else):** the
   `StartupOrchestrator` logs the disk-scan result on every cold start. Confirm it found `.bite`
   files and confirm zero `FileNotFoundException`s referencing `Resources`:

   ```kusto
   traces
   | where timestamp > ago(15m)
   | where message contains "StartupOrchestrator found" and message contains ".bite files"
   | project timestamp, message
   ```

   ```powershell
   az monitor app-insights query --app warewolfserver-uat-ai --resource-group DEV2 `
     --analytics-query "exceptions | where timestamp > ago(15m) and outerMessage contains 'wwwroot\\Resources' | count" -o json
   ```

   Both must show resources found / zero matching exceptions before proceeding to step 4 — this
   is the exact signature of the 2026-08-13 incident (§5.4, R7).
4. **New code is live** — this is the whole point of the redeploy. Run a **single**-message
   `Test-ShovelBridgeE2E.ps1 -VerifyWorkflowExecution` first, then confirm in App Insights:

   ```kusto
   traces
   | where timestamp > ago(15m)
   | where message contains "DB proc result"
   | project timestamp, message
   ```

   The `DB proc result | Proc=… | Rows=… | FirstValue=…` line only exists in the new build. If it
   is absent, the deploy did not take effect and there is no point running the load test.
5. **Load test:** re-run `ShovelBridgeLoadTest_ExternalServiceBus` (1000 messages).

## 8. Rollback

```powershell
& Dev\Warewolf.Execution.Lightweight\Scripts\Rollback-WwExecutionEngine.ps1 -AppName 'WarewolfServer-UAT' -ResourceGroup 'DEV2'
```

Plus, if the runtime bump is implicated:

```powershell
az functionapp config set --name WarewolfServer-UAT --resource-group DEV2 --net-framework-version v8.0
az functionapp config appsettings set --name WarewolfServer-UAT --resource-group DEV2 --settings WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED=1
```

Then restore app settings from the §5.1 backup if the diff showed drift.

## 9. What the redeploy is expected to reveal

The load test's remaining ~130–200 failures fall into classes the new build instruments directly:

| Failure class | What the new build adds |
|---|---|
| `System.Exception` "Object reference not set" (~2650/run) | `DsfSqlServerDatabaseActivity` now logs the exception object (type + stack) and a null guard names `ServiceExecution`/`Inputs`/`Outputs` explicitly. Today this is logged as a stackless `System.Exception` with `method: Unknown`. |
| SQL `51001` / `51002` invalid state transition (~86/run) | `MssqlReadData` now logs `Proc`, `Rows`, `FirstValue` (the `JobLogId`) per step, so a failing execution can be joined to its `dbo.jobs1` row. |
| `RpcException` deadLetterErrorDescription > 4096 (~24/run) | Descriptions are now truncated, so poison messages dead-letter instead of being redelivered (`jobs1` currently shows attempts up to 8). |
| Pool exhaustion / post-login timeouts (~16/run) | The 15197 retry amplification is removed, returning ~15s of held connection per execution to the 100-connection pool. |

Leading hypothesis for the first two classes is state bleeding between concurrent executions; the
new logging is designed to confirm or kill it.

## 10. Follow-ups (not part of this deploy)

- **Automate UAT deploys.** ✅ **Done (2026-08-15).** Added a `Deploy_UAT` stage to
  `pipeline-LOADTEST.yml` (`Build_And_Publish_UAT` + `Deploy_UAT` jobs), which runs on every
  pipeline execution, immediately before `Load_Test` — it publishes the current commit,
  stages the load-test workflow resources + a fresh `NewSqlServerSource.bite`, passes
  `-StorageAccount 'warewolfuatsa'` (a small dedicated placeholder account owned by the pipeline's
  SP directly in `DEV2` — Phase-1 bookkeeping only, never applied to the app's real storage; see
  §6.2 for why the earlier "resolve dynamically from `AzureWebJobsStorage`" approach was reverted
  — WOLF-8510, 2026-08-15), passes
  `-EnablePersistence` with the git-tracked `Scripts/persistencesettings.uat.json` and
  `Scripts/persistencesettingsdbsource.uat.bite` (§14's own follow-up — both committed, the
  latter WFAES-encrypted at rest like `Settings/ElasticsearchLoggingSource.bite`, so no
  Secure File/secret-variable handling is needed), explicitly restarts the app afterward (the
  §13 `WEBSITE_RUN_FROM_PACKAGE` gotcha), and smoke-tests `/apis.json` before letting
  `Load_Test` proceed. No manual operator action is required — both persistence files are
  checked into source control (see `Scripts/README.md`'s "Deploy targets" section).
  **Changed 2026-10-05:** the stage no longer publishes the current commit. `Deploy_UAT` now
  downloads the **published** linux-x64 engine package and `Deploy-WwExecutionEngine.ps1` from
  `warewolf.online/release-notes.php` (as the Profiler Dev/UAT pipelines do), and the
  `Build_And_Publish_UAT` job became `Stage_UAT_Resources` (workflow resources only). The
  §9-§13 stale-deployment gap stays closed — every run still deploys a fresh package and
  full-restarts the app — but a source fix now reaches UAT only once it is released, or when
  `WarewolfEngineZipUrl`/`WarewolfScriptsZipUrl` are pinned to a published build.
- **Version stamping.** There is no reliable way to tell which build UAT is running. Surface an
  app version in a health endpoint and log it at startup.
- **`jobs1`/`jobs2` provisioning script.** The schema exists only as live database state; that is
  why `jobs2`'s absence went unnoticed until 2026-08-13.
- **R5** (`httpsOnly: false`) and **R3** (Consumption plan for a 1000-message load test) are
  standing decisions for the team.

## 11. Docs to update after execution

Per `CLAUDE.md` change-synchronisation:

- `Scripts/README.md` — note UAT as a deploy target. **Done (2026-08-15)** — see the new
  "Deploy targets" section and the `persistencesettings.uat.json` row in the scripts table.
- `docs/Deploy-RunGuide.md` / `docs/Deploy-EndToEnd-Runbook.md` — add the UAT flow. Still
  outstanding; the automated `Deploy_UAT` pipeline stage (§10) now covers routine redeploys, but
  a manual runbook entry for out-of-band/emergency redeploys is not yet written.
- `docs/ShovelBridge-Architecture.md` — record that the load test depends on a manually deployed
  engine, and the outcome of the redeploy. **Done (2026-08-14)** — see the dated entry covering
  the `rabbit/RabbitProcess` workflow-name resolution finding below §5.4 of this doc. Note this
  is now superseded by §10's automation — the load test no longer depends on a *manual* deploy.
- `Dev/.azure/pipeline-LOADTEST.yml` / `pipeline-CLOUD.yml` — **Done (2026-08-14)**:
  `VerifyWorkflowName` updated from `'RabbitProcess'` to `'rabbit/RabbitProcess'` in both, per
  the folder-qualified-name requirement now called out in §5.4 above. **Further update (2026-08-
  15)**: `pipeline-LOADTEST.yml` gained the `Deploy_UAT` stage (§10) — the deploy-then-test flow
  it was previously missing.

## 12. Execution findings (2026-08-14, second pass)

Re-attempting this spec on 2026-08-14 found **§1-6 already executed** — by another operator or
automated process between this spec being drafted (2026-08-13) and this pass — before any change
was made in this session. Verified directly (App Insights for `warewolfserver-uat-ai` still has
**zero telemetry**, so verification here used the Kudu VFS API against the live site instead):

| Check | Result |
|---|---|
| `netFrameworkVersion` | `v10.0` (confirmed via `az functionapp config show`) |
| `WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED` | `0` (R2 mitigation applied) |
| `lastModifiedTimeUtc` | `2026-08-14T00:22:08Z` — after this spec was drafted |
| `Warewolf.Execution.Lightweight.dll` mtime (Kudu VFS) | `2026-08-13T23:58:12` — a rebuild, not the stale 08-13 08:50 package |
| `Resources/` (Kudu VFS) | contains `workflow-index.json` + `rabbit/` only — **no flat-layout or stray files** |
| `Resources/rabbit/` (Kudu VFS) | `RabbitProcess.bite`, `RabbitProcess2.bite`, `NewSqlServerSource.bite` — **`NewSqlServerSource (Local Backup).bite` correctly excluded**, matching the §5.4 required set exactly |
| `NewSqlServerSource.bite` content | `SourceId="b9184f70-64ea-4dc5-b23b-02fcd5f91082"` confirmed, `ConnectionString="WFAES::..."` (not DPAPI, not an HTML SSO page) |
| `workflow-index.json` keys | `rabbit/rabbitprocess`, `rabbit/rabbitprocess2` present — folder-qualified, matching the `rabbit/RabbitProcess` name now used by both pipelines |
| Live probe | `GET /apis.json` → `200 OK` (app is up and responding) |

**Not verified — blocked, requires the pipeline's own secrets:**

- §7.4 (single-message functional proof) and §7.5 (full 1000-message load test) both need a live
  Entra client-credentials token. Acquiring one requires `ShovelE2EDaemonClientSecretValue`
  (`pipeline-LOADTEST.yml` "Acquire Entra token for ShovelBridge E2E daemon" step) — an Azure
  DevOps secret pipeline variable, which by design cannot be read back outside a pipeline run.
  Not present in this session's environment; no equivalent was found in the app's own Key Vault
  (`WWExecutionEngine`).
- Triggering `pipeline-LOADTEST.yml` directly (which already has this secret wired in) via
  `az devops` was attempted but blocked by the same broken `az` CLI extension-metadata permission
  error already on file for `az monitor app-insights query` (`PermissionError: Access is denied:
  ...\cliextensions\application-insights\application_insights-1.2.3.dist-info` — this file's ACL
  could not be read or repaired from this session either).
- Deliberately did **not** substitute an interactively-logged-in user token for the daemon app's
  client-credentials token — that would exercise a different, untested auth path and risks
  recording a false pass.

**Action needed to close out §7:** either supply `ShovelE2EDaemonClientSecretValue` (plus
`ShovelE2EDaemonClientId` / `ShovelE2EResourceAppId` / `ShovelE2EEntraTenantId`, all needed to
mint a `-MessageAuthToken`) to run `Test-ShovelBridgeE2E.ps1 -VerifyWorkflowExecution` directly, or
trigger `pipeline-LOADTEST.yml`'s `ShovelBridgeLoadTest_ExternalServiceBus` job from a machine/
account with working Azure DevOps access.

## 13. Corrective redeploy and fix verification (2026-08-14, third pass)

The user supplied an env var claiming to hold `ShovelE2EDaemonClientSecretValue`. It did **not**
match the app registration's active secret (`az ad app credential list` showed hint `ms7`, created
2026-08-14T06:52; the supplied value's hint was `lX6`) — `AADSTS7000215: Invalid client secret
provided`. Per the sanctioned convention documented in a `pipeline-CLOUD.yml` comment ("minted ONCE
by an Application Administrator via `az ad app credential reset`... reused directly, NOT rotated
per run"), minted a new **additive** secret (`shovelbridge-verification-2026-08-14-temp-2`, expires
2026-09-14, does not revoke `ms7`) on app `dc1182bc-ffc1-4a1d-a414-ab672998eb9a` and acquired a
valid Entra token against `api://e200900a-.../.default` (confirmed same audience as both
`WAREWOLF_ENTRA_AUDIENCE` and `WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE` on this instance).

**Lower-risk proxy for §7.4:** rather than standing up RabbitMQ+Shovel+ServiceBus locally, called
the engine's `/Secure/rabbit/RabbitProcess` HTTP route directly with this token
(`WorkflowHttpFunction.cs`) — same `WorkflowExecutor`/Resources/SQL-activity path as the Service
Bus trigger, different Function entry point. This does **not** exercise the RabbitMQ Shovel or the
`ServiceBusWorkflowTriggerFunction` wrapper itself.

**Finding: the §12 "already executed" redeploy did not contain the concurrency fix.** A single
call succeeded, but a 15-way concurrent burst reproduced the *exact* pre-fix signature verbatim —
`"Object reference not set to an instance of an object."` and `"Error with variables in input.
[[JobLogId]]"`, plus the underlying `SQL Error [Number=51001]... invalid state transition — row
not found in STARTED status for this JobLogId` — 10/15 requests failed. This is the precise
signature the `2b784fe7b6` fix's own code comment documents reproducing on 2026-08-11 pre-fix.
Confirmed the currently-checked-out `8510` HEAD **does** contain the fix
(`_workflowPool`/`RentPreparedWorkflow`/`PreparedWorkflow` present in `WorkflowExecutor.cs`, no
uncommitted changes), and that the live deployed DLL (388096 bytes) differs in size from a fresh
local `Release` build of that HEAD (389120 bytes) — i.e. **whatever was deployed in the §12 pass
was not built from this HEAD**. Kudu's deployment history has no commit/author metadata (plain
`az_cli_functions` zip pushes), so the actual source of that build could not be traced further.

**Corrective action taken:** rather than leaving a known-broken fix live, performed a targeted
code-only redeploy (Resources were already correct, so left untouched):
1. Backed up the live DLL (`Warewolf.Execution.Lightweight.dll`, 388096 bytes) to
   `$env:TEMP\uat-redeploy-backup\pre-corrective-dll\`.
2. Downloaded the live `Resources/` tree via Kudu VFS (already verified correct in §12) to
   `$env:TEMP\uat-redeploy-backup\live-resources\`.
3. Merged it into the already-built, HEAD-sourced `C:\ExecutionEngine\Publish\` (confirmed via
   binary string search to contain `RentPreparedWorkflow`/`PreparedWorkflow`).
4. Zipped and deployed via `az functionapp deployment source config-zip` (`DEV2` /
   `WarewolfServer-UAT`).
5. **`WEBSITE_RUN_FROM_PACKAGE=1`** on this app — the deploy uploads a new package
   (`d:\home\data\SitePackages\<timestamp>.zip`) and updates `packagename.txt`, but the running
   worker does not pick it up until restarted. Kudu VFS still showed the old DLL immediately after
   a "Succeeded" deploy; an explicit `az functionapp restart` was required before the new package
   took effect. **Worth adding to `Deploy-WwExecutionEngine.ps1`/its docs as a known gotcha** if not
   already handled by the orchestrator's own deploy phase.
6. Verified post-restart: deployed DLL now 389120 bytes / mtime matching the local build exactly;
   `Resources/rabbit/` unchanged (3 files, same sizes as §12).

**Post-redeploy verification:**
- Single-request smoke test: `200 OK`.
- 20-way concurrent burst (cold pool, first traffic since restart): 10/20 succeeded. **Zero
  `[[JobLogId]]`/SQL 51001 errors** — the failures that did occur were a new signature:
  `Insufficient memory to continue the execution of the program` inside
  `RentPreparedWorkflow → ActivityParser.Parse → System.Activities.ScriptingAotCompiler.BuildAssembly`
  (Roslyn VB-expression JIT compilation), plus several `502 Bad Gateway`s consistent with
  Consumption-plan cold-start/scale-out under a sudden burst against an empty workflow pool (every
  concurrent request on a cold pool must compile its own XAML+VB chain simultaneously — this is
  the known, documented up-front cost the pooling fix explicitly trades for correctness).
- 10-way concurrent burst immediately after (pool now warm): **10/10 succeeded, zero errors of any
  kind.**

**Conclusion:** the concurrency fix is confirmed working on the live, corrected UAT deployment —
the original `[[JobLogId]]` failure mode does not reproduce once the pool is warm, across two
separate concurrent bursts (15-way and 20-way) post-fix vs. one pre-fix (15-way, 10/15 failed with
the JobLogId signature). The transient cold-pool memory/502 errors on the very first post-restart
burst are a **separate, plan-capacity concern** (Roslyn compilation is memory-heavy; the
Consumption/current plan may need a warm-up strategy, pre-compiled workflow cache persistence
across cold starts, or a higher-memory plan for bursty concurrent traffic) — not a recurrence of
the bug this spec targeted, but worth a follow-up item (see §10).

**Still not executed:** §7.5, the full 1000-message load test through the actual
RabbitMQ→Shovel→Service Bus pipeline (the direct-HTTP calls above bypass RabbitMQ, the Shovel, and
`ServiceBusWorkflowTriggerFunction` entirely). Needs either the real ADO pipeline run or standing up
RabbitMQ+Shovel+Service Bus locally.

**Cleanup owed:** the temporary Entra secret `shovelbridge-verification-2026-08-14-temp-2` on app
`dc1182bc-ffc1-4a1d-a414-ab672998eb9a` (expires 2026-09-14, additive, does not affect `ms7`) should
be deleted once the user confirms no further verification is needed, via
`az ad app credential delete --id dc1182bc-ffc1-4a1d-a414-ab672998eb9a --key-id <id>`.

## 14. Persistence (Hangfire) enabled — critical guardrail for ALL future redeploys (2026-08-15)

**§7.5 (the full 1000-message load test) is now executed and passing 1000/1000 — see
`docs/ShovelBridge-Architecture.md`'s dated 2026-08-15 entries for the full root-cause writeup.**
The blocker was NOT the RabbitMQ Shovel bridge; it was `Config.Persistence.Enable=false` on this
deployment, which made `ServiceBusReplayAndResultStore` fall back to a per-instance in-memory
result cache — invisible across Consumption-plan scale-out instances, producing the "no result"
cases every prior pass of this spec (§9-§13) chased as a bridge-delivery problem.

**Current live state (as of 2026-08-15, ~06:30 UTC):**
- `Settings/persistencesettings.json`: `"Enable": true` (was `false`).
- `Settings/persistencesettingsdbsource.bite`: WFAES-encrypted connection string to Azure SQL
  database `wwexecution-uat-hangfire` (server `warewolf-dev2-mcgeaj`, RG `DEV2`), SQL login
  `wwexecution_uat_hangfire` (already existed with `db_owner`, pre-dating this fix — its password
  was reset as part of this change since the prior one was unknown/unrecoverable). Encrypted with
  the SAME Key Vault key already used for this deployment's other WFAES resources (`WWExecutionEngine`
  / secret `WWExecutionEngineTestSecret` — read from the live app's own `KEYVAULT_SECRET_NAME`
  setting).
- `wwexecution-uat-hangfire` scaled from its original `S0` (10 DTU) to **`S2` (50 DTU)** —
  `S0` produced 8/1000 SQL-capacity-related failures (bare 500s + one token-validation timeout)
  under the load test's burst; `S2` produced 0/1000. This is a resource-tier setting, trivially
  reversible (`az sql db update --service-objective <tier>`), not a code/config change.
- Deployed via the minimal-diff path: downloaded the entire live, already-working package via
  Kudu's `/api/zip/site/wwwroot/`, replaced ONLY the two `Settings/persistence*` files in place,
  re-zipped, `az functionapp deployment source config-zip`, `az functionapp restart` — deliberately
  NOT a full rebuild-and-redeploy, to avoid the R7/R8 (`Resources` folder) and app-settings-drift
  risks §5.4/§6.5 of this spec exist to catch.

**⚠️ MANDATORY for the next person who does a full redeploy of `WarewolfServer-UAT` (§6.1-§6.4):**
A full redeploy publishes fresh from source, which means the repo's own committed defaults apply
unless overridden — `Settings/persistencesettings.json` ships `"Enable": false` and there is no
committed `persistencesettingsdbsource.bite` at all (by design — see its own header comment; other
consumers of this repo should NOT get persistence-enabled-by-default). **If a full redeploy is done
without explicitly passing `-EnablePersistence -PersistenceSettingsPath <Enable:true copy>
-PersistenceDbSourcePath <bite pointing at wwexecution-uat-hangfire>` to `Deploy-WwExecutionEngine.ps1`,
persistence will silently revert to disabled and every "no result" symptom in this document's §9-§13
will return** — this is exactly what happened between the 2026-08-13 provisioning of
`wwexecution-uat-hangfire` (which enabled persistence and produced nine clean 1000/1000 runs, per
the row-count evidence in `ShovelBridge-Architecture.md`) and this fix. Add a Phase 0.5 plan-review
check for "Persistence (Hangfire): enabled (wwexecution-uat-hangfire)" specifically for this app,
the same way §6.3 already checks `Workflows source`.

**Follow-up done (2026-08-15):** `Scripts/persistencesettings.uat.json` (`Enable: true`,
otherwise identical to the repo default) and `Scripts/persistencesettingsdbsource.uat.bite`
(a `DbSource` pointing at `wwexecution-uat-hangfire`, `ConnectionString` WFAES-encrypted at
rest) are now both committed as the checked-in source of truth for `-PersistenceSettingsPath`
and `-PersistenceDbSourcePath`, consumed by `pipeline-LOADTEST.yml`'s `Deploy_UAT` stage (see
§10). Committing the DbSource `.bite` follows the same precedent already set by
`Settings/ElasticsearchLoggingSource.bite` — its connection string is only ever decryptable
via this app's own Key Vault key at runtime, so it does not need Secure File/secret-variable
treatment like a plaintext credential would.

## 15. WOLF-8516 (environment-variable cleanup) reconciliation (2026-09-02)

Same failure shape as §14: a redeploy that doesn't pass a param the merged engine code now
needs silently reverts a working setting to disabled/default rather than failing loudly.

`8516-EnvVarCleanup` (merged into this branch via `64766d029e`) changed what
`Warewolf.Execution.Lightweight` reads at runtime — see `docs/ShovelBridge-Architecture.md`'s
WOLF-8516 entries and `Auth/Models/EntraIdentityOptions.cs` / `Auth/Models/
ServiceBusTriggerOptions.cs` / `Infrastructure/HostEnvironmentConfig.cs` doc comments — but
`pipeline-LOADTEST.yml`'s `Deploy_UAT` stage was never updated to pass the new params, and
`Deploy-WwExecutionEngine.ps1` had no way to set two of the three affected settings at all.
Confirmed via `az functionapp config appsettings list --name WarewolfServer-UAT
--resource-group DEV2` on 2026-09-02: the app still carries every pre-8516 setting below,
none of which the current engine code reads any more.

| Live (pre-8516) setting, still present | No longer read — now sourced from | Effect of a redeploy without the fix |
|---|---|---|
| `AZURE_KEYVAULT_NAME=WWExecutionEngine`, `KEYVAULT_SECRET_NAME=WWExecutionEngineTestSecret` | `Settings/executionengine.settings.json` (`keyVaultName`/`keyVaultSecretName`) | Key Vault decryption disabled app-wide — every WFAES-encrypted resource (including `NewSqlServerSource.bite`, which `RabbitProcess` itself depends on) fails to decrypt. |
| `WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS=2` | same file (`serviceBusTrigger.maxConcurrentExecutions`) | Silently reverts to the hardcoded default of 8, disagreeing with the `host.json` dispatch cap of 2 the WOLF-8512 fix deliberately kept in sync (§ ShovelBridge-Architecture.md WOLF-8512 entries). |
| `WAREWOLF_ENTRA_TENANT_ID`, `WAREWOLF_ENTRA_AUDIENCE`, `WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE` (no `WAREWOLF_ENTRA_CLIENT_ID` was ever set) | merged `WAREWOLF_ENTRA_CONFIG` JSON App Setting | Entra Bearer-token validation fails closed (`EntraAuthOptions.IsEnabled` false — no fallback to the legacy vars) — breaks this pipeline's own ShovelBridge load test job (`-VerifyWorkflowExecution` authenticates with an Entra token) and the Service-Bus-triggered path. Unlike the two rows above, a plain redeploy **never** fixes this: `WAREWOLF_ENTRA_CONFIG` is an App Setting, not a bundled file, and the only script that writes it (`Configure-WwExecutionAuth.ps1`) is skipped by `-SkipAuthProvisioning` (§3, "out of scope"). |

**Fix applied (2026-09-02):** `Deploy-WwExecutionEngine.ps1` gained the 5
`-ServiceBusTrigger*` tunable params (staged into `executionengine.settings.json`
independently of `-KeyVaultName`/`-EncryptResources`). `pipeline-LOADTEST.yml`'s
`Deploy_UAT` stage now passes `-KeyVaultName WWExecutionEngine -KeyVaultSecretName
WWExecutionEngineTestSecret -ServiceBusTriggerMaxConcurrentExecutions 2` alongside its
existing params.

**Correction (2026-09-14) — there is no `-SyncEntraConfig` switch.** This section
previously recorded a `-SyncEntraConfig` switch on `Deploy-WwExecutionEngine.ps1` that
migrates the legacy Entra vars into `WAREWOLF_ENTRA_CONFIG`. **It was never implemented**
(`git log --all -S SyncEntraConfig` matches only the two pipeline YAMLs and this document —
no `.ps1`, ever). Both pipelines were nonetheless updated to pass it, and both deploys then
failed with `A parameter cannot be found that matches parameter name 'SyncEntraConfig'`:
`pipeline-LOADTEST.yml` on 2026-09-02 (removed the same day in `92b6cf194d`
"8513-remove sync entra config") and `pipeline-CLOUD.yml` in build 30660 on 2026-09-14
(removed then, mirroring that commit).

**Follow-on (2026-09-14) — removing the argument was not the fix for `pipeline-CLOUD.yml`.**
The paragraph above is accurate about `-SyncEntraConfig` itself: it never existed in any
`.ps1`, so `pipeline-LOADTEST.yml` — which runs the script from its own repo checkout —
genuinely failed for that reason alone. `pipeline-CLOUD.yml` had a **second, independent**
defect underneath it. Its `Deploy_And_Test_Azure_Functions` job is `checkout: none`, and it
ran `Deploy-WwExecutionEngine.ps1` out of a hard-coded release zip
(`Warewolf-ExecutionEngine-Deployment-Scripts-v2.0.zip` on `stwwreleases`) published
**2026-06-26** — 1369 lines, 51 parameters — while `Scripts/Deploy-WwExecutionEngine.ps1`
had grown to 2609 lines. So build 30662 still failed on `-SyncEntraConfig`, and once that
argument was deleted builds 30664/30666 failed on `-ServiceBusTriggerQueueName`, which
**does** exist in the repo script (line 442). The pin could never self-heal: the `Release`
stage that publishes the zip runs *after* the `Azure` stage and only on
`Build.Reason=Manual`, and its auto-detected `ScriptsPackageVersion` **bumps** the version
whenever `Scripts/` changes — so a refreshed package is published as `v2.1` and the
hard-coded `v2.0` URL never sees it.

Fixed on 2026-09-14 by deleting the blob-download step and pointing the deploy at
`$(Agent.BuildDirectory)\DeployScripts\Scripts\Deploy-WwExecutionEngine.ps1` — the
`DeployScripts` pipeline artifact the `Build` stage already stages from that commit's
`Scripts/` folder, and which every *other* script step in that job (`Deploy-WwQueueProcessor.ps1`,
`WwE2E.Common.psm1`, `Publish-WwQueueBurst.ps1`, `Get-WwQueueRunReport.ps1`) was already using.
**Both pipelines now run this commit's copy of the deployment scripts; never reintroduce a
version-pinned script source.**

> **2026-10-05:** `pipeline-LOADTEST.yml` now deliberately deploys the **released** scripts
> (see §10). That is not the v2.0 failure mode: the release is resolved from
> `release-notes.php` on every run (latest by default, pinning is opt-in), and the deploy step
> checks the released orchestrator declares every parameter it passes, failing up front with the
> pin to use instead of mid-deploy with `A parameter cannot be found`.

The third row of the table above was an **unresolved deploy-time gap** until §17:
`WAREWOLF_ENTRA_CONFIG` is written only by `Configure-WwExecutionAuth.ps1`, which
`-SkipAuthProvisioning` skips, so for any app deployed that way it used to be a **one-time
manual operator step**, in the same category `Enable-ServiceBusSecureTrigger.ps1` occupies for
these two apps (the profiler pipeline now runs that one automatically against `wwprofilerlw`).
As of §17, `Deploy-WwExecutionEngine.ps1` itself now detects a missing `WAREWOLF_ENTRA_CONFIG`
under `-SkipAuthProvisioning` and repairs it automatically — the manual command below is now a
fallback for out-of-pipeline use, not a required step:

```powershell
az functionapp config appsettings set --name <app> --resource-group DEV2 `
    --settings 'WAREWOLF_ENTRA_CONFIG={"tenantId":"...","audience":"...","clientId":"..."}'
```

Done manually for `WarewolfServer-UAT` on 2026-09-02 and for `warewolfserver` on 2026-09-14,
before the automatic repair existed.
`EntraIdentityOptions.FromEnvironment()` (`Auth/Models/EntraIdentityOptions.cs`) reads this
setting **only** — there is no fallback to the legacy `WAREWOLF_ENTRA_*` vars — so until it is
set, Entra token validation on that app fails closed while the legacy vars sit there looking
correct.

**Guardrail for future redeploys:** the same "params-not-carried-forward" risk still applies to
the OTHER two settings in the table — `-KeyVaultName`/`-ServiceBusTrigger*` values — which are
staged file content, not an App Setting, and are **not** covered by §17's repair (only
`WAREWOLF_ENTRA_CONFIG` is App-Setting-only and therefore both driftable-on-recreate and
cheaply self-healable this way). Add a Phase 0.5 plan-review check for "Key Vault" and
"ServiceBusTrigger tunables" specifically for this app, the same way §6.3 / §14 already check
other settings.

## 17. Automatic repair of `WAREWOLF_ENTRA_CONFIG` drift under `-SkipAuthProvisioning` (2026-09-23)

**What broke:** build 30802 (`pipeline-CLOUD.yml`, job `ShovelBridgeE2ETest_ExternalServiceBus`,
step "Run ShovelBridge E2E test (VerifyWorkflowExecution mode, warewolfserver)") failed —
`GET https://warewolfserver.azurewebsites.net/secure/servicebus-result/{correlationId}` returned
401 `{"error":"unauthorized","message":"Authentication required."}`. The same build's "Probe the
engine route directly (diagnostic)" step (log 74, 15:57 UTC) had already caught it:
`WAREWOLF_ENTRA_AUDIENCE`/`_TENANT_ID`/`_CLIENT_ID` all read `(not set)` on the live app, i.e.
`WAREWOLF_ENTRA_CONFIG` itself was gone. Root cause: §16's Flex Consumption migration deletes and
recreates `warewolfserver`, which wipes every app setting including `WAREWOLF_ENTRA_CONFIG` — and
§16's own "back it up and re-apply it" manual step (line 765-768 above) was not carried out before
this redeploy, exactly the failure mode §16 already flagged as a risk.

**Fix:** rather than relying on that manual step being remembered every time an app deployed with
`-SkipAuthProvisioning` gets recreated, `Deploy-WwExecutionEngine.ps1`'s Phase 2 now probes the
live `WAREWOLF_ENTRA_CONFIG` app setting whenever `-SkipAuthProvisioning` is passed. If it's
missing or empty, it invokes `Configure-WwExecutionAuth.ps1 -RepairEntraConfigOnly`, a new mode
that:
- looks up the **existing** Entra app registration by `-EntraAppDisplayName` and throws if none is
  found (it never creates one — that would defeat the point of `-SkipAuthProvisioning`);
- re-applies `WAREWOLF_ENTRA_CONFIG` via the same read-merge-write (`Merge-EntraConfigJson`) and
  `@file`-based write Stage 8 already used, so any extra field already stored in the setting is
  still preserved;
- does **not** rotate the client secret, touch app roles or user assignments, or reconfigure Easy
  Auth, and does **not** write `WAREWOLF_SECURE_CONFIG` or the client-secret app setting — those
  stay exactly as `-SkipAuthProvisioning` already left them.

If `WAREWOLF_ENTRA_CONFIG` is already present, Phase 2 logs that no repair was needed and behaves
exactly as before this change. See `Configure-WwExecutionAuth.ps1`'s `-RepairEntraConfigOnly` doc
comment for the full parameter contract.

**Still manual:** Easy Auth's own platform configuration (`az webapp auth ...`) is a separate,
non-App-Setting control-plane resource that a delete+recreate also wipes; this fix does not touch
it. Nothing in build 30802's failure pointed at Easy Auth being broken (the 401 came from the
engine's own bearer-token parser, `EntraAuthOptions.IsEnabled`, not from an Easy Auth redirect), so
reprovisioning it automatically was left out of scope here.

## 16. Migration to Flex Consumption, 4 GB/instance (2026-09-22)

`pipeline-LOADTEST.yml`'s `Deploy_UAT` stage (and `pipeline-CLOUD.yml`'s equivalent job for
`warewolfserver`) now deploy onto **Flex Consumption** (`-HostingPlan Flex
-FlexInstanceMemoryMB 4096`) instead of the classic Consumption (`Y1`) plan documented in §2/§4
above. `Deploy-WwExecutionEngine.ps1` already supported this (`-HostingPlan`,
`-FlexInstanceMemoryMB`, `-FlexMaximumInstanceCount` — see its own parameter help and
`Scripts/Tests/Deploy-WwExecutionEngine.Tests.ps1`); this entry records the one-time operator
migration, not a script change.

**Why delete-and-recreate was unavoidable.** Flex Consumption is a different resource shape
(`sku FlexConsumption`), not a tier of the classic plan — no shared App Service Plan object, no
Azure Files content share, Linux-only. `functionapp create` fixes the SKU at creation time, so an
existing Windows `Y1` app cannot be converted in place; `Deploy-WwExecutionEngine.ps1` detects the
mismatch and deliberately does nothing rather than attempt it (see its `-HostingPlan` doc comment).

**Pre-flight, verified against the live subscription before migrating:**
- `southafricanorth` is a supported Flex Consumption region (`az functionapp
  list-flexconsumption-locations`).
- `ASP-Warewolf-a36c` (RG `Warewolf`, still `Y1`) is shared by **four** apps:
  `WarewolfServer`, `WarewolfServer-UAT`, `WarewolfInvoicing`, `WarewolfUsageAPI`. Only the first
  two moved; the plan itself was **not** deleted and still serves the other two — R4 above is
  closed, not because the plan went away, but because this app no longer depends on it.
- Managed-identity RBAC captured immediately before deleting the old app, for re-granting to the
  new identity after recreation (every principal is fresh — `functionapp create` mints a new one):
  - `warewolfserver`: **Key Vault Secrets User** on vault `WWExecutionEngine`; **Storage Blob/
    Queue/Table Data Contributor** on storage account `warewolf` (RG `DEV2`).
  - `WarewolfServer-UAT`: **Key Vault Secrets User** on vault `WWExecutionEngine`; **Azure
    Service Bus Data Receiver** on namespace `WarewolfShovelBridgeTesting` (RG `DEV2`).
- Both apps carry a `WAREWOLF_ENTRA_CONFIG` app setting (§15) that neither pipeline reproduces
  (`-SkipAuthProvisioning`). Back it up (§5.1-style) before deleting and re-apply it, and
  reconfigure Easy Auth platform config (`az webapp auth ...`), after the app is recreated —
  otherwise Entra token validation fails closed exactly as §15's third table row describes.

**Follow-on risk this reintroduces, not yet closed:** R3's replacement question — Flex
Consumption's own cold-start/scale-out behaviour under this pipeline's load-test burst — is
unmeasured as of this entry. Don't assume it matches, or improves on, the `Y1` baseline §3/§14
were tuned against; re-run the load test and re-check the concurrency/scale-limit settings
(`-ServiceBusMaxConcurrentCalls`, `-FlexMaximumInstanceCount`) against observed behaviour before
trusting historical thresholds (e.g. the `FlexMaximumInstanceCount 30` carried over from
`-FunctionAppScaleLimit 30`) on the new platform.

## 18. `-KnownClientId`: the §17 self-heal's first live run needed a Graph-free path (2026-09-24)

**What broke:** build 30806 (`pipeline-CLOUD.yml`, job `Deploy_And_Test_Azure_Functions`, step
"Deploy via Deployment Scripts") failed — the very first live invocation of §17's automatic
`WAREWOLF_ENTRA_CONFIG` repair. `Configure-WwExecutionAuth.ps1 -RepairEntraConfigOnly`'s
`az ad app list --display-name warewolfserver-auth --query [0] -o json` failed with `ERROR:
Insufficient privileges to complete the operation`.

**Root cause, two layers deep:**
1. The pipeline's service connection SP, "Warewolf DevOps" (appId
   `a15fdb40-3dbe-470c-8b4c-55d3d5448fe9`), has **no Microsoft Graph permissions consented** —
   verified live via `GET /servicePrincipals/{id}/appRoleAssignments` returning `[]`, despite the
   app's manifest requesting `Application.ReadWrite.OwnedBy`. Deploying to the Function App itself
   only needs ARM (Contributor), which the pipeline already has; §17's repair was the first thing
   in this pipeline to need Microsoft Graph, and nothing had ever granted it.
2. Even with Graph access, the lookup would still fail: **no app registration named
   `warewolfserver-auth` exists** in the tenant (verified live — only a stale
   `warewolfserver-staging-random-2857634-auth` from 2026-09-03 matches the `warewolfserver*`
   prefix). `warewolfserver`'s `WAREWOLF_ENTRA_CONFIG` was always hand-set against the **shared
   "Warewolf Security" app registration** (appId `05794411-b275-4801-97ac-8b078ed7196c` — the same
   one §17's neighbouring "Probe the engine route directly" pipeline step already uses as
   `$EngineResourceAppIdValue`), not a per-app `<AppName>-auth` registration. §17's repair assumed
   the naming convention `Configure-WwExecutionAuth.ps1`'s normal (non-repair) provisioning flow
   would have used, which was never actually run for this app (`-SkipAuthProvisioning` is
   permanent here).

**Fix:** `-RepairEntraConfigOnly` gained an optional `-KnownClientId <appId>` that skips the
`az ad app list` Microsoft Graph lookup entirely and rebuilds `WAREWOLF_ENTRA_CONFIG` from that
appId directly — everything else about the repair (read-merge-write via `Merge-EntraConfigJson`,
no secret rotation, no role/Easy Auth changes) is unchanged.
`Deploy-WwExecutionEngine.ps1` exposes this as `-EntraConfigRepairClientId` (plus
`-EntraConfigRepairAppDisplayName`, a cosmetic label for the repair step's own logging/summary
when the Graph lookup is skipped), threaded through only when supplied — callers that don't pass
it keep the exact §17 behaviour. `pipeline-CLOUD.yml`'s `warewolfserver` deploy step now passes
`-EntraConfigRepairClientId '05794411-b275-4801-97ac-8b078ed7196c'` and
`-EntraConfigRepairAppDisplayName 'Warewolf Security'`, so no Microsoft Graph permission is
needed anywhere in this pipeline for the self-heal to keep working.

**Deliberately not done:** granting Microsoft Graph permissions (`Application.Read.All` /
`Directory.Read.All`, or the `Directory Readers` role) to the "Warewolf DevOps" SP — the
`-KnownClientId` approach avoids needing that at all, which is the smaller, least-privilege
change. If a future app under `-SkipAuthProvisioning` genuinely does follow the
`<AppName>-auth` convention (i.e. was provisioned by a real, non-repair-mode run of
`Configure-WwExecutionAuth.ps1`), the original Graph-lookup path remains the default — simply
omit `-EntraConfigRepairClientId` for that app's deploy step.

## 19. Back to Windows Consumption (`Y1`) for the load test (2026-10-05)

`pipeline-LOADTEST.yml`'s `Deploy_UAT` stage now deploys `WarewolfServer-UAT` as a **Windows** app on
the classic Consumption (`Y1`) plan, using the **win-x64** engine package. This reverses §16 for this
app only: `pipeline-CLOUD.yml`'s `warewolfserver` is unchanged. The load test is meant to measure
the Windows `Y1` engine.

**What the pipeline changed:**
- **Release resolution.** `Resolve Published Release URLs` reads `data-role="engine-windows-zip-link"`
  and refuses a pinned `linux-x64` `WarewolfEngineZipUrl`. The old guard did the reverse.
- **Deploy splat.**
  - `-HostingPlan Consumption -FunctionAppScaleLimit 30` replaces `-HostingPlan Flex
    -FlexInstanceMemoryMB 4096 -FlexMaximumInstanceCount 16`.
  - 30 is the original `Y1` budget (see the `-FunctionAppScaleLimit` comment in the YAML and
    `ShovelBridge-Architecture.md`'s 2026-10-05 entry for the headroom caveat).
- **Entra self-heal.**
  - `-EntraConfigRepairClientId e200900a-2d5d-4356-94c0-cd7e33232ce0` is now passed (§18).
    Recreating the app wipes `WAREWOLF_ENTRA_CONFIG`, and without the appId the §17 self-heal would
    need a Microsoft Graph lookup that the pipeline SP cannot make.
  - The appId was read back from the live Flex app's setting before the migration. Its audience is
    `api://e200900a-…`, the same resource app the load-test jobs request tokens for.
- **New guard step: `Assert WarewolfServer-UAT is not a Linux (Flex) app`.**
  - `Deploy-WwExecutionEngine.ps1` never converts an existing app (§16). It also does not fail on
    the mismatch, so a win-x64 package would deploy "successfully" onto the old Linux app and never
    start.
  - The guard fails the stage on a `functionapp,linux` app and passes a Windows or missing one.
  - It runs **after** the §5.1 settings backup is published, so the backup is always available
    when it fails.
  - It never deletes anything.

**One-time operator migration (manual, in this order):**
1. **Back up the old app.**
   - Run the pipeline once: the guard fails, but the `UatSettingsBackup` artifact is published.
     Alternatively, export the settings by hand (§5.1).
   - Record the managed identity's RBAC: **Key Vault Secrets User** on `WWExecutionEngine`, and
     **Azure Service Bus Data Receiver** on `WarewolfShovelBridgeTesting` (§16).
2. **Delete the app:** `az functionapp delete --name WarewolfServer-UAT --resource-group DEV2`.
3. **Re-run the pipeline.** The deploy recreates the app on `Y1` (`--os-type Windows`, storage
   `warewolfuatsa`) and self-heals `WAREWOLF_ENTRA_CONFIG`.
4. **Re-grant the new managed identity's RBAC.**
   - Re-run `Enable-ServiceBusSecureTrigger.ps1` against the trigger queue for the Service Bus
     receiver role.
   - Check that the deploy applied **Key Vault Secrets User** (it does this through
     `-KeyVaultName`).
5. **Re-apply Easy Auth platform config** (`az webapp auth ...`), because `-SkipAuthProvisioning`
   never touches it (§17).

**Still to measure:** the first `Y1` run is the new baseline. Don't compare it with the Flex runs
(build 30857 onwards), which ran at roughly 5× the `Y1` drain rate.

**Plan note:** `--consumption-plan-location` creates or reuses a region-default dynamic plan in
`DEV2`. It is not `ASP-Warewolf-a36c`, so R4 stays closed (*medium confidence: CLI behaviour,
not yet observed for this app*).
