using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;


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
           Assert.AreEqual(CommonFunctions.atomtoString(DataStorage.WarewolfAtom.Nothing),null);
           Assert.AreEqual(CommonFunctions.atomtoString(DataStorage.WarewolfAtom.NewDataString("!")),"!");
           Assert.AreEqual(CommonFunctions.atomtoString(DataStorage.WarewolfAtom.NewInt(1)), "1");
           Assert.AreEqual(CommonFunctions.atomtoString(DataStorage.WarewolfAtom.NewFloat(1.2345)), "1.2345");
           Assert.AreEqual(CommonFunctions.atomtoString(DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int,DataStorage.WarewolfAtom>(1,DataStorage.WarewolfAtom.NewDataString("a")))), "a");
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        DataStorage.WarewolfEnvironment ExtractEnvironment(ExecutionEnvironment env)
        {
            var p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("CommonFunctions")]
        public void CommonFunctions_atomtoString_GivenJson_ExpectCorrectJson()
        {
            //------------Setup for test--------------------------
            var eEnv = new ExecutionEnvironment();
            eEnv.AssignJson(new AssignValue("[[@a]]", "{\"aa\":1}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.b]]", "{\"bb\":2}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.c]]", "[[@a]]"), 0);
            eEnv.AssignJson(new AssignValue("[[@a2(1)]]", "{\"indexNested\": 3}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.d]]", "[[@a2]]"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.e()]]", "{\"ee\":4}"), 0);
            var env = ExtractEnvironment(eEnv);

            var result = EvaluationFunctions.eval(env, 0, false, "[[@a]]");
            var a = CommonFunctions.evalResultToString(result);

            var json = (JObject)JsonConvert.DeserializeObject(a);
            Assert.AreEqual(1, json["aa"]);
            Assert.AreEqual(2, json["b"]["bb"]);
            Assert.AreEqual(1, json["c"]["aa"]);
            Assert.AreEqual(2, json["c"]["b"]["bb"]);
            Assert.AreEqual(3, json["d"][0]["indexNested"]);
            Assert.AreEqual(1, ((JArray)json["d"]).Count);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("CommonFunctions")]
        public void CommonFunctions_atomtoString_GivenCircularJson_ExpectCorrectJson()
        {
            //------------Setup for test--------------------------
            var eEnv = new ExecutionEnvironment();
            eEnv.AssignJson(new AssignValue("[[@a]]", "{\"aa\":1}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.b()]]", "{\"bb\":2}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.c(1).a1]]", "[[@a]]"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.c(1).a1.aa]]", "55"), 0);
            var env = ExtractEnvironment(eEnv);

            var result = EvaluationFunctions.eval(env, 0, false, "[[@a]]");
            var a = CommonFunctions.evalResultToString(result);

            var json = (JObject)JsonConvert.DeserializeObject(a);
            Assert.AreEqual(1, json["aa"]);
            Assert.AreEqual(1, ((JArray)json["b"]).Count);
            Assert.AreEqual(2, json["b"][0]["bb"]);
            var c1 = ((JArray)json["c"])[0];
            Assert.AreEqual(55, c1["a1"]["aa"]);
            Assert.AreEqual(2, c1["a1"]["b"][0]["bb"]);
            Assert.IsFalse(((JObject)c1["a1"]).ContainsKey("c"));
            //Assert.AreEqual(1, ((JArray)c1["a1"]["c"]).Count); // this should not exist, it is an artifact of the assign in progress when c(1).a1 is being assigned its parent @a
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("CommonFunctions")]
        public void CommonFunctions_atomtoString_GivenJsonAppend_ExpectCorrectJson()
        {
            //------------Setup for test--------------------------
            var eEnv = new ExecutionEnvironment();
            eEnv.AssignJson(new AssignValue("[[@a]]", "{\"aa\":1}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.b]]", "{\"bb\":2}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.c]]", "[[@a]]"), 0);
            eEnv.AssignJson(new AssignValue("[[@a2(1)]]", "{\"indexNested\": 3}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.d]]", "[[@a2]]"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.e()]]", "{\"ee\":4}"), 0);
            var env = ExtractEnvironment(eEnv);

            var result = EvaluationFunctions.eval(env, 0, false, "[[@a]]");
            var a = CommonFunctions.evalResultToString(result);
            var json = (JObject)JsonConvert.DeserializeObject(a);
            Assert.AreEqual(1, json["aa"]);
            Assert.AreEqual(2, json["b"]["bb"]);
            Assert.AreEqual(1, json["c"]["aa"]);
            Assert.AreEqual(2, json["c"]["b"]["bb"]);
            Assert.AreEqual(1, ((JArray)json["d"]).Count);
            Assert.AreEqual(3, json["d"][0]["indexNested"]);
            Assert.AreEqual(1, ((JArray)json["e"]).Count);
            Assert.AreEqual(4, json["e"][0]["ee"]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("CommonFunctions")]
        public void CommonFunctions_atomtoString_GivenJsonEmptyArray_ExpectCorrectJson()
        {
            //------------Setup for test--------------------------
            var eEnv = new ExecutionEnvironment();
            eEnv.AssignJson(new AssignValue("[[@a]]", "{\"aa\":1}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.b]]", "{\"bb\":2}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.c]]", "[[@a]]"), 0);
            eEnv.AssignJson(new AssignValue("[[@a2(1)]]", "{\"indexNested\": 3}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.d]]", "[[@a2]]"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.e()]]", "{\"ee\":4}"), 0);
            eEnv.AssignJson(new AssignValue("[[@a.f]]", "[]"), 0);
            var env = ExtractEnvironment(eEnv);

            var result = EvaluationFunctions.eval(env, 0, false, "[[@a]]");
            var a = CommonFunctions.evalResultToString(result);
            var json = (JObject)JsonConvert.DeserializeObject(a);
            Assert.AreEqual(1, json["aa"]);
            Assert.AreEqual(2, json["b"]["bb"]);
            Assert.AreEqual(1, json["c"]["aa"]);
            Assert.AreEqual(2, json["c"]["b"]["bb"]);
            Assert.AreEqual(1, ((JArray)json["d"]).Count);
            Assert.AreEqual(3, json["d"][0]["indexNested"]);
            Assert.AreEqual(1, ((JArray)json["e"]).Count);
            Assert.AreEqual(4, json["e"][0]["ee"]);
            Assert.AreEqual(0, ((JArray)json["f"]).Count, "empty json array expected");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_MethodName")]
        public void CommonFunctions_MethodName_AtomRecordToString_ExpectCorrectString()
        {
            //------------Setup for test--------------------------
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataStorage.WarewolfAtom.Nothing), "");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataStorage.WarewolfAtom.NewDataString("!")), "!");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataStorage.WarewolfAtom.NewInt(1)), "1");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataStorage.WarewolfAtom.NewFloat(1.2345)), "1.2345");
            Assert.AreEqual(CommonFunctions.warewolfAtomRecordtoString(DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(1, DataStorage.WarewolfAtom.NewDataString("a")))), "a");
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        public void CommonFunctions_AtomToJsonCompatable()
        {
            Assert.IsNull(CommonFunctions.atomToJsonCompatibleObject(DataStorage.WarewolfAtom.Nothing));
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataStorage.WarewolfAtom.NewFloat(1.2)),1.2);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataStorage.WarewolfAtom.NewInt(1)), 1);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataStorage.WarewolfAtom.NewDataString("true")), true);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataStorage.WarewolfAtom.NewDataString("false")), false);
            Assert.AreEqual(CommonFunctions.atomToJsonCompatibleObject(DataStorage.WarewolfAtom.NewDataString("trues")), "trues");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_AtomToInt()
        {
            CommonFunctions.atomToInt(DataStorage.WarewolfAtom.Nothing);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_AtomToInt_neg()
        {
            CommonFunctions.atomToInt(DataStorage.WarewolfAtom.NewInt(-1));
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        public void CommonFunctions_AtomToInt_Parsed()
        {
          Assert.AreEqual(1,  CommonFunctions.atomToInt(DataStorage.WarewolfAtom.NewDataString("1")));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToInt")]
        public void CommonFunctions_AtomToInt_ParsedInt()
        {
            Assert.AreEqual(1, CommonFunctions.atomToInt(DataStorage.WarewolfAtom.NewInt(1)));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        public void CommonFunctions_EvalResultToJsonCompatable()
        {
            var env = CreateEnvironmentWithData();
            var a = CommonFunctions.evalResultToString( EvaluationFunctions.eval(env, 0, false, "[[x]]"));
            Assert.AreEqual(a,"1");       
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        public void CommonFunctions_EvalResultToJsonCompatableRecset()
        {
            var env = CreateEnvironmentWithData();
            var a = CommonFunctions.evalResultToJsonCompatibleObject( EvaluationFunctions.eval(env, 0, false, "[[Rec(*).a]]"));

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

            Assert.IsTrue(CommonFunctions.isNothing(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing)));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_GetRecsetPosition")]
        public void CommonFunctions_IsNothingNot()
        {
            var env = CreateEnvironmentWithData();

            Assert.IsFalse(CommonFunctions.isNothing(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("A"))));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_GetRecsetPosition")]
        public void CommonFunctions_IsNothingLsit()
        {
            var env = CreateEnvironmentWithData();

            Assert.IsFalse(CommonFunctions.isNothing(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.Nothing))));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CommonFunctions_AtomToJsonCompatable")]
        [ExpectedException(typeof(Exception))]
        public void CommonFunctions_EvalResultToJsonCompatableJson()
        {
            var env = CreateEnvironmentWithData();
            var a = CommonFunctions.evalResultToJsonCompatibleObject(EvaluationFunctions.eval(env, 0, false, "[[Rec(*)]]"));

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

        public static DataStorage.WarewolfEnvironment CreateEnvironmentWithData()
        {

            var env = new ExecutionEnvironment();
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

            env.AssignJson(new AssignValue("[[@a]]", "{\"aa\":1}"), 0);
            env.AssignJson(new AssignValue("[[@a.b]]", "{\"bb\":2}"), 0);
            env.AssignJson(new AssignValue("[[@a.c]]", "[[@a]]"), 0);
            env.AssignJson(new AssignValue("[[@a2(1)]]", "{\"indexNested\": 3}"), 0);
            env.AssignJson(new AssignValue("[[@a.d]]", "[[@a2]]"), 0);


            var p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }
    }
}
