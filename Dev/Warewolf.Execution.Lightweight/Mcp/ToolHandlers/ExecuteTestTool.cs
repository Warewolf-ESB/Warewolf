/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common.Interfaces;
using ModelContextProtocol;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Execution;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>execute_test</c> MCP tool (Phase 3 of
/// docs/WorkflowTestFramework-Plan.md): runs a persisted <c>create_test</c>/<c>edit_test</c>
/// definition against the owning workflow via <see cref="IWorkflowExecutor.ExecuteTest"/> and
/// reports pass/fail per step and per output.
///
/// <para>
/// <b>Resolution/permission</b> mirror <see cref="ExecuteWorkflowTool"/> — same workflow lookup as
/// <see cref="CreateTestTool.ResolveWorkflow"/>, but the required flag is <b>Execute</b>
/// (<see cref="WorkflowPermission.Execute"/>), matching <c>execute_workflow</c>'s own rule; tests
/// have no independent ACL entry (plan §6.3).
/// </para>
///
/// <para>
/// <b>Read-only against the test catalog.</b> The persisted <c>.test.json</c> is never rewritten
/// after a run (no <c>LastRunDate</c>/<c>TestPassed</c> mutation on disk, plan Phase-3 decision 4) —
/// the verdict is reported in this tool's response only.
/// </para>
/// </summary>
internal static class ExecuteTestTool
{
    internal const string ToolName = "execute_test";

    internal static Task<ExecuteTestResult> Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        IWorkflowExecutor workflowExecutor,
        ClaimsPrincipal? user,
        [Description("The owning workflow's name — same lookup create_workflow/edit_workflow use.")]
        string name,
        [Description("The test's name, as passed to create_test/edit_test.")]
        string testName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        if (string.IsNullOrWhiteSpace(testName))
        {
            throw new McpException("`testName` is required.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        var (filePath, headerName) = CreateTestTool.ResolveWorkflow(workflowsDirectory, relativePath);
        if (filePath is null || headerName is null)
        {
            throw new McpException($"Workflow '{name}' was not found.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Execute))
        {
            throw new McpException($"You do not have permission to execute tests on workflow '{name}'.");
        }

        var json = TestCatalog.Load(workflowsDirectory, relativePath, testName);
        if (json is null)
        {
            throw new McpException($"Test '{testName}' was not found for workflow '{name}'.");
        }

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

        var serviceTest = TestDefinitionMapper.Map(model);

        var request = new TestExecutionRequest
        {
            WorkflowFilePath = filePath,
            WorkflowsDirectory = workflowsDirectory,
            WorkflowName = headerName,
            ServiceTest = serviceTest,
            ExecutingPrincipal = principal,
        };

        var result = workflowExecutor.ExecuteTest(request);

        return Task.FromResult(BuildResponse(name, result));
    }

    static ExecuteTestResult BuildResponse(string name, TestExecutionResult result)
    {
        var outputs = (result.ServiceTest?.Outputs ?? new List<IServiceTestOutput>())
            .Select(MapOutput)
            .ToList();

        var steps = (result.ServiceTest?.TestSteps ?? new List<IServiceTestStep>())
            .Select(MapStep)
            .ToList();

        return new ExecuteTestResult(
            name,
            result.TestName,
            result.IsSuccess,
            result.TestPassed,
            result.Result.ToString(),
            result.Message,
            outputs,
            steps,
            result.Errors,
            result.ExecutionId.ToString());
    }

    static TestOutputAssertionResult MapOutput(IServiceTestOutput output) => new(
        output.Variable,
        output.AssertOp,
        output.Value,
        output.Result?.RunTestResult.ToString() ?? "None",
        output.Result?.Message);

    static TestStepAssertionResult MapStep(IServiceTestStep step) => new(
        step.ActivityID.ToString(),
        step.ActivityType,
        step.Type.ToString(),
        step.Result?.RunTestResult.ToString() ?? "None",
        step.Result?.Message,
        (step.Children ?? new System.Collections.ObjectModel.ObservableCollection<IServiceTestStep>()).Select(MapStep).ToList());
}

/// <summary>The full <c>execute_test</c> response payload.</summary>
internal sealed record ExecuteTestResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("testName")] string TestName,
    [property: JsonPropertyName("isSuccess")] bool IsSuccess,
    [property: JsonPropertyName("testPassed")] bool TestPassed,
    [property: JsonPropertyName("result")] string Result,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("outputs")] List<TestOutputAssertionResult> Outputs,
    [property: JsonPropertyName("steps")] List<TestStepAssertionResult> Steps,
    [property: JsonPropertyName("errors")] List<string> Errors,
    [property: JsonPropertyName("executionId")] string ExecutionId);

internal sealed record TestOutputAssertionResult(
    [property: JsonPropertyName("variable")] string Variable,
    [property: JsonPropertyName("assertOp")] string AssertOp,
    [property: JsonPropertyName("expected")] string Expected,
    [property: JsonPropertyName("result")] string Result,
    [property: JsonPropertyName("message")] string? Message);

internal sealed record TestStepAssertionResult(
    [property: JsonPropertyName("activityId")] string ActivityId,
    [property: JsonPropertyName("activityType")] string ActivityType,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("result")] string Result,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("children")] List<TestStepAssertionResult> Children);
