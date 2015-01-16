
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.XmlTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class XmlPathTests {

        #region GetSegments Tests

        [TestMethod]
        public void GetSegments_Expected_CorrectSegmentCount()
        {
            XmlPath path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");

            int expected = 4;
            int actual = path.GetSegements().Count();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetSegments_Expected_LastSegmentIsCorrect()
        {
            XmlPath path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");

            string expected = "Name";
            string actual = path.GetSegements().Last().ToString();

            Assert.AreEqual(expected, actual);
        }


        #endregion GetSegments Tests

        #region Enumerable Tests

        [TestMethod]
        public void CreateEnumerablePathSegmentFromXElement_Expected_EnumerableXmlPathSegment()
        {
            XElement element = new XElement("Departments",
                new XElement("Department"),
                new XElement("Department"));

            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment(element);

            bool expected = true;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateScalarPathSegmentFromXElement_Expected_ScalarXmlPathSegment()
        {
            XElement element = new XElement("Departments");
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment(element);

            bool expected = false;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateEnumerablePathSegmentFromSegmentText_Expected_EnumerableXmlPathSegment()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Departments()");

            bool expected = true;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateScalarPathSegmentFromSegmentText_Expected_ScalarXmlPathSegment()
        {
            XmlPath path = new XmlPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            bool expected = false;
            bool actual = segment.IsEnumarable;

            Assert.AreEqual(expected, actual);
        }

        #endregion Enumerable Tests
    }
}
