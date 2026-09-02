/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Guards the invariant behind the 2026-08-30 ShovelBridge load test fix: a Service
 *  Bus message's lock must stay renewed for at least as long as
 *  ServiceBusWorkflowTriggerFunction can legitimately take to process it
 *  (ServiceBusTriggerOptions.SlotWaitTimeout + ExecutionTimeout), or the SDK's own
 *  auto-renewal expiring mid-execution lets a duplicate delivery race the original
 *  attempt and exhaust the queue's maxDeliveryCount with no result ever recorded
 *  (see the 1000-message ShovelBridge load test build 30578: 16 correlation ids stuck
 *  forever). host.json's extensions.serviceBus.maxAutoLockRenewalDuration is the knob
 *  that must keep pace with those two options' defaults.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests;

[TestClass]
public class HostJsonConfigurationTests
{
    private static JObject LoadHostJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "host.json");
        Assert.IsTrue(File.Exists(path), $"host.json was not found at '{path}' - is it still linked into the test project?");
        return JObject.Parse(File.ReadAllText(path));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ServiceBusExtension_MaxAutoLockRenewalDuration_IsConfigured()
    {
        var host = LoadHostJson();

        var raw = host["extensions"]?["serviceBus"]?["maxAutoLockRenewalDuration"]?.Value<string>();

        Assert.IsFalse(string.IsNullOrWhiteSpace(raw), "extensions.serviceBus.maxAutoLockRenewalDuration must be set explicitly - the Functions SDK default (5 minutes) is shorter than this trigger's worst-case processing time.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ServiceBusExtension_MaxAutoLockRenewalDuration_CoversWorstCaseSlotWaitPlusExecutionTimeout()
    {
        var host = LoadHostJson();
        var raw = host["extensions"]!["serviceBus"]!["maxAutoLockRenewalDuration"]!.Value<string>()!;
        var configuredRenewal = TimeSpan.Parse(raw);

        // Read from ServiceBusTriggerOptions' own defaults rather than hardcoding
        // 5+5 minutes here, so this test fails loudly if either budget is widened
        // without host.json's renewal window being widened to match.
        var defaults = new ServiceBusTriggerOptions();
        var worstCaseProcessingTime = defaults.SlotWaitTimeout + defaults.ExecutionTimeout;

        Assert.IsTrue(
            configuredRenewal >= worstCaseProcessingTime,
            $"maxAutoLockRenewalDuration ({configuredRenewal}) must be >= SlotWaitTimeout + ExecutionTimeout ({worstCaseProcessingTime}), " +
            "otherwise a delivery that genuinely needs the full budget loses its Service Bus lock mid-flight to the SDK's own auto-renewal timing out first.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ServiceBusExtension_ClientRetryOptions_IsConfiguredWithExponentialBackoff()
    {
        // WOLF-8512, step 9: bounds how hard the Service Bus SDK itself retries a transient
        // transport failure (e.g. fetching/settling under the 1000-message ShovelBridge burst)
        // before giving up and surfacing the failure to the trigger's own code - independent of,
        // and ahead of, ServiceBusTriggerOptions.SettlementTimeout and the trigger's own
        // transient/terminal classification.
        var host = LoadHostJson();
        var retryOptions = host["extensions"]?["serviceBus"]?["clientRetryOptions"];

        Assert.IsNotNull(retryOptions, "extensions.serviceBus.clientRetryOptions must be configured explicitly rather than left at the SDK default.");
        Assert.AreEqual("exponential", retryOptions!["mode"]?.Value<string>());
        Assert.IsTrue((retryOptions["maxRetries"]?.Value<int>() ?? 0) > 0, "maxRetries must be a positive, bounded value.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(retryOptions["tryTimeout"]?.Value<string>()), "tryTimeout must be set explicitly.");
    }
}
