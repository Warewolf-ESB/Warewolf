using Dev2.Common.Interfaces.Logging;
using System;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Adapts <see cref="IExecutionLogger"/> to the
    /// <see cref="Dev2.Common.Interfaces.Logging.ILogger"/> contract so it can be
    /// assigned to <c>Dev2Logger.ExternalSink</c>.
    ///
    /// This bridges every <c>Dev2Logger.Debug/Info/Warn/Error/Fatal</c> call in the
    /// codebase to the configured <see cref="IExecutionLogger"/> sinks (Azure Monitor,
    /// Elasticsearch, etc.) without any change to the call-sites.
    ///
    /// <para>
    /// <b>Level filtering</b> is handled entirely by the wrapped
    /// <see cref="IExecutionLogger"/> implementation — each sink carries its own
    /// <c>_minimumLevel</c> read from the <c>ExecutionLogLevel</c> env var.
    /// </para>
    /// </summary>
    internal sealed class Dev2LoggerSinkAdapter : Dev2.Common.Interfaces.Logging.ILogger
    {
        readonly IExecutionLogger _inner;

        public Dev2LoggerSinkAdapter(IExecutionLogger inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <summary>
        /// Dev2Logger passes <c>executionId</c> as a plain string.
        /// Parse it to <see cref="Guid"/>; fall back to <see cref="Guid.Empty"/> if invalid.
        /// </summary>
        static Guid ToGuid(string executionId) =>
            Guid.TryParse(executionId, out var g) ? g : Guid.Empty;

        static string Str(object? message) => message?.ToString() ?? string.Empty;


        public void Trace(object message, string executionId) =>
            _inner.LogTrace(Str(message), ToGuid(executionId));

        public void Trace(object message, Exception exception, string executionId) =>
            _inner.LogTrace(Str(message), exception, ToGuid(executionId));


        public void Debug(object message, string executionId) =>
            _inner.LogDebug(Str(message), ToGuid(executionId));

        public void Debug(object message, Exception exception, string executionId) =>
            _inner.LogDebug(Str(message), exception, ToGuid(executionId));


        public void Info(object message, string executionId) =>
            _inner.LogInfo(Str(message), ToGuid(executionId));

        public void Info(object message, Exception exception, string executionId) =>
            _inner.LogInfo(Str(message), exception, ToGuid(executionId));


        public void Warn(object message, string executionId) =>
            _inner.LogWarning(Str(message), ToGuid(executionId));

        public void Warn(object message, Exception exception, string executionId) =>
            _inner.LogWarning(Str(message), exception, ToGuid(executionId));


        public void Error(object message, string executionId) =>
            _inner.LogError(Str(message), ToGuid(executionId));

        public void Error(object message, Exception exception, string executionId) =>
            _inner.LogError(Str(message), exception, ToGuid(executionId));


        public void Fatal(object message, string executionId) =>
            _inner.LogFatal(Str(message), ToGuid(executionId));

        public void Fatal(object message, Exception exception, string executionId) =>
            _inner.LogFatal(Str(message), exception, ToGuid(executionId));
    }
}
