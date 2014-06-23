using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;
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
            JsonPath path = new JsonPath("EnumerableData().NestedData.Name", "EnumerableData.NestedData.Name");

            int expected = 3;
            int actual = path.GetSegements().Count();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Gets the segments expected last segment is correct.
        /// </summary>
        [TestMethod]
        public void GetSegments_Expected_LastSegmentIsCorrect()
        {
            JsonPath path = new JsonPath("EnumerableData().NestedData.NestedData.Name", "EnumerableData.NestedData.NestedData.Name");

            string expected = "Name";
            string actual = path.GetSegements().Last().ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create enumerable path segment from JSON property expected enumerable json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateEnumerablePathSegmentFromJProperty_Expected_EnumerableJsonPathSegment()
        {
            JProperty jProperty = new JProperty("EnumerableProperty", new JArray(
                new JObject(new JProperty("ScalarProperty", "ScalarValue"),
                new JProperty("ScalarProperty1", "ScalarValue1"))));
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment(jProperty);

            bool expected = true;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create the scalar path segment from JSON property expected scalar json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromJProperty_Expected_ScalarJsonPathSegment()
        {
            JProperty jProperty = new JProperty("ScalarProperty", "ScalarValue");
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment(jProperty);

            bool expected = false;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create enumerable path segment from segment text expected enumerable json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateEnumerablePathSegmentFromSegmentText_Expected_EnumerableJsonPathSegment()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("EnumerableData()");

            bool expected = true;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create scalar path segment from segment text expected scalar json path segment returned.
        /// </summary>
        [TestMethod]
        public void CreateScalarPathSegmentFromSegmentText_Expected_ScalarJsonPathSegment()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            bool expected = false;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        #endregion GetSegments Tests
    }
}
