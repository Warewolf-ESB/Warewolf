/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;

namespace Dev2.Services.Security
{
    public abstract class AuthorizationServiceBase : DisposableObject, IAuthorizationService
    {

        protected readonly ISecurityService _securityService;
        readonly bool _isLocalConnection;

        internal Func<bool> AreAdministratorsMembersOfWarewolfAdministrators;

        protected AuthorizationServiceBase(ISecurityService securityService, bool isLocalConnection)
        {
            VerifyArgument.IsNotNull("SecurityService", securityService);
            _securityService = securityService;
            _securityService.Read();
            _isLocalConnection = isLocalConnection;
            _securityService.PermissionsChanged += (s, e) => RaisePermissionsChanged();
            _securityService.PermissionsModified += (s, e) => OnPermissionsModified(e);

            AreAdministratorsMembersOfWarewolfAdministrators = delegate
            {
                var adGroup = FindGroup(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    ad.Children.SchemaFilter.Add("group");
                    foreach(DirectoryEntry dChildEntry in ad.Children)
                    {
                        if (dChildEntry.Name != "Warewolf Administrators")
                        {
                            return false;
                        }
                        // Now check group membership ;)
                        var members = dChildEntry.Invoke("Members");

                        if (members == null)
                        {
                            return false;
                        }
                        foreach(var member in (IEnumerable)members)
                        {
                            using(DirectoryEntry memberEntry = new DirectoryEntry(member))
                            {
                                if(memberEntry.Name == adGroup)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            };

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
        EventHandler<PermissionsModifiedEventArgs> _permissionsModifedHandler;
        readonly object _getPermissionsLock = new object();
        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified
        {
            add
            {
               _permissionsModifedHandler += value;
            }
            remove
            {
                
                _permissionsModifedHandler -= value;
                
            }
        }

        public virtual Permissions GetResourcePermissions(Guid resourceId)
        {
            var groupPermissions = GetGroupPermissions(Utilities.OrginalExecutingUser ?? Thread.CurrentPrincipal, resourceId.ToString()).ToList();
            var result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
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

        public Func<bool> AreAdministratorsMembersOfWarewolfAdministrators1 => AreAdministratorsMembersOfWarewolfAdministrators;

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);
        public abstract bool IsAuthorized(IAuthorizationRequest request);

        protected virtual void RaisePermissionsChanged()
        {
            PermissionsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPermissionsModified(PermissionsModifiedEventArgs e)
        {
            _permissionsModifedHandler?.Invoke(this, e);
        }

        protected bool IsAuthorizedToConnect(IPrincipal principal) => IsAuthorized(AuthorizationContext.Any, principal, () => GetGroupPermissions(principal));

        public bool IsAuthorized(IPrincipal user, AuthorizationContext context, string resource) => IsAuthorized(context, user, () => GetGroupPermissions(user, resource));

        protected void DumpPermissionsOnError(IPrincipal principal)
        {

            Dev2Logger.Error(principal.Identity != null ? "PERM DUMP FOR [ " + principal.Identity.Name + " ]" : "PERM DUMP FOR [ NULL USER ]", GlobalConstants.WarewolfError);


            foreach (var perm in _securityService.Permissions)
            {
                Dev2Logger.Error("PERM -> " + perm.WindowsGroup, GlobalConstants.WarewolfError);
                Dev2Logger.Error("IS USER IN IT [ " + principal.IsInRole(perm.WindowsGroup) + " ]", GlobalConstants.WarewolfError);
            }
        }

        bool IsAuthorized(AuthorizationContext context,IPrincipal principal, Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions)
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

        protected virtual IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            var serverPermissions = _securityService.Permissions;
            var resourcePermissions = serverPermissions.Where(p => IsInRole(principal, p) && p.Matches(resource) && !p.IsServer).ToList();
            var groupPermissions = new List<WindowsGroupPermission>();
            
            foreach(var permission in serverPermissions)
            {
                if(resourcePermissions.Any(groupPermission => groupPermission.WindowsGroup == permission.WindowsGroup))
                {
                    continue;
                }
                if(IsInRole(principal, permission) && permission.Matches(resource))
                {
                    groupPermissions.Add(permission);
                }
            }
            

            groupPermissions.AddRange(resourcePermissions);
            return groupPermissions;
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
            }
            catch (Exception e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
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
                        return false;
                    }
                    var members = dChildEntry.Invoke("Members");

                    if (members == null)
                    {
                        return false;
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
