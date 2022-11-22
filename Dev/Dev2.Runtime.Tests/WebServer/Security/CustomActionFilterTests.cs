/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Executor;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Dev2.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class CustomActionFilterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomActionFilter_Constructor")]
        public void CustomActionFilter_Constructor_Default_ProviderIsAuthorizationProviderInstance()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new CustomActionFilter();

            //------------Assert Results-------------------------
            Assert.AreSame(ServerAuthorizationService.Instance, attribute.Service);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomActionFilter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CustomActionFilter_Constructor_AuthorizationProviderIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------

            new CustomActionFilter(null);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CustomActionFilter_OnAuthorization_ActionContextIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var provider = new Mock<IAuthorizationService>();
            var attribute = new CustomActionFilter(provider.Object);

            //------------Execute Test---------------------------
            attribute.OnAuthorization(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CustomActionFilter))]
        public void CustomActionFilter_OnAuthorization_GivenUserIsNotAuthenticated_ShouldReturn401JSON()
        {
            //------------Setup for test--------------------------
            var provider = new Mock<IAuthorizationService>();
            var attribute = new CustomActionFilter(provider.Object);

            var httpActionContext = CreateActionContext(false, "http://localhost:8080/Examples/Workflow_One.json");

            //------------Execute Test---------------------------
            var result = attribute.OnAuthorization(httpActionContext);
            //------------Assert Results-------------------------
            Assert.IsFalse(IsSuccessStatusCode(result));
            var responseMessage = GetResponse(result);

            Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 401,\r\n    \"Title\": \"user_unauthorized\",\r\n    \"Message\": \"Authorization has been denied for this user.\"\r\n  }\r\n}", responseMessage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CustomActionFilter))]
        public void CustomActionFilter_OnAuthorization_GivenServicedIsNotAuthenticated_ShouldReturn403XML()
        {
            //------------Setup for test--------------------------
            var provider = new Mock<IAuthorizationService>();
            var attribute = new CustomActionFilter(provider.Object);

            var httpActionContext = CreateActionContext(true, "http://localhost:8080/Examples/Workflow_One.xml");

            //------------Execute Test---------------------------
            var result = attribute.OnAuthorization(httpActionContext);
            //------------Assert Results-------------------------
            //var result = httpActionContext.HttpContext.Response;
            Assert.IsFalse(IsSuccessStatusCode(result));

            var responseMessage = GetResponse(result);

            Assert.AreEqual("<Error>\r\n  <Status>403</Status>\r\n  <Title>user_forbidden</Title>\r\n  <Message>Authorization has been denied for this request.</Message>\r\n</Error>", responseMessage);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        public void CustomActionFilter_OnAuthorization_UserIsNotAuthenticated_ResponseIsUnauthorized()
        {
            Verify_OnAuthorization_Response(false, null, false, HttpStatusCode.Unauthorized, GlobalConstants.USER_UNAUTHORIZED, ErrorResource.AuthorizationDeniedForThisUser);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        public void CustomActionFilter_OnAuthorization_UserIsAuthenticatedAndNotAuthorized_ResponseIsAccessDenied()
        {
            Verify_OnAuthorization_Response(true, null, false, HttpStatusCode.Forbidden, GlobalConstants.USER_FORBIDDEN, ErrorResource.AuthorizationDeniedForThisRequest);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        public void CustomActionFilter_OnAuthorization_UserIsAuthenticatedAndAuthorized_ResponseIsNull()
        {
            Verify_OnAuthorization_Response(true, null, true, HttpStatusCode.OK, string.Empty, null);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        public void CustomActionFilter_OnAuthorization_WhenInternalService_UserIsAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(true, "ping", true, HttpStatusCode.OK, string.Empty, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        public void CustomActionFilter_OnAuthorization_WhenInternalService_UserIsNotAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(false, "ping", true, HttpStatusCode.Unauthorized, GlobalConstants.USER_UNAUTHORIZED, ErrorResource.AuthorizationDeniedForThisUser);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        public void CustomActionFilter_OnAuthorization_WhenNotInternalService_UserIsNotAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(false, "pingme", true, HttpStatusCode.Unauthorized, GlobalConstants.USER_UNAUTHORIZED, ErrorResource.AuthorizationDeniedForThisUser);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomActionFilter_OnAuthorization")]
        public void CustomActionFilter_OnAuthorization_WhenNotInternalService_UserIsAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(true, "pingme", true, HttpStatusCode.Forbidden, GlobalConstants.USER_FORBIDDEN, null);
            //------------Assert Results-------------------------
        }

        static void Verify_OnAuthorization_Response(bool isAuthenticated, string actionName, bool isAuthorized, HttpStatusCode expectedStatusCode, string title, string expectedMessage, EmitionTypes emitionTypes = EmitionTypes.JSON)
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            authorizationProvider.Setup(p => p.IsAuthorized(It.IsAny<IAuthorizationRequest>())).Returns(isAuthorized);

            var attribute = new CustomActionFilter(authorizationProvider.Object);
            var actionContext = CreateActionContext(isAuthenticated, actionName);

            //------------Execute Test---------------------------
            var result = attribute.OnAuthorization(actionContext);

            //------------Assert Results-------------------------
            if (isAuthorized && isAuthenticated)
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.AreEqual(expectedStatusCode, result.StatusCode);

                var errorObject = new Error
                {
                    Status = (int)expectedStatusCode,
                    Title = title,
                    Message = expectedMessage
                };

                var actualResponse = GetResponse(result);

                if (emitionTypes.Equals(EmitionTypes.XML))
                {
                    Assert.AreEqual(errorObject.ToXML(), actualResponse);
                }
                Assert.AreEqual(errorObject.ToJSON(), actualResponse);
            }
        }

        //public static ActionContext CreateActionContext(bool isAuthenticated, string actionName)
        //{
        //    var routeValues = new RouteValueDictionary();
        //    routeValues.Add("action", actionName);

        //    var defaultPath = "/content/site.css";
        //    if (!string.IsNullOrEmpty(actionName))
        //        defaultPath = string.Format("/services/{0}", actionName);

        //    var headers = new HeaderDictionary();
        //    headers.Add(new KeyValuePair<string, StringValues>("accept", new StringValues("all")));
        //    var request = new Mock<HttpRequest>();
        //    request.Setup(r => r.RouteValues).Returns(routeValues);
        //    request.Setup(r => r.Scheme).Returns("http");
        //    request.Setup(r => r.Host).Returns(new HostString("localhost", 8080));
        //    request.Setup(r => r.Path).Returns(defaultPath);
        //    request.Setup(r => r.Method).Returns("Get");
        //    request.Setup(r => r.Body).Returns(new Mock<Stream>().Object);
        //    request.Setup(r => r.Headers).Returns(headers);

        //    var user = new Mock<ClaimsPrincipal>();
        //    user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

        //    var response = new Mock<HttpResponse>();
        //    response.Setup(u => u.StatusCode).Returns(0);
        //    response.Setup(u => u.Body).Returns(new MemoryStream());

        //    var httpContext = new Mock<HttpContext>();
        //    httpContext.Setup(ad => ad.User).Returns(user.Object);
        //    httpContext.Setup(ad => ad.Request).Returns(request.Object);
        //    httpContext.Setup(ad => ad.Response).Returns(response.Object);

        //    var contextFeatures = new FeatureCollection();
        //    contextFeatures.Set(new HttpRequestMessageFeature(httpContext.Object));
        //    httpContext.Setup(ad => ad.Features).Returns(contextFeatures);

        //    var actionDescriptor = new Mock<ActionDescriptor>();
        //    actionDescriptor.Setup(ad => ad.DisplayName).Returns(actionName);

        //    return new ActionContext(httpContext.Object, new RouteData(routeValues), actionDescriptor.Object);
        //}


        public static ActionContext CreateActionContext(bool isAuthenticated, string actionName)
        {
            var routeValues = new RouteValueDictionary();
            routeValues.Add("action", actionName);

            var defaultPath = "http://localhost:8080/content/site.css";
            if (!string.IsNullOrEmpty(actionName))
                defaultPath = string.Format("http://localhost:8080/services/{0}", actionName);

            var uri = new Uri(defaultPath);

            var headers = new HeaderDictionary();
            headers.Add(new KeyValuePair<string, StringValues>("accept", new StringValues("all")));
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.RouteValues).Returns(routeValues);
            request.Setup(r => r.Scheme).Returns(uri.Scheme);
            request.Setup(r => r.Host).Returns(new HostString(uri.Host, uri.Port));
            request.Setup(r => r.Path).Returns(uri.AbsolutePath);
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

            var actionDescriptor = new Mock<ActionDescriptor>();
            actionDescriptor.Setup(ad => ad.DisplayName).Returns(actionName);

            return new ActionContext(httpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(routeValues), actionDescriptor.Object);
        }


        public bool IsSuccessStatusCode(HttpResponseMessage response)
        {
            if (response == null) return false;
            return response.IsSuccessStatusCode;
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
