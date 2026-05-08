# `Scripts/` — wwexecution provisioning toolkit

This folder is the operational source of truth for everything the
`Warewolf.Execution.Lightweight` function app needs at the Azure / Entra
control plane.

| Script                                        | Purpose                                                                 |
|-----------------------------------------------|-------------------------------------------------------------------------|
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
