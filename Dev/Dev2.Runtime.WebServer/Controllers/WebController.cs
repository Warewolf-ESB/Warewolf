using System.Collections.Specialized;
using System.Net.Http;
using System.Web.Http;
using Dev2.Runtime.WebServer.Handlers;

namespace Dev2.Runtime.WebServer.Controllers
{
    [Authorize]
    [RoutePrefix("services/{name}")]
    public class WebController : AbstractController
    {
        [Route("")] 
        [AcceptVerbs("GET", "POST")]
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

        [Route("instances/{instanceid}/bookmarks/{bookmark}")]
        [AcceptVerbs("GET", "POST")]
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