using System.Net.Http;
using System.Web.Http;

namespace Dev2.Runtime.WebServer.Controllers
{
    [Authorize]
    public class ViewsController : WebController
    {
        public override HttpResponseMessage Get(string website, string path)
        {
            return base.Get(website, path);
        }
    }
}