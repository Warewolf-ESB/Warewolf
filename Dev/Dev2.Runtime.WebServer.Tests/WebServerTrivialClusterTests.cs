/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Runtime.WebServer.Tests
{
    /// <summary>
    /// Track-C T1 batch: <see cref="StatusResponseWriter"/> (untested), small pieces of the
    /// big <c>Extensions</c> static class, and the <see cref="WarewolfErrorResponseArgs"/> POCO.
    /// </summary>
    [TestClass]
    public class WebServerTrivialClusterTests
    {
        const string Owner = "Ashley Lewis";

        // ---- StatusResponseWriter ----------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(StatusResponseWriter))]
        public void StatusResponseWriter_DefaultCtor_Write_SetsNoContent()
        {
            using var msg = new HttpResponseMessage();
            var ctx = new Mock<IResponseMessageContext>();
            ctx.SetupGet(c => c.ResponseMessage).Returns(msg);

            new StatusResponseWriter().Write(ctx.Object);

            Assert.AreEqual(HttpStatusCode.NoContent, msg.StatusCode);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(StatusResponseWriter))]
        public void StatusResponseWriter_ExplicitCtor_Write_SetsGivenStatus()
        {
            using var msg = new HttpResponseMessage();
            var ctx = new Mock<IResponseMessageContext>();
            ctx.SetupGet(c => c.ResponseMessage).Returns(msg);

            new StatusResponseWriter(HttpStatusCode.Accepted).Write(ctx.Object);

            Assert.AreEqual(HttpStatusCode.Accepted, msg.StatusCode);
        }

        // ---- Extensions.GetHttpStringContent (not in existing ExtensionsTests) -------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_GetHttpStringContent_XmlAndTrx_AreApplicationXml()
        {
            using var xmlContent = EmitionTypes.XML.GetHttpStringContent("<r/>");
            using var trxContent = EmitionTypes.TRX.GetHttpStringContent("<r/>");

            Assert.AreEqual("application/xml", xmlContent.Headers.ContentType.MediaType);
            Assert.AreEqual("application/xml", trxContent.Headers.ContentType.MediaType);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_GetHttpStringContent_Json_IsApplicationJson()
        {
            using var jsonContent = EmitionTypes.JSON.GetHttpStringContent("{}");
            Assert.AreEqual("application/json", jsonContent.Headers.ContentType.MediaType);
        }

        // ---- Extensions.IsAuthenticated ----------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_IsAuthenticated_NullUser_ReturnsFalse_AndDoesNotThrow()
        {
            // exercises the `Dev2Logger.Debug("Null User", ...)` branch and the short-circuit return false
            IPrincipal user = null;
            Assert.IsFalse(user.IsAuthenticated());
        }

        // (existing ExtensionsTests already covers the mocked-identity true/false branches.)

        // ---- Extensions.GetContentEncoding (not in existing ExtensionsTests) ---------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_GetContentEncoding_NullContent_ReturnsUtf8()
        {
            // null content takes the `content == null ? String.Empty` branch then the
            // `IsNullOrEmpty` short-circuit → default UTF8.
            HttpContent content = null;
            Assert.AreEqual(Encoding.UTF8, content.GetContentEncoding());
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_GetContentEncoding_NoEncodingHeader_ReturnsUtf8()
        {
            using var content = new StringContent("data");
            // no Content-Encoding header set → FirstOrDefault returns null → IsNullOrEmpty true → UTF8.
            Assert.AreEqual(Encoding.UTF8, content.GetContentEncoding());
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_GetContentEncoding_KnownEncoding_ReturnsThatEncoding()
        {
            using var content = new StringContent("data");
            content.Headers.ContentEncoding.Add("utf-8");
            Assert.AreEqual(Encoding.UTF8, content.GetContentEncoding());
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_GetContentEncoding_InvalidEncoding_FallsBackToUtf8()
        {
            // unknown encoding → Encoding.GetEncoding throws → catch logs and falls through to UTF8.
            using var content = new StringContent("data");
            content.Headers.ContentEncoding.Add("not-a-real-encoding");
            Assert.AreEqual(Encoding.UTF8, content.GetContentEncoding());
        }

        // ---- Extensions.CreateWarewolfErrorResponse (Uri overload) -------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(Extensions))]
        public void Extensions_CreateWarewolfErrorResponse_FromUri_BuildsResponseWithStatusAndContent()
        {
            var args = new WarewolfErrorResponseArgs
            {
                StatusCode = HttpStatusCode.BadGateway,
                Title = "t",
                Message = "m",
            };

            using var response = Extensions.CreateWarewolfErrorResponse(new Uri("http://h/file.json"), args);

            Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
            Assert.IsNotNull(response.Content);
            // .json URI → JSON emition → application/json
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.MediaType);
        }

        // ---- WarewolfErrorResponseArgs POCO ------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(WarewolfErrorResponseArgs))]
        public void WarewolfErrorResponseArgs_Properties_Roundtrip()
        {
            var args = new WarewolfErrorResponseArgs
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Title = "title",
                Message = "message",
            };
            Assert.AreEqual(HttpStatusCode.InternalServerError, args.StatusCode);
            Assert.AreEqual("title", args.Title);
            Assert.AreEqual("message", args.Message);
        }
    }
}
