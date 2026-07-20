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

> `ENABLEAPPLICATIONINSIGHTS=true` is the single authoritative switch — without it the App Insights SDK never registers and zero telemetry is sent, regardless of whether the connection string is present.
> The project uses `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` instead of the standard `APPLICATIONINSIGHTS_CONNECTION_STRING` to keep the Azure Functions host's own AI pipeline dormant.

```powershell
# Verify all required logging flags are set
az functionapp config appsettings list --name $FUNC_APP -g $RG --query "[?name=='ENABLEAPPLICATIONINSIGHTS' || name=='ENABLECONSOLELOGGING' || name=='ENABLEELASTICSEARCHLOGGING' || name=='EXECUTIONLOGLEVEL' || name=='WAREWOLF_APPINSIGHTS_CONNECTION_STRING'].{Key:name,Value:value}" --output table
```

| Setting | Required Value | Purpose |
|---|---|---|
| `ENABLEAPPLICATIONINSIGHTS` | `true` | **Must be true** — gates the entire AI SDK registration |
| `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` | `InstrumentationKey=...` | Connection string to App Insights |
| `EXECUTIONLOGLEVEL` | `TRACE` / `INFO` / `WARN` | Min log level (use `INFO` in production) |
| `ENABLECONSOLELOGGING` | `true` / `false` | stdout → filesystem / live log stream |
| `ENABLEELASTICSEARCHLOGGING` | `true` / `false` | Elasticsearch sink |

```powershell
# Set if missing
$AI_CONN_STR = az monitor app-insights component show --app $AI_NAME -g $RG --query "connectionString" -o tsv
az functionapp config appsettings set --name $FUNC_APP -g $RG --settings "ENABLEAPPLICATIONINSIGHTS=true" "WAREWOLF_APPINSIGHTS_CONNECTION_STRING=$AI_CONN_STR" "EXECUTIONLOGLEVEL=INFO"
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
$ASP_RESOURCE = az functionapp show --name $FUNC_APP -g $RG --query "appServicePlanId" -o tsv
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
$ASP_RESOURCE = az functionapp show --name $FUNC_APP -g $RG --query "appServicePlanId" -o tsv
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

```powershell
# Check row counts across all telemetry tables (aggregated — instant result)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "union requests, traces, dependencies, exceptions, customEvents | summarize count() by itemType"

# Find actual timestamp range of ingested data
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | summarize min(timestamp), max(timestamp)"

# Total request count and last seen (no time filter)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | summarize lastSeen=max(timestamp), total=count()"

# Check App Insights sampling config and retention
az monitor app-insights component show --app $AI_NAME -g $RG --query "{SamplingPercentage:samplingPercentage, RetentionDays:retentionInDays}" -o json

# Confirm WAREWOLF_APPINSIGHTS_CONNECTION_STRING and ENABLEAPPLICATIONINSIGHTS are set
az functionapp config appsettings list --name $FUNC_APP -g $RG --query "[?name=='WAREWOLF_APPINSIGHTS_CONNECTION_STRING' || name=='ENABLEAPPLICATIONINSIGHTS'].{Key:name,Value:value}" --output table
```

---

## 1. Startup Traces (Uptime / Cold Start)

> Wait 5–10 minutes after function invocation before running these.
> Startup logs from `Program.cs` are emitted via `Dev2Logger` and appear in the `traces` table.

```powershell
# All startup and program init messages
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | where message has 'Program' or message has 'Host started' or message has 'startup' or message has 'initialized' | project timestamp, severityLevel, message, cloud_RoleInstance | order by timestamp asc"

# Cold start detection
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | where customDimensions['ColdStart'] == 'True' | summarize coldStarts=count(), avgDuration=avg(duration) by name, bin(timestamp, 1h) | order by timestamp desc"

