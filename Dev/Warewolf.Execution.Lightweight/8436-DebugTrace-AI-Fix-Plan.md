# Fix Plan: Debug and Trace Logging in Application Insights & Live Log Stream

**Branch:** `8436-DebugAndTraceInApplicationInsights`  
**Target:** .NET 8 / Azure Functions v4 Isolated Worker  
**Commit under review:** `4651d8f8cecda0b213bd8f64f3e9199582cce900`  
**Date:** 2026  

---

## Executive Summary

Commit `4651d8f8` attempts to enable `Debug` and `Trace` level logging in Azure Application
Insights and the Azure Portal Live Log Stream by removing the AI SDK's built-in `Warning`
gate. The intent is correct; however six defects prevent the goal from being achieved:

| # | Severity | Description |
|---|----------|-------------|
| 1 | 🔴 Critical | `AddApplicationInsightsTelemetryWorkerService()` double-registers AI, causing duplicate telemetry |
| 2 | 🔴 Critical | `options.MinLevel = LogLevel.Debug` is hardcoded — `EXECUTIONLOGLEVEL=TRACE` is silently dropped |
| 3 | 🟠 Major   | `LoggerFilterOptions.MinLevel` is global — opens all providers, not just AI |
| 4 | 🟠 Major   | Bootstrap `LoggerFactory` hardcodes `LogLevel.Debug` — Trace logs lost before DI host exists |
| 5 | 🟡 Minor   | AI configuration runs unconditionally even when `ENABLEAPPLICATIONINSIGHTS=false` |
| 6 | 🟡 Minor   | `AzureExecutionLogger` collapses structured parameters into an interpolated string |

The plan below fixes all six defects in dependency order across four files.

---

## Files to Change

| File | Change |
|------|--------|
| `Logging/LoggingConfiguration.cs` | Add `MelMinimumLevel` computed property (Step 1) |
| `Program.cs` | Fix bootstrap gate (Step 2), fix AI registration (Step 3), fix filter logic (Step 4) |
| `Logging/AzureExecutionLogger.cs` | Fix structured logging parameters (Step 5) |
| `Warewolf.Execution.Lightweight.csproj` | Remove redundant NuGet package (Step 6) |

---

## Step 1 — Add `MelMinimumLevel` to `LoggingConfiguration`

**File:** `Logging/LoggingConfiguration.cs`  
**Defect fixed:** #2, #4  
**Why first:** All subsequent steps depend on this property to map the Dev2 level to a
Microsoft.Extensions.Logging (MEL) level. Dev2 uses a higher-is-more-verbose convention
(`TRACE=6 … OFF=0`) while MEL uses lower-is-more-verbose (`Trace=0 … None=6`). Without
an explicit mapping, `EXECUTIONLOGLEVEL=TRACE` is never honoured inside the MEL pipeline.

### What to do

1. Add a `using` alias at the top of `LoggingConfiguration.cs`:
   ```csharp
   using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;
   using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;
   ```

2. Add the following computed property to the `LoggingConfiguration` record (after the
   existing `IsDevelopment` property):
   ```csharp
   /// <summary>
   /// Maps the Dev2 <see cref="MinimumLevel"/> to its MEL equivalent for use in
   /// <see cref="Microsoft.Extensions.Logging.LoggerFilterOptions"/> and the
   /// bootstrap <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>.
   ///
   /// Dev2 convention: higher numeric value = more verbose (TRACE=6, OFF=0).
   /// MEL convention:  lower  numeric value = more verbose (Trace=0, None=6).
   /// </summary>
   public MelLogLevel MelMinimumLevel => MinimumLevel switch
   {
       Dev2LogLevel.TRACE => MelLogLevel.Trace,
       Dev2LogLevel.DEBUG => MelLogLevel.Debug,
       Dev2LogLevel.INFO  => MelLogLevel.Information,
       Dev2LogLevel.WARN  => MelLogLevel.Warning,
       Dev2LogLevel.ERROR => MelLogLevel.Error,
       Dev2LogLevel.FATAL => MelLogLevel.Critical,
       Dev2LogLevel.OFF   => MelLogLevel.None,
       _                  => MelLogLevel.Information,
   };
   ```

