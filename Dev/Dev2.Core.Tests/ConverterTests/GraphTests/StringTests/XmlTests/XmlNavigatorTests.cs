/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.XmlTests
{
    [TestClass]
    public class XmlNavigatorTests
    {
        string TestData => @"<Company Name='Dev2'>
    <Motto>Eat lots of cake</Motto>
    <PreviousMotto/>
	<Departments TestAttrib='testing'>
		<Department Name='Dev'>
			<Employees>
				<Person Name='Brendon' Surename='Page' />
				<Person Name='Jayd' Surename='Page' />
			</Employees>
		</Department>
		<Department Name='Accounts'>
			<Employees>
				<Person Name='Bob' Surename='Soap' />
				<Person Name='Joe' Surename='Pants' />
			</Employees>
		</Department>
	</Departments>
    <InlineRecordSet>
        RandomData
    </InlineRecordSet>
    <InlineRecordSet>
        RandomData1
    </InlineRecordSet>
    <OuterNestedRecordSet>
        <InnerNestedRecordSet ItemValue='val1' />
        <InnerNestedRecordSet ItemValue='val2' />
    </OuterNestedRecordSet>
    <OuterNestedRecordSet>
        <InnerNestedRecordSet ItemValue='val3' />
        <InnerNestedRecordSet ItemValue='val4' />
    </OuterNestedRecordSet>
</Company>";

        string GivenSingleNode => @"<Message>Dummy Data</Message>";

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlNavigator_SelectScalarValue_WithNull_Expected_ArgumentNullException()
        {
            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                xmlNavigator.SelectScalar(null);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        [ExpectedException(typeof(Exception))]
        public void XmlNavigator_SelectScalar_WithoutXmlPath_ExpectException()
        {
            using (var JsonNavigator = new XmlNavigator(TestData))
            {
                JsonNavigator.SelectScalar(new JsonPath());
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectScalarValue_WithPathSeperator_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath(".", ".");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectScalar(namePath).ToString();
                const string expected = @"<Company Name=""Dev2"">
  <Motto>Eat lots of cake</Motto>
  <PreviousMotto />
  <Departments TestAttrib=""testing"">
    <Department Name=""Dev"">
      <Employees>
        <Person Name=""Brendon"" Surename=""Page"" />
        <Person Name=""Jayd"" Surename=""Page"" />
      </Employees>
    </Department>
    <Department Name=""Accounts"">
      <Employees>
        <Person Name=""Bob"" Surename=""Soap"" />
        <Person Name=""Joe"" Surename=""Pants"" />
      </Employees>
    </Department>
  </Departments>
  <InlineRecordSet>
        RandomData
    </InlineRecordSet>
  <InlineRecordSet>
        RandomData1
    </InlineRecordSet>
  <OuterNestedRecordSet>
    <InnerNestedRecordSet ItemValue=""val1"" />
    <InnerNestedRecordSet ItemValue=""val2"" />
  </OuterNestedRecordSet>
  <OuterNestedRecordSet>
    <InnerNestedRecordSet ItemValue=""val3"" />
    <InnerNestedRecordSet ItemValue=""val4"" />
  </OuterNestedRecordSet>
</Company>";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectScalarValue_WithScalarPathFromXml_WherePathMapsToAnAttribute_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath("Company:Name", "Company:Name");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectScalar(namePath).ToString();
                const string expected = nameof(Dev2);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectScalarValue_WithScalarPathFromXml_WherePathMapsToANode_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath("Company.Motto", "Company.Motto");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectScalar(namePath).ToString();
                const string expected = "Eat lots of cake";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectScalarValue_WithWrongPathSegment_Expected_NoValue()
        {
            IPath namePath = new XmlPath("Company.Nogo", "Company.Nogo");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectScalar(namePath).ToString();

                Assert.AreEqual(string.Empty, actual);
            }
        }

        public void XmlNavigator_SelectScalarValue_WithScalarPathFromXmlWithASingleNode_WherePathMapsToANode_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath("Message", "Message");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectScalar(namePath).ToString();
                const string expected = "Dummy Data";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectScalarValue_WithEnumerablePathFromXml_WherePathMapsToAnAttribute_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectScalar(namePath).ToString();
                const string expected = "Joe";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectScalarValue_WithEnumerablePathFromXml_WherePathMapsToANode_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectScalar(namePath).ToString().Trim();
                const string expected = "RandomData1";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlNavigator_SelectEnumerableValueUsingNull_Expected_ArgumentNullException()
        {
            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                xmlNavigator.SelectEnumerable(null);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        [ExpectedException(typeof(Exception))]
        public void XmlNavigator_SelectEnumerableValue_WithoutXmlPath_Expected_Exception()
        {
            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                xmlNavigator.SelectEnumerable(new JsonPath());
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValueUsingPathSeperator_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath(".", ".");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = xmlNavigator.SelectEnumerable(namePath);
                var expected = new List<object> { @"<Company Name=""Dev2"">
  <Motto>Eat lots of cake</Motto>
  <PreviousMotto />
  <Departments TestAttrib=""testing"">
    <Department Name=""Dev"">
      <Employees>
        <Person Name=""Brendon"" Surename=""Page"" />
        <Person Name=""Jayd"" Surename=""Page"" />
      </Employees>
    </Department>
    <Department Name=""Accounts"">
      <Employees>
        <Person Name=""Bob"" Surename=""Soap"" />
        <Person Name=""Joe"" Surename=""Pants"" />
      </Employees>
    </Department>
  </Departments>
  <InlineRecordSet>
        RandomData
    </InlineRecordSet>
  <InlineRecordSet>
        RandomData1
    </InlineRecordSet>
  <OuterNestedRecordSet>
    <InnerNestedRecordSet ItemValue=""val1"" />
    <InnerNestedRecordSet ItemValue=""val2"" />
  </OuterNestedRecordSet>
  <OuterNestedRecordSet>
    <InnerNestedRecordSet ItemValue=""val3"" />
    <InnerNestedRecordSet ItemValue=""val4"" />
  </OuterNestedRecordSet>
</Company>" };

                Assert.AreEqual(expected.FirstOrDefault().ToString(), actual.FirstOrDefault().ToString());
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValueUsingScalarPathFromXmlWithASingleNode_WherePathMapsToANode_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath("Message", "Message");

            using (var xmlNavigator = new XmlNavigator(GivenSingleNode))
            {
                var actual = xmlNavigator.SelectEnumerable(namePath);
                const string expected = "Dummy Data";

                Assert.IsTrue(actual.Contains(expected));
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsToANode_Expected_EnumerableValue()
        {
            IPath path = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
                const string expected = "RandomData|RandomData1";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsToAnAttribute_Expected_EnumerableValue()
        {
            IPath path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
                const string expected = "Dev|Accounts";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesUsingScalarPathFromXml_WherePathMapsToANode_Expected_EnumerableValue()
        {
            IPath path = new XmlPath("Company.Motto", "Company.Motto");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
                const string expected = "Eat lots of cake";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesUsingScalarPathFromXml_WherePathMapsToAnAttribute_Expected_EnumerableValue()
        {
            IPath path = new XmlPath("Company:Name", "Company:Name");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
                const string expected = nameof(Dev2);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsThroughNestedEnumerablesScenario1_Expected_EnumerableValue()
        {
            IPath path = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
                const string expected = "Brendon|Jayd|Bob|Joe";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsThroughNestedEnumerablesScenario2_Expected_EnumerableValue()
        {
            IPath path = new XmlPath("Company().OuterNestedRecordSet().InnerNestedRecordSet:ItemValue", "Company.OuterNestedRecordSet.InnerNestedRecordSet:ItemValue");

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
                const string expected = "val1|val2|val3|val4";

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesAsRelatedUsingScalarPathFromXmlWithASingleNode_WherePathMapsToANode_Expected_ScalarValue()
        {
            IPath namePath = new XmlPath("Message", "Message");
            IList<IPath> paths = new List<IPath>
            {
                namePath
            };

            using (var xmlNavigator = new XmlNavigator(GivenSingleNode))
            {
                var actual = xmlNavigator.SelectEnumerablesAsRelated(paths);
                const string expected = "Dummy Data";

                Assert.IsTrue(actual[namePath].Contains(expected));
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainAScalarPath_Expected_FlattenedDataWithValueFromScalarPathRepeatingForEachEnumeration()
        {
            IPath path = new XmlPath("Company:Name", "Company:Name");
            IPath path1 = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");
            var paths = new List<IPath> { path, path1 };

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var data = xmlNavigator.SelectEnumerablesAsRelated(paths);

                const string expected = "Dev2|Dev2^RandomData|RandomData1";
                var actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainUnrelatedEnumerablePaths_Expected_FlattenedDataWithValuesFromUnrelatedEnumerablePathsAtMatchingIndexes()
        {
            IPath path = new XmlPath("Company().OuterNestedRecordSet().InnerNestedRecordSet:ItemValue", "Company.OuterNestedRecordSet.InnerNestedRecordSet:ItemValue");
            IPath path1 = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");
            var paths = new List<IPath> { path, path1 };

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var data = xmlNavigator.SelectEnumerablesAsRelated(paths);

                const string expected = "val1|val2|val3|val4^RandomData|RandomData1||";
                var actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainNestedEnumerablePaths_Expected_FlattenedDataWithValuesFromOuterEnumerablePathRepeatingForEveryValueFromNestedEnumerablePath()
        {
            IPath path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");
            IPath path1 = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");
            var paths = new List<IPath> { path, path1 };

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var data = xmlNavigator.SelectEnumerablesAsRelated(paths);

                const string expected = "Dev|Dev|Accounts|Accounts^Brendon|Jayd|Bob|Joe";
                var actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainASinglePathWhichIsEnumerable_Expected_FlattenedDataWithValuesFromEnumerablePath()
        {
            IPath path = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");
            var paths = new List<IPath> { path };

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var data = xmlNavigator.SelectEnumerablesAsRelated(paths);

                const string expected = "Brendon|Jayd|Bob|Joe";
                var actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainASinglePathWhichIsScalar_Expected_FlattenedDataWithValueFromScalarPath()
        {
            IPath path = new XmlPath("Company:Name", "Company:Name");
            var paths = new List<IPath> { path };

            using (var xmlNavigator = new XmlNavigator(TestData))
            {
                var data = xmlNavigator.SelectEnumerablesAsRelated(paths);

                const string expected = nameof(Dev2);
                var actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory(nameof(XmlNavigator))]
        public void XmlNavigator_SelectEnumerablesAsRelated_WithSeperatorSymbol_Expected_UnchangedPath()
        {
            var namePath = new List<IPath> { new XmlPath(".", ".") };

            using (var XmlNavigator = new XmlNavigator(TestData))
            {
                var actual = string.Join("|", XmlNavigator.SelectEnumerablesAsRelated(namePath).Values.FirstOrDefault());

                const string expected = @"<Company Name=""Dev2"">
  <Motto>Eat lots of cake</Motto>
  <PreviousMotto />
  <Departments TestAttrib=""testing"">
    <Department Name=""Dev"">
      <Employees>
        <Person Name=""Brendon"" Surename=""Page"" />
        <Person Name=""Jayd"" Surename=""Page"" />
      </Employees>
    </Department>
    <Department Name=""Accounts"">
      <Employees>
        <Person Name=""Bob"" Surename=""Soap"" />
        <Person Name=""Joe"" Surename=""Pants"" />
      </Employees>
    </Department>
  </Departments>
  <InlineRecordSet>
        RandomData
    </InlineRecordSet>
  <InlineRecordSet>
        RandomData1
    </InlineRecordSet>
  <OuterNestedRecordSet>
    <InnerNestedRecordSet ItemValue=""val1"" />
    <InnerNestedRecordSet ItemValue=""val2"" />
  </OuterNestedRecordSet>
  <OuterNestedRecordSet>
    <InnerNestedRecordSet ItemValue=""val3"" />
    <InnerNestedRecordSet ItemValue=""val4"" />
  </OuterNestedRecordSet>
</Company>";

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
