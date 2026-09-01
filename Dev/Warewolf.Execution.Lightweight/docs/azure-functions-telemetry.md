# Azure Functions Telemetry Reference
> PowerShell commands for querying Azure Function App performance data via Azure CLI and Application Insights REST API.
>
> **Project:** `Warewolf.Execution.Lightweight` — Azure Functions v4 isolated worker, .NET 8

---

## Project Overview

| Property | Value |
|---|---|
| Project | `Warewolf.Execution.Lightweight` |
| Azure Functions Version | v4 (isolated worker model) |
| Target Framework | net8.0 |
| Function App | `wwenginenewscriptai` |
| Resource Group | `DEV2` |
| Region | South Africa North |
| Runtime | Framework-dependent (Azure-provided, not self-contained) |

### Key Packages Relevant to Telemetry

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Azure.Functions.Worker.ApplicationInsights` | 1.4.0 | App Insights integration for isolated worker |
| `Microsoft.ApplicationInsights.WorkerService` | 2.22.0 | Must stay on **2.x** — 3.x causes `TypeLoadException` at startup |
| `Elastic.Clients.Elasticsearch` | 8.15.6 | Secondary log/telemetry sink |
| `Azure.Identity` | 1.20.0 | Managed Identity auth (used for Key Vault and Entra token) |

### Startup Optimization Settings (Affects Cold Start Telemetry)

```xml
<!-- Faster CLR warmup — reduces cold start duration -->
<TieredCompilation>true</TieredCompilation>
<TieredPGO>true</TieredPGO>

<!-- Prevents embedding runtime (~200MB saving) -->
<SelfContained>false</SelfContained>
<PublishSingleFile>false</PublishSingleFile>
<PublishTrimmed>false</PublishTrimmed>
<PublishReadyToRun>false</PublishReadyToRun>
```

> `TieredPGO` means the CLR JIT-compiles hot paths progressively — initial requests after a cold start may be slower than subsequent ones even within the same instance lifetime.

### Publish Gating Note

Debug builds with a `RuntimeIdentifier` set (e.g. `-r win-x64`) are **not publishable** (`IsPublishable=false`). Only Release builds trigger the full Azure publish pipeline via `Deploy-ToAzure.ps1`. Telemetry gaps during Debug CI runs are expected.

---

## Important: Ingestion Lag

> App Insights has two query paths that behave differently:
>
> - `union ... | summarize count()` — reads from a **pre-aggregated summary**. Data appears almost instantly (seconds).
> - `traces | where timestamp > ago(...)` — scans the **raw indexed table**. Requires **5–10 minutes** after ingestion before results appear.
>
> **Always wait 5–10 minutes after triggering a function before running raw table queries.**
> If queries return `{"tables": []}` immediately after an invocation, wait and retry — the data is in flight.
>
> **Use single-line `--analytics-query` strings only.** Multiline PowerShell strings passed to `--analytics-query` can silently malform the KQL and return empty results even when data exists.

---

## Prerequisites

### Install Application Insights CLI Extension
```powershell
az extension add --name application-insights --allow-preview True --upgrade
```

### Required App Settings on the Function App

> WOLF-8516: `ENABLEAPPLICATIONINSIGHTS`/`ENABLECONSOLELOGGING`/`ENABLEELASTICSEARCHLOGGING` were
> merged into one `WAREWOLF_LOGGING_CONFIG` JSON app setting. `WAREWOLF_LOGGING_CONFIG.appInsights=true`
> is the single authoritative switch — without it the App Insights SDK never registers and zero
> telemetry is sent, regardless of whether the connection string is present.
> The project uses `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` instead of the standard `APPLICATIONINSIGHTS_CONNECTION_STRING` to keep the Azure Functions host's own AI pipeline dormant.

```powershell
# Verify all required logging flags are set
az functionapp config appsettings list --name $FUNC_APP -g $RG --query "[?name=='WAREWOLF_LOGGING_CONFIG' || name=='EXECUTIONLOGLEVEL' || name=='WAREWOLF_APPINSIGHTS_CONNECTION_STRING'].{Key:name,Value:value}" --output table
```

| Setting | Required Value | Purpose |
|---|---|---|
| `WAREWOLF_LOGGING_CONFIG` | JSON, e.g. `{"appInsights":true,"console":true,"elasticsearch":false}` | **`appInsights:true` required** — gates the entire AI SDK registration; `console`/`elasticsearch` toggle those sinks (WOLF-8516) |
| `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` | `InstrumentationKey=...` | Connection string to App Insights |
| `EXECUTIONLOGLEVEL` | `TRACE` / `INFO` / `WARN` | Min log level (use `INFO` in production) |

```powershell
# Set if missing (read-merge-write — a bare `set` would wipe any other WAREWOLF_LOGGING_CONFIG fields)
$AI_CONN_STR = az monitor app-insights component show --app $AI_NAME -g $RG --query "connectionString" -o tsv
$existingLoggingConfigRaw = az functionapp config appsettings list --name $FUNC_APP -g $RG --query "[?name=='WAREWOLF_LOGGING_CONFIG'].value | [0]" -o tsv
$loggingConfig = if ($existingLoggingConfigRaw -and $existingLoggingConfigRaw -ne 'None') { $existingLoggingConfigRaw | ConvertFrom-Json } else { @{} }
$loggingConfig = $loggingConfig | Select-Object -Property * # copy to a mutable object
$loggingConfig | Add-Member -NotePropertyName appInsights -NotePropertyValue $true -Force
az functionapp config appsettings set --name $FUNC_APP -g $RG --settings "WAREWOLF_LOGGING_CONFIG=$($loggingConfig | ConvertTo-Json -Compress)" "WAREWOLF_APPINSIGHTS_CONNECTION_STRING=$AI_CONN_STR" "EXECUTIONLOGLEVEL=INFO"
```

### Look Up Variable Values

```powershell
# SUB_ID — your active subscription
$SUB_ID = az account show --query id -o tsv

