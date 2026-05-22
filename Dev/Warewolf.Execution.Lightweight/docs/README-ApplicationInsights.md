# Application Insights Configuration Guide

## Overview

This guide explains how to configure Application Insights for the Warewolf Execution Lightweight Azure Functions project to enable rich telemetry, structured logging, and performance monitoring.

## What is Application Insights?

Application Insights is an Azure monitoring service that provides:
- **Real-time telemetry**: Live metrics, request rates, response times
- **Structured logging**: Searchable logs with custom dimensions
- **Exception tracking**: Full stack traces with context
- **Performance monitoring**: Dependency tracking, performance counters
- **Distributed tracing**: End-to-end request correlation across services

## Architecture

```
Your Code → AzureExecutionLogger → ILogger<T> → AI SDK → Application Insights Cloud
                                                           ↓
                                              Azure Portal → Logs & Metrics
```

### Logger Comparison

| Logger | Purpose | Destination | When Enabled |
|--------|---------|-------------|--------------|
| `ConsoleExecutionLogger` | Basic stdout logging | Log Stream | Always (mandatory) |
| `AzureExecutionLogger` | Rich AI telemetry | Application Insights | When AI is configured |
| `ElasticsearchExecutionLogger` | Centralized log aggregation | Elasticsearch | When Elastic is enabled |
| `AuditExecutionLogger` | Security audit trail | Log Stream + AI | Always (mandatory) |

---

## Prerequisites

1. **Azure Subscription** with permissions to create resources
2. **Application Insights resource** in Azure
3. **Azure CLI** installed ([Install Guide](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli))
4. **PowerShell 7+** (for running scripts)

---

## Setup Instructions

### Step 1: Create Application Insights Resource

#### Option A: Using Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **Create a resource** → Search for **Application Insights**
3. Fill in:
   - **Name**: `warewolf-execution-ai` (or your preferred name)
   - **Resource Group**: Same as your Function App
   - **Region**: Same as your Function App
   - **Workspace**: Create new or use existing Log Analytics workspace
4. Click **Review + Create** → **Create**
5. After deployment, go to the resource and copy the **Connection String** from the Overview page

#### Option B: Using Azure CLI

```powershell
# Variables
$ResourceGroup = "your-resource-group"
$Location = "eastus"
$AppInsightsName = "warewolf-execution-ai"

# Create Application Insights
az monitor app-insights component create `
  --app $AppInsightsName `
  --location $Location `
  --resource-group $ResourceGroup `
  --application-type web `
  --kind web

# Get the connection string
az monitor app-insights component show `
  --app $AppInsightsName `
  --resource-group $ResourceGroup `
  --query connectionString `
  --output tsv
```

Copy the connection string output. It will look like:
```
InstrumentationKey=12345678-1234-1234-1234-123456789012;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=abcd1234-5678-90ef-ghij-klmnopqrstuv
```

---

### Step 2: Configure Your Azure Function App

#### Option A: Using Azure Portal

1. Go to your **Function App** in Azure Portal
2. Navigate to **Settings** → **Configuration**
3. Under **Application settings**, click **+ New application setting**
4. Add:
   - **Name**: `APPLICATIONINSIGHTS_CONNECTION_STRING`
   - **Value**: Paste the connection string from Step 1
5. Click **OK** → **Save** → **Continue**
6. The Function App will restart automatically

#### Option B: Using Azure CLI

```powershell
# Variables
$FunctionAppName = "your-function-app-name"
$ResourceGroup = "your-resource-group"
$ConnectionString = "InstrumentationKey=...your-connection-string..."

# Set the connection string
az functionapp config appsettings set `
  --name $FunctionAppName `
  --resource-group $ResourceGroup `
  --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=$ConnectionString"
```

---

### Step 3: Enable Application Insights Logging

Set the environment variable to enable `AzureExecutionLogger`:

```powershell
# Enable Application Insights logging
az functionapp config appsettings set `
  --name $FunctionAppName `
  --resource-group $ResourceGroup `
  --settings "ENABLEAPPLICATIONINSIGHTS=true"
```

---

### Step 4: Verify Configuration

#### Check Application Settings

```powershell
az functionapp config appsettings list `
  --name $FunctionAppName `
  --resource-group $ResourceGroup `
  --query "[?name=='APPLICATIONINSIGHTS_CONNECTION_STRING' || name=='ENABLEAPPLICATIONINSIGHTS']" `
  --output table
