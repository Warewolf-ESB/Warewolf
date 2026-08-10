/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// Builds the process-wide <see cref="McpServerOptions"/> used by every <c>/mcp</c>
/// request. Registered as a DI singleton so it is constructed once at startup and
/// reused by <see cref="Functions.McpFunction"/> for each incoming request.
///
/// <para>
/// Tools are registered as static-method delegates via <see cref="McpServerTool.Create"/>,
/// with <see cref="McpServerToolCreateOptions.Services"/> set to the app's root
/// <see cref="IServiceProvider"/> so that request-scoped parameters (e.g.
/// <see cref="System.Security.Claims.ClaimsPrincipal"/>, <c>HostEnvironmentConfig</c>,
/// <c>IWorkflowAuthPolicyLoader</c>) are recognised as DI-resolvable and excluded from
/// the tool's client-visible JSON input schema (see
/// <c>RequestServiceProvider{TRequestParams}.IsAugmentedWith</c> in the MCP SDK).
/// </para>
/// </summary>
public static class McpServerOptionsFactory
{
    /// <summary>Server name advertised during MCP <c>initialize</c> handshakes.</summary>
    public const string ServerName = "Warewolf.Execution.Lightweight";

    /// <summary>
    /// Creates a new <see cref="McpServerOptions"/> instance describing this server and
    /// registering its tools, bound to <paramref name="serviceProvider"/> for DI parameter
    /// resolution.
    /// </summary>
    public static McpServerOptions Create(IServiceProvider serviceProvider)
    {
        var version = typeof(McpServerOptionsFactory).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        var options = new McpServerOptions
        {
            ServerInfo = new Implementation
            {
                Name    = ServerName,
                Version = version,
            },
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolsCapability(),
            },
        };

        var toolServices = new McpServerToolCreateOptions { Services = serviceProvider };

        options.ToolCollection ??= new McpServerPrimitiveCollection<McpServerTool>();
        options.ToolCollection.Add(McpServerTool.Create(
            (Func<Infrastructure.HostEnvironmentConfig, Auth.IWorkflowAuthPolicyLoader, System.Security.Claims.ClaimsPrincipal?, string?, string?, int?, ListWorkflowsResult>)ListWorkflowsTool.Handle,
            new McpServerToolCreateOptions
            {
                Services = serviceProvider,
                Name = ListWorkflowsTool.ToolName,
                Description = "Returns workflows stored on this instance, filtered to what the caller is authorized to view.",
                ReadOnly = true,
                Idempotent = true,
                OpenWorld = false,
            }));

        options.ToolCollection.Add(McpServerTool.Create(
            (Func<ListToolsToolResult>)ListToolsTool.Handle,
            new McpServerToolCreateOptions
            {
                Services = serviceProvider,
                Name = ListToolsTool.ToolName,
                Description = "Returns the toolbox activities this server can place inside a workflow body via create_workflow/edit_workflow/add_step.",
                ReadOnly = true,
                Idempotent = true,
                OpenWorld = false,
            }));

        options.ToolCollection.Add(McpServerTool.Create(
            (Func<Infrastructure.HostEnvironmentConfig, Auth.IWorkflowAuthPolicyLoader, System.Security.Claims.ClaimsPrincipal?, string, GetWorkflowDefinitionResult>)GetWorkflowDefinitionTool.Handle,
            new McpServerToolCreateOptions
            {
                Services = serviceProvider,
                Name = GetWorkflowDefinitionTool.ToolName,
                Description = "Returns a workflow's envelope, and its body (the X6 graph) only when the round-trip fidelity gate proves every activity type it uses is editable.",
                ReadOnly = true,
                Idempotent = true,
                OpenWorld = false,
            }));

        return options;
    }
}

