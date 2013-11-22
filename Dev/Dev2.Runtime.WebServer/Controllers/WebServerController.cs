using System.Collections.Specialized;
using System.Net.Http;
using System.Web.Http;
using Dev2.Runtime.WebServer.Handlers;

namespace Dev2.Runtime.WebServer.Controllers
{
    [Authorize]
    public class WebServerController : AbstractController
    {
        [HttpGet]
        [Route("{website}/{type}/{file}")]
        public HttpResponseMessage Get(string website, string type, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", string.Format("{0}/{1}", type, file) }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpGet]
        [Route("{website}/{path}/{type}/{*file}")]
        public HttpResponseMessage Get(string website, string path, string type, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", path }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpGet]
        [Route("{website}/views/{*file}")]
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
        [Route("{website}/{path}/Service/{name}/{method}")]
        public HttpResponseMessage InvokeService(string website, string path, string name, string method)
        {
            // DO NOT replace {method} with {action} in route mapping --> {action} is a reserved placeholder!
            var requestVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", path },
                { "name", name },
                { "action", method },
            };

            return ProcessRequest<WebsiteServiceHandler>(requestVariables);
        }

        [HttpGet]
        [HttpPost]
        [Route("services/{name}")]
        public HttpResponseMessage Execute(string name)
        {
            var requestVariables = new NameValueCollection
            {
                { "servicename", name }
            };

            return Request.Method == HttpMethod.Post
                ? ProcessRequest<WebPostRequestHandler>(requestVariables)
                : ProcessRequest<WebGetRequestHandler>(requestVariables);
        }

        [HttpGet]
        [HttpPost]
        [Route("services/{name}/instances/{instanceid}/bookmarks/{bookmark}")]
        public HttpResponseMessage Bookmark(string name, string instanceid, string bookmark)
        {
            var requestVariables = new NameValueCollection
            {
                { "servicename", name }, 
                { "instanceid", instanceid },
                { "bookmark", bookmark }
            };

            return Request.Method == HttpMethod.Post
                ? ProcessRequest<WebPostRequestHandler>(requestVariables)
                : ProcessRequest<WebGetRequestHandler>(requestVariables);
        }
    }
}