using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Security
{
    public static class AuthorizationRequestHelper
    {
        public static AuthorizationRequest GetAuthorizationRequest(this HttpActionContext context)
        {
            return new AuthorizationRequest
            {
                RequestType = context.GetRequestType(),
                User = context.ControllerContext.RequestContext.Principal,
                Url = context.Request.RequestUri,
                QueryString = new QueryString(context.Request.GetQueryNameValuePairs())
            };
        }

        public static AuthorizationRequest GetAuthorizationRequest(this HubDescriptor hubDescriptor, IRequest request)
        {
            return GetAuthorizationRequest(request, WebServerRequestType.HubConnect);
        }

        public static AuthorizationRequest GetAuthorizationRequest(this IHubIncomingInvokerContext context)
        {
            return GetAuthorizationRequest(context.Hub.Context.Request, context.GetRequestType());
        }

        static AuthorizationRequest GetAuthorizationRequest(this IRequest request, WebServerRequestType requestType)
        {
            return new AuthorizationRequest
            {
                RequestType = requestType,
                User = request.User,
                Url = request.Url,
                QueryString = request.QueryString
            };
        }

        static WebServerRequestType GetRequestType(this IHubIncomingInvokerContext context)
        {
            return ParseRequestType(context.MethodDescriptor.Hub.Name, context.MethodDescriptor.Name);
        }

        static WebServerRequestType GetRequestType(this HttpActionContext context)
        {
            return ParseRequestType("Web", context.ActionDescriptor.ActionName);
        }

        static WebServerRequestType ParseRequestType(string source, string actionName)
        {
            WebServerRequestType requestType;
            Enum.TryParse(source + actionName, true, out requestType);
            return requestType;
        }
    }
}