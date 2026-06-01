/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using Dev2.Runtime.Subscription;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Enums;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Licensing;
using Warewolf.Usage;

namespace Warewolf.Execution.Lightweight.Tests.Logging
{
    /// <summary>
    /// Tests for <see cref="UsageEventEmitter"/> — the per-execution usage
    /// telemetry emitter added for 8438.
    /// </summary>
    [TestClass]
    public class UsageEventEmitterTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_HappyPath_CallsSinkOnceWithUsageType()
        {
            var sink = new CapturingSink { Result = UsageDataResult.ok };
            var subscription = new FakeSubscriptionProvider
            {
                CustomerId     = "cust-123",
                SubscriptionId = "sub-456",
                PlanId         = "developer",
                Status         = SubscriptionStatus.Active
            };

            var emitter = new UsageEventEmitter(sink, () => subscription);

            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                workflowName: "Hello World",
                executionId:  Guid.Parse("11111111-2222-3333-4444-555555555555"),
                duration:     TimeSpan.FromMilliseconds(42),
                isSuccess:    true,
                errorCount:   0,
                startedAtUtc: new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc)));

            Assert.AreEqual(1, sink.Calls.Count, "Emitter should call the sink exactly once.");
            Assert.AreEqual(UsageType.Usage, sink.Calls[0].UsageType);
            Assert.AreEqual("cust-123", sink.Calls[0].CustomerId);

            var payload = JObject.Parse(sink.Calls[0].UsageInfo);
            Assert.AreEqual("Hello World",                                 (string)payload["workflowName"]);
            Assert.AreEqual("11111111-2222-3333-4444-555555555555",        (string)payload["executionId"]);
            Assert.AreEqual(42L,                                            (long)payload["durationMs"]);
            Assert.AreEqual(true,                                           (bool)payload["isSuccess"]);
            Assert.AreEqual(0,                                              (int)payload["errorCount"]);
            Assert.AreEqual("WolfClaw",                                    (string)payload["source"]);
            Assert.AreEqual("sub-456",                                     (string)payload["subscriptionId"]);
            Assert.AreEqual("developer",                                   (string)payload["planId"]);
            Assert.AreEqual("Active",                                      (string)payload["status"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_AllSubscriptionFieldsEmpty_FallsBackToUnRegistered()
        {
            var sink = new CapturingSink { Result = UsageDataResult.ok };
            var subscription = new FakeSubscriptionProvider();   // every field empty

            var emitter = new UsageEventEmitter(sink, () => subscription);

            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));

            Assert.AreEqual("UnRegistered", sink.Calls[0].CustomerId,
                "Empty CustomerId + SubscriptionKey + SubscriptionSiteName must fall back to 'UnRegistered'.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_EmptyCustomerId_FallsBackToSubscriptionKey()
        {
            var sink = new CapturingSink { Result = UsageDataResult.ok };
            var subscription = new FakeSubscriptionProvider
            {
                CustomerId      = "",
                SubscriptionKey = "Dev2-test-account",
                SubscriptionSiteName = "warewolf"
            };

            var emitter = new UsageEventEmitter(sink, () => subscription);
            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));

            Assert.AreEqual("Dev2-test-account", sink.Calls[0].CustomerId,
                "Empty CustomerId must promote SubscriptionKey ahead of SubscriptionSiteName.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_OnlySubscriptionSiteName_FallsBackToIt()
        {
            var sink = new CapturingSink { Result = UsageDataResult.ok };
            var subscription = new FakeSubscriptionProvider { SubscriptionSiteName = "warewolf" };

            var emitter = new UsageEventEmitter(sink, () => subscription);
            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));

            Assert.AreEqual("warewolf", sink.Calls[0].CustomerId,
                "When only SubscriptionSiteName is populated it must be used as CustomerId.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_EnvVarOverride_WinsOverEverything()
        {
            const string envVar = "WAREWOLF_USAGE_CUSTOMER_ID";
            var prior = Environment.GetEnvironmentVariable(envVar);
            try
            {
                Environment.SetEnvironmentVariable(envVar, "dev-ashley@theunlimited");

                var sink = new CapturingSink { Result = UsageDataResult.ok };
                var subscription = new FakeSubscriptionProvider
                {
                    CustomerId      = "real-customer",
                    SubscriptionKey = "real-key"
                };

                var emitter = new UsageEventEmitter(sink, () => subscription);
                emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                    "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));

                Assert.AreEqual("dev-ashley@theunlimited", sink.Calls[0].CustomerId,
                    "WAREWOLF_USAGE_CUSTOMER_ID must override the subscription chain.");
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, prior);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_SinkThrows_DoesNotPropagate()
        {
            var sink = new ThrowingSink();
            var subscription = new FakeSubscriptionProvider { CustomerId = "c" };

            var emitter = new UsageEventEmitter(sink, () => subscription);

            // Must not throw — usage emission is observability, never on the hot path.
            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_NullEvent_DoesNotThrowOrEmit()
        {
            var sink = new CapturingSink();
            var emitter = new UsageEventEmitter(sink, () => new FakeSubscriptionProvider());

            emitter.TrackWorkflowExecution(null!);

            Assert.AreEqual(0, sink.Calls.Count, "Null event must short-circuit before reaching the sink.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_FailureExecution_StillEmits()
        {
            // The intent of per-execution tracking is to record EVERY execution attempt
            // including failed ones — so usage volume reflects load, not just success.
            var sink = new CapturingSink { Result = UsageDataResult.ok };
            var subscription = new FakeSubscriptionProvider { CustomerId = "c" };

            var emitter = new UsageEventEmitter(sink, () => subscription);

            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.FromMilliseconds(10), false, 2, DateTime.UtcNow));

            Assert.AreEqual(1, sink.Calls.Count);
            var payload = JObject.Parse(sink.Calls[0].UsageInfo);
            Assert.AreEqual(false, (bool)payload["isSuccess"]);
            Assert.AreEqual(2,     (int)payload["errorCount"]);
        }

        // ---------------------------------------------------------------------
        // Test doubles
        // ---------------------------------------------------------------------

        private sealed class CapturingSink : IUsageTrackerSink
        {
            public UsageDataResult Result { get; set; } = UsageDataResult.ok;
            public List<(string CustomerId, UsageType UsageType, string UsageInfo)> Calls { get; } = new();

            public UsageDataResult TrackEvent(string customerId, UsageType usageType, string usageInfo)
            {
                Calls.Add((customerId, usageType, usageInfo));
                return Result;
            }
        }

        private sealed class ThrowingSink : IUsageTrackerSink
        {
            public UsageDataResult TrackEvent(string customerId, UsageType usageType, string usageInfo)
                => throw new InvalidOperationException("simulated network failure");
        }

        private sealed class FakeSubscriptionProvider : ISubscriptionProvider
        {
            public string SubscriptionKey      { get; set; } = string.Empty;
            public string SubscriptionSiteName { get; set; } = string.Empty;
            public string CustomerId           { get; set; } = string.Empty;
            public string PlanId               { get; set; } = string.Empty;
            public string SubscriptionId       { get; set; } = string.Empty;
            public bool   IsLicensed           { get; set; }
            public bool   StopExecutions       { get; set; }
            public SubscriptionStatus Status   { get; set; } = SubscriptionStatus.NotActive;

            public void SaveSubscriptionData(ISubscriptionData subscriptionData) { }
            public ISubscriptionData GetSubscriptionData() => null!;
            public ISubscriptionData DefaultSubscription() => null!;
        }
    }
}
