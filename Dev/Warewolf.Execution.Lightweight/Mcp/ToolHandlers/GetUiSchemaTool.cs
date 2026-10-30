/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_ui_schema</c> MCP tool: returns the hard-coded JSON shape of a workflow
/// form — what <c>create_ui</c> accepts and the web Studio renders at <c>/form/{workflowId}</c> to
/// collect the workflow's inputs and run it. Static reference documentation, like
/// <see cref="GetWorkflowSchemaTool"/>; the closed value sets come from
/// <see cref="WorkflowUiCatalog"/> so the schema can't drift from what validation enforces.
/// </summary>
internal static class GetUiSchemaTool
{
    internal const string ToolName = "get_ui_schema";

    static readonly JsonElement _formSchema = Parse($$"""
        {
          "title": "string, required (the form's heading)",
          "description": "string, optional (shown under the heading)",
          "submitLabel": "string, optional (the run button's text; default \"Run\")",
          "fields": [
            {
              "input": "string, required — the name of one of the workflow's envelope inputs (get_workflow_definition), each at most once",
              "label": "string, optional (default: the input name)",
              "control": "string, optional — scalar input: {{string.Join(" | ", WorkflowUiCatalog.ControlsByKind["scalar"])}} (default text); object input: json; recordset input: table",
              "required": "boolean, optional",
              "placeholder": "string, optional",
              "help": "string, optional (hint shown under the control)",
              "default": "optional initial value — a string for a scalar (\"true\"/\"false\" for a checkbox), an object for json, an array of row objects for table",
              "options": [ { "value": "string, required", "label": "string, optional" } ],
              "min": "number or string, optional (number/date/datetime/time controls)",
              "max": "number or string, optional (number/date/datetime/time controls)",
              "step": "number, optional (number control)",
              "pattern": "string, optional (a regular expression the whole value must match)",
              "maxLength": "integer, optional",
              "rows": "integer, optional (textarea height)",
              "columns": [
                {
                  "field": "string, required — one of the recordset's input fields",
                  "label": "string, optional",
                  "control": "{{string.Join(" | ", WorkflowUiCatalog.ColumnControls)}}, optional (default text)",
                  "required": "boolean, optional",
                  "options": [ { "value": "string", "label": "string, optional" } ]
                }
              ]
            }
          ],
          "outputs": [
            {
              "output": "string, required — the name of one of the workflow's envelope outputs",
              "label": "string, optional",
              "display": "{{string.Join(" | ", WorkflowUiCatalog.OutputDisplays)}}, optional (default: table for a recordset, json for an object, text otherwise)"
            }
          ]
        }
        """);

    const string Notes =
        "One form per workflow; create_ui replaces any existing one. Inputs with no field are sent empty. " +
        "`options` only on select/radio (and select columns); `columns` only on table, and omitting it shows every input field of the recordset as text. " +
        "Omit `outputs` to show every output after a run. Unknown properties are rejected. " +
        "Scalar values are sent to the workflow as strings: a checkbox sends \"true\"/\"false\", a number its digits.";

    static readonly GetUiSchemaResult _result = new(_formSchema, Notes);

    internal static GetUiSchemaResult Handle() => _result;

    static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

/// <summary>The full <c>get_ui_schema</c> response payload.</summary>
internal sealed record GetUiSchemaResult(
    [property: JsonPropertyName("form_schema")] JsonElement FormSchema,
    [property: JsonPropertyName("notes")] string Notes);
