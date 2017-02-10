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
// ReSharper disable InconsistentNaming

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsBetweenTests
    {
        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsBetween_HandlesType")]
        public void IsBetween_HandlesType_ReturnsIsBetweenType()
        {
            const enDecisionType DecisionType = enDecisionType.IsBetween;
            //------------Setup for test--------------------------
            var isBetween = new IsBetween();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(DecisionType, isBetween.HandlesType());
        }


        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_TrueIsReturned()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "50", "2", "100" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTestLargerValueFirst_Invoke_TrueIsReturned()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "50", "100", "2" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_FalseIsReturned()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "100", "2", "50" });

            //assert
            Assert.IsFalse(actual, "IsBetween returned the wrong result when comparing integers");

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_IsFromValue_FalseIsReturned()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "2", "2", "50" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_IsToValue_FalseIsReturned()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "50", "2", "50" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_TrueIsReturned_DateTime()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "2016/10/14", "2016/10/13", "2016/10/16" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTestLargerValueFirst_Invoke_TrueIsReturned_DateTime()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "2016/10/14", "2016/10/16", "2016/10/13" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_FalseIsReturned_DateTime()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "2016/10/17", "2016/10/13", "2016/10/16" });

            //assert
            Assert.IsFalse(actual, "IsBetween returned the wrong result when comparing integers");

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_IsFromValue_FalseIsReturned_DateTime()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "2016/10/13", "2016/10/13", "2016/10/16" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        public void IsBetween_IsBetweenUnitTest_Invoke_IsToValue_FalseIsReturned_DateTime()
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "2016/10/16", "2016/10/13", "2016/10/16" });

            //assert
            Assert.IsTrue(actual, "IsBetween returned the wrong result when comparing integers");

        }        
    }
}
