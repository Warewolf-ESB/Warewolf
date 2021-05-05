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
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeWebAttribute : AuthorizationFilterAttribute //, IAuthenticationFilter
    {
        public AuthorizeWebAttribute()
            : this(ServerAuthorizationService.Instance)
        {
        }

        public AuthorizeWebAttribute(IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("AuthorizationService", authorizationService);
            Service = authorizationService;
        }

        public IAuthorizationService Service { get; private set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            VerifyArgument.IsNotNull("actionContext", actionContext);
            var user = actionContext.ControllerContext.RequestContext.Principal;
            if (actionContext.ActionDescriptor.ActionName == "ExecutePublicTokenWorkflow" ||
                actionContext.ActionDescriptor.ActionName == "ExecuteLoginWorkflow")
            {
                return;
            }

            if (user == null && (actionContext.ActionDescriptor.ActionName == "ExecutePublicWorkflow" || actionContext.ActionDescriptor.ActionName == "ExecuteGetRootLevelApisJson"))
            {
                user = GlobalConstants.GenericPrincipal;
                actionContext.ControllerContext.RequestContext.Principal = user;
            }

            if (!user.IsAuthenticated())
            {
                actionContext.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
                return;
            }

            var authorizationRequest = GetAuthorizationRequest(actionContext);
            if (!Service.IsAuthorized(authorizationRequest))
            {
                actionContext.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Forbidden, Title = GlobalConstants.USER_FORBIDDEN, Message = ErrorResource.AuthorizationDeniedForThisRequest });
            }
        }

        AuthorizationRequest GetAuthorizationRequest(HttpActionContext actionContext)
        {
            var authorizationRequest = actionContext.GetAuthorizationRequest();

            try
            {
                var absolutePath = actionContext.Request.RequestUri.AbsolutePath;
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
                            User = actionContext.ControllerContext.RequestContext.Principal,
                            Url = actionContext.Request.RequestUri,
                            QueryString = new QueryString(actionContext.Request.GetQueryNameValuePairs())
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