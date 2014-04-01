using System;
using System.Collections.Generic;
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
            var result = IsAuthorized(AuthorizationContext.Any, () => GetGroupPermissions(principal));

            ServerLogger.LogTrace("Is Authorized To Connect : " + principal.Identity.Name + " = " + result);

            return result;
        }

        public bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            return IsAuthorized(context, () => GetGroupPermissions(principal, resource));
        }

        public void DumpPermissionsOnError(IPrincipal principal)
        {
            //var permissions = GetGroupPermissions(principal);
            this.LogError("PERM DUMP FOR [ " + principal.Identity.Name + " ]");

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
                // THIS IS HERE TO AVOID THE EXPLORER NOT LOADING ANYTHING WHEN THE DOMAIN CANNOT BE CONTACTED!
                isInRole = principal.IsInRole(p.WindowsGroup);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause

            return isInRole || p.IsBuiltInGuestsForExecution;
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
                var adminGroup = groupPermissions.FirstOrDefault(gr => gr.WindowsGroup.Equals(GlobalConstants.BuiltInAdministrator));
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