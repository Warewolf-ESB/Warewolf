
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
using System.Net.Http.Headers;
using System.Text;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    public class WebServerRequestTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerRequest_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void WebServerRequest_Constructor_RequestIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
#pragma warning disable 168
            var webServerRequest = new WebServerRequest(null, null);
#pragma warning restore 168

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerRequest_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServerRequest_Constructor_BoundVariablesIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
#pragma warning disable 168
            var webServerRequest = new WebServerRequest(new HttpRequestMessage(), null);
#pragma warning restore 168

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerRequest_Constructor")]
        public void WebServerRequest_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------            
            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            //------------Execute Test---------------------------
            var webServerRequest = new WebServerRequest(request, boundVars);

            //------------Assert Results-------------------------
            VerifyProperties(request, webServerRequest, content, queryStr, boundVars);
        }

        public static HttpRequestMessage CreateHttpRequest(out string content, out NameValueCollection boundVars, out NameValueCollection queryStr, out NameValueCollection headers)
        {
            content = "Number42";
            boundVars = new NameValueCollection
            {
                { "name", "hello" }
            };
            queryStr = new NameValueCollection
            {
                { "msg", "world" }
            };
            headers = new NameValueCollection
            {
                { "Server", "Dev2" }
            };

            var request = new HttpRequestMessage(HttpMethod.Get, string.Format("http://localhost/services/{0}?{1}={2}", boundVars[0], queryStr.Keys[0], queryStr[0]))
            {
                Content = new StringContent(content, Encoding.UTF8)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("text/plain") }
                },
            };
            request.Headers.Add(headers.Keys[0], headers[0]);
            return request;
        }

        public static void VerifyProperties(HttpRequestMessage httpRequest, WebServerRequest webRequest, string expectedContent, NameValueCollection queryStr, NameValueCollection boundVars)
        {
            var encoding = httpRequest.Content.GetContentEncoding();

            Assert.AreEqual(httpRequest.Method.Method, webRequest.Method);
            Assert.AreEqual(httpRequest.RequestUri.OriginalString, webRequest.Uri.OriginalString);
            Assert.AreEqual(httpRequest.Content.Headers.ContentLength, webRequest.ContentLength);
            Assert.AreEqual(httpRequest.Content.Headers.ContentType.ToString(), webRequest.ContentType);
            Assert.AreEqual(encoding, webRequest.ContentEncoding);

            CollectionAssert.AreEqual(queryStr, webRequest.QueryString);
            CollectionAssert.AreEqual(boundVars, webRequest.BoundVariables);

            var buffer = new byte[webRequest.ContentLength];
            using(var stream = webRequest.InputStream)
            {
                stream.Read(buffer, 0, webRequest.ContentLength);
            }
            var content = encoding.GetString(buffer);

            Assert.AreEqual(expectedContent, content);
        }

        // ReSharper restore InconsistentNaming
    }
}
