/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Activities.WF;
using ModelContextProtocol;
using System;
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
/// Implements the <c>edit_workflow</c> MCP tool (<c>warewolf-lee-mcp-v3-spec.md</c>, "Tools" §
/// <c>edit_workflow</c>): re-compiles an <c>envelope</c> + <c>body</c> pair and overwrites an
/// <b>existing</b> workflow's <c>.bite</c> file in place — the save-side mirror of
/// <see cref="CreateWorkflowTool"/> for a workflow that already exists.
///
/// <para>
/// <b>Steps, in spec order (mirrors <see cref="CreateWorkflowTool"/> except where noted):</b>
/// <list type="number">
/// <item>Runs the same checks as <see cref="ValidateWorkflowTool"/> first (reused directly, not
/// duplicated) — rejects without writing on any hard-error finding.</item>
/// <item><c>name</c> MUST already resolve to an existing workflow
/// (<see cref="WorkflowNameResolver.Resolve"/>) — the inverse of <c>create_workflow</c>'s "must
/// not already exist" rule.</item>
/// <item>Requires <b>Contribute</b> permission, resource-if-present else global, Public OR'd —
/// the exact same rule <see cref="CreateWorkflowTool"/> applies
/// (<see cref="ListWorkflowsTool.HasPermission"/>).</item>
/// <item>Compiles <c>body</c> to XAML via <see cref="X6ToWorkflowConverter.X6JsonToWorkflow"/> +
/// <see cref="X6ToWorkflowConverter.AddReplaceNameSpace"/>, then wraps it in a
/// <c>&lt;Service&gt;</c> envelope via <see cref="EnvelopeBiteWriter"/> — identical pipeline to
/// <c>create_workflow</c>.</item>
/// <item><b>Preserves the existing file's <c>Service ID</c></b> (its current root <c>ID</c>
/// attribute) instead of minting a new one — this is a save of the <i>same</i> resource, not a
/// new one, so its identity must not change across an edit. Falls back to a fresh GUID only if
/// the existing file's <c>ID</c> is missing/blank (defensive; every workflow this server itself
/// wrote always has one).</item>
/// <item>Writes <c>VersionInfo</c> with <c>Reason="Save"</c>/current UTC timestamp/
/// <c>VersionNumber</c> incremented by one from whatever the existing file currently has
/// (treated as <c>0</c> — so the write becomes <c>"1"</c> — if the existing file has no
/// parseable <c>VersionNumber</c>).</item>
/// <item>Overwrites the file at its existing on-disk path in place — <c>edit_workflow</c> does
/// not move or rename a workflow; <c>name</c> only identifies which existing workflow to update.</item>
/// <item>Refreshes the relevant <see cref="WorkflowIndex"/> cache entry via
/// <see cref="WorkflowIndex.AddOrUpdate"/>, matching <c>create_workflow</c>'s own
/// cache-freshness guarantee (the name/path is unchanged here, but this keeps both tools
/// consistent and the refresh is cheap).</item>
/// <item>Resolves the workflow's HTTP invocation path(s) via
/// <see cref="WorkflowHttpEndpointResolver"/> and includes them in the response — same rationale
/// as <see cref="CreateWorkflowTool"/>.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Validation failure reporting.</b> Same as <c>create_workflow</c>: the spec's
/// <c>edit_workflow</c> success shape is <c>{ name, updated }</c> (plus the additive
/// <c>httpEndpoints</c> field below) — there is no room for a
/// structured error array like <c>validate_workflow</c>'s. A validation failure is therefore
/// reported as an <see cref="McpException"/> whose message concatenates every <c>error</c>-
/// severity finding (callers that need the full structured detail should call
/// <c>validate_workflow</c> directly first, which this tool re-runs internally regardless).
/// </para>
/// </summary>
internal static class EditWorkflowTool
{
    internal const string ToolName = "edit_workflow";

