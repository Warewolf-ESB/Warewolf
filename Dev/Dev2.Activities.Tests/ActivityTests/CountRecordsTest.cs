using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class CountRecordsTest : BaseActivityUnitTest
    {
        public CountRecordsTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Store To Scalar Tests

        [TestMethod]
        public void CountOutputToScalar_Expected_ScalarValueCorrectlySetToRecordSetCount()
        {

            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            string expected = @"5";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "TestCountvar", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        //Bug 7853
        [TestMethod]
        public void CountOutputToScalar_With_EmptyRecSet_Expected_ScalarValueCorrectlySetTo0()
        {

            SetupArguments("<root><ADL><TestCountvar/></ADL></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            string expected = @"0";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "TestCountvar", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion Store To Scalar Tests

        #region Store To RecordSet Tests

        [TestMethod]
        public void CountOutputToRecset()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            IDSFDataObject result = ExecuteProcess();

            string expected = "5";
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            string actualSet = actual.First(c => c.FieldName == "field1" && !string.IsNullOrEmpty(c.TheValue)).TheValue;
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actualSet);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        //2013.02.12: Ashley Lewis - Bug 8725, Task 8831 DONE
        [TestMethod]
        public void CountTwiceWithEmptyRecsetExpectedOutputToRecsetsSelf()
        {
            SetupArguments("<root></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            IDSFDataObject result = ExecuteProcess();

            string expected = "0";
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            string actualSet = actual.First(c => c.FieldName == "field1" && c.ItemCollectionIndex == 1).TheValue;

            SetupArguments("<root></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            result = ExecuteProcess();


            string expected2 = "1";
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            string actualSet2 = actual.First(c => c.FieldName == "field1" && c.ItemCollectionIndex == 2).TheValue;

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actualSet);
                Assert.AreEqual(expected2, actualSet2);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion Store To RecordSet Tests

        #region Error Test Cases

        [TestMethod]
        public void CountWithNoRecsetName_Expected_ErrorPopulatedFromDataList()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", string.Empty, "[[TestCountvar]]");
            IDSFDataObject result = ExecuteProcess();

            string unexpected = string.Empty;
            string error = string.Empty;


            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void CountWithNoOutputVariable()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", string.Empty);
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void CountOnScalar()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[TestCountVar]]", "[[TestCountVar]]");
            string actual = string.Empty;
            string error = string.Empty;
            IDSFDataObject result = ExecuteProcess();

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }


        [TestMethod]
        public void CountRecords_ErrorHandeling_Expected_ErrorTag()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[//().rec]]");

            IDSFDataObject result = ExecuteProcess();
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #endregion Error Test Cases

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void CountRecordsetGetDebugInputOutputWithRecordsetExpectedPass()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(92, inRes[0].Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8875 
        /// </summary>
        [TestMethod]
        public void CountRecordsetGetDebugInputOutputWithEmptyRecordsetExpectedPass()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, "<ADL><Customers><Fname></Fname></Customers><res></res></ADL>", "<ADL></ADL>", out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
            Assert.AreEqual("0", outRes[0][2].Value);
        }

        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void CountRecordsetActivity_GetInputs_Expected_One_Input()
        {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 1);
        }

        [TestMethod]
        public void CountRecordsetActivity_GetOutputs_Expected_One_Output()
        {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string recordSetName, string countNumber)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCountRecordsetActivity { RecordsetName = recordSetName, CountNumber = countNumber }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