# RG — list all resource groups
az group list --query "[].name" -o tsv

# FUNC_APP — list all function apps in the resource group
az functionapp list -g $RG --query "[].name" -o tsv

# AI_NAME — list all App Insights in the resource group
az resource list -g $RG --resource-type "microsoft.insights/components" --query "[].name" -o tsv

# APP_ID — from App Insights name
$APP_ID = az monitor app-insights component show --app $AI_NAME -g $RG --query appId -o tsv

# ASP_RESOURCE — App Service Plan resource ID
# NOTE: use `serverFarmId`. The `appServicePlanId` property returns an empty string.
$ASP_RESOURCE = az functionapp show --name $FUNC_APP -g $RG --query "serverFarmId" -o tsv
```

### Set Common Variables

> `$START_TIME`/`$END_TIME` default to the last 15 days and are used only by `az monitor metrics list`. KQL queries use `ago(1d)` or `ago(15d)` directly.

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

### Verify Variables

```powershell
Write-Host "SUB_ID:       $SUB_ID"
Write-Host "RG:           $RG"
Write-Host "FUNC_APP:     $FUNC_APP"
Write-Host "AI_NAME:      $AI_NAME"
Write-Host "APP_ID:       $APP_ID"
Write-Host "START_TIME:   $START_TIME"
Write-Host "END_TIME:     $END_TIME"
Write-Host "RESOURCE:     $RESOURCE"
Write-Host "ASP_RESOURCE: $ASP_RESOURCE"
```

> **Note:** When using `az monitor metrics list` with a full resource ID in `--resource`, do **not** also pass `--resource-type`. That combination causes a usage error.

---

## Validate Data Exists

> Run these first. These use aggregated queries and return data immediately — no ingestion lag.
>
> **Which tables are populated:** In this configuration the Functions host AI pipeline is dormant (the app uses `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`, not `APPLICATIONINSIGHTS_CONNECTION_STRING`), so the **`requests` and `exceptions` App Insights tables stay empty**. Only `traces`, `dependencies`, and `performanceCounters` receive data. Any query against `requests` (per-function latency, cold-start `customDimensions`, RPS, concurrency, error-rate) will return empty rows even though the app is executing. Use these substitutes:
> - **Execution count / duration** → Azure Monitor `FunctionExecutionCount` / `AverageResponseTime`, or the `dependencies` table (`Invoke` operation).
> - **Concurrency** → `dependencies | summarize count() by bin(timestamp, 1m), cloud_RoleInstance`.
> - **Request count** → Azure Monitor `Requests` metric (populated independently of the AI `requests` table).
> - **Errors / exceptions** → `traces | where severityLevel >= 3` (the `exceptions` table is empty).

```powershell
# Check row counts across all telemetry tables (aggregated — instant result)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union requests, traces, dependencies, exceptions, customEvents | summarize count() by itemType"

