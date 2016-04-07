using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;
using Dev2.Communication;
// ReSharper disable InconsistentNaming

// ReSharper disable PossibleNullReferenceException
namespace WarewolfParsingTest
{
    [TestClass]
    public class AssignJsonTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignSingleProperty_ValueProperty")]
        public void AssignSingleProperty_ValueProperty_Assign_A_Property()
        {
            //------------Setup for test--------------------------
           
            ExecutionEnvironment environment = new ExecutionEnvironment();
            var values = new List<IAssignValue>(){new AssignValue("[[Person.Name]]","John")};
           Dev2JsonSerializer ser = new Dev2JsonSerializer();
            
            
            //------------Execute Test---------------------------
            environment.AssignJson(values,0);
            //------------Assert Results-------------------------


            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if(obj != null)
            {
                Assert.AreEqual(obj.ToString(), "{\r\n  \"Name\": \"John\"\r\n}");
            }
            else
            {
                Assert.Fail("bob");
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignSingleProperty_ValueProperty")]
        public void AssignSingleProperty_AssignAChildArrayValue()
        {
            //------------Setup for test--------------------------

            ExecutionEnvironment environment = new ExecutionEnvironment();
            var values = new List<IAssignValue>() { new AssignValue("[[Person.Name]]","John"), new AssignValue("[[Person.Children(1).Name]]", "Mary") };
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            //var p = new Person() { Name = "John", Children = new List<Person> { new Person() { Name = "Mary",  }}};

            //var x = ser.Serialize(p);
            //------------Execute Test---------------------------
            environment.AssignJson(values,0);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), "{\r\n  \"Name\": \"John\",\r\n  \"Children\": [\r\n    {\r\n      \"Name\": \"Mary\"\r\n    }\r\n  ]\r\n}");
            }
            else
            {
                Assert.Fail("bob");
            }
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignSingleProperty_ValueProperty")]
        public void AssignSingleProperty_AssignASecondValueChildName()
        {
            //------------Setup for test--------------------------

            ExecutionEnvironment environment = new ExecutionEnvironment();
            var values = new List<IAssignValue>() { new AssignValue("[[Person.Name]]", "John"), new AssignValue("[[Person.Children(1).Name]]", "Mary"), new AssignValue("[[Person.Children(2).Name]]", "Joe") };
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            //var p = new Person() { Name = "John", Children = new List<Person> { new Person() { Name = "Mary",  }}};

            //var x = ser.Serialize(p);
            //------------Execute Test---------------------------
            environment.AssignJson(values,0);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), "{\r\n  \"Name\": \"John\",\r\n  \"Children\": [\r\n    {\r\n      \"Name\": \"Mary\"\r\n    },\r\n    {\r\n      \"Name\": \"Joe\"\r\n    }\r\n  ]\r\n}");
            }
            else
            {
                Assert.Fail("bob");
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignSingleProperty_ValueProperty")]
        public void AssignSingleProperty_AssignALastValueChildName()
        {
            //------------Setup for test--------------------------

            ExecutionEnvironment environment = new ExecutionEnvironment();
            var values = new List<IAssignValue>() { new AssignValue("[[Person.Name]]", "John"), new AssignValue("[[Person.Children(1).Name]]", "Mary"), new AssignValue("[[Person.Children(2).Name]]", "Joe"), new AssignValue("[[Person.Children(2).Name]]", "Moe") };
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            //var p = new Person() { Name = "John", Children = new List<Person> { new Person() { Name = "Mary",  }}};

            //var x = ser.Serialize(p);
            //------------Execute Test---------------------------
            environment.AssignJson(values,0);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), "{\r\n  \"Name\": \"John\",\r\n  \"Children\": [\r\n    {\r\n      \"Name\": \"Mary\"\r\n    },\r\n    {\r\n      \"Name\": \"Moe\"\r\n    }\r\n  ]\r\n}");
            }
            else
            {
                Assert.Fail("bob");
            }
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignSingleProperty_ValueProperty")]
        public void AssignSingleProperty_AssignAllValueChildName()
        {
            //------------Setup for test--------------------------

            ExecutionEnvironment environment = new ExecutionEnvironment();
            var values = new List<IAssignValue>() { new AssignValue("[[Person.Name]]", "John"), new AssignValue("[[Person.Children(1).Name]]", "Mary"), new AssignValue("[[Person.Children(2).Name]]", "Joe"), new AssignValue("[[Person.Children(*).Name]]", "Moe") };
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            //var p = new Person() { Name = "John", Children = new List<Person> { new Person() { Name = "Mary",  }}};

            //var x = ser.Serialize(p);
            //------------Execute Test---------------------------
            environment.AssignJson(values,0);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), "{\r\n  \"Name\": \"John\",\r\n  \"Children\": [\r\n    {\r\n      \"Name\": \"Moe\"\r\n    },\r\n    {\r\n      \"Name\": \"Moe\"\r\n    }\r\n  ]\r\n}");
            }
            else
            {
                Assert.Fail("bob");
            }
        }

        DataASTMutable.WarewolfEnvironment GetFromEnv(ExecutionEnvironment env)
        {
           PrivateObject p = new PrivateObject(env);
           return (DataASTMutable.WarewolfEnvironment)p.GetField("_env");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_AddProperty")]
        public void AssignEvaluation_AddProperty_AddAtom_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            JObject j = new JObject();
            
            //------------Execute Test---------------------------
            var obj = AssignEvaluation.addAtomicPropertyToJson(j, "Name", DataASTMutable.WarewolfAtom.NewDataString("a"));
            var result = obj.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual(@"{
  ""Name"": ""a""
}",result);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_AddProperty")]
        public void AssignEvaluation_AddProperty_AddAtom_AlreadyExist_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            JObject j = new JObject();

            //------------Execute Test---------------------------
            // ReSharper disable once RedundantAssignment
            var obj = AssignEvaluation.addAtomicPropertyToJson(j, "Name", DataASTMutable.WarewolfAtom.NewDataString("a"));
            obj = AssignEvaluation.addAtomicPropertyToJson(j, "Name", DataASTMutable.WarewolfAtom.NewDataString("x"));
            var result = obj.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual(@"{
  ""Name"": ""x""
}", result);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_AddProperty")]
        public void AssignEvaluation_AddProperty_AddNothing_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            JObject j = new JObject();

            //------------Execute Test---------------------------
            // ReSharper disable once RedundantAssignment
            var obj = AssignEvaluation.addAtomicPropertyToJson(j, "Name", DataASTMutable.WarewolfAtom.Nothing);
            var result = obj.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual(@"{
  ""Name"": null
}", result);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_AddProperty")]
        public void AssignEvaluation_AddProperty_AddArray_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            JObject j = new JObject();

            //------------Execute Test---------------------------
            var obj = AssignEvaluation.addArrayPropertyToJson(j, "Name", new List<DataASTMutable.WarewolfAtom>{ DataASTMutable.WarewolfAtom.NewDataString("a"),DataASTMutable.WarewolfAtom.NewDataString("b")});
            var result = obj.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual("{\r\n  \"Name\": [\r\n    \"a\",\r\n    \"b\"\r\n  ]\r\n}", result);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_AddProperty")]
        public void AssignEvaluation_AddProperty_AddArray_Exists_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            JObject j = new JObject();

            //------------Execute Test---------------------------
            // ReSharper disable once RedundantAssignment
            var obj = AssignEvaluation.addArrayPropertyToJson(j, "Name", new List<DataASTMutable.WarewolfAtom> { DataASTMutable.WarewolfAtom.NewDataString("a"), DataASTMutable.WarewolfAtom.NewDataString("b") });
            obj = AssignEvaluation.addArrayPropertyToJson(j, "Name", new List<DataASTMutable.WarewolfAtom> { DataASTMutable.WarewolfAtom.NewDataString("x"), DataASTMutable.WarewolfAtom.NewDataString("y") });
            var result = obj.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual("{\r\n  \"Name\": [\r\n    \"x\",\r\n    \"y\"\r\n  ]\r\n}", result);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_AddProperty")]
        public void AssignEvaluation_AddProperty_AddArray_withNulls_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            JObject j = new JObject();

            //------------Execute Test---------------------------
            var obj = AssignEvaluation.addArrayPropertyToJson(j, "Name", new List<DataASTMutable.WarewolfAtom> { DataASTMutable.WarewolfAtom.Nothing, DataASTMutable.WarewolfAtom.NewDataString("b") });
            var result = obj.ToString();
            //------------Assert Results-------------------------
            Assert.AreEqual("{\r\n  \"Name\": [\r\n    null,\r\n    \"b\"\r\n  ]\r\n}", result);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAValue_addsObjectIfItDoesNotExist()
        {
            var env = CreateTestEnvWithData();

            var result = PublicFunctions.EvalEnvExpression("[[a]]", 0, env);

            var env2 = AssignEvaluation.assignGivenAValue(env,result, LanguageAST.JsonIdentifierExpression.NewNestedNameExpression(new LanguageAST.JsonPropertyIdentifier("Person",LanguageAST.JsonIdentifierExpression.NewNameExpression(new LanguageAST.JsonIdentifier("Person")))));
            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAValueCreatesValidJson_addsObjectIfItDoesNotExist()
        {
            var env = CreateTestEnvWithData();

            var result = PublicFunctions.EvalEnvExpression("[[a]]", 0, env);

            var env2 = AssignEvaluation.assignGivenAValue(env, result, LanguageAST.JsonIdentifierExpression.NewNestedNameExpression(new LanguageAST.JsonPropertyIdentifier("Bob", LanguageAST.JsonIdentifierExpression.NewNameExpression(new LanguageAST.JsonIdentifier("Age")))));
            
            Assert.IsTrue(env2.JsonObjects.ContainsKey("Bob"));
            Assert.AreEqual(env2.JsonObjects["Bob"].ToString(), @"{
  ""Age"": ""5""
}");
        }


        [TestMethod]
        [Ignore]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_addsArrayIfItDoesNotExist()
        {
            var env = CreateTestEnvWithData();

            var result = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child.Name]]");
            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;

            var env2 = AssignEvaluation.assignGivenAValue(env, result, LanguageAST.JsonIdentifierExpression.NewNestedNameExpression(new LanguageAST.JsonPropertyIdentifier("Bob", LanguageAST.JsonIdentifierExpression.NewIndexNestedNameExpression(new LanguageAST.BasicJsonIndexedPropertyIdentifier("Children",LanguageAST.JsonIdentifierExpression.Terminal, LanguageAST.Index.Star) ))));

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Bob"));
            Assert.AreEqual(env2.JsonObjects["Bob"].ToString(), "{\r\n  \"Children\": [\r\n    \"2\",\r\n    \"4\",\r\n    \"3\"\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [Ignore]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_addsArrayIfItDoesNotExistIntIndex()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);

            var env2 = AssignEvaluation.assignGivenAValue(env, result, LanguageAST.JsonIdentifierExpression.NewNestedNameExpression(new LanguageAST.JsonPropertyIdentifier("Bob", LanguageAST.JsonIdentifierExpression.NewIndexNestedNameExpression(new LanguageAST.BasicJsonIndexedPropertyIdentifier("Children", LanguageAST.JsonIdentifierExpression.Terminal, LanguageAST.Index.NewIntIndex(1))))));

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Bob"));
            Assert.AreEqual(env2.JsonObjects["Bob"].ToString(), "{\r\n  \"Children\": [\r\n    \"2\"\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [Ignore]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_addsArrayIfItDoesNotExistInt_RandomIndex()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);

            var env2 = AssignEvaluation.assignGivenAValue(env, result, LanguageAST.JsonIdentifierExpression.NewNestedNameExpression(new LanguageAST.JsonPropertyIdentifier("Bob", LanguageAST.JsonIdentifierExpression.NewIndexNestedNameExpression(new LanguageAST.BasicJsonIndexedPropertyIdentifier("Children", LanguageAST.JsonIdentifierExpression.Terminal, LanguageAST.Index.NewIntIndex(5))))));

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Bob"));
            Assert.AreEqual(env2.JsonObjects["Bob"].ToString(), "{\r\n  \"Children\": [\r\n    null,\r\n    null,\r\n    null,\r\n    null,\r\n    \"2\"\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [Ignore]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_addsArrayIfItDoesNotExistIntLast()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);

            var env2 = AssignEvaluation.assignGivenAValue(env, result, LanguageAST.JsonIdentifierExpression.NewNestedNameExpression(new LanguageAST.JsonPropertyIdentifier("Bob", LanguageAST.JsonIdentifierExpression.NewIndexNestedNameExpression(new LanguageAST.BasicJsonIndexedPropertyIdentifier("Children", LanguageAST.JsonIdentifierExpression.Terminal, LanguageAST.Index.Last)))));

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Bob"));
            Assert.AreEqual(env2.JsonObjects["Bob"].ToString(), "{\r\n  \"Children\": [\r\n    \"2\"\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnObjectValueCreatesValidJson_addsObjectIfItDoesNotExistIntLastAndAddsProperty()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child.Name]]");
            var val =  ( LanguageAST.LanguageExpression.JsonIdentifierExpression) parsed;
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            Assert.AreEqual(env2.JsonObjects["Person"].ToString(), "{\r\n  \"Child\": {\r\n    \"Name\": \"2\"\r\n  }\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_addsArrayIfItDoesNotExistIntLastAndAddsProperty()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child(1).Name]]");
            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            Assert.AreEqual(env2.JsonObjects["Person"].ToString(), "{\r\n  \"Child\": [\r\n    {\r\n      \"Name\": \"2\"\r\n    }\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_LastIndex_addsArrayIfItDoesNotExistIntLastAndAddsProperty()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child().Name]]");
            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            Assert.AreEqual(env2.JsonObjects["Person"].ToString(), "{\r\n  \"Child\": [\r\n    {\r\n      \"Name\": \"2\"\r\n    }\r\n  ]\r\n}");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_LastIndex_MutateArray()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var secondResult = PublicFunctions.EvalEnvExpression("[[rec(2).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child().Name]]");

            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;
            // ReSharper disable once RedundantAssignment
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);
            env2 = AssignEvaluation.assignGivenAValue(env2, secondResult, val.Item);
            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            Assert.AreEqual(env2.JsonObjects["Person"].ToString(), "{\r\n  \"Child\": [\r\n    {\r\n      \"Name\": \"2\"\r\n    },\r\n    {\r\n      \"Name\": \"4\"\r\n    }\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_LastIndex_MutateArray_differentProperties()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var secondResult = PublicFunctions.EvalEnvExpression("[[rec(2).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child().Name]]");
            var parsed2 = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child().Age]]");
            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;
            var val2 = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed2;
            // ReSharper disable once RedundantAssignment
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);
            env2 = AssignEvaluation.assignGivenAValue(env2, secondResult, val2.Item);
            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            Assert.AreEqual(env2.JsonObjects["Person"].ToString(), "{\r\n  \"Child\": [\r\n    {\r\n      \"Name\": \"2\"\r\n    },\r\n    {\r\n      \"Age\": \"4\"\r\n    }\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_StarIndex_MutateArray()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var secondResult = PublicFunctions.EvalEnvExpression("[[rec(2).a]]", 0, env);
            var thirdResult = PublicFunctions.EvalEnvExpression("[[rec(3).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child().Name]]");
            var parsed2 = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child(*).Name]]");
            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;
            var val2 = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed2;
            // ReSharper disable once RedundantAssignment
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);
            env2 = AssignEvaluation.assignGivenAValue(env2, secondResult, val.Item);
            env2 = AssignEvaluation.assignGivenAValue(env2, thirdResult, val2.Item);
            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            Assert.AreEqual(env2.JsonObjects["Person"].ToString(), "{\r\n  \"Child\": [\r\n    {\r\n      \"Name\": \"Bob\"\r\n    },\r\n    {\r\n      \"Name\": \"Bob\"\r\n    }\r\n  ]\r\n}");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_StarIndex_MutateArray_AndAddAProperty()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var secondResult = PublicFunctions.EvalEnvExpression("[[rec(2).a]]", 0, env);
            var thirdResult = PublicFunctions.EvalEnvExpression("[[rec(3).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child().Name]]");
            var parsed2 = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child(*).Name]]");
            var parsed3 = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Age]]");
            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;
            var val2 = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed2;
            var val3 = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed3;
            // ReSharper disable once RedundantAssignment
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);
            env2 = AssignEvaluation.assignGivenAValue(env2, secondResult, val.Item);
            env2 = AssignEvaluation.assignGivenAValue(env2, thirdResult, val2.Item);
            env2 = AssignEvaluation.assignGivenAValue(env2, result, val3.Item);
            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            var obj = env2.JsonObjects["Person"];
            Assert.AreEqual(obj.ToString(), "{\r\n  \"Child\": [\r\n    {\r\n      \"Name\": \"Bob\"\r\n    },\r\n    {\r\n      \"Name\": \"Bob\"\r\n    }\r\n  ],\r\n  \"Age\": \"2\"\r\n}");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_assignGivenAnArrayValueCreatesValidJson_Invalid()
        {
            var env = CreateTestEnvWithData();
            JArray x = new JArray();

            var result = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, env);
            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Person.Child(1).Name]]");
            var val = (LanguageAST.LanguageExpression.JsonIdentifierExpression)parsed;
            var env2 = AssignEvaluation.assignGivenAValue(env, result, val.Item);

            Assert.IsTrue(env2.JsonObjects.ContainsKey("Person"));
            Assert.AreEqual(env2.JsonObjects["Person"].ToString(), "{\r\n  \"Child\": [\r\n    {\r\n      \"Name\": \"2\"\r\n    }\r\n  ]\r\n}");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluation_assignGivenAValue")]
        public void AssignEvaluation_LanguageExpressionToJsonExpression()
        {

            var parsed = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("[[Child(1).Name]]");
            var exp = AssignEvaluation.languageExpressionToJsonIdentifier(parsed);
            Assert.IsTrue(exp.IsIndexNestedNameExpression);
            var exp2 = (exp as LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression).Item;
            var index = exp2.Index;
            Assert.IsTrue(index.IsIntIndex);
            var bob = (index as LanguageAST.Index.IntIndex).Item;
            Assert.AreEqual(1,bob);
            Assert.AreEqual("Child", exp2.ObjectName);
            Assert.IsTrue( exp2.Next.IsNameExpression);
            var x2 = (exp2.Next as LanguageAST.JsonIdentifierExpression.NameExpression).Item;
            Assert.AreEqual(x2.Name,"Name");
           
        }



        private DataASTMutable.WarewolfEnvironment CreateTestEnvWithData()
        {

            IEnumerable<IAssignValue> assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "2"),
                 new AssignValue("[[rec().a]]", "4"),
                 new AssignValue("[[rec().a]]", "Bob"),
                 new AssignValue("[[a]]", "5"),
                 new AssignValue("[[b]]", "2344"),
                 new AssignValue("[[c]]", "a"),
                 new AssignValue("[[d]]", "1")

             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalMultiAssign(assigns, 0, env);
            return env2;
        }

    }


}