3. Update the XML summary block at the top of the file to document the new property.

### Verification
- `EXECUTIONLOGLEVEL=TRACE` → `MelMinimumLevel == LogLevel.Trace`
- `EXECUTIONLOGLEVEL=DEBUG` → `MelMinimumLevel == LogLevel.Debug`
- `EXECUTIONLOGLEVEL=INFO`  → `MelMinimumLevel == LogLevel.Information`
- Unset / unknown value   → `MelMinimumLevel == LogLevel.Information` (default)

---

## Step 2 — Fix Bootstrap Logger Level Gate

**File:** `Program.cs`, line 19  
**Defect fixed:** #4  
**Why before Step 3:** The bootstrap logger is instantiated before DI is built (Step 2 in
`Program.cs`). Any startup `Dev2Logger.Trace(...)` call between lines 19 and 55 routes
through this factory. The hardcoded `LogLevel.Debug` gate silently drops Trace logs
emitted during the `Load()` / `FromEnvironment()` call at lines 15–16.

### Current code
```csharp
using var bootstrapFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));
```

### What to do

Replace the `SetMinimumLevel` argument with `loggingConfig.MelMinimumLevel`:
```csharp
using var bootstrapFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(loggingConfig.MelMinimumLevel));
```

### Verification
- Set `EXECUTIONLOGLEVEL=TRACE`; restart host locally.
- Startup `Dev2Logger.Trace(...)` lines (e.g. line 28) should appear in the console
  **before** the DI host is built.

---

## Step 3 — Fix AI Service Registration (Remove Double-Registration)

**File:** `Program.cs`, lines 39–40  
**File:** `Warewolf.Execution.Lightweight.csproj`, line 98  
**Defect fixed:** #1, #5  
**Why before Step 4:** Step 4 configures filter rules for the AI provider. If two AI
registrations exist, the second `Configure<LoggerFilterOptions>` call may target the
wrong registration, producing non-deterministic results.

### Current code
```csharp
services.AddApplicationInsightsTelemetryWorkerService();  // ← remove
services.ConfigureFunctionsApplicationInsights();
```

### What to do

1. **Remove** the `AddApplicationInsightsTelemetryWorkerService()` call entirely.
   `ConfigureFunctionsApplicationInsights()` (from
   `Microsoft.Azure.Functions.Worker.ApplicationInsights`) already calls the worker
   service registration internally.

2. **Guard** `ConfigureFunctionsApplicationInsights()` behind the `EnableApplicationInsights`
   flag so AI is not registered when the user has not opted in:
   ```csharp
   if (loggingConfig.EnableApplicationInsights)
   {
       services.ConfigureFunctionsApplicationInsights();
   }
   ```

3. In `Warewolf.Execution.Lightweight.csproj` **remove** the explicit package reference:
   ```xml
   <!-- DELETE this line: -->
   <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.23.0" />
   ```
   It is already a transitive dependency of `Microsoft.Azure.Functions.Worker.ApplicationInsights`.

### Verification
- With `ENABLEAPPLICATIONINSIGHTS=false`: AI provider is not registered; no telemetry
  sent; no duplicate entries in `dotnet-trace` output.
- With `ENABLEAPPLICATIONINSIGHTS=true`: `TelemetryConfiguration` appears exactly once
  in `IServiceProvider`; no duplicate `TelemetryInitializer` registrations.

---

## Step 4 — Fix `LoggerFilterOptions` — Targeted AI Rule, Not Global Gate

**File:** `Program.cs`, lines 42–53  
**Defect fixed:** #2, #3, #5  
**Depends on:** Step 1 (`MelMinimumLevel`), Step 3 (guarded AI registration)

### Current code
```csharp
services.Configure<LoggerFilterOptions>(options =>
{
    var defaultRule = options.Rules.FirstOrDefault(rule =>
        rule.ProviderName ==
        "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
    if (defaultRule is not null)
        options.Rules.Remove(defaultRule);

    options.MinLevel = LogLevel.Debug;   // ← hardcoded global gate
});
```

