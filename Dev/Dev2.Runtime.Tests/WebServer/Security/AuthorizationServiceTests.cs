//using System;
//using System.Collections.Generic;
//using Dev2.Runtime.WebServer;
//using Dev2.Runtime.WebServer.Security;
//using Dev2.Services.Security;
//using Microsoft.AspNet.SignalR.Hosting;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;

//namespace Dev2.Tests.Runtime.WebServer.Security
//{
//    [TestClass]
//    [Ignore] // TODO: Unignore when IsAuthorized does not always return true
//    public class AuthorizationServiceTests
//    {
//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_Instance")]
//        public void AuthorizationService_Instance_Singleton()
//        {
//            //------------Setup for test--------------------------

//            //------------Execute Test---------------------------
//            var instance1 = AuthorizationService.Instance;
//            var instance2 = AuthorizationService.Instance;

//            //------------Assert Results-------------------------
//            Assert.AreSame(instance1, instance2);
//        }

//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_Constructor")]
//        public void AuthorizationService_Constructor_PermissionsChangedEvent_ClearsCachedRequests()
//        {
//            //------------Setup for test--------------------------
//            var securityService = new Mock<ISecurityService>();
//            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

//            var authorizationService = new TestAuthorizationService(securityService.Object);

//            var request = new Mock<IAuthorizationRequest>();
//            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
//            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
//            request.Setup(r => r.RequestType).Returns(WebServerRequestType.Unknown);
//            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

//            authorizationService.IsAuthorized(request.Object);
//            Assert.AreEqual(1, authorizationService.CachedRequestCount);

//            //------------Execute Test---------------------------
//            securityService.Raise(m => m.PermissionsChanged += null, EventArgs.Empty);

//            //------------Assert Results-------------------------
//            Assert.AreEqual(0, authorizationService.CachedRequestCount);
//        }

//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_IsAuthorized")]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void AuthorizationService_IsAuthorized_RequestIsNull_ThrowsArgumentNullException()
//        {
//            //------------Setup for test--------------------------
//            var securityService = new Mock<ISecurityService>();

//            var authorizationService = new TestAuthorizationService(securityService.Object);

//            //------------Execute Test---------------------------
//            authorizationService.IsAuthorized(null);

//            //------------Assert Results-------------------------
//        }

//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_IsAuthorized")]
//        public void AuthorizationService_IsAuthorized_RequestIsFirstTime_AuthorizationCalculatedAndCached()
//        {
//            //------------Setup for test--------------------------
//            var securityService = new Mock<ISecurityService>();
//            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

//            var authorizationService = new TestAuthorizationService(securityService.Object);

//            var request = new Mock<IAuthorizationRequest>();
//            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
//            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
//            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
//            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

//            Assert.AreEqual(0, authorizationService.CachedRequestCount);

//            //------------Execute Test---------------------------
//            authorizationService.IsAuthorized(request.Object);

//            //------------Assert Results-------------------------
//            securityService.VerifyGet(p => p.Permissions, Times.Once());
//            Assert.AreEqual(1, authorizationService.CachedRequestCount);
//        }

//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_IsAuthorized")]
//        public void AuthorizationService_IsAuthorized_RequestIsSecondTime_CachedAuthorizationUsed()
//        {
//            //------------Setup for test--------------------------
//            var securityService = new Mock<ISecurityService>();
//            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());
//            var authorizationService = new TestAuthorizationService(securityService.Object);

//            var request = new Mock<IAuthorizationRequest>();
//            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
//            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
//            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
//            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

//            authorizationService.IsAuthorized(request.Object);
//            securityService.VerifyGet(p => p.Permissions, Times.Once());
//            Assert.AreEqual(1, authorizationService.CachedRequestCount);

//            //------------Execute Test---------------------------
//            authorizationService.IsAuthorized(request.Object);

//            //------------Assert Results-------------------------
//            securityService.VerifyGet(p => p.Permissions, Times.Once());
//            Assert.AreEqual(1, authorizationService.CachedRequestCount);
//        }

//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_IsAuthorized")]
//        public void AuthorizationService_IsAuthorized_WebInvokeService_CorrectAuthorizations()
//        {
//            var resourceID = Guid.NewGuid();
//            var workspaceID = Guid.NewGuid();

//            var queryString = new Mock<INameValueCollection>();
//            queryString.Setup(q => q["rid"]).Returns(resourceID.ToString());
//            queryString.Setup(q => q["wid"]).Returns(workspaceID.ToString());

//            const string UrlFormat = "http://localhost:1234/wwwroot/{0}?wid={1}&rid={2}";
//            var requests = new[]
//            {
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/dbservice", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Help/GetDictionary", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Resources/PathsAndNames", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Resources/Sources", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/DbSources/Search", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/DbSources/Get", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Services/Get?", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "sservices/Service/DbSources/Get", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//                new TestAuthorizationRequest(AuthorizationContext.Contribute, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Services/Save", workspaceID, resourceID), queryString.Object, resource: resourceID.ToString()),
//            };

//            Verify_IsAuthorized(requests);
//        }

