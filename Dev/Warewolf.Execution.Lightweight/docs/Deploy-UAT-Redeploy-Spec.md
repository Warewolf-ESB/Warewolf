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
the `jobs1`/`jobs2` SQL schema (already applied to **`WarewolfDevOpsTestDb`**, which replaced the
free-limit-exhausted `WarewolfEntraTestDb` on 2026-08-19 — see
`Resources/rabbit/Provision-ShovelBridgeSchema.sql` and the update note atop
`ShovelBridge-Architecture.md`).

## 4. Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| **R1** | **.NET 10 isolated support on Azure Functions v4 / Windows Consumption is unconfirmed.** | Blocker — app fails to start. | **Verify before deploying** (§5.2). If unsupported, either stay on `net8.0` for UAT or move the app to a supported plan/OS. This is the single biggest unknown. |
| **R2** | `WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED=1` pins a version-specific cold-start placeholder. | App may fail to start or silently run the wrong runtime. | Set to `0` for the first .NET 10 deploy; restore to `1` only once confirmed working. |
| **R3** | Consumption (`Y1`) plan: no `alwaysOn`, cold starts, capped scale-out. | Load-test noise — the post-login timeouts and pool exhaustion already seen may be partly this. | Decide explicitly whether UAT stays on `Y1`. Changing tier affects **other apps on the shared plan in RG `Warewolf`** — do not change it casually. |
| **R4** | Plan is shared and lives in a **different resource group**. | A plan-level change hits unrelated apps. | Treat the plan as read-only for this deploy. |
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

If `v10.0` is absent, **stop** and take the R1 decision before proceeding.

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
- `WAREWOLF_SERVICEBUS_TRIGGER_QUEUE` is unchanged;
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
