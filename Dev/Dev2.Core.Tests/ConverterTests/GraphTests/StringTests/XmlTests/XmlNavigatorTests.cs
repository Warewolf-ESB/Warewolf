
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.XmlTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class XmlNavigatorTests
    {
        internal string Given()
        {
            return @"<Company Name='Dev2'>
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
        }

        internal string GivenSingleNode()
        {
            return @"<Message>Dummy Data</Message>";
        }

        #region SelectScalar Tests

        /// <summary>
        /// Select scalar value using scalar path from XML where path maps to an attribute expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromXml_WherePathMapsToAnAttribute_Expected_ScalarValue()
        {
            string testData = Given();

            IPath namePath = new XmlPath("Company:Name", "Company:Name");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = xmlNavigator.SelectScalar(namePath).ToString();
            string expected = "Dev2";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select scalar value using scalar path from XML where path maps to a node expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromXml_WherePathMapsToANode_Expected_ScalarValue()
        {
            string testData = Given();

            IPath namePath = new XmlPath("Company.Motto", "Company.Motto");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = xmlNavigator.SelectScalar(namePath).ToString();
            string expected = "Eat lots of cake";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select scalar value using scalar path from XML with a single node where path maps to a 
        /// node expected scalar value.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromXmlWithASingleNode_WherePathMapsToANode_Expected_ScalarValue()
        {
            string testData = GivenSingleNode();

            IPath namePath = new XmlPath("Message", "Message");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = xmlNavigator.SelectScalar(namePath).ToString();
            string expected = "Dummy Data";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select scalar value using enumerable path from XML where path maps to an attribute expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromXml_WherePathMapsToAnAttribute_Expected_ScalarValue()
        {
            string testData = Given();

            IPath namePath = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = xmlNavigator.SelectScalar(namePath).ToString();
            string expected = "Joe";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select scalar value using enumerable path from XML where path maps to a node expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromXml_WherePathMapsToANode_Expected_ScalarValue()
        {
            string testData = Given();

            IPath namePath = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = xmlNavigator.SelectScalar(namePath).ToString().Trim();
            string expected = "RandomData1";

            Assert.AreEqual(expected, actual);
        }


        #endregion SelectScalar Tests


        #region SelectEnumerable Tests

        /// <summary>
        /// Select enumerable value using scalar path from XML with a single node where path maps 
        /// to a node expected_ scalar value.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValueUsingScalarPathFromXmlWithASingleNode_WherePathMapsToANode_Expected_ScalarValue()
        {
            string testData = GivenSingleNode();

            IPath namePath = new XmlPath("Message", "Message");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            IEnumerable<object> actual = xmlNavigator.SelectEnumerable(namePath);
            string expected = "Dummy Data";

            Assert.IsTrue(actual.Contains(expected));
        }

        /// <summary>
        /// Select enumerable values using enumerable path from XML where path maps to a node
        /// expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsToANode_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "RandomData|RandomData1";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using enumerable path from XML where path maps to an attribute 
        /// expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsToAnAttribute_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "Dev|Accounts";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using scalar path from XML where path maps to a node 
        /// expected enumerable value.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromXml_WherePathMapsToANode_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new XmlPath("Company.Motto", "Company.Motto");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "Eat lots of cake";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using scalar path from XML where path maps to an attribute 
        /// expected enumerable value.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromXml_WherePathMapsToAnAttribute_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new XmlPath("Company:Name", "Company:Name");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "Dev2";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using enumerable path from XML where path maps through nested enumerables 
        /// first scenario expected enumerable value.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsThroughNestedEnumerablesScenario1_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "Brendon|Jayd|Bob|Joe";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using enumerable path from XML where path maps through nested 
        /// enumerables second expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromXml_WherePathMapsThroughNestedEnumerablesScenario2_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new XmlPath("Company().OuterNestedRecordSet().InnerNestedRecordSet:ItemValue", "Company.OuterNestedRecordSet.InnerNestedRecordSet:ItemValue");

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            string actual = string.Join("|", xmlNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "val1|val2|val3|val4";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related using scalar path from XML with a single node where path maps to 
        /// a node expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingScalarPathFromXmlWithASingleNode_WherePathMapsToANode_Expected_ScalarValue()
        {
            string testData = GivenSingleNode();

            IPath namePath = new XmlPath("Message", "Message");
            IList<IPath> paths = new List<IPath>();
            paths.Add(namePath);


            XmlNavigator xmlNavigator = new XmlNavigator(testData);
            var actual = xmlNavigator.SelectEnumerablesAsRelated(paths);
            string expected = "Dummy Data";

            Assert.IsTrue(actual[namePath].Contains(expected));
        }

        /// <summary>
        /// Select enumerable values as related using enumerable path from XML where paths contain a scalar path 
        /// expected flattened data with value from scalar path repeating for each enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainAScalarPath_Expected_FlattenedDataWithValueFromScalarPathRepeatingForEachEnumeration()
        {
            string testData = Given();

            IPath path = new XmlPath("Company:Name", "Company:Name");
            IPath path1 = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");
            List<IPath> paths = new List<IPath> { path, path1 };

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            Dictionary<IPath, IList<object>> data = xmlNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Dev2|Dev2^RandomData|RandomData1";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related using enumerable path from XML where paths contain unrelated 
        /// enumerable paths expected flattened data with values from unrelated enumerable paths at matching indexes.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainUnrelatedEnumerablePaths_Expected_FlattenedDataWithValuesFromUnrelatedEnumerablePathsAtMatchingIndexes()
        {
            string testData = Given();

            IPath path = new XmlPath("Company().OuterNestedRecordSet().InnerNestedRecordSet:ItemValue", "Company.OuterNestedRecordSet.InnerNestedRecordSet:ItemValue");
            IPath path1 = new XmlPath("Company().InlineRecordSet", "Company.InlineRecordSet");
            List<IPath> paths = new List<IPath> { path, path1 };

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            Dictionary<IPath, IList<object>> data = xmlNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "val1|val2|val3|val4^RandomData|RandomData1||";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related using enumerable path from XML where paths contain nested enumerable 
        /// paths expected flattened data with values from outer enumerable path repeating for every value from 
        /// nested enumerable path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainNestedEnumerablePaths_Expected_FlattenedDataWithValuesFromOuterEnumerablePathRepeatingForEveryValueFromNestedEnumerablePath()
        {
            string testData = Given();

            IPath path = new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name");
            IPath path1 = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");
            List<IPath> paths = new List<IPath> { path, path1 };

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            Dictionary<IPath, IList<object>> data = xmlNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Dev|Dev|Accounts|Accounts^Brendon|Jayd|Bob|Joe";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related using enumerable path from XML where paths contain a single path 
        /// which is enumerable expected flattened data with values from enumerable path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainASinglePathWhichIsEnumerable_Expected_FlattenedDataWithValuesFromEnumerablePath()
        {
            string testData = Given();

            IPath path = new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name");
            List<IPath> paths = new List<IPath> { path };

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            Dictionary<IPath, IList<object>> data = xmlNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Brendon|Jayd|Bob|Joe";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related using enumerable path from XML where paths contain a single 
        /// path which is scalar expected flattened data with value from scalar path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromXml_Where_PathsContainASinglePathWhichIsScalar_Expected_FlattenedDataWithValueFromScalarPath()
        {
            string testData = Given();

            IPath path = new XmlPath("Company:Name", "Company:Name");
            List<IPath> paths = new List<IPath> { path };

            XmlNavigator xmlNavigator = new XmlNavigator(testData);

            Dictionary<IPath, IList<object>> data = xmlNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Dev2";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectEnumerable Tests
    }
}
