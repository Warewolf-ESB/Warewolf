using System;
using System.Net.Http;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    public class WebServerResponseTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerResponse_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServerResponse_Constructor_ResponseIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var webServerResponse = new WebServerResponse(null);

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerResponse_Constructor")]
        public void WebServerResponse_Constructor_ResponseIsNotNull_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var response = new HttpResponseMessage();

            //------------Execute Test---------------------------
            var webServerResponse = new WebServerResponse(response);

            //------------Assert Results-------------------------
            Assert.IsNotNull(webServerResponse.Response);
            Assert.AreSame(response, webServerResponse.Response);            
        }
    }
}
