using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class NumberFormatActivityTests : BaseActivityUnitTest
    {
        #region Class Members

        #endregion Class Members

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion Properties

        #region Private Methods

        private void SetupArguments(string currentDL, string testData, string result, string expression,
            enRoundingType roundingType, string roundingDecimalPlaces, string decimalPlacesToShow)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfNumberFormatActivity
                {
                    Expression = expression,
                    Result = result,
                    RoundingType = Dev2EnumConverter.ConvertEnumValueToString(roundingType),
                    RoundingDecimalPlaces = roundingDecimalPlaces,
                    DecimalPlacesToShow = decimalPlacesToShow,
                }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Methods

        #region Tests

        [TestMethod]
        public void Format_Where_NumberInputIsScalar_Expected_ScalarResolvedAndFormatted()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "[[number]]", enRoundingType.Normal, "", "");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "790";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Where_NumberInputIsRecordset_Expected_EveryValueIntheRecordSetIsAndFormatted()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[resRecordSet(*).number]]", "[[recordSet(*).number]]", enRoundingType.Normal, "", "");
            IDSFDataObject result = ExecuteProcess();

            //string expected = "790";
            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "resRecordSet", "number", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(actual.Count, 2);
            Assert.AreEqual(actual[0].TheValue, "123");
            Assert.AreEqual(actual[1].TheValue, "456");
        }

        [TestMethod]
        public void Format_Where_NumberInputIsRawNumber_Expected_RawNumberIsFormatted()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "123.123", enRoundingType.Normal, "2", "1");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "123.1";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Where_NumberInputIsntNumeric_Expected_Error()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "", enRoundingType.Normal, "2", "1");
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsFalse(string.IsNullOrWhiteSpace(actual));
        }

        [TestMethod]
        public void Format_Where_RoundingDecimalPlacesIsBlank_Expected_0DesimalPlacesAssumed()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "123.123", enRoundingType.Normal, "", "3");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "123.000";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Where_ShowDecimalPlacesIsBlank_Expected_NoDecimalPlaceAdjustmentOccurs()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "123.123", enRoundingType.None, "", "");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "123.123";
            string actual;
            string error;
            string systemError;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out systemError, out error);

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        #endregion Tests

        #region GetWizardData Tests

        [TestMethod]
        public void GetWizardData_Expected_Correct_IBinaryDataList()
        {
            DsfNumberFormatActivity testAct = new DsfNumberFormatActivity
            {
                Expression = "[[SomeVariable]]",
                RoundingType = Dev2EnumConverter.ConvertEnumValueToString(enRoundingType.None),
                RoundingDecimalPlaces = "",
                DecimalPlacesToShow = "",
                Result = "[[res]]",
            };

            IBinaryDataList binaryDL = testAct.GetWizardData();
            var recsets = binaryDL.FetchRecordsetEntries();
            var scalars = binaryDL.FetchScalarEntries();

            // remove test datalist ;)
            DataListRemoval(binaryDL.UID);

            Assert.AreEqual(0, recsets.Count);
            Assert.AreEqual(5, scalars.Count);
        }

        #endregion GetWizardData Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void NumberFormating_Get_Debug_Input_Output_With_Recordset_Using_Numeric_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfNumberFormatActivity act = new DsfNumberFormatActivity { Expression = "[[Numeric(1).num]]", RoundingType = "Up", RoundingDecimalPlaces = "2", DecimalPlacesToShow = "2", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            var fetchResultsList = outRes[0].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);

            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[res]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "654.00");
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void NumberFormating_Get_Debug_Input_Output_With_Recordset_Using_Append_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfNumberFormatActivity act = new DsfNumberFormatActivity { Expression = "[[Numeric(*).num]]", RoundingType = "Up", RoundingDecimalPlaces = "2", DecimalPlacesToShow = "2", Result = "[[Numeric().num]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(10, outRes.Count);
            
            var fetchResultsList = outRes[0].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(11).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "654.00");

            fetchResultsList = outRes[1].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(12).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "668416154.00");

            fetchResultsList = outRes[2].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(13).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "51566.00");

            fetchResultsList = outRes[3].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(14).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "21.00");

            fetchResultsList = outRes[4].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(15).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "1520.00");

            fetchResultsList = outRes[5].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(16).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "1.00");

            fetchResultsList = outRes[6].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(17).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "0.00");

            fetchResultsList = outRes[7].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(18).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "2135.00");

            fetchResultsList = outRes[8].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(19).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "5123.00");

            fetchResultsList = outRes[9].FetchResultsList();
            Assert.AreEqual(3, fetchResultsList.Count);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(20).num]]");
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "110.00");

        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void NumberFormating_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfNumberFormatActivity act = new DsfNumberFormatActivity { Expression = "[[Numeric(*).num]]", RoundingType = "Up", RoundingDecimalPlaces = "2", DecimalPlacesToShow = "2", Result = "[[Numeric(*).num]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            var fetchResultsList = outRes[0].FetchResultsList();
            Assert.AreEqual(30, fetchResultsList.Count);

            Assert.AreEqual(fetchResultsList[0].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[0].GroupIndex, 1);
            Assert.AreEqual(fetchResultsList[0].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[0].Value, "[[Numeric(1).num]]");
            Assert.AreEqual(fetchResultsList[1].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[1].GroupIndex, 1);
            Assert.AreEqual(fetchResultsList[1].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[1].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[2].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[2].GroupIndex, 1);
            Assert.AreEqual(fetchResultsList[2].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[2].Value, "654.00");

            Assert.AreEqual(fetchResultsList[3].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[3].GroupIndex, 2);
            Assert.AreEqual(fetchResultsList[3].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[3].Value, "[[Numeric(2).num]]");
            Assert.AreEqual(fetchResultsList[4].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[4].GroupIndex, 2);
            Assert.AreEqual(fetchResultsList[4].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[4].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[5].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[5].GroupIndex, 2);
            Assert.AreEqual(fetchResultsList[5].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[5].Value, "668416154.00");
            
            Assert.AreEqual(fetchResultsList[6].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[6].GroupIndex, 3);
            Assert.AreEqual(fetchResultsList[6].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[6].Value, "[[Numeric(3).num]]");
            Assert.AreEqual(fetchResultsList[7].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[7].GroupIndex, 3);
            Assert.AreEqual(fetchResultsList[7].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[7].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[8].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[8].GroupIndex, 3);
            Assert.AreEqual(fetchResultsList[8].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[8].Value, "51566.00");

            Assert.AreEqual(fetchResultsList[9].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[9].GroupIndex, 4);
            Assert.AreEqual(fetchResultsList[9].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[9].Value, "[[Numeric(4).num]]");
            Assert.AreEqual(fetchResultsList[10].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[10].GroupIndex, 4);
            Assert.AreEqual(fetchResultsList[10].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[10].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[11].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[11].GroupIndex, 4);
            Assert.AreEqual(fetchResultsList[11].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[11].Value, "21.00");
            
            Assert.AreEqual(fetchResultsList[12].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[12].GroupIndex, 5);
            Assert.AreEqual(fetchResultsList[12].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[12].Value, "[[Numeric(5).num]]");
            Assert.AreEqual(fetchResultsList[13].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[13].GroupIndex, 5);
            Assert.AreEqual(fetchResultsList[13].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[13].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[14].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[14].GroupIndex, 5);
            Assert.AreEqual(fetchResultsList[14].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[14].Value, "1520.00");

            Assert.AreEqual(fetchResultsList[15].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[15].GroupIndex, 6);
            Assert.AreEqual(fetchResultsList[15].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[15].Value, "[[Numeric(6).num]]");
            Assert.AreEqual(fetchResultsList[16].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[16].GroupIndex, 6);
            Assert.AreEqual(fetchResultsList[16].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[16].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[17].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[17].GroupIndex, 6);
            Assert.AreEqual(fetchResultsList[17].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[17].Value, "1.00");

            Assert.AreEqual(fetchResultsList[18].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[18].GroupIndex, 7);
            Assert.AreEqual(fetchResultsList[18].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[18].Value, "[[Numeric(7).num]]");
            Assert.AreEqual(fetchResultsList[19].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[19].GroupIndex, 7);
            Assert.AreEqual(fetchResultsList[19].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[19].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[20].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[20].GroupIndex, 7);
            Assert.AreEqual(fetchResultsList[20].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[20].Value, "0.00");

            Assert.AreEqual(fetchResultsList[21].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[21].GroupIndex, 8);
            Assert.AreEqual(fetchResultsList[21].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[21].Value, "[[Numeric(8).num]]");
            Assert.AreEqual(fetchResultsList[22].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[22].GroupIndex, 8);
            Assert.AreEqual(fetchResultsList[22].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[22].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[23].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[23].GroupIndex, 8);
            Assert.AreEqual(fetchResultsList[23].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[23].Value, "2135.00");
            
            Assert.AreEqual(fetchResultsList[24].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[24].GroupIndex, 9);
            Assert.AreEqual(fetchResultsList[24].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[24].Value, "[[Numeric(9).num]]");
            Assert.AreEqual(fetchResultsList[25].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[25].GroupIndex, 9);
            Assert.AreEqual(fetchResultsList[25].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[25].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[26].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[26].GroupIndex, 9);
            Assert.AreEqual(fetchResultsList[26].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[26].Value, "5123.00");

            Assert.AreEqual(fetchResultsList[27].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[27].GroupIndex, 10);
            Assert.AreEqual(fetchResultsList[27].Type, DebugItemResultType.Variable);
            Assert.AreEqual(fetchResultsList[27].Value, "[[Numeric(10).num]]");
            Assert.AreEqual(fetchResultsList[28].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[28].GroupIndex, 10);
            Assert.AreEqual(fetchResultsList[28].Type, DebugItemResultType.Label);
            Assert.AreEqual(fetchResultsList[28].Value, GlobalConstants.EqualsExpression);
            Assert.AreEqual(fetchResultsList[29].GroupName, "[[Numeric(*).num]]");
            Assert.AreEqual(fetchResultsList[29].GroupIndex, 10);
            Assert.AreEqual(fetchResultsList[29].Type, DebugItemResultType.Value);
            Assert.AreEqual(fetchResultsList[29].Value, "110.00");
        } 
        
        #endregion

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNumberFormatActivity_UpdateForEachInputs")]
        public void DsfNumberFormatActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "[[Numeric(1).num]]";
            const string roundingType = "Up";
            const string result = "[[res]]";
            const string roundingDecimalPlaces = "2";
            const string decimalPlacesToShow = "2";
            var act = new DsfNumberFormatActivity { Expression = expression, RoundingType = roundingType, RoundingDecimalPlaces = roundingDecimalPlaces, DecimalPlacesToShow = decimalPlacesToShow, Result = result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(expression, act.Expression);
            Assert.AreEqual(roundingType, act.RoundingType);
            Assert.AreEqual(roundingDecimalPlaces, act.RoundingDecimalPlaces);
            Assert.AreEqual(decimalPlacesToShow, act.DecimalPlacesToShow);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNumberFormatActivity_UpdateForEachInputs")]
        public void DsfNumberFormatActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string expression = "[[Numeric(1).num]]";
            const string roundingType = "Up";
            const string result = "[[res]]";
            const string roundingDecimalPlaces = "2";
            const string decimalPlacesToShow = "3";
            var act = new DsfNumberFormatActivity { Expression = expression, RoundingType = roundingType, RoundingDecimalPlaces = roundingDecimalPlaces, DecimalPlacesToShow = decimalPlacesToShow, Result = result };

            var tuple1 = new Tuple<string, string>(expression, "Test");
            var tuple2 = new Tuple<string, string>(roundingType, "Test2");
            var tuple3 = new Tuple<string, string>(roundingDecimalPlaces, "Test3");
            var tuple4 = new Tuple<string, string>(decimalPlacesToShow, "Test4");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3, tuple4 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.RoundingType);
            Assert.AreEqual("Test", act.Expression);
            Assert.AreEqual("Test3", act.RoundingDecimalPlaces);
            Assert.AreEqual("Test4", act.DecimalPlacesToShow);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNumberFormatActivity_UpdateForEachOutputs")]
        public void DsfNumberFormatActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "[[Numeric(1).num]]";
            const string roundingType = "Up";
            const string result = "[[res]]";
            const string roundingDecimalPlaces = "2";
            const string decimalPlacesToShow = "2";
            var act = new DsfNumberFormatActivity { Expression = expression, RoundingType = roundingType, RoundingDecimalPlaces = roundingDecimalPlaces, DecimalPlacesToShow = decimalPlacesToShow, Result = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNumberFormatActivity_UpdateForEachOutputs")]
        public void DsfNumberFormatActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string expression = "[[Numeric(1).num]]";
            const string roundingType = "Up";
            const string result = "[[res]]";
            const string roundingDecimalPlaces = "2";
            const string decimalPlacesToShow = "2";
            var act = new DsfNumberFormatActivity { Expression = expression, RoundingType = roundingType, RoundingDecimalPlaces = roundingDecimalPlaces, DecimalPlacesToShow = decimalPlacesToShow, Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNumberFormatActivity_UpdateForEachOutputs")]
        public void DsfNumberFormatActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string expression = "[[Numeric(1).num]]";
            const string roundingType = "Up";
            const string result = "[[res]]";
            const string roundingDecimalPlaces = "2";
            const string decimalPlacesToShow = "2";
            var act = new DsfNumberFormatActivity { Expression = expression, RoundingType = roundingType, RoundingDecimalPlaces = roundingDecimalPlaces, DecimalPlacesToShow = decimalPlacesToShow, Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNumberFormatActivity_GetForEachInputs")]
        public void DsfNumberFormatActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string expression = "[[Numeric(1).num]]";
            const string roundingType = "Up";
            const string result = "[[res]]";
            const string roundingDecimalPlaces = "2";
            const string decimalPlacesToShow = "2";
            var act = new DsfNumberFormatActivity { Expression = expression, RoundingType = roundingType, RoundingDecimalPlaces = roundingDecimalPlaces, DecimalPlacesToShow = decimalPlacesToShow, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(4, dsfForEachItems.Count);
            Assert.AreEqual(expression, dsfForEachItems[0].Name);
            Assert.AreEqual(expression, dsfForEachItems[0].Value);
            Assert.AreEqual(roundingType, dsfForEachItems[1].Name);
            Assert.AreEqual(roundingType, dsfForEachItems[1].Value);
            Assert.AreEqual(roundingDecimalPlaces, dsfForEachItems[2].Name);
            Assert.AreEqual(roundingDecimalPlaces, dsfForEachItems[2].Value);
            Assert.AreEqual(decimalPlacesToShow, dsfForEachItems[3].Name);
            Assert.AreEqual(decimalPlacesToShow, dsfForEachItems[3].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNumberFormatActivity_GetForEachOutputs")]
        public void DsfNumberFormatActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string expression = "[[Numeric(1).num]]";
            const string roundingType = "Up";
            const string result = "[[res]]";
            const string roundingDecimalPlaces = "2";
            const string decimalPlacesToShow = "2";
            var act = new DsfNumberFormatActivity { Expression = expression, RoundingType = roundingType, RoundingDecimalPlaces = roundingDecimalPlaces, DecimalPlacesToShow = decimalPlacesToShow, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

    }
}