//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_IsAuthorized")]
//        public void AuthorizationService_IsAuthorized_WebGetXXX_CorrectAuthorizations()
//        {
//            var queryString = new Mock<INameValueCollection>();

//            const string UrlFormat = "http://localhost:1234/wwwroot/{0}";
//            var requests = new[]
//            {
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGet, string.Format(UrlFormat, "services/Scripts/warewolf-ko.js"), queryString.Object),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetContent, string.Format(UrlFormat, "content/Site.css"), queryString.Object),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetImage, string.Format(UrlFormat, "images/clear-filter.png"), queryString.Object),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetScript, string.Format(UrlFormat, "scripts/fx/jquery-1.9.1.min.js"), queryString.Object),
//                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetView, string.Format(UrlFormat, "views/services/dbservice.htm"), queryString.Object),
//            };

//            Verify_IsAuthorized(requests);
//        }

//        [TestMethod]
//        [Owner("Trevor Williams-Ros")]
//        [TestCategory("AuthorizationService_IsAuthorized")]
//        public void AuthorizationService_IsAuthorized_WebExecuteOrBookmarkWorkflow_CorrectAuthorizations()
//        {
//            var queryString = new Mock<INameValueCollection>();

//            const string ResourceName = "Test 123";

//            const string UrlFormat = "http://localhost:1234/services/{0}{1}";
//            var requests = new[]
//            {
//                new TestAuthorizationRequest(AuthorizationContext.Execute, WebServerRequestType.WebBookmarkWorkflow, string.Format(UrlFormat, ResourceName, "/instances/id/bookmark/bmk"), queryString.Object, resource: ResourceName),
//                new TestAuthorizationRequest(AuthorizationContext.Execute, WebServerRequestType.WebExecuteWorkflow, string.Format(UrlFormat, ResourceName, ""), queryString.Object, resource: ResourceName),
//            };

//            Verify_IsAuthorized(requests);
//        }

//        void Verify_IsAuthorized(TestAuthorizationRequest[] requests)
//        {
//            var isServers = new[] { false, true };

//            foreach(var isServer in isServers)
//            {
//                foreach(var request in requests)
//                {
//                    Verify_IsAuthorized(Permissions.None, request, isServer);
//                    Verify_IsAuthorized(Permissions.View, request, isServer);
//                    Verify_IsAuthorized(Permissions.Execute, request, isServer);
//                    Verify_IsAuthorized(Permissions.Contribute, request, isServer);
//                    Verify_IsAuthorized(Permissions.Administrator, request, isServer);

//                    Verify_IsAuthorized(Permissions.View | Permissions.Execute, request, isServer);
//                    Verify_IsAuthorized(Permissions.View | Permissions.Contribute, request, isServer);
//                    Verify_IsAuthorized(Permissions.Execute | Permissions.Contribute, request, isServer);
//                    Verify_IsAuthorized(Permissions.View | Permissions.Execute | Permissions.Contribute, request, isServer);
//                }
//            }
//        }

//        void Verify_IsAuthorized(Permissions configPermissions, TestAuthorizationRequest authorizationRequest, bool isServer)
//        {
//            var configPermission = new WindowsGroupPermission { WindowsGroup = TestAuthorizationRequest.UserRole, IsServer = isServer, Permissions = configPermissions };

//            if(!isServer && !string.IsNullOrEmpty(authorizationRequest.Resource))
//            {
//                Guid resourceID;
//                if(Guid.TryParse(authorizationRequest.Resource, out resourceID))
//                {
//                    configPermission.ResourceID = resourceID;
//                }
//                else
//                {
//                    configPermission.ResourceName = string.Format("TestCategory\\{0}", authorizationRequest.Resource);
//                }
//            }

//            authorizationRequest.UserIsInRole = false;
//            Verify_IsAuthorized(configPermission, authorizationRequest);

//            authorizationRequest.UserIsInRole = true;
//            Verify_IsAuthorized(configPermission, authorizationRequest);
//        }

//        void Verify_IsAuthorized(WindowsGroupPermission configPermissions, TestAuthorizationRequest authorizationRequest)
//        {
//            //------------Setup for test--------------------------           

//            var securityService = new Mock<ISecurityService>();
//            securityService.SetupGet(p => p.Permissions).Returns(new[] { configPermissions });
//            var authorizationService = new TestAuthorizationService(securityService.Object);

//            //------------Execute Test---------------------------
//            var authorized = authorizationService.IsAuthorized(authorizationRequest);

//            //------------Assert Results-------------------------
//            var allowedPermissions = AuthorizationHelpers.ToPermissions(authorizationRequest.AuthorizationContext);
//            var expected = authorizationRequest.UserIsInRole && (configPermissions.Permissions & allowedPermissions) != 0;

//            Assert.AreEqual(expected, authorized, string.Format("\nUserIsInRole: {0}\nAllowed: {1}\nConfig: {2}\nIsServer: {3}\nURL: {4}",
//                authorizationRequest.UserIsInRole, allowedPermissions, configPermissions.Permissions, configPermissions.IsServer, authorizationRequest.Url));
//        }
//    }
//}
