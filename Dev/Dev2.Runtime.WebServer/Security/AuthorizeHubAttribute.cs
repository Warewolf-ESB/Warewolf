using System;
using System.Security.Principal;
using Microsoft.AspNet.SignalR;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeHubAttribute : AuthorizeAttribute
    {
        readonly IAuthorizationProvider _authorizationProvider;

        public AuthorizeHubAttribute()
            : this(AuthorizationProvider.Instance)
        {
        }

        public AuthorizeHubAttribute(IAuthorizationProvider authorizationProvider)
        {
            VerifyArgument.IsNotNull("authorizationProvider", authorizationProvider);
            _authorizationProvider = authorizationProvider;
        }

        protected override bool UserAuthorized(IPrincipal user)
        {
            return user.IsAuthenticated() && _authorizationProvider.IsAuthorized(user);
        }
    }
}