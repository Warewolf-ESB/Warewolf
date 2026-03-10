using Microsoft.Extensions.Logging;
using System;
using Warewolf.Execution.AzureFunction.Lightweight.Models;

namespace Warewolf.Execution.AzureFunction.Lightweight.Logging
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

        public AzureExecutionLogger(ILogger<AzureExecutionLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void LogError(string activityName, Exception ex, Guid executionId)
        {
            var detail = new ExecutionErrorDetail
            {
                ActivityName = activityName,
                Message      = ex?.Message ?? string.Empty,
                StackTrace   = ex?.ToString() ?? string.Empty,   // ToString() includes inner exceptions
                Timestamp    = DateTime.UtcNow,
                ExecutionId  = executionId
            };

            using (_logger.BeginScope(detail.ToLogScope()))
            {
                _logger.LogError(
                    ex,
                    "[ExecutionId:{ExecutionId}] [{ActivityName}] {Message}",
                    executionId,
                    activityName,
                    detail.Message);
            }
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Guid executionId)
        {
            _logger.LogInformation(
                "[ExecutionId:{ExecutionId}] {Message}",
                executionId,
                message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Guid executionId)
        {
            _logger.LogWarning(
                "[ExecutionId:{ExecutionId}] {Message}",
                executionId,
                message);
        }
    }
}
