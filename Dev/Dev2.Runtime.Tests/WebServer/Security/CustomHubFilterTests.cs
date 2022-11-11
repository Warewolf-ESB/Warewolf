/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Claims;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class CustomHubFilterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomHubFilter_Constructor")]
        public void CustomHubFilter_Constructor_Default_ProviderIsAuthorizationProviderInstance()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new CustomHubFilter();

            //------------Assert Results-------------------------
            Assert.AreSame(ServerAuthorizationService.Instance, attribute.Service);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomHubFilter_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CustomHubFilter_Constructor_AuthorizationProviderIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var attribute = new CustomHubFilter(null);

            //------------Assert Results-------------------------
        }

       
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomHubFilter_AuthorizeHubMethodInvocation")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CustomHubFilter_AuthorizeHubMethodInvocation_HubIncomingInvokerContextIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            var attribute = new CustomHubFilter(authorizationProvider.Object);

            //------------Execute Test---------------------------
            var result = attribute.InvokeMethodAsync(null, null).Result;

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomHubFilter_AuthorizeHubMethodInvocation")]
        public void CustomHubFilter_AuthorizeHubMethodInvocation_UserIsNotAuthenticated_ResponseIsFalse()
        {
            Verify_AuthorizeHubMethodInvocation(isAuthenticated: false, isAuthorized: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomHubFilter_AuthorizeHubMethodInvocation")]
        public void CustomHubFilter_AuthorizeHubMethodInvocation_UserIsAuthenticatedAndNotAuthorized_ResponseIsFalse()
        {
            Verify_AuthorizeHubMethodInvocation(isAuthenticated: true, isAuthorized: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CustomHubFilter_AuthorizeHubMethodInvocation")]
        public void CustomHubFilter_AuthorizeHubMethodInvocation_UserIsAuthenticatedAndAuthorized_ResponseIsTrue()
        {
            Verify_AuthorizeHubMethodInvocation(isAuthenticated: true, isAuthorized: true);
        }

        static void Verify_AuthorizeHubMethodInvocation(bool isAuthenticated, bool isAuthorized, string methodName = null)
        {
            //------------Setup for test--------------------------
            var authorizationProvider = new Mock<IAuthorizationService>();
            authorizationProvider.Setup(p => p.IsAuthorized(It.IsAny<IAuthorizationRequest>())).Returns(isAuthorized);

            var attribute = new CustomHubFilter(authorizationProvider.Object);

            //------------Execute Test---------------------------
            var response = attribute.AuthorizeHubMethodInvocation(CreateHubInvocationContext(isAuthenticated, methodName), WebServerRequestType.Unknown);

            //------------Assert Results-------------------------
            Assert.AreEqual(isAuthenticated && isAuthorized, response);
        }

        public static Mock<HttpRequest> CreateRequest(string methodName)
        {
            var routeValues = new RouteValueDictionary();
            routeValues.Add("action", methodName);

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.RouteValues).Returns(routeValues);
            request.Setup(r => r.Scheme).Returns("http");
            request.Setup(r => r.Host).Returns(new HostString("localhost", 80));
            request.Setup(r => r.Path).Returns("");
            request.Setup(r => r.Query).Returns(new Mock<IQueryCollection>().Object);
            return request;
        }

        public static HttpContext CreateHubInvocationContext(bool isAuthenticated, string methodName, string hubName = null)
        {
            var user = new Mock<ClaimsPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

            var context = new Mock<HttpContext>();
            context.Setup(r => r.User).Returns(user.Object);
            context.Setup(r => r.Request).Returns(CreateRequest(methodName).Object);

            return context.Object;
        }


        //public static HubInvocationContext CreateHubInvocationContext1(bool isAuthenticated, string methodName, string hubName = null)
        //{
        //    var user = new Mock<ClaimsPrincipal>();
        //    user.Setup(u => u.Identity.IsAuthenticated).Returns(isAuthenticated);

        //    var callerContext = new Mock<HubCallerContext>();
        //    callerContext.Setup(r => r.User).Returns(user.Object);

        //    var context = new Mock<HubInvocationContext>();
        //    context.Setup(c => c.Context).Returns(callerContext.Object);
        //    context.Setup(c => c.Hub).Returns(new Mock<Hub>().Object);
            

        //    return context.Object;
        //}
    }
}
