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
using System.Security.Claims;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class TokenRequestHandlerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [ExpectedException(typeof(NullReferenceException))]
        public void TokenRequestHandler_ProcessRequest_GiveNullCommunicationContext_ThrowsException()
        {
            //------------Setup for test-------------------------
            var handler = new TokenRequestHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        public void TokenRequestHandler_ProcessRequest_ReturnToken()
        {
            NameValueCollection localQueryString = new NameValueCollection();

            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.BoundVariables).Returns(localQueryString);

            var response = new Mock<ICommunicationResponse>();
            request.Setup(communicationResponse => communicationResponse.ContentType).Returns(ContentTypes.Json.ToString);

            communicationContext.Setup(context => context.Request).Returns(request.Object);
            communicationContext.Setup(context => context.Response).Returns(response.Object);
            //------------Setup for test-------------------------
            var handler = new TokenRequestHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            var result = communicationContext.Object.Response;
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
        }
    }
}