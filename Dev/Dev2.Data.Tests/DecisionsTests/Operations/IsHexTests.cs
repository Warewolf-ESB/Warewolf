/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests.Operations
{
    [TestClass]
    public class IsHexTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsHex))]
        public void IsHex_Invoke_ItemsEqual_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isHex = new IsHex();
            var cols = new string[1];
            cols[0] = "01";
            //------------Execute Test---------------------------
            var result = isHex.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsHex))]
        public void IsHex_Invoke_ItemWithxEqual_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isHex = new IsHex();
            var cols = new string[1];
            cols[0] = "0x01";
            //------------Execute Test---------------------------
            var result = isHex.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsHex))]
        public void IsHex_Invoke_NotEqualItems_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isHex = new IsHex();
            var cols = new string[2];
            cols[0] = "TestData";
            //------------Execute Test---------------------------
            var result = isHex.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsHex))]
        public void IsHex_Invoke_EmptyColumns_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isHex = new IsHex();
            var cols = new string[1];
            cols[0] = null;
            //------------Execute Test---------------------------
            var result = isHex.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsHex))]
        public void IsHex_HandlesType_ReturnsIsIsHexType()
        {
            var expected = enDecisionType.IsHex;
            //------------Setup for test--------------------------
            var isisHex = new IsHex();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, isisHex.HandlesType());
        }
    }
}
