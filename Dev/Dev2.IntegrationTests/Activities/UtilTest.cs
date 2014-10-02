
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Activities
{


    /// <summary>
    ///This is a test class for UtilTest and is intended
    ///to contain all UtilTest Unit Tests
    ///</summary>
    [TestClass]
    public class UtilTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///A test for IsBetween
        ///</summary>
        [TestMethod]
        public void IsBetweenIsFalseIfStartIsNullAndEndIsNull()
        {
            const string value = "2010-01-01";
            object comparisonValueStart = "2009-12-01";
            object comparisonValueEnd = "2009-01-01";
            const bool expected = false;
            bool actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw(value, comparisonValueStart, comparisonValueEnd);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsEqualTo
        ///</summary>
        [TestMethod]
        public void IsEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            const bool expected = false; // TODO: Initialize to an appropriate value
            bool actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsGreaterThan
        ///</summary>
        [TestMethod]
        public void IsGreaterThanTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            const bool expected = false; // TODO: Initialize to an appropriate value
            bool actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh(value, comparisonValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsGreaterThanOrEqualTo
        ///</summary>
        [TestMethod]
        public void IsGreaterThanOrEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsLessThan
        ///</summary>
        [TestMethod]
        public void IsLessThanTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            const bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsLessThanOrEqualTo
        ///</summary>
        [TestMethod]
        public void IsLessThanOrEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsNotEqualTo
        ///</summary>
        [TestMethod]
        public void IsNotEqualToTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ValueIsDate
        ///</summary>
        [TestMethod]
        public void ValueIsDateTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.ValueIsDate(value);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ValueIsNumber
        ///</summary>
        [TestMethod]
        public void ValueIsNumberTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.ValueIsNumber(value);
            Assert.AreEqual(expected, actual);
            // Assert.Inconclusive("Verify the correctness of this test method.");
        }

    }
}

