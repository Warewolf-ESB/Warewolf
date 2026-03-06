using System;
using System.Collections.Generic;

namespace Warewolf.Execution.AzureFunction.Lightweight.Models
{
    /// <summary>
    /// Serializable representation of a single activity's debug state, mirroring
    /// the <c>IDebugState</c> data the full Warewolf server sends to the Studio.
    /// </summary>
    public class DebugStepResult
    {
        /// <summary>
        /// The activity's display name as configured in the designer (e.g., "Assign", "Hello World").
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Activity category: "Step", "Workflow", or "Service".
        /// </summary>
        public string ActivityType { get; set; }

        /// <summary>
        /// The C# type name of the activity (e.g., "DsfMultiAssignActivity").
        /// </summary>
        public string ActualType { get; set; }

        /// <summary>
        /// Debug state phase: Before, After, All, Start, End, etc.
        /// </summary>
        public string StateType { get; set; }

        /// <summary>
        /// Whether this activity step produced an error.
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error message if the step has an error.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Time the activity started executing.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Time the activity finished executing.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Debug input lines for this step — each outer list entry is one debug item (row),
        /// and each inner list entry is a result part within that row.
        /// </summary>
        public List<List<DebugLineItem>> Inputs { get; set; } = new();

        /// <summary>
        /// Debug output lines for this step.
        /// </summary>
        public List<List<DebugLineItem>> Outputs { get; set; } = new();
    }

    /// <summary>
    /// A single result part within a debug item row, mirroring <c>IDebugItemResult</c>.
    /// </summary>
    public class DebugLineItem
    {
        /// <summary>
        /// Result kind: Variable, Label, Value, etc.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Display label (e.g., "[[Name]] =").
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Variable expression (e.g., "[[Name]]").
        /// </summary>
        public string Variable { get; set; }

        /// <summary>
        /// Operator (e.g., "=").
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// The resolved value (e.g., "Sachin").
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Whether this item result has an error.
        /// </summary>
        public bool HasError { get; set; }
    }
}
