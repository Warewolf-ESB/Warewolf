using System;
using System.Collections.Generic;
using System.Threading;
using Dev2.Communication;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Hubs;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Security
{
    [TestClass]
    public class ServerAuthorizationServiceTests
    {
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_Instance")]
        public void ServerAuthorizationService_Instance_Singleton()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var instance1 = ServerAuthorizationService.Instance;
            var instance2 = ServerAuthorizationService.Instance;

            //------------Assert Results-------------------------
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_Constructor")]
        public void ServerAuthorizationService_Constructor_PermissionsChangedEvent_ClearsCachedRequests()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestServerAuthorizationService(securityService.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.Unknown);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            authorizationService.IsAuthorized(request.Object);
            Assert.AreEqual(1, authorizationService.CachedRequestCount);

            //------------Execute Test---------------------------
            securityService.Raise(m => m.PermissionsChanged += null, EventArgs.Empty);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, authorizationService.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerAuthorizationService_IsAuthorized_RequestIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();

            var authorizationService = new TestServerAuthorizationService(securityService.Object);

            //------------Execute Test---------------------------
            authorizationService.IsAuthorized(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_RequestWhenNotAllowedButResultsPendingAndHubConnect_AuthorizationCalculatedAndNotCachedIsTrue()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestServerAuthorizationService(securityService.Object);
            var reciept = new FutureReceipt { PartID = 0, RequestID = Guid.NewGuid(), User = "TestUser" };
            ResultsCache.Instance.AddResult(reciept, "<x>hello world</x>");

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.Identity.Name).Returns("TestUser");

            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(false);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.HubConnect);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            Assert.AreEqual(0, authorizationService.CachedRequestCount);

            //------------Execute Test---------------------------
            var result = authorizationService.IsAuthorized(request.Object);

            // Clear the cache out ;)
            ResultsCache.Instance.FetchResult(reciept);

            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.Exactly(2));
            Assert.AreEqual(0, authorizationService.CachedRequestCount);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_RequestWhenNotAllowedButResultsPendingAndPayloadFetch_AuthorizationCalculatedAndNotCachedIsTrue()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestServerAuthorizationService(securityService.Object);
            var reciept = new FutureReceipt { PartID = 0, RequestID = Guid.NewGuid(), User = "TestUser" };
            ResultsCache.Instance.AddResult(reciept, "<x>hello world</x>");

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.Identity.Name).Returns("TestUser");

            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(false);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.EsbFetchExecutePayloadFragment);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            Assert.AreEqual(0, authorizationService.CachedRequestCount);

            //------------Execute Test---------------------------
            var result = authorizationService.IsAuthorized(request.Object);

            // Clear the cache out ;)
            ResultsCache.Instance.FetchResult(reciept);

            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.Exactly(2));
            Assert.AreEqual(0, authorizationService.CachedRequestCount);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_RequestWhenNotAllowedNoResultsPendingAndHubConnect_AuthorizationCalculatedAndNotCachedIsFalse()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestServerAuthorizationService(securityService.Object);
            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.Identity.Name).Returns("TestUser");

            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(false);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.HubConnect);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            Assert.AreEqual(0, authorizationService.CachedRequestCount);

            //------------Execute Test---------------------------
            var result = authorizationService.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.Exactly(2));
            Assert.AreEqual(0, authorizationService.CachedRequestCount);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_RequestWhenNotAllowedButResultsPendingAndNotHubConnect_AuthorizationCalculatedAndNotCachedIsFalse()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestServerAuthorizationService(securityService.Object);
            var reciept = new FutureReceipt { PartID = 0, RequestID = Guid.NewGuid(), User = "TestUser" };
            ResultsCache.Instance.AddResult(reciept, "<x>hello world</x>");

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.Identity.Name).Returns("TestUser");

            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(false);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.EsbWrite);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            Assert.AreEqual(0, authorizationService.CachedRequestCount);

            //------------Execute Test---------------------------
            var result = authorizationService.IsAuthorized(request.Object);

            // Clear the cache out ;)
            ResultsCache.Instance.FetchResult(reciept);

            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.Exactly(2));
            Assert.AreEqual(1, authorizationService.CachedRequestCount);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_RequestIsFirstTime_AuthorizationCalculatedAndCached()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestServerAuthorizationService(securityService.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            Assert.AreEqual(0, authorizationService.CachedRequestCount);

            //------------Execute Test---------------------------
            authorizationService.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationService.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_RequestIsSecondTime_CachedAuthorizationUsed()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());
            securityService.SetupGet(p => p.TimeOutPeriod).Returns(new TimeSpan(0, 2, 0));

            var authorizationService = new TestServerAuthorizationService(securityService.Object);
            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            authorizationService.IsAuthorized(request.Object);
            securityService.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationService.CachedRequestCount);

            //------------Execute Test---------------------------
            authorizationService.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationService.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_TimedOutPeriodExpired_ShouldNotGetFromCache()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());
            securityService.SetupGet(p => p.TimeOutPeriod).Returns(new TimeSpan(0, 0, 1));
            var authorizationService = new TestServerAuthorizationService(securityService.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);
            authorizationService.IsAuthorized(request.Object);
            //-------------Assert Preconditions-------------------
            securityService.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationService.CachedRequestCount);
            //------------Execute Test---------------------------
            Thread.Sleep(1200);
            authorizationService.IsAuthorized(request.Object);
            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.AtLeast(2));
            Assert.AreEqual(1, authorizationService.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_WithinTimedOutPeriod_ShouldGetFromCache()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());
            securityService.SetupGet(p => p.TimeOutPeriod).Returns(new TimeSpan(0, 1, 0));
            var authorizationService = new TestServerAuthorizationService(securityService.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);
            authorizationService.IsAuthorized(request.Object);
            //-------------Assert Preconditions-------------------
            securityService.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationService.CachedRequestCount);
            //------------Execute Test---------------------------
            authorizationService.IsAuthorized(request.Object);
            //------------Assert Results-------------------------
            securityService.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationService.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_WebInvokeService_CorrectAuthorizations()
        {
            var resourceID = Guid.NewGuid();
            var workspaceID = Guid.NewGuid();

            var queryString = new Mock<INameValueCollection>();
            queryString.Setup(q => q["rid"]).Returns(resourceID.ToString());
            queryString.Setup(q => q["wid"]).Returns(workspaceID.ToString());

            const string UrlFormat = "http://localhost:1234/wwwroot/{0}?wid={1}&rid={2}";
            var requests = new[]
            {
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/dbservice", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Help/GetDictionary", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Resources/PathsAndNames", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Resources/Sources", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/DbSources/Search", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/DbSources/Get", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Services/Get?", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "sservices/Service/DbSources/Get", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(AuthorizationContext.Contribute, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Services/Save", workspaceID, resourceID), queryString.Object, resourceID.ToString())
            };

            Verify_IsAuthorized(requests);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_WebGetXXX_CorrectAuthorizations()
        {
            const string ResourceID = "ed44bc30-7bf0-4b64-a3a3-ad2c15e8eb23";
            var queryStringWithResource = new Mock<INameValueCollection>();
            queryStringWithResource.Setup(q => q["rid"]).Returns(ResourceID);

            var queryString = new Mock<INameValueCollection>();

            const string UrlFormat = "http://localhost:1234/wwwroot/{0}";
            var requests = new[]
            {
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.WebGet, string.Format(UrlFormat, "services/webservice?rid=" + ResourceID), queryStringWithResource.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.WebGetContent, string.Format(UrlFormat, "content/Site.css"), queryString.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.WebGetImage, string.Format(UrlFormat, "images/clear-filter.png"), queryString.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.WebGetScript, string.Format(UrlFormat, "scripts/fx/jquery-1.9.1.min.js"), queryString.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.WebGetView, string.Format(UrlFormat, "views/services/dbservice.htm"), queryString.Object, ResourceID),
                
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetDecisions, string.Format(UrlFormat, "decisions/wizard.htm"), queryStringWithResource.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetDialogs, string.Format(UrlFormat, "dialogs/savedialog.htm"), queryString.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetServices, string.Format(UrlFormat, "services/webservice.htm"), queryString.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetSources, string.Format(UrlFormat, "sources/pluginsource.htm"), queryString.Object, ResourceID),
                new TestAuthorizationRequest(AuthorizationContext.View, WebServerRequestType.WebGetSwitch, string.Format(UrlFormat, "switch/drag.htm"), queryString.Object, ResourceID)
            };

            Verify_IsAuthorized(requests);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_WebExecuteOrBookmarkWorkflow_CorrectAuthorizations()
        {
            var queryString = new Mock<INameValueCollection>();

            const string ResourceName = "Test 123";

            const string UrlFormat = "http://localhost:1234/services/{0}{1}";
            var requests = new[]
            {
                new TestAuthorizationRequest(AuthorizationContext.Execute, WebServerRequestType.WebBookmarkWorkflow, string.Format(UrlFormat, ResourceName, "/instances/id/bookmark/bmk"), queryString.Object, ResourceName),
                new TestAuthorizationRequest(AuthorizationContext.Execute, WebServerRequestType.WebExecuteWorkflow, string.Format(UrlFormat, ResourceName, ""), queryString.Object, ResourceName)
            };

            Verify_IsAuthorized(requests);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_HubConnect_CorrectAuthorizations()
        {
            var queryString = new Mock<INameValueCollection>();

            const string Url = "http://localhost:1234/dsf/";
            var requests = new[]
            {
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.HubConnect, Url, queryString.Object)
            };

            Verify_IsAuthorized(requests);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerAuthorizationService_IsAuthorized")]
        public void ServerAuthorizationService_IsAuthorized_EsbXXX_CorrectAuthorizations()
        {
            var queryString = new Mock<INameValueCollection>();

            const string Url = "http://localhost:1234/dsf/";
            var requests = new[]
            {
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.EsbSendMemo, Url, queryString.Object),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.EsbAddDebugWriter, Url, queryString.Object),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.EsbExecuteCommand, Url, queryString.Object),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.EsbSendDebugState, Url, queryString.Object),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.EsbWrite, Url, queryString.Object),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.EsbOnConnected, Url, queryString.Object),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.EsbFetchExecutePayloadFragment, Url, queryString.Object),
                new TestAuthorizationRequest(AuthorizationContext.Any, WebServerRequestType.ResourcesSendMemo, Url, queryString.Object)
            };

            Verify_IsAuthorized(requests);
        }

        void Verify_IsAuthorized(TestAuthorizationRequest[] requests)
        {
            var isServers = new[] { false, true };

            foreach(var isServer in isServers)
            {
                foreach(var request in requests)
                {
                    Verify_IsAuthorized(Permissions.None, request, isServer);
                    Verify_IsAuthorized(Permissions.View, request, isServer);
                    Verify_IsAuthorized(Permissions.Execute, request, isServer);
                    Verify_IsAuthorized(Permissions.Contribute, request, isServer);
                    Verify_IsAuthorized(Permissions.Administrator, request, isServer);

                    Verify_IsAuthorized(Permissions.View | Permissions.Execute, request, isServer);
                    Verify_IsAuthorized(Permissions.View | Permissions.Contribute, request, isServer);
                    Verify_IsAuthorized(Permissions.Execute | Permissions.Contribute, request, isServer);
                    Verify_IsAuthorized(Permissions.View | Permissions.Execute | Permissions.Contribute, request, isServer);
                }
            }
        }

        void Verify_IsAuthorized(Permissions configPermissions, TestAuthorizationRequest authorizationRequest, bool isServer)
        {
            var configPermission = new WindowsGroupPermission { WindowsGroup = TestAuthorizationRequest.UserRole, IsServer = isServer, Permissions = configPermissions };

            if(!isServer && !string.IsNullOrEmpty(authorizationRequest.Resource))
            {
                Guid resourceID;
                if(Guid.TryParse(authorizationRequest.Resource, out resourceID))
                {
                    configPermission.ResourceID = resourceID;
                    configPermission.ResourceName = "TestCategory\\";
                }
                else
                {
                    configPermission.ResourceID = Guid.NewGuid();
                    configPermission.ResourceName = string.Format("TestCategory\\{0}", authorizationRequest.Resource);
                }
            }

            authorizationRequest.UserIsInRole = false;
            Verify_IsAuthorized(configPermission, authorizationRequest);

            authorizationRequest.UserIsInRole = true;
            Verify_IsAuthorized(configPermission, authorizationRequest);
        }

        void Verify_IsAuthorized(WindowsGroupPermission configPermissions, TestAuthorizationRequest authorizationRequest)
        {
            //------------Setup for test--------------------------           
            var allowedPermissions = AuthorizationHelpers.ToPermissions(authorizationRequest.AuthorizationContext);
            var expected = authorizationRequest.UserIsInRole && (configPermissions.Permissions & allowedPermissions) != 0;

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new[] { configPermissions });
            var authorizationService = new TestServerAuthorizationService(securityService.Object);

            //------------Execute Test---------------------------
            var authorized = authorizationService.IsAuthorized(authorizationRequest);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, authorized, string.Format("\nUserIsInRole: {0}\nAllowed: {1}\nConfig: {2}\nIsServer: {3}\nURL: {4}",
                authorizationRequest.UserIsInRole, allowedPermissions, configPermissions.Permissions, configPermissions.IsServer, authorizationRequest.Url));
        }

        // ReSharper restore InconsistentNaming
    }
}
