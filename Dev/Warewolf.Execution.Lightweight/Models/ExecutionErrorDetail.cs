using System;
using System.Collections.Generic;

namespace Warewolf.Execution.Lightweight.Models
{
    /// <summary>
    /// Structured representation of an error that occurred during workflow execution.
    /// Captured by <c>AzureExecutionLogger</c> and emitted as a scoped log state so
    /// Application Insights / Azure Monitor can index every field individually.
    ///
    /// In Log Analytics the fields appear under <c>customDimensions</c>:
    ///   customDimensions.ExecutionId  — correlates all entries for one workflow run
    ///   customDimensions.ActivityName — the service / activity that faulted
    ///   customDimensions.Message      — short exception message
    ///   customDimensions.StackTrace   — full CLR stack trace including inner exceptions
    ///   customDimensions.Timestamp    — UTC time of the error
    /// </summary>
    public sealed class ExecutionErrorDetail
    {
        /// <summary>
        /// Workflow execution correlation ID (mirrors <c>WorkflowExecutionResult.ExecutionId</c>).
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Name of the activity or service step where the error occurred
        /// (e.g., "MssqlSqlExecution", "PostgreSqlExecution", "ExecuteActivityChain").
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Short error message (<c>Exception.Message</c>).
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Full CLR stack trace including inner exceptions
        /// (<c>Exception.ToString()</c> — includes type, message, and each frame).
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// UTC timestamp when the error was captured.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Projects the detail into a flat string-keyed dictionary suitable for
        /// <c>ILogger.BeginScope</c>. Application Insights uses this to populate
        /// <c>customDimensions</c> on the telemetry item.
        /// </summary>
        public IReadOnlyDictionary<string, object> ToLogScope() =>
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["ExecutionId"]  = ExecutionId,
                ["ActivityName"] = ActivityName ?? string.Empty,
                ["Message"]      = Message      ?? string.Empty,
                ["StackTrace"]   = StackTrace   ?? string.Empty,
                ["Timestamp"]    = Timestamp
            };
    }
}
