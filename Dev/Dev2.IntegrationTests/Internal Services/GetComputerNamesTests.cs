
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.CommonTest
{
    /// <summary>
    /// Summary description for AvlTreeTest
    /// </summary>
    [TestClass]
    public class GetComputerNamesTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetComputerNamesListExpectListOfComputerNames()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            GetComputerNames.GetComputerNamesList();
            //------------Assert Results-------------------------
            Assert.IsNotNull(GetComputerNames.ComputerNames);
            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1);
        }

    }
}
