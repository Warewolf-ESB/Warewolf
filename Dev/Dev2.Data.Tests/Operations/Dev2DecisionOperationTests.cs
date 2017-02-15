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

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
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

            //exe
            actual = comparer.Invoke(new[] { "SomeVal", "Val2" });
            //assert
            Assert.IsTrue(actual, "IsLessThan returned the wrong result when comparing strings");

            //exe
            actual = comparer.Invoke(new[] { string.Empty });
            //assert
            Assert.IsFalse(actual, "IsLessThan returned the wrong result when comparing empty string");
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]        
        public void IsLessThan_HandleType_ShouldReturnIsLessThan()
        {
            var decisionType = enDecisionType.IsLessThan;
            //------------Setup for test--------------------------
            var isLessThan = new IsLessThan();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isLessThan.HandlesType());
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

            //exe
            actual = comparer.Invoke(new[] { "SomeVal", "AnotherVal" });
            //assert
            Assert.IsTrue(actual, "IsGreaterThan returned the wrong result when comparing strings");

            //exe
            actual = comparer.Invoke(new[] { string.Empty });
            //assert
            Assert.IsFalse(actual, "IsGreaterThan returned the wrong result when comparing empty string");
        }


        [TestMethod]
        [Owner("Sanele Mthmembu")]
        public void IsGreaterThan_HandleType_ShouldReturnIsGreaterThan()
        {
            var decisionType = enDecisionType.IsGreaterThan;
            //------------Setup for test--------------------------
            var greaterThan = new IsGreaterThan();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, greaterThan.HandlesType());
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
            //exe
            actual = comparer.Invoke(new[] { "Vals", "Val2" });
            //assert
            Assert.IsTrue(actual, "IsGreaterThanOrEqual returned the wrong result when comparing strings");

            //exe
            actual = comparer.Invoke(new[] { string.Empty });
            //assert
            Assert.IsFalse(actual, "IsGreaterThanOrEqual returned the wrong result when comparing empty string");
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


        [TestMethod]
        [Owner("Sanele Mthmembu")]
        public void IsGreaterThanOrEqual_HandleType_ShouldReturbIsGreaterThanOrEqual()
        {
            var decisionType = enDecisionType.IsGreaterThanOrEqual;
            //------------Setup for test--------------------------
            var isGreaterThanOrEqual = new IsGreaterThanOrEqual();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isGreaterThanOrEqual.HandlesType());
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

            //exe
            actual = comparer.Invoke(new[] { "SomeVal", "SomeVal" });
            //assert
            Assert.IsTrue(actual, "IsLessThanOrEqual returned the wrong result when comparing strings");

            //exe
            actual = comparer.Invoke(new[] {string.Empty});
            //assert
            Assert.IsFalse(actual, "IsLessThanOrEqual returned the wrong result when comparing empty string");
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        public void IsLessThanOrEqual_ShouldReturnedIsLessThanOrEqualDecisionType()
        {
            var decisionType = enDecisionType.IsLessThanOrEqual;
            //------------Setup for test--------------------------
            var isLessThanOrEqual = new IsLessThanOrEqual();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isLessThanOrEqual.HandlesType());
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

     

        #region NotBetween

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotBetween_Invoke")]
        public void NotBetween_Invoke_IsBetween_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            string[] cols = new string[3];
            cols[0] = "15";
            cols[1] = "10";
            cols[2] = "20";

            //------------Execute Test---------------------------

            bool result = notBetween.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotBetween_Invoke")]
        public void NotBetween_Invoke_NotBetween_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            string[] cols = new string[3];
            cols[0] = "30";
            cols[1] = "10";
            cols[2] = "20";

            //------------Execute Test---------------------------

            bool result = notBetween.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("NotBetween_HandlesType")]
        public void NotBetween_HandlesType_ReturnsNotBetweenType()
        {
            var decisionType = enDecisionType.NotBetween;
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, notBetween.HandlesType());
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

            //exe
            actual = comparer.Invoke(new[] { "Val", "Val" });
            //assert
            Assert.IsTrue(actual, "IsEqual returned the wrong result when comparing strings");

            //exe
            actual = comparer.Invoke(new[] { string.Empty, "Something" });
            //assert
            Assert.IsFalse(actual, "IsEqual returned the wrong result when comparing empty string");
        }


        [TestMethod]
        [Owner("Sanele Mthmembu")]
        public void IsEqual_IsEqualUnitTest_HandleType_ShouldReturnIsEqual()
        {
            var decisionType = enDecisionType.IsEqual;
            //------------Setup for test--------------------------
            var isEqual = new IsEqual();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isEqual.HandlesType());
        }


        #endregion

        #region Equal

        [TestMethod]
        [Owner("Sanele Mthembu")]        
        public void IsNotEqual_IsNotEqualUnitTest_Invoke_TrueIsReturned()
        {
            var comparer = new IsNotEqual();            
            var actual = comparer.Invoke(new[] { "100", "100" });
            Assert.IsFalse(actual, "IsNotEqual returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]        
        public void IsNotEqual_IsNotEqualUnitTest_Invoke_TrueIsReturned_Decimal()
        // ReSharper restore InconsistentNaming
        {            
            var comparer = new IsNotEqual();
            var actual = comparer.Invoke(new[] { "1.08", "1.80" });
            Assert.IsTrue(actual, "IsNotEqual returned the wrong result when comparing integers");
            
            actual = comparer.Invoke(new[] { "Val", "Val" });
            Assert.IsFalse(actual, "IsNotEqual returned the wrong result when comparing strings");
        }


        [TestMethod]
        [Owner("Sanele Mthmembu")]
        public void IsNotEqual_IsNotEqualUnitTest_HandleType_ShouldReturnIsNotEqual()
        {
            var decisionType = enDecisionType.IsNotEqual;
            //------------Setup for test--------------------------
            var isNotEqual = new IsNotEqual();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNotEqual.HandlesType());
        }


        #endregion

        #endregion
    }
}
