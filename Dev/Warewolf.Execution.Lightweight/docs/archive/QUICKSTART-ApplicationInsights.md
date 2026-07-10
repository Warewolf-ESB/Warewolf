# 🎯 Quick Start: Application Insights Testing Guide

## Overview

This guide shows you how to configure and test Application Insights integration with your Warewolf Execution Lightweight Azure Function.

---

## ⚡ Quick Setup (5 Minutes)

### Prerequisites
- Azure CLI installed and logged in (`az login`)
- Your Azure Function App already deployed
- PowerShell 7+ (or PowerShell Core)

### Option 1: Automated Setup (Recommended)

Run the provided PowerShell script:

```powershell
cd Warewolf.Execution.Lightweight\Scripts

.\Setup-ApplicationInsights.ps1 `
    -ResourceGroup "your-resource-group-name" `
    -FunctionAppName "your-function-app-name"
```

**That's it!** The script will:
1. Create Application Insights resource (if needed)
2. Configure your Function App
3. Enable telemetry
4. Show you next steps

---

### Option 2: Manual Setup (3 Commands)

```powershell
# 1. Create Application Insights
$AppInsightsName = "your-function-app-name-ai"
$ResourceGroup = "your-resource-group"
$Location = "eastus"

az monitor app-insights component create `
    --app $AppInsightsName `
    --location $Location `
    --resource-group $ResourceGroup `
    --application-type web

# 2. Get connection string
$ConnectionString = az monitor app-insights component show `
    --app $AppInsightsName `
    --resource-group $ResourceGroup `
    --query connectionString `
    --output tsv

# 3. Configure Function App
$FunctionAppName = "your-function-app-name"

az functionapp config appsettings set `
    --name $FunctionAppName `
    --resource-group $ResourceGroup `
    --settings "WAREWOLF_APPINSIGHTS_CONNECTION_STRING=$ConnectionString" "ENABLEAPPLICATIONINSIGHTS=true"
```

> **Note**: The connection string is deployed under the deliberately non-standard name `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` (the Azure Functions host does not recognise this name, so its built-in auto-AI pipeline stays dormant). `ENABLEAPPLICATIONINSIGHTS=true` is the single authoritative switch — the connection string alone does NOT enable telemetry.

---

## 🧪 Testing Application Insights

### Step 1: Trigger a Workflow

```powershell
# Replace with your actual Function App URL
$FunctionAppUrl = "https://your-function-app-name.azurewebsites.net"

# Trigger a workflow
Invoke-RestMethod -Uri "$FunctionAppUrl/api/workflow/Hello" -Method POST
```

### Step 2: Wait 2-3 Minutes

Application Insights has a slight delay for data ingestion. Be patient!

### Step 3: View Logs in Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your **Application Insights** resource
3. Click **Logs** in the left menu
4. Run the queries below

---

## 📊 Test Queries

### Query 1: View All Warewolf Logs

```kql
traces
| where timestamp > ago(10m)
| where customDimensions.Category contains "Warewolf"
| order by timestamp desc
| take 50
```

**Expected Output:**
- Startup logs: "Program starting - bootstrap logging active"
- Execution logs: Workflow execution steps
- Audit logs: Authentication/authorization events

---

### Query 2: View Function Invocations

```kql
requests
| where timestamp > ago(10m)
| where cloud_RoleName contains "your-function-app-name"
| project timestamp, name, success, resultCode, duration
| order by timestamp desc
```

**Expected Output:**
- Each HTTP request to your function
- Success/failure status
- Response time in milliseconds

---

### Query 3: View Errors and Exceptions

```kql
exceptions
| where timestamp > ago(10m)
| project timestamp, type, outerMessage, problemId
| order by timestamp desc
```

**Expected Output:**
- Any unhandled exceptions
- Full stack traces
- Error correlation IDs

---

### Query 4: View Audit Events (Security)

```kql
traces
| where timestamp > ago(10m)
| where customDimensions.EventId == 9000
| where message contains "[AUDIT]"
| project timestamp, message, customDimensions.ExecutionId
| order by timestamp desc
```

**Expected Output:**
- Authentication attempts
- Authorization failures (401/403)
- Security-related events

---

### Query 5: Track Specific Execution

```kql
// Replace with your actual ExecutionId from logs
let executionId = "12345678-1234-1234-1234-123456789012";

