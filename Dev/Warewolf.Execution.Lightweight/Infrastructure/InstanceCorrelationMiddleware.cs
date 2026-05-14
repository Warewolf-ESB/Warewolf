using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Warewolf.Execution.Lightweight.Infrastructure
{
    /// <summary>
    /// Functions worker middleware that attaches instance-level and invocation-level
    /// correlation properties to every log entry emitted during a function invocation.
    ///
    /// <para><b>Why:</b> When Azure scales the Function App to N instances, all
    /// instances emit logs to the same Application Insights / Elasticsearch cluster.
    /// Without instance-level correlation you cannot determine which pod handled a
    /// given request.</para>
    ///
    /// <para><b>What it adds:</b></para>
    /// <list type="bullet">
    ///   <item><c>InstanceId</c> — first 8 chars of <c>WEBSITE_INSTANCE_ID</c> (stable per instance lifetime)</item>
    ///   <item><c>InvocationId</c> — unique per function invocation</item>
    ///   <item><c>Function</c> — the triggered function name</item>
    ///   <item><c>TraceId</c> — W3C distributed trace ID (if available)</item>
    /// </list>
    ///
    /// For MEL-based loggers (<see cref="Logging.AzureExecutionLogger"/>), these flow
    /// automatically via <see cref="ILogger.BeginScope{TState}"/>.
    ///
    /// For non-MEL loggers (<see cref="Logging.ElasticsearchExecutionLogger"/>), the
    /// values are exposed through <see cref="InstanceCorrelationContext.Current"/> which
    /// uses <see cref="AsyncLocal{T}"/> to propagate per-invocation.
    /// </summary>
    public sealed class InstanceCorrelationMiddleware : IFunctionsWorkerMiddleware
    {
        /// <summary>
        /// First 8 characters of <c>WEBSITE_INSTANCE_ID</c>.
        /// Stable for the entire lifetime of this container/instance.
        /// Falls back to <c>"local"</c> for local development.
        /// </summary>
        internal static readonly string InstanceId =
            (Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? "local-env");

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {

            var invocationId = context.InvocationId;
            var functionName = context.FunctionDefinition.Name;
            var traceId      = Activity.Current?.TraceId.ToString() ?? "none";

            // Set the AsyncLocal context so ElasticsearchExecutionLogger can read it.
            InstanceCorrelationContext.Current = new InstanceCorrelationContext
            {
                InstanceId   = InstanceId,
                InvocationId = invocationId,

                FunctionName = functionName,
                TraceId      = traceId,
            };

            // MEL scope — flows automatically to AzureExecutionLogger / Application Insights.
            var logger = context.GetLogger<InstanceCorrelationMiddleware>();
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["InstanceId"]   = InstanceId,
                ["InvocationId"] = invocationId,
                ["Function"]     = functionName,
                ["TraceId"]      = traceId,
            });

            logger.LogInformation("InstanceCorrelationMiddleware invoked for function '{FunctionName}' (InvocationId: {InvocationId})", context.FunctionDefinition.Name,
            context.InvocationId);

            try
            {
                await next(context);
            }
            finally
            {
                InstanceCorrelationContext.Current = null;
            }
        }
    }

    /// <summary>
    /// Per-invocation correlation data set by <see cref="InstanceCorrelationMiddleware"/>
    /// and consumed by non-MEL loggers (e.g. <see cref="Logging.ElasticsearchExecutionLogger"/>).
    /// Uses <see cref="AsyncLocal{T}"/> so it propagates across async continuations.
    /// </summary>
    public sealed class InstanceCorrelationContext
    {
        static readonly AsyncLocal<InstanceCorrelationContext?> _current = new();

        /// <summary>
        /// The correlation context for the current async flow.
        /// <c>null</c> outside of a function invocation (e.g. during startup).
        /// </summary>
        public static InstanceCorrelationContext? Current
        {
            get => _current.Value;
            internal set => _current.Value = value;
        }

        public string InstanceId   { get; init; } = "local-env";
        public string InvocationId { get; init; } = string.Empty;
        public string FunctionName { get; init; } = string.Empty;
        public string TraceId      { get; init; } = "none";
    }
}
