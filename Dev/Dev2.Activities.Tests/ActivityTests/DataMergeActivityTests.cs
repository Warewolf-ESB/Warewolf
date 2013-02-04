using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    public class DataMergeActivityTest : BaseActivityUnitTest
    {
        IList<DataMergeDTO> _mergeCollection = new List<DataMergeDTO>();
        public DataMergeActivityTest()
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            if (_mergeCollection == null)
            {
                _mergeCollection = new List<DataMergeDTO>();
            }
            _mergeCollection.Clear();
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Language Tests

        [TestMethod]
        public void Merge_Two_Sclars_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[CompanyTelNo]]", "None", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual("Dev2,0317641234", actual);
        }

        [TestMethod]
        public void Merge_Recordset_With_Star_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", ",", 1, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual("Wallis,Barney,Trevor,Travis,Jurie,Brendon,Massimo,Ashley,Sashen,Wallis,", actual);
        }

        // SN: 16-01-2012 - Added to test with merge character being the last character of an entry in 
        //                  DataList
        [TestMethod]
        public void Merge_RecordsetWithStarCharMerge_Given_CharSameAsLastCharacterInEntry_Expected_DataMergedTogether()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", "s", 1, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual("WallissBarneysTrevorsTravissJuriesBrendonsMassimosAshleysSashensWalliss", actual);
        }

        [TestMethod]
        public void Merge_Recordset_And_Scalar_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(4).FirstName]]", "Chars", " works at ", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ".", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual("Travis works at Dev2.", actual);
        }

        [TestMethod]
        public void Merge_Two_Recordsets_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", "'s phone number is ", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[TelNumbers(*).number]]", "Chars", ".", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual("Wallis's phone number is 0811452368.Barney's phone number is 0821452368.Trevor's phone number is 0831452368.Travis's phone number is 0841452368.Jurie's phone number is 0851452368.Brendon's phone number is 0861452368.Massimo's phone number is 0871452368.Ashley's phone number is 0881452368.Sashen's phone number is 0891452368.Wallis's phone number is 0801452368.", actual);
        }

        #endregion Language Tests

        #region New Line Merge Tests

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_NewLine_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", ",", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, @"res", out actual, out error);

            Assert.AreEqual(@"Wallis,Buchan
Barney,Buchan
Trevor,Williams-Ros
Travis,Frisigner
Jurie,Smit
Brendon,Page
Massimo,Guerrera
Ashley,Lewis
Sashen,Naidoo
Wallis,Buchan
", actual);
        }

        #endregion New Line Merge Tests

        #region Tab Merge Tests

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Tab_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", ",", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "Tab", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual(@"Wallis,Buchan	Barney,Buchan	Trevor,Williams-Ros	Travis,Frisigner	Jurie,Smit	Brendon,Page	Massimo,Guerrera	Ashley,Lewis	Sashen,Naidoo	Wallis,Buchan	", actual);
        }

        #endregion Tab Merge Tests

        #region Index Tests

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Index_Merge_Blank_Padding_Left_Align_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Index", "1", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual(@"WBuchan
BBuchan
TWilliams-Ros
TFrisigner
JSmit
BPage
MGuerrera
ALewis
SNaidoo
WBuchan
", actual);
        }

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Index_Merge_Char_Padding_Left_Align_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Index", "10", 1, "0", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual(@"Wallis0000Buchan
Barney0000Buchan
Trevor0000Williams-Ros
Travis0000Frisigner
Jurie00000Smit
Brendon000Page
Massimo000Guerrera
Ashley0000Lewis
Sashen0000Naidoo
Wallis0000Buchan
", actual);
        }

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Index_Merge_Char_Padding_Right_Align_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Index", "10", 1, "0", "Right"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual(@"0000WallisBuchan
0000BarneyBuchan
0000TrevorWilliams-Ros
0000TravisFrisigner
00000JurieSmit
000BrendonPage
000MassimoGuerrera
0000AshleyLewis
0000SashenNaidoo
0000WallisBuchan
", actual);
        }

        #endregion Index Tests

        #region Negative Tests

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Zero_Index_In_Recordset_Expected_No_Data_Merged()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(0).FirstName]]", "Index", "1", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(1).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Error", out actual, out error);

            Assert.AreEqual(@"<InnerError>Recordset index [ 0 ] is not greater than zero</InnerError><InnerError>Index [ 0 ] is out of bounds</InnerError>", actual);
        }

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Negative_Index_In_Recordset_Expected_No_Data_Merged()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(-1).FirstName]]", "Index", "1", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(1).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Error", out actual, out error);

            Assert.AreEqual(@"<InnerError>Recordset index [ -1 ] is not greater than zero</InnerError>", actual);
        }

        #endregion

        #region GetWizardData Tests

        [TestMethod]
        public void GetWizardData_Expected_Correct_IBinaryDataList()
        {
            bool passTest = true;
            IList<DataMergeDTO> _mergeCollection = new List<DataMergeDTO>() { new DataMergeDTO("[[result]]", "Index", "5", 1, "", "Left"), new DataMergeDTO("[[result1]]", "Index", "1", 2, "", "Left") };

            DsfDataMergeActivity testAct = new DsfDataMergeActivity { MergeCollection = _mergeCollection, Result = "[[res]]" };

            IBinaryDataList binaryDL = testAct.GetWizardData();
            var recsets = binaryDL.FetchRecordsetEntries();
            var scalars = binaryDL.FetchScalarEntries();
            if (recsets.Count != 1 && scalars.Count != 2)
            {
                passTest = false;
            }
            else
            {
                if (recsets[0].Columns.Count != 3)
                {
                    passTest = false;
                }
            }
            Assert.IsTrue(passTest);
        }

        #endregion GetWizardData Tests

        #region Get Debug Input/Output Tests
        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DataMerge_Get_Debug_Input_Output_With_Scalars_Expected_Pass()
        {
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            DsfDataMergeActivity act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(7, inRes[0].Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DataMerge_Get_Debug_Input_Output_With_Recordsets_Expected_Pass()
        {
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", ",", 1, " ", "Left"));
            DsfDataMergeActivity act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;
            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(34, inRes[0].Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
        }

        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string result, IList<DataMergeDTO> mergeCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDataMergeActivity { Result = result, MergeCollection = mergeCollection }
            };

            CurrentDL = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
