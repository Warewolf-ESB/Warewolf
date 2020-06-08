/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Cryptography;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Security;
using Warewolf.Services;


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
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_HasDefaultGuestPermissions_WithoutGivenPermission_AndTokenExists_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    IsServer = true,
                    WindowsGroup = "MySecretGroup",
                    View = true,
                    Execute = true,
                    Contribute = true,
                    DeployTo = true,
                    DeployFrom = true,
                    Administrator = true,
                    ResourceName = "MyTokenAccessibleResource",
                    ResourceID = resource,
                }
            };

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(o => o.Permissions).Returns(permissions);
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO(permissions)
            {
                SecretKey = secretKey,
            });
            var jwtManager = new JwtManager(mockSecuritySettings.Object);
            var tokenData = jwtManager.GenerateToken("{\"UserGroups\":[{\"Name\":\"MySecretGroup\"}]}");
            var principal = jwtManager.BuildPrincipal(tokenData);
            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = principal };


            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.Administrator, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.View, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.Contribute, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resource.ToString()));
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(AuthorizationServiceBase))]
        public void AuthorizationServiceBase_IsAuthorized_GivenResourceAndGroupPermissions_ExpectOverrideSystemDefault_ExecuteViewSuccess()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var permissionBuilder = new PermissionTableBuilder();
            permissionBuilder.AddServerPermission("Warewolf Administrators", AuthorizationContext.View | AuthorizationContext.Execute | AuthorizationContext.Administrator | AuthorizationContext.Contribute | AuthorizationContext.DeployFrom | AuthorizationContext.DeployTo);
            permissionBuilder.AddServerPermission("Public", AuthorizationContext.None);
            permissionBuilder.AddResourcePermission(resource, "MyTokenAccessibleResource", "Public", AuthorizationContext.View | AuthorizationContext.Execute);
            var permissions = permissionBuilder.Permissions;

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(o => o.Permissions).Returns(() => permissions);
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO(permissions)
            {
                SecretKey = secretKey,
            });
            var jwtManager = new JwtManager(mockSecuritySettings.Object);
            var tokenData = jwtManager.GenerateToken("{\"UserGroups\":[{\"Name\":\"MySecretGroup\"}]}");
            var principal = jwtManager.BuildPrincipal(tokenData);
            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = principal };


            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Administrator, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.View, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Contribute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resource.ToString()));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(AuthorizationServiceBase))]
        public void AuthorizationServiceBase_IsAuthorized_GivenResourceAndGroupPermissions_ExpectOverrideSystemDefault_ExecuteViewReject()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var permissionBuilder = new PermissionTableBuilder();
            permissionBuilder.AddServerPermission("Warewolf Administrators", AuthorizationContext.View | AuthorizationContext.Execute | AuthorizationContext.Administrator | AuthorizationContext.Contribute | AuthorizationContext.DeployFrom | AuthorizationContext.DeployTo);
            permissionBuilder.AddServerPermission("Public", AuthorizationContext.View | AuthorizationContext.Execute);
            permissionBuilder.AddResourcePermission(resource, "MyTokenAccessibleResource", "Public", AuthorizationContext.None);
            var permissions = permissionBuilder.Permissions;

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(o => o.Permissions).Returns(() => permissions);
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO(permissions)
            {
                SecretKey = secretKey,
            });
            var jwtManager = new JwtManager(mockSecuritySettings.Object);
            var tokenData = jwtManager.GenerateToken("{\"UserGroups\":[{\"Name\":\"MySecretGroup\"}]}");
            var principal = jwtManager.BuildPrincipal(tokenData);
            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = principal };


            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Administrator, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.View, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Contribute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resource.ToString()));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(AuthorizationServiceBase))]
        public void AuthorizationServiceBase_IsAuthorized_GivenResourceAndGroupPermissions_ExpectOverrideSystemDefaultDoesNotAffectOtherWorkflow()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var permissionBuilder = new PermissionTableBuilder();
            permissionBuilder.AddServerPermission("Warewolf Administrators", AuthorizationContext.View | AuthorizationContext.Execute | AuthorizationContext.Administrator | AuthorizationContext.Contribute | AuthorizationContext.DeployFrom | AuthorizationContext.DeployTo);
            permissionBuilder.AddServerPermission("Public", AuthorizationContext.View | AuthorizationContext.Execute);
            permissionBuilder.AddResourcePermission(resource, "SomeOtherResource", "Public", AuthorizationContext.None);
            var permissions = permissionBuilder.Permissions;

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(o => o.Permissions).Returns(() => permissions);
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO(permissions)
            {
                SecretKey = secretKey,
            });
            var jwtManager = new JwtManager(mockSecuritySettings.Object);
            var tokenData = jwtManager.GenerateToken("{\"UserGroups\":[{\"Name\":\"MySecretGroup\"}]}");
            var principal = jwtManager.BuildPrincipal(tokenData);
            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = principal };


            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Administrator, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ToString()));
            Assert.IsTrue(authorizationService.IsAuthorized(AuthorizationContext.View, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Contribute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resource.ToString()));
        }

        internal class PermissionTableBuilder
        {
            public List<WindowsGroupPermission> Permissions = new List<WindowsGroupPermission>();

            public void AddResourcePermission(Guid resourceId, string resourceName, string groupName, AuthorizationContext authorizationContext)
            {
                Permissions.Add(new WindowsGroupPermission
                {
                    ResourceID = resourceId,
                    ResourceName = resourceName,
                    WindowsGroup = groupName,
                    View = (authorizationContext & AuthorizationContext.View) != 0,
                    Execute = (authorizationContext & AuthorizationContext.Execute) != 0,
                    Contribute = (authorizationContext & AuthorizationContext.Contribute) != 0,
                });
            }

            public void AddServerPermission(string groupName, AuthorizationContext authorizationContext)
            {
                Permissions.Add(new WindowsGroupPermission
                {
                    WindowsGroup = groupName,
                    View = (authorizationContext & AuthorizationContext.View) != 0,
                    Execute = (authorizationContext & AuthorizationContext.Execute) != 0,
                    Contribute = (authorizationContext & AuthorizationContext.Contribute) != 0,
                });
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_HasDefaultGuestPermissions_WithoutGivenPermission_AndTokenExists_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    IsServer = true,
                    WindowsGroup = "MySecretGroup",
                    View = false,
                    Execute = false,
                    Contribute = false,
                    DeployTo = false,
                    DeployFrom = false,
                    Administrator = false,
                    ResourceName = "MyTokenAccessibleResource",
                    ResourceID = resource,
                }
            };

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(o => o.Permissions).Returns(permissions);
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO(permissions)
            {
                SecretKey = secretKey,
            });
            var jwtManager = new JwtManager(mockSecuritySettings.Object);
            var tokenData = jwtManager.GenerateToken("{\"UserGroups\":[{\"Name\":\"MySecretGroup\"}]}");
            var principal = jwtManager.BuildPrincipal(tokenData);
            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = principal };


            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Administrator, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.View, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Contribute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resource.ToString()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AuthorizationServiceBase_IsAuthorized")]
        public void AuthorizationServiceBase_IsAuthorized_HasDefaultGuestPermissions_WithoutGivenPermission_AndTokenExistsForOtherGroup_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var resource = Guid.NewGuid();
            var permissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    IsServer = true,
                    WindowsGroup = "MySecretGroup",
                    View = true,
                    Execute = true,
                    Contribute = true,
                    DeployTo = true,
                    DeployFrom = true,
                    Administrator = true,
                    ResourceName = "MyTokenAccessibleResource",
                    ResourceID = resource,
                }
            };

            var securityService = new Mock<ISecurityService>();
            securityService.Setup(o => o.Permissions).Returns(permissions);
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO(permissions)
            {
                SecretKey = secretKey,
            });
            var jwtManager = new JwtManager(mockSecuritySettings.Object);
            var tokenData = jwtManager.GenerateToken("{\"UserGroups\":[{\"Name\":\"MyOtherSecretGroup\"}]}");
            var principal = jwtManager.BuildPrincipal(tokenData);
            var authorizationService = new TestAuthorizationServiceBase(securityService.Object) { User = principal };


            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Administrator, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.View, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.Contribute, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resource.ToString()));
            Assert.IsFalse(authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resource.ToString()));
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
    }
}
