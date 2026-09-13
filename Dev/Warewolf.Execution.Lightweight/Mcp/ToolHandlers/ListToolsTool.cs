/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>list_tools</c> MCP tool: returns the toolbox activities the MCP server can place inside a
/// workflow body via <c>create_workflow</c>/<c>edit_workflow</c>/<c>add_step</c> — the static
/// **Toolbox subset (v3)** table, sourced from <see cref="ToolCatalog"/>.
///
/// <para>
/// Takes no input and depends on nothing request-scoped (no permission check — the toolbox is
/// the same for every caller on this instance), so unlike <see cref="ListWorkflowsTool"/> and
/// <see cref="GetWorkflowDefinitionTool"/> this handler needs none of the DI-resolvable
/// parameter types the MCP tool binder special-cases.
/// </para>
/// </summary>
internal static class ListToolsTool
{
    internal const string ToolName = "list_tools";

    internal static ListToolsToolResult Handle()
    {
        var tools = ToolCatalog.Entries
            .Select(entry => new ToolCatalogItem(
                entry.Name,
                entry.ActivityType,
                entry.DataTypes[0],
                entry.Category,
                entry.Description,
                true,
                entry.RequiresSource))
            .ToList();

        return new ListToolsToolResult(tools);
    }
}

/// <summary>One toolbox entry in a <c>list_tools</c> response.</summary>
internal sealed record ToolCatalogItem(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("activityType")] string ActivityType,
    [property: JsonPropertyName("dataType")] string DataType,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("editable")] bool Editable,
    [property: JsonPropertyName("requiresSource")] bool RequiresSource);

/// <summary>The full <c>list_tools</c> response payload.</summary>
internal sealed record ListToolsToolResult(
    [property: JsonPropertyName("tools")] IReadOnlyList<ToolCatalogItem> Tools);
