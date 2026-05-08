/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Provides the <see cref="WorkflowPermission"/> requirements for a named
/// Azure Function, as declared by <see cref="RequireWorkflowPermissionAttribute"/>.
///
/// Consumed by <see cref="Middleware.WorkflowAuthorizationMiddleware"/> so that
/// each route method does not need to repeat the same permission-check boilerplate.
/// </summary>
public interface IRouteAuthorizationRegistry
{
    /// <summary>
    /// Returns the required permissions for the function identified by
    /// <paramref name="functionName"/>, or <c>null</c> when no requirement
    /// was registered (meaning the route does not require permission enforcement
    /// beyond the standard authenticated / unauthenticated split).
    /// </summary>
    WorkflowPermission? GetRequiredPermissions(string functionName);
}
