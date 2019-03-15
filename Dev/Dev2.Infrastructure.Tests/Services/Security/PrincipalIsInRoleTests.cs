/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    [TestClass]
    public class PrincipalIsInRoleTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_IsInRole_Principle_IsNull()
        {
            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(principle => principle.IsInRole(It.IsAny<string>())).Returns(false);

            var windowsGroupPermission = new WindowsGroupPermission { IsServer = true };

            var isInRole = PrincipalIsInRole.IsInRole(null, windowsGroupPermission);

            Assert.IsFalse(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_IsInRole_Principle_IsNotNull_WindowsGroup_IsNull_ExpectedFalse()
        {
            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(principle => principle.IsInRole(It.IsAny<string>())).Returns(false);

            var windowsGroupPermission = new WindowsGroupPermission { IsServer = true };

            var isInRole = PrincipalIsInRole.IsInRole(mockPrinciple.Object, windowsGroupPermission);

            Assert.IsFalse(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_IsInRole_Principle_IsNotNull_WindowsGroup_IsNull_ExpectedTrue()
        {
            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(principle => principle.IsInRole(It.IsAny<string>())).Returns(true);

            var windowsGroupPermission = new WindowsGroupPermission { IsServer = true };

            var isInRole = PrincipalIsInRole.IsInRole(mockPrinciple.Object, windowsGroupPermission);

            Assert.IsTrue(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_IsInRole_WindowsGroup_BuiltInGuestsText_ExpectedTrue()
        {
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(identity => identity.Name).Returns("admin");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(principle => principle.Identity).Returns(mockIdentity.Object);

            var windowsGroupPermission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = WindowsGroupPermission.BuiltInGuestsText
            };

            var isInRole = PrincipalIsInRole.IsInRole(mockPrinciple.Object, windowsGroupPermission);

            Assert.IsTrue(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_IsInRole_WindowsGroup_BuiltInAdministratorsText_ExpectedFalse()
        {
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(identity => identity.Name).Returns("admin");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(principle => principle.Identity).Returns(mockIdentity.Object);

            var windowsGroupPermission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = GlobalConstants.WarewolfGroup
            };

            var isInRole = PrincipalIsInRole.IsInRole(mockPrinciple.Object, windowsGroupPermission);

            Assert.IsFalse(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_UserIsNotAdmin()
        {
            var mockUserIdentity = new Mock<IUserIdentity>();
            mockUserIdentity.Setup(o => o.Groups).Returns(new List<IGroup> { });

            var windowPrincipal = new WindowsPrincipalWrapper(new WindowsPrincipal(TestWindowsIdentity.New(WindowsIdentity.GetAnonymous())))
            {
                Identity = mockUserIdentity.Object
            };

            var isInRole = windowPrincipal.IsWarewolfAdmin();

            Assert.IsFalse(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_UserIsLocalAdmin()
        {
            var mockUserIdentity = new Mock<IUserIdentity>();
            mockUserIdentity.Setup(o => o.Groups).Returns(new List<IGroup> { });

            var windowPrincipal = new WindowsPrincipalWrapper(new WindowsPrincipal(TestWindowsIdentity.New(WindowsIdentity.GetCurrent())))
            {
                Identity = mockUserIdentity.Object
            };

            var isInRole = windowPrincipal.IsWarewolfAdmin();

            Assert.IsTrue(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_UserIsInWeirdAdminGroup_Matching_AdminSid()
        {
            var adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

            var mockUserIdentity = new Mock<IUserIdentity>();
            var mockGroup = new Mock<IGroup>();
            mockGroup.Setup(group => group.Id).Returns(adminSid.Value);
            var groups = new List<IGroup> { mockGroup.Object };
            mockUserIdentity.Setup(o => o.Groups).Returns(groups);

            var windowPrincipal = new WindowsPrincipalWrapper(new WindowsPrincipal(TestWindowsIdentity.New(WindowsIdentity.GetAnonymous())))
            {
                Identity = mockUserIdentity.Object
            };

            var isInRole = windowPrincipal.IsWarewolfAdmin();

            Assert.IsTrue(isInRole);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PrincipalIsInRole))]
        public void PrincipalIsInRole_UserIsInWeirdAdminGroup_Matching_GroupName()
        {
            var mockUserIdentity = new Mock<IUserIdentity>();
            var mockGroup = new Mock<IGroup>();
            mockGroup.Setup(group => group.Id).Returns("");
            mockGroup.Setup(group => group.Name).Returns(GlobalConstants.WarewolfGroup);
            var groups = new List<IGroup> { mockGroup.Object };
            mockUserIdentity.Setup(o => o.Groups).Returns(groups);

            var windowPrincipal = new WindowsPrincipalWrapper(new WindowsPrincipal(TestWindowsIdentity.New(WindowsIdentity.GetAnonymous())))
            {
                Identity = mockUserIdentity.Object
            };

            var isInRole = windowPrincipal.IsWarewolfAdmin();

            Assert.IsTrue(isInRole);
        }
    }

    class TestWindowsIdentity : WindowsIdentity
    {
        protected TestWindowsIdentity(WindowsIdentity identity)
            : base(identity)
        {
            
        }

        public static WindowsIdentity New(WindowsIdentity windowsIdentity) => new TestWindowsIdentity(windowsIdentity);
    }

    class TestWindowsPrincipal : WindowsPrincipal
    {
        public TestWindowsPrincipal(WindowsIdentity ntIdentity)
            : base(ntIdentity)
        {

        }
    }
}
