/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth.Middleware;

/// <summary>
/// First middleware in the pipeline.
/// Handles the Azure Policy constraint that forces Easy Auth into RedirectToLoginPage mode.
/// Instead of allowing Easy Auth to redirect unauthenticated requests (which breaks API clients),
/// this middleware intercepts them and returns a clean 401 JSON response.
///
/// <list type="bullet">
///   <item><c>/public/*</c> routes are passed straight through with no token check.</item>
///   <item><c>/secure/*</c> routes without a token receive a 401 immediately.</item>
///   <item><c>/secure/*</c> routes with a valid token header are passed to the next middleware.</item>
/// </list>
/// </summary>
public sealed class EasyAuthRedirectMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<EasyAuthRedirectMiddleware> _logger;

    /// <summary>Initialises the middleware with a logger.</summary>
    public EasyAuthRedirectMiddleware(ILogger<EasyAuthRedirectMiddleware> logger)
        => _logger = logger;

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

        // Public routes bypass all auth checks
        if (path.StartsWith(AuthConstants.PublicRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Public route {Path} — bypassing auth check", path);
            await next(context);
            return;
        }

        // Only enforce on /secure/* routes
        if (!path.StartsWith(AuthConstants.SecureRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // apis.json discovery — always accessible without a token.
        // WorkflowAuthorizationMiddleware handles permission-filtering (empty list when no JWT).
        if (path.EndsWith("apis.json", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Check for Easy Auth principal header (set by Azure after token validation)
        var hasPrincipalHeader = request.Headers
            .TryGetValues(AuthConstants.ClientPrincipalHeader, out var principalValues)
            && principalValues.Any(v => !string.IsNullOrWhiteSpace(v));

        // Check for raw Bearer token in Authorization header
        var hasAuthHeader = request.Headers
            .TryGetValues("Authorization", out var authValues)
            && authValues.Any(v => v.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase));

        var isBrowser = LooksLikeBrowserNavigation(request);

        if (AuthConstants.VerboseAuthLogging)
        {
            try
            {
                _logger.LogInformation(
                    "[AuthDiag] EasyAuth check: Path={Path} HasPrincipalHeader={HasPrincipal} " +
                    "HasAuthHeader={HasAuth} IsBrowserNavigation={IsBrowser}",
                    path, hasPrincipalHeader, hasAuthHeader, isBrowser);
                if (AuthConstants.VerboseConsoleAuthLogging)
                    Console.WriteLine($"[AuthDiag] EasyAuth check: Path={path} HasPrincipalHeader={hasPrincipalHeader} HasAuthHeader={hasAuthHeader} IsBrowserNavigation={isBrowser}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AuthDiag] Diagnostic logging error (non-fatal)");
            }
        }

        if (!hasPrincipalHeader && !hasAuthHeader)
        {
            if (isBrowser)
            {
                var redirect = $"/.auth/login/aad?post_login_redirect_uri={Uri.EscapeDataString(path + request.Url.Query)}";

                if (AuthConstants.VerboseAuthLogging)
                {
                    try
                    {
                        _logger.LogInformation("[AuthDiag] Browser navigation detected — 302 redirect to {Redirect}", redirect);
                        if (AuthConstants.VerboseConsoleAuthLogging)
                            Console.WriteLine($"[AuthDiag] Browser navigation detected — 302 redirect to {redirect}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AuthDiag] Diagnostic logging error (non-fatal)");
                    }
                }

                var resp302 = request.CreateResponse(HttpStatusCode.Redirect);
                resp302.Headers.Add("Location", redirect);
                context.GetInvocationResult().Value = resp302;
                return;
            }

            _logger.LogWarning(
                "Unauthenticated request to protected route {Path} — returning 401", path);

            if (AuthConstants.VerboseAuthLogging)
            {
                try
                {
                    var accept = request.Headers.TryGetValues("Accept", out var av) ? av.FirstOrDefault() : "(none)";
                    var userAgent = request.Headers.TryGetValues("User-Agent", out var ua) ? ua.FirstOrDefault() : "(none)";
                    _logger.LogInformation(
                        "[AuthDiag] 401 details: Path={Path} Accept={Accept} UserAgent={UserAgent}",
                        path, accept, userAgent);
                    if (AuthConstants.VerboseConsoleAuthLogging)
                        Console.WriteLine($"[AuthDiag] 401 details: Path={path} Accept={accept} UserAgent={userAgent}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AuthDiag] Diagnostic logging error (non-fatal)");
                }
            }

            var response = request.CreateResponse(HttpStatusCode.Unauthorized);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add("WWW-Authenticate", "Bearer realm=\"warewolf\"");
            await response.WriteStringAsync(
                $"{{\"error\":\"unauthorized\",\"message\":\"A valid Bearer token is required.\",\"path\":\"{path}\"}}");

            context.GetInvocationResult().Value = response;
            return;
        }

        _logger.LogDebug("Authenticated request to {Path} — passing to next middleware", path);
        await next(context);
    }

    static bool LooksLikeBrowserNavigation(HttpRequestData req)
    {
        var accept = req.Headers.TryGetValues("Accept", out var a) ? a.FirstOrDefault() : null;
        var fetchMode = req.Headers.TryGetValues("Sec-Fetch-Mode", out var f) ? f.FirstOrDefault() : null;
        return (accept?.Contains("text/html", StringComparison.OrdinalIgnoreCase) ?? false)
            && !string.Equals(fetchMode, "cors", StringComparison.OrdinalIgnoreCase);
    }
}
