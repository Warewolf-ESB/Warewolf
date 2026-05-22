# ✅ Application Insights Integration - Implementation Summary

## What Was Done

Application Insights integration has been successfully configured for your Warewolf Execution Lightweight Azure Functions project.

---

## 📦 Changes Made

### 1. **NuGet Package** (Already Present)
- ✅ `Microsoft.Azure.Functions.Worker.ApplicationInsights` v1.4.0
- Located in: `Warewolf.Execution.Lightweight.csproj` line 82

### 2. **host.json Configuration**
- ✅ Added Application Insights settings:
  - Sampling configuration (20 items/second max)
  - Dependency tracking enabled
  - Performance counters collection enabled
  - W3C distributed tracing enabled
  - HTTP trigger extended info collection enabled

### 3. **local.settings.json**
- ✅ Added `APPLICATIONINSIGHTS_CONNECTION_STRING` placeholder
- This will be empty locally but must be set in Azure

### 4. **Documentation**
- ✅ `docs/README-ApplicationInsights.md` - Complete guide (full documentation)
- ✅ `docs/QUICKSTART-ApplicationInsights.md` - Quick testing guide
- ✅ `Scripts/Setup-ApplicationInsights.ps1` - Automated setup script

---

## 🔍 How Application Insights Works

### Architecture Flow

```
Dev2Logger.Info("message", executionId)
    ↓
CompositeExecutionLogger (fan-out to all loggers)
    ├─→ ConsoleExecutionLogger → stdout → Azure Log Stream
    ├─→ AzureExecutionLogger → ILogger<T> → Application Insights SDK → AI Cloud
    ├─→ ElasticsearchExecutionLogger → Elasticsearch Cluster
    └─→ AuditExecutionLogger → stdout + EventId=9000 tag
```

### Key Points

1. **Automatic Integration**: When `APPLICATIONINSIGHTS_CONNECTION_STRING` is set, the Azure Functions runtime automatically sends telemetry to Application Insights

2. **AzureExecutionLogger**: Your custom logger that uses `ILogger<T>`, which feeds into Application Insights when configured

3. **CompositeExecutionLogger**: Fans out logs to all registered loggers simultaneously

4. **No Code Changes Needed**: Application Insights is configured via environment variables and `host.json`

---

## 🎯 What You Need to Do in Azure

### Step 1: Run the Setup Script

**Open PowerShell in the project directory:**

```powershell
cd D:\Warewolf\Warewolf_Net6\Dev\Warewolf.Execution.Lightweight\Scripts

.\Setup-ApplicationInsights.ps1 `
    -ResourceGroup "your-resource-group-name" `
    -FunctionAppName "your-function-app-name"
```

