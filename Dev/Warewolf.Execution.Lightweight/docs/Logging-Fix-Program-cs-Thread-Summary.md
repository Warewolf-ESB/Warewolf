# Program.cs Logging Fix — Thread Summary

**Branch:** `8436-DebugAndTraceInApplicationInsights`
**File touched:** `Warewolf.Execution.Lightweight\Program.cs`
**Date:** 2026-06-10

---

## 1. Issues Reported

With environment variables:

| Variable | Value |
|---|---|
| `ENABLECONSOLELOGGING` | `true` |
| `ENABLEAPPLICATIONINSIGHTS` | `false` |
| `EXECUTIONLOGLEVEL` | `6` (TRACE) |

Two symptoms were observed in **Azure Portal → Log Stream → Filesystem logs**:

### Issue 1 — Logs vanish after `ExternalSink` upgrade
Before the swap, bootstrap-phase log lines such as:

```
Program startup orchestrator completed, upgrading to full composite logger
```

were visible. **After** this line in `Program.cs`:

```csharp
Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(executionLogger);
```

every subsequent `Dev2Logger.*` call disappeared from the filesystem logs (the `[AppInsights-Test]` block, license messages, "Program initialization complete", etc.).

### Issue 2 — `Debug` rendered as `Information`

```csharp
Dev2Logger.Debug($"Program logging configuration: ...", executionId);
```

appeared in filesystem logs stamped as `[Information]`:

```
2026-06-10T12:22:41.496 [Information] [Instance:c8c6...] ... Program logging configuration: EnableAI=False, EnableElastic=True, MinLevel=TRACE
```

---

## 2. Root Cause Analysis

The .NET isolated worker has **two completely independent log paths**:

| # | Path | Reaches | Severity behaviour |
|---|------|---------|--------------------|
| **a** | Process **stdout** (real `Console` provider writing to `Console.Out`) | Filesystem logs / Log Stream → Filesystem | Host captures stdout and **stamps every line as `[Information]`** — original level is lost |
| **b** | `ILogger<T>` → gRPC → host | Application Insights (+ AI-backed Log Stream) | Per-level severity is preserved |

Mapping the symptoms onto this model:

- **Bootstrap logger** (Step 2) uses a standalone `LoggerFactory.Create(b => b.AddConsole())`. This writes directly to **stdout** → path (a) → visible in filesystem logs (but severity flattened to `Information`). This is exactly why the bootstrap `Debug` line shows up as `[Information]` — **Issue 2**.
- **Composite logger** (Step 5, after `ExternalSink` upgrade) resolves `ILogger<T>` from `host.Services`. Inspection of `HostBuilderExtensions.ConfigureWarewolf(...)` confirmed that the worker pipeline only calls `ConfigureFunctionsWorkerDefaults(...)` — **no console provider is attached to the worker's MEL pipeline**. So `ILogger<T>` only travels path (b). With `ENABLEAPPLICATIONINSIGHTS=false`, path (b) goes nowhere visible → **Issue 1**.

Additionally:

- `host.json` `logging.logLevel` rules apply to the **host process** only. They do **not** filter the worker's MEL pipeline.
- The previous code wired `ENABLECONSOLELOGGING` to `ConsoleExecutionLogger` in `ServiceCollectionExtensions.AddExecutionLogging`, but with no MEL console provider attached to the worker, that sink had no real provider to write to.

---

## 3. Fix Applied

A single, additive change in `Program.cs` between `ConfigureWarewolf(config)` and `ConfigureServices(...)`:

