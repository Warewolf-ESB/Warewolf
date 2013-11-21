using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dev2.Runtime.WebServer.Controllers
{
    [RoutePrefix("{website}")]
    public class WebController : ApiController
    {
        [Route("{type}/{file}")]
        public HttpResponseMessage Get(string website, string type, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", string.Format("{0}/{1}", type, file) }
            };

            return ProcessRequest(requestVariables);
        }

        [Route("{path}/{type}/{*file}")]
        public HttpResponseMessage Get(string website, string path, string type, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", path }
            };

            return ProcessRequest(requestVariables);
        }

        [Route("views/{*file}")]
        public virtual HttpResponseMessage Get(string website, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", string.Format("views/{0}", file) }
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