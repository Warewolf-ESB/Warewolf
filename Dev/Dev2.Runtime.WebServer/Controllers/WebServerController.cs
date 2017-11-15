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


namespace Dev2.Runtime.WebServer.Controllers
{
    [AuthorizeWeb]
    public class WebServerController : AbstractController
    {
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
            if (__name__.EndsWith(".debug", StringComparison.InvariantCultureIgnoreCase))
            {
                var requestVar = new NameValueCollection
                {
                    {"isPublic",isPublic.ToString()},
                    {"IsDebug",true.ToString() },
                    {"servicename",__name__ }
                };
                return Request.Method == HttpMethod.Post
                        ? ProcessRequest<WebPostRequestHandler>(requestVar)
                        : ProcessRequest<WebGetRequestHandler>(requestVar);
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
                if (requestUri.ToString().EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), false);
                }
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
                if (requestUri.ToString().EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), true);
                }
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
