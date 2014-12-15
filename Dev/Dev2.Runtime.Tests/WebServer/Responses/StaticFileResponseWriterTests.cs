
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
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    [TestClass]
    public class StaticFileResponseWriterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StaticFileResponseWriter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StaticFileResponseWriter_Constructor_FileIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new StaticFileResponseWriter(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StaticFileResponseWriter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StaticFileResponseWriter_Constructor_ContentTypeIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new StaticFileResponseWriter("XXX", null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StaticFileResponseWriter_Write")]
        public void StaticFileResponseWriter_Write_WebServerContext_WritesContent()
        {
            //------------Setup for test--------------------------
            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = WebServerRequestTests.CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            var context = new WebServerContext(request, boundVars);

            const string NewContent = "Hello world";

            var responseWriter = new TestStaticFileResponseWriter(NewContent, "text/plain");

            //------------Execute Test---------------------------
            responseWriter.Write(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(ContentTypes.Plain, context.ResponseMessage.Content.Headers.ContentType);
            Assert.IsInstanceOfType(context.ResponseMessage.Content, typeof(PushStreamContent));
            var task = context.ResponseMessage.Content.ReadAsStringAsync();
            task.Wait();

            Assert.AreEqual(NewContent, task.Result);
        }
    }
}
