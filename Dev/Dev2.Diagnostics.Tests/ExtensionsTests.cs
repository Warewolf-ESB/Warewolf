/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Extensions))]
        public void Extensions_ContainsSafe_filter_IsNotNullButEmpty_ExpectTrue()
        {
            //----------------------Arrange---------------------
            //----------------------Act-------------------------
            var extentions = Extensions.ContainsSafe("", "");
            //----------------------Assert----------------------
            Assert.IsTrue(extentions);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Extensions))]
        public void Extensions_ContainsSafe_filter_IsNull_ExpectTrue()
        {
            //----------------------Arrange---------------------
            //----------------------Act-------------------------
            var extentions = Extensions.ContainsSafe("", null);
            //----------------------Assert----------------------
            Assert.IsTrue(extentions);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Extensions))]
        public void Extensions_ContainsSafe_filter_IsNotNullOrEmpty_ExpectFalse()
        {
            //----------------------Arrange---------------------
            //----------------------Act-------------------------
            var extentions = Extensions.ContainsSafe("", "testFilterString");
            //----------------------Assert----------------------
            Assert.IsFalse(extentions);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Extensions))]
        public void Extensions_ContainsSafe_s_IsNotNullOrEmpty_ExpectFalse()
        {
            //----------------------Arrange---------------------
            //----------------------Act-------------------------
            var extentions = Extensions.ContainsSafe("testSString", "testFilterString");
            //----------------------Assert----------------------
            Assert.IsFalse(extentions);
        }
    }
}