union traces, requests, dependencies, exceptions
| where timestamp > ago(24h)
| where customDimensions.ExecutionId == executionId
| order by timestamp asc
| project timestamp, itemType, message, name, duration
```

**Expected Output:**
- Complete timeline of a single workflow execution
- All logs, requests, and dependencies for that execution

---

## ✅ Verification Checklist

| Check | Command/Action | Expected Result |
|-------|----------------|-----------------|
| **1. AI Resource Created** | Check Azure Portal → Application Insights | Resource exists |
| **2. Connection String Set** | `az functionapp config appsettings list` | `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` present |
| **3. Logging Enabled** | `az functionapp config appsettings list` | `ENABLEAPPLICATIONINSIGHTS=true` |
| **4. Telemetry Flowing** | Run Query 1 above | See logs from last 10 minutes |
| **5. Requests Tracked** | Run Query 2 above | See function invocations |

---

## 🐛 Troubleshooting

### Problem: No logs appearing in Application Insights

**Solution:**
```powershell
# 1. Verify settings
$FunctionAppName = "your-function-app-name"
$ResourceGroup = "your-resource-group"

az functionapp config appsettings list `
    --name $FunctionAppName `
    --resource-group $ResourceGroup `
    --query "[?name=='WAREWOLF_APPINSIGHTS_CONNECTION_STRING' || name=='ENABLEAPPLICATIONINSIGHTS']"

# 2. Restart Function App
az functionapp restart --name $FunctionAppName --resource-group $ResourceGroup

# 3. Check Azure Log Stream (real-time)
az webapp log tail --name $FunctionAppName --resource-group $ResourceGroup
```

### Problem: "Failed to create Application Insights"

**Solution:**
- Verify you have `Contributor` role on the resource group
- Check if the name is already taken (must be globally unique)
- Try a different Azure region

### Problem: Connection string is empty

**Solution:**
```powershell
# Manually retrieve the connection string
az monitor app-insights component show `
    --app $AppInsightsName `
    --resource-group $ResourceGroup `
    --query connectionString `
    --output tsv
```

---

## 📈 Live Metrics (Real-Time Monitoring)

For **real-time telemetry** as your function executes:

1. Go to Azure Portal → Your Application Insights resource
2. Click **Live Metrics** in the left menu
3. Trigger a workflow execution
4. Watch metrics appear in real-time:
   - Incoming requests
   - Request duration
   - Failed requests
   - Server metrics (CPU, memory)

**No delay!** This view updates instantly.

---

## 💰 Cost Management

### Typical Costs (Approximate)

For a low-traffic function app:
- **Data ingestion**: ~$2.30 per GB
- **First 5 GB/month**: FREE
- **Data retention**: 90 days included, $0.12/GB/month beyond that

### Reduce Costs

1. **Enable sampling** in `host.json`:
   ```json
   {
     "logging": {
       "applicationInsights": {
         "samplingSettings": {
           "isEnabled": true,
           "maxTelemetryItemsPerSecond": 5
         }
       }
     }
   }
   ```

2. **Lower log levels** to Warning/Error in production

3. **Disable verbose dependencies** in `host.json`

---

## 🔗 Additional Resources

- **Full Documentation**: `docs/README-ApplicationInsights.md`
- **Setup Script**: `Scripts/Setup-ApplicationInsights.ps1`
- **Microsoft Docs**: [Application Insights for Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-monitoring)
- **KQL Reference**: [Kusto Query Language](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)

---

## 🎉 Next Steps

1. ✅ Set up alerts for critical conditions
2. 📊 Create custom dashboards
3. 🔍 Explore dependencies and performance counters
4. 📖 Read the full documentation in `docs/README-ApplicationInsights.md`

---

**Questions?** Check the full guide at `docs/README-ApplicationInsights.md` or review Azure Function logs with:
```powershell
az webapp log tail --name $FunctionAppName --resource-group $ResourceGroup
```
