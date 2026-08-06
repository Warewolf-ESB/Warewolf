/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Threading;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Ambient (per-async-flow) holder for the workflow-execution facts a usage
    /// event needs — populated deep inside <c>WorkflowExecutor.Execute</c> once the
    /// workflow name / execution id / outcome are known, and consumed by
    /// <see cref="Infrastructure.UsagePublishMiddleware"/> after the ENTIRE request
    /// pipeline (auth included) has completed.
    ///
    /// <para>
    /// This indirection is what lets the timing boundary live in middleware
    /// registered first in the pipeline while the workflow-specific payload fields
    /// (name, execution id, success, error count) still come from the executor —
    /// without the executor needing any knowledge of <c>FunctionContext</c> or the
    /// Functions worker middleware pipeline.
    /// </para>
    ///
    /// <para>
    /// Uses <see cref="AsyncLocal{T}"/> (the same pattern as
    /// <see cref="Infrastructure.InstanceCorrelationContext"/>) so the value flows
    /// with the async call chain of a single invocation and never leaks across
    /// concurrent requests.
    /// </para>
    ///
    /// <para>
    /// <b>Important:</b> the <see cref="AsyncLocal{T}"/> slot is only reliable
    /// while everything stays on the SAME synchronous call stack — once code
    /// resumes on a continuation after crossing a genuine <c>await</c>
    /// suspension point, a write made further down the call chain is no longer
    /// guaranteed to flow back up (this was verified empirically: a value set
    /// inside an awaited child after it had itself awaited something was not
    /// visible to the caller once that child's task completed). Because
    /// <c>WorkflowHttpFunction</c> calls <c>WorkflowExecutor.Execute</c>
    /// synchronously (no <c>await</c> in between), reading <see cref="Current"/>
    /// immediately afterwards — in the very same synchronous frame, via
    /// <see cref="TakeCurrent"/> — is safe. That caller then hands the value off
    /// to <c>FunctionContext.Items</c> (a plain mutable dictionary tied to the
    /// <c>FunctionContext</c> object identity, not to async-flow copying) under
    /// <see cref="ItemsKey"/>, which is what <see cref="Infrastructure.UsagePublishMiddleware"/>
    /// actually reads after <c>await next(context)</c> returns — surviving any
    /// number of awaits elsewhere in the pipeline.
    /// </para>
    /// </summary>
    public sealed class UsagePublishContext
    {
        static readonly AsyncLocal<UsagePublishContext?> _current = new();

        /// <summary>
        /// Well-known <c>FunctionContext.Items</c> key under which the captured
        /// usage facts are stashed for <see cref="Infrastructure.UsagePublishMiddleware"/>
        /// to pick up once the downstream pipeline has finished.
        /// </summary>
        public const string ItemsKey = "Warewolf.UsagePublishContext";

        /// <summary>
        /// The usage facts recorded for the in-flight invocation, or <c>null</c>
        /// when no workflow execution has (yet) recorded any — e.g. requests that
        /// never reach <c>WorkflowExecutor.Execute</c> (health checks, licensing,
        /// login, auth denials) intentionally produce no usage event, matching the
        /// pre-existing behaviour.
        /// </summary>
        public static UsagePublishContext? Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        /// <summary>
        /// Reads and clears the ambient value in one step. Must be called on the
        /// same synchronous call stack as the <c>WorkflowExecutor.Execute</c> call
        /// that may have populated it — see the AsyncLocal caveat above.
        /// </summary>
        public static UsagePublishContext? TakeCurrent()
        {
            var value = _current.Value;
            _current.Value = null;
            return value;
        }

        public string WorkflowName { get; init; } = string.Empty;
        public Guid ExecutionId { get; init; }
        public bool IsSuccess { get; init; }
        public int ErrorCount { get; init; }
    }
}
