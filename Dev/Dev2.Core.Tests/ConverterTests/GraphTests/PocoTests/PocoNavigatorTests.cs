using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.UnitTest.Framework.ConverterTests.GraphTests;

namespace Dev2.Tests.ConverterTests.GraphTests.PocoTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PocoNavigatorTests
    {
        internal PocoTestData Given()
        {
            PocoTestData testData = new PocoTestData()
            {
                Name = "Brendon",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherBrendon",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData1 = new PocoTestData()
            {
                Name = "Mo",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherMo",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData2 = new PocoTestData()
            {
                Name = "Trav",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherTrav",
                    Age = 31,
                },
            };

            testData.EnumerableData = new List<PocoTestData> { nestedTestData1, nestedTestData2 };

            return testData;
        }

        internal PocoTestData GivenWithNoEnumerableData()
        {
            PocoTestData testData = new PocoTestData()
            {
                Name = "Brendon",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherBrendon",
                    Age = 31,
                },
            };

            return testData;
        }

        internal PocoTestData GivenWithParallelAndNestedEnumerables()
        {
            PocoTestData testData = new PocoTestData()
            {
                Name = "Brendon",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherBrendon",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData1 = new PocoTestData()
            {
                Name = "Mo",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherMo",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData2 = new PocoTestData()
            {
                Name = "Trav",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherTrav",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData3 = new PocoTestData()
            {
                Name = "Jayd",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherJayd",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData4 = new PocoTestData()
            {
                Name = "Dan",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherDan",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData5 = new PocoTestData()
            {
                Name = "Mark",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherMark",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData6 = new PocoTestData()
            {
                Name = "Warren",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherWarren",
                    Age = 31,
                },
            };

            PocoTestData nestedTestData7 = new PocoTestData()
            {
                Name = "Wallis",
                Age = 30,
                NestedData = new PocoTestData()
                {
                    Name = "AnotherWallis",
                    Age = 31,
                },
            };

            nestedTestData1.EnumerableData = new List<PocoTestData> { nestedTestData3, nestedTestData4, nestedTestData6 };
            nestedTestData2.EnumerableData = new List<PocoTestData> { nestedTestData5 };

            testData.EnumerableData = new List<PocoTestData> { nestedTestData1, nestedTestData2 };
            testData.EnumerableData1 = new List<PocoTestData> { nestedTestData3, nestedTestData4, nestedTestData5, nestedTestData6, nestedTestData7 };

            return testData;
        }

        [TestMethod]
        public void SelectScalarValueUsingRootPathFromPrimitive_Expected_ScalarValue()
        {
            PocoTestData testData = Given();

            IPath path = new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol);

            PocoNavigator pocoNavigator = new PocoNavigator(1);

            object data = pocoNavigator.SelectScalar(path);

            Assert.AreEqual(data, "1");
        }

        /// <summary>
        /// Select enumerable values using root path from primitive expected single value in enumeration returned.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingRootPathFromPrimitive_Expected_SingleValueInEnumeration()
        {
            PocoTestData testData = Given();

            IPath path = new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol);

            PocoNavigator pocoNavigator = new PocoNavigator(1);

            IEnumerable<object> data = pocoNavigator.SelectEnumerable(path);

            string expected = "1";
            string actual = string.Join("", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Selects the enumerable values as related using root path from primitive_ expected_ single value in enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingRootPathFromPrimitive_Expected_SingleValueInEnumeration()
        {
            PocoTestData testData = Given();

            IPath path = new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol);
            List<IPath> paths = new List<IPath> { path };

            PocoNavigator pocoNavigator = new PocoNavigator(1);

            Dictionary<IPath, IList<object>> data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            string expected = "1";
            string actual = string.Join("|", data[path]);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select scalar value using root path from enumerable containing only primitives expected last scalar value 
        /// in enumeration.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingRootPathFromEnumerableContainingOnlyPrimitives_Expected_LastScalarValueInEnumeration()
        {
            List<int> testData = new List<int> { 1, 2, 3 };

            IPath namePath = new PocoPath("().", "().");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            string expected = "3";
            string actual = pocoNavigator.SelectScalar(namePath).ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using root path from enumerable containing only primitives expected values for 
        /// each value in enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingRootPathFromEnumerableContainingOnlyPrimitives_Expected_ValuesForEachValueInEnumeration()
        {
            List<int> testData = new List<int> { 1, 2, 3 };

            IPath path = new PocoPath("().", "().");
            List<IPath> paths = new List<IPath> { path };

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            Dictionary<IPath, IList<object>> data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            string expected = string.Join("|", testData.Select(e => e));
            string actual = string.Join("|", data[path]);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related using root path from enumerable containing only primitives 
        /// expected values for each value in enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingRootPathFromEnumerableContainingOnlyPrimitives_Expected_ValuesForEachValueInEnumeration()
        {
            List<int> testData = new List<int> { 1, 2, 3 };

            IPath path = new PocoPath("().", "().");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            IEnumerable<object> data = pocoNavigator.SelectEnumerable(path);

            string expected = string.Join("|", testData.Select(e => e));
            string actual = string.Join("|", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select scalar value using scalar path from enumerable expected scalar value returned
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromEnumerable_Expected_ScalarValue()
        {
            PocoTestData testData = Given();

            IPath path = new PocoPath("EnumerableData.Count", "EnumerableData.Count");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            object data = pocoNavigator.SelectScalar(path);

            Assert.AreEqual(data, testData.EnumerableData.Count);
        }

        /// <summary>
        /// Select scalar value using scalar path from reference type expected scalar value returned.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromReferenceType_Expected_ScalarValue()
        {
            PocoTestData testData = Given();

            IPath namePath = new PocoPath("Name", "Name");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            object data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, testData.Name);
        }

        /// <summary>
        /// Select scalar value using enumerable path from reference type expected scalar 
        /// value from last item in enumerable collection.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromReferenceType_Expected_ScalarValueFromLastItemInEnumerableCollection()
        {
            PocoTestData testData = Given();

            IPath namePath = new PocoPath("EnumerableData().NestedData.Name", "EnumerableData.NestedData.Name");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            object data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, testData.EnumerableData.ElementAt(testData.EnumerableData.Count - 1).NestedData.Name);
        }

        /// <summary>
        /// Select scalar value using enumerable path from reference type where enumerable data is null expected null.
        /// </summary>
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromReferenceType_Where_EnumerableDataIsNull_Expected_Null()
        {
            PocoTestData testData = GivenWithNoEnumerableData();

            IPath namePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            object data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, null);
        }

        /// <summary>
        /// Select enumerable values using enumerable path from reference type expected values from each 
        /// item in enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromReferenceType_Expected_ValuesFromEachItemInEnumeration()
        {
            PocoTestData testData = Given();

            IPath namePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            IEnumerable<object> data = pocoNavigator.SelectEnumerable(namePath);

            string expected = string.Join("|", testData.EnumerableData.Select(e => e.Name));
            string actual = string.Join("|", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using enumerable path from reference type where enumerable data 
        /// is null expected null.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromReferenceType_Where_EnumerableDataIsNull_Expected_Null()
        {
            PocoTestData testData = GivenWithNoEnumerableData();

            IPath namePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            object data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, null);
        }

        /// <summary>
        /// Select enumerable values using scalar path from enumerable expected single value in enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromEnumerable_Expected_SingleValueInEnumeration()
        {
            PocoTestData testData = Given();

            IPath namePath = new PocoPath("EnumerableData.Count", "EnumerableData.Count");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            IEnumerable<object> data = pocoNavigator.SelectEnumerable(namePath);

            string expected = testData.EnumerableData.Count.ToString();
            string actual = string.Join("", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values using scalar path from reference type expected single value in enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromReferenceType_Expected_SingleValueInEnumeration()
        {
            PocoTestData testData = Given();

            IPath namePath = new PocoPath("NestedData.Name", "NestedData.Name");

            PocoNavigator pocoNavigator = new PocoNavigator(testData);

            IEnumerable<object> data = pocoNavigator.SelectEnumerable(namePath);

            string expected = testData.NestedData.Name;
            string actual = string.Join("", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related from reference type where paths contain a scalar path 
        /// expected flattened data with value from scalar path repeating for each enumeration.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainAScalarPath_Expected_FlattenedDataWithValueFromScalarPathRepeatingForEachEnumeration()
        {
            PocoTestData testData = GivenWithParallelAndNestedEnumerables();

            PocoPath namePath = new PocoPath("Name", "Name");
            PocoPath enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            List<IPath> paths = new List<IPath> { namePath, enumerableNamePath };

            PocoNavigator pocoNavigator = new PocoNavigator(testData);
            Dictionary<IPath, IList<object>> data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            string expected = string.Join("|", testData.EnumerableData.Select(e => testData.Name));
            string actual = string.Join("|", data[namePath]);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related from reference type where paths contain unrelated enumerable 
        /// paths expected flattened data with values from unrelated enumerable paths at matching indexes.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainUnrelatedEnumerablePaths_Expected_FlattenedDataWithValuesFromUnrelatedEnumerablePathsAtMatchingIndexes()
        {
            PocoTestData testData = GivenWithParallelAndNestedEnumerables();

            PocoPath enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            PocoPath parallelEnumerableNamePath = new PocoPath("EnumerableData1().Name", "EnumerableData1.Name");
            //PocoPath nestedEnumerableNamePath = new PocoPath("EnumerableData().EnumerableData().Name", "EnumerableData.EnumerableData.Name");
            List<IPath> paths = new List<IPath> { enumerableNamePath, parallelEnumerableNamePath };

            PocoNavigator pocoNavigator = new PocoNavigator(testData);
            Dictionary<IPath, IList<object>> data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            int maxCount = Math.Max(testData.EnumerableData.Count, testData.EnumerableData1.Count);

            #region Complex Setup for Expected

            //
            // The code in this region is used to setup the exprected value.
            // It can't be reused for other tests and can't be made generic
            // without replicating the funcationality being tested.
            //
            string tmpExpected = "";
            string tmpExpected1 = "";
            string separator = "|";
            for (int i = 0; i < maxCount; i++)
            {
                if (i == maxCount - 1) separator = "";

                if (i < testData.EnumerableData.Count)
                {
                    tmpExpected += testData.EnumerableData[i].Name + separator;
                }
                else
                {
                    tmpExpected += separator;
                }

                if (i < testData.EnumerableData1.Count)
                {
                    tmpExpected1 += testData.EnumerableData1[i].Name + separator;
                }
                else
                {
                    tmpExpected1 += separator;
                }
            }

            #endregion Complex Setup for Expected

            string expected = tmpExpected + "^" + tmpExpected1;
            string actual = string.Join("|", data[enumerableNamePath]);
            actual += "^" + string.Join("|", data[parallelEnumerableNamePath]);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related from reference type where paths contain nested enumerable 
        /// paths expected flattened data with values from outer enumerable path repeating for every value 
        /// from nested enumerable path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainNestedEnumerablePaths_Expected_FlattenedDataWithValuesFromOuterEnumerablePathRepeatingForEveryValueFromNestedEnumerablePath()
        {
            PocoTestData testData = GivenWithParallelAndNestedEnumerables();

            PocoPath enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            PocoPath nestedEnumerableNamePath = new PocoPath("EnumerableData().EnumerableData().Name", "EnumerableData.EnumerableData.Name");
            List<IPath> paths = new List<IPath> { enumerableNamePath, nestedEnumerableNamePath };

            PocoNavigator pocoNavigator = new PocoNavigator(testData);
            Dictionary<IPath, IList<object>> data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            #region Complex Setup for Expected

            //
            // The code in this region is used to setup the exprected value.
            // It can't be reused for other tests and can't be made generic
            // without replicating the funcationality being tested.
            //
            string tmpExpected = "";
            string tmpExpected1 = "";
            string separator = "|";

            for (int outerCount = 0; outerCount < testData.EnumerableData.Count; outerCount++)
            {
                for (int innerCount = 0; innerCount < testData.EnumerableData[outerCount].EnumerableData.Count; innerCount++)
                {
                    if ((outerCount == testData.EnumerableData.Count - 1) && (innerCount == testData.EnumerableData[outerCount].EnumerableData.Count - 1)) separator = "";
                    if (outerCount < testData.EnumerableData.Count)
                    {
                        tmpExpected += testData.EnumerableData[outerCount].Name + separator;
                    }
                    else
                    {
                        tmpExpected += separator;
                    }

                    if (innerCount < testData.EnumerableData[outerCount].EnumerableData.Count)
                    {
                        tmpExpected1 += testData.EnumerableData[outerCount].EnumerableData[innerCount].Name + separator;
                    }
                    else
                    {
                        tmpExpected1 += separator;
                    }
                }
            }

            #endregion Complex Setup for Expected

            string expected = tmpExpected + "^" + tmpExpected1;
            string actual = string.Join("|", data[enumerableNamePath]);
            actual += "^" + string.Join("|", data[nestedEnumerableNamePath]);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related from reference type where paths contain a single path which is 
        /// enumerable expected flattened data with values from enumerable path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainASinglePathWhichIsEnumerable_Expected_FlattenedDataWithValuesFromEnumerablePath()
        {
            PocoTestData testData = GivenWithParallelAndNestedEnumerables();

            PocoPath enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            List<IPath> paths = new List<IPath> { enumerableNamePath };

            PocoNavigator pocoNavigator = new PocoNavigator(testData);
            Dictionary<IPath, IList<object>> data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            string expected = string.Join("|", testData.EnumerableData.Select(e => e.Name));
            string actual = string.Join("|", data[enumerableNamePath]);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Select enumerable values as related from reference type where paths contain a single path 
        /// which is scalar expected flattened data with value from scalar path.
        /// </summary>
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainASinglePathWhichIsScalar_Expected_FlattenedDataWithValueFromScalarPath()
        {
            PocoTestData testData = GivenWithParallelAndNestedEnumerables();

            PocoPath namePath = new PocoPath("Name", "Name");
            List<IPath> paths = new List<IPath> { namePath };

            PocoNavigator pocoNavigator = new PocoNavigator(testData);
            Dictionary<IPath, IList<object>> data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            string expected = testData.Name;
            string actual = string.Join("|", data[namePath]);

            Assert.AreEqual(expected, actual);
        }
    }
}
