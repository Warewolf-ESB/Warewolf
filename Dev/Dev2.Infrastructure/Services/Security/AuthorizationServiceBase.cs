/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Security;

namespace Dev2.Services.Security
{
    public abstract class AuthorizationServiceBase : DisposableObject, IAuthorizationService
    {
        // ReSharper disable once InconsistentNaming
        private readonly object _getPermissionsLock = new object();
        private readonly bool _isLocalConnection;
        protected readonly ISecurityService _securityService;

        public Func<bool> AreAdministratorsMembersOfWarewolfAdministrators;
        private EventHandler<PermissionsModifiedEventArgs> _permissionsModifedHandler;

        protected AuthorizationServiceBase(ISecurityService securityService, bool isLocalConnection)
        {
            VerifyArgument.IsNotNull("SecurityService", securityService);
            _securityService = securityService;
            _securityService.Read();
            _isLocalConnection = isLocalConnection;
            _securityService.PermissionsChanged += (s, e) => RaisePermissionsChanged();
            _securityService.PermissionsModified += (s, e) => OnPermissionsModified(e);

            // set up the func for normal use ;)
            AreAdministratorsMembersOfWarewolfAdministrators = delegate
            {
                string adGroup = FindGroup(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    ad.Children.SchemaFilter.Add("group");
                    foreach (DirectoryEntry dChildEntry in ad.Children)
                    {
                        if (dChildEntry.Name == "Warewolf Administrators")
                        {
                            // Now check group membership ;)
                            object members = dChildEntry.Invoke("Members");

                            if (members != null)
                            {
                                foreach (object member in (IEnumerable) members)
                                {
                                    using (var memberEntry = new DirectoryEntry(member))
                                    {
                                        if (memberEntry.Name == adGroup)
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

        public event EventHandler PermissionsChanged;

        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified
        {
            add { _permissionsModifedHandler += value; }
            remove
            {
                // ReSharper disable DelegateSubtraction
                _permissionsModifedHandler -= value;
                // ReSharper restore DelegateSubtraction
            }
        }

        public virtual Permissions GetResourcePermissions(Guid resourceId)
        {
            List<WindowsGroupPermission> groupPermissions =
                GetGroupPermissions(ClaimsPrincipal.Current, resourceId.ToString()).ToList();
            Permissions result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
        }

        public List<WindowsGroupPermission> GetPermissions(IPrincipal user)
        {
            lock (_getPermissionsLock)
            {
                List<WindowsGroupPermission> serverPermissions = _securityService.Permissions.ToList();
                List<WindowsGroupPermission> resourcePermissions =
                    serverPermissions.Where(p => IsInRole(user, p) && !p.IsServer).ToList();
                List<WindowsGroupPermission> serverPermissionsForUser =
                    serverPermissions.Where(p => IsInRole(user, p) && (p.IsServer || p.ResourceID == Guid.Empty))
                        .ToList();
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

        public ISecurityService SecurityService
        {
            get { return _securityService; }
        }

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);
        public abstract bool IsAuthorized(IAuthorizationRequest request);

        public bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            return IsAuthorized(context, () => GetGroupPermissions(principal, resource));
        }

        public string JsonPermissions()
        {
            var result = new StringBuilder();
            result.AppendLine("{{");
            foreach (WindowsGroupPermission permission in _securityService.Permissions)
            {
                result.AppendFormat("\t{{ ResourceID:{0},\tWindowsGroup:{1},\tPermissions:{2} }}", permission.ResourceID,
                    permission.WindowsGroup, permission.Permissions);
                result.AppendLine();
            }
            result.AppendLine("}}");
            return result.ToString();
        }

        public static string FindGroup(SecurityIdentifier searchSid)
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    var bytes = (byte[]) dChildEntry.Properties["objectSid"].Value;
                    string sid = new SecurityIdentifier(bytes, 0).ToString();

                    if (sid == searchSid.ToString())
                    {
                        return dChildEntry.Name;
                    }
                }
            }
            throw new Exception("Cannot find group");
        }

        protected virtual void RaisePermissionsChanged()
        {
            if (PermissionsChanged != null)
            {
                PermissionsChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnPermissionsModified(PermissionsModifiedEventArgs e)
        {
            if (_permissionsModifedHandler != null)
            {
                _permissionsModifedHandler(this, e);
            }
        }

        protected bool IsAuthorizedToConnect(IPrincipal principal)
        {
            return IsAuthorized(AuthorizationContext.Any, () => GetGroupPermissions(principal));
        }

        public void DumpPermissionsOnError(IPrincipal principal)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (principal.Identity != null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                Dev2Logger.Log.Error("PERM DUMP FOR [ " + principal.Identity.Name + " ]");
            }
            else
                // ReSharper disable HeuristicUnreachableCode
            {
                Dev2Logger.Log.Error("PERM DUMP FOR [ NULL USER ]");
            }
            // ReSharper restore HeuristicUnreachableCode

            foreach (WindowsGroupPermission perm in _securityService.Permissions)
            {
                Dev2Logger.Log.Error("SERVER PERM -> " + perm.WindowsGroup);
                Dev2Logger.Log.Error("IS USER IN IT [ " + principal.IsInRole(perm.WindowsGroup) + " ]");
            }
        }

        private static bool IsAuthorized(AuthorizationContext context,
            Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions)
        {
            Permissions contextPermissions = context.ToPermissions();
            IEnumerable<WindowsGroupPermission> groupPermissions = getGroupPermissions();
            return groupPermissions.Any(p => (p.Permissions & contextPermissions) != 0);
        }

        protected virtual IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            IReadOnlyList<WindowsGroupPermission> serverPermissions = _securityService.Permissions;
            List<WindowsGroupPermission> resourcePermissions =
                serverPermissions.Where(p => IsInRole(principal, p) && p.Matches(resource) && !p.IsServer).ToList();

            var groupPermissions = new List<WindowsGroupPermission>();
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (WindowsGroupPermission permission in serverPermissions)
            {
                if (resourcePermissions.Any(groupPermission => groupPermission.WindowsGroup == permission.WindowsGroup))
                {
                    continue;
                }
                if (IsInRole(principal, permission) && permission.Matches(resource))
                {
                    groupPermissions.Add(permission);
                }
            }
            // ReSharper restore LoopCanBeConvertedToQuery

            groupPermissions.AddRange(resourcePermissions);
            //FilterAdminGroupForRemote(groupPermissions);
            return groupPermissions;
        }

        protected virtual bool IsInRole(IPrincipal principal, WindowsGroupPermission p)
        {
            bool isInRole = false;

            if (principal == null)
            {
                return p.IsBuiltInGuestsForExecution;
            }
            try
            {
                // If its our admin group ( Warewolf ), we need to check membership differently 
                // Prefixing with computer name does not work with Warewolf and IsInRole
                // Plain does not work with IsInRole
                // Hence this conditional check and divert
                string windowsGroup = p.WindowsGroup;
                if (windowsGroup == WindowsGroupPermission.BuiltInAdministratorsText)
                {
                    // We need to get the group as it is local then look for principle's membership
                    string principleName = principal.Identity.Name;
                    if (!string.IsNullOrEmpty(principleName))
                    {
                        // Examine if BuiltIn\Administrators is still present as a Member
                        // Then inspect BuiltIn\Administrators

                        // Examine group for this member ;)  
                        isInRole = principal.IsInRole(windowsGroup);

                        // if that fails, check Administrators group membership in the Warewolf Administrators group
                        if (!isInRole)
                        {
                            var sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                            var windowsPrincipal = principal as WindowsPrincipal;
                            var windowsIdentity = principal.Identity as WindowsIdentity;
                            if (windowsPrincipal != null)
                            {
                                isInRole = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) ||
                                           windowsPrincipal.IsInRole(sid);
                                if (windowsIdentity != null && !isInRole)
                                {
                                    if (windowsIdentity.Groups != null)
                                    {
                                        isInRole = windowsIdentity.Groups.Any(reference =>
                                        {
                                            if (reference.Value == sid.Value)
                                            {
                                                return true;
                                            }
                                            try
                                            {
                                                IdentityReference identityReference =
                                                    reference.Translate(typeof (NTAccount));
                                                if (identityReference != null)
                                                {
                                                    return identityReference.Value == windowsGroup;
                                                }
                                            }
                                            catch (Exception)
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
                                if (AreAdministratorsMembersOfWarewolfAdministrators.Invoke())
                                {
                                    // Check user's administrator membership
                                    isInRole = principal.IsInRole(sid.Value);
                                }

                                //Check regardless. Not installing the software can create a situation where the "Administrators" group is not part of Warewolf
                                isInRole = principal.IsInRole(sid.Value);
                            }
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
            catch
            {
            }
            // ReSharper restore EmptyGeneralCatchClause

            return isInRole || p.IsBuiltInGuestsForExecution;
        }

        private IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal)
        {
            List<WindowsGroupPermission> groupPermissions =
                _securityService.Permissions.Where(p => IsInRole(principal, p)).ToList();
            FilterAdminGroupForRemote(groupPermissions);
            return groupPermissions;
        }

        private void FilterAdminGroupForRemote(List<WindowsGroupPermission> groupPermissions)
        {
            if (!_isLocalConnection)
            {
                WindowsGroupPermission adminGroup =
                    groupPermissions.FirstOrDefault(
                        gr => gr.WindowsGroup.Equals(WindowsGroupPermission.BuiltInAdministratorsText));
                if (adminGroup != null)
                {
                    groupPermissions.Remove(adminGroup);
                }
            }
        }
    }
}