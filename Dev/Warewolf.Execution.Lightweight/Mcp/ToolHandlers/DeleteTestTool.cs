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
/// Implements the <c>delete_test</c> MCP tool (Phase 2 of docs/WorkflowTestFramework-Plan.md): removes
/// one persisted test, and the workflow's <c>.tests</c> directory with it when that was the last one
/// (<see cref="TestCatalog.Delete"/>). With <c>create_test</c> it is also how a test is renamed, since
/// <c>edit_test</c> does not rename.
///
/// <para>
/// Needs <b>Contribute</b>, the permission <c>create_test</c>/<c>edit_test</c> need, checked before
/// existence (see <see cref="TestToolGuard"/>). A missing test is reported, not treated as a no-op
/// success, so a misspelled name isn't mistaken for a delete — <see cref="DeleteWorkflowTool"/>'s rule.
/// </para>
/// </summary>
internal static class DeleteTestTool
{
    internal const string ToolName = "delete_test";

    internal static DeleteTestResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The owning workflow's name — same lookup create_test/execute_test use.")]
        string name,
        [Description("The test to delete, as passed to create_test/edit_test.")]
        string testName)
    {
        var relativePath = TestToolGuard.ResolveWorkflow(hostConfig, authPolicyLoader, user, name, WorkflowPermission.Contribute, "delete tests of");

        if (string.IsNullOrWhiteSpace(testName))
        {
            throw new McpException("`testName` is required.");
        }

        TestCatalog.ValidateTestName(testName);

        bool deleted;
        try
        {
            deleted = TestCatalog.Delete(hostConfig.WorkflowsDirectory, relativePath, testName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new McpException($"Test '{testName}' could not be deleted: {ex.Message}");
        }

        if (!deleted)
        {
            throw new McpException($"Test '{testName}' was not found for workflow '{name}'; nothing was deleted.");
        }

        return new DeleteTestResult(name, testName, true);
    }
}

/// <summary>The full <c>delete_test</c> response payload.</summary>
internal sealed record DeleteTestResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("testName")] string TestName,
    [property: JsonPropertyName("deleted")] bool Deleted);
