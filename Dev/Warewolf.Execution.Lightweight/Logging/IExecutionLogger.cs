using System;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Generic structured logger for the Warewolf Azure Execution Engine.
    /// Implementations emit log entries to the configured sink (e.g., Azure Monitor /
    /// Application Insights via <c>Microsoft.Extensions.Logging.ILogger</c>).
    /// </summary>
    public interface IExecutionLogger
    {
        /// <summary>
        /// Logs a full exception with activity context and execution correlation ID.
        /// The full stack trace is captured inside <see cref="ExecutionErrorDetail"/>.
        /// </summary>
        void LogError(string activityName, Exception ex, Guid executionId);

        /// <summary>
        /// Logs a structured informational message correlated to an execution run.
        /// </summary>
        void LogInfo(string message, Guid executionId);

        /// <summary>
        /// Logs a structured warning message correlated to an execution run.
        /// </summary>
        void LogWarning(string message, Guid executionId);
    }
}
