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
/// Implements the <c>get_ui</c> MCP tool: returns a workflow's form together with its envelope,
/// which is everything the web Studio needs to render <c>/form/{workflowId}</c> in one call.
/// <c>form</c> is <c>null</c> when none was saved; the Studio then builds a plain form from the
/// envelope's inputs. Needs View, as <c>get_workflow_definition</c> does — running the form still
/// goes through <c>execute_workflow</c>'s own Execute check.
/// </summary>
internal static class GetUiTool
{
    internal const string ToolName = "get_ui";

    internal static GetUiResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("Workflow name. Required unless `id` is given.")]
        string name,
        [Description("Workflow ResourceID (GUID). Used instead of `name` when given.")]
        Guid? id)
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

        if (!ListWorkflowsTool.HasViewPermission(authPolicyLoader, principal, relativePath))
        {
            throw new McpException($"You do not have permission to view workflow '{label}'.");
        }

        var (description, inputs, outputs, variables) = GetWorkflowDefinitionTool.ReadEnvelopeDetail(filePath);
        var envelope = new WorkflowEnvelope(headerName, description, inputs, outputs, variables);
        var form = WorkflowUiCatalog.Load(workflowsDirectory, relativePath);

        return new GetUiResult(headerName, relativePath, WorkflowUiCatalog.ReadResourceId(filePath), envelope, form);
    }
}

/// <summary>The <c>get_ui</c> response payload.</summary>
internal sealed record GetUiResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("envelope")] WorkflowEnvelope Envelope,
    [property: JsonPropertyName("form")] JsonElement? Form);
