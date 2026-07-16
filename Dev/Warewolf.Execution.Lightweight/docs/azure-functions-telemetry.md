# Azure Functions Telemetry Reference
> PowerShell commands for querying Azure Function App performance data via Azure CLI and Application Insights REST API.

---

## Prerequisites

### Install Application Insights CLI Extension
```powershell
az extension add --name application-insights --allow-preview True --upgrade
```

### Set Common Variables
> Update these values to match your environment. All commands below depend on these variables.

```powershell
$RG         = "DEV2"
$FUNC_APP   = "wwengineai"
$AI_NAME    = "wwengineai-ai"
$SUB_ID     = "dd0bc517-5cc7-4b56-bd6a-68e6140db7b3"
$APP_ID     = "85fdd0ce-0560-424c-8c23-4633d03c9877"
$START_TIME = "2026-07-01T00:00:00Z"
$END_TIME   = "2026-07-16T23:59:00Z"
$RESOURCE   = "/subscriptions/$SUB_ID/resourceGroups/$RG/providers/Microsoft.Web/sites/$FUNC_APP"
$ASP_RESOURCE = az functionapp show --name $FUNC_APP -g $RG --query "appServicePlanId" -o tsv
```

### Verify Variables
```powershell
Write-Host "RG: $RG"
Write-Host "AI_NAME: $AI_NAME"
Write-Host "FUNC_APP: $FUNC_APP"
Write-Host "APP_ID: $APP_ID"
Write-Host "SUB_ID: $SUB_ID"
Write-Host "RESOURCE: $RESOURCE"
Write-Host "ASP_RESOURCE: $ASP_RESOURCE"
```

> **Note:** When using `az monitor metrics list` with a full resource ID in `--resource`, do **not** also pass `--resource-type`. That combination causes a usage error.

---

## Validate Data Exists

Run these first to confirm App Insights is receiving telemetry before running detailed queries.

```powershell
# Check data exists across all telemetry tables
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "union requests, traces, dependencies, exceptions, customEvents | summarize count() by itemType"

# Check last recorded request (no time filter)
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "requests | summarize lastSeen=max(timestamp), total=count()"

# Daily request breakdown across date range
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "requests | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME')) | summarize count() by bin(timestamp, 1d) | order by timestamp desc"

# Check App Insights sampling config and retention
az monitor app-insights component show --app $AI_NAME -g $RG `
  --query "{SamplingPercentage:samplingPercentage, RetentionDays:retentionInDays}" -o json

# Confirm App Insights is linked to the Function App
az functionapp config appsettings list `
  --name $FUNC_APP -g $RG `
  --query "[?name=='APPLICATIONINSIGHTS_CONNECTION_STRING' || name=='APPINSIGHTS_INSTRUMENTATIONKEY'].{Key:name,Value:value}" `
  --output table
```

---

## 1. Startup Traces (Uptime / Cold Start)

Queries the `traces` table for Function App host startup and initialization events.

```powershell
# Host startup events
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    traces
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | where message has 'Host started' or message has 'Host initialized'
    | project timestamp, message, cloud_RoleInstance, operation_Id
    | order by timestamp desc
    | take 100
  "

# Cold start detection (requests flagged as cold starts)
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    requests
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | where customDimensions['ColdStart'] == 'True'
    | summarize coldStarts=count(), avgDuration=avg(duration) by name, bin(timestamp, 1h)
    | order by timestamp desc
  "
```

---

## 2. Running Instances (Parallel Instances / Scale Out)

Tracks how many Function App instances were active over time.

```powershell
# Active connections via Azure Monitor
az monitor metrics list `
  --resource $RESOURCE `
  --metric "AppConnections" `
  --interval PT1M `
  --start-time $START_TIME `
  --end-time $END_TIME

# Instance count via App Insights (distinct host instances)
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    traces
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | summarize instances=dcount(cloud_RoleInstance) by bin(timestamp, 5m)
    | order by timestamp desc
  "