# Find actual timestamp range of ingested data
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | summarize min(timestamp), max(timestamp)"

# Total request count and last seen (no time filter)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | summarize lastSeen=max(timestamp), total=count()"

# Check App Insights sampling config and retention
az monitor app-insights component show --app $AI_NAME -g $RG --query "{SamplingPercentage:samplingPercentage, RetentionDays:retentionInDays}" -o json

# Confirm WAREWOLF_APPINSIGHTS_CONNECTION_STRING and WAREWOLF_LOGGING_CONFIG.appInsights are set (WOLF-8516)
az functionapp config appsettings list --name $FUNC_APP -g $RG --query "[?name=='WAREWOLF_APPINSIGHTS_CONNECTION_STRING' || name=='WAREWOLF_LOGGING_CONFIG'].{Key:name,Value:value}" --output table
```

---

## 1. Startup Traces (Uptime / Cold Start)

> Wait 5–10 minutes after function invocation before running these.
> Startup logs from `Program.cs` are emitted via `Dev2Logger` and appear in the `traces` table.

```powershell
# All startup and program init messages
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | where message has 'Program' or message has 'Host started' or message has 'startup' or message has 'initialized' | project timestamp, severityLevel, message, cloud_RoleInstance | order by timestamp asc"

# Cold start detection — NOTE: the `requests` table is empty in this config, so the
# customDimensions['ColdStart'] flag is unavailable. Detect cold starts from startup traces
# instead: an instance whose first trace is a Program/Host-started message indicates a fresh
# (cold) instance. Correlate the gap to the first executed request client-side (perf harness).
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | where message has 'Host started' or message has 'startup' | summarize coldInstanceStart=min(timestamp) by cloud_RoleInstance | order by coldInstanceStart desc"

# Startup duration per instance (first to last startup trace)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | where message has 'Program' | summarize startupStart=min(timestamp), startupEnd=max(timestamp) by cloud_RoleInstance, tostring(operation_Id) | extend startupMs=datetime_diff('millisecond', startupEnd, startupStart) | order by startupStart desc"

# Active connections — proxy for uptime (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "AppConnections" --interval PT1M --start-time $START_TIME --end-time $END_TIME
```

---

## 2. Running Instances (Parallel Instances / Scale Out)

> **Scale-out is derived from the POPULATED tables only.** Because `requests` stays empty in
> this config, `dcount(cloud_RoleInstance)` must be taken from a **union of `traces`,
> `dependencies`, and `performanceCounters`** — never from `requests` alone (which returns 0
> instances). These raw tables have 5–10 min ingestion lag, so retry the query if it returns
> empty right after a burst. Azure Monitor `InstanceCount` is the lag-free fallback.

```powershell
# Total distinct instances during a burst window (union of populated tables — retry on lag)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union traces, dependencies, performanceCounters | where timestamp > ago(1d) | summarize instances=dcount(cloud_RoleInstance), samples=count(), firstSeen=min(timestamp), lastSeen=max(timestamp)"

