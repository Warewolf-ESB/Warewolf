# Spec — full redeploy of `WarewolfServer-UAT` (.NET 8 → .NET 10)

**Status:** proposed, not yet executed
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
| App Service Plan | `ASP-Warewolf-a36c`, **SKU `Y1` (Dynamic/Consumption)** | lives in RG **`Warewolf`**, not `DEV2` |
| OS | Windows (`reserved: false`) | |
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
- `BYPASS_SECURE_CONFIG = true`, `WAREWOLF_LICENSE_CHECK_ENABLED = false`,
  `ASPNETCORE_ENVIRONMENT = Production`, `EXECUTIONLOGLEVEL = INFO`.

**Take a full backup of app settings before touching anything** (§5.1) — several are secrets and
are not reconstructible from the repo.

## 3. Scope

**In scope:** publish the current branch, upgrade the app to .NET 10, deploy, verify, and re-run
the ShovelBridge load test.

**Out of scope:** changing the App Service Plan tier (see R3 — flagged as a decision, not an
action), Entra/Easy Auth re-provisioning (already configured; use `-SkipAuthProvisioning`), and
the `jobs1`/`jobs2` SQL schema (already applied directly to `WarewolfEntraTestDb`).

## 4. Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| **R1** | **.NET 10 isolated support on Azure Functions v4 / Windows Consumption is unconfirmed.** | Blocker — app fails to start. | **Verify before deploying** (§5.2). If unsupported, either stay on `net8.0` for UAT or move the app to a supported plan/OS. This is the single biggest unknown. |
| **R2** | `WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED=1` pins a version-specific cold-start placeholder. | App may fail to start or silently run the wrong runtime. | Set to `0` for the first .NET 10 deploy; restore to `1` only once confirmed working. |
| **R3** | Consumption (`Y1`) plan: no `alwaysOn`, cold starts, capped scale-out. | Load-test noise — the post-login timeouts and pool exhaustion already seen may be partly this. | Decide explicitly whether UAT stays on `Y1`. Changing tier affects **other apps on the shared plan in RG `Warewolf`** — do not change it casually. |
| **R4** | Plan is shared and lives in a **different resource group**. | A plan-level change hits unrelated apps. | Treat the plan as read-only for this deploy. |
| **R5** | `httpsOnly: false`. | Security weakness, pre-existing. | Flag to the team; do not silently change it as part of this deploy — it may be load-bearing for an existing caller. |
| **R6** | Deploy replaces app settings if the orchestrator is run without care. | Broken auth / wrong queue / lost secrets. | Back up settings first; diff after deploy (§6.5). |
| **R7** | Workflow resources must include `RabbitProcess.bite` + `RabbitProcess2.bite` and the DB source `b9184f70-…`. | Load test fails at step 1. | Confirm staged resources in the plan phase (§6.3). |
| **R8** | DPAPI-encrypted `.bite` sources cannot travel between machines. | Source fails to decrypt. | The bundled `NewSqlServerSource (Local Backup)` (`d3f6a2e1-…`) is **unreferenced** by both workflows — exclude it. Both bind to `b9184f70-…`. |

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

If `v10.0` is absent, **stop** and take the R1 decision before proceeding.

### 5.3 Build

```powershell
.\Compile.ps1 -ServerTests
dotnet publish Dev\Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj -c Release -o D:\ExecutionEngine\Publish
```

Publish path must not collide with JobProcessor/ServiceBusWorker publish dirs — the orchestrator
fails at plan time if they do.

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
    -ResourceGroup  'DEV2' `
    -Location       'eastus' `
    -StorageAccount '<UAT storage account>' `
    -AppName        'WarewolfServer-UAT' `
    -PublishPath    'D:\ExecutionEngine\Publish' `
    -SkipAuthProvisioning `
    -LicenseCheckEnabled:$false `
    -DryRun
```

`-StorageAccount` must be read from the backup (`AzureWebJobsStorage`), not guessed.

### 6.3 Review the PLAN output (Phase 0.5)

The orchestrator resolves every decision and prints a masked summary **before** changing
anything. Confirm specifically:

- targeting is `WarewolfServer-UAT` / `DEV2` — **not** `warewolfserver`;
- `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE` is unchanged;
- staged workflow resources include `RabbitProcess.bite` and `RabbitProcess2.bite`;
- the DB source `b9184f70-…` is staged and is **not** DPAPI-encrypted;
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
3. **New code is live** — this is the whole point of the redeploy. Run a **single**-message
   `Test-ShovelBridgeE2E.ps1 -VerifyWorkflowExecution` first, then confirm in App Insights:

   ```kusto
   traces
   | where timestamp > ago(15m)
   | where message contains "DB proc result"
   | project timestamp, message
   ```

   The `DB proc result | Proc=… | Rows=… | FirstValue=…` line only exists in the new build. If it
   is absent, the deploy did not take effect and there is no point running the load test.
4. **Load test:** re-run `ShovelBridgeLoadTest_ExternalServiceBus` (1000 messages).

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

- **Automate UAT deploys.** The absence of any pipeline or documented procedure is the root cause
  of this entire detour. Either add a deploy stage to `pipeline-LOADTEST.yml` or make UAT a
  target of `pipeline-CLOUD.yml`.
- **Version stamping.** There is no reliable way to tell which build UAT is running. Surface an
  app version in a health endpoint and log it at startup.
- **`jobs1`/`jobs2` provisioning script.** The schema exists only as live database state; that is
  why `jobs2`'s absence went unnoticed until 2026-08-13.
- **R5** (`httpsOnly: false`) and **R3** (Consumption plan for a 1000-message load test) are
  standing decisions for the team.

## 11. Docs to update after execution

Per `CLAUDE.md` change-synchronisation:

- `Scripts/README.md` — note UAT as a deploy target.
- `docs/Deploy-RunGuide.md` / `docs/Deploy-EndToEnd-Runbook.md` — add the UAT flow.
- `docs/ShovelBridge-Architecture.md` — record that the load test depends on a manually deployed
  engine, and the outcome of the redeploy.
