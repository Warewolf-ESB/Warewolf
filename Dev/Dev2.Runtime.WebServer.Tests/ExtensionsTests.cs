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
using Dev2.Runtime.WebServer.Executor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Dev2.Runtime.WebServer;

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
            var errorResult = sut.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = "test_title", Message = "test_message" });

            var result = GetResponse(errorResult); //sut.Response.Content.ReadAsStringAsync().Result;
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
            var errorResult = sut.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = "test_title", Message = "test_message" });

            var result = GetResponse(errorResult); //sut.Response.Content.ReadAsStringAsync().Result;
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
            var errorResult = sut.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = "test_title", Message = "test_message" });

            var result = GetResponse(errorResult); //sut.Response.Content.ReadAsStringAsync().Result;
            var expected = new Error
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "test_title",
                Message = "test_message"
            };
            Assert.AreEqual(expected.ToXML(), result);
        }


        //public static HttpActionContext CreateActionContext(bool isAuthenticated, string actionName)
        //{
        //    var user = new Mock<IPrincipal>();
        //    user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

        //    var actionDescriptor = new Mock<HttpActionDescriptor>();
        //    actionDescriptor.Setup(ad => ad.ActionName).Returns(actionName);

        //    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/content/site.css");
        //    if (!string.IsNullOrEmpty(actionName))
        //    {
        //        httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format("http://localhost:8080/services/{0}", actionName));
        //    }
        //    var actionContext = new HttpActionContext
        //    {
        //        ControllerContext = new HttpControllerContext
        //        {
        //            Request = httpRequestMessage,
        //            RequestContext = new HttpRequestContext { Principal = user.Object }
        //        },
        //        ActionDescriptor = actionDescriptor.Object
        //    };
        //    return actionContext;
        //}

        public static ActionContext CreateActionContext(bool isAuthenticated, string actionName)
        {
            var routeValues = new RouteValueDictionary();
            routeValues.Add("action", actionName);

            var defaultPath = "/content/site.css";
            if (!string.IsNullOrEmpty(actionName))
                defaultPath = string.Format("/services/{0}", actionName);

            var headers = new HeaderDictionary();
            headers.Add(new KeyValuePair<string, StringValues>("accept", new StringValues("all")));
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.RouteValues).Returns(routeValues);
            request.Setup(r => r.Scheme).Returns("http");
            request.Setup(r => r.Host).Returns(new HostString("localhost", 8080));
            request.Setup(r => r.Path).Returns(defaultPath);
            request.Setup(r => r.Method).Returns("Get");
            request.Setup(r => r.Body).Returns(new Mock<Stream>().Object);
            request.Setup(r => r.Headers).Returns(headers);

            var user = new Mock<ClaimsPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

            var response = new Mock<HttpResponse>();
            response.Setup(u => u.StatusCode).Returns(0);
            response.Setup(u => u.Body).Returns(new MemoryStream());

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(ad => ad.User).Returns(user.Object);
            httpContext.Setup(ad => ad.Request).Returns(request.Object);
            httpContext.Setup(ad => ad.Response).Returns(response.Object);

            var contextFeatures = new FeatureCollection();
            contextFeatures.Set(new HttpRequestMessageFeature(httpContext.Object));
            httpContext.Setup(ad => ad.Features).Returns(contextFeatures);

            return new ActionContext(httpContext.Object, new RouteData(routeValues), new ActionDescriptor());
        }

        public static string GetResponse(HttpResponseMessage response)
        {
            var result = string.Empty;

            if (response != null && response.Content != null)
                result = (response.Content as ObjectContent).Value.ToString();

            return result;
        }
    }
}
