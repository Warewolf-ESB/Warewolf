
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
using System.Linq;
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
            new WebServerResponse(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerResponse_Constructor")]
        public void WebServerResponse_Constructor_ResponseIsNotNull_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var response = new HttpResponseMessage();
            response.RequestMessage = new HttpRequestMessage();

            //------------Execute Test---------------------------
            var webServerResponse = new WebServerResponse(response);

            //------------Assert Results-------------------------
            Assert.IsNotNull(webServerResponse.Response);
            var accessControlList = response.Headers.GetValues("Access-Control-Allow-Credentials").ToList();
            Assert.IsNotNull(accessControlList);
            Assert.AreEqual(1,accessControlList.Count);
            Assert.AreEqual("true",accessControlList[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerResponse_Constructor")]
        public void WebServerResponse_Constructor_ResponseWithOrigin_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var response = new HttpResponseMessage { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("Origin","http://localhost");
            //------------Execute Test---------------------------
            var webServerResponse = new WebServerResponse(response);

            //------------Assert Results-------------------------
            Assert.IsNotNull(webServerResponse.Response);
            var accessControlList = response.Headers.GetValues("Access-Control-Allow-Credentials").ToList();
            var accessControlOrgins = response.Headers.GetValues("Access-Control-Allow-Origin").ToList();
            Assert.IsNotNull(accessControlList);
            Assert.AreEqual(1,accessControlList.Count);
            Assert.AreEqual("true",accessControlList[0]);
            Assert.IsNotNull(accessControlOrgins);
            Assert.AreEqual(1, accessControlOrgins.Count);
            Assert.AreEqual("http://localhost", accessControlOrgins[0]);
        }
    }
}
