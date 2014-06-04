using System;
using System.Collections.Specialized;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    /// <summary>
    /// Summary description for WebsiteResourceHandlerTest
    /// </summary>
    [TestClass]
    public class WebsiteResourceHandlerTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebsiteResourceHandler_ProcessRequest")]
        public void WebsiteResourceHandler_ProcessRequest_WhenWWWInURI_ExpectExecution()
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
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost:3142/www/foo.html"));
            ctx.Setup(c => c.Request.User).Returns(principle.Object);

            var websiteResourceHandler = new WebsiteResourceHandler();
            
            //------------Execute Test---------------------------
            websiteResourceHandler.ProcessRequest(ctx.Object);

            //------------Assert Results-------------------------
            principle.Verify(p => p.Identity.Name, Times.Once());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebsiteResourceHandler_ProcessRequest")]
        public void WebsiteResourceHandler_ProcessRequest_WhenNotWWWInURI_ExpectExecution()
        {
            //------------Setup for test--------------------------
            Mock<IPrincipal> principle = new Mock<IPrincipal>();
            principle.Setup(p => p.Identity.Name).Returns("FakeUser");
            principle.Setup(p => p.Identity.Name).Verifiable();

            Mock<ICommunicationContext> ctx = new Mock<ICommunicationContext>();
            NameValueCollection boundVariables = new NameValueCollection { { "servicename", "ping" }, { "website", "foo" }, { "path", "bar" } };
            NameValueCollection queryString = new NameValueCollection { { GlobalConstants.DLID, Guid.Empty.ToString() }, { "wid", Guid.Empty.ToString() } };
            ctx.Setup(c => c.Request.BoundVariables).Returns(boundVariables);
            ctx.Setup(c => c.Request.QueryString).Returns(queryString);
            ctx.Setup(c => c.Request.Uri).Returns(new Uri("http://localhost:3142/www/foo.html"));
            ctx.Setup(c => c.Request.User).Returns(principle.Object);

            var websiteResourceHandler = new WebsiteResourceHandler();

            //------------Execute Test---------------------------
            websiteResourceHandler.ProcessRequest(ctx.Object);

            //------------Assert Results-------------------------
            principle.Verify(p => p.Identity.Name, Times.Once());
        }
    }
}
