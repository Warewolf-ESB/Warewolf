using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.String.Json;

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.JsonTest {
    [TestClass]    
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

        internal string GivenPrimitiveRecordset()
        {
            return @"[
      ""RandomData"",
      ""RandomData1""]";
        }

        #region SelectScalar Tests

        /// <summary>
        /// Selects the scalar value using scalar path from json expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromJson_Expected_ScalarValue()
        {
            string testData = Given();

            IPath namePath = new JsonPath("Name", "Name");

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            string actual = JsonNavigator.SelectScalar(namePath).ToString();
            string expected = "Dev2";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the scalar value using enumerable path from json expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromJson_Expected_ScalarValue()
        {
            string testData = Given();

            IPath namePath = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            string actual = JsonNavigator.SelectScalar(namePath).ToString();
            string expected = "Joe";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the scalar value using enumerable path from json where path maps to primitive enumerable
        /// expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromJson_WherePathMapsToPrimitiveEnumerable_Expected_ScalarValue()
        {
            string testData = Given();

            IPath namePath = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            string actual = JsonNavigator.SelectScalar(namePath).ToString().Trim();
            string expected = "RandomData1";

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
            string testData = Given();

            IPath path = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            string actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "RandomData|RandomData1";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the enumerable values using enumerable path from json expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new JsonPath("Departments().Name", "Departments.Name");

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            string actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "Dev|Accounts";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the enumerable values using scalar path from json expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromJson_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new JsonPath("Motto", "Motto");

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            string actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "Eat lots of cake";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the enumerable values using enumerable path from json
        /// where path maps through nested enumerables expected enumerable value returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromJson_WherePathMapsThroughNestedEnumerables_Expected_EnumerableValue()
        {
            string testData = Given();

            IPath path = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            string actual = string.Join("|", JsonNavigator.SelectEnumerable(path).Select(o => o.ToString().Trim()));
            string expected = "Brendon|Jayd|Bob|Joe";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths contain a scalar path 
        /// expected flattened data with value from scalar path repeating for each enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainAScalarPath_Expected_FlattenedDataWithValueFromScalarPathRepeatingForEachEnumeration()
        {
            string testData = Given();

            IPath path = new JsonPath("Name", "Name");
            IPath path1 = new JsonPath("Departments().Name", "Departments.Name");
            List<IPath> paths = new List<IPath> { path, path1 };

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            Dictionary<IPath, IList<object>> data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Dev2|Dev2^Dev|Accounts";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

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
            string testData = Given();

            IPath path = new JsonPath("Departments().Name", "Departments.Name");
            IPath path1 = new JsonPath("Contractors().Name", "Contractors.Name");
            List<IPath> paths = new List<IPath> { path, path1 };

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            Dictionary<IPath, IList<object>> data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Dev|Accounts||^Roofs Inc.|Glass Inc.|Doors Inc.|Cakes Inc.";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim()));

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
            string testData = Given();

            IPath path3 = new JsonPath("Name", "Name");
            IPath path = new JsonPath("Departments().Name", "Departments.Name");
            IPath path1 = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");
            IPath path2 = new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset");
            List<IPath> paths = new List<IPath> { path3, path, path1, path2 };

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            Dictionary<IPath, IList<object>> data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Dev2|Dev2|Dev2|Dev2^Dev|Dev|Accounts|Accounts^Brendon|Jayd|Bob|Joe^RandomData\r\n    ,\r\n        RandomData1|||";
            string actual = string.Join("|", data[path3].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path1].Select(s => s.ToString().Trim())) + "^" + string.Join("|", data[path2].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths contain a single 
        /// path which is enumerable expected flattened data with values from enumerable path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainASinglePathWhichIsEnumerable_Expected_FlattenedDataWithValuesFromEnumerablePath()
        {
            string testData = Given();

            IPath path = new JsonPath("Departments().Employees().Name", "Departments.Employees.Name");
            List<IPath> paths = new List<IPath> { path };

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            Dictionary<IPath, IList<object>> data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Brendon|Jayd|Bob|Joe";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects enumerable values as related using enumerable path from json where paths contain a single 
        /// path which is scalar expected flattened data with value from scalar path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingEnumerablePathFromJson_Where_PathsContainASinglePathWhichIsScalar_Expected_FlattenedDataWithValueFromScalarPath()
        {
            string testData = Given();

            IPath path = new JsonPath("Name", "Name");
            List<IPath> paths = new List<IPath> { path };

            JsonNavigator JsonNavigator = new JsonNavigator(testData);

            Dictionary<IPath, IList<object>> data = JsonNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "Dev2";
            string actual = string.Join("|", data[path].Select(s => s.ToString().Trim()));

            Assert.AreEqual(expected, actual);
        }

        #endregion SelectEnumerable Tests
    }
}