# Per-minute scale-out shape (how instances ramp during a parallel burst)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union traces, dependencies, performanceCounters | where timestamp > ago(1d) | summarize instances=dcount(cloud_RoleInstance), samples=count() by bin(timestamp, 1m) | order by timestamp asc"

# All distinct instances — first/last seen (union of populated tables)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union traces, dependencies, performanceCounters | where timestamp > ago(15d) | summarize firstSeen=min(timestamp), lastSeen=max(timestamp), samples=count() by cloud_RoleInstance | order by firstSeen desc"

# Lag-free fallback — max instance count from Azure Monitor (no ingestion delay)
az monitor metrics list --resource $RESOURCE --metric "InstanceCount" --interval PT1M --aggregation Maximum --start-time $START_TIME --end-time $END_TIME

# Thread and handle count (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "Threads,Handles" --interval PT5M --start-time $START_TIME --end-time $END_TIME

# Active connections (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "AppConnections" --interval PT1M --start-time $START_TIME --end-time $END_TIME
```

> **Automated capture:** the perf harness
> [`Measure-FunctionPerf.ps1`](./perf/Measure-FunctionPerf.ps1) runs these queries for you
> when invoked with `-Correlate`. It waits out the ingestion lag and retries the union query
> until `instances > 0`, falling back to Azure Monitor `InstanceCount` if the raw tables are
> still lagging. This is what previously caused the "instance scale-out couldn't be captured"
> gap — querying `traces` alone (still lagging) instead of the union with a retry loop.

---

## 3. Execution Metrics (Per Function Endpoint)

> The `requests` table is empty here, so per-endpoint latency/success come from the
> `dependencies` table (`Invoke` operation) and Azure Monitor. The `requests`-based queries
> are kept for reference on apps that use the standard connection string, but return empty
> rows on `wwenginenewscriptai`.

```powershell
# Per-operation latency/success from dependencies (populated substitute for `requests`)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "dependencies | where timestamp > ago(1d) | summarize total=count(), success=countif(success==true), avgDuration=avg(duration), p95=percentile(duration, 95), p99=percentile(duration, 99) by name | order by total desc"

# Per-function (reference only — empty on this app): count, success rate, avg/P95/P99 latency
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | summarize total=count(), success=countif(success==true), avgDuration=avg(duration), p95=percentile(duration, 95), p99=percentile(duration, 99) by name | order by total desc"

# HTTP response code breakdown (reference only — empty on this app; use Http2xx/4xx/5xx metrics)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | summarize count() by name, resultCode | order by count_ desc"

# HTTP status breakdown from Azure Monitor (populated substitute)
az monitor metrics list --resource $RESOURCE --metric "Http2xx,Http4xx,Http5xx" --interval PT1M --aggregation Total --start-time $START_TIME --end-time $END_TIME

# Daily request volume trend
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(15d) | summarize count() by bin(timestamp, 1d) | order by timestamp desc"

# Execution count and compute units (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "FunctionExecutionCount,FunctionExecutionUnits" --interval PT5M --start-time $START_TIME --end-time $END_TIME

# Average + max response time (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "AverageResponseTime,HttpResponseTime" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME
```

---

## 4. Memory / CPU Usage

> **Important:** `CpuPercentage` is **not** valid for `Microsoft.Web/sites`. Query it on the **App Service Plan** resource instead.
>
> **On a Consumption (Dynamic SKU) plan** — which `wwenginenewscriptai` uses — `CpuPercentage`/`MemoryPercentage` on the App Service Plan return **no data points** (the CLI succeeds but the series is empty). These metrics are only populated on Dedicated/Premium plans. For CPU on Consumption, use the App Insights `performanceCounters` table (`% Processor Time`) shown below.

```powershell
# Memory — non-zero rows only, formatted as table
$result = az monitor metrics list --resource $RESOURCE --metric "MemoryWorkingSet,PrivateBytes" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME | ConvertFrom-Json
$result.value | ForEach-Object { $m = $_.name.value; $_.timeseries[0].data | Where-Object { $_.average -gt 0 -or $_.maximum -gt 0 } | ForEach-Object { [PSCustomObject]@{ Metric=$m; Timestamp=$_.timeStamp; Average_MB=[math]::Round($_.average/1MB,2); Maximum_MB=[math]::Round($_.maximum/1MB,2) } } } | Sort-Object Timestamp | Format-Table -AutoSize

