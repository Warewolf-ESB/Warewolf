using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;


namespace WarewolfParsingTest
{
    [TestClass]
    public class TestUpdate
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        public void UpdateInPlace_Update_ApplyLambda_ExpectModified()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            
            //------------Execute Test---------------------------
           var updated=  PublicFunctions.EvalUpdate("[[Rec(*).a]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
          var res = CommonFunctions.evalResultToString(  EvaluationFunctions.eval(updated, 0, false, "[[Rec(*).a]]"));
            Assert.AreEqual(res,"1x,1x,1x,1x");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        public void UpdateInPlace_Update_ApplyLambda_ExpectModified_LastIndex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("[[Rec().a]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[Rec(*).a]]"));
            Assert.AreEqual(res, "1,2,3,2,1x");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        public void UpdateInPlace_Update_ApplyLambda_ExpectModified_intIndex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("[[Rec(1).a]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[Rec(*).a]]"));
            Assert.AreEqual(res, "1x,2,3,2");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        public void UpdateInPlace_Update_ApplyLambda_Scalar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("[[x]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[x]]"));
            Assert.AreEqual(res, "1x");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        public void UpdateInPlace_Update_ApplyLambda_reordSet()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("[[Rec(*)]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[Rec(*)]]"));
            Assert.AreEqual(res, "1x,1x,1x,1x,1x,1x,1x,1x");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        [ExpectedException(typeof(Exception))]
        public void UpdateInPlace_Update_ApplyLambda_Atom()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("12", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[x]]"));
            Assert.AreEqual(res, "1x");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        [ExpectedException(typeof(Exception))]
        public void UpdateInPlace_Update_ApplyLambda_NonExistent()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("[[ressss().a]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[x]]"));
            Assert.AreEqual(res, "1x");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        public void UpdateInPlace_Update_ApplyLambda_ExpectModified_IndexExp()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("[[Rec([[x]]).a]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[Rec(*).a]]"));
            Assert.AreEqual(res, "1x,2,3,2");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
        public void UpdateInPlace_Update_ApplyLambda_ExpectModified_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var updated = PublicFunctions.EvalUpdate("[[[[r]]]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("1x"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[s]]"));
            Assert.AreEqual(res, "1x");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateInPlace_Update")]
      
        public void UpdateInPlace_Update_ApplyLambda_ExpectError_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
          
            //------------Execute Test---------------------------
            
            var updated = PublicFunctions.EvalUpdate("[[s]]", env, 0, a => DataStorage.WarewolfAtom.NewDataString("[[s]]"));
            updated = PublicFunctions.EvalUpdate("[[[[s]]]]", updated, 0, a => DataStorage.WarewolfAtom.NewDataString("e"));
            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(updated, 0, false, "[[s]]"));
            Assert.AreEqual(res, "e");
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
