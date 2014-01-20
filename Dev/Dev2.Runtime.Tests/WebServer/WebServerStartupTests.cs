using System.Net;
using Dev2.Runtime.WebServer;
using Microsoft.Owin.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
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

            var app = new AppBuilder();
            app.Properties.Add(typeof(HttpListener).FullName, listener);

            var webServerStartup = new WebServerStartup();

            //------------Execute Test---------------------------
            webServerStartup.Configuration(app);

            //------------Assert Results-------------------------
            Assert.AreEqual(AuthenticationSchemes.Ntlm, listener.AuthenticationSchemes);
            Assert.IsTrue(listener.IgnoreWriteExceptions);
        }
    }
}
