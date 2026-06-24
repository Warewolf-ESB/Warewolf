# `Scripts/` — wwexecution provisioning toolkit

This folder is the operational source of truth for everything the
`Warewolf.Execution.Lightweight` function app needs at the Azure / Entra
control plane.

| Script                                        | Purpose                                                                 |
|-----------------------------------------------|-------------------------------------------------------------------------|
| `Deploy-WwExecutionEngine.ps1`                | **End-to-end deployment orchestrator** — runs every step in `docs/Deployment-Steps.txt` (RG → storage → Function App → App Insights → Entra/Easy Auth → publish, with optional Key Vault setup + resource encryption). Thin: reuses the scripts below. |
| `Deploy-WwExecutionEngine.authconfig.example.json` | Template for the orchestrator's `-AuthConfigPath` (GroupPermissions + UserAssignments). |
| `Rollback-WwExecutionEngine.ps1`              | **Teardown companion** — deletes ONLY what a deploy run created (summary-/tag-driven), in dependency order, with a leak check. Existing resources are preserved. |
| `Tests/Deploy-WwExecutionEngine.Tests.ps1`    | Pester 5 suite for the orchestrator (helpers + DryRun end-to-end). Run: `Invoke-Pester -Path ./Tests/Deploy-WwExecutionEngine.Tests.ps1`. |
| `Tests/Rollback-WwExecutionEngine.Tests.ps1`  | Pester 5 suite for the rollback script (ownership resolver + DryRun teardown). |
| `Configure-WwExecutionAuth.ps1`               | End-to-end Entra + Easy Auth + secure-config provisioning (idempotent). |
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

- **Publish source** — `-PublishPath` accepts a folder or a `.zip` (extracted to a
  sibling folder that becomes the package dir). No `dotnet publish` is run by the script.
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
  it creates/uploads nothing in Azure: files are prepared into a timestamped sibling
  preview dir (`<PublishPath>-dryrun-<stamp>`, leaving your publish dir untouched),
  and a transcript + `*.dryrun.summary.json` (with `"dryRun": true`) are written so
  the rollback can be exercised from a dry-run summary. Encryption runs only when the
  Key Vault key is reachable, else it's staged unencrypted and deferred.
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
Invoke-Pester -Path ./Tests/Deploy-WwExecutionEngine.Tests.ps1
```

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