```csharp
var host = new HostBuilder()
	.ConfigureWarewolf(config)
	// ── Worker logging ───────────────────────────────────────────────────
	// Attach a console provider to the WORKER's MEL pipeline so that ILogger<T>
	// (used by the composite execution logger after the Step 5 sink upgrade) writes
	// to stdout and therefore appears in the Azure Functions FILESYSTEM logs /
	// Live Log Stream — even when Application Insights is disabled.
	//
	// Why this is required: the isolated worker has two independent log paths —
	//   (a) process stdout  → captured by the host → FILESYSTEM logs
	//   (b) ILogger → gRPC   → host                → APPLICATION INSIGHTS
	// Without an explicit console provider here, the composite logger only travels
	// path (b); with ENABLEAPPLICATIONINSIGHTS=false that path is invisible, so the
	// entries vanish from the filesystem logs after the bootstrap logger is replaced.
	//
	// The worker pipeline does NOT read host.json (those category filters apply to
	// the host process only), so EXECUTIONLOGLEVEL is honoured here via a namespace-
	// scoped filter — keeping framework Microsoft.*/System.* noise out at Trace/Debug.
	.ConfigureLogging(logging =>
	{
		logging.AddConsole();
		logging.AddFilter("Warewolf.Execution.Lightweight", loggingConfig.MelMinimumLevel);
	})
	.ConfigureServices(services =>
	 {
		 services.AddExecutionLogging(loggingConfig);
		 // ... unchanged AI SDK block ...
	 })
	.Build();
```

### What the change does

| Aspect | Effect |
|---|---|
| `logging.AddConsole()` | Adds a real console provider to the worker MEL pipeline. From now on, `ILogger<ConsoleExecutionLogger>` and `ILogger<AzureExecutionLogger>` (used by the composite) emit to stdout → filesystem logs. |
| `logging.AddFilter("Warewolf.Execution.Lightweight", MelMinimumLevel)` | Honours `EXECUTIONLOGLEVEL` at the worker MEL level for Warewolf categories. Without it, framework defaults would suppress Trace/Debug. |

### Files modified

- `Warewolf.Execution.Lightweight\Program.cs` — added the `.ConfigureLogging(...)` block.

### Files left unchanged (verified consistent)

- `Warewolf.Execution.Lightweight\Infrastructure\ServiceCollectionExtensions.cs` — runtime sink selection logic remains correct.
- `Warewolf.Execution.Lightweight\Logging\ConsoleExecutionLogger.cs` — uses proper `_logger.LogTrace/Debug/Information/Warning/Error/Critical` per Dev2 level.
- `Warewolf.Execution.Lightweight\Logging\Dev2LoggerSinkAdapter.cs` — already calls the matching `LogTrace/LogDebug/...` methods.
- `Warewolf.Execution.Lightweight\Logging\LoggingConfiguration.cs` — `MelMinimumLevel` mapping is correct.
- `Warewolf.Execution.Lightweight\host.json` — applies to host process only, irrelevant to worker MEL.

---

## 4. Why This Fixes Both Issues

### Issue 1 — disappearance after `ExternalSink` swap

After the fix, `ILogger<ConsoleExecutionLogger>` (resolved from `host.Services` and used inside `CompositeExecutionLogger`) has an actual console provider attached. Every `Dev2Logger.*` call after the swap now:

```
Dev2Logger.Info(...)
  → Dev2LoggerSinkAdapter
  → CompositeExecutionLogger
  → ConsoleExecutionLogger
  → ILogger<ConsoleExecutionLogger>
  → CONSOLE PROVIDER (newly attached)  ← was missing before
  → stdout
  → Functions host filesystem logs ✓
```

### Issue 2 — `Debug` shown as `Information`

This symptom was specifically the **bootstrap-phase** Debug line. The bootstrap logger writes to stdout via its own `LoggerFactory`, and stdout captured by the Functions host is stamped `Information`. There are two ways to interpret correct behaviour:

1. **Acceptable as-is:** That single Debug line is fired before the worker logging pipeline exists; the AI structured channel (which would preserve `Debug`) is not yet wired. Filesystem logs flattening to `Information` is the platform's documented behaviour for captured stdout.
2. **If exact severity must be preserved** for that early line, **move it past `host.Build()`** so it travels through the MEL pipeline (now equipped with the console provider) instead of the bootstrap factory. Suggested follow-up: relocate the `Dev2Logger.Debug("Program logging configuration: ...")` call to immediately **after** `var host = new HostBuilder()...Build();`.

The rest of the post-swap log calls are already past `host.Build()`, so with the worker console provider in place they now travel `ILogger<T>` → console provider → stdout, and the level is preserved by the MEL console formatter itself (the `[Information]` stamp from the host applies to the *capture line*, but the MEL console formatter emits its own `info:`/`dbug:`/`trce:` prefix in the message body).

