# `Scripts/` — wwexecution provisioning toolkit

This folder is the operational source of truth for everything the
`Warewolf.Execution.Lightweight` function app needs at the Azure / Entra
control plane.

| Script                                        | Purpose                                                                 |
|-----------------------------------------------|-------------------------------------------------------------------------|
| `Deploy-WwExecutionEngine.ps1`                | **End-to-end deployment orchestrator** — runs every step in `docs/Deployment-Steps.txt` (RG → storage → Function App → App Insights → Entra/Easy Auth → publish, with optional Key Vault setup + resource encryption). Thin: reuses the scripts below. |
| `Deploy-WwExecutionEngine.authconfig.example.json` | Template for the orchestrator's `-AuthConfigPath` (GroupPermissions + UserAssignments). Each GroupPermissions key becomes an MI-assignable app role; the `Warewolf_ClientApps` entry is the dedicated role for app-only client apps (daemon/MI callers get it via `Configure-WwExecutionAuth-Clients.ps1 -AppRolesToAssign`, and `secure.config` must grant the matching `WindowsGroup` Execute on the workflows they call). |
| `Deploy-WwJobProcessor.ps1`                   | **ExecutionEngineJobProcessor deploy orchestrator** — provisions the poller/reaper Function App (RG → storage → Function App + system-assigned MI → App Insights → Key Vault wiring → stage + WFAES-encrypt the persistence settings pair → app settings → publish). Standalone, or invoked by `Deploy-WwExecutionEngine.ps1 -DeployJobProcessor`. Same *params-first, prompt-if-missing*, `-DryRun`, masked-summary + transcript conventions. The persistence source files (`persistencesettings.json`, `persistencesettingsdbsource.bite`) are **prompted when not passed**. Role assignment (`Warewolf_JobProcessor`) is a separate step via `Configure-WwExecutionAuth-Clients.ps1` — see the runbook. |
| `Deploy-WwQueueProcessor.ps1`                 | **RabbitMQ QueueProcessor deployment** — Azure Container Apps, **one app per queue-trigger**, autoscaled 0→N by the KEDA `rabbitmq` scaler. Pointed at a trigger file, a folder of trigger files, or a manifest; derives `maxReplicas` from the trigger's `Concurrency` and the KEDA target from its `Prefetch`. Replaces `N × QueueWorker.exe` for the Azure path (on-prem unchanged). |
| `Rollback-WwExecutionEngine.ps1`              | **Teardown companion** — deletes ONLY what a deploy run created (summary-/tag-driven), in dependency order, with a leak check. Existing resources are preserved. |
| `New-WwE2EStaging.ps1`                        | **E2E harness — staging.** Builds a disposable staging tree (settings, generated triggers + broker source, optional `dotnet publish`) from repo templates, stamps a unique run suffix, validates readiness and emits `staging-manifest.json`. Only the Warewolf licence and broker credentials are hand-supplied; the generated source is **plaintext** so DPAPI never blocks the Linux worker. See [`docs/E2E-Harness-README.md`](../docs/E2E-Harness-README.md). |
| `Invoke-WwE2EVerification.ps1`                | **E2E harness — orchestrator.** Consumes the manifest, then deploys engine + workers, pre-creates broker topology, publishes real messages, watches KEDA scale 0→N→0, runs the drain and unacked-visibility probes, and **scores 18 acceptance criteria** to JSON + markdown. `-DryRun` by default (`-Execute` to deploy); teardown is opt-in via `-TeardownWhenDone`. Exit code 0 = all criteria passed. |
| `WwE2E.Common.psm1`                           | Shared helpers for the harness — AMQP topology/depth (with the `System.Threading.RateLimiting` shared-framework load), client-credentials engine tokens, Log Analytics evidence queries, and an `az` wrapper that **refuses `--query`** because cmd.exe mangles JMESPath. Sets `AZURE_EXTENSION_USE_DYNAMIC_INSTALL=yes_without_prompt` **at module load**: `az monitor log-analytics query` lives in an extension, and without this az stops to ask permission to install it — on stdin, with stderr suppressed — which hung a reviewer's run for 30+ minutes at the last step. |
| `Invoke-WwQueueLoadTest.ps1`                  | **Queue load test — the reviewer entry point.** `-MessageCount` sizes the run. Verifies Azure login, then Engine / ACA / KEDA / broker / database and **stops on blockers** before publishing (a competing consumer, a KEDA rule on the wrong queue, a stopped engine — each of which otherwise produces a result that looks like a product defect). Takes a `MAX(JobLogId)` watermark instead of truncating, pre-warms, publishes, waits for **rows** to stop rising rather than for the queue to empty, then reconciles the manifest against worker logs, engine logs, the database and the DLQ into **four buckets** that must sum to the published count. Exit code carries the verdict. Phase 0 verifies the required **az extensions** (`containerapp`, `log-analytics`, `application-insights`) and names the step each missing one blocks; every run writes a **transcript** to `<OutputDir>\<runLabel>.log` started before Phase 0, and `-StepTimeoutSeconds` bounds the report step so a blocked call fails the run rather than hanging it. Every default reproduces RUN 2 (100/100, 0 dead-lettered). See [`docs/LoadTest-Guide.md`](../docs/LoadTest-Guide.md). |
| `WwLoadTest.Defaults.psd1`                    | RUN 2 defaults for the load test, pre-filled at every prompt. Loaded with `Import-PowerShellDataFile` (parsed as **data**, so code slipped into it fails to load rather than running). **Contains nothing secret** — the broker password comes from the source `.bite` and the SQL connection string from `-SqlConnectionString` / `$env:WWLOADTEST_SQLCONNECTION` / a masked prompt. |
| `Publish-WwQueueBurst.ps1`                    | Publishes a burst and writes the manifest that turns the run report from a *count* into a *reconciliation*. Stamps a unique AMQP `CorrelationId` per message — the only key present on **every** worker log line including dead-letter lines, and the only handle on a deliberate-failure message whose body is empty by design. Builds and validates the manifest **before** connecting, and writes only what was actually published. |
| `Invoke-WwEnginePreWarm.ps1`                  | Two-phase warm-up for a Consumption-plan engine: sequential until latency settles (measured cold start **64,757 ms** → ~3,100 ms warm), then at the target concurrency to force scale-out. Every 502/503/504 in RUN 1 came from that cold window. **Executes the real workflow**, so take the row watermark after it, not before. |
| `Get-WwQueueRunReport.ps1`                    | **Per-message run report** — read-only and **time-window driven** (`-LastMinutes`, or `-StartUtc`/`-EndUtc`), so it re-renders any past burst still inside workspace retention rather than needing to watch one live. Reconstructs each delivery from `ContainerAppConsoleLogs_CL` into `txn → replica → revision → startedUtc → durationMs → outcome`, plus replica distribution, per-queue percentiles, reliability counts and a CSV + JSON. With `-ExpectedManifest` it **reconciles** against what was published (missing / duplicated / wrong-outcome) instead of merely counting what appears in the logs. `-IncludeEngine` joins to the engine's App Insights on `ExecutionId`. |
| `Tests/Invoke-WwQueueLoadTest.Tests.ps1`      | Pester 5 suite for the load test's decision logic via `-LoadFunctionsOnly` — concurrency arithmetic, four-bucket classification, verdict rules, pre-flight blockers, connection-string masking, percentiles. Includes four regressions: **numeric parameters must be nullable** (a bare `[int]` defaults to `0`, indistinguishable from an explicit `0`, so every unbound int silently overrode its default with zero); **a run that published nothing must FAIL** (every "no failures" condition is vacuously true on an empty run); **no `[datetime]::TryParse` with a `[ref]` on an untyped variable** (PowerShell cannot bind it — asserted against the AST, since the helper's doc comment names the broken pattern deliberately); **warm-up traffic must be excluded** (two watermarks, the reported pre-warm row count, the report-window clamp, and no row deletion anywhere); **a failure cause must be diagnosed, never assumed** (`Resolve-E2EBrokerUri` returns `$null` for six reasons — a mistyped filename was being blamed on WFAES encryption, sending the reader after a Key Vault problem that did not exist); **reconciliation must be scoped to this run's label** (a shared `-OutputDir` merged two runs' manifests and reported a flawless 1000/1000 run as `FAIL — 20 LOST`); and **`TRY_CONVERT` style 127 requires the `T` separator** (a space-separated timestamp column silently yielded NULL, blanking every latency figure). Runs offline. |
| `Tests/Publish-WwQueueBurst.Tests.ps1`        | Pester 5 suite for the manifest invariants the reconciliation depends on — unique transaction ids across a 100-message burst, distinct bodies (identical ones make duplicate *execution* invisible in the database), empty failure bodies, and the parameter guards. Runs offline. |
| `Tests/Deploy-WwExecutionEngine.Tests.ps1`    | Pester 5 suite for the orchestrator (helpers + DryRun end-to-end). Run: `Invoke-Pester -Path ./Tests/Deploy-WwExecutionEngine.Tests.ps1`. |
| `Tests/Deploy-WwJobProcessor.Tests.ps1`       | Pester 5 suite for the JobProcessor orchestrator (static/ValidateSet, helpers via `-LoadFunctionsOnly`, DryRun end-to-end with az-shim, persistence-pair staging + fail-loud prompt validation). Run: `Invoke-Pester -Path ./Tests/Deploy-WwJobProcessor.Tests.ps1`. |
| `Tests/Rollback-WwExecutionEngine.Tests.ps1`  | Pester 5 suite for the rollback script (ownership resolver + DryRun teardown). |
| `Tests/Configure-WwExecutionAuth.Tests.ps1`   | Pester 5 suite for the auth script's helpers (e.g. `Resolve-AssignmentUser` Stage 6 guard) via `-LoadFunctionsOnly`. |
| `Configure-WwExecutionAuth.ps1`               | End-to-end Entra + Easy Auth + secure-config provisioning (idempotent). |
| `Configure-WwExecutionAuth-Clients.ps1`       | Provisions client app registrations by **type** (SPA / Confidential / Daemon / Console). The **Daemon / Managed Identity** path can enable a client Function App's system-assigned MI (`-DaemonFunctionAppName`/`-DaemonFunctionAppResourceGroup`) or bind an existing SP (`-ManagedIdentityObjectId`), and defaults `-AppRolesToAssign` to `Warewolf_ClientApps`. |
| `Configure-WwExecutionAuth-ClientApps.ps1`    | **Orchestrator** — one registration per client-**example** app (Angular, React, WebMvc, Console, AzureFunction, ServiceBus); prompts, provisions via the by-type script, validates access (`/secure/{workflow}.json`), and writes a masked summary + transcript. See [`docs/KB-ClientApps-Configuration.md`](../docs/KB-ClientApps-Configuration.md). |
| `Tests/Configure-WwExecutionAuth-ClientApps.Tests.ps1` | Pester 5 suite for the orchestrator's pure helpers (`Get-ClientAppDefinitions`, `Get-ValidationPlan`, `Format-ClientAppSummary`) via `-LoadFunctionsOnly`. |
| `Configure-WwExecutionAuth-debug.ps1`         | Debug variant with extra diagnostic dumps.                              |
| `Configure-WwExecutionAuth-local.ps1` etc.    | Local dev wrappers used by individual contributors.                     |
| `Cleanup-WwExecutionAuth.ps1`                 | Tear down all artifacts created by the configure script.                |
| `Encrypt-Config.ps1`                          | Encrypts `secure.config` plaintext into the deployable form, and converts `.bite` sources from DPAPI/plaintext to **WFAES** (AES-256-GCM, key in Key Vault). Two modes: **attribute** (default — rewrites the `ConnectionString` of a `<Source>` element) and **`-WholeFile`** (encrypts the entire file; required for **queue-trigger JSON**, which has no `<Source>` element and is otherwise *silently skipped*). `-VerifyOnly` round-trips in memory without writing. |
| `Tests/Encrypt-Config.Tests.ps1`              | Pester 5 suite for `Encrypt-Config.ps1` — `-WholeFile` round-trip, idempotency, tamper detection (AES-GCM), attribute-mode regression, and the single-file-folder StrictMode fix. Runs offline (`az` is shadowed). |
| `Setup-EntraAuth.ps1`                         | Lightweight subset for Entra app + role provisioning only.              |
| `KeyVaultSetup.ps1`, `KeyVaultSetup.azcli`    | One-time KV bootstrap for SecretKey storage.                            |
| `Generate-WorkflowIndex.ps1`                  | Builds workflow discovery index used by `apis.json` route.              |
| `secure.config.example.json`                  | Documented plaintext shape (CFG-04, CFG-05).                            |
| `secure.config.schema.json`                   | JSON Schema used to validate `secure.config` in CI (CFG-07).            |
| `authsettingsV2.json`                         | Reference Easy Auth V2 settings template.                               |

