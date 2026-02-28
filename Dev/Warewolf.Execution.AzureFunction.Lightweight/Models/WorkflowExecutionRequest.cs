using System;
using System.Collections.Generic;

namespace Warewolf.Execution.AzureFunction.Lightweight.Models
{
    /// <summary>
    /// Request to execute a Warewolf workflow directly from a file.
    /// </summary>
    public class WorkflowExecutionRequest
    {
        /// <summary>
        /// Full path to the workflow resource XML file on disk.
        /// </summary>
        public string WorkflowFilePath { get; set; }

        /// <summary>
        /// Optional display name for the workflow (used in logging).
        /// If not provided, the file name is used.
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Input parameters for the workflow as key-value pairs.
        /// Keys should match the workflow DataList variable names (without [[ ]] notation).
        /// </summary>
        public Dictionary<string, string> InputParameters { get; set; } = new();

        /// <summary>
        /// Whether to execute the workflow in debug mode.
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Validates that the request has the minimum required information.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(WorkflowFilePath);
    }
}
