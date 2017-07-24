/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
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

            var eer = new EsbExecuteRequest
            {
                ServiceName = "Ping",
            };
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
        public void InternalServiceRequestHandler_ProcessRequest_WhenExecutingUser()
        {
            //------------Setup for test--------------------------
            Mock<ICommunicationContext> ctx = new Mock<ICommunicationContext>();
            NameValueCollection boundVariables = new NameValueCollection { { "servicename", "ping" }, { "instanceid", "" }, { "bookmark", "" } };
            NameValueCollection queryString = new NameValueCollection { { GlobalConstants.DLID, Guid.Empty.ToString() }, { "wid", Guid.Empty.ToString() } };
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
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
        [ExpectedException(typeof(Exception))]
        public void InternalServiceRequestHandler_ProcessRequest_WhenNullExecutingUser_ExpectException()
        {
            //------------Setup for test--------------------------
            var args = new Dictionary<string, StringBuilder>
            {
                { "DebugPayload", new StringBuilder("<DataList>Value:SomeStringAsValue,IsDebug:true</DataList>") }
            };
            EsbExecuteRequest eer = new EsbExecuteRequest
            {
                ServiceName = "Ping",
                Args = args,
                ResourceID = Guid.NewGuid()
            };

            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = null };
            //------------Execute Test---------------------------
            internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString());

        }
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
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
            internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString());

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildStudioUrl_GivenPayLoad_BuildCorrect_WebURL()
        {
            //------------Setup for test--------------------------
            var executingUser = new Mock<IPrincipal>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Contribute, Guid.Empty.ToString())).Returns(true);
            var internalServiceRequestHandler = new InternalServiceRequestHandler(resourceCatalog.Object, authorizationService.Object) { ExecutingUser = executingUser.Object };

            var privateObject = new PrivateObject(internalServiceRequestHandler);
            var xElement = XML.XmlResource.Fetch("DebugPayload");
            var s = xElement.ToString();
            var invoke = privateObject.Invoke("BuildStudioUrl", s).ToString();
            //------------Execute Test---------------------------
            Assert.IsNotNull(invoke);
            Assert.IsFalse(invoke.Contains(" "));
            Assert.IsFalse(invoke.Contains(Environment.NewLine));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildStudioUrl_GivenPayLoad_BuildCorrect_WebURL_StripDebugInfo()
        {
            //------------Setup for test--------------------------
            var executingUser = new Mock<IPrincipal>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Contribute, Guid.Empty.ToString())).Returns(true);
            var internalServiceRequestHandler = new InternalServiceRequestHandler(resourceCatalog.Object, authorizationService.Object) { ExecutingUser = executingUser.Object };

            var privateObject = new PrivateObject(internalServiceRequestHandler);
            var xElement = XML.XmlResource.Fetch("DebugPayload");
            var s = xElement.ToString();
            //------------Test Preconditions---------------------------
            var invoke = privateObject.Invoke("BuildStudioUrl", s).ToString();
            Assert.IsNotNull(invoke);
            Assert.IsFalse(invoke.Contains(" "));
            Assert.IsFalse(invoke.Contains(Environment.NewLine));
            //------------Execute Test---------------------------
            Assert.IsFalse(invoke.Contains("BDSDebugMode"));
            Assert.IsFalse(invoke.Contains("DebugSessionID"));
            Assert.IsFalse(invoke.Contains("EnvironmentID"));
            const string expected = "<DataList><input>a</input><rec%20json:Array=\"true\"%20xmlns:json=\"http://james.newtonking.com/projects/json\"><field>w</field></rec><obj><Name>nathi</Name></obj></DataList>";
            Assert.AreEqual( expected, invoke);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
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
            var processRequest = internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString());
            Assert.IsNotNull(processRequest);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InternalServiceRequestHandler_ProcessRequest")]
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
            var processRequest = internalServiceRequestHandler.ProcessRequest(eer, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString());
            Assert.IsNotNull(processRequest);
        }
    }
}
