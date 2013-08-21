using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    public class UniqueActivityTests : BaseActivityUnitTest
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
        public void EmptyInFieldsStringExpectedNoUnique()
        {
            string dataList = "<ADL><recset1>\r\n\t\t<field1/>\r\n\t</recset1>\r\n\t<recset2>\r\n\t\t<field2/>\r\n\t</recset2>\r\n\t<OutVar1/>\r\n\t<OutVar2/>\r\n\t<OutVar3/>\r\n\t<OutVar4/>\r\n\t<OutVar5/>\r\n</ADL>";
            SetupArguments("<root>" + dataList + "</root>", dataList, "", "[[recset1().field1]]","[[OutVar1]]");
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RecordsetWithWithNoRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
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
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                ,"[[recset1().field2]]"
                , "[[recset1().field1]]","[[recset2().id]]");
            List<string> expected = new List<string> { "1","2","5" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void ScalarExpectedUniqueAsCSV()
        {
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
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                ,"[[recset1().field2]]"
                , "[[recset1().field1]]","[[OutVar1]]");
            string expected = "1,2,5";

            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
        }


        [TestMethod]
        public void RecordsetWithWithRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
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
                                                        "<recset2>" +
                                                            "<id>10</id><value>zz</value>" +
                                                            "</recset2>" +
                                                        "<OutVar1/></ADL>";
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]]", "[[recset2().id]]");
            List<string> expected = new List<string> { "10","1", "2", "5" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void RecordsetWithWithMulitpleRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
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
            SetupArguments("<root>" + dataListWithData + "</root>"
                , dataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]],[[recset1().field3]]", "[[recset2().id]],[[recset2().value]]");
            List<string> expectedID = new List<string> { "1", "2", "5" };
            List<string> expectedValue = new List<string> { "Test1", "Test2", "Test5" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expectedID, actualRet, comparer); 
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "value", out actual, out error);
            actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d.TheValue));
            comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expectedValue, actualRet, comparer);
        }

        [TestMethod]
        public void UniqueGetDebugInputOutputWithScalarsExpectedPass()
        {
            DsfUniqueActivity act = new DsfUniqueActivity { InFields = "[[recset1().field2]]", ResultFields = "[[recset1().field1]]",Result = "[[OutVar1]]"};

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
            Assert.AreEqual(2, inRes.Count);
            IList<DebugItemResult> fetchResultsList = inRes[0].FetchResultsList();
            Assert.AreEqual(18, fetchResultsList.Count);
            Assert.AreEqual("1",fetchResultsList[0].Value);
            Assert.AreEqual(DebugItemResultType.Label,fetchResultsList[0].Type);
            Assert.AreEqual("In Fields", fetchResultsList[1].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[1].Type);
            Assert.AreEqual("=",fetchResultsList[2].Value);
            Assert.AreEqual(DebugItemResultType.Label,fetchResultsList[2].Type);

            Assert.AreEqual("[[recset1(1).field2]]",fetchResultsList[3].Value);
            Assert.AreEqual(DebugItemResultType.Variable,fetchResultsList[3].Type);
            Assert.AreEqual("=", fetchResultsList[4].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[4].Type);
            Assert.AreEqual("a", fetchResultsList[5].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[5].Type);

            Assert.AreEqual("[[recset1(2).field2]]", fetchResultsList[6].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[6].Type);
            Assert.AreEqual("=", fetchResultsList[7].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[7].Type);
            Assert.AreEqual("b", fetchResultsList[8].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[8].Type);

           
            Assert.AreEqual("[[recset1(3).field2]]", fetchResultsList[9].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[9].Type);
            Assert.AreEqual("=", fetchResultsList[10].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[10].Type);
            Assert.AreEqual("a", fetchResultsList[11].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[11].Type);

            Assert.AreEqual("[[recset1(4).field2]]", fetchResultsList[12].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[12].Type);
            Assert.AreEqual("=", fetchResultsList[13].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[13].Type);
            Assert.AreEqual("a", fetchResultsList[14].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[14].Type);

            Assert.AreEqual("[[recset1(5).field2]]", fetchResultsList[15].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[15].Type);
            Assert.AreEqual("=", fetchResultsList[16].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[16].Type);
            Assert.AreEqual("c", fetchResultsList[17].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[17].Type);

            IList<DebugItemResult> debugItemResults = inRes[1].FetchResultsList();
            Assert.AreEqual(2, debugItemResults.Count);
            Assert.AreEqual("Return Fields", debugItemResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugItemResults[0].Type);
            Assert.AreEqual("[[recset1().field1]]", debugItemResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugItemResults[1].Type);
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> debugOutput = outRes[0].FetchResultsList();
            Assert.AreEqual(4, debugOutput.Count);
            Assert.AreEqual("1", debugOutput[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[0].Type);
            Assert.AreEqual("[[OutVar1]]", debugOutput[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[1].Type);
            Assert.AreEqual("=", debugOutput[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[2].Type);
            Assert.AreEqual("1,2,5", debugOutput[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[3].Type);
        }

        [TestMethod]
        public void UniqueGetDebugInputOutputWithRecordsetExpectedPass()
        {
            DsfUniqueActivity act = new DsfUniqueActivity { InFields = "[[recset1().field2]]", ResultFields = "[[recset1().field1]]",Result = "[[recset2().id]]"};

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
            Assert.AreEqual(2, inRes.Count);
            IList<DebugItemResult> fetchResultsList = inRes[0].FetchResultsList();
            Assert.AreEqual(18, fetchResultsList.Count);
            Assert.AreEqual("1",fetchResultsList[0].Value);
            Assert.AreEqual(DebugItemResultType.Label,fetchResultsList[0].Type);
            Assert.AreEqual("In Fields", fetchResultsList[1].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[1].Type);
            Assert.AreEqual("=",fetchResultsList[2].Value);
            Assert.AreEqual(DebugItemResultType.Label,fetchResultsList[2].Type);

            Assert.AreEqual("[[recset1(1).field2]]",fetchResultsList[3].Value);
            Assert.AreEqual(DebugItemResultType.Variable,fetchResultsList[3].Type);
            Assert.AreEqual("=", fetchResultsList[4].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[4].Type);
            Assert.AreEqual("a", fetchResultsList[5].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[5].Type);

            Assert.AreEqual("[[recset1(2).field2]]", fetchResultsList[6].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[6].Type);
            Assert.AreEqual("=", fetchResultsList[7].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[7].Type);
            Assert.AreEqual("b", fetchResultsList[8].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[8].Type);

           
            Assert.AreEqual("[[recset1(3).field2]]", fetchResultsList[9].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[9].Type);
            Assert.AreEqual("=", fetchResultsList[10].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[10].Type);
            Assert.AreEqual("a", fetchResultsList[11].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[11].Type);

            Assert.AreEqual("[[recset1(4).field2]]", fetchResultsList[12].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[12].Type);
            Assert.AreEqual("=", fetchResultsList[13].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[13].Type);
            Assert.AreEqual("a", fetchResultsList[14].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[14].Type);

            Assert.AreEqual("[[recset1(5).field2]]", fetchResultsList[15].Value);
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[15].Type);
            Assert.AreEqual("=", fetchResultsList[16].Value);
            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[16].Type);
            Assert.AreEqual("c", fetchResultsList[17].Value);
            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[17].Type);

            IList<DebugItemResult> debugItemResults = inRes[1].FetchResultsList();
            Assert.AreEqual(2, debugItemResults.Count);
            Assert.AreEqual("Return Fields", debugItemResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugItemResults[0].Type);
            Assert.AreEqual("[[recset1().field1]]", debugItemResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugItemResults[1].Type);
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> debugOutput = outRes[0].FetchResultsList();
            Assert.AreEqual(10, debugOutput.Count);
           
            Assert.AreEqual("1", debugOutput[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[0].Type);
            Assert.AreEqual("[[recset2(1).id]]", debugOutput[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[1].Type);
            Assert.AreEqual("=", debugOutput[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[2].Type);
            Assert.AreEqual("1", debugOutput[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[3].Type);

            Assert.AreEqual("[[recset2(2).id]]", debugOutput[4].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[4].Type);
            Assert.AreEqual("=", debugOutput[5].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[5].Type);
            Assert.AreEqual("2", debugOutput[6].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[6].Type);
            
            Assert.AreEqual("[[recset2(3).id]]", debugOutput[7].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutput[7].Type);
            Assert.AreEqual("=", debugOutput[8].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutput[8].Type);
            Assert.AreEqual("5", debugOutput[9].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutput[9].Type);
        }

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string inFields, string resultFields,string result)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfUniqueActivity() { InFields = inFields, ResultFields = resultFields,Result = result}
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}