using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WwExecutionCaller;
using WwExecutionCaller.Auth;

// -----------------------------------------------------------------------------
// Warewolf Execution Engine — downstream caller (Azure Functions v4 isolated worker)
//
// DI wiring:
//   • Options       — WwExecutionCallerOptions bound from the "WwExecution" section,
//                     validated on start (DataAnnotations).
//   • TokenCredential — DefaultAzureCredential (Managed Identity in Azure, az CLI locally).
//   • WwExecutionTokenHandler — DelegatingHandler that acquires/caches/refreshes the
//                     app-only token and injects Authorization (+ x-functions-key for /services).
//   • Typed client  — AddHttpClient<IWwExecutionDownstreamService, WwExecutionDownstreamService>
//                     with the engine base address and the token handler in its pipeline.
// -----------------------------------------------------------------------------

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // --- Options binding + start-up validation ------------------------------------
        services
            .AddOptions<WwExecutionCallerOptions>()
            .Bind(configuration.GetSection(WwExecutionCallerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // --- Application Insights (idiomatic for Functions; no-op without a connection) -
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // --- Token credential: Managed Identity in Azure, az CLI locally ----------------
        // A user-assigned MI client id may be supplied via AZURE_CLIENT_ID.
        services.AddSingleton<TokenCredential>(_ =>
        {
            var managedIdentityClientId = configuration["AZURE_CLIENT_ID"];
            var credentialOptions = new DefaultAzureCredentialOptions();
            if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
            {
                credentialOptions.ManagedIdentityClientId = managedIdentityClientId;
            }
            return new DefaultAzureCredential(credentialOptions);
        });

        // --- Token-injecting DelegatingHandler ------------------------------------------
        services.AddTransient<WwExecutionTokenHandler>();

        // --- Typed HttpClient for the downstream engine ---------------------------------
        services
            .AddHttpClient<IWwExecutionDownstreamService, WwExecutionDownstreamService>((sp, client) =>
            {
                var options = sp.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<WwExecutionCallerOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(100);
            })
            .AddHttpMessageHandler<WwExecutionTokenHandler>();
    })
    .Build();

await host.RunAsync();
