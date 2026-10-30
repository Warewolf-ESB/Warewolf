/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>list_tests</c> MCP tool (Phase 2 of docs/WorkflowTestFramework-Plan.md): names
/// every test persisted for a workflow, with enough of each definition summarised (input/output/mock/
/// assert counts) for a caller to pick one without a <c>get_test</c> per test.
///
/// <para>
/// <b>Permission before existence</b>, the rule <see cref="DeleteWorkflowTool"/> applies: a caller
/// without <b>View</b> gets the same error whether or not the workflow exists, so this tool is no
/// existence oracle. View, not Execute, because listing reads definitions and runs nothing.
/// </para>
///
/// <para>
/// A <c>.test.json</c> that no longer parses is listed with <c>invalid</c> and its parse error rather
/// than failing the whole call — one hand-edited file must not hide every other test.
/// </para>
/// </summary>
internal static class ListTestsTool
{
    internal const string ToolName = "list_tests";

    internal static ListTestsResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The owning workflow's name — same lookup create_test/execute_test use.")]
        string name)
    {
        var relativePath = TestToolGuard.ResolveWorkflow(hostConfig, authPolicyLoader, user, name, WorkflowPermission.View, "list tests of");
        var workflowsDirectory = hostConfig.WorkflowsDirectory;

        var tests = TestCatalog.List(workflowsDirectory, relativePath)
            .Select(testName => Summarise(testName, TestCatalog.Load(workflowsDirectory, relativePath, testName)))
            .ToList();

        return new ListTestsResult(name, tests);
    }

    static TestSummary Summarise(string testName, string? json)
    {
        TestDefinitionModel? model;
        try
        {
            model = json is null ? null : JsonSerializer.Deserialize<TestDefinitionModel>(json, TestDefinitionModel.SerializerOptions);
        }
        catch (JsonException ex)
        {
            return new TestSummary(testName, Invalid: true, Error: ex.Message);
        }

        if (model is null)
        {
            return new TestSummary(testName, Invalid: true, Error: "The test file is empty.");
        }

        var steps = Flatten(model.TestSteps).ToList();
        return new TestSummary(
            testName,
            model.Enabled,
            model.Inputs?.Count ?? 0,
            model.Outputs?.Count ?? 0,
            steps.Count(step => string.Equals(step.Type, "Mock", StringComparison.OrdinalIgnoreCase)),
            steps.Count(step => string.Equals(step.Type, "Assert", StringComparison.OrdinalIgnoreCase)));
    }

    static IEnumerable<TestStepModel> Flatten(IEnumerable<TestStepModel>? steps) =>
        (steps ?? Enumerable.Empty<TestStepModel>()).SelectMany(step => Flatten(step.Children).Prepend(step));
}

/// <summary>The full <c>list_tests</c> response payload.</summary>
internal sealed record ListTestsResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("tests")] List<TestSummary> Tests);

/// <summary>One test in a <c>list_tests</c> response. The counts are null for an invalid test.</summary>
internal sealed record TestSummary(
    [property: JsonPropertyName("testName")] string TestName,
    [property: JsonPropertyName("enabled")] bool? Enabled = null,
    [property: JsonPropertyName("inputs")] int? Inputs = null,
    [property: JsonPropertyName("outputs")] int? Outputs = null,
    [property: JsonPropertyName("mocks")] int? Mocks = null,
    [property: JsonPropertyName("asserts")] int? Asserts = null,
    [property: JsonPropertyName("invalid")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] bool Invalid = false,
    [property: JsonPropertyName("error")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Error = null);
