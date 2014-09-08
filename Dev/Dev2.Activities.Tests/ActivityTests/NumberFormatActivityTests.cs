using ActivityUnitTests;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
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

        private void SetupArguments(string currentDl, string testData, string result, string expression,
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
            TestData = currentDl;
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

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
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

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfNumberFormatActivity_Execute")]
        public void DsfNumberFormatActivity_Execute_MultipleResults_ExpectError()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                             "[[res]],[[bes]]", "123.123", enRoundingType.None, "", "");
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            string systemError;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out systemError, out error);

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
            Assert.IsNull(actual);
        }

    }
}
