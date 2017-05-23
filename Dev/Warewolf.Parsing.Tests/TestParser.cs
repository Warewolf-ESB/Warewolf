using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace WarewolfParsingTest
{
    [TestClass]
    public class TestParser
    {
        [TestMethod]
        public void TestScalar()
        {
            var ast = EvaluationFunctions.parseLanguageExpression("[[a]]", 0);
            Assert.IsTrue(ast.IsScalarExpression);
            var astval = ast as LanguageAST.LanguageExpression.ScalarExpression;
            if (astval != null)
            {
                Assert.AreEqual(astval.Item, "a");
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }

        [TestMethod]
        public void TestRecsetExpressionLastIndex()
        {
            var ast = EvaluationFunctions.parseLanguageExpression("[[rec().a]]", 0);
            Assert.IsTrue(ast.IsRecordSetExpression);
            var astval = ast as LanguageAST.LanguageExpression.RecordSetExpression;
            if (astval != null)
            {
                Assert.AreEqual(astval.Item.Name, "rec");
                Assert.AreEqual(astval.Item.Column, "a");
                Assert.AreEqual(astval.Item.Index, LanguageAST.Index.Last);
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }

        [TestMethod]
        public void TestRecsetExpressionStarIndex()
        {
            var ast = EvaluationFunctions.parseLanguageExpression("[[rec(*).a]]", 0);
            Assert.IsTrue(ast.IsRecordSetExpression);
            var astval = ast as LanguageAST.LanguageExpression.RecordSetExpression;
            if (astval != null)
            {
                Assert.AreEqual(astval.Item.Name, "rec");
                Assert.AreEqual(astval.Item.Column, "a");
                Assert.AreEqual(astval.Item.Index, LanguageAST.Index.Star);
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }

        [TestMethod]
        public void TestRecsetNameExpressionStarIndex()
        {
            var ast = EvaluationFunctions.parseLanguageExpression("[[rec(*)]]", 0);
            Assert.IsTrue(ast.IsRecordSetNameExpression);
            var astval = ast as LanguageAST.LanguageExpression.RecordSetNameExpression;
            if (astval != null)
            {
                Assert.AreEqual(astval.Item.Name, "rec");
                Assert.AreEqual(astval.Item.Index, LanguageAST.Index.Star);
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }

        [TestMethod]
        public void TestRecsetNameExpressionLastIndex()
        {
            var ast = EvaluationFunctions.parseLanguageExpression("[[rec()]]", 0);
            Assert.IsTrue(ast.IsRecordSetNameExpression);
            var astval = ast as LanguageAST.LanguageExpression.RecordSetNameExpression;
            if (astval != null)
            {
                Assert.AreEqual(astval.Item.Name, "rec");
                Assert.AreEqual(astval.Item.Index, LanguageAST.Index.Last);
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }
        
        [TestMethod]
        public void TestRecsetExpressionLastIndex_InvalidName()
        {
            try
            {
                EvaluationFunctions.parseLanguageExpression("[[1rec().a]]", 0);
                Assert.Fail("No Exception thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Recordset name 1rec begins with a number.", e.Message);
            }

        }

        [TestMethod]
        public void TestRecsetExpressionStarIndex_InvalidName()
        {
            try
            {
                EvaluationFunctions.parseLanguageExpression("[[1rec(*).a]]", 0);
                Assert.Fail("No Exception thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Recordset name 1rec begins with a number.", e.Message);
            }

        }

        [TestMethod]
        public void TestRecsetExpressionLastIndex_InvalidName_RecOnly()
        {
            try
            {
                EvaluationFunctions.parseLanguageExpression("[[1rec()]]", 0);
                Assert.Fail("No Exception thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Recordset name 1rec begins with a number.", e.Message);
            }

        }

        [TestMethod]
        public void TestRecsetExpressionStarIndex_InvalidName_RecOnly()
        {
            try
            {
                EvaluationFunctions.parseLanguageExpression("[[1rec(*)]]", 0);
                Assert.Fail("No Exception thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Recordset name 1rec begins with a number.", e.Message);
            }

        }

        [TestMethod]
        public void TestRecsetExpressionLastIndex_InvalidColumnName()
        {
            try
            {
                EvaluationFunctions.parseLanguageExpression("[[rec().1a]]", 0);
                Assert.Fail("No Exception thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Column name 1a begins with a number.", e.Message);
            }

        }


        [TestMethod]
        public void TestRecsetExpressionStarIndex_InvalidColumnName()
        {
            try
            {
                EvaluationFunctions.parseLanguageExpression("[[rec(*).1a]]", 0);
                Assert.Fail("No Exception thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Column name 1a begins with a number.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Parse")]
        public void WarewolfParse_Parse_Nested_ExpectComplex()
        {

            var ast = EvaluationFunctions.parseLanguageExpression("[[[[a]]]]", 0);
            Assert.IsTrue(ast.IsComplexExpression);
            var astval = ast as LanguageAST.LanguageExpression.ComplexExpression;
            if (astval != null)
            {
                Assert.IsNotNull(astval);
                var data = astval.Item.ToArray();
                Assert.IsTrue(data[1].IsScalarExpression);
                Assert.IsTrue(data[0].IsWarewolfAtomExpression);
                Assert.IsTrue(data[2].IsWarewolfAtomExpression);
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Parse")]
        public void WarewolfParse_Parse_Nested_ExpectComplex_MultiNested()
        {


            var ast = EvaluationFunctions.parseLanguageExpression("[[[[[[a]]]]]]", 0);
            Assert.IsTrue(ast.IsComplexExpression);
            var astval = ast as LanguageAST.LanguageExpression.ComplexExpression;
            if (astval != null)
            {
                Assert.IsNotNull(astval);
                var data = astval.Item.ToArray();
                Assert.IsTrue(data[2].IsScalarExpression);
                Assert.IsTrue(data[0].IsWarewolfAtomExpression);
                Assert.IsTrue(data[1].IsWarewolfAtomExpression);
                Assert.IsTrue(data[3].IsWarewolfAtomExpression);
                Assert.IsTrue(data[4].IsWarewolfAtomExpression);
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Parse")]
        public void WarewolfParse_Parse_NestedDataSet_ExpectComplex_MultiNested()
        {

            var ast = EvaluationFunctions.parseLanguageExpression("[[[[[[rec(1).a]]]]]]", 0);
            Assert.IsTrue(ast.IsComplexExpression);
            var astval = ast as LanguageAST.LanguageExpression.ComplexExpression;
            if (astval != null)
            {
                Assert.IsNotNull(astval);
                var data = astval.Item.ToArray();
                Assert.IsTrue(data[2].IsRecordSetExpression);
                Assert.IsTrue(data[0].IsWarewolfAtomExpression);
                Assert.IsTrue(data[1].IsWarewolfAtomExpression);
                Assert.IsTrue(data[3].IsWarewolfAtomExpression);
                Assert.IsTrue(data[4].IsWarewolfAtomExpression);

                var x = data[2] as LanguageAST.LanguageExpression.RecordSetExpression;
                Assert.IsNotNull(x);
                Assert.IsTrue(x.Item.Index.IsIntIndex);
                Assert.AreEqual(x.Item.Column, "a");
                Assert.AreEqual(x.Item.Name, "rec");
            }
            else
            {
                Assert.Fail("Wrong type");
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Recset_ExpectAnAtom()
        {

            var env = CreateTestEnvWithData();

            var ast = PublicFunctions.EvalEnvExpression("[[rec(1).a]]", 0, false, env);
            Assert.IsTrue(ast.IsWarewolfAtomListresult);
            var x = ast as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            // ReSharper disable PossibleNullReferenceException
            var val = x.Item.First();

            Assert.IsTrue(val.IsInt);
            var intval = val as DataStorage.WarewolfAtom.Int;
            Assert.AreEqual(2, intval.Item);
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_RecsetEmpty_ExpectAnAtom()
        {

            var env = CreateTestEnvWithData();

            var ast = PublicFunctions.EvalEnvExpression("[[rec().a]]", 0, false, env);
            Assert.IsTrue(ast.IsWarewolfAtomListresult);
            var x = ast as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            // ReSharper disable PossibleNullReferenceException
            var val = x.Item.First();

            Assert.IsTrue(val.IsInt);
            var intval = val as DataStorage.WarewolfAtom.Int;
            Assert.AreEqual(3, intval.Item);
            // ReSharper restore PossibleNullReferenceException
        }

        private DataStorage.WarewolfEnvironment CreateTestEnvWithData()
        {

            IEnumerable<IAssignValue> assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "2"),
                 new AssignValue("[[rec().a]]", "4"),
                 new AssignValue("[[rec().a]]", "3"),
                 new AssignValue("[[a]]", "a"),
                 new AssignValue("[[b]]", "2344"),
                 new AssignValue("[[c]]", "a"),
                 new AssignValue("[[d]]", "1")

             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalMultiAssign(assigns, 0, env);
            return env2;
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Scalar_ExpectAnAtom()
        {

            var env = WarewolfTestData.CreateTestEnvWithData;

            var ast = PublicFunctions.EvalEnvExpression("[[a]]", 0, false, env);
            Assert.IsTrue(ast.IsWarewolfAtomResult);
            var x = ast as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(x.Item.IsDataString);
            var val = x.Item as DataStorage.WarewolfAtom.DataString;
            Assert.AreEqual("a", val.Item);
            // ReSharper restore PossibleNullReferenceException
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Scalar_ExpectAnAtomInt()
        {

            var env = CreateTestEnvWithData();

            var ast = PublicFunctions.EvalEnvExpression("[[b]]", 0, false, env);
            Assert.IsTrue(ast.IsWarewolfAtomResult);
            var x = ast as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(x != null && x.Item.IsInt);
            var val = x.Item as DataStorage.WarewolfAtom.Int;
            if (val != null)
            {
                Assert.AreEqual(2344, val.Item);
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_NestedScalar_Exists()
        {

            var env = CreateTestEnvWithData();

            var ast = PublicFunctions.EvalEnvExpression("[[[[c]]]]", 0, false, env);
            Assert.IsTrue(ast.IsWarewolfAtomListresult);
            var x = ast as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(x.Item[0].IsDataString);
            var val = x.Item[0] as DataStorage.WarewolfAtom.DataString;
            Assert.AreEqual("a", val.Item);
            // ReSharper rstore PossibleNullReferenceException
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_RecsetWithNestedIndex_Exists()
        {

            var env = CreateTestEnvWithData();

            var ast = PublicFunctions.EvalEnvExpression("[[rec([[d]]).a]]", 0, false, env);
            Assert.IsTrue(ast.IsWarewolfAtomListresult);
            var x = (ast as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item.First();

            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(x.IsInt);
            var val = x as DataStorage.WarewolfAtom.Int;
            Assert.AreEqual(2, val.Item);
            // ReSharper rstore PossibleNullReferenceException
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(NullValueInVariableException))]
        public void WarewolfParse_Eval_Scalar_NonExistent_ExpectException()
        {

            var env = CreateTestEnvWithData();

            PublicFunctions.EvalEnvExpression("[[xyz]]", 0, false, env);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(NullValueInVariableException))]
        public void WarewolfParse_Eval_Recset_NoIndex_ExpectAnException()
        {

            var env = WarewolfTestData.CreateTestEnvWithData;

            PublicFunctions.EvalEnvExpression("[[rec(4).a]]", 0, false, env);



            // ReSharper restore PossibleNullReferenceException
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_Star()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");
            var env2 = PublicFunctions.EvalAssign("[[rec(1).a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec(2).a]]", "33", 0, env2);
            var data = PublicFunctions.EvalAssign("[[rec(*).a]]", "30", 0, env3);
            var recordSet = data.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 30);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 30);
            // ReSharper rstore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_Star_Empty()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var data = PublicFunctions.EvalAssign("[[rec(*).a]]", "30", 0, env);
            var recordSet = data.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 1);

            // ReSharper rstore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_StarNonExistentColumn()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");
            var env2 = PublicFunctions.EvalAssign("[[rec(1).a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec(2).a]]", "33", 0, env2);
            var data = PublicFunctions.EvalAssign("[[rec(*).b]]", "30", 0, env3);
            var recordSet = data.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("b"));
            Assert.AreEqual(2, recordSet.Data["b"].Count);
            Assert.IsTrue(recordSet.Data["b"][0].IsInt);
            Assert.AreEqual((recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item, 30);
            Assert.AreEqual((recordSet.Data["b"][1] as DataStorage.WarewolfAtom.Int).Item, 30);

            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 33);
            // ReSharper rstore PossibleNullReferenceException
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_WithIndex()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalAssign("[[rec(1).a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec(3).a]]", "22", 0, env2);
            var env4 = PublicFunctions.EvalAssign("[[rec(2).a]]", "21", 0, env3);
            var recordSet = env4.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 3);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 22);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 21);
            Assert.AreEqual(recordSet.Optimisations, DataStorage.WarewolfAttribute.Fragmented);
            // ReSharper rstore PossibleNullReferenceException
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_WithIndex_Ordinal()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalAssign("[[rec(1).a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec(2).a]]", "22", 0, env2);
            var env4 = PublicFunctions.EvalAssign("[[rec(3).a]]", "21", 0, env3);
            var recordSet = env4.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 3);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 22);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 21);
            Assert.AreEqual(recordSet.Optimisations, DataStorage.WarewolfAttribute.Ordinal);
            // ReSharper rstore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_WithIndex_Ordered()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalAssign("[[rec(1).a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec(2).a]]", "22", 0, env2);
            var env4 = PublicFunctions.EvalAssign("[[rec(5).a]]", "21", 0, env3);
            var recordSet = env4.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 3);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 22);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 21);
            Assert.AreEqual(recordSet.Optimisations, DataStorage.WarewolfAttribute.Sorted);
            // ReSharper rstore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_WithNoIndex_Append()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalAssign("[[rec().a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec().a]]", "22", 0, env2);
            var env4 = PublicFunctions.EvalAssign("[[rec().a]]", "21", 0, env3);
            var recordSet = env4.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 3);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 22);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 21);
            Assert.AreEqual(recordSet.Optimisations, DataStorage.WarewolfAttribute.Ordinal);
            // ReSharper rstore PossibleNullReferenceException
        }





        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_WithNoIndex_ToUnordered()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");
            var env2 = PublicFunctions.EvalAssign("[[rec(3).a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec().a]]", "25", 0, env2);
            var env4 = PublicFunctions.EvalAssign("[[rec().a]]", "22", 0, env3);
            var env5 = PublicFunctions.EvalAssign("[[rec().a]]", "21", 0, env4);
            var recordSet = env5.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 4);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 22);
            Assert.AreEqual((recordSet.Data["a"][3] as DataStorage.WarewolfAtom.Int).Item, 21);
            Assert.AreEqual(recordSet.Optimisations, DataStorage.WarewolfAttribute.Sorted);
            // ReSharper rstore PossibleNullReferenceException
        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign_WithNoIndex_ToUnordered_Mixedup()
        {

            var env = WarewolfTestData.CreateTestEnvEmpty("");
            var env2 = PublicFunctions.EvalAssign("[[rec(3).a]]", "25", 0, env);
            var env3 = PublicFunctions.EvalAssign("[[rec().a]]", "25", 0, env2);
            var env4 = PublicFunctions.EvalAssign("[[rec(2).a]]", "22", 0, env3);
            var env5 = PublicFunctions.EvalAssign("[[rec().a]]", "21", 0, env4);
            var recordSet = env5.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 4);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 22);
            Assert.AreEqual((recordSet.Data["a"][3] as DataStorage.WarewolfAtom.Int).Item, 21);
            Assert.AreEqual(recordSet.Optimisations, DataStorage.WarewolfAttribute.Fragmented);
            // ReSharper rstore PossibleNullReferenceException
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndex()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[rec().a]]", "27"),

             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalMultiAssign(assigns, 0, env);

            var recordSet = env2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 3);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 27);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][2] as DataStorage.WarewolfAtom.Int).Item, 3);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndexAndMultipleColumns()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[rec().a]]", "27"),

             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var env2 = PublicFunctions.EvalMultiAssign(assigns, 0, env);

            var recordSet = env2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 3);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 27);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][2] as DataStorage.WarewolfAtom.Int).Item, 3);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_With_StarOnBothSides()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[rec().b]]", "27"),

             };

            var assigns2 = new List<IAssignValue>
             {
                 new AssignValue("[[rec(*).a]]", "[[rec(*).b]]"),


             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var envx = PublicFunctions.EvalMultiAssign(assigns, 0, env);
            var env2 = PublicFunctions.EvalMultiAssign(assigns2, 0, envx);
            var recordSet = env2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 27);

            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_FramedAssign_List_IndexExpression()
        {
            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[a]]", "1"),

             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var envx = PublicFunctions.EvalMultiAssign(assigns, 0, env);
            envx = AssignEvaluation.evalMultiAssignList(envx, new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewInt(1), DataStorage.WarewolfAtom.NewInt(2) }, "[[bec([[a]]).a]]", 0, true);


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_With_LastOnOneSideList()
        {



            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var envx = AssignEvaluation.evalMultiAssignList(env, new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewInt(1), DataStorage.WarewolfAtom.NewInt(2) }, "[[bec().a]]", 0, true);


            var recordSet = envx.RecordSets["bec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(2, recordSet.Data["a"].Count);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual(1, (recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(2, (recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item);

            Assert.AreEqual(1, (recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_With_StarOnOneSideAndLastOnOther()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[rec().b]]", "27"),

             };

            var assigns2 = new List<IAssignValue>
             {
                 new AssignValue("[[rec(*).a]]", "[[rec().b]]"),


             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var envx = PublicFunctions.EvalMultiAssign(assigns, 0, env);
            var env2 = PublicFunctions.EvalMultiAssign(assigns2, 0, envx);
            var recordSet = env2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 27);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 27);

            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_With_StarOnOneSideAndIndexOnOther()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[rec().b]]", "27"),

             };

            var assigns2 = new List<IAssignValue>
             {
                 new AssignValue("[[rec(*).a]]", "[[rec(1).b]]"),


             };
            var env = WarewolfTestData.CreateTestEnvEmpty("");

            var envx = PublicFunctions.EvalMultiAssign(assigns, 0, env);
            var env2 = PublicFunctions.EvalMultiAssign(assigns2, 0, envx);
            var recordSet = env2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 33);

            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndexAndMultipleColumns_Mixed()
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

            var recordSet = testEnv2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 27);
            Assert.AreEqual((recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["b"][1] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndexLeftAndRight()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[rec().b]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            assigns = new List<IAssignValue>
             {
                 new AssignValue("[[bec().a]]", "[[rec().a]]"),
                 new AssignValue("[[bec().b]]", "[[rec().b]]"),


             };
            var testEnv3 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv2);

            var recordSet = testEnv3.RecordSets["bec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(1, recordSet.Data["a"].Count);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual(26, (recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(27, (recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(1, (recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(NullReferenceException))]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndexLeftAndRightError()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),


             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            assigns = new List<IAssignValue>
             {
                 new AssignValue("[[bec().a]]", "[[rec().a]]"),
                 new AssignValue("[[bec().b]]", "[[rec().b]]"),


             };
            var testEnv3 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv2);

            var recordSet = testEnv3.RecordSets["bec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(1, recordSet.Data["a"].Count);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual(26, (recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(1, (recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(27, (recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndexLeftAndRightIndex()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().a]]", "26"),
                 new AssignValue("[[rec().b]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            assigns = new List<IAssignValue>
             {
                 new AssignValue("[[bec().a]]", "[[rec(1).a]]"),
                 new AssignValue("[[bec().b]]", "[[rec(1).b]]"),


             };
            var testEnv3 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv2);

            var recordSet = testEnv3.RecordSets["bec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 1);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndexAndMultipleColumns_Mixed_multipleSecondColumn()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),


             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            var recordSet = testEnv2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.IsTrue(recordSet.Data["a"][1].IsNothing);
            Assert.AreEqual((recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["b"][1] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithIndexAlreadySet()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[a]]", "25"),
                 new AssignValue("[[rec().a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),
                  new AssignValue("[[rec(*).xv]]", "[[rec(2).b]]"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            var recordSet = testEnv2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.IsTrue(recordSet.Data["a"][1].IsNothing);
            Assert.IsFalse(recordSet.Data["b"][0].IsNothing);
            Assert.AreEqual((recordSet.Data["b"][1] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_RecordSet()
        {
            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(25).a]]", "25"),
                 new AssignValue("[[rec().b]]", "33"),
                 new AssignValue("[[rec().b]]", "26"),


             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");
            var testEnv3 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            PublicFunctions.EvalEnvExpression("[[rec()]]", 0, false, testEnv3);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_RecordSet_Sorted()
        {
            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(25).a]]", "25"),
                 new AssignValue("[[rec(27).b]]", "33"),
                 new AssignValue("[[rec(29).b]]", "26"),


             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");
            var testEnv3 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            var res = PublicFunctions.EvalEnvExpression("[[rec()]]", 0, false, testEnv3);
            Assert.IsTrue(res.IsWarewolfRecordSetResult);
            var x = (res as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult).Item;
            Assert.AreEqual("29", x.Data[DataStorage.PositionColumn][0].ToString());
            Assert.AreEqual("26", x.Data["b"][0].ToString());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_RecordSet_Index()
        {
            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(25).a]]", "25"),
                 new AssignValue("[[rec(27).b]]", "33"),
                 new AssignValue("[[rec(29).b]]", "26"),


             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");
            var testEnv3 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            var res = PublicFunctions.EvalEnvExpression("[[rec(27)]]", 0, false, testEnv3);
            Assert.IsTrue(res.IsWarewolfRecordSetResult);
            var x = (res as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult).Item;
            Assert.AreEqual("27", x.Data[DataStorage.PositionColumn][0].ToString());
            Assert.AreEqual("33", x.Data["b"][0].ToString());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Assign()
        {



            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");
            var testEnv3 = PublicFunctions.EvalAssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0, testEnv);
            var testEnv4 = PublicFunctions.EvalAssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0, testEnv3);
            // ReSharper disable UnusedVariable
            var testEnv5 = PublicFunctions.EvalAssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0, testEnv4);
            // ReSharper restore UnusedVariable





        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_FramedAssign_WithNoIndexAndMultipleColumns_MultipleEvals()
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

            var recordSet = testEnv2.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 2);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 27);
            Assert.AreEqual((recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["b"][1] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);

            var env4 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv2);

            recordSet = env4.RecordSets["rec"];
            Assert.IsTrue(recordSet.Data.ContainsKey("a"));
            Assert.AreEqual(recordSet.Data["a"].Count, 4);
            Assert.IsTrue(recordSet.Data["a"][0].IsInt);
            Assert.AreEqual((recordSet.Data["a"][0] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][1] as DataStorage.WarewolfAtom.Int).Item, 27);
            Assert.AreEqual((recordSet.Data["a"][2] as DataStorage.WarewolfAtom.Int).Item, 25);
            Assert.AreEqual((recordSet.Data["a"][3] as DataStorage.WarewolfAtom.Int).Item, 27);
            Assert.AreEqual((recordSet.Data["b"][0] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["b"][1] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["b"][2] as DataStorage.WarewolfAtom.Int).Item, 33);
            Assert.AreEqual((recordSet.Data["b"][3] as DataStorage.WarewolfAtom.Int).Item, 26);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][0] as DataStorage.WarewolfAtom.Int).Item, 1);
            Assert.AreEqual((recordSet.Data["WarewolfPositionColumn"][1] as DataStorage.WarewolfAtom.Int).Item, 2);

        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_WithNoIndexAndMultipleColumns_Multipleresults()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);



            env.EvalDelete("[[rec(1)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "26");
            Assert.AreEqual(items[1], "25");
            Assert.AreEqual(items[2], "28");

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_WithIndexExpAndMultipleColumns_Multipleresults()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[a]]", "1"), 0);


            env.EvalDelete("[[rec([[a]])]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "26");
            Assert.AreEqual(items[1], "25");
            Assert.AreEqual(items[2], "28");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Mid()
        {



            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);



            env.EvalDelete("[[rec(3)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "25");
            Assert.AreEqual(items[1], "26");
            Assert.AreEqual(items[2], "28");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_Delete_Scalar()
        {



            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);



            env.EvalDelete("[[a]]", 0);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Exp()
        {



            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[a]]", "3"), 0);



            env.EvalDelete("[[rec([[a]])]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);

            Assert.AreEqual(items.Count, 2);
            Assert.AreEqual(items[0], "25");
            Assert.AreEqual(items[1], "26");


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_Delete_Exp_fail()
        {



            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[a]]", "a"), 0);



            env.EvalDelete("[[rec([[a]])]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);

            Assert.AreEqual(items.Count, 2);
            Assert.AreEqual(items[0], "25");
            Assert.AreEqual(items[1], "26");


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        [ExpectedException(typeof(Exception))]
        public void WarewolfParse_Eval_Delete_NonExistent()
        {



            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);



            env.EvalDelete("[[fec(3)]]", 0);


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec(5).a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(1).a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(3).a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(4).a]]", "28"), 0);



            env.EvalDelete("[[rec(3)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "26");
            Assert.AreEqual(items[1], "28");
            Assert.AreEqual(items[2], "25");

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec(5).a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(1).a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(3).a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(4).a]]", "28"), 0);



            env.EvalDelete("[[rec(3)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "26");
            Assert.AreEqual(items[1], "28");
            Assert.AreEqual(items[2], "25");
            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Fragmented);
            Assert.AreEqual(recset.LastIndex, 5);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Last()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);



            env.EvalDelete("[[rec(2)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "25");
            Assert.AreEqual(items[1], "25");
            Assert.AreEqual(items[2], "28");
            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Sorted);
            Assert.AreEqual(recset.LastIndex, 4);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Multiple_Columns_Last()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);



            env.EvalDelete("[[rec(2)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "25");

            items = env.EvalAsListOfStrings("[[rec(*).b]]", 0);


            Assert.AreEqual(items[0], "26");
            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Sorted);
            Assert.AreEqual(recset.LastIndex, 1);
            Assert.AreEqual(recset.Count, 1);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Multiple_Columns_Mixed()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "22"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "24"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);


            env.EvalDelete("[[rec(2)]]", 0);

            env.AssignWithFrame(new AssignValue("[[rec(1).b]]", "xxx"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(1).a]]", "yyy"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(17).b]]", "uuu"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "aaa"), 0);


            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);
            Assert.AreEqual(items[0], "yyy");
            Assert.AreEqual(items[1], "24");
            Assert.AreEqual(items[2], "1");
            Assert.AreEqual(items[3], "aaa");
            items = env.EvalAsListOfStrings("[[rec(*).b]]", 0);
            Assert.AreEqual(items[0], "xxx");
            Assert.AreEqual(items[1], "22");
            Assert.AreEqual(items[2], "27");
            Assert.AreEqual(items[3], "uuu");
            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Sorted);
            Assert.AreEqual(recset.LastIndex, 17);
            Assert.AreEqual(recset.Count, 4);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Multiple_Columns_Mixed_MoreStuff()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "22"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "24"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);


            env.EvalDelete("[[rec(2)]]", 0);

            env.AssignWithFrame(new AssignValue("[[rec(1).b]]", "xxx"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(1).a]]", "yyy"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(17).b]]", "uuu"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "aaa"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).b]]", "444"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).a]]", "222"), 0);

            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);
            Assert.AreEqual(items[0], "yyy");
            Assert.AreEqual(items[1], "24");
            Assert.AreEqual(items[2], "1");
            Assert.AreEqual(items[3], "222");
            Assert.AreEqual(items[4], "aaa");
            items = env.EvalAsListOfStrings("[[rec(*).b]]", 0);
            Assert.AreEqual(items[0], "xxx");
            Assert.AreEqual(items[1], "22");
            Assert.AreEqual(items[2], "27");
            Assert.AreEqual(items[3], "444");
            Assert.AreEqual(items[4], "uuu");
            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Fragmented);
            Assert.AreEqual(recset.LastIndex, 17);
            Assert.AreEqual(recset.Count, 5);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Multiple_Columns_Mixed_OtherMoreStuff()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "22"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "24"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);


            env.EvalDelete("[[rec(2)]]", 0);

            env.AssignWithFrame(new AssignValue("[[rec(1).b]]", "xxx"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(1).a]]", "yyy"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(17).b]]", "uuu"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "aaa"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).b]]", "444"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).a]]", "222"), 0);
            env.EvalDelete("[[rec(7)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);
            Assert.AreEqual(items[0], "yyy");
            Assert.AreEqual(items[1], "24");
            Assert.AreEqual(items[2], "1");

            Assert.AreEqual(items[3], "aaa");
            items = env.EvalAsListOfStrings("[[rec(*).b]]", 0);
            Assert.AreEqual(items[0], "xxx");
            Assert.AreEqual(items[1], "22");
            Assert.AreEqual(items[2], "27");

            Assert.AreEqual(items[3], "uuu");

            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Fragmented);
            Assert.AreEqual(recset.LastIndex, 17);
            Assert.AreEqual(recset.Count, 4);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Multiple_DeleteLast()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "22"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "24"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);


            env.EvalDelete("[[rec(2)]]", 0);

            env.AssignWithFrame(new AssignValue("[[rec(1).b]]", "xxx"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(1).a]]", "yyy"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(17).b]]", "uuu"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "aaa"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).b]]", "444"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).a]]", "222"), 0);
            env.EvalDelete("[[rec(7)]]", 0);
            env.EvalDelete("[[rec()]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);
            Assert.AreEqual(items[0], "yyy");
            Assert.AreEqual(items[1], "24");
            Assert.AreEqual(items[2], "1");


            items = env.EvalAsListOfStrings("[[rec(*).b]]", 0);
            Assert.AreEqual(items[0], "xxx");
            Assert.AreEqual(items[1], "22");
            Assert.AreEqual(items[2], "27");



            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Fragmented);
            Assert.AreEqual(recset.LastIndex, 4);
            Assert.AreEqual(recset.Count, 3);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Star_WithUpdate_ShouldDeleteUpdateIndex()
        {
            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "24"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);


            env.EvalDelete("[[rec(*)]]", 2);


            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);
            Assert.AreEqual(items[0], "25");
            Assert.AreEqual(items[1], "24");
            Assert.AreEqual(items[2], "1");

            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.LastIndex, 4);
            Assert.AreEqual(recset.Count, 3);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Multiple_Delete_Star()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "28"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "22"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "24"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().b]]", "27"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "1"), 0);


            env.EvalDelete("[[rec(2)]]", 0);

            env.AssignWithFrame(new AssignValue("[[rec(1).b]]", "xxx"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(1).a]]", "yyy"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(17).b]]", "uuu"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "aaa"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).b]]", "444"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).a]]", "222"), 0);
            env.EvalDelete("[[rec(7)]]", 0);
            env.EvalDelete("[[rec(*)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);
            Assert.AreEqual(items.Count, 0);






            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Fragmented);
            Assert.AreEqual(recset.LastIndex, 0);
            Assert.AreEqual(recset.Count, 0);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_Delete_Unordered_CheckForAttributes_Mixed()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec().a]]", "26"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(5).a]]", "25"), 0);
            env.AssignWithFrame(new AssignValue("[[rec(7).a]]", "28"), 0);



            env.EvalDelete("[[rec(2)]]", 0);
            var items = env.EvalAsListOfStrings("[[rec(*).a]]", 0);


            Assert.AreEqual(items[0], "25");
            Assert.AreEqual(items[1], "25");
            Assert.AreEqual(items[2], "28");
            PrivateObject p = new PrivateObject(env);
            var inner = p.GetField("_env") as DataStorage.WarewolfEnvironment;
            var recset = inner.RecordSets["rec"];
            Assert.AreEqual(recset.Optimisations, DataStorage.WarewolfAttribute.Sorted);
            Assert.AreEqual(recset.LastIndex, 7);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_UnOrdered()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(2).a]]", "25"),
                 new AssignValue("[[rec(3).a]]", "33"),
                 new AssignValue("[[rec(44).a]]", "25"),
                 new AssignValue("[[rec(1).a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);


            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, testEnv2);
            if (items.IsWarewolfAtomListresult)
            {
                var lst = (items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item;
                Assert.AreEqual(lst[0].ToString(), "27");
                Assert.AreEqual(lst[1].ToString(), "25");
                Assert.AreEqual(lst[2].ToString(), "33");
                Assert.AreEqual(lst[3].ToString(), "25");
            }


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_where_WithNoIndexAndMultipleColumns_UnOrdered_Delete()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(2).a]]", "25"),
                 new AssignValue("[[rec(3).a]]", "33"),
                 new AssignValue("[[rec(44).a]]", "25"),
                 new AssignValue("[[rec(1).a]]", "27"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            var env3 = PublicFunctions.EvalDelete("[[rec(1)]]", 0, testEnv2);
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, env3);
            if (items.IsWarewolfAtomListresult)
            {
                var lst = (items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item;

                Assert.AreEqual(lst[0].ToString(), "25");
                Assert.AreEqual(lst[1].ToString(), "33");
                Assert.AreEqual(lst[2].ToString(), "25");
            }


        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_ToExpression()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(2).a]]", "25"),
                 new AssignValue("[[rec(3).a]]", "33"),
                 new AssignValue("[[rec(44).a]]", "25"),
                 new AssignValue("[[a]]", "1"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            var env3 = EvaluationFunctions.evalToExpression(testEnv2, 0, "[[rec([[a]]).a]]");
            Assert.AreEqual(env3, "[[rec(1).a]]");


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_ToExpressionComplex()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(2).a]]", "25"),
                 new AssignValue("[[rec(3).a]]", "33"),
                 new AssignValue("[[rec(44).a]]", "25"),
                 new AssignValue("[[a]]", "a"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            var env3 = EvaluationFunctions.evalToExpression(testEnv2, 0, "[[[[a]]]]");
            Assert.AreEqual(env3, "[[a]]");


            env3 = EvaluationFunctions.evalToExpression(testEnv2, 1, "[[[[a]]]]");
            Assert.AreEqual(env3, "[[a]]");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_Eval_ToExpressionScalarAndRecset()
        {


            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(2).a]]", "25"),
                 new AssignValue("[[rec(3).a]]", "33"),
                 new AssignValue("[[rec(44).a]]", "25"),
                 new AssignValue("[[a]]", "a"),

             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var testEnv2 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);

            var env3 = EvaluationFunctions.evalToExpression(testEnv2, 0, "[[a]]");
            Assert.AreEqual(env3, "[[a]]");

            env3 = EvaluationFunctions.evalToExpression(testEnv2, 0, "[[rec().a]]");
            Assert.AreEqual(env3, "[[rec().a]]");

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_LanguageExpressionToString_NestedJsonArrayOfObjects()
        {
            var str = EvaluationFunctions.languageExpressionToString(EvaluationFunctions.parseLanguageExpressionWithoutUpdate("[[@Person.Child(*).Age]]"));
            Assert.AreEqual("[[@Person.Child(*).Age]]", str);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_LanguageExpressionToString_NestedProperty()
        {
            var str = EvaluationFunctions.languageExpressionToString(EvaluationFunctions.parseLanguageExpressionWithoutUpdate("[[@Person.Child]]"));
            Assert.AreEqual("[[@Person.Child]]", str);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void WarewolfParse_LanguageJson_InvalidIndex()
        {
            try
            {
                EvaluationFunctions.parseLanguageExpressionWithoutUpdate("[[@this.new(1).val(x).s]]");
                Assert.Fail("Expected exception");
            }
            catch(Exception e)
            {
                Assert.AreEqual("parse error", e.Message);
            }
        }





        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Eval")]
        public void Warewolf_AssignListToRecset()
        {
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");

            var env3 = PublicFunctions.EvalAssignFromList("[[a]]", new List<DataStorage.WarewolfAtom>() { DataStorage.WarewolfAtom.NewDataString("a"), DataStorage.WarewolfAtom.NewDataString("b") }, testEnv, 0, true);


            var items = PublicFunctions.EvalEnvExpression("[[a]]", 0, false, env3);
            if (items.IsWarewolfAtomListresult)
            {
                var lst = (items as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;

                Assert.IsTrue(lst.IsDataString);
                Assert.AreEqual((lst as DataStorage.WarewolfAtom.DataString).Item, "a,b");

            }


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Errors")]
        public void WarewolfEnvironment_ErrorsAreUnique()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AddError("bob");
            Assert.AreEqual(env.Errors.Count, 1);
            env.AddError("bob");
            Assert.AreEqual(env.Errors.Count, 1);
            env.AddError("dave");
            Assert.AreEqual(env.Errors.Count, 2);
            env.AddError("dave");
            Assert.AreEqual(env.Errors.Count, 2);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfParse_Errors")]
        public void WarewolfEnvironment_AllErrorsAreUnique()
        {


            ExecutionEnvironment env = new ExecutionEnvironment();
            env.AllErrors.Add("bob");
            Assert.AreEqual(env.AllErrors.Count, 1);
            env.AllErrors.Add("bob");
            Assert.AreEqual(env.AllErrors.Count, 1);
            env.AllErrors.Add("dave");
            Assert.AreEqual(env.AllErrors.Count, 2);
            env.AllErrors.Add("dave");
            Assert.AreEqual(env.AllErrors.Count, 2);

        }

        // ReSharper restore InconsistentNaming
    }
}
