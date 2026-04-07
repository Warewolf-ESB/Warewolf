using Dev2;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Security.Encryption;

var workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
    ?? Path.Combine(AppContext.BaseDirectory, "Resources");

// ── Encryption configuration ──────────────────────────────────────────────────
// Set AZURE_KEYVAULT_NAME to enable AES-256-GCM decryption of .bite source files.
// Leave unset for local development (plain or DPAPI-encrypted connection strings).
var vaultName  = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_NAME");
var secretName = Environment.GetEnvironmentVariable("KEYVAULT_SECRET_NAME") ?? "dp-keyring-v1";
var encryptionEnabled = !string.IsNullOrWhiteSpace(vaultName);

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();
        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
        services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));

        if (encryptionEnabled)
        {
            var vaultUri = $"https://{vaultName}.vault.azure.net/";

            services.AddSingleton(sp => new KeyVaultSecretManager(
                vaultUri,
                secretName,
                sp.GetRequiredService<ILogger<KeyVaultSecretManager>>()));

            // FileDecryptionHelper is resolved AFTER InitializeAsync() is called below,
            // so GetKeyBytes() is always safe at construction time.
            services.AddSingleton(sp =>
                new FileDecryptionHelper(sp.GetRequiredService<KeyVaultSecretManager>()));

            services.AddSingleton(sp =>
                new AuditLogger(sp.GetRequiredService<ILogger<AuditLogger>>()));
        }
    })
    .Build();

// ── Security: Key Vault initialisation (one Key Vault op per cold start) ───────
if (encryptionEnabled)
{
    var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                     ?? Environment.MachineName;

    var secretManager = host.Services.GetRequiredService<KeyVaultSecretManager>();
    var audit         = host.Services.GetRequiredService<AuditLogger>();

    try
    {
        await secretManager.InitializeAsync();

        // Wire the AES hook into DpapiWrapper so existing DbSource loading
        // transparently decrypts WFAES::-prefixed connection strings without
        // any changes to DbSource or ResourceLoadProvider.
        var decryptionHelper = host.Services.GetRequiredService<FileDecryptionHelper>();
        DpapiWrapper.AesDecryptHook = decryptionHelper.DecryptConnectionString;

        audit.LogColdStart(instanceId, secretManager.KeyId);
    }
    catch (Exception ex)
    {
        audit.LogKeyVaultError(instanceId, ex);
        throw; // Fail fast: cannot serve requests without the AES key.
    }
}

// ── Pre-load the workflow index so the first HTTP request pays no file-system cost.
WorkflowIndex.Instance.WarmUp(workflowsDirectory);

await host.RunAsync();


