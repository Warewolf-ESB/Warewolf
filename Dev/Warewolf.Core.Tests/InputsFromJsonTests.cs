/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Core.Tests
{
    [TestClass]
    public class InputsFromJsonTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InputsFromJson))]
        public void InputsFromJson_IsNullOrEmpty_String_Expect_Nothing()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var dataList = string.Empty;

            var serviceInputs = new List<IServiceInput>();
            InputsFromJson.FromJson(dataList, serviceInputs);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, serviceInputs.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InputsFromJson))]
        public void InputsFromJson_IncorrectJson_Expect_Nothing()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var dataList = "\"Environment\":{\"scalars\":{\"a\":1,\"b\":2,\"c\":3},\"record_sets\":{\"rec\":{\"WarewolfPositionColumn\":[1],\"d\":[\"\"],\"e\":[\"\"],\"f\":[\"\"]}},\"json_objects\":{\"obj\":{\"x\":{\"y\":{\"z\":\"\"}}}}},\"Errors\":[],\"AllErrors\":[]";

            var serviceInputs = new List<IServiceInput>();
            InputsFromJson.FromJson(dataList, serviceInputs);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, serviceInputs.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(InputsFromJson))]
        public void InputsFromJson_IncorrectJson_Expect_Results()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var dataList = "{\"Environment\":{\"scalars\":{\"a\":1,\"b\":2,\"c\":3},\"record_sets\":{\"rec\":{\"WarewolfPositionColumn\":[1],\"d\":[\"4\"],\"e\":[\"5\"],\"f\":[\"6\"]}},\"json_objects\":{\"obj\":{\"x\":{\"y\":{\"z\":\"\"}}}}},\"Errors\":[],\"AllErrors\":[]}";

            var serviceInputs = new List<IServiceInput>();
            InputsFromJson.FromJson(dataList, serviceInputs);
            //------------Assert Results-------------------------
            Assert.AreEqual(7, serviceInputs.Count);

            Assert.AreEqual("a", serviceInputs[0].Name);
            Assert.AreEqual("1", serviceInputs[0].Value);

            Assert.AreEqual("b", serviceInputs[1].Name);
            Assert.AreEqual("2", serviceInputs[1].Value);

            Assert.AreEqual("c", serviceInputs[2].Name);
            Assert.AreEqual("3", serviceInputs[2].Value);

            Assert.AreEqual("rec(1).d", serviceInputs[3].Name);
            Assert.AreEqual("4", serviceInputs[3].Value);

            Assert.AreEqual("rec(1).e", serviceInputs[4].Name);
            Assert.AreEqual("5", serviceInputs[4].Value);

            Assert.AreEqual("rec(1).f", serviceInputs[5].Name);
            Assert.AreEqual("6", serviceInputs[5].Value);

            Assert.AreEqual("@obj", serviceInputs[6].Name);
            Assert.IsTrue(serviceInputs[6].Value.Contains("x"));
            Assert.IsTrue(serviceInputs[6].Value.Contains("y"));
            Assert.IsTrue(serviceInputs[6].Value.Contains("z"));
        }
    }
}