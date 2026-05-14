/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Azure.Functions.Worker.Http;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Strategy abstraction for building a <see cref="WorkflowClaimsPrincipal"/> from
/// an inbound HTTP request.  Implementations are tried in registration order
/// by <see cref="Middleware.ClaimsPrincipalBuilderMiddleware"/>; the first parser
/// that returns an authenticated principal wins.
/// </summary>
public interface IPrincipalParser
{
    /// <summary>
    /// Friendly name used in diagnostic logging (e.g. <c>"EasyAuth"</c>, <c>"Bearer"</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Attempts to build a <see cref="WorkflowClaimsPrincipal"/> from the request.
    /// Returns <c>null</c> when this parser does not apply (e.g. its source header
    /// is absent) or when validation fails.  Implementations <b>must not throw</b> —
    /// any exception is swallowed and treated as "parser does not apply".
    /// </summary>
    Task<WorkflowClaimsPrincipal?> TryParseAsync(HttpRequestData request, CancellationToken cancellationToken);
}
