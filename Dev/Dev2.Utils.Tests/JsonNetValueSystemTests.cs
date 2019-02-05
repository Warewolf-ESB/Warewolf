/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;

namespace Dev2.Utils.Tests
{
    [TestClass]
    public class JsonNetValueSystemTests
    {
        public IJsonPathValueSystem ValueSystem { get; set; }
        readonly string _jsonObject = "{\"results\":[" +
               "{\"employeename\":\"name1\",\"employeesupervisor\":\"supervisor1\"}," +
               "{\"employeename\":\"name2\",\"employeesupervisor\":\"supervisor1\"}," +
               "{\"employeename\":\"name3\",\"employeesupervisor\":[\"supervisor1\",\"supervisor2\"]}" +
               "]}";
        readonly string _jsonArray = @"[  'Small',  'Medium',  'Large']";

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_HasMember_JObject_ReturnTrue()
        {
            var results = JObject.Parse(_jsonObject);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsTrue(ValueSystem.HasMember(results, "results"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_HasMember_JObject_ReturnFalse()
        {
            var results = JObject.Parse(_jsonObject);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsFalse(ValueSystem.HasMember(results, "teststring"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_HasMember_JArray_ReturnTrue()
        {
            var results = JArray.Parse(_jsonArray);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsTrue(ValueSystem.HasMember(results, "2"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_HasMember_JArray_ReturnFalse()
        {
            var results = JArray.Parse(_jsonArray);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsFalse(ValueSystem.HasMember(results, "6"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_HasMember_JArray_ReturnFalse_DefaultValue()
        {
            var results = JArray.Parse(_jsonArray);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsFalse(ValueSystem.HasMember(results, "-1"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_HasMember_Object_ReturnFalse()
        {
            ValueSystem = new JsonNetValueSystem();
            Assert.IsFalse(ValueSystem.HasMember(_jsonArray, "6"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_GetMembers()
        {
            var results = JObject.Parse(_jsonObject);
            var result = "";
            ValueSystem = new JsonNetValueSystem();
            foreach (string key in ValueSystem.GetMembers(results))
            {
                result = key;
            }
            Assert.AreEqual("results", result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_JObject_ReturnTrue()
        {
            var results = JObject.Parse(_jsonObject);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsTrue(ValueSystem.IsObject(results));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_JObject_ReturnFalse()
        {
            ValueSystem = new JsonNetValueSystem();
            Assert.IsFalse(ValueSystem.IsObject(new object()));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_IsArray_ReturnTrue()
        {
            var results = JArray.Parse(_jsonArray);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsTrue(ValueSystem.IsArray(results));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_IsArray_ReturnFalse()
        {
            var results = JObject.Parse(_jsonObject);
            ValueSystem = new JsonNetValueSystem();
            Assert.IsFalse(ValueSystem.IsArray(results));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_IsPrimitive_ReturnTrue()
        {
            ValueSystem = new JsonNetValueSystem();
            Assert.IsTrue(ValueSystem.IsPrimitive(new object()));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_IsPrimitive_ReturnFalse()
        {
            ValueSystem = new JsonNetValueSystem();
            Assert.IsFalse(ValueSystem.IsArray(new JObject()));
            Assert.IsFalse(ValueSystem.IsPrimitive(new JArray()));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JsonNetValueSystem_IsPrimitive_ReturnException()
        {
            ValueSystem = new JsonNetValueSystem();
            ValueSystem.IsPrimitive(null);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_GetMemberValue_JObject()
        {
            var myJsonString = "{report: {Id: \"aaakkj98898983\"}}";
            ValueSystem = new JsonNetValueSystem();
            var obj = ValueSystem.GetMemberValue(JObject.Parse(myJsonString), "report");
            Assert.AreEqual("{\r\n  \"Id\": \"aaakkj98898983\"\r\n}".ToString(), obj.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_GetMemberValue_JArray()
        {
            var results = JArray.Parse(_jsonArray);
            ValueSystem = new JsonNetValueSystem();
            var obj = ValueSystem.GetMemberValue(results, "1");
            Assert.AreEqual("Medium", obj.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonNetValueSystem))]
        public void JsonNetValueSystem_GetMemberValue_ReturnNull()
        {
            ValueSystem = new JsonNetValueSystem();
            var obj = ValueSystem.GetMemberValue(new object(), "results");
            Assert.IsNull(obj);
        }
    }
}
