using Dev2.Common;
using Dev2.Runtime.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;

const string executionId = "Program-Startup";

try
{
    // ── Step 1: Load configuration ───────────────────────────────────────────
    var config = HostEnvironmentConfig.Load();
    var loggingConfig = LoggingConfiguration.FromEnvironment();

    // ── Step 2: Bootstrap logging (FIRST — no log is lost) ───────────────────
    // Create a lightweight console logger before the DI host exists so that
    // all Dev2Logger calls during startup are captured immediately.
    using var bootstrapFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));
    var bootstrapLogger = new ConsoleExecutionLogger(
        bootstrapFactory.CreateLogger<ConsoleExecutionLogger>(), loggingConfig.MinimumLevel);

    Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(bootstrapLogger);
    Dev2Logger.CorrelationPrefixProvider = ExecutionLoggerBase.GetCorrelationPrefixStatic;

    Dev2Logger.Info("Program starting - bootstrap logging active", executionId);
    Dev2Logger.Info($"Program configuration loaded. WorkflowsDirectory: {config.WorkflowsDirectory}, EncryptionEnabled: {config.EncryptionEnabled}, IsDevelopment: {config.IsDevelopment}", executionId);
    Dev2Logger.Debug($"Program logging configuration: EnableAI={loggingConfig.EnableApplicationInsights}, EnableElastic={loggingConfig.EnableElasticsearch}, MinLevel={loggingConfig.MinimumLevel}", executionId);

    // ── Step 3: Build host ───────────────────────────────────────────────────
    Dev2Logger.Debug("Program building host", executionId);

    var host = new HostBuilder()
        .ConfigureWarewolf(config)
        .ConfigureServices(services =>
         {
             services.AddExecutionLogging(loggingConfig);
         })
        .Build();

    Dev2Logger.Info("Program host built successfully, running startup orchestrator", executionId);

    // ── Step 4: Run startup (encryption, index warm-up) ──────────────────────
    await StartupOrchestrator.RunStartupAsync(host, config);

    // ── Step 5: Upgrade to full composite logger ─────────────────────────────
    // Resolve IExecutionLogger AFTER RunStartupAsync so the AES decrypt hook
    // is wired before the Elasticsearch .bite file is read.
    var executionLogger = host.Services.GetRequiredService<IExecutionLogger>();

    Dev2Logger.Info("Program startup orchestrator completed, upgrading to full composite logger", executionId);

    // Replace bootstrap sink with the full composite (Console + AI + Elastic + Audit).
    Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(executionLogger);

    Dev2Logger.Info("Program Dev2Logger external sink upgraded to full CompositeExecutionLogger", executionId);

    // ── Step 6: License check ────────────────────────────────────────────────
    Dev2Logger.Debug("Program loading Warewolf License", executionId);

    var licenseProvider = SubscriptionProvider.Instance;
    if (licenseProvider.IsLicensed)
    {
        Dev2Logger.Info($"Program license loaded successfully. Status: {licenseProvider.Status}", executionId);
    }
    else
    {
        Dev2Logger.Warn($"Program server not licensed. Status: {licenseProvider.Status}, StopExecutions: {licenseProvider.StopExecutions}", executionId);
    }

    // ── Step 7: Run ──────────────────────────────────────────────────────────
    Dev2Logger.Info("Program initialization complete, starting host", executionId);

    await host.RunAsync();
}
catch (Exception ex)
{
    // Fatal cold-start failure — write to stderr so the Azure Functions runtime
    // captures it regardless of whether the logging pipeline is available.
    Dev2Logger.Fatal("Program terminated unexpectedly during startup", ex, executionId);

    await Console.Error.WriteLineAsync(
        $"[FATAL] Host terminated unexpectedly at {DateTimeOffset.UtcNow:O}: {ex}");
    throw;
}
