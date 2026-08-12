/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>execute_workflow</c> MCP tool (<c>warewolf-lee-mcp-v3-spec.md</c>, "Tools" §
/// <c>execute_workflow</c>): runs a workflow on this instance via <see cref="IWorkflowExecutor"/>
/// and returns its result. Unchanged from v2 in every respect — execution never inspects the
/// body format (XAML), editable or not, so this tool works identically for a workflow with
/// <c>bodyEditable: true</c> or <c>false</c>.
///
/// <para>
/// <b>Resolution/permission are shared with <see cref="ListWorkflowsTool"/>/
/// <see cref="GetWorkflowDefinitionTool"/></b> (<see cref="WorkflowNameResolver.Resolve"/>,
/// <see cref="ListWorkflowsTool.TryReadWorkflowHeader"/>, <see cref="ListWorkflowsTool.HasPermission"/>)
/// so a <c>name</c> that came from a prior <c>list_workflows</c>/<c>get_workflow_definition</c>
/// response's <c>path</c>/<c>name</c> resolves and reports identically here — but the required
/// flag is <b>Execute</b>, not View, per spec's Authorization table.
/// </para>
///
/// <para>
/// <b><c>inputs</c> binding.</b> The spec's input model (scalars as JSON strings, recordsets as
/// JSON arrays of objects, JSON objects as nested JSON) is exactly what
/// <c>ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload</c> already accepts as a raw
/// JSON body on the full server and on this engine's own HTTP routes (see
/// <see cref="Http.WorkflowFunctionHelper"/>'s <c>RawInputPayload</c> handling) — so <c>inputs</c>
/// is passed straight through as <see cref="WorkflowExecutionRequest.RawInputPayload"/>'s raw JSON
/// text, rather than being flattened into <see cref="WorkflowExecutionRequest.InputParameters"/>
/// (a flat <c>Dictionary&lt;string,string&gt;</c> cannot carry a recordset or nested object).
/// </para>
///
/// <para>
/// <b><c>outputs</c> production.</b> <see cref="WorkflowExecutionResult.Outputs"/> is only
/// populated as a side effect of invoking <see cref="WorkflowExecutionResult.PayloadWriter"/>
/// (see <c>WorkflowExecutor.ExtractPayload</c>/<c>TryPopulateOutputsDictionary</c>) — so this
/// handler awaits <see cref="WorkflowExecutionResult.ReadPayloadAsync"/> first (discarding the
/// possibly error-wrapped string it returns) purely to trigger that population, then reads
/// <see cref="WorkflowExecutionResult.Outputs"/> directly. Reading <c>Outputs</c> rather than the
/// returned string avoids <c>ExtractPayload</c>'s <c>{ hasErrors, errors, output }</c> envelope
/// that wraps the STREAMED payload (not <c>Outputs</c>) whenever <c>Errors.Count &gt; 0</c>.
/// </para>
/// </summary>
internal static class ExecuteWorkflowTool
{
    internal const string ToolName = "execute_workflow";

    internal static async Task<ExecuteWorkflowResult> Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        IWorkflowExecutor workflowExecutor,
        ClaimsPrincipal? user,
        [Description("Workflow name — a relative path (forward slashes), no extension. Must match a `name`/`path` from a prior list_workflows/get_workflow_definition response.")]
        string name,
        [Description("Input values matching envelope.inputs — scalars as strings, recordsets as arrays of objects, JSON objects as nested JSON. Omit for a workflow with no inputs.")]
        JsonElement? inputs = null,
        CancellationToken cancellationToken = default)
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

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Execute))
        {
            throw new McpException($"You do not have permission to execute workflow '{name}'.");
        }

        var request = new WorkflowExecutionRequest
        {
            WorkflowFilePath = filePath,
            WorkflowsDirectory = workflowsDirectory,
            WorkflowName = headerName,
            ReturnType = Dev2.Web.EmitionTypes.JSON,
            IsDebug = false,
            ExecutingPrincipal = principal,
        };

        if (inputs is { ValueKind: not (JsonValueKind.Undefined or JsonValueKind.Null) } inputsElement)
        {
            request.RawInputPayload = inputsElement.GetRawText();
        }

        var result = workflowExecutor.Execute(request);

        // Triggers WorkflowExecutionResult.Outputs population as a side effect (see class
        // remarks) — the returned string is intentionally discarded.
        await result.ReadPayloadAsync(cancellationToken).ConfigureAwait(false);

        var outputs = BuildOutputsElement(result.Outputs);
        var status = result.IsSuccess ? "success" : "error";
        var error = result.IsSuccess ? null : string.Join("; ", result.Errors);

        return new ExecuteWorkflowResult(outputs, status, error, result.ExecutionId.ToString());
    }

    /// <summary>
    /// Converts <see cref="WorkflowExecutionResult.Outputs"/> (a <c>Dictionary&lt;string,object&gt;</c>
    /// produced by Newtonsoft's <c>JsonConvert.DeserializeObject</c>, whose values may themselves be
    /// <c>JObject</c>/<c>JArray</c>/primitives) into a <see cref="JsonElement"/> for the tool's
    /// <c>System.Text.Json</c>-based response shape. Round-trips through Newtonsoft's own
    /// serializer (rather than inspecting each value's CLR type) so nested objects/arrays of any
    /// shape are preserved byte-for-byte, matching <see cref="GetWorkflowDefinitionTool"/>'s own
    /// "build via Newtonsoft, parse via System.Text.Json" pattern for <c>body</c>.
    /// </summary>
    static JsonElement BuildOutputsElement(System.Collections.Generic.Dictionary<string, object>? outputs)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(outputs ?? new());
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

/// <summary>The full <c>execute_workflow</c> response payload.</summary>
internal sealed record ExecuteWorkflowResult(
    [property: JsonPropertyName("outputs")] JsonElement Outputs,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("executionId")] string ExecutionId);