### Problems with current code
- `options.MinLevel` is a **global** floor. Setting it to `Debug` opens Debug/Trace on
  ALL providers (Console, Elasticsearch, Audit), not just AI. This floods all sinks
  with internal framework logs from `Microsoft.*` and `System.*` namespaces and
  increases AI ingestion cost.
- `LogLevel.Debug` is hardcoded so `EXECUTIONLOGLEVEL=TRACE` is never honoured.
- This block runs even when AI is disabled (now guarded by Step 3, but the logic is
  still wrong).

### What to do

Replace the entire `Configure<LoggerFilterOptions>` block with a targeted AI-only rule
**inside** the `if (loggingConfig.EnableApplicationInsights)` guard added in Step 3:

```csharp
if (loggingConfig.EnableApplicationInsights)
{
    services.ConfigureFunctionsApplicationInsights();

    services.Configure<LoggerFilterOptions>(options =>
    {
        // Remove AI SDK's built-in Warning gate so our env-var level takes effect.
        const string aiProvider =
            "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider";

        var defaultRule = options.Rules.FirstOrDefault(r => r.ProviderName == aiProvider);
        if (defaultRule is not null)
            options.Rules.Remove(defaultRule);

        // Add a targeted rule for the AI provider only — does NOT affect Console,
        // Elasticsearch, or any other registered provider.
        options.Rules.Add(new LoggerFilterRule(
            providerName: aiProvider,
            categoryName: null,
            logLevel:     loggingConfig.MelMinimumLevel,   // ← from EXECUTIONLOGLEVEL env var
            filter:       null));
    });
}
```

### Why `LoggerFilterRule` is correct here

| Approach | Scope | Side-effect |
|----------|-------|-------------|
| `options.MinLevel = LogLevel.Debug` | **All providers** | Floods Console, Elastic, Audit with framework Debug logs |
| `options.Rules.Add(new LoggerFilterRule(providerName: aiProvider, ...))` | **AI only** | No effect on other providers |

### Verification
- `EXECUTIONLOGLEVEL=TRACE`, `ENABLEAPPLICATIONINSIGHTS=true`:
  - AI provider rule: `LogLevel.Trace` → Trace logs appear in AI `traces` table.
  - Console provider: unaffected — still gates at `Trace` via `SetMinimumLevel` (Step 2).
  - No framework `Microsoft.*` Debug spam in AI or Console.
- `EXECUTIONLOGLEVEL=INFO`, `ENABLEAPPLICATIONINSIGHTS=true`:
  - AI rule: `LogLevel.Information` → Debug/Trace not sent to AI.

---

## Step 5 — Fix Structured Logging Parameters in `AzureExecutionLogger`

**File:** `Logging/AzureExecutionLogger.cs`  
**Defect fixed:** #6  
**Why last among code changes:** Non-blocking for the level-gate fixes; AI will work
after Steps 1–4. However, without this fix, Application Insights receives flat strings
— `ExecutionId`, `FunctionName`, and `Correlation` are **not** separate `customDimensions`
in the AI traces table, making structured queries impossible.

### Current pattern (all methods)
```csharp
_logger.LogTrace("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
//                              ↑ string interpolation — one flat string, no dimensions
```

### Correct pattern (matches `ConsoleExecutionLogger`)
```csharp
_logger.LogTrace("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
    Correlation, executionId, message);
//  ↑ three separate template parameters → three queryable customDimensions in AI
```

### What to do

Apply the structured parameter pattern to **every** log method in `AzureExecutionLogger`:

| Method | Template change |
|--------|----------------|
| `LogTrace` (×2) | `"{Correlation} [ExecutionId:{ExecutionId}] {Message}"` |
| `LogDebug` (×2) | `"{Correlation} [ExecutionId:{ExecutionId}] {Message}"` |
| `LogInfo` (×3) | `"{Correlation} [ExecutionId:{ExecutionId}] {Message}"` / `"{Correlation} {Message}"` |
| `LogWarning` (×2) | `"{Correlation} [ExecutionId:{ExecutionId}] {Message}"` |
| `LogError` (×3) | `"{Correlation} [ExecutionId:{ExecutionId}] {Message}"` |
| `LogFatal` (×2) | `"{Correlation} [ExecutionId:{ExecutionId}] {Message}"` |

