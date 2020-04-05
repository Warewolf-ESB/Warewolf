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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    /// <summary>
    /// Summary description for InternalServiceRequestHandlerTest
    /// </summary>
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class InternalServiceRequestHandlerTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InternalServiceRequestHandler))]
        [ExpectedException(typeof(FormatException))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenMalformedConnectionId_ExpectException()
        {
            //------------Setup for test--------------------------
            var principle = new Mock<IPrincipal>();
            principle.Setup(p => p.Identity.Name).Returns("FakeUser");

            var eer = new EsbExecuteRequest
            {
                ServiceName = "Ping",
            };
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = principle.Object };
            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, "1", null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InternalServiceRequestHandler))]
        [ExpectedException(typeof(Exception))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenNullExecutingUserInFirstOverload_ExpectException()
        {
            //------------Setup for test--------------------------
            var ctx = new Mock<ICommunicationContext>();
            var boundVariables = new NameValueCollection { { "servicename", "ping" }, { "instanceid", "" }, { "bookmark", "" } };
            var queryString = new NameValueCollection { { GlobalConstants.DLID, Guid.Empty.ToString() }, { "wid", Guid.Empty.ToString() } };
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost"));

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = null };

            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(ctx.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InternalServiceRequestHandler))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenExecutingUser()
        {
            //------------Setup for test--------------------------
            var ctx = new Mock<ICommunicationContext>();
            var boundVariables = new NameValueCollection { { "servicename", "ping" }, { "instanceid", "" }, { "bookmark", "" } };
            var queryString = new NameValueCollection { { GlobalConstants.DLID, Guid.Empty.ToString() }, { "wid", Guid.Empty.ToString() } };
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost:3142/secure/Testing123.json?<DataList></DataList>&wid=7481a128-cf36-427c-90d7-daa32368af8d"));
            var executingUser = new Mock<IPrincipal>();
            var id = new Mock<IIdentity>();
            id.Setup(identity => identity.Name).Returns("User");
            id.Setup(identity => identity.IsAuthenticated).Returns(true);
            executingUser.Setup(principal => principal.Identity).Returns(id.Object);
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = executingUser.Object };
            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(ctx.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InternalServiceRequestHandler))]
        [ExpectedException(typeof(Exception))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenNullExecutingUser_ExpectException()
        {
            //------------Setup for test--------------------------
            var args = new Dictionary<string, StringBuilder>
            {
                { "DebugPayload", new StringBuilder("<DataList>Value:SomeStringAsValue,IsDebug:true</DataList>") }
            };
            var eer = new EsbExecuteRequest
            {
                ServiceName = "Ping",
                Args = args,
                ResourceID = Guid.NewGuid()
            };

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = null };
            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString(), null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InternalServiceRequestHandler))]
        public void InternalServiceRequestHandler_ProcessRequest()
        {
            //------------Setup for test--------------------------
            var args = new Dictionary<string, StringBuilder>
            {
                { "DebugPayload", new StringBuilder("<DataList>Value:SomeStringAsValue,IsDebug:true</DataList>") },
                { "IsDebug", new StringBuilder("<DataList>true</DataList>") }
            };
            var eer = new EsbExecuteRequest
            {
                ServiceName = "Ping",
                Args = args,
            };

            var executingUser = new Mock<IPrincipal>();
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = executingUser.Object };
            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString(), null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InternalServiceRequestHandler))]
        public void InternalServiceRequestHandler_ProcessRequestGivenIsServiceTestExecution()
        {
            //------------Setup for test--------------------------
            var args = new Dictionary<string, StringBuilder>
            {
                { "DebugPayload", new StringBuilder("<DataList>Value:SomeStringAsValue,IsDebug:true</DataList>") },
                { "IsDebug", new StringBuilder("<DataList>true</DataList>") }
            };
            var eer = new EsbExecuteRequest
            {
                ServiceName = "Ping",
                Args = args,
                TestName = "Test1",
                ExecuteResult = new StringBuilder("Results")
            };

            var executingUser = new Mock<IPrincipal>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Contribute, Guid.Empty.ToString())).Returns(true);
            var internalServiceRequestHandler = new InternalServiceRequestHandler(resourceCatalog.Object, authorizationService.Object) { ExecutingUser = executingUser.Object };
            //------------Execute Test---------------------------
            var processRequest = internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString(), null);
            Assert.IsNotNull(processRequest);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InternalServiceRequestHandler))]
        public void InternalServiceRequestHandler_ProcessRequestGivenUnAuthorizedPermission()
        {
            //------------Setup for test--------------------------
            var args = new Dictionary<string, StringBuilder>
            {
                { "DebugPayload", new StringBuilder("<DataList>Value:SomeStringAsValue,IsDebug:true</DataList>") },
                { "IsDebug", new StringBuilder("<DataList>true</DataList>") },
            };
            var eer = new EsbExecuteRequest
            {
                ServiceName = "Ping",
                Args = args,
                TestName = "Test1",
                ExecuteResult = new StringBuilder("Results")
            };

            var executingUser = new Mock<IPrincipal>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Contribute, Guid.Empty.ToString())).Returns(false);
            var internalServiceRequestHandler = new InternalServiceRequestHandler(resourceCatalog.Object, authorizationService.Object) { ExecutingUser = executingUser.Object };
            //------------Execute Test---------------------------
            var processRequest = internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString(), null);
            authorizationService.Verify(service => service.IsAuthorized(AuthorizationContext.Contribute, Guid.Empty.ToString()), Times.Once);
            Assert.IsNotNull(processRequest);
        }
    }
}