---

## End-to-end deployment (`Deploy-WwExecutionEngine.ps1`)

A single orchestrator that runs the whole of `docs/Deployment-Steps.txt`, reusing
`Setup-ApplicationInsights.ps1`, `Configure-WwExecutionAuth.ps1`,
`Encrypt-Config.ps1` and `Generate-WorkflowIndex.ps1` (Phase 3.6 — writes
`Resources\workflow-index.json` into the staged package so it ships inside the
deploy zip). Inputs follow a *params-first, prompt-if-missing* model and
the script resolves **every** decision, prints a masked summary, and asks once to
proceed **before** any change. It does **not** build the project — publish once
yourself and point `-PublishPath` at that folder (or a `.zip` of it).

> **Breaking change:** `-ResourceGroup`, `-Location`, `-StorageAccount`,
> `-AppName` and `-PublishPath` no longer have defaults (and `-KeyVaultSecretName`
> is required when encrypting). Non-interactive callers must pass them explicitly.

```powershell
az login

# 1) Publish once (outside the orchestrator)
dotnet publish ..\Warewolf.Execution.Lightweight.csproj -c Release -o D:\ExecutionEngine\Publish

# 2) Dry-run the full pipeline (prints every action, mutates nothing)
./Deploy-WwExecutionEngine.ps1 -ResourceGroup DEV2 -Location southafricanorth `
    -StorageAccount stwwenginetest -AppName wwenginetest `
    -PublishPath D:\ExecutionEngine\Publish -DryRun

# 3) Non-interactive deploy (folder or .zip publish source)
./Deploy-WwExecutionEngine.ps1 `
    -ResourceGroup DEV2 -Location southafricanorth `
    -StorageAccount stwwenginetest -AppName wwenginetest `
    -PublishPath D:\ExecutionEngine\Publish `
    -AuthConfigPath ./Deploy-WwExecutionEngine.authconfig.json `
    -SecureConfigPath 'C:\cfg\secure.config' `
    -LicenseConfigPath 'C:\cfg\Warewolf License.secureconfig' `
    -WorkflowsSourcePath 'C:\Warewolf\Resources' `
    -EncryptResources:$true -VerifyDecryption -KeyVaultName kv-wwengine-test -KeyVaultSecretName wwengine-aes-key `
    -EnableAppInsights:$true `
    -EnableElasticsearch:$true -ElasticsearchSourcePath 'C:\cfg\ElasticsearchLoggingSource.bite' `
    -NonInteractive
