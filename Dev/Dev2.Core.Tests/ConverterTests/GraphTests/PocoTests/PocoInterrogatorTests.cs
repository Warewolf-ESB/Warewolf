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
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Poco;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.PocoTests
{
    [TestClass]
    public class PocoInterrogatorTests
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
        #endregion Private/Internal Methods

        #region Create Mapper Tests
        /// <summary>
        /// Create mapper expected poco mapper.
        /// </summary>
        [TestMethod]
        public void CreateMapper_Expected_PocoMapper()
        {
            var pocoTestData = Given();
            var pocoInterrogator = new PocoInterrogator();

            var mapper = pocoInterrogator.CreateMapper(pocoTestData);

            var expected = typeof(PocoMapper);
            var actual = mapper.GetType();

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
            var pocoTestData = Given();
            var pocoInterrogator = new PocoInterrogator();

            var navigator = pocoInterrogator.CreateNavigator(pocoTestData, typeof(PocoPath));

            var expected = typeof(PocoNavigator);
            var actual = navigator.GetType();

            Assert.AreEqual(expected, actual);
        }
        #endregion Create Navigator Tests
    }
}
