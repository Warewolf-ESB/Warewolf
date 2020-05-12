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
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Security;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.WebServer
{
    /// <summary>
    /// Summary description for WebPostRequestHandlerTest
    /// </summary>
    [TestClass]
    [TestCategory(nameof(WebPostRequestHandler))]
    public class WebPostRequestHandlerTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var pCounter = new Mock<IWarewolfPerformanceCounterLocater>();
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>())).Returns(new EmptyCounter());
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<WarewolfPerfCounterType>())).Returns(new EmptyCounter());
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<string>())).Returns(new EmptyCounter());
            CustomContainer.Register(pCounter.Object);
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(WebPostRequestHandler))]
        public void WebPostRequestHandler_ProcessRequest_WhenValidUserContext_ExpectExecution()
        {
            //------------Setup for test--------------------------
            var principle = new Mock<IPrincipal>();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(identity => identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity).Returns(mockIdentity.Object);
            principle.Setup(p => p.Identity.Name).Verifiable();
            principle.Setup(p => p.Identity).Returns(mockIdentity.Object).Verifiable();
            var old = Common.Utilities.OrginalExecutingUser;
            if (Common.Utilities.OrginalExecutingUser != null)
            {
                Common.Utilities.OrginalExecutingUser = principle.Object;
            }

            ClaimsPrincipal.ClaimsPrincipalSelector = () => new ClaimsPrincipal(principle.Object);
            ClaimsPrincipal.PrimaryIdentitySelector = identities => new ClaimsIdentity(mockIdentity.Object);
            var ctx = new Mock<ICommunicationContext>();
            var boundVariables = new NameValueCollection {{"servicename", "ping"}, {"instanceid", ""}, {"bookmark", ""}};
            var queryString = new NameValueCollection {{GlobalConstants.DLID, Guid.Empty.ToString()}, {"wid", Guid.Empty.ToString()}};
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost"));
            ctx.Setup(c => c.Request.User).Returns(principle.Object);

            var webPostRequestHandler = new WebPostRequestHandler();

            //------------Execute Test---------------------------
            webPostRequestHandler.ProcessRequest(ctx.Object);

            //------------Assert Results-------------------------
            mockIdentity.VerifyGet(identity => identity.Name, Times.AtLeast(1));
            Common.Utilities.OrginalExecutingUser = old;
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WebPostRequestHandler))]
        public void WebPostRequestHandler_ProcessRequest_isToken_True_ExpectExecution()
        {
            //------------Setup for test--------------------------
            var principle = new Mock<IPrincipal>();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(identity => identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity).Returns(mockIdentity.Object);
            principle.Setup(p => p.Identity.Name).Verifiable();
            principle.Setup(p => p.Identity).Returns(mockIdentity.Object).Verifiable();
            var old = Common.Utilities.OrginalExecutingUser;
            if (Common.Utilities.OrginalExecutingUser != null)
            {
                Common.Utilities.OrginalExecutingUser = principle.Object;
            }

            ClaimsPrincipal.ClaimsPrincipalSelector = () => new ClaimsPrincipal(principle.Object);
            ClaimsPrincipal.PrimaryIdentitySelector = identities => new ClaimsIdentity(mockIdentity.Object);
            var ctx = new Mock<ICommunicationContext>();
            var boundVariables = new NameValueCollection
            {
                {"servicename", "ping"},
                {"isToken", "True"},
                {"instanceid", ""},
                {"bookmark", ""}
            };
            var queryString = new NameValueCollection {{GlobalConstants.DLID, Guid.Empty.ToString()}, {"wid", Guid.Empty.ToString()}};
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost"));
            ctx.Setup(c => c.Request.User).Returns(principle.Object);

            var webPostRequestHandler = new WebPostRequestHandler();

            //------------Execute Test---------------------------
            webPostRequestHandler.ProcessRequest(ctx.Object);

            //------------Assert Results-------------------------
            mockIdentity.VerifyGet(identity => identity.Name, Times.AtLeast(1));
            Common.Utilities.OrginalExecutingUser = old;
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TokenRequestHandler))]
        public void WebPostRequestHandler_TokenRequest_ExpectExecution()
        {
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);

            var outerEnv = new ExecutionEnvironment();
            outerEnv.Assign("[[UserGroups().Name]]", "public", 0);
            outerEnv.Assign("[[UserGroups().Name]]", "whatever", 0);

            var boundVariables = new NameValueCollection
            {
                {"servicename", "ping"},
                {"isToken", "True"},
                {"instanceid", ""},
                {"bookmark", ""}
            };

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            dataObject.Setup(o => o.Environment).Returns(outerEnv);
            dataObject.Setup(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);

            var resourceId = Guid.NewGuid();
            var resourceName = "helloworld";

            var securityPermissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    ResourceName = "Category\\helloworld",
                    ResourceID = resourceId,
                    WindowsGroup = "public"
                },
            };
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            authorizationService.Setup(s => s.GetResourcePermissionsList(It.IsAny<Guid>())).Returns(securityPermissions);

            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            mockResource.Setup(o => o.ResourceName).Returns(resourceName);
            mockResource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><Message Description='' IsEditable='True' ColumnIODirection='Output'>Hello hello.</Message></DataList>"));

            var mockWarewolfResource = mockResource.As<IWarewolfResource>();
            mockWarewolfResource.Setup(o => o.ResourceID).Returns(resourceId);
            mockWarewolfResource.Setup(o => o.ResourceName).Returns(resourceName);
            mockWarewolfResource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><Message Description='' IsEditable='True' ColumnIODirection='Output'>Hello hello.</Message></DataList>"));

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockResource.Object);

            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));

            var doFactory = new TestAbstractWebRequestDataObjectFactory(dataObject.Object);

            //---------------Assert Precondition----------------
            var handlerMock = new AbstractWebRequestHandlerMock(resourceCatalog.Object, wRepo.Object, authorizationService.Object, doFactory);

            var token = DpapiWrapper.Encrypt("{'UserGroups': [{'Name': 'public' }]}");
            var headers = new Mock<NameValueCollection>();
            headers.Setup(collection => collection.Get("Authorization")).Returns("Bearer " + token);
            var webRequestTo = new WebRequestTO()
            {
                ServiceName = "helloworld",
                WebServerUrl = "http://localhost:3142/token/helloworld",
                Variables = boundVariables
            };

            //---------------Execute Test ----------------------
            var responseWriter = handlerMock.CreateFromMock(webRequestTo, "helloworld", Guid.Empty.ToString(), headers.Object, principal.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(responseWriter);

            var response = new HttpResponseMessage();
            var mockMessageContext = new Mock<IResponseMessageContext>();
            mockMessageContext.Setup(o => o.ResponseMessage).Returns(response);
            responseWriter.Write(mockMessageContext.Object);
            mockMessageContext.Verify(o => o.ResponseMessage, Times.AtLeast(1));

            var responseText = response.Content.ReadAsStringAsync().Result;
            Assert.IsTrue(!string.IsNullOrWhiteSpace(responseText), "<DataList />");
        }

        static void GetExecutingUser(Mock<IPrincipal> principal)
        {
            var identity = new Mock<IIdentity>();
            identity.Setup(p => p.Name).Returns("User1");
            principal.Setup(p => p.Identity).Returns(identity.Object);
        }
    }
}