**The script will:**
1. ✅ Create Application Insights resource (if it doesn't exist)
2. ✅ Get the connection string
3. ✅ Configure your Function App with the connection string
4. ✅ Enable `ENABLEAPPLICATIONINSIGHTS=true`
5. ✅ Verify the configuration

---

### Step 2: Test It

**Option A: Use PowerShell**

```powershell
# Trigger a workflow
$FunctionAppUrl = "https://your-function-app-name.azurewebsites.net"
Invoke-RestMethod -Uri "$FunctionAppUrl/api/workflow/Hello" -Method POST

# Wait 2-3 minutes for data to flow to Application Insights
```

**Option B: Use Azure Portal**

1. Go to your Function App → Functions → Select a function → Click "Test/Run"
2. Execute the function
3. Wait 2-3 minutes

---

### Step 3: View Logs in Application Insights

1. **Go to Azure Portal** → Your Application Insights resource
2. Click **Logs** in the left menu
3. Run this query:

```kql
traces
| where timestamp > ago(10m)
| where customDimensions.Category contains "Warewolf"
| order by timestamp desc
| take 50
```

**You should see:**
- Startup logs: "Program starting - bootstrap logging active"
- Execution logs: Workflow execution traces
- Audit logs: Security events with `[AUDIT]` prefix

---

## 📊 What Logs Will Show Up in Application Insights?

### From AzureExecutionLogger (When Enabled)

| Log Level | Method | Example | AI Table |
|-----------|--------|---------|----------|
| Debug | `Dev2Logger.Debug("msg", id)` | Detailed execution traces | `traces` |
| Info | `Dev2Logger.Info("msg", id)` | Milestones, state changes | `traces` |
| Warning | `Dev2Logger.Warn("msg", id)` | Recoverable issues | `traces` |
| Error | `Dev2Logger.Error("msg", ex, id)` | Failures with stack trace | `traces` + `exceptions` |
| Fatal | `Dev2Logger.Fatal("msg", ex, id)` | Critical failures | `traces` + `exceptions` |

### From AuditExecutionLogger (Always Enabled)

| Event | Example | AI Filter |
|-------|---------|-----------|
| Authentication failures | 401 Unauthorized | `EventId=9000` |
| Authorization failures | 403 Forbidden | `EventId=9000` |
| Security events | Policy violations | `EventId=9000` |

---

## 🔄 Difference Between Loggers

### ConsoleExecutionLogger vs AzureExecutionLogger

| Feature | ConsoleExecutionLogger | AzureExecutionLogger |
|---------|------------------------|----------------------|
| **Always Active?** | ✅ Yes (mandatory) | ⚙️ Optional (when AI is configured) |
| **Destination** | stdout → Log Stream | Application Insights Cloud |
| **Structured Logging** | ❌ Plain text | ✅ Rich structured data |
| **Custom Dimensions** | ❌ No | ✅ Yes (ExecutionId, Category, etc.) |
| **Searchable** | ❌ No | ✅ Yes (KQL queries) |
| **Retention** | ❌ Not persisted | ✅ 90 days default |
| **Cost** | 🆓 Free | 💰 Pay per GB (first 5GB free) |
| **Real-time** | ✅ Instant | ⏱️ 2-3 minute delay |
| **Best For** | Local debugging, quick checks | Production monitoring, analytics |

**Key Insight:** Both run simultaneously! ConsoleExecutionLogger ensures you never lose logs, while AzureExecutionLogger provides rich analysis.

---

## 🧪 Testing Checklist

Use this checklist to verify Application Insights is working:

### In Azure Portal

- [ ] Application Insights resource exists
- [ ] Function App has `APPLICATIONINSIGHTS_CONNECTION_STRING` setting
- [ ] Function App has `ENABLEAPPLICATIONINSIGHTS=true` setting
- [ ] Function App restarted after configuration

### Run Test Queries

1. **View recent logs** (Query 1 in QUICKSTART guide):
   ```kql
   traces | where timestamp > ago(10m) | where customDimensions.Category contains "Warewolf"
   ```
   - [ ] See startup logs
   - [ ] See execution logs

2. **View function invocations** (Query 2):
   ```kql
   requests | where timestamp > ago(10m)
   ```
   - [ ] See HTTP requests
   - [ ] See success/failure status
   - [ ] See duration

3. **View audit events** (Query 4):
   ```kql
   traces | where customDimensions.EventId == 9000
   ```
   - [ ] See security events tagged with `[AUDIT]`

---

## 🐛 Troubleshooting

### No Data in Application Insights?

**Check these in order:**

1. **Verify connection string is set:**
   ```powershell
   az functionapp config appsettings list `
       --name $FunctionAppName `
       --resource-group $ResourceGroup `
       --query "[?name=='APPLICATIONINSIGHTS_CONNECTION_STRING']"
   ```

2. **Verify logging is enabled:**
   ```powershell
   az functionapp config appsettings list `
       --name $FunctionAppName `
       --resource-group $ResourceGroup `
       --query "[?name=='ENABLEAPPLICATIONINSIGHTS']"
   ```

3. **Restart the Function App:**
   ```powershell
   az functionapp restart --name $FunctionAppName --resource-group $ResourceGroup
   ```

4. **Check console logs first (always works):**
   ```powershell
   az webapp log tail --name $FunctionAppName --resource-group $ResourceGroup
   ```

5. **Wait 2-5 minutes** - Application Insights has data ingestion delay

---

## 📖 Documentation Structure

| File | Purpose | When to Use |
|------|---------|-------------|
| **QUICKSTART-ApplicationInsights.md** | Fast testing guide | When you just want to test AI quickly |
| **README-ApplicationInsights.md** | Complete reference | For deep understanding and advanced config |
| **Setup-ApplicationInsights.ps1** | Automated setup | For quick Azure configuration |
| **This file (SUMMARY.md)** | Implementation overview | To understand what was changed |

---

## 🎉 What's Next?

### Immediate Next Steps

1. **Run the setup script** to configure Azure
2. **Trigger a workflow** to generate telemetry
3. **View logs in Application Insights** using the test queries

### Future Enhancements

1. **Create Dashboards**: Visualize workflow performance, error rates
2. **Set Up Alerts**: Get notified of failures or slow executions
3. **Analyze Trends**: Track performance over time
4. **Correlate Logs**: Use ExecutionId to trace entire request flows

---

## 💡 Key Takeaways

1. **Application Insights is configured but not enabled by default**
   - You must set `APPLICATIONINSIGHTS_CONNECTION_STRING` in Azure
   - You must set `ENABLEAPPLICATIONINSIGHTS=true`

2. **AzureExecutionLogger ≠ Application Insights**
   - `AzureExecutionLogger` is your wrapper class
   - Application Insights is the Microsoft cloud service
   - They work together when configured

3. **Console logs still work**
   - `ConsoleExecutionLogger` is always active
   - Use `az webapp log tail` for real-time logs
   - Application Insights provides persistence and analysis

4. **Zero code changes needed**
   - Everything is configured via environment variables
   - Package is already installed
   - Just run the setup script and test

---

## 🔗 Quick Links

- **Setup Script**: `Scripts/Setup-ApplicationInsights.ps1`
- **Quick Start Guide**: `docs/QUICKSTART-ApplicationInsights.md`
- **Full Documentation**: `docs/README-ApplicationInsights.md`
- **Azure Portal**: https://portal.azure.com

---

## ✅ Ready to Configure?

Run this command to get started:

```powershell
cd D:\Warewolf\Warewolf_Net6\Dev\Warewolf.Execution.Lightweight\Scripts

.\Setup-ApplicationInsights.ps1 `
    -ResourceGroup "your-rg-here" `
    -FunctionAppName "your-function-app-here"
```

**That's it!** The script will guide you through the rest.

---

**Questions?** Review the documentation files or check Azure Function logs with:
```powershell
az webapp log tail --name $FunctionAppName --resource-group $ResourceGroup
```
