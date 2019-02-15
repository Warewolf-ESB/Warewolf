/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.UnitTest.Framework.ConverterTests.GraphTests;


namespace Dev2.Tests.ConverterTests.GraphTests.PocoTests
{
    [TestClass]
    public class PocoPathTests {

        #region GetSegements Tests

        /// <summary>
        /// Get segments expected correct segment count returned.
        /// </summary>
        [TestMethod]
        public void GetSegments_Expected_CorrectSegmentCount()
        {
            var path = new PocoPath("EnumerableData().NestedData.Name", "EnumerableData.NestedData.Name");

            const int expected = 3;
            var actual = path.GetSegements().Count();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Get segments expected last segment is correct returned.
        /// </summary>
        [TestMethod]
        public void GetSegments_Expected_LastSegmentIsCorrect()
        {
            var path = new PocoPath("EnumerableData().NestedData.NestedData.Name", "EnumerableData.NestedData.NestedData.Name");

            const string expected = "Name";
            var actual = path.GetSegements().Last().ToString();

            Assert.AreEqual(expected, actual);
        }

        #endregion GetSegments Tests

        #region Enumerable Tests

        /// <summary>
        /// Create enumerable path segment from property info expected enumerable poco path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateEnumerablePathSegmentFromPropertyInfo_Expected_EnumerablePocoPathSegment()
        {
            var propertyInfo = typeof(PocoTestData).GetProperty("EnumerableData");
            var path = new PocoPath();
            var segment = path.CreatePathSegment(propertyInfo.Name,propertyInfo.PropertyType.IsEnumerable());

            const bool expected = true;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create scalar path segment from property info expected scalar poco path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromPropertyInfo_Expected_ScalarPocoPathSegment()
        {
            var propertyInfo = typeof(PocoTestData).GetProperty("Name");
            var path = new PocoPath();
            var segment = path.CreatePathSegment(propertyInfo.Name, propertyInfo.PropertyType.IsEnumerable());

            const bool expected = false;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create enumerable path segment from segment text expected enumerable poco path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateEnumerablePathSegmentFromSegmentText_Expected_EnumerablePocoPathSegment()
        {
            var path = new PocoPath();
            var segment = path.CreatePathSegment("EnumerableData()");

            const bool expected = true;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create scalar path segment from segment text expected scalar poco path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromSegmentText_Expected_ScalarPocoPathSegment()
        {
            var path = new PocoPath();
            var segment = path.CreatePathSegment("Name");

            const bool expected = false;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        #endregion Enumerable Tests
    }
}
