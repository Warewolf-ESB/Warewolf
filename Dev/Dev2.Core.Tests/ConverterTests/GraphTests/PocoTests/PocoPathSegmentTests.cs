using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Framework.Converters.Graph.Poco;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.PocoTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PocoPathSegmentTests
    {
        /// <summary>
        /// To string on enumerable segment expected enumerable format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_Expected_EnumerableFormat()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            string expected = "Collection()";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To string on scalar segment expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_Expected_ScalarFormat()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To string on enumerable segment where enumerables aren't considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            string expected = "Collection";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To string on enumerable segment where enumerables are considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesAreConsidered_Expected_ScalarFormat()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            string expected = "Collection()";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To string on scalar segment where enumerables arent considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_WhereEnumerablesArentConsidered__Expected_ScalarFormat()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To string on scalar segment where enumerables are considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegmentt_WhereEnumerablesAreConsidered__Expected_ScalarFormat()
        {
            PocoPath path = new PocoPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }
    }
}
