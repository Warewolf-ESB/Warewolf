using Dev2;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Licensing;

var workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
    ?? Path.Combine(AppContext.BaseDirectory, "Resources");

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();
        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
        services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));
        services.AddSingleton<IWarewolfLicense, WarewolfLicense>();
    })
    .Build();

// Pre-load the workflow index so the first HTTP request pays no file-system cost.
WorkflowIndex.Instance.WarmUp(workflowsDirectory);

var startupLogger = host.Services.GetRequiredService<ILogger<Program>>();
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

host.Run();

