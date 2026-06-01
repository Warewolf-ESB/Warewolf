/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Dev2.Common;
using Dev2.Runtime.Subscription;
using Newtonsoft.Json;
using Warewolf.Usage;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Default <see cref="IUsageEventEmitter"/> that forwards events to the
    /// shared <see cref="UsageTracker"/> (Warewolf.Usage) — the same path used
    /// by <c>Dev2.Server.ServerLifecycleManager</c> and
    /// <c>Dev2.Runtime.Services.UsageLogger</c> for ServerStart / ServerStop
    /// reporting.  The backend writes one row per call into the
    /// <c>UsageData</c> SQL table.
    ///
    /// <para>
    /// All work runs inside a <c>try/catch</c>; a failing emit (network error,
    /// missing license file, etc.) is logged at <see cref="Dev2Logger.Warn"/>
    /// and never surfaces to the caller.  Workflow execution latency is not
    /// affected: emission happens AFTER the result is built.
    /// </para>
    /// </summary>
    public sealed class UsageEventEmitter : IUsageEventEmitter
    {
        // Final fallback when nothing in SubscriptionProvider yields a usable
        // identifier — mirrors UsageLogger.cs (Dev2.Runtime.Services) so the
        // SQL row's CustomerId column matches the legacy server's emission
        // for a true unregistered install.
        private const string UnregisteredCustomerId = "UnRegistered";

        // Optional operator-set override.  When set, this value is used as
        // the CustomerId on every emit regardless of what SubscriptionProvider
        // returns — useful for tagging dev / test / preview runs so their
        // rows in the shared UsageData table are easy to find.
        private const string CustomerIdOverrideEnvVar = "WAREWOLF_USAGE_CUSTOMER_ID";

        private readonly IUsageTrackerSink _sink;
        private readonly Func<ISubscriptionProvider> _subscriptionProviderAccessor;

        /// <summary>
        /// Default production constructor — forwards to the real Warewolf.Usage
        /// backend and resolves the live <see cref="SubscriptionProvider.Instance"/>.
        /// </summary>
        public UsageEventEmitter()
            : this(DefaultUsageTrackerSink.Instance, () => SubscriptionProvider.Instance)
        {
        }

        /// <summary>
        /// Test seam — inject a sink + subscription accessor.
        /// </summary>
        internal UsageEventEmitter(
            IUsageTrackerSink sink,
            Func<ISubscriptionProvider> subscriptionProviderAccessor)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _subscriptionProviderAccessor = subscriptionProviderAccessor
                ?? throw new ArgumentNullException(nameof(subscriptionProviderAccessor));
        }

        public void TrackWorkflowExecution(WorkflowUsageEvent evt)
        {
            if (evt == null) return;

            try
            {
                var subscription = _subscriptionProviderAccessor();
                var customerId   = ResolveCustomerId(subscription);

                var payload = JsonConvert.SerializeObject(new
                {
                    workflowName    = evt.WorkflowName,
                    executionId     = evt.ExecutionId,
                    durationMs      = (long)evt.Duration.TotalMilliseconds,
                    isSuccess       = evt.IsSuccess,
                    errorCount      = evt.ErrorCount,
                    startedAtUtc    = evt.StartedAtUtc,
                    subscriptionId  = subscription?.SubscriptionId ?? string.Empty,
                    planId          = subscription?.PlanId ?? string.Empty,
                    status          = subscription?.Status.ToString() ?? string.Empty,
                    source          = "WolfClaw",       // identifies the lightweight engine in the UsageData row
                    machineName     = Environment.MachineName
                });

                var result = _sink.TrackEvent(customerId, UsageType.Usage, payload);
                if (result != UsageDataResult.ok)
                {
                    Dev2Logger.Warn(
                        $"UsageEventEmitter: TrackEvent returned {result} for workflow '{evt.WorkflowName}' (executionId={evt.ExecutionId}).",
                        GlobalConstants.UsageTracker);
                }
            }
            catch (Exception ex)
            {
                // Never let usage emission affect the response.
                Dev2Logger.Warn(
                    $"UsageEventEmitter: failed to emit usage event for workflow '{evt.WorkflowName}': {ex.Message}",
                    GlobalConstants.UsageTracker);
            }
        }

        /// <summary>
        /// Builds the <c>CustomerId</c> column value for the UsageData row, in
        /// preference order:
        /// <list type="number">
        ///   <item><c>WAREWOLF_USAGE_CUSTOMER_ID</c> env-var override.</item>
        ///   <item><see cref="ISubscriptionProvider.CustomerId"/> when non-empty.</item>
        ///   <item>
        ///     <see cref="ISubscriptionProvider.SubscriptionKey"/> when non-empty
        ///     (dev licences ship with this set but with an empty CustomerId).
        ///   </item>
        ///   <item><see cref="ISubscriptionProvider.SubscriptionSiteName"/> when non-empty.</item>
        ///   <item><c>"UnRegistered"</c> — final fallback, matches legacy server.</item>
        /// </list>
        /// Internal so the unit tests can exercise the chain directly.
        /// </summary>
        internal static string ResolveCustomerId(ISubscriptionProvider? subscription)
        {
            var envOverride = Environment.GetEnvironmentVariable(CustomerIdOverrideEnvVar);
            if (!string.IsNullOrWhiteSpace(envOverride))
                return envOverride;

            if (subscription is null)
                return UnregisteredCustomerId;

            if (!string.IsNullOrWhiteSpace(subscription.CustomerId))
                return subscription.CustomerId;

            if (!string.IsNullOrWhiteSpace(subscription.SubscriptionKey))
                return subscription.SubscriptionKey;

            if (!string.IsNullOrWhiteSpace(subscription.SubscriptionSiteName))
                return subscription.SubscriptionSiteName;

            return UnregisteredCustomerId;
        }
    }

    /// <summary>
    /// Test-seam abstraction over the static <see cref="UsageTracker.TrackEvent"/>
    /// call so unit tests can capture invocations without hitting the network.
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
