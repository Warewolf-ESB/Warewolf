using Microsoft.Extensions.Logging;
using System;
using Warewolf.Execution.Lightweight.Models;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Azure-aware implementation of <see cref="IExecutionLogger"/>.
    /// Wraps <see cref="ILogger{TCategoryName}"/> so all entries flow to the configured
    /// Microsoft.Extensions.Logging sink (Application Insights, Azure Monitor, console, etc.).
    ///
    /// Every log call emits a structured <see cref="ExecutionErrorDetail"/> as a scoped
    /// state object, enabling rich querying in Azure Log Analytics / Application Insights:
    ///   customDimensions.ExecutionId    — correlates all entries for one workflow run
    ///   customDimensions.ActivityName   — which activity / step produced the entry
    ///   customDimensions.StackTrace     — full CLR stack trace (errors only)
    /// </summary>
    public sealed class AzureExecutionLogger : IExecutionLogger
    {
        readonly ILogger<AzureExecutionLogger> _logger;
        readonly Dev2LogLevel _minimumLevel;

        public AzureExecutionLogger(ILogger<AzureExecutionLogger> logger,
                                    Dev2LogLevel minimumLevel = ExecutionLogLevel.Default)
        {
            _logger       = logger ?? throw new ArgumentNullException(nameof(logger));
            _minimumLevel = minimumLevel;
        }

        bool ShouldLog(Dev2LogLevel level) => ExecutionLogLevel.ShouldLog(level, _minimumLevel);


        /// <inheritdoc/>
        public void LogDebug(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug("[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogDebug(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug(exception, "[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation(exception, "[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogInfo(string message)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("{Message}", message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning("[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning(exception, "[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogError(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError("[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogError(string activityName, Exception ex, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            var detail = new ExecutionErrorDetail
            {
                ActivityName = activityName,
                Message      = ex?.Message ?? string.Empty,
                StackTrace   = ex?.ToString() ?? string.Empty,
                Timestamp    = DateTime.UtcNow,
                ExecutionId  = executionId
            };
            using (_logger.BeginScope(detail.ToLogScope()))
            {
                _logger.LogError(ex,
                    "[ExecutionId:{ExecutionId}] [{ActivityName}] {Message}",
                    executionId, activityName, detail.Message);
            }
        }

        /// <inheritdoc/>
        public void LogError(Exception ex, string log)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError(ex, "{Log}", log);
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical("[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical(exception, "[ExecutionId:{ExecutionId}] {Message}", executionId, message);
        }
    }
}

