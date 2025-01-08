#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Net.Http;
using Dev2.Common;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Runtime.WebServer;
using Warewolf.Resource.Errors;
#if NETFRAMEWORK
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Web.Http.Controllers;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
#endif

namespace Dev2.Runtime.WebServer.Security
{
    public static class AuthorizationRequestHelper
    {
#if NETFRAMEWORK
		public static AuthorizationRequest GetAuthorizationRequest(this HttpActionContext context) => new AuthorizationRequest
		{
			RequestType = context.GetRequestType(),
			User = context.ControllerContext.RequestContext.Principal,
			Url = context.Request.RequestUri,
			QueryString = new QueryString(context.Request.GetQueryNameValuePairs())
		};

		public static AuthorizationRequest GetAuthorizationRequest(this HubDescriptor hubDescriptor, IRequest request) => GetAuthorizationRequest(request, WebServerRequestType.HubConnect);

		public static AuthorizationRequest GetAuthorizationRequest(this IHubIncomingInvokerContext context) => GetAuthorizationRequest(context.Hub.Context.Request, context.GetRequestType());

		static AuthorizationRequest GetAuthorizationRequest(this IRequest request, WebServerRequestType requestType) => new AuthorizationRequest
		{
			RequestType = requestType,
			User = request.User,
			Url = request.Url,
			QueryString = request.QueryString
		};

		static WebServerRequestType GetRequestType(this IHubIncomingInvokerContext context) => ParseRequestType(context.MethodDescriptor.Hub.Name, context.MethodDescriptor.Name);

		static WebServerRequestType GetRequestType(this HttpActionContext context) => ParseRequestType("Web", context.ActionDescriptor.ActionName);

		static WebServerRequestType ParseRequestType(string source, string actionName)
		{
			Enum.TryParse(source + actionName, true, out WebServerRequestType requestType);
			return requestType;
		}
#else
        public static AuthorizationRequest GetAuthorizationRequest(this ActionContext actionContext) => GetAuthorizationRequest(actionContext, actionContext.GetRequestType());


        public static AuthorizationRequest GetAuthorizationRequest(this HubLifetimeContext hubLifeTimeContext)
        {
            var context = hubLifeTimeContext.Context.GetHttpContext();

            return new AuthorizationRequest
            {
                RequestType = WebServerRequestType.HubConnect,
                User = context.User,
                Url = context.Request.ToUri(),
                QueryString = context.Request.Query
            };
        }

        public static AuthorizationRequest GetAuthorizationRequest(this HubInvocationContext hubInvocationContext)
        {
            var context = hubInvocationContext.Context.GetHttpContext();

            return new AuthorizationRequest
            {
                RequestType = hubInvocationContext.GetRequestType(),
                User = context.User,
                Url = context.Request.ToUri(),
                QueryString = context.Request.Query
            };
        }

         static AuthorizationRequest GetAuthorizationRequest(this ActionContext actionContext, WebServerRequestType requestType)
        {
            var context = actionContext.HttpContext;
            return new AuthorizationRequest
            {
                RequestType = requestType,
                User = context.User,
                Url = context.Request.ToUri(),
                QueryString = context.Request.Query
            };
        }

        public static AuthorizationRequest GetAuthorizationRequest(this HttpContext context)
        {
            return new AuthorizationRequest
            {
                RequestType = context.GetRequestType(),
                User = context.User,
                Url = context.Request.ToUri(),
                QueryString = context.Request.Query
            };
        }

		static WebServerRequestType GetRequestType(this HubInvocationContext context)
        {
            var httpContext = context.Context.GetHttpContext();
            var hubInfo = context.Hub.GetType();
            var hubName = hubInfo.Name.Replace("hub", "", StringComparison.OrdinalIgnoreCase);

            return ParseRequestType(hubName, context.HubMethodName);
        }

        static WebServerRequestType GetRequestType(this ActionContext context)
        {
            return ParseRequestType("Web", context.GetActionName());
        }

        static WebServerRequestType GetRequestType(this HttpContext context)
        {
            return ParseRequestType("Web", context.Request.RouteValues["action"]?.ToString());
        }

        static string GetActionName(this ActionContext context)
        {
            var httpContext = context.HttpContext;
            return httpContext.Request.RouteValues["action"]?.ToString();
        }

        static WebServerRequestType ParseRequestType(string source, string actionName)
        {
            Enum.TryParse(source + actionName, true, out WebServerRequestType requestType);
            return requestType;
        }
#endif
	}
}
