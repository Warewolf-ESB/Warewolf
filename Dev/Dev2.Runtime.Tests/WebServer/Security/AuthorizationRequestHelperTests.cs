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
using System.Linq;
using System.Net;
using System.Reflection;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Hubs;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AuthorizationRequestHelper))]
        public void AuthorizationRequestHelper_CalculateResponseMessage_HttpActionContext_GivenJSONURI_ShouldReturnJSON()
        {
            var sut = AuthorizeWebAttributeTests.CreateActionContext(true, "http://localhost:3241/help/wolf-tools/redis.json");
            sut.CalculateResponseMessage(HttpStatusCode.Unauthorized, "test_title", "test_message");

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
        [TestCategory(nameof(AuthorizationRequestHelper))]
        public void AuthorizationRequestHelper_CalculateResponseMessage_HttpActionContext_GivenXMLURI_ShouldReturnXML()
        {
            var sut = AuthorizeWebAttributeTests.CreateActionContext(true, "http://localhost:3241/help/wolf-tools/gates.xml");
            sut.CalculateResponseMessage(HttpStatusCode.Unauthorized, "test_title", "test_message");

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
        [TestCategory(nameof(AuthorizationRequestHelper))]
        public void AuthorizationRequestHelper_CalculateResponseMessage_HttpActionContext_GivenTRXURI_ShouldReturnXML()
        {
            var sut = AuthorizeWebAttributeTests.CreateActionContext(true, "http://localhost:3241/help/wolf-configs/logger.trx?name=elastic");
            sut.CalculateResponseMessage(HttpStatusCode.Unauthorized, "test_title", "test_message");

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
        [TestCategory(nameof(AuthorizationRequestHelper))]
        public void AuthorizationRequestHelper_GetEmitionType_GivenAnyOther_ShouldDefaultToJSON()
        {
            var uri = new Uri("htttps://localhost:3241/help/wolf/workflows.unknown-ext");
            var result = AuthorizationRequestHelper.GetEmitionType(uri);

            Assert.AreEqual(Web.EmitionTypes.JSON, result);

            result = uri.GetEmitionType();
            Assert.AreEqual(Web.EmitionTypes.JSON, result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AuthorizationRequestHelper))]
        public void AuthorizationRequestHelper_GetEmitionType_GivenXMLExt_ShouldReturnXML()
        {
            var uri = new Uri("htttps://localhost:3241/help/wolf/workflows.xml");
            var result = AuthorizationRequestHelper.GetEmitionType(uri);

            Assert.AreEqual(Web.EmitionTypes.XML, result);

            result = uri.GetEmitionType();
            Assert.AreEqual(Web.EmitionTypes.XML, result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AuthorizationRequestHelper))]
        public void AuthorizationRequestHelper_GetEmitionType_GivenTRXExt_ShouldReturnXML()
        {
            var uri = new Uri("htttps://localhost:3241/help/wolf/workflows.trx");
            var result = AuthorizationRequestHelper.GetEmitionType(uri);

            Assert.AreEqual(Web.EmitionTypes.XML, result);

            result = uri.GetEmitionType();
            Assert.AreEqual(Web.EmitionTypes.XML, result);
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
                var hub1 = hub;
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
                Verify_RequestType(() => getAuthorizationRequest?.Invoke(actionName), expectedRequestType);
            }
        }

        static void Verify_RequestType(Func<AuthorizationRequest> getAuthorizationRequest, WebServerRequestType expectedRequestType)
        {
            //------------Execute Test---------------------------
            var authorizationRequest = getAuthorizationRequest?.Invoke();

            //------------Assert Results-------------------------
            Assert.IsNotNull(authorizationRequest);
            Assert.AreEqual(expectedRequestType, authorizationRequest.RequestType);
        }
    }
}
