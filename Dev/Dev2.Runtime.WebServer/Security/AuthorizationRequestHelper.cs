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
using System.Web.Http.Controllers;
using Dev2.Common;
using Dev2.Services.Security;
using Dev2.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Dev2.Runtime.WebServer;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.WebServer.Security
{
    public static class AuthorizationRequestHelper
    {
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
    }
}
