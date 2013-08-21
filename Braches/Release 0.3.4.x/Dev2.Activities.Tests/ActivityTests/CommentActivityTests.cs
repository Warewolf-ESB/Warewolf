using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    public class CommentActivityTests : BaseActivityUnitTest
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CommentGetDebugInputOutputWithText()
        {
            DsfCommentActivity act = new DsfCommentActivity { Text = "SomeText" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            string dataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            string dataListWithData = "<ADL>" +
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

            CheckActivityDebugInputOutput(act, dataList,
                dataListWithData, out inRes, out outRes);
            Assert.AreEqual(0, inRes.Count);
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> debugOutput = outRes[0].FetchResultsList();
            Assert.AreEqual(1, debugOutput.Count);
            Assert.AreEqual("SomeText", debugOutput[0].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[0].Type);
        }

    }
}