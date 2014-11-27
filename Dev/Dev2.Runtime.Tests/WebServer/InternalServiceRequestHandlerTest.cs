
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Specialized;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    /// <summary>
    /// Summary description for InternalServiceRequestHandlerTest
    /// </summary>
    [TestClass]
    public class InternalServiceRequestHandlerTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
        [ExpectedException(typeof(FormatException))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenMalformedConnectionId_ExpectException()
        {
            //------------Setup for test--------------------------
            Mock<IPrincipal> principle = new Mock<IPrincipal>();
            principle.Setup(p => p.Identity.Name).Returns("FakeUser");
            EsbExecuteRequest eer = new EsbExecuteRequest { ServiceName = "Ping" };

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = principle.Object };

            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, "1");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
        [ExpectedException(typeof(Exception))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenNullExecutingUserInFirstOverload_ExpectException()
        {
            //------------Setup for test--------------------------
            Mock<ICommunicationContext> ctx = new Mock<ICommunicationContext>();
            NameValueCollection boundVariables = new NameValueCollection { { "servicename", "ping" }, { "instanceid", "" }, { "bookmark", "" } };
            NameValueCollection queryString = new NameValueCollection { { GlobalConstants.DLID, Guid.Empty.ToString() }, { "wid", Guid.Empty.ToString() } };
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost"));

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = null };

            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(ctx.Object);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
        [ExpectedException(typeof(Exception))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenNullExecutingUser_ExpectException()
        {
            //------------Setup for test--------------------------
            EsbExecuteRequest eer = new EsbExecuteRequest { ServiceName = "Ping" };

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = null };

            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString());

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
        public void InternalServiceRequestHandler_ProcessRequest_WhenPassingInUserContext_ExpectThreadHasCorrectUserContext()
        {
            //------------Setup for test--------------------------
            Mock<IPrincipal> principle = new Mock<IPrincipal>();
            principle.Setup(p => p.Identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity.Name).Verifiable();
            EsbExecuteRequest eer = new EsbExecuteRequest { ServiceName = "Ping" };

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = principle.Object };

            //------------Execute Test---------------------------
            var result = internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.Empty.ToString());

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            StringAssert.Contains(result.ToString(), "Pong");
            principle.Verify(p => p.Identity.Name, Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
        public void InternalServiceRequestHandler_ProcessRequest_WhenExecutingUserInFirstOverload_ExpectThreadHasCorrectUserContext()
        {
            //------------Setup for test--------------------------
            Mock<IPrincipal> principle = new Mock<IPrincipal>();
            principle.Setup(p => p.Identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity.Name).Verifiable();

            Mock<ICommunicationContext> ctx = new Mock<ICommunicationContext>();
            NameValueCollection boundVariables = new NameValueCollection { { "servicename", "ping" }, { "instanceid", "" }, { "bookmark", "" } };
            NameValueCollection queryString = new NameValueCollection { { GlobalConstants.DLID, Guid.Empty.ToString() }, { "wid", Guid.Empty.ToString() } };
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost"));

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = principle.Object };

            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(ctx.Object);

            //------------Assert Results-------------------------
            principle.Verify(p => p.Identity.Name, Times.AtLeast(1));

        }

    }
}
