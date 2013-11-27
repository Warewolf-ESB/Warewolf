using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Dev2.Data.Settings.Security;

namespace Dev2.Runtime.WebServer.Security
{
    public class AuthorizationProvider : IAuthorizationProvider
    {
        static readonly string[] EmptyRoles = new string[0];

        readonly List<WindowsGroupPermission> _permissions;

        // Singleton instance - lazy initialization is used to ensure that the creation is threadsafe
        readonly static Lazy<AuthorizationProvider> TheInstance = new Lazy<AuthorizationProvider>(() => new AuthorizationProvider());
        public static AuthorizationProvider Instance
        {
            get
            {
                return TheInstance.Value;
            }
        }

        private AuthorizationProvider()
            : this(new List<WindowsGroupPermission>())
        {
        }

        protected AuthorizationProvider(List<WindowsGroupPermission> permissions)
        {
            _permissions = permissions;
        }

        public bool IsAuthorized(IPrincipal user, AuthorizeRequestType requestType, string resourceID)
        {
            //var roles = GetRoles(requestType, resourceID);
            //return roles.Any(user.IsInRole);
            return true;
        }

        IEnumerable<string> GetRoles(AuthorizeRequestType requestType, string resourceID)
        {
            switch(requestType)
            {
                case AuthorizeRequestType.WebGet:
                    return _permissions.Where(p => (p.View || p.Contribute) && Matches(p, resourceID)).Select(p => p.WindowsGroup);

                case AuthorizeRequestType.WebInvokeService:
                    return _permissions.Where(p => (p.Contribute || p.Execute) && Matches(p, resourceID)).Select(p => p.WindowsGroup);

                case AuthorizeRequestType.WebExecute:
                    return _permissions.Where(p => p.Execute && Matches(p, resourceID)).Select(p => p.WindowsGroup);

                case AuthorizeRequestType.WebBookmark:
                    return _permissions.Where(p => p.Execute && Matches(p, resourceID)).Select(p => p.WindowsGroup);
            }
            //roles.Add("xxxx");
            //roles.Add("LocalTestGroup");
            //roles.Add("DnsAdmins");
            //roles.Add("Administrators");
            return EmptyRoles;
        }

        static bool Matches(WindowsGroupPermission permission, string resourceID)
        {
            if(permission.IsServer)
            {
                return true;
            }

            Guid id;
            if(Guid.TryParse(resourceID, out id))
            {
                return permission.ResourceID == id;
            }
            return permission.ResourceName.EndsWith(resourceID);
        }

    }
}
