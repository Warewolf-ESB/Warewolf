#pragma warning disable
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
using Dev2.Runtime.Security;
using Dev2.Services.Security;
//using Microsoft.AspNet.SignalR;
//using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeHubAttribute : Attribute//,  IAuthorizeHubConnection, IAuthorizeHubMethodInvocation
    {
        public AuthorizeHubAttribute()
            : this(ServerAuthorizationService.Instance)
        {
        }

        public AuthorizeHubAttribute(Dev2.Services.Security.IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("AuthorizationService", authorizationService);
            Service = authorizationService;
        }

        public Dev2.Services.Security.IAuthorizationService Service { get; private set; }


        //public bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        //{
        //    VerifyArgument.IsNotNull("hubDescriptor", hubDescriptor);
        //    VerifyArgument.IsNotNull("request", request);
        //    var result = request.User.IsAuthenticated() && Service.IsAuthorized(hubDescriptor.GetAuthorizationRequest(request));
        //    return result;
        //}

        //public bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        //{
        //    VerifyArgument.IsNotNull("context", hubIncomingInvokerContext);
        //    return Service.IsAuthorized(hubIncomingInvokerContext.GetAuthorizationRequest());
        //}
    }

    public class AuthorizeHubRequirement : AuthorizationHandler<AuthorizeHubRequirement, HubInvocationContext>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeHubRequirement requirement, HubInvocationContext resource)
        {
            var httpContext = resource?.Hub?.Context?.GetHttpContext();
            if (httpContext != null && httpContext.User.IsAuthenticated())
            {
                var request = httpContext.Request;
                var url = GetUri(request);
                var auth = new AuthorizationRequest();
                auth.Url = url;
                auth.User = httpContext.User;
                auth.QueryString = request.Query;
                auth.RequestType = WebServerRequestType.HubConnect;
                if (ServerAuthorizationService.Instance.IsAuthorized(auth))
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private Uri GetUri(HttpRequest request)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Port = request.Host.Port.GetValueOrDefault(80),
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };
            return uriBuilder.Uri;
        }
    }
}
