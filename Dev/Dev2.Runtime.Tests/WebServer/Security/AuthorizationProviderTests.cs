using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Data.Settings.Security;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    public class AuthorizationProviderTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_Instance")]
        public void AuthorizationProvider_Instance_Singleton()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var instance1 = AuthorizationProvider.Instance;
            var instance2 = AuthorizationProvider.Instance;

            //------------Assert Results-------------------------
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizationProvider_Constructor_SecurityConfigProviderIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var authorizationProvider = new TestAuthorizationProvider(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_Constructor")]
        public void AuthorizationProvider_Constructor_SecurityConfigProviderChangedEvent_ClearsCachedRequests()
        {
            //------------Setup for test--------------------------
            var securityConfigProvider = new Mock<ISecurityConfigProvider>();

            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.Unknown);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            authorizationProvider.IsAuthorized(request.Object);
            Assert.AreEqual(1, authorizationProvider.CachedRequestCount);

            //------------Execute Test---------------------------
            securityConfigProvider.Raise(m => m.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.All, "temp", null));

            //------------Assert Results-------------------------
            Assert.AreEqual(0, authorizationProvider.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizationProvider_IsAuthorized_RequestIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var securityConfigProvider = new Mock<ISecurityConfigProvider>();

            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            //------------Execute Test---------------------------
            authorizationProvider.IsAuthorized(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_RequestIsFirstTime_AuthorizationCalculatedAndCached()
        {
            //------------Setup for test--------------------------
            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            Assert.AreEqual(0, authorizationProvider.CachedRequestCount);

            //------------Execute Test---------------------------
            authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            securityConfigProvider.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationProvider.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_RequestIsSecondTime_CachedAuthorizationUsed()
        {
            //------------Setup for test--------------------------
            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(It.IsAny<string>())).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(string.Empty);

            authorizationProvider.IsAuthorized(request.Object);
            securityConfigProvider.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationProvider.CachedRequestCount);

            //------------Execute Test---------------------------
            authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            securityConfigProvider.VerifyGet(p => p.Permissions, Times.Once());
            Assert.AreEqual(1, authorizationProvider.CachedRequestCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_WebInvokeService_CorrectAuthorizations()
        {
            var resourceID = Guid.NewGuid();
            var workspaceID = Guid.NewGuid();

            var queryString = new Mock<INameValueCollection>();
            queryString.Setup(q => q["rid"]).Returns(resourceID.ToString());
            queryString.Setup(q => q["wid"]).Returns(workspaceID.ToString());

            const string UrlFormat = "http://localhost:1234/wwwroot/{0}?wid={1}&rid={2}";
            var requests = new[]
            {
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/dbservice", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Help/GetDictionary", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Resources/PathsAndNames", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Resources/Sources", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/DbSources/Search", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/DbSources/Get", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Services/Get?", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "sservices/Service/DbSources/Get", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute, WebServerRequestType.WebInvokeService, string.Format(UrlFormat, "services/Service/Services/Save", workspaceID, resourceID), queryString.Object, resourceID.ToString()),
            };

            Verify_IsAuthorized(requests);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_WebGetXXX_CorrectAuthorizations()
        {
            var queryString = new Mock<INameValueCollection>();

            const string UrlFormat = "http://localhost:1234/wwwroot/{0}";
            var requests = new[]
            {
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebGet, string.Format(UrlFormat, "services/Scripts/warewolf-ko.js"), queryString.Object),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebGetContent, string.Format(UrlFormat, "content/Site.css"), queryString.Object),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebGetImage, string.Format(UrlFormat, "images/clear-filter.png"), queryString.Object),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebGetScript, string.Format(UrlFormat, "scripts/fx/jquery-1.9.1.min.js"), queryString.Object),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.View, WebServerRequestType.WebGetView, string.Format(UrlFormat, "views/services/dbservice.htm"), queryString.Object),
            };

            Verify_IsAuthorized(requests);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_WebExecuteOrBookmarkWorkflow_CorrectAuthorizations()
        {
            var queryString = new Mock<INameValueCollection>();

            const string ResourceName = "Test 123";

            const string UrlFormat = "http://localhost:1234/services/{0}{1}";
            var requests = new[]
            {
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.Execute, WebServerRequestType.WebBookmarkWorkflow, string.Format(UrlFormat, ResourceName, "/instances/id/bookmark/bmk"), queryString.Object, ResourceName),
                new TestAuthorizationRequest(Permissions.Administrator | Permissions.Contribute | Permissions.Execute, WebServerRequestType.WebExecuteWorkflow, string.Format(UrlFormat, ResourceName, ""), queryString.Object, ResourceName),
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
                }
                else
                {
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

            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(new[] { configPermissions });
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            //------------Execute Test---------------------------
            var authorized = authorizationProvider.IsAuthorized(authorizationRequest);

            //------------Assert Results-------------------------
            var expected = authorizationRequest.UserIsInRole &&
                (configPermissions.Permissions & authorizationRequest.AllowedPermissions) != 0;

            Assert.AreEqual(expected, authorized, string.Format("\nUserIsInRole: {0}\nAllowed: {1}\nConfig: {2}\nIsServer: {3}\nURL: {4}",
                authorizationRequest.UserIsInRole, authorizationRequest.AllowedPermissions, configPermissions.Permissions, configPermissions.IsServer, authorizationRequest.Url));
        }
    }
}
