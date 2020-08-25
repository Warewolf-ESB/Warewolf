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
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Xml;


namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.XmlTests
{
    [TestClass]
    public class XmlPathTests {

        #region GetSegments Tests

        [TestMethod]
        public void GetSegments_Expected_CorrectSegmentCount()
        {
            var path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");

            const int expected = 4;
            var actual = path.GetSegements().Count();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetSegments_Expected_LastSegmentIsCorrect()
        {
            var path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");

            const string expected = "Name";
            var actual = path.GetSegements().Last().ToString();

            Assert.AreEqual(expected, actual);
        }


        #endregion GetSegments Tests

        #region Enumerable Tests

        [TestMethod]
        public void CreateEnumerablePathSegmentFromXElement_Expected_EnumerableXmlPathSegment()
        {
            var element = new XElement("Departments",
                new XElement("Department"),
                new XElement("Department"));

            var path = new XmlPath();
            var segment = path.CreatePathSegment(element);

            const bool expected = true;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateScalarPathSegmentFromXElement_Expected_ScalarXmlPathSegment()
        {
            var element = new XElement("Departments");
            var path = new XmlPath();
            var segment = path.CreatePathSegment(element);

            const bool expected = false;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateEnumerablePathSegmentFromSegmentText_Expected_EnumerableXmlPathSegment()
        {
            var path = new XmlPath();
            var segment = path.CreatePathSegment("Departments()");

            const bool expected = true;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateScalarPathSegmentFromSegmentText_Expected_ScalarXmlPathSegment()
        {
            var path = new XmlPath();
            var segment = path.CreatePathSegment("Name");

            const bool expected = false;
            var actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        #endregion Enumerable Tests
    }
}