# Thread and handle count per instance
az monitor metrics list `
  --resource $RESOURCE `
  --metric "Threads,Handles,InstanceCount" `
  --interval PT5M `
  --start-time $START_TIME `
  --end-time $END_TIME
```

---

## 3. Execution Metrics (Per Function Endpoint)

Breaks down request counts, success rates, and latency per function endpoint.

```powershell
# Execution count and compute units via Azure Monitor
az monitor metrics list `
  --resource $RESOURCE `
  --metric "FunctionExecutionCount,FunctionExecutionUnits" `
  --interval PT5M `
  --start-time $START_TIME `
  --end-time $END_TIME

# Per-function breakdown: count, success rate, avg/P95/P99 latency
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    requests
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | summarize
        total=count(),
        success=countif(success==true),
        avgDuration=avg(duration),
        p95=percentile(duration, 95),
        p99=percentile(duration, 99)
      by name
    | order by total desc
  "

# HTTP response code breakdown per endpoint
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    requests
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | summarize count() by name, resultCode
    | order by count_ desc
  "

# Average + P95 response time via Azure Monitor
az monitor metrics list `
  --resource $RESOURCE `
  --metric "AverageResponseTime,HttpResponseTime" `
  --interval PT5M `
  --aggregation Average Maximum `
  --start-time $START_TIME `
  --end-time $END_TIME
```

---

## 4. Memory / CPU Usage

> **Important:** `CpuPercentage` is **not** a valid metric for `Microsoft.Web/sites`. Use IO metrics as a CPU proxy on the Function App, and query `CpuPercentage` on the **App Service Plan** resource instead.

### Memory (Function App)
```powershell
az monitor metrics list `
  --resource $RESOURCE `
  --metric "MemoryWorkingSet,AverageMemoryWorkingSet,PrivateBytes" `
  --interval PT5M `
  --aggregation Average Minimum Maximum `
  --start-time $START_TIME `
  --end-time $END_TIME
```

### IO Metrics — CPU Proxy (Function App)
```powershell
# Bytes per second
az monitor metrics list `
  --resource $RESOURCE `
  --metric "IoReadBytesPerSecond,IoWriteBytesPerSecond,IoOtherBytesPerSecond" `
  --interval PT5M `
  --aggregation Average Minimum Maximum `
  --start-time $START_TIME `
  --end-time $END_TIME

# Operations per second
az monitor metrics list `
  --resource $RESOURCE `
  --metric "IoReadOperationsPerSecond,IoWriteOperationsPerSecond,IoOtherOperationsPerSecond" `
  --interval PT5M `
  --aggregation Average Minimum Maximum `
  --start-time $START_TIME `
  --end-time $END_TIME
```

### CPU Percentage (App Service Plan)
```powershell
az monitor metrics list `
  --resource $ASP_RESOURCE `
  --metric "CpuPercentage,MemoryPercentage,DiskQueueLength,HttpQueueLength" `
  --interval PT5M `
  --aggregation Average Minimum Maximum `
  --start-time $START_TIME `
  --end-time $END_TIME
```

### .NET GC Pressure
```powershell
az monitor metrics list `
  --resource $RESOURCE `
  --metric "Gen0Collections,Gen1Collections,Gen2Collections" `
  --interval PT5M `
  --aggregation Average Maximum `
  --start-time $START_TIME `
  --end-time $END_TIME
```

### Performance Counters via App Insights
```powershell
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    performanceCounters
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | where name in ('% Processor Time', 'Private Bytes', 'IO Data Bytes/sec')
    | summarize avgValue=avg(value) by name, bin(timestamp, 5m)
    | order by timestamp desc
  "

# List all available performance counters for this app
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    performanceCounters
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | distinct name, category
  "
