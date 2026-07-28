# KB: Performance Benchmarking — Azure Telemetry
**Product:** Warewolf Execution Lightweight Engine
**Function App:** `wwenginenewscriptai` (Resource Group: `DEV2`, Region: South Africa North)
**Script:** `docs/perf/Measure-FunctionPerf.ps1`
**Telemetry Reference:** `docs/azure-functions-telemetry.md`

---

## Overview

This article describes how to run performance benchmarks against the Warewolf Lightweight Azure Function App and how to read the results — both from the harness script (client-side timings) and from Azure Monitor / Application Insights (server-side telemetry).

The benchmarking harness covers seven scenarios:

| Scenario | What it measures |
|---|---|
| Cold start vs warm start | How much the first request after a restart costs vs steady-state |
| 5 sequential requests | Per-request latency when requests are not competing |
| 5 parallel requests | Per-request latency under concurrency + how many instances scaled up |
| Cold vs warm comparison | Side-by-side of sequential and parallel under both cold and warm conditions |
| 100 sequential (load test) | Sustained single-threaded throughput and latency drift |
| 100 parallel (load test) | Concurrency ceiling, scale-out behaviour, error rate under load |
| Telemetry correlation | App Insights + Azure Monitor data correlated to the client-side run |

---

## Architecture: How Telemetry Flows

Understanding which tables receive data is critical before querying results.

```
Function App (dotnet-isolated)
        │
        ├─ WAREWOLF_APPINSIGHTS_CONNECTION_STRING  ──► App Insights worker SDK
        │       Populates: traces, dependencies, performanceCounters
        │       Does NOT populate: requests, exceptions  ← intentionally dormant
        │
        └─ Azure Monitor (always on, no configuration needed)
                Populates: FunctionExecutionCount, Requests, InstanceCount,
                           MemoryWorkingSet, IoReadBytesPerSecond, Http2xx/4xx/5xx, etc.
```

> **Key fact:** The `requests` and `exceptions` App Insights tables are **always empty** on this app. This is by design — the standard `APPLICATIONINSIGHTS_CONNECTION_STRING` is not set, keeping the host AI pipeline dormant so `ENABLEAPPLICATIONINSIGHTS` is the single authoritative switch. All per-request data comes from Azure Monitor metrics; all log/trace data comes from the `traces`, `dependencies`, and `performanceCounters` App Insights tables.

---

## Prerequisites

### 1. Tools required

```powershell
# PowerShell 7+ (required for ForEach-Object -Parallel)
pwsh --version

# Azure CLI logged in
az login
az account show --query "{Subscription:name, Id:id}" -o table

# App Insights CLI extension
az extension add --name application-insights --allow-preview True --upgrade
```

### 2. Required App Settings on the Function App

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

### 3. Set PowerShell variables

```powershell
$RG           = "DEV2"
$FUNC_APP     = "wwenginenewscriptai"
$AI_NAME      = "wwenginenewscriptai-ai"
$SUB_ID       = "dd0bc517-5cc7-4b56-bd6a-68e6140db7b3"
$APP_ID       = "1e202c97-2eaa-44ea-83f5-5f75508d3bf0"
$END_TIME     = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$START_TIME   = (Get-Date).AddDays(-15).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$RESOURCE     = "/subscriptions/$SUB_ID/resourceGroups/$RG/providers/Microsoft.Web/sites/$FUNC_APP"
$ASP_RESOURCE = az functionapp show --name $FUNC_APP -g $RG --query "serverFarmId" -o tsv
```

---

## Running the Benchmarks

All benchmarks are run via `Measure-FunctionPerf.ps1`. Navigate to the script directory first:

```powershell
cd D:\Warewolf\warewolf\Dev\Warewolf.Execution.Lightweight\docs\perf
```

### Scenario 1 — Cold Start vs Warm Start

The script stops the function app, restarts it, fires the first request (cold), then runs steady-state requests (warm).

```powershell
.\Measure-FunctionPerf.ps1 -Phases ColdStart
.\Measure-FunctionPerf.ps1 -Phases Warm
```

**What to look for in output:**
- `COLD START first-success latency` — the true cold start cost in milliseconds
- `Warm STATS` — `min`, `avg`, `p95`, `max` once the instance is hot
- Difference between cold first-success and warm avg = JIT + startup overhead

---

### Scenario 2 — 5 Sequential Requests (individual timings)

Fires 5 requests one after another and records the time for each individually.

