using System;
using System.Collections.Generic;
using System.Linq;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// <see cref="IExecutionLogger"/> that forwards every call to multiple inner
    /// loggers in registration order.
    ///
    /// Used in <c>Program.cs</c> to combine <see cref="AzureExecutionLogger"/>
    /// (Application Insights / Azure Monitor) with
    /// <see cref="ElasticsearchExecutionLogger"/> without changing any consumer code.
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
                logger.LogDebug(message, executionId);
        }

        /// <inheritdoc/>
        public void LogDebug(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogDebug(message, exception, executionId);
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogInfo(message, executionId);
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogInfo(message, exception, executionId);
        }


        /// <inheritdoc/>
        public void LogWarning(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogWarning(message, executionId);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogWarning(message, exception, executionId);
        }


        /// <inheritdoc/>
        public void LogError(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogError(message, executionId);
        }

        /// <inheritdoc/>
        public void LogError(string activityName, Exception ex, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogError(activityName, ex, executionId);
        }


        /// <inheritdoc/>
        public void LogFatal(string message, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogFatal(message, executionId);
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Exception exception, Guid executionId)
        {
            foreach (var logger in _loggers)
                logger.LogFatal(message, exception, executionId);
        }

        public void LogError(Exception ex, string log)
        {
            var exception = new Exception(log, ex);
            this.LogError("", exception, new Guid());
        }

        public void LogInfo(string message)
        {
            foreach (var logger in _loggers)
            logger.LogInfo(message);
        }
    }
}

