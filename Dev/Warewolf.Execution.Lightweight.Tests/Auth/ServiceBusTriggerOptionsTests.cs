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
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // reads/writes process-global environment variables
public class ServiceBusTriggerOptionsTests
{
    private string? _tempSettingsDirectory;

    [TestCleanup]
    public void RestoreEnvironment()
    {
        if (_tempSettingsDirectory is not null && Directory.Exists(_tempSettingsDirectory))
        {
            try { Directory.Delete(_tempSettingsDirectory, recursive: true); } catch { /* best effort */ }
        }
    }

    /// <summary>
    /// WOLF-8516: the 5 tunables below no longer have a standalone env var — they come SOLELY
    /// from <c>Settings/executionengine.settings.json</c>'s <c>serviceBusTrigger</c> block, via
    /// <see cref="HostEnvironmentConfig.ServiceBusTrigger"/>. Writes a temp settings file with
    /// the given <c>serviceBusTrigger</c> JSON body and loads a <see cref="HostEnvironmentConfig"/>
    /// from it, for use with <see cref="ServiceBusTriggerOptions.FromEnvironment(HostEnvironmentConfig)"/>.
    /// </summary>
    private HostEnvironmentConfig BuildHostConfig(string serviceBusTriggerJson)
    {
        _tempSettingsDirectory = Path.Combine(Path.GetTempPath(), "wolf8516-sbt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempSettingsDirectory);
        var json = $"{{\"serviceBusTrigger\":{serviceBusTriggerJson}}}";
        File.WriteAllText(Path.Combine(_tempSettingsDirectory, HostEnvironmentConfig.SettingsFileName), json);
        return HostEnvironmentConfig.Load(_tempSettingsDirectory);
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

    // ── FromEnvironment() — no settings file ─────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NoHostConfig_UsesAllDefaults()
    {
        var options = ServiceBusTriggerOptions.FromEnvironment();

        Assert.AreEqual(TimeSpan.FromHours(24), options.JtiReplayWindow);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.ExecutionTimeout);
        Assert.AreEqual(8, options.MaxConcurrentExecutions);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.SlotWaitTimeout);
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.SettlementTimeout);
    }

    // ── FromEnvironment(hostConfig) — JtiReplayWindow ────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidJtiWindowHours_IsUsed()
    {
        var hostConfig = BuildHostConfig(/*lang=json,strict*/ "{\"jtiWindowHours\":48}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromHours(48), options.JtiReplayWindow);
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(-5.0)]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NonPositiveJtiWindowHours_FallsBackToDefault(double value)
    {
        var hostConfig = BuildHostConfig($"{{\"jtiWindowHours\":{value}}}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromHours(24), options.JtiReplayWindow);
    }

    // ── FromEnvironment(hostConfig) — ExecutionTimeout ───────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidExecutionTimeoutSeconds_IsUsed()
    {
        var hostConfig = BuildHostConfig(/*lang=json,strict*/ "{\"executionTimeoutSeconds\":120}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromSeconds(120), options.ExecutionTimeout);
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(-30.0)]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NonPositiveExecutionTimeoutSeconds_FallsBackToDefault(double value)
    {
        var hostConfig = BuildHostConfig($"{{\"executionTimeoutSeconds\":{value}}}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromMinutes(5), options.ExecutionTimeout);
    }

    // ── FromEnvironment(hostConfig) — MaxConcurrentExecutions ────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidMaxConcurrentExecutions_IsUsed()
    {
        var hostConfig = BuildHostConfig(/*lang=json,strict*/ "{\"maxConcurrentExecutions\":16}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(16, options.MaxConcurrentExecutions);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-3)]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NonPositiveMaxConcurrentExecutions_FallsBackToDefault(int value)
    {
        var hostConfig = BuildHostConfig($"{{\"maxConcurrentExecutions\":{value}}}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(8, options.MaxConcurrentExecutions);
    }

    // ── FromEnvironment(hostConfig) — SlotWaitTimeout ────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidSlotWaitTimeoutSeconds_IsUsed()
    {
        var hostConfig = BuildHostConfig(/*lang=json,strict*/ "{\"slotWaitTimeoutSeconds\":60}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromSeconds(60), options.SlotWaitTimeout);
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(-10.0)]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NonPositiveSlotWaitTimeoutSeconds_FallsBackToExecutionTimeoutDefault(double value)
    {
        var hostConfig = BuildHostConfig($"{{\"slotWaitTimeoutSeconds\":{value}}}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

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
        var hostConfig = BuildHostConfig(/*lang=json,strict*/ "{\"executionTimeoutSeconds\":45}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromSeconds(45), options.ExecutionTimeout);
        Assert.AreEqual(TimeSpan.FromSeconds(45), options.SlotWaitTimeout);
    }

    // ── FromEnvironment(hostConfig) — SettlementTimeout ──────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ValidSettlementTimeoutSeconds_IsUsed()
    {
        var hostConfig = BuildHostConfig(/*lang=json,strict*/ "{\"settlementTimeoutSeconds\":15}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromSeconds(15), options.SettlementTimeout);
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(-5.0)]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NonPositiveSettlementTimeoutSeconds_FallsBackToDefault(double value)
    {
        var hostConfig = BuildHostConfig($"{{\"settlementTimeoutSeconds\":{value}}}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

        Assert.AreEqual(TimeSpan.FromSeconds(30), options.SettlementTimeout);
    }

    // ── FromEnvironment(hostConfig) — all fields set independently ──────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_AllFieldsSet_EachUsedIndependently()
    {
        var hostConfig = BuildHostConfig(
            /*lang=json,strict*/ "{\"jtiWindowHours\":12,\"executionTimeoutSeconds\":90,\"maxConcurrentExecutions\":4,\"slotWaitTimeoutSeconds\":30}");

        var options = ServiceBusTriggerOptions.FromEnvironment(hostConfig);

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
