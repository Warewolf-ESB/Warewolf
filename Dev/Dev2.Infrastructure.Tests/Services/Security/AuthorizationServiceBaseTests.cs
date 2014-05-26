using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Services.Security;
using Dev2.Services.Security.MoqInstallerActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
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
            // ReSharper disable ObjectCreationAsStatement
            new TestAuthorizationServiceBase(null);
            // ReSharper restore ObjectCreationAsStatement

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
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AuthorizationServiceBase_Constructor")]
        public void AuthorizationServiceBase_Constructor_PermissionsModifiedEventSubscribedTwice_OnlyOneEventIsWiredUp()
        {
            //------------Setup for test--------------------------
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission>());

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object);
            authorizationService.PermissionsModified += (sender, args) => { };
            authorizationService.PermissionsModified += (sender, args) => { };
            //------------Execute Test---------------------------
            securityService.Raise(m => m.PermissionsModified += null, EventArgs.Empty, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, authorizationService.RaisePermissionsModifiedHitCount);
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
                securityPermission.Permissions = context.ToPermissions();

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
                securityPermission.Permissions = ~context.ToPermissions();

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
                securityPermission.Permissions = context.ToPermissions();

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
                securityPermission.Permissions = ~context.ToPermissions();

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, resource.ToString());

                //------------Assert Results-------------------------
                Assert.IsFalse(authorized);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_HasDefaultGuestPermissions_False()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = WindowsGroupPermission.CreateGuests();

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                securityPermission.Permissions = ~context.ToPermissions();

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, resource.ToString());

                //------------Assert Results-------------------------
                Assert.IsFalse(authorized);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_HasDefaultGuestPermissions_WithGivenPermission_True()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = WindowsGroupPermission.CreateGuests();

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(false);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            foreach(AuthorizationContext context in Enum.GetValues(typeof(AuthorizationContext)))
            {
                securityPermission.Permissions = context.ToPermissions();

                //------------Execute Test---------------------------
                var authorized = authorizationService.IsAuthorized(context, resource.ToString());

                //------------Assert Results-------------------------
                Assert.AreEqual(context != AuthorizationContext.None, authorized);
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
                allowPermission.Permissions = context.ToPermissions();
                denyPermission.Permissions = ~context.ToPermissions();

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
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToRemoteServer_WithOnlyBuiltInAdminGroup_UserNotAuthorized()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View, WindowsGroup = GlobalConstants.WarewolfGroup };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object, false) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToLocalServer_WithOnlyBuiltInAdminGroup_UserIsAuthorized()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View, WindowsGroup = GlobalConstants.WarewolfGroup };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
            user.Setup(u => u.Identity.Name).Returns("TestUser");

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AuthorizationServiceBase_AdministratorsMembersOfWarewolfGroup_WhenAdministratorsMembersOfTheGroup")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToLocalServer_AdministratorsMembersOfWarewolfGroup_WhenAdministratorsMembersOfTheGroup_ExpectTrue()
        {
            //------------Setup for test--------------------------

            // permissions setup
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Delete warewolf if already a member...
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();

            warewolfGroupOps.AddAdministratorsGroupToWarewolf();
            var result = warewolfGroupOps.IsAdminMemberOfWarewolf();

            Assert.IsTrue(result);

            // Setup rest of test ;)
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View, WindowsGroup = GlobalConstants.WarewolfGroup };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.Name).Returns("TestUser");

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var isMember = authorizationService.AreAdministratorsMembersOfWarewolfAdministrators();

            //------------Assert Results-------------------------
            Assert.IsTrue(isMember);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AuthorizationServiceBase_AdministratorsMembersOfWarewolfGroup_WhenAdministratorsMembersOfTheGroup")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToLocalServer_AdministratorsMembersOfWarewolfGroup_WhenAdministratorsAreNotMembersOfTheGroup_ExpectFalse()
        {
            //------------Setup for test--------------------------

            // permissions setup
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Delete warewolf if already a member...
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();

            var result = warewolfGroupOps.IsAdminMemberOfWarewolf();

            Assert.IsFalse(result);

            // Setup rest of test ;)
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View, WindowsGroup = GlobalConstants.WarewolfGroup };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.Identity.Name).Returns("TestUser");

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object, true, true, true) { User = user.Object };

            //------------Execute Test---------------------------
            var isMember = authorizationService.AreAdministratorsMembersOfWarewolfAdministrators();

            //------------Assert Results-------------------------
            Assert.IsFalse(isMember);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect_WhenNotDirectlyInWarewolfGroup")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToLocalServer_WithBuiltInAdminstratorsOnlyWarewolfAdministratorsGroupMember_UserIsAuthorized()
        {
            //------------Setup for test--------------------------
            // Setup rest of test ;)
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View, WindowsGroup = GlobalConstants.WarewolfGroup };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns<string>(role =>
            {
                if(role == "Warewolf Administrators")
                {
                    return false;
                }

                return true;
            });

            user.Setup(u => u.Identity.Name).Returns("TestUser");

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToLocalServerWithNullIdentityName_WithOnlyBuiltInAdminGroup_UserIsNotAuthorized()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View, WindowsGroup = GlobalConstants.WarewolfGroup };

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
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
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_HasDefaultGuestPermissions_False()
        {
            var securityPermission = WindowsGroupPermission.CreateGuests();

            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(authorized);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizationServiceBase_IsAuthorizedToConnect")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_HasPermissionGivenInDefaultGuest_True()
        {
            var securityPermission = WindowsGroupPermission.CreateGuests();
            securityPermission.View = true;
            var securityService = new Mock<ISecurityService>();
            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });

            var user = new Mock<IPrincipal>();
            user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(false);

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = user.Object };

            //------------Execute Test---------------------------
            var authorized = authorizationService.TestIsAuthorizedToConnect(user.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(authorized);
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
