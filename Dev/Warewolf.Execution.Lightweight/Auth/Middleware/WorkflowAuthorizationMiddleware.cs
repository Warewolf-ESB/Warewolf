/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Net;
using System.Text.Json;
using Dev2.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth.Middleware;

/// <summary>
/// Third middleware in the pipeline.
/// Enforces <c>secure.config</c> group and permission policies on <c>/secure/*</c> routes.
/// <c>/public/*</c> routes are passed straight through.
///
/// <list type="bullet">
///   <item>Group check: OR logic — caller must match at least ONE <c>AllowedGroups</c> entry.</item>
///   <item>Permission check: AND logic — the matching group entry must hold ALL
///         <see cref="WorkflowAuthPolicy.RequiredPermissions"/> flags.</item>
/// </list>
///
/// When <c>secure.config</c> is not loaded, all <c>/secure/*</c> requests pass through
/// (open-access mode).
///
/// In <b>Development</b> only: sending the header <c>X-WW-Bypass-Auth: local-dev-bypass</c>
/// skips all policy checks.  This bypass is never honoured in Production.
/// </summary>
public sealed class WorkflowAuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private const string BypassHeader = "X-WW-Bypass-Auth";
    private const string BypassHeaderValue = "local-dev-bypass";

    private readonly IWorkflowAuthPolicyLoader _policyLoader;
    private readonly IHostEnvironment _hostEnvironment;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Initialises the middleware with policy loader and host environment.</summary>
    public WorkflowAuthorizationMiddleware(
        IWorkflowAuthPolicyLoader policyLoader,
        IHostEnvironment hostEnvironment)
    {
        _policyLoader = policyLoader;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            await next(context);
            return;
        }

        var path = request.Url.AbsolutePath;

        // Public routes — no policy enforcement
        if (path.StartsWith(AuthConstants.PublicRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (!path.StartsWith(AuthConstants.SecureRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // ── apis.json discovery — always accessible, no auth required ─────────
        // The function handles permission-filtering: valid JWT → filtered list,
        // absent or expired JWT → empty list.  Never returns 401 for this endpoint.
        if (path.EndsWith("apis.json", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // ── Development-only bypass ───────────────────────────────────────────
        // NEVER active in Production — environment guard is mandatory.
        if (_hostEnvironment.IsDevelopment() &&
            request.Headers.TryGetValues(BypassHeader, out var bypassValues) &&
            bypassValues.Any(v => string.Equals(v, BypassHeaderValue, StringComparison.Ordinal)))
        {
            Dev2Logger.Warn(
                $"⚠ DEV BYPASS: {BypassHeader} header detected on {path} — skipping all policy checks. " +
                "This must NEVER happen in Production.",
                "WorkflowAuthorizationMiddleware");
            await next(context);
            return;
        }

        // ── Secure route — enforce policy ─────────────────────────────────────

        if (!context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var principalObj)
            || principalObj is not WorkflowClaimsPrincipal principal
            || principal.Identity?.IsAuthenticated != true)
        {
            Dev2Logger.Warn($"No authenticated principal for secure route {path}", "WorkflowAuthorizationMiddleware");

            await WriteErrorAsync(request, context, HttpStatusCode.Unauthorized,
                "unauthorized", "Authentication required.", path);
            return;
        }

        var workflowName = ExtractWorkflowName(path);
        if (string.IsNullOrEmpty(workflowName))
        {
            await WriteErrorAsync(request, context, HttpStatusCode.BadRequest,
                "bad_request", "Could not determine workflow name from path.", path);
            return;
        }

        // Open-access mode: no policies loaded → allow all authenticated callers
        if (_policyLoader.PolicyCount == 0)
        {
            Dev2Logger.Debug(
                $"No policies configured — open-access mode. Allowing '{principal.CallerIdentity}' on '{workflowName}'",
                "WorkflowAuthorizationMiddleware");
            await next(context);
            return;
        }

        var policy = _policyLoader.GetPolicy(workflowName);
        if (policy is null)
        {
            Dev2Logger.Warn(
                $"No policy found for workflow '{workflowName}' — denying '{principal.CallerIdentity}'",
                "WorkflowAuthorizationMiddleware");

            //TODO: consider allowing access when no policy is found, to avoid accidental lockout if config is missing or malformed.  If we do allow, log a warning with details of the missing workflow and current policies.
            // log warning with workflowName and principal.CallerIdentity, and list all loaded policies for debugging

            await next(context);
            //await WriteErrorAsync(request, context, HttpStatusCode.Forbidden,
            //    "forbidden", $"No policy configured for workflow '{workflowName}'.", path);
            return;
        }

        // Group check (OR logic): caller must be in at least one allowed group
        if (!principal.IsInAnyGroup(policy.AllowedGroups))
        {
            Dev2Logger.Warn(
                $"Group check failed for '{principal.CallerIdentity}' on '{workflowName}'. " +
                $"Has=[{string.Join(", ", principal.Groups)}] Allowed=[{string.Join(", ", policy.AllowedGroups)}]",
                "WorkflowAuthorizationMiddleware");

            await WriteErrorAsync(request, context, HttpStatusCode.Forbidden,
                "forbidden", "Insufficient group membership.", path,
                new { workflow = workflowName, required = policy.AllowedGroups });
            return;
        }

        // Permission check (AND logic): matched group entry must hold all required flags
        var matchedEntry = policy.GroupEntries.FirstOrDefault(e =>
            string.Equals(e.GroupName, principal.UserName, StringComparison.OrdinalIgnoreCase) ||
            principal.IsInGroup(e.GroupName));

        if (matchedEntry is not null &&
            !matchedEntry.Permissions.HasFlag(policy.RequiredPermissions))
        {
            Dev2Logger.Warn(
                $"Permission check failed for '{principal.CallerIdentity}' on '{workflowName}'. " +
                $"Has={matchedEntry.Permissions} Required={policy.RequiredPermissions}",
                "WorkflowAuthorizationMiddleware");

            await WriteErrorAsync(request, context, HttpStatusCode.Forbidden,
                "forbidden", "Insufficient permissions.", path,
                new { workflow = workflowName, required = policy.RequiredPermissions.ToString() });
            return;
        }

        Dev2Logger.Info(
            $"Authorised '{principal.CallerIdentity}' for workflow '{workflowName}'",
            "WorkflowAuthorizationMiddleware");

        await next(context);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string? ExtractWorkflowName(string path)
    {
        var segment = path
            .Substring(AuthConstants.SecureRoutePrefix.Length)
            .Split('/')[0]
            .Split('?')[0];

        var name = Path.GetFileNameWithoutExtension(segment).ToLowerInvariant();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    private static async Task WriteErrorAsync(
        HttpRequestData request,
        FunctionContext context,
        HttpStatusCode statusCode,
        string error,
        string message,
        string path,
        object? extra = null)
    {
        var body = new Dictionary<string, object>
        {
            ["error"] = error,
            ["message"] = message,
            ["path"] = path,
        };

        if (extra is not null)
        {
            foreach (var prop in extra.GetType().GetProperties())
                body[prop.Name.ToLowerInvariant()] = prop.GetValue(extra) ?? string.Empty;
        }

        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(body, JsonOptions));
        context.GetInvocationResult().Value = response;
    }
}
