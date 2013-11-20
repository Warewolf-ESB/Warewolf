using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Dev2.Runtime.WebServer.Controllers
{
    public abstract class WebController : ApiController
    {

        public virtual HttpResponseMessage Get(string website, string path)
        {
            var user = User;
            if(user == null || !user.Identity.IsAuthenticated)
            {
                var response = Request.CreateResponse(HttpStatusCode.Unauthorized);
                return response;
            }

            var boundVariables = new NameValueCollection
            {
                { "website", website }, 
                { "path", path }
            };

            //   ICommunicationContext ctx = new OwinCommunicationContext();

            //_webRequestHandler.GetWebResource(ctx);

            return null;
        }

        //HttpResponseMessage ProcessRequest(NameValueCollection boundVariables)
        //{
        //    var user = User;
        //    if(user == null || !user.Identity.IsAuthenticated)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.Unauthorized);
        //    }

        //    var request = new WebCommunicationRequest(Request, boundVariables);
        //    //var context = new WebCommunicationContext(request);

        //    var webRequestHandler = new WebRequestHandler();
        //    webRequestHandler.GetWebResource(context);

        //    if(context.IsNotFound)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.NotFound);
        //    }

        //    //var content = File.ReadAllText(filePath);
        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    response.Content = new StringContent(context.StaticFileResponse);
        //    response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        //}

    }
}