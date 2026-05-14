/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Decorates an Azure Function method with the minimum
/// <see cref="WorkflowPermission"/> flags a caller must hold to invoke it.
///
/// <para>
/// The <see cref="RouteAuthorizationRegistry"/> reads these attributes at
/// startup and registers the requirements under the function's
/// <see cref="Microsoft.Azure.Functions.Worker.FunctionAttribute.Name"/>.
/// <see cref="Middleware.WorkflowAuthorizationMiddleware"/> then looks up the
/// requirement by function name before delegating to
/// <see cref="IWorkflowPolicyMatcher"/>, so individual route methods never
/// need to repeat the same permission-check boilerplate.
/// </para>
///
/// <example>
/// <code>
/// [Function("ExecuteSecureWorkflow")]
/// [RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
/// public async Task&lt;HttpResponseData&gt; ExecuteSecureWorkflow(...) { ... }
/// </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RequireWorkflowPermissionAttribute : Attribute
{
    /// <summary>
    /// Initialises the attribute with the required permission flags.
    /// </summary>
    /// <param name="permissions">
    /// Bitwise combination of <see cref="WorkflowPermission"/> values the
    /// caller must possess.  The default
    /// <c>View | Execute</c> is appropriate for workflow execution routes.
    /// </param>
    public RequireWorkflowPermissionAttribute(
        WorkflowPermission permissions = WorkflowPermission.View | WorkflowPermission.Execute)
    {
        Permissions = permissions;
    }

    /// <summary>The required permission flags for the decorated route.</summary>
    public WorkflowPermission Permissions { get; }
}
