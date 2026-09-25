/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.ServiceBusWorker.Tests;

/// <summary>
/// Pins the two defects that kept this worker's App Insights empty: no SDK was registered at all
/// (and the deploy wrote only the engine's non-standard connection-string name), and the SDK's
/// default provider rule would drop every Information line.
/// </summary>
[TestClass]
[DoNotParallelize] // the AI SDK also reads APPLICATIONINSIGHTS_CONNECTION_STRING from the process environment
public class WorkerTelemetryTests
{
    const string ConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000001;IngestionEndpoint=https://ingest.example/";

    string? _savedEnv;

    [TestInitialize]
    public void ClearEnvironment()
    {
        _savedEnv = Environment.GetEnvironmentVariable(WorkerTelemetry.ConnectionStringSettingName);
        Environment.SetEnvironmentVariable(WorkerTelemetry.ConnectionStringSettingName, null);
    }

    [TestCleanup]
    public void RestoreEnvironment() =>
        Environment.SetEnvironmentVariable(WorkerTelemetry.ConnectionStringSettingName, _savedEnv);

    static ServiceProvider Build(string? connectionString)
    {
        var services = new ServiceCollection();
        // The Functions host always provides IConfiguration; the AI worker integration requires it.
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddWorkerTelemetry(connectionString);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ConnectionStringSettingName_IsTheStandardNameTheDeployScriptWrites()
    {
        // Deploy-WwExecutionServiceBusWorker.ps1 passes this exact name to Setup-ApplicationInsights.ps1.
        Assert.AreEqual("APPLICATIONINSIGHTS_CONNECTION_STRING", WorkerTelemetry.ConnectionStringSettingName);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task AddWorkerTelemetry_WithConnectionString_ConfiguresTheTelemetryChannel()
    {
        // Async disposal: FunctionsTelemetryModule implements only IAsyncDisposable.
        await using var sp = Build(ConnectionString);

        var config = sp.GetRequiredService<TelemetryConfiguration>();

        Assert.AreEqual(ConnectionString, config.ConnectionString);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task AddWorkerTelemetry_ReplacesTheSdkWarningOnlyRule_SoInformationReachesAppInsights()
    {
        // Async disposal: FunctionsTelemetryModule implements only IAsyncDisposable.
        await using var sp = Build(ConnectionString);

        var aiRules = sp.GetRequiredService<IOptions<LoggerFilterOptions>>().Value.Rules
            .Where(r => r.ProviderName == ApplicationInsightsLogFilter.AiProviderName)
            .ToList();

        Assert.AreEqual(1, aiRules.Count, "exactly one App Insights provider rule should remain");
        Assert.AreEqual(LogLevel.Information, aiRules[0].LogLevel,
            "the SDK's default Warning rule would drop every Information line the worker logs");
        Assert.IsNull(aiRules[0].CategoryName, "the rule must cover every category");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task AddWorkerTelemetry_WithoutConnectionString_RegistersAndSendsNothing()
    {
        // Async disposal: FunctionsTelemetryModule implements only IAsyncDisposable.
        await using var sp = Build(null);

        var config = sp.GetRequiredService<TelemetryConfiguration>();

        Assert.IsTrue(string.IsNullOrEmpty(config.ConnectionString),
            "a local run or an unprovisioned app must start without a connection string");
    }
}
