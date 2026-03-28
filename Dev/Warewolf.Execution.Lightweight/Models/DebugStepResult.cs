using System;
using System.Collections.Generic;

namespace Warewolf.Execution.Lightweight.Models
{
    /// <summary>
    /// Serializable representation of a single activity's debug state, mirroring
    /// the <c>IDebugState</c> data the full Warewolf server sends to the Studio.
    /// </summary>
    public class DebugStepResult
    {
        // ── Identity ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Unique GUID auto-generated per <c>DebugState</c> instance.
        /// Ensures each state is individually addressable even when <see cref="ID"/> is
        /// duplicated (Start/End states share <c>Guid.Empty</c>). Used by the Studio
        /// to reconnect asynchronously-arriving debug states.
        /// </summary>
        public Guid DisconnectedID { get; set; }

        /// <summary>
        /// Activity instance ID, equal to the activity's <c>UniqueID</c> on the designer canvas.
        /// <c>DebugStateTreeBuilder.BuildTree</c> uses this together with <see cref="ParentID"/>
        /// to wire parent→child relationships. Start/End workflow states use <c>Guid.Empty</c>.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Resource (workflow) ID of the workflow file that contains this activity.
        /// Differs from <see cref="OriginatingResourceID"/> for sub-workflow steps.
        /// </summary>
        public Guid SourceResourceID { get; set; }

        /// <summary>
        /// Resource ID of the top-level workflow that started the execution chain.
        /// Equals <see cref="SourceResourceID"/> for steps in the root workflow;
        /// differs for sub-workflow steps where the parent is the originator.
        /// </summary>
        public Guid OriginatingResourceID { get; set; }

        /// <summary>
        /// Original activity instance ID. Matches <see cref="ID"/> for standard steps.
        /// Used in <c>IDebugState.IsFinalStep()</c>: End state is final when
        /// <c>OriginalInstanceID == ID</c>.
        /// </summary>
        public Guid OriginalInstanceID { get; set; }

        // ── Session / routing ────────────────────────────────────────────────────

        /// <summary>
        /// Debug session identifier shared by all states in one execution run.
        /// Together with <see cref="ClientID"/> it forms the composite key used by
        /// <c>WebDebugMessageRepo</c> to route states to the correct Studio tab.
        /// </summary>
        public Guid SessionID { get; set; }

        /// <summary>
        /// Developer workspace ID. Used to namespace debug data per workspace on the
        /// full server. <c>Guid.Empty</c> for public/lightweight executions.
        /// </summary>
        public Guid WorkspaceID { get; set; }

        /// <summary>
        /// Warewolf server instance ID. Identifies which server node produced this
        /// state in multi-server deployments. Not populated in lightweight mode.
        /// </summary>
        public Guid ServerID { get; set; }

        /// <summary>
        /// Remote environment ID for remote-execution scenarios. <c>Guid.Empty</c>
        /// for local / Azure lightweight execution.
        /// </summary>
        public Guid EnvironmentID { get; set; }

        /// <summary>
        /// Studio client connection ID. Paired with <see cref="SessionID"/> to route
        /// debug states to the correct Studio client. Not applicable in lightweight mode.
        /// </summary>
        public Guid ClientID { get; set; }

        // ── Activity metadata ────────────────────────────────────────────────────

        /// <summary>
        /// The activity's display name as configured in the designer (e.g., "Assign").
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// <c>ActivityType</c> enum integer value:
        ///   0 = Workflow (container/service level — Start and End states)
        ///   1 = Step     (individual activity)
        ///   2 = Service  (connector activity)
        /// Mirrors <c>IDebugState.ActivityType</c>.
        /// </summary>
        public int ActivityType { get; set; }

        /// <summary>
        /// The C# type name of the activity (e.g., "DsfMultiAssignActivity").
        /// </summary>
        public string ActualType { get; set; }

        /// <summary>
        /// Debug state phase string: "Start", "End", "All", "Before", "After", etc.
        /// Mirrors <c>StateType</c> enum serialised as its name.
        /// </summary>
        public string StateType { get; set; }

        // ── Status ───────────────────────────────────────────────────────────────

