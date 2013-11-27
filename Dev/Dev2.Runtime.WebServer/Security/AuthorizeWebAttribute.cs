using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Dev2.Runtime.WebServer.Security
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeWebAttribute : AuthorizationFilterAttribute
    {
        readonly IAuthorizationProvider _authorizationProvider;

        public AuthorizeWebAttribute()
            : this(AuthorizationProvider.Instance)
        {
        }

        public AuthorizeWebAttribute(IAuthorizationProvider authorizationProvider)
        {
            VerifyArgument.IsNotNull("authorizationProvider", authorizationProvider);
            _authorizationProvider = authorizationProvider;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            VerifyArgument.IsNotNull("actionContext", actionContext);
            var user = actionContext.ControllerContext.RequestContext.Principal;
            if(!user.IsAuthenticated())
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Authorization has been denied for this request.");
            }

            AuthorizeRequestType requestType;
            Enum.TryParse("Web" + actionContext.ActionDescriptor.ActionName, out requestType);

            var resourceID = requestType == AuthorizeRequestType.WebExecute || requestType == AuthorizeRequestType.WebBookmark
            ? GetResourceName(actionContext)
            : actionContext.Request.GetResourceID();

            if(!_authorizationProvider.IsAuthorized(user, requestType, resourceID))
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access has been denied for this request.");
            }
        }

        string GetResourceName(HttpActionContext actionContext)
        {
            var parm = actionContext.ActionDescriptor.GetParameters().FirstOrDefault(p => p.ParameterName == "name");
            if(parm == null)
            {
                return null;
            }
            return parm.DefaultValue.ToString();
        }
    }
}