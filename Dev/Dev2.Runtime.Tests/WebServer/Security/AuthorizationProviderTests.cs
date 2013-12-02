using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Data.Settings.Security;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Security;
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
        public void AuthorizationProvider_IsAuthorized_QueryStringHasResourceIDAndUserIsInRoleForResource_True()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            const string PermissionGroup = "Admins";
            const string UserRole = "Admins";
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { WindowsGroup = PermissionGroup, ResourceID = resourceID, Contribute = true, IsServer = false }
            };

            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(permissions);
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(UserRole)).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(resourceID.ToString());

            //------------Execute Test---------------------------
            var authorized = authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_QueryStringHasResourceIDAndUserIsNotInRoleForResource_False()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            const string PermissionGroup = "Admins";
            const string UserRole = "Admins1";
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { WindowsGroup = PermissionGroup, ResourceID = resourceID, Contribute = true, IsServer = false }
            };

            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(permissions);
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(UserRole)).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", "Url"));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebGet);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns(resourceID.ToString());

            //------------Execute Test---------------------------
            var authorized = authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_ExecuteWorkflowAndUserIsInRoleForResource_True()
        {
            //------------Setup for test--------------------------
            const string ResourceName = "Test 123";
            const string Url = "http://localhost/services/" + ResourceName;

            const string PermissionGroup = "Admins";
            const string UserRole = "Admins";
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { WindowsGroup = PermissionGroup, ResourceName = "Category\\" + ResourceName, Contribute = true, IsServer = false }
            };

            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(permissions);
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(UserRole)).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", Url));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebExecuteWorkflow);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns((string)null);
            request.Setup(r => r.Url).Returns(new Uri(Url));

            //------------Execute Test---------------------------
            var authorized = authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_ExecuteWorkflowAndUserIsNotInRoleForResource_False()
        {
            //------------Setup for test--------------------------
            const string ResourceName = "Test 123";
            const string Url = "http://localhost/services/" + ResourceName;

            const string PermissionGroup = "Admins";
            const string UserRole = "Admins1";
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { WindowsGroup = PermissionGroup, ResourceName = "Category\\" + ResourceName, Contribute = true, IsServer = false }
            };

            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(permissions);
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(UserRole)).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", Url));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebExecuteWorkflow);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns((string)null);
            request.Setup(r => r.Url).Returns(new Uri(Url));

            //------------Execute Test---------------------------
            var authorized = authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_BookmarkWorkflowAndUserIsInRoleForResource_True()
        {
            //------------Setup for test--------------------------
            const string ResourceName = "Test 123";
            const string Url = "http://localhost/services/" + ResourceName + "/instances/id/bookmark/bmk";

            const string PermissionGroup = "Admins";
            const string UserRole = "Admins";
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { WindowsGroup = PermissionGroup, ResourceName = "Category\\" + ResourceName, Contribute = true, IsServer = false }
            };

            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(permissions);
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(UserRole)).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", Url));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebBookmarkWorkflow);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns((string)null);
            request.Setup(r => r.Url).Returns(new Uri(Url));

            //------------Execute Test---------------------------
            var authorized = authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationProvider_IsAuthorized")]
        public void AuthorizationProvider_IsAuthorized_BookmarkWorkflowAndUserIsNotInRoleForResource_False()
        {
            //------------Setup for test--------------------------
            const string ResourceName = "Test 123";
            const string Url = "http://localhost/services/" + ResourceName + "/instances/id/bookmark/bmk";

            const string PermissionGroup = "Admins";
            const string UserRole = "Admins1";
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { WindowsGroup = PermissionGroup, ResourceName = "Category\\" + ResourceName, Contribute = true, IsServer = false }
            };

            var securityConfigProvider = new Mock<ISecurityConfigProvider>();
            securityConfigProvider.SetupGet(p => p.Permissions).Returns(permissions);
            var authorizationProvider = new TestAuthorizationProvider(securityConfigProvider.Object);

            var request = new Mock<IAuthorizationRequest>();
            request.Setup(r => r.User.IsInRole(UserRole)).Returns(true);
            request.Setup(r => r.Key).Returns(new Tuple<string, string>("User", Url));
            request.Setup(r => r.RequestType).Returns(WebServerRequestType.WebBookmarkWorkflow);
            request.Setup(r => r.QueryString[It.IsAny<string>()]).Returns((string)null);
            request.Setup(r => r.Url).Returns(new Uri(Url));

            //------------Execute Test---------------------------
            var authorized = authorizationProvider.IsAuthorized(request.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }
    }
}
