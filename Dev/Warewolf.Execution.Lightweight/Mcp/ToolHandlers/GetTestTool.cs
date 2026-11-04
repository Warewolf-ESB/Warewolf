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
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_test</c> MCP tool (Phase 2 of docs/WorkflowTestFramework-Plan.md): returns a
/// persisted test in the same shape <c>create_test</c>/<c>edit_test</c> take, so a caller can read a
/// test, change one field and send it back through <c>edit_test</c> without remembering the rest.
///
/// <para>
/// <b>The password never leaves.</b> It is persisted DPAPI-encrypted, and the <c>${secret}</c> name it
/// was resolved from isn't kept, so there is nothing safe or useful to return: <c>password</c> comes
/// back <c>null</c> with <c>passwordSet</c> saying whether one is stored. An <c>edit_test</c> of such a
/// test must pass the <c>${secret}</c> reference again.
/// </para>
///
/// <para>Needs <b>View</b>, checked before existence (see <see cref="TestToolGuard"/>).</para>
/// </summary>
internal static class GetTestTool
{
    internal const string ToolName = "get_test";

    internal static GetTestResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The owning workflow's name — same lookup create_test/execute_test use.")]
        string name,
        [Description("The test's name, as passed to create_test/edit_test.")]
        string testName)
    {
        var relativePath = TestToolGuard.ResolveWorkflow(hostConfig, authPolicyLoader, user, name, WorkflowPermission.View, "read tests of");

        if (string.IsNullOrWhiteSpace(testName))
        {
            throw new McpException("`testName` is required.");
        }

        TestCatalog.ValidateTestName(testName);

        var json = TestCatalog.Load(hostConfig.WorkflowsDirectory, relativePath, testName)
            ?? throw new McpException($"Test '{testName}' was not found for workflow '{name}'.");

        TestDefinitionModel? model;
        try
        {
            model = JsonSerializer.Deserialize<TestDefinitionModel>(json, TestDefinitionModel.SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new McpException($"The persisted test '{testName}' could not be parsed: {ex.Message}");
        }

        if (model is null)
        {
            throw new McpException($"The persisted test '{testName}' could not be parsed.");
        }

        return new GetTestResult(name, model with { Password = null }, !string.IsNullOrEmpty(model.Password));
    }
}

/// <summary>The full <c>get_test</c> response payload.</summary>
internal sealed record GetTestResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("test")] TestDefinitionModel Test,
    [property: JsonPropertyName("passwordSet")] bool PasswordSet);