```powershell
# Run 5 requests one after another and measure how long each takes
.\Measure-FunctionPerf.ps1 -Phases Seq5
```

**What to look for:**
- Request 1 may be slower if the instance just woke up (mini warm-up effect)
- Requests 2–5 should stabilise — large variance here indicates GC pressure or cold dependency calls
- Check `p95` vs `avg` — a high p95/avg ratio means occasional spikes

---

### Scenario 3 — 5 Parallel Requests (individual timings + instance check)

Fires 5 requests simultaneously, records each request's duration, then immediately checks Azure Monitor to see how many instances handled the burst.

```powershell
# Run 5 requests all at once, measure each individually, and check how many instances scaled up
.\Measure-FunctionPerf.ps1 -Phases Par5
```

**What to look for:**
- Individual timings — parallel requests should not be significantly slower than sequential if the app scales out
- `scale-out: N instance(s) detected` — how many instances Azure spun up to absorb 5 concurrent requests
- If scale-out = 1, all 5 landed on the same instance; if = 5, each got its own

---

### Scenario 4 — Cold vs Warm Comparison

The most comprehensive single run. Forces a cold start, runs Seq5 + Par5, warms up the instance, runs Seq5 + Par5 again, then prints a side-by-side comparison.

```powershell
# Run sequential and parallel tests under both cold start and warm start, then compare results
.\Measure-FunctionPerf.ps1 -Phases ColdVsWarm
```

**Output phases produced:** `ColdSeq5`, `ColdPar5`, `WarmSeq5`, `WarmPar5`

**What to look for:**

| Comparison | Healthy baseline |
|---|---|
| ColdSeq5 first vs WarmSeq5 avg | Cold first request ≤ 10× warm avg |
| ColdPar5 max vs WarmPar5 max | Cold parallel max ≤ 5× warm parallel max |
| ColdPar5 instances vs WarmPar5 instances | Same or more on cold (scale-out is faster when instances = 0) |

---

### Scenario 5 — Load Test: 100 Sequential Requests

Sends 100 requests back-to-back. Reports progress every 10 requests, final throughput and error count.

```powershell
# Send 100 requests back-to-back and measure throughput and latency over time
.\Measure-FunctionPerf.ps1 -Phases LoadSeq
```

**What to look for:**
- `throughput` (req/s) — baseline single-threaded capacity
- `p95` vs `avg` — should stay stable across 100 requests; rising p95 over time = memory/GC issue
- `errors` — any non-2xx under sequential load is a red flag
- Run with `-Correlate` to see Azure Monitor `FunctionExecutionCount` corroborate the count

---

### Scenario 6 — Load Test: 100 Parallel Requests

Fires all 100 requests simultaneously (throttled to `$LoadThrottle` threads, default 50). Measures per-request timing and checks instance scale-out after the burst.

```powershell
# Send 100 requests all at once and measure concurrency behaviour and scale-out
.\Measure-FunctionPerf.ps1 -Phases LoadPar
```

**What to look for:**
- `wall-clock` time — parallel wall time should be far shorter than sequential wall time
- `throughput` — effective req/s under concurrency
- `errors` — 429 (throttled) or 503 (unavailable) indicate the app hit its concurrency ceiling
- `scale-out: N instance(s)` — how many instances Azure created during the burst; higher = better horizontal scaling

---

### Run Everything in One Command

```powershell
# Run all five new phases above in one go
.\Measure-FunctionPerf.ps1 -Phases AllNew

# Run everything — original cold start, warm, concurrency phases plus all new ones, then pull telemetry from App Insights
.\Measure-FunctionPerf.ps1 -Phases @("ColdStart","Warm","Concurrency","AllNew") -Correlate

# Same as AllNew but with a larger batch size and higher load count
.\Measure-FunctionPerf.ps1 -Phases AllNew -SmallBatch 10 -LoadCount 200 -LoadThrottle 50
```

---

## Reading Script Output

Every run produces two files in `docs/perf/runs/`:

| File | Content |
|---|---|
| `perf-YYYYMMDD-HHmmss.log` | Full timestamped log of every request and summary stat |
| `perf-YYYYMMDD-HHmmss.csv` | One row per request — Phase, Seq, Ms, Status, Ok, Url |

### Summary table printed at end of run

```
Phase        N    OK   First_ms  Min_ms  Avg_ms  Max_ms
-----------  ---  ---  --------  ------  ------  ------
ColdSeq5     5    5    3241.0    210.4   642.1   3241.0
WarmSeq5     5    5    198.3     195.1   201.7   212.4
ColdPar5     5    5    2876.0    876.0   1432.6  2876.0
WarmPar5     5    5    204.1     198.2   205.3   221.0
LoadSeq      100  100  201.4     193.0   207.3   389.1
LoadPar      100  98   -         124.0   312.8   4201.0
```

