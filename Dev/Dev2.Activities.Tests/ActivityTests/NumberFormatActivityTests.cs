using ActivityUnitTests;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class NumberFormatActivityTests : BaseActivityUnitTest
    {
        #region Class Members

        private TestContext testContextInstance;

        #endregion Class Members

        #region Properties

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
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Tests

        [TestMethod]
        public void Format_Where_NumberInputIsScalar_Expected_ScalarResolvedAndFormatted()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "[[number]]", enRoundingType.Normal, "", "");
            IDSFDataObject result = ExecuteProcess();

            string expected = "790";
            string actual;
            string error;
            string systemError;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out systemError, out error);

            if (!string.IsNullOrWhiteSpace(error) || !string.IsNullOrWhiteSpace(systemError))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Where_NumberInputIsRecordset_Expected_EveryValueIntheRecordSetIsAndFormatted()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[resRecordSet(*).number]]", "[[recordSet(*).number]]", enRoundingType.Normal, "", "");
            IDSFDataObject result = ExecuteProcess();

            //string expected = "790";
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error;
            string systemError;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out systemError, out error);

            if (!string.IsNullOrWhiteSpace(error) || !string.IsNullOrWhiteSpace(systemError))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            GetRecordSetFieldValueFromDataList(result.DataListID, "resRecordSet", "number", out actual, out error);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

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

            string expected = "123.1";
            string actual;
            string error;
            string systemError;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out systemError, out error);

            if (!string.IsNullOrWhiteSpace(error) || !string.IsNullOrWhiteSpace(systemError))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

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

            if (!string.IsNullOrWhiteSpace(error))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            Assert.IsFalse(string.IsNullOrWhiteSpace(actual));
        }

        [TestMethod]
        public void Format_Where_RoundingDecimalPlacesIsBlank_Expected_0DesimalPlacesAssumed()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "123.123", enRoundingType.Normal, "", "3");
            IDSFDataObject result = ExecuteProcess();

            string expected = "123.000";
            string actual;
            string error;
            string systemError;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out systemError, out error);

            if (!string.IsNullOrWhiteSpace(error) || !string.IsNullOrWhiteSpace(systemError))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Where_ShowDecimalPlacesIsBlank_Expected_NoDecimalPlaceAdjustmentOccurs()
        {
            SetupArguments(ActivityStrings.NumberFormatActivity_DataList_WithData, ActivityStrings.NumberFormatActivity_DataList_Shape,
                           "[[res]]", "123.123", enRoundingType.None, "", "");
            IDSFDataObject result = ExecuteProcess();

            string expected = "123.123";
            string actual;
            string error;
            string systemError;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out systemError, out error);

            if (!string.IsNullOrWhiteSpace(error) || !string.IsNullOrWhiteSpace(systemError))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

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

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(4, inRes[0].Count);
            Assert.AreEqual(4, inRes[1].Count);
            Assert.AreEqual(2, inRes[2].Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
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

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(31, inRes[0].Count);
            Assert.AreEqual(4, inRes[1].Count);
            Assert.AreEqual(2, inRes[2].Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(30, outRes[0].Count);
        }

        #endregion

    }
}
