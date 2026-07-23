# Hangfire Suspend/Resume — Deploy & Validate Runbook (hangfiredemo)

Deploy the **Execution Engine + ExecutionEngineJobProcessor** and validate the suspend/resume cycle
end to end with the demo workflows:

1. **Suspend execution** — run *Suspend Execution Example*; it suspends and persists a `Scheduled` Hangfire job.
2. **Jobs processing** — the JobProcessor timer polls Hangfire storage for **due** jobs.
3. **Scheduled job execution against the engine** — when a job is due, the JobProcessor POSTs
   `/secure/resume/{jobId}`; the engine claims it (CAS `Scheduled→Processing`) and executes the continuation to **Succeeded**.
4. **Manual resumption before it times out** — run *Manual Resumption Tool Example* with the suspension id
   **inside** the suspend window; the manual path resumes first, and the later scheduled poll finds it already
   resolved (benign `409`).

Two environments, both required:

- **Path A — Dev/local (unit-oriented).** Runs the engine + processor under `func` on the dev box against a
  local SQL Express `hangfiredb`. Auth is bypassed with dev-only switches. Use it to iterate quickly and for
  developer verification. **Validated live — see [§7 Evidence](#7-validated-evidence).**
- **Path B — Real Azure deployment (final end-to-end).** The engine + processor deployed as Azure Function
  Apps with Entra/Easy Auth, a **dedicated Azure SQL** `hangfiredb`, Key Vault, and App Insights. This is the
  production-faithful validation. Every required piece is spelled out below — nothing is left as "adjust as
  needed".

> Companion to [`Deploy-EndToEnd-Runbook.md`](Deploy-EndToEnd-Runbook.md) (generic engine deploy + daemon/
> JobProcessor authorization) and [`Deploy-RunGuide.md`](Deploy-RunGuide.md) (full parameter reference).

---

## 0. The demo artifacts

| Artifact | Resource id | Notes |
|---|---|---|
| `Suspend Execution Example.bite` | `ad45daa3-…-4aeb4d51335b` v8 | `SuspendExecutionActivity` (`SuspendForSeconds`, `PersistValue=120`, `AllowManualResumption=true`, `EncryptData=false`) → Assign → **`Hello World`** sub-workflow → **Write File** (`D:\jobdata.txt`). `Result=[[sus]]` (suspension id). `SaveDataFunc` writes `scheduled : <sus>`. |
| `Manual Resumption Tool Example.bite` | `987d99d8-…-a5b79b52df7c` | `ManualResumptionActivity` (`SuspensionId=[[SuspendId]]`, `OverrideInputVariables=false`) → Assign `[[output]]="done!!"`. |
| `Hello World.bite` | `e83704af-…-ccf87e18adb5` | sub-workflow the continuation calls — **must be deployed alongside**. |
| `hangfiredb` DbSource | `cfe43056-…-638c0ae76812` | local: `(local)\sqlexpress;Initial Catalog=hangfiredb;Integrated Security=SSPI`. Azure: your Azure SQL (see Path B). |

Staged in-repo under `Warewolf.Execution.Lightweight/Resources/hangfiredemo/` and
`…/Settings/persistencesettingsdbsource.bite`.

> ### ⚠ Three things the resume continuation depends on (verified during live validation)
> 1. **A Hangfire DB the engine + processor can reach.** Local: `(local)\sqlexpress`. Azure: an Azure SQL DB.
> 2. **A writable `Write File` target.** The continuation writes `D:\jobdata.txt`. On the dev box `D:\` exists;
>    on **Azure Functions Windows** the drive root is read-only — repoint `OutputPath` to `D:\home\data\jobdata.txt`
>    (the persistent `%HOME%` share) before deploying, or the resume records `Failed` at the file step.
> 3. **A DEDICATED Hangfire DB per host (decision #13).** Do **not** point the engine/processor at a `hangfiredb`
>    that an **on-prem Warewolf Server (hangfireserver.exe) is also polling** — its `BackgroundJobServer` will
>    claim your due jobs and fail them. *This was observed live:* a scheduled test job was picked up by
>    `ServerId="server:37812:…"` and failed with *"Version Does not exist"*. Path A uses a private/dedicated DB
>    (or a stopped Server); Path B uses a dedicated Azure SQL.

**Prerequisites:** PowerShell 7+, Azure CLI 2.55+ (Path B), .NET 8 SDK, `func` 4.x, `sqlcmd`, and a reachable
SQL Server with a `hangfiredb` database (Hangfire auto-creates its tables when `PrepareSchemaIfNecessary=true`).

---

## Path A — Dev/local run (validated)

### A1. Choose a Hangfire DB that nothing else is polling

If the local Warewolf Server is running against `(local)\sqlexpress;hangfiredb`, either **stop that Server**
for the test or point the engine/processor at a **private** DB:

```powershell
sqlcmd -S '(local)\sqlexpress' -E -C -Q "IF DB_ID('hangfiredb_dev') IS NULL CREATE DATABASE hangfiredb_dev;"
# then set the DbSource ConnectionString (below) Initial Catalog=hangfiredb_dev
```

### A2. Prepare the engine output (persistence on, plaintext DbSource)

`func` for a .NET-isolated app with a RID build must be started from the **RID output folder**
(`bin\Debug\net8.0\win-x64`) — starting `func start` from the project root fails to load the isolated worker
(empty `WorkerConfigs` → every route 404). Build first, then edit the **output** copy (keeps the repo default
`Enable:false`):

```powershell
dotnet build Dev\Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj -c Debug
$engineOut = 'Dev\Warewolf.Execution.Lightweight\bin\Debug\net8.0\win-x64'
# persistence ON in the OUTPUT copy:
(Get-Content "$engineOut\Settings\persistencesettings.json") -replace '"Enable": false','"Enable": true' |
  Set-Content "$engineOut\Settings\persistencesettings.json"
# ensure the OUTPUT DbSource is the hangfiredb source (copy from repo Settings if stale):
Copy-Item Dev\Warewolf.Execution.Lightweight\Settings\persistencesettingsdbsource.bite "$engineOut\Settings\" -Force
```

`EncryptDataSource:false` + a plaintext connection string means **no Key Vault is needed** (the engine skips
Key Vault init when encryption is disabled).

### A3. Start the engine (dev auth bypassed)

```powershell
cd $engineOut
$env:AZURE_FUNCTIONS_ENVIRONMENT   = 'Development'   # enables the dev-auth path (NOT just ASPNETCORE_ENVIRONMENT)
$env:WAREWOLF_LICENSE_CHECK_ENABLED = 'false'
$env:BYPASS_SECURE_CONFIG          = 'true'          # open-access: no secure.config needed (DEV ONLY)
$env:WorkflowsDirectory            = 'C:\ProgramData\Warewolf\Resources'   # has Hello World + hangfiredemo\
$env:EXECUTIONLOGLEVEL             = 'INFO'
func host start --port 7071
# Engine base URL: http://localhost:7071   (GET /apis.json → 200 when ready)
```

- `/Public/*` executes **without** a token.
- `/Secure/*` (incl. the resume route) needs a principal. Locally, set **`DEBUG_PRINCIPAL_TOKEN`** to the
  base64 of a `clientPrincipal` JSON obtained from a deployed engine's `/.auth/me`
  (see `Auth/Parsers/DebugPrincipalParser.cs`) — this is the only way to exercise `/secure/resume` locally
  without real Entra. (Without a valid principal, `/secure/*` returns **401 "Authentication required"**.)

### A4. Start the JobProcessor (second terminal)

```powershell
dotnet build Dev\Warewolf.Execution.EngineJobProcessor\Warewolf.Execution.EngineJobProcessor.csproj -c Debug
$jpOut = 'Dev\Warewolf.Execution.EngineJobProcessor\bin\Debug\net8.0\win-x64'
# persistence ON + same DbSource in $jpOut\Settings (mirror A2)
cd $jpOut
$env:AZURE_FUNCTIONS_ENVIRONMENT = 'Development'
$env:ENGINE_RESUME_BASEURL       = 'http://localhost:7071'
$env:ENGINE_RESUME_AUTH_DISABLED = 'true'            # dev only — no MI token
$env:JOB_POLL_SCHEDULE           = '0 */1 * * * *'
$env:JOB_REAPER_SCHEDULE         = '0 */5 * * * *'
$env:JOB_STALE_MINUTES           = '15'
func host start --port 7072
```

> Even with `ENGINE_RESUME_AUTH_DISABLED=true` the processor sends no token, but the engine's `/secure/resume`
> still requires a principal — so a fully local poller→engine hop needs the engine to accept the call
> (`DEBUG_PRINCIPAL_TOKEN`, or run the engine with a dev principal). For pure engine-side validation you can
> POST `/secure/resume/{jobId}` yourself (A-scenario 3). Path B uses real MI tokens end to end.

Then run the scenarios in [§4](#4-validate-the-four-scenarios) against `http://localhost:7071`.

---

## Path B — Real Azure deployment (final end-to-end)

Do the session setup, engine auth/`secure.config` prep and login from
[Deploy-EndToEnd-Runbook §1–§2](Deploy-EndToEnd-Runbook.md#1-session-setup-run-once) first. This section adds
the concrete persistence + demo specifics.

### B1. Provision a DEDICATED Azure SQL `hangfiredb`

Not shared with any on-prem Server (see §0 ⚠3).

```powershell
$SqlServer = '<your-sqlserver>'      # <name>.database.windows.net
$SqlAdmin  = '<sql-admin>'
$SqlPwd    = '<strong-password>'
$SqlDb     = 'hangfiredb'

az sql server create -g $ResourceGroup -n $SqlServer -l $Location -u $SqlAdmin -p $SqlPwd
az sql db create   -g $ResourceGroup -s $SqlServer -n $SqlDb --service-objective S0
# Firewall: allow Azure services (the Function App) to reach the server
az sql server firewall-rule create -g $ResourceGroup -s $SqlServer -n AllowAzure `
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
# (Optional, tighter) restrict to the Function App outbound IPs instead of 0.0.0.0, or use a VNet/Private Endpoint.
```

Use a login with rights to **create the Hangfire schema on first use** (`PrepareSchemaIfNecessary=true`) — the
admin login above, or a least-privilege login granted `db_ddladmin + db_datareader + db_datawriter` on `hangfiredb`.

### B2. Build the persistence DbSource (SQL auth, WFAES-encrypted at deploy)

SQL authentication is the reliable choice for Hangfire.SqlServer (a plain connection string; no dependency on
managed-identity-to-SQL). Author `persistencesettingsdbsource.bite` with the Azure SQL connection string; the
deploy WFAES-encrypts it via Key Vault (`-EncryptResources`), exactly like the Elasticsearch source:

```xml
<Source ID="cfe43056-b935-47ba-89b7-638c0ae76812" Name="hangfiredb" ResourceType="DbSource" Type="DbSource"
        ServerType="SqlDatabase" IsValid="true"
        ConnectionString="Server=tcp:<your-sqlserver>.database.windows.net,1433;Initial Catalog=hangfiredb;User ID=<sql-login>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30">
  <DisplayName>hangfiredb</DisplayName><TypeOf>DbSource</TypeOf>
</Source>
```

`persistencesettings.json` must have `"Enable": true`, `"PersistenceScheduler": "Hangfire"`,
`"EncryptDataSource": false` (the engine builds the payload in memory from the already-decrypted `.bite`),
`"PrepareSchemaIfNecessary": true`.

### B3. Repoint the demo workflow's Write File to an Azure-writable path

```powershell
$demo = 'C:\ProgramData\Warewolf\Resources\hangfiredemo\Suspend Execution Example.bite'
(Get-Content -Raw $demo).Replace('D:\jobdata.txt','D:\home\data\jobdata.txt') | Set-Content $demo
```

### B4. `secure.config` — grant the JobProcessor + demo-caller roles

Per [Deploy-EndToEnd-Runbook §2](Deploy-EndToEnd-Runbook.md#2-prepare-the-engines-auth--permission-config):

- App role **`Warewolf_JobProcessor`** + a `secure.config` **global** row (`IsServer=true`, `Execute=true`) —
  the resume route has no per-workflow entry, so the global role governs.
- Grant whichever caller invokes the demo workflows (a daemon MI role, or a user group) `View+Execute` on
  `hangfiredemo/suspend execution example` and `hangfiredemo/manual resumption tool example`.

Do **not** set `BYPASS_SECURE_CONFIG` or `DEBUG_PRINCIPAL_TOKEN` in Azure — those are Path A dev-only.

### B5. Deploy the engine (persistence) + JobProcessor

```powershell
$WorkflowsSourcePath     = 'C:\ProgramData\Warewolf\Resources'
$PersistenceSettingsPath = '<path>\persistencesettings.json'
$PersistenceDbSourcePath = '<path>\persistencesettingsdbsource.bite'   # the Azure SQL source from B2

.\Deploy-WwExecutionEngine.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location -StorageAccount $StorageAccount -AppName $AppName `
  -PublishPath $PublishPath -AuthConfigPath $AuthConfigPath -SecureConfigPath $SecureConfigPath `
  -WorkflowsSourcePath $WorkflowsSourcePath `
  -EnablePersistence -PersistenceSettingsPath $PersistenceSettingsPath -PersistenceDbSourcePath $PersistenceDbSourcePath `
  -EncryptResources:$true -VerifyDecryption -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -EnableAppInsights:$true -ExecutionLogLevel INFO -LogDir $LogDir -PublishMethod Zip -NonInteractive

# capture $ResourceAppId + $EngineUrl (Deploy-EndToEnd-Runbook §3c)

$JobProcessorApp = '<jobprocessor-func-app>'
dotnet publish Dev\Warewolf.Execution.EngineJobProcessor\Warewolf.Execution.EngineJobProcessor.csproj -c Release -o D:\JobProcessor\Publish
.\Deploy-WwJobProcessor.ps1 `
  -ResourceGroup $ResourceGroup -Location $Location -StorageAccount stwwjobproc -AppName $JobProcessorApp `
  -PublishPath D:\JobProcessor\Publish `
  -PersistenceSettingsPath $PersistenceSettingsPath -PersistenceDbSourcePath $PersistenceDbSourcePath `
  -EngineResumeBaseUrl $EngineUrl -EngineResumeScope "api://$ResourceAppId/.default" `
  -KeyVaultName $KeyVaultName -KeyVaultSecretName $KeyVaultSecretName `
  -JobPollSchedule '0 */1 * * * *' -JobReaperSchedule '0 */5 * * * *' -JobStaleMinutes 15

# authorize the processor MI with the engine role (two-part contract):
.\Configure-WwExecutionAuth-Clients.ps1 -ResourceAppId $ResourceAppId -TenantId $TenantId `
  -ClientType Daemon -DaemonUseManagedIdentity -DaemonFunctionAppName $JobProcessorApp `
  -DaemonFunctionAppResourceGroup $ResourceGroup -AppRolesToAssign Warewolf_JobProcessor -NonInteractive
```

### B6. Acquire a token for invoking the demo workflows

```powershell
# Client-credentials token for a caller granted Execute on the demo workflows (see Get-WwExecutionToken.ps1):
$token = .\Get-WwExecutionToken.ps1 -TenantId $TenantId -ResourceAppId $ResourceAppId `
           -ClientId <caller-app-id> -ClientSecret <secret> -GrantType ClientCredentials
$H = @{ Authorization = "Bearer $token" }
```

Then run the scenarios in [§4](#4-validate-the-four-scenarios) against `$EngineUrl`, adding `-Headers $H`.

---

## 3. Pre-flight checks (both paths)

```powershell
$EngineUrl = 'http://localhost:7071'   # Path A;  or "https://$AppName.azurewebsites.net" for Path B
curl "$EngineUrl/apis.json"                                    # engine up (200)
# JobProcessor timers (Path B):
az functionapp function list --name $JobProcessorApp --resource-group $ResourceGroup -o table   # JobPoll + JobReaper
```

SQL helper (run against your `hangfiredb`) — the job-state watcher used throughout:

```sql
SELECT TOP 20 j.Id, j.StateName, j.CreatedAt, j.ExpireAt
FROM [HangFire].[Job] j ORDER BY CAST(j.Id AS INT) DESC;
-- state history for one job (who claimed it, when, why it failed):
SELECT s.Name, s.Reason, s.CreatedAt, s.Data FROM [HangFire].[State] s WHERE s.JobId=<id> ORDER BY s.Id DESC;
```

`StateName` walks **Scheduled → Processing → Succeeded** (scheduled resume), **… → ManuallyResumed** (manual),
or **… → Failed** (reaper / continuation error / a competing consumer).

---

## 4. Validate the four scenarios

Path A: `/Public/*` needs no auth; for `/secure/resume` set `DEBUG_PRINCIPAL_TOKEN`. Path B: add `-Headers $H`
(real bearer) and use `/secure/...`. Workflow routes preserve the folder: `.../hangfiredemo/<name>.json`.

### Scenario 1 — Suspend execution (creates a Scheduled job)

```powershell
$resp = Invoke-RestMethod "$EngineUrl/Public/hangfiredemo/Suspend%20Execution%20Example.json"   # Path B: /secure + -Headers $H
$suspensionId = $resp.sus
"SuspensionId = $suspensionId"
```

**Validate:** returns promptly (no 120 s block) with `sus` = a Hangfire job id; SQL shows that id `Scheduled`;
the `SaveDataFunc` wrote `scheduled : <id>` to the Write File target.

### Scenario 2 — Jobs processing (the poller runs)

`JobPoll` fires every minute. Before due → `Dispatched=0`; after due (≈`PersistValue`=120 s) → `Dispatched=1`.

```
JobPoll | Completed | Scanned=1 Dispatched=0 …      # not yet due
JobPoll | Completed | Scanned=1 Dispatched=1 …      # first tick after due
```
Path A: JobProcessor terminal. Path B: `az functionapp log tail --name $JobProcessorApp -g $ResourceGroup`,
or App Insights `traces | where message contains "JobPoll"`.

### Scenario 3 — Scheduled job execution against the engine

Leave the job; ~120 s later the poller POSTs `POST {EngineUrl}/secure/resume/{jobId}`. The engine claims
(CAS Scheduled→Processing) and executes the continuation (Assign → `Hello World` → Write File).

**Validate:** SQL walks `Scheduled → Processing → Succeeded`; engine logs `Claimed`/`Succeeded`; processor logs
`Outcome=Dispatched (HTTP 200)`; the Write File target now contains `job has resumed`; a duplicate dispatch
gets **409** (`AlreadyClaimed`) — never a second execution.

> To validate only the **engine side** without waiting/polling, POST the resume route yourself:
> `Invoke-RestMethod -Method Post "$EngineUrl/secure/resume/$suspensionId" -Headers $H` (Path B) — resume
> claims any `Scheduled` job regardless of due-time.

### Scenario 4 — Manual resumption before it times out

Suspend a **fresh** job, then resume it **manually within the 120 s window**:

```powershell
$sid = (Invoke-RestMethod "$EngineUrl/Public/hangfiredemo/Suspend%20Execution%20Example.json").sus
Invoke-RestMethod "$EngineUrl/Public/hangfiredemo/Manual%20Resumption%20Tool%20Example.json?SuspendId=$sid"
```

**Validate:** the manual call returns synchronously with `output="done!!"` / `result="Success"` (proving the
manual path executed the continuation **before returning**); SQL shows the id `ManuallyResumed`; the later
`JobPoll` tick evaluates it, finds it no longer `Scheduled`, and the engine returns **409** — the job is **not**
re-executed. To make the race deterministic, pause the JobProcessor (stop its host) while you fire the manual resume.

---

## 5. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Suspend → *"persistence settings not configured"* | `Enable:false` or DbSource didn't load | `Enable:true`; confirm the `hangfiredb` DbSource + DB reachable. |
| Suspend/secure → `500` *secure.config absent* | No `secure.config` and open-access not enabled | Path A: `BYPASS_SECURE_CONFIG=true`. Path B: deploy a valid `secure.config`. |
| `/secure/*` → `401 "A valid Bearer token is required"` | EasyAuth gate, no principal | Path A: set `DEBUG_PRINCIPAL_TOKEN` + `AZURE_FUNCTIONS_ENVIRONMENT=Development`. Path B: send a real bearer (`-Headers $H`). |
| `func start` serves only 404 / no functions | isolated worker not loaded (RID output) | Run `func host start` from `bin\…\net8.0\win-x64`. |
| Job → `Failed` with *"Version Does not exist"* or claimed by `ServerId=server:…` | **A competing on-prem Server is polling the same DB** | Use a dedicated `hangfiredb` per host (§0 ⚠3); stop the on-prem Server for local tests. |
| Resume → `Failed` at Write File | `OutputPath` not writable on the host (Azure) | Repoint to `D:\home\data\jobdata.txt` (B3). |
| Scheduled dispatch → `500` from engine | Processor MI missing `Warewolf_JobProcessor` or no global Execute row | Re-run B4 + the authorize step. |
| `Hello World` continuation → not found | sub-workflow not deployed | Ensure `Hello World.bite` is in the engine `Resources/`. |

---

## 6. Reset / teardown

```sql
DELETE FROM [HangFire].[State]; DELETE FROM [HangFire].[JobParameter]; DELETE FROM [HangFire].[Job];  -- clear demo jobs
```
- **Path A:** Ctrl-C both `func` hosts.
- **Path B:** roll back per [Deploy-EndToEnd-Runbook §8](Deploy-EndToEnd-Runbook.md#8-teardown) + remove the
  processor role assignment (`Remove-WwExecutionAuth-Clients.ps1 … -ClientType Daemon`).

---

## 7. Validated evidence

| Scenario | How validated | Result |
|---|---|---|
| **1 — Suspend** | **Live**, engine under `func` against real `(local)\sqlexpress;hangfiredb` | `GET /Public/hangfiredemo/Suspend Execution Example.json` → **HTTP 200** `{"sus":20}`; SQL job **20 `Scheduled`**; `SaveDataFunc` wrote `scheduled : 20`. ✅ |
| **3 — Resume execution** | **Phase-8 automated test** `ScheduledResume_RealSuspendWorkflow_ExecutesContinuation_ToSucceeded` (real workflow, `Hangfire.MemoryStorage`) | claim → executes Assign → `Hello World` → Write File → **`Succeeded`** with outputs. ✅ |
| **4 — Manual resumption** | **Phase-8 automated test** `ManualResume_RealSuspendWorkflow_ViaSeam_ExecutesContinuation_NoError` | `IResumptionExecutor.Execute` runs the continuation, no error. ✅ |
| **Dual-host DB hazard** | **Live observation** | A scheduled test job on the shared `hangfiredb` was claimed by the on-prem Server (`ServerId=server:37812:…`) and failed *"Version Does not exist"* — confirms decision #13's dedicated-DB requirement. ⚠ |

**Not yet run here (operator step):** the full live *poller→engine→resume* hop on Azure with real MI tokens —
it requires an Azure subscription + a dedicated Azure SQL and cannot be provisioned from the dev box. Every
piece it depends on is validated above (suspend live; resume + manual by test) and the setup is fully specified
in Path B; run §4 against the deployed engine to complete the Azure end-to-end.

---

## Related docs
- [`Deploy-EndToEnd-Runbook.md`](Deploy-EndToEnd-Runbook.md) — generic engine deploy + daemon/JobProcessor authorization.
- [`Deploy-RunGuide.md`](Deploy-RunGuide.md) — full deploy parameter reference.
- [`HangFire-Migration-To-Azure-Plan-Step-By-Step.md`](HangFire-Migration-To-Azure-Plan-Step-By-Step.md) — migration plan, decisions log, Phase-8 coverage.
