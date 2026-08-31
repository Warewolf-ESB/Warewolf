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
    private const string SlotWaitTimeoutVar = "WAREWOLF_SERVICEBUS_TRIGGER_SLOT_WAIT_TIMEOUT_SECONDS";
    private const string SettlementTimeoutVar = "WAREWOLF_SERVICEBUS_TRIGGER_SETTLEMENT_TIMEOUT_SECONDS";

    private string? _previousJtiWindow;
    private string? _previousExecutionTimeout;
    private string? _previousMaxConcurrentExecutions;
    private string? _previousSlotWaitTimeout;
    private string? _previousSettlementTimeout;

    [TestInitialize]
    public void SaveEnvironment()
    {
        _previousJtiWindow = Environment.GetEnvironmentVariable(JtiWindowVar);
        _previousExecutionTimeout = Environment.GetEnvironmentVariable(ExecutionTimeoutVar);
        _previousMaxConcurrentExecutions = Environment.GetEnvironmentVariable(MaxConcurrentExecutionsVar);
        _previousSlotWaitTimeout = Environment.GetEnvironmentVariable(SlotWaitTimeoutVar);
        _previousSettlementTimeout = Environment.GetEnvironmentVariable(SettlementTimeoutVar);
    }

    [TestCleanup]
    public void RestoreEnvironment()
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, _previousJtiWindow);
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, _previousExecutionTimeout);
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, _previousMaxConcurrentExecutions);
        Environment.SetEnvironmentVariable(SlotWaitTimeoutVar, _previousSlotWaitTimeout);
        Environment.SetEnvironmentVariable(SettlementTimeoutVar, _previousSettlementTimeout);
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

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Defaults_SlotWaitTimeoutIs5Minutes()
    {
        var options = new ServiceBusTriggerOptions();

        Assert.AreEqual(TimeSpan.FromMinutes(5), options.SlotWaitTimeout);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Defaults_SettlementTimeoutIs30Seconds()
    {
        var options = new ServiceBusTriggerOptions();

        Assert.AreEqual(TimeSpan.FromSeconds(30), options.SettlementTimeout);
    }

    // ── FromEnvironment() — no variables set ─────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NoVariablesSet_UsesAllDefaults()
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, null);
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, null);
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, null);
        Environment.SetEnvironmentVariable(SlotWaitTimeoutVar, null);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromHours(24), options.JtiReplayWindow);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.ExecutionTimeout);
        Assert.AreEqual(8, options.MaxConcurrentExecutions);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.SlotWaitTimeout);
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

    // ── FromEnvironment() — SlotWaitTimeout ──────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidSlotWaitTimeoutSeconds_IsParsed()
    {
        Environment.SetEnvironmentVariable(SlotWaitTimeoutVar, "60");

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromSeconds(60), options.SlotWaitTimeout);
    }

    [DataTestMethod]
    [DataRow("not-a-number")]
    [DataRow("0")]
    [DataRow("-10")]
    [DataRow("")]
    [TestCategory("UnitTest")]
    public void FromEnvironment_UnparseableOrNonPositiveSlotWaitTimeoutSeconds_FallsBackToExecutionTimeoutDefault(string value)
    {
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, null);
        Environment.SetEnvironmentVariable(SlotWaitTimeoutVar, value);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromMinutes(5), options.SlotWaitTimeout);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_SlotWaitTimeoutNotSet_FallsBackToTheConfiguredExecutionTimeout_NotAFixedValue()
    {
        // The fallback tracks whatever ExecutionTimeout resolved to, not a hardcoded
        // duration - a message legitimately queued behind a slow-but-healthy execution
        // must be able to wait at least one full execution's worth of time for a slot,
        // whatever that configured execution budget is.
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, "45");
        Environment.SetEnvironmentVariable(SlotWaitTimeoutVar, null);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromSeconds(45), options.ExecutionTimeout);
        Assert.AreEqual(TimeSpan.FromSeconds(45), options.SlotWaitTimeout);
    }

    // ── FromEnvironment() — SettlementTimeout ────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidSettlementTimeoutSeconds_IsParsed()
    {
        Environment.SetEnvironmentVariable(SettlementTimeoutVar, "15");

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromSeconds(15), options.SettlementTimeout);
    }

    [DataTestMethod]
    [DataRow("not-a-number")]
    [DataRow("0")]
    [DataRow("-5")]
    [DataRow("")]
    [TestCategory("UnitTest")]
    public void FromEnvironment_UnparseableOrNonPositiveSettlementTimeoutSeconds_FallsBackToDefault(string value)
    {
        Environment.SetEnvironmentVariable(SettlementTimeoutVar, value);

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromSeconds(30), options.SettlementTimeout);
    }

    // ── FromEnvironment() — all four set independently ───────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_AllFourVariablesSet_EachParsedIndependently()
    {
        Environment.SetEnvironmentVariable(JtiWindowVar, "12");
        Environment.SetEnvironmentVariable(ExecutionTimeoutVar, "90");
        Environment.SetEnvironmentVariable(MaxConcurrentExecutionsVar, "4");
        Environment.SetEnvironmentVariable(SlotWaitTimeoutVar, "30");

        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromHours(12), options.JtiReplayWindow);
        Assert.AreEqual(TimeSpan.FromSeconds(90), options.ExecutionTimeout);
        Assert.AreEqual(4, options.MaxConcurrentExecutions);
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.SlotWaitTimeout);
    }

    // ── ResolveHostFunctionTimeout(env, readFile) — WOLF-8512 five-tier resolution ──
    //    Pure-function tests: no real environment variables mutated, no disk touched.
    //    See ServiceBusTriggerOptions.ResolveHostFunctionTimeout's own doc comment for
    //    the exact tier order this is asserting.

    private static Func<string, string?> EnvFrom(params (string Key, string Value)[] pairs)
    {
        var map = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var (key, value) in pairs)
        {
            map[key] = value;
        }
        return key => map.TryGetValue(key, out var value) ? value : null;
    }

    private static Func<string, string?> FileTextOf(string? content) => _ => content;

    private const string HostJsonTenMinutes = /*lang=json,strict*/ "{ \"functionTimeout\": \"00:10:00\" }";

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_AppSettingWinsOverHostJsonAndSku()
    {
        var env = EnvFrom(
            ("AzureFunctionsJobHost__functionTimeout", "00:07:00"),
            ("WEBSITE_SKU", "Dynamic"));

        var (timeout, tier) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf(HostJsonTenMinutes));

        Assert.AreEqual(TimeSpan.FromMinutes(7), timeout);
        Assert.AreEqual("AzureFunctionsJobHost__functionTimeout", tier);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_HostJsonUsedWhenAppSettingAbsent()
    {
        var env = EnvFrom(("WEBSITE_SKU", "Dynamic"));

        var (timeout, tier) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf(HostJsonTenMinutes));

        Assert.AreEqual(TimeSpan.FromMinutes(10), timeout);
        Assert.AreEqual("host.json", tier);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_CorruptHostJson_FallsThroughWithoutThrowing()
    {
        var env = EnvFrom(("WEBSITE_SKU", "Dynamic"));

        var (timeout, tier) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf("{ not valid json"));

        Assert.AreEqual(TimeSpan.FromMinutes(5), timeout);
        Assert.AreEqual("WEBSITE_SKU=Dynamic", tier);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_HostJsonWithoutFunctionTimeoutProperty_FallsThroughToSku()
    {
        var env = EnvFrom(("WEBSITE_SKU", "Dynamic"));

        var (timeout, tier) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf("{ \"version\": \"2.0\" }"));

        Assert.AreEqual(TimeSpan.FromMinutes(5), timeout);
        Assert.AreEqual("WEBSITE_SKU=Dynamic", tier);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_HostJsonMissingEntirely_FallsThroughToSku()
    {
        var env = EnvFrom(("WEBSITE_SKU", "FlexConsumption"));

        var (timeout, tier) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf(null));

        Assert.AreEqual(TimeSpan.FromMinutes(30), timeout);
        Assert.AreEqual("WEBSITE_SKU=FlexConsumption", tier);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_SkuDynamic_ReturnsFiveMinutes()
    {
        var env = EnvFrom(("WEBSITE_SKU", "Dynamic"));

        var (timeout, _) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf(null));

        Assert.AreEqual(TimeSpan.FromMinutes(5), timeout);
    }

    [DataTestMethod]
    [DataRow("FlexConsumption")]
    [DataRow("ElasticPremium")]
    [DataRow("Dedicated")]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_NonDynamicSku_ReturnsThirtyMinutes(string sku)
    {
        var env = EnvFrom(("WEBSITE_SKU", sku));

        var (timeout, _) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf(null));

        Assert.AreEqual(TimeSpan.FromMinutes(30), timeout);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveHostFunctionTimeout_NothingResolvable_FallsBackToTenMinutes()
    {
        var env = EnvFrom();

        var (timeout, tier) = ServiceBusTriggerOptions.ResolveHostFunctionTimeout(env, FileTextOf(null));

        Assert.AreEqual(TimeSpan.FromMinutes(10), timeout);
        Assert.AreEqual("hard fallback", tier);
    }

    // ── ResolveClaimStaleAfter(env, readFile) — override + tier composition ─────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveClaimStaleAfter_ExplicitOverride_WinsOverEverythingElse()
    {
        var env = EnvFrom(
            ("WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES", "42"),
            ("AzureFunctionsJobHost__functionTimeout", "00:07:00"));

        var claimStaleAfter = ServiceBusTriggerOptions.ResolveClaimStaleAfter(env, FileTextOf(HostJsonTenMinutes));

        Assert.AreEqual(TimeSpan.FromMinutes(42), claimStaleAfter);
    }

    [DataTestMethod]
    [DataRow("not-a-number")]
    [DataRow("0")]
    [DataRow("-5")]
    [TestCategory("UnitTest")]
    public void ResolveClaimStaleAfter_UnparseableOrNonPositiveOverride_FallsBackToTierResolution(string value)
    {
        var env = EnvFrom(("WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES", value));

        var claimStaleAfter = ServiceBusTriggerOptions.ResolveClaimStaleAfter(env, FileTextOf(HostJsonTenMinutes));

        Assert.AreEqual(TimeSpan.FromMinutes(11), claimStaleAfter);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveClaimStaleAfter_NoOverride_HostJsonTenMinutes_ReturnsElevenMinutes()
    {
        var claimStaleAfter = ServiceBusTriggerOptions.ResolveClaimStaleAfter(EnvFrom(), FileTextOf(HostJsonTenMinutes));

        Assert.AreEqual(TimeSpan.FromMinutes(11), claimStaleAfter);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveClaimStaleAfter_NoOverride_SkuDynamicNoHostJson_ReturnsSixMinutes()
    {
        var claimStaleAfter = ServiceBusTriggerOptions.ResolveClaimStaleAfter(
            EnvFrom(("WEBSITE_SKU", "Dynamic")), FileTextOf(null));

        Assert.AreEqual(TimeSpan.FromMinutes(6), claimStaleAfter);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveClaimStaleAfter_NoOverride_SkuFlexConsumptionNoHostJson_ReturnsThirtyOneMinutes()
    {
        var claimStaleAfter = ServiceBusTriggerOptions.ResolveClaimStaleAfter(
            EnvFrom(("WEBSITE_SKU", "FlexConsumption")), FileTextOf(null));

        Assert.AreEqual(TimeSpan.FromMinutes(31), claimStaleAfter);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveClaimStaleAfter_NothingSet_ReturnsElevenMinutes_HardFallbackPlusMargin()
    {
        var claimStaleAfter = ServiceBusTriggerOptions.ResolveClaimStaleAfter(EnvFrom(), FileTextOf(null));

        Assert.AreEqual(TimeSpan.FromMinutes(11), claimStaleAfter);
    }
}
