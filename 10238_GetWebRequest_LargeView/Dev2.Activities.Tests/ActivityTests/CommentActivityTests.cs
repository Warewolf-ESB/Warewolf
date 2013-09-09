using System.Collections.Generic;
using ActivityUnitTests;
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
        public void CommentGetDebugInputOutputWithText()
        {
            DsfCommentActivity act = new DsfCommentActivity { Text = "SomeText" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

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
                dataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(0, inRes.Count);
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> debugOutput = outRes[0].FetchResultsList();
            Assert.AreEqual(1, debugOutput.Count);
            Assert.AreEqual("SomeText", debugOutput[0].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[0].Type);
        }

    }
}