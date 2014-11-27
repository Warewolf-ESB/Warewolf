
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
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
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
            // ReSharper disable ObjectCreationAsStatement
            new AuthorizeWebAttribute(null);
            // ReSharper restore ObjectCreationAsStatement

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
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_UserIsNotAuthenticated_ResponseIsUnauthorized()
        {
            Verify_OnAuthorization_Response(false, null, false, HttpStatusCode.Unauthorized, "Authorization has been denied for this request.");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_UserIsAuthenticatedAndNotAuthorized_ResponseIsAccessDenied()
        {
            Verify_OnAuthorization_Response(true, null, false, HttpStatusCode.Forbidden, "Access has been denied for this request.");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_UserIsAuthenticatedAndAuthorized_ResponseIsNull()
        {
            Verify_OnAuthorization_Response(true, null, true, HttpStatusCode.OK, null);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenInternalService_UserIsAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(true, "ping", true, HttpStatusCode.OK, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenInternalService_UserIsNotAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(false, "ping", true, HttpStatusCode.Unauthorized, "Authorization has been denied for this request.");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenNotInternalService_UserIsNotAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(false, "pingme", true, HttpStatusCode.Unauthorized, "Authorization has been denied for this request.");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        public void AuthorizeWebAttribute_OnAuthorization_WhenNotInternalService_UserIsAuthenticated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Verify_OnAuthorization_Response(true, "pingme", true, HttpStatusCode.Forbidden, null);
            //------------Assert Results-------------------------
        }

        static void Verify_OnAuthorization_Response(bool isAuthenticated, string actionName, bool isAuthorized, HttpStatusCode expectedStatusCode, string expectedMessage)
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            authorizationProvider.Setup(p => p.IsAuthorized(It.IsAny<IAuthorizationRequest>())).Returns(isAuthorized);

            var attribute = new AuthorizeWebAttribute(authorizationProvider.Object);
            var actionContext = CreateActionContext(isAuthenticated, actionName);

            //------------Execute Test---------------------------
            attribute.OnAuthorization(actionContext);

            //------------Assert Results-------------------------
            if(isAuthorized && isAuthenticated)
            {
                Assert.IsNull(actionContext.Response);
            }
            else
            {
                Assert.AreEqual(expectedStatusCode, actionContext.Response.StatusCode);
                Assert.AreEqual(expectedStatusCode.ToString(), actionContext.Response.ReasonPhrase);

                var task = actionContext.Response.Content.ReadAsStringAsync();
                task.Wait();
                Assert.AreEqual(string.Format("{{\"Message\":\"{0}\"}}", expectedMessage), task.Result);
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
