using Dev2;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;


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

            var data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + data + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);


            Assert.AreEqual(6, entry.ItemCollectionSize());
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

            var data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + data + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
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

            var data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + data + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
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

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
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

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();


            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void FindRecord_Get_Debug_Input_Output_With_Recordset_Using_Star_Index_With_Field_Expected_Pass()
        {
            DsfFindRecordsActivity act = new DsfFindRecordsActivity { FieldsToSearch = "[[Customers(*).DOB]]", SearchType = "Contains", SearchCriteria = "/", Result = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(30, inRes[1].FetchResultsList().Count);

            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void FindRecord_Get_Debug_Input_Output_With_Recordset_With_Star_Index_Expected_Pass()
        {
            DsfFindRecordsActivity act = new DsfFindRecordsActivity { FieldsToSearch = "[[Customers(*)]]", SearchType = "Contains", SearchCriteria = "/", Result = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(90, inRes[1].FetchResultsList().Count);

            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        #endregion

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
