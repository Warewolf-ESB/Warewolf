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
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.UnitTest.Framework.ConverterTests.GraphTests;


namespace Dev2.Tests.ConverterTests.GraphTests.PocoTests
{
    [TestClass]
    public class PocoNavigatorTests
    {
        internal PocoTestData Given()
        {
            var testData = new PocoTestData
            {
                Name = "Brendon",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherBrendon",
                    Age = 31,
                },
            };

            var nestedTestData1 = new PocoTestData
            {
                Name = "Mo",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherMo",
                    Age = 31,
                },
            };

            var nestedTestData2 = new PocoTestData
            {
                Name = "Trav",
                Age = 30,
                NestedData = new PocoTestData
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
            var testData = new PocoTestData
            {
                Name = "Brendon",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherBrendon",
                    Age = 31,
                },
            };

            return testData;
        }

        internal PocoTestData GivenWithParallelAndNestedEnumerables()
        {
            var testData = new PocoTestData
            {
                Name = "Brendon",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherBrendon",
                    Age = 31,
                },
            };

            var nestedTestData1 = new PocoTestData
            {
                Name = "Mo",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherMo",
                    Age = 31,
                },
            };

            var nestedTestData2 = new PocoTestData
            {
                Name = "Trav",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherTrav",
                    Age = 31,
                },
            };

            var nestedTestData3 = new PocoTestData
            {
                Name = "Jayd",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherJayd",
                    Age = 31,
                },
            };

            var nestedTestData4 = new PocoTestData
            {
                Name = "Dan",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherDan",
                    Age = 31,
                },
            };

            var nestedTestData5 = new PocoTestData
            {
                Name = "Mark",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherMark",
                    Age = 31,
                },
            };

            var nestedTestData6 = new PocoTestData
            {
                Name = "Warren",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherWarren",
                    Age = 31,
                },
            };

            var nestedTestData7 = new PocoTestData
            {
                Name = "Wallis",
                Age = 30,
                NestedData = new PocoTestData
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
            Given();

            IPath path = new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol);

            var pocoNavigator = new PocoNavigator(1);

            var data = pocoNavigator.SelectScalar(path);

            Assert.AreEqual(data, "1");
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingRootPathFromPrimitive_Expected_SingleValueInEnumeration()
        {
            Given();

            IPath path = new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol);

            var pocoNavigator = new PocoNavigator(1);

            var data = pocoNavigator.SelectEnumerable(path);

            const string expected = "1";
            var actual = string.Join("", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingRootPathFromPrimitive_Expected_SingleValueInEnumeration()
        {
            Given();

            IPath path = new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol);
            var paths = new List<IPath> { path };

            var pocoNavigator = new PocoNavigator(1);

            var data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            const string expected = "1";
            var actual = string.Join("|", data[path]);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectScalarValueUsingRootPathFromEnumerableContainingOnlyPrimitives_Expected_LastScalarValueInEnumeration()
        {
            var testData = new List<int> { 1, 2, 3 };

            IPath namePath = new PocoPath("().", "().");

            var pocoNavigator = new PocoNavigator(testData);

            const string expected = "3";
            var actual = pocoNavigator.SelectScalar(namePath).ToString();

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingRootPathFromEnumerableContainingOnlyPrimitives_Expected_ValuesForEachValueInEnumeration()
        {
            var testData = new List<int> { 1, 2, 3 };

            IPath path = new PocoPath("().", "().");
            var paths = new List<IPath> { path };

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            var expected = string.Join("|", testData.Select(e => e));
            var actual = string.Join("|", data[path]);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedUsingRootPathFromEnumerableContainingOnlyPrimitives_Expected_ValuesForEachValueInEnumeration()
        {
            var testData = new List<int> { 1, 2, 3 };

            IPath path = new PocoPath("().", "().");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectEnumerable(path);

            var expected = string.Join("|", testData.Select(e => e));
            var actual = string.Join("|", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromEnumerable_Expected_ScalarValue()
        {
            var testData = Given();

            IPath path = new PocoPath("EnumerableData.Count", "EnumerableData.Count");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectScalar(path);

            Assert.AreEqual(data, testData.EnumerableData.Count);
        }
        
        [TestMethod]
        public void SelectScalarValueUsingScalarPathFromReferenceType_Expected_ScalarValue()
        {
            var testData = Given();

            IPath namePath = new PocoPath("Name", "Name");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, testData.Name);
        }
        
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromReferenceType_Expected_ScalarValueFromLastItemInEnumerableCollection()
        {
            var testData = Given();

            IPath namePath = new PocoPath("EnumerableData().NestedData.Name", "EnumerableData.NestedData.Name");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, testData.EnumerableData.ElementAt(testData.EnumerableData.Count - 1).NestedData.Name);
        }
        
        [TestMethod]
        public void SelectScalarValueUsingEnumerablePathFromReferenceType_Where_EnumerableDataIsNull_Expected_Null()
        {
            var testData = GivenWithNoEnumerableData();

            IPath namePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, null);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromReferenceType_Expected_ValuesFromEachItemInEnumeration()
        {
            var testData = Given();

            IPath namePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectEnumerable(namePath);

            var expected = string.Join("|", testData.EnumerableData.Select(e => e.Name));
            var actual = string.Join("|", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }      
        
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerableReferenceType_Expected_ValuesFromEachItemInEnumeration()
        {
            var testData = Given();

            IPath namePath = new PocoPath("UnnamedArray().Name", "UnnamedArray.Name");

            var pocoNavigator = new PocoNavigator(testData.EnumerableData);

            var data = pocoNavigator.SelectEnumerable(namePath);

            var expected = string.Join("|", testData.EnumerableData.Select(e => e.Name));
            var actual = string.Join("|", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingEnumerablePathFromReferenceType_Where_EnumerableDataIsNull_Expected_Null()
        {
            var testData = GivenWithNoEnumerableData();

            IPath namePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectScalar(namePath);

            Assert.AreEqual(data, null);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromEnumerable_Expected_SingleValueInEnumeration()
        {
            var testData = Given();

            IPath namePath = new PocoPath("EnumerableData.Count", "EnumerableData.Count");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectEnumerable(namePath);

            var expected = testData.EnumerableData.Count.ToString();
            var actual = string.Join("", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesUsingScalarPathFromReferenceType_Expected_SingleValueInEnumeration()
        {
            var testData = Given();

            IPath namePath = new PocoPath("NestedData.Name", "NestedData.Name");

            var pocoNavigator = new PocoNavigator(testData);

            var data = pocoNavigator.SelectEnumerable(namePath);

            var expected = testData.NestedData.Name;
            var actual = string.Join("", data.Select(o => o.ToString()));

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainAScalarPath_Expected_FlattenedDataWithValueFromScalarPathRepeatingForEachEnumeration()
        {
            var testData = GivenWithParallelAndNestedEnumerables();

            var namePath = new PocoPath("Name", "Name");
            var enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            var paths = new List<IPath> { namePath, enumerableNamePath };

            var pocoNavigator = new PocoNavigator(testData);
            var data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            var expected = string.Join("|", testData.EnumerableData.Select(e => testData.Name));
            var actual = string.Join("|", data[namePath]);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainUnrelatedEnumerablePaths_Expected_FlattenedDataWithValuesFromUnrelatedEnumerablePathsAtMatchingIndexes()
        {
            var testData = GivenWithParallelAndNestedEnumerables();

            var enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            var parallelEnumerableNamePath = new PocoPath("EnumerableData1().Name", "EnumerableData1.Name");
            var paths = new List<IPath> { enumerableNamePath, parallelEnumerableNamePath };

            var pocoNavigator = new PocoNavigator(testData);
            var data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            var maxCount = Math.Max(testData.EnumerableData.Count, testData.EnumerableData1.Count);

            var tmpExpected = "";
            var tmpExpected1 = "";
            var separator = "|";
            for (int i = 0; i < maxCount; i++)
            {
                if (i == maxCount - 1)
                {
                    separator = "";
                }

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

            var expected = tmpExpected + "^" + tmpExpected1;
            var actual = string.Join("|", data[enumerableNamePath]);
            actual += "^" + string.Join("|", data[parallelEnumerableNamePath]);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainNestedEnumerablePaths_Expected_FlattenedDataWithValuesFromOuterEnumerablePathRepeatingForEveryValueFromNestedEnumerablePath()
        {
            var testData = GivenWithParallelAndNestedEnumerables();

            var enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            var nestedEnumerableNamePath = new PocoPath("EnumerableData().EnumerableData().Name", "EnumerableData.EnumerableData.Name");
            var paths = new List<IPath> { enumerableNamePath, nestedEnumerableNamePath };

            var pocoNavigator = new PocoNavigator(testData);
            var data = pocoNavigator.SelectEnumerablesAsRelated(paths);
            
            var tmpExpected = "";
            var tmpExpected1 = "";
            var separator = "|";

            for (int outerCount = 0; outerCount < testData.EnumerableData.Count; outerCount++)
            {
                for (int innerCount = 0; innerCount < testData.EnumerableData[outerCount].EnumerableData.Count; innerCount++)
                {
                    if (outerCount == testData.EnumerableData.Count - 1 && innerCount == testData.EnumerableData[outerCount].EnumerableData.Count - 1)
                    {
                        separator = "";
                    }

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

            var expected = tmpExpected + "^" + tmpExpected1;
            var actual = string.Join("|", data[enumerableNamePath]);
            actual += "^" + string.Join("|", data[nestedEnumerableNamePath]);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainASinglePathWhichIsEnumerable_Expected_FlattenedDataWithValuesFromEnumerablePath()
        {
            var testData = GivenWithParallelAndNestedEnumerables();

            var enumerableNamePath = new PocoPath("EnumerableData().Name", "EnumerableData.Name");
            var paths = new List<IPath> { enumerableNamePath };

            var pocoNavigator = new PocoNavigator(testData);
            var data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            var expected = string.Join("|", testData.EnumerableData.Select(e => e.Name));
            var actual = string.Join("|", data[enumerableNamePath]);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void SelectEnumerableValuesAsRelatedFromReferenceType_Where_PathsContainASinglePathWhichIsScalar_Expected_FlattenedDataWithValueFromScalarPath()
        {
            var testData = GivenWithParallelAndNestedEnumerables();

            var namePath = new PocoPath("Name", "Name");
            var paths = new List<IPath> { namePath };

            var pocoNavigator = new PocoNavigator(testData);
            var data = pocoNavigator.SelectEnumerablesAsRelated(paths);

            var expected = testData.Name;
            var actual = string.Join("|", data[namePath]);

            Assert.AreEqual(expected, actual);
        }
    }
}
