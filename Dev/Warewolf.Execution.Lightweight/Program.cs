using Microsoft.Extensions.Hosting;
using Warewolf.Execution.Lightweight.Infrastructure;

try
{
    var config = HostEnvironmentConfig.Load();

    var host = new HostBuilder()
        .ConfigureWarewolf(config)
        .Build();

    await StartupOrchestrator.RunStartupAsync(host, config);
    
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
