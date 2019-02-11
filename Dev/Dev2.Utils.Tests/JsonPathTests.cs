using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common.Utils;
using Moq;
using static Dev2.Common.Utils.JsonPathContext;
using System;

namespace Dev2.Utils.Tests
{
    [TestClass]
    public class JsonPathTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_Constractor_PathLength_IsNull_AreEqual_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => new JsonPathNode(new object(), null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_Constractor_PathLength_IsZero_AreEqual_ExpectArgumentException()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentException>(() => new JsonPathNode(new object(), string.Empty));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_Constractor_IsNotNull_AreEqual_ExpectTrue()
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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathNode")]
        public void JsonPathNode_AsBracketNotation_HasRegExp_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            string[] stringArray = { "testString1", "testString2", "testString3", "*" };

            var asBracketNotation = AsBracketNotation(stringArray);
            //--------------------------Assert----------------------------
            Assert.AreEqual("$['testString2']['testString3'][*]", asBracketNotation);
        }

        //----------------------------------------------------------------------JsonPathContextTests--------------------------------------------

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathContext")]
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
        [TestCategory("JsonPathContext")]
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
        [TestCategory("JsonPathContext")]
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
        [TestCategory("JsonPathContext")]
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
        [TestCategory("JsonPathContext")]
        public void JsonPathContext_AsBracketNotation_IsNull_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(() => AsBracketNotation(null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathContext")]
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
        [TestCategory("JsonPathContext")]
        public void JsonPathContext_SelectNodes_Expr_Index1_NotSemiColon_AreEqual_ExpectTrue()
        {
            //--------------------------Arrange---------------------------
            var obj = new object();
            obj = "testObject";

            var jsonPathContext = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<NullReferenceException>(()=> jsonPathContext.SelectNodes(obj, "$test"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("JsonPathContext")]
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
        [TestCategory("JsonPathContext")]
        public void JsonPathContext_SelectNodes_Obj_IsNull_AreEqual_ExpectArgumentNullException()
        {
            //--------------------------Arrange---------------------------
            var jsonPathContext = new JsonPathContext();
            //--------------------------Act-------------------------------
            //--------------------------Assert----------------------------
            Assert.ThrowsException<ArgumentNullException>(()=> jsonPathContext.SelectNodes(null, "$;"));
        }
    }
}
