/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ComputerNameTests
    {

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ComputerName))]
        public void ComputerName_ToString_SetProperty_Expect_SetName()
        {
            //--------------------Arrange--------------------
            var testName = "TestComputerName";

            var computerNames = new ComputerName();
            //--------------------Act------------------------
            computerNames.Name = testName;
            //--------------------Assert---------------------
            Assert.AreEqual( testName, computerNames.Name);
            Assert.AreEqual( testName, computerNames.ToString());
        }
    }
}