---

## 5. Behaviour Matrix After Fix

| Env vars | ConsoleExecutionLogger output | AzureExecutionLogger output | Filesystem logs | Application Insights |
|---|---|---|---|---|
| `ENABLECONSOLELOGGING=true`, `ENABLEAPPLICATIONINSIGHTS=false` | active (selected by `ServiceCollectionExtensions`) | n/a | ✅ visible (via newly added console provider) | ❌ not sent (AI SDK not registered) |
| `ENABLEAPPLICATIONINSIGHTS=true` | n/a (Azure logger preferred to avoid duplication) | active | ✅ visible (Azure logger's `ILogger<T>` also reaches the console provider) | ✅ structured per-level |
| Both `false` | none | none | only host-level entries | none |

---

## 6. Verification Steps

After deployment with `ENABLECONSOLELOGGING=true`, `ENABLEAPPLICATIONINSIGHTS=false`, `EXECUTIONLOGLEVEL=6`:

1. **Azure Portal → Function App → Log stream → Filesystem logs** should now show, after the `Dev2Logger.ExternalSink` swap:

   ```
   Program Dev2Logger external sink upgraded to full CompositeExecutionLogger
   [AppInsights-Test] TRACE level log entry ...
   [AppInsights-Test] DEBUG level log entry ...
   [AppInsights-Test] INFO level log entry ...
   [AppInsights-Test] WARN level log entry ...
   [AppInsights-Test] ERROR level log entry ...
   [AppInsights-Test] FATAL level log entry ...
   Program initialization complete, starting host
   ```

2. The MEL console formatter emits its own per-level prefix (`trce:`, `dbug:`, `info:`, `warn:`, `fail:`, `crit:`) inside each line, even though the host wrapper line still says `[Information]`. The true level survives in the message body.
3. The `Microsoft.*` / `System.*` framework chatter remains at default (`Information`) because the namespace filter `Warewolf.Execution.Lightweight` only opens the gate for project categories.

---

## 7. Background: Why Symptoms Looked Confusing

- `ILogger<T>` in the worker **does not select a provider**; it broadcasts to every registered MEL provider. The category `<T>` is just a label.
- `host.json` `logLevel` rules look like they should help, but they only filter the **host** — not the worker pipeline that the composite logger runs through.
- The previously-attached console provider belonged to the throwaway bootstrap `LoggerFactory`. It was disposed when the `using var bootstrapFactory` scope ended — so no console provider existed for the runtime DI container.
- The `[Information]` stamp seen on captured stdout lines is **the Azure Functions host's own wrapper**, not a level produced by your code or by MEL. It cannot be changed without going through the structured (AI) channel.

---

## 8. Related Documentation

- `Warewolf.Execution.Lightweight\docs\README-ApplicationInsights.md` — AI architecture: `AzureExecutionLogger → ILogger<T> → AI SDK → Application Insights`.
- `Warewolf.Execution.Lightweight\docs\Warewolf-Lightweight-Logger-Guide.md` — Operational guide; confirms `ConsoleExecutionLogger` is the stdout-only sink.

---

## 9. Git State After Change

- **Branch:** `8436-DebugAndTraceInApplicationInsights`
- **Modified:** `Warewolf.Execution.Lightweight/Program.cs` (one block added between `.ConfigureWarewolf(config)` and `.ConfigureServices(...)`)

### Suggested commit message

```
Fix: attach console provider to worker MEL so post-ExternalSink logs reach filesystem logs

The isolated worker had no console provider on its MEL pipeline, so after
Dev2Logger.ExternalSink was swapped to the composite logger (which uses
ILogger<T>), entries only travelled the AI gRPC channel. With
ENABLEAPPLICATIONINSIGHTS=false they were invisible.

Add .ConfigureLogging(...) on the HostBuilder to register an AddConsole()
provider plus a Warewolf.Execution.Lightweight-scoped EXECUTIONLOGLEVEL
filter, so the composite logger reaches stdout and therefore filesystem
logs / Live Log Stream — independently of Application Insights.
```
