---
name: warewolf-e2e-verify
description: Run and interpret the Azure end-to-end verification of the Execution Engine + RabbitMQ QueueProcessor (Container Apps + KEDA) using New-WwE2EStaging.ps1 and Invoke-WwE2EVerification.ps1. Invoke when asked to verify, prove, smoke-test or demo the Azure queue path end to end, to run the E2E harness, to interpret its scored summary, or to tear a verification run down. Covers the 18 acceptance criteria, the traps that make results misleading, and the surgical teardown.
---

# End-to-end verification harness (Engine + QueueProcessor on ACA/KEDA)

Two scripts, in `Dev/Warewolf.Execution.Lightweight/Scripts/` and copied to **`G:\Deployment\Scripts`**
(which runs self-contained, no repo checkout needed).

```powershell
cd G:\Deployment\Scripts          # or Dev\Warewolf.Execution.Lightweight\Scripts

# 1. Stage. No path parameters needed - everything defaults to the fixed G:\Deployment layout.
#    Add -Publish to rebuild both apps into apps\ExecutionEngine and apps\QueueProcessor.
.\New-WwE2EStaging.ps1 -Publish

# 2. DRY RUN - validates staging, prints the plan and approval inventory, changes nothing.
.\Invoke-WwE2EVerification.ps1 -StagingManifest 'G:\Deployment\logs\e2e-<suffix>\staging-manifest.json'

# 3. Real run - deploys, verifies, scores 18 criteria, writes JSON + markdown beside the manifest.
.\Invoke-WwE2EVerification.ps1 -StagingManifest 'G:\Deployment\logs\e2e-<suffix>\staging-manifest.json' -Execute
```

Add `-TeardownWhenDone` to clean up automatically; omit it (the default) to leave the deployment
standing. `-SkipDrainTest` / `-SkipUnackedProbe` shorten a run and mark those criteria `SKIP`.

**The fixed layout** (all overridable; `-StageRoot` moves items 1–8 together):

| # | Purpose | Path |
|---|---|---|
| 1 | Engine publish | `G:\Deployment\apps\ExecutionEngine` |
| 2 | Worker publish | `G:\Deployment\apps\QueueProcessor` |
| 3 | authconfig *(engine)* | `G:\Deployment\settings\Deploy-WwExecutionEngine.authconfig.json` |
| 4 | Elasticsearch source *(engine ONLY)* | `G:\Deployment\settings\ElasticsearchLoggingSource.bite` |
| 5 | `secure.config` *(engine)* | `G:\Deployment\settings\secure.config` |
| 6 | Licence *(engine)* | `G:\Deployment\settings\Warewolf License.secureconfig` |
| 7 | RabbitMQ sources | `G:\Deployment\sources` |
| 8 | RabbitMQ triggers | `G:\Deployment\triggers` |
| 9 | Workflows | `C:\ProgramData\Warewolf\Resources` |

Only the **licence** and **broker credentials** are hand-supplied; the broker URI is normally read out
of the source `.bite` in item 7 (plaintext or DPAPI). `-Publish` and `-GenerateTriggers` are the only
options that need the repo — pass `-RepoRoot` when running from `G:\Deployment\Scripts`.

**Elasticsearch is engine-only.** `-EnableElasticsearch` sets `ENABLEELASTICSEARCHLOGGING=true` and
stages item 4 (which must carry that exact filename). The QueueProcessor has **no** Elasticsearch
logger and `Deploy-WwQueueProcessor.ps1` has no ES parameter; it logs to console + App Insights.

**Queue names are not suffixed** (they come from the authored triggers), so staging checks for existing
Container Apps consuming the same queues and reports `clear` / `contended` / `unknown`. Treat
`contended` as a blocker for criteria 11 and 14 — a competing consumer on a stopped or older engine
dead-letters its share while the queue still drains. Use `-GenerateTriggers` for isolated queues, or
park the other apps at `--max-replicas 0`.

