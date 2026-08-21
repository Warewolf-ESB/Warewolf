# E2E verification harness — Execution Engine + QueueProcessor on ACA/KEDA

Two scripts that deploy the Azure queue path into disposable resources, **prove** it works, and score
18 acceptance criteria. Built from the manual runbook
([Deploy-E2E-Verification-Runbook.md](Deploy-E2E-Verification-Runbook.md)) plus everything the
2026-08-06 live run found ([Deploy-E2E-Execution-Summary.md](Deploy-E2E-Execution-Summary.md)).

| File | Role |
|---|---|
| [`Scripts/New-WwE2EStaging.ps1`](../Scripts/New-WwE2EStaging.ps1) | Builds a disposable staging tree and a manifest. Run once per machine. |
| [`Scripts/Invoke-WwE2EVerification.ps1`](../Scripts/Invoke-WwE2EVerification.ps1) | Deploys, verifies, scores, optionally tears down. Re-runnable. |
| [`Scripts/WwE2E.Common.psm1`](../Scripts/WwE2E.Common.psm1) | Shared helpers — AMQP topology, tokens, evidence queries. |
| `.claude/skills/warewolf-e2e-verify/SKILL.md` | Agent entry point: *"run the E2E verification and summarise it"*. |

---

## The fixed staging layout

Everything defaults to the layout this team maintains, so a normal run needs **no path parameters**:

| # | Purpose | Path |
|---|---|---|
| 1 | Execution Engine publish | `G:\Deployment\apps\ExecutionEngine` |
| 2 | QueueProcessor publish | `G:\Deployment\apps\QueueProcessor` |
| 3 | authconfig *(engine)* | `G:\Deployment\settings\Deploy-WwExecutionEngine.authconfig.json` |
| 4 | Elasticsearch source *(engine only — see below)* | `G:\Deployment\settings\ElasticsearchLoggingSource.bite` |
| 5 | `secure.config` *(engine)* | `G:\Deployment\settings\secure.config` |
| 6 | Warewolf licence *(engine)* | `G:\Deployment\settings\Warewolf License.secureconfig` |
| 7 | RabbitMQ sources used by triggers | `G:\Deployment\sources` |
| 8 | RabbitMQ queue-triggers | `G:\Deployment\triggers` |
| 9 | Workflows / resources | `C:\ProgramData\Warewolf\Resources` |
| — | Logs, manifests and summaries | `G:\Deployment\logs\e2e-<suffix>\` |

`-StageRoot` relocates items 1–8 in one go; each also has its own override
(`-EnginePublishPath`, `-SecureConfigPath`, `-TriggerPath`, …).

**A copy of the scripts lives at `G:\Deployment\Scripts`** and runs self-contained — it does not need
the repo checked out. Only `-Publish` and `-GenerateTriggers` need the repo (for csproj and template
files); pass `-RepoRoot` to enable them from there.

## Quick start

```powershell
cd G:\Deployment\Scripts          # or Dev\Warewolf.Execution.Lightweight\Scripts

# 1. Stage. Reuses an existing publish, or add -Publish to build fresh into items 1 and 2.
.\New-WwE2EStaging.ps1 -Publish

# 2. Dry run — prints the plan and the approval inventory, changes nothing.
.\Invoke-WwE2EVerification.ps1 -StagingManifest 'G:\Deployment\logs\e2e-<suffix>\staging-manifest.json'

