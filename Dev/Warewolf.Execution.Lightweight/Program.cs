using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Logging;

var workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
    ?? Path.Combine(AppContext.BaseDirectory, "Resources");

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();
        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
        services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));
    })
    .Build();

host.Run();
