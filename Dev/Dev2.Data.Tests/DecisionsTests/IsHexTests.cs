/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsHexTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsHex_Invoke")]
        public void IsHex_Invoke_ItemsEqual_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsHex();
            string[] cols = new string[1];
            cols[0] = "01";
            //------------Execute Test---------------------------
            bool result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsHex_Invoke")]
        public void IsHex_Invoke_ItemWithxEqual_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsHex();
            string[] cols = new string[1];
            cols[0] = "0x01";
            //------------Execute Test---------------------------
            bool result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsHex_Invoke")]
        public void IsHex_Invoke_NotEqualItems_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsHex();
            string[] cols = new string[2];
            cols[0] = "TestData";
            //------------Execute Test---------------------------
            bool result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsHex_HandlesType")]
        public void IsHex_HandlesType_ReturnsIsEndsWithType()
        {
            var expected = enDecisionType.IsHex;
            //------------Setup for test--------------------------
            var isEndsWith = new IsHex();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, isEndsWith.HandlesType());
        }
    }
}
