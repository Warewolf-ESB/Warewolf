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
        public void StringResponseWriter_Write_ICommunicationContext_WritesContent()
        {
            //------------Setup for test--------------------------
            const string NewContent = "Hello world";

            var content = string.Empty;
            var responseWriter = new StringResponseWriter(NewContent, ContentTypes.Plain);
            var response = new Mock<ICommunicationResponse>();
            response.SetupAllProperties();
            response.Setup(r => r.OutputStream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int length) =>
            {
                if(length > 0)
                {
                    content += Encoding.UTF8.GetString(buffer, offset, length);
                }
            });

            var context = new Mock<ICommunicationContext>();
            context.Setup(c => c.Response).Returns(response.Object);

            //------------Execute Test---------------------------
            responseWriter.Write(context.Object);

            //------------Assert Results-------------------------

            Assert.AreEqual(ContentTypes.Plain.ToString(), context.Object.Response.ContentType);
            Assert.AreEqual(NewContent, content);
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StringResponseWriter_Write")]
        public void StringResponseWriter_Write_LargeContentICommunicationContext_WritesContentAndUpdateContentDisposition()
        {
            //------------Setup for test--------------------------
            var content = string.Empty;
            var response = new Mock<ICommunicationResponse>();
            response.SetupAllProperties();
            response.Setup(r => r.OutputStream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int length) =>
            {
                if(length > 0)
                {
                    content += Encoding.UTF8.GetString(buffer, offset, length);
                }
            });

            var context = new Mock<ICommunicationContext>();
            context.Setup(c => c.Response).Returns(response.Object);

            var contentType = ContentTypes.Xml;
            var largeContent = CreateLargeContent(contentType);

            var responseWriter = new StringResponseWriter(largeContent, contentType);

            //------------Execute Test---------------------------
            responseWriter.Write(context.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(ContentTypes.ForceDownload.ToString(), context.Object.Response.ContentType);
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