/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Net;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NETFRAMEWORK
using Microsoft.Owin.Builder;
#else
using System.Security.Claims;
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;
#endif

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class WebServerStartupTests
    {
#if NETFRAMEWORK
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerStartup_Configuration")]
        public void WebServerStartup_Configuration_HttpListener_InitializedCorrectly()
        {
            //------------Setup for test--------------------------

            var listener = new HttpListener();

            Assert.AreEqual(AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes);
            Assert.IsFalse(listener.IgnoreWriteExceptions);

            var app = new AppBuilder();
            app.Properties.Add(typeof(HttpListener).FullName, listener);

            var webServerStartup = new WebServerStartup();

            //------------Execute Test---------------------------
            webServerStartup.Configuration(app);

            //------------Assert Results-------------------------
            Assert.AreEqual(AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes);
            Assert.IsTrue(listener.IgnoreWriteExceptions);
        }
#else
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerStartup_Configuration")]
        public void WebServerStartup_Configuration_HttpListener_InitializedCorrectly()
        {
            //------------Setup for test--------------------------
          
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Scheme).Returns("http");
            request.Setup(r => r.Host).Returns(new HostString("localhost", 8080));
            request.Setup(r => r.Path).Returns("/public/test");

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(ad => ad.Request).Returns(request.Object);

            //------------Execute Test---------------------------

            var scheme = WindowsAndAnonymousAuthenticationMiddleware.GetAuthenticationScheme(httpContext.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthenticationSchemes.Anonymous, scheme);
        }
#endif
    }
}
