
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2DecisionOperationTests
    {
        #region Comparing Integers

        #region IsLessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThan with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsLessThan_IsLessThanUnitTest_Invoke_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsLessThan();

            //exe
            var actual = comparer.Invoke(new[] { "2", "100" });

            //assert
            Assert.IsTrue(actual, "IsLessThan returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThan with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsLessThan_IsLessThanUnitTest_Invoke_FalseIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsLessThan();

            //exe
            var actual = comparer.Invoke(new[] { "100", "2" });

            //assert
            Assert.IsFalse(actual, "IsLessThan returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThan with an array of strings that can be parsed to decimals, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsLessThan_IsLessThanUnitTest_InvokeWithDecimals_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsLessThan();

            //exe
            var actual = comparer.Invoke(new[] { "2.75", "100.25" });

            //assert
            Assert.IsTrue(actual, "IsLessThan returned the wrong result when comparing integers");
        }

        #endregion

        #region IsGreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsGreaterThan with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsGreaterThan_IsGreaterThanUnitTest_Invoke_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsGreaterThan();

            //exe
            var actual = comparer.Invoke(new[] { "100", "2" });

            //assert
            Assert.IsTrue(actual, "IsGreaterThan returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsGreaterThan with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsGreaterThan_IsGreaterThanUnitTest_Invoke_FalseIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsGreaterThan();

            //exe
            var actual = comparer.Invoke(new[] { "2", "100" });

            //assert
            Assert.IsFalse(actual, "IsGreaterThan returned the wrong result when comparing integers");
        }

        #endregion

        #region IsLessThanOrEqual

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThanOrEqual with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsLessThanOrEqual_IsLessThanOrEqualUnitTest_Invoke_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsLessThanOrEqual();

            //exe
            var actual = comparer.Invoke(new[] { "2", "100" });

            //assert
            Assert.IsTrue(actual, "IsLessThanOrEqual returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThanOrEqual with an array of equal strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsLessThanOrEqual_IsLessThanOrEqualUnitTest_InvokeWithEqualStrings_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsLessThanOrEqual();

            //exe
            var actual = comparer.Invoke(new[] { "2", "2" });

            //assert
            Assert.IsTrue(actual, "IsLessThanOrEqual returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThanOrEqual with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsLessThanOrEqual_IsLessThanOrEqualUnitTest_Invoke_FalseIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsLessThanOrEqual();

            //exe
            var actual = comparer.Invoke(new[] { "100", "2" });

            //assert
            Assert.IsFalse(actual, "IsLessThanOrEqual returned the wrong result when comparing integers");
        }

        #endregion

        #region IsGreaterThanOrEqual

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsGreaterThanOrEqual with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsGreaterThanOrEqual_IsGreaterThanOrEqualUnitTest_InvokeWithEqualStrings_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsGreaterThanOrEqual();

            //exe
            var actual = comparer.Invoke(new[] { "2", "2" });

            //assert
            Assert.IsTrue(actual, "IsGreaterThanOrEqual returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsGreaterThanOrEqual with an array of equal strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsGreaterThanOrEqual_IsGreaterThanOrEqualUnitTest_Invoke_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsGreaterThanOrEqual();

            //exe
            var actual = comparer.Invoke(new[] { "100", "2" });

            //assert
            Assert.IsTrue(actual, "IsGreaterThanOrEqual returned the wrong result when comparing integers");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsGreaterThanOrEqual with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsGreaterThanOrEqual_IsGreaterThanOrEqualUnitTest_Invoke_FalseIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsGreaterThanOrEqual();

            //exe
            var actual = comparer.Invoke(new[] { "2", "100" });

            //assert
            Assert.IsFalse(actual, "IsGreaterThanOrEqual returned the wrong result when comparing integers");
        }

        #endregion

        #region IsBetween

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsBetween_IsBetweenUnitTest_Invoke_TrueIsReturned()
        // ReSharper restore InconsistentNaming
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
        [Description("Test for invoking IsBetween with an array of strings that can be parsed to integers, false is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsBetween_IsBetweenUnitTest_Invoke_FalseIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsBetween();

            //exe
            var actual = comparer.Invoke(new[] { "100", "2", "50" });

            //assert
            Assert.IsFalse(actual, "IsBetween returned the wrong result when comparing integers");
        }

        #endregion

        #region Equal

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThanOrEqual with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsEqual_IsEqualUnitTest_Invoke_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsEqual();

            //exe
            var actual = comparer.Invoke(new[] { "100", "100" });

            //assert
            Assert.IsTrue(actual, "IsEqual returned the wrong result when comparing integers");
        }           
        
        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for invoking IsLessThanOrEqual with an array of strings that can be parsed to integers, true is expected")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void IsEqual_IsEqualUnitTest_Invoke_TrueIsReturned_Decimal()
        // ReSharper restore InconsistentNaming
        {
            //init
            var comparer = new IsEqual();

            //exe
            var actual = comparer.Invoke(new[] { "1.8", "1.80" });

            //assert
            Assert.IsTrue(actual, "IsEqual returned the wrong result when comparing integers");
        }     

        #endregion
        #endregion
    }
}
