using Microsoft.Extensions.Logging;
using System;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Dedicated security/audit sink that is always present in the
    /// <see cref="CompositeExecutionLogger"/>. Unlike operational sinks, audit
    /// entries are <b>never filtered by <see cref="ExecutionLogLevel"/></b> —
    /// security events must always be written.
    ///
    /// <para>Uses a fixed <see cref="EventId"/> (<c>9000 / "AuditLog"</c>) so
    /// Application Insights can filter:
    /// <c>traces | where customDimensions.EventId == 9000</c></para>
    ///
    /// <para>Only ERROR and FATAL are written — DEBUG/INFO/WARN are no-ops
    /// because this sink is exclusively for security audit events routed via
    /// <see cref="Dev2.Common.Dev2Logger"/>.</para>
    /// </summary>
    public sealed class AuditExecutionLogger : IExecutionLogger
    {
        static readonly EventId AuditEvent = new(9000, "AuditLog");

        readonly ILogger<AuditExecutionLogger> _logger;

        public AuditExecutionLogger(ILogger<AuditExecutionLogger> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // ── Audit-relevant: always written ───────────────────────────────────────

        public void LogError(string message, Guid executionId)
            => _logger.LogError(AuditEvent, "[AUDIT] [ExecutionId:{ExecutionId}] {Message}",
                executionId, message);

        public void LogError(string activityName, Exception ex, Guid executionId)
            => _logger.LogError(AuditEvent, ex,
                "[AUDIT] [{ActivityName}] [ExecutionId:{ExecutionId}] {Message}",
                activityName, executionId, ex?.Message);

        public void LogError(Exception ex, string log)
            => _logger.LogError(AuditEvent, ex, "[AUDIT] {Message}", log);

        public void LogFatal(string message, Guid executionId)
            => _logger.LogCritical(AuditEvent, "[AUDIT] [ExecutionId:{ExecutionId}] {Message}",
                executionId, message);

        public void LogFatal(string message, Exception exception, Guid executionId)
            => _logger.LogCritical(AuditEvent, exception,
                "[AUDIT] [ExecutionId:{ExecutionId}] {Message}", executionId, message);

        // ── Non-audit: no-ops ────────────────────────────────────────────────────

        public void LogDebug(string message, Guid executionId) { }
        public void LogDebug(string message, Exception exception, Guid executionId) { }
        public void LogInfo(string message, Guid executionId) { }
        public void LogInfo(string message, Exception exception, Guid executionId) { }
        public void LogInfo(string message) { }
        public void LogWarning(string message, Guid executionId) { }
        public void LogWarning(string message, Exception exception, Guid executionId) { }
    }
}
