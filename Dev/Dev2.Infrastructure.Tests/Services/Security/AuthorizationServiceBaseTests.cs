using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    [TestClass]
    public class AuthorizationServiceBaseTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizationServiceBase_Constructor_SecurityServiceIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var authorizationService = new TestAuthorizationServiceBase(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_Constructor")]
        public void AuthorizationServiceBase_Constructor_PermissionsChangedEvent_WiredUp()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object);

            //------------Execute Test---------------------------
            securityService.Raise(m => m.PermissionsChanged += null, EventArgs.Empty);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, authorizationService.RaisePermissionsChangedHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_UserIsInResourceRoleAndResourceToBeVerifiedIsNull_False()
        {
            //------------Setup for test--------------------------
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceName = "Category\\Test1", ResourceID = Guid.NewGuid() };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.IsAuthorized(AuthorizationContext.Contribute, null);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_UserIsNotInRole_False()
        {
            //------------Setup for test--------------------------
            var securityPermissions = new List<WindowsGroupPermission>();

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(securityPermissions);

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(false);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, It.IsAny<string>());

                //------------Assert Results-------------------------
                Assert.IsFalse(authorized);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_UserIsInServerRoleAndHasPermissions_True()
        {
            //------------Setup for test--------------------------
            var securityPermission = new WindowsGroupPermission { IsServer = true };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                securityPermission.Permissions = AuthorizationHelpers.ToPermissions(context);

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, It.IsAny<string>());

                //------------Assert Results-------------------------
                Assert.AreEqual(context != AuthorizationContext.None, authorized);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_UserIsInServerRoleAndDoesNotHavePermissions_False()
        {
            //------------Setup for test--------------------------
            var securityPermission = new WindowsGroupPermission { IsServer = true };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                securityPermission.Permissions = ~AuthorizationHelpers.ToPermissions(context);

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, It.IsAny<string>());

                //------------Assert Results-------------------------
                Assert.IsFalse(authorized);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_UserIsInResourceRoleAndHasPermissions_True()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                securityPermission.Permissions = AuthorizationHelpers.ToPermissions(context);

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, resource.ToString());

                //------------Assert Results-------------------------
                Assert.AreEqual(context != AuthorizationContext.None, authorized);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_UserIsInResourceRoleAndDoesNotHavePermissions_False()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                securityPermission.Permissions = ~AuthorizationHelpers.ToPermissions(context);

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, resource.ToString());

                //------------Assert Results-------------------------
                Assert.IsFalse(authorized);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_UserIsInTwoRolesAndOneRoleDeniesAccess_True()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var allowPermission = new WindowsGroupPermission { WindowsGroup = "AllowGroup", IsServer = true };
            var denyPermission = new WindowsGroupPermission { WindowsGroup = "DenyGroup", IsServer = true };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { allowPermission, denyPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                allowPermission.Permissions = AuthorizationHelpers.ToPermissions(context);
                denyPermission.Permissions = ~AuthorizationHelpers.ToPermissions(context);

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, resource.ToString());

                //------------Assert Results-------------------------
                Assert.AreEqual(context != AuthorizationContext.None, authorized);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_UserHasPermissions_True()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_UserHasNoPermissions_False()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(false);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("AuthorizationServiceBase_Remove")]
        public void AuthorizationServiceBase_Remove_InvokesSecurityService()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(p => p.Remove(resourceID)).Verifiable();
            var authorizationService = new TestAuthorizationServiceBase(securityService.Object);

            //------------Execute Test---------------------------
            authorizationService.Remove(resourceID);

            //------------Assert Results-------------------------
            securityService.Verify(p => p.Remove(resourceID));
        }
    }
}