**Reading the columns:**
- `First_ms` — latency of the very first successful request in the phase (cold start indicator)
- `Min_ms` — fastest individual request
- `Avg_ms` — mean across all successful requests
- `Max_ms` — slowest individual request (outlier indicator)

---

## Telemetry Correlation (App Insights + Azure Monitor)

Run with `-Correlate` to automatically query server-side telemetry after the load phases. The script waits for App Insights ingestion (default 6 minutes) then queries:

```powershell
.\Measure-FunctionPerf.ps1 -Phases AllNew -Correlate
```

### What `-Correlate` queries

**Azure Monitor (no ingestion lag — available immediately):**
```powershell
# Execution count and HTTP status codes during the run
az monitor metrics list --resource $RESOURCE --metric "FunctionExecutionCount,Requests,Http2xx" --interval PT1M --start-time $START_TIME --end-time $END_TIME

# Instance scale-out over time
az monitor metrics list --resource $RESOURCE --metric "InstanceCount" --interval PT1M --aggregation Maximum --start-time $START_TIME --end-time $END_TIME
```

**App Insights (after 5–10 min ingestion lag — script waits automatically):**
```powershell
# Instance scale-out from populated tables (traces/dependencies/performanceCounters)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union traces, dependencies, performanceCounters | where timestamp > ago(1d) | summarize instances=dcount(cloud_RoleInstance), samples=count(), firstSeen=min(timestamp), lastSeen=max(timestamp)"

# Per-instance CPU and memory during the run
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(1d) | summarize avgValue=avg(value), maxValue=max(value) by name, cloud_RoleInstance | order by name asc, cloud_RoleInstance asc"
```

> **Important:** The `requests` and `exceptions` App Insights tables are always empty on this app. The `-Correlate` step never queries them. Scale-out and execution data come from `union traces, dependencies, performanceCounters` and Azure Monitor respectively.

---

## Interpreting Results — What Good Looks Like

### Cold Start

| Metric | Target | Action if exceeded |
|---|---|---|
| Cold first-success latency | < 5 000 ms | Profile startup: check `StartupOrchestrator`, Key Vault calls, index warm-up |
| Cold vs warm ratio | < 10× | Reduce startup work; consider Premium plan (pre-warmed instances) |

### Sequential Latency (Warm)

| Metric | Target | Action if exceeded |
|---|---|---|
| Avg warm latency | < 500 ms | Profile workflow execution; check Elasticsearch/dependency latency |
| p95 / avg ratio | < 2× | High ratio = GC pauses or slow dependency tail latency |

### Parallel / Scale-Out

| Metric | Target | Action if exceeded |
|---|---|---|
| Error rate under 100 parallel | < 1% | Check `RequestsInApplicationQueue`; raise `functionAppScaleLimit` if needed |
| Scale-out instances (100 parallel) | ≥ 5 | If stuck at 1, check `WEBSITE_MAX_DYNAMIC_APPLICATION_SCALE_OUT` setting |
| Parallel wall-clock vs sequential | < 20% of sequential | Low ratio = good horizontal scaling |

### Load Test

| Metric | Target | Action if exceeded |
|---|---|---|
| Sequential throughput | > 5 req/s | Optimise hot-path execution; reduce external calls |
| Parallel throughput | > 20 req/s | Check scale-out limits and instance warm-up speed |
| Latency drift over 100 requests | p95 should not grow | Growing p95 = memory leak or GC pressure — check `Private Bytes` perf counter |

---

## Manual Telemetry Queries After a Benchmark Run

If you ran the script without `-Correlate`, query manually after waiting 5–10 minutes:

