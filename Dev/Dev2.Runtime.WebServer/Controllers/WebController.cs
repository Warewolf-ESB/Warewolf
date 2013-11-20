using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Http;
using HttpFramework;
using Unlimited.Applications.WebServer;

namespace Dev2.Runtime.WebServer.Controllers
{
    public abstract class WebController : ApiController
    {
        WebRequestHandler _webRequestHandler = new WebRequestHandler();

     

        public virtual HttpResponseMessage Get(string website, string path)
        {
            var user = User;
            if(user == null || !user.Identity.IsAuthenticated)
            {
                var response = Request.CreateResponse(HttpStatusCode.Unauthorized);
                return response;
            }

         //   ICommunicationContext ctx = new OwinCommunicationContext();

            //_webRequestHandler.GetWebResource(ctx);

            return null;
        }

    }
}