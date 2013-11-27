using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
        public void WebServerController_Get_WebsiteStarFile_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite }, 
                { "path", "views/services/webservice.htm" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.Get(WebSite, "views/services/webservice.htm");

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
            controller.Execute("HelloWorld");

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
            controller.Execute("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebGetRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Bookmark")]
        public void WebServerController_Bookmark_Post_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "servicename", "HelloWorld" },
                { "instanceid", "inst" },
                { "bookmark", "bmk" }
            };
            var controller = new TestWebServerController(HttpMethod.Post);

            //------------Bookmark Test---------------------------
            controller.Bookmark("HelloWorld", "inst", "bmk");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebPostRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Bookmark")]
        public void WebServerController_Bookmark_Get_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "servicename", "HelloWorld" },
                { "instanceid", "inst" },
                { "bookmark", "bmk" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Bookmark Test---------------------------
            controller.Bookmark("HelloWorld", "inst", "bmk");

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
