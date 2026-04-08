using Dev2;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Logging;

var workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
    ?? Path.Combine(AppContext.BaseDirectory, "Resources");

// ?? Logging flags (set in local.settings.json / Azure App Settings) ?????????
static bool IsEnabled(string key) =>
    string.Equals(Environment.GetEnvironmentVariable(key), "true", StringComparison.OrdinalIgnoreCase);

var enableConsole = IsEnabled("EnableConsoleLogging");
var enableElastic = IsEnabled("EnableElasticSearchLogging");

// Resolve Elasticsearch options from the .bite source file when the flag is on.
var elasticsearchSettingsPath = Path.Combine(AppContext.BaseDirectory, "Settings", "ElasticsearchLoggingSource.bite");
var elasticOptions = enableElastic && File.Exists(elasticsearchSettingsPath)
    ? ElasticsearchLoggingOptions.FromBiteFile(elasticsearchSettingsPath)
    : null;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // IExecutionLogger registration driven by the two flags:
        //   EnableConsoleLogging=true        ? AzureExecutionLogger  (App Insights / console)
        //   EnableElasticSearchLogging=true  ? ElasticsearchExecutionLogger (.bite file)
        //   Both true                        ? CompositeExecutionLogger (both sinks)
        //   Neither                          ? AzureExecutionLogger  (safe default)
        services.AddSingleton<IExecutionLogger>(sp =>
        {
            var loggers = new List<IExecutionLogger>();

            if (enableConsole || loggers.Count == 0)
                loggers.Add(new AzureExecutionLogger(
                    sp.GetRequiredService<ILogger<AzureExecutionLogger>>()));

            if (elasticOptions is not null)
                loggers.Add(new ElasticsearchExecutionLogger(elasticOptions));

            return loggers.Count == 1
                ? loggers[0]
                : new CompositeExecutionLogger(loggers);
        });

        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
        services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));
    })
    .Build();

// Pre-load the workflow index so the first HTTP request pays no file-system cost.
WorkflowIndex.Instance.WarmUp(workflowsDirectory);

host.Run();

