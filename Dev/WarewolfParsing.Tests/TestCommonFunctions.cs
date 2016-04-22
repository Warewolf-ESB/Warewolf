﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;
// ReSharper disable InconsistentNaming

namespace WarewolfParsingTest
{
    [TestClass]
    public class TestCommonFunctions
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_MethodName")]
        public void CommonFunctions_MethodName_AtomToString_ExpectCorrectString()
        {
            //------------Setup for test--------------------------
           Assert.AreEqual(CommonFunctions.atomtoString(DataASTMutable.WarewolfAtom.Nothing),null);
           Assert.AreEqual(CommonFunctions.atomtoString(DataASTMutable.WarewolfAtom.NewDataString("!")),"!");
           Assert.AreEqual(CommonFunctions.atomtoString(DataASTMutable.WarewolfAtom.NewInt(1)), "1");
           Assert.AreEqual(CommonFunctions.atomtoString(DataASTMutable.WarewolfAtom.NewFloat(1.2345)), "1.2345");
           Assert.AreEqual(CommonFunctions.atomtoString(DataASTMutable.WarewolfAtom.NewPositionedValue(new Tuple<int,DataASTMutable.WarewolfAtom>(1,DataASTMutable.WarewolfAtom.NewDataString("a")))), "a");
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_MethodName")]
        public void CommonFunctions_MethodName_AtomRecordToString_ExpectCorrectString()
        {
            //------------Setup for test--------------------------
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataASTMutable.WarewolfAtom.Nothing), "");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataASTMutable.WarewolfAtom.NewDataString("!")), "!");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataASTMutable.WarewolfAtom.NewInt(1)), "1");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataASTMutable.WarewolfAtom.NewFloat(1.2345)), "1.2345");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataASTMutable.WarewolfAtom.NewPositionedValue(new Tuple<int, DataASTMutable.WarewolfAtom>(1, DataASTMutable.WarewolfAtom.NewDataString("a")))), "a");
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        public void CommonFunctions_AtomToJsonCompatable()
        {
            Assert.IsNull(CommonFunctions.atomToJsonCompatibleObject(DataASTMutable.WarewolfAtom.Nothing));
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataASTMutable.WarewolfAtom.NewFloat(1.2)),1.2);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataASTMutable.WarewolfAtom.NewInt(1)), 1);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataASTMutable.WarewolfAtom.NewDataString("true")), true);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataASTMutable.WarewolfAtom.NewDataString("false")), false);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataASTMutable.WarewolfAtom.NewDataString("trues")), "trues");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_AtomToInt()
        {
            CommonFunctions.atomToInt(DataASTMutable.WarewolfAtom.Nothing);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_AtomToInt_neg()
        {
            CommonFunctions.atomToInt(DataASTMutable.WarewolfAtom.NewInt(-1));
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        public void CommonFunctions_AtomToInt_Parsed()
        {
          Assert.AreEqual(1,  CommonFunctions.atomToInt(DataASTMutable.WarewolfAtom.NewDataString("1")));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        public void CommonFunctions_AtomToInt_ParsedInt()
        {
            Assert.AreEqual(1, CommonFunctions.atomToInt(DataASTMutable.WarewolfAtom.NewInt(1)));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        public void CommonFunctions_EvalResultToJsonCompatable()
        {
            var env = CreateEnvironmentWithData();
            var a = CommonFunctions.evalResultToString( WarewolfDataEvaluationCommon.eval(env, 0, "[[x]]"));
            Assert.AreEqual(a,"1");       
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        public void CommonFunctions_EvalResultToJsonCompatableRecset()
        {
            var env = CreateEnvironmentWithData();
            var a = CommonFunctions.evalResultToJsonCompatibleObject( WarewolfDataEvaluationCommon.eval(env, 0, "[[Rec(*).a]]"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_GetRecsetPosition")]
        public void CommonFunctions_GetRecsetPosition()
        {
            var env = CreateEnvironmentWithData();
            var rec = env.RecordSets["bec"];
            var a = CommonFunctions.getRecordSetIndex(rec,3);
            Assert.AreEqual(((CommonFunctions.PositionValue.IndexFoundPosition)a).Item,0);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_GetRecsetPosition")]
        public void CommonFunctions_GetRecsetPositionNonExistent()
        {
            var env = CreateEnvironmentWithData();
            var rec = env.RecordSets["bec"];
            var a = CommonFunctions.getRecordSetIndex(rec, 5);
            Assert.IsTrue(a.IsIndexDoesNotExist);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_GetRecsetPosition")]
        public void CommonFunctions_IsNothing()
        {
            var env = CreateEnvironmentWithData();

            Assert.IsTrue(CommonFunctions.isNothing(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataASTMutable.WarewolfAtom.Nothing)));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_GetRecsetPosition")]
        public void CommonFunctions_IsNothingNot()
        {
            var env = CreateEnvironmentWithData();

            Assert.IsFalse(CommonFunctions.isNothing(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataASTMutable.WarewolfAtom.NewDataString("A"))));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_GetRecsetPosition")]
        public void CommonFunctions_IsNothingLsit()
        {
            var env = CreateEnvironmentWithData();

            Assert.IsFalse(CommonFunctions.isNothing(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataASTMutable.WarewolfAtom>(DataASTMutable.WarewolfAtom.Nothing))));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_EvalResultToJsonCompatableJson()
        {
            var env = CreateEnvironmentWithData();
            var a = CommonFunctions.evalResultToJsonCompatibleObject(WarewolfDataEvaluationCommon.eval(env, 0, "[[Rec(*)]]"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_getLastIndexFromRecordSet()
        {
            var env = CreateEnvironmentWithData();
            var a = CommonFunctions.getLastIndexFromRecordSet("a",env);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_deleteValues()
        {
            var env = CreateEnvironmentWithData();
            var a = Delete.deleteValues("Resc",env);
         

        }

        public static DataASTMutable.WarewolfEnvironment CreateEnvironmentWithData()
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
            env.Assign("[[bec(3).b]]", "c", 0);
            env.Assign("[[bec(2).b]]", "c", 0);
            env.Assign("[[x]]", "1", 0);
            env.Assign("[[y]]", "y", 0);
            env.AssignJson(new AssignValue("[[Person.Name]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[Person.Age]]", "22"), 0);
            env.AssignJson(new AssignValue("[[Person.Spouse.Name]]", "dora"), 0);
            env.AssignJson(new AssignValue("[[Person.Children(1).Name]]", "Mary"), 0);
            env.AssignJson(new AssignValue("[[Person.Children(2).Name]]", "Jane"), 0);
            env.AssignJson(new AssignValue("[[Person.Score(1)]]", "2"), 0);
            env.AssignJson(new AssignValue("[[Person.Score(2)]]", "3"), 0);
            env.AssignJson(new AssignValue("[[array(1)]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[arrayObj(1).Name]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[arrayObj(2).Name]]", "bobe"), 0);
            PrivateObject p = new PrivateObject(env);
            return (DataASTMutable.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }

    }
}
