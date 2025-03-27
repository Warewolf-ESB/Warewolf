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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Security;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
#endif

namespace Dev2.Runtime.WebServer.Controllers
{
	/**
     * Entry point for web based executions of workflows. This is the entrypoint for any
     * request that comes from an HTTP, that includes REST and a user executing a workflow
     * from a web browser
     */

#if NETFRAMEWORK
	[AuthorizeWeb]
#else
    [CustomActionFilter]
    [ApiController]
    [Route("")]
#endif
	public class WebServerController : AbstractController
    {
        [HttpGet]
        [HttpPost]
        [Route("Services/{*__name__}")]
#if NETFRAMEWORK
		public HttpResponseMessage ExecuteService(string __name__) => ExecuteWorkflow(__name__, false, false);

		HttpResponseMessage ExecuteWorkflow(string __name__, bool isPublic, bool isUrlWithTokenPrefix)
#else
		public ActionResult ExecuteService(string __name__) => ExecuteWorkflow(__name__, false, false);

		ActionResult ExecuteWorkflow(string __name__, bool isPublic, bool isUrlWithTokenPrefix)
#endif
		{
			if (__name__.EndsWith("apis.json", StringComparison.OrdinalIgnoreCase))
            {
                var path = __name__.Split(new[] { "/apis.json" }, StringSplitOptions.RemoveEmptyEntries);
                if (path.Any() && path[0].Equals("apis.json", StringComparison.OrdinalIgnoreCase))
                {
                    path[0] = null;
                }

                var requestVar = new NameValueCollection
                {
                    {"path", path[0]},
                    {"isPublic", isPublic.ToString()}
                };
#if NETFRAMEWORK
				return ProcessRequest<GetApisJsonServiceHandler>(requestVar, isUrlWithTokenPrefix);
#else
				return ProcessRequest<GetApisJsonServiceHandler>(requestVar, isUrlWithTokenPrefix).ToActionResult();
#endif
			}

			if (__name__.EndsWith(".api", StringComparison.OrdinalIgnoreCase))
            {
                var path = __name__.Split(new[] { "/.api" }, StringSplitOptions.RemoveEmptyEntries);
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
#if NETFRAMEWORK
				return ProcessRequest<GetOpenAPIServiceHandler>(requestVar, isUrlWithTokenPrefix);
#else
				return ProcessRequest<GetOpenAPIServiceHandler>(requestVar, isUrlWithTokenPrefix).ToActionResult();
#endif
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

#if NETFRAMEWORK
			return Request.Method == HttpMethod.Post
				? ProcessRequest<WebPostRequestHandler>(requestVariables, isUrlWithTokenPrefix)
				: ProcessRequest<WebGetRequestHandler>(requestVariables, isUrlWithTokenPrefix);
#else
            return Request.Method == HttpMethod.Post.ToString()
                ? ProcessRequest<WebPostRequestHandler>(requestVariables, isUrlWithTokenPrefix).ToActionResult()
                : ProcessRequest<WebGetRequestHandler>(requestVariables, isUrlWithTokenPrefix).ToActionResult();
#endif
		}

#if NETFRAMEWORK
		public HttpResponseMessage ExecuteFolderTests(string url, bool isPublic)
#else
		public ActionResult ExecuteFolderTests(string url, bool isPublic)
#endif
		{
			var requestVariables = new NameValueCollection
            {
                {"path", url},
                {"isPublic", isPublic.ToString()},
                {"servicename", "*"}
            };

            var httpResponseMessage = ProcessRequest<WebGetRequestHandler>(requestVariables, false);
#if NETFRAMEWORK
			return httpResponseMessage;
#else
			return httpResponseMessage.ToActionResult();
#endif
		}

		[HttpGet]
        [HttpPost]
        [Route("Secure/{*__name__}")]
#if NETFRAMEWORK
		public HttpResponseMessage ExecuteSecureWorkflow(string __name__)
		{
			if (Request?.RequestUri != null)
			{
				var requestUri = Request.RequestUri;
#else
		public ActionResult ExecuteSecureWorkflow(string __name__)
		{
			if (Request?.ToUri() != null)
			{
				var requestUri = Request.ToUri();
#endif
				if (requestUri.ToString().EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), false);
                }

                if (requestUri.ToString().EndsWith("/.coverage", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.json", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.coverage.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), false);
                }
            }

            return ExecuteWorkflow(__name__, false, false);
        }

        [HttpGet]
        [HttpPost]
        [Route("Public/{*__name__}")]
#if NETFRAMEWORK
		public HttpResponseMessage ExecutePublicWorkflow(string __name__)
#else
		public ActionResult ExecutePublicWorkflow(string __name__)
#endif
		{
#if NETFRAMEWORK
			if (Request?.RequestUri != null)
#else
			if (Request?.ToUri() != null)
#endif
			{
#if NETFRAMEWORK
				var requestUri = Request.RequestUri;
#else
				var requestUri = Request.ToUri();
#endif
				if (requestUri.ToString().EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase) || requestUri.ToString().EndsWith("/.tests.trx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ExecuteFolderTests(requestUri.ToString(), true);
                }
            }

            return ExecuteWorkflow(__name__, true, false);
        }

        [HttpGet]
        [HttpPost]
        [Route("Token/{*__name__}")]
#if NETFRAMEWORK
		public HttpResponseMessage ExecutePublicTokenWorkflow(string __name__)
#else
        public ActionResult ExecutePublicTokenWorkflow(string __name__)
#endif
		{
			return ExecuteWorkflow(__name__, false, true);
        }

        [HttpGet]
        [HttpPost]
        [Route("login")]
#if NETFRAMEWORK
		public HttpResponseMessage ExecuteLoginWorkflow()
		{
			var requestVariables = new NameValueCollection();
			var context = new WebServerContext(Request, requestVariables) { Request = { User = User } };
			var handler = CreateHandler<TokenRequestHandler>();
			handler.ProcessRequest(context);
			return context.ResponseMessage;
		}
#else
        public ActionResult ExecuteLoginWorkflow()
		{
			var r = Request.HttpContext.GetHttpRequestMessage();
            var requestVariables = new NameValueCollection();
            var context = new WebServerContext(r, requestVariables) { Request = { User = User } };
            var handler = CreateHandler<TokenRequestHandler>();
            handler.ProcessRequest(context);
            return context.ResponseMessage.ToActionResult();
        }
#endif

		[HttpGet]
        [HttpPost]
        [Route("internal/getlogfile")]
#if NETFRAMEWORK
		public HttpResponseMessage ExecuteGetLogFile() => ProcessRequest<GetLogFileServiceHandler>();
#else
        public ActionResult ExecuteGetLogFile() => ProcessRequest<GetLogFileServiceHandler>().ToActionResult();
#endif

		[HttpGet]
        [HttpPost]
        [Route("apis.json")]
#if NETFRAMEWORK
		public HttpResponseMessage ExecuteGetRootLevelApisJson()
#else
        public ActionResult ExecuteGetRootLevelApisJson()
#endif
		{
			var requestVariables = new NameValueCollection();
#if NETFRAMEWORK
			return ProcessRequest<GetApisJsonServiceHandler>(requestVariables, false);
#else
			return ProcessRequest<GetApisJsonServiceHandler>(requestVariables, false).ToActionResult();
#endif
		}
	}


}