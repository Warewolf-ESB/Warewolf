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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
//using System.Web.Http.Controllers;

namespace Dev2.Runtime.WebServer.Security
{
    public static class AuthorizationRequestHelper
    {
        public static AuthorizationRequest GetAuthorizationRequest(this HttpContext context) => GetAuthorizationRequest(context, context.GetRequestType());

        public static AuthorizationRequest GetAuthorizationRequest(this HttpContext context, WebServerRequestType requestType) => new AuthorizationRequest
        {
            RequestType = requestType,
            User = context.User,
            Url = context.Request.ToUri(),
            //QueryString = new QueryString(context.Request.GetQueryNameValuePairs())
            QueryString = context.Request.Query
        };

        public static AuthorizationRequest GetAuthorizationRequest(this HttpContext context, string sourceName)
        {
            var requestType = ParseRequestType(sourceName, context.GetActionName());
            return new AuthorizationRequest
            {
                RequestType = requestType,
                User = context.User,
                Url = context.Request.ToUri(),
                QueryString = context.Request.Query
            };
        }

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


        static WebServerRequestType GetRequestType(this HubInvocationContext context)
        {
            var httpContext = context.Context.GetHttpContext();
            var hubInfo = context.Hub.GetType();
            var hubName = hubInfo.Name.Replace("hub", "", StringComparison.OrdinalIgnoreCase);

            return ParseRequestType(hubName, context.HubMethodName);
        }
        

        static WebServerRequestType GetRequestType(this HttpContext context)
        {
            return ParseRequestType("Web", context.GetActionName());
        }

     
        static string GetActionName(this HttpContext context)
        {
            return  context.Request.RouteValues["action"]?.ToString();
        }

        static WebServerRequestType ParseRequestType(string source, string actionName)
        {
            Enum.TryParse(source + actionName, true, out WebServerRequestType requestType);
            return requestType;
        }
    }
}
