/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Runtime.Subscription;
using Newtonsoft.Json;
using Warewolf.Usage;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Default <see cref="IUsageEventEmitter"/> that forwards events to the shared
    /// <see cref="UsageTracker"/> (Warewolf.Usage) — the same path used by
    /// <c>Dev2.Server.ServerLifecycleManager</c> and <c>Dev2.Runtime.Services.UsageLogger</c>.
    /// The backend writes one row per call into the <c>UsageData</c> SQL table.
    ///
    /// <para><b>WHY THIS IS QUEUED AND NOT CALLED INLINE.</b>
    /// <see cref="UsageTracker.TrackEvent"/> is a SYNCHRONOUS, BLOCKING HTTP POST to
    /// <c>warewolfusageapi.azurewebsites.net/api/LogUsage</c>, an externally hosted service.
    /// <see cref="Infrastructure.UsagePublishMiddleware"/> calls this emitter from its
    /// <c>finally</c> block, so anything blocking here blocks the Functions invocation from
    /// completing — the workflow has finished, but the host cannot record it as done.</para>
    ///
    /// <para>Measured on 2026-09-02 against the deployed engine, 1427 invocations, one usage POST
    /// each (App Insights <c>dependencies</c>):</para>
    /// <list type="bullet">
    ///   <item>1119 calls returned <c>200</c> — p50 <b>63 ms</b>, p95 950 ms;</item>
    ///   <item>308 calls returned <c>400</c> — p50 <b>30 048 ms</b>, p95 30 147 ms, every one of
    ///   them over 20 s.</item>
    /// </list>
    /// That 21.6 % tail was <b>the entire throughput deficit</b> of the RabbitMQ QueueProcessor
    /// path: workflows completing in 64 ms were reported to the host 30 s later. The 400s appeared
    /// only once concurrency rose (zero during low-rate warm-up), which reads as throttling by the
    /// external service, returned as a 400 after a 30 s delay.
    ///
    /// <para>The previous implementation called the sink inline, and both its own class comment
    /// ("Workflow execution latency is not affected") and
    /// <see cref="Infrastructure.UsagePublishMiddleware"/>'s ("implementations are documented as
    /// non-throwing/non-blocking") asserted a contract the code did not keep. This type now keeps
    /// it: <see cref="TrackWorkflowExecution"/> only enqueues.</para>
    ///
    /// <para><b>DELIBERATE TRADE-OFF — usage events can be DROPPED.</b> The queue is bounded and
    /// full-queue writes are discarded rather than blocked, because blocking is the defect being
    /// fixed. Usage data is commercial metering, so a drop is not free: every drop is counted and
    /// reported through <see cref="Dev2Logger"/> (throttled, never silent, with a running total).
    /// If the external service is healthy the queue never fills — at 63 ms per call, two consumers
    /// sustain ~32 events/s, far above any observed execution rate. It fills only while that
    /// service is failing, which is exactly when blocking executions would be worse.</para>
    /// </summary>
    public sealed class UsageEventEmitter : IUsageEventEmitter, IAsyncDisposable
    {
        // Match UsageLogger.cs (Dev2.Runtime.Services) so the SQL row's CustomerId column matches
        // the legacy server's emission for the same customer.
        private const string UnregisteredCustomerId = "UnRegistered";

        /// <summary>
        /// Queue depth. Sized so a burst is absorbed rather than dropped: the largest measured run
        /// is 1000 messages, so 10 000 leaves an order of magnitude of headroom while still being a
        /// hard ceiling on memory (each entry is a small record).
        /// </summary>
        internal const int DefaultCapacity = 10_000;

        /// <summary>
        /// Two is deliberate, not arbitrary. The sink is a blocking network call, so each consumer
        /// costs one parked thread while the external service is slow. Two bounds that cost at two
        /// threads no matter how badly the service behaves, and still sustains ~32 events/s at the
        /// measured healthy latency of 63 ms.
        /// </summary>
        internal const int DefaultConsumers = 2;

        /// <summary>Minimum gap between drop warnings, so a sustained outage cannot flood the log.</summary>
        private static readonly TimeSpan DropWarningInterval = TimeSpan.FromSeconds(30);

        /// <summary>A single sink call slower than this is reported once per interval.</summary>
        private static readonly TimeSpan SlowSinkThreshold = TimeSpan.FromSeconds(5);

        private readonly IUsageTrackerSink _sink;
        private readonly Func<ISubscriptionProvider> _subscriptionProviderAccessor;
        private readonly Channel<WorkflowUsageEvent> _queue;
        private readonly Task[] _consumers;
        private readonly CancellationTokenSource _shutdown = new();

        private long _dropped;
        private long _lastDropWarningTicks;
        private long _lastSlowWarningTicks;

        /// <summary>
        /// Default production constructor — forwards to the real Warewolf.Usage backend and
        /// resolves the live <see cref="SubscriptionProvider.Instance"/>.
        /// </summary>
        public UsageEventEmitter()
            : this(DefaultUsageTrackerSink.Instance, () => SubscriptionProvider.Instance)
        {
        }

        /// <summary>Test seam — inject a sink + subscription accessor.</summary>
        internal UsageEventEmitter(
            IUsageTrackerSink sink,
            Func<ISubscriptionProvider> subscriptionProviderAccessor,
            int capacity = DefaultCapacity,
            int consumers = DefaultConsumers)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _subscriptionProviderAccessor = subscriptionProviderAccessor
                ?? throw new ArgumentNullException(nameof(subscriptionProviderAccessor));

            // FullMode.Wait paired with TryWrite (never WriteAsync) is deliberate and subtle.
            //
            // The obvious choice looks like BoundedChannelFullMode.DropWrite, but with ANY Drop*
            // mode TryWrite returns TRUE even though the item was silently discarded - so the
            // drop is undetectable and DroppedCount below would always read 0. That is not a
            // theoretical concern: it was the first implementation here, and
            // TrackWorkflowExecution_QueueFull_DropsEventAndCountsIt_NeverBlocks caught it.
            //
            // FullMode.Wait only ever blocks a WRITER THAT WAITS, i.e. WriteAsync. TryWrite does
            // not wait - it returns false when the channel is full - so this combination is
            // non-blocking AND reports the drop. Never call WriteAsync on this channel.
            _queue = Channel.CreateBounded<WorkflowUsageEvent>(new BoundedChannelOptions(Math.Max(1, capacity))
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false,
            });

            var count = Math.Max(1, consumers);
            _consumers = new Task[count];
            for (var i = 0; i < count; i++)
            {
                // LongRunning: each consumer parks on a blocking network call, so it must not
                // occupy a thread-pool worker that the Functions host needs for invocations.
                _consumers[i] = Task.Factory.StartNew(
                    ConsumeAsync,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                    TaskScheduler.Default).Unwrap();
            }
        }

        /// <summary>Total events discarded because the queue was full. Diagnostics and tests.</summary>
        internal long DroppedCount => Interlocked.Read(ref _dropped);

        /// <summary>
        /// Enqueues the event and returns. Does no network I/O, no serialization and no
        /// subscription lookup — all of that moved to <see cref="ConsumeAsync"/> so the invocation
        /// path costs a single bounded-channel write.
        /// </summary>
        public void TrackWorkflowExecution(WorkflowUsageEvent evt)
        {
            if (evt == null) return;

            if (_queue.Writer.TryWrite(evt)) return;

            // TryWrite returns false when the channel is completed (shutdown) or full. Either way
            // the event is gone; make that visible rather than silent.
            var total = Interlocked.Increment(ref _dropped);
            ReportDrop(evt, total);
        }

        private void ReportDrop(WorkflowUsageEvent evt, long total)
        {
            var now = DateTime.UtcNow.Ticks;
            var last = Interlocked.Read(ref _lastDropWarningTicks);
            if (now - last < DropWarningInterval.Ticks) return;
            if (Interlocked.CompareExchange(ref _lastDropWarningTicks, now, last) != last) return;

            Dev2Logger.Warn(
                $"UsageEventEmitter: usage queue full or closed - DISCARDED the event for workflow " +
                $"'{evt.WorkflowName}' (executionId={evt.ExecutionId}). {total} event(s) dropped in total. " +
                "This means the usage backend is not keeping up; executions are unaffected by design.",
                GlobalConstants.UsageTracker);
        }

        private async Task ConsumeAsync()
        {
            try
            {
                while (await _queue.Reader.WaitToReadAsync(_shutdown.Token).ConfigureAwait(false))
                {
                    while (_queue.Reader.TryRead(out var evt))
                    {
                        Emit(evt);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Shutdown. Drain whatever is already queued so a graceful stop does not lose
                // events that were accepted; anything still arriving is dropped and counted.
                while (_queue.Reader.TryRead(out var evt))
                {
                    Emit(evt);
                }
            }
            catch (Exception ex)
            {
                // A consumer must never die silently, or usage reporting stops for the life of the
                // process with no trace.
                Dev2Logger.Error(
                    $"UsageEventEmitter: usage consumer terminated unexpectedly: {ex.Message}",
                    GlobalConstants.UsageTracker);
            }
        }

        /// <summary>
        /// The blocking part, now off the invocation path. Every failure mode is contained here:
        /// this method never throws.
        /// </summary>
        private void Emit(WorkflowUsageEvent evt)
        {
            if (evt == null) return;

            try
            {
                var subscription = _subscriptionProviderAccessor();
                var customerId = string.IsNullOrEmpty(subscription?.CustomerId)
                    ? UnregisteredCustomerId
                    : subscription.CustomerId;

                var payload = JsonConvert.SerializeObject(new
                {
                    workflowName = evt.WorkflowName,
                    executionId = evt.ExecutionId,
                    durationMs = (long)evt.Duration.TotalMilliseconds,
                    isSuccess = evt.IsSuccess,
                    errorCount = evt.ErrorCount,
                    startedAtUtc = evt.StartedAtUtc,
                    subscriptionId = subscription?.SubscriptionId ?? string.Empty,
                    planId = subscription?.PlanId ?? string.Empty,
                    marketplaceResourceId = subscription?.MarketplaceResourceId ?? string.Empty,
                    status = subscription?.Status.ToString() ?? string.Empty,
                    source = "LightweightExecution",   // identifies the lightweight engine in the UsageData row
                    machineName = Environment.MachineName
                });

                var startedAt = DateTime.UtcNow;
                var result = _sink.TrackEvent(customerId, UsageType.Usage, payload);
                var elapsed = DateTime.UtcNow - startedAt;

                if (elapsed > SlowSinkThreshold)
                {
                    ReportSlowSink(elapsed);
                }

                if (result != UsageDataResult.ok)
                {
                    Dev2Logger.Warn(
                        $"UsageEventEmitter: TrackEvent returned {result} for workflow '{evt.WorkflowName}' (executionId={evt.ExecutionId}).",
                        GlobalConstants.UsageTracker);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn(
                    $"UsageEventEmitter: failed to emit usage event for workflow '{evt.WorkflowName}': {ex.Message}",
                    GlobalConstants.UsageTracker);
            }
        }

        private void ReportSlowSink(TimeSpan elapsed)
        {
            var now = DateTime.UtcNow.Ticks;
            var last = Interlocked.Read(ref _lastSlowWarningTicks);
            if (now - last < DropWarningInterval.Ticks) return;
            if (Interlocked.CompareExchange(ref _lastSlowWarningTicks, now, last) != last) return;

            Dev2Logger.Warn(
                $"UsageEventEmitter: the usage backend took {elapsed.TotalSeconds:F1}s for a single event. " +
                "Executions are NOT affected (emission is queued), but usage rows will lag and may be " +
                "dropped if this persists.",
                GlobalConstants.UsageTracker);
        }

        /// <summary>
        /// Waits until the queue is empty, for tests. Returns false on timeout.
        /// <para>Needed because the emitter is now asynchronous: a test that asserts on the sink
        /// immediately after <see cref="TrackWorkflowExecution"/> would race the consumer.</para>
        /// </summary>
        internal bool WaitForDrain(TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (_queue.Reader.Count == 0)
                {
                    // Count hits zero when the last item is TAKEN, not when it finishes being
                    // emitted, so give the in-flight Emit a moment to complete.
                    Thread.Sleep(25);
                    if (_queue.Reader.Count == 0) return true;
                }

                Thread.Sleep(10);
            }

            return _queue.Reader.Count == 0;
        }

        /// <summary>
        /// Stops accepting events and lets the consumers drain what is already queued, bounded so
        /// shutdown cannot hang behind a slow external service.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _queue.Writer.TryComplete();

            try
            {
                var drained = Task.WhenAll(_consumers);
                await Task.WhenAny(drained, Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);
            }
            catch
            {
                // Shutdown path - a consumer fault here must not mask the real shutdown reason.
            }

            _shutdown.Cancel();
            _shutdown.Dispose();
        }
    }

    /// <summary>
    /// Test-seam abstraction over the static <see cref="UsageTracker.TrackEvent"/> call so unit
    /// tests can capture invocations without hitting the network.
    /// </summary>
    public interface IUsageTrackerSink
    {
        UsageDataResult TrackEvent(string customerId, UsageType usageType, string usageInfo);
    }

    internal sealed class DefaultUsageTrackerSink : IUsageTrackerSink
    {
        public static readonly DefaultUsageTrackerSink Instance = new();
        private DefaultUsageTrackerSink() { }

        public UsageDataResult TrackEvent(string customerId, UsageType usageType, string usageInfo)
            => UsageTracker.TrackEvent(customerId, usageType, usageInfo);
    }
}
