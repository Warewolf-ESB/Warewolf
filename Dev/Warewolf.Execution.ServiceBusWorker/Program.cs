using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Warewolf.Execution.ServiceBusWorker;
using Warewolf.Execution.ServiceBusWorker.Auth;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        // appsettings.json holds non-secret defaults; environment / app settings override it.
        config
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // --- Options ---------------------------------------------------------------------
        services
            .AddOptions<WwExecutionOptions>()
            .Bind(context.Configuration.GetSection(WwExecutionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // --- Credential ------------------------------------------------------------------
        // DefaultAzureCredential: in Azure this resolves the (system- or user-assigned)
        // Managed Identity; locally it falls through to az CLI / Visual Studio / env vars.
        // A client-secret fallback can be switched on for local dev via config.
        services.AddSingleton<TokenCredential>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<WwExecutionOptions>>().Value;

            if (opt.UseClientSecretFallback)
            {
                if (string.IsNullOrWhiteSpace(opt.ClientId) || string.IsNullOrWhiteSpace(opt.ClientSecret))
                {
                    throw new InvalidOperationException(
                        "UseClientSecretFallback is true but ClientId/ClientSecret are not configured.");
                }

                return new ClientSecretCredential(opt.TenantId, opt.ClientId, opt.ClientSecret);
            }

            var credentialOptions = new DefaultAzureCredentialOptions
            {
                TenantId = string.IsNullOrWhiteSpace(opt.TenantId) ? null : opt.TenantId,
                ManagedIdentityClientId = string.IsNullOrWhiteSpace(opt.ManagedIdentityClientId)
                    ? null
                    : opt.ManagedIdentityClientId,
            };

            return new DefaultAzureCredential(credentialOptions);
        });

        // --- Auth handler ----------------------------------------------------------------
        services.AddTransient<WwExecutionTokenHandler>();

        // --- Typed HTTP client -----------------------------------------------------------
        // The token handler is attached to the client's pipeline, so every call made through
        // IWwExecutionClient is authenticated automatically.
        services
            .AddHttpClient<IWwExecutionClient, WwExecutionClient>((sp, client) =>
            {
                var opt = sp.GetRequiredService<IOptions<WwExecutionOptions>>().Value;
                client.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(100);
            })
            .AddHttpMessageHandler<WwExecutionTokenHandler>();
    })
    .Build();

await host.RunAsync();
