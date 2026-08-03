# `Scripts/` — wwexecution provisioning toolkit

This folder is the operational source of truth for everything the
`Warewolf.Execution.Lightweight` function app needs at the Azure / Entra
control plane.

| Script                                        | Purpose                                                                 |
|-----------------------------------------------|-------------------------------------------------------------------------|
| `Deploy-WwExecutionEngine.ps1`                | **End-to-end deployment orchestrator** — runs every step in `docs/Deployment-Steps.txt` (RG → storage → Function App → App Insights → Entra/Easy Auth → publish, with optional Key Vault setup + resource encryption). Thin: reuses the scripts below. |
| `Deploy-WwExecutionEngine.authconfig.example.json` | Template for the orchestrator's `-AuthConfigPath` (GroupPermissions + UserAssignments). Each GroupPermissions key becomes an MI-assignable app role; the `Warewolf_ClientApps` entry is the dedicated role for app-only client apps (daemon/MI callers get it via `Configure-WwExecutionAuth-Clients.ps1 -AppRolesToAssign`, and `secure.config` must grant the matching `WindowsGroup` Execute on the workflows they call). |
| `Deploy-WwJobProcessor.ps1`                   | **ExecutionEngineJobProcessor deploy orchestrator** — provisions the poller/reaper Function App (RG → storage → Function App + system-assigned MI → App Insights → Key Vault wiring → stage + WFAES-encrypt the persistence settings pair → app settings → publish). Standalone, or invoked by `Deploy-WwExecutionEngine.ps1 -DeployJobProcessor`. Same *params-first, prompt-if-missing*, `-DryRun`, masked-summary + transcript conventions. The persistence source files (`persistencesettings.json`, `persistencesettingsdbsource.bite`) are **prompted when not passed**. Role assignment (`Warewolf_JobProcessor`) is a separate step via `Configure-WwExecutionAuth-Clients.ps1` — see the runbook. |
| `Deploy-WwExecutionServiceBusWorker.ps1`      | **Service Bus worker deploy orchestrator ("shovel bridge")** — provisions the Service Bus-triggered Function App published from `Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus` (RG → storage → Function App + system-assigned MI → App Insights → Service Bus namespace/queue with dead-lettering → RBAC or SAS listen auth → app settings → publish). Also provisions a queue-scoped **Send-only SAS rule** (`shovel-send` by default) — the credential a RabbitMQ Shovel plugin uses as its AMQP 1.0 destination to bridge RabbitMQ messages into this queue. Same *params-first, prompt-if-missing*, `-DryRun`, masked-summary + transcript conventions as `Deploy-WwJobProcessor.ps1`. No dedicated Rollback companion (same as the JobProcessor) — the run summary JSON records what to tear down manually. Role assignment (`Warewolf_ClientApps`) is a separate step — see `docs/KB-ClientApps-Configuration.md` §2.6. |
| `Configure-RabbitMqShovel.ps1`                | **RabbitMQ → Service Bus shovel bridge configurator** — configures a dynamic RabbitMQ Shovel (via the RabbitMQ Management HTTP API, `PUT /api/parameters/shovel/{vhost}/{name}`) that forwards messages from an existing RabbitMQ source queue (AMQP 0.9.1) to the Service Bus queue provisioned by `Deploy-WwExecutionServiceBusWorker.ps1` (AMQP 1.0), per Microsoft's documented RabbitMQ-to-Service-Bus bridging pattern. Fetches the destination SAS key live via `az` (or accepts it directly as a SecureString) — never writes it to disk. Verifies the source queue exists and polls the shovel's running state after applying. Requires the `rabbitmq_shovel`/`rabbitmq_shovel_management` plugins to already be enabled on the broker (one-time, broker-host admin action — the script probes for this and gives the exact `rabbitmq-plugins enable` instruction if missing). Same conventions: params-first/prompt-if-missing, `-DryRun`, masked summary + transcript, `-LoadFunctionsOnly` test hook. Validated against a live `rabbitmq:3-management` Docker container (see docs/ShovelBridge-Architecture.md). |
| `Rollback-WwExecutionEngine.ps1`              | **Teardown companion** — deletes ONLY what a deploy run created (summary-/tag-driven), in dependency order, with a leak check. Existing resources are preserved. |
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
| `Encrypt-Config.ps1`                          | Encrypts `secure.config` plaintext into the deployable form.            |
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
