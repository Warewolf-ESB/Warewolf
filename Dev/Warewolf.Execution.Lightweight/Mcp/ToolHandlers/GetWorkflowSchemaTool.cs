/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_workflow_schema</c> MCP tool (<c>warewolf-lee-mcp-v3-spec.md</c>,
/// "Tools" § <c>get_workflow_schema</c>): returns the hard-coded JSON shape of the overall
/// workflow <b>envelope</b> and the <b>body</b>/<b>add_step</b> graph shapes used by
/// <c>create_workflow</c>/<c>edit_workflow</c>/<c>add_step</c>.
///
/// <para>
/// Takes no input, needs no permission check, and needs nothing request-scoped — the schema
/// is static reference documentation, identical for every caller on this instance (same
/// reasoning as <see cref="ListToolsTool"/>). The three shapes below mirror the spec's own
/// "Body data model: the X6 graph" and "Dual authoring surface" sections literally; nothing
/// here is derived from <see cref="ToolCatalog"/> or any live workflow — per-tool
/// <c>data</c> field shapes are <c>get_tool_schema</c>'s job, not this tool's.
/// </para>
/// </summary>
internal static class GetWorkflowSchemaTool
{
    internal const string ToolName = "get_workflow_schema";

    // Static JSON-Schema-ish reference documents, built once and reused for every call —
    // there is nothing request-scoped or workflow-specific about any of these three shapes.
    static readonly JsonElement _envelopeSchema = Parse("""
        {
          "name": "string",
          "description": "string",
          "inputs": [
            { "kind": "scalar | object | recordset", "name": "string", "fields": ["string (recordset only)"] }
          ],
          "outputs": [
            { "kind": "scalar | object | recordset", "name": "string", "fields": ["string (recordset only)"] }
          ]
        }
        """);

    static readonly JsonElement _bodySchema = Parse("""
        {
          "resourcename": "string",
          "cells": [
            {
              "id": "string (required on nodes; a start node with data.type=start is required)",
              "shape": "string (Studio shape name; see list_tools/get_tool_schema)",
              "data": {
                "type": "string (a dataType from list_tools, case-insensitive substring match)",
                "displayname": "string (Studio label)",
                "isNested": "boolean, optional (true for a node nested inside a ForEach/Sequence/Select-and-apply container)",
                "parentId": "string, optional (the container node's id; required when isNested is true)",
                "...": "remaining fields are activity-specific — see get_tool_schema"
              },
              "position": { "x": "number, optional", "y": "number, optional" },
              "size": { "width": "number, optional", "height": "number, optional" }
            },
            {
              "source": { "cell": "string (a node id)" },
              "target": { "cell": "string (a node id)" },
              "label": "string, optional (branch/case label — e.g. true/false for Decision, a case name for Switch)"
            }
          ]
        }
        """);

    static readonly JsonElement _addStepSchema = Parse("""
        {
          "shape": "string (Studio shape name; see list_tools/get_tool_schema)",
          "label": "string, optional (Studio label; data.displayname is derived from this when data.displayname is omitted)",
          "data": {
            "type": "string, optional (derived from shape when omitted, per the Angular chatbot's normalizer leniency)",
            "...": "remaining fields are activity-specific — see get_tool_schema"
          },
          "afterStepId": "string, optional (a node id from a prior get_workflow_definition/add_step response; omit to attach after the current chain tail)",
          "branch": "string, optional (true | false | a case label; required when afterStepId names a Decision/Switch node with an unfilled branch)"
        }
        """);

    static readonly GetWorkflowSchemaResult _result = new(_envelopeSchema, _bodySchema, _addStepSchema);

    internal static GetWorkflowSchemaResult Handle() => _result;

    static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

/// <summary>The full <c>get_workflow_schema</c> response payload.</summary>
internal sealed record GetWorkflowSchemaResult(
    [property: JsonPropertyName("envelope_schema")] JsonElement EnvelopeSchema,
    [property: JsonPropertyName("body_schema")] JsonElement BodySchema,
    [property: JsonPropertyName("add_step_schema")] JsonElement AddStepSchema);
