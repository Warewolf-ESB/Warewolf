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
    public class PocoMapperTests
    {
        #region Private/Internal Methods
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
        #endregion Private/Internal Methods
        [TestMethod]
        public void MapPrimitive_Expected_PathToRoot()
        {
            var pocoMapper = new PocoMapper();
            var paths = pocoMapper.Map(1);

            Assert.IsTrue(paths.Any(p => p.ActualPath == PocoPath.SeperatorSymbol));
        }

        /// <summary>
        /// Maps the enumerable only containing primitives expected path to root.
        /// </summary>
        [TestMethod]
        public void MapEnumerableOnlyContainingPrimitives_Expected_PathToRoot()
        {
            var pocoMapper = new PocoMapper();
            var primitives = new List<int> { 1, 2, 3 };
            var paths = pocoMapper.Map(primitives);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Capacity" || p.ActualPath == "Count"));
        }

        [TestMethod]
        public void MapEnumerable_Expected_PathToPublicPrimitiveMember()
        {
            var pocoMapper = new PocoMapper();
            var primitives = new List<int>();

            var paths = pocoMapper.Map(primitives);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Count"));
        }

        [TestMethod]
        public void MapEnumerableNestedInAnEnumerable_Expected_PathToPublicPrimitiveMemberOfEnumerableNestedInTheOuterEnumerable()
        {
            var pocoMapper = new PocoMapper();
            var testData = GivenWithParallelAndNestedEnumerables();

            var paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().EnumerableData.Count"));
        }

        [TestMethod]
        public void MapReferenceType_WhereGetAccessorsOfMembersThrowExceptions_Expected_PathsToExcludeThoseMembers()
        {
            var pocoMapper = new PocoMapper();
            var uri = new Uri("/Cake", UriKind.Relative);
            var paths = pocoMapper.Map(uri);

            Assert.IsFalse(paths.Any(p => p.ActualPath == "Host"));
        }

        [TestMethod]
        public void MapReferenceType_Expected_PathToPublicPrimitiveMemberOfPublicReferenceMember()
        {
            var testData = Given();

            var pocoMapper = new PocoMapper();
            var paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Name"));
        }

        [TestMethod]
        public void MapNestedReferenceType_Expected_PathToPublicPrimitiveMemberOfNestedPublicReferenceMember()
        {
            var testData = Given();

            var pocoMapper = new PocoMapper();
            var paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "NestedData.Name"));
        }

        [TestMethod]
        public void MapReferenceTypeWithinEnumerable_Expected_PathToPublicPrimitiveMemberOfPublicEnumerableMember()
        {
            var testData = Given();

            var pocoMapper = new PocoMapper();
            var paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().Name"));
        }

        [TestMethod]
        public void MapReferenceTypeNestedInAnWithinEnumerable_Expected_PathToPublicPrimitiveMemberOfPublicReferenceMemberOfPublicEnumerableMember()
        {
            var testData = Given();

            var pocoMapper = new PocoMapper();
            var paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().NestedData.Name"));
        }

        [TestMethod]
        public void MapEnumerable_Expected_PathToEnumerable()
        {
            var pocoMapper = new PocoMapper();
            var testData = GivenWithParallelAndNestedEnumerables();

            var paths = pocoMapper.Map(testData.EnumerableData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "UnnamedArray().Name"));
        }

        [TestMethod]
        public void MapReferenceTypeNestedEnumerableAnWithinEnumerable_Expected_PathToPublicPrimitiveMemberOfNestedPublicEnumerableMember()
        {
            var testData = GivenWithParallelAndNestedEnumerables();

            var pocoMapper = new PocoMapper();
            var paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().EnumerableData().Name"));
        }
    }
}
