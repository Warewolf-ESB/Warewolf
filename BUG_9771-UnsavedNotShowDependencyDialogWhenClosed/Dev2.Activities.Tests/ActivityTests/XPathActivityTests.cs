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
    public class XPathActivityTests : BaseActivityUnitTest
    {
        IList<XPathDTO> _resultsCollection = new List<XPathDTO>();
        string Source =  "<excludelist>" + "<namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.Behaviors\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.WindowManagers\" />" + "<namespace name=\"Dev2.Studio.ActivityDesigners\" />" + "<namespace name=\"Dev2.Studio.Views.Workflow\" />" + "<type name=\"Dev2.Activities.DsfExecuteCommandLineActivity\" />" + "<type name=\"Dev2.Activities.DsfForEachItem\" />" + "<type name=\"Dev2.Activities.DsfGatherSystemInformationActivity\" />" + "<type name=\"Dev2.Activities.DsfRandomActivity\" />" + "<type name=\"Dev2.DynamicServices.DsfDataObject\" excludetype=\"false\">" + "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />" + "<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />" + "</type>" + "<type name=\"Dev2.Runtime.Hosting.DynamicObjectHelper\" excludetype=\"false\">" + "<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />" + "</type>" + "<type name=\"Dev2.CommandLineParameters\">" + "<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />" + "<method name=\"GetUsage\" signature=\"string()\" />" + "<field name=\"&lt;Install&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;IntegrationTestMode&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StartService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StopService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;Uninstall&gt;k__BackingField\" signature=\"bool\" />" + "<propertymember name=\"Install\" />" + "<propertymember name=\"IntegrationTestMode\" />" + "<propertymember name=\"StartService\" />" + "<propertymember name=\"StopService\" />" + "<propertymember name=\"Uninstall\" />" + "</type>" + "<type name=\"Dev2.WebServer\" excludetype=\"false\">" + "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" + "</type>" + "</excludelist>";

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
            if (_resultsCollection == null)
            {
                _resultsCollection = new List<XPathDTO>();
            }
            _resultsCollection.Clear();
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod] // - OK
        public void EmptySourceStringExpectedNoData()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method", 1));
            string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataList + "</root>", dataSplitPreDataList, "", _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RecordsetWithAnIndexWithNoRecordsInRecSetExpectedPathsAndAppendRecords()
        {
            _resultsCollection.Add(new XPathDTO("[[recset1(3).field1]]", "//type/method", 1));
            SetUpActivityArguments();
            List<string> expected = new List<string> { "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.Where(c => c.ItemCollectionIndex >= 3).ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void ScalarExpectedPathsAndInsertToScalarInCSV()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method", 1));
            SetUpActivityArguments();
            IDSFDataObject result = ExecuteProcess();
            string expected = "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />,<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />,<method name=\"GetUsage\" signature=\"string()\" />,<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ScalarExpectedWithXPathInScalarPathsAndInsertToScalarInCSV()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "[[xpath]]", 1));
            string dataSplitPreDataList = "<ADL><xmlData/><xpath/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            string dataSplitPreDataListWithData = "<ADL><xmlData/><xpath>//type/method</xpath><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataListWithData + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();
            string expected = "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />,<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />,<method name=\"GetUsage\" signature=\"string()\" />,<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void MultipleScalarsExpectedXPathExecuteAndInsertMutipleScalars()
        {

            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[OutVar2]]", "//type/method/@signature", 2));
            SetUpActivityArguments();
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "name=\"ExtractInMergeDataFromRequest\",name=\"ExtractOutMergeDataFromRequest\",name=\"SetID\",name=\"&lt;GetUsage&gt;b__0\",name=\"GetUsage\",name=\"CreateForm\"", 
                                                                              "signature=\"void(object)\",signature=\"void(object)\",signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\",signature=\"void(CommandLine.Text.HelpText)\",signature=\"string()\",signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\"" };
            List<string> actual = new List<string>();

            for (int i = 1; i <= 2; i++)
            {
                string returnVal = string.Empty;
                string error = string.Empty;
                GetScalarValueFromDataList(result.DataListID, "OutVar" + i, out returnVal, out error);
                actual.Add(returnVal.Trim());
            }
            Utils.StringComparer comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }  
        
        [TestMethod]
        public void ScalarWithXPathInRecsetExpectedXPathExecuteAndInsertMutipleScalars()
        {

            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "[[xpaths(*).path]]", 1));
            //_resultsCollection.Add(new XPathDTO("[[OutVar2]]", "//type/method/@signature", 2));
            string dataSplitPreDataList = "<ADL><xmlData/><xpaths><path/></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            string dataSplitPreDataListWithData = "<ADL><xmlData/><xpaths><path>//type/method/@name</path></xpaths><xpaths><path>//type/method/@signature</path></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataListWithData + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "signature=\"void(object)\",signature=\"void(object)\",signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\",signature=\"void(CommandLine.Text.HelpText)\",signature=\"string()\",signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\"" };
            List<string> actual = new List<string>();

            for (int i = 1; i <= 1; i++)
            {
                string returnVal = string.Empty;
                string error = string.Empty;
                GetScalarValueFromDataList(result.DataListID, "OutVar" + i, out returnVal, out error);
                actual.Add(returnVal.Trim());
            }
            Utils.StringComparer comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }  
        
        [TestMethod]
        public void RecsetWithXPathInRecsetExpectedXPathExecuteAndInsertMutipleScalars()
        {

            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "[[xpaths(*).path]]", 1));
            //_resultsCollection.Add(new XPathDTO("[[OutVar2]]", "//type/method/@signature", 2));
            string dataSplitPreDataList = "<ADL><xmlData/><xpaths><path/></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            string dataSplitPreDataListWithData = "<ADL><xmlData/><xpaths><path>//type/method/@name</path></xpaths><xpaths><path>//type/method/@signature</path></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataListWithData + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "signature=\"void(object)\"","signature=\"void(object)\"","signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\"","signature=\"void(CommandLine.Text.HelpText)\"","signature=\"string()\"","signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\"" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            Utils.StringComparer comparer = new Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        public void MixedScalarsAndRecordsetWithIndexExpectedXPathEvalInsertMutipleScalarAndRecordsets()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[recset1(2).field1]]", "//type/method/@signature", 2));

            SetUpActivityArguments();

            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { @"" 
                , "signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\""
            };
            List<string> actual = new List<string>();
            string actualScalar = string.Empty;
            string error = string.Empty;
            IList<IBinaryDataListItem> actualRecordSet = new List<IBinaryDataListItem>();

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);

            Assert.AreEqual("name=\"ExtractInMergeDataFromRequest\",name=\"ExtractOutMergeDataFromRequest\",name=\"SetID\",name=\"&lt;GetUsage&gt;b__0\",name=\"GetUsage\",name=\"CreateForm\"", actualScalar);

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actualRecordSet, out error);
            foreach (IBinaryDataListItem entry in actualRecordSet)
            {
                actual.Add(entry.TheValue);
            }
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        void SetUpActivityArguments()
        {
            string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataList + "</root>", dataSplitPreDataList, Source, _resultsCollection);
        }

        
        [TestMethod]
        public void MixedScalarsAndRecordsetWithoutIndexExpectedXPathValuesToEndInsertingMutipleScalarAndRecordsets()
        {

            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[recset1().field1]]", "//type/method/@signature", 2));

            string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root></root>", dataSplitPreDataList, Source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "signature=\"void(object)\"","signature=\"void(object)\"",
                                                                                "signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\"","signature=\"void(CommandLine.Text.HelpText)\"",
                                                                                 "signature=\"string()\"","signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\""
                                };
            List<string> actual = new List<string>();
            string actualScalar = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);
            Assert.AreEqual("name=\"ExtractInMergeDataFromRequest\",name=\"ExtractOutMergeDataFromRequest\",name=\"SetID\",name=\"&lt;GetUsage&gt;b__0\",name=\"GetUsage\",name=\"CreateForm\"", actualScalar);

            actual.AddRange(RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error));

            string[] foo = actual.ToArray();
            actual.Clear();

            foreach (string s in foo)
            {
                actual.Add(s.Trim());
            }

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void MutiRecsetsWithNoIndexExpectedXPathResultsAppendToTheRecordsets()
        {
            _resultsCollection.Add(new XPathDTO("[[recset1().rec1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[recset1().field1]]", "//type/method/@signature", 2));

            SetupArguments("<root></root>", "<ADL><xmlData/><recset1>\r\n\t\t<field1/>\r\n\t\t<rec1/>\r\n\t</recset1>\r\n\t<recset2>\r\n\t\t<field2/>\r\n\t</recset2>\r\n\t<OutVar1/>\r\n\t<OutVar2/>\r\n\t<OutVar3/>\r\n\t<OutVar4/>\r\n\t<OutVar5/>\r\n</ADL>", Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "name=\"ExtractInMergeDataFromRequest\"","name=\"ExtractOutMergeDataFromRequest\"",
                                                                                "name=\"SetID\"","name=\"&lt;GetUsage&gt;b__0\"","name=\"GetUsage\"","name=\"CreateForm\"",
                                                                                "signature=\"void(object)\"","signature=\"void(object)\"",
                                                                                "signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\"","signature=\"void(CommandLine.Text.HelpText)\"",
                                                                                 "signature=\"string()\"","signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\""
                                                        
            };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "rec1", out error);
            actual.AddRange(RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error));

            string[] foo = actual.ToArray();
            actual.Clear();

            foreach (string s in foo)
            {
                actual.Add(s.Trim());
            }

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void RecsetWithStarExpectedXPathsResultsOverwriteRecordsFromIndex1()
        {

            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//type/method/@signature", 1));

            SetUpActivityArguments();

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "signature=\"void(object)\"","signature=\"void(object)\"",
                                                                                "signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\"","signature=\"void(CommandLine.Text.HelpText)\"",
                                                                                 "signature=\"string()\"","signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\""
                                };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            string[] foo = actual.ToArray();
            actual.Clear();
            foreach (string f in foo)
            {
                actual.Add(f.Trim());
            }

            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
        }

        [TestMethod]
        public void XPathGetDebugInputOutputWithRecordsetsExpectedPass()
        {
            IList<XPathDTO> resultsCollection = new List<XPathDTO>();
            resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//type/method/@signature", 1));
            string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataList + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            DsfXPathActivity act = new DsfXPathActivity { SourceString = Source, ResultsCollection = resultsCollection };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, dataSplitPreDataList,
                dataSplitPreDataList, out inRes, out outRes);
            Assert.AreEqual(2, inRes.Count);
            IList<DebugItemResult> sourceResultsList = inRes[0].FetchResultsList();
            Assert.AreEqual(2, sourceResultsList.Count);
            Assert.AreEqual("XML Source",sourceResultsList[0].Value);
            Assert.AreEqual(DebugItemResultType.Label,sourceResultsList[0].Type);
            Assert.AreEqual("<excludelist><namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" /><namespace n", sourceResultsList[1].Value);
            Assert.AreEqual(DebugItemResultType.Value,sourceResultsList[1].Type);
            StringAssert.Contains(sourceResultsList[1].MoreLink,"/Services/FetchDebugItemFileService?DebugItemFilePath=");
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> outputDebugItemResults = outRes[0].FetchResultsList();
            Assert.AreEqual(19, outputDebugItemResults.Count);          
        }

        [TestMethod]
        public void XpathGetDebugInputOutputWithRecordsetAppendExpectedPass()
        {
            IList<XPathDTO> resultsCollection = new List<XPathDTO>();
            resultsCollection.Add(new XPathDTO("[[recset1().field1]]", "//type/method/@signature", 1));
            string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataList + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            DsfXPathActivity act = new DsfXPathActivity { SourceString = Source, ResultsCollection = resultsCollection };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, dataSplitPreDataList,
                dataSplitPreDataList, out inRes, out outRes);
            Assert.AreEqual(2, inRes.Count);
            IList<DebugItemResult> fetchResultsList = inRes[0].FetchResultsList();
            Assert.AreEqual(2, fetchResultsList.Count);
            Assert.AreEqual("XML Source",fetchResultsList[0].Value);
            Assert.AreEqual(DebugItemResultType.Label,fetchResultsList[0].Type);
            Assert.AreEqual("<excludelist><namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" /><namespace n", fetchResultsList[1].Value);
            Assert.AreEqual(DebugItemResultType.Value,fetchResultsList[1].Type);
            StringAssert.Contains(fetchResultsList[1].MoreLink,"/Services/FetchDebugItemFileService?DebugItemFilePath=");
            Assert.AreEqual(1, outRes.Count);
            IList<DebugItemResult> outputDebugItemResults = outRes[0].FetchResultsList();
            Assert.AreEqual(22, outputDebugItemResults.Count);          
        }

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string sourceString, IList<XPathDTO> resultCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfXPathActivity() { SourceString = sourceString, ResultsCollection = resultCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}