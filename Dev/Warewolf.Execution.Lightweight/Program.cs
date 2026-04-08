using Microsoft.Extensions.Hosting;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Infrastructure;

// ── Environment configuration ─────────────────────────────────────────────────
var workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
    ?? Path.Combine(AppContext.BaseDirectory, "Resources");

// Set AZURE_KEYVAULT_NAME to enable AES-256-GCM decryption of .bite source files.
// Leave unset for local development (plain or DPAPI-encrypted connection strings).
var vaultName         = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_NAME");
var secretName        = Environment.GetEnvironmentVariable("KEYVAULT_SECRET_NAME") ?? "dp-keyring-v1";
var encryptionEnabled = !string.IsNullOrWhiteSpace(vaultName);

// ── Host construction ─────────────────────────────────────────────────────────
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddCoreServices(workflowsDirectory);

        if (encryptionEnabled)
            services.AddKeyVaultEncryption($"https://{vaultName}.vault.azure.net/", secretName);
    })
    .Build();

// ── Security: Key Vault initialisation (one Key Vault op per cold start) ───────
if (encryptionEnabled)
{
    var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                     ?? Environment.MachineName;

    await host.InitializeKeyVaultAsync(instanceId);
}

// ── Pre-load workflow index so the first HTTP request has no file-system cost ──
WorkflowIndex.Instance.WarmUp(workflowsDirectory);

await host.RunAsync();


