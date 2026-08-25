/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusTriggerOptions — tunables for the secure, in-process
 *  Service Bus workflow trigger (Model A of
 *  Spec-Secure-ServiceBus-Triggered-Execution.md), including the ExecutionTimeout
 *  and MaxConcurrentExecutions values added to work around message loss under load
 *  on a Consumption-plan instance (see the 1000-message ShovelBridge load test
 *  incidents of 2026-08-24/25) without requiring a hosting-plan change.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // reads/writes process-global environment variables
public class ServiceBusTriggerOptionsTests
{
    private const string JtiWindowVar = "WAREWOLF_SERVICEBUS_TRIGGER_JTI_WINDOW_HOURS";
    private const string ExecutionTimeoutVar = "WAREWOLF_SERVICEBUS_TRIGGER_EXECUTION_TIMEOUT_SECONDS";
    private const string MaxConcurrentExecutionsVar = "WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS";

    private string? _previousJtiWindow;
    private string? _previousExecutionTimeout;
    private string? _previousMaxConcurrentExecutions;

    [TestInitialize]
    public void SaveEnvironment()
    {
        _previousJtiWindow = Environment.GetEnvironmentVariable(JtiWindowVar);
        _previousExecutionTimeout = Environment.GetEnvironmentVariable(ExecutionTimeoutVar);
        _previousMaxConcurrentExecutions = Environment.GetEnvironmentVariable(MaxConcurrentExecutionsVar);
    }

    [TestCleanup]
    public void RestoreEnvironment()
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, _previousJtiWindow);
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, _previousExecutionTimeout);
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, _previousMaxConcurrentExecutions);
    }

    // ── Property defaults (no environment involved) ─────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Defaults_JtiReplayWindowIs24Hours()
    {
        var options = new ServiceBusTriggerOptions();

        Assert.AreEqual(TimeSpan.FromHours(24), options.JtiReplayWindow);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Defaults_ExecutionTimeoutIs5Minutes()
    {
        var options = new ServiceBusTriggerOptions();

        Assert.AreEqual(TimeSpan.FromMinutes(5), options.ExecutionTimeout);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Defaults_MaxConcurrentExecutionsIs8()
    {
        var options = new ServiceBusTriggerOptions();

        Assert.AreEqual(8, options.MaxConcurrentExecutions);
    }

    // ── FromEnvironment() — no variables set ─────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NoVariablesSet_UsesAllDefaults()
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, null);
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, null);
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, null);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromHours(24), options.JtiReplayWindow);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.ExecutionTimeout);
        Assert.AreEqual(8, options.MaxConcurrentExecutions);
    }

    // ── FromEnvironment() — JtiReplayWindow ──────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidJtiWindowHours_IsParsed()
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, "48");

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromHours(48), options.JtiReplayWindow);
    }

    [DataTestMethod]
    [DataRow("not-a-number")]
    [DataRow("0")]
    [DataRow("-5")]
    [DataRow("")]
    [TestCategory("UnitTest")]
    public void FromEnvironment_UnparseableOrNonPositiveJtiWindowHours_FallsBackToDefault(string value)
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, value);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromHours(24), options.JtiReplayWindow);
    }

    // ── FromEnvironment() — ExecutionTimeout ─────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidExecutionTimeoutSeconds_IsParsed()
    {
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, "120");

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromSeconds(120), options.ExecutionTimeout);
    }

    [DataTestMethod]
    [DataRow("not-a-number")]
    [DataRow("0")]
    [DataRow("-30")]
    [DataRow("")]
    [TestCategory("UnitTest")]
    public void FromEnvironment_UnparseableOrNonPositiveExecutionTimeoutSeconds_FallsBackToDefault(string value)
    {
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, value);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromMinutes(5), options.ExecutionTimeout);
    }

    // ── FromEnvironment() — MaxConcurrentExecutions ──────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidMaxConcurrentExecutions_IsParsed()
    {
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, "16");

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(16, options.MaxConcurrentExecutions);
    }

    [DataTestMethod]
    [DataRow("not-a-number")]
    [DataRow("0")]
    [DataRow("-3")]
    [DataRow("")]
    [TestCategory("UnitTest")]
    public void FromEnvironment_UnparseableOrNonPositiveMaxConcurrentExecutions_FallsBackToDefault(string value)
    {
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, value);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(8, options.MaxConcurrentExecutions);
    }

    // ── FromEnvironment() — all three set independently ──────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_AllThreeVariablesSet_EachParsedIndependently()
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, "12");
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, "90");
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, "4");

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromHours(12), options.JtiReplayWindow);
        Assert.AreEqual(TimeSpan.FromSeconds(90), options.ExecutionTimeout);
        Assert.AreEqual(4, options.MaxConcurrentExecutions);
    }
}
