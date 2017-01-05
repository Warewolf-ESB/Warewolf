/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Security;
using Warewolf.Resource.Errors;

namespace Dev2.Services.Security
{


    public abstract class AuthorizationServiceBase : DisposableObject, IAuthorizationService
    {
        // ReSharper disable once InconsistentNaming
        protected readonly ISecurityService _securityService;
        readonly bool _isLocalConnection;

        public Func<bool> AreAdministratorsMembersOfWarewolfAdministrators;

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
                        if(dChildEntry.Name == "Warewolf Administrators")
                        {
                            // Now check group membership ;)
                            var members = dChildEntry.Invoke("Members");

                            if(members != null)
                            {
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
                    }
                }

                return false;
            };

        }

        private static string FindGroup(SecurityIdentifier searchSid)
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    var bytes = (byte[])dChildEntry.Properties["objectSid"].Value;
                    var sid = new SecurityIdentifier(bytes, 0).ToString();

                    if(sid == searchSid.ToString())
                    {
                        return dChildEntry.Name;
                    }
                }
            }
            throw new Exception(ErrorResource.CannotFindGroup);
        }
        public event EventHandler PermissionsChanged;
        private EventHandler<PermissionsModifiedEventArgs> _permissionsModifedHandler;
        readonly object _getPermissionsLock = new object();
        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified
        {
            add
            {
               _permissionsModifedHandler += value;
            }
            remove
            {
                // ReSharper disable DelegateSubtraction
                _permissionsModifedHandler -= value;
                // ReSharper restore DelegateSubtraction
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

        protected bool IsAuthorizedToConnect(IPrincipal principal)
        {
            return IsAuthorized(AuthorizationContext.Any, principal, () => GetGroupPermissions(principal));
        }

        public bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            return IsAuthorized(context,principal, () => GetGroupPermissions(principal, resource));
        }

        protected void DumpPermissionsOnError(IPrincipal principal)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if(principal.Identity != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                Dev2Logger.Error("PERM DUMP FOR [ " + principal.Identity.Name + " ]");
            }
            else
            // ReSharper disable HeuristicUnreachableCode
            {
                Dev2Logger.Error("PERM DUMP FOR [ NULL USER ]");
            }
            // ReSharper restore HeuristicUnreachableCode

            foreach(var perm in _securityService.Permissions)
            {
                Dev2Logger.Error("PERM -> " + perm.WindowsGroup);
                Dev2Logger.Error("IS USER IN IT [ " + principal.IsInRole(perm.WindowsGroup) + " ]");
            }
        }

        bool IsAuthorized(AuthorizationContext context,IPrincipal principal, Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions)
        {
            var contextPermissions = context.ToPermissions();
            var groupPermissions = getGroupPermissions();
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
            // ReSharper disable LoopCanBeConvertedToQuery
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
            // ReSharper restore LoopCanBeConvertedToQuery

            groupPermissions.AddRange(resourcePermissions);
            //FilterAdminGroupForRemote(groupPermissions);
            return groupPermissions;
        }

        private bool IsInRole(IPrincipal principal, WindowsGroupPermission p)
        {
            var isInRole = false;
            if(principal == null)
            {
                return p.IsBuiltInGuestsForExecution;
            }
            try
            {
                // If its our admin group ( Warewolf ), we need to check membership differently 
                // Prefixing with computer name does not work with Warewolf and IsInRole
                // Plain does not work with IsInRole
                // Hence this conditional check and divert
                var windowsGroup = p.WindowsGroup;
                if(windowsGroup == WindowsGroupPermission.BuiltInAdministratorsText)
                {
                    // We need to get the group as it is local then look for principle's membership
                    var principleName = principal.Identity.Name;
                    if(!string.IsNullOrEmpty(principleName))
                    {
                        // Examine if BuiltIn\Administrators is still present as a Member
                        // Then inspect BuiltIn\Administrators

                        // Examine group for this member ;)  
                        isInRole = principal.IsInRole(windowsGroup);
                        if (!isInRole)
                        {
                            isInRole = DoFallBackCheck(principal);
                        }
                        // if that fails, check Administrators group membership in the Warewolf Administrators group
                        if(!isInRole)
                        {
                            var sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                            var windowsPrincipal = principal as WindowsPrincipal;
                            var windowsIdentity = principal.Identity as WindowsIdentity;
                            if(windowsPrincipal != null)
                            {
                                isInRole = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) || windowsPrincipal.IsInRole("BUILTIN\\Administrators") || windowsPrincipal.IsInRole(sid);
                                if(windowsIdentity != null && !isInRole)
                                {
                                    if(windowsIdentity.Groups != null)
                                    {
                                        isInRole = windowsIdentity.Groups.Any(reference =>
                                        {
                                            if(reference.Value == sid.Value)
                                            {
                                                return true;
                                            }
                                            try
                                            {
                                                var identityReference = reference.Translate(typeof(NTAccount));
                                                if(identityReference != null)
                                                {
                                                    return identityReference.Value == windowsGroup;
                                                }
                                            }
                                            catch(Exception)
                                            {
                                                return false;
                                                //Complete failure
                                            }
                                            return false;
                                        });
                                    }
                                }
                            }
                            else
                            {

                                if(AreAdministratorsMembersOfWarewolfAdministrators.Invoke())
                                {
                                    // Check user's administrator membership
                                    isInRole = principal.IsInRole(sid.Value);
                                }

                                //Check regardless. Not installing the software can create a situation where the "Administrators" group is not part of Warewolf
                                isInRole = principal.IsInRole(sid.Value);
                            }
                        }
                        if (!isInRole)
                        {
                            isInRole = DoFallBackCheck(principal);
                        }
                        return isInRole;
                    }
                }
                else
                {
                    // THIS TRY-CATCH IS HERE TO AVOID THE EXPLORER NOT LOADING ANYTHING WHEN THE DOMAIN CANNOT BE CONTACTED!
                    isInRole = principal.IsInRole(windowsGroup);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
            
            return isInRole || p.IsBuiltInGuestsForExecution;
        }

        bool DoFallBackCheck(IPrincipal principal)
        {
            var username = principal?.Identity?.Name;
            if (username != null)
            {
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

                        if (dChildEntry.Name == WindowsGroupPermission.BuiltInAdministratorsText || dChildEntry.Name == windowsBuiltInRole || dChildEntry.Name=="Administrators" || dChildEntry.Name=="BUILTIN\\Administrators")
                        {
                            // Now check group membership ;)
                            var members = dChildEntry.Invoke("Members");

                            if (members != null)
                            {
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

        private void FilterAdminGroupForRemote(List<WindowsGroupPermission> groupPermissions)
        {
            if(!_isLocalConnection)
            {
                var adminGroup = groupPermissions.FirstOrDefault(gr => gr.WindowsGroup.Equals(WindowsGroupPermission.BuiltInAdministratorsText));
                if(adminGroup != null)
                {
                    groupPermissions.Remove(adminGroup);
                }
            }
        }
    }
}
