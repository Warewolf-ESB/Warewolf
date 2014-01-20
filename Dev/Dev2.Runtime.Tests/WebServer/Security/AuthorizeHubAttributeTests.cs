using System;
using System.Security.Principal;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    public class AuthorizeHubAttributeTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_Constructor")]
        public void AuthorizeHubAttribute_Constructor_Default_ProviderIsAuthorizationProviderInstance()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new AuthorizeHubAttribute();

            //------------Assert Results-------------------------
            Assert.AreSame(ServerAuthorizationService.Instance, attribute.Service);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeHubAttribute_Constructor_AuthorizationProviderIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new AuthorizeHubAttribute(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubConnection")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeHubAttribute_AuthorizeHubConnection_HubDescriptorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            var attribute = new AuthorizeHubAttribute(authorizationProvider.Object);

            //------------Execute Test---------------------------
            attribute.AuthorizeHubConnection(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubConnection")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeHubAttribute_AuthorizeHubConnection_RequestIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            var attribute = new AuthorizeHubAttribute(authorizationProvider.Object);

            //------------Execute Test---------------------------
            attribute.AuthorizeHubConnection(new HubDescriptor(), null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubConnection")]
        public void AuthorizeHubAttribute_AuthorizeHubConnection_UserIsNotAuthenticated_ResponseIsFalse()
        {
            Verify_AuthorizeHubConnection(isAuthenticated: false, isAuthorized: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubConnection")]
        public void AuthorizeHubAttribute_AuthorizeHubConnection_UserIsAuthenticatedAndNotAuthorized_ResponseIsFalse()
        {
            Verify_AuthorizeHubConnection(isAuthenticated: true, isAuthorized: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubConnection")]
        public void AuthorizeHubAttribute_AuthorizeHubConnection_UserIsAuthenticatedAndAuthorized_ResponseIsTrue()
        {
            Verify_AuthorizeHubConnection(isAuthenticated: true, isAuthorized: true);
        }

        static void Verify_AuthorizeHubConnection(bool isAuthenticated, bool isAuthorized)
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            authorizationProvider.Setup(p => p.IsAuthorized(It.IsAny<IAuthorizationRequest>())).Returns(isAuthorized);
            var attribute = new AuthorizeHubAttribute(authorizationProvider.Object);

            //------------Execute Test---------------------------
            var response = attribute.AuthorizeHubConnection(new HubDescriptor(), CreateRequest(isAuthenticated: isAuthenticated).Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(isAuthenticated && isAuthorized, response);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubMethodInvocation")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizeHubAttribute_AuthorizeHubMethodInvocation_HubIncomingInvokerContextIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            var attribute = new AuthorizeHubAttribute(authorizationProvider.Object);

            //------------Execute Test---------------------------
            attribute.AuthorizeHubMethodInvocation(null, false);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubMethodInvocation")]
        public void AuthorizeHubAttribute_AuthorizeHubMethodInvocation_UserIsNotAuthenticated_ResponseIsFalse()
        {
            Verify_AuthorizeHubMethodInvocation(isAuthenticated: false, isAuthorized: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubMethodInvocation")]
        public void AuthorizeHubAttribute_AuthorizeHubMethodInvocation_UserIsAuthenticatedAndNotAuthorized_ResponseIsFalse()
        {
            Verify_AuthorizeHubMethodInvocation(isAuthenticated: true, isAuthorized: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizeHubAttribute_AuthorizeHubMethodInvocation")]
        public void AuthorizeHubAttribute_AuthorizeHubMethodInvocation_UserIsAuthenticatedAndAuthorized_ResponseIsTrue()
        {
            Verify_AuthorizeHubMethodInvocation(isAuthenticated: true, isAuthorized: true);
        }

        static void Verify_AuthorizeHubMethodInvocation(bool isAuthenticated, bool isAuthorized, string methodName = null)
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            authorizationProvider.Setup(p => p.IsAuthorized(It.IsAny<IAuthorizationRequest>())).Returns(isAuthorized);

            var attribute = new AuthorizeHubAttribute(authorizationProvider.Object);

            //------------Execute Test---------------------------
            var response = attribute.AuthorizeHubMethodInvocation(CreateHubIncomingInvokerContext(isAuthenticated, methodName), false);

            //------------Assert Results-------------------------
            Assert.AreEqual(isAuthenticated && isAuthorized, response);
        }

        public static Mock<IRequest> CreateRequest(bool isAuthenticated)
        {
            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

            var request = new Mock<IRequest>();
            request.Setup(r => r.User).Returns(user.Object);
            return request;
        }

        public static IHubIncomingInvokerContext CreateHubIncomingInvokerContext(bool isAuthenticated, string methodName, string hubName = null)
        {
            var request = CreateRequest(isAuthenticated);

            var hub = new Mock<IHub>();
            hub.Setup(h => h.Context).Returns(new HubCallerContext(request.Object, "connectionId"));

            var methodDescriptor = new MethodDescriptor { Name = methodName, Hub = new HubDescriptor { Name = hubName } };

            var context = new Mock<IHubIncomingInvokerContext>();
            context.Setup(c => c.Hub).Returns(hub.Object);
            context.Setup(c => c.MethodDescriptor).Returns(methodDescriptor);

            return context.Object;
        }
    }
}