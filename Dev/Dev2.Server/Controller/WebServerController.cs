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

#if !NETFRAMEWORK
using System.Collections.Specialized;
using System.Linq;
using System;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Dev2.Controller
{
    /**
     * Entry point for web based executions of workflows. This is the entrypoint for any
     * request that comes from an HTTP, that includes REST and a user executing a workflow
     * from a web browser
     */
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ApiController]
    public class WebServerController : AbstractController
    {
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Services/{*__name__}")]
        public HttpResponseMessage ExecuteService(string __name__) => ExecuteWorkflow(__name__, false, false);

        HttpResponseMessage ExecuteWorkflow(string __name__, bool isPublic, bool isUrlWithTokenPrefix)
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
                    {"isPublic", isPublic.ToString()}
                };
                return ProcessRequest<GetApisJsonServiceHandler>(requestVar, isUrlWithTokenPrefix);
            }

            if (__name__.EndsWith(".api", StringComparison.OrdinalIgnoreCase))
            {
                var path = __name__.Split(new[] {"/.api"}, StringSplitOptions.RemoveEmptyEntries);
                if (path.Any() && path[0].Equals(".api", StringComparison.OrdinalIgnoreCase))
                {
                    path[0] = null;
                }
            
                var requestVar = new NameValueCollection
                {
                    {"servicename", __name__},
                    {"path", path[0]},
                    {"isPublic", isPublic.ToString()}
                };
                return ProcessRequest<GetOpenAPIServiceHandler>(requestVar, isUrlWithTokenPrefix);
            }
            
            var requestVariables = new NameValueCollection
            {
                {"servicename", __name__},
            };
            if (__name__.EndsWith(".debug", StringComparison.InvariantCultureIgnoreCase))
            {
                requestVariables.Add("isPublic", isPublic.ToString());
                requestVariables.Add("IsDebug", true.ToString());
            }

            return Request.Method == HttpMethod.Post.ToString()
                ? ProcessRequest<WebPostRequestHandler>(requestVariables, isUrlWithTokenPrefix)
                : ProcessRequest<WebGetRequestHandler>(requestVariables, isUrlWithTokenPrefix);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Secure/{*__name__}")]
        public string ExecuteSecureWorkflow(string __name__)
        {
            if (Request.GetDisplayUrl() != null)
            {
                var requestUri = Request.GetDisplayUrl();
                if (requestUri.EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase) || requestUri.EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri, false).ToString();
                }

                if (requestUri.EndsWith("/.coverage", StringComparison.InvariantCultureIgnoreCase) || requestUri.EndsWith("/.coverage.json", StringComparison.InvariantCultureIgnoreCase) || requestUri.EndsWith("/.coverage.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri, false).ToString();
                }
            }
            var response = ExecuteWorkflow(__name__, false, false);
            if ((int)response.StatusCode != 200)
            {
                return response.ToString();
            }
            return response.Content.ReadAsStringAsync().Result;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Public/{*__name__}")]
        public string ExecutePublicWorkflow(string __name__)
        {
            if (Request?.GetDisplayUrl() != null)
            {
                var requestUri = Request.GetDisplayUrl();
                if (requestUri.EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase) || requestUri.EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri, true).ToString();
                }
            }
            var response = ExecuteWorkflow(__name__, true, false);
            if ((int)response.StatusCode != 200)
            {
                return response.ToString();
            }
            return response.Content.ReadAsStringAsync().Result;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Token/{*__name__}")]
        public HttpResponseMessage ExecutePublicTokenWorkflow(string __name__)
        {
            return ExecuteWorkflow(__name__, false, true);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("login")]
        public HttpResponseMessage ExecuteLoginWorkflow()
        {
            var requestVariables = new NameValueCollection();
            var context = new WebServerContext(new HttpRequestMessage(ConvertStringToHttpMethod(Request.Method), Request.GetDisplayUrl()), requestVariables) {Request = {User = User}};
            var handler = CreateHandler<TokenRequestHandler>();
            handler.ProcessRequest(context);
            return context.ResponseMessage;
        }

        internal static HttpMethod ConvertStringToHttpMethod(string methodString)
        {
            switch (methodString)
            {
                case "Get": return HttpMethod.Get;
                case "Post": return HttpMethod.Post;
                case "Delete": return HttpMethod.Delete;
                case "Put": return HttpMethod.Put;
                default: return null;
            }
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("internal/getlogfile")]
        public HttpResponseMessage ExecuteGetLogFile() => ProcessRequest<GetLogFileServiceHandler>();

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("apis.json")]
        public HttpResponseMessage ExecuteGetRootLevelApisJson()
        {
            var requestVariables = new NameValueCollection();
            return ProcessRequest<GetApisJsonServiceHandler>(requestVariables, false);
        }

        HttpResponseMessage ExecuteFolderTests(string url, bool isPublic)
        {
            var requestVariables = new NameValueCollection
            {
                {"path", url},
                {"isPublic", isPublic.ToString()},
                {"servicename", "*"}
            };

            var httpResponseMessage = ProcessRequest<WebGetRequestHandler>(requestVariables, false);
            return httpResponseMessage;
        }
    }
}
#endif