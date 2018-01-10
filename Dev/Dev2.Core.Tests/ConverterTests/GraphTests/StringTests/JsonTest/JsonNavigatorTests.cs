/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Json;


namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.JsonTest {
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonNavigatorTests {

        internal string Given()
        {
            return @"{
    ""Name"": ""Dev2"",
    ""Motto"": ""Eat lots of cake"",
    ""Departments"": [      
        {
          ""Name"": ""Dev"",
          ""Employees"": [
              {
                ""Name"": ""Brendon"",
                ""Surename"": ""Page""
              },
              {
                ""Name"": ""Jayd"",
                ""Surename"": ""Page""
              }
            ]
        },
        {
          ""Name"": ""Accounts"",
          ""Employees"": [
              {
                ""Name"": ""Bob"",
                ""Surename"": ""Soap""
              },
              {
                ""Name"": ""Joe"",
                ""Surename"": ""Pants""
              }
            ]
        }
      ],
    ""Contractors"": [      
        {
          ""Name"": ""Roofs Inc."",
          ""PhoneNumber"": ""123"",
        },
        {
          ""Name"": ""Glass Inc."",
          ""PhoneNumber"": ""1234"",
        },
        {
          ""Name"": ""Doors Inc."",
          ""PhoneNumber"": ""1235"",
        },
        {
          ""Name"": ""Cakes Inc."",
          ""PhoneNumber"": ""1236"",
        }
      ],
    ""PrimitiveRecordset"": [
      ""
        RandomData
    "",
      ""
        RandomData1
    ""
    ],
  }";
        }


        #region SelectScalar Tests

        /// <summary>
        /// Selects the scalar value using scalar path from json expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromJson_Expected_ScalarValue()
        {
            var testData = Given();

            IPath namePath = new JsonPath("Name", "Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString();
            const string expected = "Dev2";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the scalar value using enumerable path from json expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromJson_Expected_ScalarValue()
        {
            var testData = Given();

            IPath namePath = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString();
            const string expected = "Joe";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the scalar value using enumerable path from json where path maps to primitive enumerable
        /// expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromJson_WherePathMapsToPrimitiveEnumerable_Expected_ScalarValue()
        {
            var testData = Given();

            IPath namePath = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = JsonNavigator.SelectScalar(namePath).ToString().Trim();
            const string expected = "RandomData1";

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectScalar Tests

        #region SelectEnumerable Tests

        /// <summary>
        /// Selects the enumerable values using enumerable path from json where path maps to primitive enumerable
        /// expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_WherePathMapsToPrimitiveEnumerable_Expected_EnumerableValue()
        {
            var testData = Given();

            IPath path = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "RandomData|RandomData1";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the enumerable values using enumerable path from json expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_Expected_EnumerableValue()
        {
            var testData = Given();

            IPath path = new JsonPath("Departments().Name", "Departments.Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "Dev|Accounts";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the enumerable values using scalar path from json expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromJson_Expected_EnumerableValue()
        {
            var testData = Given();

            IPath path = new JsonPath("Motto", "Motto");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "Eat lots of cake";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the enumerable values using enumerable path from json
        /// where path maps through nested enumerables expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_WherePathMapsThroughNestedEnumerables_Expected_EnumerableValue()
        {
            var testData = Given();

            IPath path = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");

            var JsonNavigator = new JsonNavigator(testData);

            var actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            const string expected = "Brendon|Jayd|Bob|Joe";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths contain a scalar path 
        /// expected flattened data with value from scalar path repeating for each enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainAScalarPath_Expected_FlattenedDataWithValueFromScalarPathRepeatingForEachEnumeration()
        {
            var testData = Given();

            IPath path = new JsonPath("Name", "Name");
            IPath path1 = new JsonPath("Departments().Name", "Departments.Name");
            var paths = new List<IPath> { path, path1 };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Dev2|Dev2^Dev|Accounts";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths contain 
        /// unrelated enumerable paths expected flattened data with values from unrelated enumerable 
        /// paths at matching indexes.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainUnrelatedEnumerablePaths_Expected_FlattenedDataWithValuesFromUnrelatedEnumerablePathsAtMatchingIndexes()
        {
            var testData = Given();

            IPath path = new JsonPath("Departments().Name", "Departments.Name");
            IPath path1 = new JsonPath("Contractors().Name", "Contractors.Name");
            var paths = new List<IPath> { path, path1 };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Dev|Accounts||^Roofs Inc.|Glass Inc.|Doors Inc.|Cakes Inc.";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths 
        /// contain nested enumerable paths expected flattened data with values from 
        /// outer enumerable path repeating for every value from nested enumerable path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainNestedEnumerablePaths_Expected_FlattenedDataWithValuesFromOuterEnumerablePathRepeatingForEveryValueFromNestedEnumerablePath()
        {
            var testData = Given();

            IPath path3 = new JsonPath("Name", "Name");
            IPath path = new JsonPath("Departments().Name", "Departments.Name");
            IPath path1 = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");
            IPath path2 = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");
            var paths = new List<IPath> { path3, path, path1, path2 };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            var expected = "Dev2|Dev2|Dev2|Dev2^Dev|Dev|Accounts|Accounts^Brendon|Jayd|Bob|Joe^RandomData\r\n    ,\r\n        RandomData1|||";
            var actual = string.Join("|", data[path3].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path2].Select(s => s.ToString().Trim()));
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }
        void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }
        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths contain a single 
        /// path which is enumerable expected flattened data with values from enumerable path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainASinglePathWhichIsEnumerable_Expected_FlattenedDataWithValuesFromEnumerablePath()
        {
            var testData = Given();

            IPath path = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");
            var paths = new List<IPath> { path };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Brendon|Jayd|Bob|Joe";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths contain a single 
        /// path which is scalar expected flattened data with value from scalar path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainASinglePathWhichIsScalar_Expected_FlattenedDataWithValueFromScalarPath()
        {
            var testData = Given();

            IPath path = new JsonPath("Name", "Name");
            var paths = new List<IPath> { path };

            var JsonNavigator = new JsonNavigator(testData);

            var data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "Dev2";
            var actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectEnumerable Tests
    }
}