# IO bytes per second — non-zero rows only
$result = az monitor metrics list --resource $RESOURCE --metric "IoReadBytesPerSecond,IoWriteBytesPerSecond,IoOtherBytesPerSecond" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME | ConvertFrom-Json
$result.value | ForEach-Object { $m = $_.name.value; $_.timeseries[0].data | Where-Object { $_.average -gt 0 -or $_.maximum -gt 0 } | ForEach-Object { [PSCustomObject]@{ Metric=$m; Timestamp=$_.timeStamp; Average=[math]::Round($_.average,2); Maximum=[math]::Round($_.maximum,2); Unit="Bytes/sec" } } } | Sort-Object Timestamp | Format-Table -AutoSize

# IO operations per second — non-zero rows only
$result = az monitor metrics list --resource $RESOURCE --metric "IoReadOperationsPerSecond,IoWriteOperationsPerSecond,IoOtherOperationsPerSecond" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME | ConvertFrom-Json
$result.value | ForEach-Object { $m = $_.name.value; $_.timeseries[0].data | Where-Object { $_.average -gt 0 -or $_.maximum -gt 0 } | ForEach-Object { [PSCustomObject]@{ Metric=$m; Timestamp=$_.timeStamp; Average=[math]::Round($_.average,2); Maximum=[math]::Round($_.maximum,2); Unit="Ops/sec" } } } | Sort-Object Timestamp | Format-Table -AutoSize

# CPU % on App Service Plan — non-zero rows only
$result = az monitor metrics list --resource $ASP_RESOURCE --metric "CpuPercentage,MemoryPercentage" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME | ConvertFrom-Json
$result.value | ForEach-Object { $m = $_.name.value; $_.timeseries[0].data | Where-Object { $_.average -gt 0 -or $_.maximum -gt 0 } | ForEach-Object { [PSCustomObject]@{ Metric=$m; Timestamp=$_.timeStamp; Average=[math]::Round($_.average,2); Maximum=[math]::Round($_.maximum,2); Unit="%" } } } | Sort-Object Timestamp | Format-Table -AutoSize

# .NET GC pressure (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "Gen0Collections,Gen1Collections,Gen2Collections" --interval PT5M --aggregation Average Maximum --start-time $START_TIME --end-time $END_TIME

# Performance counters via App Insights
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(1d) | where name in ('% Processor Time', 'Private Bytes', 'IO Data Bytes/sec') | summarize avgValue=avg(value) by name, bin(timestamp, 5m) | order by timestamp desc"

# Warewolf server perf counters per instance (avg + max) — the authoritative CPU/memory
# source on Consumption, and the per-instance breakdown that shows load distribution across
# the scaled-out instances. This is the primary perf-counter capture for the load harness.
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(1d) | summarize avgValue=avg(value), maxValue=max(value) by name, cloud_RoleInstance | order by name asc, cloud_RoleInstance asc"

# Perf counters over time (per-minute trend during a burst, split by counter)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(1d) | summarize avgValue=avg(value), maxValue=max(value) by name, bin(timestamp, 1m) | order by timestamp asc"

# List all available performance counters
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(15d) | distinct name, category"
```

> **Perf counters are the CPU/memory source of record on Consumption.** The `performanceCounters`
> table is populated (unlike `requests`), so `% Processor Time`, `Private Bytes`, `Available Bytes`,
> `IO Data Bytes/sec`, etc. are available per `cloud_RoleInstance`. They also have 5–10 min
> ingestion lag — the harness `-Correlate` step waits and retries before reporting them.

---

## 5. Parallel Request Execution Metrics

> Concurrency must come from `dependencies` (the `Invoke` operation), **not `requests`**
> (empty in this config). Corroborate with Azure Monitor `Requests`.

```powershell
# Max concurrent executions per minute per instance (from dependencies — populated)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "dependencies | where timestamp > ago(1d) | summarize concurrent=count() by bin(timestamp, 1m), cloud_RoleInstance | summarize maxConcurrent=max(concurrent), instances=dcount(cloud_RoleInstance) by bin(timestamp, 1m) | order by timestamp desc"

