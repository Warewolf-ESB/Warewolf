using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Collections.ObjectModel;
using System.Activities.Statements;
using Dev2;
using Dev2.DataList;

using Dev2.Diagnostics;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract;


namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for FindRecordsActivityTest
    /// </summary>
    [TestClass]
    public class FindRecordsActivityTest : BaseActivityUnitTest
    {
        public FindRecordsActivityTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        TestContext testContextInstance;

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

        [TestMethod]
        public void GreaterThan_FullRecordsetWithStarIndex_Expected_Results_For_2_3_6_7_8_9()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset(*)]]",
                    SearchCriteria = "32",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            CurrentDL = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);


            Assert.AreEqual(6,entry.ItemCollectionSize());
        }

        [TestMethod]
        public void GreaterThan_Expected_Results_For_2_3_6_7_8_9()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "32",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            CurrentDL = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);

            Assert.AreEqual(6, entry.ItemCollectionSize());
        }

        [TestMethod]
        public void GreaterThan_SearchWithIndex_Expected_Results_For_2_3_6_7_8_9()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset(*).Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "32",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            CurrentDL = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);

            Assert.AreEqual(6, entry.ItemCollectionSize());
        }

        [TestMethod]
        public void GreaterThan_WithTextInMatchField_Expected_NoResults()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "jimmy",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            CurrentDL = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);

            Assert.AreEqual(1, entry.ItemCollectionSize());
        }

        [TestMethod]
        public void ErrorHandeling_Expected_ErrorTag()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "2",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[//().res]]"
                }
            };

            CurrentDL = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();


            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        #region Get Input/Output Tests

        [TestMethod]
        public void FindRecords_GetInputs_Expected_Five_Input()
        {
            DsfFindRecordsActivity testAct = new DsfFindRecordsActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 5);
        }

        [TestMethod]
        public void FindRecords_GetOutputs_Expected_One_Output()
        {
            DsfFindRecordsActivity testAct = new DsfFindRecordsActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

    }
}
