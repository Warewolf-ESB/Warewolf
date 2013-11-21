using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dev2.Runtime.WebServer.Handlers;

namespace Dev2.Runtime.WebServer.Controllers
{
    public abstract class AbstractController : ApiController
    {
        protected HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables)
            where TRequestHandler : IRequestHandler
        {
            var user = User;
            if(user == null || !user.Identity.IsAuthenticated)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var context = new WebServerContext(Request, requestVariables);

            var handler = Activator.CreateInstance<TRequestHandler>();
            handler.ProcessRequest(context);

            return context.ResponseMessage;
        }
    }
}