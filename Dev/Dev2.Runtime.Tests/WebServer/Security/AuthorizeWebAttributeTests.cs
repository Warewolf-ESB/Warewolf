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
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;
using Dev2.Common;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class AuthorizeWebAttributeTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_Constructor")]
        public void AuthorizeWebAttribute_Constructor_Default_ProviderIsAuthorizationProviderInstance()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new AuthorizeWebAttribute();

            //------------Assert Results-------------------------
            Assert.AreSame(ServerAuthorizationService.Instance, attribute.Service);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeWebAttribute_Constructor_AuthorizationProviderIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------

            new AuthorizeWebAttribute(null);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeWebAttribute_OnAuthorization_ActionContextIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var provider = new Mock<IAuthorizationService>();
            var attribute = new AuthorizeWebAttribute(provider.Object);

            //------------Execute Test---------------------------
            attribute.OnAuthorization(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AuthorizeWebAttribute))]
        public void AuthorizeWebAttribute_OnAuthorization_GivenUserIsNotAuthenticated_ShouldReturn401JSON()
        {
            //------------Setup for test--------------------------
            var provider = new Mock<IAuthorizationService>();
            var attribute = new AuthorizeWebAttribute(provider.Object);

            var httpActionContext = CreateActionContext(false, "http://localhost:8080/Examples/Workflow_One.json");

            //------------Execute Test---------------------------
            attribute.OnAuthorization(httpActionContext);
            //------------Assert Results-------------------------
            var result = httpActionContext.Response;
            Assert.IsFalse(result.IsSuccessStatusCode);
            var responseMessage = result.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 401,\r\n    \"Title\": \"user_unauthorized\",\r\n    \"Message\": \"Authorization has been denied for this user.\"\r\n  }\r\n}", responseMessage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AuthorizeWebAttribute))]
        public void AuthorizeWebAttribute_OnAuthorization_GivenServicedIsNotAuthenticated_ShouldReturn403XML()
        {
            //------------Setup for test--------------------------
            var provider = new Mock<IAuthorizationService>();
            var attribute = new AuthorizeWebAttribute(provider.Object);

            var httpActionContext = CreateActionContext(true, "http://localhost:8080/Examples/Workflow_One.xml");

            //------------Execute Test---------------------------
            attribute.OnAuthorization(httpActionContext);
            //------------Assert Results-------------------------
            var result = httpActionContext.Response;
            Assert.IsFalse(result.IsSuccessStatusCode);
            var responseMessage = result.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("<Error>\r\n  <Status>403</Status>\r\n  <Title>user_forbidden</Title>\r\n  <Message>Authorization has been denied for this request.</Message>\r\n</Error>", responseMessage);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_UserIsNotAuthenticated_ResponseIsUnauthorized()
        {
            Verify_OnAuthorization_Response(false, null, false, HttpStatusCode.Unauthorized, GlobalConstants.USER_UNAUTHORIZED, ErrorResource.AuthorizationDeniedForThisUser);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_UserIsAuthenticatedAndNotAuthorized_ResponseIsAccessDenied()
        {
            Verify_OnAuthorization_Response(true, null, false, HttpStatusCode.Forbidden, GlobalConstants.USER_FORBIDDEN, ErrorResource.AuthorizationDeniedForThisRequest);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_UserIsAuthenticatedAndAuthorized_ResponseIsNull()
        {
            Verify_OnAuthorization_Response(true, null, true, HttpStatusCode.OK, string.Empty, null);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenInternalService_UserIsAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(true, "ping", true, HttpStatusCode.OK, string.Empty, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenInternalService_UserIsNotAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(false, "ping", true, HttpStatusCode.Unauthorized, GlobalConstants.USER_UNAUTHORIZED, ErrorResource.AuthorizationDeniedForThisUser);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenNotInternalService_UserIsNotAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(false, "pingme", true, HttpStatusCode.Unauthorized, GlobalConstants.USER_UNAUTHORIZED, ErrorResource.AuthorizationDeniedForThisUser);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenNotInternalService_UserIsAuthenticated()
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

            var attribute = new AuthorizeWebAttribute(authorizationProvider.Object);
            var actionContext = CreateActionContext(isAuthenticated, actionName);

            //------------Execute Test---------------------------
            attribute.OnAuthorization(actionContext);

            //------------Assert Results-------------------------
            if (isAuthorized && isAuthenticated)
            {
                Assert.IsNull(actionContext.Response);
            }
            else
            {
                Assert.AreEqual(expectedStatusCode, actionContext.Response.StatusCode);
                
                var errorObject = new Error
                {
                    Status = (int)expectedStatusCode,
                    Title = title,
                    Message = expectedMessage
                };
                var actualResponse = actionContext.Response.Content.ReadAsStringAsync().Result;
                if (emitionTypes.Equals(EmitionTypes.XML))
                {
                    Assert.AreEqual(errorObject.ToXML(), actualResponse);
                }
                Assert.AreEqual(errorObject.ToJSON(), actualResponse);
            }
        }

        public static HttpActionContext CreateActionContext(bool isAuthenticated, string actionName)
        {
            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

            var actionDescriptor = new Mock<HttpActionDescriptor>();
            actionDescriptor.Setup(ad => ad.ActionName).Returns(actionName);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/content/site.css");
            if(!string.IsNullOrEmpty(actionName))
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
