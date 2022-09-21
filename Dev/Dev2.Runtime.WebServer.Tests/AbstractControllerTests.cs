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
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class AbstractControllerTests
    {
        //[TestMethod]
        //[TestCategory(nameof(AbstractController))]
        //[Owner("Rory McGuire")]
        //public void AbstractController_ProcessRequest_GivenNotAuthenticated_ExpectUnauthorized()
        //{
        //    var controller = new AbstractControllerForTesting
        //    {
        //        Request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:3241/token/Hello%20World.json?Name=")
        //    };
        //    var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(false);

        //    var result = response.Content.ReadAsStringAsync().Result;
        //    Assert.AreEqual(response.StatusCode, HttpStatusCode.Unauthorized);
        //    Assert.AreEqual(response.ReasonPhrase, "Unauthorized");
        //    Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 401,\r\n    \"Title\": \"user_unauthorized\",\r\n    \"Message\": \"Authorization has been denied for this user.\"\r\n  }\r\n}", result);
        //}

        //[TestMethod]
        //[TestCategory(nameof(AbstractController))]
        //[Owner("Rory McGuire")]
        //public void AbstractController_ProcessRequest_GivenTokenEmptyNotAuthenticated_ExpectUnauthorized()
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/token/Hello%20World.json?Name=")
        //    {
        //        Headers = { {"Authorization", "bearer "}},
        //        Content = new StringContent("Some content"),
        //    };
        //    var controller = new AbstractControllerForTesting
        //    {
        //        Request = request
        //    };


        //    var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(true);

        //    var result = response.Content.ReadAsStringAsync().Result;
        //    Assert.IsFalse(response.IsSuccessStatusCode);
        //    Assert.AreEqual(HttpStatusCode.Unauthorized.ToString(), response.ReasonPhrase);
        //    Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 401,\r\n    \"Title\": \"token_unauthorized\",\r\n    \"Message\": \"Authorization has been denied for this token.\"\r\n  }\r\n}", result);
        //}

        //[TestMethod]
        //[TestCategory(nameof(AbstractController))]
        //[Owner("Rory McGuire")]
        //public void AbstractController_ProcessRequest_GivenTokenNotAuthenticated_ExpectUnauthorized()
        //{
        //    var pathhh = "https://localhost/token/Hello%20World.json?Name=";
        //    var path = pathhh.Split(new[] { "/apis.json" }, StringSplitOptions.RemoveEmptyEntries);
        //    var request = new HttpRequestMessage(HttpMethod.Get, pathhh)
        //    {
        //        Headers = { {"Authorization", "bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9hdXRoZW50aWNhdGlvbiI6InsnVXNlckdyb3Vwcyc6IFt7J05hbWUnOiAncHVibGljJyB9XX0iLCJuYmYiOjE1OTExOTcyMDEsImV4cCI6MTU5MTE5ODQwMSwiaWF0IjoxNTkxMTk3MjAxfQ.1zo2d8Z7yxDvGZGVXbCf8zQKh7WZeyGx7BfWu4drt4g"}},
        //        Content = new StringContent("Some content"),
        //    };
        //    var controller = new AbstractControllerForTesting
        //    {
        //        Request = request
        //    };
        //    var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(true);
        //    var result = response.Content.ReadAsStringAsync().Result;
        //    Assert.AreEqual(HttpStatusCode.Unauthorized.ToString(), response.ReasonPhrase);
        //    Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 401,\r\n    \"Title\": \"token_unauthorized\",\r\n    \"Message\": \"Authorization has been denied for this token.\"\r\n  }\r\n}", result);
        //}

        //[TestMethod]
        //[TestCategory(nameof(AbstractController))]
        //[Owner("Siphamandla Dube")]
        //public void AbstractController_ProcessRequest_GivenSomethingGoesWrong_ExpectInternalServerError()
        //{
        //    var mockNameValue = new Mock<NameValueCollection>();
        //    mockNameValue.Setup(o => o.Get(It.IsAny<string>()))
        //        .Throws(new Exception("false exception for testing catch"));

        //    var pathhh = "https://localhost/token/Hello%20World.json?Name=";
        //    var request = new HttpRequestMessage(HttpMethod.Get, pathhh)
        //    {
        //        Headers = { { "Authorization", "bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9hdXRoZW50aWNhdGlvbiI6InsnVXNlckdyb3Vwcyc6IFt7J05hbWUnOiAncHVibGljJyB9XX0iLCJuYmYiOjE1OTExOTcyMDEsImV4cCI6MTU5MTE5ODQwMSwiaWF0IjoxNTkxMTk3MjAxfQ.1zo2d8Z7yxDvGZGVXbCf8zQKh7WZeyGx7BfWu4drt4g" } },
        //        Content = new StringContent("Some content"),
        //    };
        //    var controller = new AbstractControllerForTesting
        //    {
        //        Request = request
        //    };
        //    var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(true, mockNameValue.Object);
        //    var result = response.Content.ReadAsStringAsync().Result;
        //    Assert.AreEqual(HttpStatusCode.Unauthorized.ToString(), response.ReasonPhrase);
        //    Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 401,\r\n    \"Title\": \"token_unauthorized\",\r\n    \"Message\": \"Authorization has been denied for this token.\"\r\n  }\r\n}", result);
        //}

        //[TestMethod]
        //[TestCategory(nameof(AbstractController))]
        //[Owner("Siphamandla Dube")]
        //public void AbstractController_ProcessRequest_GivenNotAuthenticated_ExpectUnauthorized1()
        //{
        //    var mockNameValue = new Mock<NameValueCollection>();
        //    mockNameValue.Setup(o => o.Get(It.IsAny<string>()))
        //        .Throws(new Exception("false exception for testing catch"));

        //    var mock = new Mock<IPrincipal>();
        //    mock.Setup(o => o.Identity.IsAuthenticated)
        //        .Returns(true);

        //    var path = "https://localhost/token/Hello%20World.json?Name=";
        //    var request = new HttpRequestMessage(HttpMethod.Get, path)
        //    {
        //        Headers = { { "Authorization", "bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9hdXRoZW50aWNhdGlvbiI6InsnVXNlckdyb3Vwcyc6IFt7J05hbWUnOiAncHVibGljJyB9XX0iLCJuYmYiOjE1OTExOTcyMDEsImV4cCI6MTU5MTE5ODQwMSwiaWF0IjoxNTkxMTk3MjAxfQ.1zo2d8Z7yxDvGZGVXbCf8zQKh7WZeyGx7BfWu4drt4g" } },
        //        Content = new StringContent("Some content"),
        //    };

        //    var controller = new AbstractControllerForTesting
        //    {
        //        User = mock.Object, 
        //        Request = request
        //    };
        //    var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(false, mockNameValue.Object);

        //    var result = response.Content.ReadAsStringAsync().Result;
        //    Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        //    Assert.AreEqual("Internal Server Error", response.ReasonPhrase);
        //    Assert.AreEqual("{\r\n  \"Error\": {\r\n    \"Status\": 500,\r\n    \"Title\": \"internal_server_error\",\r\n    \"Message\": \"not expected to be executed\"\r\n  }\r\n}", result);
        //}

    }

    internal class AbstractControllerForTesting : AbstractController
    {
        public HttpResponseMessage TestProcessRequest<TRequestHandler>(bool isUrlWithTokenPrefix, NameValueCollection nameValue = null) where TRequestHandler : class, IRequestHandler, new()
        {
            NameValueCollection vars = nameValue == null ? new NameValueCollection() : nameValue;
            return ProcessRequest<TRequestHandler>(vars, isUrlWithTokenPrefix);
        }
    }

    internal class AssertNotExecutedRequestHandlerForTesting : IRequestHandler
    {
        public void ProcessRequest(ICommunicationContext ctx)
        {
            throw new Exception("not expected to be executed");
        }
    }

    internal class RequestHandlerForTesting : IRequestHandler
    {
        public void ProcessRequest(ICommunicationContext ctx)
        {

        }
    }
}
