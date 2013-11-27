using System;
using System.Security.Principal;

namespace Dev2.Runtime.WebServer.Security
{
    public interface IAuthorizationProvider
    {
        bool IsAuthorized(IPrincipal user, AuthorizeRequestType requestType, string resourceID);
    }
}