#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        public HttpResponseMessage ExecuteService(string __name__) => ExecuteWorkflow(__name__, false);

        HttpResponseMessage ExecuteWorkflow(string __name__, bool isPublic)
        {
          switch (__name__)
          {
            case var _ when __name__.EndsWith("apis.json"):
              return ApiHttpResponseMessage(__name__, isPublic);
            case var _ when __name__.EndsWith(".debug"):
              return DebugHttpResponseMessage(__name__, isPublic);
            default:
              return WebHttpResponseMessage(__name__);
          }
        }

        private HttpResponseMessage WebHttpResponseMessage(string __name__)
        {
          var requestVariables = new NameValueCollection
                                 {
                                   {"servicename", __name__}
                                 };
          return Request.Method == HttpMethod.Post
                   ? ProcessRequest<WebPostRequestHandler>(requestVariables)
                   : ProcessRequest<WebGetRequestHandler>(requestVariables);
        }

        private HttpResponseMessage DebugHttpResponseMessage(string __name__, bool isPublic)
        {
          var requestVar = new NameValueCollection
                           {
                             {"isPublic", isPublic.ToString()},
                             {"IsDebug", true.ToString()},
                             {"servicename", __name__}
                           };
          return Request.Method == HttpMethod.Post
                   ? ProcessRequest<WebPostRequestHandler>(requestVar)
                   : ProcessRequest<WebGetRequestHandler>(requestVar);
        }

        private HttpResponseMessage ApiHttpResponseMessage(string __name__, bool isPublic)
        {
          var path = __name__.Split(new[] {"/apis.json"}, StringSplitOptions.RemoveEmptyEntries);
          if (path.Any() && path[0].Equals("apis.json", StringComparison.OrdinalIgnoreCase))
          {
            path[0] = null;
          }

          var requestVar = new NameValueCollection
                           {
                             {"path", path[0]},
                             {"isPublic", isPublic.ToString()}
                           };
          return ProcessRequest<GetApisJsonServiceHandler>(requestVar);
        }

        public HttpResponseMessage ExecuteFolderTests(string __url__, bool isPublic)
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
                if (requestUri.ToString().EndsWith("/.coverage", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.json", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.trx", StringComparison.InvariantCultureIgnoreCase))
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
