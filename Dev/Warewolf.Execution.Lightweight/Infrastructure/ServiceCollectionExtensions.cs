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
        HostEnvironmentConfig   config)
    {
        const string executionId = "ServiceCollectionExtensions-CoreServices";

        Dev2Logger.Info($"ServiceCollectionExtensions AddCoreServices starting. WorkflowsDirectory: {config.WorkflowsDirectory}", executionId);

        try
        {
            services.AddLogging();

            // Register the immutable environment config snapshot as a singleton so
            // any middleware or service can receive it via constructor injection.
            services.AddSingleton(config);

            services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
            services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(config.WorkflowsDirectory));

            // ── AUTH-09 / DI-06 ──────────────────────────────────────────────────
            // EntraAuthOptions is read from environment ONCE and shared as an
            // immutable DI singleton.  Required by BearerTokenPrincipalParser and
            // can be injected into health checks, audit, and tests.
            services.AddSingleton(_ => EntraAuthOptions.FromEnvironment());

            // ── DI-07 / MWA-05 / OBS-02 ──────────────────────────────────────────
            // AuditLogger is registered unconditionally so authorization middleware
            // can emit structured 401/403 audit events even when encryption is off.
            services.AddSingleton(new AuditLogger());

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
    /// <see cref="CompositeExecutionLogger"/> that fans out to all configured sinks.
    ///
    /// <para>Sink composition:</para>
    /// <list type="bullet">
    ///   <item><see cref="ConsoleExecutionLogger"/> — ALWAYS present (ensures no log is lost; feeds Azure Log Stream)</item>
    ///   <item><see cref="AzureExecutionLogger"/> — opt-in via <c>ENABLEAPPLICATIONINSIGHTS=true</c> (rich App Insights telemetry)</item>
    ///   <item><see cref="ElasticsearchExecutionLogger"/> — opt-in via <c>ENABLEELASTICSEARCHLOGGING=true</c></item>
    ///   <item><see cref="AuditExecutionLogger"/> — ALWAYS present (security audit events only)</item>
    /// </list>
    ///
    /// <para>
    /// The factory is deferred (runs on first resolution, not at registration time)
    /// so that <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook"/>
    /// is guaranteed to be wired before the encrypted
    /// <c>ElasticsearchLoggingSource.bite</c> file is read.
    /// </para>
    /// </summary>
    internal static IServiceCollection AddExecutionLogging(
        this IServiceCollection services,
        LoggingConfiguration loggingConfig)
    {
        const string executionId = "ServiceCollectionExtensions-ExecutionLogging";

        Dev2Logger.Debug($"ServiceCollectionExtensions AddExecutionLogging registering. EnableAI={loggingConfig.EnableApplicationInsights}, EnableElastic={loggingConfig.EnableElasticsearch}", executionId);

        services.AddSingleton(loggingConfig);

        services.AddSingleton<IExecutionLogger>(sp =>
        {
            var loggers = new List<IExecutionLogger>();

            // 1. ConsoleExecutionLogger — ALWAYS present (feeds stdout → Log Stream + AI traces)
            loggers.Add(new ConsoleExecutionLogger(
                sp.GetRequiredService<ILogger<ConsoleExecutionLogger>>(),
                loggingConfig.MinimumLevel));
            Dev2Logger.Debug("AddExecutionLogging added ConsoleExecutionLogger (always-on)", executionId);

            // 2. AzureExecutionLogger — opt-in (rich Application Insights telemetry)
            if (loggingConfig.EnableApplicationInsights)
            {
                loggers.Add(new AzureExecutionLogger(
                    sp.GetRequiredService<ILogger<AzureExecutionLogger>>(),
                    loggingConfig.MinimumLevel));
                Dev2Logger.Debug("AddExecutionLogging added AzureExecutionLogger", executionId);
            }

            // 3. ElasticsearchExecutionLogger — opt-in
            if (loggingConfig.EnableElasticsearch && File.Exists(loggingConfig.ElasticsearchSettingsPath))
            {
                var elasticOptions = ElasticsearchLoggingOptions.FromBiteFile(loggingConfig.ElasticsearchSettingsPath);
                elasticOptions.EnableDebugMode = loggingConfig.ElasticDebugMode;
                loggers.Add(new ElasticsearchExecutionLogger(elasticOptions, loggingConfig.MinimumLevel));
                Dev2Logger.Debug("AddExecutionLogging added ElasticsearchExecutionLogger", executionId);
            }

            // 4. AuditExecutionLogger — ALWAYS present (security events only)
            loggers.Add(new AuditExecutionLogger(
                sp.GetRequiredService<ILogger<AuditExecutionLogger>>()));
            Dev2Logger.Debug("AddExecutionLogging added AuditExecutionLogger (always-on)", executionId);

            Dev2Logger.Info($"AddExecutionLogging created CompositeExecutionLogger with {loggers.Count} sink(s)", executionId);
            return new CompositeExecutionLogger(loggers);
        });

        // Policy matcher — extracted matching strategy; swap implementation here to change behaviour.
        services.AddSingleton<IWorkflowPolicyMatcher, WorkflowPolicyMatcher>();

        // Route authorization registry — built once from [RequireWorkflowPermission] attributes.
        services.AddSingleton<IRouteAuthorizationRegistry>(
            _ => RouteAuthorizationRegistry.BuildFrom(typeof(WorkflowHttpFunction)));

        // Principal parsers — ordered chain (first parser that returns an authenticated
        // principal wins).  In development, DebugPrincipalParser is prepended so that
        // a fixed token from DEBUG_PRINCIPAL_TOKEN is used instead of requiring a live
        // EasyAuth-enabled App Service locally.
        if (config.IsDevelopment && !string.IsNullOrWhiteSpace(config.DebugPrincipalToken))
        {
            var token = config.DebugPrincipalToken;
            services.AddSingleton<IPrincipalParser>(sp =>
                new DebugPrincipalParser(
                    token,
                    sp.GetRequiredService<ILogger<DebugPrincipalParser>>()));
        }

        services.AddSingleton<IPrincipalParser, EasyAuthPrincipalParser>();
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
