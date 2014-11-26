
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
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.WebServer.Controllers
{
    [TestClass]
    public class WebServerControllerTests
    {
        const string WebSite = "wwwroot";

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Get")]
        public void WebServerController_Get_WebsiteFolderFile_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite }, 
                { "path", "dialogs/SaveDialog.htm" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.Get(WebSite, "dialogs", "SaveDialog.htm");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteResourceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Get")]
        public void WebServerController_Get_WebsitePathFolderStarFile_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite }, 
                { "path", "services" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.Get(WebSite, "services", "Scripts", "fx/jquery.caret.js");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteResourceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Get")]
        public void WebServerController_Get_WebsiteGet_InvokesWebsiteResourceHandler()
        {
            Verify_WebsiteGetFile(WebServerRequestType.WebGetContent, "content/site.css");
            Verify_WebsiteGetFile(WebServerRequestType.WebGetImage, "images/test.png");
            Verify_WebsiteGetFile(WebServerRequestType.WebGetScript, "scripts/fx/test.js");
            Verify_WebsiteGetFile(WebServerRequestType.WebGetView, "views/services/webservice.htm");
        }

        static void Verify_WebsiteGetFile(WebServerRequestType requestType, string url)
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite },
                { "path", url }
            };

            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            switch(requestType)
            {
                case WebServerRequestType.WebGetContent:
                    controller.GetContent(WebSite, url);
                    break;
                case WebServerRequestType.WebGetImage:
                    controller.GetImage(WebSite, url);
                    break;
                case WebServerRequestType.WebGetScript:
                    controller.GetScript(WebSite, url);
                    break;
                case WebServerRequestType.WebGetView:
                    controller.GetView(WebSite, url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("requestType");
            }

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteResourceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_InvokeService")]
        public void WebServerController_InvokeService_WebsitePathNameMethod_InvokesWebsiteServiceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite }, 
                { "path", "Service" },
                { "name", "Resources" },
                { "action", "PathsAndNames" },
            };
            var controller = new TestWebServerController(HttpMethod.Post);

            //------------Execute Test---------------------------
            controller.InvokeService(WebSite, "Service", "Resources", "PathsAndNames");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteServiceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerController_Execute_Post_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "servicename", "HelloWorld" }
            };
            var controller = new TestWebServerController(HttpMethod.Post);

            //------------Execute Test---------------------------
            controller.ExecuteWorkflow("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebPostRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerController_Execute_Get_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "servicename", "HelloWorld" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.ExecuteWorkflow("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebGetRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_ProcessRequest")]
        public void WebServerController_ProcessRequest_UserIsNull_Unauthorized()
        {
            //------------Setup for test--------------------------
            var controller = new TestUserWebServerController(HttpMethod.Get, null);

            //------------Bookmark Test---------------------------
            var response = controller.TestProcessRequest();

            //------------Assert Results-------------------------
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_ProcessRequest")]
        public void WebServerController_ProcessRequest_UserIsNotAuthenticated_Unauthorized()
        {
            //------------Setup for test--------------------------
            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(false);

            var controller = new TestUserWebServerController(HttpMethod.Get, user.Object);

            //------------Bookmark Test---------------------------
            var response = controller.TestProcessRequest();

            //------------Assert Results-------------------------
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_ProcessRequest")]
        public void WebServerController_ProcessRequest_UserIsAuthenticated_Ok()
        {
            //------------Setup for test--------------------------
            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(true);

            var controller = new TestUserWebServerController(HttpMethod.Get, user.Object);

            //------------Bookmark Test---------------------------
            var response = controller.TestProcessRequest();

            //------------Assert Results-------------------------
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_CreateHandler")]
        public void WebServerController_CreateHandler_IsNotNull()
        {
            //------------Setup for test--------------------------
            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.IsAuthenticated).Returns(true);

            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Bookmark Test---------------------------
            var result = controller.TestCreateHandler<WebsiteResourceHandler>();

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(WebsiteResourceHandler));
        }
    }
}
