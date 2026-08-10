/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.EngineJobProcessor;
using Warewolf.Execution.EngineJobProcessor.Services;
using Warewolf.Execution.Lightweight.Security;

/*
 * ExecutionEngineJobProcessor — dedicated Azure Function App.
 *
 * Cold-start sequence (mirrors the Execution Engine's ordering):
 *   1. Build the isolated-worker host + DI.
 *   2. Key Vault init (when configured) → wire DpapiWrapper.AesDecryptHook so the
 *      WFAES-encrypted Hangfire DbSource ConnectionString can be decrypted.
 *   3. PersistenceConfigLoader.Initialize() → hydrate Config.Persistence from
 *      Settings/persistencesettings.json + Settings/persistencesettingsdbsource.bite
 *      (fail-fast when enabled but broken — the processor must not run half-configured).
 *   4. host.RunAsync() — JobPoll + JobReaper timers take over.
 */

var settings = ProcessorSettings.FromEnvironment();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton(settings);
        services.AddSingleton<JobStorageProvider>();

        // Singleton HttpClient: socket-exhaustion-safe; per-request timeouts are
        // governed by EngineResumeClient's linked CancellationTokenSource.
        services.AddSingleton(new HttpClient());

        services.AddSingleton<IEngineResumeClient>(sp => new EngineResumeClient(
            sp.GetRequiredService<HttpClient>(),
            settings.AuthDisabled ? null : ProcessorStartup.CreateTokenCredential(),
            settings,
            sp.GetRequiredService<ILogger<EngineResumeClient>>()));
    })
    .Build();

await ProcessorStartup.InitializeAsync();

await host.RunAsync();

namespace Warewolf.Execution.EngineJobProcessor
{
    /// <summary>
    /// Cold-start initialisation shared by <c>Program</c>: Key Vault AES key +
    /// decryption hook (same app settings as the Execution Engine —
    /// <c>AZURE_KEYVAULT_NAME</c>, <c>KEYVAULT_SECRET_NAME</c>,
    /// <c>DEBUG_AZURE_KEYVAULT_SECRET</c>) and persistence-settings hydration.
    /// </summary>
    internal static class ProcessorStartup
    {
        const string VaultUriTemplate = "https://{0}.vault.azure.net/";
        const string DefaultSecretName = "dp-keyring-v1";

        internal static async Task InitializeAsync()
        {
            const string executionId = "ProcessorStartup";

            var vaultName = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_NAME");
            var secretName = Environment.GetEnvironmentVariable("KEYVAULT_SECRET_NAME") ?? DefaultSecretName;
            var debugSecret = Environment.GetEnvironmentVariable("DEBUG_AZURE_KEYVAULT_SECRET");

            if (!string.IsNullOrWhiteSpace(vaultName) || !string.IsNullOrWhiteSpace(debugSecret))
            {
                var secretManager = new KeyVaultSecretManager(
                    vaultUri: string.Format(VaultUriTemplate, vaultName ?? "debug-bypass"),
                    secretName: secretName,
                    credential: string.IsNullOrWhiteSpace(debugSecret) ? CreateTokenCredential() : null,
                    logger: Microsoft.Extensions.Logging.Abstractions.NullLogger<KeyVaultSecretManager>.Instance,
                    debugSecret: string.IsNullOrWhiteSpace(debugSecret) ? null : debugSecret);

                await secretManager.InitializeAsync().ConfigureAwait(false);

                Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook =
                    new FileDecryptionHelper(
                        secretManager,
                        Microsoft.Extensions.Logging.Abstractions.NullLogger<FileDecryptionHelper>.Instance)
                        .DecryptConnectionString;

                Dev2.Common.Dev2Logger.Info(
                    $"Startup | Phase=KeyVault | Status=Completed | KeyId={secretManager.KeyId} | AES decryption hook wired.",
                    executionId);
            }
            else
            {
                Dev2.Common.Dev2Logger.Warn(
                    "Startup | Phase=KeyVault | Status=Skipped | AZURE_KEYVAULT_NAME not set — " +
                    "the persistence DbSource ConnectionString must be plaintext (development only).",
                    executionId);
            }

            // Fail-fast when persistence is enabled but misconfigured (same policy as the engine).
            Warewolf.Execution.Lightweight.Infrastructure.PersistenceConfigLoader.Initialize();
        }

        /// <summary>
        /// Managed Identity in Azure; focused developer chain (env/az-cli/VS) locally —
        /// reused for BOTH Key Vault access and Execution Engine token acquisition
        /// (role <c>Warewolf_JobProcessor</c> via <c>%ENGINE_RESUME_SCOPE%</c>).
        /// </summary>
        internal static TokenCredential CreateTokenCredential()
        {
            var isDevelopment =
                string.Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);

            return KeyVaultCredentialFactory.Create(new KeyVaultCredentialOptions
            {
                IsDevelopment = isDevelopment,
                TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID"),
                ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
            });
        }
    }
}
