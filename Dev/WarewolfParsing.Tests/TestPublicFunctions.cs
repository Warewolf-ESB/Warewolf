using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace WarewolfParsingTest
{
    [TestClass]
    public class TestPublicFunctions
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_AddRecsetToEnvironment_NonExistent_ExpectAdded()
        {
            //------------Setup for test--------------------------
            var env =CreateEnvironmentWithData();
            
            //------------Execute Test---------------------------
            env = PublicFunctions.AddRecsetToEnv("bob", env);
            //------------Assert Results-------------------------
            Assert.IsTrue(env.RecordSets.ContainsKey("bob"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_AddRecsetToEnvironment_Existent_ExpectExisting()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            PublicFunctions.AddRecsetToEnv("Rec", env);
            //------------Assert Results-------------------------
            Assert.IsTrue(env.RecordSets.ContainsKey("Rec"));
            Assert.IsTrue(env.RecordSets["Rec"].Data.ContainsKey("a"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_EvalWithPositions_PassesThrough()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
           Assert.IsNotNull(  PublicFunctions.EvalWithPositions("[[Rec(*).a]]", 0, env));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_EvalrecsetIndexes()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString( PublicFunctions.EvalRecordSetIndexes("[[Rec(*).a]]", 0, env)),"1,2,3,2");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_EvalIndexes()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            Assert.AreEqual(PublicFunctions.GetIndexes("[[Rec(*)]]", 0, env).ToArray()[0], 1);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_RecsetExists()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            Assert.IsTrue(PublicFunctions.RecordsetExpressionExists("[[Rec(*).a]]",  env));
            Assert.IsFalse(PublicFunctions.RecordsetExpressionExists("[[Rec(*)]]", env));
            Assert.IsFalse(PublicFunctions.RecordsetExpressionExists("[[Rec]]", env));
            Assert.IsFalse(PublicFunctions.RecordsetExpressionExists("", env));
            Assert.IsFalse(PublicFunctions.RecordsetExpressionExists("[[Rec]]  ", env));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_IsValidRecsetExp()
        {
            Assert.IsTrue(PublicFunctions.IsValidRecsetExpression("[[a]]"));
            Assert.IsTrue(PublicFunctions.IsValidRecsetExpression("[[rec().a]]"));
            Assert.IsTrue(PublicFunctions.IsValidRecsetExpression("[[rec(1).a]]"));
            Assert.IsTrue(PublicFunctions.IsValidRecsetExpression("[[rec(*).a]]"));
            Assert.IsTrue(PublicFunctions.IsValidRecsetExpression("[[rec([[a]]).a]]"));
            Assert.IsTrue(PublicFunctions.IsValidRecsetExpression("[[@a.b.c]]"));
            Assert.IsTrue(PublicFunctions.IsValidRecsetExpression("a"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AtomListToSearchTo")]
        public void PublicFunctions_AtomListToSearchTo()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            var lst = new List<DataStorage.WarewolfAtom>(){DataStorage.WarewolfAtom.Nothing,DataStorage.WarewolfAtom.NewPositionedValue( new Tuple<int,DataStorage.WarewolfAtom>( 2,DataStorage.WarewolfAtom.NewDataString("a"))),DataStorage.WarewolfAtom.NewDataString("A")};
            //------------Execute Test---------------------------
            var res = PublicFunctions.AtomListToSearchTo(lst);
            var recordSetSearchPayloads = res as RecordSetSearchPayload[] ?? res.ToArray();
            Assert.AreEqual(recordSetSearchPayloads.First().Index,0);
            Assert.AreEqual(recordSetSearchPayloads.First().Payload, null);
            Assert.AreEqual(recordSetSearchPayloads.Last().Index, 2);
            Assert.AreEqual(recordSetSearchPayloads.Last().Payload, "A");
            Assert.AreEqual(recordSetSearchPayloads[1].Index, 2);
            Assert.AreEqual(recordSetSearchPayloads[1].Payload, "a");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AtomListToSearchTo")]
        public void PublicFunctionsRecsetToSearchTo()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            var lst = new List<DataStorage.WarewolfAtom>() { DataStorage.WarewolfAtom.Nothing, DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(2, DataStorage.WarewolfAtom.NewDataString("a"))), DataStorage.WarewolfAtom.NewDataString("A") };
            //------------Execute Test---------------------------
            var res = PublicFunctions.RecordsetToSearchTo(env.RecordSets["Rec"]);
            var recordSetSearchPayloads = res as RecordSetSearchPayload[] ?? res.ToArray();
            Assert.AreEqual(recordSetSearchPayloads[0].Index, 1);
            Assert.AreEqual(recordSetSearchPayloads[0].Payload, "1");
            Assert.AreEqual(recordSetSearchPayloads[1].Index, 2);
            Assert.AreEqual(recordSetSearchPayloads[1].Payload, "2");
            Assert.AreEqual(recordSetSearchPayloads[2].Index, 3);
            Assert.AreEqual(recordSetSearchPayloads[2].Payload, "3");
            Assert.AreEqual(recordSetSearchPayloads[3].Index, 4);
            Assert.AreEqual(recordSetSearchPayloads[3].Payload, "4");
        }


        public static DataStorage.WarewolfEnvironment CreateEnvironmentWithData()
        {

            ExecutionEnvironment env = new ExecutionEnvironment();
            env.Assign("[[Rec(1).a]]", "1", 0);
            env.Assign("[[Rec(2).a]]", "2", 0);
            env.Assign("[[Rec(3).a]]", "3", 0);
            env.Assign("[[Rec(4).a]]", "2", 0);
            env.Assign("[[Rec(1).b]]", "a", 0);
            env.Assign("[[Rec(2).b]]", "b", 0);
            env.Assign("[[Rec(3).b]]", "c", 0);
            env.Assign("[[Rec(4).b]]", "c", 0);
            env.Assign("[[x]]", "1", 0);
            env.Assign("[[y]]", "y", 0);
            env.Assign("[[r]]", "s", 0);
            env.Assign("[[s]]", "s", 0);
            env.AssignJson(new AssignValue("[[@Person.Name]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[@Person.Age]]", "22"), 0);
            env.AssignJson(new AssignValue("[[@Person.Spouse.Name]]", "dora"), 0);
            env.AssignJson(new AssignValue("[[@Person.Children(1).Name]]", "Mary"), 0);
            env.AssignJson(new AssignValue("[[@Person.Children(2).Name]]", "Jane"), 0);
            env.AssignJson(new AssignValue("[[@Person.Score(1)]]", "2"), 0);
            env.AssignJson(new AssignValue("[[@Person.Score(2)]]", "3"), 0);
            env.AssignJson(new AssignValue("[[array(1)]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[arrayObj(1).Name]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[arrayObj(2).Name]]", "bobe"), 0);
            PrivateObject p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }
    }
}
