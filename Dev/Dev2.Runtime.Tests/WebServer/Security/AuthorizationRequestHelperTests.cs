/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Reflection;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Hubs;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    public class AuthorizationRequestHelperTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationRequestHelper_GetAuthorizationRequest")]
        public void AuthorizationRequestHelper_GetAuthorizationRequest_HttpActionContext_RequestTypeIsParsedCorrectly()
        {
            Verify_RequestType(() =>
            {
                var context = AuthorizeWebAttributeTests.CreateActionContext(true, "xxx");
                return context.GetAuthorizationRequest();
            }, WebServerRequestType.Unknown);

            Verify_RequestTypeIsParsedCorrectly(typeof(WebServerController), "Web", actionName =>
            {
                var context = AuthorizeWebAttributeTests.CreateActionContext(true, actionName);
                return context.GetAuthorizationRequest();
            });
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationRequestHelper_GetAuthorizationRequest")]
        public void AuthorizationRequestHelper_GetAuthorizationRequest_HubDescriptor_RequestTypeIsParsedCorrectly()
        {
            Verify_RequestType(() =>
            {
                var request = AuthorizeHubAttributeTests.CreateRequest(false);
                return new HubDescriptor().GetAuthorizationRequest(request.Object);
            }, WebServerRequestType.HubConnect);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationRequestHelper_GetAuthorizationRequest")]
        public void AuthorizationRequestHelper_GetAuthorizationRequest_IHubIncomingInvokerContext_RequestTypeIsParsedCorrectly()
        {
            Verify_RequestType(() =>
            {
                var context = AuthorizeHubAttributeTests.CreateHubIncomingInvokerContext(true, "xxx");
                return context.GetAuthorizationRequest();
            }, WebServerRequestType.Unknown);


            var hubs = new[]
            {
                new Tuple<Type, string>(typeof(EsbHub), "esb"),
            };

            foreach(var hub in hubs)
            {
                Tuple<Type, string> hub1 = hub;
                Func<string, AuthorizationRequest> getAuthorizationRequest = methodName =>
                {
                    var context = AuthorizeHubAttributeTests.CreateHubIncomingInvokerContext(true, methodName, hub1.Item2);
                    return context.GetAuthorizationRequest();
                };
                Verify_RequestTypeIsParsedCorrectly(hub1.Item1, hub1.Item2, getAuthorizationRequest);
            }
        }

        static void Verify_RequestTypeIsParsedCorrectly(Type handlerType, string handlerPrefix, Func<string, AuthorizationRequest> getAuthorizationRequest)
        {
            var methodNames = handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(mi => !mi.IsSpecialName).Select(mi => mi.Name);
            foreach(var methodName in methodNames)
            {
                var expectedRequestType = (WebServerRequestType)Enum.Parse(typeof(WebServerRequestType), handlerPrefix + methodName, true);

                var actionName = methodName;
                Verify_RequestType(() => getAuthorizationRequest(actionName), expectedRequestType);
            }
        }

        static void Verify_RequestType(Func<AuthorizationRequest> getAuthorizationRequest, WebServerRequestType expectedRequestType)
        {
            //------------Execute Test---------------------------
            var authorizationRequest = getAuthorizationRequest();

            //------------Assert Results-------------------------
            Assert.IsNotNull(authorizationRequest);
            Assert.AreEqual(expectedRequestType, authorizationRequest.RequestType);
        }
    }
}
