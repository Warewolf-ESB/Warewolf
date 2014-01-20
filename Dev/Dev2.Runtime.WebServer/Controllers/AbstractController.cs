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
        protected virtual HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables)
            where TRequestHandler : class, IRequestHandler, new()
        {
            if(!IsAuthenticated())
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            var context = new WebServerContext(Request, requestVariables);
            context.Request.User = User;
            var handler = CreateHandler<TRequestHandler>();

            handler.ProcessRequest(context);
            return context.ResponseMessage;
        }

        protected virtual bool IsAuthenticated()
        {
            return User.IsAuthenticated();
        }

        protected virtual TRequestHandler CreateHandler<TRequestHandler>()
            where TRequestHandler : class, IRequestHandler, new()
        {
            return Activator.CreateInstance<TRequestHandler>();
        }
    }
}