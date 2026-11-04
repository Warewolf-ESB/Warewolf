/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Warewolf.Bite.Json;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_bite_definition_schema</c> MCP tool: returns the JSON Schema of the version 1
/// <c>&lt;BiteDefinition&gt;</c> JSON body (WF removal T3.1), as <see cref="BiteJsonSchema"/> generates it.
///
/// <para>
/// Separate from <see cref="GetWorkflowSchemaTool"/> on purpose: the schema covers every activity's
/// members and is several hundred KB, so it is fetched only by a caller that needs it rather than
/// inlined into every <c>get_workflow_schema</c> response. Generated from this engine's own
/// activity assemblies, once per process, so it always matches what this engine reads.
/// </para>
/// </summary>
internal static class GetBiteDefinitionSchemaTool
{
    internal const string ToolName = "get_bite_definition_schema";

    static readonly Lazy<GetBiteDefinitionSchemaResult> _result = new(() =>
    {
        using var document = JsonDocument.Parse(BiteJsonSchema.Generate());
        return new GetBiteDefinitionSchemaResult(BiteJsonWriter.FormatVersion, document.RootElement.Clone());
    });

    internal static GetBiteDefinitionSchemaResult Handle() => _result.Value;
}

/// <summary>The full <c>get_bite_definition_schema</c> response payload.</summary>
internal sealed record GetBiteDefinitionSchemaResult(
    [property: JsonPropertyName("format_version")] int FormatVersion,
    [property: JsonPropertyName("schema")] JsonElement Schema);
