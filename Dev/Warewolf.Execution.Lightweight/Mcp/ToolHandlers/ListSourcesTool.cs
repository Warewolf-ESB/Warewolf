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
using System.Text.Json.Serialization;
using System.Xml;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>list_sources</c> MCP tool: returns the connection sources stored on this
/// instance (name, path, resource type and resource ID), filtered to what the caller is
/// authorized to <b>view</b> — the source-side counterpart of <see cref="ListWorkflowsTool"/>.
///
/// <para>
/// <b>Why it exists.</b> A Web/Database step references its source only by <c>sourceId</c>, and
/// nothing else in <c>/mcp-api/*</c> maps a source name to that ID: <c>add_source</c>'s
/// "already exists" error had no ID, and <c>edit_source</c> only reports one after rebuilding the
/// source's whole config. So repointing a workflow at an existing source meant asking the user to
/// copy the GUID out of the Studio.
/// </para>
///
/// <para>
/// <b>Never reads the connection string.</b> Only the root element's attributes are scanned (same
/// forward-only header read as <see cref="ListWorkflowsTool"/>); the encrypted
/// <c>ConnectionString</c> is neither decrypted nor returned, so no address, user name or secret
/// leaves the engine here.
/// </para>
/// </summary>
internal static class ListSourcesTool
{
    internal const string ToolName = "list_sources";

    internal static ListSourcesResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user = null,
        [Description("Restrict results to sources under this folder (relative path, forward slashes). Omit to list from the root.")]
        string? folder = null,
        [Description("Opaque pagination token from a prior response's nextCursor. Omit to start from the first page.")]
        string? cursor = null,
        [Description("Maximum number of results to return. Default 50, max 200.")]
        int? pageSize = null)
    {
        var principal = user as WorkflowClaimsPrincipal;
        var effectivePageSize = Math.Clamp(pageSize ?? ListWorkflowsTool.DefaultPageSize, 1, ListWorkflowsTool.MaxPageSize);
        var skip = ListWorkflowsTool.DecodeCursor(cursor);

        var matches = EnumerateSources(hostConfig.WorkflowsDirectory, folder)
            .Where(s => ListWorkflowsTool.HasViewPermission(authPolicyLoader, principal, s.Path))
            .OrderBy(s => s.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var page = matches.Skip(skip).Take(effectivePageSize).ToList();
        var nextCursor = skip + page.Count < matches.Count
            ? ListWorkflowsTool.EncodeCursor(skip + page.Count)
            : null;

        return new ListSourcesResult(page, nextCursor);
    }

    static IEnumerable<SourceSummary> EnumerateSources(string workflowsDirectory, string? folder)
    {
        if (string.IsNullOrWhiteSpace(workflowsDirectory) || !Directory.Exists(workflowsDirectory))
        {
            yield break;
        }

        var searchDir = string.IsNullOrWhiteSpace(folder)
            ? workflowsDirectory
            : Path.Combine(workflowsDirectory, folder.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(searchDir))
        {
            yield break;
        }

        // Same .bite-then-legacy-.xml order and de-duplication as ListWorkflowsTool.EnumerateWorkflows.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pattern in new[] { "*.bite", "*.xml" })
        {
            foreach (var file in Directory.EnumerateFiles(searchDir, pattern, SearchOption.AllDirectories))
            {
                var header = TryReadSourceHeader(file);
                if (header is null)
                {
                    continue;
                }

                var relative = Path.GetRelativePath(workflowsDirectory, file);
                var withoutExt = relative[..^Path.GetExtension(relative).Length].Replace(Path.DirectorySeparatorChar, '/');
                if (!seen.Add(withoutExt))
                {
                    continue;
                }

                var (name, resourceType, type, id) = header.Value;
                yield return new SourceSummary(name ?? Path.GetFileName(withoutExt), withoutExt, resourceType, type, id);
            }
        }
    }

    static readonly XmlReaderSettings HeaderReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore,
        XmlResolver = null,
        IgnoreWhitespace = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
    };

    /// <summary>
    /// Reads only the root element: a source is a <c>&lt;Source&gt;</c> root (what both the Studio
    /// and <see cref="AddSourceTool.BuildSourceXml"/> write). <c>internal</c> so
    /// <see cref="AddSourceTool"/> can report an existing source's ID in its "already exists" error.
    /// </summary>
    internal static (string? Name, string? ResourceType, string? Type, string? Id)? TryReadSourceHeader(string filePath)
    {
        try
        {
            using var reader = XmlReader.Create(filePath, HeaderReaderSettings);
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (!string.Equals(reader.LocalName, "Source", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var id = Guid.TryParse(reader.GetAttribute("ID"), out var resourceId) ? resourceId.ToString() : null;
                return (reader.GetAttribute("Name"), reader.GetAttribute("ResourceType"), reader.GetAttribute("Type"), id);
            }
        }
        catch
        {
            // Skip unreadable/malformed files rather than failing the whole listing.
        }

        return null;
    }
}

/// <summary>One source in a <c>list_sources</c> response. Never carries the connection string.</summary>
internal sealed record SourceSummary(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    // As stored on disk, e.g. "WebSource", "SqlDatabase", "RedisSource".
    [property: JsonPropertyName("resourceType")] string? ResourceType,
    // The source's broader Type attribute, e.g. "DbSource" for every database engine.
    [property: JsonPropertyName("type"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Type,
    // The ResourceID a Web/Database step's sourceId refers to.
    [property: JsonPropertyName("id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Id);

/// <summary>The full <c>list_sources</c> response payload.</summary>
internal sealed record ListSourcesResult(
    [property: JsonPropertyName("sources")] IReadOnlyList<SourceSummary> Sources,
    [property: JsonPropertyName("nextCursor")] string? NextCursor);
