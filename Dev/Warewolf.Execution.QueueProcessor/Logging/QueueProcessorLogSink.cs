/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Logging;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;
using WarewolfILogger = Dev2.Common.Interfaces.Logging.ILogger;

namespace Warewolf.Execution.QueueProcessor.Logging
{
    /// <summary>
    /// The worker's <c>Dev2Logger.ExternalSink</c> implementation: adapts every
    /// <c>Dev2Logger.Trace/Debug/Info/Warn/Error/Fatal</c> call in the codebase onto
    /// Microsoft.Extensions.Logging, from where the registered providers (console and,
    /// when <c>ENABLEAPPLICATIONINSIGHTS=true</c>, the App Insights worker SDK) take it.
    ///
    /// <para><b>Why this is not the engine's sink stack.</b> The engine's
    /// <c>CompositeExecutionLogger</c> / <c>ConsoleExecutionLogger</c> / <c>AzureExecutionLogger</c>
    /// all derive from <c>ExecutionLoggerBase</c>, which reads
    /// <c>InstanceCorrelationContext</c> / <c>InstanceCorrelationMiddleware</c> — Azure
    /// <b>Functions</b> invocation correlation. Linking them into this container would drag
    /// <c>Microsoft.Azure.Functions.Worker</c> into a host that is not a Function App. The
    /// level semantics and the environment-variable contract ARE shared, via the linked
    /// <see cref="ExecutionLogLevel"/> and <see cref="LoggingConfiguration"/> files, so the
    /// two hosts cannot drift on the part that matters operationally.</para>
    ///
    /// <para>Correlation here is the worker's own unit of work — the message being processed
    /// (execution id + queue) — supplied by <see cref="QueueProcessorCorrelation"/>, rather
    /// than a function invocation.</para>
    /// </summary>
    public sealed class QueueProcessorLogSink : WarewolfILogger
    {
        readonly ILogger _logger;
        readonly Dev2LogLevel _minimumLevel;

        public QueueProcessorLogSink(ILogger logger, Dev2LogLevel minimumLevel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _minimumLevel = minimumLevel;
        }

        bool ShouldLog(Dev2LogLevel level) => ExecutionLogLevel.ShouldLog(level, _minimumLevel);

        static string Text(object? message) => message?.ToString() ?? string.Empty;

        /// <summary>
        /// Keeps the structured parameter count low (three) on purpose: the engine learned that
        /// wide structured payloads get silently dropped by the Functions gRPC channel, and the
        /// same discipline keeps App Insights records predictable here.
        /// </summary>
        void Write(LogLevel level, object? message, Exception? exception, string executionId)
        {
            var prefix = QueueProcessorCorrelation.GetPrefix();
            if (exception is null)
            {
                _logger.Log(level, "{Prefix} [ExecutionId:{ExecutionId}] {Message}",
                    prefix, executionId, Text(message));
            }
            else
            {
                _logger.Log(level, exception, "{Prefix} [ExecutionId:{ExecutionId}] {Message}",
                    prefix, executionId, Text(message));
            }
        }

        public void Trace(object message, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.TRACE)) Write(LogLevel.Trace, message, null, executionId);
        }

        public void Trace(object message, Exception exception, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.TRACE)) Write(LogLevel.Trace, message, exception, executionId);
        }

        public void Debug(object message, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.DEBUG)) Write(LogLevel.Debug, message, null, executionId);
        }

        public void Debug(object message, Exception exception, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.DEBUG)) Write(LogLevel.Debug, message, exception, executionId);
        }

        public void Info(object message, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.INFO)) Write(LogLevel.Information, message, null, executionId);
        }

        public void Info(object message, Exception exception, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.INFO)) Write(LogLevel.Information, message, exception, executionId);
        }

        public void Warn(object message, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.WARN)) Write(LogLevel.Warning, message, null, executionId);
        }

        public void Warn(object message, Exception exception, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.WARN)) Write(LogLevel.Warning, message, exception, executionId);
        }

        public void Error(object message, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.ERROR)) Write(LogLevel.Error, message, null, executionId);
        }

        public void Error(object message, Exception exception, string executionId)
        {
            if (ShouldLog(Dev2LogLevel.ERROR)) Write(LogLevel.Error, message, exception, executionId);
        }

        // FATAL and the audit channel are never level-filtered: security/terminal events must
        // always be written (engine parity - AuditExecutionLogger applies the same rule).
        public void Fatal(object message, string executionId)
            => Write(LogLevel.Critical, message, null, executionId);

        public void Fatal(object message, Exception exception, string executionId)
            => Write(LogLevel.Critical, message, exception, executionId);
    }
}