    internal static EditWorkflowResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The existing workflow's name — a relative path (forward slashes), no extension. Must already exist; use create_workflow to create a new workflow.")]
        string name,
        [Description("The workflow envelope — { name?, description?, inputs: EnvelopeVariable[], outputs: EnvelopeVariable[] } per get_workflow_schema's envelope_schema.")]
        System.Text.Json.JsonElement envelope,
        [Description("The workflow body — the X6 graph { resourcename, cells[] } per get_workflow_schema's body_schema.")]
        System.Text.Json.JsonElement body)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        var validation = ValidateWorkflowTool.Handle(envelope, body);
        if (!validation.Valid)
        {
            var messages = string.Join("; ", validation.Errors.Select(e => e.Message));
            throw new McpException($"`envelope`/`body` failed validation: {messages}");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        var existingFilePath = WorkflowNameResolver.Resolve(workflowsDirectory, relativePath);
        if (existingFilePath is null)
        {
            throw new McpException($"Workflow '{name}' was not found; use create_workflow to create a new workflow.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to edit workflow '{name}'.");
        }

        var (existingServiceId, existingVersionNumber) = ReadExistingServiceMetadata(existingFilePath);

        string xamlDefinition;
        try
        {
            var xaml = new X6ToWorkflowConverter().X6JsonToWorkflow(body.GetRawText());
            xaml = X6ToWorkflowConverter.AddReplaceNameSpace(xaml);
            xamlDefinition = xaml.ToString();
        }
        catch (Exception ex)
        {
            // validate_workflow's own final compile check should have already caught any
            // converter-level failure — this is a defensive net, not the primary error path.
            throw new McpException($"the workflow body could not be compiled: {ex.Message}");
        }

        var serviceId = string.IsNullOrWhiteSpace(existingServiceId) ? Guid.NewGuid().ToString() : existingServiceId;
        var displayName = CreateWorkflowTool.ResolveDisplayName(envelope, relativePath);
        var description = CreateWorkflowTool.ResolveDescription(envelope);
        var callerIdentity = principal?.CallerIdentity is { Length: > 0 } identity ? identity : "Anonymous";

        string biteContents;
        try
        {
            biteContents = EnvelopeBiteWriter.BuildBiteFileContents(
                serviceId,
                displayName,
                description,
                envelope,
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
            throw new McpException($"the workflow envelope could not be composed into a .bite file: {ex.Message}");
        }

        File.WriteAllText(existingFilePath, biteContents);
        // Drop any pooled compilation of this workflow so the next execution picks the new
        // definition up. WorkflowExecutor's pool key already includes the file's timestamp+length,
        // so this is belt-and-braces for the one case that cannot see: a rewrite of identical
        // length landing within the filesystem's timestamp granularity - which is exactly what an
        // agent making rapid successive edits produces.
        WorkflowExecutor.EvictWorkflow(existingFilePath);

        // The name/path is unchanged by an edit, but refreshing the cache entry keeps this tool
        // consistent with create_workflow's own guarantee and costs nothing extra.
        WorkflowIndex.Instance.AddOrUpdate(workflowsDirectory, relativePath, relativePath + ".bite");

        var httpEndpoints = WorkflowHttpEndpointResolver.Resolve(authPolicyLoader, relativePath);
        return new EditWorkflowResult(name, true, httpEndpoints);
    }

    /// <summary>
    /// Reads the existing file's root <c>ID</c> attribute (the Service ID to preserve across the
    /// save) and its current <c>VersionInfo/@VersionNumber</c> (to increment from). Tolerant of a
    /// missing/malformed file — returns <c>(null, 0)</c> rather than failing the whole edit,
    /// since both the serviceId and versionNumber callers already have safe fallbacks for a
    /// missing value.
    /// </summary>
    static (string? serviceId, int versionNumber) ReadExistingServiceMetadata(string filePath)
    {
        try
        {
            var root = XElement.Load(filePath);
            var serviceId = root.Attribute("ID")?.Value;
            var versionNumberText = root.Element("VersionInfo")?.Attribute("VersionNumber")?.Value;
            var versionNumber = int.TryParse(versionNumberText, out var parsed) ? parsed : 0;
            return (serviceId, versionNumber);
        }
        catch
        {
            // Malformed/unreadable existing file — proceed with fresh defaults rather than
            // blocking the edit entirely.
            return (null, 0);
        }
    }
}

/// <summary>The full <c>edit_workflow</c> response payload.</summary>
internal sealed record EditWorkflowResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("updated")] bool Updated,
    [property: JsonPropertyName("httpEndpoints")] WorkflowHttpEndpoints HttpEndpoints);
