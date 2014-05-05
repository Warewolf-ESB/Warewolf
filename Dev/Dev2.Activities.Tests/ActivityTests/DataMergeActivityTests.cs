using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class DataMergeActivityTest : BaseActivityUnitTest
    {
        IList<DataMergeDTO> _mergeCollection = new List<DataMergeDTO>();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        [TestInitialize]
        public void MyTestInitialize()
        {
            if(_mergeCollection == null)
            {
                _mergeCollection = new List<DataMergeDTO>();
            }
            _mergeCollection.Clear();
        }

        #endregion

        #region Language Tests

        [TestMethod]
        public void Merge_Two_Sclars_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[CompanyTelNo]]", "None", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("Dev2,0317641234", actual);
        }

        [TestMethod]
        public void Merge_Recordset_With_Star_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", ",", 1, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("WallissBarneysTrevorsTravissJuriesBrendonsMassimosAshleysSashensWalliss", actual);
        }

        [TestMethod]
        public void Merge_Recordset_And_Scalar_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(4).FirstName]]", "Chars", " works at ", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ".", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("Travis works at Dev2.", actual);
        }

        [TestMethod]
        public void Merge_Two_Recordsets_Char_Merge_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Chars", "'s phone number is ", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[TelNumbers(*).number]]", "Chars", ".", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, @"res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            const string expected = @"Wallis,Buchan
Barney,Buchan
Trevor,Williams-Ros
Travis,Frisigner
Jurie,Smit
Brendon,Page
Massimo,Guerrera
Ashley,Lewis
Sashen,Naidoo
Wallis,Buchan
";
            Assert.AreEqual(expected, actual);
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

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            const string expected = @"WBuchan
BBuchan
TWilliams-Ros
TFrisigner
JSmit
BPage
MGuerrera
ALewis
SNaidoo
WBuchan
";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Index_Merge_Char_Padding_Left_Align_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Index", "10", 1, "0", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            const string expected = @"Wallis0000Buchan
Barney0000Buchan
Trevor0000Williams-Ros
Travis0000Frisigner
Jurie00000Smit
Brendon000Page
Massimo000Guerrera
Ashley0000Lewis
Sashen0000Naidoo
Wallis0000Buchan
";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Index_Merge_Char_Padding_Right_Align_Expected_Data_Merged_Together_Success()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Index", "10", 1, "0", "Right"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            const string expected = @"0000WallisBuchan
0000BarneyBuchan
0000TrevorWilliams-Ros
0000TravisFrisigner
00000JurieSmit
000BrendonPage
000MassimoGuerrera
0000AshleyLewis
0000SashenNaidoo
0000WallisBuchan
";

            Assert.AreEqual(expected, actual);
        }

        //2013.05.31: Ashley Lewis for bug 9485 - merge tool dropping data if index equals length
        [TestMethod]
        public void MergeTwoFieldsInRecordsetsIndexWithIndexEqualToDataLengthExpectedNoDataLoss()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Index", "6", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            const string expected = @"WallisBuchan
BarneyBuchan
TrevorWilliams-Ros
TravisFrisigner
Jurie Smit
BrendoPage
MassimGuerrera
AshleyLewis
SashenNaidoo
WallisBuchan
";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMerge_Execute")]
        public void DsfDataMerge_Execute_AtValueNegativeForIndextType_HasErrorMessage()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Add(new DataMergeDTO("[[Customers(*).FirstName]]", "Index", "-6", 1, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            DataListRemoval(result.DataListID);
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);
            StringAssert.Contains(actual, "The 'Using' value must be a real number.");
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

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(@"<InnerError>Recordset index [ 0 ] is not greater than zero</InnerError>", actual);
        }

        [TestMethod]
        public void Merge_Two_Fields_In_Recordsets_Negative_Index_In_Recordset_Expected_No_Data_Merged()
        {
            _mergeCollection.Add(new DataMergeDTO("[[Customers(-1).FirstName]]", "Index", "1", 1, "", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[Customers(1).LastName]]", "New Line", "", 2, "", "Left"));
            SetupArguments(ActivityStrings.DataMergeDataListWithData, ActivityStrings.DataMergeDataListShape, "[[res]]", _mergeCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(@"<InnerError>Recordset index [ -1 ] is not greater than zero</InnerError>", actual);
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_UpdateForEachInputs")]
        public void DsfDataMergeActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            var act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[CompanyName]]", act.MergeCollection[0].InputVariable);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_UpdateForEachInputs")]
        public void DsfDataMergeActivity_UpdateForEachInputs_MoreThan1Updates_UpdatesMergeCollection()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[CompanyNumber]]", "Chars", ",", 2, " ", "Left"));
            var act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            var tuple1 = new Tuple<string, string>("[[CompanyName]]", "Test");
            var tuple2 = new Tuple<string, string>("[[CompanyNumber]]", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.MergeCollection[0].InputVariable);
            Assert.AreEqual("Test2", act.MergeCollection[1].InputVariable);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_UpdateForEachOutputs")]
        public void DsfDataMergeActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            DsfDataMergeActivity act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[res]]", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_UpdateForEachOutputs")]
        public void DsfDataMergeActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            DsfDataMergeActivity act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[res]]", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_UpdateForEachOutputs")]
        public void DsfDataMergeActivity_UpdateForEachOutputs_1Updates_UpdateCountNumber()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            DsfDataMergeActivity act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_GetForEachInputs")]
        public void DsfDataMergeActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            _mergeCollection.Add(new DataMergeDTO("[[CompanyNumber]]", "Chars", ",", 2, " ", "Left"));
            DsfDataMergeActivity act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(3, dsfForEachItems.Count);
            Assert.AreEqual("[[CompanyName]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[CompanyName]]", dsfForEachItems[0].Value);
            Assert.AreEqual("[[CompanyNumber]]", dsfForEachItems[1].Name);
            Assert.AreEqual("[[CompanyNumber]]", dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_GetForEachOutputs")]
        public void DsfDataMergeActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            _mergeCollection.Clear();
            _mergeCollection.Add(new DataMergeDTO("[[CompanyName]]", "Chars", ",", 1, " ", "Left"));
            DsfDataMergeActivity act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Value);
        }

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string result, IList<DataMergeDTO> mergeCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDataMergeActivity { Result = result, MergeCollection = mergeCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
