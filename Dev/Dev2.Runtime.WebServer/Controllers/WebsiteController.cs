using System;
using System.Activities.Expressions;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dev2.Runtime.WebServer.Handlers;

namespace Dev2.Runtime.WebServer.Controllers
{
    [Authorize]
    [RoutePrefix("{website}")]
    public class WebsiteController : AbstractController
    {
        [Route("{type}/{file}")]
        public HttpResponseMessage Get(string website, string type, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", string.Format("{0}/{1}", type, file) }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [Route("{path}/{type}/{*file}")]
        public HttpResponseMessage Get(string website, string path, string type, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", path }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [Route("views/{*file}")]
        public HttpResponseMessage Get(string website, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", string.Format("views/{0}", file) }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpPost]
        [Route("{path}/Service/{name}/{action}")]
        public void Post(string website, string path, string name, string action)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", path },
                { "name", name },
                { "action", action },
            };

            ProcessRequest<WebsiteServiceHandler>(requestVariables);
        }
    }
}