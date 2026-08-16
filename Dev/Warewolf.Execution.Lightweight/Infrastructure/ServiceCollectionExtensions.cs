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
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Licensing;

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

            // Per-execution usage telemetry (8438) — singleton emitter is injected
            // into WorkflowExecutor so each successful (or failed) workflow run
            // produces a row in the legacy UsageData SQL table via Warewolf.Usage.
            services.AddSingleton<IUsageEventEmitter, UsageEventEmitter>();

            services.AddSingleton<IWorkflowExecutor, WorkflowExecutor>();
            services.AddSingleton<IApisJsonGenerator>(_ => new ApisJsonGenerator(config.WorkflowsDirectory));

            // (8439) Chargebee-backed licence client used by LicensingHttpFunction
            // to serve /IsLicensed, /Subscriptions and /secure/Subscriptions.
            // Without this registration every call to those routes fails activation
            // with "Unable to resolve service for type 'IWarewolfLicense'" and the
            // Functions runtime returns 204 No Content (no body).  The concrete
            // WarewolfLicense exposes a parameterless ctor that builds its own
            // Subscription, so a plain singleton wiring is sufficient.
            services.AddSingleton<IWarewolfLicense, WarewolfLicense>();

            // ── AUTH-09 / DI-06 ──────────────────────────────────────────────────
            // EntraAuthOptions is read from environment ONCE and shared as an
            // immutable DI singleton.  Required by BearerTokenPrincipalParser and
            // can be injected into health checks, audit, and tests.
            services.AddSingleton(_ => EntraAuthOptions.FromEnvironment());

            // Dedicated, narrow "trigger-via-service-bus" Entra audience — a token minted
            // for the general HTTP audience must never validate against this one (spec
            // §4.2 step 2). Registered as its own singleton type rather than a second
            // keyed EntraAuthOptions instance (no keyed-service DI pattern in this project).
            services.AddSingleton(_ => ServiceBusEntraAuthOptions.FromEnvironment());
            services.AddSingleton(_ => ServiceBusTriggerOptions.FromEnvironment());

            // The Service Bus secure trigger's token validator MUST be a DI singleton, not
            // constructed per-invocation: EntraBearerTokenValidator caches Entra's OIDC
            // metadata/JWKS internally, and ServiceBusWorkflowTriggerFunction (unlike
            // IPrincipalParser's HTTP-path implementations) is not itself registered here,
            // so the Functions isolated-worker host resolves a new instance per invocation.
            // Without this singleton, a burst of concurrent Service Bus messages triggers
            // one cold OIDC-metadata fetch per message — reproduced by the ShovelBridge
            // 1000-message load test as widespread IDX20803/IDX20804 + InvalidToken
            // failures once the Function App scales out under load.
            services.AddSingleton(sp =>
                new EntraBearerTokenValidator(sp.GetRequiredService<ServiceBusEntraAuthOptions>()));

            // ── DI-07 / MWA-05 / OBS-02 ──────────────────────────────────────────
            // AuditLogger is registered unconditionally so authorization middleware
            // can emit structured 401/403 audit events even when encryption is off.
            services.AddSingleton<AuditLogger>();

            // Auth policy loader — builds WorkflowAuthPolicy from secure.config
            // WindowsGroupPermissions entries at startup.
            services.AddSingleton<IWorkflowAuthPolicyLoader, WorkflowAuthPolicyLoader>();

            // Default secret-reference resolver for the add_source MCP tool's "${NAME}"
            // placeholders — a local-dev fallback (this host's own environment variables).
            // AddKeyVaultEncryption overrides this with KeyVaultMcpSecretResolver whenever
            // Key Vault is configured (see IMcpSecretResolver for why the fallback matters).
            services.AddSingleton<Mcp.Secrets.IMcpSecretResolver, Mcp.Secrets.EnvironmentMcpSecretResolver>();

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

        Dev2Logger.Debug($"ServiceCollectionExtensions AddExecutionLogging registering. EnableAI={loggingConfig.RegisterApplicationInsightsSdk}, EnableElastic={loggingConfig.EnableElasticsearch}", executionId);

        services.AddSingleton(loggingConfig);

        services.AddSingleton<IExecutionLogger>(sp =>
        {
            var loggers = new List<IExecutionLogger>();

            // 1. General-purpose MEL logger — EXACTLY ONE of Console/Azure is added to
            //    avoid duplicate stdout AND Application Insights entries. Both loggers wrap
            //    ILogger<T>, which in the isolated worker broadcasts to EVERY registered MEL
            //    provider (Console + Application Insights); the category <T> only labels the
            //    entry, it does NOT select a provider. Adding both therefore emits everything
            //    twice, so the active sink is chosen by whether the AI SDK is registered.
            if (loggingConfig.RegisterApplicationInsightsSdk)
            {
                // AI SDK registered → AzureExecutionLogger. Its ILogger<AzureExecutionLogger>
                // reaches the Application Insights provider (primary sink, correct per-level
                // severity) AND the Console provider (stdout → Live Log Stream). Requirement 2.
                loggers.Add(new AzureExecutionLogger(
                    sp.GetRequiredService<ILogger<AzureExecutionLogger>>(),
                    loggingConfig.MinimumLevel));
                Dev2Logger.Debug("AddExecutionLogging added AzureExecutionLogger (AI + stdout)", executionId);
            }
            else if (loggingConfig.EnableConsoleLogging)
            {
                // AI SDK not registered → ConsoleExecutionLogger. With no Application Insights
                // provider attached it reaches the Console provider only (stdout → Live Log
                // Stream) and never reaches Application Insights. Requirement 1.
                loggers.Add(new ConsoleExecutionLogger(
                    sp.GetRequiredService<ILogger<ConsoleExecutionLogger>>(),
                    loggingConfig.MinimumLevel));
                Dev2Logger.Debug("AddExecutionLogging added ConsoleExecutionLogger (stdout only)", executionId);
            }

            // 2. ElasticsearchExecutionLogger — opt-in
            if (loggingConfig.EnableElasticsearch && File.Exists(loggingConfig.ElasticsearchSettingsPath))
            {
                var elasticOptions = ElasticsearchLoggingOptions.FromBiteFile(loggingConfig.ElasticsearchSettingsPath);
                elasticOptions.EnableDebugMode = loggingConfig.ElasticDebugMode;
                loggers.Add(new ElasticsearchExecutionLogger(elasticOptions, loggingConfig.MinimumLevel));
                Dev2Logger.Debug("AddExecutionLogging added ElasticsearchExecutionLogger", executionId);
            }

            // 3. AuditExecutionLogger — ALWAYS present (security events only)
            loggers.Add(new AuditExecutionLogger(
                sp.GetRequiredService<ILogger<AuditExecutionLogger>>()));
            Dev2Logger.Debug("AddExecutionLogging added AuditExecutionLogger (always-on)", executionId);

            Dev2Logger.Info($"AddExecutionLogging created CompositeExecutionLogger with {loggers.Count} sink(s)", executionId);
            return new CompositeExecutionLogger(loggers);
        });

        // Policy matcher — extracted matching strategy; swap implementation here to change behaviour.
        services.AddSingleton<IWorkflowPolicyMatcher, WorkflowPolicyMatcher>();

        // MCP hosting skeleton (warewolf-lee-mcp-v3-spec.md, "Hosting & transport") — the
        // server description (name/version/capabilities/tools) is process-wide and reused
        // by McpFunction for every /mcp request. `sp` is passed through as
        // McpServerToolCreateOptions.Services so tool parameters bound from DI (e.g.
        // HostEnvironmentConfig, IWorkflowAuthPolicyLoader) are recognised and excluded
        // from the client-visible JSON input schema.
        services.AddSingleton(sp => McpServerOptionsFactory.Create(sp));

        // Route authorization registry — built once from [RequireWorkflowPermission] attributes.
        services.AddSingleton<IRouteAuthorizationRegistry>(
            _ => RouteAuthorizationRegistry.BuildFrom(
                typeof(WorkflowHttpFunction),
                typeof(Functions.WorkflowResumeFunction),
                typeof(Functions.ServiceBusResultFunction)));

        // Suspend/resume: executes suspended-workflow continuations on the lightweight
        // pipeline (resume route + both manual-resumption paths via the driver seam).
        services.AddSingleton<ResumptionExecutor>();

        // Principal parsers — ordered chain (Easy Auth preferred, bearer fallback).
        services.AddSingleton<IPrincipalParser, EasyAuthPrincipalParser>();
        services.AddSingleton<IPrincipalParser, BearerTokenPrincipalParser>();

        // Secure Service Bus workflow trigger (Model A) — jti replay cache,
        // business-idempotency dedupe, and correlation-id → result store shared by
        // ServiceBusWorkflowTriggerFunction and the ServiceBusResultFunction polling
        // endpoint. Hangfire-hash-backed when Config.Persistence is enabled, in-memory
        // fallback otherwise (single-instance-only caveat — see the class docs).
        services.AddSingleton<IServiceBusReplayAndResultStore, ServiceBusReplayAndResultStore>();

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

        // FileDecryptionHelper / FileEncryptionHelper are resolved AFTER InitializeAsync()
        // completes, so GetKeyBytes() is always safe at construction time.
        services.AddSingleton(sp =>
            new FileDecryptionHelper(
                sp.GetRequiredService<KeyVaultSecretManager>(),
                sp.GetRequiredService<ILogger<FileDecryptionHelper>>()));
        services.AddSingleton(sp =>
            new FileEncryptionHelper(sp.GetRequiredService<KeyVaultSecretManager>()));

        // Overrides the EnvironmentMcpSecretResolver registered in AddCoreServices: the
        // add_source MCP tool's "${NAME}" placeholders now resolve against this same vault
        // (real Key Vault access, not the DEBUG_AZURE_KEYVAULT_SECRET bypass — there is no
        // vault to query in that mode, so the environment-variable fallback stays active).
        if (!useDebugBypass)
        {
            services.AddSingleton<Mcp.Secrets.IMcpSecretResolver>(
                _ => new Mcp.Secrets.KeyVaultMcpSecretResolver(config.VaultUri, KeyVaultCredentialFactory.Create(config.CredentialOptions)));
        }

        // AuditLogger is registered globally in AddCoreServices (DI-07);
        // no per-encryption registration needed here.

        return services;
    }
}
