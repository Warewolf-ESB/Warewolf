# Logging Refactor Implementation Plan
## Warewolf.Execution.Lightweight — Enterprise Azure Functions Logging

**Branch:** `8410-AddLoggingOfExecution`  
**Target:** .NET 8 / Azure Functions v4 Isolated Worker  
**Date:** 2025  

---

## Goals

1. `Dev2Logger` is the **single call-site** for all logging everywhere in the project.
2. `Dev2Logger.ExternalSink` routes to `CompositeExecutionLogger` from the **very first line** of startup — no log is lost.
3. `CompositeExecutionLogger` always has at least one working sink (console / structured stdout).
4. Application Insights, Elasticsearch are **opt-in** additive sinks.
5. Security/audit events have a dedicated, always-on audit sink inside `CompositeExecutionLogger`.
6. No `Console.WriteLine` in logger code — all fallback errors use `Dev2Logger`.
7. `InstanceCorrelationMiddleware`, `StartupOrchestrator`, `AuditLogger` no longer use raw `ILogger<T>` directly for operational logging.
8. `ElasticsearchExecutionLogger` cleaned up (remove `EnableDebugMode` in production, fix silent failure).

---

## Decision Points (Answer Before Starting)

| # | Question | Recommended Default |
|---|---|---|
| D1 | Should `ConsoleExecutionLogger` write structured JSON lines (ECS format) or plain text? | **Plain text** for local dev, JSON in Azure (read from env var `STRUCTURED_LOGS=true`) |
| D2 | Should `AuditLogger` write through `Dev2Logger` (and thus `CompositeExecutionLogger`) or remain a separate `ILogger<T>` MEL sink? | Route **security audit events** through a dedicated `AuditExecutionLogger` added to `CompositeExecutionLogger` |
| D3 | Should `InstanceCorrelationMiddleware.LogInformation(...)` per-invocation entry be kept? It creates one log line per HTTP request. | **Keep it** but route through `Dev2Logger.Info` instead of raw MEL |
| D4 | `ElasticsearchExecutionLogger.EnableDebugMode()` exposes raw HTTP request/response — keep in production? | **Remove** from production; enable only when `IsDevelopment=true` |
| D5 | `ServiceCollectionExtensions.AddCoreServices` registers `AzureExecutionLogger` directly as default `IExecutionLogger`. This is overridden by `Program.cs`. Should it be removed from `AddCoreServices`? | **Yes, remove** the direct registration. `Program.cs` owns sink composition. |
| D6 | Should `ConsoleExecutionLogger` use `ILogger<T>` (MEL console sink) or `Console.WriteLine` directly? | **`ILogger<T>` via MEL** — the Azure Functions runtime captures MEL console output automatically into Application Insights `traces` table. |

---

## Current Architecture Problems

### Problem 1 — ExternalSink Set Too Late
```
Program.cs line ~85:
    await StartupOrchestrator.RunStartupAsync(host, config);   // ← logs here bypass CompositeExecutionLogger
    Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(...);   // ← set AFTER startup
```
All `Dev2Logger` calls in `StartupOrchestrator`, `ServiceCollectionExtensions`, `KeyVaultStartupExtensions` go **only to log4net** — never to Application Insights or Elasticsearch.

### Problem 2 — Duplicate IExecutionLogger Registration
```
ServiceCollectionExtensions.AddCoreServices:
    services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();   // ← always registered

Program.cs ConfigureServices:
    services.AddSingleton<IExecutionLogger>(sp => new CompositeExecutionLogger(...));  // ← overrides above
```
The `AzureExecutionLogger` in `AddCoreServices` is a dead registration. `WorkflowExecutor` receives `CompositeExecutionLogger` from DI because `Program.cs` registers last, but this is fragile and confusing.

