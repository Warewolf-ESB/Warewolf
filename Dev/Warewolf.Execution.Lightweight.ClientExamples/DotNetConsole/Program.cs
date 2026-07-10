using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WwExecutionClient;
using WwExecutionClient.Auth;

// ---------------------------------------------------------------------------
// Warewolf Execution Engine - .NET reference client (pure HTTP client).
//
// Demonstrates four Entra ID token-acquisition flows (client-credentials,
// device-code, interactive, managed-identity) plus silent cache reuse, a
// persistent MSAL token cache, automatic bearer-token injection via a
// DelegatingHandler, and typed calls to /public, /secure and /services.
// ---------------------------------------------------------------------------

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        config.AddEnvironmentVariables(prefix: "WWEXEC_");
        config.AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        // Bind + validate options.
        services.AddOptions<WwExecutionClientOptions>()
            .Bind(context.Configuration.GetSection(WwExecutionClientOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Token acquisition + the bearer-injecting handler (singletons share the persistent MSAL cache).
        services.AddSingleton<TokenAcquirer>();
        services.AddSingleton<BearerTokenHandler>();

        // Typed HTTP client wired with the bearer handler; BaseAddress from options.
        services.AddHttpClient<WwExecutionService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<WwExecutionClientOptions>>().Value;
                client.BaseAddress = new Uri(EnsureTrailingSlash(options.BaseUrl));
                client.Timeout = TimeSpan.FromSeconds(100);
            })
            .AddHttpMessageHandler(sp => sp.GetRequiredService<BearerTokenHandler>());
    })
    .Build();

await RunDemoAsync(host.Services);
return;

static string EnsureTrailingSlash(string url) => url.EndsWith('/') ? url : url + "/";

// ---------------------------------------------------------------------------
//  Interactive demo menu
// ---------------------------------------------------------------------------
static async Task RunDemoAsync(IServiceProvider sp)
{
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Demo");
    var options = sp.GetRequiredService<IOptions<WwExecutionClientOptions>>().Value;
    var handler = sp.GetRequiredService<BearerTokenHandler>();
    var service = sp.GetRequiredService<WwExecutionService>();

    Console.WriteLine("=====================================================");
    Console.WriteLine(" Warewolf Execution Engine - .NET reference client");
    Console.WriteLine("=====================================================");
    Console.WriteLine($"Engine base URL : {options.BaseUrl}");
    Console.WriteLine($"Authority       : {options.Authority}");
    Console.WriteLine($"Audience        : {options.ResourceAppIdUri}");
    Console.WriteLine($"Sample workflow : {options.SampleWorkflow}");
    Console.WriteLine();

    while (true)
    {
        Console.WriteLine("Choose an auth flow for SECURE/SERVICES calls:");
        Console.WriteLine("  1) Interactive (browser popup)        - delegated user");
        Console.WriteLine("  2) Device code (headless / CLI)       - delegated user");
        Console.WriteLine("  3) Client credentials (daemon)        - app-only, needs ClientSecret");
        Console.WriteLine("  4) Managed identity / DefaultAzureCredential - app-only in Azure");
        Console.WriteLine("  5) Call PUBLIC workflow (anonymous, no token)");
        Console.WriteLine("  6) Call discovery /apis.json");
        Console.WriteLine("  0) Exit");
        Console.Write("> ");

        var choice = Console.ReadLine()?.Trim();
        Console.WriteLine();

        try
        {
            switch (choice)
            {
                case "1":
                    handler.Flow = AuthFlow.Interactive;
                    await CallHelloWorldSecureAsync(service, options);
                    break;
                case "2":
                    handler.Flow = AuthFlow.DeviceCode;
                    await CallHelloWorldSecureAsync(service, options);
                    break;
                case "3":
                    handler.Flow = AuthFlow.ClientCredentials;
                    await CallHelloWorldSecureAsync(service, options);
                    break;
                case "4":
                    handler.Flow = AuthFlow.ManagedIdentity;
                    await CallHelloWorldSecureAsync(service, options);
                    break;
                case "5":
                    await CallHelloWorldPublicAsync(service, options);
                    break;
                case "6":
                    var apis = await service.GetApisAsync();
                    Console.WriteLine(Truncate(apis));
                    break;
                case "0":
                case null:
                    Console.WriteLine("Bye.");
                    return;
                default:
                    Console.WriteLine("Unrecognised choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            // Keep the demo alive across auth/HTTP errors; see README troubleshooting table.
            logger.LogError(ex, "Operation failed.");
            Console.WriteLine($"ERROR: {ex.Message}");
        }

        Console.WriteLine();
    }
}

static async Task CallHelloWorldSecureAsync(WwExecutionService service, WwExecutionClientOptions options)
{
    var query = new Dictionary<string, string> { ["Name"] = "Alice" };
    Console.WriteLine($"Calling GET /secure/{options.SampleWorkflow}.json?Name=Alice ...");
    var result = await service.GetSecureAsync(options.SampleWorkflow, query);
    Console.WriteLine("Response:");
    Console.WriteLine(Truncate(result));
}

static async Task CallHelloWorldPublicAsync(WwExecutionService service, WwExecutionClientOptions options)
{
    var query = new Dictionary<string, string> { ["Name"] = "Alice" };
    Console.WriteLine($"Calling GET /public/{options.SampleWorkflow}.json?Name=Alice ...");
    var result = await service.GetPublicAsync(options.SampleWorkflow, query);
    Console.WriteLine("Response:");
    Console.WriteLine(Truncate(result));
}

static string Truncate(string value, int max = 4000) =>
    value.Length <= max ? value : value[..max] + $"... [{value.Length - max} more chars]";
