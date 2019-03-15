/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Warewolf.Resource.Errors;

namespace Dev2.Services.Security
{
    public interface IUserIdentity
    {
        IEnumerable<IGroup> Groups { get; }
        bool IsAnonymous { get; }
        bool IsAuthenticated { get; }
        bool IsGuest { get; }
        bool IsSystem { get; }
        string Name { get; }
        IWindowsImpersonationContext Impersonate();
    }

    public interface IGroup
    {
        string Id { get; }
        string Name { get; }
    }

    public interface IWindowsImpersonationContext
    {

    }

    class WindowsImpersonationContextWrapper : IWindowsImpersonationContext
    {
        readonly WindowsImpersonationContext _windowsImpersonationContext;

        public WindowsImpersonationContextWrapper(WindowsImpersonationContext windowsImpersonationContext)
        {
            _windowsImpersonationContext = windowsImpersonationContext;
        }
    }

    [ExcludeFromCodeCoverage]
    class WindowsGroupWrapper : IGroup
    {
        readonly IdentityReference _group;

        public WindowsGroupWrapper(IdentityReference group)
        {
            this._group = group;
        }

        public string Id => _group.Value;

        public string Name => _group.Translate(typeof(NTAccount))?.Value;
    }

    [ExcludeFromCodeCoverage]
    public class WindowsIdentityWrapper : IUserIdentity
    {
        private readonly WindowsIdentity _windowsIdentity;

        public WindowsIdentityWrapper(WindowsIdentity identity)
        {
            _windowsIdentity = identity;
        }

        public IEnumerable<IGroup> Groups => _windowsIdentity.Groups.Select(group => new WindowsGroupWrapper(group));

        public bool IsAnonymous => _windowsIdentity.IsAnonymous;

        public bool IsAuthenticated => _windowsIdentity.IsAuthenticated;

        public bool IsGuest => _windowsIdentity.IsGuest;

        public bool IsSystem => _windowsIdentity.IsSystem;

        public string Name => _windowsIdentity.Name;

        public IWindowsImpersonationContext Impersonate() => new WindowsImpersonationContextWrapper(_windowsIdentity.Impersonate());
    }

    public interface IUser
    {
        bool IsAdmin { get; }

        bool IsWarewolfAdmin();
    }
    
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
                        if (dChildEntry.Name != GlobalConstants.WarewolfGroup)
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

        public List<WindowsGroupPermission> GetPermissions(IPrincipal principal)
        {
            lock (_getPermissionsLock)
            {
                var resourcePermissions = _securityService.Permissions.Where(permission => PrincipalIsInRole.IsInRole(principal, permission) && !permission.IsServer).ToList();
                var serverPermissionsForUser = _securityService.Permissions.Where(permission => PrincipalIsInRole.IsInRole(principal, permission) && (permission.IsServer || permission.ResourceID == Guid.Empty)).ToList();
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

        bool IsAuthorized(AuthorizationContext context, IPrincipal principal, Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions)
        {
            var contextPermissions = context.ToPermissions();
            var groupPermissions = getGroupPermissions?.Invoke();
            if (context == AuthorizationContext.Any)
            {
                groupPermissions = _securityService.Permissions.Where(permission => PrincipalIsInRole.IsInRole(principal, permission)).ToList();
            }
            return groupPermissions.Any(p => (p.Permissions & contextPermissions) != 0);
        }

        protected virtual IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            var resourcePermissions = _securityService.Permissions.Where(permission => PrincipalIsInRole.IsInRole(principal, permission) && !permission.IsServer && permission.Matches(resource)).ToList();
            var groupPermissions = new List<WindowsGroupPermission>();

            foreach (var permission in _securityService.Permissions)
            {
                if (resourcePermissions.Any(groupPermission => groupPermission.WindowsGroup == permission.WindowsGroup))
                {
                    continue;
                }
                if (PrincipalIsInRole.IsInRole(principal, permission) && permission.Matches(resource))
                {
                    groupPermissions.Add(permission);
                }
            }

            groupPermissions.AddRange(resourcePermissions);
            return groupPermissions;
        }

        IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal)
        {
            var groupPermissions = _securityService.Permissions.Where(permission => PrincipalIsInRole.IsInRole(principal, permission)).ToList();
            FilterAdminGroupForRemote(groupPermissions);
            return groupPermissions;
        }

        void FilterAdminGroupForRemote(List<WindowsGroupPermission> groupPermissions)
        {
            if (!_isLocalConnection)
            {
                var adminGroup = groupPermissions.FirstOrDefault(gr => gr.WindowsGroup.Equals(GlobalConstants.WarewolfGroup));
                if (adminGroup != null)
                {
                    groupPermissions.Remove(adminGroup);
                }
            }
        }
    }

    public static class PrincipalIsInRole
    {
        public static bool IsInRole(IPrincipal principal, WindowsGroupPermission permission)
        {
            if (principal == null)
            {
                return permission.IsBuiltInGuestsForExecution;
            }
            var isInRole = false;
            try
            {
                var windowsGroup = permission.WindowsGroup;
                if (windowsGroup == GlobalConstants.WarewolfGroup)
                {
                    if (!string.IsNullOrEmpty(principal.Identity.Name))
                    {
                        return new WindowsPrincipalWrapper(principal as WindowsPrincipal).IsWarewolfAdmin();
                    }
                }
                else
                {
                    isInRole = principal.IsInRole(windowsGroup);
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

            return isInRole || permission.IsBuiltInGuestsForExecution;
        }
    }

    class WindowsPrincipalWrapper : IUser
    {
        static readonly SecurityIdentifier _adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

        readonly WindowsPrincipal _user;
        public IUserIdentity Identity { get; set; }

        public WindowsPrincipalWrapper(WindowsPrincipal windowsPrincipal)
        {
            _user = windowsPrincipal;
        }

        public bool IsAdmin => _user.IsInRole(WindowsBuiltInRole.Administrator) || _user.IsInRole(GlobalConstants.WarewolfGroup) || _user.IsInRole(_adminSid);

        public bool IsWarewolfAdmin()
        {
            if (_user.IsInRole(GlobalConstants.WarewolfGroup))
            {
                return true;
            }
            if (IsAdmin)
            {
                return _user.IsInRole(_adminSid.Value);
            }

            return IsInBuiltInAdministratorsGroup() || IsUserAdminOnActiveDirectory(_user.Identity.Name);
        }

        bool IsInBuiltInAdministratorsGroup() =>
                    Identity.Groups.Any(group =>
                    {
                        if (group.Id == _adminSid.Value)
                        {
                            return true;
                        }
                        try
                        {
                            var translatedName = group.Name;
                            if (translatedName != null)
                            {
                                return translatedName == GlobalConstants.WarewolfGroup;
                            }
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                        return false;
                    });

        public static bool IsUserAdminOnActiveDirectory(string username)
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
                    var notAdmin = dChildEntry.Name != GlobalConstants.WarewolfGroup;
                    notAdmin &= dChildEntry.Name != windowsBuiltInRole;
                    notAdmin &= dChildEntry.Name != "Administrators";
                    notAdmin &= dChildEntry.Name != WindowsGroupPermission.AdministratorsText;

                    if (notAdmin)
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
    }
}
