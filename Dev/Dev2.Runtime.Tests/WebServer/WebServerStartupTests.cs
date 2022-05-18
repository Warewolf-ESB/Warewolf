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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endif

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class WebServerStartupTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerStartup_Configuration")]
        public void WebServerStartup_Configuration_HttpListener_InitializedCorrectly()
        {
            //------------Setup for test--------------------------
            var listener = new HttpListener();

            Assert.AreEqual(AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes);
            Assert.IsFalse(listener.IgnoreWriteExceptions);

#if NETFRAMEWORK
            var app = new AppBuilder();
#else
            var app = new ApplicationBuilder(new DefaultHttpContext().RequestServices);
#endif
            app.Properties.Add(typeof(HttpListener).FullName, listener);

            var webServerStartup = new WebServerStartup();

            //------------Execute Test---------------------------
#if NETFRAMEWORK
            webServerStartup.Configuration(app);
#else
            webServerStartup.Configure(app);
#endif

            //------------Assert Results-------------------------
            Assert.AreEqual(AuthenticationSchemes.Anonymous, listener.AuthenticationSchemes);
            Assert.IsTrue(listener.IgnoreWriteExceptions);
        }
    }
}
