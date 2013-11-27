using System;
using System.Collections.Specialized;
using System.Net.Http;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Handlers;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Controllers
{
    public class TestWebServerController : WebServerController
    {
        Action _verifyProcessRequestInvoked;

        public TestWebServerController(HttpMethod method)
        {
            Request = new HttpRequestMessage
            {
                Method = method,
                Content = new StringContent("")
            };
        }

        public TRequestHandler TestCreateHandler<TRequestHandler>()
            where TRequestHandler : class, IRequestHandler, new()
        {
            return base.CreateHandler<TRequestHandler>();
        }

        public Type ProcessRequestHandlerType { get; private set; }
        public NameValueCollection ProcessRequestVariables { get; private set; }
        protected override HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables)
        {
            ProcessRequestHandlerType = typeof(TRequestHandler);
            ProcessRequestVariables = requestVariables;
            var result = base.ProcessRequest<TRequestHandler>(requestVariables);
            _verifyProcessRequestInvoked();
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
