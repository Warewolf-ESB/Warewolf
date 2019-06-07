/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Dev2.Common.Utils.JsonPathContext;
using Moq;
using Dev2.Common.Utils;

namespace Dev2.Utils.Tests
{
    [TestClass]
    public class InterpreterTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Expr_IsNull_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o,sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "test_test");

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object,  eval);
            var obj = new object();
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes(null, obj, "test_path");
            //--------------------------Assert----------------------------
            Assert.AreEqual(output, interpreter._output);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_SystemHasMember_True_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "test_test");
            var obj = new object();
            
            mockJsonPathValueSystem.Setup(o => o.HasMember(It.IsAny<object>(), It.IsAny<string>())).Returns(true);
            mockJsonPathValueSystem.Setup(o => o.GetMemberValue(It.IsAny<object>(), It.IsAny<string>())).Returns("test_member_value");

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("test_expr", obj, "test_path");
            //--------------------------Assert----------------------------
            Assert.AreEqual(output, interpreter._output);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Walk_SystemIsPrimitive_True_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "test_test");
            var obj = new object();

            mockJsonPathValueSystem.Setup(o => o.IsPrimitive(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("*;", obj, "test_path");
            //--------------------------Assert----------------------------
            mockJsonPathValueSystem.Verify(o => o.IsPrimitive(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Walk_SystemIsArray_True_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "test_test");
            var obj = new object();
            obj = new List<string> { "string1", "string2" }.ToArray();

            mockJsonPathValueSystem.Setup(o => o.IsArray(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("*;", obj, "test_path");
            //--------------------------Assert----------------------------
            mockJsonPathValueSystem.Verify(o => o.IsArray(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Walk_IsObject_True_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "test_test");

            var obj = new object();
            obj = "test_object".ToString();

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("*;", obj, "test_path");
            //--------------------------Assert----------------------------
            mockJsonPathValueSystem.Verify(o => o.IsObject(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Walk_IsObject_True_ContainsComma_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "test_test");

            var obj = new object();
            obj = "test_object".ToString();

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("..,asdf;", obj, "test_path");
            //--------------------------Assert----------------------------
            mockJsonPathValueSystem.Verify(o => o.IsObject(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Walk_IsObject_True_ContainsCommaParentheses_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "test_test, testsss");

            var obj = new object();
            obj = "test_object".ToString();

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("(..,asdf.*);", obj, "test_path;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(eval, interpreter._eval);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Eval_ContainsCommaParentheses_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "?(test_test, testsss)");

            var obj = new object();
            obj = "test_object".ToString();

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("(..,asdf.*);", obj, "test_path;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(eval, interpreter._eval);
            Assert.AreEqual(output, interpreter._output);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Exp_ContainsColonNumber_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "?(test_test, testsss)");
                
            var obj = new object();
            obj = new List<string> { "string1", "string2" }.ToArray();

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("09:01:00;", obj, "test_path;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(eval, interpreter._eval);
            Assert.AreEqual(output, interpreter._output);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_ParseInt_Str_IsNullOrEmpty_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "?(test_test, testsss)");

            var obj = new object();
            obj = new List<string> { "string1", "string2" }.ToArray();

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("::;", obj, "test_path;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(eval, interpreter._eval);
            Assert.AreEqual(output, interpreter._output);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_ParseInt_Throw_FormatException_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "?(test_test, testsss)");

            var obj = new object();
            obj = new List<string> { "string1", "string2" }.ToArray();

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("-1::;", obj, "test_path;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(eval, interpreter._eval);
            Assert.AreEqual(output, interpreter._output);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_Trace_Slice_Value_IsNull_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => ss = "?(test_test, testsss)");

            var obj = new object();
            obj = new List<string> { null };

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("09:01:00;", null, "test_path;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(eval, interpreter._eval);
            Assert.AreEqual(output, interpreter._output);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Interpreter))]
        public void Interpreter_WalkFiltered_IsInvoked_ExpectSuccess()
        {
            //--------------------------Arrange---------------------------
            var mockJsonPathValueSystem = new Mock<IJsonPathValueSystem>();

            var output = new JsonPathResultAccumulator((o, sa) => sa = new string[] { "string_Test" });
            var eval = new JsonPathScriptEvaluator((s, o, ss) => o = true);

            var objList = new List<string> { "string1", "string2" };
            var isInvoked = false;

            mockJsonPathValueSystem.Setup(o => o.IsObject(It.IsAny<object>())).Returns(true);
            mockJsonPathValueSystem.Setup(o => o.GetMembers(It.IsAny<object>()))
                                    .Callback(()=> isInvoked = true)
                                    .Returns(objList);

            var interpreter = new Interpreter(output, mockJsonPathValueSystem.Object, eval);
            //--------------------------Act-------------------------------
            interpreter.StoreExpressionTreeLeafNodes("?(asd);", new object(), "test_path;");
            //--------------------------Assert----------------------------
            Assert.AreEqual(eval, interpreter._eval);
            Assert.AreEqual(output, interpreter._output);

            Assert.IsTrue(isInvoked);
        }
    }
}