```
> First-time deploy shown (encrypt once). On later deploys drop `-EncryptResources`
> + `-VerifyDecryption` (sources already encrypted, staged as-is) but keep
> `-KeyVaultName`/`-KeyVaultSecretName` so the engine decrypts them at runtime.

Highlights:

- **Publish source** — `-PublishPath` accepts a folder or a `.zip`. No `dotnet publish`
  is run by the script. The publish output is **never modified**: every run copies (or
  extracts) it into a **fresh staging dir under the OS temp path**
  (`wwexecutionengine-stage-<AppName>-<stamp>`), prepares the package there, zips **that**,
  uploads it, then removes the staging dir on a successful real run. This keeps each
  zip clean and lets the engine and the JobProcessor stage in **separate** directories.
- **secure.config** — an already-AES-encrypted file is validated (must be engine-
  decryptable) and staged as-is; a plaintext-JSON file is validated then AES-encrypted
  automatically; an undecryptable file is a hard error.
- **License** — `-LicenseConfigPath` is prompted (Enter to skip) and, when supplied,
  copied to the publish root as `Warewolf License.secureconfig`. Optional: if omitted,
  the engine's license check (default on) may fail at startup.
- **Encryption (optional, default OFF)** — `-EncryptResources` is a **single** switch
  for **all** sources (workflows + Elasticsearch + others): it converts `.bite`
  connection strings to WFAES via Key Vault. **Encrypt once** — on later deploys leave
  it off and sources are staged as-is. `-VerifyDecryption` (default OFF) adds an
  optional **in-memory** round-trip decrypt (no plaintext on disk). Whenever sources
  are WFAES-encrypted you must still pass `-KeyVaultName`/`-KeyVaultSecretName` on every
  deploy so the engine's managed identity can decrypt them at runtime (the dev
  *Secrets Officer* role + key generation are added only when `-EncryptResources` is set).
- **Environment variables** — feature toggles drive the engine's app settings.
  Console logging and Application Insights default **on**; Elasticsearch logging
  defaults **off** (enabling it requires `-ElasticsearchSourcePath`; Key Vault only if
  the source is WFAES-encrypted).
  `ASPNETCORE_ENVIRONMENT` is fixed to `Production`. `BYPASS_SECURE_CONFIG`,
  `WAREWOLF_SUPER_ADMIN_ENABLED` and `SkipFailureToRetrieveSecret` are **not set at
  all** (not in logs/summary, not on the app) — the engine defaults them to
  disabled when absent; an admin adds one manually only if ever needed. App
  Insights' `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` (not the standard name) is
  **auto-read** from the App Insights resource, never prompted. See the env-var
  matrix in `docs/Deployment-Steps.txt`.
- **Dry-run parity** — `-DryRun` produces the **same** outputs as a real run except
  it creates/uploads nothing in Azure: files are prepared into a temp staging dir
  (`wwexecutionengine-stage-<AppName>-<stamp>-dryrun`, leaving your publish output
  untouched) that is **kept** for inspection, and a transcript + `*.dryrun.summary.json`
  (with `"dryRun": true`) are written so the rollback can be exercised from a dry-run
  summary. Encryption runs only when the Key Vault key is reachable, else it's staged
  unencrypted and deferred.
- **Logging** — `-ExecutionLogLevel` is prompted when interactive and sets `EXECUTIONLOGLEVEL`,
  which drives the engine's console + App Insights logging **in code** (the isolated worker
  does not read host.json). Rewriting the published host.json logLevel is **opt-in** via
  `-AlignHostJsonLogLevel` (only tunes the Functions host process). A timestamped transcript +
  masked `*.summary.json` are written under `-LogDir` for **both** dry-run and real runs;
  secret values are redacted everywhere.
- **Crash-safe summary** — the `*.summary.json` is written *before* the first mutating
  action and re-written after every phase and from the failure handler, so it **always
  exists** no matter where a run stops. It carries `status` (`in-progress`/`completed`/
  `failed`), `lastPhase`, and `error`. The `created` map records creation **intent**
  (set before each create call), so a deploy that fails mid-run (e.g. Phase 4) can still
  be rolled back directly from its summary.
- **Deploy** — `az` zip-deploy (config-zip) by default (Auto/Zip) — the correct
  method for the pre-built publish artifact. `func azure functionapp publish` is
  opt-in only (`-PublishMethod Func`); it expects a project source dir and fails on
  a pre-built package, so it is **not** used automatically.
- **Teardown** — every created resource is tagged `wwx-test-run=<runId>` and
  recorded in `summary.json`'s `created` map. `Rollback-WwExecutionEngine.ps1
  -SummaryPath <summary.json>` deletes **only** those (auth → Key Vault delete+purge
  → App Insights → Function App → storage), preserves pre-existing resources, never
  deletes the resource group unless the run created it **and** `-DeleteResourceGroup`
  is passed, and finishes with a leak check. Dry-run it first. A **partial/failed**
  deploy summary (`status = failed`/`in-progress`) is accepted too — rollback notes the
  partial run, then cleans only the recorded created-map (absent/never-created resources
  are skipped, pre-existing ones preserved).

**Tests** (Pester 5):

```powershell
Install-Module Pester -MinimumVersion 5.0 -Scope CurrentUser   # one-time
Invoke-Pester -Path ./Tests                                    # all suites
Invoke-Pester -Path ./Tests/Deploy-WwExecutionEngine.Tests.ps1
```

---

## ExecutionEngineJobProcessor deployment (`Deploy-WwJobProcessor.ps1`)

The dedicated poller/reaper Function App (`Warewolf.Execution.EngineJobProcessor`) that
replaces `hangfireserver.exe`: it polls Hangfire SQL storage for due `Scheduled`
suspend/resume jobs and fire-and-forget POSTs them to the engine's
`/secure/resume/{jobId}` route (managed-identity bearer token), and reaps stale
`Processing` jobs to `Failed` (fail-only).

Publish first, then deploy (the script does **not** build):

```powershell
dotnet publish Dev/Warewolf.Execution.EngineJobProcessor/Warewolf.Execution.EngineJobProcessor.csproj -c Release -o D:\JobProcessor\Publish

