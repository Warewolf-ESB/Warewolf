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
using System.Net.Http;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Moq;
#if NETFRAMEWORK
using Dev2.Runtime.WebServer.Controllers;
#else
using Microsoft.AspNetCore.Http;
using Dev2.Controller;
#endif

namespace Dev2.Tests.Runtime.WebServer.Controllers
{
    public class TestWebServerController : WebServerController
    {
        Action _verifyProcessRequestInvoked;

        public TestWebServerController(HttpMethod method, string requestUrl)
        {
#if NETFRAMEWORK
            Request = new HttpRequestMessage
            {
                Method = method,
                Content = new StringContent(""),
                RequestUri = new Uri(requestUrl)
            };
#else
            Request.Method = method.ToString();
            Request.Host = new HostString(new Uri(requestUrl).Host);
            Request.Path = new PathString(new Uri(requestUrl).AbsolutePath);
            Request.QueryString = QueryString.FromUriComponent(new Uri(requestUrl));
#endif
        }

#if NETFRAMEWORK
        public TestWebServerController(HttpMethod method)
        {
            Request = new HttpRequestMessage
            {
                Method = method,
                Content = new StringContent("")
            };
        }
#else
        public TestWebServerController(HttpMethod method) => Request.Method = method.ToString();
#endif

        public TRequestHandler TestCreateHandler<TRequestHandler>()
            where TRequestHandler : class, IRequestHandler, new()
        {
            return base.CreateHandler<TRequestHandler>();
        }

        public Type ProcessRequestHandlerType { get; private set; }
        public NameValueCollection ProcessRequestVariables { get; private set; }
        protected override HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables, bool isUrlWithTokenPrefix)
        {
            ProcessRequestHandlerType = typeof(TRequestHandler);
            ProcessRequestVariables = requestVariables;
            var result = base.ProcessRequest<TRequestHandler>(requestVariables, isUrlWithTokenPrefix);
            _verifyProcessRequestInvoked?.Invoke();
            return result;
        }

        protected override TRequestHandler CreateHandler<TRequestHandler>()
        {
            var handler = new Mock<TRequestHandler>();
            handler.Setup(h => h.ProcessRequest(It.IsAny<WebServerContext>())).Verifiable();
            _verifyProcessRequestInvoked = () => handler.Verify(h => h.ProcessRequest(It.IsAny<WebServerContext>()));
            return handler.Object;
        }

        protected override bool IsAuthenticated()
        {
            return true;
        }
    }
}