# Dependency summary — success vs failure with avg duration
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "dependencies | where timestamp > ago(1d) | summarize total=count(), failed=countif(success==false), avgDuration=avg(duration) by target, type | order by total desc"

# Requests and queue depth (Azure Monitor — populated independently of the AI requests table)
az monitor metrics list --resource $RESOURCE --metric "Requests,RequestsInApplicationQueue" --interval PT1M --start-time $START_TIME --end-time $END_TIME
```

---

## 6. Warewolf Performance Counters → Azure Functions Equivalents

> On the **Consumption (Dynamic) plan** the `requests`/`exceptions` App Insights tables are
> empty and App Service Plan `CpuPercentage`/`MemoryPercentage` return no data. The mappings
> below use only the sources that are actually populated in this configuration.

| Warewolf Counter | Azure Equivalent | Source |
|---|---|---|
| Requests/sec | `FunctionExecutionCount` / `Requests` metric per interval | Azure Monitor |
| Avg execution time | `avg(duration)` in `dependencies` (`Invoke`) / `AverageResponseTime` | App Insights / Azure Monitor |
| CPU % | `performanceCounters` `% Processor Time` (App Service Plan `CpuPercentage` is empty on Consumption) | App Insights |
| Memory (Private Bytes) | `performanceCounters` `Private Bytes` / `PrivateBytes` metric | App Insights / Azure Monitor |
| Exceptions/sec | `traces \| where severityLevel >= 3` (the `exceptions` table is empty) | App Insights |
| Active instances (scale-out) | `dcount(cloud_RoleInstance)` over `union traces,dependencies,performanceCounters` / `InstanceCount` | App Insights / Azure Monitor |
| Thread count | `Threads` metric | Azure Monitor |
| Queue depth | `RequestsInApplicationQueue` metric | Azure Monitor |
| IO throughput | `IoReadBytesPerSecond`, `IoWriteBytesPerSecond` | Azure Monitor |

```powershell
# Error/critical rate per minute (traces — the exceptions table is empty)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | where severityLevel >= 3 | summarize count() by bin(timestamp, 1m), severityLevel | order by timestamp desc"

# Throughput (requests per second) from Azure Monitor execution count
az monitor metrics list --resource $RESOURCE --metric "FunctionExecutionCount,Requests" --interval PT1M --aggregation Total --start-time $START_TIME --end-time $END_TIME