## What to do when asked to run this

1. **Confirm the target subscription first** — `az account show`. The defaults target the shared
   `DEV2` resource group; a wrong subscription creates billable resources in the wrong place.
2. **Always dry-run first** and show the operator the plan + approval inventory.
3. **Get explicit approval before `-Execute`.** It creates a storage account, a Function App, App
   Insights, **an Entra app registration**, two Container Apps, four role assignments, and publishes
   real messages to a live broker.
4. Expect **12–20 minutes** for a real run. Most of it is unavoidable waiting: ACA's KEDA polling
   interval is ~30 s and its scale-in cool-down is ~5 min, and Log Analytics ingestion lags 2–5 min.
   None of that is a fault; do not tune around it.
5. **Read the scored summary**, then relay `PASS/FAIL/SKIP` and any FAIL detail. The JSON is
   `<stage>\logs\e2e-result-<stamp>.json`; the markdown alongside it includes ready-to-paste teardown
   commands.
6. **Always relay the resource summary and the URLs.** Every script now prints a
   *Resources manipulated* block — grouped CREATED / UPDATED / DELETED / REUSED, with the engine URL,
   its `/apis.json` discovery URL, the `/Public`, `/Secure` and `/Services` route templates, and portal
   links. Report what was created and where to reach it, not just the pass count.

> **Never trust the exit code alone.** A PowerShell script that *throws* does not set `$LASTEXITCODE`,
> so a failed run can leave the previous `az` call's `0` in place and look successful — this happened.
> Check the transcript for exceptions, and check that all 18 criteria were actually scored.

### If a run fails part-way

The resource ledger prints **on failure too**, listing what already exists. A retry cannot reuse the
same suffix — the collision guard aborts, deliberately, because `az containerapp create` on an existing
name silently *updates* it. Either clean up first, or **resume**:

```powershell
.\Invoke-WwE2EVerification.ps1 -StagingManifest <manifest> -Execute -ResumeFromPhaseD
```

`-ResumeFromPhaseD` requires the resources to exist (the inverse of the collision guard), skips the two
deploys, resolves what they created, and runs Phases D and E. Use it to finish a run that died after
deployment rather than tearing down and redeploying.

## Per-message report (which replica ran what, and how long it took)

`Get-WwQueueRunReport.ps1` is read-only and **time-window driven**, so it re-renders any past burst
still inside workspace retention rather than needing to watch one live:

```powershell
.\Get-WwQueueRunReport.ps1 -AppNamePrefix wwqp3- -LastMinutes 45 `
    -IncludeEngine -EngineAppInsightsName <engine>-ai `
    -ExpectedManifest <manifest.json>
```

It reconstructs each delivery from `ContainerAppConsoleLogs_CL` into `txn → replica → revision →
startedUtc → durationMs → outcome`, plus replica distribution, per-queue percentiles, reliability
counts and a CSV/JSON.

- **The join key is the AMQP `CorrelationId`.** `RabbitMqMessagePump` copies it into
  `Warewolf-Custom-Transaction-Id`, which renders as `[Txn:…]` on every worker line for that
  delivery. Stamp a unique one per published message. It is the ONLY way to track a *failure*
  message, whose body is empty and so matches nothing.
- **`ExecutionId` joins worker to engine.** The worker generates it, sends it as
  `Warewolf-Execution-Id`, and the engine logs it in the same `[ExecutionId:…]` form.
- **Engine-side evidence needs `EXECUTIONLOGLEVEL=INFO`.** `AuditExecutionLogger` writes only
  ERROR/FATAL, so at the default `ERROR` a *successful* execution logs nothing at all. Raise it
  before the burst and restore it after.
- **The replica column is `ContainerGroupName_s`.** There is NO `ReplicaName_s` on that table;
  projecting it makes the query fail, and because `Invoke-E2ELogAnalytics` passes `-AllowFail` that
  arrives as an EMPTY RESULT, not an error — a run that reports zero messages processed. Never read
  zero rows as "nothing happened".

