using System;
using System.Collections.Generic;
using System.Linq;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// <see cref="IExecutionLogger"/> that forwards every call to multiple inner
    /// loggers in registration order.
    ///
    /// Used in <c>Program.cs</c> to combine <see cref="ConsoleExecutionLogger"/>
    /// (always present), <see cref="AzureExecutionLogger"/> (opt-in Application Insights),
    /// <see cref="ElasticsearchExecutionLogger"/> (opt-in), and
    /// <see cref="AuditExecutionLogger"/> (always present, security events only)
    /// without changing any consumer code.
    ///
    /// Each inner logger is wrapped in a try/catch so a failure in one sink
    /// never prevents subsequent sinks from receiving the log entry.
    /// </summary>
    public sealed class CompositeExecutionLogger : IExecutionLogger
    {
        readonly IReadOnlyList<IExecutionLogger> _loggers;

        public CompositeExecutionLogger(IEnumerable<IExecutionLogger> loggers)
        {
            ArgumentNullException.ThrowIfNull(loggers);
            _loggers = loggers.ToList();
        }

        /// <inheritdoc/>
        public void LogDebug(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogDebug(message, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogDebug(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogDebug(message, exception, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogInfo(message, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogInfo(message, exception, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogWarning(message, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogWarning(message, exception, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogError(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogError(message, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogError(string activityName, Exception ex, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogError(activityName, ex, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogFatal(message, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogFatal(message, exception, executionId); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        public void LogError(Exception ex, string log)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogError(ex, log); }
                catch { /* individual sink failure must not cascade */ }
            }
        }

        public void LogInfo(string message)
        {
            foreach (var logger in _loggers)
            {
                try { logger.LogInfo(message); }
                catch { /* individual sink failure must not cascade */ }
            }
        }
    }
}

