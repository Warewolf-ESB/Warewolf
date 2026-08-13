/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>set_var</c> MCP tool: sets or clears a process environment variable on
/// this host instance — in Azure Function App terms this is the same surface as an
/// "Application Setting", since App Settings are injected into the Function App's process as
/// environment variables (there is no separate mechanism to distinguish the two once the
/// process is running).
///
/// <para>
/// <b>Scope and permission.</b> Unlike <see cref="AddSourceTool"/>/<see cref="CreateWorkflowTool"/>,
/// this operation is not scoped to a single named workflow/resource — it affects the whole
/// host process. It therefore always resolves permission against the <i>global</i> role map (by
/// passing a sentinel scope name that can never match a real resource entry, reusing
/// <see cref="ListWorkflowsTool.HasPermission"/>'s resource-if-present-else-global fallback) and
/// requires the caller to hold <see cref="WorkflowPermission.Administrator"/> — the strictest
/// flag this system defines — rather than the <see cref="WorkflowPermission.Contribute"/> bar
/// used by workflow/source authoring tools.
/// </para>
///
/// <para>
/// <b>Effective timing.</b> A change made via this tool is visible immediately to any code path
/// that re-reads <see cref="Environment.GetEnvironmentVariable(string)"/> live (the codebase's own
/// example is <c>WAREWOLF_SUPER_ADMIN_ENABLED</c>, explicitly "read on every call to support
/// hot-toggle without restart" — see <c>WorkflowAuthPolicyLoader</c>). Settings baked once into an
/// immutable snapshot at startup (see <c>HostEnvironmentConfig.Load</c>) are unaffected until the
/// host process restarts; the response's <see cref="SetVarResult.Note"/> says so explicitly so a
/// caller doesn't assume an instant effect that isn't guaranteed.
/// </para>
///
/// <para>
/// <b>Refuses secret-shaped names.</b> Plain environment variables aren't encrypted at rest the
/// way a saved <c>.bite</c> source's connection string is (see <see cref="AddSourceTool"/>), so
/// this tool declines to set any variable whose <c>name</c> looks like it holds a credential
/// (matches <see cref="SensitiveNamePattern"/>) — callers needing that are pointed at
/// <c>add_source</c>'s existing <c>"${NAME}"</c> Key Vault reference mechanism instead.
/// </para>
/// </summary>
internal static class SetVarTool
{
    internal const string ToolName = "set_var";

    /// <summary>
    /// Sentinel passed as the "workflow name" to <see cref="ListWorkflowsTool.HasPermission"/>
    /// purely to reuse its scope-resolution logic. It is not a valid relative workflow path (it
    /// can never collide with a real resource), so lookup always falls through to the global
    /// role map — exactly the host-wide scope this tool needs.
    /// </summary>
    const string HostScopeName = "$mcp/set_var";

    static readonly Regex SensitiveNamePattern = new(
        "SECRET|PASSWORD|PWD|TOKEN|CONNECTIONSTRING|CREDENTIAL|APIKEY|CLIENTSECRET|PRIVATEKEY",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static SetVarResult Handle(
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The environment variable / application setting name to set (e.g. WAREWOLF_SUPER_ADMIN_ENABLED).")]
        string name,
        [Description("The value to assign. Omit or pass null to remove/unset the variable instead.")]
        string? value = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        var trimmedName = name.Trim();

        if (SensitiveNamePattern.IsMatch(trimmedName))
        {
            throw new McpException(
                $"`name` '{trimmedName}' looks like it holds a secret or credential. set_var refuses to store " +
                "secret-shaped values in a plain environment variable — stage the value in this host's secret " +
                "store (Key Vault, or the local-dev environment-variable fallback) and reference it via " +
                "add_source's \"${NAME}\" syntax instead.");
        }

        var principal = user as WorkflowClaimsPrincipal;
        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, HostScopeName, WorkflowPermission.Administrator))
        {
            throw new McpException("You do not have permission to set host environment variables / app settings. This requires the Administrator permission.");
        }

        var hadPreviousValue = Environment.GetEnvironmentVariable(trimmedName) is not null;
        Environment.SetEnvironmentVariable(trimmedName, value);

        return new SetVarResult(
            trimmedName,
            value is not null,
            hadPreviousValue,
            "Applied to this process immediately. Takes effect right away for any code that reads the variable " +
            "live (e.g. WAREWOLF_SUPER_ADMIN_ENABLED); settings cached once at host startup (see " +
            "HostEnvironmentConfig) only take effect after the process restarts.");
    }
}

/// <summary>The full <c>set_var</c> response payload. Never includes the variable's value.</summary>
internal sealed record SetVarResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("set")] bool Set,
    [property: JsonPropertyName("hadPreviousValue")] bool HadPreviousValue,
    [property: JsonPropertyName("note")] string Note);
