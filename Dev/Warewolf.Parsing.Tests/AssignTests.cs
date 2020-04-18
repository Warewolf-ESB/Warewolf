/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Exceptions;
using Warewolf.Storage;
using WarewolfParserInterop;
using static DataStorage;

namespace WarewolfParsingTest
{
    [TestClass]
    public class AssignTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        [ExpectedException(typeof(NullValueInVariableException))]
        public void AssignEvaluationRecsets_AssignARecset_Last_WithFraming()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            EvaluationFunctions.evalScalar("a", data);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        public void GetIntFromAtomTest()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.getIntFromAtom(DataStorage.WarewolfAtom.NewInt(1));

            //------------Assert Results-------------------------
            Assert.AreEqual(x, 1);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        [ExpectedException(typeof(Exception))]
        public void GetIntFromAtomTestLessThan0()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.getIntFromAtom(DataStorage.WarewolfAtom.NewInt(-11));

            //------------Assert Results-------------------------
            Assert.AreEqual(x, 1);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        [ExpectedException(typeof(Exception))]
        public void GetIntFromAtomTestLessNotAnInt()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.getIntFromAtom(DataStorage.WarewolfAtom.NewDataString("a"));

            //------------Assert Results-------------------------
            Assert.AreEqual(x, 1);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        public void ParseLanguage_IndexExpression()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpression("[[rec([[a]]).a]]", 1, ShouldTypeCast.Yes);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIndexExpression);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpressionStrict_IndexExpression()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec([[a]]).a]]", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIndexExpression);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        public void ParseLanguage_IndexExpression_PassAnUpdate()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpression("[[rec(1).a]]", 3, ShouldTypeCast.Yes);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIntIndex);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpressionStrict_IndexExpression_PassAnUpdate()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec(1).a]]", 3);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIntIndex);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        public void ParseLanguage_RecsetExpression_PassAnUpdate()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpression("[[rec(1)]]", 3, ShouldTypeCast.Yes);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetNameExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIntIndex);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpression_RecsetExpression_PassAnUpdate()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec(1)]]", 3);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetNameExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIntIndex);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        public void ParseLanguage_RecsetIndexExpression()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpression("[[rec([[a]])]]", 1, ShouldTypeCast.Yes);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetNameExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIndexExpression);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpression_RecsetIndexExpression()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec([[a]])]]", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetNameExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIndexExpression);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionWithoutUpdateStrict")]
        public void ParseLanguageExpressionWithoutUpdateStrict_RecsetIndexExpression()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionWithoutUpdateStrict("[[rec([[a]])]]");

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetNameExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
            
            Assert.IsTrue(rec.Item.Index.IsIndexExpression);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpression_InvalidScalar()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);           
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionWithoutUpdateStrict")]
        public void ParseLanguageExpressionWithoutUpdateStrict_InvalidScalar()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionWithoutUpdateStrict("[[rec");

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpression_InvalidRecordSet()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec()", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionWithoutUpdateStrict")]
        public void ParseLanguageExpressionWithoutUpdateStrict_InvalidRecordSet()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionWithoutUpdateStrict("[[rec()");

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpression_InvalidnamedRecordSet()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec([[a]])", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionWithoutUpdateStrict")]
        public void ParseLanguageExpressionWithoutUpdateStrict_InvalidnamedRecordSet()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionWithoutUpdateStrict("[[rec([[a]])");

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionStrict")]
        public void ParseLanguageExpression_InvalidIndexRecordSet()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionStrict("[[rec(1)", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EvaluationFunctions_parseLanguageExpressionWithoutUpdateStrict")]
        public void ParseExpressionWithoutUpdateStrict_InvalidIndexRecordSetUpdate()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpressionWithoutUpdateStrict("[[rec(1)");

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsWarewolfAtomExpression, true);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignAShape")]
        public void Assign_Shape()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = AssignEvaluation.evalDataShape("[[b]]", 1, data);

            //------------Assert Results-------------------------

            
            Assert.IsTrue(x.Scalar.ContainsKey("b"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Assign")]
        public void Assign_Given_Value_StartsWithOpeningLanguageBracktes_Should_Assign_Value_Correclty_Scalar()
        {
            //------------Setup for test--------------------------
            var emptyenv = CreateEmptyEnvironment();
            var value = "[[nathi";
            var exp = "[[myValue]]";

            //------------Execute Test---------------------------
            var envTemp = PublicFunctions.EvalAssignWithFrameStrict(new AssignValue(exp, value), 1, emptyenv);
            //------------Assert Results-------------------------
            Assert.IsNotNull(envTemp.Scalar);
            Assert.AreEqual(1, envTemp.Scalar.Count);
            Assert.AreEqual(value, envTemp.Scalar["myValue"].ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Assign")]
        public void Assign_Given_Value_ContainsOpeningLanguageBracktes_Should_Assign_Value_Correclty_Scalar()
        {
            //------------Setup for test--------------------------
            var emptyenv = CreateEmptyEnvironment();
            var value = "na[[thi";
            var exp = "[[myValue]]";

            //------------Execute Test---------------------------
            var envTemp = PublicFunctions.EvalAssignWithFrameStrict(new AssignValue(exp, value), 1, emptyenv);
            //PublicFunctions.AssignWithFrame(new AssignValue(exp, value), 1, emptyenv);
            //------------Assert Results-------------------------
           
            Assert.IsNotNull(envTemp.Scalar);
            Assert.AreEqual(1, envTemp.Scalar.Count);
            var a = PublicFunctions.EvalEnvExpression(exp, 0, false, envTemp);
            var valueFromEnv = ExecutionEnvironment.WarewolfEvalResultToString(a);
            Assert.AreEqual(value, envTemp.Scalar["myValue"].ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Assign")]
        public void Assign_Given_Value_ContainsOpeningLanguageBracktes_Should_Assign_Value_Correclty_RecordSets()
        {
            //------------Setup for test--------------------------
            var emptyenv = CreateEmptyEnvironment();
            var value = "na[[thi";
            var exp = "[[myValue().name]]";

            //------------Execute Test---------------------------
            var envTemp = PublicFunctions.EvalAssignWithFrameStrict(new AssignValue(exp, value), 1, emptyenv);
            //------------Assert Results-------------------------
            Assert.IsNotNull(envTemp.RecordSets);
            Assert.AreEqual(1, envTemp.RecordSets.Count);
            var a = PublicFunctions.EvalEnvExpression(exp, 0, false, envTemp);
            var valueFromEnv = ExecutionEnvironment.WarewolfEvalResultToString(a);
            Assert.AreEqual(value, valueFromEnv);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Assign")]
        [ExpectedException(typeof(Exception))]
        public void Assign_Given_Value_ContainsOpeningLanguageBracktes_Should_Assign_Value_Correclty_JsonObjects()
        {
            //------------Setup for test--------------------------
            var emptyenv = CreateEmptyEnvironment();
            var value = "na[[thi";
            var exp = "[[@myValue().name]]";

            //------------Execute Test---------------------------
            var envTemp = PublicFunctions.EvalAssignWithFrameStrict(new AssignValue(exp, value), 1, emptyenv);
            //------------Assert Results-------------------------
            Assert.IsNotNull(envTemp.RecordSets);
            Assert.AreEqual(1, envTemp.RecordSets.Count);
            var a = PublicFunctions.EvalEnvExpression(exp, 0, false, envTemp);
            var valueFromEnv = ExecutionEnvironment.WarewolfEvalResultToString(a);
            Assert.AreEqual(value, valueFromEnv);
        }        

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignAShape")]
        public void Assign_Shape_Recset()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = AssignEvaluation.evalDataShape("[[bx().d]]", 1, data);

            //------------Assert Results-------------------------

            
            Assert.IsTrue(x.RecordSets.ContainsKey("bx"));
            Assert.IsTrue(x.RecordSets["bx"].Data.ContainsKey("d"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignAShape")]
        public void Assign_Shape_Recset_ExistsGetsReplaced()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = AssignEvaluation.evalDataShape("[[Rec().d]]", 1, data);

            //------------Assert Results-------------------------

            
            Assert.IsTrue(x.RecordSets.ContainsKey("Rec"));
            Assert.IsTrue(x.RecordSets["Rec"].Data.ContainsKey("d"));
            Assert.IsTrue(x.RecordSets["Rec"].Data.ContainsKey("a"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignAShape")]
        [ExpectedException(typeof(Exception))]
        public void Assign_Shape_Recset_JsonThrows()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = AssignEvaluation.evalDataShape("1", 1, data);
            x = AssignEvaluation.evalDataShape("[[Rec().d.x]]", 1, x);

            //------------Assert Results-------------------------

            
            Assert.IsTrue(x.RecordSets.ContainsKey("Rec"));
            Assert.IsTrue(x.RecordSets["Rec"].Data.ContainsKey("d"));
            Assert.IsTrue(x.RecordSets["Rec"].Data.ContainsKey("a"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Assign_JsonProperty")]
        public void Assign_ValueToJsonProperty()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            AssignEvaluation.evalAssignWithFrame(new AssignValue("[[@Person.Name]]", "dora"), 0, data);

            //------------Assert Results-------------------------
            var jsonObject = data.JsonObjects["Person"];
            Assert.IsNotNull(jsonObject);
            var value = jsonObject.First;
            var token = ((Newtonsoft.Json.Linq.JProperty)value).Value;
            Assert.AreEqual("dora", token);
        }

        static DataStorage.WarewolfEnvironment CreateEmptyEnvironment()
        {
            var env = new ExecutionEnvironment();
            var p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }
        static DataStorage.WarewolfEnvironment CreateEnvironmentWithData()
        {

            var env = new ExecutionEnvironment();
            env.Assign("[[Rec(1).a]]", "1", 0);
            env.Assign("[[Rec(2).a]]", "2", 0);
            env.Assign("[[Rec(3).a]]", "3", 0);
            env.Assign("[[Rec(1).b]]", "a", 0);
            env.Assign("[[Rec(2).b]]", "b", 0);
            env.Assign("[[Rec(3).b]]", "c", 0);
            env.Assign("[[x]]", "1", 0);
            env.Assign("[[y]]", "y", 0);
            env.AssignJson(new AssignValue("[[@Person.Name]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[@Person.Age]]", "22"), 0);
            env.AssignJson(new AssignValue("[[@Person.Spouse.Name]]", "dora"), 0);
            env.AssignJson(new AssignValue("[[@Person.Children(1).Name]]", "Mary"), 0);
            env.AssignJson(new AssignValue("[[@Person.Children(2).Name]]", "Jane"), 0);
            env.AssignJson(new AssignValue("[[@Person.Score(1)]]", "2"), 0);
            env.AssignJson(new AssignValue("[[@Person.Score(2)]]", "3"), 0);
            env.AssignJson(new AssignValue("[[@array(1)]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[@arrayObj(1).Name]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[@arrayObj(2).Name]]", "bobe"), 0);
            var p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }
    }
}