```

Expected output:
```
Name                                   Value
------------------------------------   --------------------------------------------------
APPLICATIONINSIGHTS_CONNECTION_STRING  InstrumentationKey=...
ENABLEAPPLICATIONINSIGHTS              true
```

---

## Testing Application Insights

### Local Testing

For local testing, update your `local.settings.json`:

```json
{
  "Values": {
    "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=...your-connection-string...",
    "ENABLEAPPLICATIONINSIGHTS": "true"
  }
}
```

⚠️ **Note**: Do NOT commit `local.settings.json` with real connection strings to source control!

### Test 1: Verify Telemetry is Being Sent

1. **Start your function locally** or trigger an Azure function
2. **Wait 2-3 minutes** (AI has a delay for ingestion)
3. **Go to Azure Portal** → Your Application Insights resource
4. Click **Logs** in the left menu
5. Run this query:

```kql
traces
| where timestamp > ago(10m)
| where customDimensions.Category contains "Warewolf.Execution.Lightweight"
| order by timestamp desc
| take 50
```

**Expected Results**: You should see log entries from your functions with:
- `message`: Your log messages
- `customDimensions.ExecutionId`: Correlation IDs
- `customDimensions.Category`: Logger category names
- `severityLevel`: Log level (0=Verbose, 1=Info, 2=Warning, 3=Error, 4=Critical)

---

### Test 2: Check Function Invocations

```kql
requests
| where timestamp > ago(10m)
| where cloud_RoleName == "your-function-app-name"
| project timestamp, name, success, resultCode, duration
| order by timestamp desc
```

**Expected Results**: Each HTTP trigger invocation creates a `request` entry showing:
- Function name
- Success/failure status
- HTTP status code
- Duration

---

### Test 3: View Exceptions

```kql
exceptions
| where timestamp > ago(10m)
| where customDimensions.Category contains "Warewolf"
| project timestamp, type, outerMessage, innermostMessage, problemId
| order by timestamp desc
```

**Expected Results**: Any exceptions logged via `Dev2Logger.Error()` or unhandled exceptions appear here with full stack traces.

---

### Test 4: View Audit Logs (Security Events)

```kql
traces
| where timestamp > ago(10m)
| where customDimensions.EventId == 9000
| where message contains "[AUDIT]"
| order by timestamp desc
```

**Expected Results**: Security audit events from `AuditExecutionLogger` with `EventId=9000`.

---

### Test 5: Live Metrics (Real-Time Monitoring)

1. Go to **Application Insights** → **Live Metrics**
2. Trigger a workflow execution
3. Watch **real-time** incoming requests, dependencies, and logs

---

## Querying Application Insights

### Common KQL Queries

#### View All Warewolf Logs by Level

```kql
traces
| where timestamp > ago(1h)
| where customDimensions.Category contains "Warewolf"
| summarize count() by severityLevel
| render piechart
```

#### Find Slow Workflow Executions

```kql
requests
| where timestamp > ago(1h)
| where name contains "WorkflowExecutor"
| where duration > 5000  // milliseconds
| project timestamp, name, duration, resultCode, customDimensions.ExecutionId
| order by duration desc
```

#### Track Specific Execution by ID

```kql
union traces, requests, dependencies, exceptions
| where timestamp > ago(24h)
| where customDimensions.ExecutionId == "your-execution-guid"
| order by timestamp asc
| project timestamp, itemType, message, name, resultCode, duration
```

#### Monitor Authentication Failures

```kql
traces
| where timestamp > ago(1h)
| where customDimensions.EventId == 9000  // Audit events
| where message contains "401" or message contains "403"
| project timestamp, message, customDimensions.ExecutionId
```

---

## Troubleshooting

### Problem: No Data Appearing in Application Insights

**Possible Causes:**
1. ❌ Connection string not set or incorrect
2. ❌ `ENABLEAPPLICATIONINSIGHTS` not set to `true`
3. ⏱️ Data ingestion delay (wait 2-5 minutes)
4. ❌ Function app not restarted after config change

**Solution:**
```powershell
# Verify settings
az functionapp config appsettings list `
  --name $FunctionAppName `
  --resource-group $ResourceGroup `
  --query "[?name=='APPLICATIONINSIGHTS_CONNECTION_STRING' || name=='ENABLEAPPLICATIONINSIGHTS']"

# Restart function app
az functionapp restart --name $FunctionAppName --resource-group $ResourceGroup
```

