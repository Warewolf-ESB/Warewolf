
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
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    [TestClass]
    public class StringResponseWriterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringResponseWriter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StringResponseWriter_Constructor_TextIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new StringResponseWriter(null, (MediaTypeHeaderValue)null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringResponseWriter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StringResponseWriter_Constructor_ContentTypeIsNullMediaTypeHeaderValue_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new StringResponseWriter("XXX", (MediaTypeHeaderValue)null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringResponseWriter_Constructor")]
        [ExpectedException(typeof(FormatException))]
        public void StringResponseWriter_Constructor_ContentTypeIsNullString_ThrowsFormatException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var responseWriter = new StringResponseWriter("XXX", (string)null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringResponseWriter_Write")]
        public void StringResponseWriter_Write_WebServerContext_WritesContent()
        {
            //------------Setup for test--------------------------
            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = WebServerRequestTests.CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            var context = new WebServerContext(request, boundVars);

            const string NewContent = "Hello world";

            var responseWriter = new StringResponseWriter(NewContent, ContentTypes.Plain);

            //------------Execute Test---------------------------
            responseWriter.Write(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(ContentTypes.Plain, context.ResponseMessage.Content.Headers.ContentType);
            Assert.IsInstanceOfType(context.ResponseMessage.Content, typeof(StringContent));
            var task = context.ResponseMessage.Content.ReadAsStringAsync();
            task.Wait();

            Assert.AreEqual(NewContent, task.Result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringResponseWriter_Write")]
        public void StringResponseWriter_Write_LargeContentWebServerContext_WritesContentAndUpdateContentDisposition()
        {
            //------------Setup for test--------------------------
            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = WebServerRequestTests.CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            var context = new WebServerContext(request, boundVars);

            var contentType = ContentTypes.Xml;
            var largeContent = CreateLargeContent(contentType);

            var responseWriter = new StringResponseWriter(largeContent, contentType);

            //------------Execute Test---------------------------
            responseWriter.Write(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(ContentTypes.ForceDownload, context.ResponseMessage.Content.Headers.ContentType);
            Assert.AreEqual("attachment", context.ResponseMessage.Content.Headers.ContentDisposition.DispositionType);
            Assert.AreEqual("attachment; filename=Output.xml", context.ResponseMessage.Content.Headers.ContentDisposition.ToString());

            Assert.IsInstanceOfType(context.ResponseMessage.Content, typeof(StringContent));
            var task = context.ResponseMessage.Content.ReadAsStringAsync();
            task.Wait();

            Assert.AreEqual(largeContent, task.Result);
        }

        static string CreateLargeContent(MediaTypeHeaderValue contentType)
        {
            var buffer = new byte[(int)WebServerStartup.SizeCapForDownload];
            var text = Encoding.UTF8.GetString(buffer);
            if(ContentTypes.Json.Equals(contentType))
            {
                return string.Format("{{ \"value\" : \"{0}\" }}", text);
            }

            return string.Format("<data>{0}</data>", text);
        }
    }
}
