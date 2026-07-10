# Warewolf.Execution.Lightweight — Logging Guide

**Single source of truth** for every kind of logging in the lightweight execution
engine: how the sinks are wired, how to turn each on/off per environment, how to
verify each is emitting, and how the deployment script
(`Scripts/Deploy-WwExecutionEngine.ps1`) configures it all.

Grounded directly in the project source:
`Program.cs`, `Infrastructure/ServiceCollectionExtensions.cs`,
`Logging/*` (`LoggingConfiguration`, `ConsoleExecutionLogger`, `AzureExecutionLogger`,
`ElasticsearchExecutionLogger`, `AuditExecutionLogger`, `CompositeExecutionLogger`,
`ExecutionLogLevel`, `ApplicationInsightsLogFilter`), `Security/AuditLogger.cs`,
`host.json`, `local.settings*.json`, `engine/docker/`, and
`Settings/ElasticsearchLoggingSource.bite`.

> This guide supersedes the earlier `Warewolf-Lightweight-Logger-Guide.md`, the two
> `*.docx` logger guides, and the four `*-ApplicationInsights.md` documents. Those
> have been moved to [`archive/`](archive/) for historical reference.

---

## Table of Contents

- [1. Architecture](#1-architecture)
  - [1.1 The composite and its sinks](#11-the-composite-and-its-sinks)
  - [1.2 Sink selection rules (exactly what gets wired)](#12-sink-selection-rules-exactly-what-gets-wired)
  - [1.3 The two worker log paths](#13-the-two-worker-log-paths)
  - [1.4 The two audit paths](#14-the-two-audit-paths)
  - [1.5 host.json applies to the HOST, not the worker](#15-hostjson-applies-to-the-host-not-the-worker)
- [2. Configuration — environment variables](#2-configuration--environment-variables)
  - [2.1 The logging knobs](#21-the-logging-knobs)
  - [2.2 EXECUTIONLOGLEVEL values](#22-executionloglevel-values)
  - [2.3 Quick enable/disable matrix](#23-quick-enabledisable-matrix)
- [3. The sinks in detail](#3-the-sinks-in-detail)
  - [3.1 Console / stdout](#31-console--stdout)
  - [3.2 Application Insights](#32-application-insights)
  - [3.3 Elasticsearch](#33-elasticsearch)
  - [3.4 Audit logging](#34-audit-logging)
- [4. Enable / disable per environment](#4-enable--disable-per-environment)
  - [4.1 Azure (highest priority)](#41-azure-highest-priority)
  - [4.2 Docker](#42-docker)
  - [4.3 Local development (F5)](#43-local-development-f5)
- [5. Verification](#5-verification)
  - [5.1 Fingerprints](#51-fingerprints)
  - [5.2 Azure verification](#52-azure-verification)
  - [5.3 Docker verification](#53-docker-verification)
  - [5.4 Local (F5) verification](#54-local-f5-verification)
  - [5.5 Troubleshooting matrix](#55-troubleshooting-matrix)
- [6. How the deploy script wires logging](#6-how-the-deploy-script-wires-logging)
- [7. File map](#7-file-map)

---

## 1. Architecture

### 1.1 The composite and its sinks

All application logging goes through the legacy `Dev2Logger`, which bridges into a
composite that fans out to the active sinks:

```
Dev2Logger  (legacy static, used everywhere in Dev2.*)
  └── Dev2LoggerSinkAdapter            (Logging/Dev2LoggerSinkAdapter.cs)
        └── CompositeExecutionLogger    (Logging/CompositeExecutionLogger.cs)
              ├── ConsoleExecutionLogger ─┐  EXACTLY ONE of these two
              │   OR                      │  (never both — see §1.2)
              ├── AzureExecutionLogger  ──┘
              ├── ElasticsearchExecutionLogger   (opt-in)
              └── AuditExecutionLogger           (ALWAYS present)
```

| Sink | Class | Destination | When added |
|---|---|---|---|
| Console | `Logging/ConsoleExecutionLogger.cs` | `ILogger<T>` → stdout (Live Log Stream / filesystem logs) | AI **off** AND `ENABLECONSOLELOGGING=true` |
| Azure | `Logging/AzureExecutionLogger.cs` | `ILogger<T>` → Application Insights **and** stdout | `ENABLEAPPLICATIONINSIGHTS=true` |
| Elasticsearch | `Logging/ElasticsearchExecutionLogger.cs` | direct HTTP → Elasticsearch index | `ENABLEELASTICSEARCHLOGGING=true` **and** `.bite` file present |
| Audit | `Logging/AuditExecutionLogger.cs` | `ILogger<T>` (EventId 9000, `[AUDIT]`) | **always** |

There are also two MEL-only loggers that **do not go through the composite**:

- `Security/AuditLogger.cs` — security events (`SECURITY_AUDIT | …`), see [§1.4](#14-the-two-audit-paths).
- `Infrastructure/InstanceCorrelationMiddleware.cs` — per-request start/end lines.

### 1.2 Sink selection rules (exactly what gets wired)

The composite is built in
[`ServiceCollectionExtensions.AddExecutionLogging`](../Infrastructure/ServiceCollectionExtensions.cs).
The general-purpose MEL sink is **exactly one** of Console or Azure — never both,
because both wrap `ILogger<T>`, which in the isolated worker broadcasts to *every*
registered MEL provider (the category `<T>` only labels the entry; it does not pick a
provider). Adding both would emit everything twice.

```csharp
// 1. EXACTLY ONE general-purpose MEL sink
if (loggingConfig.RegisterApplicationInsightsSdk)          // ENABLEAPPLICATIONINSIGHTS=true
    loggers.Add(new AzureExecutionLogger(...));            // → AI provider + Console provider
else if (loggingConfig.EnableConsoleLogging)               // ENABLECONSOLELOGGING=true
    loggers.Add(new ConsoleExecutionLogger(...));          // → Console provider only

// 2. Elasticsearch — opt-in, requires the .bite file to exist
if (loggingConfig.EnableElasticsearch && File.Exists(loggingConfig.ElasticsearchSettingsPath))
    loggers.Add(new ElasticsearchExecutionLogger(...));

// 3. Audit — ALWAYS present
loggers.Add(new AuditExecutionLogger(...));
```

Consequences:

- If `ENABLEAPPLICATIONINSIGHTS=true`, the Azure sink is the general-purpose sink and
  the dedicated Console sink is **not** added (the Azure sink already reaches stdout
  via the console provider). `ENABLECONSOLELOGGING` then only controls whether the
  console **provider** is attached (see §1.3), i.e. whether stdout/filesystem logs receive anything.
- The composite is **never empty**: `AuditExecutionLogger` is always present. The
  startup banner reports the count — e.g. `CompositeExecutionLogger with 2 sink(s)`.

### 1.3 The two worker log paths

The .NET isolated worker has two independent log paths, each gated by a different env var:

```
(a) process stdout  → captured by the Functions host → FILESYSTEM logs / Live Log Stream
        gated by ENABLECONSOLELOGGING  (attaches/omits the MEL AddConsole() provider)

(b) worker AI SDK    → Application Insights
        gated by ENABLEAPPLICATIONINSIGHTS  (registers the worker AI SDK)
```

In [`Program.cs`](../Program.cs) the console provider is attached **only** when
`ENABLECONSOLELOGGING=true`:

```csharp
.ConfigureLogging(logging =>
{
    if (loggingConfig.EnableConsoleLogging)        // ENABLECONSOLELOGGING
        logging.AddConsole();
    logging.AddFilter("Warewolf.Execution.Lightweight", loggingConfig.MelMinimumLevel);
})
```

So with `ENABLECONSOLELOGGING=false` there is **no** console provider, and execution
logs reach only the sinks chosen in `AddExecutionLogging` (e.g. the AI SDK when
`ENABLEAPPLICATIONINSIGHTS=true`) — never the filesystem logs.

### 1.4 The two audit paths

Audit events surface through **two distinct loggers** with different markers — do not
confuse them when searching:

| | `Security/AuditLogger` | `Logging/AuditExecutionLogger` |
|---|---|---|
| Marker | `SECURITY_AUDIT \| Event=… \| …` | `[AUDIT] …` |
| EventId | none | **9000** (`"AuditLog"`) |
| Path | direct `ILogger<AuditLogger>` (MEL) | via `Dev2Logger` → composite |
| MEL category | `Warewolf.Execution.Lightweight.Security.AuditLogger` | `Warewolf.Execution.Lightweight.Logging.AuditExecutionLogger` |
| Levels emitted | ColdStart=Information, AuthOutcome=Warning, Decryption=Debug, KeyVaultError=Error | only Error/Fatal (Debug/Info/Warn are no-ops) |
| Level gating | not gated by `EXECUTIONLOGLEVEL` | **not** gated by `EXECUTIONLOGLEVEL` (always written) |

`Security/AuditLogger` emits the operational `SECURITY_AUDIT` events (cold start, Key
Vault errors, decryption, 401/403 auth outcomes) and is registered unconditionally in
`AddCoreServices`, so it works even when `ENABLECONSOLELOGGING=false`. Its invariants:
never logs key material or decrypted connection strings — only metadata.

> Note: the XML doc-comment on `Security/AuditLogger` says it "routes through
> `Dev2Logger`", but the current code injects `ILogger<AuditLogger>` and logs to MEL
> directly. Treat its MEL **category** (above) as the source of truth for filtering.

### 1.5 host.json applies to the HOST, not the worker

**The isolated worker's MEL pipeline does not read `host.json`.** The `host.json`
`logging.logLevel` rules filter the **Functions host process** only. The worker's
verbosity is controlled entirely in code:

- `EXECUTIONLOGLEVEL` → `LoggingConfiguration.MinimumLevel` → each sink's `ShouldLog`.
- The namespace filter `AddFilter("Warewolf.Execution.Lightweight", MelMinimumLevel)`
  (Program.cs) gates the worker MEL providers.
- For the AI provider specifically,
  [`ApplicationInsightsLogFilter.Apply`](../Logging/ApplicationInsightsLogFilter.cs)
  replaces the AI SDK's default `Warning` gate with a rule at `EXECUTIONLOGLEVEL`.

The deploy script's optional `-AlignHostJsonLogLevel` switch rewrites `host.json`
`logLevel`, but that only tunes the **host process** verbosity — it is **not** needed
for the engine's own logging.

---

## 2. Configuration — environment variables

### 2.1 The logging knobs

All knobs are read once at startup by
[`LoggingConfiguration.FromEnvironment()`](../Logging/LoggingConfiguration.cs). In
Azure they are Function App application settings (read on cold start; changing one
restarts the worker).

| Variable | Values | Default | Effect |
|---|---|---|---|
| `EXECUTIONLOGLEVEL` | `0`–`6` or name | `4` (INFO) | Minimum level shared by all execution sinks. |
| `ENABLECONSOLELOGGING` | `true`/`false` | unset → false (deploy script sets `true`) | Attaches the MEL console provider (stdout → filesystem logs). Also selects `ConsoleExecutionLogger` when AI is off. |
| `ENABLEAPPLICATIONINSIGHTS` | `true`/`false` | unset → false (deploy script sets `true`) | Single authoritative AI switch: registers the worker AI SDK + `AzureExecutionLogger`. Requires `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`. |
| `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` | conn string | unset | AI connection string. **Deliberately non-standard name** (see §3.2). |
| `ENABLEELASTICSEARCHLOGGING` | `true`/`false` | unset → false | Adds `ElasticsearchExecutionLogger`. Also requires `Settings/ElasticsearchLoggingSource.bite`. |
| `STRUCTURED_LOGS` | `true`/`false` | `true` in Azure / `false` in Development | Parsed into `LoggingConfiguration.StructuredJson`. **Currently read but not yet wired to a JSON console formatter** (reserved; no-op at the sink level today). |
| `ELASTIC_DEBUG_MODE` | `true`/`false` | `false` (dev-only) | Enables Elasticsearch HTTP request/response debug tracing. Forced off outside Development. Not set by the deploy script. |
| `ASPNETCORE_ENVIRONMENT` | `Development`/`Production` | `Production` (fixed by deploy script) | Selects the logging profile (affects `STRUCTURED_LOGS`/`ELASTIC_DEBUG_MODE` defaults). |

### 2.2 EXECUTIONLOGLEVEL values

From [`ExecutionLogLevel.cs`](../Logging/ExecutionLogLevel.cs). Accepts numeric or
case-insensitive name; falls back to INFO when missing/unrecognised. Dev2 convention:
**higher number = more verbose.** An entry is emitted when `minimumLevel >= messageLevel`
(and `minimumLevel != OFF`).

| Numeric | Name | Meaning |
|---|---|---|
| `0` | `OFF` | No execution logging. |
| `1` | `FATAL` | Fatal only. |
| `2` | `ERROR` | Error and above. |
| `3` | `WARN` | Warning and above. |
| `4` | `INFO` | Info and above (**DEFAULT**). |
| `5` | `DEBUG` | Debug and above. |
| `6` | `TRACE` | Everything. |

### 2.3 Quick enable/disable matrix

| Goal | `ENABLEAPPLICATIONINSIGHTS` | `ENABLECONSOLELOGGING` | `ENABLEELASTICSEARCHLOGGING` | `EXECUTIONLOGLEVEL` |
|---|---|---|---|---|
| All execution logging off* | `false` | `false` | `false` | (any) |
| Console/stdout only (dev) | `false` | `true` | `false` | `5` (DEBUG) |
| App Insights (prod) | `true` | `true` | `false` | `4` (INFO) |
| Elasticsearch (prod) | `false` | `false` | `true` | `4` (INFO) |
| App Insights + Elasticsearch | `true` | `true` | `true` | `4` (INFO) |
| Verbose troubleshooting | `true` | `true` | `true` | `6` (TRACE) |

\* `AuditExecutionLogger` is still present (security events are never gated by these flags),
but with no console/AI provider attached, audit Error/Fatal entries have nowhere visible to land.

---

## 3. The sinks in detail

### 3.1 Console / stdout

`ConsoleExecutionLogger` wraps `ILogger<ConsoleExecutionLogger>` and is the
always-available stdout sink when AI is off. Azure captures stdout into the Live Log
Stream and filesystem logs. Its entries carry the correlation prefix
`[Instance:<id>] [Invocation:<guid>] [Function:<name>] [Trace:<id>] [ExecutionId:<guid>]`.

When `ENABLEAPPLICATIONINSIGHTS=true`, `AzureExecutionLogger` takes the
general-purpose role and *also* reaches stdout (the console provider stays attached as
long as `ENABLECONSOLELOGGING=true`), so you still see the same lines in the live stream.

### 3.2 Application Insights

`ENABLEAPPLICATIONINSIGHTS=true` is the **single authoritative switch**. It registers
the worker AI SDK (`AddApplicationInsightsTelemetryWorkerService` +
`ConfigureFunctionsApplicationInsights`) and applies the
`ApplicationInsightsLogFilter` rule at `EXECUTIONLOGLEVEL`.

> ⚠️ **Why the non-standard connection-string name.** The engine reads
> `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`, **not** the standard
> `APPLICATIONINSIGHTS_CONNECTION_STRING`. The standard name auto-enables the Azure
> Functions **host's own** AI pipeline (a separate process the worker cannot switch
> off), which would forward captured stdout to AI at `Information` regardless of
> `ENABLEAPPLICATIONINSIGHTS`. Using a host-unrecognised name keeps the host pipeline
> dormant, so the worker SDK is the only path to AI and per-level severity is preserved.
>
> Do **not** click "Turn on Application Insights" in the portal — it sets the standard
> name and wakes the host pipeline. Provision via CLI (below). The portal may keep
> showing a "Turn on" button for this app **by design**; telemetry still flows — verify
> in the App Insights resource's own **Logs** blade.

**Provision the AI resource and wire it (CLI):**

```powershell
$Rg          = 'DEV2'
$AppName     = 'wwexeecutiontest'
$AppInsights = "$AppName-ai"            # component name = {AppName}-ai (matches the scripts)
$Loc         = 'southafricanorth'

# 1. Create the AI component (NOT via the portal "Turn on" button)
az monitor app-insights component create `
    --app $AppInsights --location $Loc --resource-group $Rg `
    --application-type web --kind web

# 2. Read its connection string
$cs = az monitor app-insights component show `
        --app $AppInsights --resource-group $Rg --query connectionString -o tsv

# 3. Wire it onto the Function App (engine name + authoritative switch)
az functionapp config appsettings set --name $AppName --resource-group $Rg --settings `
    ENABLEAPPLICATIONINSIGHTS=true `
    WAREWOLF_APPINSIGHTS_CONNECTION_STRING="$cs" `
    ENABLECONSOLELOGGING=true `
    EXECUTIONLOGLEVEL=4

# 4. Confirm the standard host setting is NOT present (expected: empty)
az functionapp config appsettings list --name $AppName --resource-group $Rg `
    --query "[?name=='APPLICATIONINSIGHTS_CONNECTION_STRING']" -o table
```

> If you already clicked "Turn on Application Insights", remove the host-enabling setting:
> ```powershell
> az functionapp config appsettings delete --name $AppName --resource-group $Rg `
>     --setting-names APPLICATIONINSIGHTS_CONNECTION_STRING
> ```

The automated script `Scripts/Setup-ApplicationInsights.ps1` performs steps 1–3, and
`Scripts/Deploy-WwExecutionEngine.ps1` invokes it as part of an end-to-end deploy
(see [§6](#6-how-the-deploy-script-wires-logging)).

**Telemetry needs three things to align** for execution-logger traces:
`ENABLEAPPLICATIONINSIGHTS=true` + a valid connection string + a permissive
`EXECUTIONLOGLEVEL`. Audit and middleware events flow regardless of `EXECUTIONLOGLEVEL`.

**Useful KQL** (run against the `{AppName}-ai` resource → Logs):

```kql
// Execution-logger traces (last 15 min)
traces
| where timestamp > ago(15m)
| where message has "[ExecutionId:"
| project timestamp, severityLevel, message
| order by timestamp desc

// By MEL category
traces
| where customDimensions.Category == "Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger"
| order by timestamp desc

// Audit events (composite AuditExecutionLogger)
traces
| where customDimensions.EventId == 9000 and message has "[AUDIT]"
| order by timestamp desc

// Security audit events (Security/AuditLogger)
traces
| where message has "SECURITY_AUDIT"
| order by timestamp desc

// Function invocations
requests
| where timestamp > ago(10m)
| project timestamp, name, success, resultCode, duration
| order by timestamp desc
```

**Cost control** lives in `host.json` → `logging.applicationInsights.samplingSettings`
(currently `maxTelemetryItemsPerSecond: 20`, `excludedTypes: "Request;Exception"`). This
is host-side sampling and is the one `host.json` AI knob that *does* affect what reaches AI.

### 3.3 Elasticsearch

Two conditions are required for the sink to be added at startup
([ServiceCollectionExtensions.cs](../Infrastructure/ServiceCollectionExtensions.cs)):

- `ENABLEELASTICSEARCHLOGGING=true`, **and**
- `Settings/ElasticsearchLoggingSource.bite` exists in the deployed package (shipped via
  `<Content>` `CopyToOutputDirectory=Always` in the `.csproj`).

If the env var is true but the file is missing, the sink is silently skipped.

**The `.bite` file** is a Warewolf source XML whose `ConnectionString` attribute carries
host, port, index, auth type and credentials, semicolon-delimited. It may be
AES-encrypted (`WFAES::` prefix), in which case `ElasticsearchLoggingOptions.FromBiteFile`
decrypts it via the AES hook wired by `KeyVaultStartupExtensions` before the sink is built.

```
# Decrypted ConnectionString form
HostName=http://host.docker.internal;Port=9200;
SearchIndex=warewolftestlogs;AuthenticationType=Password;Username=test;Password=test123

# AuthenticationType values: Anonymous | Password | API_Key
```

**Enable/disable:**
```powershell
az functionapp config appsettings set --name $AppName --resource-group $Rg `
    --settings ENABLEELASTICSEARCHLOGGING=true     # or =false to disable
```

**Runtime behaviour:** fire-and-forget on a background `Task` (never blocks execution);
failures are swallowed with a one-line `[ElasticsearchLogger]` diagnostic to stdout.
Verbosity is controlled by `EXECUTIONLOGLEVEL` only. To change the target, edit the
`.bite` `ConnectionString` and re-publish (re-encrypt with `Scripts/Encrypt-Config.ps1`
if needed). `ELASTIC_DEBUG_MODE=true` (Development only) captures raw HTTP traffic.

### 3.4 Audit logging

See [§1.4](#14-the-two-audit-paths) for the two paths. Both are always-on; neither is
gated by `EXECUTIONLOGLEVEL`. To **silence** them you must raise the corresponding
**host-process** `host.json` category to `None` (affects only what the host forwards),
or in App Insights they are always captured.

Expected message shapes:

```
SECURITY_AUDIT | Event=ColdStart | InstanceId=<id> | KeyId=<guid> | Utc=2026-...
SECURITY_AUDIT | Event=AuthOutcome | Outcome=401 | Caller=anonymous | Workflow=HelloWorld |
  Path=/Services/HelloWorld | Reason=... | CorrelationId=... | Utc=...
[AUDIT] [ExecutionId:<guid>] <message>          # AuditExecutionLogger (EventId 9000)
```

---

## 4. Enable / disable per environment

### 4.1 Azure (highest priority)

Set the env vars as Function App application settings (read on cold start):

```powershell
# Enable console/stdout at INFO
az functionapp config appsettings set --name $AppName --resource-group $Rg `
    --settings ENABLECONSOLELOGGING=true EXECUTIONLOGLEVEL=4

# Disable console/stdout
az functionapp config appsettings set --name $AppName --resource-group $Rg `
    --settings ENABLECONSOLELOGGING=false

# Stream the live console (App Service hosts Function Apps)
az webapp log tail --name $AppName --resource-group $Rg
```

App Insights and Elasticsearch toggles are in [§3.2](#32-application-insights) and
[§3.3](#33-elasticsearch).

### 4.2 Docker

For developers running the engine in a container via `engine/run.ps1`. The
`engine/docker/Dockerfile` pre-sets:

```dockerfile
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    ASPNETCORE_ENVIRONMENT=Development \
    ENABLEELASTICSEARCHLOGGING=true \
    ENABLECONSOLELOGGING=true \
    EXECUTIONLOGLEVEL=4
```

So out of the box the container runs console + Elasticsearch at INFO (AI off — no
connection string). Override at `docker run`:

```bash
docker run -e ENABLECONSOLELOGGING=true -e ENABLEELASTICSEARCHLOGGING=false \
           -e EXECUTIONLOGLEVEL=5 executionengine:debug
```

View logs with `docker logs -f ExecutionEngine_debug`. `run.ps1` forwards Key
Vault/identity env vars from `local.settings.json` but does **not** override the three
logging knobs — those come from the Dockerfile unless you append `-e` flags.

### 4.3 Local development (F5)

Launch profiles live in `Properties/launchSettings.json` ("Azure Functions" runs
`func start --port 7071`). Set toggles in the `Values` block of `local.settings.json`:

```jsonc
{
  "IsEncrypted": false,
  "Values": {
    "ENABLECONSOLELOGGING": "true",
    "ENABLEELASTICSEARCHLOGGING": "false",   // off while disconnected
    "EXECUTIONLOGLEVEL": "5",                // DEBUG
    "ENABLEAPPLICATIONINSIGHTS": "false"
    // "WAREWOLF_APPINSIGHTS_CONNECTION_STRING": "..."  // set + enable AI to test locally
  }
}
```

A developer variant `local.settings.dev.json` adds a Key Vault debug-secret blob so
encrypted `.bite` files still decrypt offline. Output appears in the `func start`
terminal / VS Output window.

> ⚠️ Never commit `local.settings.json` with real connection strings.

---

## 5. Verification

### 5.1 Fingerprints

**Startup banner** (every environment) — look for these `Program.cs` /
`AddExecutionLogging` lines:

```
Program starting - bootstrap logging active
Program configuration loaded. WorkflowsDirectory: ..., EncryptionEnabled: ..., IsDevelopment: ...
AddExecutionLogging added AzureExecutionLogger (AI + stdout)      # OR ConsoleExecutionLogger (stdout only)
AddExecutionLogging added ElasticsearchExecutionLogger            # only if ES enabled + .bite present
AddExecutionLogging added AuditExecutionLogger (always-on)
AddExecutionLogging created CompositeExecutionLogger with N sink(s)
Program startup orchestrator completed, upgrading to full composite logger
```

> **Sink count tells the story.** With AI on + no ES: `2 sink(s)` (Azure + Audit). With
> console-only + no ES: `2` (Console + Audit). Add Elasticsearch: `3`. A `1` would mean
> only Audit wired (both Console and AI off).

**Per-invocation markers** — every HTTP call runs `InstanceCorrelationMiddleware`,
which emits two MEL lines unconditionally:

```
InstanceCorrelationMiddleware invoked for function 'IsLicensed' (InvocationId: <guid>)
Request completed for function 'IsLicensed' (InvocationId: <guid>) in 42ms
```

**Per-logger fingerprints:**

| Logger | Distinctive marker |
|---|---|
| `ConsoleExecutionLogger` / `AzureExecutionLogger` | `[Instance:<id>] [Invocation:<guid>] [Function:<name>] [Trace:<id>] [ExecutionId:<guid>] <msg>` |
| `ElasticsearchExecutionLogger` | ECS JSON doc with `@timestamp`, `log.level`, `message`, `execution.id`, `instance.id`, `invocation.id`, `function.name`, `trace.id` |
| `AuditExecutionLogger` | `[AUDIT] …`, `customDimensions.EventId == 9000` |
| `Security/AuditLogger` | `SECURITY_AUDIT \| Event=ColdStart\|AuthOutcome\|… \| …` |

**Failure-mode lines** (bypass MEL, straight to `Console.Out`):

```
[ElasticsearchLogger] Client is null - skipping index.
[ElasticsearchLogger] Exception indexing document: <ExceptionType> - <message>
```

### 5.2 Azure verification

```bash
# 1. Open the live stream and trigger an anonymous endpoint
az webapp log tail --name wwexeecutiontest --resource-group DEV2
curl https://wwexeecutiontest.azurewebsites.net/IsLicensed -i
```

You should see the middleware lines, then the bracketed `[Instance:…] [ExecutionId:…]`
execution line. If you see middleware lines but not the bracketed format, the
execution sink is filtered — raise `EXECUTIONLOGLEVEL`. Cross-check in App Insights
with the KQL in [§3.2](#32-application-insights).

For Elasticsearch, confirm `AddExecutionLogging added ElasticsearchExecutionLogger`
and `N sink(s)` includes it, watch for `[ElasticsearchLogger]` errors, then query the
index:

```bash
curl -u user:pass "https://<es-host>:9200/warewolftestlogs/_count"   # should climb after each call
```

For audit, force a 401 against a protected route and look for `SECURITY_AUDIT |
Event=AuthOutcome`:

```bash
curl -i https://wwexeecutiontest.azurewebsites.net/Services/HelloWorld
```

### 5.3 Docker verification

```bash
docker logs -f ExecutionEngine_debug                 # banner + 2 sink(s) (Dockerfile: console+ES, AI off → 3 with ES)
curl http://localhost:7071/IsLicensed -i             # expect middleware + [Instance:local-env] lines
docker exec -it ExecutionEngine_debug curl -sS http://host.docker.internal:9200   # ES reachable?
docker logs ExecutionEngine_debug | grep SECURITY_AUDIT
```

`Instance:local-env` is the fallback when `WEBSITE_INSTANCE_ID` is unset in-container — not a bug.

### 5.4 Local (F5) verification

Read the `func start` terminal / VS Output window. Confirm the banner sink count
matches your `local.settings.json` flags, hit `http://localhost:7071/IsLicensed`, and
search the output for `[ExecutionId:` and `SECURITY_AUDIT`. For Elasticsearch, stand up
a single-node cluster and point the `.bite` `HostName` at `http://localhost;Port=9200`.

### 5.5 Troubleshooting matrix

| Symptom | Likely cause / fix |
|---|---|
| No startup banner anywhere | Console output disabled. In Docker/local check `host.json` `console.isEnabled`; in Azure check `ENABLECONSOLELOGGING` + the host is `Running`. |
| Banner shows `1 sink(s)` | Both `ENABLEAPPLICATIONINSIGHTS` and `ENABLECONSOLELOGGING` are false (only Audit wired). Enable at least one. |
| Expected ES but it's not in the count | `ENABLEELASTICSEARCHLOGGING` false, or `Settings/ElasticsearchLoggingSource.bite` missing from the package (`File.Exists` failed). |
| Middleware lines visible, execution logger silent | `EXECUTIONLOGLEVEL` too strict. Lower it (worker does **not** read `host.json`). |
| Console fine, App Insights empty | AI needs **both** `ENABLEAPPLICATIONINSIGHTS=true` **and** a valid `WAREWOLF_APPINSIGHTS_CONNECTION_STRING`. Verify the standard `APPLICATIONINSIGHTS_CONNECTION_STRING` is **not** set. |
| ES sink added but index empty | Cluster unreachable from the worker — check `[ElasticsearchLogger] Exception` lines, network/VNet/firewall, then `.bite` credentials. |
| ES docs missing `instance.id`/`invocation.id` | Startup-time entries run before the invocation context exists — null by design. |
| No `SECURITY_AUDIT` / `[AUDIT]` lines | In Azure they're in App Insights regardless. In live stream, the `Security/AuditLogger` host-process category may filter `ColdStart` (Information); rely on App Insights or lower the host category. |
| Audit `AuthOutcome` never fires | The route was anonymous or the token was accepted — hit a protected `/Services/*` route with no token. |
| `Debug` line shows as `[Information]` in filesystem logs | The Functions host stamps captured stdout as `[Information]`; the true level survives in the MEL body prefix (`dbug:`/`info:`). Use the AI structured channel for exact severity. |

---

## 6. How the deploy script wires logging

`Scripts/Deploy-WwExecutionEngine.ps1` is the end-to-end orchestrator. Its logging-relevant behaviour:

- **Prompts/accepts** `-ExecutionLogLevel` (default INFO), `-EnableConsoleLogging`
  (default true), `-StructuredLogs` (default true), `-EnableAppInsights` (default
  true), `-EnableElasticsearch` (default false).
- **Applies these app settings** (Phase 3 →
  [Deploy-WwExecutionEngine.ps1:805-811](../Scripts/Deploy-WwExecutionEngine.ps1#L805-L811)):
  `ASPNETCORE_ENVIRONMENT=Production`, `EXECUTIONLOGLEVEL`, `ENABLECONSOLELOGGING`,
  `STRUCTURED_LOGS`, `ENABLEAPPLICATIONINSIGHTS`, `ENABLEELASTICSEARCHLOGGING`,
  `WAREWOLF_LICENSE_CHECK_ENABLED` (+ Key Vault settings when encrypting).
- **App Insights:** when `-EnableAppInsights`, Phase 1 invokes
  `Setup-ApplicationInsights.ps1`, which creates the `{AppName}-ai` component and sets
  `WAREWOLF_APPINSIGHTS_CONNECTION_STRING` + `ENABLEAPPLICATIONINSIGHTS=true`. It never
  sets the standard `APPLICATIONINSIGHTS_CONNECTION_STRING`.
- **Elasticsearch:** when `-EnableElasticsearch`, stages the user-supplied
  `ElasticsearchLoggingSource.bite` into `Settings/` (encrypting it via Key Vault when
  `-EncryptResources` is set).
- **`-AlignHostJsonLogLevel`** (opt-in): also rewrites `host.json` `logLevel` to the
  `EXECUTIONLOGLEVEL`-mapped MEL level. **Not required** for the engine's own logging —
  it only tunes the Functions **host process** verbosity (the worker is code-driven).
  The script's `Convert-ToMelLevel` mirrors `LoggingConfiguration.MelMinimumLevel`
  exactly (TRACE→Trace … OFF→None).

So a default deploy yields: App Insights + console at INFO, Elasticsearch off, license
check on, structured logs flag set (currently a no-op at the sink level, see §2.1).

---

## 7. File map

| Concern | Path |
|---|---|
| Config record (reads all env vars) | `Logging/LoggingConfiguration.cs` |
| Level parsing / `ShouldLog` | `Logging/ExecutionLogLevel.cs` |
| Composite wiring (sink selection) | `Infrastructure/ServiceCollectionExtensions.cs` (`AddExecutionLogging`) |
| Host build + console provider + AI SDK | `Program.cs` |
| Console sink | `Logging/ConsoleExecutionLogger.cs` |
| App Insights sink | `Logging/AzureExecutionLogger.cs` |
| AI provider filter | `Logging/ApplicationInsightsLogFilter.cs` |
| Elasticsearch sink | `Logging/ElasticsearchExecutionLogger.cs`, `ElasticsearchLoggingOptions.cs`, `ElasticsearchLogDocument.cs` |
| Composite | `Logging/CompositeExecutionLogger.cs` |
| Dev2Logger bridge | `Logging/Dev2LoggerSinkAdapter.cs` |
| Audit (composite) | `Logging/AuditExecutionLogger.cs` |
| Audit (security/MEL) | `Security/AuditLogger.cs` |
| Host-process MEL filters + AI sampling + file logging | `host.json` |
| Local toggles | `local.settings.json`, `local.settings.dev.json` |
| Elasticsearch connection | `Settings/ElasticsearchLoggingSource.bite` |
| Docker baseline / runner | `engine/docker/Dockerfile`, `engine/run.ps1` |
| AI setup script | `Scripts/Setup-ApplicationInsights.ps1` |
| End-to-end deploy | `Scripts/Deploy-WwExecutionEngine.ps1` |
| Encrypt `.bite`/secure.config | `Scripts/Encrypt-Config.ps1` |