# Severity level breakdown of all traces
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | summarize count() by severityLevel | extend level=case(severityLevel==0,'Verbose',severityLevel==1,'Info',severityLevel==2,'Warning',severityLevel==3,'Error',severityLevel==4,'Critical','Unknown') | order by severityLevel asc"
```

---

## Application Insights REST API

An alternative to the CLI — useful for scripting, dashboards, or integrations.

### Authentication (Entra ID — API keys are retired)
```powershell
$TOKEN   = az account get-access-token --resource "https://api.applicationinsights.io" --query accessToken -o tsv
$HEADERS = @{ Authorization = "Bearer $TOKEN" }
$BASE    = "https://api.applicationinsights.io/v1/apps/$APP_ID"
$SPAN    = "timespan=$START_TIME/$END_TIME"
```

### Metrics Endpoint
```powershell
Invoke-RestMethod -Uri "$BASE/metrics/requests/count?$SPAN&interval=PT5M" -Headers $HEADERS
Invoke-RestMethod -Uri "$BASE/metrics/requests/duration?$SPAN&aggregation=avg,percentile_95,percentile_99&interval=PT5M" -Headers $HEADERS
Invoke-RestMethod -Uri "$BASE/metrics/requests/failed/count?$SPAN" -Headers $HEADERS
Invoke-RestMethod -Uri "$BASE/metrics/exceptions/count?$SPAN" -Headers $HEADERS
Invoke-RestMethod -Uri "$BASE/metrics/metadata" -Headers $HEADERS
```

### Query Endpoint (KQL via POST)
```powershell
$BODY = @{ query = "requests | where timestamp > ago(1d) | summarize count(), avg(duration) by name | order by count_ desc" } | ConvertTo-Json
Invoke-RestMethod -Method POST -Uri "$BASE/query" -Headers $HEADERS -Body $BODY -ContentType "application/json"
```

### Events Endpoint (Individual Records)
```powershell
Invoke-RestMethod -Uri "$BASE/events/exceptions?$SPAN&`$top=50&`$orderby=timestamp+desc" -Headers $HEADERS
Invoke-RestMethod -Uri "$BASE/events/requests?`$filter=request/name+eq+'ExecuteWorkflow'&`$top=100&$SPAN" -Headers $HEADERS
```

> **Supported event types:** `requests`, `traces`, `exceptions`, `dependencies`, `customEvents`, `pageViews`, `availabilityResults`, `$all`

---

## Utility Commands

```powershell
# List all valid metrics for the Function App
az monitor metrics list-definitions --resource $RESOURCE --output table

# List all valid metrics for the App Service Plan
az monitor metrics list-definitions --resource $ASP_RESOURCE --output table

# List all functions and their states
az functionapp function list --name $FUNC_APP -g $RG --output table

# Tail live logs
az webapp log tail --name $FUNC_APP --resource-group $RG

# Get subscription ID
az account show --query id -o tsv

# List all App Insights in subscription
az resource list --resource-type "microsoft.insights/components" --query "[].{Name:name, ResourceGroup:resourceGroup, Location:location}" --output table
```

---

## Severity Level Reference

| `EXECUTIONLOGLEVEL` | App Insights `severityLevel` | Notes |
|---|---|---|
| TRACE | 0 | Most verbose — high volume, use in dev only |
| DEBUG | 0 | Verbose |
| INFO | 1 | Information — recommended for production |
| WARN | 2 | Warning |
| ERROR | 3 | Error |
| FATAL | 4 | Critical |

> Use `EXECUTIONLOGLEVEL=INFO` in production to reduce telemetry noise and App Insights ingestion cost.

---

## Valid Metrics Reference for Microsoft.Web/sites

| Category | Metrics |
|---|---|
| HTTP Traffic | `Requests`, `BytesReceived`, `BytesSent`, `Http2xx`, `Http3xx`, `Http4xx`, `Http5xx`, `Http401`, `Http403`, `Http404`, `Http406`, `Http101` |
| Response Time | `AverageResponseTime`, `HttpResponseTime` |
| Functions | `FunctionExecutionCount`, `FunctionExecutionUnits` |
| Memory | `MemoryWorkingSet`, `AverageMemoryWorkingSet`, `PrivateBytes` |
| IO | `IoReadBytesPerSecond`, `IoWriteBytesPerSecond`, `IoOtherBytesPerSecond`, `IoReadOperationsPerSecond`, `IoWriteOperationsPerSecond`, `IoOtherOperationsPerSecond` |
| Connections | `AppConnections`, `Handles`, `Threads` |
| Scale | `InstanceCount`, `RequestsInApplicationQueue` |
| .NET | `CurrentAssemblies`, `TotalAppDomains`, `TotalAppDomainsUnloaded`, `Gen0Collections`, `Gen1Collections`, `Gen2Collections` |
| Health | `HealthCheckStatus`, `FileSystemUsage` |

> `CpuPercentage` and `MemoryPercentage` are only available on **App Service Plan** (`Microsoft.Web/serverfarms`) resources, not on the Function App site.
