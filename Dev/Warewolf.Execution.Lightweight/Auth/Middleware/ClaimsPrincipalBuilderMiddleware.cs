/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth.Middleware;

/// <summary>
/// Second middleware in the pipeline.
/// Builds a <see cref="WorkflowClaimsPrincipal"/> from the incoming request by
/// delegating to an ordered chain of <see cref="IPrincipalParser"/> strategies.
/// The first parser to return an authenticated principal wins; if none apply,
/// an anonymous principal is stored.
///
/// Default registration order (see <c>ServiceCollectionExtensions.AddAuthParsers</c>):
/// <list type="number">
///   <item><see cref="Parsers.EasyAuthPrincipalParser"/> — <c>X-MS-CLIENT-PRINCIPAL</c> header.</item>
///   <item><see cref="Parsers.BearerTokenPrincipalParser"/> — RS256-validated <c>Authorization: Bearer</c> JWT.</item>
/// </list>
/// </summary>
public sealed class ClaimsPrincipalBuilderMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IReadOnlyList<IPrincipalParser> _parsers;
    private readonly ILogger<ClaimsPrincipalBuilderMiddleware> _logger;

    /// <summary>Initialises the middleware with the parser chain and a logger.</summary>
    public ClaimsPrincipalBuilderMiddleware(
        IEnumerable<IPrincipalParser> parsers,
        ILogger<ClaimsPrincipalBuilderMiddleware> logger)
    {
        _parsers = parsers.ToList().AsReadOnly();
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is not null)
        {
            var principal = await BuildPrincipalAsync(request, context.CancellationToken);
            context.Items[AuthConstants.PrincipalContextKey] = principal;

            _logger.LogTrace(
                "Principal built: User={User} Authenticated={Auth} Groups=[{Groups}] Permissions={Perms}",
                principal.UserName,
                principal.Identity?.IsAuthenticated,
                string.Join(", ", principal.Groups),
                principal.Permissions);

            LogVerboseDiagnostics(principal);
        }

        await next(context);
    }

    // ── Principal resolution ──────────────────────────────────────────────────

    private async Task<WorkflowClaimsPrincipal> BuildPrincipalAsync(
        HttpRequestData request,
        CancellationToken cancellationToken)
    {
        foreach (var parser in _parsers)
        {
            WorkflowClaimsPrincipal? principal;
            try
            {
                principal = await parser.TryParseAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex,
                    "Principal parser {Parser} threw — yielding to next strategy", parser.Name);
                continue;
            }

            if (principal?.Identity?.IsAuthenticated == true)
            {
                return principal;
            }
        }

        _logger.LogDebug("[AuthDiag] No parser produced an authenticated principal — returning Anonymous");

        return WorkflowClaimsPrincipal.Anonymous();
    }

    // ── Diagnostics ───────────────────────────────────────────────────────────

    private void LogVerboseDiagnostics(WorkflowClaimsPrincipal principal)
    {
        try
        {
            var identity = principal.Identity as ClaimsIdentity;
            var roles = string.Join(", ", principal.Identities
                .SelectMany(i => i.Claims)
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value));
            var groups = string.Join(", ", principal.Groups);
            var perms = string.Join(", ", principal.Permissions);
            var claimCount = identity?.Claims.Count() ?? 0;

            _logger.LogDebug(
                "[AuthDiag] ClaimsPrincipal: User={User} AuthType={AuthType} IsAuthenticated={IsAuth} " +
                "Roles=[{Roles}] Groups=[{Groups}] Permissions=[{Perms}] ClaimCount={ClaimCount}",
                principal.UserName,
                identity?.AuthenticationType ?? "(none)",
                identity?.IsAuthenticated ?? false,
                roles, groups, perms, claimCount);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[AuthDiag] Failed to log principal details — diagnostic logging error (non-fatal)");
        }
    }
}
