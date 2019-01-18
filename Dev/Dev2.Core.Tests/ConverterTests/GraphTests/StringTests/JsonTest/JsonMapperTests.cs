/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Json;

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.JsonTest
{
    [TestClass]
    public class JsonMapperTests
    {
        [TestMethod]
        public void MapJson_GivenPrimitive_Expected_RootPrimitivePath()
        {
            var jsonMapper = new JsonMapper();

            var json = @"""Name""";
            var paths = jsonMapper.Map(json);
            
            Assert.AreEqual("String", paths.FirstOrDefault().ToString(), "JSON mapper cannot parse JSON primitives.");
        }

        [TestMethod]
        public void MapJson_GivenPrimitiveEnumerable_Expected_RootPrimitivePath()
        {
            var jsonMapper = new JsonMapper();

            var json = @"[
      ""RandomData"",
      ""RandomData1""]"; ;
            var paths = jsonMapper.Map(json);

            var condition = paths.Any(p => p.ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol);
            Assert.IsTrue(condition);
        }
        
        [TestMethod]
        public void MapJson_GivenComplexObject_Expected_CorrectPaths()
        {
            var jsonMapper = new JsonMapper();

            var json = @"{
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
            var paths = jsonMapper.Map(json);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Motto"));
            Assert.IsTrue(paths.Any(p => p.ActualPath == "Departments().Name"));
            Assert.IsTrue(paths.Any(p => p.ActualPath == "PrimitiveRecordset()"));
            Assert.IsTrue(paths.Any(p => p.ActualPath == "Departments().Employees().Name"));
        }
    }
}
