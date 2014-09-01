using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests.XmlTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class XmlPathSegmentTests {

        #region CreatePathSegment Tests

        [TestMethod]
        public void ToStringOnEnumerableSegment_Expected_EnumerableFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Departments()");

            string expected = "Departments()";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnScalarSegment_Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Departments()");

            string expected = "Departments";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesAreConsidered_Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Departments()");

            string expected = "Departments()";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnScalarSegmentt_WhereEnumerablesArentConsidered__Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnScalarSegmentt_WhereEnumerablesAreConsidered__Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            string expected = "Name";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        #endregion CreatePathSegment Tests
    }
}
