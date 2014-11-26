
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Common;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeHubAttribute : Attribute, IAuthorizeHubConnection, IAuthorizeHubMethodInvocation
    {
        public AuthorizeHubAttribute()
            : this(ServerAuthorizationService.Instance)
        {
        }

        public AuthorizeHubAttribute(IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("AuthorizationService", authorizationService);
            Service = authorizationService;
        }

        public IAuthorizationService Service { get; private set; }

        public bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            VerifyArgument.IsNotNull("hubDescriptor", hubDescriptor);
            VerifyArgument.IsNotNull("request", request);
            var result = request.User.IsAuthenticated() && Service.IsAuthorized(hubDescriptor.GetAuthorizationRequest(request));

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if(request.User.Identity != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                Dev2Logger.Log.Debug("AuthorizeHubConnection For [ " + request.User.Identity.Name + " ]");
            }

            return result;
        }

        public bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext context, bool appliesToMethod)
        {
            VerifyArgument.IsNotNull("context", context);
            return Service.IsAuthorized(context.GetAuthorizationRequest());
        }
    }
}
