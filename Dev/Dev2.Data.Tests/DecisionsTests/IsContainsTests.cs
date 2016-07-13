/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
    public class ContainsTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsContains_Invoke")]
        public void IsContains_Invoke_DoesContain_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isContains = new IsContains();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "Test";
            //------------Execute Test---------------------------
            bool result = isContains.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsContains_Invoke")]
        public void IsContains_Invoke_DoesntContain_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsContains();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "No";
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsContains_HandlesType")]
        public void IsContains_HandlesType_ReturnsIsContainsType()
        {
            var decisionType = enDecisionType.IsContains;
            //------------Setup for test--------------------------
            var isContains = new IsContains();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isContains.HandlesType());
        }
    }
}
