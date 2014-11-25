
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
using System.Net;
using System.Net.Http;
using System.Text;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    [TestClass]
    public class DynamicFileResponseWriterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DynamicFileResponseWriter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DynamicFileResponseWriter_Constructor_LayoutFileIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new DynamicFileResponseWriter(null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DynamicFileResponseWriter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DynamicFileResponseWriter_Constructor_ContentPathTokenIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new DynamicFileResponseWriter("XXX", null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DynamicFileResponseWriter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DynamicFileResponseWriter_Constructor_ContentPathIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new DynamicFileResponseWriter("XXX", "XXX", null);

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DynamicFileResponseWriter_Write")]
        public void DynamicFileResponseWriter_Write_WebServerContext_WritesContent()
        {
            //------------Setup for test--------------------------
            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = WebServerRequestTests.CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            var context = new WebServerContext(request, boundVars);

            const string Token = "%%Token%%";
            const string LayoutContentFormat = "<html><body>{0}</body></html>";
            const string NewContent = "Hello world";

            var responseWriter = new TestDynamicFileResponseWriter(string.Format(LayoutContentFormat, Token), Token, NewContent);

            //------------Execute Test---------------------------
            responseWriter.Write(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(ContentTypes.Html, context.ResponseMessage.Content.Headers.ContentType);
            Assert.IsInstanceOfType(context.ResponseMessage.Content, typeof(StringContent));
            var task = context.ResponseMessage.Content.ReadAsStringAsync();
            task.Wait();

            Assert.AreEqual(string.Format(LayoutContentFormat, NewContent), task.Result);
        }
    }
}