### Problem 3 — Raw ILogger<T> Usage Bypasses Dev2Logger
These classes log directly to MEL, bypassing `CompositeExecutionLogger`:
- `InstanceCorrelationMiddleware` — `logger.LogInformation(...)`
- `StartupOrchestrator` — `logger.LogWarning(...)`, `logger.LogCritical(...)`
- `AuditLogger` — `_logger.LogInformation(...)`, `_logger.LogError(...)`

### Problem 4 — Console.WriteLine in Logger Code
- `CompositeExecutionLogger.LogInfo(string, Exception, Guid)` — stray debug line
- `ElasticsearchExecutionLogger.IndexFireAndForget` — swallowed failures written to Console

### Problem 5 — ElasticsearchExecutionLogger.EnableDebugMode()
`settings.EnableDebugMode()` captures full HTTP request/response bodies in memory on every index call. In production this means every log entry creates a large `DebugInformation` string allocation — never collected unless `IsValid` is checked. This is a memory and performance issue.

### Problem 6 — No Always-Available Sink
If `ENABLECONSOLELOGGING=false` and `ENABLEELASTICSEARCHLOGGING=false`, `CompositeExecutionLogger` is built with **zero loggers**. All `_executionLogger` calls in `WorkflowExecutor` are silently dropped.

---

## Target Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  Call-site (any class)                                          │
│    Dev2Logger.Info(...) / Dev2Logger.Error(...) / etc.          │
└────────────────────────────┬────────────────────────────────────┘
                             │ ExternalSink (Dev2LoggerSinkAdapter)
                             ▼  [set at line 1 of Program.cs]
┌─────────────────────────────────────────────────────────────────┐
│  CompositeExecutionLogger                                       │
│  (fan-out to all registered sinks in order)                     │
│                                                                 │
│  ┌──────────────────────┐  always present                       │
│  │ ConsoleExecutionLogger│ ← MEL ILogger<T> console provider    │
│  └──────────────────────┘   (captured by AF runtime → AI traces)│
│                                                                 │
│  ┌──────────────────────┐  opt-in: ENABLEAPPLICATIONINSIGHTS    │
│  │ AzureExecutionLogger  │ ← ILogger<T> + AI SDK                │
│  └──────────────────────┘                                       │
│                                                                 │
│  ┌──────────────────────────┐  opt-in: ENABLEELASTICSEARCHLOGGING│
│  │ ElasticsearchExecution-  │ ← fire-and-forget Elastic index   │
│  │ Logger                   │                                   │
│  └──────────────────────────┘                                   │
│                                                                 │
│  ┌──────────────────────┐  always present (security events only)│
│  │ AuditExecutionLogger  │ ← writes to dedicated audit table /  │
│  └──────────────────────┘   separate AI custom event            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Implementation Steps

---

### Step 1 — Create `ConsoleExecutionLogger`

**File:** `Warewolf.Execution.Lightweight\Logging\ConsoleExecutionLogger.cs` *(new)*

**Why:** This is the always-available sink. Azure Functions isolated worker captures `ILogger<T>` console output and forwards it to Application Insights `traces` table automatically — even without the Application Insights SDK. This ensures no log is ever dropped.

**Implementation:**
- Extends `ExecutionLoggerBase` (inherits `ShouldLog` + correlation).
- Wraps `ILogger<ConsoleExecutionLogger>` (MEL).
- Registered unconditionally in `Program.cs` — it is always the first logger in the list.
- Level is still gated by `minimumLevel` (reads from `ExecutionLogLevel` env var).

```csharp
// ConsoleExecutionLogger.cs
public sealed class ConsoleExecutionLogger : ExecutionLoggerBase
{
    readonly ILogger<ConsoleExecutionLogger> _logger;

    public ConsoleExecutionLogger(ILogger<ConsoleExecutionLogger> logger,
                                  Dev2LogLevel minimumLevel = ExecutionLogLevel.Default)
        : base(minimumLevel)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    string Correlation => GetCorrelationPrefix();

    public override void LogDebug(string message, Guid executionId)
    {
        if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
        _logger.LogDebug("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
            Correlation, executionId, message);
    }
    // ... all other IExecutionLogger methods following same pattern as AzureExecutionLogger
}
```

