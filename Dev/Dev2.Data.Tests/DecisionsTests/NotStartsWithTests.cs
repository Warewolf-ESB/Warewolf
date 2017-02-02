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
    /// <summary>
    /// Summary description for NotStartsWithTests
    /// </summary>
    [TestClass]
    public class NotStartsWithTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotStartsWith_Invoke")]
        public void NotStartsWith_Invoke_DoesStartWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotStartsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "Test";
            
            //------------Execute Test---------------------------

            bool result = notStartsWith.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotStartsWith_Invoke")]
        public void NotStartsWith_Invoke_DoesntStartWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotStartsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "No";
            //------------Execute Test---------------------------

            bool result = notStartsWith.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("NotStartsWith_HandlesType")]
        public void NotStartsWith_HandlesType_ReturnsNotStartWithType()
        {
            var startsWith = enDecisionType.NotStartsWith;
            //------------Setup for test--------------------------
            var notStartsWith = new NotStartsWith();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(startsWith, notStartsWith.HandlesType());
        }
    }
}
