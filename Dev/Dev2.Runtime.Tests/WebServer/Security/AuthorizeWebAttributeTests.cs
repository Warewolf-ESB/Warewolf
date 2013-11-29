using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Web.Http.Controllers;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Security;
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
            Assert.AreSame(AuthorizationProvider.Instance, attribute.Provider);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeWebAttribute_Constructor_AuthorizationProviderIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new AuthorizeWebAttribute(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_OnAuthorization")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeWebAttribute_OnAuthorization_ActionContextIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var provider = new Mock<IAuthorizationProvider>();
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
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeWebAttribute_ParseRequestType")]
        public void AuthorizeWebAttribute_ParseRequestType_ActionNameIsParsedCorrectly()
        {
            Verify_ActionNameIsParsedCorrectly("xxx", WebServerRequestType.Unknown);

            var methodNames = typeof(WebServerController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(mi => !mi.IsSpecialName).Select(mi => mi.Name);
            foreach(var methodName in methodNames)
            {
                Verify_ActionNameIsParsedCorrectly(methodName, (WebServerRequestType)Enum.Parse(typeof(WebServerRequestType), "Web" + methodName));
            }
        }

        static void Verify_ActionNameIsParsedCorrectly(string actionName, WebServerRequestType expectedRequestType)
        {
            //------------Setup for test--------------------------
            IAuthorizationRequest authorizationRequest = null;

            var authorizationProvider = new Mock<IAuthorizationProvider>();
            authorizationProvider.Setup(p => p.IsAuthorized(It.IsAny<IAuthorizationRequest>())).Callback((IAuthorizationRequest request) => authorizationRequest = request);

            var attribute = new AuthorizeWebAttribute(authorizationProvider.Object);
            var actionContext = CreateActionContext(true, actionName);

            //------------Execute Test---------------------------
            attribute.OnAuthorization(actionContext);

            //------------Assert Results-------------------------
            Assert.IsNotNull(authorizationRequest);
            Assert.AreEqual(expectedRequestType, authorizationRequest.RequestType);
        }

        static void Verify_OnAuthorization_Response(bool isAuthenticated, string actionName, bool isAuthorized, HttpStatusCode expectedStatusCode, string expectedMessage)
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationProvider>();
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

        static HttpActionContext CreateActionContext(bool isAuthenticated, string actionName)
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