**Decision Point D6:** Uses `ILogger<T>` MEL, not `Console.WriteLine` directly.

---

### Step 2 — Create `AuditExecutionLogger`

**File:** `Warewolf.Execution.Lightweight\Logging\AuditExecutionLogger.cs` *(new)*

**Why:** Security and middleware audit events (cold start, key vault access, JWT validation, permission checks) must:
- Always be written — not filtered by `ExecutionLogLevel`.
- Go to a **separate, tamper-evident** destination distinct from operational logs.
- Be structured so they can be queried independently (e.g., `EventId`-based in Application Insights, separate index in Elasticsearch).

**Best Practice:** Azure Functions — use Application Insights **custom events** (`TelemetryClient.TrackEvent`) for audit entries, not `traces`. This allows separate retention policies and query isolation.

**Implementation:**
- Implements `IExecutionLogger` but **ignores `ShouldLog`** — audit entries are always written.
- Writes all calls through MEL using a dedicated **EventId** so they appear as a distinct category in Application Insights.
- `AuditLogger` (existing security class) is refactored to call `Dev2Logger` → routes here.
- Also added to `CompositeExecutionLogger` unconditionally.

```csharp
// AuditExecutionLogger.cs
public sealed class AuditExecutionLogger : IExecutionLogger
{
    // Audit events are always written — no ShouldLog gate.
    // Uses a fixed EventId so Application Insights can filter: customEvents | where name == "AuditLog"
    static readonly EventId AuditEvent = new(9000, "AuditLog");

    readonly ILogger<AuditExecutionLogger> _logger;

    public AuditExecutionLogger(ILogger<AuditExecutionLogger> logger)
        => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void LogError(string activityName, Exception ex, Guid executionId)
        => _logger.LogError(AuditEvent, ex,
            "[AUDIT] [{ActivityName}] [ExecutionId:{ExecutionId}] {Message}",
            activityName, executionId, ex?.Message);

    // ... only ERROR and FATAL are written for audit; DEBUG/INFO/WARN are no-ops
    // because the audit logger is for security events, not operational telemetry.
    // Decision Point D2.
}
```

**Refactor `AuditLogger`:**
- Remove `ILogger<AuditLogger>` injection.
- Replace `_logger.LogInformation(...)` → `Dev2Logger.Info(...)`.
- Replace `_logger.LogError(...)` → `Dev2Logger.Error(...)`.
- `AuditLogger` becomes a pure formatting helper (builds the structured message string and calls `Dev2Logger`).

---

### Step 3 — Fix `ElasticsearchExecutionLogger`

**File:** `Warewolf.Execution.Lightweight\Logging\ElasticsearchExecutionLogger.cs` *(modify)*

**Changes:**

**3a — Remove `EnableDebugMode()` unconditionally**
```csharp
// BEFORE (production memory issue):
settings = settings.EnableDebugMode();

// AFTER:
// EnableDebugMode only in development — reads from options or env var
if (options.EnableDebugMode)
    settings = settings.EnableDebugMode();
```
Add `bool EnableDebugMode` to `ElasticsearchLoggingOptions`. Default = `false`. Set to `true` only locally.

**3b — Replace `Console.WriteLine` fallback in `IndexFireAndForget`**
```csharp
// BEFORE:
Console.WriteLine($"[ElasticsearchLogger] Exception indexing document: ...");

// AFTER — use Dev2Logger so it routes through the chain
// NOTE: must guard against re-entrancy (Elasticsearch failure must not call back into itself)
Dev2Logger.Warn($"[ElasticsearchLogger] Failed to index document: {e.GetType().Name} — {e.Message}",
    "ElasticsearchLogger-FireAndForget");
```
**Re-entrancy guard:** Wrap the `Dev2Logger.Warn` call in a flag check so a failing Elasticsearch logger does not recursively call itself through `Dev2Logger.ExternalSink → CompositeExecutionLogger → ElasticsearchExecutionLogger`.

