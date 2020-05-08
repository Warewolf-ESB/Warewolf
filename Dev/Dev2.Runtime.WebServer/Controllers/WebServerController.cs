#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
    /**
     * Entry point for web based executions of workflows. This is the entrypoint for any
     * request that comes from an HTTP, that includes REST and a user executing a workflow
     * from a web browser
     */
    [AuthorizeWeb]
    public class WebServerController : AbstractController
    {
        [HttpGet]
        [HttpPost]
        [Route("Services/{*__name__}")]
        public HttpResponseMessage ExecuteService(string __name__) => ExecuteWorkflow(__name__, false, false);

        HttpResponseMessage ExecuteWorkflow(string __name__, bool isPublic, bool isToken)
        {
            if (__name__.EndsWith("apis.json", StringComparison.OrdinalIgnoreCase))
            {
                var path = __name__.Split(new[] {"/apis.json"}, StringSplitOptions.RemoveEmptyEntries);
                if (path.Any() && path[0].Equals("apis.json", StringComparison.OrdinalIgnoreCase))
                {
                    path[0] = null;
                }

                var requestVar = new NameValueCollection
                {
                    {"path", path[0]},
                    {"isToken", isToken.ToString()},
                    {"isPublic", isPublic.ToString()}
                };
                return ProcessRequest<GetApisJsonServiceHandler>(requestVar);
            }

            if (__name__.EndsWith(".debug", StringComparison.InvariantCultureIgnoreCase))
            {
                var requestVar = new NameValueCollection
                {
                    {"isPublic", isPublic.ToString()},
                    {"isToken", isToken.ToString()},
                    {"IsDebug", true.ToString()},
                    {"servicename", __name__}
                };
                return Request.Method == HttpMethod.Post
                    ? ProcessRequest<WebPostRequestHandler>(requestVar)
                    : ProcessRequest<WebGetRequestHandler>(requestVar);
            }

            var requestVariables = new NameValueCollection
            {
                {"servicename", __name__},
                {"isToken", isToken.ToString()},
            };

            return Request.Method == HttpMethod.Post
                ? ProcessRequest<WebPostRequestHandler>(requestVariables)
                : ProcessRequest<WebGetRequestHandler>(requestVariables);
        }

        public HttpResponseMessage ExecuteFolderTests(string __url__, bool isPublic, bool isToken)
        {
            var requestVariables = new NameValueCollection
            {
                {"path", __url__},
                {"isPublic", isPublic.ToString()},
                {"isToken", isToken.ToString()},
                {"servicename", "*"}
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
                    return ExecuteFolderTests(requestUri.ToString(), false, false);
                }

                if (requestUri.ToString().EndsWith("/.coverage", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.json", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), false, false);
                }
            }

            return ExecuteWorkflow(__name__, false, false);
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
                    return ExecuteFolderTests(requestUri.ToString(), true, false);
                }
            }

            return ExecuteWorkflow(__name__, true, false);
        }

        [HttpGet]
        [HttpPost]
        [Route("Token/{*__name__}")]
        public HttpResponseMessage ExecutePublicTokenWorkflow(string __name__)
        {
            if (Request?.RequestUri != null)
            {
                var requestUri = Request.RequestUri;
                if (requestUri.ToString().EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), false, true);
                }

                if (requestUri.ToString().EndsWith("/.coverage", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.json", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), false, true);
                }
            }

            return ExecuteWorkflow(__name__, false, true);
        }

        [HttpGet]
        [HttpPost]
        [Route("login")]
        public HttpResponseMessage ExecuteLoginWorkflow()
        {
            var requestVar = new NameValueCollection();
            return ProcessTokenRequest<TokenRequestHandler>(requestVar);
        }

        [HttpGet]
        [HttpPost]
        [Route("internal/getlogfile")]
        public HttpResponseMessage ExecuteGetLogFile() => ProcessRequest<GetLogFileServiceHandler>();

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