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
    public class IsNotDateTests
    {
        [TestInitialize]
        public void PreConditions()
        {
            var regionName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            var regionNameUI = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

            Assert.AreEqual("en-ZA", regionName);
            Assert.AreEqual("en-ZA", regionNameUI);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenSomeString_IsNotDate_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isNotDate = new IsNotDate();
            var cols = new string[2];
            cols[0] = "Yersteday";
            //------------Execute Test---------------------------
            var result = isNotDate.Invoke(cols);
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
            var cols = new string[2];
            cols[0] = "01/12/2000";
            //------------Execute Test---------------------------
            var result = notStartsWith.Invoke(cols);
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
