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

    var enableConsole = IsEnabled("ENABLECONSOLELOGGING");
    var enableElastic = IsEnabled("ENABLEELASTICSEARCHLOGGING");
    var elasticsearchSettingsPath = Path.Combine(AppContext.BaseDirectory, "Settings", "ElasticsearchLoggingSource.bite");

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

                 if (enableConsole)
                     loggers.Add(new AzureExecutionLogger(
                         sp.GetRequiredService<ILogger<AzureExecutionLogger>>()));

                 if (elasticOptions is not null)
                     loggers.Add(new ElasticsearchExecutionLogger(elasticOptions));

                 return new CompositeExecutionLogger(loggers);
             });

         })
        .Build();

    await StartupOrchestrator.RunStartupAsync(host, config);

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
