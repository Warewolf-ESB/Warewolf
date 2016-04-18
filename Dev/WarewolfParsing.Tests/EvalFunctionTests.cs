using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;
// ReSharper disable InconsistentNaming

namespace WarewolfParsingTest
{
    [TestClass]
    public class EvalFunctionTests
    {


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_Scalar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[x]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "1" );
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[Rec().a]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "3" );
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSetWithIndexExpression()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[Rec([[x]]).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Index()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[Rec(1).a]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "1" );
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[Rec(*).a]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "1,2,3" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[Rec(*).a]][[x]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "1,2,31" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Json()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[Person.Age]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "22" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Json_Array()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[Person.Score(1)]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "2" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Json_Array_star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  "[[Person.Score(*)]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "2,3" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Atom()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  " 1 2 3" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  " 1 2 3" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_RecsetResult()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0,  " [[Rec()]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  " 3,c" );
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsScalar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[x]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[Rec().a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "3");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Index()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[Rec(1).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[Rec(*).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1,2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[Rec(*).a]][[x]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1,2,31");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_PositionsRecSet_Json()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[Person.Age]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "22");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_PositionsRecSet_Json_Array()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[Person.Score(1)]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "2");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_PositionsRecSet_Json_Array_star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, "[[Person.Score(*)]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Atom()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, " 1 2 3");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), " 1 2 3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_RecsetResult()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalWithPositions(env, 0, " [[Rec()]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), " 3,c");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_evalIndexes_IndexesValid()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalIndexes(env, 0, "[[Rec()]]");
            //------------Assert Results-------------------------
            var enumerable = res as int[] ?? res.ToArray();
            Assert.AreEqual(enumerable.ToArray()[0],1);
            Assert.AreEqual(enumerable.ToArray()[1], 2);
            Assert.AreEqual(enumerable.ToArray()[2], 3);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_evalIndexes_IndexesValid_RecsetName()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalIndexes(env, 0, " [[Rec().a]]");
            //------------Assert Results-------------------------
            var enumerable = res as int[] ?? res.ToArray();
            Assert.AreEqual(enumerable.ToArray()[0], 1);
            Assert.AreEqual(enumerable.ToArray()[1], 2);
            Assert.AreEqual(enumerable.ToArray()[2], 3);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_evalIndexes_IndexesValid_RecsetName_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalIndexes(env, 0, " [[Rec()]][[Rec().a]]");
            //------------Assert Results-------------------------
            var enumerable = res as int[] ?? res.ToArray();
            Assert.AreEqual(enumerable.ToArray()[0], 1);
            Assert.AreEqual(enumerable.ToArray()[1], 2);
            Assert.AreEqual(enumerable.ToArray()[2], 3);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_Json()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[Person.Age]]");
            //------------Assert Results-------------------------
           Assert.AreEqual("22",CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonNested()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[Person.Spouse.Name]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("dora", CommonFunctions.evalResultToString(res));
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonNestedArray()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[Person.Children(1).Name]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("Mary", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonNestedArrayOfObjects()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[Person.Children(*).Name]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("Mary,Jane", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonArrayPrimitives()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[array(1)]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("bob", CommonFunctions.evalResultToString(res));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonArrayComplex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[arrayObj(1).Name]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("bob", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonArrayComplexStar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[arrayObj(*).Name]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("bob,bobe", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonArrayComplexLast()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.eval(env, 0, "[[arrayObj().Name]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("bobe", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_JsonArrayComplex_ComplexExp()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalJson(env, 0, WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate( "[[arrayObj().Name]] [[arrayObj().Name]]"));
            //------------Assert Results-------------------------
            Assert.AreEqual("bobe", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_JsonArrayComplex_Atom()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = WarewolfDataEvaluationCommon.evalJson(env, 0, WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate("1"));
            //------------Assert Results-------------------------
            Assert.AreEqual("bobe", CommonFunctions.evalResultToString(res));
        }

        public static DataASTMutable.WarewolfEnvironment CreateEnvironmentWithData()
        {

            ExecutionEnvironment env = new ExecutionEnvironment();
            env.Assign("[[Rec(1).a]]", "1", 0);
            env.Assign("[[Rec(2).a]]", "2", 0);
            env.Assign("[[Rec(3).a]]", "3", 0);
            env.Assign("[[Rec(1).b]]", "a", 0);
            env.Assign("[[Rec(2).b]]", "b", 0);
            env.Assign("[[Rec(3).b]]", "c", 0);
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
