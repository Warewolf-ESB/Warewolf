/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>edit_test</c> MCP tool: re-validates and overwrites an <b>existing</b> test
/// definition in place — the save-side mirror of <see cref="CreateTestTool"/> for a test that
/// already exists (plan §7a.4a).
///
/// <para>
/// <b>Differences from <see cref="CreateTestTool"/>:</b> the existence check inverts (the test
/// MUST already exist), and there is no rename support — <c>test.testName</c> identifies which
/// existing <c>.test.json</c> file to overwrite; a rename is <c>create_test</c> +
/// (once Phase 2 exists) <c>delete_test</c>, mirroring <see cref="EditWorkflowTool"/>'s equivalent
/// "does not move or rename" restriction. Everything else — workflow resolution, Contribute
/// permission, body-based <c>activityId</c> validation, secret resolution — is shared verbatim via
/// <see cref="CreateTestTool"/>'s <c>internal</c> helpers.
/// </para>
/// </summary>
internal static class EditTestTool
{
    internal const string ToolName = "edit_test";

    internal static async Task<EditTestResult> Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        IMcpSecretResolver secretResolver,
        ClaimsPrincipal? user,
        [Description("The owning workflow's name — same lookup create_workflow/edit_workflow use. Must already exist.")]
        string name,
        [Description("The test definition — { testName, enabled?, authenticationType?, userName?, password?, inputs[], outputs[], noErrorExpected?, errorExpected?, errorContainsText?, testSteps[] } per get_workflow_schema's test_schema. testName identifies which existing test to overwrite; edit_test does not rename.")]
        JsonElement test,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        var (filePath, headerName) = CreateTestTool.ResolveWorkflow(workflowsDirectory, relativePath);
        if (filePath is null || headerName is null)
        {
            throw new McpException($"Workflow '{name}' was not found.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to edit tests on workflow '{name}'.");
        }

        var model = CreateTestTool.ParseTestDefinition(test);
        TestCatalog.ValidateTestName(model.TestName);

        if (!TestCatalog.Exists(workflowsDirectory, relativePath, model.TestName))
        {
            throw new McpException($"Test '{model.TestName}' was not found for workflow '{name}'; use create_test to create a new test.");
        }

        var (bodyEditable, body, nonEditableReason) = GetWorkflowDefinitionTool.BuildBody(filePath, headerName);
        if (!bodyEditable || body is null)
        {
            throw new McpException($"Workflow '{name}' cannot have tests authored against it: {nonEditableReason}");
        }

        var json = await CreateTestTool.ValidateAndSerializeAsync(secretResolver, model, body.Value, cancellationToken).ConfigureAwait(false);

        TestCatalog.Save(workflowsDirectory, relativePath, model.TestName, json);

        return new EditTestResult(name, model.TestName, true);
    }
}

/// <summary>The full <c>edit_test</c> response payload.</summary>
internal sealed record EditTestResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("testName")] string TestName,
    [property: JsonPropertyName("updated")] bool Updated);
