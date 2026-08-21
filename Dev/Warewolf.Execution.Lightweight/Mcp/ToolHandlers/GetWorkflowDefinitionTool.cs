/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;
using Newtonsoft.Json.Linq;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_workflow_definition</c> MCP tool: returns a workflow's envelope, and its body
/// (the X6 graph) only when the fidelity gate proves every activity type it uses round-trips.
///
/// <para>
/// <b>Resolution/permission/envelope parsing are shared with <see cref="ListWorkflowsTool"/></b>
/// (<see cref="ListWorkflowsTool.TryReadWorkflowHeader"/>, <see cref="ListWorkflowsTool.HasViewPermission"/>,
/// <see cref="ListWorkflowsTool.ReadDetail"/>) so a <c>name</c> that came from a prior
/// <c>list_workflows</c> response's <c>path</c> resolves and reports identically here.
/// </para>
///
/// <para>
/// <b>Body production.</b> Per spec, the body is never stored/cached — it is produced fresh
/// on every call by reading the workflow's XAML (<see cref="WorkflowExecutor.ReadWorkflowFile"/>
/// / <see cref="WorkflowExecutor.ExtractWorkflowParts"/>), compiling it to an
/// <see cref="System.Activities.ActivityBuilder"/> (<see cref="XamlActivityBuilderLoader"/>),
/// and running <c>WorkflowToX6Converter.ConvertToX6Json</c>. That call returns
/// <c>X6WorkflowLoadModel</c>-shaped JSON (<c>{workflowxml, nodes, edges}</c>) — a different
/// shape from the <c>X6WorkflowSaveModel</c> (<c>{resourcename, cells}</c>) the spec's
/// <c>body_schema</c> and <c>X6ToWorkflowConverter.X6JsonToWorkflow</c> (used by
/// <c>create_workflow</c>/<c>edit_workflow</c>) expect. <see cref="Handle"/> merges
/// <c>nodes</c>++<c>edges</c> into a single <c>cells</c> array so the returned body is both
/// spec-compliant and directly round-trippable back through <c>X6JsonToWorkflow</c>.
/// </para>
///
/// <para>
/// <b><c>bodyEditable</c> (fidelity gate, v3, now implemented).</b> Every node's <c>data.type</c>
/// is resolved via <see cref="ToolCatalog.Resolve"/> to its Studio name, then checked against
/// <see cref="FidelityAllowList.IsEditable"/> — only <c>Status == "Pass"</c> qualifies (see that
/// class's remarks for why <c>PassBothFailedIdentically</c> does not). A workflow using even
/// one unresolvable or non-passing activity type stays <c>bodyEditable: false</c> with a
/// <c>nonEditableReason</c> naming the offending type, per spec.
/// </para>
/// </summary>
internal static class GetWorkflowDefinitionTool
{
    internal const string ToolName = "get_workflow_definition";

    internal static GetWorkflowDefinitionResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user = null,
        [Description("Workflow name — must match a `path` from a prior list_workflows response.")]
        string name = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
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

        if (!ListWorkflowsTool.HasViewPermission(authPolicyLoader, principal, relativePath))
        {
            throw new McpException($"You do not have permission to view workflow '{name}'.");
        }

        var detail = ListWorkflowsTool.ReadDetail(new ListWorkflowsTool.WorkflowFile(headerName, relativePath, filePath));
        var envelope = new WorkflowEnvelope(headerName, detail.Description, detail.Inputs, detail.Outputs);

        var (bodyEditable, body, nonEditableReason) = BuildBody(filePath, headerName);

