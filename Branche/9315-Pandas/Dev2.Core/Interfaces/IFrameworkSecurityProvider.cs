using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;


namespace Dev2 {
    public interface IFrameworkSecurityContext {
        IIdentity UserIdentity { get; }
        bool IsUserInRole(string[] roles);
        string[] Roles { get; }
        string[] AllRoles { get; }
    }

}
