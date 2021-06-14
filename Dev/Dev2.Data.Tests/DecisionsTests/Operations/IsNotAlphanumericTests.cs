/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
    public class IsNotAlphanumericTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsNotAlphanumeric))]
        public void IsNotAlphanumeric_Invoke_DoesEndWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotAlphanumeric = new IsNotAlphanumeric();
            var cols = new string[2];
            cols[0] = "'";
            //------------Execute Test---------------------------
            var result = isNotAlphanumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsNotAlphanumeric))]
        public void IsNotAlphanumeric_Invoke_DoesntEndWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isNotAlphanumeric = new IsNotAlphanumeric();
            var cols = new string[2];
            cols[0] = "TestData";
            //------------Execute Test---------------------------
            var result = isNotAlphanumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsNotAlphanumeric))]
        public void IsNotAlphanumeric_Invoke_EmptyColumns_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotAlphanumeric = new IsNotAlphanumeric();
            var cols = new string[1];
            cols[0] = null;
            //------------Execute Test---------------------------
            var result = isNotAlphanumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsNotAlphanumeric))]
        public void IsNotAlphanumeric_HandlesType_ReturnsIsNotAlphanumericType()
        {
            var expected = EnDecisionType.IsNotAlphanumeric;
            //------------Setup for test--------------------------
            var IsNotAlphanumeric = new IsNotAlphanumeric();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, IsNotAlphanumeric.HandlesType());
        }
    }
}