        return new GetWorkflowDefinitionResult(headerName, envelope, bodyEditable, body, nonEditableReason);
    }

    // ── Body production + fidelity gate ───────────────────────────────────────

    /// <summary>
    /// <c>internal</c> so <see cref="AddStepTool"/> can reuse the exact same
    /// "load XAML → convert to X6 → fidelity gate" pipeline rather than duplicating it —
    /// <c>add_step</c>'s "must already be <c>bodyEditable: true</c>" precondition needs to be
    /// byte-for-byte identical to what this tool reports.
    /// </summary>
    internal static (bool editable, JsonElement? body, string? reason) BuildBody(string filePath, string workflowName)
    {
        StringBuilder xamlDefinition;
        try
        {
            var fileContents = WorkflowExecutor.ReadWorkflowFile(filePath);
            (xamlDefinition, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
        }
        catch (Exception ex)
        {
            return (false, null, $"the workflow's resource file could not be parsed: {ex.Message}");
        }

        if (xamlDefinition is null || xamlDefinition.Length == 0)
        {
            return (false, null, "the workflow has no XAML definition to convert");
        }

        System.Activities.ActivityBuilder? activityBuilder;
        try
        {
            activityBuilder = XamlActivityBuilderLoader.Load(xamlDefinition);
        }
        catch (Exception ex)
        {
            return (false, null, $"the workflow's XAML could not be compiled: {ex.Message}");
        }

        if (activityBuilder is null)
        {
            return (false, null, "the workflow's XAML could not be compiled into an ActivityBuilder");
        }

        string x6Json;
        try
        {
            x6Json = new Dev2.Activities.WF.WorkflowToX6Converter().ConvertToX6Json(activityBuilder, xamlDefinition.ToString());
        }
        catch (Exception ex)
        {
            return (false, null, $"the workflow's XAML could not be converted to the X6 graph format: {ex.Message}");
        }

        JObject loadModel;
        try
        {
            loadModel = JObject.Parse(x6Json);
        }
        catch (Exception ex)
        {
            return (false, null, $"the converted X6 graph was not valid JSON: {ex.Message}");
        }

        var nodes = loadModel["nodes"] as JArray ?? new JArray();
        var edges = loadModel["edges"] as JArray ?? new JArray();

        var reason = FindFirstNonEditableReason(nodes);
        if (reason is not null)
        {
            return (false, null, reason);
        }

        var cells = new JArray();
        foreach (var node in nodes)
        {
            cells.Add(node);
        }
        foreach (var edge in edges)
        {
            // WorkflowToX6Converter/CommonHelper.CreateEdge does not stamp a "shape" on edge
            // cells, but X6ToWorkflowConverter (and add_step's own node/edge split) identify
            // connections purely via shape == "edge". Without this, every edge coming back from
            // a real, already-existing workflow would be misclassified as a node downstream.
            if (edge is JObject edgeObject)
            {
                edgeObject["shape"] = "edge";
            }

            cells.Add(edge);
        }

        var saveModel = new JObject
        {
            ["resourcename"] = workflowName,
            ["cells"] = cells,
        };

        var bodyJson = saveModel.ToString(Newtonsoft.Json.Formatting.None);
        using var document = JsonDocument.Parse(bodyJson);
        return (true, document.RootElement.Clone(), null);
    }

    /// <summary>
    /// Returns a spec-shaped <c>nonEditableReason</c>
    /// (e.g. <c>"contains activity type 'DsfPythonActivity', which has not yet passed
    /// round-trip fidelity testing"</c>) for the first node whose <c>data.type</c> either
    /// doesn't resolve to a <see cref="ToolCatalog"/> entry at all, or resolves to one that
    /// hasn't passed <see cref="FidelityAllowList"/>. Returns <c>null</c> when every node
    /// passes. The Studio "start" node is not a real activity and is always skipped.
    /// </summary>
    static string? FindFirstNonEditableReason(JArray nodes)
    {
        foreach (var node in nodes)
        {
            var dataType = node["data"]?["type"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(dataType) ||
                string.Equals(dataType, Dev2.Common.X6.Constants.START, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var entry = ToolCatalog.Resolve(dataType);
            if (entry is null)
            {
                return $"contains activity type '{dataType}', which is not a recognised/supported toolbox activity";
            }

            if (!FidelityAllowList.IsEditable(entry.Name))
            {
                return $"contains activity type '{entry.ActivityType}', which has not yet passed round-trip fidelity testing";
            }
        }

        return null;
    }
}

/// <summary>The <c>envelope</c> object in a <c>get_workflow_definition</c> response, per <c>envelope_schema</c>.</summary>
internal sealed record WorkflowEnvelope(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("inputs")] IReadOnlyList<string> Inputs,
    [property: JsonPropertyName("outputs")] IReadOnlyList<string> Outputs);

/// <summary>The full <c>get_workflow_definition</c> response payload.</summary>
internal sealed record GetWorkflowDefinitionResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("envelope")] WorkflowEnvelope Envelope,
    [property: JsonPropertyName("bodyEditable")] bool BodyEditable,
    [property: JsonPropertyName("body")] JsonElement? Body,
    [property: JsonPropertyName("nonEditableReason")] string? NonEditableReason);
