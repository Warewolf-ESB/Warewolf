﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;
using Dev2.Common.Common;
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
            var res = EvaluationFunctions.eval(env, 0,  "[[x]]" );
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
            var res = EvaluationFunctions.eval(env, 0,  "[[Rec().a]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "3" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(NullValueInVariableException))]
        public void Eval_RecSet_NonExistent()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalRecordsSet(new LanguageAST.RecordSetColumnIdentifier("gerrs", "qqq", LanguageAST.Index.Last), env);

            //------------Assert Results-------------------------

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSetIndexExpression()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[Rec([[x]])]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1,a");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSetWithIndexExpression()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[Rec([[x]]).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_RecSetWithIndexExpression_EvaluatesToExp()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[Rec([[y]]).a]]");
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
            var res = EvaluationFunctions.eval(env, 0,  "[[Rec(1).a]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "1" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Index_MixedUpLast()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[non().a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_Indexes()  
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalIndex(env, 0, "[[non().a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(res, 1);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_Indexes_ExpectException()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalIndex(env, 0, "[[non(*).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(res, 1);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_RecSet_Index_NonExistent()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[eeerRec(1).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0,  "[[Rec(*).a]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res),  "1,2,3" );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_AsString()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalRecordSetAsString(env, ((LanguageAST.LanguageExpression.RecordSetExpression)EvaluationFunctions.parseLanguageExpressionWithoutUpdate( "[[Rec(*).a]]")).Item);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.atomtoString(res), "123");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_AsStringind()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalRecordSetAsString(env, ((LanguageAST.LanguageExpression.RecordSetExpression)EvaluationFunctions.parseLanguageExpressionWithoutUpdate("[[Rec(1).a]]")).Item);
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.atomtoString(res), "1");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Complete()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[Rec(*)]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1,2,3,a,b,c");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Complete_WithUpdate()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 1, "[[Rec(*)]][[Rec(*)]]");
            //------------Assert Results-------------------------
            // note this is currently undefined behaviour
            Assert.AreEqual("11,1a,a1,aa", CommonFunctions.evalResultToString(res));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Complete_WithUpdate_Mixed()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 1, "[[Rec(*)]][[Rec(3)]]");
            //------------Assert Results-------------------------
            // note this is currently undefined behaviour
            Assert.AreEqual("13,1c,a3,ac", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Complete_WithUpdate_Recset()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 1, "[[Rec(*).a]][[Rec(*).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "11");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Complete_WithUpdate_Recset_Mixed()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 1, "[[Rec(*).a]][[Rec(3).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "13");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0,  "[[Rec(*).a]][[x]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual("11,21,31",  CommonFunctions.evalResultToString(res) );
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_RecSet_Json()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0,  "[[@Person.Age]]" );
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
            var res = EvaluationFunctions.eval(env, 0,  "[[@Person.Score(1)]]" );
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
            var res = EvaluationFunctions.eval(env, 0,  "[[@Person.Score(*)]]" );
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
            var res = EvaluationFunctions.eval(env, 0,  " 1 2 3" );
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
            var res = EvaluationFunctions.eval(env, 0,  " [[Rec()]]" );
            //------------Assert Results-------------------------
            Assert.AreEqual(" 3, c",  CommonFunctions.evalResultToString(res) );
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsScalar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[x]]");
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
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec().a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Complete()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec()]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "3,c");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Complete_Star()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec(*)]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1,2,3,a,b,c");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Complete_Last()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec()]] ");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "3,c ");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Complete_Star_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec(*)]] ");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1,2,3,a,b,c ");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(Exception))]
        public void Eval_PositionsRecSet_ComplexScalar()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[a]] ");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1 ");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Index()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec(1).a]]");
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
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec(*).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1,2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Exp()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec([[x]]).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        [ExpectedException(typeof(NullValueInVariableException))]
        public void Eval_PositionsRecSet_ExpNonExist()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[greRec([[x]]).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(CommonFunctions.evalResultToString(res), "1");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_PositionsRecSet_Complex()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();



            //------------Execute Test---------------------------
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Rec(*).a]][[x]]");
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
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Person.Age]]");
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
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Person.Score(1)]]");
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
            var res = EvaluationFunctions.evalWithPositions(env, 0, "[[Person.Score(*)]]");
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
            var res = EvaluationFunctions.evalWithPositions(env, 0, " 1 2 3");
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
            var res = EvaluationFunctions.evalWithPositions(env, 0, " [[Rec()]]");
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
            var res = EvaluationFunctions.evalIndexes(env, 0, "[[Rec()]]");
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
            var res = EvaluationFunctions.evalIndexes(env, 0, " [[Rec().a]]");
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
            var res = EvaluationFunctions.evalIndexes(env, 0, " [[Rec()]][[Rec().a]]");
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
            var res = EvaluationFunctions.eval(env, 0, "[[@Person.Age]]");
            //------------Assert Results-------------------------
           Assert.AreEqual("22",CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_Json_Root()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[@Person]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("22", CommonFunctions.evalResultToString(res));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Eval")]
        public void Eval_JsonNested()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            //------------Execute Test---------------------------
            var res = EvaluationFunctions.eval(env, 0, "[[@Person.Spouse.Name]]");
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
            var res = EvaluationFunctions.eval(env, 0, "[[@Person.Children(1).Name]]");
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
            var res = EvaluationFunctions.eval(env, 0, "[[@Person.Children(*).Name]]");
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
            var res = EvaluationFunctions.eval(env, 0, "[[array(1)]]");
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
            var res = EvaluationFunctions.eval(env, 0, "[[arrayObj(1).Name]]");
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
            var res = EvaluationFunctions.eval(env, 0, "[[arrayObj(*).Name]]");
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
            var res = EvaluationFunctions.eval(env, 0, "[[arrayObj().Name]]");
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
            var res = EvaluationFunctions.evalJson(env, 0, EvaluationFunctions.parseLanguageExpressionWithoutUpdate( "[[arrayObj().Name]] [[arrayObj().Name]]"));
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
            var res = EvaluationFunctions.evalJson(env, 0, EvaluationFunctions.parseLanguageExpressionWithoutUpdate("1"));
            //------------Assert Results-------------------------
            Assert.AreEqual("bobe", CommonFunctions.evalResultToString(res));
        }

        public static DataStorage.WarewolfEnvironment CreateEnvironmentWithData()
        {

            ExecutionEnvironment env = new ExecutionEnvironment();
            env.Assign("[[Rec(1).a]]", "1", 0);
            env.Assign("[[Rec(2).a]]", "2", 0);
            env.Assign("[[Rec(3).a]]", "3", 0);
            env.Assign("[[non(3).a]]", "1", 0);
            env.Assign("[[non(2).a]]", "2", 0);
            env.Assign("[[non(1).a]]", "3", 0);
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
            env.AssignJson(new AssignValue("[[array(1)]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[arrayObj(1).Name]]", "bob"), 0);
            env.AssignJson(new AssignValue("[[arrayObj(2).Name]]", "bobe"), 0);
            PrivateObject p = new PrivateObject(env);
            return (DataStorage.WarewolfEnvironment)p.GetFieldOrProperty("_env");
        }
    }
}
