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
using System.Collections.Generic;
using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Exceptions;
using Warewolf.Storage;
using WarewolfParserInterop;


namespace WarewolfParsingTest
{
    [TestClass]
    public class TestDistinct
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Distinct_EvalDistinct")]
        public void Distinct_EvalDistinct_HappyPath_ExpectDistinctResults()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            
            //------------Execute Test---------------------------
            var modified = Distinct.evalDistinct(env, new List<string>() { "[[Rec(*).a]]" }, new List<string> { "[[Rec(*).a]]" }, 0, new List<string> { "[[Bec(*).a]]" });
            
            //------------Assert Results-------------------------
           var res = CommonFunctions.evalResultToString( EvaluationFunctions.eval(modified, 0, false, "[[Bec(*).a]]"));
            Assert.AreEqual(res,"1,2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Distinct_EvalDistinct")]
        [ExpectedException(typeof(NullValueInVariableException))]
        public void Distinct_EvalDistinct_MultipleRecsetsExpectSomethingBad()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var modified = Distinct.evalDistinct(env, new List<string>() { "[[Rec(*).a]][[bec().a]]" }, new List<string> { "[[Rec(*).a]]" }, 0, new List<string> { "[[Bec(*).a]]" });

            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(modified, 0, false, "[[Bec(*).a]]"));
            Assert.AreEqual(res, "1,2,3");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Distinct_EvalDistinct")]
        public void Distinct_EvalDistinct_HappyPath_ExpectDistinctResults_Expression()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var modified = Distinct.evalDistinct(env, new List<string>() { "[[Rec([[z]]).a]]" }, new List<string> { "[[Rec(*).a]]" }, 0, new List<string> { "[[Bec(*).a]]" });

            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(modified, 0, false, "[[Bec(*).a]]"));
            Assert.AreEqual(res, "1,2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Distinct_EvalDistinct")]
        public void Distinct_EvalDistinct_HappyPath_Last_ExpectDistinctResults()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var modified = Distinct.evalDistinct(env, new List<string>() { "[[Rec(*).a]]" }, new List<string> { "[[Rec(*).a]]" }, 0, new List<string> { "[[Bec().a]]" });

            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(modified, 0, false, "[[Bec(*).a]]"));
            Assert.AreEqual(res, "1,2,3");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Distinct_EvalDistinct")]
        [ExpectedException(typeof(Exception))]
        public void Distinct_EvalDistinct_HappyPath_indexExp_ExpectDistinctResults()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var modified = Distinct.evalDistinct(env, new List<string>() { "[[Rec(*).a]]" }, new List<string> { "[[Rec(*).a]]" }, 0, new List<string> { "[[Bec([[x]]).a]]" });

            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(modified, 0, false, "[[Bec(*).a]]"));
            Assert.AreEqual(res, "3");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Distinct_EvalDistinct")]
        public void Distinct_EvalDistinct_HappyPathScalarResult_ExpectDistinctResults()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var modified = Distinct.evalDistinct(env, new List<string>() { "[[Rec(*).a]]" }, new List<string> { "[[Rec(*).a]]" }, 0, new List<string> { "[[ax]]" });

            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(modified, 0, false, "[[ax]]"));
            Assert.AreEqual(res, "1,2,3");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Distinct_EvalDistinct")]
        [ExpectedException(typeof(Exception))]
        public void Distinct_EvalDistinct_HappyPathComplexResult_ExpectDistinctResults()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var modified = Distinct.evalDistinct(env, new List<string>() { "[[Rec(*).a]]" }, new List<string> { "[[Rec(*).a]]" }, 0, new List<string> { "[[ax]] [[bx]]" });

            //------------Assert Results-------------------------
            var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(modified, 0, false, "[[ax]]"));
            Assert.AreEqual(res, "1,2,3");
        }

        static DataStorage.WarewolfEnvironment CreateEnvironmentWithData()
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
            env.Assign("[[z]]", "*", 0);
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
    }

}
