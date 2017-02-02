/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.DynamicServices;
using Dev2.TO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.TOTests
{

    [TestClass]
    public class JsonMappingCompoundTests
    {
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToConstructor")]
        public void JsonMappingCompoundTo_Constructor_SetsProperties_NotIsCompound()
        {
            //------------Setup for test--------------------------
            // ReSharper disable RedundantArgumentName
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());

            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            //------------Execute Test---------------------------

            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo { SourceName = "[[a]]", DestinationName = "a" }
            );
            //------------Assert Results-------------------------
            Assert.IsNotNull(jsonMappingCompound);
            Assert.IsFalse(jsonMappingCompound.IsCompound);
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToConstructor")]
        public void JsonMappingCompoundTo_Constructor_SetsProperties_IsCompound()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            //------------Execute Test---------------------------

            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[a]],[[b]]",
                    DestinationName = "myName"
                }
            );
            //------------Assert Results-------------------------
            Assert.IsNotNull(jsonMappingCompound);
            Assert.IsTrue(jsonMappingCompound.IsCompound);
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToMaxCount")]
        public void JsonMappingCompoundTo_MaxCount_NotIsCompound()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            //------------Execute Test---------------------------

            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[rec().a]]",
                    DestinationName = "myName"
                }
            );
            //------------Assert Results-------------------------
            Assert.IsNotNull(jsonMappingCompound);
            Assert.IsFalse(jsonMappingCompound.IsCompound);
            Assert.AreEqual(jsonMappingCompound.MaxCount, 1);

            // check for list
            jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[rec(*).a]]",
                    DestinationName = "myName"
                }
            );
            Assert.IsFalse(jsonMappingCompound.IsCompound);
            Assert.AreEqual(jsonMappingCompound.MaxCount, 2);
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToMaxCount")]
        public void JsonMappingCompoundTo_MaxCount_IsCompound()
        {
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            //------------Execute Test---------------------------

            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[rec().a]],[[rec().b]]",
                    DestinationName = "myName"
                }
            );
            //------------Assert Results-------------------------
            Assert.IsNotNull(jsonMappingCompound);
            Assert.IsTrue(jsonMappingCompound.IsCompound);
            Assert.AreEqual(jsonMappingCompound.MaxCount, 1);

            // check for list
            jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[rec(*).a]],[[rec(*).b]]",
                    DestinationName = "myName"
                }
            );
            Assert.IsTrue(jsonMappingCompound.IsCompound);
            Assert.AreEqual(jsonMappingCompound.MaxCount, 2);
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToEvalResultIndexed")]
        public void JsonMappingCompoundTo_EvalResultIndexed_IsCompound()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            //------------Execute Test---------------------------

            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[a]],[[b]]",
                    DestinationName = "myName"
                }
            );
            //------------Assert Results-------------------------
            Assert.IsNotNull(jsonMappingCompound);
            Assert.IsTrue(jsonMappingCompound.IsCompound);
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToEvalResultIndexed")]
        public void JsonMappingCompoundTo_EvalResultIndexed_NotIsCompound()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            //------------Execute Test---------------------------

            // for scalars
            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[a]]",
                    DestinationName = "myName"
                }
            );
            //------------Assert Results-------------------------
            Assert.IsNotNull(jsonMappingCompound);
            Assert.IsFalse(jsonMappingCompound.IsCompound);
            Assert.AreEqual(jsonMappingCompound.EvaluatedResultIndexed(0), 10);
            Assert.AreEqual(jsonMappingCompound.EvaluatedResultIndexed(1), null);

            // for lists
            jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[rec(*).a]]",
                    DestinationName = "myName"
                }
            );
            Assert.IsFalse(jsonMappingCompound.IsCompound);
            Assert.AreEqual(new JObject(new JProperty("a", jsonMappingCompound.EvaluatedResultIndexed(0))).ToString(Formatting.None), "{\"a\":[50,60]}");
            jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
            compound: new JsonMappingTo
                {
                    SourceName = "[[rec(*).b]]",
                    DestinationName = "myName"
                }
            );
            Assert.AreEqual(new JObject(new JProperty("b", jsonMappingCompound.EvaluatedResultIndexed(1))).ToString(Formatting.None), "{\"b\":[500,600]}");
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToHasRecordSetInCompound")]
        public void JsonMappingCompoundTo_HasRecordSetInCompound()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            //------------Execute Test---------------------------

            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[a]],[[b]]",
                    DestinationName = "myName"
                }
            );
            //------------Assert Results-------------------------
            Assert.IsNotNull(jsonMappingCompound);
            Assert.IsTrue(jsonMappingCompound.IsCompound);
            Assert.IsFalse(jsonMappingCompound.HasRecordSetInCompound);

            jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
                compound: new JsonMappingTo
                {
                    SourceName = "[[rec()]],[[rec(*)]]",
                    DestinationName = "myName"
                }
            );
            Assert.IsTrue(jsonMappingCompound.IsCompound);
            Assert.IsTrue(jsonMappingCompound.HasRecordSetInCompound);
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToIsValid")]
        public void JsonMappingCompoundTo_IsValid()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            Assert.IsNotNull(JsonMappingCompoundTo.IsValidJsonMappingInput(null, null));
            Assert.IsNotNull(JsonMappingCompoundTo.IsValidJsonMappingInput(null, "a"));
            JsonMappingCompoundTo.IsValidJsonMappingInput("a", "a")
    .Should().BeNull();
            JsonMappingCompoundTo.IsValidJsonMappingInput("[[a]],[[b]]", "a")
                .Should().BeNull();
            JsonMappingCompoundTo.IsValidJsonMappingInput("[[a().a]]", "a")
                .Should().BeNull();
            JsonMappingCompoundTo.IsValidJsonMappingInput("[[a()]],[[b]]", "a")
                .Should().NotBeNull();
            JsonMappingCompoundTo.IsValidJsonMappingInput("[[a(*)]],[[b]],[[c().m]]", "a")
                .Should().NotBeNull();
            JsonMappingCompoundTo.IsValidJsonMappingInput("[[a().x]],[[b().y]]", "a")
                .Should().BeNull();

            JsonMappingCompoundTo.IsValidJsonMappingInput("a", "a")
                .Should().BeNull();

            JsonMappingCompoundTo.IsValidJsonMappingInput("a,b", "a")
    .Should().BeNull();
        }

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundTo_ComplexEvaluatedResultIndexed")]
        public void JsonMappingCompoundTo_ComplexEvaluatedResultIndexed()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            CheckComplexEvaluatedResultIndexed("[[a]],[[b]]", "myName", 0, @"{""myName"":{""a"":10,""b"":20}}", dataObject);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingCompoundTo_ComplexEvaluatedResultIndexed")]
        public void JsonMappingCompoundTo_ComplexEvaluatedResultIndexedRecset()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            CheckComplexEvaluatedResultIndexed("[[rec(*)]]", "myName", 0, @"{""rec"":[{""a"":50,""b"":500},{""a"":60,""b"":600}]}", dataObject);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingCompoundTo_ComplexEvaluatedResultIndexed")]
        public void JsonMappingCompoundTo_ComplexEvaluatedResultIndexedRecsetColumnMixed()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            CheckComplexEvaluatedResultIndexed("[[rec(*).a]],[[rec(*).b]]", "myName", 0, @"{""myName"":[{""a"":50,""b"":500},{""a"":60,""b"":600}]}", dataObject);
        }
        private void CheckComplexEvaluatedResultIndexed(string expression, string name, int index, string expected, DsfDataObject dataObject)
        {
            var jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
            compound: new JsonMappingTo
                {
                    SourceName = expression,
                    DestinationName = name
                }
            );
            var a = jsonMappingCompound.ComplexEvaluatedResultIndexed(index);
            if (a is JProperty)
            {
                var jp = new JObject { a };
                Assert.AreEqual(expected, jp
                .ToString(Formatting.None));
            }
            else
            {
                var jp = new JProperty(name, jsonMappingCompound.ComplexEvaluatedResultIndexed(index));
                var j = new JObject(jp);
                var s = j.ToString(Formatting.None);
                Assert.AreEqual(expected,
                s);
            }

        }

        // ReSharper restore RedundantArgumentName
    }
}
