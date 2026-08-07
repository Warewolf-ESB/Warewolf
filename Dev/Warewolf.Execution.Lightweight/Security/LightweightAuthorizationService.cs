/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Warewolf.Data;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Authorization for nested sub-workflow invocation (<c>DsfActivity</c>) inside the
    /// Lightweight engine.
    ///
    /// The Lightweight engine authorizes the caller once, at the HTTP/claims layer
    /// (<c>WorkflowAuthPolicyLoader</c> + <c>PermissionChecker</c>), before any workflow
    /// execution begins — see docs/Part1-Architecture.md "Authorization model". There is no
    /// Windows-groups <c>secure.config</c> in this engine. <c>DsfActivity</c>'s default
    /// <c>AuthorizationService</c> (<c>ServerAuthorizationService.Instance</c>) is the Server's
    /// legacy Windows-groups check: it reads a machine-global <c>secure.config</c> that the
    /// Lightweight engine never creates, so it can never grant Execute to a persisted principal
    /// (e.g. a resumed job's restored user) — it would deny every nested sub-workflow invocation
    /// in every Lightweight deployment.
    ///
    /// <see cref="WorkflowExecutor.ExecuteActivityChain"/> (the single execution entry point
    /// shared by normal and resumed/continuation execution) wires this service onto every
    /// <c>DsfActivity</c> node so nested sub-workflow calls rely on the already-completed
    /// top-level authorization instead of the legacy Windows-groups check.
    /// </summary>
    internal sealed class LightweightAuthorizationService : IAuthorizationService
    {
        internal static readonly LightweightAuthorizationService Instance = new LightweightAuthorizationService();

        LightweightAuthorizationService()
        {
        }

        public event EventHandler PermissionsChanged { add { } remove { } }
        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified { add { } remove { } }

        public ISecurityService SecurityService =>
            throw new NotSupportedException(
                $"{nameof(LightweightAuthorizationService)} has no Windows-groups security store; " +
                "the Lightweight engine authorizes at the HTTP/claims layer, not per nested sub-workflow invocation.");

        public bool IsAuthorized(AuthorizationContext context, Guid resourceId) => true;

        public bool IsAuthorized(AuthorizationContext context, IWarewolfResource resource) => true;

        public bool IsAuthorized(IPrincipal user, AuthorizationContext context, Guid resourceId) => true;

        public bool IsAuthorized(IPrincipal user, AuthorizationContext context, IWarewolfResource resource) => true;

        public bool IsAuthorized(IAuthorizationRequest request) => true;

        public Permissions GetResourcePermissions(Guid resourceId) => Permissions.Administrator;

        public void Remove(Guid resourceId)
        {
        }

        public List<WindowsGroupPermission> GetPermissions(IPrincipal user) => new List<WindowsGroupPermission>();

        public List<WindowsGroupPermission> GetResourcePermissionsList(Guid resourceResourceId) => new List<WindowsGroupPermission>();
    }
}
