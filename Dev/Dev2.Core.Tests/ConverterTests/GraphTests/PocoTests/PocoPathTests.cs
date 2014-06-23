using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Unlimited.Framework.Converters.Graph.Interfaces;
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
            PocoPath path = new PocoPath("EnumerableData().NestedData.Name", "EnumerableData.NestedData.Name");

            int expected = 3;
            int actual = path.GetSegements().Count();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Get segments expected last segment is correct returned.
        /// </summary>
        [TestMethod]
        public void GetSegments_Expected_LastSegmentIsCorrect()
        {
            PocoPath path = new PocoPath("EnumerableData().NestedData.NestedData.Name", "EnumerableData.NestedData.NestedData.Name");

            string expected = "Name";
            string actual = path.GetSegements().Last().ToString();

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
            PropertyInfo propertyInfo = typeof(PocoTestData).GetProperty("EnumerableData");
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment(propertyInfo);

            bool expected = true;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create scalar path segment from property info expected scalar poco path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromPropertyInfo_Expected_ScalarPocoPathSegment()
        {
            PropertyInfo propertyInfo = typeof(PocoTestData).GetProperty("Name");
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment(propertyInfo);

            bool expected = false;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create enumerable path segment from segment text expected enumerable poco path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateEnumerablePathSegmentFromSegmentText_Expected_EnumerablePocoPathSegment()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("EnumerableData()");

            bool expected = true;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create scalar path segment from segment text expected scalar poco path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromSegmentText_Expected_ScalarPocoPathSegment()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            bool expected = false;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        #endregion Enumerable Tests
    }
}
