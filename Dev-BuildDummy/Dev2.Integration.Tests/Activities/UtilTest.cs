using Unlimited.Applications.BusinessDesignStudio.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Unlimited.UnitTest.Activities
{


    /// <summary>
    ///This is a test class for UtilTest and is intended
    ///to contain all UtilTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UtilTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for IsBetween
        ///</summary>
        [TestMethod()]
        public void IsBetweenIsFalseIfStartIsNullAndEndIsNull()
        {
            string value = "2010-01-01";
            object comparisonValueStart = "2009-12-01";
            object comparisonValueEnd = "2009-01-01";
            bool expected = false;
            bool actual;
            actual = Util.Btw(value, comparisonValueStart, comparisonValueEnd);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsEqualTo
        ///</summary>
        [TestMethod()]
        public void IsEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.Eq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsGreaterThan
        ///</summary>
        [TestMethod()]
        public void IsGreaterThanTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.GrTh(value, comparisonValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsGreaterThanOrEqualTo
        ///</summary>
        [TestMethod()]
        public void IsGreaterThanOrEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.GrThEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsLessThan
        ///</summary>
        [TestMethod()]
        public void IsLessThanTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.LsTh(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsLessThanOrEqualTo
        ///</summary>
        [TestMethod()]
        public void IsLessThanOrEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.LsThEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsNotEqualTo
        ///</summary>
        [TestMethod()]
        public void IsNotEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.NtEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ValueIsDate
        ///</summary>
        [TestMethod()]
        public void ValueIsDateTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.ValueIsDate(value);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ValueIsNumber
        ///</summary>
        [TestMethod()]
        public void ValueIsNumberTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Util.ValueIsNumber(value);
            Assert.AreEqual(expected, actual);
            // Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LsThEq
        ///</summary>
        [TestMethod()]
        public void LsThEqTest()
        {
            string value = "6"; // TODO: Initialize to an appropriate value
            object comparisonValue = 5;  // TODO: Initialize to an appropriate value
            // Assert.IsFalse(Util.LsThEq(value, comparisonValue));
        }
    }
}
