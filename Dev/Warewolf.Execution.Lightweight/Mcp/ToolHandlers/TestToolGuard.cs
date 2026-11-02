/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System.Security.Claims;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// The opening checks <see cref="ListTestsTool"/>, <see cref="GetTestTool"/> and
/// <see cref="DeleteTestTool"/> share: <c>name</c> is required, the caller needs
/// <paramref name="required"/> on it, and only then must it be an existing workflow — permission
/// before existence, as <see cref="DeleteWorkflowTool"/> does, so an unauthorized caller can't tell
/// "no such workflow" from "not yours".
/// </summary>
internal static class TestToolGuard
{
    /// <summary>Returns the workflow's relative path, or throws.</summary>
    /// <param name="action">How the permission error names what was refused, e.g. "list tests of".</param>
    internal static string ResolveWorkflow(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        string name,
        WorkflowPermission required,
        string action)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        var relativePath = name.Replace('\\', '/').TrimStart('/');

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, user as WorkflowClaimsPrincipal, relativePath, required))
        {
            throw new McpException($"You do not have permission to {action} workflow '{name}'.");
        }

        var (filePath, _) = CreateTestTool.ResolveWorkflow(hostConfig.WorkflowsDirectory, relativePath);
        if (filePath is null)
        {
            throw new McpException($"Workflow '{name}' was not found.");
        }

        return relativePath;
    }
}
