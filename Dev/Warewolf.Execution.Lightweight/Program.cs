using Dev2;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Logging;

// Pre-register a minimal ResourceCatalog before any workflow execution.
//
// Without this, the first ServiceExecutionAbstract constructor call accesses
// ResourceCatalog.Instance which — finding nothing in CustomContainer — falls
// through to new ResourceCatalog(EsbManagementServiceLocator.GetServices()).
// EsbManagementServiceLocator.GetServices() uses SpookyAction to scan the
// IEsbManagementEndpoint assembly, instantiate ~100 management-service classes
// via Activator.CreateInstance, and call CreateServiceEntry().Compile() on every
// one of them.  All those DynamicService objects are stored in the static
// ResourceLoadProvider.ManagementServices dictionary and never released, which
// is the primary cause of the 520 MB resident-set that persists after the first
// function invocation.
//
// ResourceCatalog.Instance checks CustomContainer.Get<IResourceCatalog>() first.
// Registering here means it returns our lightweight instance immediately and the
// EsbManagementServiceLocator path is never reached.
CustomContainer.Register<IResourceCatalog>(new ResourceCatalog());

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

// Pre-load the workflow index so the first HTTP request pays no file-system cost.
WorkflowIndex.Instance.WarmUp(workflowsDirectory);

host.Run();

