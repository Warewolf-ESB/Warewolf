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
using System.Web;
//using System.Web.Http.Controllers;
//using System.Web.Http.Filters;
using Microsoft.AspNetCore.Http;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Resource.Errors;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeWebAttribute : Attribute //  //, IAuthenticationFilter
    {
        private readonly IHttpContextAccessor actionContext;

        public AuthorizeWebAttribute()
            : this(ServerAuthorizationService.Instance)
        {
        }

        public AuthorizeWebAttribute(Dev2.Services.Security.IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("AuthorizationService", authorizationService);
            Service = authorizationService;
        }

        public Dev2.Services.Security.IAuthorizationService Service { get; private set; }

        public void OnAuthorization()
        {
            VerifyArgument.IsNotNull("actionContext", actionContext);
            var user = actionContext.HttpContext.User;
            string actionName = actionContext.HttpContext?.Request.RouteValues["action"]?.ToString();

            if (actionName == "ExecutePublicTokenWorkflow" || actionName == "ExecuteLoginWorkflow")
            {
                return;
            }

            if (user == null && (actionName == "ExecutePublicWorkflow" || actionName == "ExecuteGetRootLevelApisJson"))
            {
                user = GlobalConstants.GenericPrincipal as System.Security.Claims.ClaimsPrincipal;
                //actionContext.HttpContext.RequestContext.Principal = user;
                actionContext.HttpContext.User = user;
            }

            if (!user.IsAuthenticated())
            {
                var httpContext = actionContext.HttpContext;
                httpContext.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
                return;
            }

            var authorizationRequest = GetAuthorizationRequest();
            if (!Service.IsAuthorized(authorizationRequest))
            {
                actionContext.HttpContext.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Forbidden, Title = GlobalConstants.USER_FORBIDDEN, Message = ErrorResource.AuthorizationDeniedForThisRequest });
            }
        }

        AuthorizationRequest GetAuthorizationRequest()
        {
            var authorizationRequest = actionContext.HttpContext.GetAuthorizationRequest();

            try
            {
                var absolutePath = actionContext.HttpContext.Request.ToUri().AbsolutePath;
                var startIndex = GetNameStartIndex(absolutePath);
                if (startIndex > -1)
                {
                    var resourceName = HttpUtility.UrlDecode(absolutePath.Substring(startIndex, absolutePath.Length - startIndex));
                    var resource = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);

                    if (resource != null && resource.ResourceType == "ReservedService")
                    {
                        authorizationRequest = new AuthorizationRequest
                        {
                            RequestType = WebServerRequestType.WebExecuteInternalService,
                            User = actionContext.HttpContext.User,
                            Url = actionContext.HttpContext.Request.ToUri(),
                            //QueryString = new QueryString(actionContext.Request.GetQueryNameValuePairs())
                            QueryString = actionContext.HttpContext.Request.Query
                        };
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }

            return authorizationRequest;
        }

        int GetNameStartIndex(string absolutePath)
        {
            var startIndex = absolutePath.IndexOf("services/", StringComparison.InvariantCultureIgnoreCase);
            if (startIndex == -1)
            {
                return -1;
            }

            startIndex += 9;
            return startIndex;
        }

        int GetNameStartIndexForToken(string absolutePath)
        {
            var startIndex = absolutePath.IndexOf("token/", StringComparison.InvariantCultureIgnoreCase);
            if (startIndex == -1)
            {
                return -1;
            }

            startIndex += 5;
            return startIndex;
        }

    }
}