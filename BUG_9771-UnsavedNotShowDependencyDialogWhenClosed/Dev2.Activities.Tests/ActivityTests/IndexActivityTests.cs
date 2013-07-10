using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class IndexActivityTests : BaseActivityUnitTest
    {
        public IndexActivityTests()
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

        #region Index Positive Tests

        [TestMethod]
        public void Index_Recordset_With_Index_Expected_Index_Of_Four_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(1).field1]]", "First Occurrence", "ney", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "4";

            string actual = string.Empty;
            string error = string.Empty;
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
        public void Index_Recordset_With_Star_And_Star_Search_Criteria_Expected_Index_Of_Four_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShapeWithThreeRecordsets, ActivityStrings.IndexDataListWithDataAndThreeRecordsets,
                           "[[Customers(*).FirstName]]", "First Occurrence", "[[recset1(*).field1]]", "Left To Right", "[[results(*).resField]]", "0");
            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "results", "resField", out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(24, actual.Count);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Index_Recordset_With_No_Index_Expected_Index_Of_One_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1().field1]]", "First Occurrence", "f1", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "1";

            string actual = string.Empty;
            string error = string.Empty;
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
        public void Index_Recordset_With_Star_Expected_Six_Different_Indexs_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(*).field1]]", "First Occurrence", "f1", "Left To Right", "[[Customers(*).FirstName]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "Customers", "FirstName", out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(6, actual.Count);
                Assert.AreEqual("-1", actual[0]);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void Index_Scalar_Expected_Index_Of_Four_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[CompanyName]]", "First Occurrence", "2", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "4";

            string actual = string.Empty;
            string error = string.Empty;
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
        public void Index_Scalar_RightToLeft_Expected_Index_Of_One_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[CompanyName]]", "First Occurrence", "2", "Right to Left", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "1";

            string actual = string.Empty;
            string error = string.Empty;
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
        public void Index_Scalar_Text_Not_Found_Expected_Index_Of_Negative_One_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[CompanyName]]", "First Occurrence", "zz", "Right to Left", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "-1";

            string actual = string.Empty;
            string error = string.Empty;
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

        #endregion Index Positive Tests

        #region Index Negative Tests

        [TestMethod]
        public void Index_Recordset_With_Zero_Index_Expected_Error_In_Error_tag_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(0).field1]]", "First Occurrence", "f1", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "<InnerError>Recordset index [ 0 ] is not greater than zero</InnerError>";

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);
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
        public void Index_Recordset_With_Negative_Index_Expected_Error_In_Error_Tag_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListShape,
                           "[[recset1(-1).field1]]", "First Occurrence", "f1", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "<InnerError>Recordset index [ -1 ] is not greater than zero</InnerError>";

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);
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
        public void Index_Raw_Data_AllOccurrences_Expected_Success()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListShape,
                           "ABCFDEFGH", "All Occurrences", "F", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string expected = "4,7";

            string actual = string.Empty;
            string error = string.Empty;
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
        public void Index_Recordset_With_Star_AllOccurrences_Expected_Six_Different_Indexs_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(*).field2]]", "All Occurrences", "2", "Left To Right", "[[Customers(*).FirstName]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "Customers", "FirstName", out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(6, actual.Count);
                Assert.AreEqual("2,4", actual[1]);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }


        #endregion Index Negative Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Index_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfIndexActivity act = new DsfIndexActivity { InField = "[[CompanyName]]", Index = "First Occurance", Characters = "2", Direction = "Left To Right", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Index_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfIndexActivity act = new DsfIndexActivity { InField = "[[Customers(*).FirstName]]", Index = "First Occurance", Characters = "b", Direction = "Left To Right", Result = "[[Numeric(*).num]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(30, outRes[0].FetchResultsList().Count);
        }

        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void IndexActivity_GetInputs_Expected_Five_Input()
        {
            DsfIndexActivity testAct = new DsfIndexActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 6);
        }

        [TestMethod]
        public void IndexActivity_GetOutputs_Expected_One_Output()
        {
            DsfIndexActivity testAct = new DsfIndexActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string inField, string index, string characters, string direction, string resultValue, string startIndex)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfIndexActivity
                {
                    StartIndex = startIndex
                ,
                    InField = inField
                ,
                    Index = index
                ,
                    Characters = characters
                ,
                    Direction = direction
                ,
                    Result = resultValue
                }
            };

            CurrentDl = currentDL;
            TestData = testData;
        }

        #endregion Private Test Methods

    }
}
