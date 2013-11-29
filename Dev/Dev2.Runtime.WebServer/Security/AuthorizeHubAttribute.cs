using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeHubAttribute : Attribute, IAuthorizeHubConnection, IAuthorizeHubMethodInvocation
    {
        public AuthorizeHubAttribute()
            : this(AuthorizationProvider.Instance)
        {
        }

        public AuthorizeHubAttribute(IAuthorizationProvider authorizationProvider)
        {
            VerifyArgument.IsNotNull("authorizationProvider", authorizationProvider);
            Provider = authorizationProvider;
        }

        public IAuthorizationProvider Provider { get; private set; }

        public bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            VerifyArgument.IsNotNull("hubDescriptor", hubDescriptor);
            VerifyArgument.IsNotNull("request", request);
            return request.User.IsAuthenticated() && Provider.IsAuthorized(hubDescriptor.GetAuthorizationRequest(request));
        }

        public bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext context, bool appliesToMethod)
        {
            VerifyArgument.IsNotNull("context", context);
            return Provider.IsAuthorized(context.GetAuthorizationRequest());
        }
    }
}