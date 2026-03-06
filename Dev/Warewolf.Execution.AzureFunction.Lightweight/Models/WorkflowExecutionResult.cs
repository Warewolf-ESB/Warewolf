using System;
using System.Collections.Generic;

namespace Warewolf.Execution.AzureFunction.Lightweight.Models
{
    /// <summary>
    /// Result of a Warewolf workflow execution.
    /// </summary>
    public class WorkflowExecutionResult
    {
        /// <summary>
        /// Whether the workflow executed successfully without errors.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The execution ID assigned to this workflow run.
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Output data from the workflow environment as key-value pairs.
        /// </summary>
        public Dictionary<string, object> Outputs { get; set; } = new();

        /// <summary>
        /// JSON string of the full execution environment output.
        /// </summary>
        public string OutputJson { get; set; }

        /// <summary>
        /// Workflow output formatted as a DataList XML string (populated for .xml requests).
        /// Produced by <c>ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment</c>,
        /// which evaluates each Output/Both variable against the workflow DataList — the
        /// same path the full Warewolf server uses for .xml responses.
        /// </summary>
        public string OutputXml { get; set; }

        /// <summary>
        /// Errors that occurred during execution.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Per-activity debug states captured when the request used the .debug extension.
        /// Each entry mirrors the <c>IDebugState</c> the full Warewolf server sends to the Studio,
        /// containing the activity name, inputs, outputs, timing, and error information.
        /// Populated only when <c>IsDebug = true</c>; empty list otherwise.
        /// </summary>
        public List<DebugStepResult> DebugStates { get; set; } = new();

        /// <summary>
        /// Duration of the workflow execution.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Time the workflow execution started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Time the workflow execution completed.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Creates a failure result with the specified error message.
        /// </summary>
        public static WorkflowExecutionResult Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            Errors = new List<string> { errorMessage },
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow
        };
    }
}
