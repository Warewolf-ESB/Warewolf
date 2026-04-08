/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddLogging();
        services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();
        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
        services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));
        return services;
    }

    /// <summary>
    /// Registers Key Vault–backed AES-256-GCM encryption services.
    /// Only called when <c>AZURE_KEYVAULT_NAME</c> is set.
    /// </summary>
    internal static IServiceCollection AddKeyVaultEncryption(
        this IServiceCollection services,
        string vaultUri,
        string secretName)
    {
        services.AddSingleton(sp => new KeyVaultSecretManager(
            vaultUri,
            secretName,
            sp.GetRequiredService<ILogger<KeyVaultSecretManager>>()));

        // FileDecryptionHelper is resolved AFTER InitializeAsync() completes,
        // so GetKeyBytes() is always safe at construction time.
        services.AddSingleton(sp =>
            new FileDecryptionHelper(sp.GetRequiredService<KeyVaultSecretManager>()));

        services.AddSingleton(sp =>
            new AuditLogger(sp.GetRequiredService<ILogger<AuditLogger>>()));

        return services;
    }
}
