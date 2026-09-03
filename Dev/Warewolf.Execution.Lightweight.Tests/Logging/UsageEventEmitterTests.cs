/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using Warewolf.Execution.Lightweight.Infrastructure;
using System.Diagnostics;
using System.Threading;
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
                MarketplaceResourceId = "",
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

            var calls = sink.WaitForCalls(1);
            Assert.AreEqual(1, calls.Count, "Emitter should call the sink exactly once.");
            Assert.AreEqual(UsageType.Usage, calls[0].UsageType);
            Assert.AreEqual("cust-123", calls[0].CustomerId);

            var payload = JObject.Parse(calls[0].UsageInfo);
            Assert.AreEqual("Hello World",                                 (string)payload["workflowName"]);
            Assert.AreEqual("11111111-2222-3333-4444-555555555555",        (string)payload["executionId"]);
            Assert.AreEqual(42L,                                            (long)payload["durationMs"]);
            Assert.AreEqual(true,                                           (bool)payload["isSuccess"]);
            Assert.AreEqual(0,                                              (int)payload["errorCount"]);
            Assert.AreEqual("LightweightExecution",                        (string)payload["source"]);
            Assert.AreEqual("sub-456",                                     (string)payload["subscriptionId"]);
            Assert.AreEqual("developer",                                   (string)payload["planId"]);
            Assert.AreEqual("",                                            (string)payload["marketplaceResourceId"]);
            Assert.AreEqual("Active",                                      (string)payload["status"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_PopulatedMarketplaceResourceId_IncludesItInPayload()
        {
            // A populated MarketplaceResourceId identifies an Azure Marketplace-billed
            // customer (vs. Chargebee) — Warewolf.Invoicing's reportMarketplaceUsage
            // function relies on this field being present in UsageData rows.
            var sink = new CapturingSink { Result = UsageDataResult.ok };
            var subscription = new FakeSubscriptionProvider
            {
                CustomerId            = "cust-789",
                MarketplaceResourceId = "8f14e45f-ceea-467e-abd0-2c1a1c8b9600",
                Status                = SubscriptionStatus.Active
            };

            var emitter = new UsageEventEmitter(sink, () => subscription);

            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.FromMilliseconds(5), true, 0, DateTime.UtcNow));

            Assert.AreEqual(1, sink.WaitForCalls(1).Count);
            var payload = JObject.Parse(sink.WaitForCalls(1)[0].UsageInfo);
            Assert.AreEqual("8f14e45f-ceea-467e-abd0-2c1a1c8b9600", (string)payload["marketplaceResourceId"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_EmptyCustomerId_FallsBackToUnRegistered()
        {
            var sink = new CapturingSink { Result = UsageDataResult.ok };
            var subscription = new FakeSubscriptionProvider { CustomerId = "" };

            var emitter = new UsageEventEmitter(sink, () => subscription);

            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));

            Assert.AreEqual("UnRegistered", sink.WaitForCalls(1)[0].CustomerId,
                "Empty CustomerId must fall back to 'UnRegistered' to match UsageLogger behaviour.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_SinkThrows_DoesNotPropagate()
        {
            var sink = new ThrowingSink();
            var subscription = new FakeSubscriptionProvider { CustomerId = "c" };

            var emitter = new UsageEventEmitter(sink, () => subscription);

            // Must not throw - usage emission is observability, never on the hot path.
            emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));

            // Drain so the throwing sink is ACTUALLY invoked. Without this the test passes purely
            // because TryWrite never throws, and would still pass if the consumer crashed the
            // process on a sink exception.
            Assert.IsTrue(emitter.WaitForDrain(TimeSpan.FromSeconds(5)), "queue should drain");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_NullEvent_DoesNotThrowOrEmit()
        {
            var sink = new CapturingSink();
            var emitter = new UsageEventEmitter(sink, () => new FakeSubscriptionProvider());

            emitter.TrackWorkflowExecution(null!);

            // Drain first: with queued emission, an immediate "count == 0" would pass even if the
            // null HAD been enqueued, so the assertion would prove nothing.
            Assert.IsTrue(emitter.WaitForDrain(TimeSpan.FromSeconds(5)), "queue should drain");
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

            var calls = sink.WaitForCalls(1);
            Assert.AreEqual(1, calls.Count);
            var payload = JObject.Parse(calls[0].UsageInfo);
            Assert.AreEqual(false, (bool)payload["isSuccess"]);
            Assert.AreEqual(2,     (int)payload["errorCount"]);
        }


        // ---------------------------------------------------------------------
        // 8520 - the defect: emission used to BLOCK the invocation
        // ---------------------------------------------------------------------

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_SlowSink_ReturnsImmediately_DoesNotBlockCaller()
        {
            // THE REGRESSION TEST FOR 8520.
            // UsagePublishMiddleware calls this emitter from its finally block, so a blocking
            // emitter blocks the Functions invocation from completing even though the workflow
            // has already finished. Measured live on 2026-09-02: the external usage backend
            // returned HTTP 400 after ~30 s for 21.6 % of calls, and those 30 s were charged to
            // the invocation - the entire throughput deficit of the QueueProcessor path.
            //
            // Against the previous inline implementation this test FAILS: the caller waits for
            // the full sink duration.
            var gate = new ManualResetEventSlim(false);
            var sink = new BlockingSink(gate);
            var emitter = new UsageEventEmitter(sink, () => new FakeSubscriptionProvider { CustomerId = "c" });

            try
            {
                var sw = Stopwatch.StartNew();
                emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                    "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));
                sw.Stop();

                Assert.IsTrue(sw.ElapsedMilliseconds < 500,
                    "TrackWorkflowExecution must return without waiting for the sink, but took " +
                    sw.ElapsedMilliseconds + " ms. Emission sits on the invocation's finally path.");

                Assert.IsTrue(sink.WaitUntilEntered(TimeSpan.FromSeconds(5)),
                    "the event should still reach the sink, just not on the caller's thread");
            }
            finally
            {
                gate.Set();
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TrackWorkflowExecution_QueueFull_DropsEventAndCountsIt_NeverBlocks()
        {
            // The deliberate trade-off: when the backend cannot keep up, the event is DISCARDED
            // rather than allowed to block an execution. That must be counted, never silent -
            // usage data is commercial metering.
            var gate = new ManualResetEventSlim(false);
            var sink = new BlockingSink(gate);

            // capacity 1 with a single consumer, and that consumer parks inside the sink.
            var emitter = new UsageEventEmitter(
                sink, () => new FakeSubscriptionProvider { CustomerId = "c" }, capacity: 1, consumers: 1);

            try
            {
                sink.WaitUntilEntered(TimeSpan.FromSeconds(5));

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 200; i++)
                {
                    emitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                        "wf", Guid.NewGuid(), TimeSpan.Zero, true, 0, DateTime.UtcNow));
                }
                sw.Stop();

                Assert.IsTrue(sw.ElapsedMilliseconds < 1000,
                    "200 enqueues against a stalled backend must not block; took " +
                    sw.ElapsedMilliseconds + " ms.");
                Assert.IsTrue(emitter.DroppedCount > 0,
                    "a full queue must record drops so the loss is visible in the log, never silent");
            }
            finally
            {
                gate.Set();
            }
        }

        // ---------------------------------------------------------------------
        // 8520 - the operational kill-switch
        // ---------------------------------------------------------------------

        [TestMethod]
        [TestCategory("UnitTest")]
        [DoNotParallelize]
        public void IsUsageTrackingEnabled_DefaultsToTrue_AndOnlyExplicitFalseDisablesIt()
        {
            // Metering defaults ON: an unset or unparseable value must never silently stop usage
            // reporting. Mirrors WAREWOLF_LICENSE_CHECK_ENABLED's convention exactly, so an
            // operator does not have to remember two truthiness rules.
            const string key = "WAREWOLF_USAGE_TRACKING_ENABLED";
            var saved = Environment.GetEnvironmentVariable(key);
            try
            {
                Environment.SetEnvironmentVariable(key, null);
                Assert.IsTrue(ServiceCollectionExtensions.IsUsageTrackingEnabled(), "absent must mean enabled");

                Environment.SetEnvironmentVariable(key, "   ");
                Assert.IsTrue(ServiceCollectionExtensions.IsUsageTrackingEnabled(), "blank must mean enabled");

                Environment.SetEnvironmentVariable(key, "banana");
                Assert.IsTrue(ServiceCollectionExtensions.IsUsageTrackingEnabled(),
                    "an unparseable value must not disable metering");

                Environment.SetEnvironmentVariable(key, "false");
                Assert.IsFalse(ServiceCollectionExtensions.IsUsageTrackingEnabled());

                Environment.SetEnvironmentVariable(key, "FALSE");
                Assert.IsFalse(ServiceCollectionExtensions.IsUsageTrackingEnabled(), "case-insensitive");

                Environment.SetEnvironmentVariable(key, "0");
                Assert.IsFalse(ServiceCollectionExtensions.IsUsageTrackingEnabled());
            }
            finally
            {
                Environment.SetEnvironmentVariable(key, saved);
            }
        }

        // ---------------------------------------------------------------------
        // Test doubles
        // ---------------------------------------------------------------------

        /// <summary>
        /// Thread-safe because emission is now QUEUED: the sink is invoked on a consumer thread,
        /// not the caller's. A plain List here would be both an assertion race and a data race.
        /// </summary>
        private sealed class CapturingSink : IUsageTrackerSink
        {
            readonly object _gate = new();
            readonly List<(string CustomerId, UsageType UsageType, string UsageInfo)> _calls = new();

            public UsageDataResult Result { get; set; } = UsageDataResult.ok;

            public IReadOnlyList<(string CustomerId, UsageType UsageType, string UsageInfo)> Calls
            {
                get { lock (_gate) { return _calls.ToArray(); } }
            }

            public UsageDataResult TrackEvent(string customerId, UsageType usageType, string usageInfo)
            {
                lock (_gate) { _calls.Add((customerId, usageType, usageInfo)); }
                return Result;
            }

            /// <summary>Waits for exactly <paramref name="expected"/> calls, then returns them.</summary>
            public IReadOnlyList<(string CustomerId, UsageType UsageType, string UsageInfo)> WaitForCalls(
                int expected, int timeoutMs = 10_000)
            {
                var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                while (DateTime.UtcNow < deadline)
                {
                    var snapshot = Calls;
                    if (snapshot.Count >= expected) return snapshot;
                    Thread.Sleep(10);
                }

                Assert.Fail($"Timed out waiting for {expected} usage call(s); saw {Calls.Count}. " +
                            "Emission is asynchronous - the consumer may be stuck or the event was dropped.");
                return Array.Empty<(string, UsageType, string)>();
            }
        }

        /// <summary>
        /// Blocks inside TrackEvent until released - stands in for the external usage backend
        /// taking 30 s to return an HTTP 400.
        /// </summary>
        private sealed class BlockingSink : IUsageTrackerSink
        {
            readonly ManualResetEventSlim _release;
            readonly ManualResetEventSlim _entered = new(false);

            public BlockingSink(ManualResetEventSlim release) => _release = release;

            public bool WaitUntilEntered(TimeSpan timeout) => _entered.Wait(timeout);

            public UsageDataResult TrackEvent(string customerId, UsageType usageType, string usageInfo)
            {
                _entered.Set();
                _release.Wait(TimeSpan.FromSeconds(30));
                return UsageDataResult.ok;
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
            public string MarketplaceResourceId { get; set; } = string.Empty;
            public bool   IsLicensed           { get; set; }
            public bool   StopExecutions       { get; set; }
            public SubscriptionStatus Status   { get; set; } = SubscriptionStatus.NotActive;

            public void SaveSubscriptionData(ISubscriptionData subscriptionData) { }
            public ISubscriptionData GetSubscriptionData() => null!;
            public ISubscriptionData DefaultSubscription() => null!;
        }
    }
}
