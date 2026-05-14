/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;
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

		services.AddLogging();
        services.AddSingleton<IExecutionLogger, AzureExecutionLogger>();
        services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
        services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));

        Dev2Logger.Info($"ServiceCollectionExtensions AddCoreServices starting. WorkflowsDirectory: {workflowsDirectory}", executionId);

        try
        {
            services.AddLogging();
            services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
            services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(workflowsDirectory));

            // ── AUTH-09 / DI-06 ──────────────────────────────────────────────────
            // EntraAuthOptions is read from environment ONCE and shared as an
            // immutable DI singleton.  Required by BearerTokenPrincipalParser and
            // can be injected into health checks, audit, and tests.
            services.AddSingleton(_ => EntraAuthOptions.FromEnvironment());

            // ── DI-07 / MWA-05 / OBS-02 ──────────────────────────────────────────
            // AuditLogger is registered unconditionally so authorization middleware
            // can emit structured 401/403 audit events even when encryption is off.
            services.AddSingleton<AuditLogger>();

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
    /// Registers <see cref="IExecutionLogger"/> as a singleton
    /// <see cref="CompositeExecutionLogger"/> that fans out to Azure (MEL) and/or
    /// Elasticsearch sinks depending on environment variables.
    ///
    /// <para>
    /// The factory is deferred (runs on first resolution, not at registration time)
    /// so that <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook"/>
    /// is guaranteed to be wired before the encrypted
    /// <c>ElasticsearchLoggingSource.bite</c> file is read.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="enableConsole">Whether the Azure/console logger sink is enabled.</param>
    /// <param name="enableElastic">Whether the Elasticsearch logger sink is enabled.</param>
    /// <param name="elasticsearchSettingsPath">Absolute path to the Elasticsearch <c>.bite</c> config file.</param>
    /// <param name="minimumLevel">Minimum log level gate shared by all sinks.</param>
    internal static IServiceCollection AddExecutionLogging(
        this IServiceCollection services,
        bool enableConsole,
        bool enableElastic,
        string elasticsearchSettingsPath,
        Dev2.Data.Interfaces.Enums.LogLevel minimumLevel)
    {
        const string executionId = "ServiceCollectionExtensions-ExecutionLogging";

        Dev2Logger.Debug($"ServiceCollectionExtensions AddExecutionLogging registering. EnableConsole={enableConsole}, EnableElastic={enableElastic}", executionId);

        services.AddSingleton<IExecutionLogger>(sp =>
        {
            var loggers = new List<IExecutionLogger>();

            // AzureExecutionLogger — MEL sink (App Insights / console)
            if (enableConsole)
            {
                loggers.Add(new AzureExecutionLogger(
                    sp.GetRequiredService<ILogger<AzureExecutionLogger>>(),
                    minimumLevel));
                Dev2Logger.Debug("AddExecutionLogging added AzureExecutionLogger", executionId);
            }

            // ElasticsearchExecutionLogger — Elasticsearch sink
            // Resolved here (inside the factory) so the AES decrypt hook
            // from KeyVaultStartupExtensions is already wired by the time
            // we read the potentially-encrypted .bite file.
            if (enableElastic && File.Exists(elasticsearchSettingsPath))
            {
                var elasticOptions = ElasticsearchLoggingOptions.FromBiteFile(elasticsearchSettingsPath);
                loggers.Add(new ElasticsearchExecutionLogger(elasticOptions, minimumLevel));
                Dev2Logger.Debug("AddExecutionLogging added ElasticsearchExecutionLogger", executionId);
            }

            Dev2Logger.Info($"AddExecutionLogging created CompositeExecutionLogger with {loggers.Count} sink(s)", executionId);
            return new CompositeExecutionLogger(loggers);
        });

        // Policy matcher — extracted matching strategy; swap implementation here to change behaviour.
        services.AddSingleton<IWorkflowPolicyMatcher, WorkflowPolicyMatcher>();

        // Route authorization registry — built once from [RequireWorkflowPermission] attributes.
        services.AddSingleton<IRouteAuthorizationRegistry>(
            _ => RouteAuthorizationRegistry.BuildFrom(typeof(WorkflowHttpFunction)));

        // Principal parsers — ordered chain.
        //   1. EasyAuth        — X-MS-CLIENT-PRINCIPAL header (Azure App Service auth)
        //   2. WarewolfHmacJwt — Authorization: Bearer with HMAC-SHA256 signature
        //                        using SecretKey from secure.config (Warewolf server)
        //   3. BearerToken     — Authorization: Bearer with RS256 signature against
        //                        Entra OIDC metadata
        services.AddSingleton<IPrincipalParser, EasyAuthPrincipalParser>();
        services.AddSingleton<IPrincipalParser, WarewolfHmacJwtPrincipalParser>();
        services.AddSingleton<IPrincipalParser, BearerTokenPrincipalParser>();

        // (POL-08) Hot-reload secure.config + policy loader at runtime.
        services.AddHostedService<SecureConfigWatcher>();

        // (OBS-06) Startup health check — emits a single warning at startup
        // when bearer-token validation is not configured.  Zero per-request cost.
        services.AddHostedService<EntraAuthHealthCheck>();

        return services;
    }

    /// <summary>
    /// Registers Key Vault–backed AES-256-GCM encryption services using the
    /// vault coordinates provided by <paramref name="config"/>.
    /// Only called when <see cref="HostEnvironmentConfig.EncryptionEnabled"/> is <c>true</c>.
    /// </summary>
    internal static IServiceCollection AddKeyVaultEncryption(
        this IServiceCollection services,
        HostEnvironmentConfig config)
    {
        var useDebugBypass = config.IsDevelopment && config.DebugKeyVaultSecret is not null;
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

        // AuditLogger is registered globally in AddCoreServices (DI-07);
        // no per-encryption registration needed here.

        return services;
    }
}
