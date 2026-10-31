/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_workflow_schema</c> MCP tool: returns the hard-coded JSON shape of the overall
/// workflow <b>envelope</b>, the <b>body</b>/<b>add_step</b> graph shapes used by
/// <c>create_workflow</c>/<c>edit_workflow</c>/<c>add_step</c>, and the <b>test</b> definition
/// <c>create_test</c>/<c>edit_test</c> take.
///
/// <para>
/// Takes no input, needs no permission check, and needs nothing request-scoped — the schema
/// is static reference documentation, identical for every caller on this instance (same
/// reasoning as <see cref="ListToolsTool"/>). The three shapes below mirror the spec's own
/// "Body data model: the X6 graph" and "Dual authoring surface" sections literally; nothing
/// here is derived from <see cref="ToolCatalog"/> or any live workflow — per-tool
/// <c>data</c> field shapes are <c>get_tool_schema</c>'s job, not this tool's. The JSON Schema of the
/// stored <c>&lt;BiteDefinition&gt;</c> body is <see cref="GetBiteDefinitionSchemaTool"/>'s, kept out of
/// this response because of its size.
/// </para>
/// </summary>
internal static class GetWorkflowSchemaTool
{
    internal const string ToolName = "get_workflow_schema";

    // Static JSON-Schema-ish reference documents, built once and reused for every call —
    // there is nothing request-scoped or workflow-specific about any of these shapes.
    static readonly JsonElement _envelopeSchema = Parse("""
        {
          "name": "string",
          "description": "string",
          "inputs": [
            { "kind": "scalar | object | recordset", "name": "string", "fields": ["string (recordset only)"] }
          ],
          "outputs": [
            { "kind": "scalar | object | recordset", "name": "string", "fields": ["string (recordset only)"] }
          ],
          "variables": [
            { "kind": "scalar | object | recordset", "name": "string", "fields": ["string (recordset only)"], "...": "optional — working variables the body uses that are neither an input nor an output" }
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
              "shape": "string, required — must be exactly \"edge\"; this is how a cell is told apart from a node",
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

    // The create_test/edit_test `test` argument (TestDefinitionModel), with the closed value sets those tools and
    // execute_test enforce: AuthenticationType, StepType, and the DecisionTypeDisplayValue strings AssertOp matches.
    static readonly JsonElement _testSchema = Parse("""
        {
          "testName": "string, required (also the file name: {workflow}.tests/{testName}.test.json)",
          "enabled": "boolean, optional (default true)",
          "authenticationType": "string, optional — Windows | User | Anonymous | Public | Password | API_Key | ApiKeyQuery | ApiKeyHeader | Bearer",
          "userName": "string, optional (non-Public authenticationType only)",
          "password": "string, optional (non-Public only; may be a ${secret} placeholder)",
          "inputs": [
            { "variable": "string ([[name]] or [[rs(1).field]])", "value": "string", "emptyIsNull": "boolean, optional" }
          ],
          "outputs": [
            {
              "variable": "string ([[name]] or [[rs(1).field]])",
              "assertOp": "string, optional (default \"=\") — = | <> (Not Equal) | < | <= | > | >= | Contains | Doesn't Contain | Starts With | Doesn't Start With | Ends With | Doesn't End With | Is Between | Not Between | Is NULL | Is Not NULL | Is Numeric | Not Numeric | Is Text | Not Text | Is Alphanumeric | Not Alphanumeric | Is XML | Not XML | Is Date | Not Date | Is Email | Not Email | Is Regex | Not Regex | Is Binary | Not Binary | Is Hex | Not Hex | Is Base64 | Not Base64 | There is An Error | There is No Error",
              "value": "string (expected value; unused by the Is/Not checks)",
              "from": "string, optional (lower bound for Is Between/Not Between)",
              "to": "string, optional (upper bound for Is Between/Not Between)"
            }
          ],
          "noErrorExpected": "boolean, optional",
          "errorExpected": "boolean, optional",
          "errorContainsText": "string, optional (with errorExpected)",
          "testSteps": [
            {
              "activityId": "string, required (a node id in the workflow's current body, from get_workflow_definition)",
              "activityType": "string (the node's data.type)",
              "type": "string, required — Mock (replace the step's outputs) | Assert (check the step's outputs after it runs)",
              "stepDescription": "string, optional",
              "stepOutputs": [ { "variable": "string", "assertOp": "string, as outputs[].assertOp", "value": "string", "from": "string, optional", "to": "string, optional" } ],
              "children": ["testSteps entry, optional (steps nested in a Decision/ForEach/Sequence)"]
            }
          ]
        }
        """);

    static readonly GetWorkflowSchemaResult _result = new(_envelopeSchema, _bodySchema, _addStepSchema, _testSchema);

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
    [property: JsonPropertyName("add_step_schema")] JsonElement AddStepSchema,
    [property: JsonPropertyName("test_schema")] JsonElement TestSchema);
