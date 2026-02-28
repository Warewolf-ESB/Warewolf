using Dev2.Runtime.ESB.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

try
{
    var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
    Console.WriteLine($"[AzureFunc] Startup: BaseDirectory='{AppContext.BaseDirectory}'");
    Console.WriteLine($"[AzureFunc] Startup: ResourceBasePath='{resourcePath}'");
    Console.WriteLine($"[AzureFunc] Startup: Resources dir exists={Directory.Exists(resourcePath)}");

    AzureFunctionWorkflowRunner.ResourceBasePath = resourcePath;

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
