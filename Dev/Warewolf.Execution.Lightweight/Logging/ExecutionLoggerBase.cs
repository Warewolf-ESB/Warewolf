using System;
using Warewolf.Execution.Lightweight.Infrastructure;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Base class for <see cref="IExecutionLogger"/> implementations that need
    /// instance-level correlation context (InstanceId, InvocationId, FunctionName, TraceId).
    ///
    /// Centralises the <see cref="ShouldLog"/> gate and the correlation-prefix
    /// building so every new logger gets them for free.
    /// </summary>
    public abstract class ExecutionLoggerBase : IExecutionLogger
    {
        readonly Dev2LogLevel _minimumLevel;

        protected ExecutionLoggerBase(Dev2LogLevel minimumLevel = ExecutionLogLevel.Default)
        {
            _minimumLevel = minimumLevel;
        }

        /// <summary>Returns <c>true</c> when the given level meets the configured minimum.</summary>
        protected bool ShouldLog(Dev2LogLevel level) => ExecutionLogLevel.ShouldLog(level, _minimumLevel);

        /// <summary>
        /// Current instance ID from the middleware (stable per container lifetime).
        /// </summary>
        protected static string Instance => InstanceCorrelationContext.Current?.InstanceId
                                            ?? InstanceCorrelationMiddleware.InstanceId;

        /// <summary>Current invocation ID (unique per function trigger).</summary>
        protected static string InvocationId => InstanceCorrelationContext.Current?.InvocationId
                                                ?? string.Empty;

        /// <summary>Name of the currently executing Azure Function.</summary>
        protected static string FunctionName => InstanceCorrelationContext.Current?.FunctionName
                                                ?? string.Empty;

        /// <summary>W3C distributed trace ID.</summary>
        protected static string TraceId => InstanceCorrelationContext.Current?.TraceId ?? "none";

        /// <summary>
        /// Builds a single correlation prefix string suitable for prepending to log messages.
        /// Keeps structured parameter count low to avoid Azure Functions gRPC silent drops.
        /// </summary>
        protected static string GetCorrelationPrefix() =>
            $"[Instance:{Instance}] [Invocation:{InvocationId}] [Function:{FunctionName}] [Trace:{TraceId}]";

        /// <summary>
        /// Public accessor for <see cref="GetCorrelationPrefix"/> — intended for use as
        /// a <c>Dev2Logger.CorrelationPrefixProvider</c> delegate so the legacy logger
        /// can also include correlation context.
        /// </summary>
        public static string GetCorrelationPrefixStatic() => GetCorrelationPrefix();

        /// <summary>
        /// Returns the current <see cref="InstanceCorrelationContext"/> (may be <c>null</c>
        /// outside a function invocation). Useful for loggers that enrich structured documents
        /// rather than prepending a string.
        /// </summary>
        protected static InstanceCorrelationContext? GetCorrelationContext() =>
            InstanceCorrelationContext.Current;

        /// <summary>
        /// Enriches an <see cref="ElasticsearchLogDocument"/> with instance correlation fields.
        /// When no invocation context exists (e.g. during startup), the static instance ID is used.
        /// </summary>
        protected static ElasticsearchLogDocument EnrichWithCorrelation(ElasticsearchLogDocument doc)
        {
            var ctx = GetCorrelationContext();
            if (ctx is not null)
            {
                return doc with
                {
                    InstanceId   = ctx.InstanceId,
                    InvocationId = ctx.InvocationId,
                    FunctionName = ctx.FunctionName,
                    TraceId      = ctx.TraceId,
                };
            }

            return doc with
            {
                InstanceId = InstanceCorrelationMiddleware.InstanceId,
            };
        }

        // ?? Abstract IExecutionLogger members ????????????????????????????????????

        public abstract void LogTrace(string message, Guid executionId);
        public abstract void LogTrace(string message, Exception exception, Guid executionId);
        public abstract void LogDebug(string message, Guid executionId);
        public abstract void LogDebug(string message, Exception exception, Guid executionId);
        public abstract void LogInfo(string message, Guid executionId);
        public abstract void LogInfo(string message, Exception exception, Guid executionId);
        public abstract void LogInfo(string message);
        public abstract void LogWarning(string message, Guid executionId);
        public abstract void LogWarning(string message, Exception exception, Guid executionId);
        public abstract void LogError(string message, Guid executionId);
        public abstract void LogError(string activityName, Exception ex, Guid executionId);
        public abstract void LogError(Exception ex, string log);
        public abstract void LogFatal(string message, Guid executionId);
        public abstract void LogFatal(string message, Exception exception, Guid executionId);
    }
}