```powershell
# Execution volume during the run
az monitor metrics list --resource $RESOURCE --metric "FunctionExecutionCount,Requests" --interval PT1M --aggregation Total --start-time $START_TIME --end-time $END_TIME

# Instance count peak during the run
az monitor metrics list --resource $RESOURCE --metric "InstanceCount" --interval PT1M --aggregation Maximum --start-time $START_TIME --end-time $END_TIME

# Memory during the run
$result = az monitor metrics list --resource $RESOURCE --metric "MemoryWorkingSet,PrivateBytes" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME | ConvertFrom-Json
$result.value | ForEach-Object { $m = $_.name.value; $_.timeseries[0].data | Where-Object { $_.average -gt 0 -or $_.maximum -gt 0 } | ForEach-Object { [PSCustomObject]@{ Metric=$m; Timestamp=$_.timeStamp; Average_MB=[math]::Round($_.average/1MB,2); Maximum_MB=[math]::Round($_.maximum/1MB,2) } } } | Sort-Object Timestamp | Format-Table -AutoSize

# CPU per instance from App Insights (Consumption plan — CpuPercentage metric is empty on Consumption)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(1d) | where name == '% Processor Time' | summarize avgCpu=avg(value), maxCpu=max(value) by cloud_RoleInstance | order by maxCpu desc"

# Per-instance scale-out timeline
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union traces, dependencies, performanceCounters | where timestamp > ago(1d) | summarize instances=dcount(cloud_RoleInstance) by bin(timestamp, 1m) | order by timestamp asc"

# Error and warning traces during the run
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | where severityLevel >= 2 | project timestamp, severityLevel, message, cloud_RoleInstance | order by timestamp desc"
```

---

## Troubleshooting

### Script returns `{"tables": []}` for App Insights queries

App Insights raw tables (`traces`, `dependencies`, `performanceCounters`) have a **5–10 minute ingestion lag**. The `union | summarize count()` query shows data instantly (pre-aggregated), but filtered queries need to wait. Either use `-Correlate` (the script waits automatically) or run the queries manually after waiting.

### `ENABLEAPPLICATIONINSIGHTS` is set but App Insights shows zero data

The function app may not have been invoked since the setting was applied. A settings change triggers a restart — invoke at least one endpoint after restart, wait 5–10 minutes, then query. Verify with:
```powershell
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union requests, traces, dependencies, exceptions | summarize count() by itemType"
```

### `InstanceCount` shows 0 or no data

`InstanceCount` on a Consumption (Dynamic) plan can be delayed by 1–2 minutes. The script adds a 2-minute buffer to the query window. If still zero, check `AppConnections` as a proxy:
```powershell
az monitor metrics list --resource $RESOURCE --metric "AppConnections" --interval PT1M --start-time $START_TIME --end-time $END_TIME
```

### `CpuPercentage` returns no data on the App Service Plan

On a Consumption plan, `CpuPercentage` and `MemoryPercentage` are not populated for the App Service Plan. Use `performanceCounters` in App Insights instead:
```powershell
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(1d) | where name == '% Processor Time' | summarize avg(value) by cloud_RoleInstance, bin(timestamp, 5m)"
```

### Parallel phase produces errors (429 / 503)

The function app has hit its concurrency ceiling. Check:
```powershell
# Queue depth — requests waiting to be processed
az monitor metrics list --resource $RESOURCE --metric "RequestsInApplicationQueue" --interval PT1M --start-time $START_TIME --end-time $END_TIME

# Current scale limit
az functionapp show --name $FUNC_APP -g $RG --query "siteConfig.functionAppScaleLimit" -o tsv
```
Increase scale limit if needed: `az functionapp update --name $FUNC_APP -g $RG --set siteConfig.functionAppScaleLimit=200`

### `ForEach-Object -Parallel` error

Requires **PowerShell 7+**. Check version: `pwsh --version`. Install from https://aka.ms/powershell.

---

## Quick Reference — Phase Summary

| Phase flag | Description | Key output |
|---|---|---|
| `ColdStart` | Stop/start app, measure first request | `COLD START first-success latency` |
| `Warm` | Sequential steady-state requests | `Warm STATS: min/avg/p95/max` |
| `Concurrency` | Sequential vs parallel throughput | Wall-clock, req/s, per-phase stats |
| `HighConcurrency` | Extended parallel burst | Error count, wall-clock, throughput |
| `IdleColdStart` | Wait for scale-to-zero, then measure | `IDLE COLD START first-success latency` |
| `Seq5` | 5 sequential, individual timings | Per-request ms, p95 |
| `Par5` | 5 parallel, individual timings + instance count | Per-request ms, instances scaled |
| `ColdVsWarm` | Seq5+Par5 under cold then warm | Side-by-side comparison table |
| `LoadSeq` | 100 sequential load test | Throughput req/s, error count, p95 |
| `LoadPar` | 100 parallel load test | Wall-clock, throughput, instances, errors |
| `AllNew` | All five new phases in sequence | Combined output of above |
| `Correlate` | App Insights + Azure Monitor correlation | Instance timeline, perf counters |
