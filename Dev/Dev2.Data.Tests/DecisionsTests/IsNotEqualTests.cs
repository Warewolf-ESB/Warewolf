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

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNotEqualTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsNotEqual_Invoke")]
        public void IsEqual_Invoke_ItemsEqual_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsNotEqual();
            var cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "TestData";
            //------------Execute Test---------------------------
            var result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsEqual_Invoke")]
        public void IsEndsWith_Invoke_NotEqualItems_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsNotEqual();
            var cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "No";
            //------------Execute Test---------------------------
            var result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsEqual_HandlesType")]
        public void IsEndsWith_HandlesType_ReturnsIsEndsWithType()
        {
            var expected = enDecisionType.IsNotEqual;
            //------------Setup for test--------------------------
            var isEndsWith = new IsNotEqual();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, isEndsWith.HandlesType());
        }
    }
}
