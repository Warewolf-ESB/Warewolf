using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;
// ReSharper disable PossibleNullReferenceException

namespace WarewolfParsingTest
{
    [TestClass]
    public class TestEvaluation
    {


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalEntireObject()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person>() });
            var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));
            Assert.AreEqual((added.JsonObjects["bob"] as JObject).GetValue("Name").ToString(), "n");
            var evalled = WarewolfDataEvaluationCommon.eval(added, 0, "[[bob]]");
            Assert.IsTrue(evalled.IsWarewolfAtomResult);
            var res = (evalled as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;
            var str = (res as DataASTMutable.WarewolfAtom.DataString).ToString();
            Assert.AreEqual(str,j.ToString());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalPartialObject()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person>() });
            var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));
            Assert.AreEqual((added.JsonObjects["bob"] as JObject).GetValue("Name").ToString(), "n");
            var evalled = CommonFunctions.evalResultToString( WarewolfDataEvaluationCommon.eval(added, 0, "[[bob.Name]]"));

            Assert.AreEqual(evalled, "n");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalPartialObjectNested()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person>(), Spouse = new Person() { Name = "o", Children = new List<Person>() } });
            var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));
            Assert.AreEqual((added.JsonObjects["bob"] as JObject).GetValue("Name").ToString(), "n");
            var evalled = CommonFunctions.evalResultToString(WarewolfDataEvaluationCommon.eval(added, 0, "[[bob.Spouse.Name]]"));

            Assert.AreEqual(evalled, "o");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalPartialObjectNestedIndex()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person> { new Person() { Name = "p", Children = new List<Person>() } }, Spouse = new Person() { Name = "o", Children = new List<Person>() } });
            var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(WarewolfDataEvaluationCommon.eval(added, 0, "[[bob.Children(1).Name]]"));

            Assert.AreEqual(evalled, "p");
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalPartialObjectNestedStar()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person> { new Person() { Name = "p", Children = new List<Person>() }, new Person() { Name = "q", Children = new List<Person>() } }, Spouse = new Person() { Name = "o", Children = new List<Person>() } });
            var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(WarewolfDataEvaluationCommon.eval(added, 0, "[[bob.Children(*).Name]]"));

            Assert.AreEqual(evalled, "p,q");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalPartialObjectNestedStarAll()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person> { new Person() { Name = "p", Children = new List<Person>() }, new Person() { Name = "q", Children = new List<Person>() } }, Spouse = new Person() { Name = "o", Children = new List<Person>() } });
            var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(WarewolfDataEvaluationCommon.eval(added, 0, "[[bob.Children(*)]]"));

            Assert.AreEqual(evalled, "p,q");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalPartialObjectNestedLast()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person> { new Person() { Name = "p", Children = new List<Person>() }, new Person() { Name = "q", Children = new List<Person>() } }, Spouse = new Person() { Name = "o", Children = new List<Person>() } });
            var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(WarewolfDataEvaluationCommon.eval(added, 0, "[[bob.Children().Name]]"));

            Assert.AreEqual(evalled, "q");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void AddToScalarsCreatesAscalarEval()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            var x = new[] { new Person() { Name = "n", Children = new List<Person>() }, new Person() { Name = "n", Children = new List<Person>() } };
            var j = JArray.FromObject( x );
            var q = j.SelectTokens("[*]");
          //  var added = AssignEvaluation.addToJsonObjects(createDataSet, "bob", j);
            ////------------Execute Test---------------------------

            ////------------Assert Results-------------------------
            //Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));
            //Assert.AreEqual(added.JsonObjects["bob"].GetValue("Name").ToString(), "n");
            //var evalled = WarewolfDataEvaluationCommon.eval(added, 0, "[[bob]]");
            //Assert.IsTrue(evalled.IsWarewolfAtomResult);
            //var res = (evalled as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;
            //var str = (res as DataASTMutable.WarewolfAtom.DataString).ToString();
            //Assert.AreEqual(str, j.ToString());
        }





        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_MultipleEvals()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                 new AssignValue("[[rec().a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            // ReSharper disable UnusedVariable
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            // ReSharper restore UnusedVariable
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);

            var items = env.EvalWhere("[[rec(*).a]]", a => PublicFunctions.AtomtoString(a) == "25", 0);
            Assert.AreEqual(items.ToArray()[0], 1);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_ComplexIndex()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                 new AssignValue("[[rec().a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            // ReSharper disable UnusedVariable
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            // ReSharper restore UnusedVariable
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);

            var items = env.EvalWhere("[[rec([[rec().a]]).a]]", a => PublicFunctions.AtomtoString(a) == "25", 0);
            Assert.AreEqual(items.ToArray()[0], 1);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_where_nonExistentRecset()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                 new AssignValue("[[rec().a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            // ReSharper disable UnusedVariable
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            // ReSharper restore UnusedVariable
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);

            var items = env.EvalWhere("[[bec().a]]", a => PublicFunctions.AtomtoString(a) == "25", 0);
            Assert.AreEqual(items.ToArray()[0], 1);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_ComplexIndexThatIsStar()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                 new AssignValue("[[rec().a]]", "27"),
                      new AssignValue("[[a]]", "*"),
             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            // ReSharper disable UnusedVariable
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            // ReSharper restore UnusedVariable
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[a]]", "*"), 0);
            var items = env.EvalWhere("[[rec([[a]]).a]]", a => PublicFunctions.AtomtoString(a) == "25", 0);
            Assert.AreEqual(items.ToArray()[0], 1);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_EvalWhere")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_MultipleEvalsErrorsOnScalar()
        {



            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");


            // ReSharper restore UnusedVariable
            ExecutionEnvironment env = new ExecutionEnvironment();


            var items = env.EvalWhere("[[a]]", a => PublicFunctions.AtomtoString(a) == "25", 0);




        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_EvalWhere")]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_MultipleEvalsErrorsRecordSetName()
        {
            
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");
            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                 new AssignValue("[[rec().a]]", "27"),

             };
            ExecutionEnvironment env = new ExecutionEnvironment();

            // ReSharper disable UnusedVariable
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            // ReSharper restore UnusedVariable

            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);

            // ReSharper restore UnusedVariable
            
            var items = env.EvalWhere("[[rec()]]", a => PublicFunctions.AtomtoString(a) == "25", 0);
            Assert.AreEqual(items.ToArray()[0], 1);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_EvalWhere")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_where_recset()
        {

            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");


            // ReSharper restore UnusedVariable
            ExecutionEnvironment env = new ExecutionEnvironment();
            var items = env.EvalWhere("x", a => PublicFunctions.AtomtoString(a) == "25", 0);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_EvalWhere")]
         [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_MultipleEvalsErrorsOnComplex()
        {

            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            ExecutionEnvironment env = new ExecutionEnvironment();
            var items = env.EvalWhere("[[rec()]] b", a => PublicFunctions.AtomtoString(a) == "25", 0);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_Multipleresults()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "25"),
                 new AssignValue("[[rec().a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            // ReSharper disable UnusedVariable
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            // ReSharper restore UnusedVariable
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);

            var items = env.EvalWhere("[[rec(*).a]]", a => PublicFunctions.AtomtoString(a) == "25", 0);

            IEnumerable<int> enumerable = items as int[] ?? items.ToArray();
            Assert.AreEqual(enumerable.ToArray()[0], 1);
            Assert.AreEqual(enumerable.ToArray()[1], 3);

        }


    }
}
