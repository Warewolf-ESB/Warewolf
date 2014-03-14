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
            if(isAuthorized)
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

            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/content/site.css"),
                    RequestContext = new HttpRequestContext { Principal = user.Object }
                },
                ActionDescriptor = actionDescriptor.Object
            };
            return actionContext;
        }
    }
}
