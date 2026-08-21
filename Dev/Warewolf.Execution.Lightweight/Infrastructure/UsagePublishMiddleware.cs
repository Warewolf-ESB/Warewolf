/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.Lightweight.Infrastructure
{
    /// <summary>
    /// Functions worker middleware that publishes per-invocation usage/uptime
    /// telemetry to <see cref="IUsageEventEmitter"/>.
    ///
    /// <para><b>MUST be registered first</b> in the pipeline (ahead of instance
    /// correlation, Easy Auth, claims building, and authorization) so its
    /// stopwatch spans the FULL request/response lifecycle rather than just the
    /// workflow-execution portion. Because ASP.NET Core / Functions middleware
    /// runs its pre-<c>next()</c> code first-in, the code AFTER <c>await next()</c>
    /// still runs last-out — after every downstream middleware and the function
    /// body have completed — so being registered first does not stop this
    /// middleware from observing the full elapsed time or the final outcome.</para>
    ///
    /// <para><b>Where the payload comes from:</b> the workflow name / execution id
    /// / success / error-count fields are not known until deep inside
    /// <c>WorkflowExecutor.Execute</c>, which runs many middlewares and the function
    /// body downstream of this one. <c>WorkflowExecutor</c> records those facts into
    /// the <see cref="UsagePublishContext"/> ambient (AsyncLocal) slot; the calling
    /// Function method (still on the same synchronous call stack as the
    /// <c>Execute</c> call) immediately transfers that value into
    /// <c>FunctionContext.Items[UsagePublishContext.ItemsKey]</c> — a plain object
    /// keyed on the <c>FunctionContext</c> instance itself rather than on
    /// async-flow copying, so it reliably survives any <c>await</c>s elsewhere in
    /// the pipeline. This middleware reads that <c>Items</c> entry once
    /// <c>next()</c> returns and combines it with the full-pipeline
    /// duration/start time it measured itself.</para>
    ///
    /// <para>Requests that never reach <c>WorkflowExecutor.Execute</c> — health
    /// checks, login, licensing, apis.json, auth denials — never populate
    /// <see cref="UsagePublishContext.ItemsKey"/> in <c>context.Items</c> and
    /// therefore emit no usage event, matching the pre-existing (pre-move)
    /// behaviour where only workflow executions were tracked.</para>
    ///
    /// <para>Publishing happens in a <c>finally</c> block so it still runs on error
    /// paths (an unhandled exception thrown by downstream middleware/function
    /// code); the exception itself is never swallowed — it propagates after the
    /// usage event (if any) is published. Emission itself never throws:
    /// <see cref="IUsageEventEmitter"/> implementations are documented as
    /// non-throwing/non-blocking.</para>
    /// </summary>
    public sealed class UsagePublishMiddleware : IFunctionsWorkerMiddleware
    {
        readonly IUsageEventEmitter _usageEventEmitter;

        public UsagePublishMiddleware(IUsageEventEmitter usageEventEmitter)
        {
            _usageEventEmitter = usageEventEmitter ?? throw new ArgumentNullException(nameof(usageEventEmitter));
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var startedAtUtc = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            // Ensure a stale value from a previous invocation on the same async-local
            // slot can never leak into this one (defence in depth — AsyncLocal already
            // scopes per logical call context, but each invocation should start clean).
            UsagePublishContext.Current = null;

            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();

                if (context.Items.TryGetValue(UsagePublishContext.ItemsKey, out var raw) &&
                    raw is UsagePublishContext executed)
                {
                    _usageEventEmitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                        workflowName: executed.WorkflowName,
                        executionId: executed.ExecutionId,
                        duration: stopwatch.Elapsed,
                        isSuccess: executed.IsSuccess,
                        errorCount: executed.ErrorCount,
                        startedAtUtc: startedAtUtc));
                }

                UsagePublishContext.Current = null;
                context.Items.Remove(UsagePublishContext.ItemsKey);
            }
        }
    }
}
