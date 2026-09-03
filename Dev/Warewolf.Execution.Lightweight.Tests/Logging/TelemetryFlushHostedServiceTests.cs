/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.Lightweight.Tests.Logging
{
    /// <summary>
    /// Coverage for <see cref="TelemetryFlushHostedService"/> — the WOLF-8512 fix for the
    /// 2026-08-30/31 ShovelBridge load-test finding that a Consumption-plan instance recycled
    /// mid-burst silently discards its buffered Application Insights telemetry because nothing
    /// in this project ever called <see cref="TelemetryClient.Flush"/> before shutdown.
    /// </summary>
    [TestClass]
    public class TelemetryFlushHostedServiceTests
    {
        static TelemetryClient BuildClient(out FakeTelemetryChannel channel)
        {
            channel = new FakeTelemetryChannel();
            var configuration = new TelemetryConfiguration
            {
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000",
                TelemetryChannel = channel,
            };
            return new TelemetryClient(configuration);
        }

        // ── Constructor guard ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Ctor_NullTelemetryClient_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new TelemetryFlushHostedService(null!));
        }

        // ── StartAsync ───────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task StartAsync_CompletesWithoutFlushing()
        {
            var client = BuildClient(out var channel);
            var service = new TelemetryFlushHostedService(client, TimeSpan.Zero);

            await service.StartAsync(CancellationToken.None);

            Assert.AreEqual(0, channel.FlushCallCount);
        }

        // ── StopAsync ────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task StopAsync_FlushesTheChannelExactlyOnce()
        {
            var client = BuildClient(out var channel);
            var service = new TelemetryFlushHostedService(client, TimeSpan.Zero);

            await service.StopAsync(CancellationToken.None);

            Assert.AreEqual(1, channel.FlushCallCount);
        }

        // Task.Delay's actual wall-clock accuracy is bound by the OS timer resolution (commonly
        // ~15ms on Windows), so it can legitimately fire a few ms short of the requested delay.
        // Timing assertions below tolerate that slop rather than asserting an exact lower bound.
        static readonly TimeSpan TimerTolerance = TimeSpan.FromMilliseconds(50);

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task StopAsync_WaitsForTheConfiguredFlushWindow()
        {
            var client = BuildClient(out _);
            var wait = TimeSpan.FromMilliseconds(200);
            var service = new TelemetryFlushHostedService(client, wait);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await service.StopAsync(CancellationToken.None);
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.Elapsed >= wait - TimerTolerance,
                $"Expected StopAsync to wait close to {wait}, actually waited {stopwatch.Elapsed}.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task StopAsync_IgnoresTheCancellationTokenForItsOwnWait()
        {
            // Shutdown already signalled cancellation by the time StopAsync runs - honouring
            // it for the post-Flush wait would skip the wait entirely and reopen the exact gap
            // this service exists to close, so an already-cancelled token must NOT shorten it.
            var client = BuildClient(out var channel);
            var wait = TimeSpan.FromMilliseconds(200);
            var service = new TelemetryFlushHostedService(client, wait);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await service.StopAsync(cts.Token);
            stopwatch.Stop();

            Assert.AreEqual(1, channel.FlushCallCount);
            Assert.IsTrue(stopwatch.Elapsed >= wait - TimerTolerance,
                $"Expected StopAsync to still wait close to {wait} despite a cancelled token, actually waited {stopwatch.Elapsed}.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task StopAsync_ChannelFlushThrows_DoesNotPropagate()
        {
            var client = BuildClient(out var channel);
            channel.ThrowOnFlush = true;
            var service = new TelemetryFlushHostedService(client, TimeSpan.Zero);

            await service.StopAsync(CancellationToken.None);

            Assert.AreEqual(1, channel.FlushCallCount);
        }

        // ── Test double ──────────────────────────────────────────────────────

        sealed class FakeTelemetryChannel : ITelemetryChannel
        {
            public int FlushCallCount { get; private set; }
            public bool ThrowOnFlush { get; set; }

            public bool? DeveloperMode { get; set; }
            public string EndpointAddress { get; set; } = string.Empty;

            public void Dispose() { }

            public void Flush()
            {
                FlushCallCount++;
                if (ThrowOnFlush)
                {
                    throw new InvalidOperationException("Simulated channel failure.");
                }
            }

            public void Send(ITelemetry item) { }
        }
    }
}
