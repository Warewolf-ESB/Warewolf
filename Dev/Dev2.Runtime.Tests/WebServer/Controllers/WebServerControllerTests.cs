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
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;
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
        public void WebServerController_GetDecisions_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite },
                { "path", "dialogs/SaveDialog.htm" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.GetDecisions(WebSite, "SaveDialog.htm");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteResourceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Get")]
        public void WebServerController_GetDialogs_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite },
                { "path", "dialogs/SaveDialog.htm" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.GetDialogs(WebSite, "SaveDialog.htm");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteResourceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Get")]
        public void WebServerController_GetServices_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite },
                { "path", "dialogs/SaveDialog.htm" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.GetServices(WebSite, "SaveDialog.htm");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteResourceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Get")]
        public void WebServerController_GetSwitch_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite },
                { "path", "dialogs/SaveDialog.htm" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.GetSwitch(WebSite, "SaveDialog.htm");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebsiteResourceHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Get")]
        public void WebServerController_GetSources_InvokesWebsiteResourceHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "website", WebSite },
                { "path", "dialogs/SaveDialog.htm" }
            };
            var controller = new TestWebServerController(HttpMethod.Get);

            //------------Execute Test---------------------------
            controller.GetSources(WebSite, "SaveDialog.htm");

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
            switch (requestType)
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
                case WebServerRequestType.Unknown:
                    break;
                case WebServerRequestType.WebGetDecisions:
                    break;
                case WebServerRequestType.WebGetDialogs:
                    break;
                case WebServerRequestType.WebGetServices:
                    break;
                case WebServerRequestType.WebGetSources:
                    break;
                case WebServerRequestType.WebGetSwitch:
                    break;
                case WebServerRequestType.WebGet:
                    break;
                case WebServerRequestType.WebInvokeService:
                    break;
                case WebServerRequestType.WebExecuteService:
                    break;
                case WebServerRequestType.WebExecuteSecureWorkflow:
                    break;
                case WebServerRequestType.WebExecutePublicWorkflow:
                    break;
                case WebServerRequestType.WebExecuteGetLogFile:
                    break;
                case WebServerRequestType.WebExecuteGetRootLevelApisJson:
                    break;
                case WebServerRequestType.WebExecuteGetApisJsonForFolder:
                    break;
                case WebServerRequestType.WebBookmarkWorkflow:
                    break;
                case WebServerRequestType.HubConnect:
                    break;
                case WebServerRequestType.EsbOnConnected:
                    break;
                case WebServerRequestType.EsbOnDisconnected:
                    break;
                case WebServerRequestType.EsbOnReconnected:
                    break;
                case WebServerRequestType.EsbAddDebugWriter:
                    break;
                case WebServerRequestType.EsbFetchExecutePayloadFragment:
                    break;
                case WebServerRequestType.EsbExecuteCommand:
                    break;
                case WebServerRequestType.EsbAddItemMessage:
                    break;
                case WebServerRequestType.EsbSendMemo:
                    break;
                case WebServerRequestType.EsbFetchResourcesAffectedMemo:
                    break;
                case WebServerRequestType.EsbSendDebugState:
                    break;
                case WebServerRequestType.EsbWrite:
                    break;
                case WebServerRequestType.ResourcesSendMemo:
                    break;
                case WebServerRequestType.WebExecuteInternalService:
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
            controller.ExecuteService("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebPostRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerController_Execute_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            var requestVariables = new NameValueCollection
            {
                { "servicename", "HelloWorld" }
            };
            var controller = new TestWebServerController(HttpMethod.Post);

            //------------Execute Test---------------------------
            controller.ExecuteService("HelloWorld.apis.json");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(GetApisJsonServiceHandler), controller.ProcessRequestHandlerType);
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
            controller.ExecuteSecureWorkflow("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebGetRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerController_ExecuteGivenTestsRun_Get_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            const string requestUrl = "http://rsaklfnkosinath:3142/secure/Hello%20World/.tests";
            var requestVariables = new NameValueCollection
            {
                  { "path", requestUrl },
                  { "isPublic", false.ToString() },
                { "servicename", "*" },


            };

            var controller = new TestWebServerController(HttpMethod.Get, requestUrl);
            //------------Execute Test---------------------------
            controller.ExecuteSecureWorkflow("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebGetRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerController_ExecuteDotDebug_Get_InvokesWebGetRequestHandler()
        {
            //------------Setup for test--------------------------
            const string requestUrl = "http://http://rsaklfnkosinath:3142/secure/Hello%20World.debug?Name=&wid=7af4273a-4e31-4559-be7d-eecfb4c1b10e";
            var requestVariables = new NameValueCollection
            {
                    {"isPublic",true.ToString()},
                    {"IsDebug",true.ToString() },
                    {"servicename","HelloWorld.debug" }


            };

            var controller = new TestWebServerController(HttpMethod.Get, requestUrl);
            //------------Execute Test---------------------------
            controller.ExecuteSecureWorkflow("HelloWorld.debug");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebGetRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerController_ExecuteDotDebug_Post_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            const string requestUrl = "http://http://rsaklfnkosinath:3142/secure/Hello%20World.debug?Name=&wid=7af4273a-4e31-4559-be7d-eecfb4c1b10e";
            var requestVariables = new NameValueCollection
            {
                    {"isPublic",true.ToString()},
                    {"IsDebug",true.ToString() },
                    {"servicename","HelloWorld.debug" }


            };

            var controller = new TestWebServerController(HttpMethod.Post, requestUrl);
            //------------Execute Test---------------------------
            controller.ExecuteSecureWorkflow("HelloWorld.debug");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebPostRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerControllerPublic_ExecuteGivenTestsRun_Get_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            const string requestUrl = "http://rsaklfnkosinath:3142/Public/Hello%20World/.tests";
            var requestVariables = new NameValueCollection
            {
                  { "path", requestUrl },
                  { "isPublic", true.ToString() },
                { "servicename", "*" },


            };

            var controller = new TestWebServerController(HttpMethod.Get, requestUrl);
            //------------Execute Test---------------------------
            controller.ExecutePublicWorkflow("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebGetRequestHandler), controller.ProcessRequestHandlerType);
            CollectionAssert.AreEqual(requestVariables, controller.ProcessRequestVariables);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("WebServerController_Execute")]
        public void WebServerControllerPublic_ExecuteGivenNullUri_InvokesWebPostRequestHandler()
        {
            //------------Setup for test--------------------------
            const string requestUrl = "http://rsaklfnkosinath:3142/Public/Hello%20World/.tests";
            var requestVariables = new NameValueCollection
            {
                  { "path", requestUrl },
                  { "isPublic", true.ToString() },
                { "servicename", "*" },
            };

            var controller = new TestWebServerController(HttpMethod.Get, requestUrl);
            //------------Execute Test---------------------------
            controller.Request.RequestUri = null;
            controller.ExecutePublicWorkflow("HelloWorld");

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(WebGetRequestHandler), controller.ProcessRequestHandlerType);
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_CreateHandler")]
        public void WebServerController_ExecuteGetLogFile()
        {
            //------------Setup for test--------------------------
            var controller = new TestWebServerController(HttpMethod.Get);
            //------------Bookmark Test---------------------------
            var result = controller.ExecuteGetLogFile();
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(HttpResponseMessage));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WebServerController_CreateHandler")]
        public void WebServerController_ExecuteGetRootLevelApisJson()
        {
            //------------Setup for test--------------------------
            var controller = new TestWebServerController(HttpMethod.Get);
            //------------Bookmark Test---------------------------
            var result = controller.ExecuteGetRootLevelApisJson();
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(HttpResponseMessage));
        }
    }
}
