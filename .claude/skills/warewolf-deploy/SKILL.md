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
| `Deploy-WwExecutionEngine.ps1` | **Thin orchestrator.** Provisions infra, configures auth, stages + encrypts package contents, applies env vars, deploys an already-published package. `#Requires -Version 7.0`. Optional companion switches chain a separately-published Function App after the engine deploy: `-DeployJobProcessor` (→ `Deploy-WwJobProcessor.ps1`) and `-DeployServiceBusWorker` (→ `Deploy-WwExecutionServiceBusWorker.ps1`). |
| `Configure-WwExecutionAuth.ps1` / `Configure-WwExecutionAuth-Clients.ps1` | Entra ID + Easy Auth app registration and client (audience/role) configuration. |
| `Configure-WwExecutionAuth-ClientApps.ps1` | Orchestrator: one registration per client-**example** app (Angular/React/WebMvc/Console/AzureFunction/ServiceBus), prompts, validates `/secure/{workflow}.json`, masked summary. Companion: `docs/KB-ClientApps-Configuration.md`. |
| `Remove-WwExecutionAuth-Clients.ps1` / `Cleanup-WwExecutionAuth.ps1` | Tear down client registrations / auth artefacts. |
| `Setup-EntraAuth.ps1` | Entra tenant/app bootstrap. |
| `KeyVaultSetup.ps1` / `KeyVaultSetup.azcli` | Provision Key Vault for AES-backed secure.config encryption. |
| `Encrypt-Config.ps1` | Encrypt `secure.config` / WFAES payloads. |
| `Setup-ApplicationInsights.ps1` | Provision + wire App Insights. |
| `Get-WwExecutionToken.ps1` / `Get-WwExecutionToken-AllFlows.ps1` | Acquire JWT/Entra tokens for calling secured routes. |
| `Get-DropboxTokens.ps1` | Dropbox OAuth token retrieval. |
| `Generate-WorkflowIndex.ps1` | Regenerate `workflow-index.json` from `Resources/`. |
| `Deploy-WwJobProcessor.ps1` | Suspend/resume poller Function App (`Warewolf.Execution.EngineJobProcessor`), replacing `hangfireserver.exe`. Also runnable as an engine companion via `-DeployJobProcessor`. |
| `Deploy-WwQueueProcessor.ps1` | **RabbitMQ queue workers on Azure Container Apps** (`Warewolf.Execution.QueueProcessor`), replacing `N × QueueWorker.exe` for the Azure path. **One Container App per queue-trigger**, autoscaled 0→N by the KEDA `rabbitmq` scaler. Pointed at a trigger file / folder / manifest; also runnable as an engine companion via `-DeployRabbitMqTriggers`. |
| `Rollback-WwExecutionEngine.ps1` | Roll a deployment back. |
| `Deploy-WwExecutionServiceBusWorker.ps1` | **Shovel bridge, Azure side.** Provisions the Service Bus-triggered Function App — a **first-class supported component**, `Warewolf.Execution.ServiceBusWorker/` — a Service Bus namespace/queue + dead-lettering, Managed Identity listen auth, and a Send-only SAS rule (`shovel-send`) for the RabbitMQ Shovel. Can be run standalone or chained from `Deploy-WwExecutionEngine.ps1 -DeployServiceBusWorker`. See `docs/ShovelBridge-Architecture.md`. |
| `Configure-RabbitMqShovel.ps1` | **Shovel bridge, RabbitMQ side.** Configures a dynamic RabbitMQ Shovel (Management HTTP API) forwarding an existing RabbitMQ queue to the Service Bus queue above, per Microsoft's AMQP 0.9.1→1.0 bridging pattern. See `docs/ShovelBridge-Architecture.md`. |
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

- **Phase 6 — (optional) JobProcessor companion:** `-DeployJobProcessor` → `Deploy-WwJobProcessor.ps1`.
- **Phase 7 — (optional) QueueProcessor companion:** `-DeployRabbitMqTriggers` → `Deploy-WwQueueProcessor.ps1`, invoked **once per resolved trigger file** (`-QueueTriggerPath`/`-QueueTriggerFilePath`/`-QueueTriggerManifestPath`). Fail-fast unless `-ContinueOnQueueTriggerError`.

