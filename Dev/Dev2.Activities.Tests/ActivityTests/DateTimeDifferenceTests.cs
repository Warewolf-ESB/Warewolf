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
    public class DateTimeDifferenceTests : BaseActivityUnitTest
    {
        public DateTimeDifferenceTests()
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

        #region Positive Test Cases

        [TestMethod]
        public void Positive_With_Normal_Params_Expected_Positive()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012/03/05 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            string expected = "209";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Positive_UsingRecorsetWithStar_Expected_Positive()
        {
            SetupArguments(
                           ActivityStrings.DateTimeDifferenceDataListWithData
                         , ActivityStrings.DateTimeDifferenceDataListShape
                         , "[[recset1(*).f1]]"
                         , "[[recset2(*).f2]]"
                         , "dd/mm/yyyy"
                         , "Days"
                         , "[[resCol(*).res]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            IList<IBinaryDataListItem> results;
            GetRecordSetFieldValueFromDataList(result.DataListID, "resCol", "res", out results, out error);

            Assert.AreEqual("8847", results[0].TheValue);
            Assert.AreEqual("9477", results[1].TheValue);
            Assert.AreEqual("9090", results[2].TheValue);
        }

        #endregion Positive Test Cases

        #region Error Test Cases

        [TestMethod]
        public void Input1_Not_Matching_InputFormat_Expected_Error()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );
            IDSFDataObject result = ExecuteProcess();
            /* Expected Error 
            string expected = @"<Error><![CDATA[The following errors occured : 
Literal expressed from index 4 doesn't match what is specified in the input format.]]></Error>";
             */
            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void Input2_Not_Matching_InputFormat_Expected_Error()
        {

            SetupArguments(
                            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                          , ActivityStrings.DateTimeDiff_DataListShape
                          , "2012/03/05 09:20:30 AM"
                          , "2012 07:15:50 AM"
                          , "yyyy/mm/dd 12h:min:ss am/pm"
                          , "Days"
                          , "[[Result]]"
                          );

            IDSFDataObject result = ExecuteProcess();
            /* Expected Error = how can we retrieve the error from the DataList
            string expected = @"<Error><![CDATA[The following errors occured : 
Literal expressed from index 4 doesn't match what is specified in the input format.]]></Error>";
             */
            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void Invalid_InputFormat_Expected_Error()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012/03/05 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/wrongFromat/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );
            IDSFDataObject result = ExecuteProcess();

            /* Expected Result - how can we retrieve from datalist
            string expected = @"<Error><![CDATA[The following errors occured : 
Literal expressed from index 7 doesn't match what is specified in the input format.]]></Error>";
             */

            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void Blank_Input1_Expected_Error()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , ""
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            /* Expected Result - how can we retrieve from datalist
            string expected = @"<Error><![CDATA[The following errors occured : 
Input can't be null/empty.]]></Error>";
             */
            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void Blank_Input2_Expected_Error()
        {
            SetupArguments(
                            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                          , ActivityStrings.DateTimeDiff_DataListShape
                          , "2012/10/01 07:15:50 AM"
                          , ""
                          , "yyyy/mm/dd 12h:min:ss am/pm"
                          , "Days"
                          , "[[Result]]"
                          );

            IDSFDataObject result = ExecuteProcess();
            /* Expected Result - how can we retrieve from datalist
            string expected = @"<Error><![CDATA[The following errors occured : 
Input can't be null/empty.]]></Error>";
             */
            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void Blank_InputFormat_Expected_Error()
        {
            SetupArguments(
                              "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                            , ActivityStrings.DateTimeDiff_DataListShape
                            , "2012/10/01 07:15:50 AM"
                            , "2012/10/01 07:15:50 AM"
                            , ""
                            , "Days"
                            , "[[Result]]"
                            );
            IDSFDataObject result = ExecuteProcess();
            /* Expected Result - how can we retrieve from datalist
            string expected = @"<Error><![CDATA[The following errors occured : 
Format can't be null/empty.]]></Error>";
             */
            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void ErrorHandeling_Expected_ErrorTags()
        {
            SetupArguments(
                            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                          , ActivityStrings.DateTimeDiff_DataListShape
                          , "2012/10/01 07:15:50 AM"
                          , "2012/10/01 07:15:50 AM"
                          , "yyyy/mm/dd 12h:min:ss am/pm"
                          , "Days"
                          , "[[//().rec]]"
                          );

            IDSFDataObject result = ExecuteProcess();
            /* Expected Result - how can we retrieve from datalist
            string expected = @"<Error>";
             */
            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        #endregion Error Test Cases

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DateTimeDiffernce_Get_Debug_Input_Output_With_Scalars_Expected_Pass()
        {
            DsfDateTimeDifferenceActivity act = new DsfDateTimeDifferenceActivity { Input1 = "[[Customers(1).DOB]]", Input2 = "[[Customers(2).DOB]]", InputFormat = "yyyy/mm/dd", OutputType = "Days", Result = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(4, inRes[0].Count);
            Assert.AreEqual(4, inRes[1].Count);
            Assert.AreEqual(2, inRes[2].Count);
            Assert.AreEqual(2, inRes[3].Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DateTimeDiffernce_Get_Debug_Input_Output_With_Recordsets_Expected_Pass()
        {
            DsfDateTimeDifferenceActivity act = new DsfDateTimeDifferenceActivity { Input1 = "[[Customers(*).DOB]]", Input2 = "[[Customers(2).DOB]]", InputFormat = "yyyy/mm/dd", OutputType = "Days", Result = "[[Numeric(*).num]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(31, inRes[0].Count);
            Assert.AreEqual(4, inRes[1].Count);
            Assert.AreEqual(2, inRes[2].Count);
            Assert.AreEqual(2, inRes[3].Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(30, outRes[0].Count);
        }

        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void DateTimeDifference_GetInputs_Expected_Four_Input()
        {
            DsfDateTimeDifferenceActivity testAct = new DsfDateTimeDifferenceActivity { Input1 = "27-10-2012", Input2 = "28-10-2012", InputFormat = "dd-mm-yyyy", OutputType = "Years", Result = "[[result]]" };

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 4);
        }

        [TestMethod]
        public void DateTimeDifference_GetOutputs_Expected_One_Output()
        {
            DsfDateTimeDifferenceActivity testAct = new DsfDateTimeDifferenceActivity { Input1 = "27-10-2012", Input2 = "28-10-2012", InputFormat = "dd-mm-yyyy", OutputType = "Years", Result = "[[result]]" };

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string input1, string input2, string inputFormat, string outputType, string result)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDateTimeDifferenceActivity { Input1 = input1, Input2 = input2, InputFormat = inputFormat, OutputType = outputType, Result = result }
            };
            CurrentDL = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
