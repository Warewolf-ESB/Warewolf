/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class GetComputerNamesImplTests
    {
        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            try
            {
                SecurityIdentityFactory.Get();
            }
            catch (Exception e)
            {
                Assert.AreEqual("security identity factory not set", e.Message);
            }
            try
            {
                SecurityIdentityFactory.Set(new SecurityIdentityFactoryForWindows());
            }
            catch (Exception e2)
            {
                Assert.AreEqual("security identity factory already set", e2.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GetComputerNames))]
        public void GetComputerNames_GetComputerNamesImpl_ComputerNames_ShouldBeMoreThanZero()
        {
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            mockSecurityIdentityFactory.Setup(o => o.Current).Returns(() => null);

            var getComputerNames = new GetComputerNamesImpl(mockSecurityIdentityFactory.Object);

            var c = getComputerNames.ComputerNames.Count;

            mockSecurityIdentityFactory.VerifyGet(o => o.Current, Times.Once);
            Assert.IsTrue(c >= 1, "ComputerNames count should be greater than 1");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GetComputerNames))]
        public void GetComputerNames_GetComputerNamesImpl_UpdateComputerNamesList()
        {
            var expectedList = new List<string>();

            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockIdentity = new Mock<ISecurityIdentity>();
            mockIdentity.Setup(o => o.GetHosts()).Returns(expectedList);
            mockSecurityIdentityFactory.Setup(o => o.Current).Returns(mockIdentity.Object);

            var getComputerNames = new GetComputerNamesImpl(mockSecurityIdentityFactory.Object);

            getComputerNames.GetComputerNamesList();

            mockSecurityIdentityFactory.VerifyGet(o => o.Current, Times.Once);
            mockIdentity.Verify(o => o.GetHosts(), Times.Once);

            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1, "ComputerNames count should be greater than 1");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GetComputerNames))]
        public void GetComputerNames_GetWindowsDomainOrWorkgroupNameFromUserName_GivenDomainUser()
        {
            var result = SecurityIdentityForWindows.GetWindowsDomainOrWorkgroupName(@"WinDomain\IntegrationTester");

            Assert.AreEqual("WinDomain", result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GetComputerNames))]
        public void GetComputerNames_GetWindowsDomainOrWorkgroupNameFromUserName()
        {
            var result = SecurityIdentityForWindows.GetWindowsDomainOrWorkgroupName("");

            Assert.IsTrue(result == null || result == "WORKGROUP", "Cannot get Windows domain or workgroupname from username.");
        }
    }
}
