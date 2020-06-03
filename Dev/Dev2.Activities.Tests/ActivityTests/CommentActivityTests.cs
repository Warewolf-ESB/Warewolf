/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ActivityUnitTests;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.State;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;


namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    public class CommentActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        [Timeout(60000)]
        public void CommentGetDebugInputOutputWithText()
        {
            var act = new DsfCommentActivity { Text = "SomeText" };

            const string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string dataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";

            var result = CheckActivityDebugInputOutput(act, dataList,
                dataListWithData, out List<DebugItem> inRes, out List<DebugItem> outRes);

            // remove test datalist ;)

            Assert.AreEqual(0, inRes.Count);
            Assert.AreEqual(1, outRes.Count);
            var debugOutput = outRes[0].FetchResultsList();
            Assert.AreEqual(1, debugOutput.Count);
            Assert.AreEqual("SomeText", debugOutput[0].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[0].Type);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCommentActivity_UpdateForEachInputs")]
        public void DsfCommentActivity_UpdateForEachInputs_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new DsfCommentActivity { Text = "SomeText" };
            var tuple1 = new Tuple<string, string>("Test1", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("SomeText", act.Text);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCommentActivity_UpdateForEachOutputs")]
        public void DsfCommentActivity_UpdateForEachOutputs_NullDoesNothing_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new DsfCommentActivity { Text = "SomeText" };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("SomeText", act.Text);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCommentActivity_UpdateForEachOutputs")]
        public void DsfCommentActivity_UpdateForEachOutputsMoreThanTwoItems_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new DsfCommentActivity { Text = "SomeText" };
            var tuple1 = new Tuple<string, string>("Test1", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("SomeText", act.Text);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCommentActivity_UpdateForEachOutputs")]
        public void DsfCommentActivity_UpdateForEachOutputs_UpdatesTextValue()
        {
            //------------Setup for test--------------------------
            var act = new DsfCommentActivity { Text = "SomeText" };
            var tuple1 = new Tuple<string, string>("Test1", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Text);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCommentActivity_GetForEachInputs")]
        public void DsfCommentActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var act = new DsfCommentActivity { Text = "SomeText" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dsfForEachItems.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCommentActivity_GetForEachOutputs")]
        public void DsfCommentActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var act = new DsfCommentActivity { Text = "SomeText" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("SomeText", dsfForEachItems[0].Name);
            Assert.AreEqual("SomeText", dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfCommentActivity_GetState")]
        public void DsfCommentActivity_GetState_ReturnsStateVariable()
        {
            //------------Setup for test--------------------------
            var act = new DsfCommentActivity { Text = "SomeText" };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, stateItems.Count());
            Assert.AreEqual("Text", stateItems.ToList()[0].Name);
            Assert.AreEqual(StateVariable.StateType.InputOutput, stateItems.ToList()[0].Type);
            Assert.AreEqual("SomeText", stateItems.ToList()[0].Value);
        }
    }
}
