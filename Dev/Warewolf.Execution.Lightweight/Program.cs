using Dev2;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStack.Text;
using System.Collections;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Logging;

var workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
    ?? Path.Combine(AppContext.BaseDirectory, "Resources");

static bool IsEnabled(string key) =>
    string.Equals(Environment.GetEnvironmentVariable(key), "true", StringComparison.OrdinalIgnoreCase);

var enableConsole = IsEnabled("ENABLECONSOLELOGGING");
var enableElastic = IsEnabled("ENABLEELASTICSEARCHLOGGING");
var elasticsearchSettingsPath = Path.Combine(AppContext.BaseDirectory, "Settings", "ElasticsearchLoggingSource.bite");

var elasticOptions = enableElastic && File.Exists(elasticsearchSettingsPath)
    ? ElasticsearchLoggingOptions.FromBiteFile(elasticsearchSettingsPath)
    : null;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
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

        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
        services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));
    })
    .Build();

// Pre-load the workflow index so the first HTTP request pays no file-system cost.
WorkflowIndex.Instance.WarmUp(workflowsDirectory);

host.Run();