**3c — Remove `Console.WriteLine` when `_client is null`**
```csharp
// BEFORE:
Console.WriteLine("[ElasticsearchLogger] Client is null — skipping index.");

// AFTER:
Dev2Logger.Warn("[ElasticsearchLogger] Client is null — skipping index.", "ElasticsearchLogger");
```
Apply same re-entrancy guard.

**3d — `LogError(Exception, string)` implementation fix**
```csharp
// BEFORE — creates a new Guid() (empty) losing correlation:
this.LogError("", exception, new Guid());

// AFTER:
IndexFireAndForget(new ElasticsearchLogDocument
{
    Level        = "error",
    Message      = log,
    ErrorMessage = ex?.Message,
    StackTrace   = ex?.ToString(),
});
```

---

### Step 4 — Fix `AzureExecutionLogger`

**File:** `Warewolf.Execution.Lightweight\Logging\AzureExecutionLogger.cs` *(modify)*

**Changes:**

**4a — `LogError(Exception, string)` — consistent with other overloads**
Current implementation is correct. No change needed.

**4b — Add `ENABLEAPPLICATIONINSIGHTS` env var gate in `Program.cs`**  
`AzureExecutionLogger` should only be added to `CompositeExecutionLogger` when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set **or** `ENABLEAPPLICATIONINSIGHTS=true` — not when `ENABLECONSOLELOGGING=true` (that is the `ConsoleExecutionLogger`'s job).

> **Current issue:** `ENABLECONSOLELOGGING=true` adds `AzureExecutionLogger` — but `AzureExecutionLogger` wraps `ILogger<T>` (MEL), which already includes the console sink. This means with `ENABLECONSOLELOGGING=true` you get console output **plus** Application Insights if the connection string is set. The env var name is misleading.

**Recommended rename in `Program.cs`:**
```
ENABLECONSOLELOGGING     → ENABLEAPPLICATIONINSIGHTS
```
`ConsoleExecutionLogger` is always added (no env var gate). `AzureExecutionLogger` is added only when `ENABLEAPPLICATIONINSIGHTS=true`.

---

### Step 5 — Fix `CompositeExecutionLogger`

**File:** `Warewolf.Execution.Lightweight\Logging\CompositeExecutionLogger.cs` *(modify)*

**5a — Remove stray `Console.WriteLine`**
```csharp
// BEFORE:
public void LogInfo(string message, Exception exception, Guid executionId)
{
    foreach (var logger in _loggers)
    {
        Console.WriteLine($"Logging info: {message} with logger: {logger.ToString()}");  // ← REMOVE
        logger.LogInfo(message, exception, executionId);
    }
}

// AFTER:
public void LogInfo(string message, Exception exception, Guid executionId)
{
    foreach (var logger in _loggers)
        logger.LogInfo(message, exception, executionId);
}
```

**5b — Add per-logger exception isolation**
Currently if the first logger throws, subsequent loggers are skipped. Add a try/catch per logger:
```csharp
foreach (var logger in _loggers)
{
    try   { logger.LogInfo(message, executionId); }
    catch { /* individual sink failure must not cascade */ }
}
```

**5c — Update XML doc comment** to reflect `ConsoleExecutionLogger` as the always-present default.

---

### Step 6 — Fix `Program.cs` — Wire `ExternalSink` First

**File:** `Warewolf.Execution.Lightweight\Program.cs` *(modify)*

**This is the core fix.** The `Dev2Logger.ExternalSink` must be set **before** any other startup code so all `Dev2Logger` calls during startup are captured.

**Current order (broken):**
```
1. HostEnvironmentConfig.Load()
2. Build loggers list
3. new HostBuilder().Build()
4. StartupOrchestrator.RunStartupAsync()   ← Dev2Logger calls here go only to log4net
5. Dev2Logger.ExternalSink = ...           ← too late
6. host.RunAsync()
```

**New order (fixed):**
```
1. HostEnvironmentConfig.Load()
2. Build minimumLevel + logger options
3. Build a "bootstrap" CompositeExecutionLogger with just ConsoleExecutionLogger
   (ILogger factory not yet available, so use a lightweight console writer)
4. Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(bootstrapLogger)   ← FIRST
5. Dev2Logger.CorrelationPrefixProvider = ...                             ← FIRST
6. new HostBuilder().Build()
7. Upgrade ExternalSink to full CompositeExecutionLogger (with AI + Elastic)
8. StartupOrchestrator.RunStartupAsync()   ← Dev2Logger calls now reach all sinks
9. host.RunAsync()
```

**Bootstrap logger challenge:** `AzureExecutionLogger` requires `ILogger<T>` which needs the DI container. For bootstrap (pre-host), use `ConsoleExecutionLogger` with a `LoggerFactory.Create(b => b.AddConsole())` factory:

```csharp
// Step 4 — bootstrap sink (pre-host, before DI)
using var bootstrapFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));
var bootstrapLogger = new ConsoleExecutionLogger(
    bootstrapFactory.CreateLogger<ConsoleExecutionLogger>(), minimumLevel);
Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(bootstrapLogger);
Dev2Logger.CorrelationPrefixProvider = ExecutionLoggerBase.GetCorrelationPrefixStatic;

// ... build host ...

// Step 7 — upgrade to full composite after host is built
Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(
    host.Services.GetRequiredService<IExecutionLogger>());
```

---

### Step 7 — Fix `ServiceCollectionExtensions.AddCoreServices`

**File:** `Warewolf.Execution.Lightweight\Infrastructure\ServiceCollectionExtensions.cs` *(modify)*

Remove the direct `IExecutionLogger` registration — `Program.cs` owns this:
```csharp
// REMOVE this line:
services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();
```

Register `ConsoleExecutionLogger` for DI resolution (needed by the full `CompositeExecutionLogger`):
```csharp
services.AddSingleton<ConsoleExecutionLogger>(sp =>
    new ConsoleExecutionLogger(
        sp.GetRequiredService<ILogger<ConsoleExecutionLogger>>(),
        minimumLevel));
```

Register `AuditExecutionLogger` unconditionally:
```csharp
services.AddSingleton<AuditExecutionLogger>(sp =>
    new AuditExecutionLogger(
        sp.GetRequiredService<ILogger<AuditExecutionLogger>>()));
```

---

### Step 8 — Fix `Program.cs` — Restructure Logger Composition

**File:** `Warewolf.Execution.Lightweight\Program.cs` *(modify)*

Full composition after host is built:
```csharp
services.AddSingleton<IExecutionLogger>(sp =>
{
    var loggers = new List<IExecutionLogger>();

    // 1. ConsoleExecutionLogger — ALWAYS present
    loggers.Add(sp.GetRequiredService<ConsoleExecutionLogger>());

    // 2. AzureExecutionLogger — opt-in via ENABLEAPPLICATIONINSIGHTS=true
    if (IsEnabled("ENABLEAPPLICATIONINSIGHTS"))
    {
        loggers.Add(new AzureExecutionLogger(
            sp.GetRequiredService<ILogger<AzureExecutionLogger>>(),
            minimumLevel));
        Dev2Logger.Debug("Program added AzureExecutionLogger to pipeline", executionId);
    }

    // 3. ElasticsearchExecutionLogger — opt-in via ENABLEELASTICSEARCHLOGGING=true
    if (elasticOptions is not null)
    {
        loggers.Add(new ElasticsearchExecutionLogger(elasticOptions, minimumLevel));
        Dev2Logger.Debug("Program added ElasticsearchExecutionLogger to pipeline", executionId);
    }

    // 4. AuditExecutionLogger — ALWAYS present (security/audit sink)
    loggers.Add(sp.GetRequiredService<AuditExecutionLogger>());

    return new CompositeExecutionLogger(loggers);
});
```

---

### Step 9 — Fix `InstanceCorrelationMiddleware`

**File:** `Warewolf.Execution.Lightweight\Infrastructure\InstanceCorrelationMiddleware.cs` *(modify)*

Remove `ILogger<InstanceCorrelationMiddleware>` injection and raw `LogInformation` call.  
Replace with `Dev2Logger.Info`:

```csharp
// BEFORE:
var logger = context.GetLogger<InstanceCorrelationMiddleware>();
using var scope = logger.BeginScope(...);
logger.LogInformation("InstanceCorrelationMiddleware invoked for ...", ...);

// AFTER:
// MEL scope for AzureExecutionLogger correlation — keep the BeginScope but
// remove the direct LogInformation.
using var scope = context.GetLogger<InstanceCorrelationMiddleware>()
    .BeginScope(new Dictionary<string, object> { ... });

Dev2Logger.Info(
    $"InstanceCorrelationMiddleware invoked for '{functionName}' (InvocationId: {invocationId})",
    invocationId);
```

> **Note:** `BeginScope` on the MEL logger must be **kept** — it enriches Application Insights structured properties (`InstanceId`, `InvocationId`, `Function`, `TraceId`) for the `AzureExecutionLogger` MEL sink. Only the `LogInformation` line is replaced.

---

### Step 10 — Fix `StartupOrchestrator`

**File:** `Warewolf.Execution.Lightweight\Infrastructure\StartupOrchestrator.cs` *(modify)*

Remove `ILogger logger` parameter from `LogEnvironmentDiagnostics`, `InitializeEncryptionAsync`, and `WarmUpWorkflowIndex`. Replace all `logger.LogWarning(...)` / `logger.LogCritical(...)` calls with `Dev2Logger.Warn(...)` / `Dev2Logger.Fatal(...)`.

```csharp
// BEFORE:
logger.LogWarning("Startup | Phase=Diagnostics | ...", ...);

// AFTER:
Dev2Logger.Warn($"Startup | Phase=Diagnostics | EncryptionEnabled={config.EncryptionEnabled} | ...",
    executionId);
```

The `ILoggerFactory` / `ILogger` resolved from `host.Services` is removed from `RunStartupAsync`.

---

### Step 11 — Refactor `AuditLogger`

**File:** `Warewolf.Execution.Lightweight\Security\AuditLogger.cs` *(modify)*

Remove `ILogger<AuditLogger>` constructor parameter.  
Replace all `_logger.Log*(...)` calls with `Dev2Logger.*`:

```csharp
// BEFORE:
public AuditLogger(ILogger<AuditLogger> logger) => _logger = ...;
public void LogColdStart(string instanceId, string keyId)
    => _logger.LogInformation(GetColdStartLog(instanceId, keyId));

// AFTER:
public AuditLogger() { }  // no logger dependency
public void LogColdStart(string instanceId, string keyId)
    => Dev2Logger.Info(GetColdStartLog(instanceId, keyId), instanceId);
```

Update `ServiceCollectionExtensions.AddKeyVaultEncryption`:
```csharp
// BEFORE:
services.AddSingleton(sp =>
    new AuditLogger(sp.GetRequiredService<ILogger<AuditLogger>>()));

// AFTER:
services.AddSingleton(new AuditLogger());
```

---

### Step 12 — Build & Validate

1. Build the solution: `dotnet build`.
2. Run existing unit tests.
3. Verify no `Console.WriteLine` remains in logger classes (search across project).
4. Verify no direct `ILogger<T>` usage remains in `StartupOrchestrator`, `InstanceCorrelationMiddleware`, `AuditLogger` for operational logging.
5. Verify `Dev2Logger.ExternalSink` is set before `StartupOrchestrator.RunStartupAsync` is called.

---

## Files Changed Summary

| File | Change Type | Reason |
|---|---|---|
| `Logging\ConsoleExecutionLogger.cs` | **New** | Always-available sink |
| `Logging\AuditExecutionLogger.cs` | **New** | Dedicated audit/security sink |
| `Logging\CompositeExecutionLogger.cs` | Modify | Remove `Console.WriteLine`; add per-logger isolation; update doc |
| `Logging\ElasticsearchExecutionLogger.cs` | Modify | Remove `EnableDebugMode`; replace `Console.WriteLine`; fix `LogError(Exception, string)` |
| `Logging\AzureExecutionLogger.cs` | Minor modify | Env var rename doc only |
| `Infrastructure\ServiceCollectionExtensions.cs` | Modify | Remove duplicate `IExecutionLogger` registration; register `ConsoleExecutionLogger`, `AuditExecutionLogger` |
| `Infrastructure\StartupOrchestrator.cs` | Modify | Remove raw `ILogger<T>` calls; use `Dev2Logger` exclusively |
| `Infrastructure\InstanceCorrelationMiddleware.cs` | Modify | Replace `LogInformation` with `Dev2Logger.Info`; keep `BeginScope` |
| `Security\AuditLogger.cs` | Modify | Remove `ILogger<T>` dependency; use `Dev2Logger` |
| `Program.cs` | Modify | Wire `ExternalSink` first; restructure composite; rename env var |

---

## What Does NOT Change

| Item | Reason |
|---|---|
| `Dev2Logger` (Dev2.Diagnostics) | No changes needed — `ExternalSink` hook already exists |
| `Dev2LoggerSinkAdapter` | No changes needed — bridge is correct |
| `ExecutionLoggerBase` | No changes needed — base class is clean |
| `ExecutionLogLevel` | No changes needed |
| `ElasticsearchLogDocument` | No changes needed — ECS format is correct |
| `ElasticsearchLoggingOptions` | Add `EnableDebugMode` bool property only |
| `WorkflowExecutor` | No changes — already uses `Dev2Logger` + `_executionLogger` correctly |

---

## Open Questions Before Implementation

1. **`ENABLECONSOLELOGGING` rename** — Do you want to rename the env var to `ENABLEAPPLICATIONINSIGHTS` to accurately reflect what it controls? This is a breaking change for any existing deployment scripts that set `ENABLECONSOLELOGGING=true`.

2. **`AuditExecutionLogger` destination** — Should audit events go to:
   - (a) MEL `ILogger<T>` with a fixed `EventId` (simplest — same App Insights workspace, but filtered by `EventId=9000`)
   - (b) A separate Application Insights custom event via `TelemetryClient.TrackEvent` (requires `Microsoft.ApplicationInsights` package)
   - (c) Azure Table Storage (separate tamper-evident store)

   Option (a) is recommended for initial implementation; (b) or (c) can be layered on top.

3. **Re-entrancy guard in `ElasticsearchExecutionLogger`** — A `[ThreadStatic]` or `AsyncLocal<bool>` guard is needed to prevent `Dev2Logger.Warn(...)` inside `IndexFireAndForget` from recursively calling back into `ElasticsearchExecutionLogger`. Should this be a shared utility in `ExecutionLoggerBase` or local to `ElasticsearchExecutionLogger`?

4. **`StartupOrchestrator` `logger.LogCritical` for Key Vault failure** — This is currently the only `LogCritical` call. `Dev2Logger` only has `Fatal` (maps to `LogCritical` via `Dev2LoggerSinkAdapter`). Confirm `Dev2Logger.Fatal(...)` is the correct replacement.

5. **Bootstrap `LoggerFactory`** — The bootstrap `ConsoleExecutionLogger` (Step 6) creates a short-lived `LoggerFactory`. This factory is disposed after the host is built and `ExternalSink` is upgraded. Confirm this disposal timing is acceptable.
