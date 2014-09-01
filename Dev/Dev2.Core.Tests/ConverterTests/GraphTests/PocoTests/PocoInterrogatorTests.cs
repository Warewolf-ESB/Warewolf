using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Framework.Converters.Graph.Poco;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.PocoTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PocoInterrogatorTests
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
        #endregion Private/Internal Methods

        #region Create Mapper Tests
        /// <summary>
        /// Create mapper expected poco mapper.
        /// </summary>
        [TestMethod]
        public void CreateMapper_Expected_PocoMapper()
        {
            PocoTestData pocoTestData = Given();
            PocoInterrogator pocoInterrogator = new PocoInterrogator();

            IMapper mapper = pocoInterrogator.CreateMapper(pocoTestData);

            Type expected = typeof(PocoMapper);
            Type actual = mapper.GetType();

            Assert.AreEqual(expected, actual);
        }
        #endregion Create Mapper Tests

        #region Create Navigator Tests

        /// <summary>
        /// Creates the navigator expected poco navigator.
        /// </summary>
        [TestMethod]
        public void CreateNavigator_Expected_PocoNavigator()
        {
            PocoTestData pocoTestData = Given();
            PocoInterrogator pocoInterrogator = new PocoInterrogator();

            INavigator navigator = pocoInterrogator.CreateNavigator(pocoTestData, typeof(PocoPath));

            Type expected = typeof(PocoNavigator);
            Type actual = navigator.GetType();

            Assert.AreEqual(expected, actual);
        }
        #endregion Create Navigator Tests
    }
}
