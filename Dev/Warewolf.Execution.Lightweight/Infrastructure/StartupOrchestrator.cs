/*
 *  Warewolf - Once bitten, there's no goingback
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Orchestrates the post-<see cref="IHost.Build"/> startup sequence:
/// <list type="number">
///   <item>Key Vault initialisation (when encryption is enabled).</item>
///   <item>Workflow index warm-up (pre-loads the O(1) lookup table).</item>
/// </list>
///
/// Both phases are individually guarded with structured error handling so that
/// a failure in either phase produces a meaningful, structured log entry before
/// the process terminates.
/// </summary>
internal static class StartupOrchestrator
{
    /// <summary>
    /// Executes all startup phases in order.  Throws on the first unrecoverable
    /// failure so the Azure Functions host refuses to accept traffic.
    /// </summary>
    /// <param name="host">The fully built <see cref="IHost"/>.</param>
    /// <param name="config">Immutable environment configuration snapshot.</param>
    internal static async Task RunStartupAsync(IHost host, HostEnvironmentConfig config)
    {
        var logger = host.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(StartupOrchestrator));

        await InitializeEncryptionAsync(host, config, logger);
        WarmUpWorkflowIndex(config, logger);
    }

    // ── Private helpers ──────────────────────────────────────────────────────────

    static async Task InitializeEncryptionAsync(
        IHost                 host,
        HostEnvironmentConfig config,
        ILogger               logger)
    {
        if (!config.EncryptionEnabled)
            return;

        try
        {
            await host.InitializeKeyVaultAsync(config).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var (category, guidance) = ClassifyKeyVaultException(ex, config);

            if (!config.SkipFailureToRetrieveSecret)
            {
                logger.LogCritical(ex,
                    "Startup | Phase=KeyVaultInit | Status=Failed | Category={Category} | " +
                    "VaultName={VaultName} | SecretName={SecretName} | InstanceId={InstanceId} | " +
                    "Guidance={Guidance} | " +
                    "To bypass this failure and start with degraded decryption, " +
                    "set environment variable SkipFailureToRetrieveSecret=true (NOT recommended for production).",
                    category, config.VaultName, config.SecretName, config.InstanceId, guidance);

                throw; // Fail fast — host cannot serve encrypted sources without the AES key.
            }

            // SkipFailureToRetrieveSecret=true: allow host to start in degraded mode.
            // Every encrypted .bite source will throw at execution time instead of startup.
            logger.LogWarning(ex,
                "Startup | Phase=KeyVaultInit | Status=Degraded | Category={Category} | " +
                "VaultName={VaultName} | SecretName={SecretName} | InstanceId={InstanceId} | " +
                "Guidance={Guidance} | " +
                "SkipFailureToRetrieveSecret=true — host is starting WITHOUT the AES decryption key. " +
                "All workflows that read encrypted sources (connection strings, credentials) " +
                "will FAIL at execution time with a decryption error. " +
                "Only unencrypted workflows will execute successfully. " +
                "Resolve the Key Vault connectivity issue and restart to restore full functionality.",
                category, config.VaultName, config.SecretName, config.InstanceId, guidance);
        }
    }

    /// <summary>
    /// Maps a Key Vault exception to a short category tag and a human-readable
    /// remediation hint so log entries are actionable without reading stack traces.
    /// </summary>
    static (string Category, string Guidance) ClassifyKeyVaultException(
        Exception             ex,
        HostEnvironmentConfig config)
        => ex switch
        {
            AuthenticationFailedException =>
                ("AuthenticationFailed",
                 "The host could not authenticate to Azure Key Vault. " +
                 "In Azure: verify the Function App's System-Assigned Managed Identity is enabled " +
                 $"and has 'Key Vault Secrets User' role on vault '{config.VaultName}'. " +
                 "Locally: run 'az login' or sign in to Visual Studio with an account that has vault access."),

            Azure.RequestFailedException rfe when rfe.Status == 404 =>
                ("SecretNotFound",
                 $"Secret '{config.SecretName}' was not found in vault '{config.VaultName}'. " +
                 "Verify the secret name matches KEYVAULT_SECRET_NAME and that the secret exists and is enabled."),

            Azure.RequestFailedException rfe when rfe.Status == 403 =>
                ("AccessDenied",
                 $"Access to secret '{config.SecretName}' in vault '{config.VaultName}' was denied (HTTP 403). " +
                 "Grant the Function App's Managed Identity the 'Key Vault Secrets User' role " +
                 "on the vault or on the specific secret."),

            Azure.RequestFailedException rfe when rfe.Status is >= 500 and <= 599 =>
                ("KeyVaultUnavailable",
                 $"Azure Key Vault '{config.VaultName}' returned HTTP {rfe.Status}. " +
                 "This is a transient service error. The Function App will recover on the next cold start."),

            Azure.RequestFailedException rfe =>
                ("KeyVaultRequestFailed",
                 $"Key Vault request failed with HTTP {rfe.Status}: {rfe.Message}. " +
                 $"Verify AZURE_KEYVAULT_NAME='{config.VaultName}' is correct and the vault is accessible."),

            InvalidOperationException =>
                ("InvalidKeyMaterial",
                 $"The secret '{config.SecretName}' was retrieved but its content is invalid or malformed. " +
                 "Re-run Encrypt-Config.ps1 -GenerateKeys to regenerate and store a valid key ring."),

            UriFormatException =>
                ("InvalidVaultUri",
                 $"The vault URI derived from AZURE_KEYVAULT_NAME='{config.VaultName}' is malformed. " +
                 "Ensure the value contains only the vault name, not a full URI."),

            _ =>
                ("UnexpectedError",
                 $"An unexpected error occurred: {ex.GetType().Name}. " +
                 "Check the full exception details above for diagnostics.")
        };

    static void WarmUpWorkflowIndex(HostEnvironmentConfig config, ILogger logger)
    {
        try
        {
            WorkflowIndex.Instance.WarmUp(config.WorkflowsDirectory);
            logger.LogInformation(
                "Startup | Phase=WorkflowIndexWarmUp | Status=Completed | " +
                "Directory={WorkflowsDirectory}", config.WorkflowsDirectory);
        }
        catch (Exception ex)
        {
            // Warm-up failure is non-fatal: WorkflowIndex falls back to
            // disk-based resolution on the first HTTP request.
            logger.LogWarning(ex,
                "Startup | Phase=WorkflowIndexWarmUp | Status=Degraded | " +
                "Falling back to on-demand disk resolution. " +
                "Directory={WorkflowsDirectory}", config.WorkflowsDirectory);
        }
    }
}
