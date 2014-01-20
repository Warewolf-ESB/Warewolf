using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeWebAttribute : AuthorizationFilterAttribute
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
            if(!user.IsAuthenticated())
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Authorization has been denied for this request.");
                return;
            }

            if(!Service.IsAuthorized(actionContext.GetAuthorizationRequest()))
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access has been denied for this request.");
            }
        }
    }
}