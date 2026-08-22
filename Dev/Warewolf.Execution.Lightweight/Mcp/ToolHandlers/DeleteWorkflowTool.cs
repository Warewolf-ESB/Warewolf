/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

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
/// Implements the <c>delete_workflow</c> MCP tool: removes a workflow's <c>.bite</c> file from
/// disk and drops every cached trace of it, so the name is immediately free for reuse.
///
/// <para>
/// <b>Why this exists.</b> Every other authoring tool in this namespace could create resources
/// (<see cref="CreateWorkflowTool"/>, <see cref="DeployWorkflowTool"/>) but none could remove
/// one, so an agent exercising the toolset accumulated scratch workflows permanently. This is
/// the missing inverse of <c>create_workflow</c>.
/// </para>
///
/// <para>
/// <b>Steps, in order:</b>
/// <list type="number">
/// <item><c>name</c> is required.</item>
/// <item>Requires <b>Contribute</b> permission, resource-if-present else global, Public OR'd —
/// the same rule <see cref="CreateWorkflowTool"/>/<see cref="EditWorkflowTool"/>/
/// <see cref="DeployWorkflowTool"/> already apply (<see cref="ListWorkflowsTool.HasPermission"/>).
/// Deleting is deliberately gated at the same level as <c>deploy_workflow</c>'s
/// <c>overwrite: true</c>, which already destroys an existing workflow's contents — one level of
/// permission for one outcome.</item>
/// <item><b>Permission is checked before existence</b>, unlike <see cref="DeployWorkflowTool"/>,
/// which resolves the file first. A caller who may not touch <c>name</c> gets the identical
/// error whether or not a workflow is there, so this tool cannot be used as an existence oracle
/// for workflows the caller cannot see.</item>
/// <item><c>name</c> must resolve to an existing workflow
/// (<see cref="WorkflowNameResolver.Resolve"/>); a miss is reported rather than treated as a
/// no-op success, so a caller that misspells a name learns about it instead of believing a
/// delete happened.</item>
/// <item>Deletes the resolved <c>.bite</c> file, then clears the two caches that would otherwise
/// keep serving it: <see cref="WorkflowExecutor.EvictWorkflow"/> drops any pooled compilation,
/// and <see cref="WorkflowIndex.Remove"/> drops the in-memory name → path entry. Without the
/// latter the deleted name keeps resolving until the process restarts.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Not gated on <c>bodyEditable</c>.</b> Unlike <see cref="EditWorkflowTool"/>/
/// <see cref="AddStepTool"/>, this tool never parses the workflow body, so a workflow whose XAML
/// does not round-trip losslessly through <see cref="FidelityAllowList"/> is still deletable —
/// the same reasoning that lets <see cref="ExecuteWorkflowTool"/> run one.
/// </para>
///
/// <para>
/// Empty parent folders are intentionally left behind: they may be shared with sibling workflows
/// or sources, and pruning them would make one tool's blast radius wider than the resource it
/// was pointed at.
/// </para>
/// </summary>
internal static class DeleteWorkflowTool
{
    internal const string ToolName = "delete_workflow";

    internal static DeleteWorkflowResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The workflow's name — a relative path (forward slashes), no extension. Must already exist.")]
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        // Permission first, existence second — see the class remarks: an unauthorized caller must
        // not be able to distinguish "exists" from "does not exist" by the error they get back.
        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to delete workflow '{name}'.");
        }

        var existingFilePath = WorkflowNameResolver.Resolve(workflowsDirectory, relativePath);
        if (existingFilePath is null)
        {
            throw new McpException($"Workflow '{name}' was not found; nothing was deleted.");
        }

        try
        {
            File.Delete(existingFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new McpException($"Workflow '{name}' could not be deleted: {ex.Message}");
        }

        // Order matters on the way out too: drop the pooled compilation before the index entry, so
        // no window exists in which the name still resolves but a stale compilation is what runs.
        WorkflowExecutor.EvictWorkflow(existingFilePath);
        WorkflowIndex.Instance.Remove(workflowsDirectory, relativePath);

        return new DeleteWorkflowResult(name, true);
    }
}

/// <summary>The full <c>delete_workflow</c> response payload.</summary>
internal sealed record DeleteWorkflowResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("deleted")] bool Deleted);
