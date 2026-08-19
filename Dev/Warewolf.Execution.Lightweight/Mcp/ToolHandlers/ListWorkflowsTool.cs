/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>list_workflows</c> MCP tool (<c>warewolf-lee-mcp-v3-spec.md</c>,
/// "Tools" § <c>list_workflows</c>): returns workflows stored on this instance, filtered
/// to what the caller is authorized to <b>view</b>.
///
/// <para>
/// <b>Enumeration.</b> Mirrors <see cref="ApisJsonGenerator"/>'s forward-only
/// <see cref="XmlReader"/> header scan (name + <c>ResourceType</c> only — the XAML body
/// is never parsed during enumeration) so permission filtering and pagination over a
/// large corpus stay cheap. The heavier <c>DataList</c>/<c>Comment</c> parse
/// (<see cref="ReadDetail"/>) is paid only once per item that survives filtering and
/// pagination — never for the full corpus.
/// </para>
///
/// <para>
/// <b>Permission filtering.</b> Uses the same <see cref="IWorkflowAuthPolicyLoader"/>
/// already gating <c>/mcp-api/*</c> at the HTTP layer (<see cref="Functions.McpApiFunctions"/>),
/// rather than <see cref="Security.PermissionChecker"/> (the <c>/Public/*</c> helper) or
/// <see cref="IWorkflowPolicyMatcher"/> (the <c>/Secure/*</c> per-route gate) — both of
/// those encode a stricter "config missing → 503 unless BYPASS_SECURE_CONFIG" policy that
/// the <c>/mcp-api/*</c> baseline gate deliberately does not apply (see
/// <c>McpApiFunctions.TryAuthenticate</c>). Reusing
/// <see cref="IWorkflowAuthPolicyLoader.IsConfigEffective"/> and
/// <see cref="IWorkflowAuthPolicyLoader.GetEffectivePermissions"/> directly keeps this
/// tool's authorization semantics consistent with that same gate.
/// </para>
///
/// <para>
/// <b><c>bodyEditable</c> (fidelity gate, v3).</b> Per spec, "bodyEditable" now means
/// "this workflow's XAML round-trips losslessly through <c>WorkflowToX6Converter</c> →
/// <c>X6ToWorkflowConverter</c>" — a per-activity-type allow-list built from executing
/// both the original and round-tripped XAML and diffing outputs. That allow-list
/// consumption (mapping each workflow's actual activity types to the fidelity-test
/// results and flipping <c>bodyEditable</c> only when every type used passes) is a
/// separate, not-yet-implemented increment (no <c>StudioName</c> ↔ XAML-activity-type
/// mapping exists yet, and <c>create_workflow</c> — the only source of unconditionally
/// editable workflows — is not implemented either). Per the spec's own default
/// ("a workflow using even one non-allow-listed or failing-fidelity activity type stays
/// <c>bodyEditable: false</c>"), every workflow in this increment is conservatively
/// reported as <c>bodyEditable: false</c> until that consumption logic lands — this is
/// the spec-compliant default for "unproven", not a shortcut.
/// </para>
/// </summary>
internal static class ListWorkflowsTool
{
    internal const string ToolName = "list_workflows";
    internal const int DefaultPageSize = 50;
    internal const int MaxPageSize = 200;

    /// <summary>
    /// Handles a <c>list_workflows</c> tool call. Parameter types recognised by
    /// <c>ModelContextProtocol.Core</c>'s parameter binder (<see cref="ClaimsPrincipal"/>,
    /// <see cref="HostEnvironmentConfig"/>, <see cref="IWorkflowAuthPolicyLoader"/>) are
    /// excluded from the generated JSON input schema and bound automatically per-request;
    /// the remaining parameters are the tool's actual (schema-visible) input.
    /// </summary>
    internal static ListWorkflowsResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user = null,
        [Description("Restrict results to workflows under this folder (relative path, forward slashes). Omit to list from the root.")]
        string? folder = null,
        [Description("Opaque pagination token from a prior response's nextCursor. Omit to start from the first page.")]
        string? cursor = null,
        [Description("Maximum number of results to return. Default 50, max 200.")]
        int? pageSize = null)
    {
        var principal = user as WorkflowClaimsPrincipal;
        var effectivePageSize = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        var skip = DecodeCursor(cursor);

        var matches = EnumerateWorkflows(hostConfig.WorkflowsDirectory, folder)
            .Where(w => HasViewPermission(authPolicyLoader, principal, w.RelativePath))
            .OrderBy(w => w.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var page = matches.Skip(skip).Take(effectivePageSize).ToList();

        var workflows = page
            .Select(w => ReadDetail(w))
            .ToList();

        var nextCursor = skip + page.Count < matches.Count
            ? EncodeCursor(skip + page.Count)
            : null;

        return new ListWorkflowsResult(workflows, nextCursor);
    }

    // ── Permission filtering ──────────────────────────────────────────────────

    /// <summary>
    /// <c>internal</c> (not <c>private</c>) so <see cref="GetWorkflowDefinitionTool"/> can
    /// apply the exact same View-permission rule to a single named workflow, rather than
    /// duplicating this logic. Thin wrapper over <see cref="HasPermission"/> fixed to
    /// <see cref="WorkflowPermission.View"/>.
    /// </summary>
    internal static bool HasViewPermission(
        IWorkflowAuthPolicyLoader authPolicyLoader,
        WorkflowClaimsPrincipal? principal,
        string workflowRelativePath) =>
        HasPermission(authPolicyLoader, principal, workflowRelativePath, WorkflowPermission.View);

    /// <summary>
    /// Returns whether <paramref name="principal"/> holds every flag in
    /// <paramref name="requiredPermission"/> for <paramref name="workflowRelativePath"/>, using
    /// the same open-access rule as <c>Functions.McpApiFunctions.TryAuthenticate</c>: when
    /// <c>secure.config</c> is not effective, every caller is allowed through. <c>internal</c>
    /// so write tools (e.g. <see cref="CreateWorkflowTool"/>, requiring
    /// <see cref="WorkflowPermission.Contribute"/>) share the exact same resource-if-present-
    /// else-global, Public-OR'd resolution as the View check, rather than duplicating it.
    /// </summary>
    internal static bool HasPermission(
        IWorkflowAuthPolicyLoader authPolicyLoader,
        WorkflowClaimsPrincipal? principal,
        string workflowRelativePath,
        WorkflowPermission requiredPermission)
    {
        if (!authPolicyLoader.IsConfigEffective)
        {
            return true;
        }

        var groups = principal?.Groups ?? Array.Empty<string>();
        var callerRoles = groups
            .Append(principal?.UserName ?? string.Empty)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var effectivePermissions = authPolicyLoader.GetEffectivePermissions(workflowRelativePath, callerRoles);
        return effectivePermissions.HasFlag(requiredPermission);
    }

    // ── File scanning (cheap: header-only, mirrors ApisJsonGenerator) ────────

    internal readonly record struct WorkflowFile(string Name, string RelativePath, string FullPath);

    static IEnumerable<WorkflowFile> EnumerateWorkflows(string workflowsDirectory, string? folder)
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

        // Primary format (.bite) first, legacy XML fallback second — a HashSet keyed on
        // the XML Name attribute avoids emitting the same workflow twice when both exist.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pattern in new[] { "*.bite", "*.xml" })
        {
            foreach (var file in Directory.EnumerateFiles(searchDir, pattern, SearchOption.AllDirectories))
            {
                var (name, isWorkflow) = TryReadWorkflowHeader(file);
                if (!isWorkflow || name is null || !seen.Add(name))
                {
                    continue;
                }

                var relative = Path.GetRelativePath(workflowsDirectory, file);
                var ext = Path.GetExtension(relative);
                var withoutExt = relative[..^ext.Length].Replace(Path.DirectorySeparatorChar, '/');

                yield return new WorkflowFile(name, withoutExt, file);
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
    /// Opens <paramref name="filePath"/> and reads only the root element's attributes —
    /// the XAML body is never touched during enumeration. <c>internal</c> so
    /// <see cref="GetWorkflowDefinitionTool"/> can validate a single resolved file the
    /// same way enumeration does (and consistently treat "not a WorkflowService" as
    /// not-found) without re-implementing the header scan.
    /// </summary>
    internal static (string? name, bool isWorkflow) TryReadWorkflowHeader(string filePath)
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

                var resourceType = reader.GetAttribute("ResourceType");
                if (!string.Equals(resourceType, "WorkflowService", StringComparison.OrdinalIgnoreCase))
                {
                    return (null, false);
                }

                return (reader.GetAttribute("Name"), true);
            }
        }
        catch
        {
            // Skip unreadable/malformed files rather than failing the whole listing.
        }

        return (null, false);
    }

    // ── Per-result detail (DataList + description) ────────────────────────────

    /// <summary>
    /// Parses the full workflow file once to extract the <c>Comment</c> element
    /// (used as <c>description</c>) and the <c>DataList</c>'s input/output variable
    /// names. Only called for items in the returned page, never the full corpus.
    /// <c>internal</c> so <see cref="GetWorkflowDefinitionTool"/> can reuse the same
    /// description/inputs/outputs extraction for a single workflow rather than
    /// duplicating the <c>DataListTO</c>/<c>Comment</c> parse.
    /// </summary>
    internal static WorkflowSummary ReadDetail(WorkflowFile file)
    {
        string description = string.Empty;
        var inputs = new List<string>();
        var outputs = new List<string>();

        try
        {
            var root = XElement.Load(file.FullPath);
            description = root.Element("Comment")?.Value.Trim() ?? string.Empty;

            var dataListXml = root.Element("DataList")?.ToString() ?? "<DataList />";
            var dataListTo = new DataListTO(dataListXml);
            inputs = dataListTo.Inputs;
            outputs = dataListTo.Outputs;
        }
        catch
        {
            // Malformed body — still list the workflow (enumeration already proved it's
            // a valid WorkflowService by its root attributes), just without detail.
        }

        // bodyEditable: see the fidelity-gate remarks on this class — conservatively
        // false until the allow-list consumption logic (StudioName ↔ activity-type
        // mapping + per-workflow scan) is implemented in a follow-up increment.
        return new WorkflowSummary(file.Name, file.RelativePath, description, inputs, outputs, false);
    }

    // ── Pagination cursor (opaque offset token) ───────────────────────────────

    static int DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return 0;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            const string prefix = "offset:";
            if (decoded.StartsWith(prefix, StringComparison.Ordinal) &&
                int.TryParse(decoded.AsSpan(prefix.Length), out var offset) &&
                offset >= 0)
            {
                return offset;
            }
        }
        catch
        {
            // Malformed/foreign cursor — restart from the first page rather than failing.
        }

        return 0;
    }

    static string EncodeCursor(int offset) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"offset:{offset}"));
}

/// <summary>One workflow entry in a <c>list_workflows</c> response.</summary>
internal sealed record WorkflowSummary(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("inputs")] IReadOnlyList<string> Inputs,
    [property: JsonPropertyName("outputs")] IReadOnlyList<string> Outputs,
    [property: JsonPropertyName("bodyEditable")] bool BodyEditable);

/// <summary>The full <c>list_workflows</c> response payload.</summary>
internal sealed record ListWorkflowsResult(
    [property: JsonPropertyName("workflows")] IReadOnlyList<WorkflowSummary> Workflows,
    [property: JsonPropertyName("nextCursor")] string? NextCursor);
