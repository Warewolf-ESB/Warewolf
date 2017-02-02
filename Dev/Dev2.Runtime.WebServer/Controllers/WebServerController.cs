/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Security;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.WebServer.Controllers
{
    [AuthorizeWeb]
    public class WebServerController : AbstractController
    {
        [HttpGet]
        [Route("{website}/decisions/{file}")]
        public HttpResponseMessage GetDecisions(string website, string file)
        {
            return Get(website, "decisions", file);
        }

        [HttpGet]
        [Route("{website}/dialogs/{file}")]
        public HttpResponseMessage GetDialogs(string website, string file)
        {
            return Get(website, "dialogs", file);
        }

        [HttpGet]
        [Route("{website}/services/{file}")]
        public HttpResponseMessage GetServices(string website, string file)
        {
            return Get(website, "services", file);
        }

        [HttpGet]
        [Route("{website}/sources/{file}")]
        public HttpResponseMessage GetSources(string website, string file)
        {
            return Get(website, "sources", file);
        }

        [HttpGet]
        [Route("{website}/switch/{file}")]
        public HttpResponseMessage GetSwitch(string website, string file)
        {
            return Get(website, "switch", file);
        }

        [HttpGet]
        [Route("{website}/{folder}/{file}")]
        public HttpResponseMessage Get(string website, string folder, string file)
        {
            // DO NOT replace {folder} with {type} in route mapping --> {type} is a query string parameter!
            var requestVariables = new NameValueCollection
            {
                { "website", website },
                { "path", $"{folder}/{file}" }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpGet]
        [Route("{website}/{path}/{folder}/{*file}")]
        public HttpResponseMessage Get(string website, string path, string folder, string file)
        {
            // DO NOT replace {folder} with {type} in route mapping --> {type} is a query string parameter!
            var requestVariables = new NameValueCollection
            {
                { "website", website },
                { "path", path }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpGet]
        [Route("{website}/content/{*file}")]
        public HttpResponseMessage GetContent(string website, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website },
                { "path", $"content/{file}" }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpGet]
        [Route("{website}/images/{*file}")]
        public HttpResponseMessage GetImage(string website, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website },
                { "path", $"images/{file}" }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpGet]
        [Route("{website}/scripts/{*file}")]
        public HttpResponseMessage GetScript(string website, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website },
                { "path", $"scripts/{file}" }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpGet]
        [Route("{website}/views/{*file}")]
        public HttpResponseMessage GetView(string website, string file)
        {
            var requestVariables = new NameValueCollection
            {
                { "website", website },
                { "path", $"views/{file}" }
            };

            return ProcessRequest<WebsiteResourceHandler>(requestVariables);
        }

        [HttpPost]
        [Route("{__website__}/{__path__}/Service/{__name__}/{__method__}")]
        public HttpResponseMessage InvokeService(string __website__, string __path__, string __name__, string __method__)
        {
            // DO NOT replace {method} with {action} in route mapping --> {action} is a reserved placeholder!
            var requestVariables = new NameValueCollection
            {
                { "website", __website__ },
                { "path", __path__ },
                { "name", __name__ },
                { "action", __method__ },
            };

            return ProcessRequest<WebsiteServiceHandler>(requestVariables);
        }

        [HttpGet]
        [HttpPost]
        [Route("Services/{*__name__}")]
        public HttpResponseMessage ExecuteService(string __name__)
        {
            return ExecuteWorkflow(__name__, false);
        }

        private HttpResponseMessage ExecuteWorkflow(string __name__, bool isPublic)
        {

            if (__name__.EndsWith("apis.json"))
            {
                var path = __name__.Split(new[] { "/apis.json" }, StringSplitOptions.RemoveEmptyEntries);
                if (path.Any())
                {
                    if (path[0].Equals("apis.json", StringComparison.OrdinalIgnoreCase))
                    {
                        path[0] = null;
                    }

                }
                var requestVar = new NameValueCollection
                {
                    {"path", path[0]},
                    {"isPublic",isPublic.ToString()}
                };
                return ProcessRequest<GetApisJsonServiceHandler>(requestVar);
            }
            var requestVariables = new NameValueCollection
            {
                { "servicename", __name__ }
            };


            return Request.Method == HttpMethod.Post
                ? ProcessRequest<WebPostRequestHandler>(requestVariables)
                : ProcessRequest<WebGetRequestHandler>(requestVariables);
        }

        private HttpResponseMessage ExecuteFolderTests(string __url__, bool isPublic)
        {

            var requestVariables = new NameValueCollection
            {
                { "path", __url__ },
                {"isPublic",isPublic.ToString()},
                {"servicename","*" }
            };

            var httpResponseMessage = ProcessRequest<WebGetRequestHandler>(requestVariables);
            return httpResponseMessage;
        }


        [HttpGet]
        [HttpPost]
        [Route("Secure/{*__name__}")]
        public HttpResponseMessage ExecuteSecureWorkflow(string __name__)
        {
            if (Request?.RequestUri != null)
            {
                var requestUri = Request.RequestUri;
                if (requestUri.ToString().EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase))
                    return ExecuteFolderTests(requestUri.ToString(), false);
            }
            return ExecuteWorkflow(__name__, false);
        }


        [HttpGet]
        [HttpPost]
        [Route("Public/{*__name__}")]
        public HttpResponseMessage ExecutePublicWorkflow(string __name__)
        {
            if (Request?.RequestUri != null)
            {
                var requestUri = Request.RequestUri;
                if (requestUri.ToString().EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase))
                    return ExecuteFolderTests(requestUri.ToString(), true);
            }
            return ExecuteWorkflow(__name__, true);
        }

        [HttpGet]
        [HttpPost]
        [Route("internal/getlogfile")]
        public HttpResponseMessage ExecuteGetLogFile()
        {
            return ProcessRequest<GetLogFileServiceHandler>();
        }

        [HttpGet]
        [HttpPost]
        [Route("apis.json")]
        public HttpResponseMessage ExecuteGetRootLevelApisJson()
        {
            var requestVariables = new NameValueCollection();
            return ProcessRequest<GetApisJsonServiceHandler>(requestVariables);
        }

    }
}
