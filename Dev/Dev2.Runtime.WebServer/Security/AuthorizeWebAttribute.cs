using System;
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

            AuthorizationRequestType requestType;
            Enum.TryParse("Web" + actionContext.ActionDescriptor.ActionName, out requestType);

            var request = new AuthorizationRequest
            {
                RequestType = requestType,
                User = user,
                Url = actionContext.Request.RequestUri,
                QueryString = new QueryString(actionContext.Request.GetQueryNameValuePairs())
            };

            if(!_authorizationProvider.IsAuthorized(request))
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access has been denied for this request.");
            }
        }
    }
}