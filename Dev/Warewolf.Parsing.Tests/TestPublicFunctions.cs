using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Warewolf.Data;
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
            var env = CreateEnvironmentWithData();

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
            Assert.IsNotNull(PublicFunctions.EvalWithPositions("[[Rec(*).a]]", 0, env));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PublicFunctions_AddRecsetToEnvironment")]
        public void PublicFunctions_EvalrecsetIndexes()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(PublicFunctions.EvalRecordSetIndexes("[[Rec(*).a]]", 0, env)), "1,2,3,2");
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
            Assert.IsTrue(PublicFunctions.RecordsetExpressionExists("[[Rec(*).a]]", env));
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

            var lst = new List<DataStorage.WarewolfAtom>() { DataStorage.WarewolfAtom.Nothing, DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(2, DataStorage.WarewolfAtom.NewDataString("a"))), DataStorage.WarewolfAtom.NewDataString("A") };
            //------------Execute Test---------------------------
            var res = PublicFunctions.AtomListToSearchTo(lst);
            var recordSetSearchPayloads = res as RecordSetSearchPayload[] ?? res.ToArray();
            Assert.AreEqual(recordSetSearchPayloads.First().Index, 0);
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

            var env = new ExecutionEnvironment();
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
            var p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("PublicFunctions")]
        public void PublicFunctions_EvalEnv_ShouldReturn()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var res = PublicFunctions.EvalEnv(env);

            var sb = new StringBuilder();
            foreach (var s in res)
            {
                sb.Append(s);
            }
            var expected = "{\"scalars\":{\"r\":\"s\",\"s\":\"s\",\"x\":1,\"y\":\"y\"},\"record_sets\":{\"Rec\":{\"WarewolfPositionColumn\":[1,2,3,4],\"a\":[1,2,3,2],\"b\":[\"a\",\"b\",\"c\",\"c\"]}},\"json_objects\":{\"Person\":{\"Name\":\"bob\",\"Age\":22,\"Spouse\":{\"Name\":\"dora\"},\"Children\":[{\"Name\":\"Mary\"},{\"Name\":\"Jane\"}],\"Score\":[2,3]},\"array\":[\"bob\"],\"arrayObj\":[{\"Name\":\"bob\"},{\"Name\":\"bobe\"}]}}";

            var actual = sb.ToString();
            Assert.AreEqual(expected, actual);
            var jsonOb = JsonConvert.DeserializeObject(actual);
            Assert.IsNotNull(jsonOb);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("PublicFunctions_Eval")]
        public void PublicFunctionsEvalEnvExpressionToTable()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            var lst = new List<DataStorage.WarewolfAtom>
            {
                DataStorage.WarewolfAtom.Nothing,
                DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(2, DataStorage.WarewolfAtom.NewDataString("a"))),
                DataStorage.WarewolfAtom.NewDataString("A")
            };
            //------------Execute Test---------------------------
            var res = PublicFunctions.EvalEnvExpressionToTable("[[Rec(*)]]", 0, env, true);

            Assert.IsNotNull(res);

            var allTestData = new string[][][] {
                new string[][] {
                    new string [] {"a", "1" },
                    new string [] {"b", "a" },
                },
                new string[][]
                {
                    new string [] {"a", "2" },
                    new string [] {"b", "b" },
                },
                new string[][]
                {
                    new string [] {"a", "3" },
                    new string [] {"b", "c" },
                },
                new string[][]
                {
                    new string [] {"a", "2" },
                    new string [] {"b", "c" },
                },

            };

            Assert.AreEqual(4, res.Count());

            var combined = allTestData.Zip(res, (test, result) => (test, result));

            foreach (var (testdata, rowTuple) in combined)
            {
                var index = 0;
                foreach (var (field, value) in rowTuple)
                {
                    Assert.IsTrue(field == testdata[index][0]);
                    Assert.IsTrue(value.Equals(testdata[index][1]));
                    index++;
                }
            }
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("PublicFunctions")]
        public void PublicFunctions_EvalEnv_ShouldReturn1()
        {
            //------------Setup for test--------------------------
            var e = new ExecutionEnvironment();
            var p = new PrivateObject(e);
            var env = (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");

            //------------Execute Test---------------------------
            var res = PublicFunctions.EvalEnv(env);

            var sb = new StringBuilder();
            foreach (var s in res)
            {
                sb.Append(s);
            }
            var expected = "{\"scalars\":{}," +
                             "\"record_sets\":{}," +
                             "\"json_objects\":{}}";

            var actual = sb.ToString();
            Assert.AreEqual(expected, actual);
            var jsonOb = JsonConvert.DeserializeObject(actual);
            Assert.IsNotNull(jsonOb);
        }

        [TestCategory("PublicFunctions_Eval")]
        public void PublicFunctionsEvalEnvExpressionToArrayTable()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            var lst = new List<DataStorage.WarewolfAtom>
            {
                DataStorage.WarewolfAtom.Nothing,
                DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(2, DataStorage.WarewolfAtom.NewDataString("a"))),
                DataStorage.WarewolfAtom.NewDataString("A")
            };
            //------------Execute Test---------------------------
            var enumerator = PublicFunctions.EvalEnvExpressionToArrayTable("[[Rec(*)]]", 0, env, true);
            var res = enumerator.ToArray();

            Assert.IsNotNull(res);

            Assert.AreEqual(5, res.Length);

            Assert.AreEqual(2, res[0].Length);
            Assert.AreEqual(2, res[1].Length);
            Assert.AreEqual(2, res[2].Length);
            Assert.AreEqual(2, res[3].Length);

            // column names in first row
            Assert.IsTrue(res[0][0].Equals("a"));
            Assert.IsTrue(res[0][1].Equals("b"));

            // data
            Assert.IsTrue(res[1][0].Equals("1"));
            Assert.IsTrue(res[1][1].Equals("a"));
            Assert.IsTrue(res[2][0].Equals("2"));
            Assert.IsTrue(res[2][1].Equals("b"));
            Assert.IsTrue(res[3][0].Equals("3"));
            Assert.IsTrue(res[3][1].Equals("c"));
            Assert.IsTrue(res[4][0].Equals("2"));
            Assert.IsTrue(res[4][1].Equals("c"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("PublicFunctions_Eval")]
        public void PublicFunctionsEvalEnvExpressionToArrayTable_Throws()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            var lst = new List<DataStorage.WarewolfAtom>
            {
                DataStorage.WarewolfAtom.Nothing,
                DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(2, DataStorage.WarewolfAtom.NewDataString("a"))),
                DataStorage.WarewolfAtom.NewDataString("A")
            };
            //------------Execute Test---------------------------
            Assert.ThrowsException<Dev2.Common.Common.NullValueInVariableException>(() =>
            {
                var enumerator = PublicFunctions.EvalEnvExpressionToArrayTable("[[NotExistingRec(*)]]", 0, env, true);
                var res = enumerator.ToArray();
            });
        }
    }
}
