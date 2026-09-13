/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System;
using System.Collections.Generic;
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
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>create_test</c> MCP tool: persists a new workflow-test definition (Warewolf's
/// "Service Tests" feature, ported Lightweight-native — see
/// docs/WorkflowTestFramework-Plan.md §7a) as
/// <c>{WorkflowsDirectory}/{relativePath}.tests/{TestName}.test.json</c>. Write-only in this phase —
/// the test is validated and stored but never executed by this tool (see <c>execute_test</c>).
///
/// <para>
/// <b>Steps, in spec order (plan §7a.4):</b> resolve the owning workflow (must already exist and be
/// a real workflow, not a source), Contribute-permission check (same rule
/// <see cref="CreateWorkflowTool"/> applies), test-not-already-exists check, resolve the workflow's
/// current body via <see cref="GetWorkflowDefinitionTool.BuildBody"/> (must be
/// <c>bodyEditable</c>), validate every <c>testSteps[].activityId</c> (recursively through
/// <c>children</c>) against that body's node ids, validate <c>authenticationType</c>/
/// <c>testSteps[].type</c> against their closed value sets, resolve any <c>${secret}</c> placeholder
/// in <c>password</c> and DPAPI-encrypt it, then persist.
/// </para>
///
/// <para>
/// <b>Deliberately not cross-checked</b> (plan §6.6): <c>inputs</c>/<c>outputs</c>/<c>stepOutputs</c>
/// variable names are not required to already be declared in the workflow's envelope —
/// mocking/asserting mid-workflow state doesn't require that state to be a public envelope
/// input/output.
/// </para>
///
/// <para>
/// <b>Shared with <see cref="EditTestTool"/>.</b> Workflow resolution (<see cref="ResolveWorkflow"/>),
/// request parsing (<see cref="ParseTestDefinition"/>), and validation/secret-resolution/
/// serialization (<see cref="ValidateAndSerializeAsync"/>) are <c>internal</c> so <c>edit_test</c>
/// reuses them verbatim rather than duplicating this tool's logic.
/// </para>
/// </summary>
internal static class CreateTestTool
{
    internal const string ToolName = "create_test";

    internal static async Task<CreateTestResult> Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        IMcpSecretResolver secretResolver,
        ClaimsPrincipal? user,
        [Description("The owning workflow's name — same lookup create_workflow/edit_workflow use. Must already exist.")]
        string name,
        [Description("The test definition — { testName, enabled?, authenticationType?, userName?, password?, inputs[], outputs[], noErrorExpected?, errorExpected?, errorContainsText?, testSteps[] } per get_workflow_schema's test_schema.")]
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

        var (filePath, headerName) = ResolveWorkflow(workflowsDirectory, relativePath);
        if (filePath is null || headerName is null)
        {
            throw new McpException($"Workflow '{name}' was not found.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to add tests to workflow '{name}'.");
        }

        var model = ParseTestDefinition(test);
        TestCatalog.ValidateTestName(model.TestName);

        if (TestCatalog.Exists(workflowsDirectory, relativePath, model.TestName))
        {
            throw new McpException($"A test named '{model.TestName}' already exists for workflow '{name}'.");
        }

        var (bodyEditable, body, nonEditableReason) = GetWorkflowDefinitionTool.BuildBody(filePath, headerName);
        if (!bodyEditable || body is null)
        {
            throw new McpException($"Workflow '{name}' cannot have tests authored against it: {nonEditableReason}");
        }

        var json = await ValidateAndSerializeAsync(secretResolver, model, body.Value, cancellationToken).ConfigureAwait(false);

        TestCatalog.Save(workflowsDirectory, relativePath, model.TestName, json);

        return new CreateTestResult(name, model.TestName, true);
    }

    /// <summary>
    /// <c>internal</c> so <see cref="EditTestTool"/> resolves the owning workflow identically —
    /// must be an existing, real workflow (<c>ResourceType="WorkflowService"</c>), not a source or
    /// other resource type.
    /// </summary>
    internal static (string? FilePath, string? HeaderName) ResolveWorkflow(string workflowsDirectory, string relativePath)
    {
        var filePath = WorkflowNameResolver.Resolve(workflowsDirectory, relativePath);
        if (filePath is null)
        {
            return (null, null);
        }

        var (headerName, isWorkflow) = ListWorkflowsTool.TryReadWorkflowHeader(filePath);
        return isWorkflow && headerName is not null ? (filePath, headerName) : (null, null);
    }

    /// <summary><c>internal</c> so <see cref="EditTestTool"/> parses the request body identically.</summary>
    internal static TestDefinitionModel ParseTestDefinition(JsonElement test)
    {
        if (test.ValueKind != JsonValueKind.Object)
        {
            throw new McpException("`test` must be a JSON object.");
        }

        TestDefinitionModel? model;
        try
        {
            model = JsonSerializer.Deserialize<TestDefinitionModel>(test.GetRawText(), TestDefinitionModel.SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new McpException($"`test` could not be parsed: {ex.Message}");
        }

        if (model is null || string.IsNullOrWhiteSpace(model.TestName))
        {
            throw new McpException("`test.testName` is required.");
        }

        return model;
    }

    /// <summary>
    /// Validates <paramref name="model"/> against <paramref name="body"/>'s current node ids and the
    /// closed <c>authenticationType</c>/<c>StepType</c> value sets, resolves any <c>${secret}</c>
    /// placeholder in <c>password</c> and DPAPI-encrypts it, then serializes the result.
    /// <c>internal</c> so <see cref="EditTestTool"/> shares this identically.
    /// </summary>
    internal static async Task<string> ValidateAndSerializeAsync(
        IMcpSecretResolver secretResolver, TestDefinitionModel model, JsonElement body, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (model.AuthenticationType is { Length: > 0 } authType &&
            !Enum.TryParse<Dev2.Runtime.ServiceModel.Data.AuthenticationType>(authType, ignoreCase: true, out _))
        {
            errors.Add(
                $"`test.authenticationType` ('{authType}') must be one of: " +
                $"{string.Join(", ", Enum.GetNames<Dev2.Runtime.ServiceModel.Data.AuthenticationType>())}.");
        }

        var cellIds = CollectCellIds(body);
        ValidateSteps(model.TestSteps, cellIds, errors, "test.testSteps");

        if (errors.Count > 0)
        {
            throw new McpException($"`test` failed validation: {string.Join("; ", errors)}");
        }

        var password = model.Password;
        if (!string.IsNullOrEmpty(password))
        {
            if (!AddSourceTool.SecretPlaceholder.IsMatch(password))
            {
                throw new McpException("`test.password` must reference a secret as \"${secret-name}\" — a literal password is not accepted.");
            }

            var resolvedSecretFields = new List<string>();
            var resolvedPassword = await AddSourceTool.ResolveSecretPlaceholdersAsync(
                password, "password", secretResolver, resolvedSecretFields, cancellationToken).ConfigureAwait(false);
            password = DpapiWrapper.Encrypt(resolvedPassword);
        }

        var toWrite = model with { Password = password };
        return JsonSerializer.Serialize(toWrite, TestDefinitionModel.SerializerOptions);
    }

    static HashSet<string> CollectCellIds(JsonElement body)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        if (body.ValueKind == JsonValueKind.Object &&
            body.TryGetProperty("cells", out var cells) &&
            cells.ValueKind == JsonValueKind.Array)
        {
            foreach (var cell in cells.EnumerateArray())
            {
                if (cell.ValueKind == JsonValueKind.Object &&
                    cell.TryGetProperty("id", out var idEl) &&
                    idEl.ValueKind == JsonValueKind.String)
                {
                    var id = idEl.GetString();
                    if (!string.IsNullOrEmpty(id))
                    {
                        ids.Add(id);
                    }
                }
            }
        }

        return ids;
    }

    static void ValidateSteps(IReadOnlyList<TestStepModel>? steps, HashSet<string> cellIds, List<string> errors, string pathPrefix)
    {
        if (steps is null)
        {
            return;
        }

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var path = $"{pathPrefix}[{i}]";

            if (string.IsNullOrWhiteSpace(step.ActivityId))
            {
                errors.Add($"`{path}.activityId` is required.");
            }
            else if (!cellIds.Contains(step.ActivityId))
            {
                errors.Add($"`{path}.activityId` ('{step.ActivityId}') does not match any node in the workflow's current body.");
            }

            if (!Enum.TryParse<Dev2.Common.Interfaces.StepType>(step.Type, ignoreCase: true, out _))
            {
                errors.Add($"`{path}.type` ('{step.Type}') must be 'Mock' or 'Assert'.");
            }

            ValidateSteps(step.Children, cellIds, errors, $"{path}.children");
        }
    }
}

/// <summary>The full <c>create_test</c> response payload.</summary>
internal sealed record CreateTestResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("testName")] string TestName,
    [property: JsonPropertyName("created")] bool Created);
