/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;
using Dev2.Runtime.WebServer.Executor;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    [TestCategory(nameof(Extensions))]
    public class ExtensionsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void Extensions_GetEmitionType_GivenAnyOther_ShouldDefaultToJSON()
        {
            var uri = new Uri("https://localhost:3241/help/wolf/workflows.unknown-ext");
            var result = Extensions.GetEmitionType(uri);

            Assert.AreEqual(Web.EmitionTypes.JSON, result);

            result = uri.GetEmitionType();
            Assert.AreEqual(Web.EmitionTypes.JSON, result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void Extensions_GetEmitionType_GivenXMLExt_ShouldReturnXML()
        {
            var uri = new Uri("https://localhost:3241/help/wolf/workflows.xml");
            var result = Extensions.GetEmitionType(uri);

            Assert.AreEqual(Web.EmitionTypes.XML, result);

            result = uri.GetEmitionType();
            Assert.AreEqual(Web.EmitionTypes.XML, result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void Extensions_GetEmitionType_GivenTRXExt_ShouldReturnXML()
        {
            var uri = new Uri("https://localhost:3241/help/wolf/workflows.trx");
            var result = Extensions.GetEmitionType(uri);

            Assert.AreEqual(Web.EmitionTypes.XML, result);

            result = uri.GetEmitionType();
            Assert.AreEqual(Web.EmitionTypes.XML, result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void Extensions_CreateWarewolfErrorResponse_HttpActionContext_GivenJSONURI_ShouldReturnJSON()
        {
            var sut = CreateActionContext(true, "http://localhost:3241/help/wolf-tools/redis.json");
            sut.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = "test_title", Message = "test_message" });

            var result = sut.Response.Content.ReadAsStringAsync().Result;
            var expected = new Error
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "test_title",
                Message = "test_message"
            };
            Assert.AreEqual(expected.ToJSON(), result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void Extensions_CreateWarewolfErrorResponse_HttpActionContext_GivenXMLURI_ShouldReturnXML()
        {
            var sut = CreateActionContext(true, "http://localhost:3241/help/wolf-tools/gates.xml");
            sut.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = "test_title", Message = "test_message" });

            var result = sut.Response.Content.ReadAsStringAsync().Result;
            var expected = new Error
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "test_title",
                Message = "test_message"
            };
            Assert.AreEqual(expected.ToXML(), result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void Extensions_CreateWarewolfErrorResponse_HttpActionContext_GivenTRXURI_ShouldReturnXML()
        {
            var sut = CreateActionContext(true, "http://localhost:3241/help/wolf-configs/logger.trx?name=elastic");
            sut.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = "test_title", Message = "test_message" });

            var result = sut.Response.Content.ReadAsStringAsync().Result;
            var expected = new Error
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "test_title",
                Message = "test_message"
            };
            Assert.AreEqual(expected.ToXML(), result);
        }


        public static HttpActionContext CreateActionContext(bool isAuthenticated, string actionName)
        {
            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

            var actionDescriptor = new Mock<HttpActionDescriptor>();
            actionDescriptor.Setup(ad => ad.ActionName).Returns(actionName);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/content/site.css");
            if (!string.IsNullOrEmpty(actionName))
            {
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format("http://localhost:8080/services/{0}", actionName));
            }
            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Request = httpRequestMessage,
                    RequestContext = new HttpRequestContext { Principal = user.Object }
                },
                ActionDescriptor = actionDescriptor.Object
            };
            return actionContext;
        }

    }
}
