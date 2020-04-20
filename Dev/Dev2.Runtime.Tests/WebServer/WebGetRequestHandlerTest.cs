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
using System.Collections.Specialized;
using System.Security.Claims;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class WebGetRequestHandlerTest
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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebGetRequestHandler_ProcessRequest")]
        public void WebGetRequestHandler_ProcessRequest_WhenValidUserContext_ExpectExecution()
        {
            //------------Setup for test--------------------------

            var principle = new Mock<IPrincipal>();
            var mockIdentity = new Mock<IIdentity>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            var resource = mockResource.Object;
            resourceCatalog.Setup(o => o.GetResource(Guid.Empty, "ping")).Returns(resource);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
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
            var boundVariables = new NameValueCollection { { "servicename", "ping" }, { "instanceid", "" }, { "bookmark", "" } };
            var queryString = new NameValueCollection { { GlobalConstants.DLID, Guid.Empty.ToString() }, { "wid", Guid.Empty.ToString() } };
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost"));
            ctx.Setup(c => c.Request.User).Returns(principle.Object);

            var webGetRequestHandler = new WebGetRequestHandler(resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object);

            //------------Execute Test---------------------------
            webGetRequestHandler.ProcessRequest(ctx.Object);

            //------------Assert Results-------------------------
            mockIdentity.VerifyGet(identity => identity.Name, Times.AtLeast(1));
            Common.Utilities.OrginalExecutingUser = old;
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebGetRequestHandler_ProcessRequest")]
        public void WebGetRequestHandler_ProcessRequest_WhenValidUserContextWhenNullDataListID_ExpectExecution()
        {
            //------------Setup for test--------------------------
            var principle = new Mock<IPrincipal>();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(identity => identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity).Returns(mockIdentity.Object);
            ClaimsPrincipal.ClaimsPrincipalSelector = () => new ClaimsPrincipal(principle.Object);
            ClaimsPrincipal.PrimaryIdentitySelector = identities => new ClaimsIdentity(mockIdentity.Object);

            var ctx = new Mock<ICommunicationContext>();
            var boundVariables = new NameValueCollection { { "servicename", "ping" }, { "instanceid", "" }, { "bookmark", "" } };
            var queryString = new NameValueCollection { { GlobalConstants.DLID, null }, { "wid", Guid.Empty.ToString() } };
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost"));
            ctx.Setup(c => c.Request.User).Returns(principle.Object);

            var webGetRequestHandler = new WebGetRequestHandler();
            //------------Execute Test---------------------------
            webGetRequestHandler.ProcessRequest(ctx.Object);
            //------------Assert Results-------------------------
            principle.Verify(p => p.Identity, Times.AtLeast(1));
        }
    }
}
