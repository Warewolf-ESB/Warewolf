using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common.Utils;
using Moq;
using System.Collections;
using static Dev2.Common.Utils.JsonPathContext;
using System.Collections.Generic;
using System;

namespace Dev2.Utils.Tests
{
    [TestClass]
    public class JsonPathTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_SetProperty_AreEqual_ToSetValue_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();
            //--------------------------Act-------------------------------
            var jsonPathNode = new JsonPathContext()
            {
                ValueSystem = mockJsonPathValueSystem.Object,
            };
            //--------------------------Assert----------------------------
            Assert.AreEqual(mockJsonPathValueSystem.Object, jsonPathNode.ValueSystem);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_SelectNodes_Obj_IsNull_AreEqual_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            var jsonPathNode = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(()=> jsonPathNode.SelectNodes(null, "$;"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_Constractor_Path_IsNull_AreEqual_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => new JsonPathNode(new object(), null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_Constractor_PathLength_IsZoro_AreEqual_ExpectArgumentException()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentException>(() => new JsonPathNode(new object(), string.Empty));
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_Constractor_IsZoro_AreEqual_ExpectArgumentException()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();
            obj = "testObject";

            var testString = "testString";

            var jsonPathNode = new JsonPathNode(obj, testString);
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
             Assert.AreEqual(testString +" = " + obj, jsonPathNode.ToString());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_SelectTo_Output_IsNull_AreEqual_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();

            var jsonPathNode = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => jsonPathNode.SelectTo(obj, "$;", null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_SelectNodes_Obj_NotNull_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();

            var jsonPathNode = new JsonPathContext();
            //--------------------------Act-------------------------------
            var testList = jsonPathNode.SelectNodes(obj, "$;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(1, testList.Length);
            Assert.AreEqual("$", testList[0].Path);
            Assert.AreEqual(obj, testList[0].Value);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_SelectNodes_Expr_Index1_NotSemiColon_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();
            obj = "testObject";

            var jsonPathNode = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<NullReferenceException>(()=> jsonPathNode.SelectNodes(obj, "$test"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_AsBracketNotation_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => AsBracketNotation(null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_AsBracketNotation_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            string[] stringArray = { "testString1", "testString2", "testString3" };

            var asBracketNotation = AsBracketNotation(stringArray);
            //--------------------------Assert----------------------------
            Assert.AreEqual("$['testString2']['testString3']", asBracketNotation);
        }
    }
}
