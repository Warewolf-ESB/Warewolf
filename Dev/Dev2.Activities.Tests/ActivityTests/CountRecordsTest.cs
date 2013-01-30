using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2;
using Dev2.DataList;
using Dev2.DataList.Contract.Binary_Objects;

namespace ActivityUnitTests.ActivityTest {
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class CountRecordsTest : BaseActivityUnitTest {
        public CountRecordsTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
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
        public void CountOutputToScalar_Expected_ScalarValueCorrectlySetToRecordSetCount() {

            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            string expected = @"5";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "TestCountvar", out actual, out error);
            if(string.IsNullOrEmpty(error)) {
                Assert.AreEqual(expected, actual);
            }
            else {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        //Bug 7853
        [TestMethod]
        public void CountOutputToScalar_With_EmptyRecSet_Expected_ScalarValueCorrectlySetTo0()
        {
            var test =ActivityStrings.CountRecordsDataListShape;

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
        public void CountOutputToRecset() {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            IDSFDataObject result = ExecuteProcess();

            string expected = "5";
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            string actualSet = actual.First(c => c.FieldName == "field1" && !string.IsNullOrEmpty(c.TheValue)).TheValue;
            if(string.IsNullOrEmpty(error)) {
                Assert.AreEqual(expected, actualSet);
            }
            else {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion Store To RecordSet Tests

        #region Error Test Cases

        [TestMethod]
        public void CountWithNoRecsetName_Expected_ErrorPopulatedFromDataList() {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", string.Empty, "[[TestCountvar]]");
            IDSFDataObject result = ExecuteProcess();

            string unexpected = string.Empty;
            string error = string.Empty;


            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void CountWithNoOutputVariable() {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", string.Empty);
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;

            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void CountOnScalar() {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[TestCountVar]]", "[[TestCountVar]]");
            string actual = string.Empty;
            string error = string.Empty;
            IDSFDataObject result = ExecuteProcess();

            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }


        [TestMethod]
        public void CountRecords_ErrorHandeling_Expected_ErrorTag() {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[//().rec]]");

            IDSFDataObject result = ExecuteProcess();
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;

            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        #endregion Error Test Cases

        #region Get Input/Output Tests

        [TestMethod]
        public void CountRecordsetActivity_GetInputs_Expected_One_Input() {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 1);
        }

        [TestMethod]
        public void CountRecordsetActivity_GetOutputs_Expected_One_Output() {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string recordSetName, string countNumber) {
            TestStartNode = new FlowStep {
                Action = new DsfCountRecordsetActivity { RecordsetName = recordSetName, CountNumber = countNumber }
            };

            CurrentDL = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
