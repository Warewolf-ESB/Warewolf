/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.Utils
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
        [Timeout(60000)]
        public void IsBetweenIsFalseIfStartIsNullAndEndIsNull()
        {
            const string value = "2010-01-01";
            object comparisonValueStart = "2009-12-01";
            object comparisonValueEnd = "2009-01-01";
            const bool expected = false;
            var actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw(value, comparisonValueStart, comparisonValueEnd);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsEqualTo
        ///</summary>
        [TestMethod]
        [Timeout(60000)]
        public void IsEqualToTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            const bool expected = false; // TODO: Initialize to an appropriate value
            var actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsGreaterThan
        ///</summary>
        [TestMethod]
        [Timeout(60000)]
        public void IsGreaterThanTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            const bool expected = false; // TODO: Initialize to an appropriate value
            var actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh(value, comparisonValue);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsGreaterThanOrEqualTo
        ///</summary>
        [TestMethod]
        [Timeout(60000)]
        public void IsGreaterThanOrEqualToTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            var expected = false; // TODO: Initialize to an appropriate value
            var actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsLessThan
        ///</summary>
        [TestMethod]
        [Timeout(60000)]
        public void IsLessThanTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
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
        [Timeout(60000)]
        public void IsLessThanOrEqualToTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            var expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsNotEqualTo
        ///</summary>
        [TestMethod]
        [Timeout(60000)]
        public void IsNotEqualToTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
            object comparisonValue = null; // TODO: Initialize to an appropriate value
            var expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq(value, comparisonValue);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ValueIsDate
        ///</summary>
        [TestMethod]
        [Timeout(60000)]
        public void ValueIsDateTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
            var expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.ValueIsDate(value);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ValueIsNumber
        ///</summary>
        [TestMethod]
        [Timeout(60000)]
        public void ValueIsNumberTest()
        {
            var value = string.Empty; // TODO: Initialize to an appropriate value
            var expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = Unlimited.Applications.BusinessDesignStudio.Activities.Util.ValueIsNumber(value);
            Assert.AreEqual(expected, actual);
            // Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void Util_Eq()
        {
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq("theString", null));


            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq("1970-01-01", "1970-01-01"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq("1970-01-01", "1970-01-01 12:01:01"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq("not a date", "1970-01-01 12:01:01"));

            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq("1.45", "1.45"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Eq("1.45", "1.454"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void Util_NtEq()
        {
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq("theString", null));

            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq("1970-01-01", "1970-01-01"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq("1970-01-01", "1970-01-01 12:01:01"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq("not a date", "1970-01-01 12:01:01"));

            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq("1.45", "1.45"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.NtEq("1.45", "1.454"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void Util_LsTh()
        {
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh("theString", null));

            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh("1970-01-01", "1970-01-01"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh("1970-01-01", "1970-01-01 12:01:01"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh("1970-01-02", "1970-01-01 12:01:01"));

            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh("1.45", "1.45"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh("1.451", "1.45"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsTh("1.45", "1.454"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void Util_LsThEq()
        {
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq("theString", null));

            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq("1970-01-01", "1970-01-01"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq("1970-01-01", "1970-01-01 12:01:01"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq("1970-01-02", "1970-01-01 12:01:01"));

            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq("1.45", "1.45"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq("1.451", "1.45"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.LsThEq("1.45", "1.454"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void Util_GrTh()
        {
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh("theString", null));

            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh("1970-01-01", "1970-01-01"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh("1970-01-01", "1970-01-01 12:01:01"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh("1970-01-02", "1970-01-01 12:01:01"));

            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh("1.45", "1.45"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh("1.451", "1.45"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrTh("1.45", "1.454"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void Util_GrThEq()
        {
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq("theString", null));

            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq("1970-01-01", "1970-01-01"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq("1970-01-01", "1970-01-01 12:01:01"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq("1970-01-02", "1970-01-01 12:01:01"));

            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq("1.45", "1.45"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq("1.451", "1.45"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.GrThEq("1.45", "1.454"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void Util_Btw()
        {
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("", null, null));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("theString", null, null));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("theString", "from", null));

            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1970-01-01", "1970-01-01", "1970-01-02"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1970-01-01 12:01:01", "1970-01-01", "1970-01-02"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1970-01-01", "1970-01-01 12:01:01", "1970-01-02"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1970-01-01 12:00:00", "1970-01-01", "1970 -01-01 12:01:01"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1970-01-01 13:00:00", "1970-01-01", "1970 -01-01 12:01:01"));

            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1.45", "1.45", "1.45"));
            Assert.IsTrue(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1.451", "1.45", "1.5"));
            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("1.45", "1.454", "1.5"));

            Assert.IsFalse(Unlimited.Applications.BusinessDesignStudio.Activities.Util.Btw("bob", "apple", "cat"));
        }
    }
}

