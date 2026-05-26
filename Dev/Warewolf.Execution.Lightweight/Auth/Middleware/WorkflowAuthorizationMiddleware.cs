/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Auth.Middleware;

/// <summary>
/// Third middleware in the pipeline.
/// Enforces <c>secure.config</c> group and permission policies on
/// <c>/secure/*</c> and <c>/services/*</c> routes.
/// <c>/public/*</c> routes are passed straight through.
///
/// <para>
/// Matching decisions are fully delegated to <see cref="IWorkflowPolicyMatcher"/>
/// so the strategy can be replaced without touching this class.
/// Per-route required permissions are resolved from
/// <see cref="IRouteAuthorizationRegistry"/> (populated at startup by reflecting
/// over <see cref="RequireWorkflowPermissionAttribute"/> on function methods),
/// meaning individual route methods do not repeat permission-check boilerplate.
/// </para>
///
/// <list type="bullet">
///   <item>Group check: OR logic — caller must match at least ONE <c>AllowedGroups</c> entry.</item>
///   <item>Permission check: AND logic — the matching group entry must hold ALL
///         required permission flags.</item>
/// </list>
///
/// When <c>secure.config</c> is not loaded, all authenticated routes pass through
/// (open-access mode).
///
/// In <b>Development</b> only: sending the header <c>X-WW-Bypass-Auth: local-dev-bypass</c>
/// skips all policy checks.  This bypass is never honoured in Production.
/// </summary>
public sealed class WorkflowAuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private const string BypassHeader        = "X-WW-Bypass-Auth";
    private const string BypassHeaderValue   = "local-dev-bypass";
    private const string CorrelationIdHeader = "X-WW-Correlation-Id";

    private readonly IWorkflowPolicyMatcher                      _policyMatcher;
    private readonly IRouteAuthorizationRegistry                 _routeRegistry;
    private readonly IHostEnvironment                            _hostEnvironment;
    private readonly AuditLogger                                 _auditLogger;
    private readonly ILogger<WorkflowAuthorizationMiddleware>    _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Initialises the middleware with policy matcher, route registry,
    /// host environment, audit logger and logger.
    /// </summary>
    public WorkflowAuthorizationMiddleware(
        IWorkflowPolicyMatcher                      policyMatcher,
        IRouteAuthorizationRegistry                 routeRegistry,
        IHostEnvironment                            hostEnvironment,
        AuditLogger                                 auditLogger,
        ILogger<WorkflowAuthorizationMiddleware>    logger)
    {
        _policyMatcher   = policyMatcher;
        _routeRegistry   = routeRegistry;
        _hostEnvironment = hostEnvironment;
        _auditLogger     = auditLogger;
        _logger          = logger;
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

        // (MWA-07 / OBS-05) Per-request correlation id — accept caller-supplied
        // value or generate a short one when missing.  Same value is included
        // in audit logs and 401/403 response headers + body.
        var correlationId = ResolveCorrelationId(request);

        // Public routes — no policy enforcement
        if (path.StartsWith(AuthConstants.PublicRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (!path.StartsWith(AuthConstants.SecureRoutePrefix,  StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith(AuthConstants.ServicesRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // ── apis.json bypass — discovery has its own permission filtering ─────
        if (path.EndsWith("/apis.json", StringComparison.OrdinalIgnoreCase))
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
            _logger.LogDebug(
                "⚠ DEV BYPASS: {Header} header detected on {Path} — skipping all policy checks. " +
                "This must NEVER happen in Production.",
                BypassHeader, path);
            await next(context);
            return;
        }

        // ── Require authenticated principal ───────────────────────────────────
        if (!context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var principalObj)
            || principalObj is not WorkflowClaimsPrincipal principal
            || principal.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("No authenticated principal for route {Path}", path);
            LogDiag(context, principalObj, path);

            _auditLogger.LogAuthOutcome(
                outcome: "401",
                caller:  "(anonymous)",
                workflow: string.Empty,
                path: path,
                reason: "no_authenticated_principal",
                correlationId: correlationId);

            await WriteErrorAsync(request, context, HttpStatusCode.Unauthorized,
                "unauthorized", "Authentication required.", path, correlationId);
            return;
        }

        var isSecure     = path.StartsWith(AuthConstants.SecureRoutePrefix,   StringComparison.OrdinalIgnoreCase);
        var workflowName = ExtractWorkflowName(path, isSecure);
        if (string.IsNullOrEmpty(workflowName))
        {
            await WriteErrorAsync(request, context, HttpStatusCode.BadRequest,
                "bad_request", "Could not determine workflow name from path.", path, correlationId);
            return;
        }

        // ── Resolve required permissions from route registry ──────────────────
        // Use the per-route [RequireWorkflowPermission] declaration when present;
        // fall back to View | Execute for any route without the attribute.
        var functionName = context.FunctionDefinition.Name;
        var requiredPermissions =
            _routeRegistry.GetRequiredPermissions(functionName)
            ?? (WorkflowPermission.View | WorkflowPermission.Execute);

        // ── Delegate to IWorkflowPolicyMatcher ────────────────────────────────
        var result = _policyMatcher.Evaluate(workflowName, principal, requiredPermissions);

        switch (result.Outcome)
        {
            case PolicyMatchOutcome.Allowed:
                _logger.LogDebug(
                    "Authorised '{Caller}' for workflow '{Workflow}'",
                    principal.CallerIdentity, workflowName);
                await next(context);
                return;

            case PolicyMatchOutcome.NoPolicyFound:
                // BYPASS_SECURE_CONFIG=true and config not effective — open-access mode.
                // Operator has explicitly opted in; log a prominent warning.
                _logger.LogDebug(
                    "OPEN-ACCESS MODE: secure.config not effective and BYPASS_SECURE_CONFIG=true. " +
                    "Allowing '{Caller}' for workflow '{Workflow}' without policy enforcement. " +
                    "This setting must NOT be used in production.",
                    principal.CallerIdentity, workflowName);
                await next(context);
                return;

            case PolicyMatchOutcome.ConfigMissingDeny:
                // secure.config absent or blank, bypass not set — deployment error.
                _logger.LogError(
                    "DEPLOYMENT ERROR: secure.config is absent or contains no permission entries. " +
                    "Returning 503 for workflow '{Workflow}' requested by '{Caller}'. " +
                    "Deploy a valid secure.config or set BYPASS_SECURE_CONFIG=true to enable open-access mode.",
                    workflowName, principal.CallerIdentity);

                _auditLogger.LogAuthOutcome(
                    outcome: "503",
                    caller:  principal.CallerIdentity,
                    workflow: workflowName,
                    path: path,
                    reason: "config_missing",
                    correlationId: correlationId);

                await WriteErrorAsync(request, context, HttpStatusCode.ServiceUnavailable,
                    "config_missing",
                    "Server configuration error: secure.config is absent or empty. " +
                    "Contact your administrator.",
                    path, correlationId,
                    new { workflow = workflowName });
                return;

            case PolicyMatchOutcome.Forbidden:
            default:
                _logger.LogWarning(
                    "Access denied for '{Caller}' on '{Workflow}': {Reason}",
                    principal.CallerIdentity, workflowName, result.DenialReason);

                // (OBS-03) Custom AppInsights metric — single counter increment
                // via structured logging keeps cost negligible.
                _logger.LogInformation(
                    "auth.policy.denied | Workflow={Workflow} | Caller={Caller} | CorrelationId={CorrelationId}",
                    workflowName, principal.CallerIdentity, correlationId);

                _auditLogger.LogAuthOutcome(
                    outcome: "403",
                    caller:  principal.CallerIdentity,
                    workflow: workflowName,
                    path: path,
                    reason: result.DenialReason ?? "policy_denied",
                    correlationId: correlationId);

                await WriteErrorAsync(request, context, HttpStatusCode.Forbidden,
                    "forbidden", result.DenialReason ?? "Insufficient permissions.", path, correlationId,
                    new { workflow = workflowName });
                return;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string ResolveCorrelationId(HttpRequestData request)
    {
        if (request.Headers.TryGetValues(CorrelationIdHeader, out var values))
        {
            var v = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(v)) return v!;
        }
        // Compact ID — short enough for HTTP headers, unique enough for tracing.
        return Guid.NewGuid().ToString("N")[..16];
    }

    /// <summary>
    /// Extracts a normalised workflow name from a <c>/secure/*</c> or
    /// <c>/services/*</c> path, preserving any folder prefix so that
    /// resource-scope lookups in <see cref="IWorkflowAuthPolicyLoader"/> resolve
    /// correctly for nested workflows (e.g. <c>folder/workflow</c>).
    ///
    /// <para>Examples:</para>
    /// <code>
    ///   /secure/MyWorkflow.json     → "myworkflow"
    ///   /secure/Folder/Sub.json     → "folder/sub"
    ///   /services/A/B/Flow.json     → "a/b/flow"
    /// </code>
    ///
    /// Marked <c>internal</c> for unit testing — no production caller exists
    /// outside this assembly.
    /// </summary>
    internal static string? ExtractWorkflowName(string path, bool isSecure)
    {
        var prefix    = isSecure ? AuthConstants.SecureRoutePrefix : AuthConstants.ServicesRoutePrefix;
        var remainder = path.Substring(prefix.Length).Split('?')[0];

        var segments = remainder.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
            return null;

        // Strip the recognised suffix (.json, .xml, .debug, .api) from the last segment only.
        var lastSegment    = Uri.UnescapeDataString(segments[^1]);
        var strippedLast   = Path.GetFileNameWithoutExtension(lastSegment);
        if (string.IsNullOrEmpty(strippedLast))
            return null;

        // Preserve all preceding folder segments to support resource-scope policy matching.
        if (segments.Length == 1)
            return strippedLast.ToLowerInvariant();

        var folderParts = segments[..^1]
            .Select(Uri.UnescapeDataString)
            .Select(s => s.ToLowerInvariant());

        return string.Join("/", folderParts.Append(strippedLast.ToLowerInvariant()));
    }

    private void LogDiag(FunctionContext context, object? principalObj, string path)
    {
        try
        {
            var hasPrincipalKey = context.Items.ContainsKey(AuthConstants.PrincipalContextKey);
            var principalType   = principalObj?.GetType().Name ?? "(null)";
            var isAuth          = (principalObj as WorkflowClaimsPrincipal)?.Identity?.IsAuthenticated;
            _logger.LogDebug(
                "[AuthDiag] 401 on {Path}: PrincipalKeyExists={KeyExists} PrincipalType={Type} IsAuthenticated={IsAuth}",
                path, hasPrincipalKey, principalType, isAuth);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[AuthDiag] Diagnostic logging error (non-fatal)");
        }
    }

    private static async Task WriteErrorAsync(
        HttpRequestData request,
        FunctionContext context,
        HttpStatusCode statusCode,
        string error,
        string message,
        string path,
        string correlationId,
        object? extra = null)
    {
        var body = new Dictionary<string, object>
        {
            ["error"] = error,
            ["message"] = message,
            ["path"] = path,
            ["correlationId"] = correlationId,
        };

        if (extra is not null)
        {
            foreach (var prop in extra.GetType().GetProperties())
                body[prop.Name.ToLowerInvariant()] = prop.GetValue(extra) ?? string.Empty;
        }

        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        response.Headers.Add(CorrelationIdHeader, correlationId);
        await response.WriteStringAsync(JsonSerializer.Serialize(body, JsonOptions));
        context.GetInvocationResult().Value = response;
    }
}
