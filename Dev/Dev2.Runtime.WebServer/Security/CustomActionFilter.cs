
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
    public sealed class CustomActionFilter : ActionFilterAttribute
    {
        public CustomActionFilter() : this(ServerAuthorizationService.Instance)
        {

        }
        public CustomActionFilter(Services.Security.IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("AuthorizationService", authorizationService);
            Service = authorizationService;
        }
        public Services.Security.IAuthorizationService Service { get; private set; }

        public override void OnActionExecuting(ActionExecutingContext objContext)
        {
            OnAuthorization(objContext);
        }

        public override void OnActionExecuted(ActionExecutedContext objContext)
        {
        }

        public void OnAuthorization(ActionContext actionContext)
        {
            VerifyArgument.IsNotNull(nameof(actionContext), actionContext);
            var user = actionContext.HttpContext.User;
            var actionName = actionContext.HttpContext?.Request.RouteValues["action"]?.ToString();

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

            var authorizationRequest = GetAuthorizationRequest(actionContext);
            if (!Service.IsAuthorized(authorizationRequest))
            {
                actionContext.HttpContext.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Forbidden, Title = GlobalConstants.USER_FORBIDDEN, Message = ErrorResource.AuthorizationDeniedForThisRequest });
            }
        }

        static AuthorizationRequest GetAuthorizationRequest(ActionContext actionContext)
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

        static int GetNameStartIndex(string absolutePath)
        {
            var startIndex = absolutePath.IndexOf("services/", StringComparison.InvariantCultureIgnoreCase);
            if (startIndex == -1)
            {
                return -1;
            }

            startIndex += 9;
            return startIndex;
        }

        static int GetNameStartIndexForToken(string absolutePath)
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
