#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Warewolf.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Services.Security
{
    public abstract class AuthorizationServiceBase : DisposableObject, IAuthorizationService
    {
        private readonly IDirectoryEntryFactory _directoryEntryFactory;
        protected readonly ISecurityService _securityService;
        readonly bool _isLocalConnection;

        internal Func<bool> AreAdministratorsMembersOfWarewolfAdministrators;

        protected AuthorizationServiceBase(ISecurityService securityService, bool isLocalConnection)
        : this(new DirectoryEntryFactory(), securityService, isLocalConnection)
        {
        }
        protected AuthorizationServiceBase(IDirectoryEntryFactory directoryEntryFactory, ISecurityService securityService, bool isLocalConnection)
        {
            VerifyArgument.IsNotNull("SecurityService", securityService);
            VerifyArgument.IsNotNull("DirectoryEntryFactory", directoryEntryFactory);
            _securityService = securityService;
            _securityService.Read();
            _isLocalConnection = isLocalConnection;
            _securityService.PermissionsChanged += (s, e) => RaisePermissionsChanged();
            _securityService.PermissionsModified += (s, e) => OnPermissionsModified(e);
            _directoryEntryFactory = directoryEntryFactory;

            AreAdministratorsMembersOfWarewolfAdministrators = delegate
            {
                var adGroup = FindGroup(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                using (var ad = directoryEntryFactory.Create("WinNT://" + Environment.MachineName + ",computer"))
                {
                    ad.Children.SchemaFilter.Add("group");
                    foreach (IDirectoryEntry dChildEntry in ad.Children)
                    {
                        if (dChildEntry.Name != "Warewolf Administrators")
                        {
                            continue;
                        }
                        var members = dChildEntry.Invoke("Members");

                        if (members == null)
                        {
                            continue;
                        }
                        foreach (var member in (IEnumerable)members)
                        {
                            if (IsGroupNameAdministrators(member, adGroup))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            };
        }

        protected virtual bool IsGroupNameAdministrators<T>(T member, string adGroup)
        {
            using (IDirectoryEntry memberEntry = _directoryEntryFactory.Create(member))
            {
                if (memberEntry.Name == adGroup)
                {
                    return true;
                }
            }
            return false;
        }

        static string FindGroup(SecurityIdentifier searchSid)
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    var bytes = (byte[])dChildEntry.Properties["objectSid"].Value;
                    var sid = new SecurityIdentifier(bytes, 0).ToString();

                    if (sid == searchSid.ToString())
                    {
                        return dChildEntry.Name;
                    }
                }
            }
            throw new Exception(ErrorResource.CannotFindGroup);
        }
        public event EventHandler PermissionsChanged;
        EventHandler<PermissionsModifiedEventArgs> _permissionsModifiedHandler;
        readonly object _getPermissionsLock = new object();

        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified
        {
            add => _permissionsModifiedHandler += value;
            remove => _permissionsModifiedHandler -= value;
        }

        public virtual Permissions GetResourcePermissions(Guid resourceId)
        {
            var groupPermissions = GetGroupPermissions(Utilities.OrginalExecutingUser ?? Thread.CurrentPrincipal, resourceId.ToString()).ToList();
            var result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
        }
        public virtual List<WindowsGroupPermission> GetResourcePermissionsList(Guid resourceId)
        {
            var groupPermissions = GetGroupPermissions(Utilities.OrginalExecutingUser ?? Thread.CurrentPrincipal, resourceId.ToString()).ToList();
            return groupPermissions;
        }
        public List<WindowsGroupPermission> GetPermissions(IPrincipal user)
        {
            lock (_getPermissionsLock)
            {
                var serverPermissions = _securityService.Permissions.ToList();
                var resourcePermissions = serverPermissions.Where(p => IsInRole(user, p) && !p.IsServer).ToList();
                var serverPermissionsForUser = serverPermissions.Where(p => IsInRole(user, p) && (p.IsServer || p.ResourceID == Guid.Empty)).ToList();
                var groupPermissions = new List<WindowsGroupPermission>();
                groupPermissions.AddRange(resourcePermissions);
                groupPermissions.AddRange(serverPermissionsForUser);
                return groupPermissions;
            }
        }

        public virtual void Remove(Guid resourceId)
        {
            _securityService.Remove(resourceId);
        }

        public ISecurityService SecurityService => _securityService;

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);
        public abstract bool IsAuthorized(IAuthorizationRequest request);

        protected virtual void RaisePermissionsChanged()
        {
            PermissionsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPermissionsModified(PermissionsModifiedEventArgs e)
        {
            _permissionsModifiedHandler?.Invoke(this, e);
        }

        protected bool IsAuthorizedToConnect(IPrincipal principal) => IsAuthorized(AuthorizationContext.Any, principal, () => GetGroupPermissions(principal));

        public bool IsAuthorized(IPrincipal user, AuthorizationContext context, string resource) => IsAuthorized(context, user, () => GetGroupPermissions(user, resource));
        public bool IsAuthorized(IPrincipal user, AuthorizationContext context, IAuthorizationRequest request) => IsAuthorized(context, user, () => GetGroupPermissions(user, request));
        public bool IsAuthorized(IPrincipal user, AuthorizationContext context, IWarewolfResource resource) => IsAuthorized(context, user, () => GetGroupPermissions(user, resource));

        protected void DumpPermissionsOnError(IPrincipal principal)
        {

            Dev2Logger.Error(principal.Identity != null ? "PERM DUMP FOR [ " + principal.Identity.Name + " ]" : "PERM DUMP FOR [ NULL USER ]", GlobalConstants.WarewolfError);


            foreach (var perm in _securityService.Permissions)
            {
                Dev2Logger.Error("PERM -> " + perm.WindowsGroup, GlobalConstants.WarewolfError);
                Dev2Logger.Error("IS USER IN IT [ " + principal.IsInRole(perm.WindowsGroup) + " ]", GlobalConstants.WarewolfError);
            }
        }

        bool IsAuthorized(AuthorizationContext context, IPrincipal principal, Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions)
        {
            var contextPermissions = context.ToPermissions();
            var groupPermissions = getGroupPermissions?.Invoke();
            if (context == AuthorizationContext.Any)
            {
                groupPermissions = _securityService.Permissions.Where(p => IsInRole(principal, p)).ToList();
                return groupPermissions.Any(p => (p.Permissions & contextPermissions) != 0);
            }
            return groupPermissions.Any(p => (p.Permissions & contextPermissions) != 0);
        }

        protected virtual IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, Guid resourceId)
        {
            var matchedResources = _securityService.Permissions.Where(p => p.Matches(resourceId));
            var permissionsForResource = matchedResources.Where(p => !p.IsServer);
            permissionsForResource = permissionsForResource.Where(p => IsInRole(principal, p)).ToArray();

            var serverPermissionsNotOverridden = matchedResources.Where(permission =>
                permissionsForResource.All(groupPermission => groupPermission.ResourceID == Guid.Empty || (groupPermission.WindowsGroup != permission.WindowsGroup && groupPermission.ResourceID != permission.ResourceID)));

            var permissionsForServer = serverPermissionsNotOverridden
                .Where(permission => IsInRole(principal, permission)).ToList();

            permissionsForServer.AddRange(permissionsForResource);
            return permissionsForServer;
        }
        protected virtual IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            var matchedResources = _securityService.Permissions.Where(p => p.Matches(resource));
            var permissionsForResource = matchedResources.Where(p => !p.IsServer);
            permissionsForResource = permissionsForResource.Where(p => IsInRole(principal, p)).ToArray();

            var serverPermissionsNotOverridden = matchedResources.Where(permission =>
                permissionsForResource.All(groupPermission => groupPermission.ResourceID == Guid.Empty || (groupPermission.WindowsGroup != permission.WindowsGroup && groupPermission.ResourceID != permission.ResourceID)));

            var permissionsForServer = serverPermissionsNotOverridden
                .Where(permission => IsInRole(principal, permission)).ToList();

            permissionsForServer.AddRange(permissionsForResource);
            return permissionsForServer;
        }

        protected virtual IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, IAuthorizationRequest request)
        {
            var matchedResources = _securityService.Permissions.Where(p => p.Matches(request));
            var permissionsForResource = matchedResources.Where(p => !p.IsServer);
            permissionsForResource = permissionsForResource.Where(p => IsInRole(principal, p)).ToArray();

            var serverPermissionsNotOverridden = matchedResources.Where(permission =>
                permissionsForResource.All(groupPermission => groupPermission.ResourceID == Guid.Empty || (groupPermission.WindowsGroup != permission.WindowsGroup && groupPermission.ResourceID != permission.ResourceID)));

            var permissionsForServer = serverPermissionsNotOverridden
                .Where(permission => IsInRole(principal, permission)).ToList();

            permissionsForServer.AddRange(permissionsForResource);
            return permissionsForServer;
        }
        protected virtual IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, IWarewolfResource resource)
        {
            var matchedResources = _securityService.Permissions.Where(p => p.Matches(resource));
            var permissionsForResource = matchedResources.Where(p => !p.IsServer);
            permissionsForResource = permissionsForResource.Where(p => IsInRole(principal, p)).ToArray();

            var serverPermissionsNotOverridden = matchedResources.Where(permission =>
                permissionsForResource.All(groupPermission => groupPermission.ResourceID == Guid.Empty || (groupPermission.WindowsGroup != permission.WindowsGroup && groupPermission.ResourceID != permission.ResourceID)));

            var permissionsForServer = serverPermissionsNotOverridden
                .Where(permission => IsInRole(principal, permission)).ToList();

            permissionsForServer.AddRange(permissionsForResource);
            return permissionsForServer;
        }

        bool IsInRole(IPrincipal principal, WindowsGroupPermission p)
        {
            var isInRole = false;
            if (principal == null)
            {
                return p.IsBuiltInGuestsForExecution;
            }
            try
            {
                var windowsGroup = p.WindowsGroup;
                if (windowsGroup == WindowsGroupPermission.BuiltInAdministratorsText)
                {
                    var principleName = principal.Identity.Name;
                    if (!string.IsNullOrEmpty(principleName))
                    {
                        return TryIsInRole(principal, windowsGroup);
                    }
                }
                else
                {
                    isInRole = principal.IsInRole(windowsGroup);
                }
                if (!isInRole && principal is System.Security.Claims.ClaimsPrincipal claimsPrincipal)
                {
                    try
                    {
                        isInRole = claimsPrincipal.GetUserGroups().Any(groupName => groupName == windowsGroup);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Warn($"failed using group override from ClaimsPrinciple: {e.Message}", GlobalConstants.WarewolfWarn);
                    }
                }
            }
            catch (ObjectDisposedException e)
            {
                Dev2Logger.Warn(e.Message, GlobalConstants.WarewolfWarn);
                throw;
            }
            catch (Exception e)
            {
                Dev2Logger.Warn(e.Message, GlobalConstants.WarewolfWarn);
            }

            return isInRole || p.IsBuiltInGuestsForExecution;
        }

        bool TryIsInRole(IPrincipal principal, string windowsGroup)
        {
            bool isInRole = principal.IsInRole(windowsGroup);
            if (!isInRole)
            {
                isInRole = DoFallBackCheck(principal);
            }
            if (!isInRole)
            {
                isInRole = IsInRole(principal, windowsGroup);
            }
            if (!isInRole)
            {
                isInRole = DoFallBackCheck(principal);
            }
            return isInRole;
        }

        static bool IsInRole(IPrincipal principal, string windowsGroup)
        {
            bool isInRole;
            var sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            var windowsPrincipal = principal as WindowsPrincipal;
            var windowsIdentity = principal.Identity as WindowsIdentity;
            if (windowsPrincipal != null)
            {
                isInRole = IsInRole(windowsGroup, sid, windowsPrincipal, windowsIdentity);
            }
            else
            {
                isInRole = principal.IsInRole(sid.Value);
            }

            return isInRole;
        }

        static bool IsInRole(string windowsGroup, SecurityIdentifier sid, WindowsPrincipal windowsPrincipal, WindowsIdentity windowsIdentity)
        {
            bool isInRole = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) || windowsPrincipal.IsInRole("BUILTIN\\Administrators") || windowsPrincipal.IsInRole(sid);
            if (windowsIdentity != null && !isInRole && windowsIdentity.Groups != null)
            {
                isInRole = windowsIdentity.Groups.Any(reference =>
                {
                    if (reference.Value == sid.Value)
                    {
                        return true;
                    }
                    try
                    {
                        var identityReference = reference.Translate(typeof(NTAccount));
                        if (identityReference != null)
                        {
                            return identityReference.Value == windowsGroup;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    return false;
                });
            }

            return isInRole;
        }

        bool DoFallBackCheck(IPrincipal principal)
        {
            var username = principal?.Identity?.Name;
            if (username == null)
            {
                return false;
            }
            var theUser = username;
            var domainChar = username.IndexOf("\\", StringComparison.Ordinal);
            if (domainChar >= 0)
            {
                theUser = username.Substring(domainChar + 1);
            }
            var windowsBuiltInRole = WindowsBuiltInRole.Administrator.ToString();
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name != WindowsGroupPermission.BuiltInAdministratorsText && dChildEntry.Name != windowsBuiltInRole && dChildEntry.Name != "Administrators" && dChildEntry.Name != "BUILTIN\\Administrators")
                    {
                        continue;
                    }
                    var members = dChildEntry.Invoke("Members");

                    if (members == null)
                    {
                        continue;
                    }
                    foreach (var member in (IEnumerable)members)
                    {
                        using (var memberEntry = new DirectoryEntry(member))
                        {
                            if (memberEntry.Name == theUser)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal)
        {
            var groupPermissions = _securityService.Permissions.Where(p => IsInRole(principal, p)).ToList();
            FilterAdminGroupForRemote(groupPermissions);
            return groupPermissions;
        }

        void FilterAdminGroupForRemote(List<WindowsGroupPermission> groupPermissions)
        {
            if (!_isLocalConnection)
            {
                var adminGroup = groupPermissions.FirstOrDefault(gr => gr.WindowsGroup.Equals(WindowsGroupPermission.BuiltInAdministratorsText));
                if (adminGroup != null)
                {
                    groupPermissions.Remove(adminGroup);
                }
            }
        }
    }
}
