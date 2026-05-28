using Microsoft.Extensions.Logging;
using System;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Always-available <see cref="IExecutionLogger"/> sink that writes to stdout
    /// via Microsoft.Extensions.Logging (<see cref="ILogger{TCategoryName}"/>).
    ///
    /// <para><b>Why this exists separately from <see cref="AzureExecutionLogger"/>:</b></para>
    /// <list type="bullet">
    ///   <item>This logger is <b>unconditionally registered</b> — it ensures no log is ever dropped.</item>
    ///   <item>Azure Functions runtime captures stdout and forwards it to:
    ///     <list type="bullet">
    ///       <item>Application Insights <c>traces</c> table (when connection string is set)</item>
    ///       <item>Azure Portal → Log Stream (live tail, always available)</item>
    ///     </list>
    ///   </item>
    ///   <item><see cref="AzureExecutionLogger"/> is an <b>opt-in premium</b> sink that provides
    ///     rich structured properties, custom dimensions, and exception telemetry via the
    ///     Application Insights SDK — but requires explicit enablement.</item>
    /// </list>
    ///
    /// <para><b>Output format:</b> Controlled by MEL console provider configuration.
    /// In development: simple text. In Azure: the runtime formats output for Log Analytics.</para>
    /// </summary>
    public sealed class ConsoleExecutionLogger : ExecutionLoggerBase
    {
        readonly ILogger<ConsoleExecutionLogger> _logger;

        public ConsoleExecutionLogger(ILogger<ConsoleExecutionLogger> logger,
                                      Dev2LogLevel minimumLevel = ExecutionLogLevel.Default)
            : base(minimumLevel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        string Correlation => GetCorrelationPrefix();

        /// <inheritdoc/>
        public override void LogTrace(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.TRACE)) return;
            _logger.LogTrace("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogTrace(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.TRACE)) return;
            _logger.LogTrace(exception, "{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogDebug(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogDebug(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            _logger.LogDebug(exception, "{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogInfo(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogInfo(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation(exception, "{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogInfo(string message)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            _logger.LogInformation("{Correlation} {Message}", Correlation, message);
        }

        /// <inheritdoc/>
        public override void LogWarning(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogWarning(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            _logger.LogWarning(exception, "{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogError(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogError(string activityName, Exception ex, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError(ex, "{Correlation} [ExecutionId:{ExecutionId}] [{ActivityName}] {Message}",
                Correlation, executionId, activityName, ex?.Message);
        }

        /// <inheritdoc/>
        public override void LogError(Exception ex, string log)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            _logger.LogError(ex, "{Correlation} {Message}", Correlation, log);
        }

        /// <inheritdoc/>
        public override void LogFatal(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical("{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }

        /// <inheritdoc/>
        public override void LogFatal(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            _logger.LogCritical(exception, "{Correlation} [ExecutionId:{ExecutionId}] {Message}",
                Correlation, executionId, message);
        }
    }
}
