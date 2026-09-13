/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_tool_schema</c> MCP tool: returns the JSON shape of one toolbox tool's <c>data</c> fields, for
/// placement inside a workflow body's <c>cells</c> array (<c>create_workflow</c>/
/// <c>edit_workflow</c>) or as an <c>add_step</c> payload.
///
/// <para>
/// <b>Lookup key.</b> Per spec, <c>tool_name</c> "must match a <c>name</c> from
/// <c>list_tools</c>" — i.e. the Studio display name (<see cref="ToolCatalog.Entry.Name"/>,
/// e.g. <c>"Assign"</c>, <c>"SQL Server Database"</c>), matched case-insensitively. This is
/// deliberately <b>not</b> <see cref="ToolCatalog.Resolve"/>, which matches a <c>data.type</c>
/// value (e.g. <c>"dsfdotnetmultiassignactivity"</c>) via substring — a different lookup
/// direction used by the body/graph tools, not this one.
/// </para>
///
/// <para>
/// <b>Field-level content</b> is static reference data authored in <see cref="ToolSchemaCatalog"/>,
/// one document per <see cref="ToolCatalog.Entries"/> row, grounded directly in each activity's
/// own <c>ToX6Json</c>/<c>FromX6Json</c> source and the <c>Constants.*</c> field-name group it
/// reads/writes (see that class's remarks for the full sourcing methodology).
/// </para>
///
/// <para>
/// Takes no request-scoped input beyond <paramref name="tool_name"/> and needs no permission
/// check — like <see cref="ListToolsTool"/> and <see cref="GetWorkflowSchemaTool"/>, this is
/// static reference documentation, identical for every caller on this instance.
/// </para>
/// </summary>
internal static class GetToolSchemaTool
{
    internal const string ToolName = "get_tool_schema";

    internal static GetToolSchemaResult Handle(
        [Description("Tool name — must match a `name` from a prior list_tools response, e.g. \"Assign\" or \"SQL Server Database\".")]
        string tool_name = "")
    {
        if (string.IsNullOrWhiteSpace(tool_name))
        {
            throw new McpException("`tool_name` is required.");
        }

        var entry = FindEntry(tool_name);
        if (entry is null)
        {
            throw new McpException($"'{tool_name}' is not a recognized Warewolf tool. Call list_tools for the supported set.");
        }

        if (!ToolSchemaCatalog.TryGet(entry.Name, out var schema))
        {
            // Every ToolCatalog entry has a matching ToolSchemaCatalog document (enforced by
            // ToolSchemaCatalogTests); reaching here means the two catalogs have drifted.
            throw new McpException($"No schema is authored yet for '{entry.Name}'.");
        }

        return new GetToolSchemaResult(entry.Name, entry.ActivityType, entry.DataTypes[0], schema);
    }

    static ToolCatalog.Entry? FindEntry(string toolName)
    {
        foreach (var entry in ToolCatalog.Entries)
        {
            if (string.Equals(entry.Name, toolName, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }
}

/// <summary>The full <c>get_tool_schema</c> response payload.</summary>
internal sealed record GetToolSchemaResult(
    [property: JsonPropertyName("tool_name")] string ToolName,
    [property: JsonPropertyName("activity_type")] string ActivityType,
    [property: JsonPropertyName("dataType")] string DataType,
    [property: JsonPropertyName("schema")] JsonElement Schema);
