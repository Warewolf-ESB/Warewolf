using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BaseActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void DsfBaseActivityWhenHasVariableShouldEvaluate()
        {
            //---------------Set up test pack-------------------
            var act = new MySimpleActivity { Input1 = "[[OutVar1]]", Input2 = "[[OutVar2]]", Result = "[[ResultVar]]" };
            List<DebugItem> inRes;
            List<DebugItem> outRes;

            const string dataList = "<ADL><OutVar1/><OutVar2/><ResultVar/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<OutVar1>TestVal</OutVar1><OutVar2>TestVal2</OutVar2></ADL>";



            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = CheckActivityDebugInputOutput(act, dataList,
                dataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfBaseActivity_UpdateForEachInputs")]
        public void DsfBaseActivity_UpdateForEachInputs_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new MySimpleActivity() { Input1 = "SomeText" };
            var tuple1 = new Tuple<string, string>("Test1", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("SomeText", act.Input1);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfBaseActivity_UpdateForEachOutputs")]
        public void DsfBaseActivity_UpdateForEachOutputs_NullDoesNothing_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new MySimpleActivity { Input1 = "SomeText" };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("SomeText", act.Input1);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfBaseActivity_UpdateForEachOutputs")]
        public void DsfBaseActivity_UpdateForEachOutputsMoreThanTwoItems_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new MySimpleActivity { Input1 = "SomeText" };
            var tuple1 = new Tuple<string, string>("Test1", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("SomeText", act.Input1);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfBaseActivity_UpdateForEachOutputs")]
        public void DsfBaseActivity_UpdateForEachOutputs_UpdatesTextValue()
        {
            //------------Setup for test--------------------------
            var act = new MySimpleActivity { Result = "SomeText" };
            var tuple1 = new Tuple<string, string>("SomeText", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfBaseActivity_GetForEachInputs")]
        public void DsfBaseActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var act = new MySimpleActivity { Input1 = "SomeText" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfBaseActivity_GetForEachOutputs")]
        public void DsfBaseActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var act = new MySimpleActivity { Input1 = "SomeText" , Result = "[[Bob]]"};
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[Bob]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[Bob]]", dsfForEachItems[0].Value);
        }
    }

    internal sealed class MySimpleActivity : DsfBaseActivity
    {
        public MySimpleActivity()
        {
            DisplayName = "MySimpleActivty";
        }

        public override string DisplayName { get; set; }

        [Inputs("My Input 1")]
        public string Input1 { get; set; }

        [Inputs("My Input 2")]
        public string Input2 { get; set; }

        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            var result = evaluatedValues["Input1"] + " - " + evaluatedValues["Input2"];
            return result;
        }
    }
}