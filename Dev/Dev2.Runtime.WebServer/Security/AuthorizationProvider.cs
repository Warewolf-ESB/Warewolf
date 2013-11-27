using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Dev2.Runtime.WebServer.Security
{
    public class AuthorizationProvider : IAuthorizationProvider
    {
        public static AuthorizationProvider Instance
        {
            get
            {
                return TheInstance.Value;
            }
        }

        // Singleton instance - lazy initialization is used to ensure that the creation is threadsafe
        readonly static Lazy<AuthorizationProvider> TheInstance = new Lazy<AuthorizationProvider>(() => new AuthorizationProvider());


        public bool IsAuthorized(IPrincipal user)
        {
            var roles = GetRoles(user);
            return roles.Any(user.IsInRole);
        }

        IList<string> GetRoles(IPrincipal user)
        {
            var roles = new List<string>();
            roles.Add("xxxx");
            //roles.Add("LocalTestGroup");
            //roles.Add("DnsAdmins");
            roles.Add("Administrators");
            return roles;
        }
    }
}
