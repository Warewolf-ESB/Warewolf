using Microsoft.Extensions.Logging;
using System;
using Warewolf.Execution.Lightweight.Infrastructure;
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

        static string Instance     => InstanceCorrelationContext.Current?.InstanceId ?? InstanceCorrelationMiddleware.InstanceId;
        static string InvocationId => InstanceCorrelationContext.Current?.InvocationId ?? string.Empty;
        static string FunctionName => InstanceCorrelationContext.Current?.FunctionName ?? string.Empty;
        static string TraceId      => InstanceCorrelationContext.Current?.TraceId ?? "none";

        /// <summary>
        /// Builds a single correlation prefix string to keep structured parameter count low.
        /// Azure Functions worker gRPC can silently drop logs with too many parameters (6+).
        /// </summary>
        static string Correlation => $"[Instance:{Instance}] [Invocation:{InvocationId}] [Function:{FunctionName}] [Trace:{TraceId}]";


        /// <inheritdoc/>
        public void LogDebug(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogDebug(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogInfo(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogInfo(string message)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("{Message}", $"{Correlation} {message}");
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogError(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogError(string activityName, Exception ex, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError(ex, "{Message}", $"{Correlation} [ExecutionId:{executionId}] [{activityName}] {ex?.Message}");
        }

        /// <inheritdoc/>
        public void LogError(Exception ex, string log)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError(ex, "{Message}", $"{Correlation} {log}");
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical("{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }

        /// <inheritdoc/>
        public void LogFatal(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical(exception, "{Message}", $"{Correlation} [ExecutionId:{executionId}] {message}");
        }
    }
}




