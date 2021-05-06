/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static Dev2.Common.Utils.JsonPathContext;

namespace Dev2.Utils.Tests
{
    [TestClass]
    public class JsonPathContextTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_SelectTo_Output_IsNull_AreEqual_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();

            var jsonPathContext = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => jsonPathContext.SelectTo(obj, "$;", null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_SelectNodes_Obj_NotNull_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();

            var jsonPathContext = new JsonPathContext();
            //--------------------------Act-------------------------------
            var testList = jsonPathContext.SelectNodes(obj, "$;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(1, testList.Length);
            Assert.AreEqual("$", testList[0].Path);
            Assert.AreEqual(obj, testList[0].Value);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_SelectNodes_ValueSystem_NotNull_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var obj = new object();

            var jsonPathContext = new JsonPathContext()
            {
                ValueSystem = mockJsonPathValueSystem.Object
            };
            //--------------------------Act-------------------------------
            var testList = jsonPathContext.SelectNodes(obj, "$;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(1, testList.Length);
            Assert.AreEqual("$", testList[0].Path);
            Assert.AreEqual(obj, testList[0].Value);
            Assert.AreEqual(mockJsonPathValueSystem.Object, jsonPathContext.ValueSystem);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_SelectNodes_JsonPathNodeArray_IsNull_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var obj = new object();

            var jsonPathContext = new JsonPathContext()
            {
                ValueSystem = mockJsonPathValueSystem.Object,
            };
            //--------------------------Act-------------------------------
            var testList = jsonPathContext.SelectNodes(obj, "$T;");
            //--------------------------Assert----------------------------
            mockJsonPathValueSystem.VerifyAll();
            Assert.AreEqual(0, testList.Length);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_AsBracketNotation_IsNull_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => AsBracketNotation(null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_AsBracketNotation_NotNull_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            string[] stringArray = { "testString1", "testString2", "testString3", "*" };
            //--------------------------Act-------------------------------
            var asBracketNotation = AsBracketNotation(stringArray);
            //--------------------------Assert----------------------------
            Assert.AreEqual("$['testString2']['testString3'][*]", asBracketNotation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_SelectNodes_Expr_Index1_NotSemiColon_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();
            obj = "testObject";

            var jsonPathContext = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<NullReferenceException>(() => jsonPathContext.SelectNodes(obj, "$test"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_SetProperty_AreEqual_ToSetValue_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();
            //--------------------------Act-------------------------------
            var jsonPathContext = new JsonPathContext()
            {
                ValueSystem = mockJsonPathValueSystem.Object,
            };
            //--------------------------Assert----------------------------
            Assert.AreEqual(mockJsonPathValueSystem.Object, jsonPathContext.ValueSystem);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(JsonPathContext))]
        public void JsonPathContext_SelectNodes_Obj_IsNull_AreEqual_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            var jsonPathContext = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => jsonPathContext.SelectNodes(null, "$;"));
        }
    }
}