./Deploy-WwJobProcessor.ps1 `
  -ResourceGroup   DEV2 `
  -Location        southafricanorth `
  -StorageAccount  stwwjobproc `
  -AppName         wwjobproc `
  -PublishPath     D:\JobProcessor\Publish `
  -EngineResumeBaseUrl https://wwengine.azurewebsites.net `
  -EngineResumeScope   api://<engine-app-id>/.default `
  -KeyVaultName    kv-warewolf `
  -KeyVaultSecretName dp-keyring-v1
  # -PersistenceSettingsPath / -PersistenceDbSourcePath are PROMPTED if omitted
```

Highlights:

- **Same conventions as the engine deploy** — `Write-Phase` banners, `Invoke-Az`
  wrapper, secret masking, `-DryRun` (produces the same logs + `*.summary.json` with a
  `dryRun: true` flag), and a *params-first, prompt-if-missing* model that resolves
  everything, prints a masked plan, and asks once before any change.
- **Persistence pair, prompted** — `persistencesettings.json` (staged as-is) and
  `persistencesettingsdbsource.bite` (ConnectionString WFAES-encrypted with the engine's
  Key Vault key when `-EncryptResources`, exactly like the Elasticsearch source) are
  prompted when not passed, and validated to their exact filenames.
- **Own publish output + clean zip** — the processor is a **different** Function App
  built from a **different** csproj, so it must be published to a **separate directory**
  from the engine. Like the engine, it never mutates the publish output: it stages into
  a fresh temp dir (`wwjobprocessor-stage-<AppName>-<stamp>`), zips that, uploads it, and
  removes the staging dir on a successful real run — so the engine and processor prepare
  and upload their zips independently.
