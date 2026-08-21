/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Activities.WF;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using ModelContextProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>add_step</c> MCP tool: appends a single activity node to the end (or a specified
/// branch point) of an existing, <c>bodyEditable: true</c> workflow's X6 graph, without the
/// caller resubmitting the whole <c>body</c> — mirrors the Angular Web Studio chatbot's
/// <c>dropToolOntoBottomAutoConnector</c>/<c>findChainTail</c> protocol
/// (<c>graph.component.ts:1863</c>/<c>:1952</c>).
///
/// <para>
/// <b>Pipeline:</b>
/// <list type="number">
/// <item>Resolves <c>name</c> (<see cref="WorkflowNameResolver.Resolve"/>), requires
/// <b>Contribute</b> permission — same rule as <see cref="EditWorkflowTool"/>
/// (<see cref="ListWorkflowsTool.HasPermission"/>).</item>
/// <item>Requires <c>bodyEditable: true</c>, reusing
/// <see cref="GetWorkflowDefinitionTool.BuildBody"/> so this precondition is byte-for-byte
/// identical to what <c>get_workflow_definition</c> itself reports.</item>
/// <item>Deserializes the body into <see cref="X6WorkflowSaveModel"/>/<see cref="Cell"/> —
/// the exact same Newtonsoft-attributed shape <see cref="X6ToWorkflowConverter"/> itself
/// (de)serializes — rather than hand-building raw JSON.</item>
/// <item>Resolves the attach point (<c>afterStepId</c>, or the <c>start</c> node) and walks
/// <see cref="FindChainTail"/> — exactly the same "walk single-outgoing-edge nodes, stop at
/// 0 or ≥2 outgoing edges" rule as the Angular chatbot's <c>findChainTail</c>.</item>
/// <item>Normalizes <c>step</c> (<see cref="NormalizeStepData"/>) — derives <c>data.type</c>
/// from <c>shape</c>, derives display-name fields from <c>label</c>, aliases HTTP
/// <c>requestUrl</c>/<c>url</c> to <c>querystring</c> — the leniency <c>get_tool_schema</c>
/// documents and this tool's own spec text calls out explicitly.</item>
/// <item>Builds the new node + connecting edge (<see cref="CommonHelper.CreateEdge"/> for the
/// edge — the exact helper the read-direction converter itself uses), appends both, recompiles
/// via <see cref="X6ToWorkflowConverter"/>.</item>
/// <item>Persists by preserving the existing file's <c>Service ID</c>/<c>DisplayName</c>/
/// <c>Comment</c>/<c>DataList</c> verbatim and only replacing <c>XamlDefinition</c> +
/// incrementing <c>VersionInfo/@VersionNumber</c> — via the new
/// <see cref="EnvelopeBiteWriter.BuildBiteFileContents(string,string,string,XElement,string,int,DateTimeOffset,string)"/>
/// overload. <c>add_step</c> has no <c>envelope</c> input, so rebuilding the <c>&lt;DataList&gt;</c>
/// from <see cref="ListWorkflowsTool.ReadDetail"/>'s flat name lists would silently degrade
/// recordset/IO-direction fidelity — preserving the existing element avoids that entirely.</item>
/// <item>Same <see cref="WorkflowIndex"/> cache refresh as <c>edit_workflow</c>.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Known current limitation (fidelity-allowlist.json, not a bug in this tool):</b> as of the
/// 2026-08-21 allow-list regeneration, "Decision" (<c>DsfFlowDecisionActivity</c>) is
/// <c>Pass</c>-fidelity, so the two-call sequence — add a Decision node, then a follow-up call to
/// wire its True/False arm — now completes end-to-end, as it already did for Switch. Only
/// "Decision (legacy)" (<c>DsfDecision</c>) remains non-Pass (<c>NoCorpusSample</c>): once a
/// workflow contains one, <see cref="GetWorkflowDefinitionTool.BuildBody"/> reports
/// <c>bodyEditable:false</c>, and because <c>add_step</c> reuses that exact gate the second call is
/// correctly rejected as not-editable. The Decision/Switch branch-selection
/// rules themselves (required branch, invalid value, already-wired arm/case) are still fully
/// implemented and unit-tested directly against <see cref="BuildConnectingEdge"/>, independent of
/// the fidelity gate.
/// </para>
///
/// <para>
/// <b>Node id stability:</b> <c>afterStepId</c> and the returned <c>stepId</c> only make sense if
/// the same node id is reported by every subsequent read of the same workflow. This previously
/// wasn't true — <see cref="WorkflowToX6Converter"/> minted a fresh random id for every activity on
/// every <c>ConvertToX6Json</c> call, so an id from one <c>get_workflow_definition</c>/<c>add_step</c>
/// call could never be resolved by a later call's <c>afterStepId</c>. Fixed by having
/// <see cref="X6ToWorkflowConverter"/> persist the X6 cell's <c>id</c> as the created activity's
/// <c>UniqueID</c> (a real, XAML-serialized property), and having <see cref="WorkflowToX6Converter"/>
/// prefer that persisted <c>UniqueID</c> over minting a fresh id when converting back. This is scoped
/// to the top-level activity chain (where <c>add_step</c> always attaches); ids for nodes nested
/// inside containers (ForEach/Sequence/etc. children) are still freshly generated on each read.
/// </para>
/// </summary>
internal static class AddStepTool
{
    internal const string ToolName = "add_step";

    static readonly HashSet<string> HttpActivityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "webgetactivity",
        "webpostactivitynew",
        "webputactivity",
        "dsfwebdeleteactivity",
        "dsfwebgetrequestwithtimeoutactivity",
    };

    static readonly HashSet<string> DecisionActivityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "flowdecision",
        "dsfdecision",
    };

    static readonly HashSet<string> SwitchActivityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "dsfflowswitchactivity",
        "flowswitch",
    };

    internal static AddStepResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The existing, editable workflow's name — a relative path (forward slashes), no extension. Must already exist and have bodyEditable:true.")]
        string name,
        [Description("The single tool payload to append — { shape, label?, data? } per add_step_schema. No id/position/edges — the server assigns those.")]
        System.Text.Json.JsonElement step,
        [Description("The node id (from a prior get_workflow_definition/add_step response) to attach after. Omit to attach after the current chain tail from the flowchart's start node.")]
        string? afterStepId = null,
        [Description("\"true\"/\"false\" for attaching under a Decision, or a case label for attaching under a Switch. Required when the resolved attach point is a Decision/Switch node with an unfilled branch.")]
        string? branch = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        if (step.ValueKind is not System.Text.Json.JsonValueKind.Object)
        {
            throw new McpException("`step` is required and must be a JSON object.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        var filePath = WorkflowNameResolver.Resolve(workflowsDirectory, relativePath);
        var (headerName, isWorkflow) = filePath is null
            ? (null, false)
            : ListWorkflowsTool.TryReadWorkflowHeader(filePath);

        if (filePath is null || !isWorkflow || headerName is null)
        {
            throw new McpException($"Workflow '{name}' was not found.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to edit workflow '{name}'.");
        }

        var (bodyEditable, body, nonEditableReason) = GetWorkflowDefinitionTool.BuildBody(filePath, headerName);
        if (!bodyEditable || body is null)
        {
            var reasonText = string.IsNullOrWhiteSpace(nonEditableReason) ? "" : $": {nonEditableReason}";
            throw new McpException($"Workflow '{name}' is not editable{reasonText}");
        }

        X6WorkflowSaveModel graph;
        try
        {
            graph = JsonConvert.DeserializeObject<X6WorkflowSaveModel>(body.Value.GetRawText())
                ?? new X6WorkflowSaveModel();
        }
        catch (Exception ex)
        {
            throw new McpException($"the workflow's current body could not be parsed: {ex.Message}");
        }

        graph.Cells ??= new List<Cell>();
        var nodes = graph.Cells.Where(c => !string.Equals(c.shape, "edge", StringComparison.OrdinalIgnoreCase)).ToList();
        var edges = graph.Cells.Where(c => string.Equals(c.shape, "edge", StringComparison.OrdinalIgnoreCase)).ToList();

        Cell attachPoint;
        if (!string.IsNullOrWhiteSpace(afterStepId))
        {
            attachPoint = nodes.FirstOrDefault(n => string.Equals(n.id, afterStepId, StringComparison.Ordinal))
                ?? throw new McpException($"`afterStepId` '{afterStepId}' was not found in workflow '{name}'.");
        }
        else
        {
            attachPoint = nodes.FirstOrDefault(IsStartNode)
                ?? throw new McpException($"Workflow '{name}' has no start node to attach from.");
        }

        var tail = FindChainTail(attachPoint, nodes, edges);

        // ── Normalize the caller-supplied step (leniency per get_tool_schema/add_step spec) ──
        var stepObj = JObject.Parse(step.GetRawText());
        var shape = (string?)stepObj["shape"];
        if (string.IsNullOrWhiteSpace(shape))
        {
            throw new McpException("`step.shape` is required.");
        }

        var label = (string?)stepObj["label"];
        var data = stepObj["data"] as JObject ?? new JObject();

        NormalizeStepData(shape, data, label);

        var dataType = (string?)data["type"];
        if (string.IsNullOrWhiteSpace(dataType))
        {
            throw new McpException("`step.data.type` (or a resolvable `step.shape`) is required.");
        }

        var entry = ToolCatalog.Resolve(dataType);
        if (entry is null)
        {
            throw new McpException($"'{dataType}' is not a recognised/supported toolbox activity type. Call list_tools/get_tool_schema for the supported set.");
        }

        // ── Resolve whether the tail requires `branch`, and build the connecting edge ──
        var newNodeId = Guid.NewGuid().ToString();
        var newEdge = BuildConnectingEdge(tail, branch, edges, newNodeId);

        var newNode = new Cell
        {
            id = newNodeId,
            shape = shape,
            data = data.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>(),
            position = tail.position is not null
                ? new Position(tail.position.X, tail.position.Y + 150)
                : new Position(100, 100),
        };

        graph.Cells.Add(newNode);
        graph.Cells.Add(newEdge);

        // ── Recompile ──
        string xamlDefinition;
        try
        {
            var xaml = new X6ToWorkflowConverter().X6JsonToWorkflow(JsonConvert.SerializeObject(graph));
            xaml = X6ToWorkflowConverter.AddReplaceNameSpace(xaml);
            xamlDefinition = xaml.ToString();
        }
        catch (Exception ex)
        {
            throw new McpException($"the updated workflow body could not be compiled: {ex.Message}");
        }

        // ── Persist, preserving the existing file's Service ID/DisplayName/Comment/DataList ──
        var (existingServiceId, existingVersionNumber, existingDisplayName, existingDescription, existingDataList) =
            ReadExistingServiceMetadata(filePath);

        var serviceId = string.IsNullOrWhiteSpace(existingServiceId) ? Guid.NewGuid().ToString() : existingServiceId;
        var displayName = string.IsNullOrWhiteSpace(existingDisplayName) ? headerName : existingDisplayName;
        var callerIdentity = principal?.CallerIdentity is { Length: > 0 } identity ? identity : "Anonymous";

        string biteContents;
        try
        {
            biteContents = EnvelopeBiteWriter.BuildBiteFileContents(
                serviceId,
                displayName,
                existingDescription,
                existingDataList,
                xamlDefinition,
                versionNumber: existingVersionNumber + 1,
                timestampUtc: DateTimeOffset.UtcNow,
                user: callerIdentity);
        }
        catch (McpException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new McpException($"the updated workflow could not be composed into a .bite file: {ex.Message}");
        }

        File.WriteAllText(filePath, biteContents);
        // Drop any pooled compilation of this workflow so the next execution picks the new
        // definition up. WorkflowExecutor's pool key already includes the file's timestamp+length,
        // so this is belt-and-braces for the one case that cannot see: a rewrite of identical
        // length landing within the filesystem's timestamp granularity - which is exactly what an
        // agent making rapid successive edits produces.
        WorkflowExecutor.EvictWorkflow(filePath);

        WorkflowIndex.Instance.AddOrUpdate(workflowsDirectory, relativePath, relativePath + ".bite");

        return new AddStepResult(name, newNodeId, true);
    }

    // ── findChainTail (mirrors graph.component.ts:1952) ────────────────────────

    /// <summary>
    /// Walks forward from <paramref name="start"/> through nodes that have exactly one
    /// outgoing edge, advancing to that edge's target each time. Stops (returns the current
    /// node) when the current node has 0 or ≥2 outgoing edges, the target is missing/not a
    /// node, or a cycle is detected — the exact same rule as the Angular chatbot's
    /// <c>findChainTail</c> (<c>graph.component.ts:1952-1981</c>). A Decision/Switch node with
    /// only one arm/case wired is walked through, not stopped at — only 0 or ≥2 outgoing edges
    /// halts the walk.
    /// </summary>
    internal static Cell FindChainTail(Cell start, IReadOnlyList<Cell> nodes, IReadOnlyList<Cell> edges)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var current = start;

        while (visited.Add(current.id))
        {
            var outgoing = edges.Where(e => string.Equals(e.Source?.Id, current.id, StringComparison.Ordinal)).ToList();
            if (outgoing.Count != 1)
            {
                return current;
            }

            var targetId = outgoing[0].Target?.Id;
            var next = targetId is null ? null : nodes.FirstOrDefault(n => string.Equals(n.id, targetId, StringComparison.Ordinal));
            if (next is null)
            {
                return current;
            }

            current = next;
        }

        return current;
    }

    static bool IsStartNode(Cell node) =>
        string.Equals(GetDataType(node), Constants.START, StringComparison.OrdinalIgnoreCase);

    static string? GetDataType(Cell node) =>
        node.data.TryGetValue("type", out var typeObj) ? typeObj?.ToString() : null;

    // ── Attach-point branch resolution / edge construction ─────────────────────

    /// <summary>
    /// Resolves whether <paramref name="tail"/> requires a <paramref name="branch"/> argument
    /// (Decision → "true"/"false", Switch → a case label, plain node → none) and builds the
    /// connecting <see cref="Cell"/> edge via <see cref="CommonHelper.CreateEdge"/> — the same
    /// helper <see cref="X6ToWorkflowConverter"/> itself uses when writing Decision/Switch
    /// edges. Exposed <c>internal</c> so branch-validation rules (required branch, invalid
    /// value, already-wired arm/case) can be unit-tested directly against a hand-built graph,
    /// independent of the <c>bodyEditable</c> fidelity gate in <see cref="Handle"/> — which,
    /// as of the current <c>fidelity-allowlist.json</c>, blocks a real end-to-end two-call
    /// "add a Decision, then branch it" sequence since "Decision" itself is not yet
    /// Pass-fidelity (see <see cref="Handle"/>'s XML doc and the accompanying test suite).
    /// </summary>
    /// <exception cref="McpException">
    /// Thrown when <paramref name="branch"/> is missing/invalid for a Decision/Switch tail, or
    /// when the requested arm/case is already wired to another step, or when a non-branching
    /// tail already has an outgoing connection.
    /// </exception>
    internal static Cell BuildConnectingEdge(Cell tail, string? branch, IReadOnlyList<Cell> edges, string newNodeId)
    {
        var tailDataType = GetDataType(tail);
        var tailIsDecision = tailDataType is not null && DecisionActivityTypes.Contains(tailDataType);
        var tailIsSwitch = tailDataType is not null && SwitchActivityTypes.Contains(tailDataType);

        if (tailIsDecision)
        {
            if (string.IsNullOrWhiteSpace(branch) ||
                !(string.Equals(branch, "true", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(branch, "false", StringComparison.OrdinalIgnoreCase)))
            {
                throw new McpException($"`branch` must be \"true\" or \"false\" to attach after '{tail.id}', a Decision node.");
            }

            var isTrueArm = string.Equals(branch, "true", StringComparison.OrdinalIgnoreCase);
            var armAlreadyWired = edges.Any(e =>
                string.Equals(e.Source?.Id, tail.id, StringComparison.Ordinal) &&
                CommonHelper.TryGetBool(e.data, Constants.ISDECISIONARM, out var isDecisionArm) && isDecisionArm &&
                CommonHelper.TryGetBool(e.data, Constants.ISTRUEARM, out var existingIsTrue) && existingIsTrue == isTrueArm);

            if (armAlreadyWired)
            {
                throw new McpException($"'{tail.id}' already has its \"{branch.ToLowerInvariant()}\" arm wired to another step.");
            }

            var edge = CommonHelper.CreateEdge(tail.id, newNodeId, label: isTrueArm ? Constants.TRUE : Constants.FALSE, isDecisionArm: true, isTrueArm: isTrueArm);
            edge.shape = "edge";
            return edge;
        }

        if (tailIsSwitch)
        {
            if (string.IsNullOrWhiteSpace(branch))
            {
                throw new McpException($"`branch` (a case label) is required to attach after '{tail.id}', a Switch node.");
            }

            var caseAlreadyWired = edges.Any(e =>
                string.Equals(e.Source?.Id, tail.id, StringComparison.Ordinal) &&
                string.Equals(e.label, branch, StringComparison.OrdinalIgnoreCase));

            if (caseAlreadyWired)
            {
                throw new McpException($"'{tail.id}' already has a case '{branch}' wired to another step.");
            }

            var switchEdge = CommonHelper.CreateEdge(tail.id, newNodeId, label: branch);
            switchEdge.shape = "edge";
            return switchEdge;
        }

        var alreadyHasOutgoing = edges.Any(e => string.Equals(e.Source?.Id, tail.id, StringComparison.Ordinal));
        if (alreadyHasOutgoing)
        {
            throw new McpException($"'{tail.id}' already has an outgoing connection; specify `afterStepId` naming a different attach point.");
        }

        var plainEdge = CommonHelper.CreateEdge(tail.id, newNodeId);
        plainEdge.shape = "edge";
        return plainEdge;
    }

    // ── Leniency normalization (get_tool_schema / add_step spec) ───────────────

    /// <summary>
    /// Applies the two leniency rules the <c>add_step</c> spec text explicitly calls out
    /// (mirroring <c>chatbot-tool-normalizer.service.ts</c>): derives <c>data.type</c> from
    /// <c>shape</c> when omitted, derives a display name from <c>label</c> when omitted (also
    /// setting <c>displaytext</c> for Decision-family types, which
    /// <see cref="X6ToWorkflowConverter"/> requires non-blank to compile at all), and aliases
    /// HTTP tools' <c>requestUrl</c>/<c>url</c> to <c>querystring</c>.
    /// </summary>
    internal static void NormalizeStepData(string shape, JObject data, string? label)
    {
        if (string.IsNullOrWhiteSpace((string?)data["type"]))
        {
            var shapeEntry = ToolCatalog.Resolve(shape);
            if (shapeEntry is not null)
            {
                data["type"] = shapeEntry.DataTypes[0];
            }
        }

        var dataType = (string?)data["type"];
        var hasDisplayName = !string.IsNullOrWhiteSpace((string?)data["displayname"]) ||
                              !string.IsNullOrWhiteSpace((string?)data["displayName"]);

        if (!hasDisplayName)
        {
            var derivedName = !string.IsNullOrWhiteSpace(label) ? label : dataType is null ? null : ToolCatalog.Resolve(dataType)?.Name;
            if (!string.IsNullOrWhiteSpace(derivedName))
            {
                data["displayname"] = derivedName;
                data["displayName"] = derivedName;
            }
        }

        if (dataType is not null && DecisionActivityTypes.Contains(dataType) &&
            string.IsNullOrWhiteSpace((string?)data["displaytext"]))
        {
            var derivedText = !string.IsNullOrWhiteSpace(label) ? label : ToolCatalog.Resolve(dataType)?.Name;
            if (!string.IsNullOrWhiteSpace(derivedText))
            {
                data["displaytext"] = derivedText;
            }
        }

        if (dataType is not null && HttpActivityTypes.Contains(dataType) &&
            string.IsNullOrWhiteSpace((string?)data[Constants.WEBMETHOD_QUERYSTRING]))
        {
            var alias = (string?)data["requestUrl"] ?? (string?)data["url"];
            if (!string.IsNullOrWhiteSpace(alias))
            {
                data[Constants.WEBMETHOD_QUERYSTRING] = alias;
            }
        }
    }

    // ── Existing-file metadata preservation (mirrors EditWorkflowTool.ReadExistingServiceMetadata) ──

    /// <summary>
    /// Reads the existing file's root <c>ID</c>/<c>Name</c>/<c>Comment</c>/<c>DataList</c> —
    /// preserved verbatim across an <c>add_step</c> save, since it has no <c>envelope</c> input
    /// and must not alter the workflow's declared variables — and its current
    /// <c>VersionInfo/@VersionNumber</c> (to increment from). Tolerant of a missing/malformed
    /// file, returning safe fallbacks rather than failing the whole call.
    /// </summary>
    static (string? serviceId, int versionNumber, string? displayName, string description, XElement dataList) ReadExistingServiceMetadata(string filePath)
    {
        try
        {
            var root = XElement.Load(filePath);
            var serviceId = root.Attribute("ID")?.Value;
            var versionNumberText = root.Element("VersionInfo")?.Attribute("VersionNumber")?.Value;
            var versionNumber = int.TryParse(versionNumberText, out var parsed) ? parsed : 0;
            var displayName = root.Element("DisplayName")?.Value ?? root.Attribute("Name")?.Value;
            var description = root.Element("Comment")?.Value ?? string.Empty;
            var dataList = root.Element("DataList") is { } existing ? new XElement(existing) : new XElement("DataList");
            return (serviceId, versionNumber, displayName, description, dataList);
        }
        catch
        {
            return (null, 0, null, string.Empty, new XElement("DataList"));
        }
    }
}

/// <summary>The full <c>add_step</c> response payload.</summary>
internal sealed record AddStepResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("stepId")] string StepId,
    [property: JsonPropertyName("updated")] bool Updated);
