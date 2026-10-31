/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>delete_ui</c> MCP tool: removes a workflow's form, so the web Studio falls
/// back to the plain form it builds from the envelope. Needs Contribute, like <c>create_ui</c>.
/// </summary>
internal static class DeleteUiTool
{
    internal const string ToolName = "delete_ui";

    internal static DeleteUiResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("Workflow name. Required unless `id` is given.")]
        string name,
        [Description("Workflow ResourceID (GUID). Used instead of `name` when given.")]
        Guid? id)
    {
        var resourceId = id is { } value && value != Guid.Empty ? value : (Guid?)null;
        if (resourceId is null && string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` or `id` is required.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var (_, relativePath, headerName, label) =
            GetWorkflowDefinitionTool.ResolveByNameOrId(workflowsDirectory, name, resourceId);

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to delete the form of workflow '{label}'.");
        }

        if (!WorkflowUiCatalog.Delete(workflowsDirectory, relativePath))
        {
            throw new McpException($"Workflow '{label}' has no form; nothing was deleted.");
        }

        return new DeleteUiResult(headerName, relativePath, true);
    }
}

/// <summary>The <c>delete_ui</c> response payload.</summary>
internal sealed record DeleteUiResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("deleted")] bool Deleted);
