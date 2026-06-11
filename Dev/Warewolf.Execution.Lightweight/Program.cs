using Dev2.Common;
using Dev2.Runtime.Subscription;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;

const string executionId = "Program-Startup";

try
{
    // ── Step 1: Load configuration ───────────────────────────────────────────
    var config = HostEnvironmentConfig.Load();
    var loggingConfig = LoggingConfiguration.FromEnvironment();

    // ── Step 2: Bootstrap logging (FIRST — no log is lost) ───────────────────
    using var bootstrapFactory = LoggerFactory.Create(b =>
        b.AddConsole().SetMinimumLevel(loggingConfig.MelMinimumLevel));
    var bootstrapLogger = new ConsoleExecutionLogger(
        bootstrapFactory.CreateLogger<ConsoleExecutionLogger>(), loggingConfig.MinimumLevel);

    Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(bootstrapLogger);
    Dev2Logger.CorrelationPrefixProvider = ExecutionLoggerBase.GetCorrelationPrefixStatic;

    Dev2Logger.Info("Program starting - bootstrap logging active", executionId);
    Dev2Logger.Info($"Program configuration loaded. WorkflowsDirectory: {config.WorkflowsDirectory}, EncryptionEnabled: {config.EncryptionEnabled}, IsDevelopment: {config.IsDevelopment}", executionId);

    // ── Step 3: Build host ───────────────────────────────────────────────────
    Dev2Logger.Debug("Program building host", executionId);

    var host = new HostBuilder()
        .ConfigureWarewolf(config)
        // ── Worker logging (driven entirely by environment variables, NOT host.json) ──
        // The isolated WORKER's MEL pipeline does NOT read host.json — host.json logging
        // rules apply to the host process only. Every worker logging knob is an env var:
        //
        //   • ENABLECONSOLELOGGING — when true, attach a console provider so worker logs
        //     reach stdout → Azure Functions FILESYSTEM logs / Live Log Stream. When false,
        //     NO console provider is attached, so execution logs travel ONLY to the sinks
        //     selected in AddExecutionLogging (e.g. the AI SDK when ENABLEAPPLICATIONINSIGHTS
        //     =true) and never reach the filesystem logs. This is the single switch that
        //     turns the worker's filesystem/console channel on or off.
        //   • EXECUTIONLOGLEVEL — honoured here via a namespace-scoped filter, keeping
        //     framework Microsoft.*/System.* noise out at Trace/Debug.
        //
        // The isolated worker has two independent log paths:
        //   (a) process stdout  → captured by the host → FILESYSTEM logs   (gated by ENABLECONSOLELOGGING)
        //   (b) worker AI SDK    → Application Insights                     (gated by ENABLEAPPLICATIONINSIGHTS)
        .ConfigureLogging(logging =>
        {
            if (loggingConfig.EnableConsoleLogging)
                logging.AddConsole();
            logging.AddFilter("Warewolf.Execution.Lightweight", loggingConfig.MelMinimumLevel);
        })
        .ConfigureServices(services =>
         {
             services.AddExecutionLogging(loggingConfig);

             if (loggingConfig.RegisterApplicationInsightsSdk)
             {
                 // Connection string is read from WAREWOLF_APPINSIGHTS_CONNECTION_STRING — a
                 // DELIBERATELY non-standard name. The standard APPLICATIONINSIGHTS_CONNECTION_STRING
                 // auto-enables the Azure Functions HOST's own App Insights pipeline (a separate
                 // process this worker cannot switch off); the host would then forward captured
                 // worker stdout to AI at Information (severity 1) even when ENABLEAPPLICATIONINSIGHTS
                 // =false. Using a host-unrecognised name keeps host-side AI dormant, so
                 // ENABLEAPPLICATIONINSIGHTS is the single authoritative switch and the worker SDK
                 // below is the only path to AI — preserving true per-level severity.
                 var aiConnectionString =
                     Environment.GetEnvironmentVariable("WAREWOLF_APPINSIGHTS_CONNECTION_STRING");

                 // BOTH calls are required and complementary:
                 //  • AddApplicationInsightsTelemetryWorkerService() registers the AI SDK
                 //    (TelemetryConfiguration / TelemetryClient).
                 //  • ConfigureFunctionsApplicationInsights() adds the Functions-worker
                 //    integration AND validates the SDK above was registered — calling it
                 //    alone throws OptionsValidationException:
                 //    "Application Insights SDK has not been added".
                 services.AddApplicationInsightsTelemetryWorkerService(options =>
                     options.ConnectionString = aiConnectionString);
                 services.ConfigureFunctionsApplicationInsights();

                 // Replace the AI SDK's built-in Warning gate with a targeted rule at the
                 // configured EXECUTIONLOGLEVEL — scoped to the AI provider only, so Console,
                 // Elasticsearch, and Audit sinks are unaffected.
                 services.Configure<LoggerFilterOptions>(options =>
                     ApplicationInsightsLogFilter.Apply(options, loggingConfig.MelMinimumLevel));
             }
         })
        .Build();

    // Logged AFTER host.Build() (deliberately NOT in the Step 2 bootstrap phase): this routes the
    // line through the worker MEL pipeline → the console provider attached in .ConfigureLogging(...)
    // above, so the MEL console formatter stamps the message body with its real "dbug:" prefix.
    // Emitting it via the throwaway bootstrap LoggerFactory instead writes raw to stdout, which the
    // Azure Functions host captures and flattens to a [Information] wrapper (the reported symptom).
    // NOTE: with ENABLEAPPLICATIONINSIGHTS=false the host's outer capture stamp is still its own
    // platform behaviour; true per-level severity records require the AI structured channel.
    Dev2Logger.Debug($"Program logging configuration: EnableAI={loggingConfig.RegisterApplicationInsightsSdk}, EnableElastic={loggingConfig.EnableElasticsearch}, MinLevel={loggingConfig.MinimumLevel}", executionId);

    Dev2Logger.Info("Program host built successfully, running startup orchestrator", executionId);

    // ── Step 4: Run startup (encryption, index warm-up) ──────────────────────
    await StartupOrchestrator.RunStartupAsync(host, config);

    // ── Step 5: Upgrade to full composite logger ─────────────────────────────
    // Resolve IExecutionLogger AFTER RunStartupAsync so the AES decrypt hook
    // is wired before the Elasticsearch .bite file is read.
    var executionLogger = host.Services.GetRequiredService<IExecutionLogger>();

    Dev2Logger.Info("Program startup orchestrator completed, upgrading to full composite logger", executionId);

    // Replace bootstrap sink with the full composite (Console + AI + Elastic + Audit).
    Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(executionLogger);

    Dev2Logger.Info("Program Dev2Logger external sink upgraded to full CompositeExecutionLogger", executionId);

    // AppInsights severity validation — MUST run AFTER the ExternalSink upgrade above so
    // entries flow through AzureExecutionLogger (the AI SDK ILogger provider) which preserves
    // per-level severity. Before the upgrade only the console sink is active, so the Functions
    // host captures stdout and stamps every entry as Information (severity 1).
    Dev2Logger.Trace("[AppInsights-Test] TRACE level log entry - validates TRACE severity in Application Insights", executionId);
    Dev2Logger.Debug("[AppInsights-Test] DEBUG level log entry - validates DEBUG severity in Application Insights", executionId);
    Dev2Logger.Info("[AppInsights-Test] INFO level log entry - validates INFO severity in Application Insights", executionId);
    Dev2Logger.Warn("[AppInsights-Test] WARN level log entry - validates WARN severity in Application Insights", executionId);
    Dev2Logger.Error("[AppInsights-Test] ERROR level log entry - validates ERROR severity in Application Insights", executionId);
    Dev2Logger.Fatal("[AppInsights-Test] FATAL level log entry - validates FATAL severity in Application Insights", executionId);

    // ── Step 6: License check ────────────────────────────────────────────────
    Dev2Logger.Debug("Program loading Warewolf License", executionId);

    var licenseProvider = SubscriptionProvider.Instance;
    if (licenseProvider.IsLicensed)
    {
        Dev2Logger.Info($"Program license loaded successfully. Status: {licenseProvider.Status}", executionId);
    }
    else
    {
        Dev2Logger.Warn($"Program server not licensed. Status: {licenseProvider.Status}, StopExecutions: {licenseProvider.StopExecutions}", executionId);
    }

    // ── Step 7: Run ──────────────────────────────────────────────────────────   
    Dev2Logger.Info("Program initialization complete, starting host", executionId);

    await host.RunAsync();
}
catch (Exception ex)
{
    // Fatal cold-start failure — write to stderr so the Azure Functions runtime
    // captures it regardless of whether the logging pipeline is available.
    Dev2Logger.Fatal("Program terminated unexpectedly during startup", ex, executionId);

    await Console.Error.WriteLineAsync(
        $"[FATAL] Host terminated unexpectedly at {DateTimeOffset.UtcNow:O}: {ex}");
    throw;
}