## Reading the result without being misled

These are the ways a run looks fine and isn't. Each was hit for real.

| Symptom | What it actually means |
|---|---|
| Revision `Healthy`, 0 replicas | Proves **provisioning only**. No container ever started, so nothing about the image was exercised. Criterion 4 is scored from *executions*, not health |
| Work queue drained | Proves nothing on its own. The contract is *2xx → ack* and *non-2xx → dead-letter **and** ack*, so **both** outcomes drain the queue and scale back to zero. Distinguish via `succeeded`/`failed` and dead-letter depth |
| Peak replicas = 1 for a multi-message burst | `value` is too high, not a broken scaler. Messages still process, just serially |
| Replicas above `maxReplicas` during the drain test | Expected. A restart starts replacements while old replicas drain (5 observed against max=3) |
| `/Secure` returns **500** with a valid token | Three different causes. **Read the body**: `Workflow file not found` = not staged; a bare nested `Error{…}` = authorization denial (WOLF-8418 wraps denials as 500, not 403); a workflow-level message like `Scalar value { x } is NULL` = found, authorized and **executed**, which is a pass |
| Any route returns **404** | The `/api` prefix was used. `host.json` sets `routePrefix: ""` |
| Every route returns **403 Site Disabled** | The Function App is **stopped**, not an auth failure |
| `succeeded` ≠ `distinctBodies` | Redelivery or duplicate execution — the drain budget was exceeded |
| **All 18 criteria pass** | Provisioning, scaling and drainage are sound. It says **nothing** about whether messages were processed correctly — all 18 passed on 2026-08-06 while a defect was silently dead-lettering ~45% of valid messages. Reconcile per-message with `Get-WwQueueRunReport.ps1 -ExpectedManifest` |
| Queue static, one consumer attached, replica healthy | A delivery was left unacked and `Prefetch=1` blocks everything behind it. Fixed 2026-08-11; on an older image the only recovery is `az containerapp revision restart` |
| Valid messages dead-lettered under load, fine when tested one at a time | The engine concurrency defect (shared activity instances). Confirm by hitting the engine directly: sequential passes, concurrency ≥4 fails with `Error with variables in input. [[…]]`. A SQL-free workflow survives concurrency 20 |

## Things the harness handles that manual runs get wrong

Do not "fix" these by reverting to the obvious approach:

- **Broker topology is pre-created over AMQP.** `PublishRabbitMQActivity` **cannot** create a queue or
  exchange: a failed passive declare closes the channel and the active declare is then reissued on that
  dead channel, so a first publish to a new queue fails with `NOT_FOUND - no exchange`. The exchange is
  *direct and named after the queue*, and the binding uses an **empty routing key** — without the
  binding every message is silently discarded.
- **The engine token comes from client credentials.** `az account get-access-token --resource api://…`
  fails on a freshly created app registration with `AADSTS65001 consent_required`, because the Azure
  CLI client is not pre-authorized on it. The harness authenticates the app as itself using the Easy
  Auth secret. The resulting token has **no app roles**, which is sufficient wherever the `Public`
  group holds global Execute.
- **Execution evidence comes from Log Analytics.** The worker's `Dev2Logger` lines never reach App
  Insights `traces`; only HttpClient *dependency* telemetry does. `ContainerAppConsoleLogs_CL` also
  spans every replica, which `az containerapp logs show` (a single-replica live tail) cannot.
- **No `--query` in `az` calls.** On Windows `az` is a `.cmd` shim and cmd.exe mangles JMESPath
  filters/projections — `[?…]` or `{…}` arrives unquoted and fails with
  `].{name:name was unexpected at this time.` The module refuses `--query` outright.
