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
using System;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class GetComputerNamesTests
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
        public void GetComputerNames_SecurityIdentityFactory_Get_Current_HasHosts()
        {
            var identity = new SecurityIdentityFactory().Current;
            Assert.IsNotNull(identity.GetHosts().Count >= 1, "SecurityIdentityFactory.Current should have hosts");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GetComputerNames))]
        public void GetComputerNames_ComputerNames_ExpectListOfComputerNames()
        {

            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GetComputerNames))]
        public void GetComputerNames_ComputerNames_UpdateComputerNames()
        {

            GetComputerNames.GetComputerNamesList();
            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1);
        }

    }
}
