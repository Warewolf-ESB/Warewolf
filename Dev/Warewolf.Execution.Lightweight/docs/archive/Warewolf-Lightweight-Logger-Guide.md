# Warewolf.Execution.Lightweight — Logger Guide

Combined reference covering:

- **Part I — Logger Enable / Disable Guide** — how to turn each logger on or off per environment.
- **Part II — Logger Verification Guide** — how to confirm each logger is actually emitting.

Scope: Azure Cloud (highest priority), Docker (developer convenience), Local F5 (least priority).

Grounded in the project source at `Warewolf.Execution.Lightweight.csproj` (Program.cs, Infrastructure/, Logging/, Security/, host.json, local.settings*.json, engine/docker/, Settings/ElasticsearchLoggingSource.bite).

---

## Table of Contents

- [Part I — Logger Enable / Disable](#part-i--logger-enable--disable)
  - [I.1 Architecture overview](#i1-architecture-overview)
  - [I.2 Environment variables (all environments)](#i2-environment-variables-all-environments)
  - [I.3 Azure Cloud deployment](#i3-azure-cloud-deployment-highest-priority)
  - [I.4 Docker](#i4-docker-developers-only)
  - [I.5 Local development (F5)](#i5-local-development-f5)
  - [I.6 Quick reference cheat sheet](#i6-quick-reference-cheat-sheet)
- [Part II — Logger Verification](#part-ii--logger-verification)
  - [II.0 Fingerprints — what each logger looks like](#ii0-fingerprints--what-each-logger-looks-like)
  - [II.1 Azure Cloud verification (highest priority)](#ii1-azure-cloud-verification-highest-priority)
  - [II.2 Docker verification](#ii2-docker-verification)
  - [II.3 Local (F5) verification — least priority](#ii3-local-f5-verification--least-priority)
  - [II.4 Troubleshooting matrix](#ii4-troubleshooting-matrix)
  - [II.5 One-page cheat sheet](#ii5-one-page-cheat-sheet)

---

# Part I — Logger Enable / Disable

## I.1 Architecture overview

The execution engine emits log entries through three layers, with the legacy `Dev2Logger` bridging into a composite that fans out to the active sinks:

```
Dev2Logger  (legacy, used everywhere in Dev2.*)
  └── Dev2LoggerSinkAdapter   (Logging/Dev2LoggerSinkAdapter.cs)
        └── CompositeExecutionLogger   (Logging/CompositeExecutionLogger.cs)
              ├── AzureExecutionLogger        →  MEL (ILogger) → Console / App Insights
              └── ElasticsearchExecutionLogger →  direct HTTP → Elasticsearch index

Side channel — Azure Functions host MEL pipeline (never goes through Composite):
  ├── AuditLogger                 (Security/AuditLogger.cs)            — security events
  ├── InstanceCorrelationMiddleware                                    — request start/end
  └── StartupOrchestrator                                              — cold-start diagnostics
```

Two distinct things, often confused:

- **Execution loggers** — the composite sinks (Azure console + Elasticsearch). Controlled by env vars (`ENABLECONSOLELOGGING`, `ENABLEELASTICSEARCHLOGGING`, `EXECUTIONLOGLEVEL`).
- **Infrastructure loggers** — Audit, middleware and startup loggers that write via MEL only. Controlled by `host.json` category filters (and Application Insights settings when in Azure).

### I.1.1 Where the toggles are read

All three env vars are read in `Program.cs` at startup and passed into the DI registration:

```csharp
// Program.cs
static bool IsEnabled(string key) =>
    string.Equals(Environment.GetEnvironmentVariable(key), "true",
                  StringComparison.OrdinalIgnoreCase);

var enableConsole = IsEnabled("ENABLECONSOLELOGGING");
var enableElastic = IsEnabled("ENABLEELASTICSEARCHLOGGING");
var minimumLevel  = ExecutionLogLevel.Read();   // EXECUTIONLOGLEVEL

services.AddExecutionLogging(enableConsole, enableElastic,
                             elasticsearchSettingsPath, minimumLevel);
```

Inside `Infrastructure/ServiceCollectionExtensions.AddExecutionLogging`, each sink is added only if its flag is true. For Elasticsearch there is an additional requirement: the file `Settings/ElasticsearchLoggingSource.bite` must exist (the path is checked with `File.Exists` before the sink is created).

### I.1.2 Two-gate filtering model

Every log entry is filtered twice:

```
[Gate 1 — EXECUTIONLOGLEVEL]              [Gate 2 — host.json logLevel]
 ExecutionLoggerBase.ShouldLog()           MEL category filter
        │                                          │
        └────── Entry must pass BOTH to be emitted ┘
```

- **Gate 1** — `EXECUTIONLOGLEVEL` is applied inside `ExecutionLoggerBase.ShouldLog()`. It gates BOTH execution sinks (Azure + Elasticsearch).
- **Gate 2** — `host.json` `logging.logLevel` filters happen inside MEL. They only affect things that go through MEL (Azure execution logger, AuditLogger, middleware). Elasticsearch bypasses MEL, so `host.json` does NOT throttle Elasticsearch.

---

## I.2 Environment variables (all environments)

These are the only three knobs that control which sinks are active and at what verbosity. They behave identically in Azure, Docker, and local F5 — only the place you set them changes.

| Variable | Values | Default | Effect |
|---|---|---|---|
| `ENABLECONSOLELOGGING` | `true` / `false` | unset → false | Enables `AzureExecutionLogger` (MEL → console / Application Insights). |
| `ENABLEELASTICSEARCHLOGGING` | `true` / `false` | unset → false | Enables `ElasticsearchExecutionLogger`. ALSO requires `Settings/ElasticsearchLoggingSource.bite` to exist. |
| `EXECUTIONLOGLEVEL` | `0`–`6` or name | `4` (INFO) | Minimum level for BOTH execution sinks (Gate 1). |

### I.2.1 `EXECUTIONLOGLEVEL` values

Implemented in `Logging/ExecutionLogLevel.cs`. Accepts the numeric or the case-insensitive name; falls back to INFO when missing or unrecognised.

| Numeric | Name | Meaning |
|---|---|---|
| `0` | `OFF` | No execution logging emitted. |
| `1` | `FATAL` | Fatal only. |
| `2` | `ERROR` | Error and above. |
| `3` | `WARN` | Warning and above. |
| `4` | `INFO` | Info and above (**DEFAULT**). |
| `5` | `DEBUG` | Debug and above. |
| `6` | `TRACE` | Everything. |

### I.2.2 Quick enable / disable matrix

| Scenario | `ENABLECONSOLELOGGING` | `ENABLEELASTICSEARCHLOGGING` | `EXECUTIONLOGLEVEL` |
|---|---|---|---|
| All logging off | `false` | `false` | (any) |
| Console only (dev) | `true` | `false` | `5` (DEBUG) |
| Elasticsearch only (prod) | `false` | `true` | `4` (INFO) |
| Both sinks active | `true` | `true` | `4` (INFO) |
| Verbose troubleshooting | `true` | `true` | `5` (DEBUG) |

---

## I.3 Azure Cloud deployment (highest priority)

In Azure, the three env vars are set as Function App application settings. They are read on cold start. Changing any of them restarts the worker.

### I.3.1 Console — Azure Live Log Stream

When `ENABLECONSOLELOGGING=true`, `AzureExecutionLogger` writes to `ILogger<AzureExecutionLogger>`. From there entries flow into the Azure Functions worker console (visible via Live Log Stream / Kudu). They also flow into Application Insights, but only when `ENABLEAPPLICATIONINSIGHTS=true` (the single authoritative AI switch) and the connection string is supplied via `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`. The connection string is deliberately NOT deployed under the standard `APPLICATIONINSIGHTS_CONNECTION_STRING` name — that name would auto-enable the Functions host's own AI pipeline, which forwards worker stdout to App Insights regardless of `ENABLEAPPLICATIONINSIGHTS`. The non-standard name keeps the host pipeline dormant so the worker AI SDK is the only thing shipping telemetry.

**Enable / disable from the Azure CLI:**

```bash
# Enable
az functionapp config appsettings set \
  --name wwexeecutiontest \
  --resource-group DEV2 \
  --settings ENABLECONSOLELOGGING=true EXECUTIONLOGLEVEL=4

# Disable
az functionapp config appsettings set \
  --name wwexeecutiontest \
  --resource-group DEV2 \
  --settings ENABLECONSOLELOGGING=false
```

**Stream the live console:**

```bash
# Stream live logs from the Function App
az webapp log tail --name wwexeecutiontest --resource-group DEV2
```

`az webapp log tail` works against Function Apps because they are hosted on App Service. The stream shows only what is being written to stdout/file logging, so it is silent unless `ENABLECONSOLELOGGING=true` AND the category passes the `host.json` filter (see below).

**Console verbosity comes from `host.json`:**

Even with the env var on, the MEL layer (Gate 2) filters per category in `host.json`. Current settings:

```jsonc
// host.json (project root, copied to publish output by .csproj)
{
  "logging": {
    "fileLoggingMode": "always",
    "logLevel": {
      "default": "Warning",
      "Function": "Error",
      "Host.Results": "Error",
      "Host.Aggregator": "Error",
      "Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger": "Warning",
      "Warewolf.Execution.Lightweight.Security.AuditLogger": "Warning",
      "Warewolf.Execution.Lightweight.Infrastructure.StartupOrchestrator": "Warning"
    },
    "console": { "isEnabled": true }
  }
}
```

> ⚠️ **Gotcha:** A log line must pass BOTH gates. `EXECUTIONLOGLEVEL=5` (DEBUG) without lowering `"AzureExecutionLogger": "Warning"` in `host.json` will still suppress DEBUG output in the live stream.

### I.3.2 Elasticsearch

Two conditions are required for the Elasticsearch sink to be added at startup:

- `ENABLEELASTICSEARCHLOGGING=true` is set on the Function App.
- The file `Settings/ElasticsearchLoggingSource.bite` is present in the deployed package (it is included in the publish output via `<Content>` in the `.csproj` — `CopyToOutputDirectory=Always`).

If the env var is true but the file is missing, the sink is silently NOT added. See `ServiceCollectionExtensions.AddExecutionLogging`:

```csharp
if (enableElastic && File.Exists(elasticsearchSettingsPath))
{
    var elasticOptions = ElasticsearchLoggingOptions.FromBiteFile(elasticsearchSettingsPath);
    loggers.Add(new ElasticsearchExecutionLogger(elasticOptions, minimumLevel));
}
```

**The `.bite` file (connection details):**

`Settings/ElasticsearchLoggingSource.bite` is a Warewolf source XML. Its `ConnectionString` attribute carries host, port, index, auth type and credentials, semicolon-delimited. The value may be AES-encrypted (prefix `WFAES::`) — in that case `ElasticsearchLoggingOptions.FromBiteFile` decrypts it via `DpapiWrapper.Decrypt` (the AES hook is wired by `KeyVaultStartupExtensions` before the sink is built).

```
# Decrypted form of the ConnectionString attribute
HostName=http://host.docker.internal;Port=9200;
SearchIndex=warewolftestlogs;AuthenticationType=Password;
Username=test;Password=test123

# AuthenticationType values recognised by FromBiteFile:
#   Anonymous   → no auth
#   Password    → Username + Password
#   API_Key     → API key in the Password field
```

**Enable / disable from the Azure CLI:**

```bash
# Enable
az functionapp config appsettings set \
  --name wwexeecutiontest --resource-group DEV2 \
  --settings ENABLEELASTICSEARCHLOGGING=true

# Disable (the env var alone is enough)
az functionapp config appsettings set \
  --name wwexeecutiontest --resource-group DEV2 \
  --settings ENABLEELASTICSEARCHLOGGING=false
```

**Change the Elasticsearch target:**

Edit `Settings/ElasticsearchLoggingSource.bite` and re-publish. If the new connection string should be encrypted, run `Scripts/Encrypt-Config.ps1` against the value before pasting it into the `ConnectionString` attribute.

**Runtime behaviour:**

- Fire-and-forget: indexing runs on a background `Task` and never blocks workflow execution.
- Failures are swallowed (a one-line message is written to stdout for diagnostics).
- **Verbosity is controlled by `EXECUTIONLOGLEVEL` only.** `host.json` does not affect this sink because it bypasses MEL.

### I.3.3 Audit Logger

`Security/AuditLogger.cs` is a thin wrapper over `ILogger<AuditLogger>` that emits structured security events (cold start, Key Vault errors, decryption invocations, authn/authz outcomes). It writes through MEL only, so it does not need `ENABLECONSOLELOGGING` and is not affected by `EXECUTIONLOGLEVEL`. It is also registered unconditionally in `ServiceCollectionExtensions.AddCoreServices` so that authorization middleware can audit 401/403 events even when encryption is off.

**Enable / disable via `host.json`:**

```jsonc
// host.json → logging → logLevel
"Warewolf.Execution.Lightweight.Security.AuditLogger": "Warning"

// "Information" — every auditable action (compliance posture)
// "Warning"     — anomalies / policy violations only  (current default)
// "Error"       — only failed audit writes
// "None"        — disables audit logging entirely
```

> 🛡️ **Invariants enforced in code:** AuditLogger never logs key material (raw or base64), never logs decrypted connection strings, and only includes metadata (timestamps, instance IDs, key IDs).

**What flows into Application Insights:**

Because AuditLogger uses MEL, every audit event is captured by Application Insights when AI is enabled on the Function App — i.e. `ENABLEAPPLICATIONINSIGHTS=true` with the connection string supplied via `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`. KQL queries can filter on `Event=AuthOutcome`, `Outcome`, `Workflow`, `Caller` — these are the structured fields emitted by `LogAuthOutcome`.

---

## I.4 Docker (developers only)

Intended for developers running the engine in a local container via `engine/run.ps1`. The published output is layered into the Azure Functions isolated worker image.

### I.4.1 What the Dockerfile sets

From `engine/docker/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice AS final

WORKDIR /home/site/wwwroot
COPY ./publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Development \
    AZURE_TENANT_ID= \
    DEBUG_AZURE_KEYVAULT_SECRET="" \
    ENABLEELASTICSEARCHLOGGING=true \
    ENABLECONSOLELOGGING=true \
    EXECUTIONLOGLEVEL=4
```

So out of the box the container starts with both sinks active at INFO. The `AzureFunctionsJobHost__Logging__Console__IsEnabled=true` line is the env-var form of `host.json → logging.console.isEnabled=true`; the runtime auto-binds env vars with that double-underscore syntax.

### I.4.2 Where to view logs

| Sink | How to view it | Notes |
|---|---|---|
| Console (stdout) | `docker logs <container>` / `docker logs -f <container>` | `AzureExecutionLogger` output. `run.ps1` prints `docker logs ExecutionEngine_debug --follow` at the end. |
| Elasticsearch | Kibana / Elasticsearch REST | Use `host.docker.internal` in the `.bite` ConnectionString when ES runs on the host. |
| File logging | Inside the container, under `/home/LogFiles` when `fileLoggingMode=always` | The default in `host.json`. |

### I.4.3 Overriding at `docker run`

```bash
# Console only, DEBUG verbosity
docker run -e ENABLECONSOLELOGGING=true \
           -e ENABLEELASTICSEARCHLOGGING=false \
           -e EXECUTIONLOGLEVEL=5 \
           executionengine:debug

# Disable all execution logging
docker run -e ENABLECONSOLELOGGING=false \
           -e ENABLEELASTICSEARCHLOGGING=false \
           executionengine:debug
```

### I.4.4 docker-compose override snippet

```yaml
services:
  execution-engine:
    environment:
      - ENABLECONSOLELOGGING=true
      - ENABLEELASTICSEARCHLOGGING=true
      - EXECUTIONLOGLEVEL=5     # DEBUG
```

### I.4.5 What `run.ps1` forwards

`engine/run.ps1` forwards Key Vault / identity env vars from `local.settings.json` into the container (`AZURE_KEYVAULT_NAME`, `KEYVAULT_SECRET_NAME`, `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, etc.). It does NOT override the three logging env vars — those come from the Dockerfile defaults unless you append more `-e` flags inside the script or pass them on `docker run`.

---

## I.5 Local development (F5)

There are four launch profiles in `Properties/launchSettings.json`: "Docker (Azure Functions)" (runs `engine\run.ps1`), "Azure Functions" (runs `func start --port 7071`), "Azure Functions (Debug)" (the .NET worker project), and "Azure Functions (Coverage)" (collects coverage during `func start`). For F5, you typically use one of the func-start profiles.

### I.5.1 Set the toggles in `local.settings.json`

When running via `func start` (or the Debug profile), entries inside the `Values` block of `local.settings.json` are exported as process env vars before the host boots.

```jsonc
// local.settings.json (current values in the repo)
{
  "IsEncrypted": false,
  "Values": {
    "ENABLEELASTICSEARCHLOGGING": "true",
    "ENABLECONSOLELOGGING": "true",
    "EXECUTIONLOGLEVEL": "4"
    // ...
  }
}
```

A developer-only variant lives at `local.settings.dev.json` (same shape, plus a `DEBUG_AZURE_KEYVAULT_SECRET` blob). Use whichever the func host is configured to read.

### I.5.2 Where the output appears

| Sink | Where output appears locally |
|---|---|
| Console (`AzureExecutionLogger`) | Visual Studio Output window / the Azure Functions terminal that pops up. |
| Elasticsearch | Whatever cluster `Settings/ElasticsearchLoggingSource.bite` points at (typically `http://localhost:9200` for a local stack). |
| `Dev2Logger` (log4net fallback) | Only active before `ExternalSink` is wired (i.e. very early in startup). After `Program.cs` finishes wiring `Dev2LoggerSinkAdapter`, everything routes through the composite. |

### I.5.3 Useful local settings for offline debugging

```jsonc
{
  "Values": {
    "ENABLECONSOLELOGGING": "true",
    "ENABLEELASTICSEARCHLOGGING": "false",   // turn off ES while disconnected
    "EXECUTIONLOGLEVEL": "5",                // DEBUG
    "SkipFailureToRetrieveSecret": "true",   // tolerate KV unreachable
    "DEBUG_AZURE_KEYVAULT_SECRET": "{version:1,keyId:...,key:...,created:...}"
  }
}
```

With `SkipFailureToRetrieveSecret=true` and `DEBUG_AZURE_KEYVAULT_SECRET` set, encrypted `.bite` files still decrypt (AES hook uses the embedded key), so you can keep `ENABLEELASTICSEARCHLOGGING=true` even offline if a local Elasticsearch is running.

### I.5.4 Verbose `host.json` overrides for local debugging

```jsonc
// host.json — temporarily, while debugging
{
  "logging": {
    "logLevel": {
      "default": "Debug",
      "Function": "Debug",
      "Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger": "Debug",
      "Warewolf.Execution.Lightweight.Security.AuditLogger": "Information"
    }
  }
}
```

> 📌 **Remember:** `host.json` is part of the deployed package. Revert these overrides before publishing, or commit them in a branch you do not deploy from.

---

## I.6 Quick reference cheat sheet

| What you want to do | How to do it |
|---|---|
| Turn ALL execution logging off | `ENABLECONSOLELOGGING=false` and `ENABLEELASTICSEARCHLOGGING=false` |
| Reduce noise in production | `EXECUTIONLOGLEVEL=3` (WARN) |
| Maximum verbosity for debugging | `EXECUTIONLOGLEVEL=5` (DEBUG) and lower the `host.json` levels to `Debug` |
| Disable Elasticsearch only | `ENABLEELASTICSEARCHLOGGING=false` |
| Change Elasticsearch target | Edit `Settings/ElasticsearchLoggingSource.bite` ConnectionString (re-encrypt if needed) |
| Silence audit logs | `host.json`: set `AuditLogger` category to `None` |
| View live Azure logs | `az webapp log tail --name <app> --resource-group <rg>` |
| Apply env var change in Azure | `az functionapp config appsettings set --settings KEY=VALUE` (restart is automatic) |
| View Docker logs | `docker logs -f <container>` (e.g. `ExecutionEngine_debug`) |
| Change toggles locally (F5) | Edit `Values` block in `local.settings.json` then relaunch |

### File map — where each setting lives

| Setting / file | Path in the repo |
|---|---|
| Project file | `Warewolf.Execution.Lightweight.csproj` |
| Composite wiring | `Infrastructure/ServiceCollectionExtensions.cs` (`AddExecutionLogging`) |
| Env var reading | `Program.cs` and `Logging/ExecutionLogLevel.cs` |
| Azure execution sink | `Logging/AzureExecutionLogger.cs` |
| Elasticsearch sink | `Logging/ElasticsearchExecutionLogger.cs` and `ElasticsearchLoggingOptions.cs` |
| Composite | `Logging/CompositeExecutionLogger.cs` |
| Dev2Logger bridge | `Logging/Dev2LoggerSinkAdapter.cs` |
| Audit logger | `Security/AuditLogger.cs` |
| Host.json (MEL filters, file logging) | `host.json` |
| Local Values block | `local.settings.json` and `local.settings.dev.json` |
| Elasticsearch connection `.bite` | `Settings/ElasticsearchLoggingSource.bite` |
| Docker baseline | `engine/docker/Dockerfile` |
| Container runner | `engine/run.ps1` |
| Existing in-repo guide | `docs/Logging-Configuration-Guide.md` |

---

# Part II — Logger Verification

## II.0 Fingerprints — what each logger looks like

Verification is easier when you know the exact "fingerprint" each sink leaves behind. Every sink in this project has a distinctive shape; if you see the shape, the sink is working.

### II.0.1 Startup banner (works in every environment)

Whatever the environment, the very first lines from `Program.cs` and `AddExecutionLogging` are gold for verification. Look for these in the console / live stream / file logs:

```
Program starting - loading host environment configuration
Program configuration loaded. WorkflowsDirectory: ..., EncryptionEnabled: ..., IsDevelopment: ...
Program logging configuration: EnableConsole=true, EnableElastic=true, ElasticsearchSettingsPath=...
Program minimum log level set to: INFO
AddExecutionLogging registering. EnableConsole=True, EnableElastic=True
AddExecutionLogging added AzureExecutionLogger
AddExecutionLogging added ElasticsearchExecutionLogger
AddExecutionLogging created CompositeExecutionLogger with 2 sink(s)
Program startup orchestrator completed, configuring Dev2Logger sinks
Program Dev2Logger external sink configured successfully
```

> 🔍 **Pay attention to the sink count.** `CompositeExecutionLogger with 0 sink(s)` → both env vars off (or both flags read as false). `1 sink(s)` → only one is on (or ES is on but `Settings/ElasticsearchLoggingSource.bite` is missing). `2 sink(s)` → both wired correctly.

### II.0.2 Per-invocation markers

Every HTTP function invocation runs through `InstanceCorrelationMiddleware`. It emits two MEL log lines unconditionally (no env var gating):

```
InstanceCorrelationMiddleware invoked for function 'IsLicensed' (InvocationId: <guid>)
Request completed for function 'IsLicensed' (InvocationId: <guid>) in 42ms
```

If you trigger an HTTP endpoint and you don't see these two lines, the Functions worker isn't logging at all — the problem is below the application (`host.json` filter blocking `default`, or `console.isEnabled=false`, or Application Insights not connected).

### II.0.3 Logger fingerprints

| Logger | Distinctive marker | Source |
|---|---|---|
| `AzureExecutionLogger` | Message starts with: `[Instance:<id>] [Invocation:<guid>] [Function:<name>] [Trace:<id>]` | `ExecutionLoggerBase.GetCorrelationPrefix()` |
| `ElasticsearchExecutionLogger` | JSON document in the index with ECS fields: `@timestamp`, `log.level`, `message`, `execution.id`, `instance.id`, `invocation.id`, `function.name`, `trace.id` | `ElasticsearchLogDocument.cs` |
| `AuditLogger` | Message contains: `SECURITY_AUDIT \| Event=ColdStart \| …` or `Event=AuthOutcome \| Outcome=401\|403 \| …` | `Security/AuditLogger.cs` |
| `InstanceCorrelationMiddleware` | `"InstanceCorrelationMiddleware invoked for function ..."` and `"Request completed for function ..."` | `Infrastructure/InstanceCorrelationMiddleware.cs` |

### II.0.4 Failure-mode fingerprints (Console.WriteLine, bypasses MEL)

Two diagnostic lines bypass MEL entirely and write straight to `Console.Out` — so they appear even when the Elasticsearch sink can't emit:

```
[ElasticsearchLogger] Client is null - skipping index.
[ElasticsearchLogger] Exception indexing document: <ExceptionType> - <message>
```

If you see the second line in the console / docker logs / live stream, Elasticsearch is unreachable or misconfigured. The composite swallows the failure so workflow execution continues, but you have a clear signal in the console.

---

## II.1 Azure Cloud verification (highest priority)

In Azure the three sinks surface in three different places: live log stream (console), Application Insights (Azure + Audit), and the Elasticsearch cluster (Elasticsearch). Verify each in order — Azure is where production runs, so this is the section that matters most.

### II.1.1 Verify `AzureExecutionLogger` (console / live stream)

**Step 1 — Open the live stream:**

```bash
az webapp log tail --name wwexeecutiontest --resource-group DEV2

# Or via the portal: Function App → Monitoring → Log stream
```

**Step 2 — Trigger an invocation:**

Hit any anonymous endpoint to force the worker to handle a request. The cheapest one is `GET /IsLicensed` (anonymous, defined in `Functions/LicensingHttpFunction.cs`):

```bash
curl https://wwexeecutiontest.azurewebsites.net/IsLicensed -i

# Or the equally anonymous workflow-catalog endpoint:
curl https://wwexeecutiontest.azurewebsites.net/apis.json -i
```

**Step 3 — Confirm what you see:**

You should see the middleware lines first, then any execution logs:

```
...InstanceCorrelationMiddleware invoked for function 'IsLicensed' (InvocationId: 6f4...)
...[Instance:abc12345] [Invocation:6f4...] [Function:IsLicensed] [Trace:...] [ExecutionId:...] <message>
...Request completed for function 'IsLicensed' (InvocationId: 6f4...) in 18ms
```

If you see the middleware lines but NOT the bracketed `[Instance:...] [Invocation:...]` format, then `AzureExecutionLogger` is being filtered out — check `EXECUTIONLOGLEVEL` and the `host.json` category `Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger` (currently `Warning`). Lower one or both to see more.

**Step 4 — Cross-check in Application Insights:**

Application Insights captures everything that flows through MEL when AI is enabled — that is, `ENABLEAPPLICATIONINSIGHTS=true` with the connection string supplied via `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` (the standard `APPLICATIONINSIGHTS_CONNECTION_STRING` name is deliberately not used, so the host's auto-AI pipeline never forwards stdout on its own). From the Function App → Application Insights → Logs, run:

```kql
// Last 15 min of execution-logger traces
traces
| where timestamp > ago(15m)
| where message has "[ExecutionId:"
| project timestamp, severityLevel, message
| order by timestamp desc

// Or filter by MEL category
traces
| where timestamp > ago(15m)
| where customDimensions.Category == "Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger"
| order by timestamp desc
```

**Step 5 — If nothing appears, walk this checklist:**

1. `ENABLECONSOLELOGGING` is `true` in app settings (cold-start required).
2. `EXECUTIONLOGLEVEL` is permissive enough (`4` = INFO).
3. `host.json` has `"console": { "isEnabled": true }` and a permissive category level.
4. Worker actually scaled to handle the request — `az functionapp show --query state` returns `Running`.

### II.1.2 Verify `ElasticsearchExecutionLogger`

**Step 1 — Confirm the sink was added:**

Find the startup line in the live stream / App Insights:

```
AddExecutionLogging added ElasticsearchExecutionLogger
AddExecutionLogging created CompositeExecutionLogger with 2 sink(s)
```

If you only see `1 sink(s)` even though `ENABLEELASTICSEARCHLOGGING=true`, either the env var is not actually set on the App or `Settings/ElasticsearchLoggingSource.bite` is missing from the publish package. Both conditions are checked: `if (enableElastic && File.Exists(elasticsearchSettingsPath))`.

**Step 2 — Confirm the cluster is reachable from Azure:**

Watch the live stream while the worker starts. If ES is unreachable, you will see one of:

```
[ElasticsearchLogger] Client is null - skipping index.
[ElasticsearchLogger] Exception indexing document: HttpRequestException - ...
```

Network-level: ensure the Function App can resolve and reach the host in the connection string (VNet integration, NSGs, firewall rules on the ES side).

**Step 3 — Query the index directly:**

Replace `warewolftestlogs` with the `SearchIndex` from your `.bite` connection string. Run from a host that has network access to the cluster:

```bash
# Does the index even exist?
curl -u user:pass "https://<es-host>:9200/_cat/indices/warewolftestlogs?v"

# Count of documents (sanity check — should grow with every log line)
curl -u user:pass "https://<es-host>:9200/warewolftestlogs/_count"

# Latest 5 documents
curl -u user:pass -H "Content-Type: application/json" \
  "https://<es-host>:9200/warewolftestlogs/_search" -d '{
    "size": 5,
    "sort": [{"@timestamp": "desc"}],
    "_source": ["@timestamp","log.level","message","execution.id","instance.id","function.name"]
  }'
```

**Step 4 — Cross-check in Kibana Discover:**

In Kibana → Discover, pick the data view matching the index pattern (e.g. `warewolftestlogs*`). Filter by `instance.id` or `function.name` to isolate logs from a specific worker / endpoint. Confirm the ECS fields are populated:

- `@timestamp` — non-null UTC timestamp
- `log.level` — `info` / `warn` / `error` / `fatal` / `debug`
- `message` — the human-readable text
- `execution.id` — workflow correlation Guid (`Guid.Empty` for non-workflow startup messages)
- `instance.id`, `invocation.id`, `function.name`, `trace.id` — populated only inside an invocation (null/empty for startup logs)

**Step 5 — Trigger an invocation and watch the count climb:**

```bash
# In one terminal — poll the count
while true; do
  curl -s -u user:pass "https://<es-host>:9200/warewolftestlogs/_count" | jq .count
  sleep 2
done

# In another — hit an endpoint
curl -s https://wwexeecutiontest.azurewebsites.net/IsLicensed >/dev/null
```

The count should tick up within a few seconds (fire-and-forget indexing). If the count does not change, indexing is failing silently — go back to Step 2 and look at the console for `[ElasticsearchLogger]` errors.

### II.1.3 Verify `AuditLogger`

AuditLogger writes through MEL with category `Warewolf.Execution.Lightweight.Security.AuditLogger`, so it shows up in the same places as the console logger but with the `SECURITY_AUDIT` marker.

**Step 1 — Find the cold-start audit event:**

Every cold start, after the AES key is loaded from Key Vault, the engine emits one `Event=ColdStart` line.

```bash
# Live stream
az webapp log tail --name wwexeecutiontest --resource-group DEV2 | grep SECURITY_AUDIT
```

```kql
// Or in App Insights
traces
| where timestamp > ago(1h)
| where message has "SECURITY_AUDIT"
| where message has "Event=ColdStart"
| project timestamp, message
```

Expected message shape:

```
SECURITY_AUDIT | Event=ColdStart | InstanceId=<id> | KeyId=<guid> | Utc=2026-05-21T...
```

If you don't see it, force a cold start with `az functionapp restart --name <app> --resource-group <rg>` and watch again.

**Step 2 — Force an `Event=AuthOutcome` (401 / 403):**

Audit only fires for security-relevant moments. The easiest way to provoke one is an unauthenticated call to a protected endpoint (which returns 401):

```bash
curl -i https://wwexeecutiontest.azurewebsites.net/Services/HelloWorld
```

```kql
// In App Insights, the audit line for this:
traces
| where timestamp > ago(5m)
| where message has "SECURITY_AUDIT"
| where message has "Event=AuthOutcome"
| project timestamp, message
```

```
SECURITY_AUDIT | Event=AuthOutcome | Outcome=401 | Caller=anonymous |
  Workflow=HelloWorld | Path=/Services/HelloWorld | Reason=... |
  CorrelationId=... | Utc=...
```

**Step 3 — Tune verbosity if you see nothing:**

AuditLogger category is set to `Warning` in `host.json`. `LogColdStart` uses `LogInformation`, so cold-start audit lines may be filtered out in the live stream. Two options:

- Drop the level to `Information` in `host.json` and redeploy.
- Use App Insights — telemetry is captured regardless of the live-stream filter (App Insights uses its own sampling, not `host.json` level filtering for the worker process).

> 📌 **Audit-only quirk:** `AuditLogger` is registered unconditionally (in `AddCoreServices`, not `AddExecutionLogging`). It works even when `ENABLECONSOLELOGGING=false`. The console env var only controls the EXECUTION logger.

---

## II.2 Docker verification

Verification is simpler than Azure because all stdout-bound logs land in one place: `docker logs`. Use this for fast iteration when something stops emitting.

### II.2.1 Verify `AzureExecutionLogger` (stdout)

**Step 1 — Tail the container:**

```bash
# Replace name to match engine/run.ps1 default
docker logs -f ExecutionEngine_debug

# Or by container id
docker ps --format '{{.Names}}\t{{.ID}}'
docker logs -f <id>
```

**Step 2 — Confirm startup banner:**

Within a few seconds of container start you should see the banner from section II.0.1. The sink count should be `2` with the Dockerfile defaults (both env vars are pre-set to true).

**Step 3 — Trigger an invocation and watch the prefix:**

```bash
# From the host (run.ps1 maps host port 7071 -> container 80)
curl http://localhost:7071/IsLicensed -i
```

Expected in the docker logs stream:

```
InstanceCorrelationMiddleware invoked for function 'IsLicensed' ...
[Instance:local-env] [Invocation:<guid>] [Function:IsLicensed] [Trace:...] [ExecutionId:...] ...
Request completed for function 'IsLicensed' ... in 12ms
```

Notice `Instance:local-env` — that's the fallback value because `WEBSITE_INSTANCE_ID` is unset inside the container. Don't treat it as a bug.

### II.2.2 Verify `ElasticsearchExecutionLogger`

**Step 1 — Confirm the sink was added:**

Look for the same `AddExecutionLogging added ElasticsearchExecutionLogger` line in `docker logs`. Failure modes are the same as Azure (env var false, or `.bite` missing in the publish output).

**Step 2 — Pick the right ES host:**

The repo-shipped `Settings/ElasticsearchLoggingSource.bite` uses `HostName=http://host.docker.internal` and `Port=9200` so the container can reach an Elasticsearch instance running on the host. Confirm with a curl from inside the container:

```bash
docker exec -it ExecutionEngine_debug \
  curl -sS http://host.docker.internal:9200
```

A JSON response with `"You Know, for Search"` means the path is open. If you see `Could not resolve host` or `Connection refused`, the index will never receive logs and the console will print `[ElasticsearchLogger] Exception indexing document`.

**Step 3 — Query the local index:**

```bash
# From the host
curl -u test:test123 "http://localhost:9200/warewolftestlogs/_count"

# Trigger something
curl -s http://localhost:7071/IsLicensed >/dev/null

# Count again — should grow
curl -u test:test123 "http://localhost:9200/warewolftestlogs/_count"
```

### II.2.3 Verify `AuditLogger`

The cold-start audit line surfaces in `docker logs` the same way as in Azure live stream:

```bash
docker logs ExecutionEngine_debug | grep SECURITY_AUDIT
```

Provoke an `Event=AuthOutcome` by hitting the protected `/Services/*` route without a token:

```bash
curl -i http://localhost:7071/Services/HelloWorld

# Then look for the audit entry
docker logs ExecutionEngine_debug | grep "Event=AuthOutcome"
```

> 📌 **Tip:** If you bumped `EXECUTIONLOGLEVEL` to `5` (DEBUG), `AuditLogger.LogDecryption` also fires per AES decrypt invocation. That makes the volume of `SECURITY_AUDIT` lines explode — handy in development, leave it at `Warning` anywhere else.

---

## II.3 Local (F5) verification — least priority

Local verification is largely visual — you read the Visual Studio Output window (or the `func start` terminal) and the local Elasticsearch instance if one is running. Use this when iterating on logging code itself before pushing to Docker / Azure.

### II.3.1 Verify `AzureExecutionLogger`

**Step 1 — Launch the right profile:**

From `Properties/launchSettings.json`, pick either:

- `Azure Functions` — runs `func start --port 7071` in an external terminal, easiest to read.
- `Azure Functions (Debug)` — runs the project under the VS debugger; output appears in the Output window (View → Output → Show output from: Debug).

**Step 2 — Read the startup banner in the terminal / Output window:**

The lines from section II.0.1 appear inline among the func host's coloured output. Confirm `CompositeExecutionLogger with 2 sink(s)` (matches the `Values` block of `local.settings.json`, which has both flags `"true"`).

**Step 3 — Hit an endpoint and see the bracketed prefix:**

```bash
curl -i http://localhost:7071/IsLicensed
```

In the terminal you'll see the host's own `Executed 'IsLicensed' (Succeeded, Duration=...)` line plus the middleware and execution-logger lines. The `[Instance:local-env]` fallback applies here too.

### II.3.2 Verify `ElasticsearchExecutionLogger`

**Step 1 — Stand up a local Elasticsearch:**

Easiest: a single-node Elasticsearch on the host:

```bash
docker run -d --name es-local -p 9200:9200 -e discovery.type=single-node \
  -e xpack.security.enabled=false \
  docker.elastic.co/elasticsearch/elasticsearch:8.15.6

# Confirm reachable
curl http://localhost:9200
```

**Step 2 — Point the `.bite` at it:**

Either edit `Settings/ElasticsearchLoggingSource.bite` to use `HostName=http://localhost;Port=9200;AuthenticationType=Anonymous` (and remove the `WFAES::` prefix), or keep the default and disable security on the local cluster so the embedded `test/test123` still works.

**Step 3 — Hit an endpoint, then query the index:**

```bash
curl -s http://localhost:7071/IsLicensed >/dev/null

curl "http://localhost:9200/warewolftestlogs/_search?size=3&sort=@timestamp:desc" | jq .
```

Documents should show up immediately. If `_count` stays at 0, scroll the func terminal for `[ElasticsearchLogger]` lines.

### II.3.3 Verify `AuditLogger`

In the Visual Studio Output window, search for `SECURITY_AUDIT`. Cold-start audit appears once, very early. To force an AuthOutcome locally:

```bash
curl -i http://localhost:7071/Services/HelloWorld
```

Expect `SECURITY_AUDIT | Event=AuthOutcome | Outcome=401 | ...`. If nothing comes through, check that the AuditLogger category in `host.json` isn't set to `None` (`Warning` is the current default and is sufficient for AuthOutcome lines because they are logged at Warning level).

---

## II.4 Troubleshooting matrix

When verification fails, work this table top-to-bottom. The first matching row is almost always the cause.

| Symptom | Where to look | Likely cause / fix |
|---|---|---|
| No startup banner anywhere | `host.json` `logging.console.isEnabled` | Console output disabled. Set `"console": { "isEnabled": true }` and redeploy / restart. |
| Banner shows "0 sink(s)" | Env vars / app settings | Both `ENABLECONSOLELOGGING` and `ENABLEELASTICSEARCHLOGGING` are unset or false. Set at least one. |
| Banner shows "1 sink(s)" but ES expected | `Settings/ElasticsearchLoggingSource.bite` in publish | File missing → `File.Exists()` returned false, sink was silently skipped. Confirm `<Content>` entry in `.csproj` has `CopyToPublishDirectory=Always`. |
| Middleware lines visible, execution logger silent | `EXECUTIONLOGLEVEL` and `host.json` category | Gate 1 (env var) or Gate 2 (host.json) is too strict for `AzureExecutionLogger`. Lower one or both. |
| Console fine, App Insights empty | `ENABLEAPPLICATIONINSIGHTS` and `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` app settings | AI is off unless BOTH are set: `ENABLEAPPLICATIONINSIGHTS=true` (the authoritative switch) AND a valid connection string in `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`. Check both; a missing/wrong connection string or `ENABLEAPPLICATIONINSIGHTS≠true` means no worker telemetry is shipped. |
| ES sink added but index empty | Console for `[ElasticsearchLogger] Exception` lines | Cluster unreachable from worker. Check network rules / VNET / firewall, then the credentials in the `.bite` ConnectionString. |
| ES index has data but missing `instance.id` / `invocation.id` | Whether logs were emitted during an invocation | Startup-time entries (Program.cs `Dev2Logger.Info` calls) run before `InstanceCorrelationContext` is set; correlation fields are null by design. |
| No `SECURITY_AUDIT` lines at all | `host.json` `AuditLogger` category | Set to `None` → audit disabled. Change to `Warning` or `Information` and redeploy. |
| `SECURITY_AUDIT` visible in App Insights but not live stream | `host.json` `AuditLogger` category level vs `LogColdStart` severity | `LogColdStart` uses `Information`; with category=`Warning` the live stream filters it. Lower to `Information` OR rely on App Insights. |
| Audit `AuthOutcome` never fires | Auth middleware actually returned 401/403 | If the endpoint is anonymous or accepts the token you sent, no audit event is generated. Hit a protected route without a token. |

---

## II.5 One-page cheat sheet

| Goal | Command / query |
|---|---|
| Watch Azure live console | `az webapp log tail --name <app> --resource-group <rg>` |
| Trigger an invocation in Azure | `curl https://<app>.azurewebsites.net/IsLicensed -i` |
| App Insights — execution logger | `traces \| where message has "[ExecutionId:" \| order by timestamp desc` |
| App Insights — audit logger | `traces \| where message has "SECURITY_AUDIT" \| order by timestamp desc` |
| Query ES doc count | `curl -u u:p "https://<es>:9200/<index>/_count"` |
| Query ES recent docs | `curl -u u:p "https://<es>:9200/<index>/_search?size=5&sort=@timestamp:desc"` |
| Watch Docker stdout | `docker logs -f ExecutionEngine_debug` |
| Trigger an invocation in Docker | `curl http://localhost:7071/IsLicensed -i` |
| Force a 401 to fire audit | `curl -i http://localhost:7071/Services/HelloWorld` |
| Confirm container can reach host ES | `docker exec -it ExecutionEngine_debug curl -sS http://host.docker.internal:9200` |
| Local Output window — filter audit | Search for `SECURITY_AUDIT` in VS Output → Debug |
| Look for ES failure in any env | Search the console for `[ElasticsearchLogger] Exception` |