For the `LogError(string activityName, Exception ex, Guid executionId)` overload use:
```csharp
_logger.LogError(ex,
    "{Correlation} [ExecutionId:{ExecutionId}] [{ActivityName}] {Message}",
    Correlation, executionId, activityName, ex?.Message);
```

### Verification
- In Azure Portal → Application Insights → Logs → `traces` table:
  ```kql
  traces
  | where customDimensions.ExecutionId == "<guid>"
  ```
  Should return rows — this query fails today because `ExecutionId` is embedded in
  the flat `message` string, not a dimension.

---

## Step 6 — Remove Redundant NuGet Package

**File:** `Warewolf.Execution.Lightweight.csproj`  
**Defect fixed:** #1 (package hygiene)  
**Covered by:** Step 3 already documents the removal. Listed separately as a reminder
for the PR checklist.

### What to do

In `Warewolf.Execution.Lightweight.csproj`, delete:
```xml
<PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.23.0" />
```

Run `dotnet restore` and confirm that `Microsoft.ApplicationInsights.WorkerService` is
still resolved as a **transitive** dependency (it will be — via
`Microsoft.Azure.Functions.Worker.ApplicationInsights`).

### Verification
```pwsh
dotnet list Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj package --include-transitive |
    Select-String "WorkerService"
```
Must appear under `Transitive Package`, not `Top-level Package`.

---

## Complete Change Checklist

```
[ ] Step 1 — LoggingConfiguration.cs      : Add MelMinimumLevel computed property
[ ] Step 2 — Program.cs line 19           : Replace hardcoded LogLevel.Debug with MelMinimumLevel
[ ] Step 3 — Program.cs lines 39-40       : Remove AddApplicationInsightsTelemetryWorkerService()
             Program.cs                   : Guard ConfigureFunctionsApplicationInsights() behind EnableApplicationInsights
             .csproj line 98              : Remove Microsoft.ApplicationInsights.WorkerService package reference
[ ] Step 4 — Program.cs lines 42-53       : Replace global MinLevel with targeted LoggerFilterRule for AI provider
[ ] Step 5 — AzureExecutionLogger.cs      : Fix all log methods — use separate template parameters (not interpolated string)
[ ] Step 6 — .csproj                      : dotnet restore + verify WorkerService is transitive only
```

---

## Environment Variable Reference (After Fix)

| Variable | Values | Effect after fix |
|----------|--------|-----------------|
| `EXECUTIONLOGLEVEL` | `TRACE`, `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL`, `OFF` | Controls the level gate for **all sinks** (Console, AI, Elastic) via `MelMinimumLevel` |
| `ENABLEAPPLICATIONINSIGHTS` | `true` / `false` | Enables AI provider + filter rule. If `false`, AI SDK is not registered at all |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Azure AI connection string | Required for AI telemetry to reach Azure Monitor |
| `ENABLEELASTICSEARCHLOGGING` | `true` / `false` | Enables Elasticsearch sink independently |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` | Controls `ElasticDebugMode` and `StructuredJson` defaults |

---

## Level Mapping Reference

| `EXECUTIONLOGLEVEL` | Dev2 `LogLevel` value | MEL `LogLevel` value | Visible in AI / Log Stream |
|---|---|---|---|
| `TRACE` | 6 | `Trace` (0) | Trace + Debug + Info + Warn + Error + Fatal |
| `DEBUG` | 5 | `Debug` (1) | Debug + Info + Warn + Error + Fatal |
| `INFO` *(default)* | 4 | `Information` (2) | Info + Warn + Error + Fatal |
| `WARN` | 3 | `Warning` (3) | Warn + Error + Fatal |
| `ERROR` | 2 | `Error` (4) | Error + Fatal |
| `FATAL` | 1 | `Critical` (5) | Fatal only |
| `OFF` | 0 | `None` (6) | Nothing |
