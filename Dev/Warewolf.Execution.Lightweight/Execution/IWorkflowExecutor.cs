using System;
using System.Collections.Generic;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Interface for executing Warewolf workflows directly from file.
    /// </summary>
    public interface IWorkflowExecutor
    {
        /// <summary>
        /// Executes a workflow from a file path with the provided input parameters.
        /// </summary>
        WorkflowExecutionResult Execute(string workflowFilePath, Dictionary<string, string> inputs = null);

        /// <summary>
        /// Executes a workflow based on a <see cref="WorkflowExecutionRequest"/>.
        /// </summary>
        WorkflowExecutionResult Execute(WorkflowExecutionRequest request);
    }
}
