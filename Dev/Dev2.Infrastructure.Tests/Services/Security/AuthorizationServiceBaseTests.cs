/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Services.Security;
using Dev2.Services.Security.MoqInstallerActions;
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

            new TestAuthorizationServiceBase(null);


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

            Console.WriteLine("BEFOREEntering AddAdministratorsGroupToWarewolf");
            try
            {
                warewolfGroupOps.AddAdministratorsGroupToWarewolf();
            }
            catch (COMException e)
            {
                //'The Server service is not started.' error is expected in containers. See: https://github.com/moby/moby/issues/26409#issuecomment-304978309
                if (e.Message != "The Server service is not started.\r\n")
                {
                    throw e;
                }
            }
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
        [Owner("Siphamandla Dube")]
        [TestCategory("AuthorizationServiceBase_AdministratorsMembersOfWarewolfGroup_WhenAdministratorsMembersOfTheGroup")]
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToLocalServer_AdministratorsMembersOfWarewolfGroup_WhenNonAdministratorsAreNotMembersOfTheGroup_ExpectFalse()
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

            var authorizationService = new TestAuthorizationServiceBase( securityService.Object, true, false, true) { User = user.Object };

            authorizationService.MemberOfAdminOverride = false;

            var isMemberTestAgain = authorizationService.AreAdministratorsMembersOfWarewolfAdministrators();

            //------------Assert Results Again-------------------------
            Assert.IsFalse(isMemberTestAgain);

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

            var authorizationService = new TestAuthorizationServiceBase(securityService.Object, true, false, true) { User = user.Object };
            authorizationService.MemberOfAdminOverride = true;

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
        class TestDirectoryEntry : IDirectoryEntry
        {
            private string _name;

            public TestDirectoryEntry(string name)
            {
                _name = name;
            }
            public IDirectoryEntries Children => new TestDirectoryEntries();

            public string SchemaClassName => throw new NotImplementedException();

            public string Name => _name;

            public DirectoryEntry Instance => throw new NotImplementedException();

            public void Dispose()
            {

            }

            public object Invoke(string methodName, params object[] args)
            {
                if (methodName == "Members" && Name == "Warewolf Administrators")
                {
                    return new List<string> { "Administrators".ToString() };
                }
                return null;
            }
        }
        class TestDirectoryEntries : IDirectoryEntries
        {

            public SchemaNameCollection SchemaFilter => new DirectoryEntry("LDAP://dev2.local", "IntegrationTester", "I73573r0").Children.SchemaFilter;

            public DirectoryEntries Instance => throw new NotImplementedException();

            public IEnumerator GetEnumerator()
            {
                yield return new TestDirectoryEntry("Test Group");
                yield return new TestDirectoryEntry("Warewolf Administrators");
                yield return new TestDirectoryEntry("no users");
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("AuthorizationServiceBase_AdministratorsMembersOfWarewolfGroup_WhenAdministratorsMembersOfTheGroup")]
        [Ignore]//TODO: re-introduce this test on the premier.local domain
        public void AuthorizationServiceBase_IsAuthorizedToConnect_ToLocalServer_AdministratorsMembersOfWarewolfGroup_WhenMemberOfAdministrator_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var getPassword = TestEnvironmentVariables.GetVar("dev2\\IntegrationTester");
            // permissions setup
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //Delete warewolf if already a member...
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();

            var result = warewolfGroupOps.IsAdminMemberOfWarewolf();

            Assert.IsFalse(result);

            // Setup rest of test
            var resource = Guid.NewGuid();
            var securityPermission = new WindowsGroupPermission { IsServer = false, ResourceID = resource, Permissions = Permissions.View, WindowsGroup = GlobalConstants.WarewolfGroup };
            var securityService = new Mock<ISecurityService>();
            var user = new Mock<IPrincipal>();
            var actualGChildren = new List<Mock<IDirectoryEntry>> { new Mock<IDirectoryEntry>() };
            var gChildren = new Mock<IDirectoryEntries>();
            var actualChildren = new List<Mock<IDirectoryEntry>> { new Mock<IDirectoryEntry>() };
            var children = new Mock<IDirectoryEntries>();
            var dir = new Mock<IDirectoryEntryFactory>();

            securityService.SetupGet(p => p.Permissions).Returns(new List<WindowsGroupPermission> { securityPermission });
            user.Setup(u => u.Identity.Name).Returns("TestUser");
            actualGChildren.ForEach(b => b.Setup(a => a.Name).Returns("Warewolf Administrators"));
            actualGChildren.ForEach(b => b.Setup(a => a.SchemaClassName).Returns("Computer"));

            gChildren.Setup(a => a.GetEnumerator()).Returns(actualGChildren.Select(a => a.Object).GetEnumerator());
            actualChildren.First().Setup(a => a.Children).Returns(gChildren.Object);
            children.Setup(a => a.GetEnumerator()).Returns(actualChildren.Select(a => a.Object).GetEnumerator());
            SchemaNameCollection filterList = new DirectoryEntry("LDAP://dev2.local", "IntegrationTester", getPassword).Children.SchemaFilter;
            children.Setup(a => a.SchemaFilter).Returns(filterList);
            var ss = "WinNT://" + Environment.MachineName + ",computer";
            dir.Setup(a => a.Create(ss)).Returns(new TestDirectoryEntry(ss));

            var authorizationService = new TestAuthorizationServiceBase(dir.Object, securityService.Object, true, true, false) { User = user.Object };
            authorizationService.MemberOfAdminOverride = true;
            //------------Execute Test---------------------------
            var isMember = authorizationService.AreAdministratorsMembersOfWarewolfAdministrators();

            //------------Assert Results-------------------------
            Assert.IsTrue(isMember);
        }
    }
}
