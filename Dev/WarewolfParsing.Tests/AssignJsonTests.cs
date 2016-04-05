using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;
using Dev2.Communication;
using System.Linq;
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
            environment.AssignJson(values);
            //------------Assert Results-------------------------


            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if(obj != null)
            {
                Assert.AreEqual(obj.ToString(), @"{
	""firstName"": ""John""
}");
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
            environment.AssignJson(values);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), @"{
  ""Name"": ""John"",
  ""Children"": [
    {
      ""Name"": ""Mary"",
    }
  ],
}");
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
            environment.AssignJson(values);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), @"{
  ""Name"": ""John"",
  ""Children"": [
    {
      ""Name"": ""Mary"",
    },
    {
      ""Name"": ""Joe"",
    }
  ],
}");
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
            environment.AssignJson(values);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), @"{
  ""Name"": ""John"",
  ""Children"": [
    {
      ""Name"": ""Mary"",
    },
    {
      ""Name"": ""Moe"",
    }
  ],
}");
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
            environment.AssignJson(values);
            //------------Assert Results-------------------------
            var data = GetFromEnv(environment);
            Assert.IsTrue(data.JsonObjects.ContainsKey("Person"));
            var obj = data.JsonObjects["Person"] as JObject;
            if (obj != null)
            {
                Assert.AreEqual(obj.ToString(), @"{
  ""Name"": ""John"",
  ""Children"": [
    {
      ""Name"": ""Moe"",
    },
    {
      ""Name"": ""Moe"",
    }
  ],
}");
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

        private DataASTMutable.WarewolfEnvironment CreateTestEnvWithData()
        {

            IEnumerable<IAssignValue> assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "2"),
                 new AssignValue("[[rec().a]]", "4"),
                 new AssignValue("[[rec().a]]", "3"),
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
