using System;
using System.CodeDom;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class AbstractControllerTests
    {
        [TestMethod]
        public void AbstractController_ProcessRequest_GivenNotAuthenticated_ExpectUnauthorized()
        {
            var controller = new AbstractControllerForTesting
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "/token/Hello%20World.json?Name=")
            };
            var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(false);

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Unauthorized);
            Assert.AreEqual(response.ReasonPhrase, "Unauthorized");
        }

        [TestMethod]
        public void AbstractController_ProcessRequest_GivenTokenEmptyNotAuthenticated_ExpectUnauthorized()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/token/Hello%20World.json?Name=")
            {
                Headers = { {"Authorization", "bearer "}},
            };
            var controller = new AbstractControllerForTesting
            {
                Request = request
            };
            var hadHttpException = false;
            try
            {
                var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(true);
                Assert.AreEqual(response.ReasonPhrase, Warewolf.Resource.Errors.ErrorResource.TokenNotAuthorizedToExecuteOuterWorkflowException);
                Assert.Fail("expected exception");
            }
            catch (HttpException e)
            {
                Assert.IsTrue(string.Equals(e.Message, Warewolf.Resource.Errors.ErrorResource.TokenNotAuthorizedToExecuteOuterWorkflowException, StringComparison.InvariantCultureIgnoreCase));
                hadHttpException = true;
            } catch (Exception e)
            {
                Assert.Fail("expected HttpException");
            }

            Assert.IsTrue(hadHttpException, "expected HttpException");
        }

        [TestMethod]
        public void AbstractController_ProcessRequest_GivenTokenNotAuthenticated_ExpectUnauthorized()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/token/Hello%20World.json?Name=")
            {
                Headers = { {"Authorization", "bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9hdXRoZW50aWNhdGlvbiI6InsnVXNlckdyb3Vwcyc6IFt7J05hbWUnOiAncHVibGljJyB9XX0iLCJuYmYiOjE1OTExOTcyMDEsImV4cCI6MTU5MTE5ODQwMSwiaWF0IjoxNTkxMTk3MjAxfQ.1zo2d8Z7yxDvGZGVXbCf8zQKh7WZeyGx7BfWu4drt4g"}},
                Content = new StringContent("Some content"),
            };
            var controller = new AbstractControllerForTesting
            {
                Request = request
            };
            var hadHttpException = false;
            try
            {
                var response = controller.TestProcessRequest<AssertNotExecutedRequestHandlerForTesting>(true);
                Assert.AreEqual(response.ReasonPhrase, Warewolf.Resource.Errors.ErrorResource.TokenNotAuthorizedToExecuteOuterWorkflowException);
                Assert.Fail("expected exception");
            }
            catch (HttpException e)
            {
                Assert.IsTrue(string.Equals(e.Message, Warewolf.Resource.Errors.ErrorResource.TokenNotAuthorizedToExecuteOuterWorkflowException, StringComparison.InvariantCultureIgnoreCase));
                hadHttpException = true;
            } catch (Exception e)
            {
                Assert.Fail($"expected HttpException, not {e.Message}");
            }

            Assert.IsTrue(hadHttpException, "expected HttpException");
        }


        [TestMethod]
        public void AbstractController_ProcessRequest_GivenValidTokenAuthenticated_ExpectSuccess()
        {
            var token = ""; // TODO: generate real token here
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/token/Hello%20World.json?Name=")
            {
                Headers = { {"Authorization", $"bearer {token}"}},
                Content = new StringContent("Some content"),
            };
            var controller = new AbstractControllerForTesting
            {
                Request = request
            };

            var response = controller.TestProcessRequest<RequestHandlerForTesting>(true);
            Assert.IsTrue(response.IsSuccessStatusCode, "expected successful response");
        }
    }

    public class AbstractControllerForTesting : AbstractController
    {
        public HttpResponseMessage TestProcessRequest<TRequestHandler>(bool isUrlWithTokenPrefix) where TRequestHandler : class, IRequestHandler, new()
        {
            var vars = new NameValueCollection();
            return ProcessRequest<TRequestHandler>(vars, isUrlWithTokenPrefix);
        }
    }

    class AssertNotExecutedRequestHandlerForTesting : IRequestHandler
    {
        public void ProcessRequest(ICommunicationContext ctx)
        {
            throw new Exception("not expected to be executed");
        }
    }

    class RequestHandlerForTesting : IRequestHandler
    {
        public void ProcessRequest(ICommunicationContext ctx)
        {

        }
    }
}