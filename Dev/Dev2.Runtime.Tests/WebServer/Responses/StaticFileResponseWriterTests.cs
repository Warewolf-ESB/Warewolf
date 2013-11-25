using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StaticFileResponseWriter_Write")]
        public void StaticFileResponseWriter_Write_ICommunicationContext_WritesContent()
        {
            //------------Setup for test--------------------------
            const string NewContent = "Hello world";

            var content = string.Empty;
            var responseWriter = new TestStaticFileResponseWriter(NewContent, "text/plain");
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

    }
}