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
using System.Security.Claims;
using System.Text.Json.Serialization;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>create_workflow</c> MCP tool (<c>warewolf-lee-mcp-v3-spec.md</c>, "Tools" §
/// <c>create_workflow</c>): compiles an <c>envelope</c> + <c>body</c> pair to a <c>.bite</c> file
/// and persists it as a new workflow.
///
/// <para>
/// <b>Steps, in spec order:</b>
/// <list type="number">
/// <item>Runs the same checks as <see cref="ValidateWorkflowTool"/> first (reused directly, not
/// duplicated) — rejects without writing on any hard-error finding.</item>
/// <item><c>name</c> must not already resolve to an existing workflow
/// (<see cref="WorkflowNameResolver.Resolve"/> — the same lookup <c>get_workflow_definition</c>
/// uses, so "already exists" means exactly what every other tool means by that name).</item>
/// <item>Requires <b>Contribute</b> permission, resource-if-present else global, Public OR'd —
/// the same rule <see cref="ListWorkflowsTool.HasPermission"/> already applies for View, generalised
/// to accept the required flag.</item>
/// <item>Compiles <c>body</c> to XAML via <see cref="X6ToWorkflowConverter.X6JsonToWorkflow"/>,
/// fixes up namespaces via <see cref="X6ToWorkflowConverter.AddReplaceNameSpace"/> (mirroring
/// <see cref="X6ToWorkflowConverter.X6JsonToXaml"/>'s own two-step pipeline), then wraps it in a
/// <c>&lt;Service&gt;</c> envelope via <see cref="EnvelopeBiteWriter"/> — new Lightweight-local
/// code (mirrors <c>Hello World.bite</c>'s structure), not <c>Dev2.Runtime.Services</c>'s
/// <c>Workflow.ToServiceDefinition()</c>, per spec.</item>
/// <item>Generates a fresh <c>Service ID</c> GUID, writes <c>VersionInfo</c> with
/// <c>Reason="Save"</c>/current UTC timestamp/<c>VersionNumber="1"</c> (a first save has no prior
/// version to increment from).</item>
/// <item>Invalidates the relevant <see cref="WorkflowIndex"/> cache entry via
/// <see cref="WorkflowIndex.AddOrUpdate"/> so the new workflow is immediately resolvable by name
/// without waiting for a process restart.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Validation failure reporting.</b> The spec's <c>create_workflow</c> output shape is only
/// <c>{ name, created }</c> — there is no room for a structured error array like
/// <c>validate_workflow</c>'s. A validation failure is therefore reported as an
/// <see cref="McpException"/> whose message concatenates every <c>error</c>-severity finding
/// (callers that need the full structured detail should call <c>validate_workflow</c> directly
/// first, which this tool re-runs internally regardless).
/// </para>
/// </summary>
internal static class CreateWorkflowTool
{
    internal const string ToolName = "create_workflow";

    internal static CreateWorkflowResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The new workflow's name — a relative path (forward slashes), no extension. Must not already exist.")]
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
            var messages = string.Join("; ", validation.Errors);
            throw new McpException($"`envelope`/`body` failed validation: {messages}");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        if (WorkflowNameResolver.Resolve(workflowsDirectory, relativePath) is not null)
        {
            throw new McpException($"A workflow named '{name}' already exists.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to create workflow '{name}'.");
        }

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

        var serviceId = Guid.NewGuid().ToString();
        var displayName = ResolveDisplayName(envelope, relativePath);
        var description = ResolveDescription(envelope);
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
                versionNumber: 1,
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

        var fullPath = Path.Combine(workflowsDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar) + ".bite");
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, biteContents);

        WorkflowIndex.Instance.AddOrUpdate(workflowsDirectory, relativePath, relativePath + ".bite");

        return new CreateWorkflowResult(name, true);
    }

    /// <summary><c>internal</c> (not <c>private</c>) so <see cref="EditWorkflowTool"/> resolves the
    /// same "envelope name wins over the file-path name" rule identically, rather than duplicating it.</summary>
    internal static string ResolveDisplayName(System.Text.Json.JsonElement envelope, string relativePath)
    {
        if (envelope.ValueKind == System.Text.Json.JsonValueKind.Object &&
            envelope.TryGetProperty("name", out var nameEl) &&
            nameEl.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var envelopeName = nameEl.GetString();
            if (!string.IsNullOrWhiteSpace(envelopeName))
            {
                return envelopeName;
            }
        }

        return Path.GetFileName(relativePath.TrimEnd('/'));
    }

    /// <summary><c>internal</c> (not <c>private</c>) so <see cref="EditWorkflowTool"/> shares the
    /// same description-resolution rule identically, rather than duplicating it.</summary>
    internal static string ResolveDescription(System.Text.Json.JsonElement envelope)
    {
        if (envelope.ValueKind == System.Text.Json.JsonValueKind.Object &&
            envelope.TryGetProperty("description", out var descriptionEl) &&
            descriptionEl.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            return descriptionEl.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}

/// <summary>The full <c>create_workflow</c> response payload.</summary>
internal sealed record CreateWorkflowResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created")] bool Created);