---

### Problem: Too Much Telemetry / High Costs

**Solution:** Adjust sampling in `host.json`:

```json
{
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "maxTelemetryItemsPerSecond": 5  // Reduce from 20
      }
    }
  }
}
```

Redeploy after changing `host.json`.

---

### Problem: Console Logs vs Application Insights Logs

**Question:** Why do I see logs in both places?

**Answer:**
- `ConsoleExecutionLogger` → Always writes to stdout (Log Stream)
- `AzureExecutionLogger` → Writes to Application Insights (when enabled)
- Both are active simultaneously in the `CompositeExecutionLogger`

**When AI is configured:**
- Console logs appear in **Log Stream** (real-time, no persistence)
- AI logs appear in **Application Insights** (searchable, retained based on retention policy)

---

## Production Recommendations

### 1. Set Appropriate Log Levels

In `host.json`, set production-friendly levels:

```json
{
  "logging": {
    "logLevel": {
      "default": "Warning",
      "Function": "Information",  // See all function invocations
      "Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger": "Information",
      "Warewolf.Execution.Lightweight.Security.AuditLogger": "Information"  // All audit events
    }
  }
}
```

### 2. Enable Sampling to Control Costs

```json
{
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "maxTelemetryItemsPerSecond": 20,
        "excludedTypes": "Request;Exception"  // Never sample critical data
      }
    }
  }
}
```

### 3. Set Data Retention

In Azure Portal → Application Insights → Usage and estimated costs → Data Retention:
- Default: 90 days
- Maximum: 730 days (additional cost)

### 4. Create Alerts

Set up alerts for:
- **Failed requests** > threshold
- **High exception rate**
- **Slow response times**
- **Audit failures** (EventId=9000 with errors)

### 5. Use Log Analytics Workspaces

Link Application Insights to a **Log Analytics Workspace** for:
- Cross-resource queries
- Long-term data retention
- Advanced KQL queries across multiple services

---

## Cost Optimization

### Understanding Costs

Application Insights charges based on:
1. **Data ingestion** (GB per month)
2. **Data retention** (beyond 90 days)
3. **Web tests** and **live metrics**

### Reduce Costs

1. **Enable sampling** (as shown above)
2. **Lower log levels** to Warning/Error in production
3. **Disable trace collection** for noisy dependencies:
   ```json
   {
     "logging": {
       "applicationInsights": {
         "samplingSettings": {
           "excludedTypes": "Dependency;Event"
         }
       }
     }
   }
   ```
4. **Set shorter retention** for less critical data

---

## Integration with Other Loggers

All loggers work together in the `CompositeExecutionLogger`:

| Logger | Always Active? | Best For |
|--------|----------------|----------|
| **ConsoleExecutionLogger** | ✅ Yes | Real-time debugging, local development |
| **AzureExecutionLogger** | ⚙️ Optional | Production monitoring, alerts, dashboards |
| **ElasticsearchExecutionLogger** | ⚙️ Optional | Centralized log aggregation, compliance |
| **AuditExecutionLogger** | ✅ Yes | Security compliance, forensics |

**All four can be active simultaneously** — logs are fanned out to all registered sinks.

---

## Next Steps

1. ✅ Configure Application Insights (this guide)
2. 📊 Create custom dashboards in Azure Portal
3. 🔔 Set up alerts for critical conditions
4. 📖 Read [Elasticsearch Logging Guide](README-Encryption.md) for centralized logging
5. 🔐 Read [Security Audit Guide](SecurityChecklist.md) for audit trail best practices

---

## Resources

- [Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure Functions Monitoring](https://learn.microsoft.com/en-us/azure/azure-functions/functions-monitoring)
- [KQL Query Language](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)
- [Application Insights Pricing](https://azure.microsoft.com/en-us/pricing/details/monitor/)

---

## Support

For issues or questions:
- Review Azure Function App logs: `az webapp log tail`
- Check Application Insights diagnostics: Portal → Diagnose and solve problems
- Review this project's documentation in `/docs`