        /// <summary>Whether this activity step produced an error.</summary>
        public bool HasError { get; set; }

        /// <summary>Error message if the step has an error.</summary>
        public string ErrorMessage { get; set; }

        // ── Execution origin ─────────────────────────────────────────────────────

        /// <summary>
        /// Human-readable execution origin string computed from <see cref="ExecutionOrigin"/>:
        ///   ""         — Unknown
        ///   "Debug"    — triggered from Studio
        ///   "External" — triggered via HTTP/REST (includes Azure Function invocations)
        ///   "Workflow - &lt;description&gt;" — triggered by a parent workflow
        /// Displayed in the Studio Debug Output panel origin column.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// <c>ExecutionOrigin</c> enum integer value:
        ///   0 = Unknown
        ///   1 = Debug    (triggered from Studio)
        ///   2 = External (triggered via HTTP/REST)
        ///   3 = Workflow (called from a parent workflow)
        /// </summary>
        public int ExecutionOrigin { get; set; }

        /// <summary>
        /// Design-surface mapping ID. The Studio uses this to highlight the currently
        /// executing activity on the workflow canvas. Equals the designer's
        /// <c>WorkSurfaceMappingId</c> attribute set on each activity shape.
        /// </summary>
        public Guid WorkSurfaceMappingId { get; set; }

        // ── Timing ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Controls whether the duration is rendered in the Studio debug panel.
        /// <c>false</c> for Start states (zero duration); <c>true</c> for all others.
        /// </summary>
        public bool IsDurationVisible { get; set; }

        /// <summary>
        /// Wall-clock duration of this activity step (<c>EndTime - StartTime</c>).
        /// Placed before <see cref="StartTime"/> to match the full-server serialisation order.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>Time the activity started executing (UTC).</summary>
        public DateTime StartTime { get; set; }

        /// <summary>Time the activity finished executing (UTC).</summary>
        public DateTime EndTime { get; set; }

        // ── Debug items ──────────────────────────────────────────────────────────

        /// <summary>
        /// Debug input lines for this step — each outer list entry is one debug item (row),
        /// and each inner list entry is a result part within that row.
        /// </summary>
        public List<List<DebugLineItem>> Inputs { get; set; } = new();

        /// <summary>Debug output lines for this step.</summary>
        public List<List<DebugLineItem>> Outputs { get; set; } = new();

        /// <summary>
        /// Test assertion results — populated when a workflow is run as a service test.
        /// Mirrors <c>IDebugState.AssertResultList</c>.
        /// </summary>
        public List<List<DebugLineItem>> AssertResultList { get; set; } = new();

        // ── Tree ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Nested child steps — populated for sub-workflow and ForEach activities
        /// using the parent→child relationships built by <c>DebugStateTreeBuilder.BuildTree</c>.
        /// </summary>
        public List<DebugStepResult> Children { get; set; } = new();
    }

    /// <summary>
    /// A single result part within a debug item row, mirroring <c>IDebugItemResult</c>.
    /// </summary>
    public class DebugLineItem
    {
        /// <summary>
        /// <c>DebugItemResultType</c> enum integer value:
        ///   0 = Label
        ///   1 = Variable
        ///   2 = Value
        /// Mirrors <c>IDebugItemResult.Type</c>.
        /// </summary>
        public int Type { get; set; }

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

        /// <summary>
        /// Truncated display value when the full value exceeds <c>DebugItem.MaxCharDispatchCount</c>
        /// (150 chars). Mirrors <c>IDebugItemResult.TruncatedValue</c> — equals <see cref="Value"/>
        /// for short values; shorter prefix + MoreLink for long ones.
        /// </summary>
        public string TruncatedValue { get; set; }

        /// <summary>
        /// Record-set group name used to correlate multi-row debug items (e.g., "[[Rows(*)]]").
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Zero-based row index within a record-set group.
        /// </summary>
        public int GroupIndex { get; set; }

        /// <summary>
        /// URL to a temp file containing the full value when truncation occurred.
        /// Null when the value fits within the dispatch limit.
        /// </summary>
        public string MoreLink { get; set; }
    }
}
