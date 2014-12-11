
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
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeWebAttribute : AuthorizationFilterAttribute//, IAuthenticationFilter 
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

            if (user == null && actionContext.ActionDescriptor.ActionName == "ExecutePublicWorkflow")
            {
                var genericIdentity = new GenericIdentity("GenericPublicUser");
                user = new GenericPrincipal(genericIdentity, new string[0]);
                actionContext.ControllerContext.RequestContext.Principal = user; 
            }

            if(!user.IsAuthenticated())
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Authorization has been denied for this request.");
                return;
            }
            var authorizationRequest = GetAuthorizationRequest(actionContext);
            if(!Service.IsAuthorized(authorizationRequest))
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access has been denied for this request.");
            }
        }

        AuthorizationRequest GetAuthorizationRequest(HttpActionContext actionContext)
        {
            AuthorizationRequest authorizationRequest = actionContext.GetAuthorizationRequest();

            try
            {
                var absolutePath = actionContext.Request.RequestUri.AbsolutePath;
                var startIndex = GetNameStartIndex(absolutePath);
                if(startIndex > -1)
                {
                    var resourceName = HttpUtility.UrlDecode(absolutePath.Substring(startIndex, absolutePath.Length - startIndex));
                    var resource = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);

                    if(resource != null && resource.ResourceType == ResourceType.ReservedService)
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
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
            }

            return authorizationRequest;
        }

        int GetNameStartIndex(string absolutePath)
        {
            var startIndex = absolutePath.IndexOf("services/", StringComparison.InvariantCultureIgnoreCase);
            if(startIndex == -1)
            {
                return -1;
            }

            startIndex += 9;
            return startIndex;
        }
    }

}
