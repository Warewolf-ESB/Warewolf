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
    public class IsNotDateTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenSomeString_IsNotDate_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isNotDate = new IsNotDate();
            string[] cols = new string[2];
            cols[0] = "Yersteday";
            //------------Execute Test---------------------------
            bool result = isNotDate.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsNotDate_Invoke")]
        public void IsNotDate_Invoke_IsNotDate_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsNotDate();
            string[] cols = new string[2];
            cols[0] = "01/12/2000";
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNotDate_HandlesType")]
        public void IsNotDate_HandlesType_ReturnsIsNotDateType()
        {
            var decisionType = enDecisionType.IsNotDate;
            //------------Setup for test--------------------------
            var isNotDate = new IsNotDate();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNotDate.HandlesType());
        }
    }
}
