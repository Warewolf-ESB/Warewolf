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
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>create_ui</c> MCP tool: validates a workflow form (see
/// <see cref="GetUiSchemaTool"/>) against the workflow's current envelope and saves it with
/// <see cref="WorkflowUiCatalog"/>, replacing any form the workflow already had. Needs Contribute
/// on the workflow, the same rule <see cref="CreateTestTool"/> applies to its tests.
/// </summary>
internal static class CreateUiTool
{
    internal const string ToolName = "create_ui";

    internal static CreateUiResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("Workflow name. Required unless `id` is given.")]
        string name,
        [Description("Workflow ResourceID (GUID). Used instead of `name` when given.")]
        Guid? id,
        [Description("The form, per get_ui_schema's form_schema.")]
        JsonElement form)
    {
        var resourceId = id is { } value && value != Guid.Empty ? value : (Guid?)null;
        if (resourceId is null && string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` or `id` is required.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var (filePath, relativePath, headerName, label) =
            GetWorkflowDefinitionTool.ResolveByNameOrId(workflowsDirectory, name, resourceId);

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to add a form to workflow '{label}'.");
        }

        var model = WorkflowUiCatalog.Parse(form);
        var (_, inputs, outputs, _) = GetWorkflowDefinitionTool.ReadEnvelopeDetail(filePath);
        var json = WorkflowUiCatalog.ValidateAndSerialize(model, inputs, outputs);

        var replaced = WorkflowUiCatalog.Exists(workflowsDirectory, relativePath);
        WorkflowUiCatalog.Save(workflowsDirectory, relativePath, json);

        return new CreateUiResult(headerName, relativePath, WorkflowUiCatalog.ReadResourceId(filePath), true, replaced);
    }
}

/// <summary>The <c>create_ui</c> response payload.</summary>
internal sealed record CreateUiResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("saved")] bool Saved,
    [property: JsonPropertyName("replaced")] bool Replaced);