- **`RabbitMQ.Client` needs `System.Threading.RateLimiting`**, which is absent from the worker publish
  *correctly* — it ships in the `Microsoft.AspNetCore.App` shared framework the container's
  `aspnet:8.0` base image provides. Outside a container it must be loaded from the installed shared
  framework, or the failure reads as `BrokerUnreachableException: None of the specified endpoints were
  reachable`.
- **Generated sources are plaintext.** A Studio-exported source is usually DPAPI-encrypted, which is
  Windows- and machine-scoped and can never be read inside the Linux worker; `-EncryptStagedSettings`
  converts plaintext to WFAES at deploy time instead.
- **Never add a per-workflow `secure.config` row "to be safe".** A workflow uses the resource role map
  *exclusively* once it has any Execute-bearing resource entry, and stops inheriting the global grant —
  so a row naming the wrong group **locks the worker out** of a workflow it could previously execute.
- **Publish with a unique AMQP `CorrelationId` per message.** It is the join key for every worker log
  line and the only handle on a *failure* message, whose body is empty. Without it a run can be counted
  but not reconciled.
- **The timeout chain has a fourth member on the engine:** `ENGINE__TIMEOUTSECONDS (180)` ≤
  `WORKER__SHUTDOWNGRACESECONDS (210)` < `terminationGracePeriodSeconds (240)` < `functionTimeout (600)`,
  the last of which lives in the engine's `host.json`. Verify the third on the live app — it is a template
  property that was never actually applied before 2026-08-11, so older revisions sit on ACA's 30 s
  default whatever the plan output said.

## Teardown

Every run's markdown summary carries its own teardown block. The rules that matter:

- ⛔ **Never `az group delete`** — `DEV2` is shared (AKS, live `WarewolfServer*`, the Key Vault, the ACA
  environment, the registry).
- ⛔ Never delete the ACA environment, the registry, the Log Analytics workspace, or the shared AES key
  secret (`WWExecutionEngineTestSecret`) — other deployments depend on it.
- Delete only this run's Container Apps (by the run-suffixed prefix), its **own** image repository, its
  **own** KEDA secret, its broker topology, and the engine via
  `Rollback-WwExecutionEngine.ps1 -SummaryPath <engine summary>`.
- The Entra app registration is a **directory** object and survives resource-level teardown; the
  rollback script removes it when the summary records `created.entraApp = true`.
- Pick the engine summary whose `created.functionApp = true`. If an engine was deployed twice under one
  name, later summaries record `false` and would under-delete.
- **`created.entraApp` in summaries written before 2026-08-12 is unreliable — it was hard-coded `true`
  whenever auth provisioning ran, without ever checking whether the registration already existed.** So a
  redeploy, or even a run that FAILED before creating anything, still recorded ownership. It is now
  probed with `az ad app list --display-name` and defaults to *pre-existing* when the probe fails, so an
  unproven assumption can never arm a delete. Before rolling back with an older summary, confirm which
  run actually created the registration (`az ad app list --display-name <app>-auth` and compare
  `createdDateTime` against the run), or pass `-IncludeEntraApp` deliberately. Note this is the mirror
  of the `functionApp` hazard above: that one under-deletes, this one **over**-deletes.

Full detail: `docs/Deploy-E2E-Rollback-Commands.md`.

## Reference

| Document | Purpose |
|---|---|
| `docs/E2E-Harness-README.md` | Prerequisites, parameters, what is secret, how to read the output |
| `docs/Deploy-E2E-Verification-Runbook.md` | The manual runbook this harness automates |
| `docs/Deploy-E2E-Execution-StepByStep.md` | A worked isolated-parallel run |
| `docs/Deploy-E2E-Execution-Summary.md` | Findings and measurements from the 2026-08-06 run |
| `docs/Deploy-E2E-Rollback-Commands.md` | Surgical teardown |
| `docs/QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md` | Design rationale, concurrency/prefetch → KEDA mapping |

Deployment-script internals and the phased deploy flow → invoke `warewolf-deploy`.
