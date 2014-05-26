using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Dev2.Common;

namespace Dev2.Services.Security
{
    public abstract class AuthorizationServiceBase : DisposableObject, IAuthorizationService
    {
        readonly ISecurityService _securityService;
        readonly bool _isLocalConnection;

        protected AuthorizationServiceBase(ISecurityService securityService, bool isLocalConnection)
        {
            VerifyArgument.IsNotNull("SecurityService", securityService);
            _securityService = securityService;
            _isLocalConnection = isLocalConnection;
            _securityService.PermissionsChanged += (s, e) => RaisePermissionsChanged();
            _securityService.PermissionsModified += (s, e) => OnPermissionsModified(e);
        }

        public event EventHandler PermissionsChanged;
        private EventHandler<PermissionsModifiedEventArgs> _permissionsModifedHandler;
        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified
        {
            add
            {
                if(_permissionsModifedHandler == null)
                {
                    _permissionsModifedHandler += value;
                }
            }
            remove
            {
                // ReSharper disable DelegateSubtraction
                _permissionsModifedHandler -= value;
                // ReSharper restore DelegateSubtraction
            }
        }

        public Permissions GetResourcePermissions(Guid resourceID)
        {
            var groupPermissions = GetGroupPermissions(ClaimsPrincipal.Current, resourceID.ToString()).ToList();
            var result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
        }

        public virtual void Remove(Guid resourceID)
        {
            _securityService.Remove(resourceID);
        }

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);
        public abstract bool IsAuthorized(IAuthorizationRequest request);

        protected virtual void RaisePermissionsChanged()
        {
            if(PermissionsChanged != null)
            {
                PermissionsChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnPermissionsModified(PermissionsModifiedEventArgs e)
        {
            if(_permissionsModifedHandler != null)
            {
                _permissionsModifedHandler(this, e);
            }
        }

        protected bool IsAuthorizedToConnect(IPrincipal principal)
        {
            return IsAuthorized(AuthorizationContext.Any, () => GetGroupPermissions(principal));
        }

        public bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            return IsAuthorized(context, () => GetGroupPermissions(principal, resource));
        }

        public void DumpPermissionsOnError(IPrincipal principal)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if(principal.Identity != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                this.LogError("PERM DUMP FOR [ " + principal.Identity.Name + " ]");
            }
            else
            // ReSharper disable HeuristicUnreachableCode
            {
                this.LogError("PERM DUMP FOR [ NULL USER ]");
            }
            // ReSharper restore HeuristicUnreachableCode

            foreach(var perm in _securityService.Permissions)
            {
                this.LogError("SERVER PERM -> " + perm.WindowsGroup);
                this.LogError("IS USER IN IT [ " + principal.IsInRole(perm.WindowsGroup) + " ]");
            }
        }

        static bool IsAuthorized(AuthorizationContext context, Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions)
        {
            var contextPermissions = context.ToPermissions();
            var groupPermissions = getGroupPermissions();
            return groupPermissions.Any(p => (p.Permissions & contextPermissions) != 0);
        }

        IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            var groupPermissions = _securityService.Permissions.Where(p => IsInRole(principal, p) && p.Matches(resource)).ToList();
            FilterAdminGroupForRemote(groupPermissions);
            return groupPermissions;
        }

        static bool IsInRole(IPrincipal principal, WindowsGroupPermission p)
        {
            var isInRole = false;

            try
            {
                // If its our admin group ( Warewolf ), we need to check membership differently 
                // Prefixing with computer name does not work with Warewolf and IsInRole
                // Plain does not work with IsInRole
                // Hence this conditional check and divert
                if(p.WindowsGroup == WindowsGroupPermission.BuiltInAdministratorsText)
                {
                    // We need to get the group as it is local then look for principle's membership
                    var principleName = principal.Identity.Name;
                    if(!string.IsNullOrEmpty(principleName))
                    {
                        // Examine if BuiltIn\Administrators is still present as a Member
                        // Then inspect BuiltIn\Administrators

                        // Examine group for this member ;)  
                        isInRole = principal.IsInRole(p.WindowsGroup);

                        // if that fails, check Administrators group membership in the Warewolf Administrators group
                        if(!isInRole)
                        {
                            if(AreAdministratorsMemebersOfWarewolf())
                            {
                                // Check user's administrator membership
                                isInRole = principal.IsInRole(WindowsGroupPermission.AdministratorsText);
                            }
                        }

                        return isInRole;
                    }
                }
                else
                {
                    // THIS TRY-CATCH IS HERE TO AVOID THE EXPLORER NOT LOADING ANYTHING WHEN THE DOMAIN CANNOT BE CONTACTED!
                    isInRole = principal.IsInRole(p.WindowsGroup);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause

            return isInRole || p.IsBuiltInGuestsForExecution;
        }

        static bool AreAdministratorsMemebersOfWarewolf()
        {
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
                                    if(memberEntry.Name == "Administrators")
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
        public string JsonPermissions()
        {
            var result = new StringBuilder();
            result.AppendLine("{{");
            foreach(var permission in _securityService.Permissions)
            {
                result.AppendFormat("\t{{ ResourceID:{0},\tWindowsGroup:{1},\tPermissions:{2} }}", permission.ResourceID, permission.WindowsGroup, permission.Permissions);
                result.AppendLine();
            }
            result.AppendLine("}}");
            return result.ToString();
        }
    }
}