- **System-assigned MI** is always enabled (Key Vault decrypt + engine token).
- **App settings** applied: `JOB_POLL_SCHEDULE`, `JOB_REAPER_SCHEDULE`,
  `JOB_STALE_MINUTES`, `ENGINE_RESUME_BASEURL`, `ENGINE_RESUME_SCOPE`,
  `ENGINE_RESUME_TIMEOUT_SECONDS`, `ENGINE_RESUME_AUTH_DISABLED`, plus
  `AZURE_KEYVAULT_NAME` / `KEYVAULT_SECRET_NAME` when a vault is in play.
- **Companion mode** — `Deploy-WwExecutionEngine.ps1 -DeployJobProcessor` runs this
  script after the engine deploy, passing the shared context (subscription/tenant/RG/
  location/Key Vault/persistence pair + the engine's own URL as `-EngineResumeBaseUrl`);
  the child prompts for anything not supplied (its own `AppName`/`PublishPath`/`StorageAccount`).
  The engine resolves `-JobProcessorPublishPath` at **plan time** and **fails loudly** if
  it is the **same directory** as the engine's `-PublishPath` (it would otherwise zip the
  engine's binaries into the processor app) — publish the two projects to separate folders.

**Authorization is a separate operator step** (the two-part contract): grant this app's
managed identity the engine app role `Warewolf_JobProcessor` and add the matching
global-scope `Execute` row to the engine's `secure.config`. See
[`docs/Deploy-EndToEnd-Runbook.md`](../docs/Deploy-EndToEnd-Runbook.md) (JobProcessor section).

Tested by `Tests/Deploy-WwJobProcessor.Tests.ps1` (Pester 5) following the same
`-LoadFunctionsOnly` + az-shim / `-DryRun` conventions as the engine suite.

---

## RabbitMQ QueueProcessor deployment (`Deploy-WwQueueProcessor.ps1`)

The Linux **container** worker (`Warewolf.Execution.QueueProcessor`) that replaces
`N × QueueWorker.exe` for the Azure path: it consumes one RabbitMQ queue trigger and POSTs
the mapped message to the engine's `/Secure/{workflow}.json` with a managed-identity token.
Hosted on **Azure Container Apps**, autoscaled **0 → N replicas** by the KEDA `rabbitmq`
scaler, **one Container App per queue-trigger**. The on-prem Server +
`QueueWorker.exe` path is untouched.

Publish first, then deploy (the script does **not** build the .NET project; it does build the
container image via `az acr build`):

```powershell
dotnet publish Dev\Warewolf.Execution.QueueProcessor\Warewolf.Execution.QueueProcessor.csproj -c Release -o D:\QueueProcessor\Publish

./Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup   DEV2 `
  -Location        southafricanorth `
  -AcaEnvironment  aca-warewolf `
  -AcrName         acrwarewolf `
  -PublishPath     D:\QueueProcessor\Publish `
  -TriggerPath     'C:\ProgramData\Warewolf\Triggers\Queue' `
  -QueueSourcePath 'C:\ProgramData\Warewolf\Resources\Sources' `
  -EngineBaseUrl   https://wwengine.azurewebsites.net `
  -EngineResourceAppId <engine-app-id> `
  -KeyVaultName kvwarewolf -KeyVaultSecretName wwaeskey -EncryptStagedSettings `
  -RabbitMqSecretUri https://kvwarewolf.vault.azure.net/secrets/rabbitmq-uri
```

**How the trigger file is pointed at the script** (mutually exclusive; `-TriggerId` narrows a
folder/manifest to one trigger; **zero matches is a hard error**):

| Parameter | Result |
|---|---|
| `-TriggerFilePath <file>` | one Container App |
| `-TriggerPath <folder>` + `-TriggerFilter` (default `*.bite`) | one Container App **per matching file** |
| `-TriggerManifestPath <json>` | one per entry, with per-trigger overrides |

**Derived from the trigger file** (single source of truth — operators keep editing the trigger /
its release variable): `maxReplicas = Concurrency`; KEDA `value = Prefetch × MaxConcurrency`
(messages per replica, so `replicas = ceil(queueLength / value)` capped at `maxReplicas`);
The worker reads `Prefetch` from the staged trigger for its per-consumer QoS (no prefetch env
var — the trigger is the single source of truth). `-ScalingMode` defaults to `Elastic` (`minReplicas = 0`);
`Fixed`/`Warm` and any `-MaxReplicas` above `Concurrency` are exception paths and are flagged in
the plan output. `Concurrency = 0` deploys `min = max = 0` (disabled), mirroring on-prem.

**Fail-loud plan-time guards:** unsubstituted `#{…}` release token; derived app-name collision;
invalid timeout nesting (`-EngineTimeoutSeconds ≤ -ShutdownGraceSeconds < -TerminationGracePeriodSeconds`,
and `-EngineTimeoutSeconds <` the **engine's** `functionTimeout`); missing trigger/source file. Peak core
usage (`Σ maxReplicas × cpu`) is printed to check against the ACA environment quota.

**Failure handling.** Engine 2xx → ack. Engine non-2xx → dead-letter the mapped body **and** ack (so a
drained queue proves nothing on its own — compare `succeeded` vs `deadLettered`). A **transport** failure
(timeout, socket error) → nack with requeue, and dead-letter once `-MaxDeliveryAttempts` is spent.

`-MaxDeliveryAttempts` accepts only **1** (dead-letter at once) or **2** (requeue once, then dead-letter,
the default). Attempts are counted with the AMQP `redelivered` flag, which is a boolean — the broker
records that a message has been seen before, not how many times — so higher values are clamped with a
logged warning. Before 2026-08-11 a transport failure was left **unacked**, which with `Prefetch=1`
stopped the consumer permanently (34 messages stranded for 20+ minutes behind one message).

**Timeout defaults** (raised from 45/60/90 on 2026-08-11 after 45 s proved under-sized in a live run;
sized from measured engine latency, see the migration plan §2.6):

| Setting | Default |
|---|---|
| `-EngineTimeoutSeconds` | 180 |
| `-ShutdownGraceSeconds` | 210 |
| `-TerminationGracePeriodSeconds` | 240 |
| the engine's `functionTimeout` (`host.json`, not a script parameter) | 600 |

> ⚠️ `-TerminationGracePeriodSeconds` was validated and printed but **never applied** until 2026-08-11 —
> it is a Container App *template* property, not an env var, so `--set-env-vars` could not carry it.
> Older revisions run on ACA's 30 s default regardless of what the plan output claimed. Verify with
> `az containerapp show` and check `properties.template.terminationGracePeriodSeconds` is not empty.

Can also run as a **companion of the engine deploy**, fanning out over every pointed trigger:

```powershell
./Deploy-WwExecutionEngine.ps1 ... `
  -DeployRabbitMqTriggers `
  -QueueTriggerPath 'C:\ProgramData\Warewolf\Triggers\Queue' `
  -QueueSourcePath  'C:\ProgramData\Warewolf\Resources\Sources' `
  -AcaEnvironment aca-warewolf -AcrName acrwarewolf `
  -QueueProcessorPublishPath D:\QueueProcessor\Publish `
  -RabbitMqSecretUri https://kvwarewolf.vault.azure.net/secrets/rabbitmq-uri
```

**What gets staged into the container** — config is **baked into the image**, not mounted or
fetched at startup (this worker scales to zero, so any share mount or blob round-trip would be paid
on every 0→1 scale and could stop a replica starting):

```
/app/Settings/triggers/<TriggerId>.bite      the queue-trigger definition
/app/Settings/sources/<QueueSourceId>.bite   every source the trigger references…
/app/Settings/sources/<QueueSinkId>.bite     …including the dead-letter sink, when it differs
```

Sources are prompted for with `-QueueSourcePath` and copied automatically for **both**
`QueueSourceId` and `QueueSinkId` — a sink source that is not staged gives a replica that starts
and then cannot dead-letter. **Every referenced source is resolved at plan time** and listed with
the triggers that reference it; any gap aborts the run before a single Container App is created, so
a fan-out over many triggers is all-or-nothing rather than half-deployed. At runtime the worker
reads all staged sources **once at startup** and caches them indexed by `ID`, so nothing re-reads a
`.bite` after cold start. The script writes `{sourceId}.bite`; the worker also resolves an
operator-named Studio file (e.g. `Warewolf DevOps RabbitMQ Source.bite`) by scanning for a matching
`ID` attribute. `Settings\` itself is still searched **after** `Settings\sources\`, so a deployment
staged under the earlier flat layout keeps working. Paths are overridable with
`QUEUE__SETTINGSPATH` / `QUEUE__TRIGGERSSUBPATH` / `QUEUE__SOURCESSUBPATH` / `QUEUE__TRIGGERFILTER`;
defaults resolve against **`AppContext.BaseDirectory`**, not the working directory, and folder
casing is matched case-insensitively (the worker is Linux, the deploy is Windows).

**Tenant id is always set.** `-EngineTenantId` defaults from `az account show` and is prompted if
that yields nothing, because a blank tenant is legal only for a *system-assigned* managed identity
— for anything else the credential chain fails with *"Invalid tenant id provided"* at the first
message, which reads like a missing app role rather than missing config.

**Optimum scale shape:** `minReplicas 0`, `maxReplicas = Concurrency`, and trigger
`Prefetch = MaxConcurrency` (both 1 unless a workflow is measured safe to run concurrently).
Dispatch is serial per channel, so a larger prefetch adds no throughput — it raises the KEDA target
(`value = Prefetch × MaxConcurrency`), which **delays** scale-out, and leaves more buffered messages
to nack on drain. Scale **out**, not up; a prefetch above the cap is flagged as a plan-time advisory.

**Testing locally without deploying:** `Dev\Warewolf.Execution.QueueProcessor\Settings\` holds a
committed working sample (dev broker source under `sources\` + an `order-queue` trigger under
`triggers\`) copied to the build output, so the worker runs straight from `bin\`. It is deliberately
**excluded from `dotnet publish`**, so it never reaches a container image. For `docker compose`,
copy it to `Settings.local\` (git-ignored) and repoint the `ConnectionString` at the compose broker.

**After deploying:** grant **each** app's managed identity the engine app role
`Warewolf_QueueProcessor`, and add a **per-workflow** (`IsServer=false`) `View`+`Execute` row to
`secure.config` for each trigger's `WorkflowName` — unlike the JobProcessor's global-scope row.
Full walkthrough, verification, and teardown: `docs\Deploy-EndToEnd-Runbook.md` §8.

To **prove** the whole Azure path (engine `/Public` + `/Secure`, one Container App per trigger, the
ACA/KEDA scale rule, then a live scale-`0→N` test by publishing to the queue) in a disposable
resource group you delete afterwards, follow
[`docs/Deploy-E2E-Verification-Runbook.md`](../docs/Deploy-E2E-Verification-Runbook.md).

---

## PRV-15 — Dry-run mode

```powershell
./Configure-WwExecutionAuth.ps1 -DryRun       # alias for -WhatIfOnly
./Configure-WwExecutionAuth.ps1 -WhatIfOnly   # original switch
```

Both forms print the resolved plan (Entra app, app roles, secret strategy,
app-settings, Easy Auth provider, smoke-test target) and exit without
mutating Azure.  Use as a pre-merge gate in CI.

## PRV-16 — Module split / future layout

`Configure-WwExecutionAuth.ps1` is intentionally a single file so it can
run in Cloud Shell or a deployment runner without fighting a module path.
It is internally organised by logical region:

```
Stage 0   Pre-flight           (Test-FunctionAppExists, az version)
Stage 1   Entra app create     (graph retry + dedupe)
Stage 2   ID-token issuance
Stage 3   API expose + scope
Stage 4   App-roles declarative replace
Stage 5   Service principal
Stage 6   User assignment
Stage 7   Client secret
Stage 8   App settings
Stage 9   Easy Auth V2
Stage 10  Verification
Stage 11  Smoke test
```

When this file exceeds 2,000 lines the recommended split is:

```
Modules/
    Wwx.Provisioning.Common.psm1     # Invoke-AzCli, ConvertFrom-AzJson, retry helpers
    Wwx.Provisioning.Entra.psm1      # stages 1..6
    Wwx.Provisioning.AppService.psm1 # stages 7..9
    Wwx.Provisioning.Verify.psm1     # stages 10..11
```

The single-script entry point is preserved as a thin orchestrator that
imports the modules.

## PRV-17 — Managed Identity

```powershell
./Configure-WwExecutionAuth.ps1 -UseManagedIdentity
```

Suppresses optional secret rotations beyond the minimum required by Easy
Auth's confidential-client flow.  Use this for production tenants that
prefer MI for app-only and downstream calls.  The function app's
system-assigned identity must already be enabled and granted any required
RBAC roles (e.g. on Key Vault).

## PRV-18 — Secret lifetime

```powershell
./Configure-WwExecutionAuth.ps1 -SecretLifetimeYears 2 -RotateSecret
```

Sets the validity period of any new client secret.  Range: 1–2 years
(Entra cap).  Combine with `-RotateSecret` to force rotation on every run.

---

## CFG-03 — `secure.config` CI/CD deployment

Recommended pipeline (Azure DevOps / GitHub Actions):

1. `Encrypt-Config.ps1 -Input ./secure.config.dev.json -Output ./secure.config`
2. `Test-Json -Path ./secure.config.dev.json -SchemaFile ./secure.config.schema.json`
3. Upload `secure.config` to Azure App Service `D:\home\site\wwwroot\` via
   `az webapp deploy --type static --src-path secure.config --target-path secure.config`.
4. The running function picks up the change automatically — `SecureConfigWatcher`
   debounces the file-system event and atomically swaps the policy snapshot
   (no app restart, no warm-up cost).

Sample CI step (GitHub Actions):

```yaml
- name: Validate secure.config shape
  shell: pwsh
  run: |
    Test-Json -Path Scripts/secure.config.example.json `
              -SchemaFile Scripts/secure.config.schema.json `
              -ErrorAction Stop

- name: Provisioning dry-run
  shell: pwsh
  env:
    AZURE_CREDENTIALS: ${{ secrets.AZURE_CREDENTIALS }}
  run: |
    az login --service-principal `
      -u $env:CLIENT_ID -p $env:CLIENT_SECRET --tenant $env:TENANT_ID
    ./Scripts/Configure-WwExecutionAuth.ps1 -DryRun -NonInteractive

- name: Encrypt and deploy secure.config
  shell: pwsh
  run: |
    ./Scripts/Encrypt-Config.ps1 -Input ./secure.config.${{ github.ref_name }}.json `
                                 -Output ./secure.config
    az webapp deploy --resource-group $env:RG --name $env:APP `
                     --type static --src-path ./secure.config `
                     --target-path secure.config
```

> The encryption key MUST be the same on the build runner and the function
> app.  Source it from Key Vault (`KeyVaultSetup.ps1` provisions the vault
> + access policy).
