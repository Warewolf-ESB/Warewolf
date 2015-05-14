
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.DynamicServices;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using FluentAssertions;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonMappingEvaluatedTests
    {
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingEvaluated_Constructor")]
        public void JsonMappingEvaluated_Constructor_SetsProperties()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[as]]", "hellow world");
            dataObject.Environment.Assign("[[af]]", "9.9");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
            //------------Execute Test---------------------------

            // scalar evaluating to atom
            string sn = "[[a]]", sns = "[[as]]", snf = "[[af]]",
                dn = "a", dns = "as", dnf = "af";
            var scalarsSn = new[] { sn, sns, snf };
            var scalarsDn = new[] { dn, dns, dnf };
            var scalarsV = new object[] { (int)10, (string)"hellow world", (double)9.9 };
            for (int i = 0; i < scalarsSn.Length; i++)
            {
                var jsonMappingEvaluatedLocal = new JsonMappingEvaluated(
                    env: dataObject.Environment,
                    sourceName: scalarsSn[i]);
                //------------Assert Results-------------------------
                jsonMappingEvaluatedLocal.Should().NotBeNull();
                jsonMappingEvaluatedLocal.Simple.Should().NotBeNull();
                jsonMappingEvaluatedLocal.Simple.SourceName.Should().Be(scalarsSn[i]);
                jsonMappingEvaluatedLocal.Simple.DestinationName.Should().Be(scalarsDn[i]);
                jsonMappingEvaluatedLocal.EvalResult.Should().Be(dataObject.Environment.Eval(scalarsSn[i]));
                jsonMappingEvaluatedLocal.EvalResultAsObject.Should().Be(scalarsV[i]);
                jsonMappingEvaluatedLocal.Count.Should().Be(1);
            }


            // recordset
            sn = "[[rec().a]]"; dn = "a";
            var jsonMappingEvaluated = new JsonMappingEvaluated(
                env: dataObject.Environment,
                sourceName: sn);
            //------------Assert Results-------------------------
            jsonMappingEvaluated.Should().NotBeNull();
            jsonMappingEvaluated.Simple.Should().NotBeNull();
            jsonMappingEvaluated.Simple.SourceName.Should().Be(sn);
            jsonMappingEvaluated.Simple.DestinationName.Should().Be(dn);
            ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult)jsonMappingEvaluated.EvalResult).Item.GetValue(0).Should().Be(
                ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult)dataObject.Environment.Eval(sn)).Item.GetValue(0));

            // recordset name
            sn = "[[rec()]]"; dn = "rec";
            jsonMappingEvaluated = new JsonMappingEvaluated(
                env: dataObject.Environment,
                sourceName: sn);
            //------------Assert Results-------------------------
            jsonMappingEvaluated.Should().NotBeNull();
            jsonMappingEvaluated.Simple.Should().NotBeNull();
            jsonMappingEvaluated.Simple.SourceName.Should().Be(sn);
            jsonMappingEvaluated.Simple.DestinationName.Should().Be(dn);
            //jsonMappingEvaluated.EvalResult.Should().Be(dataObject.Environment.Eval(sn));
            //((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult)jsonMappingEvaluated.EvalResult).Item.Data["a"][0].Should().Be(
            //  ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult)dataObject.Environment.Eval(sn)).Item.Data["a"][0]);
        }
    }

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonMappingCompoundTests
    {
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToConstructor")]
        public void JsonMappingCompoundTo_Constructor_SetsProperties_NotIsCompound()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
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
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
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
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
            dataObject.Environment.Assign("[[rec(2).a]]", "60");
            dataObject.Environment.Assign("[[rec(2).b]]", "600");
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
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
            dataObject.Environment.Assign("[[rec(2).a]]", "60");
            dataObject.Environment.Assign("[[rec(2).b]]", "600");
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
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
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
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
            dataObject.Environment.Assign("[[rec(2).a]]", "60");
            dataObject.Environment.Assign("[[rec(2).b]]", "600");
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
            Assert.AreEqual((new JObject(new JProperty("a", jsonMappingCompound.EvaluatedResultIndexed(0)))).ToString(Formatting.None), "{\"a\":[50,60]}");
            jsonMappingCompound = new JsonMappingCompoundTo(
                env: dataObject.Environment,
            compound: new JsonMappingTo
                {
                    SourceName = "[[rec(*).b]]",
                    DestinationName = "myName"
                }
            );
            Assert.AreEqual((new JObject(new JProperty("b", jsonMappingCompound.EvaluatedResultIndexed(1)))).ToString(Formatting.None), "{\"b\":[500,600]}");
        }
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingCompoundToHasRecordSetInCompound")]
        public void JsonMappingCompoundTo_HasRecordSetInCompound()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10");
            dataObject.Environment.Assign("[[b]]", "20");
            dataObject.Environment.Assign("[[rec(1).a]]", "50");
            dataObject.Environment.Assign("[[rec(1).b]]", "500");
            dataObject.Environment.Assign("[[rec(2).a]]", "60");
            dataObject.Environment.Assign("[[rec(2).b]]", "600");
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
    }
}
