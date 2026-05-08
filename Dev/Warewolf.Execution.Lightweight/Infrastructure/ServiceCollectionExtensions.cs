/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> that register
/// core application services and, optionally, Key Vault–backed encryption
/// services.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core workflow execution services that are always needed,
    /// regardless of whether encryption is enabled.
    /// </summary>
    internal static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        string workflowsDirectory)
    {
        const string executionId = "ServiceCollectionExtensions-CoreServices";

        Dev2Logger.Info($"ServiceCollectionExtensions AddCoreServices starting. WorkflowsDirectory: {workflowsDirectory}", executionId);

        try
        {
            services.AddLogging();
            services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();
            services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
            services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));

            // Auth policy loader — builds WorkflowAuthPolicy from secure.config
            // WindowsGroupPermissions entries at startup.
            services.AddSingleton<IWorkflowAuthPolicyLoader, WorkflowAuthPolicyLoader>();

            Dev2Logger.Info("ServiceCollectionExtensions AddCoreServices completed successfully", executionId);
            return services;
        }
        catch (Exception ex)
        {
            Dev2Logger.Error("ServiceCollectionExtensions AddCoreServices failed", ex, executionId);
            throw;
        }
    }

    /// <summary>
    /// Registers Key Vault–backed AES-256-GCM encryption services using the
    /// vault coordinates provided by <paramref name="config"/>.
    /// Only called when <see cref="HostEnvironmentConfig.EncryptionEnabled"/> is <c>true</c>.
    /// </summary>
    internal static IServiceCollection AddKeyVaultEncryption(
        this IServiceCollection services,
        HostEnvironmentConfig   config)
    {
        const string executionId = "ServiceCollectionExtensions-Encryption";

        Dev2Logger.Info($"ServiceCollectionExtensions AddKeyVaultEncryption starting. VaultName: {config.VaultName}, SecretName: {config.SecretName}, IsDevelopment: {config.IsDevelopment}", executionId);

        try
        {
            var useDebugBypass = config.IsDevelopment && config.DebugKeyVaultSecret is not null;

            if (useDebugBypass)
            {
                Dev2Logger.Warn("ServiceCollectionExtensions using DEBUG KeyVault bypass (DebugKeyVaultSecret is set)", executionId);
            }

            services.AddSingleton(sp => new KeyVaultSecretManager(
                config.VaultUri,
                config.SecretName,
                useDebugBypass
                    ? null
                    : KeyVaultCredentialFactory.Create(config.CredentialOptions),
                sp.GetRequiredService<ILogger<KeyVaultSecretManager>>(),
                useDebugBypass ? config.DebugKeyVaultSecret : null));

            // FileDecryptionHelper is resolved AFTER InitializeAsync() completes,
            // so GetKeyBytes() is always safe at construction time.
            services.AddSingleton(sp =>
                new FileDecryptionHelper(sp.GetRequiredService<KeyVaultSecretManager>()));

            services.AddSingleton(sp =>
                new AuditLogger(sp.GetRequiredService<ILogger<AuditLogger>>()));

            Dev2Logger.Info("ServiceCollectionExtensions AddKeyVaultEncryption completed successfully", executionId);
            return services;
        }
        catch (Exception ex)
        {
            Dev2Logger.Error("ServiceCollectionExtensions AddKeyVaultEncryption failed", ex, executionId);
            throw;
        }
    }
}
