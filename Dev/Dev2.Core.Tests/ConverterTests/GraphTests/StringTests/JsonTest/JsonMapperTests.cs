
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
using Unlimited.Framework.Converters.Graph.String.Json;

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.JsonTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonMapperTests
    {
        /// <summary>
        /// Given Test Data
        /// </summary>
        /// <returns></returns>
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


        #region Map Tests

        /// <summary>
        /// Test that Mapping json with primitive enumerable returns root primitive path.
        /// </summary>
        [TestMethod]
        public void MapJsonWithPrimitiveEnumerable_Expected_RootPrimitivePath()
        {
            JsonMapper jsonMapper = new JsonMapper();

            string json = GivenPrimitiveRecordset();
            IEnumerable<IPath> paths = jsonMapper.Map(json);

            bool condition = paths.Any(p => p.ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol);
            Assert.IsTrue(condition);
        }

        /// <summary>
        /// Tests that mapping the json with scalar value returns path to scalar value.
        /// </summary>
        [TestMethod]
        public void MapJsonWithScalarValue_Expected_PathToScalarValue()
        {
            JsonMapper jsonMapper = new JsonMapper();

            string json = Given();
            IEnumerable<IPath> paths = jsonMapper.Map(json);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Motto"));
        }


        /// <summary>
        /// Tests that mapping json with a recordset returns path to property on enumerable.
        /// </summary>
        [TestMethod]
        public void MapJsonWithARecordset_Expected_PathToPropertyOnEnumerable()
        {
            JsonMapper jsonMapper = new JsonMapper();

            string json = Given();
            IEnumerable<IPath> paths = jsonMapper.Map(json);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Departments().Name"));
        }

        /// <summary>
        /// Tests that mapping json with a recordset containing scalar values returns path to enumerable.
        /// </summary>
        [TestMethod]
        public void MapJsonWithARecordsetContainingScalarValues_Expected_PathToEnumerable()
        {
            JsonMapper jsonMapper = new JsonMapper();

            string json = Given();
            IEnumerable<IPath> paths = jsonMapper.Map(json);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "PrimitiveRecordset()"));
        }

        /// <summary>
        /// Tests that mapping json with nested recordsets containing scalar values returns path to property of enumerable.
        /// </summary>
        [TestMethod]
        public void MapJsonWithNestedRecordsetsContainingScalarValues_Expected_PathToPropertyOfEnumerable()
        {
            JsonMapper jsonMapper = new JsonMapper();

            string json = Given();
            IEnumerable<IPath> paths = jsonMapper.Map(json);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Departments().Employees().Name"));
        }

        #endregion Map Tests
    }
}
