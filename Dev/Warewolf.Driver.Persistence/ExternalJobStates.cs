/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using Hangfire.Common;
using Hangfire.States;

namespace Warewolf.Driver.Persistence
{
    /// <summary>
    /// Hangfire job states applied by an EXTERNAL executor (the Azure Execution Engine)
    /// instead of a Hangfire worker.
    ///
    /// Hangfire's own <see cref="ProcessingState"/> / <see cref="SucceededState"/> have
    /// internal constructors — they can only be created by a <c>BackgroundJobServer</c>
    /// worker, which the Azure topology deliberately never runs. These equivalents carry
    /// the same canonical state <b>names</b> and the same serialized-data keys, so
    /// Hangfire's monitoring API, the dashboard, and the existing state guards in
    /// <c>HangfireScheduler</c> (which compare state names) treat them identically.
    /// Follows the <see cref="ManuallyResumedState"/> custom-state precedent.
    /// </summary>
    public sealed class ExternalProcessingState : IState
    {
        public ExternalProcessingState(string serverId, string workerId)
        {
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            WorkerId = workerId ?? string.Empty;
            StartedAt = DateTime.UtcNow;
        }

        public string ServerId { get; }
        public string WorkerId { get; }
        public DateTime StartedAt { get; }

        public string Name => ProcessingState.StateName;
        public string Reason => "Claimed by the Execution Engine resume route";
        public bool IsFinal => false;
        public bool IgnoreJobLoadException => false;

        // Same keys as Hangfire's ProcessingState.SerializeData so MonitoringApi's
        // ProcessingJobs (StartedAt/ServerId) — and therefore the JobProcessor's
        // reaper — read this state exactly like a worker-created one.
        public Dictionary<string, string> SerializeData() => new()
        {
            { "StartedAt", JobHelper.SerializeDateTime(StartedAt) },
            { "ServerId", ServerId },
            { "WorkerId", WorkerId },
        };
    }

    /// <summary>
    /// Terminal success state recorded by the external executor — canonical name and
    /// data keys of Hangfire's <see cref="SucceededState"/> (internal ctor), so
    /// monitoring, dashboards, and the driver's "already resumed" guards behave
    /// identically. Being final, the state machine applies the normal job expiration.
    /// </summary>
    public sealed class ExternalSucceededState : IState
    {
        public ExternalSucceededState(long latencyMilliseconds, long performanceDurationMilliseconds)
        {
            SucceededAt = DateTime.UtcNow;
            Latency = latencyMilliseconds;
            PerformanceDuration = performanceDurationMilliseconds;
        }

        public DateTime SucceededAt { get; }
        public long Latency { get; }
        public long PerformanceDuration { get; }

        public string Name => SucceededState.StateName;
        public string Reason => "Executed by the Execution Engine";
        public bool IsFinal => true;
        public bool IgnoreJobLoadException => false;

        public Dictionary<string, string> SerializeData() => new()
        {
            { "SucceededAt", JobHelper.SerializeDateTime(SucceededAt) },
            { "PerformanceDuration", PerformanceDuration.ToString(CultureInfo.InvariantCulture) },
            { "Latency", Latency.ToString(CultureInfo.InvariantCulture) },
        };
    }
}
