using Dev2.Common;
using Dev2.Runtime.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;

const string executionId = "Program-Startup";

try
{
    Dev2Logger.Info("Program starting - loading host environment configuration", executionId);

    var config = HostEnvironmentConfig.Load();

    Dev2Logger.Info($"Program configuration loaded. WorkflowsDirectory: {config.WorkflowsDirectory}, EncryptionEnabled: {config.EncryptionEnabled}, IsDevelopment: {config.IsDevelopment}", executionId);

    static bool IsEnabled(string key) =>
        string.Equals(Environment.GetEnvironmentVariable(key), "true", StringComparison.OrdinalIgnoreCase);

    var enableConsole   = IsEnabled("ENABLECONSOLELOGGING");
    var enableElastic   = IsEnabled("ENABLEELASTICSEARCHLOGGING");
    var elasticsearchSettingsPath = Path.Combine(AppContext.BaseDirectory, "Settings", "ElasticsearchLoggingSource.bite");

    Dev2Logger.Debug($"Program logging configuration: EnableConsole={enableConsole}, EnableElastic={enableElastic}, ElasticsearchSettingsPath={elasticsearchSettingsPath}", executionId);

    // Single log-level gate shared by all sinks.
    // Set ExecutionLogLevel=Warning  → only Warning / Error / Critical reach any sink.
    // Set ExecutionLogLevel=Debug    → everything flows through.
    var minimumLevel = Warewolf.Execution.Lightweight.Logging.ExecutionLogLevel.Read();

    Dev2Logger.Info($"Program minimum log level set to: {minimumLevel}", executionId);

    var elasticOptions = enableElastic && File.Exists(elasticsearchSettingsPath)
        ? ElasticsearchLoggingOptions.FromBiteFile(elasticsearchSettingsPath)
        : null;

    if (enableElastic && elasticOptions == null)
    {
        Dev2Logger.Warn($"Program Elasticsearch enabled but settings file not found or failed to load: {elasticsearchSettingsPath}", executionId);
    }

    Dev2Logger.Debug("Program building host", executionId);

    var host = new HostBuilder()
        .ConfigureWarewolf(config)
        .ConfigureServices(services =>
         {
             services.AddSingleton<IExecutionLogger>(sp =>
             {
                 var loggers = new List<IExecutionLogger>();

                 // AzureExecutionLogger — MEL sink (App Insights / console)
                 if (enableConsole)
                 {
                     loggers.Add(new AzureExecutionLogger(
                         sp.GetRequiredService<ILogger<AzureExecutionLogger>>(),
                         minimumLevel));
                     Dev2Logger.Debug("Program added AzureExecutionLogger to logging pipeline", executionId);
                 }

                 // ElasticsearchExecutionLogger — Elasticsearch sink
                 if (elasticOptions is not null)
                 {
                     loggers.Add(new ElasticsearchExecutionLogger(elasticOptions, minimumLevel));
                     Dev2Logger.Debug("Program added ElasticsearchExecutionLogger to logging pipeline", executionId);
                 }

                 return new CompositeExecutionLogger(loggers);
             });

         })
        .Build();

    Dev2Logger.Info("Program host built successfully, running startup orchestrator", executionId);

    await StartupOrchestrator.RunStartupAsync(host, config);

    Dev2Logger.Info("Program startup orchestrator completed, configuring Dev2Logger sinks", executionId);

    // Route every Dev2Logger.X() call to the IExecutionLogger sinks (Azure / Elasticsearch).
    // Must be set after RunStartupAsync so Config.Server is initialised before any Dev2Logger call.
    Dev2.Common.Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(
        host.Services.GetRequiredService<IExecutionLogger>());

    // Also set the correlation prefix provider so that Dev2Logger's own log4net path
    // (when ExternalSink is bypassed) includes instance/invocation correlation.
    Dev2.Common.Dev2Logger.CorrelationPrefixProvider =
        Warewolf.Execution.Lightweight.Logging.ExecutionLoggerBase.GetCorrelationPrefixStatic;

    Dev2Logger.Info("Program Dev2Logger external sink configured successfully", executionId);

    var startupLogger = host.Services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup");

    Dev2Logger.Debug("Program loading Warewolf License", executionId);

    startupLogger.LogInformation("Loading \"Warewolf License.secureconfig\"...");
    var licenseProvider = SubscriptionProvider.Instance;
    if (licenseProvider.IsLicensed)
    {
        Dev2Logger.Info($"Program license loaded successfully. Status: {licenseProvider.Status}", executionId);
        startupLogger.LogInformation("\"Warewolf License.secureconfig\" loaded successfully. Server is licensed (Status: {Status}).", licenseProvider.Status);
    }
    else
    {
        Dev2Logger.Warn($"Program server not licensed. Status: {licenseProvider.Status}, StopExecutions: {licenseProvider.StopExecutions}", executionId);
        startupLogger.LogWarning("\"Warewolf License.secureconfig\" loaded. Server is not licensed (Status: {Status}, StopExecutions: {StopExecutions}).", licenseProvider.Status, licenseProvider.StopExecutions);
    }

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