A timestamped transcript log and a masked `*.summary.json` are written for every real (non-`-DryRun`) run. Use `-DryRun` to preview without changes.

## Queue workers (ACA + KEDA) — the bits that surprise people

- **Publish paths must differ.** Engine, JobProcessor and QueueProcessor are three different projects; the engine orchestrator **fails at plan time** if their publish paths collide.
- **Scale is derived from the trigger `.bite`, not invented:** `maxReplicas = Concurrency`; KEDA `value = Prefetch × MaxConcurrency` (messages per replica ⇒ `replicas = ceil(queueLength / value)`); `Concurrency = 0` ⇒ `min = max = 0` (disabled). `-ScalingMode` defaults to `Elastic` (`min = 0`); `Fixed`/`Warm` are flagged exceptions.
- **`Prefetch` bounds useful parallelism** — one replica claims up to `Prefetch` messages, so a `value` far below `Prefetch` starts replicas that find an empty queue.
- **Unsubstituted `#{…}` release tokens fail loudly** at plan time and at container startup: `Concurrency` must be substituted before deploy because `maxReplicas` derives from it.
- **DPAPI cannot travel.** Trigger/source `.bite` files must be plaintext or WFAES (`-EncryptStagedSettings`); a Windows DPAPI blob fails with an actionable error in the Linux container.
- **Authorization differs from the JobProcessor:** one MI **per app** (assign `Warewolf_QueueProcessor` in a loop, `-ManagedIdentityObjectId` works unchanged for Container Apps) and `secure.config` needs a **per-workflow** `Execute` row, not a global one.
- **TLS is opt-in, default off** (parity with `PublishRabbitMQActivity`, which never sets `Ssl`); production requires `amqps` + `RABBITMQ__USESSL=true` — a documented go-live gate.

## Docs to keep in sync

When changing deployment scripts or their behaviour, update the relevant docs (see `warewolf-sync` rule in CLAUDE.md):
- `Dev/Warewolf.Execution.Lightweight/Scripts/README.md` — script set overview.
- `Dev/Warewolf.Execution.Lightweight/docs/Deploy-RunGuide.md` — run guide.
- `Dev/Warewolf.Execution.Lightweight/docs/Deploy-EndToEnd-Runbook.md` — §7 JobProcessor, **§8 QueueProcessors (ACA + KEDA)**, §9 teardown.
- `Dev/Warewolf.Execution.Lightweight/docs/Deploy-E2E-Verification-Runbook.md` — **full E2E deploy + PROOF** into a disposable RG: engine (`/Public` + `/Secure`), one Container App per RabbitMQ trigger, ACA/KEDA scale rules, then a live scale-`0→N` test by publishing to the queue, plus teardown. Use this to validate the Azure queue path end to end. **NB:** ACA runs KEDA internally — an ACA `rabbitmq` scale rule *is* the KEDA scaler; there is no separate KEDA instance to deploy.
- `Dev/Warewolf.Execution.Lightweight/docs/QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md` — queue-worker migration plan (phases, decisions, concurrency/prefetch → KEDA mapping).
- `docs/Part3-ImplementationPlan.md`, `Part4-ResourceProvisioning.md`, `Part5-ClientTokenManagement.md`, `Part6-FullImplementationTaskList.md` — execution plans / task lists.
- `docs/EasyAuth-Runbook.md`, `EasyAuth-Entra-Tutorial.md`, `README-Authentication.md` — auth.
- `docs/KeyRotationRunbook.md`, `README-Encryption.md`, `SecurityChecklist.md` — Key Vault / encryption / security.
- `docs/README-ApplicationInsights.md`, `QUICKSTART-ApplicationInsights.md` — App Insights.
- `docs/ShovelBridge-Architecture.md`, `Warewolf.Execution.ServiceBusWorker/README.md` — shovel bridge topology + the Service Bus worker's own docs.

When a script's **roles/requirements** change, reconcile the script, its `Tests/`, the run guide, and the implementation-plan docs together — and confirm the changes with the user before committing.
