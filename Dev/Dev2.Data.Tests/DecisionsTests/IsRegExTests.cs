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
    public class IsRegExTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsRegEx_Invoke")]
        public void GivenSomeString_IsRegEx_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isRegEx = new IsRegEx();
            string[] cols = new string[2];
            cols[0] = "Number 5 should";
            cols[1] = "d";
            //------------Execute Test---------------------------
            bool result = isRegEx.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsRegEx_Invoke")]
        public void IsRegEx_Invoke_IsRegEx_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsRegEx();
            string[] cols = new string[2];
            cols[0] = "324";
            cols[1] = "d";
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsRegEx_HandlesType")]
        public void IsRegEx_HandlesType_ReturnsIsRegExType()
        {
            var decisionType = enDecisionType.IsRegEx;
            //------------Setup for test--------------------------
            var isRegEx = new IsRegEx();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isRegEx.HandlesType());
        }
    }
}
