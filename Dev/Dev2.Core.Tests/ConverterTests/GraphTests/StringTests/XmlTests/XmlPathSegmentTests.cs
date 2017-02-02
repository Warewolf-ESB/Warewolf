/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Xml;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.XmlTests
{
    [TestClass]
    public class XmlPathSegmentTests {

        #region CreatePathSegment Tests

        [TestMethod]
        public void ToStringOnEnumerableSegment_Expected_EnumerableFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Departments()");

            const string expected = "Departments()";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnScalarSegment_Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            const string expected = "Name";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Departments()");

            const string expected = "Departments";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesAreConsidered_Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Departments()");

            const string expected = "Departments()";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnScalarSegmentt_WhereEnumerablesArentConsidered__Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            const string expected = "Name";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToStringOnScalarSegmentt_WhereEnumerablesAreConsidered__Expected_ScalarFormat()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            const string expected = "Name";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        #endregion CreatePathSegment Tests
    }
}
