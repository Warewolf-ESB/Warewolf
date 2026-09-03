# Queue load test — how it works, how to run it, how to read it

`Invoke-WwQueueLoadTest.ps1` publishes N messages to a RabbitMQ/LavinMQ queue, watches the
QueueProcessor Container App replicas drain them, and reconciles what actually happened against
what was published.

Every default reproduces **RUN 2** (2026-08-13): 100 messages, 6 replicas, **100/100 succeeded,
0 dead-lettered, ~90 seconds**. Pressing Enter through every prompt repeats that run.

---

## 1. Why this exists — "the queue drained" proves nothing

The forwarder's contract is:

| Engine response | Worker action |
|---|---|
| 2xx | ack |
| non-2xx | dead-letter **and** ack |

**Both outcomes drain the queue.** Both let the replicas scale back to zero. A run that discarded a
third of its messages looks identical, from the queue's point of view, to a perfect one.

This is not hypothetical. RUN 1 (2026-08-12, 10 replicas) ended with an empty queue, zero replicas
and no obvious errors — and had processed **68 of 100** messages. The other 32 were dead-lettered
after the engine returned 502/503/504 under cold-start and scale-out, and 30 of them had **no
database row at all**, proving the workflow never ran.

So the question this script answers is not *did the queue drain* but *did every published message
reach the state it was supposed to reach, exactly once*.

---

## 2. The four buckets

Every published message lands in exactly one bucket, and the four **must** sum to the published
count. If they don't, the reconciliation itself is broken and nothing else in the report is
trustworthy.

| Bucket | Database row | Dead-letter | Meaning |
|---|:---:|:---:|---|
| **Clean** | ✅ | ❌ | The workflow ran and committed. |
| **DlqOnly** | ❌ | ✅ | Never reached the workflow. Recoverable — safe to replay. |
| **Both** | ✅ | ✅ | The workflow **committed** and the response was lost. **Replaying duplicates committed work.** |
| **Neither** | ❌ | ❌ | Silently lost. The worst outcome, and the one a drained queue hides. |

Expectation is **per message, from the manifest**: a message published with `-FailureCount` is
*supposed* to land in `DlqOnly`, and is scored as a pass when it does. A valid message that
dead-letters, or a deliberate failure that succeeds, is a `WrongOutcome` and fails the run.

Two messages hit `Both` in RUN 1 — the two slowest, at ~5.4 s. They finished, the response was lost,
and they were dead-lettered anyway. Without this bucket they would have been double-counted as
successes.

---

## 3. Setup

### Prerequisites

