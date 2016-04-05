using System;
using System.Collections;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;
using Dev2.Communication;
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
        
    }
}
