# KB: Performance Benchmarking — Azure Telemetry
**Product:** Warewolf Execution Lightweight Engine
**Function App:** `wwenginenewscriptai` (Resource Group: `DEV2`, Region: South Africa North)
**Script:** `docs/perf/Measure-FunctionPerf.ps1`
**Telemetry Reference:** `docs/azure-functions-telemetry.md`
**Results of record:** `docs/perf/RESULTS.md`

---

## ⚠️ Read before running

- **Two phases take the Function App OFFLINE.** `ColdStart` and `ColdVsWarm` run `az functionapp stop`, wait 60 s, then `az functionapp start`. `wwenginenewscriptai` is a **shared DEV2 app** — coordinate with the team before running either, and never run them during a demo or an active integration test.
- **`AllNew` includes `ColdVsWarm`**, so `-Phases AllNew` also causes an outage.
- **Default `-Phases` (no arguments) includes `ColdStart` and `IdleColdStart`** — this stops the app and then idles for 16 minutes. Always pass an explicit `-Phases` unless you intend the full default run.
- **RBAC:** stop/start requires **Contributor** or **Website Contributor** on the Function App. Read-only Reader + Monitoring Reader is sufficient only for `Warm`, `Concurrency`, `Seq5`, `LoadSeq`, `LoadPar`, and the telemetry queries.
- **Plan SKU:** every number in this article is from a **Consumption (Dynamic)** plan in South Africa North. Cold start dominates all results. Figures are not transferable to Premium or Dedicated plans.
- **Cost:** repeated 100-request load runs incur Function execution charges and App Insights ingestion cost. Keep `EXECUTIONLOGLEVEL=INFO` for load runs — `TRACE` multiplies ingestion volume.

---

## Overview

This article describes how to run performance benchmarks against the Warewolf Lightweight Azure Function App and how to read the results — both from the harness script (client-side timings) and from Azure Monitor / Application Insights (server-side telemetry).

The benchmarking harness has 12 phase flags (see Quick Reference). The seven scenarios most commonly run are:

| Scenario | Phase flag(s) | What it measures |
|---|---|---|
| Cold start | `ColdStart` | First-request latency after stop/start |
| Warm steady-state | `Warm` | Per-request latency once the instance is hot |
| Sequential vs parallel | `Concurrency` | Throughput and scale-out with Dice Roll workflow |
| High-concurrency burst | `HighConcurrency` | 100-request parallel burst; best observed throughput |
| 5 sequential (individual timings) | `Seq5` | Per-request latency, no competition |
| 5 parallel (individual timings) | `Par5` | Per-request latency under concurrency + instance check |
| Cold vs warm comparison | `ColdVsWarm` | Side-by-side cold/warm for Seq5 and Par5 |

