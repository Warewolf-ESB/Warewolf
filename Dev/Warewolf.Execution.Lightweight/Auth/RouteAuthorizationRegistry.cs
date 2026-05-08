/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Builds the route-permission registry at startup by reflecting over every
/// public method in the supplied function types that carries both a
/// <see cref="FunctionAttribute"/> and a
/// <see cref="RequireWorkflowPermissionAttribute"/>.
///
/// <para>
/// The registry is keyed by the function name declared in
/// <see cref="FunctionAttribute.Name"/> — the same name that the Azure Functions
/// worker stores in
/// <see cref="Microsoft.Azure.Functions.Worker.FunctionContext.FunctionDefinition"/>.Name
/// so middleware can look it up without touching HTTP request state.
/// </para>
///
/// <example>
/// Registration at startup (in <c>ServiceCollectionExtensions</c>):
/// <code>
/// services.AddSingleton&lt;IRouteAuthorizationRegistry&gt;(
///     _ => RouteAuthorizationRegistry.BuildFrom(typeof(WorkflowHttpFunction)));
/// </code>
/// </example>
/// </summary>
public sealed class RouteAuthorizationRegistry : IRouteAuthorizationRegistry
{
    private readonly IReadOnlyDictionary<string, WorkflowPermission> _map;

    private RouteAuthorizationRegistry(IReadOnlyDictionary<string, WorkflowPermission> map)
        => _map = map;

    /// <summary>
    /// Reflects over all public methods on <paramref name="functionTypes"/> and
    /// builds a registry from the <see cref="FunctionAttribute.Name"/> →
    /// <see cref="RequireWorkflowPermissionAttribute.Permissions"/> mapping.
    /// </summary>
    public static RouteAuthorizationRegistry BuildFrom(params Type[] functionTypes)
    {
        var map = new Dictionary<string, WorkflowPermission>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in functionTypes)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var funcAttr = method.GetCustomAttribute<FunctionAttribute>();
                var permAttr = method.GetCustomAttribute<RequireWorkflowPermissionAttribute>();

                if (funcAttr is not null && permAttr is not null)
                    map[funcAttr.Name] = permAttr.Permissions;
            }
        }

        return new RouteAuthorizationRegistry(map);
    }

    /// <inheritdoc/>
    public WorkflowPermission? GetRequiredPermissions(string functionName)
        => _map.TryGetValue(functionName, out var perms) ? perms : null;
}
