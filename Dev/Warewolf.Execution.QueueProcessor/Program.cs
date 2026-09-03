/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Azure.Core;
using Azure.Identity;
using Dev2.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Consumers;
using Warewolf.Execution.QueueProcessor.Engine;
using Warewolf.Execution.QueueProcessor.Hosting;
using Warewolf.Execution.QueueProcessor.Logging;
using Warewolf.Execution.QueueProcessor.Messaging;
using Warewolf.Security.Encryption;
using Warewolf.Streams;

const string executionId = "QueueProcessor-Startup";

try
{
    // ── Step 1: logging configuration (identical env-var contract to the engine) ──
    var loggingConfig = LoggingConfiguration.FromEnvironment();

    // ── Step 2: bootstrap logging FIRST so no startup line is lost ───────────────
    using var bootstrapFactory = LoggerFactory.Create(b =>
        b.AddSimpleConsole(o => o.SingleLine = true).SetMinimumLevel(loggingConfig.MelMinimumLevel));

    Dev2Logger.ExternalSink = new QueueProcessorLogSink(
        bootstrapFactory.CreateLogger("Warewolf.Execution.QueueProcessor"),
        loggingConfig.MinimumLevel);
    Dev2Logger.CorrelationPrefixProvider = QueueProcessorCorrelation.GetPrefix;

    Dev2Logger.Info(
        $"QueueProcessor starting. replica='{QueueProcessorCorrelation.ReplicaId}' " +
        $"app='{QueueProcessorCorrelation.AppName}' minLevel={loggingConfig.MinimumLevel} " +
        $"console={loggingConfig.EnableConsoleLogging} appInsights={loggingConfig.RegisterApplicationInsightsSdk}",
        executionId);

    // ── Step 3: build the host ───────────────────────────────────────────────────
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables();

    builder.Logging.ClearProviders();
    if (loggingConfig.EnableConsoleLogging)
    {
        builder.Logging.AddSimpleConsole(o => o.SingleLine = true);
    }
    builder.Logging.SetMinimumLevel(loggingConfig.MelMinimumLevel);

    if (loggingConfig.RegisterApplicationInsightsSdk)
    {
        // Same deliberately non-standard setting name as the engine, so no platform-side
        // pipeline auto-enables itself behind our back.
        var aiConnectionString =
            Environment.GetEnvironmentVariable("WAREWOLF_APPINSIGHTS_CONNECTION_STRING");
        builder.Services.AddApplicationInsightsTelemetryWorkerService(o =>
            o.ConnectionString = aiConnectionString);
    }

    // ── Options ─────────────────────────────────────────────────────────────────
    // Flat + sectioned binding: QUEUE__*, ENGINE__*, WORKER__*, RABBITMQ__*, KEYVAULT__*
    // all land on one options object, so operators keep the plan's documented names.
    // The mapping itself lives in QueueProcessorOptionsBinder so it can be unit tested - top-level
    // statements cannot. That is not cosmetic: while this was a lambda here, WORKER__MAXDELIVERYATTEMPTS
    // and WORKER__RETRYENGINEINTERNALERRORS were emitted by Deploy-WwQueueProcessor.ps1, set on the
    // live Container App, and never assigned by it.
    builder.Services.AddOptions<QueueProcessorOptions>()
        .Configure(o => QueueProcessorOptionsBinder.Apply(
            builder.Configuration, o, loggingConfig.IsDevelopment))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // ── Credential (MI in Azure, az CLI locally, client-secret only for dev) ─────
    builder.Services.AddSingleton<TokenCredential>(sp =>
    {
        var opt = sp.GetRequiredService<IOptions<QueueProcessorOptions>>().Value;

        // ENGINE__TENANTID is set by Deploy-WwQueueProcessor.ps1 (defaulted from the deploying
        // `az account show`). Blank is legal ONLY for a system-assigned managed identity, where the
        // tenant is implied by the platform; anywhere else - a user-assigned MI, the az CLI
        // credential, a client secret - a blank tenant makes the credential chain fail late with
        // "Invalid tenant id provided", which reads like a role problem rather than a config gap.
        // Say so at startup rather than at the first message.
        if (string.IsNullOrWhiteSpace(opt.TenantId))
        {
            Dev2Logger.Warn(
                "ENGINE__TENANTID is not set. This is only correct for a SYSTEM-ASSIGNED managed " +
                "identity, where the tenant is implied. For a user-assigned identity, the local az " +
                "CLI credential, or a client secret, set ENGINE__TENANTID (Deploy-WwQueueProcessor.ps1 " +
                "-TenantId) or token acquisition will fail with 'Invalid tenant id provided'.",
                executionId);
        }

        if (opt.UseClientSecretFallback)
        {
            if (string.IsNullOrWhiteSpace(opt.ClientId) || string.IsNullOrWhiteSpace(opt.ClientSecret))
            {
                throw new InvalidOperationException(
                    "Engine:UseClientSecretFallback is true but Engine:ClientId/ClientSecret are unset.");
            }
            return new ClientSecretCredential(opt.TenantId, opt.ClientId, opt.ClientSecret);
        }

        return new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            TenantId = opt.EffectiveTenantId,
            ManagedIdentityClientId = ConfigurationValues.NullIfBlank(opt.ManagedIdentityClientId),
        });
    });

    // ── Key Vault AES (WFAES::) — must be wired BEFORE any .bite is read ────────
    builder.Services.AddSingleton(sp =>
    {
        var opt = sp.GetRequiredService<IOptions<QueueProcessorOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<KeyVaultSecretManager>>();
        var credential = sp.GetRequiredService<TokenCredential>();

        return new KeyVaultSecretManager(
            vaultUri: $"https://{opt.KeyVaultName}.vault.azure.net/",
            secretName: opt.KeyVaultSecretName ?? string.Empty,
            credential: credential,
            logger: logger,
            debugSecret: opt.DebugKeyVaultSecret);
    });

    builder.Services.AddSingleton<TriggerBiteReader>();

    // Singleton: the constructor performs the ONE startup scan of the sources folder and caches
    // every source by id. Registered before the loader so the cache is populated as part of cold
    // start - after this point nothing re-reads a source file, so a broker reconnect or a
    // dead-letter publish can never fail on a missing/half-written .bite mid-message.
    builder.Services.AddSingleton<RabbitMqSourceCatalog>();
    builder.Services.AddSingleton<QueueConfigurationLoader>();

    // ── Trigger + source configuration (cold start, read-only) ──────────────────
    builder.Services.AddSingleton(sp => sp.GetRequiredService<QueueConfigurationLoader>().Load());

    // ── Engine client: typed HttpClient + MI token handler ──────────────────────
    builder.Services.AddTransient<WwExecutionTokenHandler>();
    // Built with AddTypedClient rather than AddHttpClient<TClient,TImpl> so the retry-classification
    // flag is passed EXPLICITLY. The generic overload activates the client through
    // ActivatorUtilities, which would silently fall back to the parameter's default and make the
    // option look configurable while never actually taking effect.
    builder.Services
        .AddHttpClient(nameof(EngineWorkflowClient), (sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<QueueProcessorOptions>>().Value;
            client.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
            // Bounded, unlike the on-prem forwarder's Timeout.InfiniteTimeSpan.
            client.Timeout = TimeSpan.FromSeconds(opt.EngineTimeoutSeconds + 5);
        })
        .AddHttpMessageHandler<WwExecutionTokenHandler>()
        .AddTypedClient<IEngineWorkflowClient>((client, sp) =>
        {
            var opt = sp.GetRequiredService<IOptions<QueueProcessorOptions>>().Value;
            return new EngineWorkflowClient(client, opt.RetryEngineInternalErrors);
        });

    // ── Messaging ───────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IDeadLetterPublisher>(sp =>
        new RabbitMqDeadLetterPublisher(sp.GetRequiredService<ResolvedQueueConfiguration>()));

    builder.Services.AddSingleton<IConsumer>(sp =>
    {
        var config = sp.GetRequiredService<ResolvedQueueConfiguration>();
        var forwarder = new EngineForwarder(
            config,
            sp.GetRequiredService<IEngineWorkflowClient>(),
            sp.GetRequiredService<IDeadLetterPublisher>(),
            sp.GetRequiredService<IOptions<QueueProcessorOptions>>());

        return new AuditingConsumerDecorator(forwarder, config);
    });

    builder.Services.AddSingleton<IMessagePump>(sp =>
    {
        var opt = sp.GetRequiredService<IOptions<QueueProcessorOptions>>().Value;
        return new RabbitMqMessagePump(
            sp.GetRequiredService<ResolvedQueueConfiguration>(),
            sp.GetRequiredService<IConsumer>(),
            opt.MaxConcurrency,
            opt.MaxDeliveryAttempts,
            // The pump needs its own handle on the dead-letter publisher: a TRANSPORT failure never
            // reaches EngineForwarder's dead-letter path, which only fires on a non-2xx response.
            sp.GetRequiredService<IDeadLetterPublisher>());
    });

    builder.Services.AddHostedService<QueueConsumerService>();

    var host = builder.Build();

    // ── Step 4: initialise Key Vault + wire the decrypt hook ────────────────────
    var options = host.Services.GetRequiredService<IOptions<QueueProcessorOptions>>().Value;
    options.Validate(ReadInt(Environment.GetEnvironmentVariable("CONTAINER_APP_TERMINATION_GRACE_SECONDS"), 0));

    if (options.KeyVaultConfigured || !string.IsNullOrWhiteSpace(options.DebugKeyVaultSecret))
    {
        var secretManager = host.Services.GetRequiredService<KeyVaultSecretManager>();
        await secretManager.InitializeAsync().ConfigureAwait(false);

        var decryptionHelper = new FileDecryptionHelper(
            secretManager,
            host.Services.GetRequiredService<ILogger<FileDecryptionHelper>>());
        DpapiWrapper.AesDecryptHook = decryptionHelper.DecryptConnectionString;

        Dev2Logger.Info("Key Vault AES decrypt hook wired (WFAES:: values are now readable).", executionId);
    }
    else
    {
        Dev2Logger.Warn(
            "No Key Vault configured (KEYVAULT__NAME / KEYVAULT__SECRETNAME). Only PLAINTEXT staged " +
            ".bite files can be read - WFAES-encrypted files will fail. This is expected for local " +
            "development only.", executionId);
    }

    // ── Step 5: upgrade the sink now that providers are live ────────────────────
    var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
    Dev2Logger.ExternalSink = new QueueProcessorLogSink(
        loggerFactory.CreateLogger("Warewolf.Execution.QueueProcessor"),
        loggingConfig.MinimumLevel);

    Dev2Logger.Info("QueueProcessor logging upgraded to the host pipeline.", executionId);

    await host.RunAsync().ConfigureAwait(false);
    Dev2Logger.Info("QueueProcessor exited cleanly.", executionId);
    return 0;
}
catch (TriggerConfigurationException ex)
{
    // Configuration problems get a clean, actionable message - never a stack-trace dump that
    // buries "which file is wrong".
    Dev2Logger.Fatal($"QueueProcessor configuration error: {ex.Message}", executionId);
    Console.Error.WriteLine($"CONFIGURATION ERROR: {ex.Message}");
    return 2;
}
catch (Exception ex)
{
    Dev2Logger.Fatal("QueueProcessor failed to start.", ex, executionId);
    Console.Error.WriteLine($"FATAL: {ex}");
    return 1;
}

// Thin forwarder to ConfigurationValues, which carries the "empty means unset" rule and its
// regression tests. Top-level statements are not unit testable, so the rule lives there - as does
// the whole options mapping, in QueueProcessorOptionsBinder.
static int ReadInt(string? value, int @default) => ConfigurationValues.ReadInt(value, @default);
