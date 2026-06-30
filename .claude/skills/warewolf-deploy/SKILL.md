---
name: warewolf-deploy
description: Deploying the Lightweight Execution Engine to Azure and authoring its execution/deployment plans. Invoke when working on deployment scripts under Dev/Warewolf.Execution.Lightweight/Scripts/ (Deploy-WwExecutionEngine.ps1 and companions), their docs, Entra/EasyAuth/Key Vault/App Insights setup, rollback, or roles/requirements. Covers the script set, phased deploy flow, and which docs to keep in sync.
---

# Lightweight Execution Engine — deployment & execution plans

All deployment tooling lives in `Dev/Warewolf.Execution.Lightweight/Scripts/`. The engine is published once, then an orchestrator stages, configures, and deploys the package to an Azure Function App.

**Publish first (the orchestrator does NOT build):**
```powershell
dotnet publish Dev/Warewolf.Execution.Lightweight/Warewolf.Execution.Lightweight.csproj -c Release -o D:\ExecutionEngine\Publish
```

## Scripts (`Scripts/`)

| Script | Role |
|---|---|
| `Deploy-WwExecutionEngine.ps1` | **Thin orchestrator.** Provisions infra, configures auth, stages + encrypts package contents, applies env vars, deploys an already-published package. `#Requires -Version 7.0`. |
| `Configure-WwExecutionAuth.ps1` / `Configure-WwExecutionAuth-Clients.ps1` | Entra ID + Easy Auth app registration and client (audience/role) configuration. |
| `Remove-WwExecutionAuth-Clients.ps1` / `Cleanup-WwExecutionAuth.ps1` | Tear down client registrations / auth artefacts. |
| `Setup-EntraAuth.ps1` | Entra tenant/app bootstrap. |
| `KeyVaultSetup.ps1` / `KeyVaultSetup.azcli` | Provision Key Vault for AES-backed secure.config encryption. |
| `Encrypt-Config.ps1` | Encrypt `secure.config` / WFAES payloads. |
| `Setup-ApplicationInsights.ps1` | Provision + wire App Insights. |
| `Get-WwExecutionToken.ps1` / `Get-WwExecutionToken-AllFlows.ps1` | Acquire JWT/Entra tokens for calling secured routes. |
| `Get-DropboxTokens.ps1` | Dropbox OAuth token retrieval. |
| `Generate-WorkflowIndex.ps1` | Regenerate `workflow-index.json` from `Resources/`. |
| `Rollback-WwExecutionEngine.ps1` | Roll a deployment back. |
| `Example-ClientApps-OrdersSales.ps1` | Worked client-app example. |
| `*.example.json`, `authsettingsV2.json`, `secure.config.*.json` | Config templates/examples. |
| `Tests/` | Pester/script tests for the deployment tooling. |

## Phased deploy flow (`Deploy-WwExecutionEngine.ps1`)

"Params first, prompt if missing." REQUIRED targeting params have **no defaults** (`ResourceGroup`, `Location`, `StorageAccount`, `AppName`); `SubscriptionId`/`TenantId` resolve from `az account show`.

- **Phase 0 — Pre-flight:** az CLI + login, resolve subscription/tenant.
- **Phase 0.5 — PLAN:** resolve every decision (targeting, publish source, feature toggles, all env-var values, secure.config classification, encryption/Key Vault, Elasticsearch), print a masked summary, and ask **once** to proceed before anything is created/changed.
- **Phase 1 — Infra:** resource group, storage, Function App, App Insights.
- **Phase 2 — Auth:** Entra ID + Easy Auth (`Configure-WwExecutionAuth.ps1`).
- **Phase 3 — Stage:** secure.config (validate / auto-encrypt plaintext), workflow resources (+ optional WFAES encryption), Elasticsearch source (+ WFAES encryption), environment variables.
- **Phase 4 — Deploy:** publish the package dir to the Function App (`func`, falling back to `az` zip-deploy).
- **Phase 5 — Verify:** endpoint banner + optional HTTP probe.

A timestamped transcript log and a masked `*.summary.json` are written for every real (non-`-DryRun`) run. Use `-DryRun` to preview without changes.

## Docs to keep in sync

When changing deployment scripts or their behaviour, update the relevant docs (see `warewolf-sync` rule in CLAUDE.md):
- `Dev/Warewolf.Execution.Lightweight/Scripts/README.md` — script set overview.
- `Dev/Warewolf.Execution.Lightweight/docs/Deploy-RunGuide.md` — run guide.
- `docs/Part3-ImplementationPlan.md`, `Part4-ResourceProvisioning.md`, `Part5-ClientTokenManagement.md`, `Part6-FullImplementationTaskList.md` — execution plans / task lists.
- `docs/EasyAuth-Runbook.md`, `EasyAuth-Entra-Tutorial.md`, `README-Authentication.md` — auth.
- `docs/KeyRotationRunbook.md`, `README-Encryption.md`, `SecurityChecklist.md` — Key Vault / encryption / security.
- `docs/README-ApplicationInsights.md`, `QUICKSTART-ApplicationInsights.md` — App Insights.

When a script's **roles/requirements** change, reconcile the script, its `Tests/`, the run guide, and the implementation-plan docs together — and confirm the changes with the user before committing.
