using Dev2.Common;
using Dev2.Runtime.Subscription;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;

const string executionId = "Program-Startup";

try
{
    // ── Step 1: Load configuration ───────────────────────────────────────────
    var config = HostEnvironmentConfig.Load();
    var loggingConfig = LoggingConfiguration.FromEnvironment();

    // WOLF-8512: raised as early in cold start as possible (before host build / DI
    // resolution) rather than lazily on first trigger invocation — see
    // ThreadPoolStartupConfigurator's own doc comment for why. ServiceBusTriggerOptions.
    // FromEnvironment() is cheap/pure (env-var parsing only), so calling it here in
    // addition to its existing DI registration (ServiceCollectionExtensions) is harmless.
    ThreadPoolStartupConfigurator.Configure(ServiceBusTriggerOptions.FromEnvironment().MaxConcurrentExecutions);

    // ── Step 2: Bootstrap logging (FIRST — no log is lost) ───────────────────
    using var bootstrapFactory = LoggerFactory.Create(b =>
        b.AddConsole().SetMinimumLevel(loggingConfig.MelMinimumLevel));
    var bootstrapLogger = new ConsoleExecutionLogger(
        bootstrapFactory.CreateLogger<ConsoleExecutionLogger>(), loggingConfig.MinimumLevel);

    Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(bootstrapLogger);
    Dev2Logger.CorrelationPrefixProvider = ExecutionLoggerBase.GetCorrelationPrefixStatic;

    Dev2Logger.Info($"Program configuration loaded. EncryptionEnabled: {config.EncryptionEnabled}, IsDevelopment: {config.IsDevelopment}", executionId);

    // ── Step 3: Build host ───────────────────────────────────────────────────

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

                 // Opt-in only (ENABLEPERFORMANCECOUNTERS) — the AI SDK never registers this on
                 // its own, so App Insights' performanceCounters table stays empty otherwise.
                 // Auto-detects Azure Web App/Functions hosting (WEBSITE_SITE_NAME) and reads
                 // from the sandboxed %WEBSITE_COUNTERS_APP% source instead of raw Windows perf
                 // counters, which is what makes this safe to run under a Consumption-plan
                 // Functions worker.
                 if (loggingConfig.EnablePerformanceCounters)
                 {
                     services.ConfigureTelemetryModule<PerformanceCollectorModule>((module, _) => { });
                 }

                 // Replace the AI SDK's built-in Warning gate with a targeted rule at the
                 // configured EXECUTIONLOGLEVEL — scoped to the AI provider only, so Console,
                 // Elasticsearch, and Audit sinks are unaffected.
                 services.Configure<LoggerFilterOptions>(options =>
                     ApplicationInsightsLogFilter.Apply(options, loggingConfig.MelMinimumLevel));

                 // WOLF-8512: without an explicit flush on shutdown, a Consumption-plan
                 // instance recycled mid-burst discards its buffered telemetry silently — see
                 // TelemetryFlushHostedService's own doc comment for the live incident this closes.
                 services.AddHostedService<TelemetryFlushHostedService>();
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


    // ── Step 4: Run startup (encryption, index warm-up) ──────────────────────
    await StartupOrchestrator.RunStartupAsync(host, config);

    // ── Step 5: Upgrade to full composite logger ─────────────────────────────
    // Resolve IExecutionLogger AFTER RunStartupAsync so the AES decrypt hook
    // is wired before the Elasticsearch .bite file is read.
    var executionLogger = host.Services.GetRequiredService<IExecutionLogger>();


    // Replace bootstrap sink with the full composite (Console + AI + Elastic + Audit).
    Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(executionLogger);


    // ── Step 6: License check ────────────────────────────────────────────────

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
    // The exception object is withheld from the log sinks (they persist ex.ToString()):
    // this is the outermost catch, so a rethrown Key Vault or persistence failure lands
    // here carrying vault/secret names, identity detail, absolute paths or connection
    // detail. executionId correlates to the phase-specific Fatal already emitted.
    Dev2Logger.Fatal($"Program terminated unexpectedly during startup. ExceptionType={ex.GetType().Name}", executionId);

    await Console.Error.WriteLineAsync(
        $"[FATAL] Host terminated unexpectedly at {DateTimeOffset.UtcNow:O}: {ex}");
    throw;
}
