using Dev2.Diagnostics.Debug;
using Dev2.Common;
using Dev2.Runtime.ESB.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Warewolf.Execution.AzureFunctions;

try
{
    var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
    Console.WriteLine($"[AzureFunc] Startup: BaseDirectory='{AppContext.BaseDirectory}'");
    Console.WriteLine($"[AzureFunc] Startup: ResourceBasePath='{resourcePath}'");
    Console.WriteLine($"[AzureFunc] Startup: Resources dir exists={Directory.Exists(resourcePath)}");

    AzureFunctionWorkflowRunner.ResourceBasePath = resourcePath;

    // Register an Azure SignalR debug writer when a connection string is configured.
    // This enables workflow execution to stream debug states to all Studio clients
    // connected to the "esb" hub on the Azure SignalR Service, matching the same
    // "SendDebugState" messages that EsbHub sends in the regular web server.
    var signalRConnStr = Environment.GetEnvironmentVariable("AzureSignalRConnectionString");
    if (!string.IsNullOrWhiteSpace(signalRConnStr))
    {
        Console.WriteLine("[AzureFunc] Startup: AzureSignalRConnectionString present — registering debug writer");
        var debugWriter = AzureSignalRDebugWriter.CreateAsync(signalRConnStr).GetAwaiter().GetResult();
        DebugDispatcher.Instance.Add(GlobalConstants.ServerWorkspaceID, debugWriter);
        AzureFunctionWorkflowRunner.IsDebugEnabled = true;
        Console.WriteLine("[AzureFunc] Startup: Azure SignalR debug writer registered for hub 'esb'");
    }
    else
    {
        Console.WriteLine("[AzureFunc] Startup: AzureSignalRConnectionString not set — debug dispatch disabled");
    }

    var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults()
        .Build();

    Console.WriteLine("[AzureFunc] Startup: host built, calling Run()");
    host.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"[AzureFunc] STARTUP CRASH: {ex.GetType().FullName}: {ex.Message}");
    Console.WriteLine($"[AzureFunc] Stack: {ex.StackTrace}");
    for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
        Console.WriteLine($"[AzureFunc]   --> inner: {inner.GetType().FullName}: {inner.Message}");
    throw;
}
