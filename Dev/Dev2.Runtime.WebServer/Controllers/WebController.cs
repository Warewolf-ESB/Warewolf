using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dev2.Runtime.WebServer.Controllers
{
    public abstract class WebController : ApiController
    {
        public virtual HttpResponseMessage Get(string website, string path)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", path }
            };

            return ProcessRequest(requestVariables);
        }

        protected HttpResponseMessage ProcessRequest(NameValueCollection requestVariables)
        {
            var user = User;
            if(user == null || !user.Identity.IsAuthenticated)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var context = new WebControllerContext(Request, requestVariables);

            var webRequestHandler = new WebRequestHandler();
            webRequestHandler.GetWebResource(context);

            return context.ResponseMessage;
        }

    }
}