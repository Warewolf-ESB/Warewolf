/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Emits per-execution usage telemetry to the Warewolf usage backend
    /// (the same backend the legacy server's <c>ServerLifecycleManager</c>
    /// reports server start/stop events to).
    ///
    /// <para>
    /// Implementations MUST be non-throwing and non-blocking — usage emission
    /// is observability, never on the workflow execution hot path.  Any failure
    /// is logged and swallowed.
    /// </para>
    /// </summary>
    public interface IUsageEventEmitter
    {
        /// <summary>
        /// Records a single workflow execution to the usage backend.
        /// </summary>
        void TrackWorkflowExecution(WorkflowUsageEvent evt);
    }

    /// <summary>
    /// Immutable data describing a single workflow execution to be reported to
    /// the usage backend.  Mirrors the per-execution shape so the legacy
    /// <c>UsageData</c> SQL table can correlate executions across the legacy
    /// server and the lightweight engine.
    /// </summary>
    public sealed class WorkflowUsageEvent
    {
        public string WorkflowName { get; }
        public Guid ExecutionId { get; }
        public TimeSpan Duration { get; }
        public bool IsSuccess { get; }
        public int ErrorCount { get; }
        public DateTime StartedAtUtc { get; }

        public WorkflowUsageEvent(
            string workflowName,
            Guid executionId,
            TimeSpan duration,
            bool isSuccess,
            int errorCount,
            DateTime startedAtUtc)
        {
            WorkflowName = workflowName ?? string.Empty;
            ExecutionId  = executionId;
            Duration     = duration;
            IsSuccess    = isSuccess;
            ErrorCount   = errorCount;
            StartedAtUtc = startedAtUtc;
        }
    }

    /// <summary>
    /// Null-object emitter used when no real emitter is supplied (e.g. in unit
    /// tests that don't care about usage tracking).  Keeps <c>WorkflowExecutor</c>
    /// branch-free on the hot path.
    /// </summary>
    internal sealed class NoOpUsageEventEmitter : IUsageEventEmitter
    {
        public static readonly NoOpUsageEventEmitter Instance = new();
        private NoOpUsageEventEmitter() { }
        public void TrackWorkflowExecution(WorkflowUsageEvent evt) { }
    }
}
