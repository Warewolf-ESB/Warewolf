using ActivityUnitTests;
using Dev2.Activities;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]    
    // ReSharper disable InconsistentNaming
    public class XPathActivityTests : BaseActivityUnitTest
    {
        IList<XPathDTO> _resultsCollection = new List<XPathDTO>();
        private const string Source = "<excludelist>" + "<namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.Behaviors\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.WindowManagers\" />" + "<namespace name=\"Dev2.Studio.ActivityDesigners\" />" + "<namespace name=\"Dev2.Studio.Views.Workflow\" />" + "<type name=\"Dev2.Activities.DsfExecuteCommandLineActivity\" />" + "<type name=\"Dev2.Activities.DsfForEachItem\" />" + "<type name=\"Dev2.Activities.DsfGatherSystemInformationActivity\" />" + "<type name=\"Dev2.Activities.DsfRandomActivity\" />" + "<type name=\"Dev2.DynamicServices.DsfDataObject\" excludetype=\"false\">" + "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />" + "<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />" + "</type>" + "<type name=\"Dev2.Runtime.Hosting.DynamicObjectHelper\" excludetype=\"false\">" + "<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />" + "</type>" + "<type name=\"Dev2.CommandLineParameters\">" + "<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />" + "<method name=\"GetUsage\" signature=\"string()\" />" + "<field name=\"&lt;Install&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;IntegrationTestMode&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StartService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StopService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;Uninstall&gt;k__BackingField\" signature=\"bool\" />" + "<propertymember name=\"Install\" />" + "<propertymember name=\"IntegrationTestMode\" />" + "<propertymember name=\"StartService\" />" + "<propertymember name=\"StopService\" />" + "<propertymember name=\"Uninstall\" />" + "</type>" + "<type name=\"Dev2.WebServer\" excludetype=\"false\">" + "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" + "</type>" + "</excludelist>";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
        [TestInitialize]
        public void MyTestInitialize()
        {
            if(_resultsCollection == null)
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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPath_Execute")]
        public void XPath_Execute_WhenLoadingTestResultsFile_ExpectParsableXML()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method", 1));
            const string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataList + "</root>", dataSplitPreDataList, "", _resultsCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();

            //------------Assert Results-------------------------
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(string.Empty, actual);

        }

        [TestMethod] // - OK
        public void EmptySourceStringExpectedNoData()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method", 1));
            const string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataList + "</root>", dataSplitPreDataList, "", _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RecordsetWithAnIndexWithNoRecordsInRecSetExpectedPathsAndAppendRecords()
        {
            _resultsCollection.Add(new XPathDTO("[[recset1(3).field1]]", "//type/method", 1));
            SetUpActivityArguments();
            List<string> expected = new List<string> { "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            List<string> actualRet = new List<string>();
            actual.Where(c => c.ItemCollectionIndex >= 3).ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void ScalarExpectedPathsAndInsertToScalarInCsv()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method", 1));
            SetUpActivityArguments();
            IDSFDataObject result = ExecuteProcess();
            const string Expected = "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />,<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />,<method name=\"GetUsage\" signature=\"string()\" />,<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(Expected, actual, "Got " + actual + " expected " + Expected);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ScalarExpectedWithXPathInScalarPathsAndInsertToScalarInCsv()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "[[xpath]]", 1));
            const string dataSplitPreDataList = "<ADL><xmlData/><xpath/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            const string dataSplitPreDataListWithData = "<ADL><xmlData/><xpath>//type/method</xpath><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataListWithData + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();
            const string expected = "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />,<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />,<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />,<method name=\"GetUsage\" signature=\"string()\" />,<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathActivity_OnExecute")]
        public void XPathActivity_Execute_MultipleScalars_XPathExecuteAndInsertMutipleScalars()
        {

            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[OutVar2]]", "//type/method/@signature", 2));
            SetUpActivityArguments();
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "ExtractInMergeDataFromRequest,ExtractOutMergeDataFromRequest,SetID,<GetUsage>b__0,GetUsage,CreateForm", 
                                                       "void(object),void(object),void(Dev2.DynamicServices.IDynamicServiceObject, object),void(CommandLine.Text.HelpText),string(),Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)" };
            List<string> actual = new List<string>();

            for(int i = 1; i <= 2; i++)
            {
                string returnVal;
                string error;
                GetScalarValueFromDataList(result.DataListID, "OutVar" + i, out returnVal, out error);

                actual.Add(returnVal.Trim());
            }
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathActivity_OnExecute")]
        public void XPathActivity_Execute_ScalarWithXPathInRecset_XPathExecuteAndInsertMutipleScalars()
        {

            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "[[xpaths(*).path]]", 1));
            const string dataSplitPreDataList = "<ADL><xmlData/><xpaths><path/></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            const string dataSplitPreDataListWithData = "<ADL><xmlData/><xpaths><path>//type/method/@name</path></xpaths><xpaths><path>//type/method/@signature</path></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataListWithData + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "void(object),void(object),void(Dev2.DynamicServices.IDynamicServiceObject, object),void(CommandLine.Text.HelpText),string(),Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)" };
            List<string> actual = new List<string>();

            for(int i = 1; i <= 1; i++)
            {
                string returnVal;
                string error;
                GetScalarValueFromDataList(result.DataListID, "OutVar" + i, out returnVal, out error);
                actual.Add(returnVal.Trim());
            }

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        public void RecsetWithXPathInRecsetExpectedXPathExecuteAndInsertMutipleScalars()
        {

            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "[[xpaths(*).path]]", 1));
            const string dataSplitPreDataList = "<ADL><xmlData/><xpaths><path/></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            const string dataSplitPreDataListWithData = "<ADL><xmlData/><xpaths><path>//type/method/@name</path></xpaths><xpaths><path>//type/method/@signature</path></xpaths><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataListWithData + "</root>", dataSplitPreDataList, Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "void(object)", "void(object)", "void(Dev2.DynamicServices.IDynamicServiceObject, object)", "void(CommandLine.Text.HelpText)", "string()", "Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathActivity_OnExecute")]
        public void MixedScalarsAndRecordsetWithIndexExpectedXPathEvalInsertMutipleScalarAndRecordsets()
        {
            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[recset1(2).field1]]", "//type/method/@signature", 2));

            SetUpActivityArguments();

            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { @"" 
                , "Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)"
            };
            string actualScalar;
            string error;
            IList<IBinaryDataListItem> actualRecordSet;

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);

            Assert.AreEqual("ExtractInMergeDataFromRequest,ExtractOutMergeDataFromRequest,SetID,<GetUsage>b__0,GetUsage,CreateForm", actualScalar);

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actualRecordSet, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            List<string> actual = actualRecordSet.Select(entry => entry.TheValue).ToList();
            var comparer = new ActivityUnitTests.Utils.StringComparer();

            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XPathActivity_OnExecute")]
        public void XPathActivity_Execute_MixedScalarsAndRecordsetWithoutIndex_XPathValuesToEndInsertingMutipleScalarAndRecordsets()
        {

            _resultsCollection.Add(new XPathDTO("[[OutVar1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[recset1().field1]]", "//type/method/@signature", 2));

            const string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root></root>", dataSplitPreDataList, Source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "void(object)","void(object)",
                                                        "void(Dev2.DynamicServices.IDynamicServiceObject, object)","void(CommandLine.Text.HelpText)",
                                                         "string()","Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)"
                                };
            List<string> actual = new List<string>();
            string actualScalar;
            string error;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);


            Assert.AreEqual("ExtractInMergeDataFromRequest,ExtractOutMergeDataFromRequest,SetID,<GetUsage>b__0,GetUsage,CreateForm", actualScalar);

            actual.AddRange(RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error));

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            string[] foo = actual.ToArray();
            actual.Clear();

            actual.AddRange(foo.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void MutiRecsetsWithNoIndexExpectedXPathResultsAppendToTheRecordsets()
        {
            _resultsCollection.Add(new XPathDTO("[[recset1().rec1]]", "//type/method/@name", 1));
            _resultsCollection.Add(new XPathDTO("[[recset1().field1]]", "//type/method/@signature", 2));

            SetupArguments("<root></root>", "<ADL><xmlData/><recset1>\r\n\t\t<field1/>\r\n\t\t<rec1/>\r\n\t</recset1>\r\n\t<recset2>\r\n\t\t<field2/>\r\n\t</recset2>\r\n\t<OutVar1/>\r\n\t<OutVar2/>\r\n\t<OutVar3/>\r\n\t<OutVar4/>\r\n\t<OutVar5/>\r\n</ADL>", Source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "ExtractInMergeDataFromRequest","ExtractOutMergeDataFromRequest",
                                                    "SetID","<GetUsage>b__0","GetUsage","CreateForm",
                                                    "void(object)","void(object)",
                                                    "void(Dev2.DynamicServices.IDynamicServiceObject, object)","void(CommandLine.Text.HelpText)",
                                                    "string()","Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)"
                                                        
            };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "rec1", out error);
            actual.AddRange(RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error));

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            string[] foo = actual.ToArray();
            actual.Clear();

            actual.AddRange(foo.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void RecsetWithStarExpectedXPathsResultsOverwriteRecordsFromIndex1()
        {

            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//type/method/@signature", 1));

            SetUpActivityArguments();

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "void(object)","void(object)",
                                                        "void(Dev2.DynamicServices.IDynamicServiceObject, object)","void(CommandLine.Text.HelpText)",
                                                        "string()","Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)"
                                };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            string[] foo = actual.ToArray();
            actual.Clear();
            actual.AddRange(foo.Select(f => f.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [TestCategory("XPathActivity_Execution")]
        [Description("XPathActivity execute upserts one result only")]
        [Owner("Ashley Lewis")]
        public void XPath_Execute_RecordsetWithStar_OneXPathResultUpserted()
        {
            //init
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            SetupArguments("<root><recset1><field1/></recset1></root>", "<root><recset1><field1/></recset1></root>", "<x><a>1</a></x>", _resultsCollection);

            //exe
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            //assert
            Assert.AreEqual(string.Empty, error, "XPath execution threw error: " + error);
            Assert.AreEqual(1, actual.Count, "XPath tool upserted to many results");
            Assert.AreEqual("1", actual[0], "XPath tool upserted the wrong result");
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfXPathActivity_UpdateForEachInputs")]
        public void DsfXPathActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            var act = new DsfXPathActivity { ResultsCollection = _resultsCollection, SourceString = "xml" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[recset1(*).field1]]", act.ResultsCollection[0].OutputVariable);
            Assert.AreEqual("//x/a/text()", act.ResultsCollection[0].XPath);
            Assert.AreEqual("xml", act.SourceString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfXPathActivity_UpdateForEachInputs")]
        public void DsfXPathActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            var act = new DsfXPathActivity { ResultsCollection = _resultsCollection, SourceString = "xml" };

            var tuple1 = new Tuple<string, string>("//x/a/text()", "Test");
            var tuple2 = new Tuple<string, string>("xml", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[recset1(*).field1]]", act.ResultsCollection[0].OutputVariable);
            Assert.AreEqual("Test", act.ResultsCollection[0].XPath);
            Assert.AreEqual("Test2", act.SourceString);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfXPathActivity_UpdateForEachOutputs")]
        public void DsfXPathActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            var act = new DsfXPathActivity { ResultsCollection = _resultsCollection, SourceString = "xml" };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[recset1(*).field1]]", act.ResultsCollection[0].OutputVariable);
            Assert.AreEqual("//x/a/text()", act.ResultsCollection[0].XPath);
            Assert.AreEqual("xml", act.SourceString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfXPathActivity_UpdateForEachOutputs")]
        public void DsfXPathActivity_UpdateForEachOutputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            var act = new DsfXPathActivity { ResultsCollection = _resultsCollection, SourceString = "xml" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("[[recset1(*).field1]]", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.ResultsCollection[0].OutputVariable);
            Assert.AreEqual("//x/a/text()", act.ResultsCollection[0].XPath);
            Assert.AreEqual("xml", act.SourceString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachOutputs")]
        public void DsfXPathActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            var act = new DsfXPathActivity { ResultsCollection = _resultsCollection, SourceString = "xml" };

            var tuple1 = new Tuple<string, string>("[[recset1(*).field1]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.ResultsCollection[0].OutputVariable);
            Assert.AreEqual("//x/a/text()", act.ResultsCollection[0].XPath);
            Assert.AreEqual("xml", act.SourceString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_GetForEachInputs")]
        public void DsfXPathActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            var act = new DsfXPathActivity { ResultsCollection = _resultsCollection, SourceString = "xml" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual("xml", dsfForEachItems[0].Name);
            Assert.AreEqual("xml", dsfForEachItems[0].Value);
            Assert.AreEqual("//x/a/text()", dsfForEachItems[1].Name);
            Assert.AreEqual("//x/a/text()", dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfXPathActivity_GetForEachOutputs")]
        public void DsfXPathActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Add(new XPathDTO("[[recset1(*).field1]]", "//x/a/text()", 1));
            var act = new DsfXPathActivity { ResultsCollection = _resultsCollection, SourceString = "xml" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[recset1(*).field1]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[recset1(*).field1]]", dsfForEachItems[0].Value);
        }


        #region Private Test Methods


        void SetUpActivityArguments()
        {
            const string dataSplitPreDataList = "<ADL><xmlData/><recset1><field1/></recset1><recset2><field2/></recset2><OutVar1/><OutVar2/><OutVar3/><OutVar4/><OutVar5/></ADL>";
            SetupArguments("<root>" + dataSplitPreDataList + "</root>", dataSplitPreDataList, Source, _resultsCollection);
        }



        private void SetupArguments(string currentDL, string testData, string sourceString, IList<XPathDTO> resultCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfXPathActivity { SourceString = sourceString, ResultsCollection = resultCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}