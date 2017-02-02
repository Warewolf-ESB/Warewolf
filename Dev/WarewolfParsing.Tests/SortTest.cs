using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace WarewolfParsingTest
{
    [TestClass] 
    public class SortTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Sort_SortRecset")]
        public void Sort_SortRecset_ExpectSortedRecset()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();
            
            //------------Execute Test---------------------------
           var env2 = Sort.sortRecset("[[Rec(*).a]]", false, 0, env);
            //------------Assert Results-------------------------

           var res = CommonFunctions.evalResultToString(EvaluationFunctions.eval(env2, 0, false, "[[Rec(*).a]]"));

            Assert.AreEqual("1,2,2,3",res);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Sort_SortRecset")]
        public void Sort_SortRecset_ExpectSortedRecset_desc()
        {
            //------------Setup for test--------------------------
            var env = CreateEnvironmentWithData();

            //------------Execute Test---------------------------
            var env2 = Sort.sortRecset("[[Rec(*).a]]", true, 0, env);
            //------------Assert Results-------------------------

            var res = CommonFunctions.evalResultToString( EvaluationFunctions.eval(env2, 0, false, "[[Rec(*).a]]"));

            Assert.AreEqual("3,2,2,1", res);
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
