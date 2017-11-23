using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;
using Newtonsoft.Json.Linq;





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
            var added = WarewolfDataEvaluationCommon.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));
            Assert.AreEqual((added.JsonObjects["bob"] as JObject).GetValue("Name").ToString(), "n");
            var evalled = EvaluationFunctions.eval(added, 0, false, "[[@bob]]");
            Assert.IsTrue(evalled.IsWarewolfAtomResult);
            var res = (evalled as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;
            var str = (res as DataStorage.WarewolfAtom.DataString).ToString();
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
            var added = WarewolfDataEvaluationCommon.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));
            Assert.AreEqual((added.JsonObjects["bob"] as JObject).GetValue("Name").ToString(), "n");
            var evalled = CommonFunctions.evalResultToString( EvaluationFunctions.eval(added, 0, false, "[[@bob.Name]]"));

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
            var added = WarewolfDataEvaluationCommon.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));
            Assert.AreEqual((added.JsonObjects["bob"] as JObject).GetValue("Name").ToString(), "n");
            var evalled = CommonFunctions.evalResultToString(EvaluationFunctions.eval(added, 0, false, "[[@bob.Spouse.Name]]"));

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
            var added = WarewolfDataEvaluationCommon.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(EvaluationFunctions.eval(added, 0, false, "[[@bob.Children(1).Name]]"));

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
            var added = WarewolfDataEvaluationCommon.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(EvaluationFunctions.eval(added, 0, false, "[[@bob.Children(*).Name]]"));

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
            var added = WarewolfDataEvaluationCommon.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(EvaluationFunctions.eval(added, 0, false, "[[@bob.Children(*)]]"));
            Assert.IsTrue(evalled.Contains(@"""Name"": ""p"""));
            Assert.IsTrue(evalled.Contains(@"""Name"": ""q"""));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateJSONAndEvalPartialObjectNestedLast()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            JObject j = JObject.FromObject(new Person() { Name = "n", Children = new List<Person> { new Person() { Name = "p", Children = new List<Person>() }, new Person() { Name = "q", Children = new List<Person>() } }, Spouse = new Person() { Name = "o", Children = new List<Person>() } });
            var added = WarewolfDataEvaluationCommon.addToJsonObjects(createDataSet, "bob", j);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(added.JsonObjects.ContainsKey("bob"));

            var evalled = CommonFunctions.evalResultToString(EvaluationFunctions.eval(added, 0, false, "[[@bob.Children().Name]]"));

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
            //var evalled = EvaluationFunctions.eval(added, 0, "[[bob]]");
            //Assert.IsTrue(evalled.IsWarewolfAtomResult);
            //var res = (evalled as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;
            //var str = (res as DataStorage.WarewolfAtom.DataString).ToString();
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

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            
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

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            
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

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            
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

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            
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

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            

            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);

            
            
            var items = env.EvalWhere("[[rec()]]", a => PublicFunctions.AtomtoString(a) == "25", 0);
            Assert.AreEqual(items.ToArray()[0], 1);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_EvalWhere")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_where_recset()
        {

            WarewolfTestData.CreateTestEnvEmpty("");


            
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
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_Scalar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();


            
            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[x]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),GlobalConstants.CalculateTextConvertPrefix + "Sum(1)" + GlobalConstants.CalculateTextConvertSuffix );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecsetIndex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[Rec([[x]]).a]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum(1)" + GlobalConstants.CalculateTextConvertSuffix);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_Scalar_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[x]][[z]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum(12)" + GlobalConstants.CalculateTextConvertSuffix);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_recset_ComplexIndex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[[[q]]]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum(1)" + GlobalConstants.CalculateTextConvertSuffix);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[Rec().a]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum(3)" + GlobalConstants.CalculateTextConvertSuffix);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_Index()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[Rec(1).a]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum(1)" + GlobalConstants.CalculateTextConvertSuffix);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_Star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[Rec(*).a]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(GlobalConstants.CalculateTextConvertPrefix + "Sum(1)" + GlobalConstants.CalculateTextConvertSuffix+","+ GlobalConstants.CalculateTextConvertPrefix + "Sum(2)" + GlobalConstants.CalculateTextConvertSuffix+","+ GlobalConstants.CalculateTextConvertPrefix + "Sum(3)" + GlobalConstants.CalculateTextConvertSuffix, CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EvalCalculate")]
        public void EvalAggregateCalculate_RecSet_Star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculateAggregate(env, 0, GlobalConstants.AggregateCalculateTextConvertPrefix + "Sum([[Rec(*).a]])" + GlobalConstants.AggregateCalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(GlobalConstants.AggregateCalculateTextConvertPrefix + "Sum(1,2,3)" + GlobalConstants.AggregateCalculateTextConvertSuffix, CommonFunctions.evalResultToString(res));
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[Rec(*).a]][[x]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(GlobalConstants.CalculateTextConvertPrefix + "Sum(11)" + GlobalConstants.CalculateTextConvertSuffix+","+ GlobalConstants.CalculateTextConvertPrefix + "Sum(21)" + GlobalConstants.CalculateTextConvertSuffix + "," + GlobalConstants.CalculateTextConvertPrefix + "Sum(31)" + GlobalConstants.CalculateTextConvertSuffix, CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EvalCalculate")]
        public void EvalAggregateCalculate_RecSet_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculateAggregate(env, 0, GlobalConstants.AggregateCalculateTextConvertPrefix + "Sum([[Rec(*).a]][[x]])" + GlobalConstants.AggregateCalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.AggregateCalculateTextConvertPrefix+ "Sum(1,2,31)" + GlobalConstants.AggregateCalculateTextConvertSuffix);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_Json()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[@Person.Age]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum(\"22\")" + GlobalConstants.CalculateTextConvertSuffix);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_Json_Array()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[@Person.Score(1)]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum(\"2\")" + GlobalConstants.CalculateTextConvertSuffix);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EvalCalculate")]
        public void EvalAggregateCalculate_RecSet_Json_Array()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculateAggregate(env, 0, GlobalConstants.AggregateCalculateTextConvertPrefix + "Sum([[@Person.Score(1)]])" + GlobalConstants.AggregateCalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.AggregateCalculateTextConvertPrefix + "Sum(\"2\")" + GlobalConstants.AggregateCalculateTextConvertSuffix);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_Json_Array_star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[@Person.Score(*)]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(GlobalConstants.CalculateTextConvertPrefix + "Sum(\"2\")" + GlobalConstants.CalculateTextConvertSuffix+","+ GlobalConstants.CalculateTextConvertPrefix + "Sum(\"3\")" + GlobalConstants.CalculateTextConvertSuffix, CommonFunctions.evalResultToString(res) );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_Atom()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum( 1 2 3)" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.CalculateTextConvertPrefix + "Sum( 1 2 3)" + GlobalConstants.CalculateTextConvertSuffix);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EvalCalculate")]
        public void EvalCalculate_RecSet_RecsetResult()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculate(env, 0, GlobalConstants.CalculateTextConvertPrefix + "Sum([[Rec()]])" + GlobalConstants.CalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(GlobalConstants.CalculateTextConvertPrefix + "Sum(3)" + GlobalConstants.CalculateTextConvertSuffix+","+ GlobalConstants.CalculateTextConvertPrefix + "Sum(c)" + GlobalConstants.CalculateTextConvertSuffix, CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EvalCalculate")]
        public void EvalAggregateCalculate_RecSet_RecsetResult()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalForCalculateAggregate(env, 0, GlobalConstants.AggregateCalculateTextConvertPrefix + "Sum([[Rec()]])" + GlobalConstants.AggregateCalculateTextConvertSuffix);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), GlobalConstants.AggregateCalculateTextConvertPrefix + "Sum(3,c)" + GlobalConstants.AggregateCalculateTextConvertSuffix);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeScalar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[x]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "1");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeRecSet()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Rec().a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeRecSet_Name()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Rec()]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "3,c");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeRecSet_Index()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Rec(1).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "1");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeRecSet_Star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Rec(*).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "1,2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeRecSet_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Rec(*).a]][[x]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "1,2,3");
            Assert.AreEqual(CommonFunctions.evalResultToString(res.Last()), "1");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_DataMergeRecSet_Json()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Person.Age]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "22");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_DataMergeRecSet_Json_Array()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Person.Score(1)]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "2");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_DataMergeRecSet_Json_Array_star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, "[[Person.Score(*)]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), "2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeRecSet_Atom()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, " 1 2 3");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.First()), " 1 2 3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_DataMergeRecSet_RecsetResult()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = DataMergeFunctions.evalForDataMerge(env, 0, " [[Rec()]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res.Last()), "3,c");
        }

        private static DataStorage.WarewolfEnvironment CreateEnvironmentWithData()
        {
            
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.Assign("[[Rec(1).a]]","1",0);
            env.Assign("[[Rec(2).a]]", "2", 0);
            env.Assign("[[Rec(3).a]]", "3", 0);
            env.Assign("[[Rec(1).b]]", "a", 0);
            env.Assign("[[Rec(2).b]]", "b", 0);
            env.Assign("[[Rec(3).b]]", "c", 0);
            env.Assign("[[x]]","1",0);
            env.Assign("[[y]]", "y", 0);
            env.Assign("[[z]]", "2", 0);
            env.Assign("[[q]]", "r", 0);
            env.Assign("[[r]]", "1", 0);
            env.AssignJson(new AssignValue("[[@Person.Name]]","bob"),0 );
            env.AssignJson(new AssignValue("[[@Person.Age]]", "22"), 0);
            env.AssignJson(new AssignValue("[[@Person.Spouse.Name]]", "dora"), 0);
            env.AssignJson(new AssignValue("[[@Person.Children(1).Name]]", "Mary"), 0);
            env.AssignJson(new AssignValue("[[@Person.Children(2).Name]]", "Jane"), 0);
            env.AssignJson(new AssignValue("[[@Person.Score(1)]]", "2"), 0);
            env.AssignJson(new AssignValue("[[@Person.Score(2)]]", "3"), 0);
            env.AssignJson(new AssignValue("[[array(1)]]", "bob"), 0);
            PrivateObject p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
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

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            
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
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_MultipleEvals_Star()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                 new AssignValue("[[rec(*).a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(*).a]]", "28"), 0);

            var items = env.EvalAsListOfStrings("[[rec(*).a]]",0);
            Assert.AreEqual(items.ToArray()[0], "28");
            Assert.AreEqual(items.ToArray()[1], "28");

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_MultipleEvals_Star_NonExistent()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                 new AssignValue("[[rec(*).a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            
            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rsec(*).a]]", "28"), 0);

            var items = env.EvalAsListOfStrings("[[rsec(*).a]]", 0);
            Assert.AreEqual(items.Count, 1);
            Assert.AreEqual(items[0], "28");
        }
    }
}