# 3. Real run.
.\Invoke-WwE2EVerification.ps1 -StagingManifest 'G:\Deployment\logs\e2e-<suffix>\staging-manifest.json' -Execute
```

If the publish directories are empty and you did not pass `-Publish`, staging **asks** whether to
publish (and fails with a clear message under `-NonInteractive`).

### Elasticsearch applies to the ENGINE only

`-EnableElasticsearch` stages item 4 and sets `ENABLEELASTICSEARCHLOGGING=true` on the engine. The file
must be named exactly `ElasticsearchLoggingSource.bite` — the engine reads that literal path under
`Settings\`.

**The QueueProcessor has no Elasticsearch logger**, and `Deploy-WwQueueProcessor.ps1` has no ES
parameter; its source catalog deliberately *skips* non-RabbitMQ sources. Worker logging is console +
App Insights. Wiring ES into the worker would need new worker code.

### Queue names are NOT suffixed — and that is checked

Azure resource names carry a unique run suffix, but queue names come from your authored triggers as
written. Staging therefore checks whether any **existing** Container App already has a `rabbitmq` scale
rule on those queues, and reports one of `clear` / `contended` / `unknown`. Contention matters because
a competing consumer pointed at a stopped or older engine **dead-letters and acks** its share — the
queue still drains and the run looks clean while half the messages never executed. Remedies it offers:
`-GenerateTriggers` (isolated, suffixed queues), park the other apps at `--max-replicas 0`, or delete
them if stale. A check that cannot complete is reported as **unknown, never clear**.

Or hand it to Claude: *"run the Warewolf E2E verification and summarise the result"* — the
`warewolf-e2e-verify` skill covers the workflow, the approval gates and how to read the output.

**A real run takes 12–20 minutes**, most of it unavoidable waiting: KEDA polls about every 30 s, ACA's
scale-in cool-down is ~5 min, and Log Analytics ingestion lags 2–5 min. None of these is configurable
and none is a fault.

---

## Prerequisites

| | |
|---|---|
| PowerShell | **7.0+** (both scripts declare `#Requires -Version 7.0`) |
| .NET SDK | **8.x** installed, for `-Publish` |
| `Microsoft.AspNetCore.App` | **8.x** shared framework — supplies `System.Threading.RateLimiting`, which `RabbitMQ.Client` 7.x needs outside a container |
| Azure CLI | signed in (`az login`); the `containerapp` commands are core in current versions |
| Azure permissions | Contributor on the resource group, **Key Vault Secrets Officer** on the vault (data-plane; Owner is *not* enough), and `Application.ReadWrite.All` to create the Entra app |
| RabbitMQ | reachable broker + credentials. Production requires `amqps` |
| Warewolf resources | `Hello World`, `rabbit\RabbitProcess`, `rabbit\RabbitProcessFailure`, `rabbit\RabbitPublish` under `-WorkflowsSourcePath` |

`New-WwE2EStaging.ps1` checks all of this and refuses to declare itself ready otherwise.

---

## What you must supply

Only two things, and both already exist in the standard layout:

| Input | Why it cannot be generated |
|---|---|
| `settings\Warewolf License.secureconfig` (item 6) | licensed artefact. Without it the engine's licence check — **on by default** — may fail at startup |
| RabbitMQ broker credentials | live in the source `.bite` (item 7). The harness reads them from there; see below |

Everything else is either already in the layout (items 3, 5, 8, 9) or built by `-Publish` (items 1, 2).

### How the broker URI is obtained

The harness needs a URI for its own checks — topology pre-create, queue depth, the unacked probe — and
for the KEDA Key Vault secret. It tries, in order:

1. `-AmqpUri` if you pass it;
2. the manifest (populated by `-GenerateTriggers`);
3. **the source `.bite` in item 7**, decoding either a **plaintext** connection string or a **DPAPI**
   blob. DPAPI only decrypts on the machine and account that created it — which is also exactly why
   such a source can never be read inside the Linux worker, and must be converted to WFAES at deploy
   time by `-EncryptStagedSettings`.

A **WFAES** source cannot be read here (that needs the Key Vault key, which `Encrypt-Config.ps1` owns).
In that case pass `-AmqpUri`, or the broker-side steps are **skipped** and their criteria marked `SKIP`
— never silently passed. Set `-RabbitMqSecretName` to reuse an existing Key Vault secret when no URI is
available at all.

### Secrets