For a full run use `-Phases AllNew` (includes `Seq5`, `Par5`, `ColdVsWarm`, `LoadSeq`, `LoadPar`) or `-Phases @("ColdStart","Warm","Concurrency","AllNew")`. See [AllNew execution order](#allnew-execution-order-matters-when-reading-results) before you do.

---

## Architecture: How Telemetry Flows

Understanding which tables receive data is critical before querying results.

```
Function App (dotnet-isolated)
        |
        +-- WAREWOLF_APPINSIGHTS_CONNECTION_STRING  --> App Insights worker SDK
        |       Populates: traces, dependencies, performanceCounters
        |       Does NOT populate: requests, exceptions  <- intentionally dormant
        |
        +-- Azure Monitor (always on, no configuration needed)
                Populates: FunctionExecutionCount, Requests, InstanceCount,
                           MemoryWorkingSet, IoReadBytesPerSecond, Http2xx/4xx/5xx, etc.
```

> **Key fact:** The `requests` and `exceptions` App Insights tables are **always empty** on this app. This is by design — `APPLICATIONINSIGHTS_CONNECTION_STRING` is not set, keeping the host AI pipeline dormant; `ENABLEAPPLICATIONINSIGHTS` is the single authoritative switch. All per-request counts come from Azure Monitor metrics; all log/trace data comes from `traces`, `dependencies`, and `performanceCounters`.

---

## Prerequisites

### 1. Tools required

```powershell
# PowerShell 7+ — required for ForEach-Object -Parallel.
# Launch scripts with pwsh, NOT powershell.exe (Windows PowerShell 5.1 fails at the first parallel phase).
pwsh --version

# Azure CLI — logged in
az login
az account show --query "{Subscription:name, Id:id}" -o table

# App Insights CLI extension
az extension add --name application-insights --allow-preview True --upgrade
```

### 2. Required workflows must be deployed

Despite the log line `Probing .../public/apis.json`, the harness **does not use** the discovered list — `Get-Targets` probes it for logging only and returns two hardcoded targets (`Measure-FunctionPerf.ps1:76–80`):

| Workflow | Used by |
|---|---|
| `/Public/Hello World.json?Name=...` | `ColdStart`, `Warm`, `IdleColdStart`, `Seq5`, `Par5`, `ColdVsWarm`, `LoadSeq`, `LoadPar` |
| `/Public/Examples/Dice Roll Example/Dice Roll.json` | `Concurrency`, `HighConcurrency` |

If either is absent, every request in the dependent phases returns 404. Verify before running:

```powershell
Invoke-WebRequest "https://wwenginenewscriptai.azurewebsites.net/Public/Hello World.json?Name=probe" -UseBasicParsing | Select-Object StatusCode
Invoke-WebRequest "https://wwenginenewscriptai.azurewebsites.net/Public/Examples/Dice Roll Example/Dice Roll.json" -UseBasicParsing | Select-Object StatusCode
```

`/Public/*` must be reachable anonymously — the harness sends no token.

### 3. Required App Settings on the Function App

| Setting | Required Value | Purpose |
|---|---|---|
| `ENABLEAPPLICATIONINSIGHTS` | `true` | Gates App Insights SDK — must be true or all AI tables stay empty |
| `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` | `InstrumentationKey=...` | Destination for traces/dependencies/perf counters |
| `EXECUTIONLOGLEVEL` | `INFO` (prod) / `TRACE` (debug) | Log verbosity — TRACE generates high volume |

```powershell
# Verify settings before benchmarking
az functionapp config appsettings list --name wwenginenewscriptai -g DEV2 `
  --query "[?name=='ENABLEAPPLICATIONINSIGHTS' || name=='WAREWOLF_APPINSIGHTS_CONNECTION_STRING' || name=='EXECUTIONLOGLEVEL'].{Key:name,Value:value}" `
  --output table
```

### 4. Set PowerShell variables for manual telemetry queries

```powershell
$RG       = "DEV2"
$FUNC_APP = "wwenginenewscriptai"
$AI_NAME  = "wwenginenewscriptai-ai"
$SUB_ID   = "dd0bc517-5cc7-4b56-bd6a-68e6140db7b3"
$RESOURCE = "/subscriptions/$SUB_ID/resourceGroups/$RG/providers/Microsoft.Web/sites/$FUNC_APP"

# Set these to bracket the specific run you are analysing.
# Read the start time from the run's .log file header line (RUN 20260724-080843 ...).
$START_TIME = "2026-07-24T08:08:00Z"   # <- replace with your run start
$END_TIME   = "2026-07-24T08:15:00Z"   # <- replace with your run end
```

> Using a 15-day window here inflates `dcount(cloud_RoleInstance)` by merging all previous benchmark runs. Always use a per-run window that brackets only the run you are analysing.

---

## Parameters

| Parameter | Default | Notes |
|---|---|---|
| `-BaseUrl` | `https://wwenginenewscriptai.azurewebsites.net` | Override to benchmark another deployment |
| `-ResourceGroup` | `DEV2` | Used for stop/start and AI queries |
| `-FunctionApp` | `wwenginenewscriptai` | Used for stop/start and Azure Monitor resource ID |
| `-AiName` | `wwenginenewscriptai-ai` | App Insights component for KQL queries |
| `-Phases` | `ColdStart, Warm, Concurrency, IdleColdStart` | **See warning above** |
| `-WarmRequests` | `30` | Warm phase count; also the `ColdVsWarm` warm-up burst size |
| `-SeqRequests` | `40` | `Concurrency` sequential leg |
| `-ParRequests` | `40` | `Concurrency` + `HighConcurrency` parallel leg |
| `-ParThrottle` | `20` | Max parallel threads for the above |
| `-IdleMinutes` | `16` | `IdleColdStart` idle wait — **blocks for this long** |
| `-SmallBatch` | `5` | `Seq5` / `Par5` request count |
| `-LoadCount` | `100` | `LoadSeq` / `LoadPar` request count |
| `-LoadThrottle` | `50` | Max parallel threads for `LoadPar` |
| `-Correlate` | off | Telemetry correlation — **see the known limitation below** |
| `-IngestWaitSeconds` | `360` | Initial App Insights ingestion wait |
| `-IngestRetries` | `6` | Retry attempts for the raw-table query |
| `-IngestRetryWait` | `60` | Seconds between retries |
| `-OutDir` | `<script dir>/runs` | Where the `.log` and `.csv` are written |
| `-LoadFunctionsOnly` | off | Dot-sources helpers without dispatching phases (for Pester tests) |

### Running with no arguments

`pwsh -File ./Measure-FunctionPerf.ps1` with no `-Phases` runs **`ColdStart`, `Warm`, `Concurrency`, `IdleColdStart`** — this **stops and starts the app** and then **idles for 16 minutes**. Expect a ~25–30 minute run with an outage at the start.

| Phase | Approx. duration | Takes app offline? |
|---|---|---|
| `ColdStart` | ~2–3 min | **Yes** |
| `Warm` (30 req) | ~25 s | No |
| `Concurrency` (40 seq + 40 par) | ~1.5 min | No |
| `HighConcurrency` (100 @50) | ~30 s | No |
| `IdleColdStart` | **~18 min** (16 min idle) | No |
| `Seq5` / `Par5` | ~5–35 s each | No |
| `ColdVsWarm` | ~4–5 min | **Yes** |
| `LoadSeq` (100) | ~2 min | No |
| `LoadPar` (100 @50) | ~1.5 min | No |
| `AllNew` | **~10 min** | **Yes** (via `ColdVsWarm`) |
| `-Correlate` | **up to ~11 min** of waiting | No |

---

## Running the Benchmarks

Navigate to the script directory first:

```powershell
cd D:\Warewolf\warewolf\Dev\Warewolf.Execution.Lightweight\docs\perf
```

### Scenario 1 — Cold Start

The script stops the function app, restarts it, fires the first request (cold), then runs steady-state requests (warm).

```powershell
pwsh -File ./Measure-FunctionPerf.ps1 -Phases ColdStart
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Warm
```

**What to look for in output:**
- `COLD START first-success latency` — the true cold start cost in milliseconds. Note that a 403 immediately before this is expected — see [First request returns HTTP 403](#first-request-after-a-cold-start-returns-http-403).
- `ColdStart STATS` log line — min/avg/p50/p95/max including the cold first request
- Difference between cold first-success and warm avg = JIT + startup overhead

---

### Scenario 2 — Warm Steady-State (`Warm`)

Sends 30 sequential requests (default) once the instance is hot.

```powershell
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Warm
```

**Measured baseline:** min 554 / avg 687 / p50 623 / p95 827 / max 894 ms (see `RESULTS.md` Phase 2).

---

### Scenario 3 — Sequential vs Parallel (`Concurrency`)

Runs `-SeqRequests` (default 40) sequentially against the Dice Roll workflow, then `-ParRequests` (default 40) in parallel. Demonstrates that parallel is slower than sequential on a cold Consumption plan due to scale-out latency.

```powershell
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Concurrency
```

**Measured baseline:** Sequential 40: avg 752 ms / 1.31 req/s. Parallel 40 @20 (cold scale-out): avg 7,870 ms / 0.74 req/s (see `RESULTS.md` Phase 3a/3b).

---

### Scenario 4 — High-Concurrency Burst (`HighConcurrency`)

100 parallel requests at throttle 50 against the Dice Roll workflow. The best-observed throughput phase.

```powershell
pwsh -File ./Measure-FunctionPerf.ps1 -Phases HighConcurrency
```

**Measured baseline:** avg 11,909 ms / 3.77 req/s / 4 instances / 0 errors (see `RESULTS.md` Phase 3c).

---

### Scenario 5 — Idle Cold Start (`IdleColdStart`)

Waits `-IdleMinutes` (default 16) for the app to scale to zero, then measures first-request latency. **Blocks for ~18 minutes.**

```powershell
pwsh -File ./Measure-FunctionPerf.ps1 -Phases IdleColdStart
```

**Measured baseline:** first-success 30,660 ms (see `RESULTS.md` Phase 4).

---

### Scenario 6 — 5 Sequential Requests (`Seq5`)

Fires 5 requests one after another and records the duration of each individually.

```powershell
# Run 5 requests one after another and measure how long each takes
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Seq5
```

**What to look for:**
- Request 1 may be slower if the instance just woke up (mini warm-up effect)
- Requests 2–5 should stabilise — large variance indicates GC pressure or cold dependency calls
- With n=5, p95 equals max (nearest-rank percentile). Use min/avg/max spread instead of p95 for small batches. Percentile statistics are in the `.log` file `STATS:` lines, not the summary table.

---

### Scenario 7 — 5 Parallel Requests (`Par5`)

Fires 5 requests simultaneously, records each request's duration, then immediately checks Azure Monitor to see how many instances handled the burst.

```powershell
# Run 5 requests all at once, measure each individually, and check how many instances scaled up
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Par5
```

**What to look for:**
- Per-request spread — a wide min/max gap means requests hit different instance states
- `scale-out: N instance(s) detected during burst` — how many instances Azure spun up
- If the metric is not yet available you will see `scale-out: unknown (metric not yet available — retry with -Correlate)` — this is normal; Azure Monitor InstanceCount lags 1–2 minutes on Consumption

---

### Scenario 8 — Cold vs Warm Comparison (`ColdVsWarm`)

Forces a cold start, runs Seq5 + Par5, warms the instance, runs Seq5 + Par5 again, then prints a side-by-side comparison.

```powershell
# Run sequential and parallel tests under both cold start and warm start, then compare
pwsh -File ./Measure-FunctionPerf.ps1 -Phases ColdVsWarm
```

**Output phases produced:** `ColdSeq5`, `ColdPar5`, `WarmSeq5`, `WarmPar5`

#### AllNew execution order matters when reading results

`AllNew` runs phases in this fixed order: `Seq5` → `Par5` → `ColdVsWarm` → `LoadSeq` → `LoadPar`.

Two consequences:

1. **The leading `Seq5`/`Par5` are thermally ambiguous** — they hit whatever state the app was already in. A real run captured `Seq5` seq 1 = 30,295.6 ms when the app had scaled to zero. Use `ColdSeq5`/`WarmSeq5` from `ColdVsWarm` for controlled comparisons.
2. **`LoadSeq`/`LoadPar` are implicitly warm** — they inherit the 30-request warm-up `ColdVsWarm` performed at its end. `AllNew` contains no cold load test. For a cold load test, run `-Phases ColdStart` then `-Phases LoadPar` as separate invocations.

---

### Scenario 9 — Load Test: Sequential (`LoadSeq`)

Sends 100 requests back-to-back. Reports progress every 10 requests.

```powershell
# Send 100 requests back-to-back and measure throughput and latency over time
pwsh -File ./Measure-FunctionPerf.ps1 -Phases LoadSeq
```

**Measured baseline (2026-07-24, 20 requests):** avg 1,254 ms / p95 3,984 ms / 0.79 req/s (see run log `perf-20260724-080843`).

**What to look for:**
- `throughput` (req/s) — single-threaded capacity
- p95 in the `.log` STATS line — should not grow over time; rising p95 = GC pressure or memory leak
- `errors` — any non-2xx under sequential load is a red flag

---

### Scenario 10 — Load Test: Parallel (`LoadPar`)

Fires all 100 requests simultaneously (throttled to `-LoadThrottle`, default 50).

```powershell
# Send 100 requests all at once and measure concurrency behaviour and scale-out
pwsh -File ./Measure-FunctionPerf.ps1 -Phases LoadPar
```

**Measured baseline (2026-07-24, 20 @10):** avg 6,087 ms / p95 17,522 ms / 1.07 req/s (see run log `perf-20260724-080843`).

**What to look for:**
- `wall-clock` — parallel wall time should be far shorter than sequential wall time
- `errors` — 429/503 = concurrency ceiling hit; `-1` = timeout (see [Status = -1](#status---1-client-side-timeout))
- `scale-out` log line — how many instances Azure created during the burst

---

### Run Everything in One Command

```powershell
# All five new phases in one go (includes ColdVsWarm — takes app offline)
pwsh -File ./Measure-FunctionPerf.ps1 -Phases AllNew

# Original cold start + warm + concurrency + all new phases
# Note: do NOT add -Correlate here unless you also want correlation for the original phases.
# For new phases, use manual queries after waiting 5-10 min (see below).
pwsh -File ./Measure-FunctionPerf.ps1 -Phases @("ColdStart","Warm","Concurrency","AllNew")

# Same as AllNew with larger batch size
pwsh -File ./Measure-FunctionPerf.ps1 -Phases AllNew -SmallBatch 10 -LoadCount 200 -LoadThrottle 50
```

---

## Reading Script Output

Every **completed** run produces two files in `docs/perf/runs/`:

| File | Content |
|---|---|
| `perf-YYYYMMDD-HHmmss.log` | Full timestamped log of every request and summary stat |
| `perf-YYYYMMDD-HHmmss.csv` | One row per request — 9 columns (see below) |

**If the run fails or is cancelled mid-phase, only the `.log` survives.** The CSV is written in a `finally` block after all phases complete, so an interrupted run may produce an empty or partial CSV — the `.log` is then the only record.

### CSV schema (9 columns)

| Column | Meaning |
|---|---|
| `Phase` | Phase label (e.g. `ColdSeq5`, `LoadPar`) |
| `Seq` | Request ordinal within the phase |
| `TimestampUtc` | UTC timestamp of request completion (`o` format) |
| `Ms` | Client-side stopwatch duration, 1 decimal place |
| `Status` | HTTP status code, or **`-1`** — see below |
| `Ok` | `True` only for a 2xx response |
| `Bytes` | Response length. **Always `0` for parallel phases** (not captured in the runspace) |
| `Url` | Target URL including query string |
| `Error` | Exception message when `Ok=False`; empty otherwise. **Always empty for `Concurrency`/`HighConcurrency`** |

> **CSV decimal separator is invariant (dot).** As of the S3 fix, `Ms` is written with a dot decimal separator regardless of machine locale. Older files created on an en-ZA machine use a comma (`30295,6`). To read old files:
> ```powershell
> Import-Csv ./runs/perf-20260724-080843.csv |
>   Select-Object Phase, Seq, @{n='Ms';e={[double]($_.Ms -replace ',','.')}}, Status, Ok
> ```

### Summary table printed at end of run

The final `Format-Table` shows 7 columns. Percentile statistics (p50, p95) are in the `.log` file `STATS:` lines only — they do not appear in the summary table.

```
Phase        N    OK   First_ms  Min_ms  Avg_ms  Max_ms
-----------  ---  ---  --------  ------  ------  ------
ColdSeq5     5    5    30295.6   623.4   7452.3  30295.6
WarmSeq5     5    5    701.2     645.1   687.4   821.0
LoadSeq      20   20   712.4     701.3   1254.1  4486.0
LoadPar      20   20   -         782.0   6087.2  18542.0
```

*(Actual output — shape varies by run. For real baseline numbers see `RESULTS.md`.)*

**Column meanings:**
- `First_ms` — latency of the very first successful request in the phase (cold start indicator); `-` for parallel phases where ordering is non-deterministic
- `Min_ms` / `Avg_ms` / `Max_ms` — fastest, mean, slowest across successful requests

> **Percentile note:** p95 is nearest-rank with no interpolation. With `-SmallBatch 5`, the p95 of a 5-sample phase equals its max — p95 is not meaningful for `Seq5`/`Par5`/`ColdSeq5`/`WarmSeq5`. Use min/avg/max for small batches, and p95 only for `LoadSeq`/`LoadPar`/`Warm` (n ≥ 30).

---

## Telemetry Correlation (App Insights + Azure Monitor)

### ⚠️ Known limitation — `-Correlate` must run after all phases

In the current script (after the S1 fix), the correlation step is dispatched **after** all phases. If you are using an older copy of the script, check that `Invoke-CorrelationPhase` is dispatched after the `AllNew` block — not before it. With the old ordering, the telemetry window closes before the new phases execute.

`-Correlate` is valid for all phase combinations. After the fix, the window spans from run start to correlation time, covering every phase.

### Running with correlation

```powershell
# Run all phases and correlate telemetry at the end (waits up to ~11 min for App Insights)
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Seq5,LoadSeq -Correlate
```

### What `-Correlate` queries

**Azure Monitor** (available within 1–2 minutes — not instant on Consumption; treat a blank `InstanceCount` result as "not yet available"):

```powershell
# Execution count and HTTP status codes during the run
az monitor metrics list --resource $RESOURCE --metric "FunctionExecutionCount,Requests,Http2xx" --interval PT1M --start-time $START_TIME --end-time $END_TIME

# Instance scale-out over time
az monitor metrics list --resource $RESOURCE --metric "InstanceCount" --interval PT1M --aggregation Maximum --start-time $START_TIME --end-time $END_TIME
```

**App Insights** (after 5–10 min ingestion lag; script waits up to ~11 min total — 6 min initial + up to 5 × 1 min retries):

```powershell
# Instance scale-out from populated tables (never queries requests/exceptions — always empty)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union traces, dependencies, performanceCounters | where timestamp between (datetime('$START_TIME') .. datetime('$END_TIME')) | summarize instances=dcount(cloud_RoleInstance), samples=count(), firstSeen=min(timestamp), lastSeen=max(timestamp)"

# Per-instance CPU and memory during the run
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp between (datetime('$START_TIME') .. datetime('$END_TIME')) | summarize avgValue=avg(value), maxValue=max(value) by name, cloud_RoleInstance | order by name asc, cloud_RoleInstance asc"
```

---

## Interpreting Results — Measured Baselines and Regression Thresholds

### Measured Consumption baseline (what this app actually does)

All figures from `RESULTS.md` and run `perf-20260724-080843`.

| Metric | Measured | Source |
|---|---|---|
| Cold start (stop/start), first success | **22,857 ms** | RESULTS.md Phase 1 |
| Cold start (idle scale-to-zero), first success | **30,660 ms** | RESULTS.md Phase 4 |
| Warm steady-state (30 seq) | min 554 / avg 687 / p50 623 / p95 827 / max 894 ms | RESULTS.md Phase 2 |
| Sequential 40 | avg 752 / p95 948 / max 2,161 ms · 30.6 s wall · **1.31 req/s** · 1 instance | RESULTS.md Phase 3a |
| Parallel 40 @20 (cold scale-out) | avg 7,870 / p50 1,355 / p95 50,306 / max 52,241 ms · **0.74 req/s** · 1–4 instances | RESULTS.md Phase 3b |
| Parallel 100 @50 (warm) | avg 11,909 / p50 11,538 / p95 21,154 / max 21,646 ms · 26.6 s wall · **3.77 req/s** · 0 errors · 4 instances | RESULTS.md Phase 3c |
| LoadSeq 20 (2026-07-24) | min 701 / avg 1,254 / p50 800 / p95 3,984 / max 4,486 ms · **0.79 req/s** | perf-20260724-080843 |
| LoadPar 20 @10 (2026-07-24) | min 782 / avg 6,087 / p50 1,002 / p95 17,522 / max 18,542 ms · 18.8 s wall · **1.07 req/s** | perf-20260724-080843 |
| Peak `% Processor Time` | 0.33–0.50 % avg / **3.04 % max** — CPU is never the bottleneck | RESULTS.md Phase 3c |
| `Private Bytes` per instance | 676 MB – 1.20 GB | RESULTS.md Phase 3c |
| Max observed scale-out | **4 instances** | RESULTS.md Phase 3c |

### Regression thresholds (use to judge a run)

| Metric | Investigate if | Rationale |
|---|---|---|
| Cold first-success | > 35,000 ms | ~15% above worst observed (30.7 s) |
| Warm avg | > 1,000 ms | ~45% above observed 687 ms |
| Warm p95 / avg ratio | > 2× | Observed 827/687 ≈ 1.2× |
| Sequential throughput | < 0.7 req/s | Observed floor 0.79 req/s |
| Error rate, 100 parallel | > 0 | Observed 0 errors at 100 @50 |
| Scale-out, 100 parallel | < 4 instances | Observed 4 |

### Aspirational targets (NOT met on Consumption)

> Cold start < 5 s · warm avg < 500 ms · sequential > 5 req/s · parallel > 20 req/s.
> On the current Consumption (Dynamic) plan every one of these fails by design — cold start alone is 23–31 s. These targets require a **Premium or always-ready plan**. Do not use them as pass/fail criteria for this app.

---

## Manual Telemetry Queries After a Benchmark Run

Set `$START_TIME` and `$END_TIME` to bracket your run (read from the `.log` header line), then:

```powershell
# Execution volume during the run
az monitor metrics list --resource $RESOURCE --metric "FunctionExecutionCount,Requests" --interval PT1M --aggregation Total --start-time $START_TIME --end-time $END_TIME

# Instance count peak during the run
az monitor metrics list --resource $RESOURCE --metric "InstanceCount" --interval PT1M --aggregation Maximum --start-time $START_TIME --end-time $END_TIME

# Memory during the run
$result = az monitor metrics list --resource $RESOURCE --metric "MemoryWorkingSet,PrivateBytes" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME | ConvertFrom-Json
$result.value | ForEach-Object { $m = $_.name.value; $_.timeseries[0].data | Where-Object { $_.average -gt 0 -or $_.maximum -gt 0 } | ForEach-Object { [PSCustomObject]@{ Metric=$m; Timestamp=$_.timeStamp; Average_MB=[math]::Round($_.average/1MB,2); Maximum_MB=[math]::Round($_.maximum/1MB,2) } } } | Sort-Object Timestamp | Format-Table -AutoSize

# CPU per instance (Consumption plan: CpuPercentage metric is empty, use performanceCounters)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp between (datetime('$START_TIME') .. datetime('$END_TIME')) | where name == '% Processor Time' | summarize avgCpu=avg(value), maxCpu=max(value) by cloud_RoleInstance | order by maxCpu desc"

# Per-instance scale-out timeline
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union traces, dependencies, performanceCounters | where timestamp between (datetime('$START_TIME') .. datetime('$END_TIME')) | summarize instances=dcount(cloud_RoleInstance) by bin(timestamp, 1m) | order by timestamp asc"

# Error and warning traces during the run
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp between (datetime('$START_TIME') .. datetime('$END_TIME')) | where severityLevel >= 2 | project timestamp, severityLevel, message, cloud_RoleInstance | order by timestamp desc"
```

---

## Troubleshooting

### First request after a cold start returns HTTP 403

**Expected, not an auth failure.** The Functions host brings the auth pipeline up before the Warewolf workflow engine finishes initialising, so a request arriving in that gap is rejected with 403. It shows in the log as a fast (~700 ms) failure immediately before the slow (~23 s) first success.

`ColdStart` and `IdleColdStart` handle this: they retry up to 15 (resp. 10) times with a 3 s back-off and report the **first successful** request as the cold-start figure. Only treat a 403 as a real fault if it persists after the host is warm — then verify EasyAuth is not enforcing auth on `/Public/*`.

### Status = -1 (client-side timeout)

`Status = -1` means no HTTP response was received — the `Error` column has the exception message. Request timeouts:

| Phase | Timeout |
|---|---|
| Sequential phases, `Seq5`, `Par5` | 120 s |
| `LoadPar`, `HighConcurrency` | 180 s |

A `-1` during a cold scale-out usually means the request was queued behind an instance that had not finished starting (observed p95 up to 52 s in `RESULTS.md` Phase 3b). Compare against the tail before concluding the app is broken.

### Script returns `{"tables": []}` for App Insights queries

App Insights raw tables have a **5–10 minute ingestion lag**. The `union | summarize count()` query shows data instantly (pre-aggregated), but filtered queries need to wait. Either use `-Correlate` (the script waits automatically) or run the queries manually after waiting.

### `ENABLEAPPLICATIONINSIGHTS` is set but App Insights shows zero data

The function app may not have been invoked since the setting was applied. A settings change triggers a restart — invoke at least one endpoint after restart, wait 5–10 minutes, then query:

```powershell
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union requests, traces, dependencies, exceptions | summarize count() by itemType"
```

### `InstanceCount` shows 0 or blank

Azure Monitor `InstanceCount` on a Consumption plan lags 1–2 minutes and frequently returns nothing when polled immediately after a burst. The in-phase `Par5`/`LoadPar` scale-out poll is best-effort — treat a blank result as "not yet available", not "1 instance". Use `-Correlate` or the manual query with an explicit window to get the post-ingestion reading.

### `CpuPercentage` returns no data on the App Service Plan

On a Consumption plan, `CpuPercentage` and `MemoryPercentage` are not populated for the App Service Plan. Use `performanceCounters` in App Insights instead:

```powershell
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp between (datetime('$START_TIME') .. datetime('$END_TIME')) | where name == '% Processor Time' | summarize avg(value) by cloud_RoleInstance, bin(timestamp, 5m)"
```

### Parallel phase produces errors (429 / 503)

The function app has hit its concurrency ceiling. Check:

```powershell
az monitor metrics list --resource $RESOURCE --metric "RequestsInApplicationQueue" --interval PT1M --start-time $START_TIME --end-time $END_TIME
az functionapp show --name $FUNC_APP -g $RG --query "siteConfig.functionAppScaleLimit" -o tsv
```

Increase if needed: `az functionapp update --name $FUNC_APP -g $RG --set siteConfig.functionAppScaleLimit=200`

### `ForEach-Object -Parallel` error

Requires **PowerShell 7+**. Always launch via `pwsh`, not `powershell.exe`. Check: `pwsh --version`.

---

## Quick Reference — Phase Summary

| Phase flag | Description | Key output | Takes app offline? |
|---|---|---|---|
| `ColdStart` | Stop/start app, measure first request | `COLD START first-success latency` | **Yes** |
| `Warm` | 30 sequential steady-state requests | `Warm STATS` in `.log` | No |
| `Concurrency` | 40 seq + 40 par vs Dice Roll | Wall-clock, req/s, per-phase STATS | No |
| `HighConcurrency` | 100 parallel @50 burst | Wall-clock, req/s, errors | No |
| `IdleColdStart` | 16 min idle → scale-to-zero → first request | `IDLE COLD START first-success latency` | No |
| `Seq5` | 5 sequential, individual timings | Per-request ms, STATS in `.log` | No |
| `Par5` | 5 parallel, individual timings + instance count | Per-request ms, scale-out log line | No |
| `ColdVsWarm` | Seq5+Par5 cold then warm, side-by-side | Comparison table in `.log` | **Yes** |
| `LoadSeq` | 100 sequential load test | Throughput req/s, errors, STATS | No |
| `LoadPar` | 100 parallel load test | Wall-clock, throughput, instances, errors | No |
| `AllNew` | Seq5 → Par5 → ColdVsWarm → LoadSeq → LoadPar | Combined output | **Yes** |
| `Correlate` | App Insights + Azure Monitor correlation | Instance timeline, perf counters | No |