| Requirement | Why |
|---|---|
| PowerShell **7+** | `Invoke-WebRequest -SkipHttpErrorCheck`, `ForEach-Object -Parallel`, `??` |
| Azure CLI, signed in | Phase 0 aborts if not — `az login` |
| **az extensions** `containerapp`, `log-analytics`, `application-insights` | Not in the base CLI. Phase 0 checks them — see below |
| Windows PowerShell **5.1** | Only for the SQL queries. `Microsoft.Data.SqlClient` needs its native SNI companion and will not load standalone; 5.1 has `System.Data.SqlClient` in-box |
| QueueProcessor publish output | Supplies `RabbitMQ.Client.dll` — default `G:\Deployment\apps\QueueProcessor` |
| .NET 8 **ASP.NET Core shared framework** | `RabbitMQ.Client` 7.x needs `System.Threading.RateLimiting`, which is absent from the publish output *correctly* (the container's `aspnet:8.0` base image provides it) |
| A deployed engine + worker | The script verifies; it does not provision |

### What must already be true

- The **queue, its exchange and the binding exist.** `PublishRabbitMQActivity` cannot create them —
  a failed passive declare closes the channel and the active declare is then reissued on that dead
  channel. The exchange is *direct and named after the queue*; the binding uses an **empty routing
  key**. Without the binding every message is silently discarded.
- **`EXECUTIONLOGLEVEL=INFO`** on the Function App. `AuditExecutionLogger` writes only ERROR/FATAL,
  so at the default a *successful* execution logs nothing and the engine column is correctly, but
  uselessly, empty.
- **No competing consumer** on the queue. Pre-flight blocks on this — see §7.

### The az extensions, and the hang they used to cause

`az monitor log-analytics query` and `az monitor app-insights query` live in **extensions**, not the
base CLI. When one is missing, az does not fail — it *asks*:

```
The command requires the extension log-analytics. Do you want to install it now? (Y/n)
```

and blocks on stdin. Because `Invoke-E2EAzJson` sends stderr to `$null`, the question is never even
displayed. A reviewer's run sat at `Querying Log Analytics (worker containers)` for **over 30
minutes** — at the *last* step, with the whole burst already published and drained.

Three things now prevent it:

1. `WwE2E.Common.psm1` sets `AZURE_EXTENSION_USE_DYNAMIC_INSTALL=yes_without_prompt` **at module
   load**, so az installs quietly instead of asking. Process-scoped; nothing changes permanently.
   This covers every entry point, including `Get-WwQueueRunReport.ps1` run on its own.
2. **Phase 0 checks the extensions** and names the step each missing one blocks, in seconds rather
   than after a full run.
3. **`-StepTimeoutSeconds`** (default 900) bounds the report step, so anything else that blocks
   fails the run instead of hanging it.

Install them up front to avoid a mid-run stall:

```powershell
az extension add --name containerapp --name log-analytics --name application-insights
```

### Every run writes a log

A transcript starts **before Phase 0** and captures everything as it happens, so an abandoned or
killed run can still be diagnosed:

```
<OutputDir>\<runLabel>.log
```

Override with `-LogFile`, or disable with `-NoTranscript`. The path is recorded in
`loadtest-summary.json`. A transcript left running by an earlier aborted run is stopped first —
PowerShell allows only one at a time, so otherwise the *next* run would fail to log exactly when it
mattered most.

### Running from the repo only, with no `G:\Deployment`

The scripts resolve each other, the shared module and the defaults file **relative to their own
folder**, so a clean checkout runs without any staging tree. `G:\Deployment` appears only as a
*default value* at prompts, and as a `Test-Path`-guarded fallback for the output folder (which drops
to `%TEMP%\ww-queue-runs` when the drive is absent).

Three inputs cannot come from the repo, because they are build or environment artefacts rather than
source:

| Input | Why | How to supply |
|---|---|---|
| QueueProcessor publish output | Supplies `RabbitMQ.Client.dll` | `dotnet publish` the worker, then `-WorkerPublishPath` |
| Broker credentials | Not committed, ever | `-BrokerSourceBitePath`, or `-AmqpUri` directly |
| SQL connection string | Not committed, ever | `-SqlConnectionString` / env var / prompt, or `-SkipDatabase` |

```powershell
# One-time: produce the publish output the AMQP client is loaded from
dotnet publish Dev\Warewolf.Execution.QueueProcessor\Warewolf.Execution.QueueProcessor.csproj `
    -c Release -o D:\QueueProcessor

cd Dev\Warewolf.Execution.Lightweight\Scripts
.\Invoke-WwQueueLoadTest.ps1 -MessageCount 20 `
    -WorkerPublishPath    D:\QueueProcessor `
    -BrokerSourceBitePath D:\sources\RabbitMQSourceAshley.bite `
    -OutputDir            D:\loadtest-out
```

Both paths are validated **at the prompt**, not several hundred lines later, and a missing file lists
what *is* in the folder:

```
  [x] RabbitMQ source .bite does not exist: 'D:\sources\RabbitMQAshley.bite'.
      Found in 'D:\sources': RabbitMQSourceAshley.bite
```

If a source cannot be read, the reason is diagnosed rather than assumed — `FileNotFound`,
`NotXml`, `NoConnectionString`, `WfAesEncrypted`, `DpapiUndecryptable` or `NoHostName` — because an
assumed cause sends the reader after the wrong problem entirely.

### Configuration

All defaults live in [`Scripts/WwLoadTest.Defaults.psd1`](../Scripts/WwLoadTest.Defaults.psd1), which
is committed and contains **nothing secret**. Credentials resolve at run time:

| Secret | Resolution |
|---|---|
| Broker password | Read from the RabbitMQ source `.bite` (plaintext or DPAPI), or `-AmqpUri` |
| SQL connection string | `-SqlConnectionString` → `$env:WWLOADTEST_SQLCONNECTION` → masked prompt → **skip DB phases** |

Set the SQL one once and every run picks it up:

```powershell
$env:WWLOADTEST_SQLCONNECTION = 'Server=...;Database=...;User ID=...;Password=...'
```

Only the server, database and auth mode are ever displayed. The connection string is passed to the
child `powershell.exe` in an **environment variable, never on the command line** — arguments are
visible in the process list to every user on the box.

---

## 4. Running it

```powershell
cd Dev\Warewolf.Execution.Lightweight\Scripts
```

**Reproduce RUN 2 interactively** — press Enter through every prompt:

```powershell
.\Invoke-WwQueueLoadTest.ps1
```

**A quick 20-message smoke test, no questions:**

```powershell
.\Invoke-WwQueueLoadTest.ps1 -MessageCount 20 -Yes
```

**100 valid plus 5 deliberate failures**, proving the dead-letter path still works, on clean queues:

```powershell
.\Invoke-WwQueueLoadTest.ps1 -MessageCount 100 -FailureCount 5 -PurgeQueues -Yes
```

**Against a different deployment:**

```powershell
.\Invoke-WwQueueLoadTest.ps1 -MessageCount 100 `
    -EngineAppName wwengine-e2e-abc123 `
    -EngineAppId   11111111-2222-3333-4444-555555555555 `
    -WorkerAppName wwqp9-ordersuccessqueue `
    -QueueName     order-success-queue `
    -MaxReplicas 6 -MaxConcurrency 1 -Yes
```

**Unattended / CI** — every value from the defaults file, exit code carries the verdict:

```powershell
.\Invoke-WwQueueLoadTest.ps1 -MessageCount 100 -NonInteractive
if ($LASTEXITCODE -ne 0) { throw 'Load test FAILED' }
```

**Without a database** — queue-side only. Note this can never be a clean PASS, because "committed"
is unproven:

```powershell
.\Invoke-WwQueueLoadTest.ps1 -MessageCount 50 -SkipDatabase -Yes
```

### Useful switches

| Switch | Effect |
|---|---|
| `-PurgeQueues` | Empties the queue and DLQ first. Off by default — the watermark isolates the run without destroying anyone's data |
| `-SkipPreWarm` | Skips Phase 5. Warned loudly; see §6 |
| `-Yes` | Skips the confirmation gate (blockers still stop the run) |
| `-NonInteractive` | No prompts at all; implies `-Yes` |
| `-Mode` | `Existing` (default), `DeployWorker`, `DeployAll` |
| `-OutputDir` | Where artefacts land |

---

## 5. The phases

| # | Phase | What it does | Mutates? |
|---|---|---|---|
| 0 | Azure login | Prints signed-in user, subscription name **and id**, tenant | no |
| 1 | Mode | Existing / DeployWorker / DeployAll | no |
| 2 | Targets | One prompt per value, RUN 2 default pre-filled | no |
| 3 | Pre-flight | Engine, ACA, KEDA, broker, database — then blockers and a gate | no |
| 4 | Baseline | Optional purge; always a `MAX(JobLogId)` watermark | purge only |
| 5 | Pre-warm | Sequential until latency settles, then **ramping** to target concurrency | executes workflows |
| 6 | Publish | N unique messages + manifest | **yes** |
| 7 | Drain | Polls until **rows** stop rising | no |
| 8 | Report | Worker logs, engine logs, database, DLQ | no |
| 9 | Verdict | PASS/FAIL with every failing criterion named | no |

**Nothing is ever truncated.** The watermark makes every query count only rows above it, which
isolates the run without destroying data.

---

## 6. Sizing the run — the number that decides success

```
concurrent engine requests = maxReplicas × WORKER__MAXCONCURRENCY
```

With `MaxConcurrency=1` that is simply the replica count. **Both operands matter** — halving
replicas while doubling concurrency changes nothing.

Measured directly against a Consumption-plan engine:

| Concurrency | Result |
|---|---|
| 6 | **24/24 clean** |
| 8 | **24/24 clean**, max 4.7 s |
| 10 | 26/30 — `Insufficient memory to continue the execution of the program` |

Memory rose **396 MB → 712 MB** across 10 concurrent executions (~32 MB per execution) against the
~1.5 GB a Consumption instance gets. 6 leaves clear margin; the script warns above 8.

**Re-measured 2026-08-20 against `wwengine3` (also `Y1/Dynamic`), 1000 messages per run.** The failure
mode reproduces, but the threshold sits higher than the table above — treat those numbers as
deployment-specific, not universal:

| Ceiling | Drain (publish → all accounted) | Throughput | Succeeded | Dead-lettered | Duplicates |
|---|---|---|---|---|---|
| 1 | 43.4 min | 0.38 msg/s | 1000 | 0 | 0 |
| 2 | 23.6 min | 0.71 msg/s | 1000 | 0 | 0 |
| 6 | 19.0 min | 0.88 msg/s | 997 | 3 | 0 |
| 10 | 14.3 min | 1.17 msg/s | **1000** | **0** | **0** |
| 20 | **5.6 min** | 2.96 msg/s | 992 | 8 | **2** |

Two corrections to the older guidance. **10 replicas did not OOM** here — that run was 1000/1000
clean and the second-fastest of the set. **20 did**, with the same `Insufficient memory` signature,
and the stack shows it failing during *assembly loading* (`PEReader`, `StreamMemoryBlockProvider`) —
i.e. **before** `usp_jobs1_LogStart`, so those messages leave **no database row at all** and
`usp_jobs1_Summary` cannot see them. Always reconcile against the publisher manifest.

Throughput keeps improving to 20 but sub-linearly: **7.7× for 20× the compute**, per-replica
efficiency falling 100% → 92% → 38% → 30% → 39%. 10 was the best all-round setting measured — 3× the
single-replica throughput with zero losses; 2 was the most efficient.

A 500 from an OOM is classified `BUSINESS` and **dead-lettered permanently**, while a 502 from the
same event is classified `TRANSPORT` and retried successfully. Valid messages are lost or kept
depending only on which status the dying host happened to return.

> **`maxReplicas` is not a deploy flag.** `Deploy-WwQueueProcessor.ps1` derives it from the trigger's
> `Concurrency` field. To change it, edit a **copy** of the trigger `.bite` — never the staged
> original — and redeploy:
>
> ```powershell
> $f = "$run\triggers\03fb9052-7fe4-4e8b-ac18-53779b0ebcba.bite"
> (Get-Content $f -Raw) -replace '"Concurrency":\s*\d+','"Concurrency": 6' | Set-Content $f -NoNewline
> ```

### Varying the ceiling in `Mode Existing` — use `-ApplyScale`

`-MaxReplicas` on its own is **planning-only** in `Mode Existing`: the run's real ceiling is read from
the deployed Container App, and the flag only feeds the `maxReplicas × MaxConcurrency` arithmetic in
the plan. Passing a value that disagrees with the deployment is now a **pre-flight blocker**, because
the alternative is worse than stopping — on 2026-08-20 a run invoked with `-MaxReplicas 10` against a
worker deployed at 20 executed at 20 while its plan, its summary and every derived measurement said
10, and a 20-replica result was filed as a 10-replica one.

Two ways to set it deliberately:

```powershell
# let the script do it (mints a new revision, records scaleBefore/scaleAfter in the summary)
.\Invoke-WwQueueLoadTest.ps1 -MessageCount 1000 -MaxReplicas 10 -ApplyScale -Yes

# or set it yourself first, then run with a matching -MaxReplicas
az containerapp update -g DEV2 -n wwqp3-ordersuccessqueue --max-replicas 10
```

`-ApplyScale` does **not** restore the previous ceiling when the run ends — reverting infrastructure
on the way out is surprising, and a failed run would revert half-way. The run prints the restore
command and records it as `scale.restoreHint` in `loadtest-summary.json`.

Two things the summary now makes explicit, both worth checking before you trust a measurement:
`scale.deployed` (what the run used) and `concurrency`, which is derived from `scale.deployed` and
never from `targets.MaxReplicas`.

### Pre-warming is not optional

On the Consumption (Y1) plan the platform adds instances gradually and sheds load while it does. A
burst arriving into that window fails because the request never reaches the workflow.

| | RUN 1 (no pre-warm, 10 replicas) | RUN 2 (pre-warmed, 6 replicas) |
|---|---|---|
| Succeeded | 68 | **100** |
| Dead-lettered | 32 (502×17, 503×8, 504×6, 500×1) | **0** |
| Drain time | ~4 min | **~90 s** |

Measured cold start: **64,757 ms**, settling to ~3,100 ms by the fifth call.

Fewer replicas processed everything *and* finished faster. The RUN 1 failures were pure waste.

#### Phase B ramps — it does not open at full concurrency

Phase B runs `-ConcurrentRounds` rounds along a ladder that reaches `-TargetConcurrency` on the
**last** round: `ceil(Target / 2^(Rounds - i))`, so `-TargetConcurrency 20` over 3 rounds is
**5 → 10 → 20**, and the queue path's 6 is **2 → 3 → 6**.

Opening straight at the target defeats the point of warming: it slams a still-single-instance app
with `Target × 3` simultaneous executions, which is the same load the pre-warm exists to protect
the burst from. `pipeline-LOADTEST.yml`'s 2026-08-24 run opened at 20 and recorded `OK=22/60`,
median **41.7 s**, max **159.9 s** (`500×16 502×20 503×2`).

Because the final round is always the full target, `[+] WARM — the final round was clean at
concurrency N` still means *clean at target concurrency* and nothing weaker. The ramp also costs
fewer calls — 105 rather than 180 for a target of 20 — so it writes fewer warm-up rows.

#### An unwarmable engine warns; it never fails the caller

`Invoke-WwEnginePreWarm.ps1` **always exits 0** once its parameters validate. A cold engine is the
condition the script exists to detect and the caller is already equipped to retry or measure — it
is not a reason to abort the stage. Only parameter validation throws.

A per-call `-TimeoutSec` expiry is therefore reported as a `TIMEOUT` result and counted in the
round, not raised as an error. This is load-bearing: `-SkipHttpErrorCheck` suppresses non-2xx
*status codes* only, and before 2026-08-24 a single timeout inside Phase B terminated the whole
pipeline step. See `docs/ShovelBridge-Architecture.md`'s 2026-08-24 entry.

Read the verdict alongside the load-test result: the run proceeds even when warming failed, so a
poor load-test outcome that follows `[!] the final round still had failures` is likely an engine
capacity problem rather than a product defect.

### How warm-up traffic is kept out of the result

The pre-warm **executes the real workflow**. It commits real rows and drives real engine HTTP
requests, so it contaminates the result in two different ways that need two different mechanisms:

| Contamination | Mechanism |
|---|---|
| **Database rows** | The run watermark is read **after** warming. Every later query counts only `JobLogId > watermark`, so warm-up rows fall below it |
| **Engine requests** | The App Insights window is time-based, not watermarked, so `$startUtc` is **clamped** to the moment the pre-warm ended |
| **Worker logs** | Nothing needed — the pre-warm calls the engine directly over HTTP and never touches the queue, so it produces no worker log lines |

The clamp matters more than it looks. `$startUtc` carries a minute of slack for clock skew; on a
*warm* engine Phase 5 finishes in well under a minute, and without the clamp that slack reaches back
into the warm-up and folds its requests into the run's result-code distribution and latency
percentiles.

Two watermarks are taken — one before warming, one after — and the difference is reported:

```
  [+] Baseline watermark (before pre-warm): JobLogId = 44
  [+] Run watermark: JobLogId > 62
  [-] Pre-warm wrote 18 row(s) (JobLogId 45-62). They are EXCLUDED from every count below
      and are not deleted.
```

**Warm-up rows are never deleted.** Excluding them by watermark is safer than removing them: it
cannot destroy anyone else's data, and it works even when the table is shared. Both watermarks and
the pre-warm row count are recorded in `loadtest-summary.json`.

---

## 7. Pre-flight blockers

Each of these produces a result that looks like a product defect and is not, so the run stops before
publishing a single message.

| Blocker | What it would look like otherwise |
|---|---|
| Engine not `Running` | 403 *Site Disabled* on every route — reads as an auth failure |
| Container App missing | Nothing consumes; the run times out |
| `maxReplicas = 0` | App is parked; queue fills and never drains |
| No `rabbitmq` scale rule | Worker never scales off queue depth |
| KEDA rule on a **different queue** | Worker sits at zero replicas while the queue fills — reads as a dead engine |
| **Competing consumer** | See below |
| Queue absent on the broker | First publish fails with `NOT_FOUND - no exchange` |

**The competing consumer is the most damaging false pass there is.** A second Container App bound to
the same queue takes its share of the messages. If it forwards to a stopped or older engine, it
dead-letters **and acks** them — so the queue still drains, the app still scales to zero, and the run
looks clean while half the messages never executed. **Stop the other app** — `az containerapp stop -g
<rg> -n <app>` — or repoint its `rabbitmq` scale rule at another queue.

> ⚠ Earlier revisions of this guide said to park the competing app at `--max-replicas 0`. **Core az
> rejects that** — `--max-replicas must be in the range [1,1000]` (confirmed on az 2.87). Use
> `az containerapp stop`, or `az containerapp revision deactivate` for a single revision. Note that a
> *stopped* app is still reported as a competing consumer by the contention check, which reads the
> KEDA rule and not the app's `runningStatus`; repointing the rule is the only change that clears it.

The timeout chain is also checked: **`EngineTimeout ≤ ShutdownGrace < TerminationGracePeriod`**
(RUN 2: 180 / 210 / 240). KEDA counts only *ready* messages, so in-flight work is invisible to the
scaler and a replica can be scaled away underneath a running message — the drain path is the only
protection, and it only works if those three are ordered.

---

## 8. Where the data comes from

Three independent sources, joined on two keys.

### The join keys

Every worker log line is prefixed by `QueueProcessorCorrelation.GetPrefix()`:

```
[Replica:<r>] [Queue:<q>] [Tag:<deliveryTag>] [Txn:<correlationId>] [ExecutionId:<guid>] <message>
```

| Key | What it joins | Why it is the one that works |
|---|---|---|
| **Txn** | manifest ↔ worker logs ↔ DLQ ↔ database | The AMQP `CorrelationId`, stamped uniquely per message at publish. Present on **every** line including dead-letter lines. It is the **only** handle on a deliberate-failure message, whose body is empty by design |
| **ExecutionId** | worker ↔ engine | The per-message GUID `AuditingConsumerDecorator` generates and forwards as the `Warewolf-Execution-Id` header; the engine logs it in the same `[ExecutionId:…]` form |

### The sources

| Source | Query | Gives |
|---|---|---|
| **Worker** — `ContainerAppConsoleLogs_CL` (Log Analytics) | by app name + time window | replica, delivery tag, duration, outcome, dead-letter |
| **Engine** — Application Insights `traces` + `requests` | by `ExecutionId`, and result codes | engine-side lines, errors, HTTP status distribution |
| **Database** — `dbo.jobs1` above the watermark | `JSON_VALUE(MessageContent,'$.txn')` | proof of commit, `AttemptNumber`, three timestamps |
| **DLQ** — AMQP `BasicGet`, held unacked | `BasicProperties.CorrelationId` | which messages were discarded |

Why **not** App Insights for the worker: the worker's `Dev2Logger` lines never reach App Insights
`traces` — only HttpClient *dependency* telemetry does. `ContainerAppConsoleLogs_CL` is also the only
source spanning **every** replica, which `az containerapp logs show` (a single-replica live tail)
cannot.

### The files

| File | Role |
|---|---|
| [`Invoke-WwQueueLoadTest.ps1`](../Scripts/Invoke-WwQueueLoadTest.ps1) | Orchestrator, pre-flight, drain, reconciliation, verdict |
| [`Publish-WwQueueBurst.ps1`](../Scripts/Publish-WwQueueBurst.ps1) | Publishes the burst, writes the manifest |
| [`Get-WwQueueRunReport.ps1`](../Scripts/Get-WwQueueRunReport.ps1) | Parses worker logs into per-message records; correlates the engine |
| [`Invoke-WwEnginePreWarm.ps1`](../Scripts/Invoke-WwEnginePreWarm.ps1) | Two-phase warm-up |
| [`WwE2E.Common.psm1`](../Scripts/WwE2E.Common.psm1) | Broker, `az`, tokens, Log Analytics helpers |
| [`WwLoadTest.Defaults.psd1`](../Scripts/WwLoadTest.Defaults.psd1) | RUN 2 defaults (no secrets) |

Log-line shapes are parsed in PowerShell rather than KQL — the volume is small and one regex per
line-shape is far easier to keep in step with the C# strings than nested `extract()` chains. The
regexes mirror `AuditingConsumerDecorator.Consume` and `RabbitMqDeadLetterPublisher.PublishAsync`
exactly; **if those log strings change, these break.**

---

## 9. Artefacts

Written to `-OutputDir` (default `G:\Deployment\logs\<runLabel>`, or a temp folder elsewhere):

| File | Contents |
|---|---|
| `loadtest-summary.json` | Verdict, reasons, targets, pre-flight, buckets, DB stats, lost txns |
| `manifest.json` | Every published message: txn, kind, bytes, body |
| `drain-samples.json` | Queue depth, DLQ depth, replica count and row count per poll |
| `queue-run-messages-*.csv` | One row per message delivery |
| `queue-run-report-*.json` | Full worker/engine report |

---

## 10. Expected results

RUN 2, for comparison:

| Measure | Value |
|---|---|
| Delivered / succeeded | 100 / 100 |
| Dead-lettered, duplicates, redelivered | 0 / 0 / 0 |
| Peak replicas | 6 |
| Drain | ~90 s |
| Distribution | 11–20 messages per replica |
| `jobs1` rows | 100, all `FINISHED` / 200 |
| Start → Processing | avg 798 ms |
| Processing → Finished | avg 804 ms |
| Total | avg 1,602 ms · p50 1,579 · p95 1,758 · max 2,018 |

A PASS requires **all** of: four buckets sum to published; zero `Neither`; zero `Both`; every message
in its expected bucket; no duplicate rows; no redeliveries; no deliveries without a terminal outcome;
no unexpected transaction ids; and the database actually consulted.

---

## 11. Traps

Each was hit for real.

- **A manifest is per-run, and reusing `-OutputDir` used to merge them.** `Publish-WwQueueBurst`
  *appends* to an existing manifest by design, so several bursts can share one. With a fixed
  `manifest.json` filename, a second run into the same folder inherited the first run's messages —
  which had been processed long before and sit below the new watermark, so they reconciled as
  **LOST**. Observed 2026-08-14: a flawless 1000/1000 run reported `FAIL — 20 message(s) LOST`
  because a 20-message run had used the same folder. Now the filename carries the run label
  (`manifest-<runLabel>.json`) *and* reconciliation filters by label, so a shared manifest is
  harmless.
- **Engine `traces` without engine `requests` is configuration, not a fault.** `requests` telemetry
  is emitted by the Functions **host**, which needs `APPLICATIONINSIGHTS_CONNECTION_STRING`. This
  engine sets only `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`, which its own `AzureExecutionLogger`
  uses to write `traces` through its own `TelemetryClient`. So traces flow and requests never do
  (observed: 1,656 execution ids, 0 request rows). Read outcome from the worker side; engine result
  codes and request durations are simply unavailable on this deployment.
- **`TRY_CONVERT(..., 127)` needs the `T` separator.** Style 127 is strict ISO 8601, so a column
  holding `2026-08-14 09:03:15.123` — a space, which is what SQL Server renders by default —
  converts to `NULL`. All 1,000 rows came back with every latency figure blank for that reason. The
  queries now use the styleless form, and a run that still cannot read its timestamps **says so**
  rather than printing empty averages that read as "too fast to measure".
- **A missing az extension hangs rather than fails.** `extension.use_dynamic_install` defaults to
  `yes_prompt`, so az stops and asks — on stdin, with stderr suppressed, so nothing is visible. See
  the pre-flight section above. This is the single worst failure mode in the whole harness, because
  it looks exactly like a slow query.
- **Never trust the exit code.** A throwing PowerShell script leaves the previous `az` exit code in
  place, so a failed step can report 0. Read the log.
- **Zero log rows ≠ nothing happened.** `Invoke-E2ELogAnalytics` passes `-AllowFail`, so a bad column
  returns an *empty result* rather than an error. The replica column is **`ContainerGroupName_s`**;
  `ReplicaName_s` does not exist on `ContainerAppConsoleLogs_CL`.
- **`@($null).Count` is 1**, not 0 — it silently inflates "no rules" and "no replicas" counts.
- **A drained queue proves nothing.** See §1.
- **`terminationGracePeriodSeconds` is a template property**, not an env var. Verify it on the app.
- **Ad-hoc tokens expire mid-run** and surface as a wall of 401s that reads as an auth misconfiguration.
  The worker refreshes its own; the pre-warm script fetches one and reuses it.
- **Reading the DLQ with nack-requeue inside the loop re-reads the same messages.** They are held
  unacked and released by closing the connection instead.
- **Log Analytics ingestion lags 2–5 minutes.** Phase 8 waits 120 s. Not a fault; do not tune around it.
- **The broker source is baked into the image at build time.** Changing brokers needs a rebuild —
  updating Key Vault alone leaves the old endpoint inside the container.
- **`Concurrency` is `maxReplicas`.** Editing the staged trigger instead of a copy changes the next
  real deployment.

---

## 12. Tests

```powershell
Import-Module Pester -RequiredVersion 5.7.1
Invoke-Pester -Path .\Tests\Invoke-WwQueueLoadTest.Tests.ps1, .\Tests\Publish-WwQueueBurst.Tests.ps1
```

129 tests, no Azure, no broker, no database — `-LoadFunctionsOnly` dot-sources the decision logic in
isolation. They cover the concurrency arithmetic, bucket classification, verdict rules, pre-flight
blockers, connection-string masking, percentiles, timestamp parsing, path validation, source
diagnosis, manifest scoping, az-extension detection, step timeouts and manifest invariants.

Eight are regression tests for defects found while building and running this:

- **numeric parameters must be nullable** — a bare `[int]` that is not supplied defaults to `0`, which
  is indistinguishable from an explicit `0`, so every unbound int overrode its RUN 2 default with zero
- **a run that published nothing must FAIL** — every "no failures" condition is vacuously true on an
  empty run, so an unguarded verdict reports a clean PASS for a run that never happened
- **no `[datetime]::TryParse` with a `[ref]` on an untyped variable** — PowerShell cannot bind it and
  fails overload resolution outright (`Cannot find an overload for "TryParse" and the argument
  count: "2"`). It threw in Phase 8, *after* a full run had been published, drained and reported —
  so the expensive part was done and the numbers were lost. Asserted against the **AST**, not the
  source text, because the helper's own doc comment names the broken pattern deliberately
- **warm-up traffic must be excluded** — the two watermarks, the reported pre-warm row count, the
  report-window clamp, and that no code path deletes rows
- **a failure cause must be diagnosed, never assumed** — `Resolve-E2EBrokerUri` returns `$null` for
  six different reasons, and the original message asserted one of them ("a WFAES-encrypted source
  needs the Key Vault key") unconditionally. A reviewer who mistyped one character in a filename was
  sent looking for a Key Vault key for a file that did not exist
- **reconciliation must be scoped to this run's label** — a shared `-OutputDir` merged two runs'
  manifests and reported the earlier run's 20 messages as LOST from a flawless 1000/1000 run. The
  filename now carries the run label, the label filter is belt-and-braces, and a count mismatch
  *within* a run throws instead of reconciling against numbers that cannot describe what was sent
- **evidence on screen must be acted on** — the failing run printed `Distinct bodies 1020 / 1000`
  and carried straight on. Unactioned evidence is worse than none: it makes the eventual FAIL look
  like a product defect
- **a missing az extension must not be able to hang the run** — the extension list is checked in
  Phase 0, `Initialize-WwAzNonInteractive` is asserted to run before the first az call, and the
  report step is bounded by a timeout that names the step and points at stdin as the usual cause

---

## Related

| Document | Purpose |
|---|---|
| [`RUN-LoadTest-Runbook.md`](RUN-LoadTest-Runbook.md) | The manual 7-step runbook this script automates |
| [`E2E-Harness-README.md`](E2E-Harness-README.md) | The deploy-and-verify harness (different job: provisioning proof, not load) |
| [`QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md`](QueueWorker-Migration-To-AzureContainerApps-KEDA-Plan-Step-By-Step.md) | Concurrency/prefetch → KEDA mapping, design rationale |
| [`Deploy-EndToEnd-Runbook.md`](Deploy-EndToEnd-Runbook.md) | §8 QueueProcessors (ACA + KEDA) |
