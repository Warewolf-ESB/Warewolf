using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Interfaces;
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
        #endregion Private/Internal Methods
        [TestMethod]
        public void MapPrimitive_Expected_PathToRoot()
        {
            PocoMapper pocoMapper = new PocoMapper();
            IEnumerable<IPath> paths = pocoMapper.Map(1);

            Assert.IsTrue(paths.Any(p => p.ActualPath == PocoPath.SeperatorSymbol));
        }

        /// <summary>
        /// Maps the enumerable only containing primitives expected path to root.
        /// </summary>
        [TestMethod]
        public void MapEnumerableOnlyContainingPrimitives_Expected_PathToRoot()
        {
            PocoMapper pocoMapper = new PocoMapper();
            List<int> primitives = new List<int> { 1, 2, 3 };
            IEnumerable<IPath> paths = pocoMapper.Map(primitives);

            Assert.IsTrue(paths.Any(p => p.ActualPath == PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol));
        }

        [TestMethod]
        public void MapEnumerable_Expected_PathToPublicPrimitiveMember()
        {
            PocoMapper pocoMapper = new PocoMapper();
            List<int> primitives = new List<int>();

            IEnumerable<IPath> paths = pocoMapper.Map(primitives);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Count"));
        }

        [TestMethod]
        public void MapEnumerableNestedInAnEnumerable_Expected_PathToPublicPrimitiveMemberOfEnumerableNestedInTheOuterEnumerable()
        {
            PocoMapper pocoMapper = new PocoMapper();
            PocoTestData testData = GivenWithParallelAndNestedEnumerables();

            IEnumerable<IPath> paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().EnumerableData.Count"));
        }

        [TestMethod]
        public void MapReferenceType_WhereGetAccessorsOfMembersThrowExceptions_Expected_PathsToExcludeThoseMembers()
        {
            PocoMapper pocoMapper = new PocoMapper();
            Uri uri = new Uri("/Cake", UriKind.Relative);
            IEnumerable<IPath> paths = pocoMapper.Map(uri);

            Assert.IsFalse(paths.Any(p => p.ActualPath == "Host"));
        }

        [TestMethod]
        public void MapReferenceType_Expected_PathToPublicPrimitiveMemberOfPublicReferenceMember()
        {
            PocoTestData testData = Given();

            PocoMapper pocoMapper = new PocoMapper();
            IEnumerable<IPath> paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Name"));
        }

        [TestMethod]
        public void MapNestedReferenceType_Expected_PathToPublicPrimitiveMemberOfNestedPublicReferenceMember()
        {
            PocoTestData testData = Given();

            PocoMapper pocoMapper = new PocoMapper();
            IEnumerable<IPath> paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "NestedData.Name"));
        }

        [TestMethod]
        public void MapReferenceTypeWithinEnumerable_Expected_PathToPublicPrimitiveMemberOfPublicEnumerableMember()
        {
            PocoTestData testData = Given();

            PocoMapper pocoMapper = new PocoMapper();
            IEnumerable<IPath> paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().Name"));
        }

        [TestMethod]
        public void MapReferenceTypeNestedInAnWithinEnumerable_Expected_PathToPublicPrimitiveMemberOfPublicReferenceMemberOfPublicEnumerableMember()
        {
            PocoTestData testData = Given();

            PocoMapper pocoMapper = new PocoMapper();
            IEnumerable<IPath> paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().NestedData.Name"));
        }

        [TestMethod]
        public void MapReferenceTypeNestedEnumerableAnWithinEnumerable_Expected_PathToPublicPrimitiveMemberOfNestedPublicEnumerableMember()
        {
            PocoTestData testData = GivenWithParallelAndNestedEnumerables();

            PocoMapper pocoMapper = new PocoMapper();
            IEnumerable<IPath> paths = pocoMapper.Map(testData);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "EnumerableData().EnumerableData().Name"));
        }
    }
}
