using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Framework.Converters.Graph.String.Json;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests.JsonTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonPathSegmentTests {
        
        #region CreatePathSegment Tests

        /// <summary>
        /// To the string on enumerable segment expected enumerable format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_Expected_EnumerableFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            string expected = "Collection()";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on scalar segment expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on enumerable segment where enumerables arent considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            string expected = "Collection";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on enumerable segment where enumerables are considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesAreConsidered_Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            string expected = "Collection()";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on scalar segment where enumerables aren't considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_WhereEnumerablesArentConsidered__Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on scalar segment where enumerables are considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_WhereEnumerablesAreConsidered__Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        #endregion CreatePathSegment Tests
    }
}
