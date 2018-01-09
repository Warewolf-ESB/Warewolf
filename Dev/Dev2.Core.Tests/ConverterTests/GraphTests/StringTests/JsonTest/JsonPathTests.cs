/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Unlimited.Framework.Converters.Graph.String.Json;


namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.JsonTest
{
    [TestClass]
    public class JsonPathTests {

        #region GetSegments Tests

        /// <summary>
        /// Gets the segments of a path expected correct segment count.
        /// </summary>
        [TestMethod]
        public void GetSegments_Expected_CorrectSegmentCount()
        {
            var path = new JsonPath("EnumerableData().NestedData.Name", "EnumerableData.NestedData.Name");

            const int expected = 3;
            var actual = path.GetSegements().Count();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Gets the segments expected last segment is correct.
        /// </summary>
        [TestMethod]
        public void GetSegments_Expected_LastSegmentIsCorrect()
        {
            var path = new JsonPath("EnumerableData().NestedData.NestedData.Name", "EnumerableData.NestedData.NestedData.Name");

            const string expected = "Name";
            var actual = path.GetSegements().Last().ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create enumerable path segment from JSON property expected enumerable json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateEnumerablePathSegmentFromJProperty_Expected_EnumerableJsonPathSegment()
        {
            var jProperty = new JProperty("EnumerableProperty", new JArray(
                new JObject(new JProperty("ScalarProperty", "ScalarValue"),
                new JProperty("ScalarProperty1", "ScalarValue1"))));
            var path = new JsonPath();
            var segment = path.CreatePathSegment(jProperty);

            const bool expected = true;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create the scalar path segment from JSON property expected scalar json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromJProperty_Expected_ScalarJsonPathSegment()
        {
            var jProperty = new JProperty("ScalarProperty", "ScalarValue");
            var path = new JsonPath();
            var segment = path.CreatePathSegment(jProperty);

            const bool expected = false;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create enumerable path segment from segment text expected enumerable json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateEnumerablePathSegmentFromSegmentText_Expected_EnumerableJsonPathSegment()
        {
            var path = new JsonPath();
            var segment = path.CreatePathSegment("EnumerableData()");

            const bool expected = true;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create scalar path segment from segment text expected scalar json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromSegmentText_Expected_ScalarJsonPathSegment()
        {
            var path = new JsonPath();
            var segment = path.CreatePathSegment("Name");

            const bool expected = false;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        #endregion GetSegments Tests
    }
}
