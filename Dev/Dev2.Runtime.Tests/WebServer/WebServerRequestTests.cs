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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class WebServerRequestTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("WebServerRequest")]
        public void WebServerRequest_CreateResponseWriter()
        {
            var executionDto = new ExecutionDto();
            var mock = new Mock<IDSFDataObject>();
            mock.SetupGet(o => o.Environment).Returns(new ExecutionEnvironment());
            mock.SetupGet(o => o.IsDebug).Returns(true);
            mock.SetupGet(o => o.RemoteInvoke).Returns(false);
            mock.SetupGet(o => o.RemoteNonDebugInvoke).Returns(false);
            executionDto.DataListFormat = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            executionDto.ErrorResultTO = new Data.TO.ErrorResultTO();
            executionDto.DataObject = mock.Object;


            executionDto.Request = new Communication.EsbExecuteRequest();
            executionDto.Request.WasInternalService = false;

            var executionDtoExtentions = new ExecutionDtoExtensions(executionDto);
            executionDtoExtentions.CreateResponseWriter(new StringResponseWriterFactory());
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerRequest")]
        [ExpectedException(typeof(ArgumentNullException))]
        
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
        [TestCategory("WebServerRequest")]
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
        [TestCategory("WebServerRequest")]
        public void WebServerRequest_Constructor_PropertiesInitialized()
        {
            var request = CreateHttpRequest(out string content, out NameValueCollection boundVars, out NameValueCollection queryStr, out NameValueCollection headers);

            //------------Execute Test---------------------------
            var webServerRequest = new WebServerRequest(request, boundVars);

            //------------Assert Results-------------------------
            VerifyProperties(request, webServerRequest, content, queryStr, boundVars);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WebServerRequest")]
        public void WebServerRequest_GetContentEncoding_ParseSimpleEncoding()
        {
            var Content = new StringContent("Number42", Encoding.UTF8)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("text/plain") }
            };
            Content.Headers.Add("Content-Encoding", "utf8");

            //------------Execute Test---------------------------
            var ContentEncoding = Content.GetContentEncoding();

            //------------Assert Results-------------------------
            Assert.AreEqual(Encoding.UTF8, ContentEncoding, "WebServerRequest parsed the wrong content encoding.");
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


    }
}
