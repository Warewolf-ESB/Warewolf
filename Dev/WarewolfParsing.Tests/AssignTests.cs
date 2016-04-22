﻿using System;
using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;

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
            var data =  CreateEnvironmentWithData();
            
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
            Assert.AreEqual(x,1);
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
            var x = EvaluationFunctions.parseLanguageExpression("[[rec([[a]]).a]]", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetExpression;
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue( rec.Item.Index.IsIndexExpression);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignARecset")]
        public void ParseLanguage_IndexExpression_PassAnUpdate()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = EvaluationFunctions.parseLanguageExpression("[[rec(1).a]]", 3);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetExpression;
            // ReSharper disable once PossibleNullReferenceException
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
            var x = EvaluationFunctions.parseLanguageExpression("[[rec(1)]]", 3);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetNameExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
            // ReSharper disable once PossibleNullReferenceException
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
            var x = EvaluationFunctions.parseLanguageExpression("[[rec([[a]])]]", 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(x.IsRecordSetNameExpression, true);
            var rec = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue(rec.Item.Index.IsIndexExpression);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AssignEvaluationRecsets_AssignAShape")]
        public void Assign_Shape()
        {
            //------------Setup for test--------------------------
            var data = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var x = AssignEvaluation.evalDataShape( "[[b]]", 1,data);

            //------------Assert Results-------------------------

            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue(x.Scalar.ContainsKey("b"));
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

            // ReSharper disable once PossibleNullReferenceException
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

            // ReSharper disable once PossibleNullReferenceException
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

            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue(x.RecordSets.ContainsKey("Rec"));
            Assert.IsTrue(x.RecordSets["Rec"].Data.ContainsKey("d"));
            Assert.IsTrue(x.RecordSets["Rec"].Data.ContainsKey("a"));
        }


        public static DataStorage.WarewolfEnvironment CreateEnvironmentWithData()
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
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }
    }
}
