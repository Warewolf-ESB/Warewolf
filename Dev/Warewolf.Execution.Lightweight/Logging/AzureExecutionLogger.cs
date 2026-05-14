using Microsoft.Extensions.Logging;
using System;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Azure-aware implementation of <see cref="IExecutionLogger"/>.
    /// Wraps <see cref="ILogger{TCategoryName}"/> so all entries flow to the configured
    /// Microsoft.Extensions.Logging sink (Application Insights, Azure Monitor, console, etc.).
    ///
    /// Inherits correlation-prefix logic from <see cref="ExecutionLoggerBase"/>.
    /// </summary>
    public sealed class AzureExecutionLogger : ExecutionLoggerBase
    {
        readonly ILogger<AzureExecutionLogger> _logger;

        public AzureExecutionLogger(ILogger<AzureExecutionLogger> logger,
                                    Dev2LogLevel minimumLevel = ExecutionLogLevel.Default)
            : base(minimumLevel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        string Correlation => GetCorrelationPrefix();

        /// <inheritdoc/>
        public override void LogDebug(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogDebug(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogInfo(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogInfo(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogInfo(string message)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("{Message}", $"{Correlation} {message}");
        }

        /// <inheritdoc/>
        public override void LogWarning(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogWarning(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogError(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogError(string activityName, Exception ex, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError(ex, "{Message}", $"{Correlation} [ExecutionId:{executionId}] [{activityName}] {ex?.Message}");
        }

        /// <inheritdoc/>
        public override void LogError(Exception ex, string log)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError(ex, "{Message}", $"{Correlation} {log}");
        }

        /// <inheritdoc/>
        public override void LogFatal(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public override void LogFatal(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }
    }
}




