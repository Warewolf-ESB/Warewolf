/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json.Serialization;
using ModelContextProtocol;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_workflow_url</c> MCP tool: returns the HTTP URL a workflow is invocable
/// at over the <b>JWT-secured</b> <c>/Secure/*</c> route only.
///
/// <para>
/// <b>Never <c>/Public/</c>.</b> Unlike <see cref="CreateWorkflowTool"/>/<see cref="EditWorkflowTool"/>,
/// which report both <c>publicUrl</c> and <c>secureUrl</c> because a caller who just authored a
/// workflow may have deliberately made it anonymously reachable, this tool exists for callers who
/// want the authenticated invocation path specifically and should never be handed an anonymous one
/// by mistake. It therefore reads only <see cref="WorkflowHttpEndpoints.SecureUrl"/> off the same
/// <see cref="WorkflowHttpEndpointResolver"/> those tools already use — <see cref="WorkflowHttpEndpoints.PublicUrl"/>
/// is not part of this tool's response shape at all, so there is no field a future edit could
/// accidentally start populating.
/// </para>
///
/// <para>
/// <b>Resolution/permission are shared with <see cref="ListWorkflowsTool"/></b> (via
/// <see cref="GetWorkflowDefinitionTool"/>'s own pattern): <see cref="WorkflowNameResolver.Resolve"/>
/// for not-found, <see cref="ListWorkflowsTool.HasViewPermission"/> for the View gate — the same
/// rule <c>get_workflow_definition</c> applies, since knowing a workflow's invocation URL is no more
/// sensitive than seeing its definition.
/// </para>
///
/// <para>
/// <b>When <c>secure.config</c> is not effective.</b> <see cref="WorkflowHttpEndpointResolver.Resolve"/>
/// returns a <c>null</c> <c>SecureUrl</c> in that case, because <c>/Secure/*</c> always 401s with no
/// JWT secret to validate against regardless of <c>BYPASS_SECURE_CONFIG</c> (see that type's remarks).
/// Rather than fabricate a URL that cannot actually be called, this tool throws <see cref="McpException"/>
/// explaining why.
/// </para>
/// </summary>
internal static class GetWorkflowUrlTool
{
    internal const string ToolName = "get_workflow_url";

    internal static GetWorkflowUrlResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user = null,
        [Description("Workflow name — must match a `path` from a prior list_workflows response.")]
        string name = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        var filePath = WorkflowNameResolver.Resolve(workflowsDirectory, relativePath);
        var (headerName, isWorkflow) = filePath is null
            ? (null, false)
            : ListWorkflowsTool.TryReadWorkflowHeader(filePath);

        if (filePath is null || !isWorkflow || headerName is null)
        {
            throw new McpException($"Workflow '{name}' was not found.");
        }

        if (!ListWorkflowsTool.HasViewPermission(authPolicyLoader, principal, relativePath))
        {
            throw new McpException($"You do not have permission to view workflow '{name}'.");
        }

        var secureUrl = WorkflowHttpEndpointResolver.Resolve(authPolicyLoader, relativePath).SecureUrl;
        if (secureUrl is null)
        {
            throw new McpException(
                $"Workflow '{name}' has no reachable /Secure/ URL: secure.config is not effective on this " +
                "engine instance, so /Secure/* always returns 401 regardless of caller. Deploy a valid " +
                "secure.config to enable it.");
        }

        return new GetWorkflowUrlResult(headerName, secureUrl);
    }
}

/// <summary>The full <c>get_workflow_url</c> response payload.</summary>
internal sealed record GetWorkflowUrlResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url);
