/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusTriggerInstanceHeartbeat — the build-30653-addendum
 *  background service that records this instance's own liveness (independent of any
 *  single Service Bus correlation id), so a future silent-death recurrence can tell
 *  whether the WHOLE instance died versus a single execution merely hanging while its
 *  instance stayed alive. See IServiceBusReplayAndResultStore.RecordInstanceHeartbeat's
 *  doc comment for the full rationale.
 */

using Hangfire.MemoryStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure;

[TestClass]
public class ServiceBusTriggerInstanceHeartbeatTests
{
    private static ServiceBusTriggerInstanceHeartbeat NewSut(
        IServiceBusReplayAndResultStore store,
        SemaphoreSlim executionConcurrencyLimiter,
        ServiceBusTriggerOptions triggerOptions) =>
        new(store, executionConcurrencyLimiter, triggerOptions, HostEnvironmentConfig.Load());

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteAsync_TicksAtLeastOnce_RecordsThisInstancesHeartbeat()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 4, HeartbeatInterval = TimeSpan.FromMilliseconds(20) };
        using var limiter = new SemaphoreSlim(triggerOptions.MaxConcurrentExecutions, triggerOptions.MaxConcurrentExecutions);
        var sut = NewSut(store, limiter, triggerOptions);
        var config = HostEnvironmentConfig.Load();

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(80));
        await sut.StopAsync(CancellationToken.None);

        var (inFlight, atUtc) = store.GetInstanceHeartbeatForTests(config.InstanceId);
        Assert.IsNotNull(atUtc, "Expected at least one heartbeat tick within 80ms at a 20ms interval.");
        Assert.AreEqual(0, inFlight, "No execution has taken a concurrency slot yet, so in-flight must be 0.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteAsync_WithSlotsHeld_RecordsInFlightCountAsMaxMinusCurrentCount()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 4, HeartbeatInterval = TimeSpan.FromMilliseconds(20) };
        using var limiter = new SemaphoreSlim(triggerOptions.MaxConcurrentExecutions, triggerOptions.MaxConcurrentExecutions);
        // Simulate 3 in-flight executions holding a slot each.
        await limiter.WaitAsync();
        await limiter.WaitAsync();
        await limiter.WaitAsync();
        var sut = NewSut(store, limiter, triggerOptions);
        var config = HostEnvironmentConfig.Load();

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(80));
        await sut.StopAsync(CancellationToken.None);

        var (inFlight, _) = store.GetInstanceHeartbeatForTests(config.InstanceId);
        Assert.AreEqual(3, inFlight);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteAsync_TicksMultipleTimes_LatestHeartbeatKeepsAdvancing()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 2, HeartbeatInterval = TimeSpan.FromMilliseconds(15) };
        using var limiter = new SemaphoreSlim(triggerOptions.MaxConcurrentExecutions, triggerOptions.MaxConcurrentExecutions);
        var sut = NewSut(store, limiter, triggerOptions);
        var config = HostEnvironmentConfig.Load();

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(50));
        var (_, firstAtUtc) = store.GetInstanceHeartbeatForTests(config.InstanceId);
        await Task.Delay(TimeSpan.FromMilliseconds(80));
        await sut.StopAsync(CancellationToken.None);

        var (_, secondAtUtc) = store.GetInstanceHeartbeatForTests(config.InstanceId);
        Assert.IsNotNull(firstAtUtc);
        Assert.IsNotNull(secondAtUtc);
        Assert.IsTrue(secondAtUtc > firstAtUtc, "A later read must observe a more recent heartbeat timestamp - proof the loop keeps ticking, not just firing once.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task StopAsync_StopsFurtherTicks()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 2, HeartbeatInterval = TimeSpan.FromMilliseconds(15) };
        using var limiter = new SemaphoreSlim(triggerOptions.MaxConcurrentExecutions, triggerOptions.MaxConcurrentExecutions);
        var sut = NewSut(store, limiter, triggerOptions);
        var config = HostEnvironmentConfig.Load();

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(50));
        await sut.StopAsync(CancellationToken.None);
        var (_, atStopUtc) = store.GetInstanceHeartbeatForTests(config.InstanceId);
        await Task.Delay(TimeSpan.FromMilliseconds(80));

        var (_, afterDelayUtc) = store.GetInstanceHeartbeatForTests(config.InstanceId);
        Assert.AreEqual(atStopUtc, afterDelayUtc, "No further heartbeat should be recorded once the background service has stopped.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteAsync_StoreThrows_TickFailureIsSwallowed_LoopKeepsRunning()
    {
        var throwingStore = new ThrowingHeartbeatStore();
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 2, HeartbeatInterval = TimeSpan.FromMilliseconds(15) };
        using var limiter = new SemaphoreSlim(triggerOptions.MaxConcurrentExecutions, triggerOptions.MaxConcurrentExecutions);
        var sut = NewSut(throwingStore, limiter, triggerOptions);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(60));

        // Must not have crashed the background service - StopAsync completing normally
        // (not hanging, not throwing) is the proof: a crashed BackgroundService's task
        // would fault, and awaiting StopAsync after a fault surfaces that exception.
        await sut.StopAsync(CancellationToken.None);
        Assert.IsTrue(throwingStore.CallCount > 1, "A single throwing tick must not stop subsequent ticks from being attempted.");
    }

    private sealed class ThrowingHeartbeatStore : IServiceBusReplayAndResultStore
    {
        public int CallCount { get; private set; }

        public void RecordInstanceHeartbeat(string instanceId, int inFlightExecutions)
        {
            CallCount++;
            throw new InvalidOperationException("simulated heartbeat write failure");
        }

        public bool TryRegisterJti(string jti) => true;
        public bool TryGetResult(string correlationId, out Models.ServiceBusTriggerResult? result) { result = null; return false; }
        public void SaveResult(Models.ServiceBusTriggerResult result) { }
        public bool TryClaim(string correlationId) => true;
        public void ReleaseClaim(string correlationId) { }
        public void RecordClaimStage(string correlationId, string stage) { }
    }
}
