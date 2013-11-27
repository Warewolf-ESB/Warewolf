using System.Collections.Specialized;
using System.Net;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    [TestClass]
    public class StatusResponseWriterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StatusResponseWriter_Write")]
        public void StatusResponseWriter_Write_WebServerContext_WritesTheStatus()
        {
            //------------Setup for test--------------------------
            const HttpStatusCode Expected = HttpStatusCode.PaymentRequired;

            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = WebServerRequestTests.CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            var context = new WebServerContext(request, boundVars);

            var responseWriter = new StatusResponseWriter(Expected);

            //------------Execute Test---------------------------
            responseWriter.Write(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(Expected, context.ResponseMessage.StatusCode);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StatusResponseWriter_Write")]
        public void StatusResponseWriter_Write_ICommunicationContext_WritesTheStatus()
        {
            //------------Setup for test--------------------------
            const HttpStatusCode Expected = HttpStatusCode.PaymentRequired;

            var response = new Mock<ICommunicationResponse>();
            response.SetupAllProperties();

            var context = new Mock<ICommunicationContext>();
            context.Setup(c => c.Response).Returns(response.Object);

            var responseWriter = new StatusResponseWriter(Expected);

            //------------Execute Test---------------------------
            responseWriter.Write(context.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(Expected, context.Object.Response.Status);
        }
    }
}
