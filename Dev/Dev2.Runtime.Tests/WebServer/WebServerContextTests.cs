
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
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    public class WebServerContextTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerContext_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServerContext_Constructor_NullRequest_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var context = new WebServerContext(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerContext_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServerContext_Constructor_NullRequestPaths_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var context = new WebServerContext(new HttpRequestMessage(), null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerContext_Constructor")]
        public void WebServerContext_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------       
            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = WebServerRequestTests.CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            //------------Execute Test---------------------------
            var context = new WebServerContext(request, boundVars);

            //------------Assert Results-------------------------
            Assert.IsNotNull(context.ResponseMessage);
            Assert.IsNotNull(context.Request);
            Assert.IsNotNull(context.Response);
            CollectionAssert.AreEqual(headers, context.FetchHeaders());

            WebServerRequestTests.VerifyProperties(request, (WebServerRequest)context.Request, content, queryStr, boundVars);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerContext_Send")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServerContext_Send_ResponseIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------            
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/services")
            {
                Content = new StringContent("", Encoding.UTF8)
            };
            var context = new WebServerContext(request, new NameValueCollection());

            //------------Execute Test---------------------------
            context.Send(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerContext_Send")]
        public void WebServerContext_Send_ResponseIsNotNull_InvokesWriteOnResponse()
        {
            //------------Setup for test--------------------------            
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/services")
            {
                Content = new StringContent("", Encoding.UTF8)
            };
            var context = new WebServerContext(request, new NameValueCollection());

            var response = new Mock<IResponseWriter>();
            response.Setup(r => r.Write(It.IsAny<WebServerContext>())).Verifiable();
         

            //------------Execute Test---------------------------
            context.Send(response.Object);

            //------------Assert Results-------------------------
            response.Verify(r => r.Write(It.IsAny<WebServerContext>()));
        }
    }
}
