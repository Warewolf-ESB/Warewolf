using System.Collections.Specialized;
using System.Net.Http;
using System.Web.Http;
using Dev2.Runtime.WebServer.Handlers;

namespace Dev2.Runtime.WebServer.Controllers
{
    [Authorize]
    [RoutePrefix("services/{servicename}")]
    public class WebController : AbstractController
    {
        [Route("{wid}")]
        public HttpResponseMessage Get(string serviceName, string wid)
        {
            var requestVariables = new NameValueCollection
            {
                { "servicename", serviceName }, 
                { "clientid", wid }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }
    }
}