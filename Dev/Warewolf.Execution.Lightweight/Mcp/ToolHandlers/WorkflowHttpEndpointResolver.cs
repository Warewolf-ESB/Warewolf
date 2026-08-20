/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Text.Json.Serialization;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Resolves the concrete HTTP path(s) a workflow just written by <see cref="CreateWorkflowTool"/>/
/// <see cref="EditWorkflowTool"/> is (or will be) reachable at, so their responses can tell a caller
/// exactly how to invoke it over HTTP instead of leaving them to guess.
///
/// <para>
/// <b>Why this exists.</b> A newly created/edited workflow is immediately invocable over HTTP —
/// <c>/Public/{*name}</c> and <c>/Secure/{*name}</c> (<see cref="WorkflowHttpFunction"/>) are wildcard
/// catch-all routes already live at host startup, and name resolution (<see cref="Http.WorkflowFunctionHelper.ParseRequestAsync"/>)
/// falls back to a live on-disk scan when the in-memory <see cref="WorkflowIndex"/> entry is stale, so
/// no restart/republish/"sync triggers" step is ever required. The gap this closes is purely
/// discoverability: there is no <c>api/</c> route prefix (<c>host.json</c>'s <c>routePrefix</c> is
/// empty) and no bare-name route — a caller trying <c>{name}</c> or <c>api/{name}</c> gets a bare 404
/// from the Functions host itself (a route that plainly does not exist), easily mistaken for "the
/// workflow isn't reachable yet". Surfacing the real path(s) up front eliminates that class of
/// confusion at its source.
/// </para>
///
/// <para>
/// <b>Which path(s) are returned.</b>
/// <list type="bullet">
/// <item><c>publicUrl</c> — <c>/Public/{name}</c>, included only when the real <c>/Public/*</c> gate
/// (<see cref="WorkflowHttpFunction"/>'s <c>PublicExecution</c> case, via
/// <see cref="Auth.IWorkflowPolicyMatcher"/>/<see cref="Auth.WorkflowAuthPolicyLoader.GetPolicy"/>)
/// would actually allow it: either <c>secure.config</c> is effective and the <em>Public</em> group is
/// granted View+Execute, <b>or</b> <c>secure.config</c> is not effective <i>and</i>
/// <c>BYPASS_SECURE_CONFIG=true</c> is explicitly set (the only case that makes <c>/Public/*</c> truly
/// open-access — config merely being absent, without the bypass flag, is a deployment error that
/// <see cref="WorkflowPolicyMatcher"/> denies with a 500, not an allow). Getting this wrong in either
/// direction would be worse than not reporting it at all, so this mirrors the real gate exactly rather
/// than the more lenient <see cref="IWorkflowAuthPolicyLoader.IsConfigEffective"/> check the MCP-API
/// tools themselves use for their own baseline gate.</item>
/// <item><c>secureUrl</c> — <c>/Secure/{name}</c>, included only when <c>secure.config</c> is
/// effective (JWT mode). In open-access mode <c>/Secure/*</c> always 401s (there is no secret key to
/// validate a JWT against — see <see cref="WorkflowHttpFunction"/>'s own remarks) regardless of
/// <c>BYPASS_SECURE_CONFIG</c>, so returning it there would be actively misleading; it is omitted
/// instead. When present, reaching it still requires the caller's own JWT to carry a role with
/// Execute permission on this workflow, evaluated per-request exactly like any other secured
/// resource.</item>
/// </list>
/// Both may be <c>null</c> — e.g. when <c>secure.config</c> is absent and <c>BYPASS_SECURE_CONFIG</c>
/// is not set, neither route is reachable until an operator deploys a valid <c>secure.config</c> or
/// explicitly opts into open-access mode.
/// </para>
/// </summary>
internal static class WorkflowHttpEndpointResolver
{
    const string BypassEnvVar = "BYPASS_SECURE_CONFIG";

    /// <summary>
    /// Builds the <see cref="WorkflowHttpEndpoints"/> for a workflow at <paramref name="relativePath"/>
    /// (the same forward-slash, extension-free path <c>create_workflow</c>/<c>edit_workflow</c>'s own
    /// <c>name</c> input and <c>list_workflows</c>' <c>path</c> output use).
    /// </summary>
    internal static WorkflowHttpEndpoints Resolve(IWorkflowAuthPolicyLoader authPolicyLoader, string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');

        var secureUrl = authPolicyLoader.IsConfigEffective ? "/Secure/" + normalized : null;
        var publicUrl = HasPublicExecutePermission(authPolicyLoader, normalized) ? "/Public/" + normalized : null;

        return new WorkflowHttpEndpoints(publicUrl, secureUrl);
    }

    /// <summary>
    /// Mirrors <see cref="WorkflowHttpFunction"/>'s <c>PublicExecution</c> case exactly: when
    /// <c>secure.config</c> is effective, the same effective-permissions check with an empty
    /// caller-roles set (so only an active <em>Public</em> group entry, auto-applied regardless of
    /// caller, can satisfy it); when not effective, only an explicit <c>BYPASS_SECURE_CONFIG=true</c>
    /// makes it open-access — matching <see cref="Auth.WorkflowAuthPolicyLoader.GetPolicy"/>'s own
    /// bypass check so this can never claim a URL is reachable when the real gate would 500 it.
    /// </summary>
    static bool HasPublicExecutePermission(IWorkflowAuthPolicyLoader authPolicyLoader, string relativePath)
    {
        if (!authPolicyLoader.IsConfigEffective)
        {
            return string.Equals(
                Environment.GetEnvironmentVariable(BypassEnvVar),
                "true", StringComparison.OrdinalIgnoreCase);
        }

        var permissions = authPolicyLoader.GetEffectivePermissions(relativePath, Array.Empty<string>());
        return permissions.HasFlag(WorkflowPermission.View | WorkflowPermission.Execute);
    }
}

/// <summary>The HTTP path(s) a workflow is reachable at, included in <c>create_workflow</c>/<c>edit_workflow</c> responses.</summary>
internal sealed record WorkflowHttpEndpoints(
    [property: JsonPropertyName("publicUrl")] string? PublicUrl,
    [property: JsonPropertyName("secureUrl")] string? SecureUrl);
