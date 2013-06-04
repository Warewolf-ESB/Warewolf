using Dev2;
using Dev2.Common;
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
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class ReplaceActivityTests : BaseActivityUnitTest
    {
        public ReplaceActivityTests()
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

        #region Replace Positive Tests

        [TestMethod]
        public void Replace_In_Two_Recordset_Fields_Expected_Two_Replaces_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"2";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_In_Two_Recordset_Fields_With_Space_Between_Expected_Two_Replaces_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]], [[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"2";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_With_Recset_ToFind_To_Replace_Expected_six_Replaced()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "[[ReplaceRecset(*).replace]]", "TEST", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"6";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_In_Scalar_Field_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[CompanyName]]", "Dev2", "TheUnlimted", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"1";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "CompanyName", out actual, out error);
                Assert.AreEqual("TheUnlimted", actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_In_Scalar_Field_With_CaseMatch_On_Expected_Zero_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[CompanyName]]", "dev2", "TheUnlimted", "[[res]]", true);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"0";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "CompanyName", out actual, out error);
                Assert.AreEqual("Dev2", actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_Recordset_Fields_With_CaseMatch_On_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", true);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"1";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("barney", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_Recordset_Fields_With_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(1).field1]],[[Customers(2).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"1";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);
                Assert.AreEqual("Barney", dataListItems[0].TheValue);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_Scalar_With_Backlash_Expected_One_Replace()
        {
            SetupArguments(@"<DataList><Thing>testlol\</Thing><Res></Res></DataList>", @"<DataList><Thing></Thing><Res></Res></DataList>", @"[[Thing]]", @"lol\", @"Wallis", "[[Res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"1";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "Res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "Thing", out actual, out error);
                Assert.AreEqual("testWallis", actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ReplaceScalarWithBracketExpectedOneReplace()
        {
            SetupArguments(@"<DataList><Thing>(0)</Thing><Res></Res></DataList>", @"<DataList><Thing></Thing><Res></Res></DataList>", @"[[Thing]]", @"(0)", @"+1", "[[Res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"1";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "Res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "Thing", out actual, out error);
                Assert.AreEqual("+1", actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        //2013.02.12: Ashley Lewis - Bug 8800
        [TestMethod]
        public void ReplaceInAllRecordsetFieldsExpectedTwoReplacesSuccess()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData.Replace("f2r2", "barney"), ActivityStrings.ReplaceDataListShape, "[[recset1(*)]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"2";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field2", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[1].TheValue);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion Replace Positive Tests

        #region Replace Negative Tests

        [TestMethod]
        public void ReplaceRawStringAsInputExpectedFriendlyErrorMessage()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "rawstringdata", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"<InnerError>Please insert only variables into Fields To Search</InnerError>";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_Recordset_Field_With_Negative_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(-1).field1]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"<InnerError>Recordset index [ -1 ] is not greater than zero</InnerError>";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Replace_Recordset_Field_With_Zero_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListShape, ActivityStrings.ReplaceDataListWithData, "[[recset1(0).field1]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"<InnerError>Recordset index [ 0 ] is not greater than zero</InnerError>";
            string actual = string.Empty;
            List<string> recsetData = new List<string>();
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion Replace Negative Tests

        #region Get Input/Output Tests

        [TestMethod]
        public void CountRecordsetActivity_GetInputs_Expected_Four_Input()
        {
            DsfReplaceActivity testAct = new DsfReplaceActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 4);
        }

        [TestMethod]
        public void CountRecordsetActivity_GetOutputs_Expected_One_Output()
        {
            DsfReplaceActivity testAct = new DsfReplaceActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Replace_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfReplaceActivity act = new DsfReplaceActivity { FieldsToSearch = "[[CompanyName]]", Find = "2", ReplaceWith = "3", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(1, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(3, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Replace_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfReplaceActivity act = new DsfReplaceActivity { FieldsToSearch = "[[Customers(*).DOB]]", Find = "/", ReplaceWith = ".", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(1, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(30, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string fieldsToSearch, string find, string replaceWith, string result, bool caseMatch)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfReplaceActivity { FieldsToSearch = fieldsToSearch, Find = find, ReplaceWith = replaceWith, Result = result, CaseMatch = caseMatch }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
