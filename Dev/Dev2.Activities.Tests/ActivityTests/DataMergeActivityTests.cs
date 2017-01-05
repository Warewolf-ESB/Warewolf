/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);

            // remove test datalist ;)

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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

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
            GetScalarValueFromEnvironment(result.Environment, @"res", out actual, out error);
            // remove test datalist ;)

             string expected = @"Wallis,Buchan
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
            FixBreaks(ref expected, ref actual);
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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

             string expected = @"WBuchan
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

            FixBreaks(ref expected, ref actual);
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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

            string expected = @"Wallis0000Buchan
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
           
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }
        private void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

             string expected = @"0000WallisBuchan
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

            FixBreaks(ref expected, ref actual);
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
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)

             string expected = "WallisBuchan\nBarneyBuchan\nTrevorWilliams-Ros\nTravisFrisigner\nJurie Smit\nBrendoPage\nMassimGuerrera\nAshleyLewis\nSashenNaidoo\nWallisBuchan\n";

            FixBreaks(ref expected, ref actual);
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
            string actual = result.Environment.FetchErrors();
            StringAssert.Contains(actual, "The 'Using' value must be a real number.");
        }

        #endregion Index Tests

        #region Negative Tests

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
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[CompanyName]]", act.MergeCollection[0].InputVariable);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataMergeActivity_GetOutputs")]
        public void DsfDataMergeActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfDataMergeActivity { Result = "[[res]]", MergeCollection = _mergeCollection };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[res]]", outputs[0]);
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
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
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
            act.UpdateForEachOutputs(null);
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
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
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
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
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
