using Dev2.Runtime.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;

try
{
    var config = HostEnvironmentConfig.Load();
    
    static bool IsEnabled(string key) =>
        string.Equals(Environment.GetEnvironmentVariable(key), "true", StringComparison.OrdinalIgnoreCase);

    var enableConsole   = IsEnabled("ENABLECONSOLELOGGING");
    var enableElastic   = IsEnabled("ENABLEELASTICSEARCHLOGGING");
    var elasticsearchSettingsPath = Path.Combine(AppContext.BaseDirectory, "Settings", "ElasticsearchLoggingSource.bite");

    // Single log-level gate shared by all sinks.
    // Set ExecutionLogLevel=Warning  → only Warning / Error / Critical reach any sink.
    // Set ExecutionLogLevel=Debug    → everything flows through.
    var minimumLevel = Warewolf.Execution.Lightweight.Logging.ExecutionLogLevel.Read();

    var elasticOptions = enableElastic && File.Exists(elasticsearchSettingsPath)
        ? ElasticsearchLoggingOptions.FromBiteFile(elasticsearchSettingsPath)
        : null;


    var host = new HostBuilder()
        .ConfigureWarewolf(config)
        .ConfigureServices(services =>
         {
             services.AddSingleton<IExecutionLogger>(sp =>
             {
                 var loggers = new List<IExecutionLogger>();

                 // AzureExecutionLogger — MEL sink (App Insights / console)
                 if (enableConsole)
                     loggers.Add(new AzureExecutionLogger(
                         sp.GetRequiredService<ILogger<AzureExecutionLogger>>(),
                         minimumLevel));

                 // ElasticsearchExecutionLogger — Elasticsearch sink
                 if (elasticOptions is not null)
                     loggers.Add(new ElasticsearchExecutionLogger(elasticOptions, minimumLevel));

                 return new CompositeExecutionLogger(loggers);
             });

         })
        .Build();

    await StartupOrchestrator.RunStartupAsync(host, config);

    // Route every Dev2Logger.X() call to the IExecutionLogger sinks (Azure / Elasticsearch).
    // Must be set after RunStartupAsync so Config.Server is initialised before any Dev2Logger call.
    Dev2.Common.Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(
        host.Services.GetRequiredService<IExecutionLogger>());

    // Also set the correlation prefix provider so that Dev2Logger's own log4net path
    // (when ExternalSink is bypassed) includes instance/invocation correlation.
    Dev2.Common.Dev2Logger.CorrelationPrefixProvider =
        Warewolf.Execution.Lightweight.Logging.ExecutionLoggerBase.GetCorrelationPrefixStatic;

    var startupLogger = host.Services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup");

    startupLogger.LogInformation("Loading \"Warewolf License.secureconfig\"...");
    var licenseProvider = SubscriptionProvider.Instance;
    if (licenseProvider.IsLicensed)
    {
        startupLogger.LogInformation("\"Warewolf License.secureconfig\" loaded successfully. Server is licensed (Status: {Status}).", licenseProvider.Status);
    }
    else
    {
        startupLogger.LogWarning("\"Warewolf License.secureconfig\" loaded. Server is not licensed (Status: {Status}, StopExecutions: {StopExecutions}).", licenseProvider.Status, licenseProvider.StopExecutions);
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    // Fatal cold-start failure — write to stderr so the Azure Functions runtime
    // captures it regardless of whether the logging pipeline is available.
    await Console.Error.WriteLineAsync(
        $"[FATAL] Host terminated unexpectedly at {DateTimeOffset.UtcNow:O}: {ex}");
    throw;
}