`staging-manifest.json` contains the broker password **only when `-GenerateTriggers` was used**;
otherwise credentials stay in your source `.bite`. Either way the manifest lives under
`G:\Deployment\logs\`, outside the repo, and staging refuses a `-StageRoot` inside a detected repo
unless `-Force`.

Trigger files are *not* secret in this design — credentials belong in the source, and generated
triggers keep `UserName`/`Password` null deliberately, so they can be diffed and shared freely.

---

## What a run creates

Every run gets a 6-character suffix, so two reviewers never collide. This matters more than it looks:
Function App and storage names are **globally** unique, and `az containerapp create` on an existing
name silently **updates** that app instead of failing — an unsuffixed run could reconfigure someone
else's deployment.

| Created | Example |
|---|---|
| Function App + storage + App Insights | `wwengine-e2e-ab12cd`, `stwwe2eab12cd`, `wwengine-e2e-ab12cd-ai` |
| **Entra app registration** + SP + Easy Auth secret | `wwengine-e2e-ab12cd-auth` |
| Container Apps, one per trigger | `wwqpe2eab12cd-…` |
| ACR repository | `warewolf/queueprocessor-e2e-ab12cd` |
| Key Vault secret for KEDA | `rabbitmq-uri-e2e-ab12cd` |
| Broker objects | 2 exchanges + 2 queues + 2 bindings (+ DLQs if anything is dead-lettered) |

**Reused, never created or deleted:** the resource group, the Key Vault **and its AES key**, the
container registry, the ACA environment, and the Log Analytics workspace.

---

## The 18 criteria

| # | Criterion | Phase |
|---|---|---|
| 1 | `/Public/*` returns 200 anonymously | B |
| 2 | `/Secure/*` returns 401 without a token, 200 with one | B |
| 3 | Both trigger workflows resolve and execute | B |
| 4 | Cold start logs catalog → loader → pump; no tenant warning | E |
| 5 | One Container App per trigger, exactly one `rabbitmq` rule each | D |
| 6 | `min = 0`; `max` = trigger `Concurrency`; `value` = `MaxConcurrency` | D |
| 7 | No `activationValue` — a single message must wake the app | D |
| 8 | A trigger consumes a queue whose durability it did not set | E |
| 9 | Replicas sit at 0 on an empty queue | E1 |
| 10 | Publishing raises replicas above 0 unaided | E3 |
| 11 | Peak replicas = `min(ceil(messages / value), maxReplicas)` | E3 |
| 12 | `T` (median `durationMs`) recorded | E3a |
| 13 | Whether `QueueLength` counts unacked messages is recorded | E3b |
| 14 | One success per message; engine returned 200 | E4 |
| 15 | Work queues drain; dead-letter queues do not grow | E5 |
| 16 | Replicas return to 0 within the cool-down | E6 |
| 17 | A forced restart drains cleanly | E7 |
| 18 | Only this run's resources were created | E |

Criteria **5–8** and **11** are the ones worth more than "it deployed": they show each trigger scales
independently, from zero, by parameters chosen deliberately.

---

## Output

Written to `<stage>\logs\`:

| File | Contents |
|---|---|
| `e2e-result-<stamp>.json` | machine-readable: metrics, per-criterion state, identifiers |
| `e2e-result-<stamp>.md` | the same as a table, **plus ready-to-paste teardown commands** |
| `deploy-WwExecutionEngine-<stamp>.summary.json` | the engine deploy's own summary — **the rollback input** |
| `deploy-WwQueueProcessor-<stamp>.summary.json` | per-app scale settings and managed-identity ids |

Exit code is **0** when no criterion failed, **1** otherwise — so it slots into CI unchanged.

---

## Reading the result without being misled

| Looks like | Actually means |
|---|---|
| Revision `Healthy`, 0 replicas | **Provisioning only.** No container started. Three startup failures once hid behind a `Healthy` revision: exit 150 (wrong base image), exit 2 (`Settings/` never reached the image), and `no such file or directory` (Windows publish has no Linux apphost) |
| Work queue drained | Nothing on its own — *non-2xx → dead-letter **and** ack* also drains it. Check `succeeded`/`failed` and dead-letter depth |
| Peak = 1 on a burst | `value` too high, not a broken scaler |
| More replicas than `maxReplicas` | Expected during the drain test — replacements start while old replicas drain |
| `/Secure` → 500 | Read the **body**: `Workflow file not found` = not staged; bare nested `Error{…}` = authorization denial (WOLF-8418 returns 500, not 403); a workflow-level message = found, authorized, **executed** — a pass |
| Any route → 404 | The `/api` prefix was used; `host.json` sets `routePrefix: ""` |
| Every route → 403 `Site Disabled` | The Function App is **stopped** |
| `succeeded` ≠ `distinctBodies` | Redelivery or duplicate execution |

---

## Parameters worth knowing

### `New-WwE2EStaging.ps1`

| Parameter | Default | Notes |
|---|---|---|
| `-StageRoot` | `G:\Deployment` | Relocates items 1–8 together |
| `-RunSuffix` | generated | 4–8 lowercase alphanumerics; `stwwe2e<suffix>` must stay ≤ 24 chars |
| `-Publish` | off | Publishes both apps into items 1 and 2, **sequentially** — shared project references make parallel builds contend on `obj\`. Needs the repo |
| `-EnableElasticsearch` | off | Engine only. Requires item 4, named exactly `ElasticsearchLoggingSource.bite` |
| `-GenerateTriggers` | off | Isolated suffixed queues + a generated plaintext source. Needs `-AmqpUri` and the repo |
| `-WorkflowsSourcePath` | `C:\ProgramData\Warewolf\Resources` | **Not optional**: the engine publish contains no `Resources` folder, so without it every route 404s |
| `-SkipQueueContentionCheck` | off | Skips the competing-consumer check; recorded as `skipped`, not `clear` |
| `-RepoRoot` | inferred **and validated** | Needed only for `-Publish` / `-GenerateTriggers`. Inference is verified against `Dev\Warewolf.Execution.{Lightweight,QueueProcessor}`, so running from a copied location cannot mis-detect it |
| `-SuccessConcurrency` / `-FailureConcurrency` | 3 / 1 | `-GenerateTriggers` only; become `maxReplicas` |
| `-Prefetch` | 1 | `-GenerateTriggers` only. Otherwise settable **only** in the trigger — anything higher parks messages where the scaler cannot see them |
| `-InputName` | `message` | `-GenerateTriggers` only. Must match what the workflow reads, or it binds nothing |

### `Invoke-WwE2EVerification.ps1`

| Parameter | Default | Notes |
|---|---|---|
| `-Execute` | off | Without it, a dry run |
| `-TeardownWhenDone` | off | Evidence survives by default |
| `-BurstSize` | 5 | Expected peak = `min(ceil(BurstSize / MaxConcurrency), Concurrency)` |
| `-MaxConcurrency` | 1 | Dispatch is serial per channel. **Scale out, not up** |
| `-EngineLogLevel` | `ERROR` | Engine app setting |
| `-SkipDrainTest` / `-SkipUnackedProbe` | off | Shorten a run; the criteria are marked `SKIP` |
| `-SkipTopologyCreate` | off | Use when the queues already exist and must not be re-declared |
| `-AmqpUri` | resolved from the source | Needed when the source is WFAES-encrypted |
| `-RabbitMqSecretName` | per-run, else `rabbitmq-uri` | Key Vault secret holding the URI for the KEDA rule |
| `-ResourceGroup`, `-KeyVaultName`, `-AcrName`, `-AcaEnvironment` | shared `DEV2` values | Override for another environment |

### `Get-WwQueueRunReport.ps1` — per-message report

Read-only, and driven by a **time window** rather than a live watch, so it re-renders any past burst
still inside workspace retention:

```powershell
.\Get-WwQueueRunReport.ps1 -AppNamePrefix wwqp3- -LastMinutes 45 `
    -IncludeEngine -EngineAppInsightsName <engine>-ai `
    -ExpectedManifest <manifest.json>
```

| Parameter | Default | Notes |
|---|---|---|
| `-AppName` / `-AppNamePrefix` | — | One or the other; the prefix form discovers the apps |
| `-LastMinutes` | 60 | Ignored when `-StartUtc` is given |
| `-StartUtc` / `-EndUtc` | — | Explicit window; use these to re-render a historical run |
| `-ExpectedManifest` | — | JSON of `{ txn, queue, kind }`. Enables **reconciliation** — missing, duplicated and wrong-outcome — instead of counting whatever is in the logs |
| `-IncludeEngine` + `-EngineAppInsightsName` | off | Joins to the engine's App Insights on `ExecutionId` |
| `-ShowAllRows` | off | Print every row rather than first 40 / last 10 |

Produces `txn → replica → revision → startedUtc → durationMs → outcome` per delivery, replica
distribution, per-queue percentiles and throughput, reliability counts, plus a CSV and JSON.

**The `EngStatus` / `EngBody` columns are where a dead-letter is triaged.** `DeadLettered(acked)` is
a *symptom*: the worker only ever takes that path on a non-2xx from the engine
(`EngineForwarder.cs`), so the status code is the finding. The worker logs it itself
(`EngineWorkflowClient.cs:154-159`), which means these columns are populated straight from the
container logs — **no `-IncludeEngine`, no App Insights and no `EXECUTIONLOGLEVEL` change required**.
Read them first:

| `EngStatus` | What it means | Where to look |
|---|---|---|
| `401` / `403` | Rejected by EasyAuth **before** the engine app ran | The caller's managed identity, the audience/scope, `authsettingsV2` — *never* the engine's own logs, which will be empty by construction |
| `404` | The workflow is absent from the **deployed** engine's `Resources`, whatever the repo contains | The publish output |
| `500` | Overloaded by design — a workflow error, a WOLF-8418 authorization denial, or host exhaustion | The `EngBody` column, not the status |
| `timed out after …` / `failed` | Transport, not HTTP. Ends in a **redelivery**, not a dead-letter | Reachability, TLS/DNS, token acquisition |

A dead-letter reported with a blank `EngStatus` means the status line was not parsed — either the
window clipped it or the C# message changed and the script's regex needs updating. The report says
so explicitly rather than leaving the column empty and silent.

**Three things that decide whether the output means anything:**

- **The join key is the AMQP `CorrelationId`,** which the pump copies into
  `Warewolf-Custom-Transaction-Id` and which renders as `[Txn:…]` on every worker line for that
  delivery. Stamp a unique one per published message. It is the only way to track a *failure* message,
  whose body is empty and therefore matches nothing.
- **Engine-side evidence needs `EXECUTIONLOGLEVEL=INFO`.** `AuditExecutionLogger` writes only
  ERROR/FATAL, so at the default `ERROR` a *successful* execution logs nothing at all. Raise it before
  the burst, restore it after.
- **Zero rows is not "nothing happened".** The replica column is `ContainerGroupName_s`; there is no
  `ReplicaName_s` on that table, and because `Invoke-E2ELogAnalytics` passes `-AllowFail` a bad column
  makes the query return an **empty result rather than an error**.

---

## Teardown

Each run's markdown summary carries its own commands. See
[Deploy-E2E-Rollback-Commands.md](Deploy-E2E-Rollback-Commands.md) for the full procedure and the
guard rails — in short:

⛔ **Never `az group delete`.** ⛔ Never delete the ACA environment, the registry, the workspace, or the
shared AES key secret. Delete only the run-suffixed Container Apps, this run's **own** image
repository and KEDA secret, its broker topology, and the engine via
`Rollback-WwExecutionEngine.ps1 -SummaryPath <engine summary>`.

The Entra app registration lives **outside** the resource group and survives resource-level teardown;
the rollback script removes it when the summary records `created.entraApp = true`. If an engine was
deployed twice under one name, only the **first** summary records `created.functionApp = true` — point
rollback at that one, or it will under-delete.

---

## Known limitations

- **Criterion 17 is not stressed** by a fast workflow. With `Prefetch = 1` and sub-second executions,
  the drain path reports `draining 0 in-flight message(s)` — it engages correctly but never consumes
  `-ShutdownGraceSeconds`. The `succeeded` vs `distinctBodies` comparison is what proves no loss. This
  matters because criterion 13 usually measures `ReadyOnly`: unacked work is invisible to the scaler,
  so the drain path is the only protection for in-flight messages.
- **Criterion 8 is weaker than it looks** here. The harness creates the queues itself, so it proves a
  trigger consumes a queue *whose durability it did not set*. It cannot prove the publisher created
  them, because `PublishRabbitMQActivity` **cannot** create a queue or exchange at all — see
  [Deploy-E2E-Execution-Summary.md](Deploy-E2E-Execution-Summary.md) **S11**.
- **`multipart/form-data` triggers are flagged, not verified.** Whether an `@`-prefixed
  `MapEntireMessage` input binds depends on the deployed **engine build**, and nothing detects a
  mismatch at runtime — a worker against an older engine dead-letters every such message while
  appearing to process normally.
- **TLS is off by default**, matching `PublishRabbitMQActivity`. The worker logs a warning per app.
  Production requires `amqps` + `RABBITMQ__USESSL=true` — a documented go-live gate.
