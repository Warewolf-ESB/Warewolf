/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Web;
using Dev2.Runtime.WebServer.Controllers;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Security;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class AbstractControllerTests
    {
        [TestMethod]
        [TestCategory(nameof(AbstractController))]
        [Owner("Rory McGuire")]
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
        [TestCategory(nameof(AbstractController))]
        [Owner("Rory McGuire")]
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
                Assert.Fail($"expected HttpException but found {e.GetType().FullName} with message \"{e.Message}\"");
            }

            Assert.IsTrue(hadHttpException, "expected HttpException");
        }

        [TestMethod]
        [TestCategory(nameof(AbstractController))]
        [Owner("Rory McGuire")]
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
        [TestCategory(nameof(AbstractController))]
        [Owner("Rory McGuire")]
        public void AbstractController_ProcessRequest_GivenValidTokenAuthenticated_ExpectSuccess()
        {
            var securitySettings = new SecuritySettings();
            var jwtManager = new JwtManager(securitySettings);
            var token = jwtManager.GenerateToken("{'UserGroups': [{'Name': 'public' }]}");
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

    internal class AbstractControllerForTesting : AbstractController
    {
        public HttpResponseMessage TestProcessRequest<TRequestHandler>(bool isUrlWithTokenPrefix) where TRequestHandler : class, IRequestHandler, new()
        {
            var vars = new NameValueCollection();
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