```

---

## 5. Parallel Request Execution Metrics

Measures concurrency — how many requests were in-flight simultaneously across instances.

```powershell
# Requests and queue depth via Azure Monitor
az monitor metrics list `
  --resource $RESOURCE `
  --metric "Requests,RequestsInApplicationQueue" `
  --interval PT1M `
  --start-time $START_TIME `
  --end-time $END_TIME

# Max concurrent requests per minute via App Insights
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    requests
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | summarize concurrent=count() by bin(timestamp, 1m), cloud_RoleInstance
    | summarize maxConcurrent=max(concurrent) by bin(timestamp, 1m)
    | order by timestamp desc
  "

# Dependency (outbound call) failures
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    dependencies
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | where success == false
    | summarize count() by target, type, bin(timestamp, 15m)
    | order by timestamp desc
  "
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
# Exception rate
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    exceptions
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | summarize count() by bin(timestamp, 1m), type
    | order by timestamp desc
  "

# Error rate per function
az monitor app-insights query `
  --app $AI_NAME -g $RG `
  --analytics-query "
    requests
    | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME'))
    | summarize total=count(), failures=countif(success==false) by name
    | extend errorRate=round(100.0 * failures / total, 2)
    | order by errorRate desc
  "
```

---

## Application Insights REST API

An alternative to the CLI — useful for scripting, dashboards, or integrations.

### Authentication (Entra ID — API keys are retired)
```powershell
# Get bearer token
$TOKEN   = az account get-access-token --resource "https://api.applicationinsights.io" --query accessToken -o tsv
$HEADERS = @{ Authorization = "Bearer $TOKEN" }
$BASE    = "https://api.applicationinsights.io/v1/apps/$APP_ID"
$SPAN    = "timespan=$START_TIME/$END_TIME"
```

### Metrics Endpoint
```powershell
# Request count over time
Invoke-RestMethod -Uri "$BASE/metrics/requests/count?$SPAN&interval=PT5M" -Headers $HEADERS

# Avg + P95 + P99 duration
Invoke-RestMethod -Uri "$BASE/metrics/requests/duration?$SPAN&aggregation=avg,percentile_95,percentile_99&interval=PT5M" -Headers $HEADERS

# Failed requests
Invoke-RestMethod -Uri "$BASE/metrics/requests/failed/count?$SPAN" -Headers $HEADERS

# Exception count
Invoke-RestMethod -Uri "$BASE/metrics/exceptions/count?$SPAN" -Headers $HEADERS

# List all available metric IDs
Invoke-RestMethod -Uri "$BASE/metrics/metadata" -Headers $HEADERS
```

### Query Endpoint (KQL via POST)
```powershell
$BODY = @{
  query    = "requests | where timestamp between(datetime('$START_TIME') .. datetime('$END_TIME')) | summarize count(), avg(duration) by name | order by count_ desc"
  timespan = "$START_TIME/$END_TIME"
} | ConvertTo-Json

Invoke-RestMethod -Method POST -Uri "$BASE/query" -Headers $HEADERS -Body $BODY -ContentType "application/json"
```

### Events Endpoint (Individual Records)
```powershell
# Last 50 exceptions
Invoke-RestMethod -Uri "$BASE/events/exceptions?$SPAN&`$top=50&`$orderby=timestamp+desc" -Headers $HEADERS

# Recent requests for a specific function
Invoke-RestMethod -Uri "$BASE/events/requests?`$filter=request/name+eq+'HttpTrigger1'&`$top=100&$SPAN" -Headers $HEADERS
```

> **Supported event types:** `requests`, `traces`, `exceptions`, `dependencies`, `customEvents`, `pageViews`, `availabilityResults`, `$all`

---

## Utility Commands

```powershell
# List all valid metrics for the Function App
az monitor metrics list-definitions --resource $RESOURCE --output table

# List all valid metrics for the App Service Plan
az monitor metrics list-definitions --resource $ASP_RESOURCE --output table

# List all functions in the app
az functionapp function list --name $FUNC_APP -g $RG --output table

# Tail live logs
az webapp log tail --name $FUNC_APP --resource-group $RG

# Get subscription ID
az account show --query id -o tsv

# List all App Insights in subscription
az resource list `
  --resource-type "microsoft.insights/components" `
  --query "[].{Name:name, ResourceGroup:resourceGroup, Location:location}" `
  --output table
```

---

## Valid Metrics Reference for Microsoft.Web/sites

The following are all valid metric names for `--metric` when targeting a Function App resource:

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
