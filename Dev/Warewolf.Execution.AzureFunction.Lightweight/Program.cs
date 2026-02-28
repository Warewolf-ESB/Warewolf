using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Warewolf.Execution.AzureFunction.Lightweight;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
    })
    .Build();

host.Run();
