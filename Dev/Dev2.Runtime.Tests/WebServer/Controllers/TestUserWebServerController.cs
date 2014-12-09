
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Handlers;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Controllers
{
    public class TestUserWebServerController : WebServerController
    {
        readonly IPrincipal _user;

        public TestUserWebServerController(HttpMethod method, IPrincipal user)
        {
            _user = user;
            Request = new HttpRequestMessage
            {
                Method = method,
                Content = new StringContent("")
            };
        }

        public HttpResponseMessage TestProcessRequest()
        {
            return ProcessRequest<WebsiteResourceHandler>(new NameValueCollection());
        }

        protected override HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables)
        {
            User = _user;
            var result = base.ProcessRequest<TRequestHandler>(requestVariables);
            return result;
        }

        protected override TRequestHandler CreateHandler<TRequestHandler>()
        {
            var handler = new Mock<TRequestHandler>();
            handler.Setup(h => h.ProcessRequest(It.IsAny<WebServerContext>())).Callback((ICommunicationContext context) =>
            {
                ((WebServerContext)context).ResponseMessage.StatusCode = HttpStatusCode.OK;
            });
            return handler.Object;
        }
    }
}