# Startup duration per instance (first to last startup trace)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | where message has 'Program' | summarize startupStart=min(timestamp), startupEnd=max(timestamp) by cloud_RoleInstance, tostring(operation_Id) | extend startupMs=datetime_diff('millisecond', startupEnd, startupStart) | order by startupStart desc"

# Active connections — proxy for uptime (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "AppConnections" --interval PT1M --start-time $START_TIME --end-time $END_TIME
```

---

## 2. Running Instances (Parallel Instances / Scale Out)

```powershell
# Distinct active instances in 5-minute buckets
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(1d) | summarize instances=dcount(cloud_RoleInstance) by bin(timestamp, 5m) | order by timestamp desc"

# All distinct instances — first/last seen
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "traces | where timestamp > ago(15d) | summarize firstSeen=min(timestamp), lastSeen=max(timestamp), traceCount=count() by cloud_RoleInstance | order by firstSeen desc"

# Thread and handle count (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "Threads,Handles,InstanceCount" --interval PT5M --start-time $START_TIME --end-time $END_TIME

# Active connections (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "AppConnections" --interval PT1M --start-time $START_TIME --end-time $END_TIME
```

---

## 3. Execution Metrics (Per Function Endpoint)

```powershell
# Per-function: count, success rate, avg/P95/P99 latency
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | summarize total=count(), success=countif(success==true), avgDuration=avg(duration), p95=percentile(duration, 95), p99=percentile(duration, 99) by name | order by total desc"

# HTTP response code breakdown per endpoint
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | summarize count() by name, resultCode | order by count_ desc"

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

# List all available performance counters
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "performanceCounters | where timestamp > ago(15d) | distinct name, category"
```

---

## 5. Parallel Request Execution Metrics

```powershell
# Max concurrent requests per minute per instance
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | summarize concurrent=count() by bin(timestamp, 1m), cloud_RoleInstance | summarize maxConcurrent=max(concurrent) by bin(timestamp, 1m) | order by timestamp desc"

# Dependency summary — success vs failure with avg duration
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "dependencies | where timestamp > ago(1d) | summarize total=count(), failed=countif(success==false), avgDuration=avg(duration) by target, type | order by total desc"

# Requests and queue depth (Azure Monitor)
az monitor metrics list --resource $RESOURCE --metric "Requests,RequestsInApplicationQueue" --interval PT1M --start-time $START_TIME --end-time $END_TIME
```

---

## 6. Warewolf Performance Counters → Azure Functions Equivalents

| Warewolf Counter | Azure Equivalent | Source |
|---|---|---|
| Requests/sec | `requests \| summarize count() by bin(timestamp, 1s)` | App Insights |
| Avg execution time | `avg(duration)` in `requests` table | App Insights |
| CPU % | `CpuPercentage` on App Service Plan | Azure Monitor |
| Memory (Private Bytes) | `PrivateBytes` metric / `performanceCounters` | Azure Monitor / App Insights |
| Exceptions/sec | `exceptions \| summarize count() by bin(timestamp, 1m)` | App Insights |
| Active sessions | `dcount(session_Id)` in `requests` | App Insights |
| Thread count | `Threads` metric | Azure Monitor |
| Queue depth | `RequestsInApplicationQueue` metric | Azure Monitor |
| IO throughput | `IoReadBytesPerSecond`, `IoWriteBytesPerSecond` | Azure Monitor |

```powershell
# Exception rate per minute
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "exceptions | where timestamp > ago(1d) | summarize count() by bin(timestamp, 1m), type | order by timestamp desc"

# Error rate per function endpoint
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | summarize total=count(), failures=countif(success==false) by name | extend errorRate=round(100.0 * failures / total, 2) | order by errorRate desc"

# Requests per second (throughput)
az monitor app-insights query --app $AI_NAME -g $RG --analytics-query "requests | where timestamp > ago(1d) | summarize rps=count() by bin(timestamp, 1s) | summarize avgRps=avg(rps), maxRps=max(rps)"

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
